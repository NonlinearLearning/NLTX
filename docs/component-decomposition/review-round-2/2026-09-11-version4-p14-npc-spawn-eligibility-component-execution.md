# Version4 P14 NPC Spawn Eligibility Component Execution Plan

```yaml
partitionId: P14
sessionId: a6d2b55112b341bc939787b194febfa8
claimMode: manual
inputReport: D:\\TRbackup\\NLTX\\docs\\组件文档\\迁移参考表\\Version4权威模拟系统字段属性逐成员源码声明-20分区\\P14-NPC-Spawn-Eligibility.md
designPath: D:\\TRbackup\\NLTX\\docs\\组件文档\\第二轮审查\\2026-09-11-version4-p14-npc-spawn-eligibility-component-design.md
executionPath: D:\\TRbackup\\NLTX\\docs\\组件文档\\第二轮审查\\2026-09-11-version4-p14-npc-spawn-eligibility-component-execution.md
executionStatus: planned
implementationStatus: partial
verificationStatus: partial
evidenceStatus: partial
completedComponents: [NpcSpawnAndCritterState, NpcSpawnContextAndCapacityInputs, NpcSpawnSpatialEligibilityInputs, NpcSpawnBiomeAndDungeonEligibilityInputs, NpcSpawnPolicyAndEventEligibilityInputs, NpcSpawnBiomeZoneInputs, NpcSpawnEventAndTowerInputs, NpcSpawnTargetSelectionState]
currentComponent: null
pendingComponents: [NpcSpawnAndCritterStateRemainingMembers, NpcSpawnBudgetAndActivityState, NpcSpawnCooldownAndEnvironment, NpcTargetAndIdentityProperties, NpcProgressionAndEnvironmentProperties]
lastCheckpointUtc: 2026-09-11T19:12:54.3024955Z
evidence-gap: C01 source implementation exists only for the two confirmed per-NPC state members. C06 source implementation exists as an immutable ten-field spawn-evaluation snapshot with focused value checks passing. C07 source implementation exists as an immutable ten-flag spatial snapshot with focused value checks passing. C08 source implementation exists as an immutable six-flag biome/dungeon snapshot with focused value checks passing. C09 source implementation exists as an immutable eleven-field policy/event snapshot and its focused eleven-value check passes; town/event/player/weather adapter construction, stale-version semantics, and runtime integration remain unverified. C10 source implementation exists as an immutable thirteen-field biome-zone snapshot; the serial NPC build exited 0 with 0 warnings and 0 errors and the combined thirteen-value focused check passed. C11 source implementation exists as an immutable eight-field event/tower snapshot; the serial NPC build exited 0 with 0 warnings and 0 errors and the combined eight-value focused check passed, while event/candle adapter construction, stale-version semantics, event progression ownership, and runtime integration remain unverified. C12 source implementation exists as an immutable one-field target-selection snapshot; the serial NPC build exited 0 with 0 warnings and 0 errors and the focused check preserved sentinel 255 and an explicit target value, while target routing, entity commit, stale-version semantics, network projection, and runtime integration remain unverified. The current serial focused verifier command exited 0 with output `PASS: NPC and town component field composition`; this does not establish P14 runtime integration, network replay, persistence restore, or behavior equivalence. The remaining 56 source members remain design-only.
blocking-decision: Continue only with evidence-complete source-only units. Do not add commit systems, registration keys, persistence/network adapters, lifecycle writers, biome/weather/tile/event/target reads, stale-version semantics, or partial C01/C06/C07/C08/C09/C10/C11/C12 fields until their owners and APIs are confirmed.
```

## 1. Plan Boundary

This is a reversible migration plan for the P14 source inventory. It records proposed future
boundaries and the explicitly authorized source-only implementation checkpoint. It does not
authorize edits to Version4, the authoritative report, other session documents, or the ledger.
All unimplemented types, paths, systems, queries, commands, adapters, and projections have
`status: proposed`.

The plan processes the 12 report leaf groups in order. After each group is reviewed, both P14
documents are saved with the same `completedComponents`, `currentComponent`, `pendingComponents`,
`lastCheckpointUtc`, `evidence-gap`, `blocking-decision`, and `verificationStatus` values before
the next group starts.

## 2. Migration Guardrails

- Keep `NPC.Spawner` as a short-lived context boundary. Materialize an immutable snapshot at the
  spawn evaluation boundary and do not register one-time eligibility results as durable state.
