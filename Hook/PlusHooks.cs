using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Gold;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Relics;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using UltraLib.Variables;

namespace UltraLib.Hook;

public static class PlusHooks
{
    private static IEnumerable<AbstractModel> GetHookListeners()
    {
        RunState? runState = RunManager.Instance.DebugOnlyGetState();
        CombatState? combatState = CombatManager.Instance.DebugOnlyGetState();
        if (runState == null) return [];
        return runState.IterateHookListeners(combatState);
    }

    private static IEnumerable<IPlusHooks> GetPlusHookListeners()
        => GetHookListeners().OfType<IPlusHooks>();

    private static async Task Dispatch(Func<IPlusHooks, Task> action)
    {
        foreach (var listener in GetPlusHookListeners())
        {
            if (listener != null) await action(listener);
        }
    }

    private static decimal Pipeline(decimal initial, Func<IPlusHooks, decimal, decimal> action)
    {
        decimal current = initial;
        foreach (var listener in GetPlusHookListeners())
            current = action(listener, current);
        return current;
    }

    private static decimal Sum(decimal initial, Func<IPlusHooks, decimal> action)
    {
        decimal total = initial;
        foreach (var listener in GetPlusHookListeners())
            total += action(listener);
        return total;
    }

    private static decimal Product(decimal initial, Func<IPlusHooks, decimal> action)
    {
        decimal total = initial;
        foreach (var listener in GetPlusHookListeners())
            total *= action(listener);
        return total;
    }

    // ==========================================
    // Gold Triggers
    // ==========================================

    public static decimal Plus_ModifyGoldLoss(decimal amount, Player player, GoldLossType goldLossType) =>
        Pipeline(amount, (h, v) => h.Plus_ModifyGoldLoss(v, player, goldLossType));
    public static decimal Plus_ModifyGoldLossMultiplicative(decimal amount, Player player, GoldLossType goldLossType) =>
        Product(1m, h => h.Plus_ModifyGoldLossMultiplicative(amount, player, goldLossType));
    public static decimal Plus_ModifyGoldLossAddictive(decimal amount, Player player, GoldLossType goldLossType) =>
        Sum(0m, h => h.Plus_ModifyGoldLossAddictive(amount, player, goldLossType));
    public static decimal Plus_ModifyGoldGain(decimal amount, Player player, bool wasStolenBack) =>
        Pipeline(amount, (h, v) => h.Plus_ModifyGoldGain(v, player, wasStolenBack));
    public static decimal Plus_ModifyGoldGainMultiplicative(decimal amount, Player player, bool wasStolenBack) =>
        Product(1m, h => h.Plus_ModifyGoldGainMultiplicative(amount, player, wasStolenBack));
    public static decimal Plus_ModifyGoldGainAddictive(decimal amount, Player player, bool wasStolenBack) =>
        Sum(0m, h => h.Plus_ModifyGoldGainAddictive(amount, player, wasStolenBack));

    // ==========================================
    // Room Triggers
    // ==========================================

    public static async Task Plus_TriggerAfterRandomRoomRolled(RoomType roomType) =>
        await Dispatch(h => h.Plus_AfterRandomRoomRolled(roomType));

    // ==========================================
    // Orb Triggers
    // ==========================================

    public static async Task Plus_TriggerAfterOrbEvokeRemoved(PlayerChoiceContext ctx, OrbModel orb) =>
        await Dispatch(h => h.Plus_AfterOrbEvokeRemoved(ctx, orb));
    public static async Task Plus_TriggerBeforeOrbEvoke(PlayerChoiceContext ctx, OrbModel orb) =>
        await Dispatch(h => h.Plus_BeforeOrbEvoke(ctx, orb));
    public static async Task Plus_TriggerAfterOrbPassive(PlayerChoiceContext ctx, Creature? c, OrbModel orb) =>
        await Dispatch(h => h.Plus_AfterOrbPassive(ctx, c, orb));
    public static async Task Plus_TriggerBeforeOrbPassive(PlayerChoiceContext ctx, Creature? c, OrbModel orb) =>
        await Dispatch(h => h.Plus_BeforeOrbPassive(ctx, c, orb));

