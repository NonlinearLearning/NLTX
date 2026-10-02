# System Decomposition Report: authoritative P03

## Scope and Evidence

| Item | Value |
| --- | --- |
| `designStatus` | `proposed` |
| `verificationStatus` | `not-run` |
| `migrationStatus` | `deferred` |
| partition | `P03` |
| task | `AUTH-SYS-P03` |
| claim mode | `manual` |
| original `sessionId` | `453b6a2e98c54a31a10b0a17d4c1e816` |
| claimed scope | `MountAndVehicleSimulation` |
| members | `163` (`136` fields + `27` properties) |
| leaf groups | `17` |
| input report | `D:\TRbackup\NLTX\docs\migration\ledgers\authoritative-20-partitions\P03-Mount-Vehicle.md` |
| authorized output | `D:\TRbackup\NLTX\docs\system-decomposition\reports\2026-09-18-system-decomposition-authoritative-P03-mount-vehicle.md` |
| Version4 source | `D:\TRbackup\Version4` |
| ECS organization reference | `C:\Users\shan\Downloads\ECS\space-station-14-master` |
| current migration tree inspected | `D:\TRbackup\NLTX\src\NSSLC` |

This is a read-only static System boundary proposal for one authoritative partition. No source,
test, project, input ledger, prompt, other report, or CPG artifact was modified. Completing the
runner document task would settle this report only; it would not establish migration success,
behavioral equivalence, compilation, or test passage.

### Evidence classes and query record

| Source | Scope and use | Status |
| --- | --- | --- |
| P03 input ledger | 17 leaf groups and the authoritative 163-member inventory, including declaration lines | `source-inventory-confirmed` |
| Version4 `Terraria/Mount.cs` | Catalog definitions, runtime fields, reset/initialize, mount/flight/ability/drill/frame/effect/aim/dismount transitions | `confirmed` for inspected source facts |
| Version4 `Terraria/Player.cs` | Mount callers, movement/flight/frame/drill/buff/item/death/dismount integration | `confirmed` for inspected static call sites; runtime ordering remains `partial` |
| Version4 `Terraria/Main.cs`, `Projectile.cs`, `Collision.cs` | Bootstrap, projections, ability callers, visibility and collision seams | `confirmed` for inspected static call sites; ownership remains `partial` |
| current NLTX `src/NSSLC` | Existing components and Query surfaces | `existing-evidence`; not proof of a completed migration |
| prior P03 Component design/execution | Candidate component boundaries and implementation checkpoint | `reconciled`; System ownership is re-evaluated here |
| SS14 reference | Read-only organization examples for focused EntitySystem/Query boundaries | `reference-only`; no Terraria behavior inferred |
| CPG Query API | Read-only `CpgEvidence.ps1` symbol and call-site queries against the Version4 SQLite index | `confirmed` for returned static edges, `partial` for unresolved/zero call-site results |

The CPG reader was initialized and started through the restored query API using
`D:\TRbackup\Version4-cpg-export\out-dop8-interproc.sqlite`. Health reported schema `1`, import
status `complete`, 967 shards, 8,166,789 nodes, 71,038,907 edges, 1,317 diagnostics, manifest
SHA-256 `6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`, and no source snapshot
ID. The database is read-only. The following selected queries were run:

| Query | Result |
| --- | --- |
| `Find-CpgSymbols` for mount entry points | `complete` for the selected symbols, with exact method identities returned |
| `Find-CpgCallSites(SetMount)` | `complete`, 2 sites in `Terraria/Player.cs` (spans `79998-80054`, `300918-300944`) |
| `Find-CpgCallSites(Dismount)` | `complete`, 3 sites: 2 internal `Mount.cs`, 1 `Player.cs` |
| `Find-CpgCallSites(UpdateFrame)` | `complete`, 5 sites for the mount method, including 4 `Player.cs` frame paths |
| `Find-CpgCallSites(UseAbility)` | `complete`, 4 sites across `Player.cs` and `Projectile.cs` |
| `Find-CpgCallSites(AimAbility)` | `complete`, 3 sites across `Mount.cs` and `Projectile.cs` |
| `Find-CpgCallSites(CanFly)` | `complete`, 3 sites across `Player.cs` and `Projectile.cs` |
| `Find-CpgCallSites(CanHover)` | `complete`, 2 sites in `Mount.cs` and `Player.cs` |
| `Find-CpgCallSites(UpdateAfterEquips)`, `CheckMountBuff`, `TryEarlyDismount`, `GetHeightBoost`, `DismountsOnItemUse` | `complete`, positive `Player.cs` sites returned |
| `Find-CpgCallSites(UpdateEffects)`, `UpdateDrill`, `UseDrill`, `Hover`, `Flight`, `AbilityRecovery`, `FatigueRecovery`, `ResetFlightTime` | `partial`, zero returned with `NoMatchingFactInScannedScope`; source callers were checked separately and zero is not treated as no caller |

