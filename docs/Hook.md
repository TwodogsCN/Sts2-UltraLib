# UltraLib Hook System

[English](Hook.md) · [中文](Hook.zh-CN.md)

The **hook system** is UltraLib's main extension point. It lets your mod react to game events (a relic is obtained, an orb is evoked, a card is returned, gold changes, a charge relic fills up, and so on) and **modify values** (gold loss/gain, charges, card effects) in a composable way.

There are three pieces you need to understand:

| Piece | File | Role |
|-------|------|------|
| `IPlusHooks` | `Hook/IPlusHooks.cs` | The **contract**: declares every hook a listener can implement. |
| `PlusHooks` | `Hook/PlusHooks.cs` | The **dispatcher**: collects listeners from the current run/combat state and raises the hooks. |
| `HookPatches/*` | `Hook/HookPatches/` | **Harmony patches** that detect game events and call into the dispatcher at the right moment. |

---

## 1. How it works (concept)

1. A game event happens (e.g. the player gains gold).
2. A Harmony patch in `HookPatches/` fires.
3. The patch calls the matching `PlusHooks.Plus_TriggerXxx(...)` dispatcher method.
4. The dispatcher gathers every live "hook listener" from the current run/combat state.
5. The dispatcher applies the hook to each listener — either as an event (`Dispatch`) or as a value modifier (`Pipeline` / `Product` / `Sum`).

**Who can be a hook listener?** Any object in the run/combat state that implements `IPlusHooks`. Your content models (relics, powers, singletons) implement `IPlusHooks` and are automatically picked up while they are active.

## 2. The contract: `IPlusHooks`

`IPlusHooks` (in `UltraLib.Hook`) declares hooks grouped by domain. Every member has a **default implementation**, so you only override the ones you care about — you never have to write empty methods.

### Event (fire-and-forget) hooks — `Task`

These let you react to something happening. Each returns `Task`:

- **Room:** `Plus_AfterRandomRoomRolled(RoomType)`
- **Hand:** `Plus_AfterHandPileMoved(CardModel)`, `Plus_BeforeHandPileMoved(CardModel)`
- **Power:** `Plus_PowerRightClick(PowerModel, NPower)`
- **Relic:** `Plus_RelicRightClick(RelicModel, NRelicInventoryHolder?)`, `Plus_AfterRelicObtain(IRunState, RelicModel, Player)`, `Plus_BeforeRelicObtain(IRunState, RelicModel, Player)`, `Plus_CardRightClick(CardModel, NCardHolder)`
- **Charge:** `Plus_AfterChargeSpend`, `Plus_AfterChargeGain`, `Plus_OnChargeFullyCharged`, `Plus_OnChargeNoLongerFullyCharged`, `Plus_OnChargeChanged(int old, int new, ...)`, `Plus_AfterChargeEffected`, `Plus_AfterChargeTotallyEffected`, `Plus_BeforeChargeEffected`, `Plus_BeforeChargeTotallyEffected`
- **Orb:** `Plus_AfterOrbEvokeRemoved`, `Plus_BeforeOrbEvoke`, `Plus_BeforeOrbPassive`, `Plus_AfterOrbPassive`
- **Keyword/Card:** `Plus_BeforeIsomorphism(CardModel)`, `Plus_AfterIsomorphism(CardModel)`, `Plus_BeforeCastWhenDrawn(...)`, `Plus_AfterCastWhenDrawn(...)`
- **Dynamic vars:** `Plus_BeforeCardReturn(CardModel)`, `Plus_AfterCardReturn(CardModel)`
- **Empower:** `Plus_BeforeCardEmpower(CardModel, EmpowerVar, List<Creature>)`, `Plus_AfterCardEmpower(...)`

### Value-modifier hooks — `decimal`

These **modify a number**. For each modifiable value `X`, there are three hooks:

| Hook | Semantics | Identity |
|------|-----------|----------|
| `Plus_ModifyX(amount, ...)` | **Pipeline** — pass the running value through, each listener may replace it | `amount` |
| `Plus_ModifyXMultiplicative(amount, ...)` | **Product** — multiply all listeners' factors together | `1m` |
| `Plus_ModifyXAddictive(amount, ...)` | **Sum** — add all listeners' deltas | `0m` |

The three are combined by the dispatcher/patches as:
`Modify( (amount + Addictive) × Multiplicative )`

So: additive deltas are summed first, then the whole thing is multiplied by the product of all multiplicative factors, then the final pipeline `ModifyX` may further set/replace the value. (This mirrors the gold-gain/loss patches in `HookPatches/GoldPatch.cs`.)

Covered values: **Gold loss & gain**, **MaxCharge / ChargeUpgrade / ChargeSpend / ChargeRepeatTimes**, and **RoseCard**.

## 3. The dispatcher: `PlusHooks`

`PlusHooks` is a static class in `UltraLib.Hook`. You usually **call** it from your own code to raise a hook, or read it to understand how a value is computed.

Key patterns used internally:

- `Dispatch(action)` — awaits `action(listener)` for each listener in order (for `Task` hooks).
- `Pipeline(initial, action)` — `current = action(listener, current)` in turn (set/replace modifiers).
- `Product(initial, action)` — multiply factors across listeners.
- `Sum(initial, action)` — add deltas across listeners.

> **Modifier convention:** when you add a new modifiable value `X`, expose three hooks — `Plus_ModifyX` (pipeline), `Plus_ModifyXMultiplicative` (product), `Plus_ModifyXAddictive` (sum) — so arbitrary combinations compose. See [CODE_CONVENTIONS.md](CODE_CONVENTIONS.md).

## 4. Implementing a hook listener

There are two ways your content becomes a hook listener:

**A. Inherit a Plus base model** (recommended). `PlusRelicModel`, `PlusPowerModel`, `PlusSingletonModel` all implement `IPlusHooks` and give you empty `virtual` overrides. Just override what you need:

```csharp
public class MyRelic : PlusRelicModel
{
    // Called after the player gains gold — reduce it by half.
    public override decimal Plus_ModifyGoldGain(decimal amount, Player player, bool wasStolenBack)
        => amount * 0.5m;
}
```

**B. Implement `IPlusHooks` directly** on any model. Because all members have defaults, you only write the ones you use.

## 5. Raising a hook from your own code

When you build your own effect that other mods should be able to extend, raise a hook:

```csharp
// Example: let other listeners modify a custom value
decimal final = PlusHooks.Plus_TriggerModifyMaxCharge(100m, player, relic);
```

For event hooks:

```csharp
await PlusHooks.Plus_TriggerAfterRelicObtain(runState, relic, player);
```

## 6. Extending the hook system (adding a new hook)

If UltraLib doesn't cover an event you need, add the hook following [CODE_CONVENTIONS.md](CODE_CONVENTIONS.md) §4:

1. Add the member to `IPlusHooks` (with a default implementation).
2. Add a dispatcher method to `PlusHooks`.
3. Add an empty override in each `Plus*Model` that implements `IPlusHooks`.
4. Raise it from the relevant Harmony patch in `HookPatches/` at the correct lifecycle point.

---

See [API_INDEX.md](API_INDEX.md) for the full list of provided types, and [CODE_CONVENTIONS.md](CODE_CONVENTIONS.md) for conventions.
