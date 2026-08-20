using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using UltraLib.Hook;

namespace UltraLib.Variables;

/// <summary>
/// Base class for the Rose-family dynamic variables, providing the shared rose-value
/// modification pipeline.
/// </summary>
/// <remarks>
/// 玫瑰（Rose）系列动态变量的基类 —— 提供通用的玫瑰值修正管线。
/// <para>
/// 玫瑰值的最终数值 = (基础值 + 加算修正) × 乘算修正 → 管线修正。
/// 这些修正通过 <see cref="PlusHooks.Plus_TriggerModifyRoseCardAddictive"/>、
/// <see cref="PlusHooks.Plus_TriggerModifyRoseCardMultiplicative"/> 和
/// <see cref="PlusHooks.Plus_TriggerModifyRoseCard"/> 三个 Hook 实现。
/// </para>
/// </remarks>
public static class RoseHelper
{
    /// <summary>
    /// Runs the rose-value modification pipeline on the given amount.
    /// </summary>
    /// <remarks>
    /// 对指定数额执行玫瑰值修正管线。
    /// </remarks>
    /// <param name="amount">原始数值。</param>
    /// <param name="card">关联的卡牌。</param>
    /// <returns>修正后的数值。</returns>
    public static decimal GetResolvedRoseAmount(decimal amount, CardModel card)
    {
        amount += PlusHooks.Plus_TriggerModifyRoseCardAddictive(amount, card.Owner, card);
        amount *= PlusHooks.Plus_TriggerModifyRoseCardMultiplicative(amount, card.Owner, card);
        amount = PlusHooks.Plus_TriggerModifyRoseCard(amount, card.Owner, card);
        return amount;
    }
}

/// <summary>
/// Rose block dynamic variable.
/// </summary>
/// <remarks>
/// 玫瑰格挡值动态变量。
/// </remarks>
public class RoseBlockVar : DynamicVar
{
    /// <summary>
    /// Default variable name.
    /// </summary>
    /// <remarks>
    /// 默认变量名。
    /// </remarks>
    public const string DefaultName = "RoseBlock";

    /// <summary>
    /// Property flags of the block value.
    /// </summary>
    /// <remarks>
    /// 格挡值的属性标记。
    /// </remarks>
    public ValueProp Props { get; }

    /// <summary>
    /// Rose base value (before modification).
    /// </summary>
    /// <remarks>
    /// 玫瑰基础值（修正前）。
    /// </remarks>
    public decimal RoseBaseValue { get; set; }

    /// <summary>
    /// Creates a rose block dynamic variable.
    /// </summary>
    /// <remarks>
    /// 创建玫瑰格挡值动态变量。
    /// </remarks>
    public RoseBlockVar(string name, decimal block, ValueProp props)
        : base(name, block)
    {
        Props = props;
    }

    /// <inheritdoc />
    public override void UpdateCardPreview(
        CardModel card,
        CardPreviewMode previewMode,
        Creature? target,
        bool runGlobalHooks)
    {
        decimal originalBlock = RoseHelper.GetResolvedRoseAmount(BaseValue, card);
        RoseBaseValue = originalBlock;

        // 处理附魔加成
        EnchantmentModel enchantment = card.Enchantment;
        if (enchantment != null)
        {
            decimal enchanted = originalBlock + enchantment.EnchantBlockAdditive(originalBlock);
            originalBlock = enchanted * enchantment.EnchantBlockMultiplicative(enchanted);
            if (!card.IsEnchantmentPreview)
                EnchantedValue = originalBlock;
        }

        if (runGlobalHooks)
            originalBlock = MegaCrit.Sts2.Core.Hooks.Hook.ModifyBlock(
                card.CombatState, card.Owner.Creature,
                originalBlock, Props, card, null, out _);

        PreviewValue = originalBlock;
    }
}

/// <summary>
/// Rose card-count dynamic variable.
/// </summary>
/// <remarks>
/// 玫瑰卡牌数动态变量。
/// </remarks>
public class RoseCardsVar : DynamicVar
{
    /// <summary>
    /// Default variable name.
    /// </summary>
    /// <remarks>
    /// 默认变量名。
    /// </remarks>
    public const string DefaultName = "RoseCards";