The CPG result is an indexed static fact, not a complete runtime graph. Open dispatch, reflection,
callbacks, aliases, callee effects, scheduler order, serialization, network, persistence,
exception, and unload paths remain `partial` or `unknown` where not closed by source inspection.

### Complete claimed member coverage

The table covers all 17 groups in the claimed ledger. Source member names and declaration lines
remain authoritative in the input report; the ranges below are coverage indexes, not runtime order.

| Leaf group | Count | Covered member set / source range | Candidate role |
| --- | ---: | --- | --- |
| `MountFrameAndDrawCatalog` | 11 | `FrameStanding`, `FrameRunning`, `FrameInAir`, `FrameFlying`, `FrameSwimming`, `FrameDashing`, `DrawBack`, `DrawBackExtra`, `DrawFront`, `DrawFrontExtra`, `idleFrames_Rat` (rows 269-278, 323) | immutable Definition + Query |
| `MountSpecialVehicleCatalog` | 5 | `mounts`, `scutlixEyePositions`, `scutlixTextureSize`, `scutlixBaseDamage`, `santankTextureSize` (279-282, 292) | immutable Definition + Query |
| `MountDrillConstants` | 9 | `drillDiodePoint1`, `drillDiodePoint2`, `drillTextureSize`, `drillTextureWidth`, `drillRotationChange`, `drillPickPower`, `drillPickTime`, `amountOfBeamsAtOnce`, `maxDrillLength` (283-291) | immutable Definition + Query |
| `MountSuperCartConstants` | 5 | `SuperCartRunSpeed`, `SuperCartDashSpeed`, `SuperCartAcceleration`, `SuperCartJumpHeight`, `SuperCartJumpSpeed` (317-321) | immutable Definition + Query |
| `MountRuntimeFrameAndFlightState` | 15 | `_data`, `_type`, `_flipDraw`, `_frame`, `_frameCounter`, `_frameExtra`, `_frameExtraCounter`, `_frameState`, `_flyTime`, `_idleTime`, `_idleTimeNext`, `_shouldSuperCart`, `_walkingGraceTimeLeft`, `_mountSpecificData`, `_active` (293-303, 312-313, 315-316) | authoritative runtime Component |
| `MountFatigueAndAbilityState` | 8 | `_fatigue`, `_fatigueMax`, `_abilityCharging`, `_abilityCharge`, `_abilityCooldown`, `_abilityDuration`, `_abilityActive`, `_aiming` (304-311) | authoritative runtime Component |
| `MountRuntimeIdentityAndFrameProjection` | 13 | `_debugDraw`, `_defaultDelegatesData`, `Active`, `Type`, `Frame`, `FlyTime`, `BodyFrame`, `RunningGraceTime`, `PlayerXOFfset`, `PlayerOffset`, `PlayerOffsetHitbox`, `PlayerHeadOffset`, `HeightBoost` (314, 322, 324-334) | read-only Query/Projection |
| `MountRuntimeMobilityAndAbilityProjection` | 16 | `RunSpeed`, `DashSpeed`, `Acceleration`, `AutoJump`, `BlockExtraJumps`, `IsConsideredASlimeMount`, `Cart`, `CanGrindRails`, `AnyTrackRider`, `CanUseWings`, `Delegations`, `AbilityCharging`, `AbilityActive`, `AbilityCharge`, `AllowDirectionChange`, `DismountOnItemUse` (335-350) | read-only Query/qualification |
| `MountGeometryAndOffsetCatalog` | 9 | `textureWidth`, `textureHeight`, `xOffset`, `yOffset`, `playerYOffsets`, `bodyFrame`, `playerHeadOffset`, `heightBoost`, `playerXOffset` (210-217, 268) | immutable Definition + Query |
| `MountGroundAnimationFrames` | 11 | `totalFrames`, `standingFrameStart`, `standingFrameCount`, `standingFrameDelay`, `runningFrameStart`, `runningFrameCount`, `runningFrameDelay`, `idleFrameStart`, `idleFrameCount`, `idleFrameDelay`, `idleFrameLoop` (239-245, 252-255) | immutable Definition + Query |
| `MountAerialAndWaterAnimationFrames` | 9 | `flyingFrameStart`, `flyingFrameCount`, `flyingFrameDelay`, `inAirFrameStart`, `inAirFrameCount`, `inAirFrameDelay`, `swimFrameStart`, `swimFrameCount`, `swimFrameDelay` (246-251, 256-258) | immutable Definition + Query |
| `MountDashAnimationFrames` | 3 | `dashingFrameStart`, `dashingFrameCount`, `dashingFrameDelay` (259-261) | immutable Definition + Query |
| `MountMovementAndAbilityCatalog` | 18 | `flightTimeMax`, `usesHover`, `runSpeed`, `dashSpeed`, `swimSpeed`, `acceleration`, `jumpSpeed`, `jumpHeight`, `fallDamage`, `extraFall`, `fatigueMax`, `constantJump`, `blockExtraJumps`, `abilityChargeMax`, `abilityDuration`, `abilityCooldown`, `walkingGraceTimeMax`, `dismountsOnItemUse` (219-236) | immutable Definition + Query |
| `MountVehicleAndPresentationCatalog` | 9 | `buff`, `spawnDust`, `spawnDustNoGravity`, `Minecart`, `CanRideMinecartTracks`, `CanUseWings`, `lightColor`, `emitsLight`, `delegations` (218, 237-238, 262-267) | immutable Definition + effect descriptors |
| `MountDelegateContract` | 8 | `MinecartDust`, `MinecartJumpingSound`, `MinecartLandingSound`, `MinecartBumperSound`, `MouthPosition`, `HandPosition`, `PlayerSize`, `DashDust` (202-209) | explicit Adapter/Effect Port |
| `DrillMountRuntime` | 9 | `curTileTarget`, `cooldown`, `lastPurpose`, `diodeRotationTarget`, `diodeRotation`, `outerRingRotation`, `beams`, `beamCooldown`, `crosshairPosition` (188-196) | authoritative drill Component |
| `MountVariantFlags` | 5 | `boolean`, `showFlyingFrames`, `allowedToFly`, `frame`, `frameCounter` (197-201) | tagged variant state |