    // ==========================================
    // Hand Triggers
    // ==========================================

    public static async Task Plus_TriggerAfterHandPileMoved(CardModel card) =>
        await Dispatch(h => h.Plus_AfterHandPileMoved(card));
    public static async Task Plus_TriggerBeforeHandPileMoved(CardModel card) =>
        await Dispatch(h => h.Plus_BeforeHandPileMoved(card));

    // ==========================================
    // Card Triggers
    // ==========================================

    public static decimal Plus_TriggerModifyRoseCard(decimal amount, Player player, CardModel card) =>
        Pipeline(amount, (h, v) => h.Plus_ModifyRoseCard(v, player, card));
    public static decimal Plus_TriggerModifyRoseCardMultiplicative(decimal amount, Player player, CardModel card) =>
        Product(1m, h => h.Plus_ModifyRoseCardMultiplicative(amount, player, card));
    public static decimal Plus_TriggerModifyRoseCardAddictive(decimal amount, Player player, CardModel card) =>
        Sum(0m, h => h.Plus_ModifyRoseCardAddictive(amount, player, card));

    // ==========================================
    // Power Triggers
    // ==========================================

    public static async Task Plus_TriggerPowerRightClick(PowerModel power, NPower holder) =>
        await Dispatch(h => h.Plus_PowerRightClick(power, holder));

    // ==========================================
    // Relic Triggers
    // ==========================================

    public static async Task Plus_TriggerAfterRelicObtain(IRunState rs, RelicModel rm, Player p) =>
        await Dispatch(h => h.Plus_AfterRelicObtain(rs, rm, p));
    public static async Task Plus_TriggerBeforeRelicObtain(IRunState rs, RelicModel rm, Player p) =>
        await Dispatch(h => h.Plus_BeforeRelicObtain(rs, rm, p));

    // ==========================================
    // Charge Triggers
    // ==========================================

    public static async Task Plus_TriggerOnChargeFullyCharged(Player p, RelicModel r) =>
        await Dispatch(h => h.Plus_OnChargeFullyCharged(p, r));
    public static async Task Plus_TriggerOnChargeNoLongerFullyCharged(Player p, RelicModel r) =>
        await Dispatch(h => h.Plus_OnChargeNoLongerFullyCharged(p, r));
    public static async Task Plus_TriggerOnChargeChanged(int old, int nw, Player p, RelicModel r) =>
        await Dispatch(h => h.Plus_OnChargeChanged(old, nw, p, r));
    public static async Task Plus_TriggerAfterChargeEffected(Player p, RelicModel r) =>
        await Dispatch(h => h.Plus_AfterChargeEffected(p, r));
    public static async Task Plus_TriggerAfterChargeTotallyEffected(Player p, RelicModel r) =>
        await Dispatch(h => h.Plus_AfterChargeTotallyEffected(p, r));
    public static async Task Plus_TriggerBeforeChargeEffected(Player p, RelicModel r) =>
        await Dispatch(h => h.Plus_BeforeChargeEffected(p, r));
    public static async Task Plus_TriggerBeforeChargeTotallyEffected(Player p, RelicModel r) =>
        await Dispatch(h => h.Plus_BeforeChargeTotallyEffected(p, r));

    public static decimal Plus_TriggerModifyMaxCharge(decimal a, Player p, RelicModel r) =>
        Pipeline(a, (h, v) => h.Plus_ModifyMaxCharge(v, p, r));
    public static decimal Plus_TriggerModifyMaxChargeMultiplicative(decimal a, Player p, RelicModel r) =>
        Product(1m, h => h.Plus_ModifyMaxChargeMultiplicative(a, p, r));
    public static decimal Plus_TriggerModifyMaxChargeAddictive(decimal a, Player p, RelicModel r) =>
        Sum(0m, h => h.Plus_ModifyMaxChargeAddictive(a, p, r));

    public static decimal Plus_TriggerModifyChargeUpgrade(decimal a, Player p, RelicModel r) =>
        Pipeline(a, (h, v) => h.Plus_ModifyChargeUpgrade(v, p, r));
    public static decimal Plus_TriggerModifyChargeUpgradeMultiplicative(decimal a, Player p, RelicModel r) =>
        Product(1m, h => h.Plus_ModifyChargeUpgradeMultiplicative(a, p, r));
    public static decimal Plus_TriggerModifyChargeUpgradeAddictive(decimal a, Player p, RelicModel r) =>
        Sum(0m, h => h.Plus_ModifyChargeUpgradeAddictive(a, p, r));

