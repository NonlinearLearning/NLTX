# Version4 Authoritative Partition Session Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Add a PowerShell session runner that exclusively locks the checkout, claims one validated
authoritative partition report, executes a caller-supplied command, and records a recoverable result.

**Architecture:** Keep the twenty-report manifest immutable in the runner. Use an OS-held exclusive
lock-file handle for the entire claim/execution/finalization critical section and a JSON ledger for
auditable state. Treat a dead `running` owner as `abandoned`; never infer completion from a process
or lock failure. Use a child process environment contract instead of rewriting caller arguments.
Accept both a direct PowerShell argument array and a JSON-encoded argument array for separate-process
callers, whose `-Command` child argument would otherwise be ambiguous with runner parameters.

**Tech Stack:** PowerShell 7, .NET `FileStream`, `System.Diagnostics.Process`, JSON, focused
PowerShell integration tests. No C# or .NET project input changes.

---

### Task 1: Add the focused session-runner test harness

**Files:**
- Create: `Build/Tools/Test-Version4AuthoritativePartitionSession.ps1`
- Read: `docs/migration/ledgers/authoritative-20-partitions/*.md`

**Step 1: Define test helpers and isolated paths**

Create a temporary directory under the system temporary directory, use a copied or fixture-backed
partition directory, and pass explicit `-StatePath` and `-LockPath` values so the test never uses a
developer's real private ledger. Invoke the runner through a fresh `pwsh -NoProfile -File` process
and capture stdout, stderr, and exit code.

**Step 2: Add red tests for the public contract**

Cover `List`, successful `Run`, nonzero child failure, retry of a failed partition, `RunNext` order,
completed-partition refusal, invalid report refusal, dead-owner recovery, and concurrent execution
where a second invocation returns `busy` while the first child is sleeping.

**Step 3: Run the test before implementing the runner**

Run:

```powershell
pwsh -NoProfile -File .\Build\Tools\Test-Version4AuthoritativePartitionSession.ps1
```

Expected: FAIL because `Invoke-Version4AuthoritativePartitionSession.ps1` does not exist yet.

### Task 2: Implement the immutable manifest and report validation

**Files:**
- Create: `Build/Tools/Invoke-Version4AuthoritativePartitionSession.ps1`

**Step 1: Define parameters and exit codes**

Expose `-Action List|Run|RunNext`, conditional `-PartitionId`, `-CommandPath`,
`-CommandArgumentList` or `-CommandArgumentListJson`, `-Retry`, `-LockWaitSeconds`,
`-PartitionDirectory`, `-StatePath`, and `-LockPath`. Use stable exit codes for success, lock
contention, validation/input failure, state conflict, child failure, and internal failure.

**Step 2: Register P01-P20**

Store each partition ID, report filename, and expected member count in manifest order. Do not infer
the manifest from directory enumeration. Keep the report files read-only.

**Step 3: Validate selected reports**

Resolve the report path, require it to exist, count field/property member rows, compare against the
manifest, and reject ID-class source rows. Return structured validation errors before changing the
ledger.

### Task 3: Implement the exclusive lock and JSON ledger

**Files:**
- Modify: `Build/Tools/Invoke-Version4AuthoritativePartitionSession.ps1`

**Step 1: Acquire the checkout lock**

Open or create the lock path with `FileMode.OpenOrCreate`, `FileAccess.ReadWrite`, and
`FileShare.None`. Retry only until `-LockWaitSeconds` expires. Keep the `FileStream` alive through
child execution and final state persistence; write owner metadata into the stream for diagnosis.

**Step 2: Load and atomically persist state**

Initialize missing state with all twenty partitions as `available`. Read malformed JSON as an
error. Persist under the lock via a same-directory temporary file and replacement/move. Never use
file existence as the lock signal.

**Step 3: Reconcile dead owners**

For every `running` record, check the recorded process ID. A missing/dead process changes the record
to `abandoned` and preserves the original session and timestamps. A live owner remains protected
from a second claim.

### Task 4: Implement claim, execution, and finalization

**Files:**
- Modify: `Build/Tools/Invoke-Version4AuthoritativePartitionSession.ps1`

**Step 1: Select a claimable partition**

`Run` selects the requested partition; `RunNext` scans manifest order. `available` is claimable;
`failed`/`abandoned` require `-Retry`; `completed` is never silently rerun. Record `running`,
session ID, PID, process start time, report path, command, and claim timestamp before launching.

**Step 2: Run the external command**

Use `System.Diagnostics.Process` with an argument list and redirected output. Export
`VERSION4_PARTITION_ID`, `VERSION4_PARTITION_REPORT_PATH`, and `VERSION4_PARTITION_SESSION_ID` to
the child process. Capture bounded stdout/stderr for the ledger and result without allowing output
pipes to deadlock.

**Step 3: Finalize under the same lock**

Exit code zero changes `running` to `completed`; a nonzero exit or launch exception changes it to
`failed`. Preserve audit details and release the lock in `finally`. Return JSON with status,
partition, report, session, exit code, and bounded output/error.

### Task 5: Run the red-green test suite and harden edge cases

**Files:**
- Modify: `Build/Tools/Test-Version4AuthoritativePartitionSession.ps1`
- Modify: `Build/Tools/Invoke-Version4AuthoritativePartitionSession.ps1`

**Step 1: Run the focused tests after the minimal implementation**

Run the test command from Task 1. Expected: all contract, state, failure, retry, and contention
checks pass.

**Step 2: Add/adjust assertions for state invariants**

Verify at most one `running` record, exactly one final record per invocation, no ledger write on
report validation failure, no completed-to-failed regression, and lock usability after child
failure or runner exception.

**Step 3: Re-run the focused tests and formatting checks**

Run:

```powershell
pwsh -NoProfile -File .\Build\Tools\Test-Version4AuthoritativePartitionSession.ps1
git diff --check -- Build/Tools/Invoke-Version4AuthoritativePartitionSession.ps1 Build/Tools/Test-Version4AuthoritativePartitionSession.ps1 docs/plans/component-decomposition/2026-09-11-version4-authoritative-partition-session-implementation.md
```

Record exit codes, assertion counts, and the temporary test scope. No `dotnet` command is needed.

### Task 6: Review scope and handoff

**Files:**
- Read: `Build/Tools/Invoke-Version4AuthoritativePartitionSession.ps1`
- Read: `Build/Tools/Test-Version4AuthoritativePartitionSession.ps1`
- Read: `docs/plans/component-decomposition/2026-09-11-version4-authoritative-partition-session-design.md`

Check that only the new runner, focused test, and this implementation plan are part of this task's
unstaged changes; preserve all pre-existing user modifications. Confirm that runtime ledger and
lock artifacts remain under `.agent-workplace/state/` and that no report or source file was edited.
