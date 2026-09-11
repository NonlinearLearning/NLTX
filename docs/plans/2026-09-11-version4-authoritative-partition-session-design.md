# Version4 Authoritative Partition Session Design

## Context

The repository contains twenty authoritative Version4 partition reports under
`docs/迁移参考表/Version4权威模拟系统字段属性逐成员源码声明-20分区/`. Multiple sessions may try to
take different reports at the same time, but the shared checkout must permit only one active
claim-and-execution session. The reports are read-only inputs for this tool; the tool owns only a
private claim ledger and a checkout-scoped lock.

## Goal

Provide a PowerShell interface that validates, claims, executes, and records one of the twenty
partition reports while ensuring that only one invocation is active for the checkout at a time.

## Options Considered

1. A sentinel lock file checked with `Test-Path` is simple but racy, and a killed process can leave
   a stale file that blocks every later session.
2. A named Windows mutex releases automatically when the process exits, but its scope is tied to
   the host and requires careful naming to distinguish checkouts.
3. An exclusively opened lock file is scoped to the actual checkout path, is available to all
   PowerShell processes, and the operating system releases the handle after process termination.
   This is the selected approach. The file remains as a harmless metadata file, while the open
   handle is the lock.

## Interface

Create `Build/Tools/Invoke-Version4AuthoritativePartitionSession.ps1` with these actions:

- `List`: validate the fixed twenty-report manifest and print each partition's state.
- `Run`: require `-PartitionId`, atomically claim the selected available partition, run the
  executable in `-CommandPath` with `-CommandArgumentList`, and record the exit result.
- `RunNext`: choose the first available partition in P01-P20 order, then run it using the same
  external command interface.

The command also accepts `-LockWaitSeconds`, defaulting to zero for fail-fast contention;
`-Retry` is required to run a failed or abandoned partition again. `-StatePath`, `-LockPath`, and
`-PartitionDirectory` are explicit overrides for isolated tests and controlled tooling use.

Example:

```powershell
pwsh -NoProfile -File .\\Build\\Tools\\Invoke-Version4AuthoritativePartitionSession.ps1 `
  -Action Run `
  -PartitionId P01 `
  -CommandPath pwsh `
  -CommandArgumentList @('-NoProfile', '-File', '.\\path\\run-p01.ps1')
```

## State Model

The private ledger is `.agent-workplace/state/version4-authoritative-partition-session.json`.
Each partition transitions through:

```text
available -> running -> completed
                     \-> failed
running (dead owner) -> abandoned -> available with -Retry
```

The ledger records partition ID, report path, expected and observed member count, session ID,
owner process ID, owner start time, claim/start/end timestamps, command path, exit code, and error
text. A `running` entry whose owner process no longer exists is recorded as `abandoned`; it is not
silently treated as completed. A failed command retains its audit record and releases the lock.

## Lock and Failure Semantics

The script acquires the lock file using `FileMode.OpenOrCreate`, `FileAccess.ReadWrite`, and
`FileShare.None`. The handle remains open from partition selection through the external command
and final ledger write. All actions use the same lock so state reads cannot observe a concurrent
write. The owner metadata is written to the lock file for diagnosis, but lock ownership is the
exclusive handle rather than file existence.

Ledger writes occur under the lock and use a temporary file followed by replacement where
possible. `try/finally` always disposes the handle. A second invocation returns a structured busy
result and a nonzero exit code unless it is configured to wait. A missing executable or nonzero
child exit updates the selected partition to `failed` before returning the corresponding failure.

## Validation

The manifest contains the twenty fixed report paths and expected member totals: 113, 98, 163, 112,
164, 254, 112, 100, 105, 118, 164, 189, 106, 117, 126, 133, 146, 136, 84, and 119. Before
claiming, the tool verifies the selected report exists, contains exactly the expected number of
field/property member rows, and contains no ID-class source row. It does not alter the report or
run C# compilation.

## Compatibility and Scope

The tool does not modify Version4 source code, `src/`, the formal subsystem index, coverage TSV,
or partition report contents. Existing reports remain valid inputs. The JSON ledger and lock are
private process artifacts under `.agent-workplace/` and are intentionally ignored by Git.

## Verification Plan

The focused PowerShell test will use a temporary state and lock path, then verify:

- the manifest has all twenty expected reports and counts;
- a successful child command records `completed` and exit code zero;
- a nonzero child command records `failed` and releases the lock;
- `-Retry` permits an intentional retry of a failed partition;
- two concurrent invocations cannot both enter execution;
- a dead owner process is recovered as `abandoned` without falsely completing work;
- `RunNext` follows P01-P20 order;
- all report validation failures stop before claim state is written.

No `dotnet` build is required because the implementation is PowerShell-only and does not change
project inputs.
