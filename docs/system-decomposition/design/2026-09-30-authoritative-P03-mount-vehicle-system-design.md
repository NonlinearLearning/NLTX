# Authoritative P03 Mount and Vehicle System Design

documentKind: system-design
partitionId: P03
taskId: AUTH-SYS-P03
derivedFrom: D:\TRbackup\NLTX\docs\system-decomposition\reports\2026-09-18-system-decomposition-authoritative-P03-mount-vehicle.md
claimInputReport: D:\TRbackup\NLTX\docs\migration\ledgers\authoritative-20-partitions\P03-Mount-Vehicle.md
outputReport: D:\TRbackup\NLTX\docs\system-decomposition\reports\2026-09-18-system-decomposition-authoritative-P03-mount-vehicle.md
settledSessionId: 453b6a2e98c54a31a10b0a17d4c1e816
targetSource: D:\TRbackup\Version4
fullReferenceSource: D:\TRbackup\无任何删减通过编译
ecsReferenceSource: C:\Users\shan\Downloads\ECS\space-station-14-master
migrationRoot: D:\TRbackup\NLTX\src\NSSLC
sourceModified: false
testsRun: false
buildRun: false

| Field | Value |
| --- | --- |
| `partitionId` | `P03` |
| `taskId` | `AUTH-SYS-P03` |
| `sourceReportSessionId` | `453b6a2e98c54a31a10b0a17d4c1e816` |
| `designStatus` | `proposed` |
| `implementationStatus` | `partial` |
| `verificationStatus` | `not-run` |
| `coreSliceVerification` | `existing-evidence` |
| `migrationStatus` | `deferred` |
| primary static report | `docs/system-decomposition/reports/2026-09-18-system-decomposition-authoritative-P03-mount-vehicle.md` |
| primary source | `D:\TRbackup\Version4` |
| complete reference source | `D:\TRbackup\无任何删减通过编译` |
| migration tree | `D:\TRbackup\NLTX\src\NSSLC` |
| organization reference | `C:\Users\shan\Downloads\ECS\space-station-14-master` |

This document turns the P03 static report into a proposed System design and records the isolated
core slice implemented under `src/NSSLC/Component/Player/Mount`. The slice is evidence for the
listed local invariants only; it does not authorize or prove full Player, world, network,
persistence, effect, scheduling, or behavior-equivalence migration. All external integration
contracts remain proposed until a separately authorized implementation task closes the required
ownership and behavior gaps.

## 1. Design Intent and Scope

P03 covers the 17 `MountAndVehicleSimulation` leaf groups in the authoritative ledger: 136 fields,
27 properties, and 163 members. The design separates four lifetimes that are mixed in the legacy
`Mount` type:

1. immutable mount, vehicle, geometry, animation, drill, and presentation definitions;
2. per-player authoritative mount, fatigue, ability, variant, frame, flight, and drill state;
3. read-only runtime and mobility projections;
4. external Player, Collision, Tile, Projectile, lighting, sound, dust, network, and persistence
   effects.

The design excludes final ownership for those external boundaries. Each unresolved shared owner is
marked `crossSubsystemOwner: integration-review`.

## 2. Evidence Baseline

### 2.1 Primary source and query API

The P03 report used `D:\TRbackup\Version4` as its primary source and the read-only
`CpgEvidence.ps1` API over `D:\TRbackup\Version4-cpg-export\out-dop8-interproc.sqlite`.
The query artifact reported schema `1`, import status `complete`, 967 shards, 8,166,789 nodes,
71,038,907 edges, 1,317 diagnostics, manifest SHA-256
`6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`, and no source snapshot ID.

The selected queries returned these positive static relationships:

| API query | Result used by this design |
| --- | --- |
| `Find-CpgCallSites(SetMount)` | 2 `Player.cs` sites, status `complete` |
| `Find-CpgCallSites(Dismount)` | 3 sites across `Mount.cs` and `Player.cs`, status `complete` |
| `Find-CpgCallSites(UpdateFrame)` | 5 mount-method sites, status `complete` |
| `Find-CpgCallSites(UseAbility)` / `AimAbility` | 4 / 3 sites across Player and Projectile, status `complete` |
| `Find-CpgCallSites(CanFly)` / `CanHover` | 3 / 2 sites, status `complete` |
| update/effect/drill/recovery methods | status `partial`, zero returned with `NoMatchingFactInScannedScope`; zero is not treated as no caller |