## Prior Component Decomposition Reconciliation

The prior P03 Component design and execution documents are useful checkpoints, not System owner
evidence. They record four component source files as present (`MountRuntimeFrameAndFlightState`,
`MountFatigueAndAbilityState`, `DrillMountRuntime`, and `MountVariantFlags`) while Systems,
complete catalogs, Definitions, Query composition, Effect Adapters, and focused verifiers remain
unimplemented. The affected Player build failure recorded there is historical evidence only; no
build was run for this report.

| Prior candidate | Reconciliation in this System report |
| --- | --- |
| `MountRuntimeFrameAndFlightStateComponent` | Keep as data owned by one proposed `MountRuntimeSystem`; `_data` becomes a validated definition handle and `_mountSpecificData` cannot remain an untyped parallel owner. |
| `MountFatigueAndAbilityStateComponent` | Keep the state component, but ability, fatigue, hover and flight transitions remain one coordinated Mount runtime domain until write order and commit ownership are verified. |
| `DrillMountRuntimeComponent` | Keep as short-lived authoritative drill state; tile mutation, projectile creation, dust, random and cursor services remain explicit ports. |
| `MountVariantStateComponent` | Keep as a tagged variant representation; do not infer serialization or network compatibility from the existing source alone. |
| `PlayerMountState.cs`, `PlayerMountComponent.cs`, empty `MountDefinition`, and `PlayerMountVehicleIntegrationComponent` | Existing overlapping skeletons are not accepted as owners. An integration review must select one runtime owner and retire or adapt duplicates before any write path is routed. |
| Existing `PlayerItemMountAndRuntimePropertiesQuery`, `PlayerSpatialDerivedPropertiesQuery`, `UsingSuperCartQuery` | Treat as candidate read surfaces. They must not acquire hidden writes or silently replace Version4 mount semantics without focused evidence. |

