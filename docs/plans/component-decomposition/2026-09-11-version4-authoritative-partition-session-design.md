# Version4 Authoritative Partition Session Design

## Context

The repository contains twenty authoritative Version4 partition reports under
`docs/migration/ledgers/authoritative-20-partitions/`. Multiple sessions may try to
take different reports at the same time. The shared checkout must permit only one short claim or
settlement transaction at a time, while allowing different claimed partitions to be analyzed and
executed in parallel. The reports are read-only inputs for this tool; the tool owns only a private
claim ledger and a checkout-scoped lock.

## Goal

Provide a PowerShell interface that validates, claims, executes, and records one of the twenty
partition reports while ensuring that the same partition cannot be claimed twice. The checkout lock
must be released immediately after a claim is persisted.

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
- `Claim`: require `-PartitionId`, atomically claim the selected available partition, persist
  `running`, and release the lock immediately.
- `ClaimNext`: choose the first claimable partition in P01-P20 order, persist `running`, and
  release the lock immediately.
- `Complete`, `Fail`, `Abandon`: settle a manual claim using its returned `sessionId` under a
  short lock transaction.
- `Run`: claim `-PartitionId`, release the claim lock, run the executable in `-CommandPath` with
  `-CommandArgumentList`, then reacquire the lock only for final state persistence.
- `RunNext`: choose and claim the first available partition in P01-P20 order, then run and settle
  it using the same separated-lock external command interface.

The command also accepts `-LockWaitSeconds`, defaulting to zero for fail-fast contention;
`-Retry` is required to run a failed or abandoned partition again. `-StatePath`, `-LockPath`, and
`-PartitionDirectory` are explicit overrides for isolated tests and controlled tooling use.

For direct PowerShell callers, `-CommandArgumentList` accepts a string array. Callers that launch
the runner as a separate process should use `-CommandArgumentListJson` so child arguments such as
`-Command` are not mistaken for runner parameters by PowerShell's command-line binder.

Example:

```powershell
$childArguments = @('-NoProfile', '-File', '.\\path\\run-p01.ps1')
pwsh -NoProfile -File .\\Build\\Tools\\Invoke-Version4AuthoritativePartitionSession.ps1 `
  -Action Run `
  -PartitionId P01 `
  -CommandPath (Get-Command pwsh).Source `
  -CommandArgumentListJson ($childArguments | ConvertTo-Json -Compress)
```

For a direct invocation from another PowerShell script, `-CommandArgumentList @(...)` may be used
instead.

## State Model

The private ledger is `.agent-workplace/state/version4-authoritative-partition-session.json`.
Each partition transitions through:

```text
available --Claim/Run--> running --Complete/child exit 0--> completed
                       \                    \--Fail/child nonzero--> failed
                        \--Abandon or dead managed owner--> abandoned
failed/abandoned --Retry--> running
```

The ledger records partition ID, report path, expected and observed member count, claim mode,
session ID, optional owner process ID, claim/start/end timestamps, command path, exit code, and
error text. Manual `Claim` records do not have a process owner and remain `running` until the
session explicitly calls `Complete`, `Fail`, or `Abandon`. A managed `Run` entry whose owner
process no longer exists is recorded as `abandoned`; it is not silently treated as completed.

## Lock and Failure Semantics

The script acquires the lock file using `FileMode.OpenOrCreate`, `FileAccess.ReadWrite`, and
`FileShare.None`. The handle remains open only from the start of a claim/settlement transaction
through its ledger write. It is released before manual analysis or an external managed worker
executes. All ledger reads/writes inside those transactions are serialized. The owner metadata is
written to the lock file for diagnosis, but lock ownership is the exclusive handle rather than file
existence.

Ledger writes occur under the lock and use a temporary file followed by replacement where
possible. `try/finally` always disposes the handle. A second invocation waits or returns a
structured busy result only when it overlaps one of these short transactions; it does not become
busy merely because another partition is being analyzed or executed. A missing executable or
nonzero child exit updates the selected managed partition to `failed` during the final short
transaction.

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
- a manual claim records `running`, releases the lock, and allows a different partition claim;
- a duplicate claim for the same partition returns `conflict`;
- `Complete` and `Fail` require the returned session ID;
- a successful child command records `completed` and exit code zero;
- a nonzero child command records `failed` and releases the lock;
- `-Retry` permits an intentional retry of a failed partition;
- two concurrent claims cannot claim the same partition, while different partitions can proceed;
- a dead owner process is recovered as `abandoned` without falsely completing work;
- `RunNext` follows P01-P20 order;
- all report validation failures stop before claim state is written.

No `dotnet` build is required because the implementation is PowerShell-only and does not change
project inputs.
