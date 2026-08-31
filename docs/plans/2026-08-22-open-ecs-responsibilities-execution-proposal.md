# Open ECS Responsibilities Execution Proposal


**Goal:** Close the remaining open responsibilities in the Main-to-ECS migration by first building source-backed authority models, then implementing only the branches that have a unique owner and deterministic verification contract.

**Architecture:** Keep `Terraria.Dome.Simulation` authoritative for immutable gameplay snapshots, typed commands and named tick phases. Keep WLD parsing/persistence and protocol projection in their existing Server/Compatibility/Protocol boundaries. Mixed legacy responsibilities are split by owner; unresolved source semantics remain explicitly `unknown` or `deferred` rather than being normalized to defaults.

**Tech Stack:** .NET 10, C#, Roslyn/source inspection, immutable records, Arch ECS, executable verifier projects, WLD V319 fixtures, PowerShell. All .NET commands run from `D:\TRbackup\NLTX` with `-p:UseSharedCompilation=false -p:MSBuildNodeReuse=false`.

---

## Operating Constraints

1. Read `AGENTS.md`, `约束/Google-CSharp-Style-Guide-约束.md`, this proposal, and the exact Version4 source member before each code change.
2. Use `apply_patch` for edits. Preserve unrelated dirty changes and generated evidence.
3. Use focused verification only, targeting approximately 40% of the former test workload. Do not run the full regression matrix, complete client/runtime suites, duplicate replay matrix, or root Release build unless the user expands the validation scope.
4. Every card must produce a fresh artifact under `Build/diagnostics/main-migration/<card>/<run-id>/` containing source anchors, accepted/deferred predicates, exact command and exit code.
5. A card may move from `planned`/`unknown` to `accepted` only when its source oracle, authority owner, focused RED/GREEN evidence, persistence/projection effects and scoped `git diff --check` agree.
6. Never cast, truncate, round, default or silently normalize a legacy value when its source-visible semantics are not proven.

## Current Open Scope

The coverage matrix currently has 30 rows: 17 accepted narrowly, 7 excluded, 1 planned and 5 unknown. The open rows are M-001, M-007, M-008, M-009, M-014 and M-024. Existing evidence and prohibitions are recorded in:

- `docs/research/2026-08-20-main-member-coverage-matrix.md`
- `docs/research/2026-08-22-main-initialize-almost-everything-disposition.md`
- `docs/research/2026-08-22-main-queue-caller-inventory.md`
- `Build/diagnostics/main-migration/task-9-acceptance-review/20260823-070000/remaining-work-queue.txt`

## Task 1: Split `Initialize_AlmostEverything` by Owner

**Files:**

- Read: `D:\TRbackup\Version4物理删除了某些文件\Terraria\Main.cs:3732-3858`
- Read/modify: `docs/research/2026-08-22-main-initialize-almost-everything-disposition.md`
- Create: `docs/research/2026-08-22-initialize-almost-everything-owner-matrix.md`
- Create: `Build/diagnostics/main-migration/task-9-main-member-coverage/<run-id>/initializer-owner-matrix.yaml`
- Modify: `docs/research/2026-08-20-main-member-coverage-matrix.md` only after an individual family is accepted

**Step 1: Inventory every call in source order.** Record the source line, side effects, mutable globals read/written, lifecycle, candidate owner (`Simulation`, `Server`, `Protocol`, `Client`, or `excluded`) and whether an existing accepted card covers it.

**Step 2: Write the RED accounting gate.** The gate must fail if a server-relevant call family has no owner, source anchor, accepted predicate or explicit deferred reason. It must allow acknowledged client/content families to remain excluded.

**Step 3: Run the accounting gate.**

```powershell
dotnet run --project Test\Terraria.Dome.MainBoundary.Verification\Terraria.Dome.MainBoundary.Verification.csproj -c Release -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false
```

Expected: exit `0` only when all rows use `accepted`, `planned`, `unknown` or `excluded`; this does not accept the entire initializer.

**Step 4: Select one unresolved family.** Do not implement the aggregate initializer. Select only a family with a unique owner and a falsifiable source predicate; otherwise leave M-001 `planned`.

**Step 5: Record the decision.** Update the owner matrix, the coverage matrix and `progress.md`; run `git diff --check` and preserve the unresolved families.

## Task 2: Decide the WLD Fractional Clock Contract (M-007)

**Files:**

