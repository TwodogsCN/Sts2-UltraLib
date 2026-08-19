# UltraLib CHM 文档构建 / Building the CHM documentation

[English](#english) · [中文](#中文)

UltraLib 同时维护**在线 Wiki**（双语 Markdown，位于 `.md` 文件）和**离线 CHM 文档**（Windows Compiled HTML Help）。两者**内容同源**：CHM 由同一组 Markdown 编译而来，方便模组作者在本地离线检索本库提供的功能/API。

---

## 中文

### 内容来源

CHM 由以下双语 Markdown 编译生成（`docs/` 与根目录 `README*`）：

| 源文件 | 说明 |
|--------|------|
| `README.md` / `README.zh-CN.md` | 项目总览与快速上手（根目录） |
| `docs/API_INDEX.md` / `.zh-CN.md` | **功能 / API 索引**（模组作者查找本库功能的入口） |
| `docs/CODE_CONVENTIONS.md` / `.zh-CN.md` | 代码规范 |
| `docs/DEVELOPMENT_WORKFLOW.md` / `.zh-CN.md` | 开发流程 |

> **保持内容一致**：改这些 `.md` 源文件后重新构建 CHM（以及同步 Wiki），不要在 `out/` 里手改 HTML。

### 构建步骤（Windows）

1. **安装 Node.js**（https://nodejs.org），确保 `node` 在 PATH 中。
2. **安装 HTML Help Workshop**（微软官方免费工具，含 `hhc.exe`）：
   - https://learn.microsoft.com/en-us/previous-versions/windows/desktop/htmlhelp/microsoft-html-help-workshop
   - 默认装到 `C:\Program Files (x86)\HTML Help Workshop\`，脚本会自动查找该路径。
3. 双击运行 `tools/chm/build-chm.bat`（或命令行执行）：
   ```bat
   tools\chm\build-chm.bat
   ```
4. 产物：`tools/chm/out/UltraLib.chm`。

若不想运行 bat，也可手动：
```bat
node tools\chm\build-chm.js --source docs --out tools\chm\out
"C:\Program Files (x86)\HTML Help Workshop\hhc.exe" tools\chm\out\UltraLib.hhp
```

### 输出说明

- `out/*.html` — 转换后的 HTML 页面（自带目录树、样式、页面间导航）。
- `out/UltraLib.hhc` — CHM 目录树（Table of Contents）。
- `out/UltraLib.hhp` — CHM 工程文件（由 hhc.exe 编译）。
- `out/UltraLib.chm` — **最终 CHM 文档**（可分发/随仓库发布）。

### 维护规则

- 新增文档页时，在 `tools/chm/build-chm.js` 的 `pages` 数组中登记（含英/中文两版）。
- 每次更新 `.md` 源后重新构建 CHM，保证 Wiki 与 CHM 一致。
- `out/` 为构建产物，已加入 `.gitignore`，不入库；发布时单独附加 CHM。

---

## English

### Content source

The CHM is compiled from the same bilingual Markdown (in `docs/` and root `README*`):

| Source | Description |
|--------|-------------|
| `README.md` / `README.zh-CN.md` | Project overview & quick start (repo root) |
| `docs/API_INDEX.md` / `.zh-CN.md` | **API / feature index** (entry point for mod authors) |
| `docs/CODE_CONVENTIONS.md` / `.zh-CN.md` | Code conventions |
| `docs/DEVELOPMENT_WORKFLOW.md` / `.zh-CN.md` | Development workflow |

> **Keep content in sync**: edit the `.md` sources and rebuild the CHM (and mirror the Wiki). Don't hand-edit the HTML in `out/`.

### Build steps (Windows)

1. **Install Node.js** (https://nodejs.org) and make sure `node` is on PATH.
2. **Install HTML Help Workshop** (free Microsoft tool providing `hhc.exe`):
   - https://learn.microsoft.com/en-us/previous-versions/windows/desktop/htmlhelp/microsoft-html-help-workshop
   - Default install path is `C:\Program Files (x86)\HTML Help Workshop\`; the script finds it automatically.
3. Run the batch script:
   ```bat
   tools\chm\build-chm.bat
   ```
4. Output: `tools/chm/out/UltraLib.chm`.

Or manually:
```bat
node tools\chm\build-chm.js --source docs --out tools\chm\out
"C:\Program Files (x86)\HTML Help Workshop\hhc.exe" tools\chm\out\UltraLib.hhp
```

### Output

- `out/*.html` — converted HTML pages (with TOC, styling, cross-page navigation).
- `out/UltraLib.hhc` — CHM table-of-contents.
- `out/UltraLib.hhp` — CHM project file (compiled by hhc.exe).
- `out/UltraLib.chm` — **final CHM document** (distributable).

### Maintenance rules

- When adding a page, register it in the `pages` array in `tools/chm/build-chm.js` (both EN and zh variants).
- Rebuild the CHM after every `.md` source change to keep Wiki and CHM consistent.
- `out/` is build output and is git-ignored; attach the CHM separately on release.
