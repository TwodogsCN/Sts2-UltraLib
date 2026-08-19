using HarmonyLib;
using MegaCrit.Sts2.Core.Odds;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace UltraLib.Hook.HookPatches;

/// <summary>
/// 随机房间 Roll 点完成后的 Hook Patch。
/// 在 UnknownMapPointOdds.Roll 返回结果后触发 Plus_AfterRandomRoomRolled。
/// </summary>
[HarmonyPatch(typeof(UnknownMapPointOdds), nameof(UnknownMapPointOdds.Roll), new[] { typeof(IEnumerable<RoomType>), typeof(IRunState) })]
public static class RandomRoomRolledPatch
{
    [HarmonyPostfix]
    public static void Postfix(IEnumerable<RoomType> blacklist, IRunState runState, ref RoomType __result)
    {
        PlusHooks.Plus_TriggerAfterRandomRoomRolled(__result).GetAwaiter().GetResult();
    }
}
