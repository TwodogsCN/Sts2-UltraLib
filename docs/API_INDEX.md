# UltraLib API / Feature Index

[English](API_INDEX.md) · [中文](API_INDEX.zh-CN.md)

A map of everything **UltraLib** provides, so mod authors can quickly find the piece they need. Namespaces mirror folder paths; every public type below lives in `namespace UltraLib.<path>;`.

## 1. Abstract base models — `Base/Abstract`

The building blocks for content mods. Inherit these and override the members you need.

| Type | What it gives you |
|------|-------------------|
| `PlusRelicModel` | Base for custom relics. Adds `RelicLevel` (appearance weight), `ItemPool`, `Tags`, typed internal data (`GetInternalData<T>` / `GetOrInitInternalData<T>`), correct `DeepCloneFields`, and empty default implementations of **all** `IPlusHooks`. This is the usual relic base to subclass. |
| `PlusPowerModel` | Base for custom powers, wired to the hook system. |
| `PlusSingletonModel` | Base for singletons (one-instance-per-run models). |
| `PlusChargeRelic` | Charge-based relic base: auto-gains charge on entering a room (unless `AutoCharge` is `false`); when fully charged the player right-clicks to trigger the effect and consume charge. |

## 2. Hook system — `Hook`

> 📘 Detailed guide: [Hook.md](Hook.md) · [钩子系统（中文）](Hook.zh-CN.md)

The main extension point. Modifiers use composable `Pipeline` / `Product` / `Sum` semantics; event hooks dispatch safely and in order.

| Type | What it gives you |
|------|-------------------|
| `IPlusHooks` | Contract with **default implementations**, so implementers only override what they need. |
| `PlusHooks` | Static dispatcher: gathers hook listeners from the current run/combat state and applies them. |
| `HookPatches/*` | Harmony patches that raise the hooks (`ChargeRelicUiPatch`, `GoldPatch`, `OrbHooksPatches`, `RandomPositionFixPatch`, `RandomRoomRolledPatch`, `RelicObtainPatch`, `RelicRightClickPatch`). |

**Hook categories covered** (see `IPlusHooks`): Room · Hand · Card · Power · Relic · Charge · Gold · Orb · Isomorphism · CastWhenDrawn · CardReturn · Empower. For any modifiable value `X` there are typically three hooks: `Plus_ModifyX` (pipeline), `Plus_ModifyXMultiplicative` (product), `Plus_ModifyXAddictive` (sum).

## 3. Utility helpers — `Base/Utils`

> 📘 Detailed guide: [Utils.md](Utils.md) · [工具辅助类（中文）](Utils.zh-CN.md)

Static `*Helper` classes for common operations. Pick by what you're touching.

| Helper | Typical use |
|--------|-------------|
| `CardHelper` | Card model/shape creation and manipulation. |
| `CardListHelper` | Operations over card lists/piles. |
| `CardPileHelper` | Helpers for draw/discard/hand piles. |
| `CardExporter` | (see also `Base/Exporter/CardExporter`) card export support. |
| `CreatureHelper` | Creature-related operations. |
| `PowerHelper` | Power creation/application helpers. |
| `OrbHelper` | Orb helpers (channels, evocations, passives). |
| `ActionQueueHelper` | Queuing custom actions. |
| `AttackHelper` | Attack-related helpers. |
| `AncientEventHelper` | Ancient-event interactions. |
| `DiscoverHelper` | Discover / choice-screen helpers. |
| `RewardsHelper` | Reward screen helpers. |
| `HandUiHelper` | In-hand UI manipulation. |
| `HoverTipHelper` | Hover-tip wiring (used with `HoverTip/PlusHoverTips`). |
| `DynamicVarHelper` | Dynamic-variable helpers. |
| `LocStringHelper` | Localization string lookup. |
| `RelicSelectionHelper` | Relic selection helpers. |

## 4. Relic building blocks — `Base/Relic`, `Base/Label`

| Type | What it gives you |
|------|-------------------|
| `PlusRelicLevel` | Relic rarity/level enum controlling appearance weight. |
| `RelicItemPool` | Relic item-pool flags. |
| `PlusRelicTags` | Relic tag enum. |
| `PlusCardKeyWord` | Custom card keyword marker. |
| `PlusCardTags` | Custom card tag enum. |

## 5. Singletons — `Base/Singleton`

| Type | What it gives you |
|------|-------------------|
| `CastWhenDrawnSingleton` | Backing singleton for the cast-when-drawn hook. |
| `IsomorphismSingleton` | Backing singleton for the isomorphism hook. |

## 6. Multiplayer / networking — `Base/Multiplayer`, `Net`

| Type | What it gives you |
|------|-------------------|
| `Base/Multiplayer/Cmds/PlusRelicSelectCmd` | Multiplayer command for relic selection. |
| `Net/RelicRightClickSync` | Network sync for the relic right-click action. |
| `GameActions/RelicRightClickAction` | The custom game action behind relic right-click. |

## 7. Variables — `Variables`

| Type | What it gives you |
|------|-------------------|
| `EmpowerVar` | Dynamic variable for empower (with `VariablePatches/EmpowerVarPatch`). |
| `ReturnVar` | Dynamic variable for card return (with `VariablePatches/ReturnVarPatch`). |
| `RoseVars` | Dynamic variables for rose-related modifiers. |

## 8. Misc / scripts / UI

| Type | What it gives you |
|------|-------------------|
| `Base/Scripts/NSimpleRelicSelectScreen` | A simple relic-select screen scene/script. |
| `Base/Exporter/CardExporter` | Card export helper. |
| `Base/Patches/OrbChannelPatch` | Harmony patch for orb channeling. |
| `HoverTip/PlusHoverTips` | Hover-tip support integration. |
| `Test/TestCards` | Sample/test cards showing usage. |
| `UltraLibCode/MainFile` | Mod entry point: `ModId`, `Logger`, Harmony init + `PatchAll`. |

## Localization

User-facing strings go in `UltraLib/localization/{eng,zhs}/` (`cards.json`, `powers.json`, `card_keywords.json`, `static_hover_tips.json`) — never hardcoded.

---

For naming/style conventions and how the hook system composes, see [CODE_CONVENTIONS.md](CODE_CONVENTIONS.md). For how to build mods against this library, see [README](../README.md).