## Conceptual Behaviors

The legacy `Mount` type combines immutable catalog data, per-player authoritative state, derived
views, animation selection, vehicle variants, drill state, callbacks, and external effects. The
proposed behavior slices are:

1. **Catalog bootstrap and lookup**: `Mount.Initialize` builds static definition data; immutable
   catalog views provide geometry, movement, animation, vehicle, drill, and presentation values by
   mount type. Static arrays are not entity state.
2. **Mount activation and deactivation**: `SetMount`, `Dismount`, `Reset`, `CanMount`, and
   `CanDismountWithResult` coordinate qualification, definition selection, active/type state,
   player geometry, buffs, variant initialization, and effect intents. Qualification and commit
   must be one composition with commit-time recheck; a standalone Query must not create a TOCTOU
   window.
3. **Frame and mobility transition**: `UpdateFrame`, `Hover`, `Flight`, `ResetFlightTime`, and
   `UpdateAfterEquips` read player/collision inputs and commit frame, flip, grace, flight, velocity,
   position, fall and rotation changes. These are Commands/state transitions, not Queries.
4. **Fatigue and ability transition**: `StartAbilityCharge`, `AbilityRecovery`,
   `FatigueRecovery`, `UseAbility`, and `AimAbility` update timers, charge, active/aiming state,
   direction and special mount data. They may return qualification results, but the call is not
   pure when state or effects change.
5. **Drill transition**: `UpdateDrill` and `UseDrill` update beam targets, rotations and
   cooldowns and may emit tile, projectile, dust and random effects. `DrillSmartCursor_Blocks` and
   `DrillSmartCursor_Walls` are empty in the inspected source; their semantics remain `unknown`.
6. **Presentation/effect publication**: `UpdateEffects`, frame drawing data, light, dust, sound,
   player-size delegates and projectile effects produce external observations. They consume a
   committed mount view and must not become hidden state writers.
7. **Read-only projections**: `Active`, `Type`, `Frame`, `FlyTime`, offsets, speed, cart/rail/wing
   qualification and item-use dismount policy can be exposed as immutable views only after the
   underlying state and catalog snapshot are fixed. `Delegations` is a data-only descriptor.

## State Ownership and Write Closure

### Proposed owner and write sets

`MountRuntimeSystem` is the proposed single authoritative owner for a player mount's active/type,
frame/flight/grace and coordinated fatigue/ability transitions. It owns the transition commands,
not the Player entity's complete movement state. `DrillMountSystem` owns the drill component's
beam/rotation/cooldown state. Catalog Definitions own immutable bootstrap data. Queries and
Projections never write. Effect Adapters execute explicit intents after authoritative commit.

