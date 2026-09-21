# Version4 Non-Authoritative Top-30 Peer Refinement Implementation Plan

> **For Codex:** Execute the tasks in order. Preserve unrelated worktree changes.

**Goal:** Replace five over-broad first-30 final peers with ten evidence-backed third-level peers while preserving all member facts and report lineage.

**Architecture:** Extend the existing deterministic PowerShell mapping with a third-level mapping. Keep the original 239-group baseline on every member, record the retired 289-group peer as immediate lineage for the ten new groups, and regenerate the report from the unchanged input inventory.

**Tech Stack:** PowerShell, Markdown member inventories, SHA-256 trace records.

---

### Task 1: Add third-level mapping and definitions

**Files:**
- Modify: `Build/Tools/Generate-Version4NonAuthoritativeFineSubsystemReport.ps1`

Add the five retired immediate-peer IDs, ten new definitions, type/path mapping rules, and the
third-level lineage metadata. Keep unmatched rows as hard errors.

### Task 2: Render third-level lineage and updated ranking

**Files:**
- Modify: `Build/Tools/Generate-Version4NonAuthoritativeFineSubsystemReport.ps1`

Update active counts, ranking prose, per-peer metadata, and add the third-level retired-peer
summary. Keep the existing 239 baseline mapping and all source declaration cells unchanged.

### Task 3: Strengthen full report verification

**Files:**
- Modify: `Build/Tools/Test-Version4NonAuthoritativeFineSubsystemReport.ps1`

Parse immediate-peer metadata and assert 294 active peers, ten expected children, five retired
current peers, one immediate peer per new child, and unchanged 4,542-row/4,017-field/525-property
closure and rollups.

### Task 4: Strengthen focused split verification

**Files:**
- Modify: `Build/Tools/Test-Version4NonAuthoritativeFineSubsystemSplit.ps1`

Retain first- and second-level assertions, then add the ten third-level IDs, five retired IDs,
two-child minimum, and exact active-count assertion.

### Task 5: Regenerate and inspect the report

**Files:**
- Modify: `docs/migration/ledgers/Version4非权威模拟系统字段属性逐成员源码声明-更细子系统拆分-去除ID类文件.md`

Run the generator from the repository root. Inspect the top-30 prefix, complete 294-row ranking,
third-level lineage section, and the ten new member sections.

### Task 6: Verify the generated artifacts

Run the generator, focused verifier, full verifier, and an independent PowerShell summary audit
serially. Record each command, exit code, counts, and generated output path. No `dotnet` command is
needed because this change touches only PowerShell and Markdown.

