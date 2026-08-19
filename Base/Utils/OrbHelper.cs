using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace UltraLib.Base.Utils;

/// <summary>
/// 充能球（Orb）操作辅助工具。
/// <para>
/// 提供充能球的生成（Channel）、激发（Evoke）、数值修改、槽位管理等便捷方法。
/// </para>
/// </summary>
public static class OrbHelper
{
    private static readonly MethodInfo EvokeMethod = AccessTools.Method(typeof(OrbCmd), "Evoke");

    /// <summary>
    /// 为玩家生成一个指定类型的充能球。
    /// </summary>
    /// <typeparam name="T">充能球类型。</typeparam>
    /// <param name="choiceContext">玩家选择上下文。</param>
    /// <param name="player">目标玩家。</param>
    /// <returns>生成的充能球实例，失败返回 null。</returns>
    public static async Task<T?> Channel<T>(PlayerChoiceContext choiceContext, Player player) where T : OrbModel
    {
        var blueprint = ModelDb.Orb<T>();
        if (blueprint == null)
        {
            GD.PrintErr($"[OrbHelper] Failed to find Orb blueprint for type: {typeof(T).Name}");
            return null;
        }

        T mutableOrb = (T)blueprint.ToMutable();
        await OrbCmd.Channel(choiceContext, mutableOrb, player);
        return mutableOrb;
    }

    /// <summary>
    /// 安全地异步生成充能球（不阻塞调用方）。
    /// </summary>
    /// <typeparam name="T">充能球类型。</typeparam>
    /// <param name="choiceContext">玩家选择上下文。</param>
    /// <param name="player">目标玩家。</param>
    public static void NoAwaitChannel<T>(PlayerChoiceContext choiceContext, Player player) where T : OrbModel
    {
        _ = SafeChannelAsync<T>(choiceContext, player);
    }

    private static async Task SafeChannelAsync<T>(PlayerChoiceContext choiceContext, Player player) where T : OrbModel
    {
        try
        {
            await Task.Yield();
            if (player != null && player.Creature.IsAlive)
                await Channel<T>(choiceContext, player);
        }
        catch (Exception e)
        {
            GD.PrintErr($"[OrbHelper] SafeChannelAsync Error: {e.Message}");
        }
    }

    /// <summary>
    /// 通过反射调用原版私有的 Evoke 方法，激发指定充能球。
    /// </summary>
    /// <param name="choiceContext">玩家选择上下文。</param>
    /// <param name="player">目标玩家。</param>
    /// <param name="targetOrb">要激发的充能球。</param>
    /// <param name="dequeue">是否从队列中移除该球。</param>
    public static async Task EvokeOrb(PlayerChoiceContext choiceContext, Player player,
        OrbModel targetOrb, bool dequeue = true)
    {
        if (targetOrb == null || player == null || EvokeMethod == null) return;
        if (CombatManager.Instance == null || CombatManager.Instance.IsOverOrEnding) return;

        await (Task)EvokeMethod.Invoke(null, [choiceContext, player, targetOrb, dequeue]);
    }

    /// <summary>
    /// 强制修改充能球的数值并同步 UI。
    /// <para>通过 Traverse 设置 _passiveVal 字段。</para>
    /// </summary>
    /// <param name="orb">目标充能球。</param>
    /// <param name="amount">新数值。</param>
    public static void SetVal(OrbModel orb, decimal amount)
    {
        if (orb == null) return;
        Traverse.Create(orb).Field("_passiveVal").SetValue(amount);
    }

    /// <summary>
    /// 安全地获取当前充能球列表的只读快照。
    /// </summary>
    /// <param name="player">目标玩家。</param>
    /// <returns>充能球列表。</returns>
    public static List<OrbModel> GetOrbList(Player player)
    {
        var orbs = player?.PlayerCombatState?.OrbQueue?.Orbs;
        return orbs != null ? orbs.ToList() : [];
    }

    /// <summary>
    /// 移除充能球槽位（带战斗结束检查）。
    /// </summary>
    /// <param name="player">目标玩家。</param>
    /// <param name="amount">移除的槽位数。</param>
    public static void RemoveSlots(Player player, int amount)
    {
        if (CombatManager.Instance.IsOverOrEnding) return;

        amount = Math.Min(player.PlayerCombatState.OrbQueue.Capacity, amount);
        player.PlayerCombatState.OrbQueue.RemoveCapacity(amount);
        NCombatRoom.Instance?.GetCreatureNode(player.Creature).OrbManager?.RemoveSlotAnim(amount);
    }
}
