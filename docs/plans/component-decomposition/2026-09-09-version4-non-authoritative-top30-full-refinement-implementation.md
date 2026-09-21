# Version4 Non-Authoritative Top-30 Full Refinement Implementation Plan

> Preserve unrelated worktree changes. This change updates the deterministic report pipeline and
> generated documentation only; it does not modify `src/`, `Test/`, or the Version4 reference checkout.

## Completion Status

All tasks in this plan are complete. The final report has 324 active peer groups: the 30 current
top-ranked peers retired in this pass are represented by 60 non-empty fourth-level children. The
two third-level children that were also selected are retained in the old five-to-ten lineage and
expanded through their fourth-level descendants in section 3.6.

## Task 1: Add the next-level mapping [complete]

Modify `Build/Tools/Generate-Version4NonAuthoritativeFineSubsystemReport.ps1`:

- Add 30 retired current top-30 peer IDs.
- Add two explicit child definitions for each retired peer.
- Map by declaration type, source path, or stable member-family semantics.
- Preserve original baseline and immediate-peer lineage.
- Fail closed for an unmapped target member.

## Task 2: Render the new lineage and ranking [complete]

Update the generator to:

- Produce 324 active final peer groups.
- Add the 30-to-60 lineage section after the existing five-to-ten lineage section.
- Render dynamic active counts in ranking text and report headers.
- Mark next-level children with `next-level-peer-split-complete` and retain the evidence status.

## Task 3: Strengthen focused verification [complete]

Modify `Build/Tools/Test-Version4NonAuthoritativeFineSubsystemSplit.ps1` to assert:

- exactly 30 new retired current peers;
- exactly 60 expected children;
- exactly two active children per retired peer;
- all child statistics are non-zero and sum to the retired peer total;
- exactly 324 active fine subsystems.

## Task 4: Strengthen full verification [complete]

Modify `Build/Tools/Test-Version4NonAuthoritativeFineSubsystemReport.ps1` to verify the new
lineage, immediate-peer mappings, expected active count, complete ranking, and unchanged global
member totals. Keep the existing five-to-ten third-level assertions.

## Task 5: Regenerate and audit [complete]

Run from the repository root:

1. `pwsh -NoProfile -File .\Build\Tools\Generate-Version4NonAuthoritativeFineSubsystemReport.ps1`
2. `pwsh -NoProfile -File .\Build\Tools\Test-Version4NonAuthoritativeFineSubsystemSplit.ps1`
3. `pwsh -NoProfile -File .\Build\Tools\Test-Version4NonAuthoritativeFineSubsystemReport.ps1`
4. independent summary/lineage audit and `git diff --check`

### Verification Record

| Command | Exit code | Result |
|---|---:|---|
| `pwsh -NoProfile -File .\Build\Tools\Generate-Version4NonAuthoritativeFineSubsystemReport.ps1` | 0 | Wrote `docs/migration/ledgers/Version4非权威模拟系统字段属性逐成员源码声明-更细子系统拆分-去除ID类文件.md`; 324 active groups; 4,017 fields; 525 properties; 4,542 total members. |
| `pwsh -NoProfile -File .\Build\Tools\Test-Version4NonAuthoritativeFineSubsystemSplit.ps1` | 0 | 30 retired peers, 60 fourth-level children, two non-empty children per peer, 324 active groups. |
| `pwsh -NoProfile -File .\Build\Tools\Test-Version4NonAuthoritativeFineSubsystemReport.ps1` | 0 | Input/report row closure, complete ranking, baseline rollups, parent rollups, and ID-file exclusion passed. |
| Independent summary/lineage audit | 0 | Top-50 rows 50; top ranks 1-30 continuous; full ranking 324; fourth-level lineage 30; detail rows 4,542. |
| `git diff --check` | 0 | No whitespace errors. Git reported existing LF-to-CRLF normalization warnings only. |

Remaining evidence gaps are unchanged: this report confirms source-inventory ownership and
decomposition boundaries, not runtime ECS implementation, complete readers/writers, lifecycle,
serialization, network closure, or scheduling behavior.
