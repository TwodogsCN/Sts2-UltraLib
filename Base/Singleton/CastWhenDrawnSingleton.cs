using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using UltraLib.Base.Abstract;
using UltraLib.Base.Label.Card;
using UltraLib.Base.Utils;
using UltraLib.Hook;

namespace UltraLib.Base.Singleton;

/// <summary>
/// 抽牌时施放（CastWhenDrawn）单例。
/// <para>
/// 当带有 <see cref="PlusCardKeyWord.CastWhenDrawn"/> 关键词的卡牌被抽到手中时，
/// 自动将其打出并再抽一张牌以补偿消耗。
/// </para>
/// </summary>
public class CastWhenDrawnSingleton() : PlusSingletonModel(CustomSingletonModel.HookType.Combat)
{
    /// <summary>
    /// 在卡牌被抽入手中后触发。
    /// 如果卡牌带有 CastWhenDrawn 关键词，自动打出并补牌。
    /// </summary>
    /// <param name="choiceContext">玩家选择上下文。</param>
    /// <param name="card">被抽到的卡牌。</param>
    /// <param name="fromHandDraw">是否是从手牌中抽出。</param>
    public override async Task AfterCardDrawn(
        PlayerChoiceContext choiceContext,
        CardModel card,
        bool fromHandDraw)
    {
        // 仅处理带有 CastWhenDrawn 关键词的卡牌
        if (!card.Keywords.Contains(PlusCardKeyWord.CastWhenDrawn))
            return;

        // 触发抽牌时施放前 Hook
        await PlusHooks.Plus_TriggerBeforeCastWhenDrawn(choiceContext, card);

        // 获取自动目标（如果卡牌需要目标）
        var target = CardHelper.GetAutoTarget(card, card.CombatState);

        // 自动打出卡牌（跳过 X 费用捕获）
        await CardCmd.AutoPlay(choiceContext, card, target, skipXCapture: true);

        // 补偿性地再抽一张牌
        await CardPileCmd.Draw(choiceContext, card.Owner);

        // 触发抽牌时施放后 Hook
        await PlusHooks.Plus_TriggerAfterCastWhenDrawn(choiceContext, card);
    }
}
