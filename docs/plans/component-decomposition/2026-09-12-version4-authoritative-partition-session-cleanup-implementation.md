# Version4 Authoritative Partition Session Cleanup Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Add an explicit `Cleanup -PartitionId` action that atomically clears every session field
for a known authoritative partition and makes it claimable again.

**Architecture:** Reuse the existing authoritative runner's manifest validation, exclusive checkout
lock, JSON ledger, reset helper, result format, and exit-code contract. Cleanup is a partition-scoped
state reset performed under the same short lock transaction as claim and settlement; it preserves
fixed manifest/report fields but clears all mutable claim and execution fields, including a previous
`completed` state.

**Tech Stack:** PowerShell 7, .NET `FileStream`, JSON, focused PowerShell contract tests. No C# or
project/build input changes.

---

### Task 1: Add the failing cleanup contract tests

**Files:**
- Modify: `Build/Tools/Test-Version4AuthoritativePartitionSession.ps1`
- Read: `Build/Tools/Invoke-Version4AuthoritativePartitionSession.ps1`

**Step 1: Add a helper assertion for cleared mutable fields**

Use the existing isolated temporary ledger and assert that `status` is `available`, while
`claimMode`, `sessionId`, owner data, timestamps, exit code, command data, output, and error are
empty after cleanup.

**Step 2: Add tests for cleanup state transitions**

Create isolated records through the public runner, then invoke `Cleanup` for `running`, `completed`,
`failed`, and `abandoned` records. Assert each cleanup reports the prior status, resets all mutable
fields, and allows a subsequent `Claim`.

**Step 3: Add idempotence and partition-isolation assertions**

Run cleanup twice for an already available partition and verify both calls succeed. Claim a second
partition before cleaning the first and verify its `running` record is unchanged.

**Step 4: Run the test to verify the new tests fail**

Run:

```powershell
pwsh -NoProfile -File .\Build\Tools\Test-Version4AuthoritativePartitionSession.ps1
```

Expected: FAIL because `Cleanup` is not an accepted runner action and no cleanup result is produced.

### Task 2: Implement the minimal partition reset operation

**Files:**
- Modify: `Build/Tools/Invoke-Version4AuthoritativePartitionSession.ps1`

**Step 1: Expose `Cleanup` in the action parameter contract**

Add `Cleanup` to the action validation set and require `-PartitionId` for it. Do not require
`-SessionId`, command path, or retry arguments.

**Step 2: Extract or add a reset helper**

Reset only session-owned mutable fields to their initial values and set `status` to `available`.
Preserve `id`, `reportFileName`, `reportPath`, `expectedMemberCount`, and `observedMemberCount`.

**Step 3: Add the locked cleanup branch**

Inside the existing lock transaction, read the target record, save its prior status, reset it, save
the ledger, and return `status: cleaned`, `partition`, `previousStatus`, and `lockReleased: true`.
Do not call automatic dead-owner reconciliation in a way that changes the reported prior status.

**Step 4: Map cleanup errors to the existing contract**

Reuse named partition/report validation and busy/internal error handling. A cleanup of an already
available record must remain successful and must not alter other partitions.

### Task 3: Run the focused suite and harden the boundary

**Files:**
- Modify: `Build/Tools/Test-Version4AuthoritativePartitionSession.ps1`
- Modify: `Build/Tools/Invoke-Version4AuthoritativePartitionSession.ps1`

**Step 1: Run the focused suite after the minimal implementation**

Run the test command from Task 1. Expected: all existing contract tests and cleanup tests pass.

**Step 2: Verify lock and validation behavior**

Confirm cleanup is serialized by the existing lock, validation failures do not create or mutate the
ledger, and a cleanup of one partition cannot reset another partition.

**Step 3: Run formatting and scope checks**

Run:

```powershell
pwsh -NoProfile -File .\Build\Tools\Test-Version4AuthoritativePartitionSession.ps1
git diff --check -- Build/Tools/Invoke-Version4AuthoritativePartitionSession.ps1 Build/Tools/Test-Version4AuthoritativePartitionSession.ps1 docs/cr/component-decomposition/CR-2026-09-12-version4-authoritative-partition-session-cleanup.md docs/plans/component-decomposition/2026-09-12-version4-authoritative-partition-session-cleanup-design.md docs/plans/component-decomposition/2026-09-12-version4-authoritative-partition-session-cleanup-implementation.md
```

Record exit codes and confirm no `.agent-workplace/state` artifacts are included in the change.

### Task 4: Review the final scope

**Files:**
- Read: `Build/Tools/Invoke-Version4AuthoritativePartitionSession.ps1`
- Read: `Build/Tools/Test-Version4AuthoritativePartitionSession.ps1`
- Read: `docs/cr/component-decomposition/CR-2026-09-12-version4-authoritative-partition-session-cleanup.md`

Review only the cleanup behavior and its focused tests. Preserve unrelated user changes in the
working tree and do not modify Version4 reports, C# source, or generated outputs.
