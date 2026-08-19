# UltraLib Code Conventions

This is the style and architecture guideline for contributing to **UltraLib** and for mods that consume it. It is maintained in the repository at `docs/CODE_CONVENTIONS.md`; you can paste this page into the project's Wiki.

The conventions below are distilled from the existing codebase so new code stays consistent.

---

## 1. Project layout & namespaces

- **Namespaces mirror folder paths.** A type in `Base/Utils/CardHelper.cs` lives in `namespace UltraLib.Base.Utils;`. Use **file-scoped namespaces** (`namespace UltraLib.X;` on its own line, no braces).
- The root namespace is always `UltraLib`.
- Directory responsibilities:
  - `UltraLibCode/` — the mod entry point only (`MainFile.cs`). No logic here.
  - `Base/` — the reusable library (abstract models, helpers, labels, patches, scripts, singletons, multiplayer).
  - `Hook/` — the hook contract (`IPlusHooks`), the dispatcher (`PlusHooks`), and `HookPatches/` (Harmony patches that raise hooks).
  - `Variables/` — custom dynamic variables plus `VariablePatches/`.
  - `Net/` — network/sync.  `GameActions/` — custom actions.  `HoverTip/` — hover tips.  `Test/` — sample cards.
- **One primary public type per file**, named after the file.

## 2. Naming

- **Types, methods, properties, constants:** `PascalCase`.
- **Private fields:** `_camelCase` (leading underscore), e.g. `_internalData`.
- **Local variables / parameters:** `camelCase`.
- **Custom "extension" base classes** provided by this library use the **`Plus` prefix** (e.g. `PlusRelicModel`, `PlusPowerModel`, `PlusChargeRelic`). New base models / helpers should keep this prefix.
- **Hook event methods** use the `Plus_<EventName>` naming (`Plus_AfterRelicObtain`, `Plus_BeforeOrbEvoke`, …).
- **Boolean-returning / capability members** read as questions where appropriate (`IsX`, `CanDoY`).

## 3. Language features & style

- `ImplicitUsings` **enabled**, `Nullable` **enabled** (treat warnings as important — annotate `?` where a value can be null).
- Plugins: use **list/collection expressions** (`[]`, `[a, b]`) as already done in `HashSet` initializers.
- Avoid `var` when the type is not obvious; prefer explicit types at public API boundaries.
- Use **expression-bodied members** for single-line accessors/returns.
- Keep files focused; extract helpers into `Base/Utils/*Helper.cs` rather than piling logic into a model.
- Use explicit `Task`-based async consistent with the game's model APIs; return `Task.CompletedTask` for no-op async defaults.

## 4. The hook system

The hook system is the core extension point. Understand these pieces before writing new hooks:

- **`IPlusHooks`** (`Hook/IPlusHooks.cs`) declares the contract. Use **default interface implementations** so implementers are not forced to implement every member.
- **`PlusHooks`** (`Hook/PlusHooks.cs`) is the static dispatcher that listeners subscribe through. It gathers hook listeners from the current run/combat state and applies one of four patterns:
  - `Dispatch` — fire an async event hook in order (e.g. `Plus_AfterRelicObtain`).
  - `Pipeline` — modify a value, passing the result through each listener in turn (the "set/replace" modifier).
  - `Product` — multiply modifiers together (the `…Multiplicative` variant, identity `1m`).
  - `Sum` — sum additive modifiers (the `…Addictive` variant, identity `0m`).

  > Modifier convention: for any modifiable value `X`, expose three hooks — `Plus_ModifyX` (pipeline), `Plus_ModifyXMultiplicative` (product), `Plus_ModifyXAddictive` (sum) — so arbitrary combinations compose.
- **Concrete content models** (e.g. `PlusRelicModel`) implement `IPlusHooks` and provide empty default overrides, so subclasses only override what they need.
- When you add a hook: add the member to `IPlusHooks` (with a default), add the dispatcher method to `PlusHooks`, add an empty override to each `Plus*Model` that implements it, and raise it from the relevant `HookPatches/*` Harmony patch at the correct lifecycle point.

## 5. Harmony patches

- Place patches in `Hook/HookPatches/` (or `Base/Patches/`, `Variables/VariablePatches/` when they belong to that subsystem).
- Patch classes are **static classes annotated with `[HarmonyPatch]`** and are auto-applied by `MainFile.Initialize()` via `PatchAll` on the assembly.
- A failing patch must **never** crash the mod. Wrap risky patch application/bodies so failures log a warning (see `MainFile.Initialize`).
- Patch methods that forward into the hook system should call the matching `Plus_Trigger…` / `Plus_…` dispatcher.

## 6. Logging & error handling

- Use the library logger (`MainFile.Logger`) or `Log.*` for diagnostics. **Do not** use `Console.WriteLine` in shipped code.
- Prefer safe dispatch: hook code that can throw should be caught and logged (see `Plus_TriggerRelicRightClick`), not allowed to propagate and break a game action.
- Annotate intent with short `//` comments for non-obvious decisions; rely on XML doc comments for public API meaning instead of narrating statements.

## 7. Documentation comments

- **All public API members** get `/// <summary>` XML doc comments. The existing code comments are in Chinese; keep the same language when editing existing files, and choose one language and stay consistent within each file.
- Use `<para>`, `<list type="bullet">`, `<see cref="…"/>`, and `<c>…</c>` tags where they add clarity (see `MainFile.cs`).
- Document any non-obvious invariant, lifecycle requirement, or default behavior in the member docs.

## 8. Localization

- Do not hardcode user-facing strings in code. Add keys to the matching files under `UltraLib/localization/{eng,zhs}/` (`cards.json`, `powers.json`, `card_keywords.json`, `static_hover_tips.json`).
- Keep `eng` and `zhs` key sets in sync.
- The `UltraLib/localization/**/*.json` files are registered as analyzer input (see `UltraLib.csproj` `AdditionalFiles`), so keep them valid JSON and follow the existing key structure.

## 9. Versioning & manifest

- Keep `UltraLib.json` in sync: bump `version` for releases; add/update `dependencies` (currently requires `BaseLib >= 3.3.0`).
- Keep `ModId` constant in `MainFile.cs` equal to the manifest `id` (`UltraLib`) — it is used for the Harmony instance and log prefix.

## 10. Pull request checklist

- [ ] Namespace = folder path; one public type per file
- [ ] `Plus` prefix for new base models/helpers; `_camelCase` private fields
- [ ] Public API has XML doc comments (consistent language with the file)
- [ ] New modifiers follow the `Pipeline` / `Multiplicative` / `Addictive` trio convention
- [ ] New hooks wired into `IPlusHooks`, `PlusHooks`, and the relevant `*Model`
- [ ] No `Console` logging; uses `MainFile.Logger` / `Log.*`
- [ ] No new user-facing strings hardcoded — localization keys added to both `eng` and `zhs`
- [ ] Build passes; `UltraLib.json` version bumped if needed
