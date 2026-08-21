# UltraLib 开发流程规范

[English](DEVELOPMENT_WORKFLOW.md) · [中文](DEVELOPMENT_WORKFLOW.zh-CN.md)

本页定义 **UltraLib** 的贡献开发流程。它维护在仓库的 `docs/DEVELOPMENT_WORKFLOW.md`，并计划同步到项目的 Wiki。

---

## 1. 全局原则

一切改动走闭环：

**Issue → 分支 → 提交 → PR → Review → 合并**

全局原则：

- 每个需求（功能）/ Bug 都有一个 GitHub **Issue**。
- 每个 **PR** 都关联一个（或多个）Issue。
- 每次状态迁移（Issue ↔ PR ↔ 合并）都有留痕。
- **禁止绕过流程。** 不得直接在 `main` 上开发或 push 到 `main`；不得提交未关联 Issue 的 PR。未关联 / 仓促提交的 PR 一律直接关闭，不予审查。

## 2. Issue 规范

### 2.1 什么时候建 Issue

- 每个**独立可交付的功能点 / Bug / 需求**开一个 Issue。
- 一个 Issue 对应一个小而完整的业务动作（例如"新增 `PlusChargeRelic` 抽象基类""修复充能球被动钩子未分发"）。
- 批量 / 大块需求先拆成多个 Issue，避免超大 PR。

### 2.2 Title 规范

```
[标题前缀] 一句话说明
```

实际风格示例（动词开头、一句话、可带编号）：

- `[Feature] 新增充能遗物抽象基类 PlusChargeRelic`
- `[Bug] 充能球被动钩子异常未被捕获导致战斗流程中断`
- `[Doc] 钩子系统补充 Pipeline/Product/Sum 语义说明`
- `[Refactor] PlusHooks 分发器统一日志与异常捕获`

推荐前缀：`[Feature]` / `[Bug]` / `[Doc]` / `[Refactor]` / `[Test]` / `[Infra]`。

### 2.3 Body 规范（模板）

```
### 背景 / 目标
(为什么要做，要实现什么功能，举例说明)

### 需求清单
- [ ] 子任务 1
- [ ] 子任务 2

### 验收口径（尽量可测）

### 关联
- 依赖的 Issue / PR
```

## 3. 分支规范

### 3.1 命名

一律 `feat/<slug>`。slug 用短横线小写、含义清晰即可（项目实际风格，不强绑定 Issue 号，但建议关联，如 `feat/plus-charge-relic`）。

### 3.2 生命周期

- 分支从最新 `main` 拉出，尽早合并 `main`，避免长期分支冲突。
- 功能完成 → 推送 → 提 PR → review 通过 → 合并后**删除远端分支**。
- 未合并前保持小步提交、可读历史。

### 3.3 禁止

- 禁止直接在 `main` 上开发或直接 push 到 `main`。

## 4. 提交信息规范（Commit Message）

项目实际风格：**`<type>(<scope>): <摘要> (Issue #N: 要点)`**。它同时承担「给人类看 + 给 AI/回溯用」的双重作用，信息密度要高。

### 4.1 格式

```
<type>(<scope>): <一句话摘要> (Issue #N: 关键要点)
```

- **type**（必填）：`feat` / `fix` / `refactor` / `docs` / `test` / `ci` / `chore`
- **scope**（可选但推荐）：业务域，如 `core`（核心库 Base/）/ `hook`（钩子系统）/ `patch`（Harmony 补丁）/ `base` / `net`（多人/网络）/ `variables`（动态变量）/ `localization`（本地化）/ `cfg`（工程配置/清单）
- **摘要**：动宾结构，做了什么
- **Issue 引用 + 要点**：关联 Issue 号，并用冒号后列关键点（便于回溯）

### 4.2 原则

- **一条提交只做一件事**（与 §6 对应）。
- **信息密度高**：摘要里就把"做了啥 + 关联 Issue + 验收证据"写清楚，方便回溯与喂给 AI。
- 动词用祈使句：`feat(...): 新增...`、`fix(...): 修复...`、`test(...): 补用例...`。
- 涉及验收编号（Issue #N、A0x/B0x）时，在提交里带出，便于对验收矩阵。

