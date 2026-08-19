using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Gold;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Relics;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using UltraLib.Variables;

namespace UltraLib.Hook;

public interface IPlusHooks
{
    // Room Hooks
    Task Plus_AfterRandomRoomRolled(RoomType roomType) => Task.CompletedTask;

    // Hand Hooks
    Task Plus_AfterHandPileMoved(CardModel card) => Task.CompletedTask;
    Task Plus_BeforeHandPileMoved(CardModel card) => Task.CompletedTask;

    // Card Hooks
    decimal Plus_ModifyRoseCard(decimal amount, Player player, CardModel card) => amount;
    decimal Plus_ModifyRoseCardMultiplicative(decimal amount, Player player, CardModel card) => 1m;
    decimal Plus_ModifyRoseCardAddictive(decimal amount, Player player, CardModel card) => 0m;

    // Power Hooks
    Task Plus_PowerRightClick(PowerModel powerModel, NPower holder) => Task.CompletedTask;

    // Relic Hooks
    Task Plus_RelicRightClick(RelicModel relicModel, NRelicInventoryHolder? holder) => Task.CompletedTask;
    Task Plus_AfterRelicObtain(IRunState runState, RelicModel relicModel, Player holder) => Task.CompletedTask;
    Task Plus_BeforeRelicObtain(IRunState runState, RelicModel relicModel, Player holder) => Task.CompletedTask;
    Task Plus_CardRightClick(CardModel relicModel, NCardHolder holder) => Task.CompletedTask;

    // Charge Hooks
    decimal Plus_ModifyMaxCharge(decimal amount, Player player, RelicModel relic) => amount;
    decimal Plus_ModifyMaxChargeMultiplicative(decimal amount, Player player, RelicModel relic) => 1m;
    decimal Plus_ModifyMaxChargeAddictive(decimal amount, Player player, RelicModel relic) => 0m;
    decimal Plus_ModifyChargeUpgrade(decimal amount, Player player, RelicModel relic) => amount;
    decimal Plus_ModifyChargeUpgradeMultiplicative(decimal amount, Player player, RelicModel relic) => 1m;
    decimal Plus_ModifyChargeUpgradeAddictive(decimal amount, Player player, RelicModel relic) => 0m;
    decimal Plus_ModifyChargeSpend(decimal amount, Player player, RelicModel relic) => amount;
    decimal Plus_ModifyChargeSpendMultiplicative(decimal amount, Player player, RelicModel relic) => 1m;
    decimal Plus_ModifyChargeSpendAddictive(decimal amount, Player player, RelicModel relic) => 0m;
    Task Plus_AfterChargeSpend(decimal amount, Player player, RelicModel relic) => Task.CompletedTask;
    Task Plus_AfterChargeGain(decimal amount, Player player, RelicModel relic) => Task.CompletedTask;
    Task Plus_OnChargeFullyCharged(Player player, RelicModel relic) => Task.CompletedTask;
    Task Plus_OnChargeNoLongerFullyCharged(Player player, RelicModel relic) => Task.CompletedTask;
    Task Plus_OnChargeChanged(int oldCharge, int newCharge, Player player, RelicModel relic) => Task.CompletedTask;
    Task Plus_AfterChargeEffected(Player player, RelicModel relic) => Task.CompletedTask;
    Task Plus_AfterChargeTotallyEffected(Player player, RelicModel relic) => Task.CompletedTask;
    Task Plus_BeforeChargeEffected(Player player, RelicModel relic) => Task.CompletedTask;
    Task Plus_BeforeChargeTotallyEffected(Player player, RelicModel relic) => Task.CompletedTask;
    decimal Plus_ModifyChargeRepeatTimes(decimal amount, Player player, RelicModel relic) => amount;
    decimal Plus_ModifyChargeRepeatTimesMultiplicative(decimal amount, Player player, RelicModel relic) => 1m;
    decimal Plus_ModifyChargeRepeatTimesAddictive(decimal amount, Player player, RelicModel relic) => 0m;

    // Gold Hooks
    decimal Plus_ModifyGoldLoss(decimal amount, Player player, GoldLossType goldLossType) => amount;
    decimal Plus_ModifyGoldLossMultiplicative(decimal amount, Player player, GoldLossType goldLossType) => 1m;
    decimal Plus_ModifyGoldLossAddictive(decimal amount, Player player, GoldLossType goldLossType) => 0m;
    decimal Plus_ModifyGoldGain(decimal amount, Player player, bool wasStolenBack) => amount;
    decimal Plus_ModifyGoldGainMultiplicative(decimal amount, Player player, bool wasStolenBack) => 1m;
    decimal Plus_ModifyGoldGainAddictive(decimal amount, Player player, bool wasStolenBack) => 0m;

    // Orb Hooks
    Task Plus_AfterOrbEvokeRemoved(PlayerChoiceContext choiceContext, OrbModel orb) => Task.CompletedTask;
    Task Plus_BeforeOrbEvoke(PlayerChoiceContext choiceContext, OrbModel orb) => Task.CompletedTask;
    Task Plus_BeforeOrbPassive(PlayerChoiceContext choiceContext, Creature? creature, OrbModel orb) => Task.CompletedTask;
    Task Plus_AfterOrbPassive(PlayerChoiceContext choiceContext, Creature? creature, OrbModel orb) => Task.CompletedTask;

    // PlusCardKeyWords Hooks
    Task Plus_BeforeIsomorphism(CardModel card) => Task.CompletedTask;
    Task Plus_AfterIsomorphism(CardModel card) => Task.CompletedTask;
    Task Plus_BeforeCastWhenDrawn(PlayerChoiceContext choiceContext, CardModel card) => Task.CompletedTask;
    Task Plus_AfterCastWhenDrawn(PlayerChoiceContext choiceContext, CardModel card) => Task.CompletedTask;

    // PlusDynamicVars Hooks
    Task Plus_BeforeCardReturn(CardModel card) => Task.CompletedTask;
    Task Plus_AfterCardReturn(CardModel card) => Task.CompletedTask;

    // Empower Hooks
    Task Plus_BeforeCardEmpower(CardModel card, EmpowerVar empowerVar, List<Creature> targets) => Task.CompletedTask;
    Task Plus_AfterCardEmpower(CardModel card, EmpowerVar empowerVar, List<Creature> targets) => Task.CompletedTask;
}
