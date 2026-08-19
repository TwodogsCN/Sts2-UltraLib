using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace UltraLib.Base.Utils;

/// <summary>
/// 发现（Discover）机制辅助工具。
/// <para>
/// 提供从多个卡池中随机选取卡牌供玩家选择的便捷方法，
/// 类似于炉石传说中的「发现」机制。
/// </para>
/// </summary>
public static class DiscoverHelper
{
    /// <summary>
    /// 从指定的卡池列表中随机展示若干卡牌供玩家选择。
    /// </summary>
    /// <param name="choiceContext">玩家选择上下文。</param>
    /// <param name="player">进行发现的玩家。</param>
    /// <param name="cardPools">可选的卡池列表。</param>
    /// <param name="cardPlay">来源的卡牌打出记录（可为 null）。</param>
    /// <param name="filter">卡牌筛选条件。</param>
    /// <param name="isUpgrade">是否将展示的卡牌升级。</param>
    /// <param name="canSkip">是否允许跳过（不选）。</param>
    /// <returns>玩家选择的卡牌，如果跳过则返回 null。</returns>
    public static async Task<CardModel?> Discover(
        PlayerChoiceContext choiceContext,
        Player player,
        List<CardPoolModel> cardPools,
        CardPlay? cardPlay,
        Func<CardModel, bool>? filter,
        bool isUpgrade = false,
        bool canSkip = false)
    {
        const int selectAmount = 3;

        // 从所有卡池中获取解锁的卡牌
        IEnumerable<CardModel> cards = cardPools
            .SelectMany(p => p.GetUnlockedCards(player.UnlockState, player.RunState.CardMultiplayerConstraint))
            .ToList();

        // 应用筛选条件
        cards = cards.Where(filter ?? (_ => true)).ToList();

        // 随机选取去重后的卡牌
        List<CardModel> distinctCards = CardFactory
            .GetDistinctForCombat(player, cards, selectAmount, player.RunState.Rng.CombatCardGeneration)
            .ToList();

        // 可选：升级
        if (isUpgrade)
            CardHelper.Upgrade(distinctCards);

        // 展示选择界面
        return await CardSelectCmd.FromChooseACardScreen(choiceContext, distinctCards, player, canSkip);
    }
}
