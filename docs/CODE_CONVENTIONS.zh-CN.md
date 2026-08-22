# UltraLib 代码规范

[English](CODE_CONVENTIONS.md) · [中文](CODE_CONVENTIONS.zh-CN.md)

本文档是为 **UltraLib** 贡献代码、以及编写依赖本库的模组时的风格与架构准则。它维护在仓库的 `docs/CODE_CONVENTIONS.md`；你也可以把本页内容粘贴到项目的 Wiki 中。

以下规范是从现有代码中归纳出来的，目的是让新代码保持一致。

---

## Wiki / 文档链接规范（link conventions）

这些规范在编写在线文档和 Wiki 页面时适用（同样适用于 [DEVELOPMENT_WORKFLOW.zh-CN.md](DEVELOPMENT_WORKFLOW.zh-CN.md) 与 API 指南）：

- 每个页面顶部的**语言切换**用 `[English](页面) · [中文](页面.zh-CN)` 格式。
- **导航链接**（侧边栏、首页索引、交叉引用）在同一行用 `/` 配对中英两种语言：
  `- [Utils Helper](Utils) / [工具辅助类](Utils.zh-CN)`
- 在 **GitHub Wiki** 中，链接指向 **Wiki 页面名**（不加 `.md` 后缀，空格用连字符代替，如 `Code-Conventions`、`Hook-System`）。在**仓库 `docs/`** 中，链接保留 `.md` 后缀，因为它们指向仓库文件。

---

## 1. 工程布局与命名空间

- **命名空间与文件夹路径一一对应。** `Base/Utils/CardHelper.cs` 中的类型位于 `namespace UltraLib.Base.Utils;`。使用**文件级命名空间**（单独的 `namespace UltraLib.X;` 行，不加花括号）。
- 根命名空间始终是 `UltraLib`。
- 目录职责划分：
  - `UltraLibCode/` — 仅存放模组入口（`MainFile.cs`）。这里不放任何业务逻辑。
  - `Base/` — 可复用库（抽象模型、辅助、标签、补丁、脚本、单例、多人）。
  - `Hook/` — 钩子契约（`IPlusHooks`）、分发器（`PlusHooks`），以及 `HookPatches/`（触发钩子的 Harmony 补丁）。
  - `Variables/` — 自定义动态变量及 `VariablePatches/`。
  - `Net/` — 网络/同步。`GameActions/` — 自定义动作。`HoverTip/` — 悬浮提示。`Test/` — 示例卡牌。
- **每个文件一个主公共类型**，以文件名命名。

## 2. 命名

- **类型、方法、属性、常量：** `PascalCase`。
- **私有字段：** `_camelCase`（下划线开头），例如 `_internalData`。
- **局部变量 / 参数：** `camelCase`。
- **本库提供的自定义“扩展”基类**使用 **`Plus` 前缀**（如 `PlusRelicModel`、`PlusPowerModel`、`PlusChargeRelic`）。新增的基类模型 / 辅助类应保持该前缀。
- **钩子事件方法**使用 `Plus_<EventName>` 命名（`Plus_AfterRelicObtain`、`Plus_BeforeOrbEvoke`，…）。
- **返回布尔值 / 能力类成员**在合适时读起来像提问（`IsX`、`CanDoY`）。

## 3. 语言特性与风格

- `ImplicitUsings` **已启用**，`Nullable` **已启用**（重视告警——可为空的值要标注 `?`）。
- 集合写法：使用**集合表达式**（`[]`、`[a, b]`），如同现有 `HashSet` 初始化中的写法。
- 当类型不明显时避免用 `var`；在公共 API 边界处优先使用显式类型。
- 单行访问器/返回值使用**表达式体成员**（expression-bodied members）。
- 保持文件聚焦；把逻辑抽取到 `Base/Utils/*Helper.cs`，而不是堆进模型里。
- 与游戏的模型 API 保持一致使用显式 `Task` 异步；无操作的异步默认返回 `Task.CompletedTask`。

