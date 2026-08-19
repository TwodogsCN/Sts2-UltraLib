# UltraLib 工具辅助类 / Utils Helper

[English](Utils.md) · [中文](Utils.zh-CN.md)

**`Base/Utils`** 目录包含多个静态 `*Helper` 类,封装了常见的 StS2 操作(卡牌、能力、充能球、牌堆、发现等),让你不必重复实现或深入游戏私有 API。按你操作的对象挑选对应的 Helper。

本页覆盖**最常用的 Helper**。每个方法给出签名与简短说明。

> 所有辅助类都位于 `namespace UltraLib.Base.Utils;`。

---

## CardHelper

静态卡牌操作工具类,封装 `CardCmd` / `CardPileCmd` 的常用操作。

| 方法 | 说明 |
|------|------|
| `PreviewAddGeneratedCardToCombat(card, pile, player, position, style)` | 带有预览的生成卡牌到战斗。 |
| `PreviewCardPileAddResult(result)` | 预览一个牌堆添加的结果。 |
| `Exhaust(List<CardModel>)` / `Exhaust(CardModel)` | 消耗一个牌或一个列表的牌。 |
| `Upgrade(CardModel)` / `Upgrade(List<CardModel>)` | 升级一个牌或列表的牌。 |
| `Downgrade(CardModel)` / `Downgrade(List<CardModel>)` | 降级一个牌或列表的牌。 |
| `AddGeneratedCardToCombat(...)` | 生成卡牌到战斗(无预览)。 |
| `AddToPile(...)` | 添加卡牌/卡牌列表到牌堆。 |
| `GetModelDb(cardModel)` | 获取某卡的模型数据库条目。 |
| `Clone(cardModel)` | 克隆卡牌模型。 |
| `CloneOrigin(cardModel, player, combatState)` | 以原版形态克隆一张卡。 |
| `ApplyKeyword(card, keywords...)` | 给卡牌/列表施加卡牌关键词。 |
| `RemoveKeyword(card, keywords...)` | 移除卡牌关键词。 |
| `AddReturnVar(this card, value)` | 给卡牌增加"返回"动态变量(+数值)。 |
| `AddEmpowerVar(this card, power, value)` | 给卡牌增加赋能动态变量。 |
| `RemoveReturnVar(this card)` / `RemoveEmpowerVar(this card)` | 移除对应的动态变量。 |
| `RefreshHoverTips(this card)` | 刷新卡牌的悬浮提示。 |
| `AutoPlay(ctx, card, combatState, skipX)` / `AutoPlay(card, combatState, skipX)` | 自动打出卡牌(用于自我施放的卡)。 |
| `GetAutoTarget(card, combatState)` | 获取卡的自动目标(如有)。 |
| `SetCardType(this card, newType)` | 更改卡牌类型。 |
| `Discard(ctx, card(s))` | 弃掉一张或多张卡。 |
| `Preview(card, time, style)` | 展示卡牌预览。 |
| `Enchant<T>(card, amount)` / `Enchant(enchantment, card, amount)` | 给卡牌添加附魔。 |
| `CreateCard<T>(...)` / `CreateCard(canonicalCard, ...)` | 为战斗创建卡牌实例。 |
| `TransformTo<T>(card)` / `Transform(...)` | 把卡转化为另一张(可选预览)。 |
| `PreviewTransform(...)` | 预览卡牌转化。 |
| `PreviewSovereignBlade(...)` | 预览 Sovereign Blade 对卡牌的效果。 |

## PowerHelper

能力创建 / 施加辅助。

| 方法 | 说明 |
|------|------|
| `Apply<T>(target)` | 为目标添加指定类型 `T` 的能力。 |
| `Apply<T>(targets)` | 为多个目标添加指定类型的能力。 |
| `Apply(power)` | 施加指定的 `PowerModel` 实例(必须为 Mutable)。 |
| `GetPowerTip(power)` | 获取能力的悬浮提示。 |
| `RefreshVisuals(this power)` | 刷新该能力拥有者的视觉显示。 |
| `GetPower<T>(creature)` | 从生物身上获取类型 `T` 的能力。 |
| `Remove(power)` | 移除并清理指定能力。 |
| `Decrement(power)` | 能力层数减 1。 |
| `ModifyAmount(...)` | 修改能力层数。 |

## OrbHelper

充能球操作。

| 方法 | 说明 |
|------|------|
| `Channel<T>(ctx, player)` | 为玩家生成一个类型 `T` 的充能球。 |
| `NoAwaitChannel<T>(ctx, player)` | 安全地异步生成充能球(不阻塞调用方)。 |
| `EvokeOrb(ctx, player, ...)` | 通过反射调用原版私有 Evoke 方法激发充能球。 |
| `SetVal(orb, amount)` | 强制修改充能球数值并同步 UI。 |
| `GetOrbList(player)` | 安全获取当前充能球列表的只读快照。 |
| `RemoveSlots(player, amount)` | 移除充能球槽位(带战斗结束检查)。 |

## CardListHelper

卡牌列表操作。

| 方法 | 说明 |
|------|------|
| `FromPile(owner, pileType)` | 从玩家指定牌堆获取所有卡牌。 |
| `SelectCardFromHand(...)` | 从手牌中选择最多 `maxCount` 张(排除源卡)。 |
| `SelectCardFromList(...)` | 从列表中选择指定数量的卡。 |
| `SelectCardFromPile(...)` | 从指定牌堆选择卡牌(有序),支持 `min..max`。 |
| `Filter(rarity, cardList, cmp)` | 按稀有度筛选(`cmp`: 0=等于, <0=小于等于, >0=大于等于)。 |
| `Filter(energyCost, cardList, cmp)` | 按能量费用筛选(同 `cmp` 语义)。 |
| `RandomizeOrder(creature, cardList)` | 用生物所属战斗的 RNG 打乱卡牌。 |
| `RandomizeOrder(rng, cardList)` | 用指定 RNG 打乱卡牌。 |

## CardPileHelper

牌堆相关辅助。

| 方法 | 说明 |
|------|------|
| `AddToPile(card, pile, position)` | 把卡添加到指定牌堆的指定位置。 |
| `RandomizeOrderForPile(pile, player)` | 用游戏 RNG 随机打乱指定牌堆。 |
| `Draw(...)` | 从抽牌堆抽牌。 |

## DiscoverHelper

发现 / 选择辅助。

| 方法 | 说明 |
|------|------|
| `Discover(...)` | 从卡池随机展示卡牌供玩家选择;返回所选卡,跳过则返回 `null`。 |

---

其它辅助类(`ActionQueueHelper`、`AttackHelper`、`CreatureHelper`、`HandUiHelper`、`HoverTipHelper`、`LocStringHelper`、`RewardsHelper`、`RelicSelectionHelper`、`DynamicVarHelper`、`AncientEventHelper`)遵循同样的模式——封装各自领域的游戏操作。需要时在此补充细节。
