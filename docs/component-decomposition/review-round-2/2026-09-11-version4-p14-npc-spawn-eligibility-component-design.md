# Version4 P14 NPC Spawn Eligibility Component Design

```yaml
partitionId: P14
sessionId: a6d2b55112b341bc939787b194febfa8
claimMode: manual
inputReport: D:\\TRbackup\\NLTX\\docs\\组件文档\\迁移参考表\\Version4权威模拟系统字段属性逐成员源码声明-20分区\\P14-NPC-Spawn-Eligibility.md
componentDesignPath: D:\\TRbackup\\NLTX\\docs\\组件文档\\第二轮审查\\2026-09-11-version4-p14-npc-spawn-eligibility-component-design.md
componentExecutionPath: D:\\TRbackup\\NLTX\\docs\\组件文档\\第二轮审查\\2026-09-11-version4-p14-npc-spawn-eligibility-component-execution.md
designStatus: proposed
executionStatus: planned
implementationStatus: partial
evidenceStatus: partial
currentNltxStatus: partial
verificationStatus: partial
completedComponents: [NpcSpawnAndCritterState, NpcSpawnContextAndCapacityInputs, NpcSpawnSpatialEligibilityInputs, NpcSpawnBiomeAndDungeonEligibilityInputs, NpcSpawnPolicyAndEventEligibilityInputs, NpcSpawnBiomeZoneInputs, NpcSpawnEventAndTowerInputs, NpcSpawnTargetSelectionState]
currentComponent: null
pendingComponents: [NpcSpawnAndCritterStateRemainingMembers, NpcSpawnBudgetAndActivityState, NpcSpawnCooldownAndEnvironment, NpcTargetAndIdentityProperties, NpcProgressionAndEnvironmentProperties]
lastCheckpointUtc: 2026-09-11T19:12:54.3024955Z
evidence-gap: C01 source compiles and focused value checks pass for 2 of 13 members; its remaining 11 members still lack an approved owner boundary. C06 source-only snapshot compiles and all ten focused value checks pass; C07 source-only spatial snapshot compiles and all ten focused value checks pass; C08 source-only biome/dungeon snapshot compiles and all six focused value checks pass; C09 source-only immutable eleven-field policy/event snapshot has a focused value check passing, but town/event/player/weather adapter construction proof, version/retry policy, and runtime integration remain unverified. C10 source-only immutable thirteen-field biome-zone snapshot compiles with 0 warnings and 0 errors and passes the combined thirteen-value focused check. C11 source-only immutable eight-field event/tower snapshot compiles with 0 warnings and 0 errors and passes the combined eight-value focused check; event/candle adapter construction, stale-version semantics, event progression ownership, and runtime integration remain unverified. C12 source-only immutable one-field target-selection snapshot compiles with 0 warnings and 0 errors and preserves the default target sentinel `255` plus explicit target values; target routing, entity commit, stale-version semantics, network projection, and runtime integration remain unverified. C01 has no runtime owner, spawn/replacement commit integration, despawn reset proof, network/loot projection proof, or registered component key. The current focused verifier command exits `0` with output `PASS: NPC and town component field composition`; this does not establish P14 runtime integration, network replay, persistence restore, or behavior equivalence. The remaining 56 source members and all cross-partition ownership evidence remain open.
blocking-decision: C01, C06, C07, C08, C09, C10, C11, and C12 remain source-only. Do not add registration keys, commit systems, lifecycle reset, network/loot adapters, biome/weather/tile/event/target reads, stale-version semantics, or guessed query rules. All other P14 groups remain integration-review.
```

## 1. Scope and Status

This document covers exactly the 12 leaf groups and 117 members listed in the P14 authoritative
report under the formal parent `NpcAndTownSimulation`. It is a proposed component, query, command,
adapter, and projection decomposition. It is not a runtime migration, behavior-equivalence proof,
API-compatibility proof, network-closure proof, persistence-closure proof, or complete C# runtime
implementation. It contains the explicitly authorized source-only checkpoints below.

The source inventory is authoritative for member identity, declaration type, source path, line,
column, and original declaration. The design may classify a legacy field as a component field,
definition value, query input, registry entry, command payload, or projection input when that is
required to preserve ownership and side-effect boundaries. Such classification does not remove or
rename the legacy member.

Checkpoint `NpcSpawnAndCritterState` is complete as the first source-member design checkpoint and
has a source-only implementation for its two confirmed per-NPC state members. Its remaining
partial-evidence members are explicitly pending implementation. The remaining groups are processed
in report order. The YAML checkpoint metadata in this document and the execution document is
updated together after every implementation unit, before the next unit begins.

## 2. Boundary Decision

`Terraria.NPC` combines per-NPC state, process/world spawn policy, short-lived `NPC.Spawner`
context, pure-looking properties, localization, network projections, and stateful compatibility
setters. The proposed boundary is therefore capability-first and access-pattern-first:

```text
player/world/tile/event inputs
  -> SpawnContextSnapshot and eligibility queries
  -> spawn candidate / rejection reason
  -> NpcSpawnCommitSystem through an explicit commit port
  -> NPC lifecycle and slot accounting owned by their partitions
  -> target, network, persistence, localization, and presentation projections
```

The following invariants apply to every checkpoint:

- `Spawner` data is an ephemeral snapshot or value input. It is constructed, populated, consumed,
  and discarded within a spawn evaluation. One-time SpawnInfo or eligibility results are not
  registered as durable ECS components.
- A query reads explicit immutable input and returns a value, reason, or candidate. It does not
  write a component, clear a registry slot, publish a message, log, use time or randomness, or
  reach into a renderer or transport.
- The owner system is the only writer of mutable state. Cross-partition changes use a command,
  event, or projection with an explicit ordering and duplicate/failure policy.
- `npcSlots`, `dontCountMe`, replacement state, active ranges, generated entity lifecycle, and
  population accounting are not duplicated between P14 and the NPC lifecycle/slot owners.
- Static arrays and global flags are not automatically entity components. Their actual owner may
  be a definition/catalog, world progression registry, day/event state, or adapter.
- The source setter on `Opacity`, the cleanup behavior inside `IsMechQueenUp`, and the setter on
  `GivenName` cannot be hidden behind a read-only query until their write ownership is closed.

## 3. Evidence Register