## 4. 钩子系统

钩子系统是核心扩展点。在编写新钩子前先理解这些部分：

- **`IPlusHooks`**（`Hook/IPlusHooks.cs`）声明契约。使用**默认接口实现**，这样实现者不必实现每个成员。
- **`PlusHooks`**（`Hook/PlusHooks.cs`）是静态分发器，监听者通过它订阅。它从当前 run/combat 状态收集钩子监听者，并应用以下四种模式之一：
  - `Dispatch` — 按顺序触发异步事件钩子（例如 `Plus_AfterRelicObtain`）。
  - `Pipeline` — 修改一个值，将结果依次传递给每个监听者（“set/replace”型修改器）。
  - `Product` — 将各修改器相乘（`…Multiplicative` 变体，恒等元 `1m`）。
  - `Sum` — 累加各加法修改器（`…Addictive` 变体，恒等元 `0m`）。

  > 修改器约定：对于任何可修改的值 `X`，暴露三个钩子——`Plus_ModifyX`（pipeline）、`Plus_ModifyXMultiplicative`（product）、`Plus_ModifyXAddictive`（sum），以便任意组合都能复合。
- **具体内容模型**（如 `PlusRelicModel`）实现 `IPlusHooks` 并提供空默认覆写，因此子类只需覆写自己需要的部分。
- 新增一个钩子时：在 `IPlusHooks` 中添加该成员（带默认实现），在 `PlusHooks` 中添加分发方法，在每个实现了它的 `Plus*Model` 中添加空覆写，并在正确的生命周期点从相关的 `HookPatches/*` Harmony 补丁中触发它。

## 5. Harmony 补丁

- 将补丁放在 `Hook/HookPatches/`（属于相应子系统时放在 `Base/Patches/`、`Variables/VariablePatches/`）。
- 补丁类是**标注了 `[HarmonyPatch]` 的静态类**，由 `MainFile.Initialize()` 通过 `PatchAll` 对程序集自动应用。
- 失败的补丁**绝不能**让模组崩溃。对可能存在风险的补丁应用/方法体做包裹处理，使失败时只记录警告（参见 `MainFile.Initialize`）。
- **要考虑程序集加载时机。** `PatchAll` 只能看到它运行那一刻已加载的程序集。UltraLib 先于内容模组加载，因此任何动态扫描程序集的补丁（例如用 `TargetMethods()` 遍历 `AppDomain.CurrentDomain.GetAssemblies()` 查找某游戏类型的子类）都会漏掉后加载模组中的类型——那些钩子会永远静默不触发。若补丁必须发现模组自定义类型：
  - 在 `PatchAll` **之前**订阅 `AppDomain.CurrentDomain.AssemblyLoad`（参见 `Hook/HookPatches/OrbHooksPatches.cs` 的 `LateOrbPatchHelper`），并在每个程序集加载时重新执行发现逻辑。
  - 用 `HashSet<MethodBase>` 记录已打过补丁的方法，初始 `TargetMethods()` 扫描与延迟路径共用同一集合，并且**仅在 `Add` 返回 true 时才 `yield`/打补丁**——避免两条路径重叠时对同一方法重复打补丁。
  - **不要**扫描 `System` / `mscorlib` / `Steamworks` / `Godot` / `Unity` 程序集。
  - 对 `GetTypes()` 要防护 `ReflectionTypeLoadException`（用 `e.Types.Where(t => t != null)`）。
  - 注意：目标是固定游戏类型的补丁（如 `[HarmonyPatch(typeof(CardModel), ...)]`）不受影响，无需延迟补丁机制。
- 转发到钩子系统的补丁方法应调用对应的 `Plus_Trigger…` / `Plus_…` 分发器。

## 6. 日志与错误处理