| State or member family | Proposed owner | Direct readers | Direct/indirect writers | Effects and status |
| --- | --- | --- | --- | --- |
| `MountData` catalog and constants | `MountDefinitionCatalog` bootstrap | mount runtime, mobility/frame/geometry/drill queries, Main/Projectile consumers | bootstrap registration only; mod/config registration is `unknown` | immutable read; `confirmed` structure, lifecycle/persistence `partial` |
| active/type/definition handle, frame counters, flip, grace, flight | `MountRuntimeSystem` over `MountRuntimeFrameAndFlightStateComponent` | Player movement/frame, Main geometry, Projectile visibility, projections | Set/dismount/reset/frame/flight paths; no second writer permitted | player position/velocity/size and collision are external commit ports; writer closure `partial` |
| fatigue/ability charge/cooldown/duration/active/aiming | coordinated Mount ability transition within `MountRuntimeSystem` using `MountFatigueAndAbilityStateComponent` | movement, ability/projectile callers, mobility Query | ability, recovery, hover/flight and aim paths | projectile/tile/effect publication through ports; fractional/zero-max semantics need verifier |
| variant flags and special mount state | `MountVariantSystem` as a partial collaborator under runtime owner | frame, mobility and vehicle projections | `SetMount`, variant initialization, frame/ability paths | tagged state proposed; serialization/network scope `unknown` |
| drill beams, target, rotations and cooldown | `DrillMountSystem` over `DrillMountRuntimeComponent` | Player drill path, draw/effect adapter, tile/projectile adapters | `UpdateDrill`, `UseDrill`, ability integration | tile mutation/projectile/random/dust through explicit ports; smart-cursor behavior `unknown` |
| geometry, animation and vehicle catalogs | immutable Definitions | read-only Queries and adapters | initialization only | no per-player counters or callbacks allowed |
| delegates (`MinecartDust`, sound, position, size, dash dust) | `IMountEffectPort` adapter | presentation/effect paths | adapter invocation from committed intents | executable delegate stored in core is rejected; failure/retry `unknown` |

The write closure is intentionally conservative. Player remains the owner of player position,
velocity, hitbox, buffs, fall flags, input and entity lifecycle; Collision remains an external
qualification/commit authority; Tile, Projectile, lighting, sound, dust, random, network and
persistence owners are not claimed by P03.

### Query API rule

The proposed mount Queries return immutable catalog or runtime views and perform no callbacks,
random reads, lazy writes, logging, network publication, collision mutation, or persistence.
`CanMount`, `CanDismountWithResult`, drill cursor checks and any method that invokes Collision,
updates caches, reads time/random, or prepares effects are `QueryCandidate/mixed` until the source
and commit protocol prove otherwise. A qualification Query must be paired with a commit-time
recheck inside the owner System.

## Boundary Role and Decision

| Boundary | Decision | Reason and rejected alternative |
| --- | --- | --- |
| Mount runtime transition owner | `separate` proposed `MountRuntimeSystem` with a coordinated partial Ability/Fatigue collaborator | One owner is needed for active/type/frame/flight/resource invariants. Splitting every field into scheduler systems would create ordering and double-write risk. Keeping all legacy methods in one facade would retain catalog/effect/player coupling. |
| Frame/flight and ability/fatigue | `partial` within the runtime domain, with explicit state components and collaboration API | They share mount activation, flight, fatigue and frame invariants. Independent scheduling is not justified until source ordering and commit barriers are verified. |
| Drill runtime | `separate` `DrillMountSystem` | Beam/target/rotation state has a distinct lifecycle and external tile/projectile effects, while activation/ability qualification is supplied by the runtime owner. A generic Mount component would hide drill ownership. |
| Catalogs and animation data | `separate` immutable Definition/Catalog services plus read-only Queries | Bootstrap lifetime differs from player lifetime. Copying catalogs into entity state or leaving mutable static arrays exposed is rejected. |
| Runtime projections | `separate` pure Query/Projection boundary | Derived values must not become a second state owner. Existing Query names are candidate surfaces only. |
| Delegate and presentation effects | `separate` Adapter/Effect Port | Executable delegates inside a Component would hide side effects and retry semantics. Adapter must not copy mount rules. |
| Player, Collision, Tile, Projectile, network, persistence | `crossSubsystemOwner: integration-review` | These boundaries have shared IDs, state, ordering or effects not fully closed in P03. P03 does not assign their final owner. |

## System API and Legacy Behavior Mapping

The mapping uses stable conceptual behavior IDs; one legacy method may compose several new calls.
All new names are proposed and no corresponding System implementation is claimed.

