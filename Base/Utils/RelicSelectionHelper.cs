using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using UltraLib.Base.Multiplayer.Cmds;
using UltraLib.Base.Scripts;

namespace UltraLib.Base.Utils;

/// <summary>
/// 遗物选择界面辅助工具。
/// 提供从多个遗物中让玩家选择其一的功能（支持联机同步）。
/// </summary>
public static class RelicSelectionHelper
{
    /// <summary>
    /// 从指定的遗物列表中让玩家选择一个。
    /// </summary>
    /// <param name="relics">可选的遗物列表。</param>
    /// <param name="owner">选择遗物的玩家。</param>
    /// <param name="prefs">选择器配置（标题、数量等）。</param>
    /// <returns>玩家选择的遗物，如果列表为空则返回 null。</returns>
    public static async Task<RelicModel?> SelectRelic(
        IReadOnlyList<RelicModel> relics,
        Player owner,
        RelicSelectorPrefs prefs)
    {
        if (relics.Count == 0)
            return null;

        return await PlusRelicSelectCmd.FromCustomSelectScreen(owner, relics, prefs);
    }
}
