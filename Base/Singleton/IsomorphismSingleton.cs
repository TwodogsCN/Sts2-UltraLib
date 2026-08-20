using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using UltraLib.Base.Abstract;
using UltraLib.Base.Label.Card;
using UltraLib.Base.Utils;
using UltraLib.Hook;

namespace UltraLib.Base.Singleton;

/// <summary>
/// Isomorphism singleton.
/// </summary>
/// <remarks>
/// 同构（Isomorphism）单例。
/// <para>
/// 当手牌中存在两张带有 <see cref="PlusCardKeyWord.Isomorphism"/> 关键词的卡牌
/// 且间隔一张卡牌（即索引 i 和 i+2 均有 Isomorphism）时，
/// 自动打出中间的那张卡牌。
/// </para>
/// <para>
/// 触发时机：
/// <list type="bullet">
///   <item>卡牌变更牌堆时（<see cref="AfterCardChangedPiles"/>）</item>
///   <item>手牌移动后（通过 <see cref="IPlusHooks.Plus_AfterHandPileMoved"/>）</item>
/// </list>
/// </para>
/// </remarks>
public class IsomorphismSingleton() : PlusSingletonModel(CustomSingletonModel.HookType.Combat)
{
    /// <summary>
    /// Fired when a card changes piles; triggers isomorphism detection.
    /// </summary>
    /// <remarks>
    /// 卡牌变更牌堆后触发同构检测。
    /// </remarks>
    /// <param name="card">变更牌堆的卡牌。</param>
    /// <param name="oldPileType">原牌堆类型。</param>
    /// <param name="source">变更来源。</param>
    public override async Task AfterCardChangedPiles(
        CardModel card,
        PileType oldPileType,
        AbstractModel? source)
    {
        await ExecuteIsomorphismLogic(card);
    }

    /// <summary>
    /// Fired after the hand pile moves; triggers isomorphism detection.
    /// </summary>
    /// <remarks>
    /// 手牌移动后触发同构检测。
    /// </remarks>
    /// <param name="card">被移动的卡牌。</param>
    public override async Task Plus_AfterHandPileMoved(CardModel card)
    {
        await ExecuteIsomorphismLogic(card);
    }

    /// <summary>
    /// Core method that executes the isomorphism logic.
    /// </summary>
    /// <remarks>
    /// 执行同构逻辑的核心方法。
    /// <para>
    /// 扫描手牌，找到所有满足「i 和 i+2 均有 Isomorphism」的中间卡牌，
    /// 将它们自动打出。
    /// </para>
    /// </remarks>
    /// <param name="triggerCard">触发检测的卡牌（用于定位玩家和战斗状态）。</param>
    private static async Task ExecuteIsomorphismLogic(CardModel triggerCard)
    {
        // 获取当前手牌列表
        List<CardModel> hand = CardListHelper.FromPile(triggerCard.Owner, PileType.Hand);

        // 手牌不足 3 张则无法形成「两张 Isomorphism 夹一张」的模式
        if (hand.Count < 3) return;

        // 找出所有满足同构模式的中间卡牌
        List<CardModel> cardsToPlay = new List<CardModel>();

        for (int i = 0; i <= hand.Count - 3; i++)
        {
            if (hand[i].Keywords.Contains(PlusCardKeyWord.Isomorphism) &&
                hand[i + 2].Keywords.Contains(PlusCardKeyWord.Isomorphism))
            {
                CardModel targetCard = hand[i + 1];
                if (!cardsToPlay.Contains(targetCard))
                {
                    cardsToPlay.Add(targetCard);
                }
            }
        }

        // 逐张自动打出满足条件的中间卡牌
        foreach (var cardToPlay in cardsToPlay)
        {
            // 确保卡牌仍在手牌中
            if (cardToPlay.Pile.Type != PileType.Hand)
                continue;

            // 触发同构前 Hook
            await PlusHooks.Plus_TriggerBeforeIsomorphism(cardToPlay);

            // 自动打出（获取自动目标，跳过 X 费用捕获）
            var target = CardHelper.GetAutoTarget(cardToPlay, cardToPlay.CombatState);
            await CardCmd.AutoPlay(new BlockingPlayerChoiceContext(), cardToPlay, target, skipXCapture: true);

            // 触发同构后 Hook
            await PlusHooks.Plus_TriggerAfterIsomorphism(cardToPlay);
        }
    }
}