- Separate pure calculations from effects. Time, randomness, global registries, logging, messages,
  persistence, and transport are explicit adapter inputs or commit outputs.
- Establish one owner writer before removing a legacy writer. Reject or deduplicate duplicate
  commands according to the existing identity/slot contract; never silently last-write-wins.
- Keep lifecycle, slot accounting, generated NPC creation, and town/event progression in their
  existing or explicitly approved owners. P14 must not duplicate them.
- Preserve namespaces and public APIs through compatibility facades until focused verifiers pass.
- Do not infer scheduler order from file order. The contract is facts committed -> query snapshot ->
  candidate/command -> owner commit -> projections/adapters.
- Generated output, test results, and build artifacts remain under `Build/`; no source-side output
  is planned.

## 3. Proposed Future File Organization

Use the existing `dome/src/Terraria.Dome.Simulation/Npc` domain root. Keep a small capability flat
until stable scale justifies subdirectories; if the implementation grows, use domain-specific
directories such as `Npc/Spawn`, `Npc/Targeting`, and `Npc/Adapters`, never a generic
`Shared/Components` directory.

| Proposed type | Kind | Proposed path | Owner/seam |
|---|---|---|---|
| `NpcSpawnAndCritterStateComponent` | Component | `dome/src/Terraria.Dome.Simulation/Npc/Spawn/NpcSpawnAndCritterStateComponent.cs` | spawn commit owns per-NPC mutable facts |
| `NpcSpawnAndCritterRules` | Definition | `dome/src/Terraria.Dome.Simulation/Npc/Spawn/NpcSpawnAndCritterRules.cs` | catalog/world rule adapter owns static tuning |
| `NpcSpawnBudgetAndActivityStateComponent` | Component | `dome/src/Terraria.Dome.Simulation/Npc/Spawn/NpcSpawnBudgetAndActivityStateComponent.cs` | approved budget owner only |
| `NpcSpawnCooldownAndEnvironmentState` | Component/registry value | `dome/src/Terraria.Dome.Simulation/Npc/Spawn/NpcSpawnCooldownAndEnvironmentState.cs` | event-day/cooldown owner |
| `NpcTargetAndIdentityPropertiesQuery` | Query | `dome/src/Terraria.Dome.Simulation/Npc/Targeting/NpcTargetAndIdentityPropertiesQuery.cs` | explicit NPC/player snapshot |
| `NpcProgressionAndEnvironmentPropertiesQuery` | Query | `dome/src/Terraria.Dome.Simulation/Npc/Spawn/NpcProgressionAndEnvironmentPropertiesQuery.cs` | progression and environment snapshot |
| `NpcSpawnContextSnapshot` | Snapshot | `dome/src/Terraria.Dome.Simulation/Npc/Spawn/NpcSpawnContextSnapshot.cs` | ephemeral spawn evaluation input |
| `NpcSpawnSpatialEligibilityQuery` | Query | `dome/src/Terraria.Dome.Simulation/Npc/Spawn/NpcSpawnSpatialEligibilityQuery.cs` | tile/position snapshot |
| `NpcSpawnBiomeAndDungeonEligibilityQuery` | Query | `dome/src/Terraria.Dome.Simulation/Npc/Spawn/NpcSpawnBiomeAndDungeonEligibilityQuery.cs` | zone/dungeon snapshot |
| `NpcSpawnPolicyEligibilityQuery` | Query | `dome/src/Terraria.Dome.Simulation/Npc/Spawn/NpcSpawnPolicyEligibilityQuery.cs` | policy/event facts |
| `NpcSpawnBiomeZoneQuery` | Query | `dome/src/Terraria.Dome.Simulation/Npc/Spawn/NpcSpawnBiomeZoneQuery.cs` | immutable biome-zone facts |
| `NpcSpawnEventAndTowerQuery` | Query | `dome/src/Terraria.Dome.Simulation/Npc/Spawn/NpcSpawnEventAndTowerQuery.cs` | tower/event facts |
| `NpcSpawnTargetSelectionQuery` | Query | `dome/src/Terraria.Dome.Simulation/Npc/Targeting/NpcSpawnTargetSelectionQuery.cs` | sentinel-safe target candidate |
| `NpcSpawnCommitSystem` adapter seam | System/command port | existing `Npc/Systems/NpcSpawnCommitSystem.cs` | generated entity handoff; lifecycle remains external |

The current checkout uses `src/Npc` as the affected project domain root, so the first implemented
type is at `src/Npc/NpcSpawnAndCritterStateComponent.cs`. The other rows remain future targets;
their paths and namespaces require project-level review before implementation.

