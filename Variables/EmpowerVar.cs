using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace UltraLib.Variables;

/// <summary>
/// Empower dynamic variable.
/// </summary>
/// <remarks>
/// 赋能（Empower）动态变量。
/// <para>
/// 为卡牌注入指定的能力（Power），在打出时使目标获得该能力的若干层数。
/// </para>
/// </remarks>
public class EmpowerVar : DynamicVar
{
    /// <summary>
    /// Key name used in <c>CardModel.DynamicVars</c>.
    /// </summary>
    /// <remarks>
    /// 在 CardModel.DynamicVars 中使用的键名。
    /// </remarks>
    public const string Key = "Empower";

    /// <summary>
    /// The power type to grant to the target.
    /// </summary>
    /// <remarks>
    /// 要赋予目标的能力类型。
    /// </remarks>
    public PowerModel Power { get; private set; }

    /// <summary>
    /// The number of stacks of the power.
    /// </summary>
    /// <remarks>
    /// 能力的层数。
    /// </remarks>
    public int PowerCount => IntValue;

    /// <summary>
    /// Creates an empower dynamic variable.
    /// </summary>
    /// <remarks>
    /// 创建一个赋能动态变量。
    /// </remarks>
    /// <param name="power">要赋予的能力模型。</param>
    /// <param name="count">能力层数。</param>
    /// <exception cref="ArgumentNullException">如果 power 为 null。</exception>
    public EmpowerVar(PowerModel power, decimal count) : base(Key, count)
    {
        Power = power ?? throw new ArgumentNullException(nameof(power));
        this.WithTooltip();
    }

    /// <inheritdoc />
    public override void UpdateCardPreview(
        CardModel card,
        CardPreviewMode previewMode,
        Creature? target,
        bool runGlobalHooks)
    {
        PreviewValue = IntValue;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        if (Power == null)
        {
            var loc = new LocString("static_hover_tips", "POWER.base.title");
            return "X " + loc.GetFormattedText();
        }
            
        return IntValue.ToString() + ' ' + Power.Title.GetFormattedText();
    }
}