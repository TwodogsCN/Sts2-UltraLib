using Godot;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using UltraLib.Base.Scripts;

namespace UltraLib.Base.Multiplayer.Cmds;

/// <summary>
/// Custom relic-selection command with online (multiplayer) synchronization.
/// </summary>
/// <remarks>
/// 自定义遗物选择命令，支持联机同步。
/// 本地玩家通过 NSimpleRelicSelectScreen 界面选择，远程通过 PlayerChoiceSynchronizer 同步。
/// </remarks>
public static class PlusRelicSelectCmd
{
    private static bool ShouldSelectLocalRelic(Player player)
    {
        return LocalContext.IsMe(player) && RunManager.Instance.NetService.Type != NetGameType.Replay;
    }

    /// <summary>
    /// Opens the custom relic-selection screen and returns the relic the player chose.
    /// </summary>
    /// <remarks>
    /// 打开自定义遗物选择界面，返回玩家选择的遗物。
    /// </remarks>
    /// <param name="player">进行选择的玩家。</param>
    /// <param name="relics">可选的遗物列表。</param>
    /// <param name="prefs">选择器配置。</param>
    /// <returns>玩家选择的遗物，如果未选择则返回 null。</returns>
    public static async Task<RelicModel?> FromCustomSelectScreen(
        Player player,
        IReadOnlyList<RelicModel> relics,
        RelicSelectorPrefs prefs)
    {
        if (relics.Count == 0)
            return null;

        // 保留 choiceId 供联机同步使用
        var choiceId = RunManager.Instance.PlayerChoiceSynchronizer.ReserveChoiceId(player);
        var finalIndex = -1;

        if (ShouldSelectLocalRelic(player))
        {
            // 本地玩家：打开 UI 界面
            var result = await NSimpleRelicSelectScreen.Create(relics, prefs, player);
            var selectedRelic = result.FirstOrDefault();

            if (selectedRelic is not null)
            {
                for (int i = 0; i < relics.Count; i++)
                {
                    if (!relics[i].Id.Equals(selectedRelic.Id)) continue;
                    finalIndex = i;
                    break;
                }
            }

            // 同步到联机同步器
            RunManager.Instance.PlayerChoiceSynchronizer.SyncLocalChoice(
                player,
                choiceId,
                PlayerChoiceResult.FromIndex(finalIndex)
            );

            // 记录已见过的遗物
            foreach (var relic in relics)
                SaveManager.Instance.MarkRelicAsSeen(relic);
        }
        else
        {
            // 远端玩家：等待联机同步数据
            var remoteResult = await RunManager.Instance.PlayerChoiceSynchronizer.WaitForRemoteChoice(player, choiceId);
            finalIndex = remoteResult.AsIndex();

            if (finalIndex >= 0 && finalIndex < relics.Count)
            {
                GD.Print($"[UltraLib] Remote player selected relic index: {finalIndex}, ID: {relics[finalIndex].Id.Entry} / 远端玩家选择遗物索引: {finalIndex}");
            }
            else
            {
                GD.PrintErr($"[UltraLib] Remote selection index out of range: {finalIndex} (count={relics.Count}) / 远端选择索引越界: {finalIndex} (共{relics.Count})");
            }
        }

        if (finalIndex < 0 || finalIndex >= relics.Count)
            return null;

        var canonicalRelic = relics[finalIndex];
        return canonicalRelic.IsCanonical ? canonicalRelic : canonicalRelic.CanonicalInstance;
    }
}