## 4. Verification Commands And Evidence

Compile-capable commands for the current C01/C06 checkpoint were run through the repository wrapper
from the repository root after confirming no active `dotnet.exe` or `csc.exe` process. Later units
must use the same serialized form:

```powershell
pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 `
  build .\dome\src\Terraria.Dome.Simulation\Terraria.Dome.Simulation.csproj `
  -m:1 -nr:false -p:UseSharedCompilation=false `
  -p:MSBuildNodeReuse=false -p:BuildInParallel=false

pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 `
  test .\dome\Test\<affected-npc-verifier>.csproj --no-build --no-restore `
  -m:1 -nr:false -p:UseSharedCompilation=false `
  -p:MSBuildNodeReuse=false -p:BuildInParallel=false
```

Acceptance evidence must record the exact command, affected project, exit code, warning/error
counts, and artifact path under `Build/bin/`. The current evidence is recorded in Section 7.1 and
Section 7.2.

### 4.1 Actual C01/C06 Verification Record

- NPC project build: `pwsh -NoProfile -Command "& '.\Build\Tools\Invoke-SerialDotnet.ps1'
  'build' '.\src\Npc\Terraria.Npc.csproj' '--no-restore' '-m:1' '-nr:false'
  '-p:UseSharedCompilation=false' '-p:MSBuildNodeReuse=false' '-p:BuildInParallel=false'"`;
  exit code `0`; 0 warnings; 0 errors; artifact
  `Build/bin/Terraria.Npc/Debug/net10.0/Terraria.Npc.dll`.
- Focused C01/C06 value check: PowerShell reflection against the compiled NPC assembly;
  exit code `0`; output `PASS: P14 C01 and C06 focused state checks`.
- Verifier project build: `pwsh -NoProfile -Command "& '.\Build\Tools\Invoke-SerialDotnet.ps1'
  'build' '.\Test\Terraria.Npc.Components.Verification\Terraria.Npc.Components.Verification.csproj'
  '--no-restore' '-m:1' '-nr:false' '-p:UseSharedCompilation=false'
  '-p:MSBuildNodeReuse=false' '-p:BuildInParallel=false'"`; exit code `0`; 0 warnings;
  0 errors; artifact
  `Build/bin/Terraria.Npc.Components.Verification/Debug/net10.0/Terraria.Npc.Components.Verification.dll`.
- Verifier run: `pwsh -NoProfile -Command "& '.\Build\Tools\Invoke-SerialDotnet.ps1'
  'run' '--project' '.\Test\Terraria.Npc.Components.Verification\Terraria.Npc.Components.Verification.csproj'
  '--no-build' '--no-restore' '-m:1' '-nr:false' '-p:UseSharedCompilation=false'
  '-p:MSBuildNodeReuse=false' '-p:BuildInParallel=false'"`; exit code `0`; output
  `PASS: NPC and town component field composition`; verifier artifact
  `Build/bin/Terraria.Npc.Components.Verification/Debug/net10.0/Terraria.Npc.Components.Verification.dll`.

## 5. Completed Checkpoint: C01 NpcSpawnAndCritterState

- Proposed boundary: per-NPC provenance/replacement/transient flags plus immutable critter rules;
  static type-indexed arrays remain registry/catalog state.
- Sequence: define explicit spawn-source input; calculate critter rule from supplied definition and
  randomness; commit `SpawnedFromStatue` and replacement state once; publish loot/network/presentation
  snapshots; clear entity state on despawn.
- Dependencies: reads NPC type definitions and source facts; emits a candidate/commit command. It
  does not own generated lifecycle, loot, networking, localization, or town progression.
- Scheduler contract: source facts -> pure critter rule -> `NpcSpawnCommitSystem` -> lifecycle/slot
  commit -> downstream projections.
- Side-effect register: the rule calculation reads no external state. The commit writes entity
  state exactly once. Loot/network adapters may publish after commit, with duplicate handling owned
  by their partitions. Registry invalidation is owned by the registry adapter.
- Rollback: retain the legacy fields and disable the new adapter if provenance, replacement,
  shimmer reset, or duplicate commit tests diverge.
- Verification later: member/source mapping, deterministic chance calculation, spawn/despawn reset,
  one-writer proof, and `SpawnedFromStatue` projection parity.

## 6. Design Checkpoint: C02 NpcSpawnBudgetAndActivityState

