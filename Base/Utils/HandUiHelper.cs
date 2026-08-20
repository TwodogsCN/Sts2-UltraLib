using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using UltraLib.Hook;

namespace UltraLib.Base.Utils;

/// <summary>
/// Hand-UI operation helpers.
/// </summary>
/// <remarks>
/// 手牌 UI 操作辅助工具。
/// <para>
/// 由于 StS2 原版手牌的视觉顺序与逻辑顺序不一致，
/// 此类提供了在 <b>逻辑层</b> 操作手牌排列的静态方法，
/// 并自动将变更同步到 UI 视觉层。
/// </para>
/// <para>
/// 所有变更手牌顺序的操作都会触发
/// <see cref="PlusHooks.Plus_TriggerBeforeHandPileMoved"/> 和
/// <see cref="PlusHooks.Plus_TriggerAfterHandPileMoved"/> Hook，
/// 方便其他模块（如 <see cref="IsomorphismSingleton"/>）响应手牌变化。
/// </para>
/// </remarks>
public static class HandUiHelper
{
    /// <summary>
    /// Gets the hand pile's internal <c>_cards</c> list via reflection (raw reference, mutable).
    /// </summary>
    /// <remarks>
    /// 通过反射获取手牌牌堆的内部 _cards 列表（原始引用，可直接修改）。
    /// </remarks>
    /// <param name="owner">手牌持有者（玩家）。</param>
    /// <returns>手牌 CardModel 的内部列表，如果获取失败返回 null。</returns>
    public static List<CardModel>? GetRawCards(Player owner)
    {
        var handPile = owner.PlayerCombatState.Hand;
        var field = AccessTools.Field(handPile.GetType(), "_cards");
        return field?.GetValue(handPile) as List<CardModel>;
    }

    // ==========================================
    // 交换
    // ==========================================

    /// <summary>
    /// Swaps two hand cards by index (logical + UI layers kept in sync).
    /// </summary>
    /// <remarks>
    /// 根据索引交换两张手牌的位置（逻辑层 + UI 层同时同步）。
    /// </remarks>
    /// <param name="owner">手牌持有者（玩家）。</param>
    /// <param name="indexA">第一张牌的索引。</param>
    /// <param name="indexB">第二张牌的索引。</param>
    public static async Task SwapPlayerHandCards(Player owner, int indexA, int indexB)
    {
        var cardsList = GetRawCards(owner);
        if (cardsList == null || indexA == indexB ||
            indexA < 0 || indexA >= cardsList.Count ||
            indexB < 0 || indexB >= cardsList.Count)
            return;

        // 触发交换前 Hook（两张牌都要触发）
        await PlusHooks.Plus_TriggerBeforeHandPileMoved(cardsList[indexA]);
        await PlusHooks.Plus_TriggerBeforeHandPileMoved(cardsList[indexB]);

        // 逻辑层交换
        (cardsList[indexA], cardsList[indexB]) = (cardsList[indexB], cardsList[indexA]);

        // 视觉层同步
        SyncVisuals(owner, cardsList);

        // 触发交换后 Hook
        await PlusHooks.Plus_TriggerAfterHandPileMoved(cardsList[indexA]);
        await PlusHooks.Plus_TriggerAfterHandPileMoved(cardsList[indexB]);
    }

    /// <summary>
    /// Swaps two hand cards by CardModel reference.
    /// </summary>
    /// <remarks>
    /// 根据 CardModel 引用交换两张手牌的位置。
    /// <para>
    /// 会在执行前重新查找索引，防止 Hook 异步操作导致索引偏移。
    /// </para>
    /// </remarks>
    /// <param name="owner">手牌持有者（玩家）。</param>
    /// <param name="cardA">第一张牌。</param>
    /// <param name="cardB">第二张牌。</param>
    public static async Task SwapPlayerHandCards(Player owner, CardModel cardA, CardModel cardB)
    {
        var cardsList = GetRawCards(owner);
        if (cardsList == null || cardA == cardB) return;

        // 实时查找索引
        int indexA = cardsList.IndexOf(cardA);
        int indexB = cardsList.IndexOf(cardB);
        if (indexA == -1 || indexB == -1) return;

        // 触发交换前 Hook
        await PlusHooks.Plus_TriggerBeforeHandPileMoved(cardA);
        await PlusHooks.Plus_TriggerBeforeHandPileMoved(cardB);

        // 重新索引（防止 Hook 期间列表变动）
        indexA = cardsList.IndexOf(cardA);
        indexB = cardsList.IndexOf(cardB);

        if (indexA != -1 && indexB != -1)
        {
            // 逻辑层交换
            (cardsList[indexA], cardsList[indexB]) = (cardsList[indexB], cardsList[indexA]);

            // 视觉层同步
            SyncVisuals(owner, cardsList);
        }

        // 触发交换后 Hook
        await PlusHooks.Plus_TriggerAfterHandPileMoved(cardA);
        await PlusHooks.Plus_TriggerAfterHandPileMoved(cardB);
    }