The CPG index is evidence for Version4's indexed source only. It does not bind to the complete
reference tree, does not close dynamic delegates or scheduler order, and does not prove behavior.

The query API was refreshed against the same read-only database for this document pass. The
selected scope was `Terraria/Mount.cs`, `Terraria/Player.cs`, `Terraria/Projectile.cs`, and
`Terraria/Main.cs`; symbol lookup was done with `Find-CpgSymbols` and relationships with
`Find-CpgCallSites` after `Initialize-CpgEvidence` and `Start-CpgEvidenceServer`. The returned
static facts were:

| Method | Symbol lookup | Call-site query | Returned sites | Evidence use |
| --- | --- | --- | ---: | --- |
| `SetMount` | `complete` | `complete` | 2 | Player activation/replacement callers |
| `Dismount` | `complete` | `complete` | 3 | Mount and Player teardown callers |
| `UpdateFrame` | `complete` | `complete` | 5 | Mount and Player frame paths |
| `UseAbility` | `complete` | `complete` | 4 | Player and Projectile ability paths |
| `AimAbility` | `complete` | `complete` | 3 | Mount and Projectile aim paths |
| `CanFly` | `complete` | `complete` | 3 | Player and Projectile mobility reads |
| `CanHover` | `complete` | `complete` | 2 | Mount and Player hover paths |
| `UpdateEffects`, `UpdateDrill`, `UseDrill`, `Hover`, `Flight`, `AbilityRecovery`, `FatigueRecovery`, `ResetFlightTime` | `complete` | `partial` | 0 | source inspection required; each returned `NoMatchingFactInScannedScope` |

The final row is an evidence gap, not a negative call-graph result. No query result was used to
claim that a caller, writer, scheduler edge, or effect does not exist. The reader was closed after
the queries; no CPG, source, or generated artifact was written.

### 2.2 Complete reference source

The complete reference tree is used as a read-only semantic supplement because the primary source
has version drift and incomplete relationship evidence. SHA-256 hashes captured for the comparison
were:

| File | SHA-256 |
| --- | --- |
| `D:\TRbackup\Version4\Terraria\Mount.cs` | `2DED2B174731DDDD003BCD1B03F83853D0AD39183CD3683B4C5289459AEDCEFC` |
| `D:\TRbackup\无任何删减通过编译\Terraria\Mount.cs` | `3F94D523F50A44498BB3E8FC7F98FA18AD11DE5EA1E65AFAD6E0D81F05040964` |
| `D:\TRbackup\Version4\Terraria\Player.cs` | `E5B301E3401F61E37BF3291754670BCC123D6593EDD94597C4E9109C4030FE86` |
| `D:\TRbackup\无任何删减通过编译\Terraria\Player.cs` | `367E6C3249F064DD9CCF8E421EBEBF6EE3AA745B6521F07A8BEBA4A3C2FF612C` |

Relevant complete-tree facts are:

- `Main.cs:6743-6744` and `Main.cs:11037-11038` both call `Mount.Initialize` and
  `Minecart.Initialize`; bootstrap must therefore be idempotent or explicitly scoped.
- `Player.cs:25126`, `26051`, `27020`, `27041`, `27136`, `27317`, `27321`, `28658`, and
  `36670-36686` show the fatigue, effects, ability, drill, flight, hover, and frame tick paths.
- `Mount.cs:3193-3268` contains non-empty `DrillSmartCursor_Blocks` and
  `DrillSmartCursor_Walls` implementations. They use crosshair input, `Utils.PlotTileLine`,
  beam deduplication, `WorldGen.CanKillTile`, `Main.tile`, and `Player.CanPlayerSmashWall`.
- `Mount.cs:4882-5290` contains the complete `UpdateEffects` body, including Player stat writes,
  lighting, dust, random values, and projectile creation.
- `Mount.cs:6302-6360` and `6245-6295` show `SetMount` and `Dismount` changing both Mount state
  and Player state. `CanMount` at `6705-6734` invokes a Player-size delegate and Collision, so it
  cannot be exposed as an unchecked pure Query.

These facts supplement, but do not silently replace, the Version4 evidence. A version-drift item
is upgraded only after a source revision decision and focused behavior comparison.