- Proposed boundary: immutable spawn-window/rate/cap definitions plus the existing population
  accounting seam for per-NPC slot weight and count exemption.
- Sequence: provide scheduler snapshot; evaluate no-spawn and range/capacity policy; allocate a
  slot; commit the entity; apply accounting once; release the weight on despawn.
- Dependencies: reads player/world window facts and active-population snapshot; it does not own
  generated NPC lifecycle, town progression, or event progression.
- Scheduler contract: cycle gate -> readiness query -> slot allocation -> spawn commit -> accounting
  update -> downstream projections.
- Side-effect register: readiness is pure over explicit inputs. Allocation and accounting write the
  single population owner. Rejected candidates consume nothing; uncertain commits require identity
  reconciliation before retry.
- Rollback: retain existing allocator/accounting paths if boundary values, no-spawn gating, or
  duplicate/despawn accounting differs.
- Verification later: threshold cases, duplicate command handling, zero release, and owner-writer
  proof against `NpcSlotAllocator` and `NpcSlotAccountingSystem`.

## 7. Design Checkpoint: C03 NpcSpawnCooldownAndEnvironment

- Proposed boundary: immutable wave/luck/protection definitions plus explicit event-day, weather,
  interaction-suppression, and spawn-cycle timer owners.
- Sequence: event/weather/day snapshots enter the eligibility query; the query calculates thresholds;
  event and timer systems commit their own state; spawn commit consumes the resulting candidate.
- Dependencies: event progression and weather are external owners; the group does not create a
  second world-state registry or attach global timers to NPC entities.
- Scheduler contract: world/event facts -> pure threshold query -> explicit timer/event commit ->
  spawn candidate/commit -> downstream projections.
- Side-effect register: definition lookups are pure reads. Day/timer writes are owned by event,
  weather, interaction, or spawn-cycle systems, with tick order and duplicate event identity visible.
- Rollback: retain legacy event/day/timer adapters if daily reset, protection duration, or repeated
  event message behavior differs.
- Verification later: immutable lookup, timer bounds, day reset, protection interval, and proof that
  eligibility queries do not write global state.

## 8. Design Checkpoint: C04 NpcTargetAndIdentityProperties

- Proposed boundary: one read-only target/identity query over explicit snapshots, with a separate
  name command and localization/persistence adapter.
- Sequence: normalize target encoding and liveness; calculate interaction and display results;
  accept `GivenName` only through a versioned command; publish localized output.
- Dependencies: target routing reads player/NPC liveness; localization and persistence remain
  external. The query does not own NPC lifecycle, targeting mutation, or network transport.
- Scheduler contract: identity/liveness facts -> query -> optional name commit -> localization and
  network projections.
- Side-effect register: query calculation is pure. Localization is an external read adapter;
  name persistence is an explicit write with duplicate/version handling and integration-review.
- Rollback: retain the legacy property facade if target sentinel behavior, talk eligibility, or name
  fallback differs; do not add a query setter.
- Verification later: player/NPC sentinel boundaries, stale target rejection, town-pet rules,
  localized fallback, null normalization, and no-write query proof.

## 9. Design Checkpoint: C05 NpcProgressionAndEnvironmentProperties

- Proposed boundary: pure progression/weather/classification queries, plus explicit opacity write and
  mech-queen registry repair seams, with network section coordinates as a one-way projection.
- Sequence: consume progression/environment snapshots; calculate properties; route `Opacity` and
  stale-registry repair through their owners; publish section coordinates downstream.
- Dependencies: world progression, weather, NPC definitions, presentation alpha, and network section
  mapping are external inputs/ports. P14 does not own boss progression, position, or transport.
- Scheduler contract: committed world/entity facts -> property queries -> explicit repair/write
  commands -> network/presentation projections.
- Side-effect register: most calculations are pure; opacity setter writes entity/presentation state;
  mech-queen repair writes a registry. Both effects require versioned owner checks and idempotence.
- Rollback: retain legacy property facades if opacity clamping, stale registry cleanup, progression
  thresholds, or section mapping differs.
- Verification later: boss/tower thresholds, opacity clamp/round-trip, registry repair, wind and
  critter boundaries, and projection-only network coordinates.

## 10. Completed Checkpoint: C06 NpcSpawnContextAndCapacityInputs

- Proposed boundary: ephemeral immutable context snapshot for one spawn evaluation, not a durable
  component or registry entry.
