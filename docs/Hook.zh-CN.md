# UltraLib 钩子系统 / Hook System

[English](Hook.md) · [中文](Hook.zh-CN.md)

**钩子系统（Hook System)** 是 UltraLib 最主要的扩展点。它让你的模组能够响应游戏事件(遗物获得、充能球触发、卡牌返回、金币变化、充能遗物充满等),并能以可组合的方式**修改数值**(金币损失/获得、充能、卡牌效果等)。

需要理解三个组成部分:

| 组成 | 文件 | 作用 |
|------|------|------|
| `IPlusHooks` | `Hook/IPlusHooks.cs` | **契约**:声明监听者可以实现的每一个钩子。 |
| `PlusHooks` | `Hook/PlusHooks.cs` | **分发器**:从当前 run/combat 状态收集监听者并触发钩子。 |
| `HookPatches/*` | `Hook/HookPatches/` | **Harmony 补丁**:检测游戏事件,在正确时机调用分发器。 |

---

## 1. 工作原理(概念)

1. 发生一个游戏事件(例如玩家获得金币)。
2. `HookPatches/` 里的 Harmony 补丁被触发。
3. 补丁调用对应的 `PlusHooks.Plus_TriggerXxx(...)` 分发方法。
4. 分发器从当前 run/combat 状态收集所有存活的"钩子监听者"。
5. 分发器对每个监听者应用钩子——事件钩子用 `Dispatch`,数值修改钩子用 `Pipeline` / `Product` / `Sum`。

**谁能成为钩子监听者?** 任何实现了 `IPlusHooks` 且位于 run/combat 状态中的对象。你的内容模型(遗物、能力、单例)实现 `IPlusHooks` 后,只要处于激活状态就会自动被收集到。

## 2. 契约:`IPlusHooks`

`IPlusHooks`(位于 `UltraLib.Hook`)按领域分组声明钩子。每个成员都有**默认实现**,所以你只需要覆写你想用的那部分——永远不必写空方法。

### 事件钩子(触发后执行)— `Task`

这些钩子让你对某事做出响应,返回 `Task`:

- **房间:** `Plus_AfterRandomRoomRolled(RoomType)`
- **手牌:** `Plus_AfterHandPileMoved(CardModel)`, `Plus_BeforeHandPileMoved(CardModel)`
- **能力:** `Plus_PowerRightClick(PowerModel, NPower)`
- **遗物:** `Plus_RelicRightClick(RelicModel, NRelicInventoryHolder?)`, `Plus_AfterRelicObtain(IRunState, RelicModel, Player)`, `Plus_BeforeRelicObtain(IRunState, RelicModel, Player)`, `Plus_CardRightClick(CardModel, NCardHolder)`
- **充能:** `Plus_AfterChargeSpend`, `Plus_AfterChargeGain`, `Plus_OnChargeFullyCharged`, `Plus_OnChargeNoLongerFullyCharged`, `Plus_OnChargeChanged(int old, int new, ...)`, `Plus_AfterChargeEffected`, `Plus_AfterChargeTotallyEffected`, `Plus_BeforeChargeEffected`, `Plus_BeforeChargeTotallyEffected`
- **充能球:** `Plus_AfterOrbEvokeRemoved`, `Plus_BeforeOrbEvoke`, `Plus_BeforeOrbPassive`, `Plus_AfterOrbPassive`
- **关键词 / 卡牌:** `Plus_BeforeIsomorphism(CardModel)`, `Plus_AfterIsomorphism(CardModel)`, `Plus_BeforeCastWhenDrawn(...)`, `Plus_AfterCastWhenDrawn(...)`
- **动态变量:** `Plus_BeforeCardReturn(CardModel)`, `Plus_AfterCardReturn(CardModel)`
- **赋能:** `Plus_BeforeCardEmpower(CardModel, EmpowerVar, List<Creature>)`, `Plus_AfterCardEmpower(...)`

### 数值修改钩子 — `decimal`

这些钩子**修改一个数字**。对于每个可修改值 `X`,有三个钩子:

