# Charge Relic

[English](ChargeRelic.md) · [中文](ChargeRelic.zh-CN.md)

`PlusChargeRelic` (`Base/Abstract/PlusChargeRelic.cs`) is UltraLib's **abstract base class for charge-based relics**.
It gives a relic two values — **current charge** / **max charge**: charge regenerates over time, and once fully charged the relic can trigger an effect and consume its charge.

## Design

- The relic has `NowCharge` (current) and `TotalCharge` (max).
- Charge regenerates: by default it **auto-gains +1 on every room enter** (can be disabled for custom logic).
- When fully charged (`NowCharge >= TotalCharge`), trigger the effect and **spend charge**.
- **The trigger method is not hard-coded**: right-click, automatic, turn-start, etc. — **the creator decides** (see below).
- The effect logic is written in `MainEffect` and executed via `DoChargeRelicEffect()`, so the "repeat times" hooks wrap your effect correctly.

## Quick Start

Inherit `PlusChargeRelic` and write two things: the max charge value, and the effect in `MainEffect`.

```csharp
public sealed class ExampleChargeRelic : PlusChargeRelic
{
    public ExampleChargeRelic()
    {
        TotalCharge = 5; // needs 5 charge to trigger
    }

    // ① Trigger wiring (example: right-click; can be automatic/turn-start etc., up to you)
    public override async Task Plus_RelicRightClick(RelicModel relicModel, NRelicInventoryHolder? holder)
    {
        if (!IsFullyCharged) return;

        await DoChargeRelicEffect(); // runs MainEffect (with repeat-times hooks)
        SpendCharge();               // consume charge
    }

    // ② The actual effect when triggered
    public override async Task MainEffect()
    {
        await CreatureHelper.Damage(
            new BlockingPlayerChoiceContext(),
            Owner.Creature.CombatState.HittableEnemies.First(),
            20m, ValueProp.None, Owner.Creature, null, null);
    }
}
```

## Core Members

| Member | Description |
|--------|-------------|
| `NowCharge` | Current charge (`[SavedProperty]`; clamps ≥ 0 and syncs `RelicStatus`). |
| `TotalCharge` | Max charge (`[SavedProperty]`; value needed to be full). |
| `IsFullyCharged` | Whether fully charged (`protected`): `NowCharge >= TotalCharge`. |
| `AutoCharge` | Auto-gain charge on room enter (`virtual`, default `true`). |
| `UseChargeBarDisplay` | Use the charge progress-bar UI (`virtual`, default `true`; `false` falls back to the number display). |
| `GainCharge(amount = 1)` | Gain charge (runs through the charge-value hooks). |
| `SetCharge(now, total)` / `SetCharge(amount)` | Set charge (the latter fills it completely). |
| `SpendCharge()` | Spend charge (amount corrected by `Plus_ModifyChargeSpend`; default drains all). |
| `DoChargeRelicEffect()` | Effect entry point: resolve repeat times → global before/after hooks → loop `MainEffect`. |
| `MainEffect()` | **Core effect entry** (`virtual`), overridden by subclasses. |

## Default Charge Regeneration

When `AutoCharge == true`, the base overrides `AfterRoomEntered` to auto-gain +1 charge per room.

```csharp
public override Task AfterRoomEntered(AbstractRoom room)
{
    NowCharge = _nowCharge;
    if (AutoCharge)
        NowChargeUpgrade(); // +1 charge
    return Task.CompletedTask;
}
```

## Custom Charge Regeneration

Override `AutoCharge` to `false`, then call `GainCharge(amount)` manually wherever it fits (combat hooks, turn-start hooks, etc.):

```csharp
public sealed class CustomChargeRelic : PlusChargeRelic
{
    public CustomChargeRelic() { TotalCharge = 3; }

    public override bool AutoCharge => false; // disable room-enter +1

    public override Task AfterCombatEnd(CombatRoom room)
    {
        GainCharge(2); // +2 charge per combat end
        return Task.CompletedTask;
    }

    public override async Task MainEffect() { /* your effect */ }
}
```

## Trigger Method (not hard-coded)

`PlusChargeRelic` does **not** bind a trigger method. The creator calls, at any trigger point (right-click hook, automatic hook, turn start, etc.):
check `IsFullyCharged` → call `DoChargeRelicEffect()` → call `SpendCharge()`.
As long as the effect lives in `MainEffect` and runs through `DoChargeRelicEffect`, the "repeat times" hooks
(`Plus_ModifyChargeRepeatTimes` family) automatically wrap and repeat your effect correctly.

## Related Hooks

All charge hooks live on `IPlusHooks` (the `PlusRelicModel` base provides empty implementations; override only what you need):

| Kind | Hook | When |
|------|------|------|
| Modifier | `Plus_ModifyMaxCharge` / `...Multiplicative` / `...Addictive` | Correct max charge |
| Modifier | `Plus_ModifyChargeUpgrade` / `...Multiplicative` / `...Addictive` | Correct charge gained per tick |
| Modifier | `Plus_ModifyChargeSpend` / `...Multiplicative` / `...Addictive` | Correct charge spent per trigger |
| Modifier | `Plus_ModifyChargeRepeatTimes` / `...Multiplicative` / `...Addictive` | Correct repeat times |
| Event | `Plus_OnChargeFullyCharged` | Charge reached max |
| Event | `Plus_OnChargeNoLongerFullyCharged` | Charge no longer max |
| Event | `Plus_OnChargeChanged(old, new, ...)` | Charge value changed |
| Event | `Plus_AfterChargeGain` / `Plus_AfterChargeSpend` | After gaining / spending charge |
| Event | `Plus_BeforeChargeEffected` / `Plus_AfterChargeEffected` | Before / after a single effect run |
| Event | `Plus_BeforeChargeTotallyEffected` / `Plus_AfterChargeTotallyEffected` | Before / after the whole effect flow |

Modifier composition: `Modify((amount + Addictive) × Multiplicative)`. See [Hook System](Hook.md).

## Other Features

- **Charge-bar UI**: with `UseChargeBarDisplay = true`, `ChargeRelicUiPatch` renders a charge progress bar under the relic icon (green until full, recolor on overcharge).
- **Right-click sync**: right-click goes through `RelicRightClickAction` (GameAction) + `RelicRightClickSyncNet` for multiplayer consistency.
- **Persistence**: `NowCharge` / `TotalCharge` are `[SavedProperty]` and save/load with the run.

## See Also

- Full API map: [API_INDEX.md](API_INDEX.md)
- Code conventions: [CODE_CONVENTIONS.md](CODE_CONVENTIONS.md)