- **使用 Godot 原生日志** —— `GD.Print` / `GD.PrintErr` / `GD.PrintPush` 进行诊断。这会直接写入 Godot 控制台，并在游戏内可见、便于快速审查（例如 `GD.PrintErr` 会标记一条 "ERR"，在游戏 UI 里能直接看到）。**不要**在发布的代码中使用 `Console.WriteLine`。
- **每条日志都必须以 `[UltraLib]` 标签开头**，以便过滤和归属到本模组，例如 `GD.PrintErr($"[UltraLib] ...")`。
- **日志内容必须中英双语**，让同一行日志对中英文使用者都可读，例如：
  ```csharp
  GD.PrintErr($"[UltraLib] [{cardModel.Id}] 渲染出空白，请检查资源路径 / render produced blank image, check resource path");
  GD.Print($"[UltraLib] [{cardModel.Id}] 保存成功 / save succeeded");
  ```
- 优先使用安全分发：可能抛异常的钩子代码应被捕获并记录日志，而不是让其传播并破坏一个游戏动作。
- 用简短的 `//` 注释标注非显而易见的决策；公共 API 含义依靠 XML 文档注释表达，而不是逐行叙述语句。

## 7. 文档注释

- **所有公共 API 成员**都要有 `/// <summary>` XML 文档注释。现有代码注释为中文；编辑既有文件时保持同一种语言，并在每个文件内保持语言一致。
- **新增功能（新的公共类型 / 方法 / 钩子 / 辅助类）必须带双语的 `<summary>`** 说明其用途和用法。标准双语布局为**英文放在 `<summary>`，简体中文放在紧邻的 `<remarks>`**，例如：
  ```csharp
  /// <summary>
  /// Generate an image of a card and save it to the specified path.
  /// </summary>
  /// <remarks>
  /// 生成一个卡牌的图像并保存到指定路径。
  /// </remarks>
  public static async Task<Error> RenderCardToImage(CardModel cardModel, string savePath = "");
  ```
  这样能直接喂给双语的 Wiki/API 文档与 CHM 参考，无需二次翻译，保持一致。
- 在能增加清晰度的地方使用 `<para>`、`<list type="bullet">`、`<see cref="…"/>`、`<c>…</c>` 等标签（参见 `MainFile.cs`）。
- 在成员文档中说明任何非显而易见的约束、生命周期要求或默认行为。

## 8. 本地化

- 不要在代码中硬编码面向用户的字符串。在 `UltraLib/localization/{eng,zhs}/` 的对应文件中添加键（`cards.json`、`powers.json`、`card_keywords.json`、`static_hover_tips.json`）。
- 保持 `eng` 与 `zhs` 键集合同步。
- `UltraLib/localization/**/*.json` 文件已注册为分析器输入（见 `UltraLib.csproj` 的 `AdditionalFiles`），因此请保持它们是有效的 JSON，并遵循既有键结构。

## 9. 版本与清单

- 保持 `UltraLib.json` 同步：发布时提升 `version`；新增/更新 `dependencies`（目前要求 `BaseLib >= 3.3.0`）。
- 保持 `MainFile.cs` 中的 `ModId` 常量与清单 `id`（`UltraLib`）一致——它用于 Harmony 实例与日志前缀。

## 10. Pull Request 检查清单

- [ ] 命名空间 = 文件夹路径；每文件一个公共类型
- [ ] 新基类模型/辅助使用 `Plus` 前缀；私有字段使用 `_camelCase`
- [ ] 公共 API 有 XML 文档注释（与文件语言一致）
- [ ] 新修改器遵循 `Pipeline` / `Multiplicative` / `Addictive` 三件套约定
- [ ] 新钩子已接入 `IPlusHooks`、`PlusHooks` 和对应的 `*Model`
- [ ] 无 `Console` 日志；使用 `MainFile.Logger` / `Log.*`
- [ ] 没有硬编码新的面向用户字符串——同时为 `eng` 和 `zhs` 添加本地化键
- [ ] 构建通过；如有需要已提升 `UltraLib.json` 版本
