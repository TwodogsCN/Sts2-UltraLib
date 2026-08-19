using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace UltraLib.Base.Utils;

/// <summary>
/// 牌堆操作辅助工具，封装 <see cref="CardPileCmd"/> 的常用操作。
/// </summary>
public static class CardPileHelper
{
    /// <summary>
    /// 将指定卡牌添加到指定牌堆的指定位置。
    /// </summary>
    /// <param name="card">要添加的卡牌。</param>
    /// <param name="pile">目标牌堆类型。</param>
    /// <param name="position">在牌堆中的位置（顶部/底部/随机）。</param>
    /// <returns>添加操作的结果。</returns>
    public static async Task<CardPileAddResult> AddToPile(CardModel card, PileType pile, CardPilePosition position)
        => await CardPileCmd.Add(card, pile, position);

    /// <summary>
    /// 使用游戏 RNG 随机打乱指定牌堆的顺序。
    /// </summary>
    /// <param name="cardPile">要打乱的牌堆类型。</param>
    /// <param name="player">牌堆的所属玩家。</param>
    public static void RandomizeOrderForPile(PileType cardPile, Player player)
        => cardPile.GetPile(player).RandomizeOrderInternal(
            player, player.RunState.Rng.Shuffle, CombatManager.Instance.DebugOnlyGetState());

    /// <summary>
    /// 从抽牌堆抽牌。
    /// </summary>
    /// <param name="choiceContext">玩家选择上下文。</param>
    /// <param name="count">抽牌数量。</param>
    /// <param name="player">抽牌的玩家。</param>
    /// <param name="fromHandDraw">是否为从手牌中抽出（即回收）。</param>
    /// <returns>抽取的卡牌列表。</returns>
    public static async Task<IEnumerable<CardModel>> Draw(
        PlayerChoiceContext choiceContext,
        decimal count,
        Player player,
        bool fromHandDraw = false)
        => await CardPileCmd.Draw(choiceContext, count, player, fromHandDraw);
}