| Source | Location | Fact supported | evidenceStatus |
|---|---|---|---|
| Version4 | `D:\TRbackup\Version4\Terraria\NPC.cs:39-157` | `NPC.Spawner` declares the 117-member context and target inputs that are evaluated around a player/world snapshot | confirmed |
| Version4 | `D:\TRbackup\Version4\Terraria\NPC.cs:159-183` | `Spawner` construction initializes context and computes active-cap inputs; it is short-lived qualification state | confirmed |
| Version4 | `D:\TRbackup\Version4\Terraria\NPC.cs:185-205` | `SpawnNPC` is the spawn entry boundary and consumes a spawner context before a generated NPC is committed | confirmed |
| Version4 | `D:\TRbackup\Version4\Terraria\NPC.cs:279-340` | `SetSpawnFlags` mutates player position, luck, weather, biome, dungeon, tower, candle, invasion, Remix, and health predicates | confirmed |
| Version4 | `D:\TRbackup\Version4\Terraria\NPC.cs:5933-5999` | critter, statue, replacement, shimmer, and firefly/butterfly state declarations | confirmed |
| Version4 | `D:\TRbackup\Version4\Terraria\NPC.cs:6043-6063` | range, spawn rate, active time, capacity, and slot-accounting declarations | confirmed |
| Version4 | `D:\TRbackup\Version4\Terraria\NPC.cs:6461-6484` | event wave lookup, daily kill flags, protection/cooldown, luck, rain, and offset timing declarations | confirmed |
| Version4 | `D:\TRbackup\Version4\Terraria\NPC.cs:6486-6714` | target, identity, progression, opacity, boss, town, wind, critter, and network-section properties | confirmed |
| Current NLTX | `dome/src/Terraria.Dome.Simulation/Npc/Snapshots/NpcSpawnSnapshot.cs` and `NpcSpawnPlayerReadiness.cs` | existing snapshot/readiness seams partially model spawn inputs | existing-evidence |
| Current NLTX | `dome/src/Terraria.Dome.Simulation/Npc/Components/NpcSpawnStateComponent.cs` and `NpcSpawnCycleStateComponent.cs` | existing partial spawn state and cycle state; ownership is not yet closed for all P14 members | existing-evidence |
| Current NLTX | `dome/src/Terraria.Dome.Simulation/Npc/Systems/NpcSpawnEligibilitySystem.cs`, `NpcSpawnPlayerReadinessQuery.cs`, `NpcSpawnCommitSystem.cs` | current eligibility, readiness, and commit seams | existing-evidence |
| Current NLTX | `dome/src/Terraria.Dome.Simulation/Npc/Systems/NpcEventSpawnSystem.cs`, `NpcSlotAllocator.cs`, `NpcSlotAccountingSystem.cs`, `NpcSpawnSlotPolicy.cs`, `NpcInvasionBossCapPolicy.cs` | current event spawn and capacity seams; no duplicate ownership is proposed | existing-evidence |
| Current NLTX | `dome/src/Terraria.Dome.Simulation/Npc/Systems/NpcTargetRoutingSystem.cs`, `NpcTargetSelectionSystem.cs` | current target routing/selection seams | existing-evidence |
| Current NLTX | `dome/src/Terraria.Dome.Simulation/Npc/Systems/NpcLifecycleSystem.cs` and `Npc/Components/NpcLifecycleComponent.cs` | generated entity lifecycle is an existing adjacent owner and remains cross-partition | existing-evidence |
| Current NLTX | `dome/src/Terraria.Dome.Simulation/Npc/Components/NpcAuthorityComponent.cs` | current NPC authority boundary; P14 does not replace it with a second lifecycle owner | existing-evidence |
| tModLoader stable mirror | `D:\TRbackup\tmodloader-api-docs-stable\class_n_p_c.html` and `class_entity_source___spawn_n_p_c.html` (`v2026.07`) | public spawn/entity API boundary only; it is not private Version4 semantic evidence | confirmed-public-boundary |
| SS14 reference | `C:\Users\shan\Downloads\ECS\space-station-14-master\Content.Server\NPC` | organization reference for narrow components and system-owned side effects only | organization-only |

The source report contains all 117 members and reports the expected/observed count as `117 / 117`.
The remaining gap is ownership evidence, not a missing declaration.

## 4. Formal Groups and Proposed Seams

| Leaf group | Members | Proposed boundary | Classification | Primary seam | Cross-partition decision |
|---|---:|---|---|---|---|
| `NpcSpawnAndCritterState` | 13 | `NpcSpawnAndCritterStateComponent` plus immutable critter rules | component/definition | spawn owner and commit port | `SpawnedFromStatue` crosses spawn/loot/network integration-review |
| `NpcSpawnBudgetAndActivityState` | 10 | `NpcSpawnBudgetAndActivityStateComponent` plus budget definition | component/policy | slot accounting and scheduler port | lifecycle and population owner is external |
| `NpcSpawnCooldownAndEnvironment` | 10 | cooldown/environment state and event definition values | component/registry | event-day state and cooldown port | event progression owner integration-review |
| `NpcTargetAndIdentityProperties` | 16 | `NpcTargetAndIdentityPropertiesQuery` plus explicit name command/adapter | query/command/projection | target snapshot and localization boundary | `GivenName` persistence owner integration-review |
| `NpcProgressionAndEnvironmentProperties` | 9 | `NpcProgressionAndEnvironmentPropertiesQuery` plus network projection | query/projection | progression, boss registry, transport adapters | external world/protocol owners |
| `NpcSpawnContextAndCapacityInputs` | 10 | `NpcSpawnContextSnapshot` | ephemeral input | spawn evaluation port | never a durable entity component |
| `NpcSpawnSpatialEligibilityInputs` | 10 | `NpcSpawnSpatialEligibilityQuery` | pure query input | explicit tile/position snapshot | tile/world readers external |
| `NpcSpawnBiomeAndDungeonEligibilityInputs` | 6 | `NpcSpawnBiomeAndDungeonEligibilityQuery` | pure query input | zone/dungeon snapshot | world zone owner external |
| `NpcSpawnPolicyAndEventEligibilityInputs` | 11 | `NpcSpawnPolicyEligibilityQuery` | pure query input | policy/event facts | town/event/lifecycle owners external |
| `NpcSpawnBiomeZoneInputs` | 13 | `NpcSpawnBiomeZoneQuery` | pure query input | immutable zone snapshot | biome calculation owner external |
| `NpcSpawnEventAndTowerInputs` | 8 | `NpcSpawnEventAndTowerQuery` | pure query input | event/tower snapshot | event world owner external |
| `NpcSpawnTargetSelectionState` | 1 | `NpcSpawnTargetSelectionQuery` | pure query input | sentinel-safe target candidate | target commit owner external |

Every proposed type and path has `status: proposed`. The formal source group is preserved in the
mapping table even where a group contains definition values or an explicit command because a
one-field-per-component split would create false ownership and duplicate global state.

## 5. First Checkpoint: NpcSpawnAndCritterState

The first group has 13 members. Its durable per-NPC state is separated from static critter rules;
the report group remains intact for traceability, but its candidate boundary is not a license to
put static arrays or tuning constants on every entity.