The complete reference is the preserved, compiling tree requested for this design. Its inspected
relation anchors are:

| Complete-reference location | Observed relation or behavior | Design consequence |
| --- | --- | --- |
| `Terraria/Main.cs:6743-6744`, `11037-11038` | `Mount.Initialize` and `Minecart.Initialize` are each invoked from two initialization paths | catalog bootstrap must be idempotent or scoped to an explicit world/content lifecycle |
| `Terraria/Player.cs:25126`, `26051`, `27020`, `27041`, `27136`, `27317`, `27321`, `28658`, `36670-36686` | fatigue, effects, ability recovery, drill update, flight reset, hover/flight, drill use and frame updates are interleaved with Player movement | these are Commands/state transitions; a pure Query cannot own them |
| `Terraria/Mount.cs:3044-3189` | drill cooldowns, target clearing, beam reservation, tile/wall picking and dust/tile effects are mixed | `DrillMountSystem` owns only drill runtime state; tile/projectile/effect work stays behind ports |
| `Terraria/Mount.cs:3193-3268` | block/wall smart cursors use crosshair input, `PlotTileLine`, beam deduplication, `CanKillTile`, `Main.tile` and `CanPlayerSmashWall` | cursor qualification is `mixed` until its input, collision and tile authorities are assigned |
| `Terraria/Mount.cs:4882-5290` | `UpdateEffects` writes Player stats and emits lighting, dust, random and projectile effects | effect publication is an Adapter/Port boundary after authoritative commit |
| `Terraria/Mount.cs:6245-6360`, `6705-6743` | dismount/set-mount mutate both Mount and Player; `CanMount` invokes a size delegate and Collision | activation/deactivation require commit-time recheck and cannot be exposed as unchecked pure Queries |

The SS14 reference was read at `Content.Shared/Alert/AlertsSystem.cs:9-69` and
`Content.Shared/Animals/UdderSystem.cs:21-89`. Its `EntitySystem` examples keep lifecycle/event
subscriptions and component writes in Systems, use injected `EntityQuery<T>` for component access,
and enumerate components in an explicit update phase. This supports the proposed owner/query
separation and scheduling vocabulary only; it is not Terraria behavior evidence.

### 2.3 Current NLTX material

The target tree now contains mount components, definitions, queries, systems, and adapter contracts
under `src/NSSLC/Component/Player/Mount`. The implemented core owns only the isolated runtime
components and catalog/drill state described below. The overlapping `PlayerMountState`,
`PlayerMountComponent`, and `PlayerMountVehicleIntegrationComponent` remain unresolved
compatibility surfaces; this design claims no unique production writer for them.

## 3. Ownership Model

### 3.1 Proposed authoritative owner

`MountRuntimeSystem` is the proposed single owner for per-player Mount lifecycle and coordinated
frame, flight, fatigue, and ability transitions. It owns the commit protocol for:

- active/type/definition handle and reset state;
- frame, extra-frame, flip, idle and walking-grace counters;
- flight time and fatigue transitions;
- ability charge, cooldown, duration, active, charging, and aiming transitions;
- variant activation handoff, without storing an untyped `_mountSpecificData` equivalent.

`DrillMountSystem` is a separate owner for drill beam, target, rotation, and cooldown state. It
accepts a validated drill input snapshot from the runtime boundary and returns explicit tile,
projectile, dust, and sound intents. It does not own tile mutation or Projectile lifetime.

### 3.2 Definitions, Queries, Adapters, and Projections

