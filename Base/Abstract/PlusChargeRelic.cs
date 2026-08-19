using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;
using UltraLib.Hook;

namespace UltraLib.Base.Abstract;

/// <summary>
/// 充能遗物（Charge Relic）的抽象基类。
/// <para>
/// 充能遗物会在进入房间时自动获得充能（除非 <see cref="AutoCharge"/> 为 false），
/// 充能满后玩家可以右键触发遗物效果，消耗充能。
/// </para>
/// </summary>
public abstract class PlusChargeRelic : PlusRelicModel
{
    // ========== 核心配置 ==========

    /// <summary>
    /// 是否在进入房间时自动充能。
    /// <para>重写此属性返回 false 可阻止自动充能，由外部逻辑手动调用充能。</para>
    /// </summary>
    public virtual bool AutoCharge => true;

    /// <summary>
    /// 是否使用自定义充能条 UI 显示（而非默认的数字显示）。
    /// </summary>
    public virtual bool UseChargeBarDisplay => true;

    // ========== 充能状态 ==========

    private int _nowCharge;
    private int _totalCharge = 1;

    /// <inheritdoc />
    public override int DisplayAmount => _nowCharge;

    /// <inheritdoc />
    public override bool ShowCounter => true;

    /// <summary>当前充能数。</summary>
    [SavedProperty]
    public int NowCharge
    {
        get => _nowCharge;
        set
        {
            _nowCharge = value;
            if (_nowCharge < 0) _nowCharge = 0;
            Status = _nowCharge >= TotalCharge ? RelicStatus.Active : RelicStatus.Normal;
            InvokeDisplayAmountChanged();
        }
    }

    /// <summary>最大充能数（满充所需值）。</summary>
    [SavedProperty]
    public int TotalCharge
    {
        get => _totalCharge;
        set
        {
            _totalCharge = value;
            if (_totalCharge < 0) _totalCharge = 0;
            Status = _nowCharge >= TotalCharge ? RelicStatus.Active : RelicStatus.Normal;
            InvokeDisplayAmountChanged();
        }
    }

    /// <summary>是否已满充。</summary>
    protected bool IsFullyCharged => NowCharge >= TotalCharge;

    // ========== 内部方法 ==========

    /// <summary>
    /// 增加指定层数的充能（默认 1）。
    /// </summary>
    public void GainCharge(int amount = 1)
    {
        NowChargeUpgrade(amount);
    }

    /// <summary>
    /// 直接设置当前充能和最大充能。
    /// </summary>
    public void SetCharge(int nowCharge, int totalCharge)
    {
        TotalCharge = totalCharge;
        NowCharge = nowCharge;
    }

    /// <summary>将当前充能和最大充能都设为同一值（即充满）。</summary>
    public void SetCharge(int amount)
    {
        TotalCharge = amount;
        NowCharge = amount;
    }

    // ========== 生命周期：房间进入与战斗结束 ==========

    /// <summary>
    /// 进入房间后自动充能（如果 <see cref="AutoCharge"/> 为 true）。
    /// </summary>
    public override Task AfterRoomEntered(AbstractRoom room)
    {
        NowCharge = _nowCharge;
        if (AutoCharge)
            NowChargeUpgrade();
        return Task.CompletedTask;
    }

    /// <summary>战斗结束后刷新满充状态显示。</summary>
    public override Task AfterCombatEnd(CombatRoom room)
    {
        NowCharge = _nowCharge;
        CalculatedIsFullyCharged();
        InvokeDisplayAmountChanged();
        return Task.CompletedTask;
    }

    /// <summary>战斗胜利后刷新满充状态显示。</summary>
    public override Task AfterCombatVictory(CombatRoom room)
    {
        NowCharge = _nowCharge;
        CalculatedIsFullyCharged();
        InvokeDisplayAmountChanged();
        return Task.CompletedTask;
    }

    // ========== 充能核心逻辑 ==========

    /// <summary>
    /// 增加充能（经过 Hook 修正）。
    /// </summary>
    /// <param name="amount">基础充能增量。</param>
    public async void NowChargeUpgrade(int amount = 1)
    {
        var maxCharge = GetResolvedMaxCharge();
        amount = GetResolvedChargeUpgrade(amount);
        var oldCharge = _nowCharge;

        if (_nowCharge < maxCharge)
            Flash();

        var gainAmount = Math.Min(amount, maxCharge - _nowCharge);
        await PlusHooks.Plus_TriggerAfterChargeGain(gainAmount, Owner, this);

        _nowCharge = Math.Min(maxCharge, _nowCharge + amount);

        await PlusHooks.Plus_TriggerOnChargeChanged(oldCharge, _nowCharge, Owner, this);
        CalculatedIsFullyCharged();
        InvokeDisplayAmountChanged();
    }

