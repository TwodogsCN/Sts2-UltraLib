# UltraLib CHM 代码规范 / CHM Conventions

[English](CHM_CONVENTIONS.md) · [中文](CHM_CONVENTIONS.zh-CN.md)

> 适用对象：给 `tools/chm-win/`（WinCHM 工程）新增/修改页面的贡献者。
> **核心原则：样式已经全部写好了，直接复用，不要自造样式。**

## 1. 工程结构

```
tools/chm-win/
├─ UltraLibHelper.wcp      # WinCHM 工程文件（用 WinCHM 打开）
├─ code-style.css          # 全局样式（已就绪，直接引用）
├─ code-copy.js            # 代码复制脚本（已就绪，直接引用）
└─ data/
   ├─ intro/               # 前言
   ├─ check-update/        # 检查更新（版本检测 + 下载）
   ├─ learn-csharp/        # 简单认识 C#
   ├─ changelog/           # 更新日志
   ├─ tutorial/            # 入门模组开发教学
   ├─ new-features/        # 新增功能专项（总览）
   ├─ new-hooks/           # 新 Hooks 表
   ├─ new-utils/           # 新 Utils 表 + 17 个工具类子页
   ├─ charge-relic/        # 充能遗物（3 页）
   ├─ variables/           # 动态变量（2 词条）
   └─ card-keywords/       # 卡牌关键词（6 词条）
```

## 2. 样式已就绪：直接复用，不要自造

每个 HTML 页面**必须**引用工程根的两个共享文件（页面位于 `data/<子目录>/`，需上溯两级）：

```html
<link rel="stylesheet" type="text/css" href="../../code-style.css">
<script type="text/javascript" src="../../code-copy.js"></script>
```

已提供且可直接使用的样式类（都定义在 `code-style.css`，深色终端/Geek 极简风，CHM/IE7 兼容）：

| 类名 | 用途 |
|------|------|
| `info-box` + `info-title` + `info-content` | 页面开头说明块（深色底、白字）。**info-content 是白字，只能在深色 info-box 内使用** |
| `plain` | 正文段落（深色文字）。普通段落用这个，不要用 info-content 脱框 |
| `code-box` + `copy-btn` + `<pre>` | 代码框（深色底），右上角自带「复制」按钮 |
| `hook-table` / `utils-table` | 表格（Hook 表 / Utils 表共用样式）；`<tr class="alt">` 做斑马纹 |
| `v-item` | 方法/词条详解块（浅色底、边框） |
| `hl` | 行内高亮 |
| `tip` | 黄色提示条（警告类） |
| `card` + `title` + `label` + `value` | 检查更新页卡片样式 |
| `btn` + `btn-blue` + `btn-white` | 按钮样式 |
| `keyword` / `type` / `property` / `comment` / `number` / `string` / `operator` | 代码框内语法高亮 span |

代码框写法（复制按钮自动生效）：

```html
<div class="code-box">
    <button class="copy-btn" onclick="copyCode(this)">复制</button>
    <pre><span class="keyword">public static void</span> <span class="property">Foo</span>() { }</pre>
</div>
```

表格斑马纹（CHM/IE7 不支持 `:nth-child`，必须手动加类）：

```html
<table class="utils-table">
    <tr><th>列1</th><th>列2</th></tr>
    <tr><td>...</td><td>...</td></tr>
    <tr class="alt"><td>...</td><td>...</td></tr>
</table>
```

## 3. 页面硬性规则（踩过的坑）

- `htm` 文件名与文件夹必须**纯 ASCII**（英文，无 CJK / 特殊字符）——中文或 `C#` 文件名会导致 WinCHM 显示异常。
- 页面编码 **UTF-8 BOM + CRLF**；`.wcp` 的 URL 分隔符用 `\`（如 `data\new-utils\card-helper.htm`）。
- `<meta http-equiv="Content-Type" content="text/html; charset=UTF-8">` 必须保留。
- 页面间跳转用相对路径（同目录直接文件名，跨目录 `../xxx/xxx.htm`）。
- 页面标题 `<title>` 用「标题 - UltraLib Helper」格式。

## 4. 新增页面后要做的三件事

1. 在 `data/` 下建纯 ASCII 子目录 + HTML 文件。
2. 在 WinCHM 中把页面加进目录树（或直接编辑 `UltraLibHelper.wcp` 的 `[TOPICS]`：每个页面一组 `TitleList.Title.N / Level.N / Url.N / ...`，Level 控制层级，编号连续）。
3. 同步更新：`data/intro/intro.htm` 的「文档结构」、`tools/chm-win/README.md`、在线 Wiki（`docs/`）。

## 5. 检查更新页特殊规则

- 版本号四处同步：`version.txt`、`version.js`、`UltraLib.json`、`check-update.htm` 的 `CONFIG.current`（详见 `docs/DEVELOPMENT_WORKFLOW.md` §10.4.1）。
- 版本脚本必须走 jsDelivr（`https://cdn.jsdelivr.net/gh/TwodogsCN/Sts2-UltraLib@main/version.js`），**禁止** `raw.githubusercontent.com`——其 CSP sandbox 头会阻止脚本执行。
- 发布后需 `curl https://purge.jsdelivr.net/gh/TwodogsCN/Sts2-UltraLib@main/version.js` 刷新 jsDelivr 缓存。

## 6. 修改 `code-style.css` / `code-copy.js`

这两个文件是**全局共享**的，改动会影响到所有页面。修改前：

- 保持 CHM/IE7 兼容（**禁止** `var()` / `:root` / `:nth-child` / `transition` / `rgba()`，颜色一律硬编码，见文件头部注释）。
- 先小范围验证（在 WinCHM 中编译看效果），再合入。
- 如需新增样式类，遵循现有命名与注释风格。
