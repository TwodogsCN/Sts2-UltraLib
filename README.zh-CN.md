# UltraLib

[English](README.md) · [中文](README.zh-CN.md)

**UltraLib** 是面向 *Slay the Spire 2*（StS2）的**基础 / 工具库模组**，构建于 [BaseLib](https://github.com/Alchyr/BaseLib-StS2) 之上。它为其它依赖它的模组提供可复用的抽象模型、统一的事件/钩子系统、工具方法和本地化脚手架。

> UltraLib 本身是**依赖型模组**（dependency mod），而非内容型模组：它提供的是供其它模组消费的可复用框架。

## 功能特性

- **通用内容类型的抽象基类模型**：
  - `PlusRelicModel` — 带等级、出现池与标签的遗物模型
  - `PlusPowerModel` / `PlusSingletonModel` — 能力与单例模型基类
  - `PlusChargeRelic` — 充能式遗物支持
- **统一的钩子系统** — `IPlusHooks` 接口（带默认实现）与 `PlusHooks` 静态分发器。修改器采用可组合的 `Pipeline` / `Product` / `Sum` 语义，事件钩子以安全、有序的方式分发。覆盖遗物、能力、卡牌、充能球、金币、充能、房间等场景。
- **丰富的工具库**（位于 `Base/Utils`）— 卡牌、能力、遗物、充能球、发现、奖励、手牌 UI、本地化、悬浮提示、动态变量等。
- **Harmony 补丁基础设施** — 加载时自动扫描 `[HarmonyPatch]` 类；单个补丁失败时记录警告而不会让模组崩溃。
- **多人模式支持** — `Net/` 与 `Base/Multiplayer` 提供同步动作辅助。
- **本地化脚手架** — 结构化的 `eng` / `zhs` JSON 键文件，并已接入模组分析器。

## 环境要求

| 要求 | 说明 |
|------|------|
| *Slay the Spire 2* | Steam 版，需开启模组支持 |
| [BaseLib](https://github.com/Alchyr/BaseLib-StS2/releases) 模组 | 游戏端依赖；清单要求 `BaseLib >= 3.3.0` |
| Godot 4.5.1 **Mono** | 版本必须精确匹配——**使用更新的 Godot 导出的 `.pck` 游戏无法加载** |
| .NET 9 SDK | 用于 C# 代码（`net9.0`） |

## 快速开始（构建模组）

1. 安装 *Slay the Spire 2*、[BaseLib](https://github.com/Alchyr/BaseLib-StS2/releases)、Godot 4.5.1 Mono 以及 .NET 9 SDK。
2. 用 Rider 或 Visual Studio 打开 `UltraLib.sln`。
3. 如果你的 Steam 库或 Godot 安装在别处，请在 [`UltraLib.csproj`](UltraLib.csproj) 中调整：
   - `<GodotPath>` — 你的 Godot 4.5.1 Mono 可执行文件路径
   - `<SteamLibraryPath>` — 你的 Steam `steamapps` 目录（StS2 会尽可能自动检测）
4. 构建。csproj 会自动：
   - 将构建好的 `.dll` 与 `UltraLib.json` 清单复制到 StS2 的 `mods/UltraLib/`
   - 将对应构建导出的 Godot `.pck` 导出到同一目录

   ```
   dotnet build
   ```
   （或在 IDE 中执行 Build）。

5. 启动游戏，UltraLib 会像其它模组一样被加载。

## 从其它模组使用 UltraLib

1. 在你的 `mod.json` 的 `dependencies` 中加入 `"UltraLib"`（例如 `"dependencies": [{ "id": "UltraLib", "min_version": "0.1.0" }]`）。
2. 在工程中引用构建出的 `UltraLib.dll`。
3. 消费其 API——例如继承 `PlusRelicModel` 并覆写所需的钩子方法，或直接调用 `Base/Utils` 中的辅助方法。

关于代码的组织方式，以及为本库贡献或编写依赖模组时需遵循的约定，请参阅 [docs/CODE_CONVENTIONS.md](docs/CODE_CONVENTIONS.md)。

## 仓库结构

```
UltraLib/
├─ UltraLibCode/            # 模组入口（MainFile.cs：Harmony 初始化 + PatchAll）
├─ Base/                    # 核心库
│  ├─ Abstract/             # 抽象基类模型（PlusRelicModel、PlusPowerModel ...）
│  ├─ Exporter/             # 卡牌导出辅助
│  ├─ Label/                # 卡牌关键词 / 标签
│  ├─ Multiplayer/          # 多人模式命令
│  ├─ Patches/              # Harmony 补丁
│  ├─ Power/ Relic/ Scripts/ Singleton/
│  ├─ Utils/                # *Helper 工具类
│  └─ Scenes/               # Godot 场景
├─ Hook/                    # IPlusHooks + PlusHooks 分发器 + HookPatches/
├─ HoverTip/                # 悬浮提示支持
├─ GameActions/             # 自定义游戏动作
├─ Net/                     # 网络 / 同步辅助
├─ Variables/               # 自定义动态变量（+ VariablePatches/）
├─ Test/                    # 示例 / 测试卡牌（TestCards.cs）
└─ UltraLib/                # Godot 资源：localization/{eng,zhs} + mod_image.png
```

## 相关链接

- [BaseLib 仓库](https://github.com/Alchyr/BaseLib-StS2) · [BaseLib 发布页](https://github.com/Alchyr/BaseLib-StS2/releases) · [NuGet 上的 `Alchyr.Sts2.BaseLib`](https://www.nuget.org/packages/Alchyr.Sts2.BaseLib)
- [StS2 模组 Wiki](https://slay-the-spire.fandom.com/wiki/Slay_the_Spire_2_Wiki)（BaseLib 页面）

## License / 许可证

Released under the [MIT License](LICENSE). You are free to use, modify and redistribute it, including in closed-source projects, provided the copyright notice is retained.

本项目采用 [MIT 许可证](LICENSE)。在保留版权声明的前提下，你可以自由使用、修改和再分发，包括用于闭源项目。
