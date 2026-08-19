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

/// <summary>
/// Plus 单例模型的抽象基类。
/// <para>
/// 继承自 <see cref="CustomSingletonModel"/> 并实现 <see cref="IPlusHooks"/>，
/// 为所有 Plus Hook 提供默认空实现（virtual），子类可按需覆写。
/// </para>
/// </summary>
/// <param name="hook">指定该单例要监听的 Hook 阶段。</param>
public abstract class PlusSingletonModel(CustomSingletonModel.HookType hook) : CustomSingletonModel(hook), IPlusHooks
{
    // ==========================================
    // Orb Hooks
    // ==========================================

    /// <inheritdoc />
    public virtual Task Plus_AfterOrbEvokeRemoved(PlayerChoiceContext choiceContext, OrbModel orb)
        => Task.CompletedTask;

    /// <inheritdoc />
    public virtual Task Plus_BeforeOrbEvoke(PlayerChoiceContext choiceContext, OrbModel orb)
        => Task.CompletedTask;

    /// <inheritdoc />
    public virtual Task Plus_BeforeOrbPassive(PlayerChoiceContext choiceContext, Creature? creature, OrbModel orb)
        => Task.CompletedTask;

    /// <inheritdoc />
    public virtual Task Plus_AfterOrbPassive(PlayerChoiceContext choiceContext, Creature? creature, OrbModel orb)
        => Task.CompletedTask;

    // ==========================================
    // Card Keyword Hooks
    // ==========================================

    /// <inheritdoc />
    public virtual Task Plus_BeforeIsomorphism(CardModel card)
        => Task.CompletedTask;

    /// <inheritdoc />
    public virtual Task Plus_AfterIsomorphism(CardModel card)
        => Task.CompletedTask;

    /// <inheritdoc />
    public virtual Task Plus_BeforeCastWhenDrawn(PlayerChoiceContext choiceContext, CardModel card)
        => Task.CompletedTask;

    /// <inheritdoc />
    public virtual Task Plus_AfterCastWhenDrawn(PlayerChoiceContext choiceContext, CardModel card)
        => Task.CompletedTask;

    // ==========================================
    // Dynamic Var Hooks
    // ==========================================

    /// <inheritdoc />
    public virtual Task Plus_BeforeCardReturn(CardModel card)
        => Task.CompletedTask;

    /// <inheritdoc />
    public virtual Task Plus_AfterCardReturn(CardModel card)
        => Task.CompletedTask;

    /// <inheritdoc />
    public virtual Task Plus_BeforeCardEmpower(CardModel card, EmpowerVar empowerVar, List<Creature> targets)
        => Task.CompletedTask;

    /// <inheritdoc />
    public virtual Task Plus_AfterCardEmpower(CardModel card, EmpowerVar empowerVar, List<Creature> targets)
        => Task.CompletedTask;

    // ==========================================
    // Hand Hooks
    // ==========================================

    /// <inheritdoc />
    public virtual Task Plus_AfterHandPileMoved(CardModel card)
        => Task.CompletedTask;

    /// <inheritdoc />
    public virtual Task Plus_BeforeHandPileMoved(CardModel card)
        => Task.CompletedTask;

    // ==========================================
    // Relic Hooks
    // ==========================================

    /// <inheritdoc />
    public virtual Task Plus_RelicRightClick(RelicModel relicModel, NRelicInventoryHolder? holder)
        => Task.CompletedTask;

    /// <inheritdoc />
    public virtual Task Plus_AfterRelicObtain(IRunState runState, RelicModel relicModel, Player holder)
        => Task.CompletedTask;

    /// <inheritdoc />
    public virtual Task Plus_BeforeRelicObtain(IRunState runState, RelicModel relicModel, Player holder)
        => Task.CompletedTask;

    /// <inheritdoc />
    public virtual Task Plus_PowerRightClick(PowerModel powerModel, NPower holder)
        => Task.CompletedTask;

    /// <inheritdoc />
    public virtual Task Plus_CardRightClick(CardModel cardModel, NCardHolder holder)
        => Task.CompletedTask;

    // ==========================================
    // Gold Hooks
    // ==========================================

    /// <inheritdoc />
    public virtual decimal Plus_ModifyGoldLoss(decimal amount, Player player, GoldLossType goldLossType)
        => amount;

    /// <inheritdoc />
    public virtual decimal Plus_ModifyGoldLossMultiplicative(decimal amount, Player player, GoldLossType goldLossType)
        => 1m;

    /// <inheritdoc />
    public virtual decimal Plus_ModifyGoldLossAddictive(decimal amount, Player player, GoldLossType goldLossType)
        => 0m;

    /// <inheritdoc />
    public virtual decimal Plus_ModifyGoldGain(decimal amount, Player player, bool wasStolenBack)
        => amount;

    /// <inheritdoc />
    public virtual decimal Plus_ModifyGoldGainAddictive(decimal amount, Player player, bool wasStolenBack)
        => 0m;

    /// <inheritdoc />
    public virtual decimal Plus_ModifyGoldGainMultiplicative(decimal amount, Player player, bool wasStolenBack)
        => 1m;

    // ==========================================
    // Room Hooks
    // ==========================================

    /// <inheritdoc />
    public virtual Task Plus_AfterRandomRoomRolled(RoomType roomType)
        => Task.CompletedTask;
}