| Legacy entry point(s) | Concept ID | Proposed composition | Classification |
| --- | --- | --- | --- |
| `Mount.Initialize` from `Main.Initialize` | `MountCatalog.Bootstrap` | `MountDefinitionCatalog.Register/Freeze` -> immutable catalog snapshot -> Query availability | Command/bootstrap; call-site `confirmed`, extension lifecycle `unknown` |
| `CanMount` + `SetMount` | `MountLifecycle.Activate` | `MountQualificationQuery` (read-only snapshot) -> owner commit/recheck -> runtime state transition -> player geometry/buff/effect ports | mixed qualification + Command; must not expose an unchecked two-step API |
| `CanDismountWithResult` + `TryDismountWithResult` + `Dismount` | `MountLifecycle.Deactivate` | dismount-space qualification -> owner transition -> player reset/size/buff commit -> effect publication | mixed Query/Command composition; Collision and failure semantics `partial` |
| `Reset` | `MountLifecycle.Reset` | owner reset command clears runtime/variant/drill state and requests player reset | Command; world/session reset scope `unknown` |
| `UpdateFrame`, `UpdateFrame_Velociraptor` | `MountPresentation.FrameTransition` | runtime state snapshot + catalog frame Query + injected input/velocity/random decision -> frame commit -> presentation projection | state transition; random and special override closure `partial` |
| `Hover`, `Flight`, `ResetFlightTime`, `AbilityRecovery`, `FatigueRecovery` | `MountMobility.ResourceTick` | owner tick command consumes Player/Collision snapshot -> fatigue/flight/ability transition -> Player movement commit | Command; not a Query |
| `UseAbility`, `AimAbility` and Projectile ability sites | `MountAbility.ActivateOrAim` | input command -> owner qualification/recheck -> ability state commit -> Projectile/effect intents | Command with external effects; call edges `confirmed`, dispatch/retry `partial` |
| `UpdateDrill`, `UseDrill` and Player drill sites | `MountDrill.Execute` | drill input/cursor snapshot -> `DrillMountSystem` -> tile pick/projectile/effect adapters -> cooldown/beam commit | Command; cursor stubs and tile ownership `unknown` |
| `UpdateEffects` and delegate fields | `MountEffects.Publish` | committed mount view -> `IMountEffectPort` descriptors -> lighting/dust/sound/size/projectile adapters | effect Adapter/Projection; no hidden writes |
| `Active`, `Type`, `Frame`, `FlyTime`, offsets, speed/cart/wing properties | `MountReadModel.GetView` | immutable Definition + committed runtime components -> pure Query/Projection | Query candidate; must preserve inactive/default semantics |
| `CheckMountBuff`, `UpdateAfterEquips`, item-use/death/receive-hit dismount callers | `MountLifecycle.ExternalReconciliation` | external Player event/input adapter -> owner command -> ordered state/effect commits | integration command; buff ownership and event order `partial` |

Compatibility requires preserving the observation vector `(return/error, authoritative state delta,
events/effects, order/visibility, lifecycle/scope, retry/idempotency)`. Signature compatibility or
an old facade remaining callable is insufficient.

## Call and Dependency DAG

The following is a proposed composition constrained by observed static edges. It is a dependency
DAG for the proposed integration contract, not evidence that Version4 currently schedules these
phases this way.

```text
Bootstrap phase
  Main.Initialize
    -> MountDefinitionCatalog.Register/Freeze
    -> Minecart/vehicle catalog handoff

Input/qualification phase
  Player input/buff/item/death + Projectile ability input
    -> MountQualificationQuery / MountExternalEventAdapter
    -> Collision/Player snapshot (read-only handoff)

Authoritative transition phase
  MountRuntimeSystem
    -> activation/dismount/reset
    -> frame/flight/fatigue/ability commits
  DrillMountSystem
    -> beam/rotation/cooldown commit

Commit/effect phase
  Player movement/geometry commit and Collision commit
    -> MountEffects.Publish
    -> Tile/Projectile/Lighting/Sound/Dust adapters

Observation phase
  MountRuntime*Query / Projection
    -> Main item/geometry reads, Player movement reads, Projectile visibility, network/persistence projections
```

Required ordering edges are `catalog freeze -> activation`, `qualification snapshot -> commit-time
recheck`, `runtime state commit -> projection/effect publication`, and `drill intent validation ->
tile/projectile effect -> drill cooldown/beam commit` where the legacy behavior requires the effect
to be observable before the next cooldown state. `Dismount` must clear runtime state before final
player presentation and must not publish a stale mounted projection.

