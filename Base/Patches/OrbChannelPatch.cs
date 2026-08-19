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

    private static async Task SafeChannelInternal(PlayerChoiceContext choiceContext, OrbModel orb, Player player)
    {
        _recursionDepth++;
        try
        {
            if (orb == null || player == null || CombatManager.Instance == null || CombatManager.Instance.IsOverOrEnding)
                return;

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
                GD.PrintRaw($"[UltraLib] OrbQueue full, skipping channel of {orb.Title.GetFormattedText()}");
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[UltraLib] Error in SafeChannelInternal: {ex.Message}");
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

    private static void ForceFixOrbLayout(NOrbManager orbManager, OrbQueue orbQueue)
    {
        try
        {
            var orbsField = AccessTools.Field(typeof(NOrbManager), "_orbs");
            List<NOrb> orbsList = (List<NOrb>)orbsField.GetValue(orbManager);
            bool isLocal = (bool)AccessTools.Property(typeof(NOrbManager), "IsLocal").GetValue(orbManager, null);

            if (orbsList == null || orbsList.Count == 0) return;

            int capacity = orbQueue.Capacity;
            if (capacity <= 0) return;

            float num1 = 125f;
            float num2 = num1 / (float)(capacity - 1);
            float num3 = Mathf.Lerp(225f, 300f, (float)(((double)capacity - 3.0) / 7.0));
            if (!isLocal) num3 *= 0.75f;

            for (int index = 0; index < Math.Min(orbsList.Count, capacity); ++index)
            {
                NOrb? norb = orbsList[index];
                if (norb == null || !GodotObject.IsInstanceValid(norb)) continue;
                
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
            GD.PrintErr($"[UltraLib] Layout selective correction failed: {ex.Message}");
        }
    }
}