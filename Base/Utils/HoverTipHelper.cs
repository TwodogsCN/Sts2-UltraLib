using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Rooms;

namespace UltraLib.Base.Utils;

/// <summary>
/// 玩家悬停提示刷新辅助。
/// </summary>
public static class HoverTipHelper
{
    /// <summary>
    /// 刷新玩家的所有悬停提示（商店 + 战斗卡牌）。
    /// </summary>
    public static void Refresh(this Player owner)
    {
        if (owner.RunState.CurrentRoom is MerchantRoom room)
        {
            foreach (var entrys in room.Inventories)
            {
                foreach (var entry in entrys.AllEntries)
                    entry.OnMerchantInventoryUpdated();
            }
        }

        if (owner?.PlayerCombatState != null)
        {
            foreach (var card in owner.PlayerCombatState.AllCards)
                card.RefreshHoverTips();
        }
    }
}
