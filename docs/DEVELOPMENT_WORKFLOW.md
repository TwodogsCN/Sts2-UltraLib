# UltraLib Development Workflow

[English](DEVELOPMENT_WORKFLOW.md) · [中文](DEVELOPMENT_WORKFLOW.zh-CN.md)

This page defines the contribution workflow for **UltraLib**. It is maintained in the repository at `docs/DEVELOPMENT_WORKFLOW.md` and is meant to be mirrored into the project Wiki.

---

## 1. Overall principle

All changes go through a closed loop:

**Issue → Branch → Commit → PR → Review → Merge**

Global rules:

- Every requirement / feature / bug has one GitHub **Issue**.
- Every **PR** is linked to one (or more) Issue(s).
- Every **state transition** (Issue ↔ PR ↔ merge) leaves a trace.
- **No bypassing.** Never develop or push directly to `main`, and never open a PR that is not linked to an Issue. Unlinked / rushed PRs will be closed without review.

## 2. Issue conventions

### 2.1 When to open an Issue

- Open one Issue per **independently deliverable feature / bug / requirement**.
- One Issue corresponds to one small, complete piece of work (e.g. "add the `PlusChargeRelic` abstract base", "fix orb passive hook not being dispatched").
- Split large / bulk requirements into **multiple Issues** to avoid oversized PRs.

### 2.2 Title format

```
[Title prefix] One-sentence summary
```

Practical examples (verb-first, one sentence, may carry an ID):

- `[Feature] 新增充能遗物抽象基类 PlusChargeRelic`
- `[Bug] 充能球被动钩子异常未被捕获导致战斗流程中断`
- `[Doc] 钩子系统补充 Pipeline/Product/Sum 语义说明`
- `[Refactor] PlusHooks 分发器统一日志与异常捕获`

Recommended prefixes: `[Feature]` / `[Bug]` / `[Doc]` / `[Refactor]` / `[Test]` / `[Infra]`.

### 2.3 Body template

```
### 背景 / 目标 (Background / Goal)
(Why are we doing this, what feature it delivers, example usage)

### 需求清单 (Requirement checklist)
- [ ] Subtask 1
- [ ] Subtask 2

### 验收口径 (Acceptance criteria, preferably testable)

### 关联 (Related)
- Dependent Issue / PR
```

## 3. Branch conventions

### 3.1 Naming

Always `feat/<slug>`. The slug is lowercase-with-hyphens and should be self-explanatory (project practical style; not strictly bound to an Issue number, but linking is encouraged, e.g. `feat/plus-charge-relic`).

### 3.2 Lifecycle

- Branch is created from the latest `main`, and `main` is merged in early and often to avoid long-lived branch conflicts.
- Feature complete → push → open PR → review approved → merge → **delete the remote branch**.
- Keep small, reviewable commits and readable history until the branch is merged.

### 3.3 Forbidden

- Never develop directly on `main`.
- Never push directly to `main`.

## 4. Commit message conventions

Project practical style: **`<type>(<scope>): <summary> (Issue #N: key points)`**. It serves both "for humans" and "for AI / traceability", so keep information density high.

### 4.1 Format

```
<type>(<scope>): <one-line summary> (Issue #N: key points)
```

- **type** (required): `feat` / `fix` / `refactor` / `docs` / `test` / `ci` / `chore`
- **scope** (optional but recommended): project domain, e.g. `core` / `hook` / `patch` / `base` / `net` / `variables` / `localization` / `cfg`
- **summary**: verb-object structure, what was done
- **Issue reference + key points**: link the Issue number, then list key points after the colon for easy traceability

### 4.2 Principles

