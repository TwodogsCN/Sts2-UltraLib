using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace UltraLib.Base.Utils;

/// <summary>
/// 生物（Creature）操作辅助工具。
/// 封装 CreatureCmd 的常用操作，提供格挡、伤害、治疗、动画触发等便捷方法。
/// </summary>
public static class CreatureHelper
{
    /// <summary>触发生物的指定动画。</summary>
    public static async Task TriggerAnim(Creature creature, string triggerName, float waitTime)
        => await CreatureCmd.TriggerAnim(creature, triggerName, waitTime);

    /// <summary>触发玩家的施法动画。</summary>
    public static async Task CastCardOnCreature(Player player)
        => await CreatureCmd.TriggerAnim(player.Creature, "Cast", player.Character.CastAnimDelay);

    /// <summary>为生物增加格挡值。</summary>
    public static async Task<decimal> GainBlock(Creature creature, BlockVar blockVar, CardPlay? cardPlay, bool fast = false)
        => await CreatureCmd.GainBlock(creature, blockVar.BaseValue, blockVar.Props, cardPlay, fast);

    /// <summary>为生物增加格挡值（直接指定数值）。</summary>
    public static async Task<decimal> GainBlock(Creature creature, decimal amount, ValueProp props, CardPlay? cardPlay, bool fast = false)
        => await CreatureCmd.GainBlock(creature, amount, props, cardPlay, fast);

    /// <summary>
    /// 对指定生物造成伤害。
    /// </summary>
    /// <param name="choiceContext">玩家选择上下文。</param>
    /// <param name="target">伤害目标。</param>
    /// <param name="amount">伤害数值。</param>
    /// <param name="props">伤害属性标记。</param>
    /// <param name="dealer">造成伤害的生物（可为 null）。</param>
    /// <param name="cardSource">来源卡牌（可为 null）。</param>
    /// <param name="cardPlay">来源的卡牌打出记录（可为 null）。</param>
    /// <returns>伤害结果列表。</returns>
    public static async Task<IEnumerable<DamageResult>> Damage(
        PlayerChoiceContext choiceContext,
        Creature target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        return await CreatureCmd.Damage(choiceContext, [target], amount, props, dealer, cardSource, cardPlay);
    }

    /// <summary>
    /// 对指定生物列表造成伤害（使用 DamageVar）。
    /// </summary>
    /// <param name="choiceContext">玩家选择上下文。</param>
    /// <param name="targets">伤害目标列表。</param>
    /// <param name="damageVar">伤害变量（含基础值和属性标记）。</param>
    /// <param name="dealer">造成伤害的生物（可为 null）。</param>
    /// <param name="cardSource">来源卡牌（可为 null）。</param>
    /// <param name="cardPlay">来源的卡牌打出记录（可为 null）。</param>
    /// <returns>伤害结果列表。</returns>
    public static async Task<IEnumerable<DamageResult>> Damage(
        PlayerChoiceContext choiceContext,
        IEnumerable<Creature>? targets,
        DamageVar damageVar,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        return await CreatureCmd.Damage(choiceContext, targets, damageVar.BaseValue, damageVar.Props, dealer, cardSource, cardPlay);
    }

    /// <summary>
    /// 增加最大生命值，并可选择是否回血。
    /// </summary>
    /// <exception cref="ArgumentException">如果 amount 为负数。</exception>
    public static async Task GainMaxHpAndHeal(Creature creature, decimal amount, bool isHeal = true)
    {
        if (amount < 0m)
            throw new ArgumentException("amount must be non-negative. Use LoseMaxHp for max HP loss.", nameof(amount));

        decimal gained = await CreatureCmd.SetMaxHp(creature, creature.MaxHp + amount);

        var pointEntry = creature.Player?.RunState.CurrentMapPointHistoryEntry;
        if (pointEntry != null)
            pointEntry.GetEntry(creature.Player.NetId).MaxHpGained += (int)gained;

        if (isHeal)
            await CreatureCmd.Heal(creature, gained);
    }

    /// <summary>治疗生物。</summary>
    public static async Task Heal(Creature creature, decimal amount)
        => await CreatureCmd.Heal(creature, amount);
}