| Member | C# type | Proposed classification | Proposed owner/seam | evidenceStatus |
|---|---|---|---|---|
| `maxAI` | `int` | immutable behavior-shape definition | `NpcSpawnAndCritterRules`/NPC definition catalog; validated at spawn | partial |
| `goldCritterChance` | `int` | immutable critter rule | `NpcSpawnAndCritterRules`; random input supplied by spawn policy | partial |
| `SpawnedFromStatue` | `bool` | per-NPC authoritative provenance state | `NpcSpawnAndCritterStateComponent`; spawn commit writes once, loot/network consume projections | confirmed |
| `CanBeReplacedByOtherNPCs` | `bool` | per-NPC replacement policy state | same component; replacement policy is the sole writer | confirmed |
| `dripping` | `bool` | per-NPC transient presentation/gameplay flag | same component or narrow visual-state value; owner system writes | partial |
| `drippingSlime` | `bool` | per-NPC transient presentation/gameplay flag | same component; shimmer/slime system writes through command | partial |
| `drippingSparkleSlime` | `bool` | per-NPC transient presentation/gameplay flag | same component; shimmer/slime system writes through command | partial |
| `ShimmeredTownNPCs` | `bool[]` | type-indexed transformation registry | `NpcShimmerTownRegistryAdapter`; not copied into entities | partial |
| `fireFlyFriendly` | `int` | world/critter spawn rule | immutable rule or world event input | partial |
| `fireFlyChance` | `int` | world/critter spawn rule | immutable rule or explicit spawn-policy input | partial |
| `fireFlyMultiple` | `int` | world/critter spawn rule | immutable rule or explicit spawn-policy input | partial |
| `butterflyChance` | `int` | world/critter spawn rule | immutable rule or explicit spawn-policy input | partial |
| `stinkBugChance` | `int` | world/critter spawn rule | immutable rule or explicit spawn-policy input | partial |

`SpawnedFromStatue` is deliberately not absorbed into a loot component: Version4 uses it across
network and loot-related paths. The spawn commit owns the authoritative fact; downstream partitions
receive a snapshot or event. `ShimmeredTownNPCs` is a type-indexed registry and needs an explicit
owner/invalidation policy. Source-only implementation exists at
`src/Npc/NpcSpawnAndCritterStateComponent.cs` for the two confirmed per-NPC state members; the
remaining eleven members stay unimplemented pending owner and lifecycle evidence.

### 5.1 Checkpoint execution contract

- Inputs: validated NPC type definition, statue/source provenance, shimmer registry snapshot, and
  explicit randomness supplied by the spawn policy.
- Calculation: resolve immutable critter rules and produce a spawn candidate; no registry writes,
  time reads, logging, or random calls inside the pure rule calculation.
- Commit: `NpcSpawnCommitSystem` writes per-NPC provenance/replacement state exactly once. Loot,
  network, and presentation receive output events or projections and cannot write the component.
- Reset: entity creation initializes transient flags; despawn clears entity state; global registries
  are invalidated by their own adapter, not by a query.
- Verification later: source-line mapping, duplicate commit rejection, spawn/despawn reset,
  statue provenance projection, and deterministic rule evaluation.

## 6. Second Checkpoint: NpcSpawnBudgetAndActivityState

The second group has 10 members. It contains global spawn envelope values and per-NPC slot
accounting state. Constants and scheduler configuration stay outside entity components; `npcSlots`
and `dontCountMe` remain at the explicit lifecycle/accounting seam.

| Member | C# type | Proposed classification | Proposed owner/seam | evidenceStatus |
|---|---|---|---|---|
| `safeRangeX` | `int` | spawn-envelope definition value | `NpcSpawnBudgetDefinition` read by readiness query | partial |
| `safeRangeY` | `int` | spawn-envelope definition value | `NpcSpawnBudgetDefinition` read by readiness query | partial |
| `activeRangeX` | `int` | active-window definition value | `NpcSpawnBudgetDefinition`/activity query | partial |
| `activeRangeY` | `int` | active-window definition value | `NpcSpawnBudgetDefinition`/activity query | partial |
| `npcSlots` | `float` | per-NPC population weight | existing `NpcSlotAccountingSystem` owner through an explicit NPC population component or adapter | partial |
| `noSpawnCycle` | `bool` | world scheduler gate | spawn-cycle owner; query input only | partial |
| `activeTime` | `int` | spawn/activity timing policy | spawn-cycle definition or clock-owned state | partial |
| `defaultSpawnRate` | `int` | spawn scheduler definition | `NpcSpawnSlotPolicy`/spawn-rate definition | partial |
| `defaultMaxSpawns` | `int` | spawn-cap definition | existing slot policy and allocator seam | partial |
| `dontCountMe` | `bool` | per-NPC population-accounting exemption | `NpcSlotAccountingSystem` owner; P14 must not duplicate its count | partial |

The source declarations are `NPC.cs:6043-6063` (report source sequence `1651-1659` and `1661`).
`safeRangeX/Y`, `activeRangeX/Y`, `activeTime`, `defaultSpawnRate`, and `defaultMaxSpawns` are
screen/world policy values and are not copied to every NPC. `npcSlots` is mutable population weight;
`dontCountMe` is an accounting exemption. Both require entity identity, one owner writer, and a
despawn release path. `noSpawnCycle` is a world-level gate, not an entity component.

### 6.1 Checkpoint execution contract

- Inputs: explicit player/world window snapshot, clock tick supplied by the scheduler, active NPC
  population snapshot, and spawn policy definition.
- Calculation: readiness computes range/capacity eligibility without mutating population or
  advancing the clock; it returns a candidate or rejection reason.
- Commit: the population owner applies `npcSlots` and `dontCountMe` once per entity transition.
  The slot allocator and accounting system remain the only writers of totals.
- Ordering: spawn-cycle gate -> readiness query -> slot allocation -> entity commit -> accounting
  projection. File order is not the runtime contract.
- Failure/retry: a rejected candidate consumes no slot; an ambiguous commit is reconciled by the
  slot owner using entity identity rather than blindly retried.
- Verification later: range thresholds, no-spawn gating, duplicate accounting, despawn release,
  and parity with `NpcSlotAllocator`/`NpcSlotAccountingSystem`.

No C# file or test is added by this checkpoint. The budget and activity members remain design-only
until the slot-accounting owner, entity lifecycle, and scheduler APIs are confirmed; adapter
construction, duplicate accounting, despawn release, and runtime integration remain unverified.

## 15. Eleventh Checkpoint: NpcSpawnEventAndTowerInputs

The eleventh group has 8 boolean event-area inputs. They are read-only facts for event/tower spawn
eligibility and remain owned by event-world systems. They do not create a tower registry or mutate
event progress from a query.

