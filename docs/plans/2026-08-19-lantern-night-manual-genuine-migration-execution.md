# Lantern Night Manual/Genuine ECS Migration Implementation Plan

> task-by-task.

**Goal:** Recover and migrate the server-authoritative distinction between manual and genuine
Lantern Night state without changing the meaning of the already-accepted effective
`IsLanternNight` state, save format, or V1456 WorldData projection by assumption.

**Architecture:** `Terraria.Dome.Simulation` may own immutable Lantern Night state and a named
progression system only after every write path has a source-backed authority contract. The Server
and Protocol layers translate requests and project the effective state only; neither selects a
Lantern Night mode. The existing effective state remains the wire-facing compatibility value until
the source-to-ECS mapping is proven.

**Tech Stack:** .NET 10, C#, executable verifier projects, V1456 compatibility verification,
versioned `DomeStatePersistenceFormat`, serial Release gates using
`-p:UseSharedCompilation=false -p:MSBuildNodeReuse=false`.

---

## 1. Why This Is a Separate Execution Plan

The complete archive implementation has two independent persisted state values:

```csharp
public static bool ManualLanterns;
public static bool GenuineLanterns;
public static bool LanternsUp => GenuineLanterns || ManualLanterns;
```

Their lifecycle differs materially:

| Legacy operation | Manual | Genuine | Effective `LanternsUp` |
|---|---:|---:|---:|
| `ToggleManualLanterns` | flips only manual state | unchanged | changes only if the other mode is false |
| `NaturalAttempt` / next-night schedule | unchanged | becomes true | true |
| `UpdateTime` eligibility failure | retained | cleared | may remain true through manual mode |
| `CheckMorning` | cleared | cleared | false |
| V1456 WorldData bit 11[1] | not sent separately | not sent separately | sent |
| WorldFile v207+ | saved | saved | derived |

Current Dome has one persisted `WorldProgressionState.IsLanternNight` Boolean. It is used by the
accepted schedule path, generic `WorldEventStartCommand(LanternNight)`, Rain suppression and the
V1456 projection. That Boolean has no recorded source-backed origin mode. Mapping it to either
manual or genuine would silently change one of the following:

- whether an eligibility failure clears the active event;
- whether a manual toggle should turn the display off;
- what a v17 Dome snapshot restores after upgrading;
- which existing generic command owns the state transition.

The implementation must therefore begin with authority recovery, not a v18 persistence-field
addition.

## 2. Frozen Source Evidence

Before changing production code, create
`Build/diagnostics/main-migration/task-7-lantern-manual-genuine/<runId>/source-manifest.txt` with
the exact file hashes, line ranges, commands and search results below.

| Evidence | Required range | Fact it establishes |
|---|---|---|
| `D:\TRbackup\无任何删减通过编译\Terraria.GameContent.Events\LanternNight.cs` | 9-172 | field identities, `LanternsUp`, morning clear, genuine-only persistence clear, natural start and manual toggle |
| `D:\TRbackup\Version4物理删除了某些文件\Terraria.IO\WorldFile.cs` | 136-142, 175-178, 1093-1096, 1419-1422, 2370-2382 | persisted field order and v207 defaulting |
| `D:\TRbackup\无任何删减通过编译\Terraria\NetMessage.cs` | 225-336 | bit 11[1] writes effective `LanternsUp`, not a mode |
| `D:\TRbackup\无任何删减通过编译\Terraria\MessageBuffer.cs` | 583-590 | received WorldData writes only the replicated manual value on the client path; it is not evidence for a client-to-server manual-toggle command |
| full archive and Version4 source search | all `ToggleManualLanterns` callers | actual authority of the manual toggle, or an explicit no-caller result |
| current Dome source | `WorldProgressionState`, `WorldProgressionSystem`, `DomeSimulation`, persistence and verifiers | every existing write/read/projection of `IsLanternNight` |

