# UltraLib

A **base / utility library mod** for *Slay the Spire 2* (StS2), built on top of [BaseLib](https://github.com/Alchyr/BaseLib-StS2). It provides reusable abstract models, a unified event/hook system, helper utilities, and localization scaffolding for other mods that depend on it.

> UltraLib itself is a *dependency mod*, not a content mod: it adds the reusable framework that other mods consume.

## Features

- **Abstract base models** for common content types:
  - `PlusRelicModel` — relic model with levels, item pools and tags
  - `PlusPowerModel` / `PlusSingletonModel` — power and singleton model bases
  - `PlusChargeRelic` — charge-based relic support
- **Unified hook system** — the `IPlusHooks` interface (with default implementations) and the `PlusHooks` static dispatcher. Modifiers use composable `Pipeline` / `Product` / `Sum` semantics, and event hooks dispatch in a safe, ordered way. Covers relics, powers, cards, orbs, gold, charge, rooms and more.
- **Broad helper library** under `Base/Utils` — cards, powers, relics, orbs, discover, rewards, hand UI, localization, hover tips, dynamic variables and more.
- **Harmony patch infrastructure** — `[HarmonyPatch]` classes are auto-scanned on load; a failed patch logs a warning instead of crashing the mod.
- **Multiplayer support** — `Net/` and `Base/Multiplayer` helpers for synced actions.
- **Localization scaffolding** — structured `eng` / `zhs` JSON key files wired into the mod analyzer.

## Requirements

| Requirement | Notes |
|-------------|-------|
| *Slay the Spire 2* | Steam, with mod support enabled |
| [BaseLib](https://github.com/Alchyr/BaseLib-StS2/releases) mod | The game-side dependency; manifest requires `BaseLib >= 3.3.0` |
| Godot 4.5.1 **Mono** | Exact version matters — **the game won't load a `.pck` exported by a newer Godot** |
| .NET 9 SDK | for the C# code (`net9.0`) |

## Getting started (building the mod)

1. Install *Slay the Spire 2*, [BaseLib](https://github.com/Alchyr/BaseLib-StS2/releases), Godot 4.5.1 Mono, and the .NET 9 SDK.
2. Open `UltraLib.sln` in Rider or Visual Studio.
3. If your Steam library or Godot install lives somewhere else, adjust these in [`UltraLib.csproj`](UltraLib.csproj):
   - `<GodotPath>` — path to your Godot 4.5.1 Mono executable
   - `<SteamLibraryPath>` — your Steam `steamapps` folder (StS2 is auto-detected where possible)
4. Build. The csproj automatically:
   - copies the built `.dll` and `UltraLib.json` manifest into StS2's `mods/UltraLib/`
   - exports the Godot `.pck` for that build into the same folder

   ```
   dotnet build
   ```
   (or the Build action in your IDE).

5. Launch the game; UltraLib loads like any other mod.

## Using UltraLib from another mod

1. Add `"UltraLib"` to your mod's `dependencies` in your `mod.json` (e.g. `"dependencies": [{ "id": "UltraLib", "min_version": "0.1.0" }]`).
2. Reference the built `UltraLib.dll` from your project.
3. Consume the API — e.g. inherit `PlusRelicModel` and override the hook methods you need, or call the `Base/Utils` helpers directly.

See [docs/CODE_CONVENTIONS.md](docs/CODE_CONVENTIONS.md) for how the code is organised and the conventions to follow when contributing or writing mods against this library.

## Repository structure

```
UltraLib/
├─ UltraLibCode/            # Mod entry point (MainFile.cs: Harmony init + PatchAll)
├─ Base/                    # Core library
│  ├─ Abstract/             # Abstract base models (PlusRelicModel, PlusPowerModel, ...)
│  ├─ Exporter/             # Card export helpers
│  ├─ Label/                # Card keywords / tags
│  ├─ Multiplayer/          # Multiplayer commands
│  ├─ Patches/              # Harmony patches
│  ├─ Power/ Relic/ Scripts/ Singleton/
│  ├─ Utils/                # *Helper utility classes
│  └─ Scenes/               # Godot scenes
├─ Hook/                    # IPlusHooks + PlusHooks dispatcher + HookPatches/
├─ HoverTip/                # Hover tip support
├─ GameActions/             # Custom game actions
├─ Net/                     # Network / sync helpers
├─ Variables/               # Custom dynamic variables (+ VariablePatches/)
├─ Test/                    # Sample/test cards (TestCards.cs)
└─ UltraLib/                # Godot resources: localization/{eng,zhs} + mod_image.png
```

## Related links

- [BaseLib repo](https://github.com/Alchyr/BaseLib-StS2) · [BaseLib releases](https://github.com/Alchyr/BaseLib-StS2/releases) · [`Alchyr.Sts2.BaseLib` on NuGet](https://www.nuget.org/packages/Alchyr.Sts2.BaseLib)
- [StS2 modding Wiki](https://slay-the-spire.fandom.com/wiki/Slay_the_Spire_2_Wiki) (BaseLib page)

## License

Released under the [MIT License](LICENSE). You are free to use, modify and redistribute it, including in closed-source projects, provided the copyright notice is retained.