- Sequence: sample player/world/event facts; materialize context; run pure eligibility queries;
  submit candidate to the existing commit/slot/lifecycle seams; discard context.
- Dependencies: player readiness, active-player count, invasion cap, time/weather, luck, and spawn
  geometry are adapter inputs. No generated NPC lifecycle or population owner is duplicated.
- Scheduler contract: sample facts -> context snapshot -> eligibility queries -> candidate -> slot and
  lifecycle commit -> optional diagnostics.
- Side-effect register: snapshot/query are pure over explicit inputs; diagnostics and commit effects
  belong to outer adapters and owners. Stale versions are rejected and resampled.
- Rollback: keep the legacy `Spawner` construction path if snapshot lifetime, coordinate conversion,
  cap handling, or diagnostic behavior diverges.
- Verification later: immutable context, version staleness, capacity boundaries, clock/weather/luck
  propagation, and proof that no one-time SpawnInfo becomes a durable component.

## 11. Completed Checkpoint: C07 NpcSpawnSpatialEligibilityInputs

- Proposed boundary: one pure spatial eligibility query over an immutable position/tile/world-layout
  snapshot; no durable component for the ten flags.
- Sequence: sample one candidate location and relevant tile/region facts; evaluate all predicates;
  pass qualification/rejection to biome and policy queries; discard the snapshot after evaluation.
- Dependencies: tile, structure, depth, region, dungeon, and Remix layout adapters are external
  readers. The query does not write world tiles or NPC state.
- Scheduler contract: world/tile snapshot -> spatial query -> biome/policy queries -> candidate ->
  spawn commit.
- Side-effect register: snapshot acquisition is an adapter read with explicit failure/staleness;
  calculation is pure. Retry samples a new version and never reuses partial results.
- Rollback: retain the legacy spawn flag path if spatial boundary cases, tile version checks, or
  sky/tree/region behavior diverges.
- Verification later: depth, ocean/beach, sky/tree, dungeon, Remix region, stale snapshot, and
  no-world-write scenarios.

### 11.1 Implementation checkpoint

- Actual file: `src/Npc/NpcSpawnSpatialEligibilitySnapshot.cs`.
- Implemented behavior: stores the ten Version4 spatial eligibility inputs as one immutable
  spawn-evaluation snapshot; it does not calculate them or own tile/world state.
- Dependency impact: references only `Terraria.Npc`; no tile/world adapter, clock, random source,
  logger, network, persistence, lifecycle, or registration dependency was added.
- Verification: the compiled-assembly ten-value focused check passed; the post-checkpoint build
  record and adapter/runtime integration remain unverified.

## 12. Completed Checkpoint: C08 NpcSpawnBiomeAndDungeonEligibilityInputs

- Proposed boundary: pure water/special-biome/dual-dungeon query over a versioned immutable zone
  snapshot; no durable component or dungeon registry is added.
- Sequence: consume the spatially consistent tile/region snapshot; calculate all six rules; pass
  rejection or candidate onward to policy/event queries; discard flags after evaluation.
- Dependencies: tile, structure, biome, world-layout, and dual-dungeon policy adapters are external
  readers. The query cannot mutate dungeon or world state.
- Scheduler contract: spatial snapshot -> biome/dungeon query -> policy/event query -> candidate ->
  spawn commit.
- Side-effect register: snapshot acquisition has explicit staleness/failure; calculation is pure;
  retry samples a new matching version and never reuses partial flags.
- Rollback: retain legacy dual-dungeon and special-biome reads if combination behavior, version
  handling, or compatibility spelling differs.
- Verification later: water, Granite/Marble, dual-dungeon combinations, stale versions, and no-write
  proof.

### 12.1 Implementation checkpoint

- Actual file: `src/Npc/NpcSpawnBiomeAndDungeonEligibilitySnapshot.cs`.
- Implemented behavior: stores the six Version4 water, special-biome, and dual-dungeon inputs as
  one immutable spawn-evaluation snapshot, preserving `TresspassingDualDungeon` at the boundary.
- Dependency impact: references only `Terraria.Npc`; no tile/world/dungeon adapter, clock, random
  source, logger, network, persistence, lifecycle, or registration dependency was added.
- Verification: the compiled-assembly six-value focused check passed; the post-checkpoint build
  record and adapter/runtime integration remain unverified.

## 13. Completed Checkpoint: C09 NpcSpawnPolicyAndEventEligibilityInputs

- Proposed boundary: pure policy query over coherent town/player/event/weather/wall snapshots; no
  second town, invasion, or lifecycle owner.