- **One commit does one thing** (matches §6).
- **High information density**: put "what was done + linked Issue + acceptance evidence" in the summary so it is easy to trace back and to feed to AI.
- Use imperative mood verbs: `feat(...): 新增...`, `fix(...): 修复...`, `test(...): 补用例...`.
- When acceptance IDs apply (Issue #N, A0x/B0x), include them in the commit so they map to the acceptance matrix.

## 5. PR conventions

### 5.1 Prerequisites

- A PR **solves only one topic** (usually one or more linked Issues) and stays small enough to review.

### 5.2 Title

```
feat(hook): 新增充能球被动钩子 Plus_BeforeOrbPassive (Issue #12)
```

Same style as the commit message: `<type>(<scope>): <summary> (Issue #N)`.

### 5.3 PR description template (recommended)

Structured for reviewers and AI to read quickly:

```
## 变更内容 (Change summary)
(one sentence + covered Issues / acceptance IDs)

### 新增 (Added)
- files / capabilities (one by one)

### 修改 (Modified)
- files / behavior changes (one by one)

### 修复 (Fixed, if any)
- bugs / defects

## 验收口径 / 与文档一致性 (Acceptance / doc consistency)
- how non-crash tolerance / hook dispatch / localization sync are satisfied
- which existing docs (Wiki) need to be synced

## 验证 (Verification)
- which checks were run: build result (e.g. `dotnet build` BUILD SUCCESS), game-load smoke test, `eng`/`zhs` localization parity
- local / CI status
```

### 5.4 Linking Issues (Development panel)

Use trigger words in the PR description so GitHub auto-creates the Development link:

- `Closes #12` → auto-closes the Issue on merge (use when the Issue is fully implemented).
- `Fixes #xx` / `Resolves #xx` → same (for fix-type PRs).
- `Part of #xx` → links only, **does not auto-close** (use when the Issue has other subtasks).

> Note: GitHub only recognizes English trigger words (`closes` / `fixes` / `resolves` / `part of`); the Chinese word "关联" does not work.

### 5.5 Review is mandatory — no merge without approval

- A PR is **only merged after at least one reviewer approves it** and there are **no unresolved requested changes**.
- Authors do not merge their own PR unless they are the sole maintainer and reviewers are unavailable.
- A PR with a failing build / check, unresolved review threads, or no linked Issue is blocked.
- Rushed, unrelated, or unlinked "drive-by" PRs are **closed without review** — open an Issue and follow the loop instead.

---

## 10. Wiki & documentation maintenance

> Applies when your change ends with **new functionality** or **revised behavior/content**.

### 10.1 Rule: keep docs in sync with code

- When you add a **new feature** or **fix/change existing behavior**, the change is not complete until the documentation is updated (if docs apply).
- UltraLib maintains **two** surfaces that serve different audiences. They are **NOT** generated from the same source and must be maintained **separately**:
  - **GitHub Wiki** (online, bilingual) — the primary human-readable documentation, maintained in `docs/` (Markdown) and mirrored to the Wiki.
  - **CHM document** (offline) — a WinCHM project under `tools/chm-win/` with its **own structure and content** (前言 / 检查更新 / 简单认识C# / 更新日志 / 原版Hook列表 / 入门模组开发教学 / 新增功能-新Hooks表-新Utils表). It is the offline reference for mod authors, compiled with WinCHM into `UltraLib Helper.chm`.
- **Mandatory:** every new public type / method / hook / helper must carry a **bilingual `<summary>`** in its XML doc comment (an English description + a 简体中文 description). This feeds the bilingual Wiki and the CHM so no separate translation step is needed. See [Code conventions §7](CODE_CONVENTIONS.md).

### 10.2 What triggers a docs update

| Change | Update Wiki? | Update CHM? |
|--------|--------------|-------------|
| New public type / method / hook / helper | **Yes** | **Yes** (new feature) |
| Behavior change of an existing feature | **Yes (if user-facing)** | **Yes** |
| Internal refactor with no visible change | No | No |
| Bug fix that changes how something works | Yes | Yes |
| Docs-only change (typography, wording) | Yes | Yes |

- **New functionality is always synced into the CHM** — the CHM is the offline reference that mod authors consult, so it must include any new API/feature.

### 10.3 How to update the Wiki

- Wiki pages live in `docs/` in this repository; the GitHub Wiki is kept in sync with them.
- Edit the matching Markdown source in `docs/`, e.g.:
  - `docs/API_INDEX.md` / `.zh-CN.md` — add new types/helpers to the index.
  - `docs/Hook.md` / `docs/Utils.md` / `.zh-CN.md` — add descriptions for new methods/hooks.
- Follow the [link conventions](CODE_CONVENTIONS.md): bilingual, `[Page] / [页面]`-style, Wiki page names without `.md` suffix.
- Push the `docs/` changes; mirror them to the GitHub Wiki so the Wiki stays current.

### 10.4 How to update the CHM

- The CHM is a **WinCHM project** in `tools/chm-win/` (`UltraLibHelper.wcp`). Its content/structure is independent of the Wiki — update it when the change affects anything a mod author needs offline (new hooks, new utils, tutorials, changelog).
- After a new feature:
  1. Open `tools/chm-win/UltraLibHelper.wcp` in WinCHM.
  2. Add/update the relevant page(s) (e.g. `新Hooks表`, `新Utils表`, tutorial pages) following the project's existing HTML style (`code-style.css` + `code-copy.js`).
  3. All `htm` file names and folders must be **ASCII-only** (English, no CJK / special characters).
  4. Rebuild in WinCHM; verify the new page appears in the TOC; publish the rebuilt `.chm` with the release.

### 10.4.1 Release build output & version sync (REQUIRED for every release)

The CHM compiles **directly to `tools/chm-win/UltraLib.chm`** (the `.wcp` sets `CompiledFile=UltraLib.chm` with an empty `RootDir`, so WinCHM outputs next to the project file). **This compiled file is the release artifact** — it is committed to the repo (do not re-ignore it in `.gitignore`), and the in-CHM "检查更新 / Check Update" page downloads it directly from `raw/main/tools/chm-win/UltraLib.chm`.

Before shipping a new release, update **all four version sources** so the in-CHM "检查更新 / Check Update" page can detect it:

| # | File | Where to edit |
|---|------|---------------|
| 1 | `version.txt` | repository root — plain-text version, read by the check page as a fallback |
| 2 | `version.js` | repository root — `window.ULTRALIB_LATEST = "x.y.z";` (JSONP, loaded by the check page) |
| 3 | `UltraLib.json` | `"version": "x.y.z"` — the mod's own version |
| 4 | `tools/chm-win/data/check-update/check-update.htm` | `CONFIG.current` and the visible `<span class="value">` — the in-CHM local version |

Then:
1. Rebuild the CHM in WinCHM → output goes to `tools/chm-win/UltraLib.chm` (already the committed release artifact).
2. Commit & push `version.txt`, `version.js`, `UltraLib.json`, the rebuilt `tools/chm-win/UltraLib.chm`, and any CHM source changes.
3. **Purge the jsDelivr cache** so the check page picks up the new `version.js` immediately:
   `curl https://purge.jsdelivr.net/gh/TwodogsCN/Sts2-UltraLib@main/version.js` (jsDelivr caches branch files up to 24h).

> The check page loads the version via **JSONP from jsDelivr** (`https://cdn.jsdelivr.net/gh/TwodogsCN/Sts2-UltraLib@main/version.js`) and downloads the CHM from `raw/main/tools/chm-win/UltraLib.chm`. Do **not** use `raw.githubusercontent.com` for the version script — GitHub raw returns a `Content-Security-Policy: sandbox` header on every file, which blocks script execution and breaks JSONP. All four version sources must stay **identical**; forgetting any one makes the check page report the wrong result.

### 10.5 Checklist for PRs that change code

- [ ] `docs/` updated for the change (new API/feature documented).
- [ ] Wiki mirrored to match `docs/`.
- [ ] CHM (WinCHM project `tools/chm-win/`) updated for new features — pages added in English/ASCII file names and rebuilt in WinCHM.
- [ ] **New functionality ships with a bilingual XML `<summary>`** (EN + 中文) on all new public types / methods / hooks / helpers, explaining purpose and usage.
- [ ] Bilingual (EN + 中文) where applicable.