    public static decimal Plus_TriggerModifyChargeSpend(decimal a, Player p, RelicModel r) =>
        Pipeline(a, (h, v) => h.Plus_ModifyChargeSpend(v, p, r));
    public static decimal Plus_TriggerModifyChargeSpendMultiplicative(decimal a, Player p, RelicModel r) =>
        Product(1m, h => h.Plus_ModifyChargeSpendMultiplicative(a, p, r));
    public static decimal Plus_TriggerModifyChargeSpendAddictive(decimal a, Player p, RelicModel r) =>
        Sum(0m, h => h.Plus_ModifyChargeSpendAddictive(a, p, r));

    public static decimal Plus_TriggerModifyChargeRepeatTimes(decimal a, Player p, RelicModel r) =>
        Pipeline(a, (h, v) => h.Plus_ModifyChargeRepeatTimes(v, p, r));
    public static decimal Plus_TriggerModifyChargeRepeatTimesMultiplicative(decimal a, Player p, RelicModel r) =>
        Product(1m, h => h.Plus_ModifyChargeRepeatTimesMultiplicative(a, p, r));
    public static decimal Plus_TriggerModifyChargeRepeatTimesAddictive(decimal a, Player p, RelicModel r) =>
        Sum(0m, h => h.Plus_ModifyChargeRepeatTimesAddictive(a, p, r));

    public static async Task Plus_TriggerAfterChargeSpend(decimal a, Player p, RelicModel r) =>
        await Dispatch(h => h.Plus_AfterChargeSpend(a, p, r));
    public static async Task Plus_TriggerAfterChargeGain(decimal a, Player p, RelicModel r) =>
        await Dispatch(h => h.Plus_AfterChargeGain(a, p, r));

    public static async Task Plus_TriggerRelicRightClick(RelicModel relic, NRelicInventoryHolder? holder)
    {
        if (relic == null) return;
        try { await Dispatch(h => h.Plus_RelicRightClick(relic, holder)); }
        catch (Exception ex) { Log.Error($"[UltraLib] Relic right click hook error: {ex.Message}"); }
    }

    // ==========================================
    // Card & Interaction Triggers
    // ==========================================

    public static async Task Plus_TriggerCardRightClick(CardModel card, NCardHolder holder) =>
        await Dispatch(h => h.Plus_CardRightClick(card, holder));

    public static async Task Plus_TriggerBeforeIsomorphism(CardModel card) =>
        await Dispatch(h => h.Plus_BeforeIsomorphism(card));
    public static async Task Plus_TriggerAfterIsomorphism(CardModel card) =>
        await Dispatch(h => h.Plus_AfterIsomorphism(card));

    public static async Task Plus_TriggerBeforeCastWhenDrawn(PlayerChoiceContext ctx, CardModel card) =>
        await Dispatch(h => h.Plus_BeforeCastWhenDrawn(ctx, card));
    public static async Task Plus_TriggerAfterCastWhenDrawn(PlayerChoiceContext ctx, CardModel card) =>
        await Dispatch(h => h.Plus_AfterCastWhenDrawn(ctx, card));

    public static async Task Plus_TriggerBeforeCardReturn(CardModel card) =>
        await Dispatch(h => h.Plus_BeforeCardReturn(card));
    public static async Task Plus_TriggerAfterCardReturn(CardModel card) =>
        await Dispatch(h => h.Plus_AfterCardReturn(card));

    public static async Task Plus_TriggerBeforeCardEmpower(CardModel card, EmpowerVar ev, List<Creature> targets) =>
        await Dispatch(h => h.Plus_BeforeCardEmpower(card, ev, targets));
    public static async Task Plus_TriggerAfterCardEmpower(CardModel card, EmpowerVar ev, List<Creature> targets) =>
        await Dispatch(h => h.Plus_AfterCardEmpower(card, ev, targets));
}
