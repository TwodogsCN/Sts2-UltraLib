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

*Section 10 (Wiki maintenance) is deferred — to be discussed and added later.*
