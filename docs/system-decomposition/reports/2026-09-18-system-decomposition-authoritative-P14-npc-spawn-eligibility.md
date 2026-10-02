# System Decomposition Report: authoritative P14

## Scope and Evidence

| Item | Value |
| --- | --- |
| `taskSetName` | `authoritative-system-decomposition` |
| `partitionId` | `P14` |
| `sessionId` | `cae65855b1f9451ba3241f48fe20d1c8` |
| `designStatus` | `proposed` |
| `migrationStatus` | `proposed` |
| `verificationStatus` | `not-run` |
| claimed scope | 12 leaf groups, 92 fields, 25 properties, 117 members |
| input ledger | `D:\TRbackup\NLTX\docs\migration\ledgers\authoritative-20-partitions\P14-NPC-Spawn-Eligibility.md` |
| source project | `D:\TRbackup\Version4` |
| reference project | `C:\Users\shan\Downloads\ECS\space-station-14-master` |
| migration tree inspected | `D:\TRbackup\NLTX\src\NSSLC` |
| authorized output | this file only |

This is a read-only static System boundary proposal. No Version4 source, NLTX production
code, tests, project files, input ledger, prompt, CPG database, or another partition report was
modified. Runner `Complete`, when settled, means only that this document task was settled; it
does not mean that a System exists, that behavior is equivalent, or that migration succeeded.

Evidence used:

- Version4 source: `Terraria/NPC.cs` (the `NPC.Spawner` declaration and spawn pipeline),
  `Terraria/Main.cs`, and `Terraria/MessageBuffer.cs`.
- Read-only CPG Query API against
  `D:\TRbackup\Version4-cpg-export\out-dop8-interproc.sqlite`, manifest
  `6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`:
  `Find-CpgSymbols`, `Get-CpgTypeSurface`, `Find-CpgCallSites`, `Get-CpgMemberUses`, and
  `Get-CpgCallableFacts`.
- Current NLTX evidence: the existing `Npc*` snapshots/components, `NpcTargetComponent`,
  `NpcLifecycleComponent`, `SpawnAdmissionState`, `WorldSpawnPressureState`, and
  `WorldNpcSpawnPacingState` under `src/NSSLC`.
- SS14 reference: `StationSpawningSystem`, `SpawnerSystem`, shared station spawning, and
  `NPCSystem`, used only to compare ECS organization vocabulary and not as behavior evidence.

CPG source-excerpt lookup for `Terraria/NPC.cs` returned `unknown` because the database has no
adjacent source tree. CPG symbol and positive call-site results are therefore evidence for the
reported identities only. Partial call-graph results, unknown member access modes, unexpanded
callee effects, aliases, dynamic dispatch, and missing source snapshots remain unresolved.

## Prior Component Decomposition Reconciliation

The input ledger is a source-inventory partition, not an ownership proof. The existing NLTX
types are useful proposals/current evidence but are not runtime integration evidence:

| Existing NLTX material | Reconciliation |
| --- | --- |
| `NpcSpawnAndCritterStateComponent` | Current partial state shape; ownership and write closure remain unknown. |
| `NpcSpawnContextSnapshot`, `NpcSpawnSpatialEligibilitySnapshot`, `NpcSpawnBiomeAndDungeonEligibilitySnapshot`, `NpcSpawnPolicyEligibilitySnapshot`, `NpcSpawnBiomeZoneEligibilitySnapshot`, `NpcSpawnEventAndTowerEligibilitySnapshot` | Candidate immutable inputs; they must be versioned per spawn attempt and cannot be treated as proof that the legacy `Spawner` fields are no longer written. |
| `NpcSpawnTargetSelectionSnapshot` | Candidate value result; default target commit and cross-NPC targeting authority are unproven. |
| `NpcTargetComponent` | Existing target representation; it does not establish the P14 owner of legacy `target`, `GivenName`, or target invalidation. |
| `NpcLifecycleComponent` | Adjacent lifecycle evidence only. P14 must hand a successful spawn to the lifecycle owner and must not create a second lifecycle owner. |
| `SpawnAdmissionState` | Candidate admission/decision representation; no Version4 adapter or command consumer was found in this review. |
| `WorldSpawnPressureState` and `WorldNpcSpawnPacingState` | Candidate world pacing/pressure state; active-player, NPC-slot, event, and timer writers are not closed. |

The prior decomposition correctly identifies useful seams, but all runtime statuses remain
`partial` or `unknown`. No proposed type is labeled as existing or behavior-equivalent.