- Read: `D:\TRbackup\Version4物理删除了某些文件\Terraria\IO\WorldFile.cs:1312-1313,2125-2126`
- Read: `D:\TRbackup\Version4物理删除了某些文件\Terraria\Main.cs:13525-13527`
- Read/modify: `docs/plans/2026-08-22-wld-clock-fraction-boundary.md`
- Read/modify: `src/Terraria.Dome.Simulation/World/WorldClock.cs`
- Read/modify: `src/Terraria.Dome.Simulation/World/WorldClockSnapshot.cs`
- Modify later: `src/Terraria.Dome.Server/Persistence/DomeStatePersistenceFormat.cs`
- Test: `Test/Terraria.Dome.WorldClock.Verification/Program.cs`
- Test: `Test/Terraria.WorldFile.V319.Verification/Program.cs`

**Step 1: Establish the RED fixture.** Add a WLD fixture with a non-integral saved `Double` time and assert that the current `Int32` clock cannot round-trip it without loss.

**Step 2: Audit the source contract.** Search all supported source versions for proof that server-visible saved times are integral. Record the version, source lines and counterexamples, if any.

**Step 3: Choose one of two outcomes.**

- If integrality is proven, implement a documented integral import predicate and boundary test.
- Otherwise design a versioned fractional clock snapshot with explicit day/night, persistence and V1456 projection semantics.

**Step 4: Implement only after the decision is source-backed.** Do not use `(int)time`, `Math.Round`, or a silent rejection threshold.

**Step 5: Run focused verification.**

```powershell
dotnet run --project Test\Terraria.Dome.WorldClock.Verification\Terraria.Dome.WorldClock.Verification.csproj -c Release -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false
dotnet run --project Test\Terraria.WorldFile.V319.Verification\Terraria.WorldFile.V319.Verification.csproj -c Release -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false
```

Expected: exit `0` for the chosen representation and explicit fractional fixture; otherwise retain M-007 as `unknown`.

## Task 3: Complete the WLD Rain Raw-to-Runtime Model (M-008)

**Files:**

- Read: `D:\TRbackup\Version4物理删除了某些文件\Terraria\Main.cs` members `UpdateWeather`, `StartRain`, `StopRain`, `ChangeRain`, `FixEndlessRainWorlds`
- Read: `D:\TRbackup\Version4物理删除了某些文件\Terraria\IO\WorldFile.cs` rain fields and version branches
- Read/modify: `docs/plans/2026-08-22-wld-rain-runtime-boundary.md`
- Modify: `src/Terraria.WorldFile.V319/Model/LegacyWorldMetadata.cs`
- Modify: `src/Terraria.WorldCompatibility/Model/CompatibilityWorldMetadata.cs`
- Modify: `src/Terraria.Dome.Simulation/World/WorldRuleState.cs` only if a unique runtime owner is proven
- Modify: `src/Terraria.Dome.Server/Persistence/WorldPersistenceFormat.cs` only if the owner changes
- Test: `Test/Terraria.WorldFile.V319.Verification/Program.cs`
- Test: `Test/Terraria.Dome.WorldImport.Verification/Program.cs`
- Test: `Test/Terraria.Dome.WorldRules.Verification/Program.cs`

**Step 1: Build the raw-state RED matrix.** Cover independent `raining`, `rainTime`, `maxRaining`, old-version absence and invalid combinations.

**Step 2: Trace every `FixEndlessRainWorlds()` branch.** For each branch record version, secret-seed dependency, required source state and whether the current snapshot carries it.

**Step 3: Define the runtime mapping.** Specify units, bounds, current-vs-target strength, duration semantics, restart behavior and rejection rules. Any branch without a source-backed mapping remains deferred.

**Step 4: Implement only direct lossless restoration.** Do not map `maxRaining` to an unrelated current-strength field or default old layouts to zero.

**Step 5: Run focused parser/import/rules verification.** Expected: valid independent triples round-trip; invalid or unsupported repair branches reject explicitly.

## Task 4: Resolve Invasion Travel Authority (M-009)

**Files:**

- Read: `D:\TRbackup\Version4物理删除了某些文件\Terraria\Main.cs` `StartInvasion`, `UpdateInvasion`, `UpdateTime*`
- Read: `docs/plans/2026-08-22-invasion-travel-integration-boundary.md`
- Modify only after decision: `src/Terraria.Dome.Simulation/World/WorldProgressionSystem.cs`
- Modify only after decision: `src/Terraria.Dome.Simulation/World/WorldClock.cs`
- Test: `Test/Terraria.Dome.WorldRules.Verification/Program.cs`
- Test: `Test/Terraria.Dome.TickOrder.Verification/Program.cs`

**Step 1: Capture a RED travel trace.** Include normal rate, pause, fast-forward, sleeping-player acceleration, menu and dawn boundary cases.

**Step 2: Compare source `Main.dayRate` with ECS clock inputs.** Prove an equivalent authority or document why no equivalence exists.

