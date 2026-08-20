using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace UltraLib.Hook.HookPatches;

/// <summary>
/// Harmony patch for hand position fix.
/// </summary>
/// <remarks>
/// 手牌位置修正 Patch。
/// <para>
/// 原版 StS2 手牌的 UI 视觉顺序与逻辑层牌堆顺序不一致。
/// 当 NPlayerHand.Add(card, -1) 时，游戏总是将卡牌追加到 UI 末尾。
/// 此 Patch 在 index == -1 时去逻辑牌堆查找卡牌真实索引，插入正确位置。
/// </para>
/// </remarks>
[HarmonyPatch(typeof(NPlayerHand), nameof(NPlayerHand.Add))]
public static class RandomPositionFixPatch
{
    public static bool Prefix(NPlayerHand __instance, NCard card, int index, ref NHandCardHolder __result)
    {
        if (__instance == null || card == null || !GodotObject.IsInstanceValid(card))
        {
            __result = null;
            return false;
        }

        Vector2 globalPosition = card.GlobalPosition;
        int finalIndex = index;

        if (index == -1 && card.Model?.Pile?.Cards != null)
        {
            int realIndex = card.Model.Pile.Cards.IndexOf(card.Model);
            if (realIndex >= 0)
            {
                int currentChildCount = __instance.CardHolderContainer?.GetChildCount() ?? 0;
                finalIndex = Math.Min(realIndex, currentChildCount);
            }
        }

        NHandCardHolder holder = NHandCardHolder.Create(card, __instance);

        var addCardHolderMethod = AccessTools.Method(typeof(NPlayerHand), "AddCardHolder",
            new Type[] { typeof(NHandCardHolder), typeof(int) });
        if (addCardHolderMethod != null)
        {
            addCardHolderMethod.Invoke(__instance, new object[] { holder, finalIndex });
        }
        else
        {
            __result = holder;
            return true;
        }

        holder.GlobalPosition = globalPosition;

        var refreshLayoutMethod = AccessTools.Method(typeof(NPlayerHand), "RefreshLayout");
        refreshLayoutMethod?.Invoke(__instance, null);

        if (__instance.CurrentMode is NPlayerHand.Mode.SimpleSelect or NPlayerHand.Mode.UpgradeSelect)
        {
            var updateVisibilityMethod = AccessTools.Method(typeof(NPlayerHand), "UpdateSelectModeCardVisibility");
            updateVisibilityMethod?.Invoke(__instance, null);
        }

        __result = holder;
        return false;
    }
}