Complete claimed-member coverage (names are preserved from the input ledger; the source
sequence range is a boundary marker, not a contiguous-source claim):

| Leaf group | Source sequence range | Count | Complete member names |
| --- | ---: | ---: | --- |
| `NpcSpawnAndCritterState` | 1596-1629 | 13 | `maxAI`, `goldCritterChance`, `SpawnedFromStatue`, `CanBeReplacedByOtherNPCs`, `dripping`, `drippingSlime`, `drippingSparkleSlime`, `ShimmeredTownNPCs`, `fireFlyFriendly`, `fireFlyChance`, `fireFlyMultiple`, `butterflyChance`, `stinkBugChance` |
| `NpcSpawnBudgetAndActivityState` | 1651-1661 | 10 | `safeRangeX`, `safeRangeY`, `activeRangeX`, `activeRangeY`, `npcSlots`, `noSpawnCycle`, `activeTime`, `defaultSpawnRate`, `defaultMaxSpawns`, `dontCountMe` |
| `NpcSpawnCooldownAndEnvironment` | 1860-1869 | 10 | `MoonEventRequiredPointsPerWaveLookup`, `EoCKilledToday`, `WoFKilledToday`, `SPAWN_SLOT_PROTECTION_TIME`, `ignorePlayerInteractions`, `ladyBugGoodLuckTime`, `ladyBugBadLuckTime`, `ladyBugRainTime`, `maximumAmountOfTimesLadyBugRainCanStack`, `offSetDelayTime` |
| `NpcTargetAndIdentityProperties` | 1886-1901 | 16 | `CanTalk`, `CanBeTalkedTo`, `HasValidTarget`, `HasPlayerTarget`, `HasNPCTarget`, `SupportsNPCTargets`, `TranslatedTargetIndex`, `WhoAmIToTargetingIndex`, `IsShimmerVariant`, `TypeName`, `FullName`, `HasGivenName`, `GivenOrTypeName`, `GivenName`, `sWidth`, `sHeight` |
| `NpcProgressionAndEnvironmentProperties` | 1902-1910 | 9 | `DownedAnyPreHardmodeBoss`, `ShieldStrengthTowerMax`, `Opacity`, `TreatedAsABossForRainbowBoulders`, `isLikeATownNPC`, `IsMechQueenUp`, `TooWindyForButterflies`, `CountsAsACritter`, `NetSectionCoordinates` |
| `NpcSpawnContextAndCapacityInputs` | 1519-1528 | 10 | `spawnSpaceX`, `spawnSpaceY`, `fairyLog`, `numberOfActivePlayers`, `reachedInvasionBossCap`, `pX`, `pY`, `luck`, `dayTime`, `raining` |
| `NpcSpawnSpatialEligibilityInputs` | 1540-1553 | 10 | `surfaceSpawn`, `spawnUndergroundDesert`, `hardDungeon`, `deeperThanRockLayer`, `underGround`, `isOcean`, `isBeach`, `skyBehindPlayer`, `livingTree`, `inRemixStartingArea` |
| `NpcSpawnBiomeAndDungeonEligibilityInputs` | 1536-1552 | 6 | `waterTile`, `nearGranite`, `nearMarble`, `dualDungeonsSpawnRules`, `inDualDungeon`, `tresspassingDualDungeon` |
| `NpcSpawnPolicyAndEventEligibilityInputs` | 1529-1555 | 11 | `townNPCs`, `skyMob`, `noWorms`, `noGroundWorms`, `invaders`, `spawnFriendly`, `ignoreSafeWalls`, `spawnSpider`, `isSpawningInWindDirection`, `offensiveToTim`, `playerHasStartingHealth` |
| `NpcSpawnBiomeZoneInputs` | 1556-1568 | 13 | `ZoneCorrupt`, `ZoneCrimson`, `ZoneHallow`, `ZoneJungle`, `ZoneSnow`, `ZoneGlowshroom`, `ZoneMeteor`, `ZoneGraveyard`, `ZoneDungeon`, `ZoneLihzhardTemple`, `ZoneGranite`, `ZoneMarble`, `ZoneSandstorm` |
| `NpcSpawnEventAndTowerInputs` | 1569-1576 | 8 | `ZoneTowerSolar`, `ZoneTowerVortex`, `ZoneTowerNebula`, `ZoneTowerStardust`, `ZoneOldOneArmy`, `ZoneWaterCandle`, `ZonePeaceCandle`, `ZoneShadowCandle` |
| `NpcSpawnTargetSelectionState` | 1577 | 1 | `defaultTarget` |