CPG confirms positive static edges for `Main -> Mount.Initialize`, `Player -> SetMount`,
`Player/Mount -> Dismount`, `Player -> UpdateFrame`, `Player/Projectile -> UseAbility` and
`AimAbility`, and `Player/Projectile -> CanFly`. CPG results for several update methods were
`partial` with zero returned; those paths are retained from source inspection and are not removed
from the DAG. Dynamic delegates, event registration, scheduler phase names, re-entrancy, and
cross-world ordering remain `unknown`.

## Lifecycle and Side Effects

| Phase | Proposed responsibility | Side effects and open facts |
| --- | --- | --- |
| create/bootstrap | construct immutable catalog and validate IDs/ranges | `Mount.Initialize` is called during Main initialization; mod registration and reload/unload behavior `unknown` |
| activate | qualify and commit active/type/definition/variant, initialize resources, request player geometry/buff changes | `SetMount` mutates Mount and Player and emits effects; atomicity and failure compensation `partial` |
| update | process movement, flight, fatigue, ability, frame and drill commands in an explicit schedule | velocity/position/fall/rotation, tile/projectile, random/dust/light/sound effects cross owners; exact legacy order `partial` |
| observe | expose immutable runtime/catalog views | Query must not write or invoke delegates; current snapshot/network coverage is incomplete |
| end/dismount | qualify space, clear buffs/runtime/variant/drill, restore player geometry and publish effects | `Dismount` and early/item/death paths are observed; no final owner for Player geometry or buff storage in P03 |
| reset/rebuild | clear per-player state and optionally rebuild catalog under an explicit world/session boundary | reset scope, multi-world isolation, reinitialization and stale adapter cleanup `unknown` |
| persistence/network | project committed mount type/resources/variant/drill state through reviewed versioned adapters | existing current NLTX snapshots expose only a subset; schema, prediction, save/load and retry policy `unknown` |
| exception/cancellation | preserve owner invariants and prevent partial external effects from becoming duplicate commits | delegate/tile/projectile failure, cancellation, retry and idempotency are `unknown` |

Effects must be represented as explicit intents or ports. A Query, Projection, Definition or
Component may not call sound, dust, lighting, random, tile, projectile, network or persistence
services. Adapter failures and duplicate publication require a later integration verifier.

## Integration Handoff

The following are mandatory `crossSubsystemOwner: integration-review` handoffs:

| Handoff | Reason |
| --- | --- |
| Player entity identity, position, velocity, hitbox, fall flags, buffs, input, death and item lifecycle | Mount transitions mutate or depend on these fields, but P03 cannot make Player the Mount state owner. |
| Collision fit, landing, slope, dismount-space and smart-cursor behavior | Collision is an external authority; empty drill smart-cursor bodies and shared result flags prevent a closed Query contract. |
| Tile mutation and mining transaction | Drill emits a validated intent; tile ownership, authority, ordering and retry are outside P03. |
| Projectile allocation, ability release and visibility/alpha | Projectile is a positive caller/consumer of Mount APIs; IDs, lifetime, network and failure semantics are not closed. |
| Minecart collision, track movement, sounds, dust and player-size delegates | Delegate descriptors can cross the port, but execution owner and ordering are unresolved. |
| Network input/replication, persistence, snapshots and save/load | Current NLTX state is partial and does not represent all mount runtime or variant/drill state. |
| Lighting, rendering, random, dust and audio services | `UpdateEffects` and animation paths produce external effects; adapter contracts and idempotency are not verified. |
| World/session/multi-player scope, scheduler phases and unload/reload | Static catalog and per-player runtime have different lifetimes; no complete session boundary evidence exists. |

No cross-partition owner is claimed by this report, and no existing duplicate skeleton is deleted.

## Migration Behavior Contract

The proposed migration sequence is:

1. Freeze immutable catalog Definitions and Query views without routing writes.
2. Select one Mount runtime owner and adapt or retire overlapping NLTX mount skeletons; establish a
   one-writer assertion before moving any transition.