## 5. PR 规范

### 5.1 前提

- 一个 PR **只解决一个主题**（通常一个或多个关联 Issue），小而可评审。

### 5.2 Title

```
feat(hook): 新增充能球被动钩子 Plus_BeforeOrbPassive (Issue #12)
```

同提交信息风格：`<type>(<scope>): <摘要> (Issue #N)`。

### 5.3 PR 描述模板（推荐）

套餐结构，便于 Reviewer 与 AI 快速理解：

```
## 变更内容
（一句话 + 覆盖的 Issue/验收编号）

### 新增
- 文件 / 能力（逐个）

### 修改
- 文件 / 行为变化（逐个）

### 修复（如有）
- Bug/缺陷

## 验收口径 / 与文档一致性
- 不崩溃容错 / 钩子分发 / 本地化同步如何满足
- 涉及哪些已有文档需同步（Wiki）

## 验证
- 跑了哪些检查：构建结果（如 `dotnet build` BUILD SUCCESS）、游戏内加载冒烟验证、`eng`/`zhs` 本地化一致性
- 本地 / CI 情况
```

### 5.4 关联 Issue（Development 面板）

在 PR 描述里用**触发词**关联 Issue，让 GitHub 自动建立 Development 关联：

- `Closes #12` → 合并后**自动关闭** Issue（完整实现该 Issue 时用）。
- `Fixes #xx` / `Resolves #xx` → 同上（修复类）。
- `Part of #xx` → 仅关联，**不自动关闭**（该 Issue 还有其他子任务时用）。

> 注意：GitHub 只认英文触发词（`closes` / `fixes` / `resolves` / `part of`），中文"关联"不生效。

### 5.5 Review 为强制性 —— 未经批准不得合并

- 一个 PR 只有在**至少一位 Reviewer 批准**、且**没有未解决的 requested changes** 之后才能合并。
- 除非作者是唯一的维护者且没有可用 Reviewer，否则作者**不得合并自己的 PR**。
- 构建/检查失败、存在未解决的评审讨论、或未关联 Issue 的 PR 一律被**阻塞**。
- 仓促、无关或未关联的"顺手 PR"**直接关闭、不予审查**——请先建 Issue 并遵循闭环流程。

---

## 10. Wiki 与文档维护规范

> 适用于你的改动以**新增功能**或**修改既有行为/内容**为结束的情况。

### 10.1 总则：文档与代码保持同步

- 当你**新增功能**或**修复/修改既有行为**时，在文档更新之前（若适用），该改动不算完成。
- UltraLib 维护**两个**面向不同受众的载体。它们**并非**由同一份源生成，必须**分别维护**：
  - **GitHub Wiki**（在线、双语）——主要的人类可读文档，在 `docs/`（Markdown）中维护并镜像到 Wiki。
  - **CHM 文档**（离线）——位于 `tools/chm-win/` 的 WinCHM 工程，拥有**独立的结构与内容**（前言 / 检查更新 / 简单认识C# / 更新日志 / 原版Hook列表 / 入门模组开发教学 / 新增功能-新Hooks表-新Utils表）。它是模组作者查阅的离线参考，用 WinCHM 编译为 `UltraLib Helper.chm`。
- **强制要求：** 每个新增的公共类型 / 方法 / 钩子 / 辅助类，其 XML 文档注释必须带**双语 `<summary>`**（一段英文描述 + 一段简体中文描述）。这是喂养双语 Wiki 与 CHM 的来源，因此无需再做单独的翻译步骤。参见[代码规范 §7](CODE_CONVENTIONS.md)。

### 10.2 什么情况需要更新文档

| 变更类型 | 更新 Wiki? | 更新 CHM? |
|----------|------------|-----------|
| 新增公共类型 / 方法 / 钩子 / 辅助类 | **是** | **是**（新功能） |
| 既有功能的行为变更 | **是（若面向用户）** | **是** |
| 仅内部重构、无可见变化 | 否 | 否 |
| 改变实现方式的 Bug 修复 | 是 | 是 |
| 纯文档变更（措辞、排印） | 是 | 是 |

- **新功能必须同步进 CHM** —— CHM 是模组作者查阅的离线参考，任何新增 API/功能都必须包含在内。

