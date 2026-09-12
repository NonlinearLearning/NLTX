# Version4 Non-Authoritative Top-30 Full Refinement Implementation Plan

> Preserve unrelated worktree changes. This change updates the deterministic report pipeline and
> generated documentation only; it does not modify `src/`, `Test/`, or the Version4 reference checkout.

## Task 1: Add the next-level mapping

Modify `Build/Tools/Generate-Version4NonAuthoritativeFineSubsystemReport.ps1`:

- Add 30 retired current top-30 peer IDs.
- Add two explicit child definitions for each retired peer.
- Map by declaration type, source path, or stable member-family semantics.
- Preserve original baseline and immediate-peer lineage.
- Fail closed for an unmapped target member.

## Task 2: Render the new lineage and ranking

Update the generator to:

- Produce 324 active final peer groups.
- Add the 30-to-60 lineage section after the existing five-to-ten lineage section.
- Render dynamic active counts in ranking text and report headers.
- Mark next-level children with `next-level-peer-split-complete` and retain the evidence status.

## Task 3: Strengthen focused verification

Modify `Build/Tools/Test-Version4NonAuthoritativeFineSubsystemSplit.ps1` to assert:

- exactly 30 new retired current peers;
- exactly 60 expected children;
- exactly two active children per retired peer;
- all child statistics are non-zero and sum to the retired peer total;
- exactly 324 active fine subsystems.

## Task 4: Strengthen full verification

Modify `Build/Tools/Test-Version4NonAuthoritativeFineSubsystemReport.ps1` to verify the new
lineage, immediate-peer mappings, expected active count, complete ranking, and unchanged global
member totals. Keep the existing five-to-ten third-level assertions.

## Task 5: Regenerate and audit

Run from the repository root:

1. `pwsh -NoProfile -File .\Build\Tools\Generate-Version4NonAuthoritativeFineSubsystemReport.ps1`
2. `pwsh -NoProfile -File .\Build\Tools\Test-Version4NonAuthoritativeFineSubsystemSplit.ps1`
3. `pwsh -NoProfile -File .\Build\Tools\Test-Version4NonAuthoritativeFineSubsystemReport.ps1`
4. independent summary/lineage audit and `git diff --check`

Record exit codes, counts, report path, and any remaining evidence gaps.