The table totals 117 members (92 fields and 25 properties). It is the complete P14 scope; no
member from another partition is included.

## Conceptual Behaviors

P14 covers the following conceptual behaviors while keeping adjacent ownership explicit:

1. Build a per-player natural-spawn context from active players, world/event state, NPC
   population and capacity, weather, biome, dungeon, tile and wall inputs.
2. Apply spawn suppression, capacity, spawn-rate and target eligibility rules.
3. Choose and validate a candidate tile, including safe-area, tile-space, liquid, dungeon,
   sky, ocean, beach, tower and event predicates.
4. Produce an immutable eligibility decision containing acceptance/rejection, reason, selected
   target and candidate location. The decision is a value for one attempt, not a persistent
   entity component.
5. Hand an accepted decision to an explicit spawn commit boundary. NPC construction,
   lifecycle activation, population accounting, persistence and network visibility remain
   cross-subsystem integration work.
6. Preserve special paths (`SpawnFaelings`, `SpawnOnPlayer`, event/boss paths) as separate
   commands/adapters until their authority and ordering are proven; they must not be silently
   folded into natural-spawn Query logic.

## State Ownership and Write Closure

The legacy `Spawner` is a short-lived mutable context. Its fields are populated by the
constructor and `SetSpawnFlags`, changed again by tile selection, and consumed by
`SpawnAnNPC`. This makes a single broad "all fields are Query inputs" boundary invalid.

| State family | Proposed owner/seam | Closure status |
| --- | --- | --- |
| Natural-spawn attempt context and player/world inputs | `NpcSpawnContextCapture` adapter producing immutable snapshots | `partial`; constructor, `SetSpawnFlags`, player/world readers, and invalidation order are not closed. |
| Eligibility and tile predicates | `NpcSpawnEligibilityQuery` over a versioned snapshot | `proposed`; Query must not read `Main`, random, clock, logger, network writer, or mutate shared state. |
| Rate/capacity and active-player pressure | existing world/NPC authority through an explicit read Query | `unknown`; `npcSlots`, `dontCountMe`, active counts, and event caps have multiple writers. |
| Attempt scheduling and `noSpawnCycle` | existing NPC/world scheduler, not a second P14 allocator | `unknown`; static gate and tick order are not closed. |
| Spawn result, NPC lifecycle and population accounting | cross-subsystem `SpawnCommitPort` owned by integration review with adjacent NPC lifecycle authority | `unknown`; P14 may issue a command but may not commit a second lifecycle or population owner. |
| Network section/sync and persistence | Network/Persistence owners (`integration-review`) | `unknown`; `SyncNewlySpawnedNPCs` sends `NetMessage` type 23. |
| Target selection and target mutation | read-only target value plus explicit target command to the existing NPC target authority | `partial`; `defaultTarget` has only initialization evidence and target submission closure is absent. |

Known write boundaries:

- `NPC.Spawner.GetSpawnArea` writes static `safeRangeX` and `safeRangeY` before returning
  rectangles (`NPC.cs:837-875`); it is not a pure Query.
- `FindSpawnTile` mutates `skyMob` while sampling random tiles and checking `Main.tile`
  (`NPC.cs:877-920`).
- `SetSpawnFlagsForChosenTile` writes `waterTile`, `nearGranite`, `nearMarble`, underground,
  ocean/beach, spider/desert and other flags while reading tiles, walls, world configuration,
  wind and random state (`NPC.cs:952-1188`).
- `SetSpawnFlags` writes the broad context at `NPC.cs:279-340` while crossing Player, Main,
  Tile, WorldGen, equipment, event and invasion state.
- `SpawnAnNPC` creates NPCs and may update spawn flags; `SyncNewlySpawnedNPCs` scans active
  NPCs and calls `NetMessage.SendData(23, ...)` (`NPC.cs:5185-5198`).
- `SpawnedFromStatue`, `CanBeReplacedByOtherNPCs`, `npcSlots`, `dontCountMe`, `GivenName`,
  `Opacity`, `NetSectionCoordinates`, and event/day fields have cross-subsystem reads or
  writes. Their authoritative owners are `crossSubsystemOwner: integration-review` until
  focused evidence closes them.

## Boundary Role and Decision

**Decision: `partial` proposed boundary.** Retain the existing NPC generation runtime boundary
until the legacy writer and side-effect closure is proven. Introduce the following conceptual
seams for a future migration:

- `NpcSpawnContextCapture` (adapter/system): samples one coherent player/world/NPC snapshot;
  owns no long-lived eligibility result.
- `NpcSpawnEligibilityQuery` (Query): consumes only immutable, versioned snapshot values and
  returns `SpawnEligibilityDecision`.
- `NpcSpawnTargetSelectionQuery` (Query): returns a target value and reason; it does not write
  NPC target fields.
- `NpcSpawnAttemptCommand` and `SpawnCommitPort` (Command/adapter): carry accepted decisions
  to the already-authoritative NPC lifecycle/population/network owners.
- `NpcSpawnSchedulingSystem` (System, only after owner proof): runs the existing tick cadence,
  `noSpawnCycle`, pressure and cooldown inputs without introducing another slot allocator.

The proposal deliberately does not create a new independent runtime System in NLTX. The
existing snapshots are `current-NLTX partial` evidence, not an implementation of these seams.

## System API and Legacy Behavior Mapping

| Legacy behavior | Proposed API shape | Required side-effect rule | Status |
| --- | --- | --- | --- |
| `NPC.SpawnNPC()` (`NPC.cs` near 66509) | `NpcSpawnSchedulingSystem.Tick(SpawnTickSnapshot)` | Preserve tick order and `noSpawnCycle`; command only | `partial` |
| `Spawner.SpawnNPC()` (`NPC.cs:185-205`) | iterate eligible player snapshots and issue at most one attempt according to legacy break behavior | no direct entity/network write in Query | `partial` |
| `CanSpawnEnemiesNear(Player)` | `SpawnEligibilityQuery.CanAttempt(playerSnapshot)` | read-only, deterministic for a fixed snapshot | `partial` |
| `TrySpawnAnNPC(Player)` (`NPC.cs:206-255`) | `EvaluateAttempt(context) -> SpawnEligibilityDecision` then `SpawnCommitPort.Commit` | split at the first side effect; preserve rejection order | `proposed` |
| `SetSpawnFlags` | `NpcSpawnContextCapture.Capture` | legacy writes require adapter/commit ownership; not a pure Query | `unknown` |
| `GetSpawnRate` | `SpawnRateQuery` over explicit pressure/event inputs | no hidden writes or static cache mutation | `partial` |
| `GetSpawnArea` | `SpawnAreaQuery` returning rectangles and derived safe extents | move `safeRangeX/Y` to explicit value; do not write global fields | `partial` |
| `FindSpawnTile` | `SpawnCandidateQuery` with injected random sample stream | random source is an adapter input, never a Query global | `partial` |
| `PostCheckChosenSpawnTile` | `SpawnTilePolicyQuery` | no random/global writes inside Query; preserve liquid/dungeon/event order | `partial` |
| `SetSpawnFlagsForChosenTile` | `ChosenTileContextBuilder` plus explicit commit of derived context | tile/world reads are snapshot inputs; current method writes context | `unknown` |
| `SpawnAnNPC` | `SpawnCommitPort.Commit(SpawnAdmission)` | lifecycle, slot accounting, release owner and network are external owners | `unknown` |
| `SyncNewlySpawnedNPCs` | `NpcNetworkProjection` after commit | only network owner may send section/sync messages | `unknown` |
| `SpawnFaelings(Player)` | `SpecialSpawnCommand(FaelingSpawnRequest)` | separate special-spawn authority; do not call natural Query implicitly | `partial` |
| `SpawnOnPlayer(...)` | `SpecialSpawnCommand(OnPlayerSpawnRequest)` | preserve boss/event uniqueness and target assignment; integration review owns commit | `partial` |
| `NpcTargetComponent` / `NpcSpawnTargetSelectionSnapshot` | `SelectTarget(snapshot) -> TargetSelection` | target mutation is an explicit command to target authority | `proposed` |

Query contracts require a snapshot version, player/entity identity, world revision, candidate
random token (when sampling is needed), and rejection reason. A Query cannot reach `Main`,
`Main.rand`, mutable `Tile` storage, logger, network sender, clock, or persistence writer.
Commands require an attempt key and must be idempotent or explicitly reject duplicate commits;
the exact key and replay semantics are `unknown` until the lifecycle/network owners are mapped.

## Call and Dependency DAG

The source-confirmed natural-spawn path is:

```text
Main.UpdateWorld / Main spawn tick
  -> NPC.SpawnNPC()
     -> new NPC.Spawner().SpawnNPC()
        -> CanSpawnEnemiesNear(player)
        -> SlimeRainSpawns(player) [event branch]
        -> TrySpawnAnNPC(player)
           -> SetSpawnFlags(player)
           -> GetSpawnRate(...)
           -> FindSpawnTile(...)
              -> GetSpawnArea(...)
           -> CheckNotSpawningOnScreen(...)
           -> GetProperGroundSpawnTileTypeAndWallType(...)
           -> PostCheckChosenSpawnTile(...)
           -> SetSpawnFlagsForChosenTile(...)
           -> SpawnAnNPC(...)
           -> SyncNewlySpawnedNPCs()
```

Additional edges are source-confirmed at `Main.cs:13197-13265` for Eye, hard-boss,
Deerclops and other special paths; `MessageBuffer.cs:1721-1733` calls `Spawner.SpawnFaelings`,
and `MessageBuffer.cs:2114-2124` can call `NPC.SpawnOnPlayer` from message type 61.

The proposed composition is:

```text
World/NPC/Player authority
  -> ContextCaptureAdapter
     -> SpawnContextSnapshot (versioned value)
        -> EligibilityQuery / TilePolicyQuery / TargetQuery
           -> SpawnEligibilityDecision (ephemeral value)
              -> NpcSpawnAttemptCommand
                 -> existing SpawnCommitPort / NPC lifecycle owner
                    -> population accounting + target command + network/persistence projections
```

The arrows describe a proposed dependency direction, not an implemented call graph. Any
cross-partition owner, especially slot accounting, NPC lifecycle, target authority, network
section and persistence, is `crossSubsystemOwner: integration-review`.

## Lifecycle and Side Effects

1. **Capture:** create a snapshot at one world/player revision. Snapshot lifetime is one spawn
   attempt; it must be discarded after the decision.
2. **Evaluate:** apply the legacy order: active/dead/suppression, rate/capacity, random
   admission, tile search, screen/safe-area checks, tile policy and event/biome rules.
3. **Decide:** return accepted/rejected with a stable reason and candidate metadata. No entity
   component is installed for a rejected or pending candidate.
4. **Commit:** send one explicit command to the existing NPC creation/lifecycle authority.
   Commit may allocate an NPC slot, assign release owner/target, and emit events, but P14 does
   not own those effects.
5. **Project:** network synchronization, persistence and presentation observe the committed
   result. `SyncNewlySpawnedNPCs` is a legacy side effect and is not a Query.
6. **Invalidate:** a world revision, player state, event switch, tile mutation, capacity change
   or failed commit invalidates the snapshot. Reusing it is an unknown behavior until verified.

Random selection, global mutable arrays, static safe ranges, tile construction, `NewNPC`,
`NetMessage.SendData`, logging and persistence are side effects or effectful inputs. They must
be behind adapters/commit ports. No second owner may be introduced for NPC lifecycle, population
slots, target authority, network section membership, or persistence.

## Integration Handoff

| Handoff | Required receiving owner | Contract to settle |
| --- | --- | --- |
| accepted natural-spawn decision | NPC lifecycle/creation owner | slot allocation, `NewNPC`, activation, release owner, duplicate command behavior |
| `npcSlots`, `dontCountMe`, active counts | population/pressure owner | unique writer, despawn/replacement accounting, event cap ordering |
| `defaultTarget`, `target`, target properties | NPC target authority | target selection vs target mutation, invalidation and network replication |
| `SpawnedFromStatue`, replacement flags, critter state | NPC lifecycle/statue owner | creation source and replacement policy across statue/natural/event spawns |
| `GivenName`, `Opacity`, `NetSectionCoordinates` | identity/render/network owners | setter side effects, persistence, section projection and replication |
| `EoCKilledToday`, `WoFKilledToday`, ladybug timers, event lookup | world-event/day-state owner | reset boundary, save/load, event ordering and authoritative writer |
| special `SpawnFaelings` / `SpawnOnPlayer` | special spawn/boss/event owner | uniqueness, world tile search, target assignment and notification ordering |
| `SyncNewlySpawnedNPCs` | network projection | message type 23, section visibility and retry/ordering semantics |

Each handoff is a blocking integration decision, not an assertion that the receiving owner
already exists in NLTX.

## Migration Behavior Contract

The migration contract is `proposed` and preserves the following observable requirements:

- Keep the legacy `NPC.SpawnNPC` entry point and cadence through an adapter until the new
  composition is proven. Preserve the first-success break in `Spawner.SpawnNPC`.
- Preserve rejection ordering and distinguish inactive/dead/suppressed, capacity, random,
  tile, safe-area, liquid, dungeon, event and special-spawn rejection reasons.
- Evaluate all predicates against one coherent snapshot revision. A stale snapshot must be
  rejected or re-captured; exact stale behavior is `unknown`.
- Keep natural, faeling, boss, event and `SpawnOnPlayer` requests separate. Do not let a
  special path bypass the authoritative commit boundary.
- Commit accepted results exactly once through the existing NPC lifecycle/population authority;
  duplicate/replayed commands need an attempt key and explicit policy before implementation.
- Network, persistence, target mutation and lifecycle effects occur only after the authoritative
  commit. No proposed Query writes authority.
- Existing NLTX snapshots/components may supply evidence or future adapter inputs, but their
  presence does not demonstrate migration, API compatibility, or behavioral equivalence.

## Evidence Gaps and Blocking Decisions

1. **CPG closure gap:** `Spawner.SpawnNPC` inbound call-site search was `partial`; callable
   facts reported unexpanded callee effects. The static `NPC.SpawnNPC(): void` call from
   `Main.cs` was positive, but dynamic/event paths are not fully closed.
2. **Source snapshot gap:** CPG source excerpts are `unknown` because the companion source tree
   is absent. Version4 files were read directly and line references are evidence only for this
   checkout.
3. **Writer closure gap:** `npcSlots`, `dontCountMe`, `SpawnedFromStatue`,
   `CanBeReplacedByOtherNPCs`, `GivenName`, `Opacity`, event-day flags and section coordinates
   have cross-file users/writers. A unique owner cannot be declared from P14 evidence.
4. **Mutable Query gap:** `GetSpawnArea`, `FindSpawnTile`, `SetSpawnFlags` and
   `SetSpawnFlagsForChosenTile` mix reads, random sampling and writes. Their split requires a
   focused verifier for write sets and ordering.
5. **Target gap:** `defaultTarget` has initialization evidence but no closed target-submission,
   invalidation or replication path. Target authority remains `integration-review`.
6. **Lifecycle/network/persistence gap:** `SpawnAnNPC` and `SyncNewlySpawnedNPCs` cross NPC
   creation, population, network and persistence boundaries. No P14-only owner can safely
   absorb them.
7. **Current NLTX gap:** existing components and snapshots have no evidence here of registration,
   scheduler wiring, adapter consumption, network replication, persistence, or behavior parity.

Blocking decisions before implementation:

- Name the single authoritative owner for NPC slot/population accounting and `dontCountMe`.
- Name the target mutation owner and define `defaultTarget`/legacy index conversion.
- Decide whether static safe extents become value outputs or remain behind a compatibility
  adapter; do not leave `safeRangeX/Y` as an implicit global writer.
- Define commit idempotency and stale-snapshot policy for natural and special spawn commands.
- Define network section and persistence contracts for newly spawned NPCs.
- Re-run CPG/source queries with a bound source snapshot and complete call/effect expansion.

## Verification Plan

`verificationStatus: not-run`. No build, test, runtime, behavior verifier, network verifier or
persistence verifier was executed for this report.

Focused verification required before any migration claim:

1. **Inventory:** assert all 117 ledger members are represented once, with source sequence and
   declaring type preserved.
2. **Query purity:** run read/write instrumentation for each proposed Query; fail on `Main`,
   random, clock, logger, tile construction, static cache or authority writes.
3. **Write closure:** trace every writer and reset path for budget, cooldown, event, target,
   critter and context fields; prove one owner or record an integration owner.
4. **Ordering:** compare natural-spawn rejection order, first-success break, special path
   precedence and commit/network ordering against Version4 source behavior.
5. **Capacity/replay:** verify cap exhaustion, `npcSlots`, `dontCountMe`, duplicate commands,
   stale snapshots and failed commits.
6. **Spatial/event matrix:** cover safe-area and screen checks, liquid/wall/dungeon rules,
   remix/dual-dungeon, biome and tower/event switches, and random boundary values.
7. **Target/lifecycle handoff:** verify target selection, NPC activation/despawn, population
   accounting, section visibility and persistence using the named receiving owners.

Until these checks are run and the blocking decisions are resolved, the result remains a
`proposed` static decomposition with `unknown` gaps. It must not be described as migration
success or behavioral equivalence.