Use a fresh evidence directory and record the commands. The archive search must include both the
complete archive and the physically deleted Version4 tree. A no-caller search result is useful
evidence; it is not permission to invent a player packet or an administrator API.

```powershell
$runId = Get-Date -Format "yyyyMMdd-HHmmss"
$evidence = "Build\diagnostics\main-migration\task-7-lantern-manual-genuine\$runId"
New-Item -ItemType Directory -Force -Path $evidence | Out-Null

Get-FileHash 'D:\TRbackup\无任何删减通过编译\Terraria.GameContent.Events\LanternNight.cs' |
  Tee-Object "$evidence\source-manifest.txt"
rg -n -C 4 'ToggleManualLanterns|ManualLanterns|GenuineLanterns|LanternsUp' `
  'D:\TRbackup\无任何删减通过编译' -g '*.cs' |
  Tee-Object "$evidence\manual-authority-search-archive.txt"
rg -n -C 4 'ToggleManualLanterns|ManualLanterns|GenuineLanterns|LanternsUp' `
  'D:\TRbackup\Version4物理删除了某些文件' -g '*.cs' |
  Tee-Object "$evidence\manual-authority-search-version4.txt"
rg -n -C 3 'IsLanternNight|LanternNight' src Test -g '*.cs' |
  Tee-Object "$evidence\dome-usage.txt"
```

## 3. Non-Negotiable Decision Gate

Task 2 must end in exactly one of these outcomes. It is a required completion condition, not a
question to defer to the next AI turn.

| Outcome | Condition | What the AI does next |
|---|---|---|
| `A. IMPLEMENTABLE_MANUAL_AUTHORITY` | A source-backed server producer identifies who may request a manual toggle, how its identity/permission is checked, and what state is replicated | Execute Tasks 3-7 in this document. |
| `B. MANUAL_AUTHORITY_UNPROVEN` | Searches find no valid server producer, or the only evidence is client-side WorldData consumption | Do not add `ManualLanternNight`, a public toggle queue, a protocol dispatcher or a persistence version. Record `BLOCKED_EXTERNAL_ORACLE`; immediately select the next independent Task 7 behavior from the responsibility ledger. |
| `C. CONFLICTING_EXISTING_DOME_SEMANTICS` | A producer is found but the old Dome `IsLanternNight` field cannot be classified as manual, genuine or a compatibility-only effective state without changing v17 restore behavior | Preserve the existing field and write a migration ADR with concrete alternatives. Do not mutate production state or persistence until an explicit compatibility decision exists. |

`B` is a successful research completion, not a failure of the migration program. The next AI must
continue with another source-backed behavior family rather than repeatedly attempting a manual
toggle API.

## 4. Invariants for Any Approved Implementation

The following are acceptance constraints for Tasks 3-7.

1. `LanternsUp` is an effective derived value. V1456 still receives one bit and no new WorldData
   field or packet length.
2. Manual and genuine state are separately inspectable in immutable Simulation snapshots only when
   their authority is proved.
3. A manual toggle must not clear genuine state. Clearing genuine state for failed eligibility must
   not clear manual state.
4. The day transition clears both modes only when source evidence identifies its phase equivalent
   to legacy `CheckMorning`.
5. Natural/scheduled start writes genuine state only. It must not make a public generic event
   command appear to be the source of a manual toggle.
6. Rejected commands leave progression state, revision and pending command queues unchanged.
7. Historic Dome v17 payloads retain their effective Lantern Night behavior. No mode is assigned
   merely to make an old payload parse.
8. Simulation may not reference `Terraria.Main`, `NetMessage`, `MessageBuffer`, sockets, XNA, UI,
   paths or mutable global arrays.

## 5. Task 1: Establish the Authority and Compatibility Record

**Files:**

- Create: `Build/diagnostics/main-migration/task-7-lantern-manual-genuine/<runId>/source-manifest.txt`
- Create: `Build/diagnostics/main-migration/task-7-lantern-manual-genuine/<runId>/manual-authority-*.txt`
- Modify: `progress.md`
- Modify: `docs/plans/2026-08-19-main-server-ecs-migration-execution.md`
- Test: read-only source and current-Dome inventory commands