    /// <summary>
    /// Rose base value (before modification).
    /// </summary>
    /// <remarks>
    /// 玫瑰基础值（修正前）。
    /// </remarks>
    public decimal RoseBaseValue { get; set; }

    /// <summary>
    /// Creates a rose card-count dynamic variable.
    /// </summary>
    /// <remarks>
    /// 创建玫瑰卡牌数动态变量。
    /// </remarks>
    public RoseCardsVar(string name, int cards)
        : base(name, cards) { }

    /// <inheritdoc />
    public override void UpdateCardPreview(
        CardModel card,
        CardPreviewMode previewMode,
        Creature? target,
        bool runGlobalHooks)
    {
        decimal amount = RoseHelper.GetResolvedRoseAmount(BaseValue, card);
        RoseBaseValue = amount;
        PreviewValue = amount;
    }
}

/// <summary>
/// Rose generic numeric dynamic variable.
/// </summary>
/// <remarks>
/// 玫瑰通用数值动态变量。
/// </remarks>
public class RoseDynamicVar : DynamicVar
{
    /// <summary>
    /// Rose base value (before modification).
    /// </summary>
    /// <remarks>
    /// 玫瑰基础值（修正前）。
    /// </remarks>
    public decimal RoseBaseValue { get; set; }

    /// <summary>
    /// Creates a rose generic numeric dynamic variable.
    /// </summary>
    /// <remarks>
    /// 创建玫瑰通用数值动态变量。
    /// </remarks>
    public RoseDynamicVar(string name, decimal amount)
        : base(name, amount) { }

    /// <inheritdoc />
    public override void UpdateCardPreview(
        CardModel card,
        CardPreviewMode previewMode,
        Creature? target,
        bool runGlobalHooks)
    {
        decimal amount = RoseHelper.GetResolvedRoseAmount(BaseValue, card);
        RoseBaseValue = amount;
        PreviewValue = amount;
    }
}

/// <summary>
/// Rose summon dynamic variable.
/// </summary>
/// <remarks>
/// 玫瑰召唤值动态变量。
/// </remarks>
public class RoseSummonVar : DynamicVar
{
    /// <summary>
    /// Default variable name.
    /// </summary>
    /// <remarks>
    /// 默认变量名。
    /// </remarks>
    public const string DefaultName = "RoseSummon";

    /// <summary>
    /// Rose base value (before modification).
    /// </summary>
    /// <remarks>
    /// 玫瑰基础值（修正前）。
    /// </remarks>
    public decimal RoseBaseValue { get; set; }

    /// <summary>
    /// Creates a rose summon dynamic variable.
    /// </summary>
    /// <remarks>
    /// 创建玫瑰召唤值动态变量。
    /// </remarks>
    public RoseSummonVar(decimal summonAmount)
        : base("Summon", summonAmount) { }

    /// <summary>
    /// Creates a rose summon dynamic variable (with a custom name).
    /// </summary>
    /// <remarks>
    /// 创建玫瑰召唤值动态变量（自定义名称）。
    /// </remarks>
    public RoseSummonVar(string name, decimal summonAmount)
        : base(name, summonAmount) { }

    /// <inheritdoc />
    public override void UpdateCardPreview(
        CardModel card,
        CardPreviewMode previewMode,
        Creature? target,
        bool runGlobalHooks)
    {
        decimal amount = RoseHelper.GetResolvedRoseAmount(BaseValue, card);
        RoseBaseValue = amount;
        if (!runGlobalHooks)
            return;

        PreviewValue = MegaCrit.Sts2.Core.Hooks.Hook.ModifySummonAmount(
            card.CombatState, card.Owner, amount, card);
    }
}

/// <summary>
/// Rose ebony damage dynamic variable.
/// </summary>
/// <remarks>
/// 玫瑰黑檀伤害动态变量。
/// </remarks>
public class RoseOstyDamageVar : DynamicVar
{
    /// <summary>
    /// Default variable name.
    /// </summary>
    /// <remarks>
    /// 默认变量名。
    /// </remarks>
    public const string DefaultName = "RoseOstyDamage";

