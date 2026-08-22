# UltraLib Helper —— WinCHM 工程

这里是 UltraLib 的 **WinCHM 离线文档工程**（与在线 Wiki / 仓库 docs 内容同源，但改用 WinCHM 编译、风格更贴近 CHM 阅读体验）。

> 📘 **新增/修改页面之前，请先读 [CHM 代码规范](../../docs/CHM_CONVENTIONS.zh-CN.md)**（`docs/CHM_CONVENTIONS.md` 双语）——样式全部写好了，直接复用，不要自造。

## 目录结构

```
tools/chm-win/
├─ UltraLibHelper.wcp        # WinCHM 工程文件（用 WinCHM 打开）
├─ code-style.css            # 全局样式（复制按钮、代码框、语法高亮、info-box）——已就绪，直接引用
├─ code-copy.js              # 代码复制脚本（复制按钮交互）——已就绪，直接引用
└─ data/                     # 页面源（每个子目录一页，HTML）
   ├─ intro/                 # 前言
   ├─ check-update/          # 检查更新（版本检测 + jsDelivr JSONP + GitHub 下载）
   ├─ learn-csharp/          # 简单认识 C#
   ├─ changelog/             # 更新日志
   ├─ tutorial/              # 入门模组开发教学（占位骨架）
   ├─ new-features/          # 新增功能专项（总览）
   ├─ new-hooks/             # 新Hooks表
   ├─ new-utils/             # 新Utils表 + 17 个工具类子页
   ├─ charge-relic/          # 充能遗物（总览 / 机制 / 触发与 Hook）
   ├─ variables/             # 动态变量（EmpowerVar / ReturnVar）
   └─ card-keywords/         # 卡牌关键词（6 词条）
```

## 如何编译

1. **安装 WinCHM**（https://www.everbsoft.com/winchm.html）。
2. 用 WinCHM 打开 `tools/chm-win/UltraLibHelper.wcp`。
3. 点击 **编译（Compile）**，产物直接输出到 `tools/chm-win/UltraLib.chm`（`.wcp` 中 `CompiledFile=UltraLib.chm`、`RootDir` 为空）。
4. 编译产物即发布产物：**提交入库**（`.gitignore` 已放开 `tools/chm-win/*.chm`），检查更新页从 `raw/main/tools/chm-win/UltraLib.chm` 下载。

## 维护规则

- **样式**：所有页面引用 `../../code-style.css` 与 `../../code-copy.js`；代码框用 `class="code-box"` + 复制按钮，说明用 `class="info-box"`，表格用 `utils-table`/`hook-table` + `class="alt"` 斑马纹。完整类清单与示例见 **CHM 代码规范**（`docs/CHM_CONVENTIONS.zh-CN.md`）。
- **新增页面**：在 `data/` 下建纯 ASCII 子目录写 HTML → 在 WinCHM 里加进目录树（或直接改 `UltraLibHelper.wcp` 的 `[TOPICS]`，每个页面一组 `TitleList.*.N`，Level 控制层级）→ 同步 `data/intro/intro.htm` 的「文档结构」。
- **硬性规则**：`htm` 文件名/文件夹纯 ASCII（中文或 `C#` 会导致 WinCHM 显示异常）；页面 UTF-8 BOM + CRLF；`.wcp` URL 用 `\` 分隔符。
- **更新日志**：每次发布在 `data/changelog/` 追加，并同步 4 处版本号（`version.txt` / `version.js` / `UltraLib.json` / `check-update.htm` 的 `CONFIG.current`）。
- **检查更新**：版本脚本必须走 jsDelivr（禁止 `raw.githubusercontent.com`，其 CSP sandbox 会阻止 JSONP 执行）；发布后需 `curl https://purge.jsdelivr.net/gh/TwodogsCN/Sts2-UltraLib@main/version.js` 刷新缓存。
- 编译产物（`UltraLib.chm`）由 WinCHM 生成，**提交入库**，随发布提供。

## 与仓库其它文档的关系

- `docs/`（Markdown）→ 在线 Wiki 内容源
- `docs/CHM_CONVENTIONS.md`（双语）→ **CHM 代码规范**：样式类清单、页面规则、检查更新特殊规则
- `tools/chm-win/`（本工程）→ **WinCHM 离线手册**（CHM 的主用工程）

> 建议：以 `docs/` 为内容权威源；需要精修/排版时改这里的 HTML。
