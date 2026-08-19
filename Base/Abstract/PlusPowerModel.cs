using BaseLib.Abstracts;
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
using UltraLib.Hook;
using UltraLib.Variables;

namespace UltraLib.Base.Abstract;

public abstract class PlusPowerModel : CustomPowerModel, IPlusHooks
{
    public virtual Task Plus_AfterOrbEvokeRemoved(PlayerChoiceContext choiceContext, OrbModel orb)
    {
        return Task.CompletedTask;
    }

    public virtual Task Plus_BeforeOrbEvoke(PlayerChoiceContext choiceContext, OrbModel orb)
    {
        return Task.CompletedTask;
    }

    public virtual Task Plus_BeforeIsomorphism(CardModel card)
    {
        return Task.CompletedTask;
    }

    public virtual Task Plus_AfterIsomorphism(CardModel card)
    {
        return Task.CompletedTask;
    }

    public virtual Task Plus_BeforeCastWhenDrawn(PlayerChoiceContext choiceContext, CardModel card)
    {
        return Task.CompletedTask;
    }

    public virtual Task Plus_AfterCastWhenDrawn(PlayerChoiceContext choiceContext, CardModel card)
    {
        return Task.CompletedTask;
    }

    public virtual Task Plus_BeforeCardReturn(CardModel card)
    {
        return Task.CompletedTask;
    }

    public virtual Task Plus_AfterCardReturn(CardModel card)
    {
        return Task.CompletedTask;
    }

    public virtual Task Plus_BeforeCardEmpower(CardModel card, EmpowerVar empowerVar, List<Creature> targets)
    {
        return Task.CompletedTask;
    }

    public virtual Task Plus_AfterCardEmpower(CardModel card, EmpowerVar empowerVar, List<Creature> targets)
    {
        return Task.CompletedTask;
    }

    public virtual Task Plus_BeforeOrbPassive(PlayerChoiceContext choiceContext, Creature? creature, OrbModel orb)
    {
        return Task.CompletedTask;
    }

    public virtual Task Plus_AfterOrbPassive(PlayerChoiceContext choiceContext, Creature? creature, OrbModel orb)
    {
        return Task.CompletedTask;
    }

    public virtual Task Plus_AfterHandPileMoved(CardModel card)
    {
        return Task.CompletedTask;
    }

    public virtual Task Plus_BeforeHandPileMoved(CardModel card)
    {
        return Task.CompletedTask;
    }
    
    public virtual Task Plus_RelicRightClick(RelicModel relicModel, NRelicInventoryHolder? holder)
    {
        return Task.CompletedTask;
    }
    
    public virtual Task Plus_AfterRelicObtain(IRunState runState, RelicModel relicModel, Player holder)
    {
        return Task.CompletedTask;
    }

    public virtual Task Plus_BeforeRelicObtain(IRunState runState, RelicModel relicModel, Player holder)
    {
        return Task.CompletedTask;
    }
    
    public virtual Task Plus_PowerRightClick(PowerModel powerModel, NPower holder)
    {
        return Task.CompletedTask;
    }
    public virtual Task Plus_CardRightClick(CardModel relicModel, NCardHolder holder)
    {
        return Task.CompletedTask;
    }
    
    public virtual decimal Plus_ModifyGoldLoss(decimal amount, Player player, GoldLossType goldLossType)
    {
        return amount;
    }

    public virtual decimal Plus_ModifyGoldLossMultiplicative(decimal amount, Player player, GoldLossType goldLossType)
    {
        return 1m;
    }

    public virtual decimal Plus_ModifyGoldLossAddictive(decimal amount, Player player, GoldLossType goldLossType)
    {
        return 0m;
    }

    public virtual decimal Plus_ModifyGoldGain(decimal amount, Player player, bool wasStolenBack)
    {
        return amount;
    }

    public virtual decimal Plus_ModifyGoldGainAddictive(decimal amount, Player player, bool wasStolenBack)
    {
        return 0m;
    }
    
    public virtual decimal Plus_ModifyGoldGainMultiplicative(decimal amount, Player player, bool wasStolenBack)
    {
        return 1m;
    }
    
    public virtual Task Plus_AfterRandomRoomRolled(RoomType roomType) => Task.CompletedTask;
    
    public virtual decimal Plus_ModifyMaxCharge(decimal amount, Player player, RelicModel relic) => amount;
    public virtual decimal Plus_ModifyMaxChargeMultiplicative(decimal amount, Player player, RelicModel relic) => 1m;
    public virtual decimal Plus_ModifyMaxChargeAddictive(decimal amount, Player player, RelicModel relic) => 0m;
    public virtual decimal Plus_ModifyChargeUpgrade(decimal amount, Player player, RelicModel relic) => amount;
    public virtual decimal Plus_ModifyChargeUpgradeMultiplicative(decimal amount, Player player, RelicModel relic) => 1m;
    public virtual decimal Plus_ModifyChargeUpgradeAddictive(decimal amount, Player player, RelicModel relic) => 0m;
    public virtual decimal Plus_ModifyChargeSpend(decimal amount, Player player, RelicModel relic) => amount;
    public virtual decimal Plus_ModifyChargeSpendMultiplicative(decimal amount, Player player, RelicModel relic) => 1m;
    public virtual decimal Plus_ModifyChargeSpendAddictive(decimal amount, Player player, RelicModel relic) => 0m;
    public virtual Task Plus_AfterChargeSpend(decimal amount, Player player, RelicModel relic) => Task.CompletedTask;
    public virtual Task Plus_AfterChargeGain(decimal amount, Player player, RelicModel relic) => Task.CompletedTask;
    public virtual Task Plus_OnChargeFullyCharged(Player player, RelicModel relic) => Task.CompletedTask;
    public virtual Task Plus_OnChargeNoLongerFullyCharged(Player player, RelicModel relic) => Task.CompletedTask;
    public virtual Task Plus_OnChargeChanged(int oldCharge, int newCharge, Player player, RelicModel relic) => Task.CompletedTask;
    public virtual decimal Plus_ModifyChargeRepeatTimes(decimal amount, Player player, RelicModel relic) => amount;
    public virtual decimal Plus_ModifyChargeRepeatTimesMultiplicative(decimal amount, Player player, RelicModel relic) => 1m;
    public virtual  decimal Plus_ModifyChargeRepeatTimesAddictive(decimal amount, Player player, RelicModel relic) => 0m;
    public virtual Task Plus_AfterChargeEffected(Player player, RelicModel relic) => Task.CompletedTask;
    public virtual Task Plus_AfterChargeTotallyEffected(Player player, RelicModel relic) => Task.CompletedTask;
    public virtual Task Plus_BeforeChargeEffected(Player player, RelicModel relic) => Task.CompletedTask;
    public virtual Task Plus_BeforeChargeTotallyEffected(Player player, RelicModel relic) => Task.CompletedTask;
    
    public virtual decimal Plus_ModifyRoseCard(decimal amount, Player player, CardModel card) => amount;
    public virtual decimal Plus_ModifyRoseCardMultiplicative(decimal amount, Player player, CardModel card) => 1m;
    public virtual decimal Plus_ModifyRoseCardAddictive(decimal amount, Player player, CardModel card) => 0m;
}