- Sequence: combine spatial/biome facts with policy inputs; return a candidate/rejection reason;
  apply disposition and generated state only at the existing spawn commit seam.
- Dependencies: town progression, invasion/event, weather, wall, player readiness, and Tim policy
  are external readers. P14 does not write any of those source states.
- Scheduler contract: coherent spatial/biome snapshot -> policy query -> candidate -> slot/lifecycle
  and event commit -> projections.
- Side-effect register: snapshot acquisition can fail or be stale; calculation is pure. Event
  duplicate handling and generated disposition writes belong to their owners.
- Rollback: retain legacy policy evaluation if town/event combinations, wall handling, wind policy,
  or starting-health behavior differs.
- Verification later: town/invasion thresholds, worm/spider/wall rules, wind/Tim/health cases,
  coherent-version rejection, and no-source-write proof.

### 13.1 Implementation checkpoint

- Actual file: `src/Npc/NpcSpawnPolicyEligibilitySnapshot.cs`.
- Implemented behavior: stores the eleven Version4 policy/event inputs as one immutable
  spawn-evaluation snapshot; `townNPCs` is exposed as the semantic `TownNpcCount` member.
- Dependency impact: references only `Terraria.Npc`; no town, event, weather, wall, player,
  lifecycle, registration, network, or persistence dependency was added.
- Verification: the compiled-assembly eleven-value focused check passed; the post-checkpoint build
  record and adapter/runtime integration remain unverified.

## 14. Completed Checkpoint: C10 NpcSpawnBiomeZoneInputs

- Proposed boundary: one immutable, versioned biome/weather zone snapshot consumed by a pure zone
  query; no per-NPC zone component.
- Sequence: sample world/tile/biome/weather facts once; project 13 flags; pass the coherent snapshot
  to later eligibility queries; discard it after evaluation.
- Dependencies: biome, weather, tile, and world-zone adapters are external readers. P14 does not
  write zone registries or tiles.
- Scheduler contract: world zone adapter -> zone query -> spatial/biome/policy evaluation -> spawn
  candidate/commit.
- Side-effect register: snapshot acquisition has explicit version/staleness failure; projection is
  pure. Retry takes a complete new snapshot and never reuses partial flags.
- Rollback: retain legacy zone reads if combined-zone or sandstorm boundary behavior changes.
- Verification later: all zone flags, combinations, weather boundary, version consistency, and no
  world-write proof.

### 14.1 Implementation checkpoint

- Actual file: `src/Npc/NpcSpawnBiomeZoneEligibilitySnapshot.cs`.
- Implemented behavior: stores the thirteen Version4 biome/zone inputs as one immutable
  spawn-evaluation snapshot, preserving the legacy field spellings including `ZoneLihzhardTemple`.
- Dependency impact: references only `Terraria.Npc`; no biome, weather, tile, world-zone, clock,
  random source, logger, network, persistence, lifecycle, or registration dependency was added.
- Lifecycle boundary: the snapshot is an ephemeral input for one spawn evaluation and is not a
  durable NPC component or zone registry.
- Verification: the serial NPC build exited `0` with `0` warnings and `0` errors; the compiled-
  assembly focused check preserved all thirteen values. Version/expiry behavior, adapter
  construction, and runtime integration remain unverified.

## 15. Completed Checkpoint: C11 NpcSpawnEventAndTowerInputs

- Proposed boundary: pure event/tower/candle eligibility query over a versioned world snapshot; no
  event progression or tower state component is added to NPC entities.
- Sequence: sample event-area and candle facts; evaluate eight inputs; pass candidate/rejection to
  policy and target queries; let event/lifecycle owners commit generated state.
- Dependencies: tower, Old One Army, candle, world-region, progression, and network adapters are
  external. P14 does not own event progression or event NPC lifecycle.
- Scheduler contract: event/world snapshot -> event/tower query -> policy/target query -> candidate
  -> event/lifecycle commit -> projections.
- Side-effect register: snapshot read can be stale or fail; query is pure. Event deduplication and
  progression writes belong to the event owner.
- Rollback: retain legacy event-area and candle reads if tower/candle combinations or version
  handoff changes.
- Verification later: four tower flags, Old One Army, three candle modifiers, stale version, and
  no-progress-write proof.

### 15.1 Implementation checkpoint

- Actual file: `src/Npc/NpcSpawnEventAndTowerEligibilitySnapshot.cs`.
- Implemented behavior: stores the eight Version4 event/tower/candle inputs as one immutable
  spawn-evaluation snapshot, preserving the authoritative member names and declaration order.
