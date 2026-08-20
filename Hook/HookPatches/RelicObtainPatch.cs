using HarmonyLib;
using MegaCrit.Sts2.Core.Audio.Debug;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Runs.History;
using MegaCrit.Sts2.Core.Saves;

namespace UltraLib.Hook.HookPatches;

/// <summary>
/// Harmony patch on <c>RelicCmd.Obtain</c>: injects Before/After hooks around relic
/// acquisition while retaining the full original flow.
/// </summary>
/// <remarks>
/// RelicCmd.Obtain 的 Hook Patch：在遗物获取前后注入 Before/After Hook。
/// 保留原版完整逻辑（添加到玩家、更新抓包、播动画、保存等）。
/// </remarks>
[HarmonyPatch(typeof(RelicCmd), nameof(RelicCmd.Obtain), new[] { typeof(RelicModel), typeof(Player), typeof(int) })]
public static class RelicCmdObtainPatch
{
    [HarmonyPrefix]
    public static bool Prefix(RelicModel relic, Player player, int index, ref Task<RelicModel> __result)
    {
        __result = FullAsyncObtainFlow(relic, player, index);
        return false;
    }

    private static async Task<RelicModel> FullAsyncObtainFlow(RelicModel relic, Player player, int index)
    {
        if (relic == null || player == null) return relic;

        relic.AssertMutable();
        IRunState runState = player.RunState;

        await PlusHooks.Plus_TriggerBeforeRelicObtain(runState, relic, player);

        runState.CurrentMapPointHistoryEntry?.GetEntry(player.NetId)
            .RelicChoices.Add(new ModelChoiceHistoryEntry(relic.Id, wasPicked: true));
        player.AddRelicInternal(relic, index);

        if (!relic.IsStackable)
        {
            player.RelicGrabBag.Remove(relic);
            runState.SharedRelicGrabBag.Remove(relic);
        }

        if (LocalContext.IsMe(player))
        {
            NRun.Instance?.GlobalUi.RelicInventory.AnimateRelic(relic);
            NDebugAudioManager.Instance?.Play("relic_get.mp3");
            SaveManager.Instance.MarkRelicAsSeen(relic);
        }

        relic.FloorAddedToDeck = runState.TotalFloor;
        await relic.AfterObtained();

        await PlusHooks.Plus_TriggerAfterRelicObtain(runState, relic, player);
        return relic;
    }
}
