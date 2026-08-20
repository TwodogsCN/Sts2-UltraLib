using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Gold;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Runs;

namespace UltraLib.Hook.HookPatches;

/// <summary>
/// Harmony patch on <c>PlayerCmd.GainGold</c>: intercepts gold gain and injects Plus
/// additive / multiplicative / pipeline modifiers.
/// </summary>
/// <remarks>
/// PlayerCmd.GainGold 的 Hook Patch：拦截金币获得，注入 Plus 加算/乘算/管线修正。
/// </remarks>
[HarmonyPatch(typeof(PlayerCmd), nameof(PlayerCmd.GainGold), new[] { typeof(decimal), typeof(Player), typeof(bool) })]
public static class PlayerCmdGainGoldPatch
{
    [HarmonyPrefix]
    public static bool Prefix(decimal amount, Player player, bool wasStolenBack, ref Task __result)
    {
        __result = ProxyGainGoldAsync(amount, player, wasStolenBack);
        return false;
    }

    private static async Task ProxyGainGoldAsync(decimal amount, Player player, bool wasStolenBack)
    {
        decimal totalBonus = PlusHooks.Plus_ModifyGoldGainAddictive(amount, player, wasStolenBack);
        decimal totalMultiplier = PlusHooks.Plus_ModifyGoldGainMultiplicative(amount, player, wasStolenBack);
        decimal finalAmount = PlusHooks.Plus_ModifyGoldGain((amount + totalBonus) * totalMultiplier, player, wasStolenBack);

        if (finalAmount <= 0m) return;

        if (player == LocalContext.GetMe((IPlayerCollection)player.RunState))
        {
            string sfx = finalAmount >= 100M
                ? "event:/sfx/ui/gold/gold_3"
                : (finalAmount > 30M ? "event:/sfx/ui/gold/gold_2" : "event:/sfx/ui/gold/gold_1");
            SfxCmd.Play(sfx);
        }

        PlayerMapPointHistoryEntry entry = player.RunState.CurrentMapPointHistoryEntry?.GetEntry(player.NetId);
        if (entry != null)
        {
            if (wasStolenBack)
                entry.GoldStolen -= (int)finalAmount;
            else
                entry.GoldGained += (int)finalAmount;
        }

        player.Gold += (int)finalAmount;
        await MegaCrit.Sts2.Core.Hooks.Hook.AfterGoldGained(player.RunState, player);
    }
}

/// <summary>
/// Harmony patch on <c>PlayerCmd.LoseGold</c>: intercepts gold loss and injects Plus
/// additive / multiplicative / pipeline modifiers.
/// </summary>
/// <remarks>
/// PlayerCmd.LoseGold 的 Hook Patch：拦截金币损失，注入 Plus 加算/乘算/管线修正。
/// </remarks>
[HarmonyPatch(typeof(PlayerCmd), nameof(PlayerCmd.LoseGold), new[] { typeof(decimal), typeof(Player), typeof(GoldLossType) })]
public static class PlayerCmdLoseGoldPatch
{
    [HarmonyPrefix]
    public static bool Prefix(decimal amount, Player player, GoldLossType goldLossType, ref Task __result)
    {
        decimal totalBonus = PlusHooks.Plus_ModifyGoldLossAddictive(amount, player, goldLossType);
        decimal totalMultiplier = PlusHooks.Plus_ModifyGoldLossMultiplicative(amount, player, goldLossType);
        decimal finalLoss = PlusHooks.Plus_ModifyGoldLoss((amount + totalBonus) * totalMultiplier, player, goldLossType);

        if (finalLoss <= 0m)
        {
            __result = Task.CompletedTask;
            return false;
        }

        SfxCmd.Play("event:/sfx/ui/gold/gold_1");

        PlayerMapPointHistoryEntry entry = player.RunState.CurrentMapPointHistoryEntry?.GetEntry(player.NetId);
        if (entry != null)
        {
            switch (goldLossType)
            {
                case GoldLossType.Spent: entry.GoldSpent += (int)finalLoss; break;
                case GoldLossType.Lost: entry.GoldLost += (int)finalLoss; break;
                case GoldLossType.Stolen: entry.GoldStolen += (int)finalLoss; break;
            }
        }

        player.Gold = int.Max(0, player.Gold - (int)finalLoss);
        __result = Task.CompletedTask;
        return false;
    }
}