| Member | C# type | Proposed classification | Proposed owner/seam | evidenceStatus |
|---|---|---|---|---|
| `ZoneTowerSolar` | `bool` | tower-region eligibility input | `NpcSpawnEventAndTowerQuery` over event-zone snapshot | partial |
| `ZoneTowerVortex` | `bool` | tower-region eligibility input | same query/snapshot | partial |
| `ZoneTowerNebula` | `bool` | tower-region eligibility input | same query/snapshot | partial |
| `ZoneTowerStardust` | `bool` | tower-region eligibility input | same query/snapshot | partial |
| `ZoneOldOneArmy` | `bool` | event-region eligibility input | Old One Army event snapshot | partial |
| `ZoneWaterCandle` | `bool` | candle spawn-modifier input | world/candle effect snapshot | partial |
| `ZonePeaceCandle` | `bool` | candle spawn-modifier input | world/candle effect snapshot | partial |
| `ZoneShadowCandle` | `bool` | candle spawn-modifier input | world/candle effect snapshot | partial |

Tower and Old One Army flags can affect spawn selection but do not imply that P14 owns the event
state, shield progression, or generated event NPC lifecycle. Candle flags are modifiers sampled from
world/player effects, not mutable fields on the candidate NPC. The source boundary is
`NPC.cs:141-155` (report source sequence `1569-1576`).

### 15.1 Checkpoint execution contract

- Inputs: one versioned event/tower/candle-area snapshot aligned with the candidate position and
  world tick.
- Calculation: evaluate event/tower/candle predicates deterministically and pass results to policy
  and candidate queries; no event progress, candle effect, or tower registry writes occur.
- Commit: event owners commit progression and generated NPC state; spawn commit consumes the
  candidate through the existing event/lifecycle seams.
- Ordering: event/world adapter -> event/tower query -> policy/target query -> spawn candidate ->
  event/lifecycle commit -> network/projection outputs.
- Failure/retry: stale event versions reject the evaluation; retry obtains a complete new snapshot.
  Duplicate event messages are deduplicated by event identity outside P14.
- Verification later: each tower and event flag, candle modifier combinations, stale event version,
  no progression writes, and cross-partition handoff parity.

### 15.2 Implementation checkpoint

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

## 14. Tenth Checkpoint: NpcSpawnBiomeZoneInputs

The tenth group has 13 boolean biome/zone flags. They are a compact immutable zone snapshot for
spawn rule evaluation. They should be calculated once from world/tile state and passed to queries;
they are not copied to NPC entities and do not mutate biome registries.

| Member | C# type | Proposed classification | Proposed owner/seam | evidenceStatus |
|---|---|---|---|---|
| `ZoneCorrupt` | `bool` | biome-zone input | `NpcSpawnBiomeZoneQuery` over immutable zone snapshot | partial |
| `ZoneCrimson` | `bool` | biome-zone input | same query/snapshot | partial |
| `ZoneHallow` | `bool` | biome-zone input | same query/snapshot | partial |
| `ZoneJungle` | `bool` | biome-zone input | same query/snapshot | partial |
| `ZoneSnow` | `bool` | biome-zone input | same query/snapshot | partial |
| `ZoneGlowshroom` | `bool` | biome-zone input | same query/snapshot | partial |
| `ZoneMeteor` | `bool` | biome-zone input | same query/snapshot | partial |
| `ZoneGraveyard` | `bool` | biome-zone input | same query/snapshot | partial |
| `ZoneDungeon` | `bool` | biome-zone input | same query/snapshot | partial |
| `ZoneLihzhardTemple` | `bool` | biome-zone input | same query/snapshot | partial |
| `ZoneGranite` | `bool` | biome-zone input | same query/snapshot | partial |
| `ZoneMarble` | `bool` | biome-zone input | same query/snapshot | partial |
| `ZoneSandstorm` | `bool` | weather/biome-zone input | weather/biome snapshot; sandstorm owner external | partial |

The capitalization and legacy spelling are retained at the compatibility boundary. A zone query
must not call mutable global zone state for each member; it consumes one versioned snapshot so the
13 flags cannot describe different world ticks. The source boundary is `NPC.cs:115-139` (report
source sequence `1556-1568`).

### 14.1 Checkpoint execution contract

- Inputs: one world/tile/biome/weather zone snapshot with a version and candidate position.
- Calculation: project all zone flags deterministically and feed the policy/eligibility queries;
  no zone registry writes, tile writes, clocks, or random calls occur.
- Commit: no durable zone component is created for the NPC. A successful candidate proceeds to spawn
  commit; the zone snapshot expires after evaluation.
- Ordering: world zone adapter -> zone projection/query -> spatial/biome/policy evaluation -> spawn
  candidate/commit. Versions must remain coherent across the chain.
- Failure/retry: stale or incomplete zone snapshots cause explicit rejection and fresh sampling;
  partial flag arrays are not cached or reused.
- Verification later: all 13 flag mappings, cross-zone exclusivity/combination cases, sandstorm
  weather boundary, snapshot versioning, and no-world-write proof.

### 14.2 Implementation checkpoint

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

## 13. Ninth Checkpoint: NpcSpawnPolicyAndEventEligibilityInputs

The ninth group has 11 boolean/integer `NPC.Spawner` fields. It represents policy inputs assembled
from town population, event state, worm/spider rules, safe-wall policy, wind direction, Tim status,
and starting health. These are facts supplied to a pure policy query; they are not a second town,
event, or lifecycle authority.

| Member | C# type | Proposed classification | Proposed owner/seam | evidenceStatus |
|---|---|---|---|---|
| `townNPCs` | `int` | town-population input | town progression snapshot; lifecycle/town owner external | partial |
| `skyMob` | `bool` | spawn policy input | sky/event policy query | partial |
| `noWorms` | `bool` | enemy policy input | spawn policy definition/event snapshot | partial |
| `noGroundWorms` | `bool` | enemy policy input | spawn policy definition/event snapshot | partial |
| `invaders` | `bool` | invasion policy input | invasion/event snapshot and cap policy | existing-evidence |
| `spawnFriendly` | `bool` | disposition policy input | spawn policy query; generated NPC disposition owner external | partial |
| `ignoreSafeWalls` | `bool` | wall policy input | safe-wall policy snapshot | partial |
| `spawnSpider` | `bool` | special spawn policy input | spider/event policy snapshot | partial |
| `isSpawningInWindDirection` | `bool` | wind-direction policy input | weather/wind snapshot | partial |
| `offensiveToTim` | `bool` | NPC-specific event policy input | Tim/event policy adapter | partial |
| `playerHasStartingHealth` | `bool` | player progression eligibility input | player readiness snapshot | existing-evidence |

Policy inputs must be evaluated with the same world/player version as the spatial and biome
snapshots. `townNPCs` is a count input, not a town registry; `invaders` and cap facts remain with
the event owner. `spawnFriendly` changes candidate disposition through the later spawn commit and
must not make the policy query mutate an NPC. The source boundary is `NPC.cs:61-113` (report source
sequence `1529-1535`, `1539`, `1547`, and `1554-1555`).

