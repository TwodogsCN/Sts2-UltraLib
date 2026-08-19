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
using UltraLib.Base.Relic;
using UltraLib.Hook;
using UltraLib.Variables;

namespace UltraLib.Base.Abstract;

/// <summary>
/// UltraLib 自定义遗物模型的抽象基类。
/// <para>
/// 继承自 <see cref="CustomRelicModel"/> 并实现 <see cref="IPlusHooks"/>，
/// 为所有 Plus Hook 提供默认空实现。子类可按需覆写。
/// </para>
/// </summary>
public abstract class PlusRelicModel : CustomRelicModel, IPlusHooks
{
    /// <summary>遗物等级，用于控制出现权重。</summary>
    public virtual PlusRelicLevel RelicLevel => PlusRelicLevel.Level1;

    /// <summary>遗物出现池。</summary>
    public virtual HashSet<RelicItemPool> ItemPool => [RelicItemPool.Normal];

    /// <summary>遗物标签集合。</summary>
    public virtual HashSet<RelicTag> Tags => [];

    private object? _internalData;

    /// <summary>
    /// 初始化内部数据，子类必须实现此方法以返回各自的 Data 对象。
    /// </summary>
    protected virtual object InitInternalData() => null!;

    /// <summary>获取内部数据。</summary>
    protected T GetInternalData<T>() => (T)_internalData!;

    /// <summary>
    /// 获取内部数据，如果未初始化则先初始化。
    /// </summary>
    protected T GetOrInitInternalData<T>() where T : class, new()
    {
        _internalData ??= new T();
        return (T)_internalData;
    }

    /// <summary>
    /// 覆盖 DeepCloneFields，确保克隆时重新初始化 _internalData。
    /// </summary>
    protected override void DeepCloneFields()
    {
        base.DeepCloneFields();
        _internalData = InitInternalData();
    }

    // ==========================================
    // IPlusHooks 默认实现
    // ==========================================

    public virtual Task Plus_AfterOrbEvokeRemoved(PlayerChoiceContext ctx, OrbModel orb) => Task.CompletedTask;
    public virtual Task Plus_BeforeOrbEvoke(PlayerChoiceContext ctx, OrbModel orb) => Task.CompletedTask;
    public virtual Task Plus_BeforeOrbPassive(PlayerChoiceContext ctx, Creature? c, OrbModel orb) => Task.CompletedTask;
    public virtual Task Plus_AfterOrbPassive(PlayerChoiceContext ctx, Creature? c, OrbModel orb) => Task.CompletedTask;
    public virtual Task Plus_BeforeIsomorphism(CardModel card) => Task.CompletedTask;
    public virtual Task Plus_AfterIsomorphism(CardModel card) => Task.CompletedTask;
    public virtual Task Plus_BeforeCastWhenDrawn(PlayerChoiceContext ctx, CardModel card) => Task.CompletedTask;
    public virtual Task Plus_AfterCastWhenDrawn(PlayerChoiceContext ctx, CardModel card) => Task.CompletedTask;
    public virtual Task Plus_BeforeCardReturn(CardModel card) => Task.CompletedTask;
    public virtual Task Plus_AfterCardReturn(CardModel card) => Task.CompletedTask;
    public virtual Task Plus_BeforeCardEmpower(CardModel card, EmpowerVar ev, List<Creature> targets) => Task.CompletedTask;
    public virtual Task Plus_AfterCardEmpower(CardModel card, EmpowerVar ev, List<Creature> targets) => Task.CompletedTask;
    public virtual Task Plus_AfterHandPileMoved(CardModel card) => Task.CompletedTask;
    public virtual Task Plus_BeforeHandPileMoved(CardModel card) => Task.CompletedTask;
    public virtual Task Plus_RelicRightClick(RelicModel rm, NRelicInventoryHolder? h) => Task.CompletedTask;
    public virtual Task Plus_AfterRelicObtain(IRunState rs, RelicModel rm, Player p) => Task.CompletedTask;
    public virtual Task Plus_BeforeRelicObtain(IRunState rs, RelicModel rm, Player p) => Task.CompletedTask;
    public virtual Task Plus_PowerRightClick(PowerModel pm, NPower h) => Task.CompletedTask;
    public virtual Task Plus_CardRightClick(CardModel cm, NCardHolder h) => Task.CompletedTask;
    public virtual decimal Plus_ModifyGoldLoss(decimal a, Player p, GoldLossType t) => a;
    public virtual decimal Plus_ModifyGoldLossMultiplicative(decimal a, Player p, GoldLossType t) => 1m;
    public virtual decimal Plus_ModifyGoldLossAddictive(decimal a, Player p, GoldLossType t) => 0m;
    public virtual decimal Plus_ModifyGoldGain(decimal a, Player p, bool w) => a;
    public virtual decimal Plus_ModifyGoldGainMultiplicative(decimal a, Player p, bool w) => 1m;
    public virtual decimal Plus_ModifyGoldGainAddictive(decimal a, Player p, bool w) => 0m;
    public virtual Task Plus_AfterRandomRoomRolled(RoomType rt) => Task.CompletedTask;