### Step 1: Prove every legacy state transition

Record one row for `WorldClear`, `CheckMorning`, `NaturalAttempt`, `ToggleManualLanterns`,
`UpdateTime`, save, load and WorldData projection. For each row record its caller, authority,
inputs, write set, and whether it can be represented by the current ECS.

### Step 2: Trace every current Dome write

Trace the following to their callers and append the result to the source manifest:

```text
WorldProgressionState.WithLanternNight
WorldProgressionSystem.TryStartLanternNight
WorldProgressionSystem.TryStartScheduledLanternNight
DomeSimulation.TryQueueWorldEvent
DomeSimulation.TryQueueNpcGameEventFirstClear
DomeStatePersistenceFormat.WriteWorldProgression
LegacyWorldDataContext / V1456 WorldData assembly
```

### Step 3: Produce the decision-gate result

Write one of `A`, `B`, or `C` from section 3 to `progress.md`, including the source artifact paths
and the reason. Do not write "needs user guidance" while source recovery is still possible.

### Step 4: Verify the documentation boundary

Run:

```powershell
git diff --check
```

Expected: exit `0`. This task changes no C# production source and makes no behavior claim.

### Step 5: Commit

```powershell
git add docs/plans/2026-08-19-lantern-night-manual-genuine-migration-execution.md `
  docs/plans/2026-08-19-main-server-ecs-migration-execution.md progress.md
git commit -m "docs: gate Lantern Night mode migration on authority evidence"
```

Commit only the listed documentation files. Do not stage unrelated dirty worktree changes.

## 6. Task 2: Write RED Only After Outcome A

**Precondition:** Task 1 recorded `A. IMPLEMENTABLE_MANUAL_AUTHORITY`.

**Files:**

- Modify: `Test/Terraria.Dome.WorldRules.Verification/Program.cs`
- Modify: `Test/Terraria.Dome.Persistence.Verification/Program.cs`
- Modify: `Test/Terraria.Dome.Protocol.Compatibility.Verification/Program.cs`
- Test: the three verifier projects above

### Step 1: Add focused behavioral assertions

The verifier must name the recovered authority command rather than assume a command name. It must
prove all of the following before any production implementation exists:

```text
manual=false, genuine=false -> authorized manual toggle -> effective=true
manual=true,  genuine=false -> authorized manual toggle -> effective=false
manual=false, genuine=true  -> authorized manual toggle -> effective=true
manual=true,  genuine=true  -> authorized manual toggle -> effective=true
genuine=true + failed persist eligibility -> manual state unchanged
day transition -> manual=false and genuine=false
rejected authority request -> snapshot and revision unchanged
```

The persistence verifier must also declare the exact pre-v18 behavior. It must fail if the
implementation silently converts a v17 `IsLanternNight=true` payload into either mode without the
Task 1 compatibility record proving that choice.

The protocol verifier must decode the existing WorldData layout and assert bit 11[1] equals the
effective OR result for all four mode combinations, without a length change.

### Step 2: Run and preserve RED

```powershell
dotnet run --project Test\Terraria.Dome.WorldRules.Verification\Terraria.Dome.WorldRules.Verification.csproj `
  -c Release -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false
```

Expected: exit nonzero because the typed state/command/system does not exist yet. Save output as
`world-rules-red.txt`; a failing test caused by an unrelated build issue is not valid RED.

## 7. Task 3: Implement a Minimal, Typed State Contract

**Precondition:** Task 2 RED is valid and Task 1 proves the mode and compatibility mapping.

**Files:**

- Modify: `src/Terraria.Dome.Simulation/World/WorldProgressionState.cs`
- Modify: `src/Terraria.Dome.Simulation/World/Systems/WorldProgressionSystem.cs`
- Modify: `src/Terraria.Dome.Simulation/Simulation/DomeSimulation.cs`
- Create: `src/Terraria.Dome.Simulation/World/<RecoveredManualCommandName>.cs`
- Modify: `Test/Terraria.Dome.WorldRules.Verification/Program.cs`