Source-only implementation exists at `src/Npc/NpcSpawnPolicyEligibilitySnapshot.cs`. It stores the
eleven explicit policy/event inputs, maps `townNPCs` to `TownNpcCount`, and does not read or write
town, event, weather, wall, player, or lifecycle state; adapter construction, stale-version
handling, query rules, and runtime integration remain unverified.

### 13.1 Checkpoint execution contract

- Inputs: town/player readiness, event/invasion, sky/worm/spider, safe-wall, wind, Tim, and starting
  health snapshots plus the candidate spatial/biome version.
- Calculation: evaluate policy predicates and return candidate/rejection reasons; no query writes
  town counts, event flags, wall tiles, wind state, or candidate disposition.
- Commit: the spawn owner applies friendly/hostile disposition and event-specific generated state;
  town/event/lifecycle owners remain the writers of source facts.
- Ordering: coherent spatial/biome facts -> policy query -> spawn candidate -> slot/lifecycle/event
  commit -> projections.
- Failure/retry: mismatched versions reject the evaluation and resample all dependent snapshots;
  duplicate invasion inputs are handled by event identity in the event owner.
- Verification later: town count thresholds, invasion cap combinations, wall/worm/spider policy,
  wind direction, Tim and starting-health conditions, and no-source-write proof.

## 12. Eighth Checkpoint: NpcSpawnBiomeAndDungeonEligibilityInputs

The eighth group has 6 boolean `NPC.Spawner` fields. It narrows spatial eligibility to water,
special underground structures, and dual-dungeon policy. The fields remain ephemeral query inputs;
the world zone/dungeon owners supply the snapshot and retain authority over their source state.

| Member | C# type | Proposed classification | Proposed owner/seam | evidenceStatus |
|---|---|---|---|---|
| `waterTile` | `bool` | water-tile eligibility input | `NpcSpawnBiomeAndDungeonEligibilityQuery` over tile snapshot | partial |
| `nearGranite` | `bool` | special-biome proximity input | same query over structure/biome snapshot | partial |
| `nearMarble` | `bool` | special-biome proximity input | same query over structure/biome snapshot | partial |
| `dualDungeonsSpawnRules` | `bool` | world-layout rule input | dual-dungeon policy adapter | partial |
| `inDualDungeon` | `bool` | candidate region input | dual-dungeon region snapshot | partial |
| `tresspassingDualDungeon` | `bool` | policy violation/input flag | dual-dungeon policy query; preserve source spelling at compatibility boundary | partial |

`waterTile`, Granite/Marble proximity, and dual-dungeon facts must be sampled consistently with
the spatial snapshot. `tresspassingDualDungeon` is a policy input, not a command to mutate the
world or candidate. No dual-dungeon registry is introduced by this group. The source boundary is
`NPC.cs:75-107` (report source sequence `1536-1538` and `1550-1552`).

### 12.1 Checkpoint execution contract

- Inputs: immutable candidate tile/region snapshot, water and structure facts, and dual-dungeon
  world-layout/policy snapshot.
- Calculation: evaluate water, Granite/Marble, and dual-dungeon rules deterministically; report a
  rejection reason without mutating zone or dungeon state.
- Commit: only the later spawn owner creates the NPC. These booleans are discarded after evaluation.
- Ordering: spatial snapshot -> biome/dungeon query -> policy/event query -> candidate -> spawn
  commit; source versions must match.
- Failure/retry: inconsistent tile/zone versions produce an explicit stale-input failure and a new
  snapshot; no partial dungeon flags are reused.
- Verification later: water and special-biome boundaries, dual-dungeon policy combinations, source
  spelling compatibility, version mismatch, and no-world-write proof.

Source-only implementation exists at `src/Npc/NpcSpawnBiomeAndDungeonEligibilitySnapshot.cs`. It
stores the six explicit inputs, preserves the Version4 compatibility spelling, and does not read or
write tile/world/dungeon state; adapter construction, stale-version handling, query rules, and
runtime integration remain unverified.

## 11. Seventh Checkpoint: NpcSpawnSpatialEligibilityInputs

The seventh group has 10 boolean `NPC.Spawner` fields. They are a spatial qualification snapshot
derived from the candidate location, player position, tile scan, and world rules. They do not form
durable entity components and are not authoritative claims that an NPC has spawned.

| Member | C# type | Proposed classification | Proposed owner/seam | evidenceStatus |
|---|---|---|---|---|
| `surfaceSpawn` | `bool` | spatial eligibility input | `NpcSpawnSpatialEligibilityQuery` over position/terrain snapshot | partial |
| `spawnUndergroundDesert` | `bool` | spatial eligibility input | same query; desert depth facts explicit | partial |
| `hardDungeon` | `bool` | dungeon/spatial eligibility input | same query with dungeon snapshot; final dungeon policy remains external | partial |
| `deeperThanRockLayer` | `bool` | depth eligibility input | explicit tile-depth snapshot | partial |
| `underGround` | `bool` | depth eligibility input | explicit position/rock-layer snapshot | partial |
| `isOcean` | `bool` | ocean-region eligibility input | explicit world-region snapshot | partial |
| `isBeach` | `bool` | beach-region eligibility input | explicit world-region snapshot | partial |
| `skyBehindPlayer` | `bool` | sky/background eligibility input | explicit tile/background snapshot | partial |
| `livingTree` | `bool` | structure-region eligibility input | explicit tile/structure snapshot | partial |
| `inRemixStartingArea` | `bool` | world-layout eligibility input | explicit world-layout snapshot | partial |

The query must use a single position/tile snapshot for one candidate. It should not rescan mutable
world state between predicates or cache flags beyond the evaluation lifetime. A candidate rejected
because the snapshot is stale is re-evaluated; the flags are not persisted on an entity. The source
boundary is `NPC.cs:83-109` (report source sequence `1540-1549`).

### 11.1 Checkpoint execution contract

- Inputs: candidate position, player position/window, tile/structure scan result, depth layers,
  ocean/beach/sky/tree regions, dungeon facts, and world-layout version.
- Calculation: evaluate all ten predicates against one immutable snapshot and return qualification
  results/reasons. The query has no tile writes, random calls, logging, or network effects.
- Commit: only a later spawn commit can create an NPC. Spatial flags are discarded after candidate
  evaluation and do not become NPC components.
- Ordering: world/tile adapter snapshot -> spatial query -> biome/policy queries -> candidate ->
  spawn commit. Snapshot version is checked at the boundary.
- Failure/retry: tile/world read failure or stale version is an explicit evaluation failure; retry
  obtains a fresh snapshot and does not reuse partial flags.
