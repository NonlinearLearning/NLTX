# Remaining ECS Migration Execution Plan

> **For Claude:** REQUIRED SUB-SKILL: Use `executing-plans` to implement this plan one scenario
> card at a time. Do not combine batches or mark Task 9 complete from any individual card.

**Goal:** Continue the Terraria server ECS migration from the accepted WLD/import slices toward
source-backed, authoritative simulation behavior without reintroducing legacy `Main` ownership.

**Architecture:** Every batch follows Task 0: source oracle -> scenario card -> real RED -> narrow
authority route -> focused GREEN -> affected gates -> two deterministic replays -> MainBoundary ->
serial root Release build -> independent acceptance. `Terraria.Dome.Simulation` owns gameplay;
Server owns WLD I/O, persistence orchestration and protocol projection. Unknown legacy behavior
remains explicitly unavailable, deferred or fail-closed.

**Tech Stack:** .NET 10, C#, Arch ECS, immutable snapshots, executable verification projects,
deterministic fixture/replay traces, PowerShell, `Terraria.Dome.sln`.

---

## Operating Rules

Run all .NET commands at repository root with:

```powershell
-p:UseSharedCompilation=false -p:MSBuildNodeReuse=false
```

Before each code change, read:

- `AGENTS.md`
- `约束/Google-CSharp-Style-Guide-约束.md`
- `docs/plans/2026-08-20-task-0-ecs-migration-execution.md`
- the exact source member in `D:\TRbackup\Version4物理删除了某些文件`

For every card, create:

```text
Build/diagnostics/main-migration/task-<n>-<behavior>/<run-id>/scenario.yaml
```

The card records source path/hash/line scope, accepted and rejected outcomes, ownership route,
fixtures, exact deferred branches and gates. A batch may be accepted only after the final source
tree passes its focused verifier, affected regressions, two clean deterministic replays,
MainBoundary, scoped `git diff --check`, and the serial root Release build.

## Phase 0: Close the Current Meteor Import Batch

### Task 0.1: Record Task 9 WLD pending-meteor acceptance

**Files:**

- Modify: `Build/diagnostics/main-migration/task-9-wld-meteor-schedule-import/20260820-000004/scenario.yaml`
- Modify: `docs/plans/2026-08-19-main-server-ecs-migration-execution.md`
- Modify: `progress.md`

**Steps:**

1. Preserve the existing direct route:
   `WorldGen.spawnMeteor -> LegacyWorldMetadata -> CompatibilityWorldMetadata ->
   WorldProgressionState.IsMeteorScheduled`.
2. Record the already completed focused parser, WorldImport, Persistence, Protocol Compatibility,
   twice-run WorldRules, MainBoundary, and root Release evidence.
3. Set the card to `accepted` only after the post-document scoped diff check exits zero.
4. State that acceptance is limited to restoring the saved pending schedule flag.
5. Keep `shadowOrbSmashed`, orb/altar counters, random scheduling, landing search, impact behavior
   and client ambience deferred.

**Acceptance:** no source logic changes; the evidence card names MainBoundary `534` files with zero
violations and the serial Release build has zero warnings and errors.

## Phase 1: Finish Direct, Lossless WLD State Recovery

The next cards are limited to saved fields that have a unique authoritative destination. Do not
use a zero/default value when old layouts lack a source-backed equivalent; model absence explicitly.

### Task 1.1: Build a WLD header disposition matrix

**Files:**

- Create: `docs/research/2026-08-20-wld-header-disposition-matrix.md`
- Create: `Build/diagnostics/main-migration/task-9-wld-header-matrix/<run-id>/scenario.yaml`
- Read: `Terraria.IO/WorldFile.cs` in the physical-deletion oracle
- Read: `src/Terraria.WorldFile.V319/Format/WldHeaderReader.cs`
- Read: `src/Terraria.WorldFile.V319/Format/WldLegacyV1ToV87Reader.cs`

**Steps:**

1. Enumerate every source Header read in wire order, including its first/last WLD version.
2. For each field, classify `accepted`, `candidate`, `derived`, `no-owner`, or `client-only`.
3. Map each accepted/candidate field to exactly one immutable metadata, rule, progression, clock or
   world-object owner; no destination means `no-owner`.
4. Mark rain, wind, clock time/day-time, ore tiers, orb counters and seed-dependent effects as
   blocked until their semantic prerequisites exist.
5. Add a verifier-only fixture offset assertion for each direct candidate before starting its
   implementation card.

**Acceptance:** a reviewable matrix prevents accidental field consumption, layout drift and
overlapping ownership. It does not add gameplay code or claim state recovery.

### Task 1.2: Import one directly owned world identity field per card

