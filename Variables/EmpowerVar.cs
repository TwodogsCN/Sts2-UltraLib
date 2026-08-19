using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace UltraLib.Variables;

/// <summary>
/// 赋能（Empower）动态变量。
/// <para>
/// 为卡牌注入指定的能力（Power），在打出时使目标获得该能力的若干层数。
/// </para>
/// </summary>
public class EmpowerVar : DynamicVar
{
    /// <summary>
    /// 在 CardModel.DynamicVars 中使用的键名。
    /// </summary>
    public const string Key = "Empower";

    /// <summary>
    /// 要赋予目标的能力类型。
    /// </summary>
    public PowerModel Power { get; private set; }

    /// <summary>
    /// 能力的层数。
    /// </summary>
    public int PowerCount => IntValue;

    /// <summary>
    /// 创建一个赋能动态变量。
    /// </summary>
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