- Dependency impact: references only `Terraria.Npc`; no tower registry, event progression,
  world-region reader, candle adapter, clock, network, persistence, lifecycle, or registration
  dependency was added.
- Lifecycle boundary: the snapshot is an ephemeral input for one spawn evaluation and is not a
  durable NPC component or event-state owner. Version/expiry semantics remain an external adapter
  responsibility.
- Verification: the serial NPC build exited `0` with `0` warnings and `0` errors; the compiled-
  assembly focused check preserved all eight values. Event/candle adapter construction, stale
  version handling, no-progress-write proof, and runtime integration remain unverified.

## 6. Checkpoint Rule Used for Each Group

Each checkpoint appended the group-specific decision and updated both YAML blocks before the next
group began:

1. Confirm every report member and source line is mapped exactly once.
2. Identify mutable owner, pure input, definition/registry, command, and projection boundaries.
3. Record dependencies, scheduler order, effect ownership, failure/retry/duplicate semantics, and
   rollback condition.
4. Mark unresolved cross-partition ownership as `integration-review`; do not invent an owner.
5. Save design and execution documents together, then proceed to the next group.

## 7. Current Verification Statement

The current task added seven source-only files after inspecting the Version4 declarations and current
NLTX conventions. Prior serial NPC and verifier-project builds succeeded, and focused value checks
passed for C01, C06, C07, C08, and C09. The C10 and C11 source checkpoints were built by the serial
NPC command with `0` warnings and `0` errors; the combined compiled-assembly check preserved all
thirteen and eight values. The current serial focused verifier command exited `0` with output
`PASS: NPC and town component field composition`; no runtime behavior equivalence, network replay,
or persistence restore has been obtained. Adapter construction, version/expiry handling, target
routing, and runtime integration remain unverified. The required
execution state remains:

```text
executionStatus: planned
implementationStatus: partial
verificationStatus: partial
```

### 7.1 Implementation checkpoint: C01 NpcSpawnAndCritterState

- Actual file: `src/Npc/NpcSpawnAndCritterStateComponent.cs`.
- Implemented behavior: stores `SpawnedFromStatue` and
  `CanBeReplacedByOtherNpcs` as an immutable per-NPC state snapshot.
- Dependency impact: references only `Terraria.Npc`; no project, registration, lifecycle, slot,
  network, loot, localization, or persistence dependency was added.
- Deliberate exclusions: `maxAI`, `goldCritterChance`, `dripping`, `drippingSlime`,
  `drippingSparkleSlime`, `ShimmeredTownNPCs`, and the firefly/butterfly/stink-bug values remain
  unimplemented because their owner or lifecycle evidence is partial.
- Verification: partial. The serial NPC build succeeded with 0 warnings and 0 errors; the serial
  verifier-project build succeeded with 0 warnings and 0 errors. A focused value check passed for
  both flag combinations. The current serial focused verifier command exited 0 with output
  `PASS: NPC and town component field composition`. No behavior equivalence, network replay, or
  persistence restore is claimed.

### 7.2 Implementation checkpoint: C06 NpcSpawnContextAndCapacityInputs

- Actual file: `src/Npc/NpcSpawnContextSnapshot.cs`.
- Implemented behavior: stores the ten Version4 `NPC.Spawner` context values as one immutable
  spawn-evaluation snapshot, including spawn geometry, active-player/capacity facts, player tile
  coordinates, luck, daytime, and rain.
- Dependency impact: references only `Terraria.Npc`; no durable entity registration, world/player
  lookup, clock, random source, logger, network, persistence, or lifecycle dependency was added.
- Lifecycle boundary: the snapshot is a value payload for one spawn evaluation and is not a durable
  ECS component or a spawn commit claim.
- Verification: the serial NPC project build after this checkpoint succeeded with 0 warnings and 0
  errors. A compiled-assembly focused value check passed for all ten snapshot values. Adapter
  construction, version/retry behavior, and runtime integration remain unverified.

## 8. Completed Checkpoint: C12 NpcSpawnTargetSelectionState

- Proposed boundary: ephemeral sentinel-valued target input and pure target candidate query; target
  routing and commit remain existing adjacent owners.
- Sequence: receive default `255`, evaluate eligible player/NPC snapshot, return candidate/no-target,
  then let target routing commit after entity creation.
