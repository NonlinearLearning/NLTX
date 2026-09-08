# Version4 Field Property Inventory Tool Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Add a repeatable tool that converts the complete Version4 Roslyn snapshot into a Markdown inventory containing every retained field/property member with its file path and source line.

**Architecture:** Keep Roslyn scanning and Markdown projection separate. The existing `Version4MemberMigrationScanner` produces the source snapshot; a new PowerShell generator validates that snapshot, excludes files whose stem ends in uppercase `ID` or `IDs`, builds an all-file index, and writes one member row per retained `field` or `property`. The generator writes to an explicit output path and fails closed on incomplete snapshots or malformed member locations.

**Tech Stack:** PowerShell, JSON snapshot produced by `Microsoft.CodeAnalysis.CSharp` Roslyn scanner, Markdown.

---

### Task 1: Define the generator contract and validation invariants

**Files:**
- Create: `Build/Tools/Generate-Version4FieldPropertyInventory.ps1`
- Test: `Build/Tools/Test-Generate-Version4FieldPropertyInventory.ps1`

**Step 1: Write the failing test**

Create a fixture-driven PowerShell test that invokes the generator against the current complete snapshot and asserts:

- output Markdown exists;
- the file index contains 915 retained source files;
- the detail table contains 7,201 retained `field`/`property` records;
- every detail row has a normalized file path and positive source line number;
- no detail row or file-index row has a file stem ending in uppercase `ID` or `IDs`;
- the retained member count equals the sum of per-file counts;
- the generated summary reports `complete` and zero diagnostics.

**Step 2: Run the test to verify it fails**

Run the test before the generator exists. Expected: failure because `Generate-Version4FieldPropertyInventory.ps1` does not exist.

**Step 3: Implement the minimal generator**

Implement parameters for snapshot path, output path, and optional excluded-file output. Load JSON, validate `snapshotKind=source`, `completeness=complete`, diagnostics empty, and required arrays. Filter members to `field` and `property`; exclude file paths whose filename stem matches the case-sensitive `IDs?$` suffix; require one valid location with a positive line number; sort the file index by count descending/path ascending and detail rows by path/line/kind/declaring type/member. Emit summary, all-file index, and full detail table.

**Step 4: Run the test to verify it passes**

Run the same test. Expected: PASS with 915 files and 7,201 detail records.

**Step 5: Commit**

Do not commit automatically in this session; preserve the existing user worktree state and report the exact new files instead.

### Task 2: Generate and independently verify the requested Markdown artifact

**Files:**
- Create: `docs/迁移参考表/Version4全部字段属性明细-去除ID类文件.md`
- Create: `Build/generated/version4-field-property-inventory-2026-09-07.json` (optional machine summary)

**Step 1: Run the generator**

Run the generator from the repository root using the freshly generated all-project snapshot:

```powershell
& .\Build\Tools\Generate-Version4FieldPropertyInventory.ps1 `
  -SnapshotPath .\Build\generated\version4-source-all-projects-2026-09-07.json `
  -OutputPath '.\docs\迁移参考表\Version4全部字段属性明细-去除ID类文件.md'
```

**Step 2: Verify the artifact independently**

Parse the generated Markdown and assert 915 file-index rows, 7,201 detail rows, zero ID-suffix paths, positive line numbers, and the expected field/property totals of 6,414 and 787. Run `git diff --check` for the new Markdown and script.

**Step 3: Report evidence**

Report the snapshot path, scanner exit code, generator result, output path, row counts, member totals, excluded-file count, and verification status. Do not claim runtime or migration completion from this inventory.

---