3. Route activation, dismount, reset, frame, flight, fatigue and ability through the owner System
   with commit-time qualification rechecks.
4. Route drill state through `DrillMountSystem`, keeping tile/projectile/random/effect commits at
   explicit adapters.
5. Add read-only projections and versioned network/persistence adapters only after authoritative
   state and variant scope are accepted.
6. Retain compatibility adapters until focused behavior evidence covers normal, inactive, invalid,
   duplicate, reset, death, item-use, ability, drill and recovery paths; then apply a separate
   deletion decision.

For each input, compare the old and proposed composition on return/error, state delta, emitted
effects/events, ordering and visibility, lifecycle scope, retry and idempotency. The following
cases are required later and are currently unrun: catalog bootstrap/defaults, activate/dismount
with blocked space, reset, frame sequences including idle randomness, hover/flight/fatigue edge
cases, ability aim/charge/release, drill target/cooldown/duplicate effects, vehicle delegates,
death/item/buff reconciliation, snapshot/network/save-load, multi-world isolation, and adapter
failure/retry. No behavior-equivalence or migration-success claim is made.

## Evidence Gaps and Blocking Decisions

| Gap or decision | Status | Blocking consequence |
| --- | --- | --- |
| CPG database has no source snapshot ID and selected update call-sites returned `partial`/zero | `partial` | Source inspection supplements positive edges; no zero result is treated as no caller. |
| Runtime scheduling, phase barriers, re-entrancy and multi-world/session scope | `unknown` | Cannot finalize scheduler registration or order beyond the proposed must-before edges. |
| Unique writer for Player geometry, velocity, buffs, collision flags and mount type across current NLTX | `partial` | Blocks implementation routing and legacy deletion. |
| `MountData` registration, mod extension, reload/unload and immutable snapshot lifecycle | `partial` | Blocks catalog freeze and hot-reload contract. |
| Empty `DrillSmartCursor_Blocks/Walls` behavior | `unknown` | Blocks pure drill qualification and behavior equivalence. |
| Drill tile authority, beam/projectile identity, random/dust effects, cooldown commit and retries | `unknown` | Blocks DrillMountSystem commit contract. |
| Delegate invocation order, exception policy, sound/dust/light/player-size effect ownership | `unknown` | Blocks `IMountEffectPort` failure and idempotency contract. |
| Variant object state schema, serialization, network/prediction and reset semantics | `unknown` | Blocks tagged variant persistence and compatibility. |
| Existing `PlayerMountState`/`PlayerMountComponent`/`MountDefinition` overlap | `partial` | Integration review must select one owner before any second writer is introduced. |
| Snapshot/network/persistence fields for fatigue, ability, frame, variant and drill | `unknown` | Blocks boundary compatibility and rollback claims. |
| Historical failed build evidence for prior components | `existing-evidence` | It does not establish current compilation; no build was run here. |

Blocking decisions for the next integration session are: select the single runtime owner; approve
the Query purity and commit-time recheck contract; assign Player/Collision/Tile/Projectile/effect
ports; define variant and drill schema; and define scheduler, network, persistence, retry and
multi-world policies. Until then the migration status remains `deferred`.

## Verification Plan

`verificationStatus: not-run` is intentional. This session did not run a build, test, runtime,
behavior verifier, migration harness, or compile-capable command.

Future verification must be performed in the real migration project and must include:

- static member-coverage and one-owner checks for all 163 claimed members;
- immutable catalog/default/range and Query no-write checks;
- activation, dismount, reset, frame, flight, fatigue and ability state-transition scenarios;
- drill cursor, target, beam, cooldown, tile/projectile/effect ordering and duplicate/retry cases;
- vehicle/delegate effect ordering, failure and idempotency cases;
- Player/Collision integration, death/item/buff reconciliation, network/persistence and multi-world scope;
- observation-vector comparison through the new owner and API composition, followed by a separate
  old-entry deletion gate.

The report is `proposed` and the migration is `deferred`; neither the report nor a future runner
`Complete` settlement may be described as migration success.
