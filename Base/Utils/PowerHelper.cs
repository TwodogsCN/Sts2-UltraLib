using System.Reflection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace UltraLib.Base.Utils;

/// <summary>
/// 能力（Power）相关辅助方法。
/// 提供能力施加、获取、移除、修改层数等便捷操作。
/// </summary>
public static class PowerHelper
{
    /// <summary>
    /// 为目标添加指定类型的能力。
    /// </summary>
    public static async Task<T?> Apply<T>(
        PlayerChoiceContext choiceContext,
        Creature target,
        Decimal amount,
        Creature? applier,
        CardModel? cardSource,
        bool silent = false)
        where T : PowerModel
    {
        return await PowerCmd.Apply<T>(choiceContext, target, amount, applier, cardSource, silent);
    }

    /// <summary>
    /// 为多个目标添加指定类型的能力。
    /// </summary>
    public static async Task<IReadOnlyList<T>> Apply<T>(
        PlayerChoiceContext choiceContext,
        IEnumerable<Creature> targets,
        Decimal amount,
        Creature? applier,
        CardModel? cardSource,
        bool silent = false)
        where T : PowerModel
    {
        return await PowerCmd.Apply<T>(choiceContext, targets, amount, applier, cardSource, silent);
    }

    /// <summary>
    /// 使用指定的 PowerModel 实例施加能力（必须为 Mutable）。
    /// </summary>
    public static async Task Apply(
        PlayerChoiceContext choiceContext,
        PowerModel power,
        Creature target,
        Decimal amount,
        Creature? applier,
        CardModel? cardSource,
        bool silent = false)
    {
        await PowerCmd.Apply(choiceContext, power, target, amount, applier, cardSource);
    }

    /// <summary>
    /// 获取能力的悬浮提示。
    /// </summary>
    public static IHoverTip? GetPowerTip(PowerModel power)
    {
        var method = typeof(HoverTipFactory).GetMethod("FromPower",
            BindingFlags.Public | BindingFlags.Static, null, Type.EmptyTypes, null);
        if (method != null && method.IsGenericMethod)
        {
            var specificMethod = method.MakeGenericMethod(power.GetType());
            return specificMethod.Invoke(null, null) as IHoverTip;
        }
        return null;
    }

    /// <summary>
    /// 刷新能力拥有者的视觉显示。
    /// </summary>
    public static void RefreshVisuals(this PowerModel power)
    {
        if (power?.Owner == null) return;
        var creatureNode = NCombatRoom.Instance?.GetCreatureNode(power.Owner.Player.Creature);
        if (creatureNode != null)
        {
            creatureNode.RefreshIntents();
        }
    }

    /// <summary>
    /// 从生物身上获取指定类型的能力。
    /// </summary>
    public static T? GetPower<T>(Creature creature) where T : PowerModel
    {
        return creature.GetPower<T>();
    }

    /// <summary>
    /// 移除并清理指定能力。
    /// </summary>
    public static async Task Remove(PowerModel? power)
    {
        if (power == null)
            return;
        power.RemoveInternal();
        await Cmd.CustomScaledWait(0.2f, 0.4f);
        await power.AfterRemoved(power.Owner);
    }

    /// <summary>
    /// 减少能力层数（减 1）。
    /// </summary>
    public static async Task Decrement(PowerModel power)
    {
        int num = await ModifyAmount(new ThrowingPlayerChoiceContext(), power, -1M, null, null);
    }

    /// <summary>
    /// 修改能力层数。
    /// </summary>
    public static async Task<int> ModifyAmount(
        PlayerChoiceContext choiceContext,
        PowerModel power,
        Decimal offset,
        Creature? applier,
        CardModel? cardSource,
        bool silent = false)
    {
        return await PowerCmd.ModifyAmount(choiceContext, power, offset, applier, cardSource, silent);
    }
}
