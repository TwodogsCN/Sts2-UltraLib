# UltraLib Utils Helper

[English](Utils.md) · [中文](Utils.zh-CN.md)

The **`Base/Utils`** folder contains static `*Helper` classes that wrap common StS2 operations (cards, powers, orbs, piles, discover, …) so you don't have to reimplement them or reach into private game APIs. Choose the helper that matches what you're touching.

This page covers the most commonly used helpers. Each method lists its signature and a short explanation.

> All helpers live in `namespace UltraLib.Base.Utils;`.

---

## CardHelper

Static card-operation utilities wrapping `CardCmd` / `CardPileCmd`.

| Method | What it does |
|--------|--------------|
| `PreviewAddGeneratedCardToCombat(card, pile, player, position, style)` | Generate a card into combat with a preview. |
| `PreviewCardPileAddResult(result)` | Preview the result of a pile add. |
| `Exhaust(List<CardModel>)` / `Exhaust(CardModel)` | Exhaust a card or a list of cards. |
| `Upgrade(CardModel)` / `Upgrade(List<CardModel>)` | Upgrade a card or a list of cards. |
| `Downgrade(CardModel)` / `Downgrade(List<CardModel>)` | Downgrade a card or a list of cards. |
| `AddGeneratedCardToCombat(...)` | Add a generated card to combat (no preview). |
| `AddToPile(...)` | Add a card / card list to a pile. |
| `GetModelDb(cardModel)` | Get the model database entry for a card. |
| `Clone(cardModel)` | Clone a card model. |
| `CloneOrigin(cardModel, player, combatState)` | Clone a card as its origin form. |
| `ApplyKeyword(card, keywords...)` | Apply card keyword(s) to a card / list. |
| `RemoveKeyword(card, keywords...)` | Remove card keyword(s). |
| `AddReturnVar(this card, value)` | Add a "return" dynamic variable (+value) to a card. |
| `AddEmpowerVar(this card, power, value)` | Add an empower dynamic variable to a card. |
| `RemoveReturnVar(this card)` / `RemoveEmpowerVar(this card)` | Remove the corresponding dynamic variable. |
| `RefreshHoverTips(this card)` | Refresh the card's hover tips. |
| `AutoPlay(ctx, card, combatState, skipX)` / `AutoPlay(card, combatState, skipX)` | Autoplay a card (used for cards that play themselves). |
| `GetAutoTarget(card, combatState)` | Get the auto-target for a card, if any. |
| `SetCardType(this card, newType)` | Change a card's type. |
| `Discard(ctx, card(s))` | Discard card(s). |
| `Preview(card, time, style)` | Show a card preview. |
| `Enchant<T>(card, amount)` / `Enchant(enchantment, card, amount)` | Add an enchantment to a card. |
| `CreateCard<T>(...)` / `CreateCard(canonicalCard, ...)` | Create a card instance for combat. |
| `TransformTo<T>(card)` / `Transform(...)` | Transform a card into another (with optional preview). |
| `PreviewTransform(...)` | Preview a card transformation. |
| `PreviewSovereignBlade(...)` | Preview the Sovereign Blade effect on card(s). |

## PowerHelper

Power creation / application helpers.

| Method | What it does |
|--------|--------------|
| `Apply<T>(target)` | Add a power of type `T` to a target. |
| `Apply<T>(targets)` | Add a power of type `T` to multiple targets. |
| `Apply(power)` | Apply a specific `PowerModel` instance (must be mutable). |
| `GetPowerTip(power)` | Get a power's hover tip. |
| `RefreshVisuals(this power)` | Refresh the owning creature's visuals for this power. |
| `GetPower<T>(creature)` | Get a power of type `T` from a creature. |
| `Remove(power)` | Remove and clean up a power. |
| `Decrement(power)` | Decrease a power's stacks by 1. |
| `ModifyAmount(...)` | Modify a power's stack amount. |

## OrbHelper

Orb (charger) operations.

| Method | What it does |
|--------|--------------|
| `Channel<T>(ctx, player)` | Generate a charge orb of type `T` for a player. |
| `NoAwaitChannel<T>(ctx, player)` | Safely channel an orb without blocking the caller. |
| `EvokeOrb(ctx, player, ...)` | Evoke an orb via reflection on the game's private evoke method. |
| `SetVal(orb, amount)` | Force-set an orb's value and sync the UI. |
| `GetOrbList(player)` | Get a safe read-only snapshot of the current orb list. |
| `RemoveSlots(player, amount)` | Remove orb slots (with end-of-combat check). |

## CardListHelper

Operations over lists of cards.

| Method | What it does |
|--------|--------------|
| `FromPile(owner, pileType)` | Get all cards from a player's pile. |
| `SelectCardFromHand(...)` | Choose up to `maxCount` cards from hand (excluding a source card). |
| `SelectCardFromList(...)` | Choose a number of cards from a list. |
| `SelectCardFromPile(...)` | Choose card(s) from a pile (ordered), `min..max` supported. |
| `Filter(rarity, cardList, cmp)` | Filter by rarity (`cmp`: 0 = equal, <0 = <=, >0 = >=). |
| `Filter(energyCost, cardList, cmp)` | Filter by energy cost (same `cmp` semantics). |
| `RandomizeOrder(creature, cardList)` | Shuffle with the creature's combat RNG. |
| `RandomizeOrder(rng, cardList)` | Shuffle with a given RNG. |

## CardPileHelper

Pile-specific helpers.

| Method | What it does |
|--------|--------------|
| `AddToPile(card, pile, position)` | Add a card to a pile at a position. |
| `RandomizeOrderForPile(pile, player)` | Shuffle a pile with the game RNG. |
| `Draw(...)` | Draw cards from the draw pile. |

## DiscoverHelper

Discover / choice helpers.

| Method | What it does |
|--------|--------------|
| `Discover(...)` | Show cards from a pool for the player to pick; returns the chosen card or `null` if skipped. |

---

Other helpers (`ActionQueueHelper`, `AttackHelper`, `CreatureHelper`, `HandUiHelper`, `HoverTipHelper`, `LocStringHelper`, `RewardsHelper`, `RelicSelectionHelper`, `DynamicVarHelper`, `AncientEventHelper`) follow the same pattern — they wrap game operations for their respective domain. Add details here as needed.
