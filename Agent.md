# Agent 交接文档（UltraLib）

> 本文档写给**下一个接手本项目的 Agent 会话**，用于快速恢复上下文。
> 维护者：TwodogsCN（项目作者）。更新时间：2026-08。

## 1. 项目是什么

**UltraLib** 是一个基于 [BaseLib-StS2](https://github.com/Alchyr/BaseLib-StS2/releases) 的 *Slay the Spire 2*（StS2）**基础库模组**。

- 仓库：https://github.com/TwodogsCN/Sts2-UltraLib
- Wiki：https://github.com/TwodogsCN/Sts2-UltraLib/wiki
- 定位：作者把自己的模组拆分为 **功能实现（基础库）** 与 **游戏内容实现（具体模组）**。别人或其他模组可以直接依赖本基础库拿到作者写好的功能（钩子、工具类、抽象模型、补丁等）。
- 技术栈：Godot 4.5.1 **Mono** + .NET 9 + C#（file-scoped namespace）+ Harmony 补丁 + BaseLib ≥ 3.3.0。
- 目标受众：模组制作者与贡献者；文档必须**双语（EN + 简体中文）**。

## 2. 当前状态（重要！先读这里）

- **Issue #1（代码规范性自审查）**：✅ **代码部分已完成**，由 **PR #3** 合入 `main`。全部核心源码目录已按规范双语化（`Base/`、`Hook/`、`GameActions/`、`HoverTip/`、`Variables/`、`UltraLibCode/`）。
- **Issue #2（目前功能的 CHM 维护）**：🔄 **进行中**。WinCHM 工程已搭建完成（`tools/chm-win/`），PR #4 已创建且 OPEN，**等待 review/合并**。剩余项：编译出的 `.chm` 直接输出到 `tools/chm-win/UltraLib.chm` 并随发布归档分发（发布时执行；该文件已从 `.gitignore` 移除，作为发布产物入库）。
- **充能遗物（PlusChargeRelic）专项文档已完成**（2026-08）：新增「新增功能 → 充能遗物」目录（3 个 CHM 子页）+ docs/ChargeRelic.md 双语 Wiki 文档；同时为「新增功能」分组增加专项介绍页 `data/new-features/new-features.htm`；Utils 总表下新增 17 个工具类方法级子页（`data/new-utils/*-helper.htm`）；新增「动态变量」（EmpowerVar/ReturnVar 各一页）与「卡牌关键词」（6 个词条各一页）子目录。
- **PR #4**：`docs(docs): 构建并归档可分发 UltraLib.chm (Closes #2)` —— 分支 `feat/chm-docs-maintenance`，OPEN 待合并。
- 本地工作分支：`feat/chm-docs-maintenance`（与 PR #4 对应）。
- 主分支 `main` 是干净的，本地已与 `origin/main` 同步。

## 3. 文档体系：两套独立载体（极易混淆，务必记住）

UltraLib 维护 **两套不同受众、不同内容、不同维护方式的文档**，**并非**由同一份源生成：

| 载体 | 位置 | 内容/结构 | 维护方式 |
|------|------|-----------|----------|
| **GitHub Wiki**（在线、双语） | 仓库 `docs/`（Markdown）→ 镜像到 Wiki（独立 wiki git 仓库） | README / API 索引 / 代码规范 / 开发流程 / Hook 系统 / Utils / 充能遗物 | 编辑 `docs/` 中的 Markdown，同步到 Wiki |
| **CHM 文档**（离线） | `tools/chm-win/`（WinCHM 工程 `UltraLibHelper.wcp`，编译产物直接输出 `tools/chm-win/UltraLib.chm`） | **独立结构**：前言 / 检查更新 / 简单认识C# / 更新日志 / 原版Hook列表 / 入门模组开发教学 / 新增功能（专项页、新Hooks表、新Utils表→17 个工具类子页、充能遗物→3 子页、动态变量→2 词条、卡牌关键词→6 词条） | 用 **WinCHM** 打开 `.wcp` 编辑/编译，产出 `tools/chm-win/UltraLib.chm` |

- 新功能需要时**同步进 CHM**（开发流程 §10）。
- CHM 页面规则（踩过的坑，务必遵守）：
  - `htm` 文件名与文件夹必须**纯 ASCII**（英文、无 CJK/特殊字符）——中文或 `C#` 文件名会导致 WinCHM 显示异常。
  - CSS 引用用 `../../code-style.css`（页面位于 `data/<dir>/`，上溯两级到工程根）。
  - 页面编码 **UTF-8 BOM + CRLF**；`.wcp` 的 URL 分隔符用 `\`。
  - 样式复用 `code-style.css` + `code-copy.js`（来自作者 Sls2HelperCN 的风格：info-box / code-box / 复制按钮 / 深色代码框）。
  - `info-content` 是白字，只能在深色 `info-box` 内使用；普通段落用 `.plain`（深色文字）。
- 本地 WinCHM 工程参考：`E:\Slay2Moder\Sts2ModderTwodogs说明书.wcp`（作者的 Sls2HelperCN 工程，风格来源）。

## 4. 代码规范要点（详见 docs/CODE_CONVENTIONS.md）

- **日志**：用 Godot 原生 `GD.Print` / `GD.PrintErr` / `GD.PrintPush`（游戏内可见可审查），**禁用** `Console.WriteLine` 和 `MainFile.Logger`/`Log.*`。
  - 每条日志必须以 `[UltraLib]` 标签开头。
  - 日志内容必须**中英双语**（EN + 中文），例：`GD.PrintErr($"[UltraLib] [...] 渲染出空白 / render produced blank image")`。
- **XML 文档注释**：所有公共 API 必须有 `/// <summary>`；**新增功能必须带双语 `<summary>`**——规范布局：英文放 `<summary>`，简体中文放紧邻的 `<remarks>`。
- **命名**：`Plus` 前缀用于基础模型/辅助类；私有字段 `_camelCase`；公共成员 `PascalCase`；命名空间 = 文件夹路径；每文件一个公共类型（file-scoped namespace）。
- **术语**：Orb = **充能球**（不要写"宝珠"）；Relic = 遗物；Power = 能力。
- **修改器三件套**：`Pipeline` / `Multiplicative` / `Addictive` 组合约定，公式 `Modify((amount + Addictive) × Multiplicative)`。
- **钩子**：新钩子接入 `IPlusHooks`（契约）、`PlusHooks`（分发器）、对应 `*Model`（触发点）；Patch 注释要写明"到底在干嘛"。
- 本地化：用户可见字符串不得硬编码，键进 `UltraLib/localization/{eng,zhs}/` 且两语言同步。

## 5. 开发流程（强约束，防止瞎交 PR）

一切改动走 **Issue → 分支 → 提交 → PR → Review → 合并** 闭环（详见 docs/DEVELOPMENT_WORKFLOW.md）：

- 每个需求/功能/Bug 先建 **Issue**（标题 `[Feature]`/`[Bug]`/`[Doc]`/`[Refactor]`/`[Test]`/`[Infra]` + 一句话；Body 用模板：背景/目标、需求清单、验收口径、关联）。
- 分支一律 `feat/<slug>`；**禁止直接在 main 上开发或 push main**。
- Commit 格式：`<type>(<scope>): <摘要> (Issue #N: 要点)`，type ∈ feat/fix/refactor/docs/test/ci/chore。
- PR：一个 PR 一个主题；描述用套餐模板（变更内容/新增/修改/修复/验收/验证/关联）；用触发词关联 Issue（`Closes #N` 自动关闭、`Part of #N` 仅关联）。
- **Review 强制**：至少一位 Reviewer 批准才能合并；作者不合并自己的 PR（唯一维护者除外）；未关联 Issue、构建失败或仓促的 PR 直接关闭不予审查。
- PR 检查清单（§10.5）：docs 更新、Wiki 镜像、CHM（新功能）更新、双语 summary、双语文本。

## 6. 常用命令与环境

- `gh` CLI 已登录（TwodogsCN），可建 Issue/PR：`gh issue create/edit`、`gh pr create/edit`、`gh pr merge`。
- 构建：`dotnet build`（当前有 0 错误；存在若干**既有业务 nullable 警告**，如 GoldPatch/OrbHooksPatches/RoseVars 等，非注释引入，修需业务判断——**不知道效果不要猜**）。
- CHM 编译：用户在 **WinCHM** 打开 `tools/chm-win/UltraLibHelper.wcp` 编译，产物直接输出到 `tools/chm-win/UltraLib.chm`（`.wcp` 的 `CompiledFile=UltraLib.chm` + 空 `RootDir`）；**该文件即发布产物**，提交入库（检查更新下载链接指向 `raw/main/tools/chm-win/UltraLib.chm`，无 dist）。
- **发布版本同步（每次发布必做，详见 docs/DEVELOPMENT_WORKFLOW.md §10.4.1）**：CHM「检查更新」页依赖 4 个版本来源保持完全一致——`version.txt`、`version.js`（JSONP：`window.ULTRALIB_LATEST = "x.y.z";`）、`UltraLib.json` 的 `"version"`、`tools/chm-win/data/check-update/check-update.htm` 的 `CONFIG.current` 与页面显示版本。漏改任一，检查页会误报。
- Wiki 镜像：Wiki 独立 git 仓库（`https://github.com/TwodogsCN/Sts2-UltraLib.wiki.git`），本地克隆在 `C:\Users\Administrator\AppData\Local\Temp\sts2wiki`（**临时目录，重启可能丢失**，需要时重新 clone 或用 `gh` 处理）。

## 7. 给下一个 Agent 的待办/提醒

- [ ] PR #4 待 review/合并（合并后 Issue #2 自动关闭）。
- [ ] 发布时：用 WinCHM 编译 `UltraLibHelper.wcp` → 产物 `tools/chm-win/UltraLib.chm`（即发布产物，提交入库）并随 Release 归档（Issue #2 剩余项；注意本次已新增充能遗物/Utils 子页/新增功能专项页/动态变量/卡牌关键词，需重新编译才可见）。
- [ ] **发布新版本时同步 4 处版本号**：`version.txt`、`version.js`、`UltraLib.json`、`check-update.htm` 的 `CONFIG.current`/显示版本（漏改会致检查更新误报，规范见 docs/DEVELOPMENT_WORKFLOW.md §10.4.1）。
- [ ] 代码库中仍有既有 nullable 警告（GoldPatch、OrbHooksPatches、RandomPositionFixPatch、RelicObtainPatch、ChargeRelicUiPatch、RoseVars 等），修复需要业务判断，先问维护者。
- [ ] CHM 的"入门模组开发教学"为骨架页，待后续填充（6 个章节）。
- [ ] 新增功能后记得：双语 summary（代码）→ docs/（Wiki 源）→ Wiki 镜像 → CHM 页面（如适用）。

## 8. 关键历史（避免重复踩坑）

- 曾用 GitHub MCP 工具建 Issue 报编码错误（`value of 20320` = "字"）——改用 `gh` CLI 解决（gh 原生支持中文）。
- `tools/chm/`（Markdown→CHM 自动管线）已整体删除，只保留 `tools/chm-win/`（WinCHM 方案）。
- Orb 术语曾误译为"宝珠"，已全端（代码注释、日志、docs、Wiki、CHM）统一为"充能球"。
- CHM 页面中文文件名曾导致 WinCHM 显示异常，已全部改为 ASCII 名。