- Verification later: boundary tile/depth cases, ocean/beach separation, sky/tree detection, Remix
  region, stale snapshot rejection, and proof of no world writes.

No C# file or test is added by this checkpoint.

## 10. Sixth Checkpoint: NpcSpawnContextAndCapacityInputs

The sixth group has 10 `NPC.Spawner` fields. These values are assembled for one spawn attempt and
must remain an immutable `NpcSpawnContextSnapshot` or equivalent value payload. They are not
durable ECS components and do not survive a rejected or completed spawn evaluation.

| Member | C# type | Proposed classification | Proposed owner/seam | evidenceStatus |
|---|---|---|---|---|
| `spawnSpaceX` | `int` | spawn geometry definition input | `NpcSpawnContextSnapshot`; initialized from spawn policy | partial |
| `spawnSpaceY` | `int` | spawn geometry definition input | `NpcSpawnContextSnapshot`; initialized from spawn policy | partial |
| `fairyLog` | `bool` | diagnostic/compatibility evaluation flag | spawn adapter input; logging policy remains outside pure query | partial |
| `numberOfActivePlayers` | `int` | derived capacity input | active-player snapshot adapter | partial |
| `reachedInvasionBossCap` | `bool` | event-capacity input | `NpcInvasionBossCapPolicy`/event snapshot | existing-evidence |
| `pX` | `int` | player-position input | player readiness snapshot; tile coordinate conversion is explicit | existing-evidence |
| `pY` | `int` | player-position input | player readiness snapshot; tile coordinate conversion is explicit | existing-evidence |
| `luck` | `float` | player luck input | player luck snapshot; random selection remains an explicit policy dependency | partial |
| `dayTime` | `bool` | world clock input | world-time snapshot; clock owner supplies value | existing-evidence |
| `raining` | `bool` | weather input | weather snapshot; weather owner supplies value | existing-evidence |

`spawnSpaceX/Y` are policy values, not per-NPC state. `numberOfActivePlayers`, `reachedInvasionBossCap`,
player coordinates, luck, daytime, and rain are sampled inputs whose validity is bounded by the
spawn evaluation. `fairyLog` is a diagnostic/compatibility flag and must not make a pure eligibility
query log or otherwise publish an effect; any diagnostic output is owned by the outer adapter.
The source boundary is `NPC.cs:39-59` (report source sequence `1519-1528`).

### 10.1 Checkpoint execution contract

- Inputs: player readiness, active-player count, invasion-cap policy, world clock/weather snapshot,
  luck value, spawn geometry definition, and optional diagnostic flag.
- Calculation: materialize one immutable `NpcSpawnContextSnapshot`; eligibility queries consume it
  and return a candidate or rejection reason. No query reaches `Main`, the clock, RNG, or logger.
- Commit: the outer `NpcSpawnCommitSystem` accepts a valid candidate and passes a separate command
  to lifecycle/slot owners. The context is discarded after evaluation.
- Ordering: adapters sample facts -> context snapshot -> spatial/biome/policy queries -> candidate ->
  slot/lifecycle commit -> optional diagnostic projection.
- Failure/retry: a stale snapshot rejects or is re-evaluated with a fresh snapshot; no old context
  is replayed after world/player version changes.
- Verification later: snapshot immutability, active-player/capacity boundaries, position conversion,
  clock/weather sampling, luck propagation, and absence of durable SpawnInfo registration.

Source-only implementation exists at `src/Npc/NpcSpawnContextSnapshot.cs`. It is an immutable
value payload and is not registered as durable entity state; adapter construction, version/retry
behavior, and runtime integration remain unverified.

## 9. Fifth Checkpoint: NpcProgressionAndEnvironmentProperties

The fifth group has 9 properties. It is a read/projection boundary over world progression,
weather, NPC definitions, presentation alpha, and network section calculation. Two source members
are intentionally not treated as pure queries: `Opacity` writes `alpha` through its setter and
`IsMechQueenUp` repairs the stale `mechQueen` registry while reading.

| Member | C# type | Proposed classification | Proposed owner/seam | evidenceStatus |
|---|---|---|---|---|
| `DownedAnyPreHardmodeBoss` | `bool` | world progression query | progression snapshot/query; boss defeat flags remain external owner | partial |
| `ShieldStrengthTowerMax` | `int` | tower progression query | tower/world progression snapshot; derives from shield baseline and Moon Lord state | partial |
| `Opacity` | `float` | derived value plus compatibility write command | `NpcOpacityQuery` reads alpha; `NpcOpacityCommand` routes setter write to presentation/entity owner | confirmed |
| `TreatedAsABossForRainbowBoulders` | `bool` | definition-backed combat classification query | NPC type/boss catalog and `boss` input | partial |
| `isLikeATownNPC` | `bool` | derived NPC classification query | NPC type/town state snapshot; no second town owner | confirmed |
| `IsMechQueenUp` | `bool` | registry status query with repair command | `MechQueenRegistryOwner`; stale index repair is explicit effect, not hidden pure read | confirmed |
| `TooWindyForButterflies` | `bool` | pure weather eligibility query | explicit wind snapshot; threshold `abs(windSpeedTarget) >= 0.4f` | confirmed |
| `CountsAsACritter` | `bool` | pure critter classification query | explicit life/damage/type snapshot and catalog exceptions | confirmed |
| `NetSectionCoordinates` | `Point` | network section projection | `NpcNetworkSectionProjection` over position and section mapper; transport owns packet use | confirmed |

`DownedAnyPreHardmodeBoss` depends on world progression and hardmode state, while
`ShieldStrengthTowerMax` depends on tower baseline and Moon Lord defeat state; neither creates a
second progression registry. `Opacity` may calculate `1 - alpha / 255` purely, but its setter
changes `alpha`, so an ECS query cannot expose it as an unrestricted mutable property. `IsMechQueenUp`
returns true for a valid active type-127 NPC and otherwise clears `mechQueen`; the replacement must
return a status plus an explicit repair action or let the registry owner perform that repair.

`NetSectionCoordinates` is a transport projection derived from position using the section mapper;
it does not own position, section visibility, or network serialization. The source range is
`NPC.cs:6622-6714` (report source sequence `1902-1910`).

### 9.1 Checkpoint execution contract

- Inputs: immutable progression, tower, boss, weather, NPC definition, alpha, position, and network
  section-map snapshots.
- Calculation: progression, weather, boss classification, critter classification, opacity read,
  and section coordinate results are deterministic over explicit inputs.
- Commit: `NpcOpacityCommand` is handled by the presentation/entity owner; a stale mech-queen index
  is repaired only by the registry owner. Queries cannot mutate alpha or global registry state.
- Ordering: world progression/weather facts -> pure property queries -> explicit repair/write command
  -> network/presentation projection. Packet emission is downstream of committed facts.