    // Charge
    public virtual decimal Plus_ModifyMaxCharge(decimal a, Player p, RelicModel r) => a;
    public virtual decimal Plus_ModifyMaxChargeMultiplicative(decimal a, Player p, RelicModel r) => 1m;
    public virtual decimal Plus_ModifyMaxChargeAddictive(decimal a, Player p, RelicModel r) => 0m;
    public virtual decimal Plus_ModifyChargeUpgrade(decimal a, Player p, RelicModel r) => a;
    public virtual decimal Plus_ModifyChargeUpgradeMultiplicative(decimal a, Player p, RelicModel r) => 1m;
    public virtual decimal Plus_ModifyChargeUpgradeAddictive(decimal a, Player p, RelicModel r) => 0m;
    public virtual decimal Plus_ModifyChargeSpend(decimal a, Player p, RelicModel r) => a;
    public virtual decimal Plus_ModifyChargeSpendMultiplicative(decimal a, Player p, RelicModel r) => 1m;
    public virtual decimal Plus_ModifyChargeSpendAddictive(decimal a, Player p, RelicModel r) => 0m;
    public virtual Task Plus_AfterChargeSpend(decimal a, Player p, RelicModel r) => Task.CompletedTask;
    public virtual Task Plus_AfterChargeGain(decimal a, Player p, RelicModel r) => Task.CompletedTask;
    public virtual Task Plus_OnChargeFullyCharged(Player p, RelicModel r) => Task.CompletedTask;
    public virtual Task Plus_OnChargeNoLongerFullyCharged(Player p, RelicModel r) => Task.CompletedTask;
    public virtual Task Plus_OnChargeChanged(int old, int nw, Player p, RelicModel r) => Task.CompletedTask;
    public virtual decimal Plus_ModifyChargeRepeatTimes(decimal a, Player p, RelicModel r) => a;
    public virtual decimal Plus_ModifyChargeRepeatTimesMultiplicative(decimal a, Player p, RelicModel r) => 1m;
    public virtual decimal Plus_ModifyChargeRepeatTimesAddictive(decimal a, Player p, RelicModel r) => 0m;
    public virtual Task Plus_AfterChargeEffected(Player p, RelicModel r) => Task.CompletedTask;
    public virtual Task Plus_AfterChargeTotallyEffected(Player p, RelicModel r) => Task.CompletedTask;
    public virtual Task Plus_BeforeChargeEffected(Player p, RelicModel r) => Task.CompletedTask;
    public virtual Task Plus_BeforeChargeTotallyEffected(Player p, RelicModel r) => Task.CompletedTask;

    // Rose
    public virtual decimal Plus_ModifyRoseCard(decimal a, Player p, CardModel c) => a;
    public virtual decimal Plus_ModifyRoseCardMultiplicative(decimal a, Player p, CardModel c) => 1m;
    public virtual decimal Plus_ModifyRoseCardAddictive(decimal a, Player p, CardModel c) => 0m;
}
