# Version4 Non-Authoritative Top-12 Peer Refinement Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Extend the deterministic non-authoritative fine-subsystem report from the current fourth-level decomposition to a fifth-level split for the current top 12 peers.

**Architecture:** Keep the existing baseline/second/third/fourth lineage and add a narrow fifth-level mapping layer. Retire exactly 12 fourth-level peers, map each member to one of two path/type/family children, and render nested lineage explicitly so a fourth-level child that is also retired is not treated as an active terminal peer.

**Tech Stack:** PowerShell 7, Markdown report generation, read-only Version4 source inventory, focused and full PowerShell verifiers.

---

### Task 1: Lock the fifth-level evidence and mapping contract

**Files:**
- Create: `docs/plans/component-decomposition/2026-09-09-version4-non-authoritative-top12-peer-refinement-design.md`
- Modify: `Build/Tools/Generate-Version4NonAuthoritativeFineSubsystemReport.ps1`

Add the 12 retired peer IDs, the `Get-FifthLevelFineSubsystemId` mapping, and 24 definitions. Fail closed when a member of a retired peer does not match a declared boundary.

### Task 2: Integrate nested fifth-level ownership

**Files:**
- Modify: `Build/Tools/Generate-Version4NonAuthoritativeFineSubsystemReport.ps1`

Apply fifth-level mapping after fourth-level mapping, assign each child the original baseline and immediate retired peer, retire the 12 peers from the active definition set, and change the expected active count to `336`. Extend decomposition assessments and render a fifth-level lineage table. Fourth-level and third-level lineage must recursively include active descendants when one of their direct children is retired at the next level.

### Task 3: Strengthen focused split verification

**Files:**
- Modify: `Build/Tools/Test-Version4NonAuthoritativeFineSubsystemSplit.ps1`

Assert the 12 retired peer map, 24 fifth-level children, non-empty child totals, rollup equality, immediate-peer metadata, nested fourth-level direct-child counts, and the final `336` active peer count.

### Task 4: Strengthen full report verification

**Files:**
- Modify: `Build/Tools/Test-Version4NonAuthoritativeFineSubsystemReport.ps1`

Update summary parsing for the dynamic `336` heading, verify all fourth-level and fifth-level mappings and exact member statistics, check the nested baseline rollup, and preserve sequence closure and unchanged global totals.

### Task 5: Regenerate and verify

Run from `D:\TRbackup\NLTX`:

```powershell
pwsh -NoProfile -File .\Build\Tools\Generate-Version4NonAuthoritativeFineSubsystemReport.ps1
pwsh -NoProfile -File .\Build\Tools\Test-Version4NonAuthoritativeFineSubsystemSplit.ps1
pwsh -NoProfile -File .\Build\Tools\Test-Version4NonAuthoritativeFineSubsystemReport.ps1
git diff --check
```

Also run a read-only audit that recomputes the full `1..336` ranking, the top 50, fifth-level lineage count, detail-row count `4,542`, and field/property totals. Record exit codes, warning/error counts, and report path in this plan.

### Completion Status

All implementation tasks are complete. The generated report now has `336` active final peer groups; the 12 retired fifth-level peers expand to 24 non-empty children. Member facts remain `4,017` fields, `525` properties, and `4,542` rows.

### Verification Record

| Command | Exit code | Result |
|---|---:|---|
| `pwsh -NoProfile -File .\Build\Tools\Generate-Version4NonAuthoritativeFineSubsystemReport.ps1` | 0 | Generated `docs/migration/ledgers/Version4非权威模拟系统字段属性逐成员源码声明-更细子系统拆分-去除ID类文件.md`; 336 groups; 4,017 fields; 525 properties; 4,542 total members. |
| `pwsh -NoProfile -File .\Build\Tools\Test-Version4NonAuthoritativeFineSubsystemSplit.ps1` | 0 | 12 retired fifth-level peers, 24 children, non-empty rollups, nested fourth-level handling, and 336 active groups passed. |
| `pwsh -NoProfile -File .\Build\Tools\Test-Version4NonAuthoritativeFineSubsystemReport.ps1` | 0 | Input/report closure, ranking, baseline rollups, third/fourth/fifth lineage, summary rows, and ID-file exclusion passed. |
| Independent read-only ranking/lineage audit | 0 | 336 groups; 4,542 detail rows; 4,017 fields; 525 properties; sequence closure `1..4,542`; 12 fifth-level lineage rows. |
| `git diff --check` | 0 | No whitespace errors; Git emitted only existing LF-to-CRLF normalization warnings. |

Remaining evidence gaps are unchanged: this report confirms source-inventory ownership and decomposition boundaries, not runtime ECS implementation, complete readers/writers, lifecycle, serialization, network closure, or scheduling behavior.