- Failure/retry: section projection failure is an adapter/transport error, not a spawn rejection;
  repair commands are idempotent for the same registry version and must not clear a newly assigned
  index.
- Verification later: hardmode/boss thresholds, tower shield halving, opacity round-trip/clamping,
  stale registry repair, wind threshold, critter exceptions, and section coordinate mapping.

No C# file or test is added by this checkpoint.

## 8. Fourth Checkpoint: NpcTargetAndIdentityProperties

The fourth group has 16 properties. The read-only target and identity results are grouped as a
query over explicit NPC/player/definition snapshots. Name values and display dimensions remain
external boundaries: `GivenName` has a setter with normalization side effects, while localization
and viewport configuration must not become gameplay authority.

| Member | C# type | Proposed classification | Proposed owner/seam | evidenceStatus |
|---|---|---|---|---|
| `CanTalk` | `bool` | pure interaction eligibility query | `NpcTargetAndIdentityPropertiesQuery` over NPC state and type catalog | partial |
| `CanBeTalkedTo` | `bool` | pure interaction eligibility query | same query; velocity/AI facts are explicit inputs | partial |
| `HasValidTarget` | `bool` | validated target query | target snapshot plus player/NPC liveness projection | partial |
| `HasPlayerTarget` | `bool` | sentinel/range classification query | target encoding value object; player index range is explicit | confirmed |
| `HasNPCTarget` | `bool` | sentinel/range classification query | target encoding value object; NPC index range is explicit | confirmed |
| `SupportsNPCTargets` | `bool` | immutable type capability query | NPC definition/catalog lookup | partial |
| `TranslatedTargetIndex` | `int` | normalized target projection | target query returns validated player/NPC index; preserves legacy sentinel semantics | partial |
| `WhoAmIToTargetingIndex` | `int` | target-encoding projection | NPC identity/target value object; not a second identity authority | partial |
| `IsShimmerVariant` | `bool` | definition-backed variant query | town transformation catalog plus `townNpcVariationIndex` input | partial |
| `TypeName` | `string` | localization projection | localization adapter keyed by `netID`; no string in authoritative spawn state | partial |
| `FullName` | `string` | localized display projection | name query plus localization adapter; `GivenName` remains an input | partial |
| `HasGivenName` | `bool` | pure name-state query | explicit NPC name snapshot | partial |
| `GivenOrTypeName` | `string` | display-name projection | name query with localization fallback | partial |
| `GivenName` | `string` | compatibility value with explicit write command | `NpcGivenNameCommand`/persistence adapter; normalize null to empty at command boundary | partial |
| `sWidth` | `int` | viewport definition value | `NpcViewportDefinition`/configuration adapter; not entity state | partial |
| `sHeight` | `int` | viewport definition value | `NpcViewportDefinition`/configuration adapter; not entity state | partial |

The target encoding remains explicit: `HasPlayerTarget` recognizes `[0, 255)`, `HasNPCTarget`
recognizes `[300, 300 + maxNPCs)`, and `TranslatedTargetIndex` must not silently reinterpret
invalid or sentinel values. `HasValidTarget` may read liveness snapshots, but it must not clear a
target or mutate `Main.player`/`Main.npc`. `WhoAmIToTargetingIndex` is a projection into the
legacy target namespace, not a globally unique entity ID.

`TypeName`, `FullName`, and `GivenOrTypeName` are output projections. `GivenName` is not modeled as
a freely mutable query result because the source setter writes `_givenName = value ?? ""`; it needs
an explicit owner, validation/normalization rule, persistence decision, and network decision. The
source range is `NPC.cs:6486-6620` (report source sequence `1886-1901`).

### 8.1 Checkpoint execution contract

- Inputs: immutable NPC motion/AI/identity snapshot, target encoding, player/NPC liveness snapshot,
  type catalog, town-variant catalog, name value, localization service, and viewport definition.
- Calculation: target validity, interaction eligibility, variant state, and fallback display names
  are deterministic over supplied inputs; localization is an adapter effect/read boundary.
- Commit: a validated `NpcGivenNameCommand` is accepted by the NPC identity/persistence owner;
  the query and localization projection cannot write `_givenName` or any target field.
- Ordering: identity/liveness snapshot -> pure target/name query -> optional name command commit ->
  localization/transport projection. No file order is implied.
- Failure/retry: localization failure returns a stable adapter error/fallback policy; name-command
  retry requires entity/version identity and must not duplicate persistence writes.
- Verification later: sentinel boundaries, stale/dead target rejection, town-pet talk rules,
  localized fallback, null-name normalization, and proof that read queries have no writes.

No C# file or test is added by this checkpoint.

## 7. Third Checkpoint: NpcSpawnCooldownAndEnvironment

The third group has 10 members. It combines event-wave lookup data, daily kill markers, spawn-slot
protection, player-interaction suppression, ladybug luck/rain timers, and offset delay. The
decomposition keeps immutable lookup/constants separate from mutable world/day state and avoids
making a timer a property of an individual NPC.

| Member | C# type | Proposed classification | Proposed owner/seam | evidenceStatus |
|---|---|---|---|---|
| `MoonEventRequiredPointsPerWaveLookup` | `int[]` | immutable event definition lookup | moon-event definition/catalog adapter; expose read-only snapshot | partial |
| `EoCKilledToday` | `bool` | world/day event state | event-day progression owner; P14 consumes a snapshot | partial |
| `WoFKilledToday` | `bool` | world/day event state | event-day progression owner; P14 consumes a snapshot | partial |
| `SPAWN_SLOT_PROTECTION_TIME` | `int` | immutable spawn policy constant | spawn policy definition; not entity state | confirmed |
| `ignorePlayerInteractions` | `int` | world/global interaction suppression timer | interaction/event state owner; explicit tick input | partial |
| `ladyBugGoodLuckTime` | `int` | luck-duration policy value | critter/luck definition or world policy adapter | partial |
| `ladyBugBadLuckTime` | `int` | luck-duration policy value | critter/luck definition or world policy adapter | partial |
| `ladyBugRainTime` | `int` | rain luck-duration policy value | weather/luck definition or world policy adapter | partial |
| `maximumAmountOfTimesLadyBugRainCanStack` | `int` | bounded timer policy value | weather/luck definition; validate against `ladyBugRainTime` | partial |
| `offSetDelayTime` | `int` | spawn offset delay policy | spawn scheduler definition/clock seam | partial |

The lookup array is a definition, not a mutable component collection. Daily kill flags and
`ignorePlayerInteractions` are world/day state with explicit writers and reset boundaries. The
remaining duration values and `SPAWN_SLOT_PROTECTION_TIME` are definitions; any runtime countdown
must be owned by the event, weather, or spawn-cycle system that consumes it. The source property
group is `NPC.cs:6461-6484` (report source sequence `1860-1869`).