**Step 3: If equivalent, add a typed travel-rate snapshot field.** Persist it and expose the exact event-phase consumer.

**Step 4: If not equivalent, retain the existing narrow invasion guards/progression and keep full travel integration `unknown`.** Do not substitute `WorldClock.TicksPerUpdate`.

**Step 5: Run only the focused WorldRules/TickOrder checks.** Expected: travel snapshots are deterministic and pause/rate transitions are source-backed.

## Task 5: Split `MainThreadAction` Callers (M-014)

**Files:**

- Read: `D:\TRbackup\Version4物理删除了某些文件\Terraria\Main.cs` `QueueMainThreadAction` and `ConsumeAllMainThreadActions`
- Read: `D:\TRbackup\Version4物理删除了某些文件\Terraria\WorldGen.cs:10529,26122`
- Read/modify: `docs/research/2026-08-22-main-queue-caller-inventory.md`
- Create: `docs/research/2026-08-23-main-thread-action-contracts.md`
- Create/modify: typed command definitions under `src/Terraria.Dome.Simulation/Commands/` only for a proven Simulation-owned caller
- Test: a focused verifier for each selected typed command

**Step 1: Assign each caller a domain owner.** Record inputs, output effects, phase, ordering, cancellation, retry, persistence and protocol visibility.

**Step 2: Write a RED contract test.** The test must reject arbitrary delegates and require a typed command identity.

**Step 3: Implement one caller-specific command.** Keep section/UI host work in Server/Protocol and background generation continuation in its own owner unless source proves a Simulation route.

**Step 4: Verify deterministic ordering and rejection.** Use `TickOrder.Verification` or a new small verifier; do not add `Queue<Action>`.

**Step 5: Accept only the selected caller.** M-014 remains `unknown` for all other arbitrary callers.

## Task 6: Model Delayed `IEnumerator` Semantics (M-024)

**Files:**

- Read: `D:\TRbackup\Version4物理删除了某些文件\Terraria\Main.cs` `DelayedProcesses`, `DelayedProcessesInGame` and their loop advancement
- Read: `docs/research/2026-08-22-main-queue-caller-inventory.md`
- Create: `docs/research/2026-08-23-delayed-process-contract-matrix.md`
- Create: `Build/diagnostics/main-migration/task-9-delayed-process-semantics-boundary/<run-id>/contract-matrix.yaml`
- Create/modify: typed process state under `src/Terraria.Dome.Simulation/` only after a caller contract is accepted
- Test: a focused phase/cancellation/restart verifier for each selected process

**Step 1: Inventory every reachable caller.** Include external/plugin possibility because the lists are public mutable collections.

**Step 2: Define process identity and state.** Specify owner, phase, resume condition, pause behavior, cancellation, disconnect, persistence and deterministic replay identity.

**Step 3: Write RED tests for ambiguous semantics.** Show why an untyped `IEnumerator` or delegate queue cannot satisfy snapshot/replay requirements.

**Step 4: Select one source-backed process family.** Implement a typed state machine, not a generic coroutine queue.

**Step 5: Verify phase behavior and restart continuation.** If no process family has a complete contract, retain M-024 `unknown/deferred`.

## Task 7: Batch Acceptance Review

**Files:**

- Modify: `docs/research/2026-08-20-main-member-coverage-matrix.md`
- Modify: `progress.md`
- Create: `Build/diagnostics/main-migration/task-9-acceptance-review/<run-id>/status.txt`

**Step 1: Collect evidence.** Include source hash/line anchors, scenario, RED/GREEN output, affected project build, MainBoundary output and scoped diff check.

**Step 2: Audit claim scope.** Ensure accepted rows identify the exact predicate; ensure planned/unknown/deferred rows retain their blocker.

**Step 3: Run focused gates only.** Use the single verifier for the changed card, one affected-project build when needed, MainBoundary and `git diff --check`.

**Step 4: Update matrix and progress.** Never report an accounting score as a migration percentage.

**Step 5: Stop at unresolved prerequisites.** Return to the relevant modeling task rather than inventing a default implementation.

## Definition of Done

This proposal is complete only when every open row has one of these evidence-backed outcomes:

- `accepted`: unique owner, source-backed behavior, focused verifier, persistence/projection effects and affected build are green;
- `unknown`: source semantics or authority remain unavailable, with a recorded reason and no guessed implementation;
- `excluded`: responsibility is outside server ECS scope with an explicit owner boundary.

M-001/M-007/M-008/M-009/M-014/M-024 must not be marked accepted merely because their focused tests compile or because the completion manifest score increases. The current reduced validation policy remains approximately 40% until the user explicitly changes it.
