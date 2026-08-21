# 充能遗物 / Charge Relic

[English](ChargeRelic.md) · [中文](ChargeRelic.zh-CN.md)

`PlusChargeRelic`（`Base/Abstract/PlusChargeRelic.cs`）是 UltraLib 提供的**充能遗物抽象基类**。
它让遗物拥有「当前充能 / 最大充能」两个数值：充能会回复，满充后可触发效果并消耗充能。

## 设计思路

- 遗物有 `NowCharge`（当前充能）与 `TotalCharge`（最大充能）。
- 充能会回复：默认**进入每个房间自动 +1**（可关闭，改为自定义回复逻辑）。
- 满充（`NowCharge >= TotalCharge`）后触发效果，并**消耗充能**。
- **触发方式不写死**：右键、自动触发、回合开始触发等由**创作者自行决定**（见下文）。
- 效果逻辑统一写在 `MainEffect`，由 `DoChargeRelicEffect()` 执行——这样「触发次数」Hook 能正确包裹循环。

## 快速上手

继承 `PlusChargeRelic`，写两样东西：最大充能值、触发效果 `MainEffect`。

```csharp
public sealed class ExampleChargeRelic : PlusChargeRelic
{
    public ExampleChargeRelic()
    {
        TotalCharge = 5; // 需要 5 点充能才能触发
    }

    // ① 触发接线（示例：右键触发；也可以是自动/回合开始等，由你决定）
    public override async Task Plus_RelicRightClick(RelicModel relicModel, NRelicInventoryHolder? holder)
    {
        if (!IsFullyCharged) return;

        await DoChargeRelicEffect(); // 执行 MainEffect（含触发次数 Hook）
        SpendCharge();               // 消耗充能
    }

    // ② 充能触发时的实际效果
    public override async Task MainEffect()
    {
        await CreatureHelper.Damage(
            new BlockingPlayerChoiceContext(),
            Owner.Creature.CombatState.HittableEnemies.First(),
            20m, ValueProp.None, Owner.Creature, null, null);
    }
}
```

## 核心成员

| 成员 | 说明 |
|------|------|
| `NowCharge` | 当前充能数（`[SavedProperty]`，设值自动钳制 ≥ 0，并同步 `RelicStatus`）。 |
| `TotalCharge` | 最大充能数（`[SavedProperty]`，满充所需值）。 |
| `IsFullyCharged` | 是否已满充（`protected`）：`NowCharge >= TotalCharge`。 |
| `AutoCharge` | 是否进入房间时自动充能（`virtual`，默认 `true`）。 |
| `UseChargeBarDisplay` | 是否使用充能进度条 UI（`virtual`，默认 `true`；`false` 回退数字显示）。 |
| `GainCharge(amount = 1)` | 增加充能（经充能数值 Hook 修正）。 |
| `SetCharge(now, total)` / `SetCharge(amount)` | 设置充能（后者直接充满）。 |
| `SpendCharge()` | 消耗充能（消耗量经 `Plus_ModifyChargeSpend` 修正，默认清空全部）。 |
| `DoChargeRelicEffect()` | 效果总入口：计算重复次数 → 全局前后 Hook → 循环 `MainEffect`。 |
| `MainEffect()` | **效果核心入口**（`virtual`），子类覆写。 |

## 默认充能回复逻辑

`AutoCharge == true` 时，基类覆写 `AfterRoomEntered`：每次进入房间自动 `NowChargeUpgrade()`（+1）。

```csharp
public override Task AfterRoomEntered(AbstractRoom room)
{
    NowCharge = _nowCharge;
    if (AutoCharge)
        NowChargeUpgrade(); // 充能 +1
    return Task.CompletedTask;
}
```

## 不用默认逻辑：自定义充能回复

把 `AutoCharge` 覆写为 `false`，然后在合适的地方（战斗 Hook、回合开始 Hook 等）手动 `GainCharge(amount)`：

```csharp
public sealed class CustomChargeRelic : PlusChargeRelic
{
    public CustomChargeRelic() { TotalCharge = 3; }

    public override bool AutoCharge => false; // 关闭进房间 +1

    public override Task AfterCombatEnd(CombatRoom room)
    {
        GainCharge(2); // 每场战斗结束充能 +2
        return Task.CompletedTask;
    }

    public override async Task MainEffect() { /* 你的效果 */ }
}
```

## 触发方式（不写死）

`PlusChargeRelic` **不绑定**触发方式。创作者在任意触发点（右键 Hook、自动 Hook、回合开始等）：
检查 `IsFullyCharged` → 调用 `DoChargeRelicEffect()` → 调用 `SpendCharge()`。
只要效果写在 `MainEffect`、通过 `DoChargeRelicEffect` 执行，「触发次数」Hook
（`Plus_ModifyChargeRepeatTimes` 系列）就能自动、正确地包裹并重复执行你的效果。

## 相关 Hook

充能相关 Hook 全部定义在 `IPlusHooks`（基类 `PlusRelicModel` 提供空实现，只覆写需要的即可）：

| 类别 | Hook | 时机 |
|------|------|------|
| 数值修正 | `Plus_ModifyMaxCharge` / `...Multiplicative` / `...Addictive` | 修正最大充能 |
| 数值修正 | `Plus_ModifyChargeUpgrade` / `...Multiplicative` / `...Addictive` | 修正每次充能获得量 |
| 数值修正 | `Plus_ModifyChargeSpend` / `...Multiplicative` / `...Addictive` | 修正每次触发消耗量 |
| 数值修正 | `Plus_ModifyChargeRepeatTimes` / `...Multiplicative` / `...Addictive` | 修正触发重复次数 |
| 事件 | `Plus_OnChargeFullyCharged` | 充能到达满值 |
| 事件 | `Plus_OnChargeNoLongerFullyCharged` | 充能不再满值 |
| 事件 | `Plus_OnChargeChanged(old, new, ...)` | 充能数值变化 |
| 事件 | `Plus_AfterChargeGain` / `Plus_AfterChargeSpend` | 获得 / 消耗充能后 |
| 事件 | `Plus_BeforeChargeEffected` / `Plus_AfterChargeEffected` | 单次效果前 / 后 |
| 事件 | `Plus_BeforeChargeTotallyEffected` / `Plus_AfterChargeTotallyEffected` | 整个效果流程前 / 后 |

修改器组合约定：`Modify((amount + Addictive) × Multiplicative)`。详见 [Hook 系统](Hook.zh-CN.md)。

## 其他功能

- **充能条 UI**：`UseChargeBarDisplay = true` 时，`ChargeRelicUiPatch` 自动在遗物图标下方渲染充能进度条（未满绿色，过载变色）。
- **右键同步**：右键经 `RelicRightClickAction`（GameAction）+ `RelicRightClickSyncNet` 联机同步，多端一致执行。
- **保存**：`NowCharge` / `TotalCharge` 均为 `[SavedProperty]`，随存档保存/读取。

## 参考

- 完整 API 地图：[API_INDEX.zh-CN.md](API_INDEX.zh-CN.md)
- 代码规范：[CODE_CONVENTIONS.zh-CN.md](CODE_CONVENTIONS.zh-CN.md)