| Proposed boundary | Owns | Does not own |
| --- | --- | --- |
| `MountDefinitionCatalog` | immutable catalog registration and frozen definition views | per-player counters, callbacks, Player state, persistence |
| `MountRuntimeFrameAndFlightStateComponent` | typed active/type/frame/flight/grace state | Player position, velocity, hitbox, network ID |
| `MountFatigueAndAbilityStateComponent` | fractional fatigue and ability timers/flags | Projectile IDs, mouse state, random source, effects |
| `MountVariantStateComponent` | tagged variant payload and reset defaults | serialization or network schema until integration approves it |
| `DrillMountRuntimeComponent` | bounded beams, target, purpose, rotations, crosshair and cooldown | tile authority, projectile allocator, cursor service |
| `MountRuntimeIdentityAndFrameProjectionQuery` | immutable identity/frame/geometry view | writes, callbacks, collision mutation, lazy caches |
| `MountRuntimeMobilityAndAbilityProjectionQuery` | immutable speed/cart/wing/ability qualification view | state transitions, effect execution, Player writes |
| `MountQualificationQuery` | snapshot-based mount/dismount eligibility result | final commit; owner must recheck at commit time |
| `IMountEffectPort` / `MountEffectAdapter` | explicit light/dust/sound/size/projectile/tile effect execution | mount rules, authoritative component writes |
| `MountNetworkProjection` / `MountPersistenceProjection` | one-way committed-state output | reverse writes, ownership decisions, schema invention |

The existing query files may be reused only where their input snapshots preserve these purity
rules. A type named `Query` is not enough: any read that invokes Collision, time, random,
callbacks, lazy initialization, or shared cache mutation remains `QueryCandidate/mixed`.

## 4. Conceptual API Contract

The canonical composition is `LegacyEntryPoints -> ConceptId -> CompositionId -> new owner`.
The following API names are design proposals, not existing implementations.

| Concept ID | Legacy entry points | Proposed composition | Result and commit rule |
| --- | --- | --- | --- |
| `MountCatalog.Bootstrap` | `Mount.Initialize` | `Register` -> validate IDs/ranges -> `Freeze` -> immutable snapshot | repeatable within an explicit world/content scope; duplicate bootstrap behavior is `unknown` |
| `MountLifecycle.Activate` | `CanMount` + `SetMount` | snapshot qualification -> commit-time recheck -> runtime commit -> Player/effect ports | no externally visible TOCTOU gap |
| `MountLifecycle.Deactivate` | `CanDismountWithResult`, `TryDismountWithResult`, `Dismount` | space/CC qualification -> runtime clear -> Player geometry/buff commit -> effects | preserve failure result and no-space behavior |
| `MountMobility.ResourceTick` | `Hover`, `Flight`, `ResetFlightTime`, recovery methods | input/collision snapshot -> runtime transition -> Player movement commit | no Query classification; state mutation is explicit |
| `MountPresentation.FrameTransition` | `UpdateFrame`, `UpdateFrame_Velociraptor` | catalog frame view + injected velocity/random decision -> frame commit | preserve idle randomness and special variant branches |
| `MountAbility.ActivateOrAim` | `StartAbilityCharge`, `UseAbility`, `AimAbility`, Projectile callers | input -> owner qualification/recheck -> ability commit -> Projectile/effect intents | effect order and retry remain integration gates |
| `MountDrill.Execute` | `UpdateDrill`, `UseDrill`, Player drill path | cursor/input snapshot -> drill state transition -> tile/projectile intents -> cooldown commit | complete-reference cursor rules must be version-checked |
| `MountEffects.Publish` | `UpdateEffects`, delegate fields | committed view -> effect descriptor -> explicit adapters | adapter does not write Mount state |
| `MountReadModel.GetView` | `Active`, `Type`, `Frame`, offsets, mobility properties | immutable catalog + committed components -> pure view | inactive/default and invalid ID semantics are explicit |

All API contracts must preserve this observation vector:

```text
Observation = (
  return_or_error,
  authoritative_state_delta,
  emitted_events_and_external_effects,
  order_and_visibility,
  lifecycle_and_scope,
  retry_and_idempotency
)
```

## 5. Scheduling and Lifecycle Design

The proposed scheduler has explicit barriers; directory or file order is not a schedule:

```text
catalog bootstrap
  -> input/qualification snapshot
  -> MountRuntimeSystem commit
  -> DrillMountSystem commit
  -> Player/Collision commit ports
  -> effect publication
  -> read-only projections/network/persistence
```

Required edges are:

- catalog freeze before activation;
- qualification snapshot before commit-time recheck;
- runtime commit before effects and projections;
- drill intent validation before tile/projectile effects and cooldown commit;
- dismount clear before final unmounted presentation;
- projection publication after all authoritative writes in the current barrier.

Lifecycle responsibilities are:

| Phase | Owner | Required behavior |
| --- | --- | --- |
| bootstrap | catalog boundary | deterministic registration, validation, freeze and explicit reload scope |
| activate | MountRuntimeSystem | set type/definition/variant, initialize resources, request external Player changes |
| update | MountRuntimeSystem + DrillMountSystem | commit frame/flight/resource/ability/drill state in approved order |
| observe | Query/Projection | expose committed immutable views only |
| end/reset | MountRuntimeSystem | clear runtime, variant, drill and request Player restoration |
| external reconciliation | Player integration adapter | buff loss, item use, death, hit and equipment changes become owner commands |
| persistence/network | integration owners | versioned one-way projections after owner and schema approval |

## 6. Side-Effect Isolation

The complete reference confirms that the legacy Mount path writes Player stats and geometry,
calls Collision and `WorldGen`, reads time/random and creates dust, lighting, sound and
projectiles. These effects must be visible in the proposed ports:

| Effect | Proposed port | Failure/retry status |
| --- | --- | --- |
| Player position/velocity/hitbox/rotation/buffs | `IPlayerMountCommitPort` | `unknown`; integration review required |
| collision and fit | `IMountCollisionPort` | qualification/commit split and exception behavior `partial` |
| tile mining | `IMountTileActionPort` | authority, duplicate and retry `unknown` |
| projectile creation | `IMountProjectilePort` | identity, lifetime and network semantics `unknown` |
| dust/light/sound | `IMountPresentationEffectPort` | ordering and idempotency `unknown` |
| random/time | explicit `IRandomSource` / tick input | seed, replay and scope `unknown` |
| network/save | one-way projections | schema and recovery `unknown` |

No Query, Definition, Projection or Component may invoke these ports. A Command/System may invoke
them only after recording the intended state delta and the required ordering.

## 7. Integration Handoff and Blocking Decisions

Every item below remains `crossSubsystemOwner: integration-review`:

- Player entity identity, movement, collision shape, buffs, death, equipment and item lifecycle;
- Collision fit, landing, slopes, water and dismount space;
- tile authority and drill mining transaction;
- Projectile allocation, ability release, visibility and network lifetime;
- minecart track behavior, sound, dust, player size and delegates;
- network input/replication, snapshots, save/load and prediction;
- world/session scope, scheduler registration, reload/unload and catalog ownership.

Blocking decisions before implementation are:

1. select one owner between the existing mount skeletons and the proposed runtime components;
2. approve the source revision used for catalog and drill behavior;
3. define commit-time recheck and effect failure semantics;
4. define variant/drill serialization and network schema;
5. define scheduler barriers, multi-world scope, retry and rollback behavior.

## 8. Design Acceptance Conditions

Full P03 integration and migration may proceed only after the integration review records the five
decisions above. This design must remain `proposed` while any owner, version, scheduler, persistence,
network, dynamic delegate, or effect contract is `unknown`. Static query evidence, a complete
reference source tree, a document, a compile result, or an existing Component file cannot be
called migration success.

## 9. Isolated Core Evidence

The implementation covers an intentionally narrow core slice:

- immutable mount Definitions and duplicate-rejecting catalog registration, including Scutlix
  eye/texture/base-damage data and Santank texture size;
- defensive geometry offset copies and Version4 drill/Super Cart constants;
- activation, repeated activation, dismount/reset, frame/grace, flight, fatigue and ability
  timer transitions in `MountRuntimeSystem`;
- read-only runtime, identity/frame, mobility/ability and qualification projections;
- drill rotation, bounded beam reservation, duplicate-target suppression, cooldown expiry and
  target clearing in `DrillMountSystem`;
- explicit Player, Collision, Tile, Projectile and presentation Adapter contracts without concrete
  external integration.

An earlier workspace checkpoint contains a focused verifier record for
`Test/Terraria.Player.MountSystem.CoreVerification` with a serial build and a no-build/no-restore
run reported as passing. That record is retained as `existing-evidence`; this documentation pass
did not invoke the verifier, a build, or a test command. It is local core evidence only. Player
geometry/buffs, Collision, Tile, Projectile, network, persistence, random/time, effects,
scheduler registration, compatibility callers, multi-world scope, exception/retry semantics and
deletion gates remain `unknown` or `partial`; the design therefore keeps
`implementationStatus: partial`, `verificationStatus: not-run`,
`coreSliceVerification: existing-evidence`, and `migrationStatus: deferred`.