### Step 1: Add only source-proven state

Use the exact names and values selected by the Task 1 compatibility record. The immutable state
must expose an effective value derived by the equivalent of:

```csharp
public bool IsLanternNight => IsGenuineLanternNight || IsManualLanternNight;
```

Do not retain a second mutable effective Boolean. Every `With...` method must retain both origin
values. Constructor validation must reject any impossible state identified by source evidence, but
must not invent a rule such as "manual and genuine cannot both be true"; legacy permits both.

### Step 2: Establish exactly one authority path

Implement the recovered command as:

```text
typed command -> DomeSimulation validation and deduplication -> private pending queue
-> WorldProgressionSystem named transition -> immutable progression snapshot
```

Only the recovered source-backed authority can enqueue the manual command. `WorldEventStartCommand`
must not be reclassified as manual or genuine unless Task 1 proves that mapping. If it remains a
legacy effective-state compatibility command, document it as such and keep its behavior unchanged.

### Step 3: Preserve source-specific transitions

- Schedule/Natural paths set genuine state only.
- Eligibility failure clears genuine state only and only when the full eligibility snapshot has an
  authoritative owner; until then, retain the currently accepted non-tick behavior.
- The clock transition equivalent to `CheckMorning` clears both modes.
- Rain suppression consumes only the effective derived state.

### Step 4: Run focused GREEN

Run the WorldRules verifier and save `world-rules-final.txt`. Expected: exit `0`; each accepted and
rejected path from Task 2 passes.

## 8. Task 4: Evolve Persistence Without Guessing Old Meaning

**Precondition:** Task 3 is green and the Task 1 compatibility mapping is explicit.

**Files:**

- Modify: `src/Terraria.Dome.Server/Persistence/DomeStatePersistenceFormat.cs`
- Modify: `Test/Terraria.Dome.Persistence.Verification/Program.cs`

### Step 1: Append, never reorder

If and only if two independent values are persisted, increment the Dome format version once and
append the fields after v17's `LanternNightCooldownTicks`. Do not alter byte positions of existing
v1-v17 fields. Reader code must retain strict trailing-data rejection.

### Step 2: Test every compatibility edge

Verify current round-trip for all four manual/genuine combinations, v17 restore according to the
approved compatibility record, and rejection of payloads with an invalid trailing byte sequence.
For an unprovable v17-origin mapping, stop at outcome C rather than choosing a default.

### Step 3: Run persistence GREEN

```powershell
dotnet run --project Test\Terraria.Dome.Persistence.Verification\Terraria.Dome.Persistence.Verification.csproj `
  -c Release -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false
```

Expected: exit `0`, with the old-layout default and strict-tail behavior stated in the verifier.

## 9. Task 5: Preserve Protocol Projection

**Files:**

- Modify only if necessary: `src/Terraria.Dome.Protocol.V1456/Compatibility/LegacyWorldDataContext.cs`
- Modify only if necessary: the existing WorldData assembler
- Modify: `Test/Terraria.Dome.Protocol.Compatibility.Verification/Program.cs`

### Step 1: Keep the wire contract

Map the derived effective value to the existing compatibility context. Do not add a V1456 Manual or
Genuine field, alter packet 7's size, or allow a client decode path to mutate Simulation.

### Step 2: Run protocol GREEN

```powershell
dotnet run --project Test\Terraria.Dome.Protocol.Compatibility.Verification\Terraria.Dome.Protocol.Compatibility.Verification.csproj `
  -c Release -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false
```

Expected: exit `0`; original layout is retained and bit 11[1] equals the OR of source-proven modes.

## 10. Task 6: Final Acceptance Gates and Handoff

Use one fresh evidence directory. Run all commands from repository root, stop at the first new
nonzero exit, and never use `--no-build` as evidence after source changes.

