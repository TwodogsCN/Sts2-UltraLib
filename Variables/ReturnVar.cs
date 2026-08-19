using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace UltraLib.Variables;

/// <summary>
/// 返回（Return）动态变量。
/// <para>
/// 使卡牌在每回合前 N 次打出时返回手牌，而非进入弃牌堆。
/// 计数值随每回合的打出次数递减。
/// </para>
/// </summary>
public class ReturnVar : DynamicVar
{
    /// <summary>
    /// 在 CardModel.DynamicVars 中使用的键名。
    /// </summary>
    public const string Key = "Return";

    /// <summary>
    /// 创建一个返回动态变量。
    /// </summary>
    /// <param name="returnCount">每回合可以返回的次数。</param>
    public ReturnVar(decimal returnCount) : base(Key, returnCount)
    {
        // 启用 Tooltip 支持，允许在 HoverTip 中显示动态值
        this.WithTooltip();
    }

    /// <inheritdoc />
    public override void UpdateCardPreview(
        CardModel card,
        CardPreviewMode previewMode,
        Creature? target,
        bool runGlobalHooks)
    {
        // 预览时显示当前剩余的返回次数
        PreviewValue = ReturnCount(card, IntValue);
    }

    /// <summary>
    /// 计算当前剩余的返回次数（基础次数 - 本回合已打出次数）。
    /// </summary>
    /// <param name="card">要计算的卡牌。</param>
    /// <param name="basePersist">基础返回次数。</param>
    /// <returns>剩余的返回次数，最少为 0。</returns>
    public static int ReturnCount(CardModel card, int basePersist)
    {
        // 统计本回合内该卡牌已完成打出的次数
        int playCount = CombatManager.Instance.History.CardPlaysFinished.Count(entry =>
            BetaMainCompatibility.CardModel_.WrappedCombatState(card)?.HappenedThisTurn(entry) == true
            && entry.CardPlay.Card == card);

        return Math.Max(0, basePersist - playCount);
    }
}
