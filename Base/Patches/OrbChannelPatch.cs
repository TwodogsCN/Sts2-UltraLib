using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Orbs;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Orbs;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace UltraLib.Base.Patches;

/// <summary>
/// Harmony patch on <c>OrbCmd.Channel</c>: replaces the channel flow with a safer
/// implementation that handles empty slots, full-queue eviction and orb-layout fixes.
/// </summary>
/// <remarks>
/// OrbCmd.Channel 的 Hook Patch：用更安全的实现替换充能流程，
/// 处理空槽补槽、满队列自动激发、以及充能球 UI 布局的强制修正。
/// </remarks>
[HarmonyPatch(typeof(OrbCmd), nameof(OrbCmd.Channel), typeof(PlayerChoiceContext), typeof(OrbModel), typeof(Player))]
public static class OrbCmdChannelPatch
{
    [ThreadStatic]
    private static int _recursionDepth = 0;

    [HarmonyPrefix]
    public static bool Prefix(PlayerChoiceContext choiceContext, OrbModel orb, Player player, ref Task __result)
    {
        __result = SafeChannelInternal(choiceContext, orb, player);
        return false;
    }

    /// <summary>
    /// Safely channels an orb: adds a slot when the player has none, evicts the oldest
    /// orb when the queue is full (up to 10 times), then enqueues, plays SFX/animation
    /// and fires the after-channel hook. Guarded against re-entrancy.
    /// </summary>
    /// <remarks>
    /// 安全充能核心：当玩家无槽位时自动补 1 槽；队列满时先激发最旧充能球（最多 10 次）；
    /// 随后入队、播放音效与动画、触发充能后 Hook。带递归深度保护。
    /// </remarks>
    private static async Task SafeChannelInternal(PlayerChoiceContext choiceContext, OrbModel orb, Player player)
    {
        _recursionDepth++;
        try
        {
            if (orb == null || player == null || CombatManager.Instance == null || CombatManager.Instance.IsOverOrEnding)
            {
                return;
            }

            ICombatState combatState = player.Creature.CombatState;
            OrbQueue orbQueue = player.PlayerCombatState.OrbQueue;

            if (player.Character.BaseOrbSlotCount == 0 && orbQueue.Capacity == 0)
            {
                await OrbCmd.AddSlots(player, 1);
            }

            orb.AssertMutable();
            orb.Owner = player;

            int loopCount = 0;
            while (orbQueue.Orbs.Count >= orbQueue.Capacity && loopCount < 10)
            {
                loopCount++;
                await OrbCmd.EvokeNext(choiceContext, player);
                await Task.Delay(10);
            }

            if (orbQueue.Orbs.Count < orbQueue.Capacity)
            {
                if (await orbQueue.TryEnqueue(orb))
                {
                    CombatManager.Instance.History.OrbChanneled(combatState, orb);
                    orb.PlayChannelSfx();

                    var playerNode = NCombatRoom.Instance?.GetCreatureNode(player.Creature);
                    NOrbManager? orbManager = playerNode?.OrbManager;

                    if (orbManager != null)
                    {
                        orbManager.AddOrbAnim();
                        ForceFixOrbLayout(orbManager, orbQueue);
                    }

                    await MegaCrit.Sts2.Core.Hooks.Hook.AfterOrbChanneled(combatState, choiceContext, player, orb);
                }
            }
            else
            {
                GD.Print($"[UltraLib] 充能球队列已满，跳过 {orb.Title.GetFormattedText()} 的充能 / orb queue full, skipping channel of {orb.Title.GetFormattedText()}");
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[UltraLib] 安全充能内部出错: {ex.Message} / error in SafeChannelInternal: {ex.Message}");
        }
        finally
        {
            _recursionDepth--;

            if (_recursionDepth == 0)
            {
                var playerNode = NCombatRoom.Instance?.GetCreatureNode(player.Creature);
                NOrbManager? orbManager = playerNode?.OrbManager;
                if (orbManager != null)
                {
                    ForceFixOrbLayout(orbManager, player.PlayerCombatState.OrbQueue);
                    AccessTools.Method(typeof(NOrbManager), "TweenLayout").Invoke(orbManager, null);
                }
            }
        }
    }

    /// <summary>
    /// Forces a fix on the orb manager's layout: repositions orbs that haven't been placed
    /// yet and refreshes controller navigation.
    /// </summary>
    /// <remarks>
    /// 强制修正 NOrbManager 的充能球布局：为尚未摆放的充能球计算并设置位置，并刷新控制器导航。
    /// </remarks>
    private static void ForceFixOrbLayout(NOrbManager orbManager, OrbQueue orbQueue)
    {
        try
        {
            var orbsField = AccessTools.Field(typeof(NOrbManager), "_orbs");
            List<NOrb> orbsList = (List<NOrb>)orbsField.GetValue(orbManager);
            bool isLocal = (bool)AccessTools.Property(typeof(NOrbManager), "IsLocal").GetValue(orbManager, null);

            if (orbsList == null || orbsList.Count == 0)
            {
                return;
            }

            int capacity = orbQueue.Capacity;
            if (capacity <= 0)
            {
                return;
            }

            float num1 = 125f;
            float num2 = num1 / (float)(capacity - 1);
            float num3 = Mathf.Lerp(225f, 300f, (float)(((double)capacity - 3.0) / 7.0));
            if (!isLocal)
            {
                num3 *= 0.75f;
            }

            for (int index = 0; index < Math.Min(orbsList.Count, capacity); ++index)
            {
                NOrb? norb = orbsList[index];
                if (norb == null || !GodotObject.IsInstanceValid(norb))
                {
                    continue;
                }

                if (norb.Position.LengthSquared() < 25f)
                {
                    float radians = float.DegreesToRadians(-25f - num1);
                    Vector2 finalPos = new Vector2(-Mathf.Cos(radians), Mathf.Sin(radians)) * num3;
                    norb.Position = finalPos;
                }
                num1 -= num2;
            }

            AccessTools.Method(typeof(NOrbManager), "UpdateControllerNavigation").Invoke(orbManager, null);
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[UltraLib] 充能球布局选择性修正失败: {ex.Message} / layout selective correction failed: {ex.Message}");
        }
    }
}
