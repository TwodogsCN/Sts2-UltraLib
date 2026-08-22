# UltraLib CHM Conventions

[English](CHM_CONVENTIONS.md) · [中文](CHM_CONVENTIONS.zh-CN.md)

> Audience: contributors adding/editing pages in `tools/chm-win/` (the WinCHM project).
> **Core principle: the styles are already written — reuse them, do not invent new ones.**

## 1. Project layout

```
tools/chm-win/
├─ UltraLibHelper.wcp      # WinCHM project file (open with WinCHM)
├─ code-style.css          # Global styles (ready to use — just reference it)
├─ code-copy.js            # Code-copy script (ready to use — just reference it)
└─ data/
   ├─ intro/               # Preface
   ├─ check-update/        # Check update (version check + download)
   ├─ learn-csharp/        # A quick look at C#
   ├─ changelog/           # Changelog
   ├─ tutorial/            # Beginner mod-dev tutorial
   ├─ new-features/        # New-features overview
   ├─ new-hooks/           # New Hooks table
   ├─ new-utils/           # New Utils table + 17 helper sub-pages
   ├─ charge-relic/        # Charge relic (3 pages)
   ├─ variables/           # Dynamic variables (2 entries)
   └─ card-keywords/       # Card keywords (6 entries)
```

## 2. Styles are ready — reuse, don't reinvent

Every HTML page **must** reference the two shared root files (pages live under `data/<subdir>/`, two levels up to the project root):

```html
<link rel="stylesheet" type="text/css" href="../../code-style.css">
<script type="text/javascript" src="../../code-copy.js"></script>
```

Ready-to-use style classes (all defined in `code-style.css`; dark terminal / geek-minimal theme, CHM/IE7 compatible):

| Class | Purpose |
|-------|---------|
| `info-box` + `info-title` + `info-content` | Intro callout at the top (dark background, white text). **`info-content` is white text — use it only inside a dark `info-box`** |
| `plain` | Body paragraph (dark text). Use this for normal paragraphs, not `info-content` outside a box |
| `code-box` + `copy-btn` + `<pre>` | Code block (dark background) with a built-in "Copy" button |
| `hook-table` / `utils-table` | Tables (shared style for hooks/utils); `<tr class="alt">` for striping |
| `v-item` | Detailed entry block (method / keyword deep-dive) |
| `hl` | Inline highlight |
| `tip` | Yellow warning strip |
| `card` + `title` + `label` + `value` | Check-update page card |
| `btn` + `btn-blue` + `btn-white` | Buttons |
| `keyword` / `type` / `property` / `comment` / `number` / `string` / `operator` | Syntax-highlight spans inside code boxes |

Code box (copy button works automatically):

```html
<div class="code-box">
    <button class="copy-btn" onclick="copyCode(this)">复制</button>
    <pre><span class="keyword">public static void</span> <span class="property">Foo</span>() { }</pre>
</div>
```

Table striping (CHM/IE7 has no `:nth-child` — add the class manually):

```html
<table class="utils-table">
    <tr><th>Col1</th><th>Col2</th></tr>
    <tr><td>...</td><td>...</td></tr>
    <tr class="alt"><td>...</td><td>...</td></tr>
</table>
```

## 3. Hard page rules (lessons learned)

- `htm` file names and folders must be **ASCII-only** (English, no CJK / special characters) — Chinese or `C#` names break WinCHM rendering.
- Page encoding **UTF-8 BOM + CRLF**; `.wcp` URLs use `\` separators (e.g. `data\new-utils\card-helper.htm`).
- Keep `<meta http-equiv="Content-Type" content="text/html; charset=UTF-8">`.
- Cross-page links use relative paths (same dir: file name; across dirs: `../xxx/xxx.htm`).
- `<title>` format: "Page Title - UltraLib Helper".

## 4. After adding a page, do three things

1. Create an ASCII-only subdirectory + HTML file under `data/`.
2. Add the page to the TOC in WinCHM (or edit `UltraLibHelper.wcp` `[TOPICS]` directly: each page is a `TitleList.Title.N / Level.N / Url.N / ...` group; `Level` controls hierarchy, numbering must be contiguous).
3. Keep in sync: `data/intro/intro.htm` "Document structure", `tools/chm-win/README.md`, and the online Wiki (`docs/`).

## 5. Check-update page special rules

- Four version sources stay in sync: `version.txt`, `version.js`, `UltraLib.json`, and `CONFIG.current` in `check-update.htm` (see `docs/DEVELOPMENT_WORKFLOW.md` §10.4.1).
- The version script must be loaded via jsDelivr (`https://cdn.jsdelivr.net/gh/TwodogsCN/Sts2-UltraLib@main/version.js`); **never** `raw.githubusercontent.com` — its `Content-Security-Policy: sandbox` header blocks script execution and breaks JSONP.
- After a release, purge the jsDelivr cache: `curl https://purge.jsdelivr.net/gh/TwodogsCN/Sts2-UltraLib@main/version.js`.

## 6. Modifying `code-style.css` / `code-copy.js`

These two files are **global** — changes affect every page. Before touching them:

- Keep CHM/IE7 compatibility (**no** `var()` / `:root` / `:nth-child` / `transition` / `rgba()`; hard-code colors — see the file header comment).
- Validate in a small scope (compile in WinCHM and check) before committing.
- If adding a class, follow the existing naming and comment style.