**Candidate order:** world ID/name/generator version, then only those source Header fields whose
authoritative destination already exists and is semantically exact.

**Files per selected card:**

- Modify: `src/Terraria.WorldFile.V319/Model/LegacyWorldMetadata.cs`
- Modify: `src/Terraria.WorldCompatibility/Model/CompatibilityWorldMetadata.cs`
- Modify: `src/Terraria.WorldFile.V319/Format/WldHeaderReader.cs`
- Modify: `src/Terraria.WorldFile.V319/Format/WldLegacyV1ToV87Reader.cs` when the historical layout contains it
- Modify: `src/Terraria.WorldCompatibility/Projection/CompatibilityToDomeProjection.cs`
- Modify: the owning Simulation state/persistence type only when the value affects an immutable snapshot
- Test: `Test/Terraria.WorldFile.V319.Verification/Program.cs`
- Test: `Test/Terraria.Dome.WorldImport.Verification/Program.cs`

**Steps:**

1. Select exactly one field from the disposition matrix and make a version-boundary parser test RED.
2. Add a source-position fixture value and run the parser before production changes.
3. Add the smallest immutable metadata/property route with authoritative validation.
4. Project it into the existing owner without deriving it from seed, tiles, GameMode or gameplay state.
5. Add persistence migration only when the owner is included in Dome snapshots; old snapshots restore
   `unknown`/the documented legacy-compatible value, never a fabricated fact.
6. Run parser matrix, recorded oracle, WorldImport, Persistence if applicable, protocol if exposed,
   two replays, MainBoundary and root Release.

**Stop condition:** if a field has more than one plausible owner, reads differently by source branch,
or needs a new runtime behavior, defer it to the relevant phase below.

## Phase 2: Repair State-Model Gaps Before Importing Rain, Wind or Time

### Task 2.1: Lossless weather persistence model

**Files:**

- Create: `docs/plans/2026-08-20-weather-state-model-design.md`
- Create: `Build/diagnostics/main-migration/task-9-weather-state-model/<run-id>/scenario.yaml`
- Modify later: `src/Terraria.Dome.Simulation/World/WorldRuleState.cs`
- Modify later: `src/Terraria.Dome.Server/Persistence/DomeStatePersistenceFormat.cs`
- Test later: `Test/Terraria.Dome.WorldRules.Verification/Program.cs`
- Test later: `Test/Terraria.Dome.Persistence.Verification/Program.cs`

**Steps:**

1. Capture a RED proving the current rain representation cannot distinguish source states with the
   same duration but different `raining`/`maxRaining` facts.
2. Specify an immutable raw-weather snapshot that can represent all three WLD values independently.
3. Define validation invariants and every permitted conversion to the existing runtime weather
   systems; invalid combinations must reject rather than normalize silently.
4. Version persistence append-only and prove old-format recovery separately.
5. Run restore/replay continuation tests twice, including an invalid raw-state rejection case.

**Acceptance:** this card changes representation only. It must not import WLD rain or alter random
weather transitions until the next card.

### Task 2.2: WLD rain state import after Task 2.1

**Source facts:** source saves independent `raining`, `rainTime`, `maxRaining` and calls
`FixEndlessRainWorlds()` on load.

**Steps:**

1. Record exact `FixEndlessRainWorlds()` branches and all version/secret-seed dependencies.
2. Make RED fixtures for a normal independent triple, a rejected invalid triple, and each source
   repair branch with an available oracle.
3. Implement only the source-backed raw-state restoration and explicit repair rule.
4. Defer any repair branch whose required seed/state source is unavailable; do not approximate it.
5. Prove import, persistence/restart, deterministic continuation and WorldData projection.

**Stop condition:** a source repair branch without a compatible seed/definition oracle blocks that
branch, not the entire weather model.

### Task 2.3: Authoritative clock representation decision

**Files:**

- Create: `docs/plans/2026-08-20-world-clock-representation-decision.md`
- Create: `Build/diagnostics/main-migration/task-9-world-clock-representation/<run-id>/scenario.yaml`
- Read: legacy `Terraria/Main.cs` time update members and `Terraria.IO/WorldFile.cs`
- Read: `src/Terraria.Dome.Simulation/World/WorldClock*.cs`

**Steps:**

1. Trace every source use of `Main.time` and determine whether fractional values are observable on
   server persistence, packets or event boundaries.
2. Make RED from a fractional source value that cannot round-trip through current `Int32` time.
3. Choose one explicitly documented solution: widen the authoritative clock representation, or prove
   the server-visible source contract is integral for the supported version.
4. Version snapshot/persistence and protocol projection only after that choice is source-backed.
5. Replay day/night transition, pause, load/restart and event-boundary cases twice.

