using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;

namespace UltraLib.Base.Utils;

/// <summary>
/// Card list operation helpers.
/// </summary>
/// <remarks>
/// 卡牌列表操作辅助工具。
/// 提供从牌堆取牌、选择、筛选、随机排序等功能。
/// </remarks>
public static class CardListHelper
{
    /// <summary>
    /// Gets all cards from the player's specified pile.
    /// </summary>
    /// <remarks>
    /// 从玩家指定牌堆获取所有卡牌。
    /// </remarks>
    public static List<CardModel> FromPile(Player owner, PileType pileType)
        => pileType.GetPile(owner).Cards.ToList();

    /// <summary>
    /// Selects up to maxCount cards from the hand (excluding the source card).
    /// </summary>
    /// <remarks>
    /// 从手牌中选择最多 maxCount 张牌（排除源卡牌 card）。
    /// </remarks>
    public static async Task<List<CardModel>> SelectCardFromHand(
        PlayerChoiceContext choiceContext,
        Player player,
        LocString prompt,
        int minCount,
        int maxCount,
        CardModel card)
    {
        return (await CardSelectCmd.FromHand(
            choiceContext,
            player,
            new CardSelectorPrefs(prompt, minCount, maxCount),
            null,
            card
        )).ToList();
    }

    /// <summary>
    /// Selects the given number of cards from a list.
    /// </summary>
    /// <remarks>
    /// 从列表中选择指定数量的卡牌。
    /// </remarks>
    public static async Task<List<CardModel>> SelectCardFromList(
        PlayerChoiceContext choiceContext,
        IReadOnlyList<CardModel> cardsIn,
        Player player,
        LocString prompt,
        int minCount,
        int maxCount)
    {
        return (await CardSelectCmd.FromSimpleGrid(
            choiceContext,
            cardsIn,
            player,
            new CardSelectorPrefs(prompt, minCount, maxCount)
        )).ToList();
    }

    /// <summary>
    /// Selects count cards from the hand matching the given filter.
    /// </summary>
    /// <remarks>
    /// 从手牌中选择 count 张符合筛选条件的卡牌。
    /// </remarks>
    public static async Task<List<CardModel>> SelectCardFromHand(
        PlayerChoiceContext choiceContext,
        Player player,
        LocString prompt,
        int count,
        Func<CardModel, bool>? filter,
        AbstractModel source)
    {
        return (await CardSelectCmd.FromHand(
            choiceContext,
            player,
            new CardSelectorPrefs(prompt, count),
            filter,
            source
        )).ToList();
    }

    /// <summary>
    /// Selects cards from a specified pile (ordered).
    /// </summary>
    /// <remarks>
    /// 从指定牌堆中选择卡牌（有序）。
    /// </remarks>
    public static async Task<List<CardModel>> SelectCardFromPile(
        PlayerChoiceContext choiceContext,
        Player player,
        PileType pile,
        LocString prompt,
        int count,
        CardModel? cardSource = null,
        bool isOrdered = false,
        Func<CardModel, bool>? filter = null)
    {
        var cardList = FromPile(player, pile);
        if (isOrdered)
            cardList = cardList.OrderBy(c => c.Rarity).ThenBy(c => c.Id).ToList();

        if (cardList.Count == 0) return [];

        return (await CardSelectCmd.FromSimpleGrid(
            choiceContext, cardList, player, new CardSelectorPrefs(prompt, count)
        )).ToList();
    }

    /// <summary>
    /// Selects min~max cards from a specified pile (ordered).
    /// </summary>
    /// <remarks>
    /// 从指定牌堆中选择 min~max 张牌（有序）。
    /// </remarks>
    public static async Task<List<CardModel>> SelectCardFromPile(
        PlayerChoiceContext choiceContext,
        Player player,
        PileType pile,
        LocString prompt,
        int minCount,
        int maxCount,
        CardModel? cardSource = null,
        bool isOrdered = false)
    {
        var cardList = FromPile(player, pile);
        if (isOrdered)
            cardList = cardList.OrderBy(c => c.Rarity).ThenBy(c => c.Id).ToList();

        if (cardList.Count == 0) return [];

        return (await CardSelectCmd.FromSimpleGrid(
            choiceContext, cardList, player, new CardSelectorPrefs(prompt, minCount, maxCount)
        )).ToList();
    }

    /// <summary>
    /// Filters cards by rarity (cmp: 0 = equal, &lt;0 = or below, &gt;0 = or above).
    /// </summary>
    /// <remarks>
    /// 按稀有度筛选（cmp: 0=等于, &lt;0=小于等于, &gt;0=大于等于）。
    /// </remarks>
    public static List<CardModel> Filter(CardRarity rarity, List<CardModel> cardList, int cmp = 0)
    {
        return cmp switch
        {
            0 => cardList.Where(c => c.Rarity == rarity).ToList(),
            < 0 when rarity == CardRarity.Rare => cardList.ToList(),
            < 0 when rarity == CardRarity.Uncommon => cardList.Where(c => c.Rarity == rarity || c.Rarity == CardRarity.Common).ToList(),
            < 0 when rarity == CardRarity.Common => cardList.Where(c => c.Rarity == rarity).ToList(),
            > 0 when rarity == CardRarity.Rare => cardList.Where(c => c.Rarity == rarity).ToList(),
            > 0 when rarity == CardRarity.Uncommon => cardList.Where(c => c.Rarity == rarity || c.Rarity == CardRarity.Rare).ToList(),
            > 0 when rarity == CardRarity.Common => cardList.ToList(),
            _ => cardList
        };
    }

    /// <summary>
    /// Filters cards by energy cost (cmp: 0 = equal, &lt;0 = or below, &gt;0 = or above).
    /// </summary>
    /// <remarks>
    /// 按能量费用筛选（cmp: 0=等于, &lt;0=小于等于, &gt;0=大于等于）。
    /// </remarks>
    public static List<CardModel> Filter(int energyCost, List<CardModel> cardList, int cmp = 0)
    {
        return cmp switch
        {
            0 => cardList.Where(c => c.EnergyCost.GetResolved() == energyCost).ToList(),
            < 0 => cardList.Where(c => c.EnergyCost.GetResolved() <= energyCost).ToList(),
            > 0 => cardList.Where(c => c.EnergyCost.GetResolved() >= energyCost).ToList()
        };
    }

    /// <summary>
    /// Shuffles a card list using the creature's combat RNG.
    /// </summary>
    /// <remarks>
    /// 使用生物所属战斗的 RNG 打乱卡牌列表。
    /// </remarks>
    public static List<CardModel> RandomizeOrder(Creature creature, List<CardModel> cardList)
    {
        if (creature.CombatState == null) return cardList;
        return cardList.UnstableShuffle(creature.CombatState.RunState.Rng.Shuffle);
    }

    /// <summary>
    /// Shuffles a card list using the given RNG.
    /// </summary>
    /// <remarks>
    /// 使用指定 RNG 打乱卡牌列表。
    /// </remarks>
    public static List<CardModel> RandomizeOrder(Rng rng, List<CardModel> cardList)
        => cardList.UnstableShuffle(rng);
}