    /// <summary>
    /// 消耗充能（通常在使用效果后调用）。
    /// </summary>
    public async void SpendCharge()
    {
        var chargeSpend = _nowCharge - Math.Max(0, _nowCharge - GetResolvedChargeSpend());
        var oldCharge = _nowCharge;

        _nowCharge -= chargeSpend;

        await PlusHooks.Plus_TriggerOnChargeChanged(oldCharge, _nowCharge, Owner, this);
        await PlusHooks.Plus_TriggerAfterChargeSpend(chargeSpend, Owner, this);

        CalculatedIsFullyCharged();
        InvokeDisplayAmountChanged();
    }

    /// <summary>
    /// 检测并触发满充/不满充状态变更 Hook。
    /// </summary>
    public async void CalculatedIsFullyCharged()
    {
        var preStatus = Status;
        Status = _nowCharge >= TotalCharge ? RelicStatus.Active : RelicStatus.Normal;

        if (preStatus == RelicStatus.Active && Status != RelicStatus.Active)
            await PlusHooks.Plus_TriggerOnChargeNoLongerFullyCharged(Owner, this);
        else if (preStatus == RelicStatus.Normal && Status == RelicStatus.Active)
            await PlusHooks.Plus_TriggerOnChargeFullyCharged(Owner, this);
    }

    /// <summary>
    /// 执行充能遗物的右键效果。
    /// <para>会经过重复次数 Hook 修正后，循环调用 <see cref="MainEffect"/>。</para>
    /// </summary>
    public async Task DoChargeRelicEffect()
    {
        var repeatedTimes = 1;
        repeatedTimes += (int)PlusHooks.Plus_TriggerModifyChargeRepeatTimesAddictive(repeatedTimes, Owner, this);
        repeatedTimes = (int)(PlusHooks.Plus_TriggerModifyChargeRepeatTimesMultiplicative(repeatedTimes, Owner, this) * repeatedTimes);
        repeatedTimes = (int)PlusHooks.Plus_TriggerModifyChargeRepeatTimes(repeatedTimes, Owner, this);

        await PlusHooks.Plus_TriggerBeforeChargeTotallyEffected(Owner, this);

        for (var i = 0; i < repeatedTimes; i++)
        {
            await PlusHooks.Plus_TriggerBeforeChargeEffected(Owner, this);
            await MainEffect();
            await PlusHooks.Plus_TriggerAfterChargeEffected(Owner, this);
        }

        await PlusHooks.Plus_TriggerAfterChargeTotallyEffected(Owner, this);
    }

    /// <summary>
    /// 充能遗物的主要效果。子类在此实现具体的遗物逻辑。
    /// </summary>
    public virtual Task MainEffect()
    {
        return Task.CompletedTask;
    }

    // ========== 数值修正解析 ==========

    private int GetResolvedChargeUpgrade(int amount = 1)
    {
        var v = (decimal)amount;
        v += PlusHooks.Plus_TriggerModifyChargeUpgradeAddictive(v, Owner, this);
        v = PlusHooks.Plus_TriggerModifyChargeUpgradeMultiplicative(v, Owner, this) * v;
        v = PlusHooks.Plus_TriggerModifyChargeUpgrade(v, Owner, this);
        return Math.Max(0, (int)v);
    }

    private int GetResolvedChargeSpend()
    {
        var v = (decimal)TotalCharge;
        v += PlusHooks.Plus_TriggerModifyChargeSpendAddictive(v, Owner, this);
        v = PlusHooks.Plus_TriggerModifyChargeSpendMultiplicative(v, Owner, this) * v;
        v = PlusHooks.Plus_TriggerModifyChargeSpend(v, Owner, this);
        return Math.Max(0, (int)v);
    }

    private int GetResolvedMaxCharge()
    {
        var v = (decimal)TotalCharge;
        v += PlusHooks.Plus_TriggerModifyMaxChargeAddictive(v, Owner, this);
        v = PlusHooks.Plus_TriggerModifyMaxChargeMultiplicative(v, Owner, this) * v;
        v = PlusHooks.Plus_TriggerModifyMaxCharge(v, Owner, this);
        return Math.Max(0, (int)v);
    }
}