**Acceptance:** no WLD `time`/`dayTime` import before this decision passes.

## Phase 3: Randomness Ownership Before Wind and Probabilistic Events

### Task 3.1: Domain-scoped deterministic random-stream contract

**Files:**

- Create: `docs/plans/2026-08-20-simulation-random-stream-contract.md`
- Create: `Build/diagnostics/main-migration/task-9-random-stream-contract/<run-id>/scenario.yaml`
- Modify later: `src/Terraria.Dome.Simulation/World/` random-state owner and snapshot integration
- Test later: `Test/Terraria.Dome.WorldRules.Verification/Program.cs`
- Test later: `Test/Terraria.Dome.Persistence.Verification/Program.cs`

**Steps:**

1. Inventory current random abstractions and all supported event-system call sites.
2. Make RED showing identical snapshot plus same trace does not preserve the next random decision
   across save/restart, if that is currently true.
3. Define named domain streams with stable seeds/state, persisted snapshot values and no dependency on
   client-only calls or `Main.rand` ordering.
4. Add a fixed-seed test that replays accepted commands, persists mid-trace, restores, and produces
   identical subsequent command/snapshot hashes.
5. Document that exact global `Main.rand` call-order parity remains deferred unless an executable
   source trace proves it.

**Acceptance:** deterministic server streams for supported domains, not a claim of global legacy
random-order compatibility.

### Task 3.2: WLD wind import

**Source facts:** v62+ saves wind target/current compatibility state; older layouts call
`WorldGen.RandomizeWeather()`.

**Steps:**

1. Create a card with separate v1-v61 and v62+ accepted/rejected predicates.
2. Make RED for v62+ direct target restoration and its source-defined current relation.
3. For v1-v61, use the Task 3.1 server weather stream only when its algorithm/seed relation is
   source-backed; otherwise mark that historical branch `unknown` and reject/defer WLD import.
4. Do not default historical wind to zero.
5. Prove source-version matrix, persistence, rain-adjusted wind behavior, two replays and protocol
   projection.

**Acceptance:** direct modern restoration and only demonstrably compatible old-layout behavior.

### Task 3.3: Probabilistic meteor scheduling, if source trace is recoverable

**Precondition:** Task 3.1 and a source-backed event eligibility/random branch trace.

**Steps:**

1. Keep existing WLD `IsMeteorScheduled` import independent and accepted.
2. Write RED for a seeded, qualified scheduling window and rejected ineligible window.
3. Add a world-event command/system that schedules the flag; do not combine it with landing search
   or impact placement.
4. Prove save/reload does not reschedule or consume random state twice.
5. Defer the card when the required legacy random ordering cannot be reproduced or bounded.

## Phase 4: World Events as Separate State Machines

Each event has its own card. Event starts are validated commands or source-backed scheduled inputs;
systems only mutate immutable world rule/progression state during their documented tick phase.

### Task 4.1: Invasion lifecycle beyond imported type/size

**Scope:** the lifecycle owner for delay, X position, start size, spawn/clear/recovery transitions.

**Steps:**

1. Trace source transitions and identify which fields are persistent facts versus derived runtime values.
2. Write accepted start/progress/end and rejected invalid-order/duplicate tests.
3. Add exactly one command and one deterministic progression system/commit route.
4. Add only required WLD/persistence fields after their runtime meaning is fixed.
5. Exercise two-session replication/PVS when visible world state is emitted.

**Excluded:** invasion NPC spawn tables and client announcements unless separately sourced.

### Task 4.2: Lantern Night, Slime Rain and remaining weather/event consumers

**Scope:** audit existing partial systems against their original source members, one event branch per
card.

**Steps:**

1. Produce a member-to-system coverage table, marking every unsupported guard/side effect.
2. Select one missing guard or transition with a falsifiable source predicate.
3. Add RED, minimal authoritative state transition, persistence if needed, replay and loopback tests.
4. Leave all source branches with client/UI/NPC-table dependencies explicitly deferred.

## Phase 5: Replace `Main.cs` Ownership by Domain, Not by File Sections

### Task 5.1: Convert the Main responsibility ledger into executable coverage

**Files:**

- Modify: `docs/migrations/main-server-responsibility-ledger.md`
- Create: `docs/research/2026-08-20-main-member-coverage-matrix.md`
- Create: `Test/Terraria.Dome.MainBoundary.Verification/` coverage verifier only if it can remain
  independent of forbidden Simulation dependencies

**Steps:**

1. Partition the 13,996-line `Main.cs` by ownership: world clock/rules, entities, event scheduling,
   persistence, protocol/server orchestration, and permanently client-only state.