### 10.3 如何更新 Wiki

- Wiki 页面位于本仓库的 `docs/`；GitHub Wiki 与它们保持同步。
- 编辑 `docs/` 下对应的 Markdown 源文件，例如：
  - `docs/API_INDEX.md` / `.zh-CN.md` —— 在索引中补充新增的类型/辅助类。
  - `docs/Hook.md` / `docs/Utils.md` / `.zh-CN.md` —— 为新增的方法/钩子补充说明。
- 遵循[链接规范](CODE_CONVENTIONS.md)：双语、`[页面] / [页面]` 格式、Wiki 页面名不带 `.md` 后缀。
- 推送 `docs/` 的改动，并镜像到 GitHub Wiki，保持 Wiki 最新。

### 10.4 如何更新 CHM

- CHM 是 `tools/chm-win/` 下的 **WinCHM 工程**（`UltraLibHelper.wcp`）。其内容/结构与 Wiki 相互独立——当改动影响模组作者离线所需的任何内容（新钩子、新工具类、教学、更新日志）时才更新它。
- 新增功能后：
  1. 用 WinCHM 打开 `tools/chm-win/UltraLibHelper.wcp`。
  2. 按工程既有 HTML 风格（`code-style.css` + `code-copy.js`）新增/更新相关页面（例如 `新Hooks表`、`新Utils表`、教学页）。
  3. 所有 `htm` 文件名与文件夹必须是**纯 ASCII**（英文，无 CJK / 特殊字符）。
  4. 在 WinCHM 中重新编译，确认新页面出现在目录树中；随发布附上重新编译的 `.chm`。

### 10.4.1 发布产物输出与版本同步（每次发布必做）

CHM **直接编译到 `tools/chm-win/UltraLib.chm`**（`.wcp` 中 `CompiledFile=UltraLib.chm` 且 `RootDir` 为空，因此 WinCHM 输出在工程文件所在目录）。**这个编译产物就是发布产物**——提交进仓库（不要在 `.gitignore` 里重新忽略它），CHM 内的「检查更新」页直接从 `raw/main/tools/chm-win/UltraLib.chm` 下载。

每次发布新版本前，必须同步更新**四个版本来源**，CHM 内的「检查更新」页才能检测到新版本：

| # | 文件 | 改哪里 |
|---|------|--------|
| 1 | `version.txt` | 仓库根目录——纯文本版本号，检查页备用读取 |
| 2 | `version.js` | 仓库根目录——`window.ULTRALIB_LATEST = "x.y.z";`（JSONP，检查页实际加载它） |
| 3 | `UltraLib.json` | `"version": "x.y.z"`——模组自身版本 |
| 4 | `tools/chm-win/data/check-update/check-update.htm` | `CONFIG.current` 与页面可见的 `<span class="value">`——CHM 内本地版本 |

然后：
1. 在 WinCHM 中重新编译 CHM → 产物输出到 `tools/chm-win/UltraLib.chm`（即提交的发布产物）。
2. 提交并推送 `version.txt`、`version.js`、`UltraLib.json`、重新编译的 `tools/chm-win/UltraLib.chm` 及 CHM 源码改动。检查页从 `raw.githubusercontent.com/TwodogsCN/Sts2-UltraLib/main/version.js` 拉取版本、从 `raw/main/tools/chm-win/UltraLib.chm` 下载——两者都必须推到 `main` 分支后才会生效。

> 四个版本来源必须**保持一致**；漏掉任何一个都会导致检查页报告错误结果。

### 10.5 涉及代码改动的 PR 检查清单

- [ ] `docs/` 已为该改动更新（新 API/功能已文档化）。
- [ ] Wiki 已镜像，与 `docs/` 一致。
- [ ] 新功能已更新 CHM（WinCHM 工程 `tools/chm-win/`）——页面用英文/ASCII 文件名新增并在 WinCHM 中重新编译。
- [ ] **新增功能的所有新公共类型 / 方法 / 钩子 / 辅助类都带双语 XML `<summary>`**（EN + 中文），说明用途与用法。
- [ ] 适用的地方已做到双语（EN + 中文）。