```powershell
$runId = Get-Date -Format "yyyyMMdd-HHmmss"
$evidence = "Build\diagnostics\main-migration\task-7-lantern-manual-genuine\$runId"
New-Item -ItemType Directory -Force -Path $evidence | Out-Null

function Invoke-RecordedDotnet {
  param([string] $name, [string[]] $arguments)
  & dotnet @arguments 2>&1 | Tee-Object "$evidence\$name.txt"
  if ($LASTEXITCODE -ne 0) { throw "$name failed: $LASTEXITCODE" }
}

Invoke-RecordedDotnet 'world-rules-final' @(
  'run', '--project', 'Test\Terraria.Dome.WorldRules.Verification\Terraria.Dome.WorldRules.Verification.csproj',
  '-c', 'Release', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false')
Invoke-RecordedDotnet 'persistence-final' @(
  'run', '--project', 'Test\Terraria.Dome.Persistence.Verification\Terraria.Dome.Persistence.Verification.csproj',
  '-c', 'Release', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false')
Invoke-RecordedDotnet 'world-rules-loopback-final' @(
  'run', '--project', 'Test\Terraria.Dome.WorldRules.Loopback.Verification\Terraria.Dome.WorldRules.Loopback.Verification.csproj',
  '-c', 'Release', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false')
Invoke-RecordedDotnet 'protocol-compatibility-final' @(
  'run', '--project', 'Test\Terraria.Dome.Protocol.Compatibility.Verification\Terraria.Dome.Protocol.Compatibility.Verification.csproj',
  '-c', 'Release', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false')
Invoke-RecordedDotnet 'full-client-bootstrap-final' @(
  'run', '--project', 'Test\Terraria.Dome.FullClientBootstrap.Verification\Terraria.Dome.FullClientBootstrap.Verification.csproj',
  '-c', 'Release', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false')
Invoke-RecordedDotnet 'main-boundary-final' @(
  'run', '--project', 'Test\Terraria.Dome.MainBoundary.Verification\Terraria.Dome.MainBoundary.Verification.csproj',
  '-c', 'Release', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false')
Invoke-RecordedDotnet 'root-release-build-final' @(
  'build', 'Terraria.Dome.sln', '-c', 'Release', '-m:1',
  '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:FixtureHostBuild=false')
```

Append the following record to `progress.md` only after the source tree is unchanged since the
seven gates:

```text
# YYYY-MM-DD Main ECS migration Task 7 Lantern Night manual/genuine <status>

Source: <exact files, hashes and line ranges>
Decision gate: <A, B or C and reason>
Proposition: <one falsifiable sentence>
Authority: <typed command -> validation -> system -> snapshot>
RED: <artifact path, exit code and expected failure>
GREEN: <artifact path and exit code>
Persistence: <format version, v17 policy, strict-tail result>
Protocol: <unchanged V1456 layout and effective bit assertion>
Boundary: <checked source count and violation count>
Release: <exit code, warnings and errors>
Accepted boundary: <only behavior proved by the evidence>
Deferred/Unknown: <unmigrated natural eligibility, random stream, presentation or authority gaps>
Next action: <one independent source-backed behavior family>
```

## 11. Explicit Non-Goals

This plan does not authorize any of the following without a separate source/authority/verifier
slice:

- defaulting absent boss, Pumpkin Moon, Snow Moon or Moon Lord inputs and attaching natural
  Lantern Night attempts to the tick;
- reproducing `Main.rand.Next(14)` or `Main.rand.Next(5, 11)` with an unspecified random stream;
- copying SkyManager, sound, UI, client WorldData consumption or `NetMessage.SendData` into
  Simulation;
- exposing manual Lantern Night as a network client command merely because the legacy client can
  receive a Manual bit;
- reclassifying the existing generic `WorldEventStartCommand(LanternNight)` by assumption;
- claiming Task 7 or the Main ECS migration is complete after this slice.