2. Assign every server-relevant member exactly one status: accepted card, planned card, unknown,
   blocked, or intentionally excluded.
3. Require each accepted card to cite its source member lines and ECS authority route.
4. Add a report gate that fails only on unclassified server-relevant members, not on acknowledged
   client-only members.
5. Do not copy `Main` fields or static methods into Simulation.

**Acceptance:** transparent coverage accounting, not gameplay parity.

### Task 5.2: Complete entity ownership in dependency order

Execute distinct cards in this order:

1. Player command validation, input sequencing, movement/collision and lifecycle gaps.
2. NPC spawn eligibility, stable identity, AI families, combat/death/loot and replication gaps.
3. Projectile spawn/ownership, motion/collision, damage, lifetime and replication gaps.
4. Item inventory/world-item lifecycle, use, placement, drop and persistence gaps.
5. World-object and tile changes only through deterministic tile/object command commits.

For every entity card, require an accepted trace plus forged/duplicate/invalid rejection trace,
stable identity checks, deterministic replay, persistence where the entity survives restart, and
two-session loopback/PVS evidence where replication changes.

### Task 5.3: Complete tick-order ownership

**Files:**

- Read/modify: the current Simulation tick pipeline and its verifier
- Test: `Test/Terraria.Dome.TickOrder.Verification/Program.cs`

**Steps:**

1. Record the source-derived phase order for each selected domain rather than importing Main's whole
   loop order.
2. Add a RED assertion for one ordering invariant at a time, such as clock cleanup before event
   eligibility or entity death before loot replication.
3. Register named systems in the minimal phase sequence and expose no mutable cross-phase globals.
4. Replay same-tick competing commands twice and compare ordered commit/snapshot hashes.
5. Treat a phase-order conflict as an architecture decision with source evidence, not a test-order tweak.

## Phase 6: Acceptance and Release Readiness

### Task 6.1: Independent acceptance review per batch

**Files:**

- Modify: current card only after final-source evidence exists
- Modify: `progress.md`
- Modify: `docs/plans/2026-08-19-main-server-ecs-migration-execution.md`

**Steps:**

1. Capture `worktree.txt`, reference hash, source manifest, focused RED/final outputs, two replay
   outputs, affected gate outputs, `main-boundary-final.txt`, root build output and diff check.
2. Use a fresh evaluator pass to validate that the card, source hash, tests and final tree agree.
3. Mark `accepted` only when every required gate is present and exits zero.
4. Append the precise accepted predicate, deferred branches and next candidate card to `progress.md`.
5. Never convert an accepted narrow slice into a domain-complete percentage claim.

## Proposed Queue and Dependencies

| Order | Card | Depends on | Risk | Result |
|---:|---|---|---|---|
| 0 | Close WLD pending-meteor evidence | existing green gates | low | accepted documentation only |
| 1 | WLD Header disposition matrix | none | low | prevents unsound field imports |
| 2 | Direct identity metadata fields | matrix | low-medium | exact source state recovery |
| 3 | Lossless weather state model | none | medium | can represent rain without conflation |
| 4 | WLD rain import | weather model + source repairs | high | exact or explicitly partial restoration |
| 5 | Clock representation decision | source time trace | high | enables/blocks time WLD import |
| 6 | Random-stream contract | snapshot/persistence | high | restart-stable supported randomness |
| 7 | WLD wind import | random contract for v1-v61 | high | version-correct direct/derived import |
| 8 | Invasion lifecycle | imported type/size | high | authoritative transitions |
| 9 | Event guard/transition cards | event-specific source trace | medium-high | incrementally closes event coverage |
| 10 | Main coverage matrix | responsibility ledger | medium | auditable ownership backlog |
| 11 | Player/NPC/Projectile/Item gap cards | coverage matrix | high | domain-by-domain ECS completion evidence |
| 12 | Tick-order reconciliation | each domain card | high | deterministic cross-domain ordering |

## Required Final Gate Per Accepted Card

```powershell
dotnet run --project Test\Terraria.Dome.MainBoundary.Verification\Terraria.Dome.MainBoundary.Verification.csproj `
  -c Release -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false

dotnet build Terraria.Dome.sln -c Release -m:1 `
  -p:UseSharedCompilation=false `
  -p:MSBuildNodeReuse=false `
  -p:FixtureHostBuild=false
```

The final build does not replace domain-level acceptance. It only proves the accepted card has not
broken the repository's compile graph. Full migration completion requires the coverage matrix to
have no unclassified server-relevant legacy responsibility, each non-excluded family to have its
own accepted evidence, and all unknown branches to be either recovered or explicitly retained as
unsupported.