- Dependencies: target snapshot, routing policy, and lifecycle identity are external seams. P14 does
  not own target mutation or generated lifecycle.
- Scheduler contract: spawn context -> target query -> spawn/lifecycle commit -> target commit and
  projection.
- Side-effect register: target calculation is pure; stale snapshots cause no candidate or fresh
  evaluation; duplicate commands use entity/version identity.
- Rollback: retain the legacy default-target path if sentinel or Player/NPC target encoding differs.
- Verification later: `255` preservation, NPC offset encoding, stale target rejection, no query
  writes, and handoff to `NpcTargetRoutingSystem`/`NpcTargetSelectionSystem`.

### 8.1 Implementation checkpoint

- Actual file: `src/Npc/NpcSpawnTargetSelectionSnapshot.cs`.
- Implemented behavior: stores the one Version4 `defaultTarget` input as an immutable
  spawn-evaluation snapshot, preserving sentinel `255` and explicit target values without
  calculating or committing a target.
- Dependency impact: references only `Terraria.Npc`; no target routing system, entity reference,
  lifecycle writer, player/NPC lookup, network adapter, persistence adapter, or registration key
  was added.
- Lifecycle boundary: the snapshot is an ephemeral input for one spawn evaluation and is not a
  durable target component or target authority. Target encoding, stale-version semantics, and
  commit ownership remain external.
- Verification: the serial NPC build exited `0` with `0` warnings and `0` errors; the compiled-
  assembly focused check preserved sentinel `255` and an explicit target value. Target routing,
  entity commit, stale-input handling, network projection, and runtime integration remain
  unverified.

## 9. Final Plan Checkpoint

All 12 report leaf groups are complete in the plan, and the design document contains the source
sequence index for all 117 members exactly once. The confirmed C01 state subset and the C06 context
snapshot are implemented:

```text
executionStatus: planned
implementationStatus: partial
verificationStatus: partial
```

Eight C# files under `src/Npc` were added. No project file, test, authoritative report, ledger, or
other session document was modified. C01 and C06-C12 have source and focused value evidence but are
not runtime integrated; the remaining P14 groups remain design-only until their evidence gaps are
closed.

## 10. Complete 117-Member Mapping Index

The design document contains the per-member ownership tables. This execution document repeats the
authoritative source-sequence assignment so the plan is independently auditable and every member
is assigned to exactly one completed leaf group.

| Leaf group | Source sequence members | Count |
|---|---|---:|
| `NpcSpawnAndCritterState` | 1596, 1597, 1604, 1605, 1606, 1607, 1608, 1609, 1625, 1626, 1627, 1628, 1629 | 13 |
| `NpcSpawnBudgetAndActivityState` | 1651, 1652, 1653, 1654, 1655, 1656, 1657, 1658, 1659, 1661 | 10 |
| `NpcSpawnCooldownAndEnvironment` | 1860, 1861, 1862, 1863, 1864, 1865, 1866, 1867, 1868, 1869 | 10 |
| `NpcTargetAndIdentityProperties` | 1886, 1887, 1888, 1889, 1890, 1891, 1892, 1893, 1894, 1895, 1896, 1897, 1898, 1899, 1900, 1901 | 16 |
| `NpcProgressionAndEnvironmentProperties` | 1902, 1903, 1904, 1905, 1906, 1907, 1908, 1909, 1910 | 9 |
| `NpcSpawnContextAndCapacityInputs` | 1519, 1520, 1521, 1522, 1523, 1524, 1525, 1526, 1527, 1528 | 10 |
| `NpcSpawnSpatialEligibilityInputs` | 1540, 1541, 1542, 1543, 1544, 1545, 1546, 1548, 1549, 1553 | 10 |
| `NpcSpawnBiomeAndDungeonEligibilityInputs` | 1536, 1537, 1538, 1550, 1551, 1552 | 6 |
| `NpcSpawnPolicyAndEventEligibilityInputs` | 1529, 1530, 1531, 1532, 1533, 1534, 1535, 1539, 1547, 1554, 1555 | 11 |
| `NpcSpawnBiomeZoneInputs` | 1556, 1557, 1558, 1559, 1560, 1561, 1562, 1563, 1564, 1565, 1566, 1567, 1568 | 13 |
| `NpcSpawnEventAndTowerInputs` | 1569, 1570, 1571, 1572, 1573, 1574, 1575, 1576 | 8 |
| `NpcSpawnTargetSelectionState` | 1577 | 1 |
| **Total** | **all report members exactly once** | **117** |