    // ==========================================
    // 视觉同步
    // ==========================================

    /// <summary>
    /// Aligns the hand UI children order with the logical card list order.
    /// </summary>
    /// <remarks>
    /// 将手牌 UI 层的子节点顺序与逻辑层卡片顺序对齐。
    /// <para>
    /// 通过 <c>MoveChild</c> 调整每个 <c>NHandCardHolder</c> 在容器中的索引，
    /// 使其与 <paramref name="cardsList"/> 中的顺序一致。
    /// </para>
    /// </remarks>
    /// <param name="owner">手牌持有者（玩家）。</param>
    /// <param name="cardsList">逻辑层的手牌顺序列表。</param>
    public static void SyncVisuals(Player owner, List<CardModel> cardsList)
    {
        var handUi = NPlayerHand.Instance;
        if (handUi == null || cardsList == null) return;

        var container = handUi.CardHolderContainer;
        if (container == null || !GodotObject.IsInstanceValid(container)) return;

        // 用字典缓存 CardModel → NHandCardHolder 的映射
        var holderMap = handUi.ActiveHolders.ToDictionary(h => h.CardModel, h => h);

        int currentChildCount = container.GetChildCount();

        for (int i = 0; i < cardsList.Count; i++)
        {
            if (holderMap.TryGetValue(cardsList[i], out var holder))
            {
                if (!GodotObject.IsInstanceValid(holder)) continue;

                int targetIndex = Math.Min(i, currentChildCount - 1);
                if (targetIndex >= 0 && holder.GetIndex() != targetIndex)
                {
                    container.MoveChild(holder, targetIndex);
                    currentChildCount = container.GetChildCount();
                }
            }
        }

        // 通知 NPlayerHand 刷新卡牌交互索引
        handUi.ForceRefreshCardIndices();
    }

    // ==========================================
    // 打乱
    // ==========================================

    /// <summary>
    /// Shuffles the player's hand using the game RNG (Fisher-Yates).
    /// </summary>
    /// <remarks>
    /// 使用游戏的 RNG 对玩家手牌进行 Fisher-Yates 洗牌。
    /// </remarks>
    /// <param name="owner">手牌持有者（玩家）。</param>
    public static async Task ShuffleHand(Player owner)
    {
        var list = GetRawCards(owner);
        if (list == null || list.Count <= 1) return;

        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = owner.RunState.Rng.Shuffle.NextInt(0, i + 1);

            // 触发交换前 Hook
            await PlusHooks.Plus_TriggerBeforeHandPileMoved(list[i]);
            await PlusHooks.Plus_TriggerBeforeHandPileMoved(list[j]);

            // 逻辑层交换
            (list[i], list[j]) = (list[j], list[i]);

            // 视觉层同步
            SyncVisuals(owner, list);

            // 触发交换后 Hook
            await PlusHooks.Plus_TriggerAfterHandPileMoved(list[i]);
            await PlusHooks.Plus_TriggerAfterHandPileMoved(list[j]);

            // 重新获取列表（防止其他操作改变了引用）
            list = GetRawCards(owner);
        }
    }

    // ==========================================
    // 移动到边缘
    // ==========================================

    /// <summary>
    /// Moves a card to the far left (toLeft = true) or far right (toLeft = false).
    /// </summary>
    /// <remarks>
    /// 将指定卡牌移动到最左侧（toLeft = true）或最右侧（toLeft = false）。
    /// </remarks>
    /// <param name="owner">手牌持有者（玩家）。</param>
    /// <param name="card">要移动的卡牌。</param>
    /// <param name="toLeft">true 移到最左，false 移到最右。</param>
    public static async Task MoveToEdges(Player owner, CardModel card, bool toLeft)
    {
        var list = GetRawCards(owner);
        if (list == null || !list.Contains(card)) return;

        // 触发移动前 Hook
        await PlusHooks.Plus_TriggerBeforeHandPileMoved(card);

        // 从原位置移除，插入到目标位置
        list.Remove(card);
        if (toLeft)
            list.Insert(0, card);
        else
            list.Add(card);

        // 视觉层同步
        SyncVisuals(owner, list);

        // 触发移动后 Hook
        await PlusHooks.Plus_TriggerAfterHandPileMoved(card);
    }
}