    /// <summary>
    /// Damage property flags.
    /// </summary>
    /// <remarks>
    /// 伤害属性标记。
    /// </remarks>
    public ValueProp Props { get; set; }

    /// <summary>
    /// Rose base value (before modification).
    /// </summary>
    /// <remarks>
    /// 玫瑰基础值（修正前）。
    /// </remarks>
    public decimal RoseBaseValue { get; set; }

    /// <summary>
    /// Creates a rose ebony damage dynamic variable.
    /// </summary>
    /// <remarks>
    /// 创建玫瑰黑檀伤害动态变量。
    /// </remarks>
    public RoseOstyDamageVar(decimal damage, ValueProp props)
        : base("OstyDamage", damage)
    {
        Props = props;
    }

    /// <summary>
    /// Creates a rose ebony damage dynamic variable (with a custom name).
    /// </summary>
    /// <remarks>
    /// 创建玫瑰黑檀伤害动态变量（自定义名称）。
    /// </remarks>
    public RoseOstyDamageVar(string name, decimal damage, ValueProp props)
        : base(name, damage)
    {
        Props = props;
    }

    /// <inheritdoc />
    public override void UpdateCardPreview(
        CardModel card,
        CardPreviewMode previewMode,
        Creature? target,
        bool runGlobalHooks)
    {
        decimal originalDamage = RoseHelper.GetResolvedRoseAmount(BaseValue, card);
        RoseBaseValue = originalDamage;

        EnchantmentModel enchantment = card.Enchantment;
        if (enchantment != null)
        {
            decimal enchanted = originalDamage + enchantment.EnchantDamageAdditive(originalDamage, Props);
            originalDamage = enchanted * enchantment.EnchantDamageMultiplicative(enchanted, Props);
            if (!card.IsEnchantmentPreview)
                EnchantedValue = originalDamage;
        }

        if (runGlobalHooks)
        {
            ICombatState combatState = card.CombatState ?? card.Owner.Creature.CombatState;
            originalDamage = MegaCrit.Sts2.Core.Hooks.Hook.ModifyDamage(card.Owner.RunState, combatState, target, card.Owner.Osty, this.BaseValue, this.Props, card, (CardPlay) null, ModifyDamageHookType.All, previewMode, out IEnumerable<AbstractModel> _);

        }

        PreviewValue = originalDamage;
    }
}

/// <summary>
/// Rose power-grant dynamic variable (generic).
/// </summary>
/// <remarks>
/// 玫瑰能力赋予动态变量（泛型）。
/// </remarks>
/// <typeparam name="T">要赋予的能力类型。</typeparam>
public class RosePowerVar<T> : DynamicVar where T : PowerModel
{
    /// <summary>
    /// Rose base value (before modification).
    /// </summary>
    /// <remarks>
    /// 玫瑰基础值（修正前）。
    /// </remarks>
    public decimal RoseBaseValue { get; set; }

    /// <summary>
    /// Creates a rose power-grant dynamic variable.
    /// </summary>
    /// <remarks>
    /// 创建玫瑰能力赋予动态变量。
    /// </remarks>
    public RosePowerVar(decimal powerAmount)
        : base(typeof(T).Name, powerAmount) { }

    /// <summary>
    /// Creates a rose power-grant dynamic variable (with a custom name).
    /// </summary>
    /// <remarks>
    /// 创建玫瑰能力赋予动态变量（自定义名称）。
    /// </remarks>
    public RosePowerVar(string name, decimal powerAmount)
        : base(name, powerAmount) { }

    /// <inheritdoc />
    public override void UpdateCardPreview(
        CardModel card,
        CardPreviewMode previewMode,
        Creature? target,
        bool runGlobalHooks)
    {
        decimal amount = RoseHelper.GetResolvedRoseAmount(BaseValue, card);
        RoseBaseValue = amount;

        if (!runGlobalHooks)
            return;

        PreviewValue = MegaCrit.Sts2.Core.Hooks.Hook.ModifyPowerAmountGiven(
            card.CombatState, ModelDb.Power<T>(), card.Owner.Creature,
            amount, target, card, out _);
    }
}
