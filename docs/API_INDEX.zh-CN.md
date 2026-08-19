# UltraLib 功能 / API 索引

[English](API_INDEX.md) · [中文](API_INDEX.zh-CN.md)

本库为模组作者提供的**全部内容地图**，方便快速找到所需的功能。命名空间与文件夹路径一一对应，以下所有公共类型都位于 `namespace UltraLib.<路径>;`。

## 1. 抽象基类模型 — `Base/Abstract`

内容型模组的构建基石。继承它们并覆写你需要的成员。

| 类型 | 提供能力 |
|------|----------|
| `PlusRelicModel` | 自定义遗物基类。新增 `RelicLevel`（出现权重）、`ItemPool`、`Tags`、类型化内部数据（`GetInternalData<T>` / `GetOrInitInternalData<T>`）、正确的 `DeepCloneFields`，以及**全部 `IPlusHooks`** 的空默认实现。这是通常应继承的遗物基类。 |
| `PlusPowerModel` | 自定义能力基类，已接入钩子系统。 |
| `PlusSingletonModel` | 单例型模型基类（每次 run 一个实例的模型）。 |
| `PlusChargeRelic` | 充能遗物基类：进入房间时自动获得充能（除非 `AutoCharge` 为 `false`）；充能满后玩家右键触发效果并消耗充能。 |

## 2. 钩子系统 — `Hook`

> 📘 详细指南: [Hook.md](Hook.md) · [钩子系统（中文）](Hook.zh-CN.md)

最主要的扩展点。修改器采用可组合的 `Pipeline` / `Product` / `Sum` 语义；事件钩子安全、有序地分发。

| 类型 | 提供能力 |
|------|----------|
| `IPlusHooks` | 带**默认实现**的契约，实现者只需覆写需要的部分。 |
| `PlusHooks` | 静态分发器：从当前 run/combat 状态收集钩子监听者并应用。 |
| `HookPatches/*` | 触发钩子的 Harmony 补丁（`ChargeRelicUiPatch`、`GoldPatch`、`OrbHooksPatches`、`RandomPositionFixPatch`、`RandomRoomRolledPatch`、`RelicObtainPatch`、`RelicRightClickPatch`）。 |

**覆盖的钩子类别**（见 `IPlusHooks`）：房间 · 手牌 · 卡牌 · 能力 · 遗物 · 充能 · 金币 · 宝珠 · 同构 · 抽到时施放 · 卡牌返回 · 赋能。对于任何可修改值 `X`，通常有三个钩子：`Plus_ModifyX`（pipeline）、`Plus_ModifyXMultiplicative`（product）、`Plus_ModifyXAddictive`（sum）。

## 3. 工具辅助类 — `Base/Utils`

> 📘 详细指南: [Utils.md](Utils.md) · [工具辅助类（中文）](Utils.zh-CN.md)

静态 `*Helper` 类，按你操作的对象挑选。

| Helper | 典型用途 |
|--------|----------|
| `CardHelper` | 卡牌模型/卡的创建与操作。 |
| `CardListHelper` | 卡牌列表/卡组的操作。 |
| `CardPileHelper` | 抽牌堆/弃牌堆/手牌堆的辅助。 |
| `CardExporter` |（另见 `Base/Exporter/CardExporter`）卡牌导出支持。 |
| `CreatureHelper` | 生物相关操作。 |
| `PowerHelper` | 能力创建/施加辅助。 |
| `OrbHelper` | 宝珠辅助（充能、触发、被动）。 |
| `ActionQueueHelper` | 排队自定义动作。 |
| `AttackHelper` | 攻击相关辅助。 |
| `AncientEventHelper` | 古老事件互动。 |
| `DiscoverHelper` | 发现/选择界面辅助。 |
| `RewardsHelper` | 奖励界面辅助。 |
| `HandUiHelper` | 手牌中 UI 操作。 |
| `HoverTipHelper` | 悬浮提示接线（配合 `HoverTip/PlusHoverTips`）。 |
| `DynamicVarHelper` | 动态变量辅助。 |
| `LocStringHelper` | 本地化字符串查询。 |
| `RelicSelectionHelper` | 遗物选择辅助。 |

## 4. 遗物与标签构建块 — `Base/Relic`、`Base/Label`

| 类型 | 提供能力 |
|------|----------|
| `PlusRelicLevel` | 遗物稀有度/等级枚举，控制出现权重。 |
| `RelicItemPool` | 遗物出现池标记。 |
| `PlusRelicTags` | 遗物标签枚举。 |
| `PlusCardKeyWord` | 自定义卡牌关键词标记。 |
| `PlusCardTags` | 自定义卡牌标签枚举。 |

## 5. 单例 — `Base/Singleton`

| 类型 | 提供能力 |
|------|----------|
| `CastWhenDrawnSingleton` | 抽到时施放钩子的底层单例。 |
| `IsomorphismSingleton` | 同构钩子的底层单例。 |

## 6. 多人 / 网络 — `Base/Multiplayer`、`Net`

| 类型 | 提供能力 |
|------|----------|
| `Base/Multiplayer/Cmds/PlusRelicSelectCmd` | 遗物选择的多人命令。 |
| `Net/RelicRightClickSync` | 遗物右键动作的网络同步。 |
| `GameActions/RelicRightClickAction` | 遗物右键背后的自定义游戏动作。 |

## 7. 变量 — `Variables`

| 类型 | 提供能力 |
|------|----------|
| `EmpowerVar` | 赋能动态变量（含 `VariablePatches/EmpowerVarPatch`）。 |
| `ReturnVar` | 卡牌返回动态变量（含 `VariablePatches/ReturnVarPatch`）。 |
| `RoseVars` | 玫瑰相关修改器的动态变量。 |

## 8. 其它 / 脚本 / UI

| 类型 | 提供能力 |
|------|----------|
| `Base/Scripts/NSimpleRelicSelectScreen` | 一个简单的遗物选择界面场景/脚本。 |
| `Base/Exporter/CardExporter` | 卡牌导出辅助。 |
| `Base/Patches/OrbChannelPatch` | 宝珠充能的 Harmony 补丁。 |
| `HoverTip/PlusHoverTips` | 悬浮提示支持集成。 |
| `Test/TestCards` | 展示用法的示例/测试卡牌。 |
| `UltraLibCode/MainFile` | 模组入口：`ModId`、`Logger`、Harmony 初始化 + `PatchAll`。 |

## 本地化

面向用户的字符串放在 `UltraLib/localization/{eng,zhs}/`（`cards.json`、`powers.json`、`card_keywords.json`、`static_hover_tips.json`）——绝不硬编码。

---

命名/风格约定与钩子系统如何组合，见 [CODE_CONVENTIONS.zh-CN.md](CODE_CONVENTIONS.zh-CN.md)。如何基于本库构建模组，见 [README](../README.md)。