### 7.1 Checkpoint execution contract

- Inputs: immutable event definition, explicit world-day snapshot, weather/luck snapshot, and clock
  tick supplied by the owning scheduler.
- Calculation: wave thresholds and eligibility are pure over those snapshots; arrays are exposed as
  immutable/read-only values and cannot be mutated through a query.
- Commit: event-day, interaction suppression, and countdown owners apply changes with explicit tick
  ordering. P14 does not write `EoCKilledToday`, `WoFKilledToday`, or global timers from eligibility.
- Failure/retry: a rejected spawn does not consume a protection interval; repeated event messages
  are deduplicated by event/day identity in the owning event system.
- Verification later: lookup immutability, day reset, timer bounds, protection-window boundaries,
  and no duplicate writes from spawn queries.

No C# file or test is added by this checkpoint.

## 16. Dependency and Effect Rules

The intended direction is:

```text
world/player/tile/event adapters
  -> immutable SpawnContextSnapshot and zone snapshots
  -> pure eligibility and target queries
  -> spawn candidate / explicit command
  -> spawn commit and lifecycle/slot owners
  -> event, loot, network, persistence, localization, and presentation projections
```

Queries may depend on read-only snapshots and definitions. They may not depend on mutable ECS
handles, global `Main` lookups, current time, random APIs, logging, network writers, or registry
cleanup. Adapters may read those external services, but the effect register must state ownership,
ordering, failure, and duplicate semantics. The migration must not infer execution order from file
or directory order.

## 17. Compatibility, Non-Split Items, and Integration Handoff

- Preserve Version4 names, namespaces, public signatures, and external type boundaries behind an
  adapter during migration. Introduce one owner writer before removing a legacy write path; do not
  dual-write authority.
- Keep the existing NLTX spawn eligibility/readiness/commit seams as partial evidence. Extend or
  narrow them only after call-site and lifecycle evidence is closed; do not create a second slot
  allocator, lifecycle system, or target authority.
- Do not split `Spawner` fields into durable components. Split their pure calculation inputs into
  snapshots only when a stable query boundary and focused verifier exist.
- Do not merge target identity with localization, `Opacity` with a pure derived query, or network
  section coordinates with authoritative NPC position. Keep projection and compatibility adapters
  one-way.
- Generated NPC lifecycle, town NPC progression, boss/event world state, persistence, localization,
  and network transport are explicit integration-review handoffs. P14 owns neither their hidden
  writer nor their protocol format.
- New files, if later approved, follow the ECS organization constraint: domain-first `Npc/` paths,
  one core public type per same-named PascalCase file, no generic `Shared/Components/`, and no
  namespace change caused only by a path move.

## 18. Verification Status

The C01 implementation checkpoint compiled successfully. A focused source-level check constructed
the component with both flag combinations and confirmed both public values are preserved. The
existing NPC verifier project was rebuilt successfully, and the current serial focused verifier
command exited `0` with output `PASS: NPC and town component field composition`. No runtime behavior
comparison, network replay, or persistence restore was run. C06-C09 were rebuilt or checked through the compiled
assembly with all ten, ten, six, and eleven snapshot values respectively preserved. C10 and C11
were rebuilt by the serial NPC command with `0` warnings and `0` errors, and the combined compiled-
assembly focused check preserved all thirteen and eight values respectively. Adapter construction,
version/expiry handling, runtime integration, and behavior equivalence remain unverified. C12 was
also rebuilt by the serial NPC command with `0` warnings and `0` errors; its compiled-assembly
focused check preserved sentinel `255` and an explicit target value. Target routing, stale-input
handling, and runtime integration remain unverified.
The implementation state is therefore:

```text
executionStatus: planned
implementationStatus: partial
verificationStatus: partial
```

All 12 leaf groups and the complete 117-member source-sequence mapping are recorded below. This
is still a proposed decomposition except for the source-only C01 value component and C06-C12 snapshots,
with focused source checks only; no behavior-equivalence, runtime integration, network replay, or
persistence restore was run. Settlement must use the claimed
`partitionId: P14` and exact session ID; no other session document, authoritative report, or ledger
may be changed.

## 19. Twelfth Checkpoint: NpcSpawnTargetSelectionState

The final group contains one `NPC.Spawner` field. `defaultTarget` is the initial target sentinel
used while a spawn candidate is evaluated. It is a value input to target selection, not durable NPC
target authority and not a replacement for the target routing/lifecycle systems.

| Member | C# type | Proposed classification | Proposed owner/seam | evidenceStatus |
|---|---|---|---|---|
| `defaultTarget` | `int` | sentinel-valued target-selection input | `NpcSpawnTargetSelectionQuery` returns a candidate; target commit/routing owner applies result | partial |

The default value `255` preserves the legacy “no selected player target” sentinel while target
selection may produce a player or NPC target according to explicit routing rules. The query must
not write the NPC target field, inspect mutable global entities after snapshot construction, or turn
the sentinel into a durable component. Existing `NpcTargetSelectionSystem` and
`NpcTargetRoutingSystem` are partial current seams; generated entity lifecycle remains outside P14.
The source declaration is `NPC.cs:157` (report source sequence `1577`).

### 19.1 Checkpoint execution contract

- Inputs: default sentinel, eligible player/NPC target snapshot, target policy, and source/world
  version.
- Calculation: return a stable target candidate or explicit no-target result; preserve `255` and
  NPC target offset encoding without hidden writes.
- Commit: target routing/commit owner applies a validated result after the NPC entity exists; the
  spawn query does not mutate target state.
- Ordering: spawn context -> target eligibility/routing query -> spawn/lifecycle commit -> target
  commit/projection. The exact system schedule is explicit, not file-order based.
- Failure/retry: stale target snapshots return no candidate or require fresh evaluation; repeated
  target commands are deduplicated by entity/version identity.
- Verification later: sentinel preservation, player/NPC target encoding, stale target rejection,
  no-query-write proof, and handoff to existing routing systems.

### 19.2 Implementation checkpoint

- Actual file: `src/Npc/NpcSpawnTargetSelectionSnapshot.cs`.
- Implemented behavior: stores the one Version4 `defaultTarget` input as an immutable
  spawn-evaluation snapshot, preserving the explicit sentinel value `255` when supplied by the
  caller and allowing explicit target values to be carried without transformation.
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

## 20. Complete 117-Member Mapping Index

The authoritative report remains the complete declaration table, including original source paths,
lines, types, declarations, and source sequence. This index proves that every member is assigned to
exactly one completed leaf group; the per-group tables above record the ownership classification for
each row.

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

The index is source-sequence based so it can be compared mechanically with the authoritative
report. No member is absorbed into another partition. Generated NPC lifecycle, town progression,
loot, event progression, persistence, transport, and presentation remain explicit
cross-partition/integration-review boundaries.