| 钩子 | 语义 | 恒等元 |
|------|------|--------|
| `Plus_ModifyX(amount, ...)` | **Pipeline** —— 把运行中的值依次传入,每个监听者可以替换它 | `amount` |
| `Plus_ModifyXMultiplicative(amount, ...)` | **Product** —— 把所有监听者的系数相乘 | `1m` |
| `Plus_ModifyXAddictive(amount, ...)` | **Sum** —— 把所有监听者的增量相加 | `0m` |

三个钩子由分发器/补丁按以下方式组合:
`Modify( (amount + Addictive) × Multiplicative )`

即:先求和所有加算增量,再整体乘以所有乘算系数的积,最后通过管线 `ModifyX` 进一步设置/替换数值。(这与 `HookPatches/GoldPatch.cs` 中的金币获得/损失补丁一致。)

覆盖的数值:**金币获得与损失**、**MaxCharge / ChargeUpgrade / ChargeSpend / ChargeRepeatTimes**、以及 **RoseCard**。

## 3. 分发器:`PlusHooks`

`PlusHooks` 是位于 `UltraLib.Hook` 的静态类。你通常在自己的代码里**调用**它来触发一个钩子,或读取它来理解某数值是如何计算的。

内部使用的主要模式:

- `Dispatch(action)` —— 对每个监听者按顺序 `await action(listener)`(用于 `Task` 钩子)。
- `Pipeline(initial, action)` —— 依次 `current = action(listener, current)`(set/replace 型修改器)。
- `Product(initial, action)` —— 跨监听者相乘系数。
- `Sum(initial, action)` —— 跨监听者相加增量。

> **修改器约定:** 当你新增一个可修改值 `X` 时,暴露三个钩子——`Plus_ModifyX`(pipeline)、`Plus_ModifyXMultiplicative`(product)、`Plus_ModifyXAddictive`(sum),以便任意组合都能复合。参见 [CODE_CONVENTIONS.zh-CN.md](CODE_CONVENTIONS.zh-CN.md)。

## 4. 实现一个钩子监听者

你的内容有两种方式成为钩子监听者:

**A. 继承 Plus 基类模型(推荐)。** `PlusRelicModel`、`PlusPowerModel`、`PlusSingletonModel` 都实现了 `IPlusHooks` 并给你空的 `virtual` 覆写方法。只需覆写你需要的:

```csharp
public class MyRelic : PlusRelicModel
{
    // 玩家获得金币后调用——将其减半。
    public override decimal Plus_ModifyGoldGain(decimal amount, Player player, bool wasStolenBack)
        => amount * 0.5m;
}
```

**B. 在任何模型上直接实现 `IPlusHooks`。** 因为所有成员都有默认实现,你只需写用到的那些。

## 5. 从你自己的代码触发钩子

当你构建的效果希望其它模组也能扩展时,触发一个钩子:

```csharp
// 示例:让其它监听者修改一个自定义数值
decimal final = PlusHooks.Plus_TriggerModifyMaxCharge(100m, player, relic);
```

事件钩子:

```csharp
await PlusHooks.Plus_TriggerAfterRelicObtain(runState, relic, player);
```

## 6. 扩展钩子系统(新增一个钩子)

如果 UltraLib 没有覆盖你需要的某个事件,按 [CODE_CONVENTIONS.zh-CN.md](CODE_CONVENTIONS.zh-CN.md) §4 添加:

1. 在 `IPlusHooks` 中添加该成员(带默认实现)。
2. 在 `PlusHooks` 中添加分发方法。
3. 在每个实现了 `IPlusHooks` 的 `Plus*Model` 中添加空覆写。
4. 在正确的生命周期点从相关 `HookPatches/` Harmony 补丁中触发它。

---

完整的提供类型列表见 [API_INDEX.zh-CN.md](API_INDEX.zh-CN.md),约定见 [CODE_CONVENTIONS.zh-CN.md](CODE_CONVENTIONS.zh-CN.md)。
