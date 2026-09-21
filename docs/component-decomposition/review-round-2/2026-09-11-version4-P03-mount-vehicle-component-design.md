# Version4 P03 坐骑与车辆组件拆分设计

> 本文最初是 P03 的只读边界设计；当前 checkpoint 已保存四个组件源码，系统、查询、定义和适配器仍未实现。

partitionId: P03  
sessionId: 1c308b3eb4f84a0987329c3385ba10ee  
inputReport: D:\TRbackup\NLTX\docs\migration\ledgers\authoritative-20-partitions\P03-Mount-Vehicle.md  
componentDesignPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\2026-09-11-version4-P03-mount-vehicle-component-design.md  
componentExecutionPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\2026-09-11-version4-P03-mount-vehicle-component-execution.md  
reportType: public-decomposition  
executionMode: independent-read-only-partition  
formalParentSubsystem: MountAndVehicleSimulation  
inputMemberCount: 163  
inputLeafSubsystemCount: 17  
evidenceStatus: source-inventory-confirmed; Version4-source-confirmed; current-NLTX-partial; tModLoader-cross-reference; SS14-organization-reference  
nltxStatus: partial  
verificationStatus: not-verified  
implementationStatus: partial  
completedComponents: [MountRuntimeFrameAndFlightState, MountFatigueAndAbilityState, DrillMountRuntime, MountVariantFlags]  
currentComponent: none  
pendingComponents: [MountFrameAndDrawCatalog, MountSpecialVehicleCatalog, MountDrillConstants, MountSuperCartConstants, MountRuntimeIdentityAndFrameProjection, MountRuntimeMobilityAndAbilityProjection, MountGeometryAndOffsetCatalog, MountGroundAnimationFrames, MountAerialAndWaterAnimationFrames, MountDashAnimationFrames, MountMovementAndAbilityCatalog, MountVehicleAndPresentationCatalog, MountDelegateContract]  
lastCheckpointUtc: 2026-09-12T07:05:35.6354864Z  
evidence-gap: Component source for C05, C06, C16, and C17 is saved under src/Player/Mount/Components, but no state-transition system, query, definition catalog, adapter, test, or project-specific verifier was added. The affected Player project build was attempted and failed on five pre-existing cross-project reference errors; no successful compile evidence exists for the new components. Existing PlayerMountComponent/PlayerMountState remain overlapping legacy skeletons and have not been rewired. Tagged-variant serialization, drill target commit, beamCooldown ownership, delegate registration, and external-effect retry policy remain open.  
blocking-decision: This checkpoint is limited to ECS component source. The planned C01-C04 and C07-C15 units require definitions, queries, systems, or adapters and are therefore pending under the task's absolute component-only scope. Integration review must still choose the single runtime owner, reset semantics, variant schema, drill/effect commit, and adapter failure policy; no Player, collision, tile, projectile, random, dust, sound, lighting, network, or persistence owner is claimed. The failed predecessor session `2ff556cf95db4c3b98a147cc837f2863` was retried through the runner as the current session `1c308b3eb4f84a0987329c3385ba10ee`; no component boundary was broadened.  

## 1. 设计摘要

### Version4 facts

- The authoritative report contains 17 leaf subsystems under `MountAndVehicleSimulation`, with 136 fields, 27 properties, and 163 members.
- `D:\TRbackup\Version4\Terraria\Mount.cs` combines static mount definitions, per-player runtime state, derived properties, animation catalogs, special vehicle data, drill state, and delegate callbacks in one legacy type.
- `Mount.Initialize` constructs the static `MountData[]` catalog. `SetMount` and `Dismount` change per-player state and also mutate player geometry, buffs, rotation, sounds, and dust. `UpdateFrame`, `Hover`, `Flight`, `AbilityRecovery`, `FatigueRecovery`, `UpdateDrill`, and `UpdateEffects` are state transitions or effect-producing behavior.
- `Player.cs` is a major caller and writer. `Main.cs`, `Projectile.cs`, `Collision.cs`, and delegate methods are additional consumers or effect boundaries. File order is not a runtime ordering contract.

### Current NLTX status

- `dome/src/Terraria.Dome.Simulation/Player/Components/PlayerMountStateComponent.cs:6-141` owns a narrow typed mount state with mount type, flight time, fatigue, frame state, and consume/recovery methods.
- `dome/src/Terraria.Dome.Simulation/Player/Definitions/MountCapabilityRegistry.cs:6-386` contains source-backed capability and movement tables, but it is not a complete `MountData` catalog and does not cover all animation, geometry, delegate, drill, or presentation members.
- `dome/src/Terraria.Dome.Simulation/Player/Systems/PlayerControlSystem.cs:26-80` reads mount capabilities for horizontal movement, jump, and a narrow flight-input path. `PlayerGravitySystem` only reads the hover projection.
- `dome/src/Terraria.Dome.Simulation/Simulation/DomeSimulation.cs:4411-4422`, `5900-5936`, and `7530-7534` show mount control, death/respawn clearing, and item-summon application. `PlayerSnapshot` and `PlayerStateSnapshot` expose only `MountType`, `FlightTimeRemaining`, and `FatigueRemaining`; the full Mount runtime and catalog are not represented.
- `src/Player/PlayerMountState.cs:3-40`, `src/Player/PlayerMountComponent.cs:3-27`, and the empty `MountDefinition` at `src/Player/PlayerValueTypes.cs:102` are separate skeleton surfaces. They must not become parallel authority owners.
- `dome/src/Terraria.Dome.Server/Replication/PlayerStateProjection.cs:10-49` and `dome/src/Terraria.Dome.Protocol.V1456/Compatibility/LegacyPlayerControlsState.cs:3-16` carry mount type through a protocol projection/input boundary, not a complete mount snapshot.

### Proposed design rules

1. Keep static mount definition/catalog data out of entity components. Catalogs are immutable after bootstrap and are read through narrow queries.
2. Keep per-player authoritative runtime state in separate components for frame/flight state, fatigue/ability state, variant state, and drill state. No `MountComponent` may reassemble every `MountData` field.
3. Keep `RunSpeed`, `DashSpeed`, `PlayerOffset`, `CanFly`, `Cart`, and similar values as pure projections over definition plus authoritative state. Projections do not write state.
4. Treat delegates as adapters/ports. Dust, sound, lighting, player resizing, projectile creation, tile mutation, and network publication are explicit effects and do not live in a component.
5. Use explicit commands and a scheduler contract for mount/dismount, movement, frame selection, ability recovery, drill actions, and effect publication. Directory or document order must not imply execution order.

## 2. Evidence log

| Evidence source | Exact evidence | What it establishes | Status |
|---|---|---|---|
| `D:\TRbackup\Version4\Terraria\Mount.cs` | `:18-127`, `:129-264` | Nested drill, variant, delegate, geometry, movement, animation, vehicle, and presentation data exist with different roles. | confirmed |
| `D:\TRbackup\Version4\Terraria\Mount.cs` | `:273-385`, `:387-613` | Frame/draw constants, static catalog, runtime fields, and derived public properties are separate semantic groups despite one legacy class. | confirmed |
| `D:\TRbackup\Version4\Terraria\Mount.cs` | `:616-643`, `:645-693` and the remaining `Initialize` body | Reset lifecycle and static catalog initialization. | confirmed |
| `D:\TRbackup\Version4\Terraria\Mount.cs` | `:2454-2593`, `:2650-2773`, `:2844-3059`, `:3089-3091`, `:4039-4090` | Flight, hover, ability, fatigue, drill, and frame transitions; these are behavior/system inputs, not passive data. | confirmed |
| `D:\TRbackup\Version4\Terraria\Mount.cs` | `:4097-4127`, `:4540-4702`, `:4721-4990` | Effects, aiming, dismount, mount activation, variant initialization, and collision-dependent geometry changes. | confirmed |
| `D:\TRbackup\Version4\Terraria\Player.cs` | `:4324`, `:11562-11650`, `:15102`, `:15319-15321`, `:16205-16600`, `:17664-17666`, `:20519-20536`, `:22648`, `:25284`, `:25741-25755` | Player is a broad caller/writer; movement, flight, drill, frame, item-use, dismount, and effect order must be integrated explicitly. | confirmed |
| `D:\TRbackup\Version4\Terraria\Main.cs` | `:3368-3369`, `:11786-11792`, `:12409` | Bootstrap, mounted geometry projection, and item-holding presentation consume mount state. | confirmed |
| `D:\TRbackup\Version4\Terraria\Projectile.cs` | `:26724-26766`, `:39824`, `:40660`, `:45062`, `:49300-49306` | Mount abilities and visibility affect projectile/effect paths. | confirmed |
| `D:\TRbackup\Version4\Terraria\Collision.cs` | `:1798-1830`; `Mount.cs:3061-3069`, `:4043-4058`, `:4974-4977` | Collision is an external authority for mount fit, landing, slope stabilization, and dismount space. | confirmed |
| `D:\TRbackup\tmodloader-api-docs-stable\index.html` | title `tModLoader v2026.07`; `class_mount.html`, `class_mount_1_1_mount_data.html`, `class_mount_1_1_mount_delegates_data.html` | Public API shape confirms that Mount exposes catalog, runtime properties, methods, and delegate data; it does not confirm private Version4 lifecycle or persistence semantics. | partial |
| `C:\Users\shan\Downloads\ECS\space-station-14-master\Content.Shared\Actions\ActionGrantSystem.cs:10-55` and related component files | EntitySystem queries a focused component and delegates external action creation/removal to explicit services. | Organization reference only; no Terraria semantics are inferred. | reference-only |
| `D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Player\Components\PlayerMountStateComponent.cs:6-141` | Current typed mount state and its mutation methods. | Current NLTX narrow implementation. | existing-evidence |
| `D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Player\Definitions\MountCapabilityRegistry.cs:6-386` | Current capability tables and movement values. | Current NLTX partial definition/query mapping. | existing-evidence |
| `D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Snapshots\PlayerSnapshot.cs:51-68`, `PlayerStateSnapshot.cs:47-64` | Snapshot includes mount type, flight remaining, and fatigue remaining only. | Current snapshot gap. | existing-evidence |
| `D:\TRbackup\NLTX\dome\src\Terraria.Dome.Server\Replication\PlayerStateProjection.cs:10-49` | Mount type is encoded through player controls. | Current protocol projection boundary. | existing-evidence |

The tModLoader queries used the required sequence: the local index identified `class_mount.html`; the type page confirmed public `SetMount` at `class_mount.html#a4a3117ac2a52c31e47f6e600f686dbf8`, `Type` at `class_mount.html#a4e26217fb77afaab91f4cff22379851f`, `RunSpeed` at `class_mount.html#afa396a22e873b102f050fd62cabf3766`, `PlayerOffset` at `class_mount.html#ad621f3d4207020f76de9fce374b34130`, `FlyTime` at `class_mount.html#a4365ef7b3d909bca14295e77d98b59d1`, and `SetAsMinecart` at `class_mount.html#a3bad3d641fffc13730fda7b3da72316c`. Private and undocumented lifecycle details remain `partial`.

## 3. Complete member coverage and proposed ownership

The following table covers every member row in the P03 report. The source row numbers are the report's stable inventory sequence, not a runtime ordering claim.

| Leaf subsystem | Report members | Proposed boundary | State kind | Main seam |
|---|---|---|---|---|
| `MountFrameAndDrawCatalog` | `FrameStanding`, `FrameRunning`, `FrameInAir`, `FrameFlying`, `FrameSwimming`, `FrameDashing`, `DrawBack`, `DrawBackExtra`, `DrawFront`, `DrawFrontExtra`, `idleFrames_Rat` (269-278, 323) | `MountFrameAndDrawCatalogDefinition` + `MountFrameAndDrawQuery` (proposed) | immutable definition/query | read-only catalog view |
| `MountSpecialVehicleCatalog` | `mounts`, `scutlixEyePositions`, `scutlixTextureSize`, `scutlixBaseDamage`, `santankTextureSize` (279-282, 292) | `MountSpecialVehicleCatalogDefinition` + query (proposed) | immutable definition/query | vehicle and ability consumers read by mount type |
| `MountDrillConstants` | `drillDiodePoint1`, `drillDiodePoint2`, `drillTextureSize`, `drillTextureWidth`, `drillRotationChange`, `drillPickPower`, `drillPickTime`, `amountOfBeamsAtOnce`, `maxDrillLength` (283-291) | `MountDrillConstantsDefinition` + `MountDrillRulesQuery` (proposed) | immutable definition/query | drill system gets validated constants |
| `MountSuperCartConstants` | `SuperCartRunSpeed`, `SuperCartDashSpeed`, `SuperCartAcceleration`, `SuperCartJumpHeight`, `SuperCartJumpSpeed` (317-321) | `MountSuperCartDefinition` + query (proposed) | immutable definition/query | cart movement projection |
| `MountRuntimeFrameAndFlightState` | `_data`, `_type`, `_flipDraw`, `_frame`, `_frameCounter`, `_frameExtra`, `_frameExtraCounter`, `_frameState`, `_flyTime`, `_idleTime`, `_idleTimeNext`, `_shouldSuperCart`, `_walkingGraceTimeLeft`, `_mountSpecificData`, `_active` (293-303, 312-313, 315-316) | `MountRuntimeFrameAndFlightStateComponent` (proposed) | authoritative entity state | `MountRuntimeSystem` + commit port |
| `MountFatigueAndAbilityState` | `_fatigue`, `_fatigueMax`, `_abilityCharging`, `_abilityCharge`, `_abilityCooldown`, `_abilityDuration`, `_abilityActive`, `_aiming` (304-311) | `MountFatigueAndAbilityStateComponent` (proposed) | authoritative entity state | `MountAbilitySystem` + explicit input command |
| `MountRuntimeIdentityAndFrameProjection` | `_debugDraw`, `_defaultDelegatesData`, `Active`, `Type`, `Frame`, `FlyTime`, `BodyFrame`, `RunningGraceTime`, `PlayerXOFfset`, `PlayerOffset`, `PlayerOffsetHitbox`, `PlayerHeadOffset`, `HeightBoost` (314, 322, 324-334) | `MountRuntimeIdentityAndFrameProjectionQuery` (proposed) | derived projection/cache-free query | immutable mount runtime view |
| `MountRuntimeMobilityAndAbilityProjection` | `RunSpeed`, `DashSpeed`, `Acceleration`, `AutoJump`, `BlockExtraJumps`, `IsConsideredASlimeMount`, `Cart`, `CanGrindRails`, `AnyTrackRider`, `CanUseWings`, `Delegations`, `AbilityCharging`, `AbilityActive`, `AbilityCharge`, `AllowDirectionChange`, `DismountOnItemUse` (335-350) | `MountRuntimeMobilityAndAbilityProjectionQuery` (proposed) | derived projection/qualification | pure query; no writeback |
| `MountGeometryAndOffsetCatalog` | `textureWidth`, `textureHeight`, `xOffset`, `yOffset`, `playerYOffsets`, `bodyFrame`, `playerHeadOffset`, `heightBoost`, `playerXOffset` (210-217, 268) | `MountGeometryAndOffsetCatalogDefinition` + query (proposed) | immutable definition/query | geometry snapshot to movement/presentation |
| `MountGroundAnimationFrames` | `totalFrames`, `standingFrameStart`, `standingFrameCount`, `standingFrameDelay`, `runningFrameStart`, `runningFrameCount`, `runningFrameDelay`, `idleFrameStart`, `idleFrameCount`, `idleFrameDelay`, `idleFrameLoop` (239-245, 252-255) | `MountGroundAnimationFramesDefinition` + query (proposed) | immutable definition/query | frame selection query |
| `MountAerialAndWaterAnimationFrames` | `flyingFrameStart`, `flyingFrameCount`, `flyingFrameDelay`, `inAirFrameStart`, `inAirFrameCount`, `inAirFrameDelay`, `swimFrameStart`, `swimFrameCount`, `swimFrameDelay` (246-251, 256-258) | `MountAerialAndWaterAnimationFramesDefinition` + query (proposed) | immutable definition/query | frame selection query |
| `MountDashAnimationFrames` | `dashingFrameStart`, `dashingFrameCount`, `dashingFrameDelay` (259-261) | `MountDashAnimationFramesDefinition` + query (proposed) | immutable definition/query | dash frame query |
| `MountMovementAndAbilityCatalog` | `flightTimeMax`, `usesHover`, `runSpeed`, `dashSpeed`, `swimSpeed`, `acceleration`, `jumpSpeed`, `jumpHeight`, `fallDamage`, `extraFall`, `fatigueMax`, `constantJump`, `blockExtraJumps`, `abilityChargeMax`, `abilityDuration`, `abilityCooldown`, `walkingGraceTimeMax`, `dismountsOnItemUse` (219-236) | `MountMovementAndAbilityCatalogDefinition` + query (proposed) | immutable definition/query | movement and ability systems read by type |
| `MountVehicleAndPresentationCatalog` | `buff`, `spawnDust`, `spawnDustNoGravity`, `Minecart`, `CanRideMinecartTracks`, `CanUseWings`, `lightColor`, `emitsLight`, `delegations` (218, 237-238, 262-267) | `MountVehicleAndPresentationCatalogDefinition` + adapter-facing query (proposed) | immutable definition plus effect descriptors | presentation/effect adapters |
| `MountDelegateContract` | `MinecartDust`, `MinecartJumpingSound`, `MinecartLandingSound`, `MinecartBumperSound`, `MouthPosition`, `HandPosition`, `PlayerSize`, `DashDust` (202-209) | `IMountEffectPort` / `MountDelegateAdapter` (proposed) | external effect port; not a component | explicit command/effect boundary |
| `DrillMountRuntime` | `curTileTarget`, `cooldown`, `lastPurpose`, `diodeRotationTarget`, `diodeRotation`, `outerRingRotation`, `beams`, `beamCooldown`, `crosshairPosition` (188-196) | `DrillMountRuntimeComponent` + `DrillMountSystem` (proposed) | authoritative short-lived state | validated drill command to tile/projectile adapters |
| `MountVariantFlags` | `boolean`, `showFlyingFrames`, `allowedToFly`, `frame`, `frameCounter` (197-201) | `MountVariantStateComponent` with an explicit tagged variant (proposed) | authoritative mutually-exclusive variant state | variant command/query; no object-typed state |

## 4. Boundary contracts

### 4.1 Proposed components and systems

All names in this section have `status: proposed`.

- `MountRuntimeFrameAndFlightStateComponent` owns active/type/frame/frame-state/flight counter/grace counter and a stable reference to a catalog definition. It does not own player position, velocity, collision storage, network IDs, or presentation callbacks.
- `MountFatigueAndAbilityStateComponent` owns fatigue and ability timers. Its invariants are non-negative timers, bounded charge, and no fatigue update when the mount is inactive. `MountAbilitySystem` consumes input and produces a state transition or an effect command.
- `DrillMountRuntimeComponent` owns a bounded beam array and drill rotations. `DrillMountSystem` consumes a validated cursor/input command, emits tile-pick or projectile/effect intents, then commits cooldown and target state.
- `MountVariantStateComponent` represents the currently active variant as a discriminated state. `allowedToFly`, `showFlyingFrames`, and extra frame state cannot be read from an untyped object or silently coexist with a different variant.
- Catalog definitions are immutable bootstrap data. They carry no per-player counters and are not persisted as entity state.

### 4.2 Queries and projections

`MountRuntimeIdentityAndFrameProjectionQuery` and `MountRuntimeMobilityAndAbilityProjectionQuery` are pure. They may combine a definition view with authoritative components to return an immutable `MountRuntimeView`, `MountGeometryView`, or `MountMobilityView`; they must not call collision, random, logging, network, sound, dust, or persistence services. Cached projections are deferred until invalidation ownership and consistency tests exist.

### 4.3 Commands and adapters

Proposed commands are `MountEquipCommand`, `MountDismountCommand`, `MountFlightInputCommand`, `MountAbilityInputCommand`, `MountFrameIntent`, `DrillUseCommand`, and `MountEffectIntent`. They express intent and structural changes; they do not expose mutable component references.

`IMountEffectPort` adapts dust, sound, lighting, player-size changes, and projectile/tile effects. `MountNetworkProjection` and `MountPersistenceProjection` are one-way outputs from committed state. They do not write back to mount components.

### 4.4 Explicit scheduler order

The following order is a proposed contract and requires an integration verifier:

1. Apply mount equip/dismount and validate player/collision preconditions.
2. Resolve catalog and variant eligibility.
3. Apply player input to movement/flight/hover intent.
4. Advance ability and fatigue timers.
5. Resolve drill and special vehicle commands.
6. Resolve frame and geometry projection after authoritative state changes.
7. Commit movement/collision effects and player size changes.
8. Publish effect, network, persistence, and presentation projections from committed snapshots.

No source file or directory order may encode this sequence.

## 5. Current integration risks

- `PlayerMountStateComponent` currently stores integer fatigue remaining, while Version4 uses floating `_fatigue` and `_fatigueMax` with mount-specific recovery and hover coupling. This is a behavior risk, not a naming difference.
- Current snapshots expose only a small subset of mount state, and current replication carries mount type through player controls. Full frame, ability, geometry, drill, variant, and vehicle presentation state are not closed.
- Version4 `SetMount` and `Dismount` mutate player dimensions and use collision checks. The final owner of player hitbox and mount geometry is cross-subsystem `integration-review`.
- Drill actions use tile selection, randomness, dust, and pick timing. Tile mutation and projectile/effect publication belong to explicit adapters and cannot be claimed by P03 alone.
- `MountDelegateContract` is intentionally reclassified as an adapter/port. Putting callbacks in a component would leak side effects and third-party types into core state.
- `mounts` is a static mutable array in Version4. The proposed catalog requires bootstrap ownership and immutable reads; the registry replacement and mod/content extension policy are blocking decisions.

## 6. Focused verifier plan and actual status

The focused verifiers below remain plans only; none were run in this session. A serial affected-project build was attempted as a source-level gate, but it did not produce compile evidence because unrelated existing Player project errors stopped compilation.

| Verifier | Scope | Required assertions | Status |
|---|---|---|---|
| `MountCatalogDefinitionVerifier` | catalog and defaults | all 64 type slots, source-backed defaults, no mutable collection leakage, stable IDs | planned; not-run |
| `MountRuntimeTransitionVerifier` | equip/dismount/reset | invalid type, wet restriction, buff lifecycle, geometry fit, reset, duplicate commands | planned; not-run |
| `MountFlightFatigueVerifier` | flight/hover | frame qualification, fractional fatigue behavior, exhaustion, recovery, hover-ignore variants | planned; not-run |
| `MountFrameProjectionVerifier` | pure queries | frame selection, offset bounds, draw layer, no state writes, deterministic repeated queries | planned; not-run |
| `MountVehicleVerifier` | carts/special vehicles | minecart rails, super cart overrides, visibility, ability projection, explicit effect intents | planned; not-run |
| `DrillMountVerifier` | drill runtime | bounded eight-beam state, target reset, cooldown, block/wall purpose, duplicate input, tile/projectile seam | planned; not-run |
| `MountProtocolPersistenceVerifier` | snapshots/adapters | IDs, mount type, runtime fields, restore defaults, versioning, committed-only projection | planned; not-run |
| `MountSystemOrderVerifier` | scheduler | equip -> movement -> ability -> drill -> frame -> collision -> projection ordering | planned; not-run |

The affected-project build command was run through the repository serial wrapper after checking for active `dotnet.exe`/`csc.exe` processes:

```powershell
pwsh -NoProfile -Command "& '.\Build\Tools\Invoke-SerialDotnet.ps1' @('build', '.\src\Player\Terraria.Player.csproj', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')"
```

It exited with code `1`, reporting `0` warnings and `5` existing errors in `PlayerMinionCapacityCommitSystem.cs` and `SubmitMinionCapacityDeltaCommand.cs`. The expected output path is `Build/bin/Terraria.Player/Debug/net10.0/Terraria.Player.dll`; the existing file at that path is not attributable to this failed build. No build, test, behavior-equivalence, network, or persistence claim is made.

## 7. Integration handoff

`subsystemId: MountAndVehicleSimulation`  
`partitionId: P03`  
`evidenceStatus: source-inventory-confirmed; Version4-source-confirmed; current-NLTX-partial`  
`verificationStatus: not-verified`

Cross-subsystem findings requiring integration review:

- Player entity and player hitbox/position owner.
- Spatial movement, collision, tile access, and slope/landing authority.
- Projectile spawn and drill tile mutation commit roots.
- Player equipment/item-use/dismount triggers.
- Network control input, player replication, and persistence snapshot schema.
- External dust, sound, lighting, and delegate callback adapters.

The P03 session does not add members from other partitions and does not assign those owners.

## 8. Incremental component checkpoint

### C01 - `MountFrameAndDrawCatalog`

- `status: proposed-design-recorded`
- Proposed types: `MountFrameAndDrawCatalogDefinition` and `MountFrameAndDrawQuery`.
- Members completed: report rows 269-278 and 323: frame state constants, draw-layer constants, and `idleFrames_Rat`.
- Ownership: immutable definition/catalog. It is not entity state and it has no writer after bootstrap.
- Seam: `MountFrameAndDrawQuery.GetFrameKind`/`GetDrawLayer`/`GetRatIdleFrames`-style read-only view to frame and presentation systems. Exact API remains proposed.
- Evidence: `Mount.cs:273-291`, `Mount.cs:381-385`, and the Player frame call sites at `Player.cs:20519-20536`.
- Evidence gap: exact presentation consumers and whether `idleFrames_Rat` is mutable at runtime are not closed.
- Verification: not-run.

### C02 - `MountSpecialVehicleCatalog`

- `status: proposed-design-recorded`
- Proposed types and paths: `Player/Mount/Definitions/MountSpecialVehicleCatalogDefinition.cs` and `Player/Mount/Queries/MountSpecialVehicleCatalogQuery.cs` (proposed; no files created).
- Members completed: report rows 279-282 and 292: `mounts`, `scutlixEyePositions`, `scutlixTextureSize`, `scutlixBaseDamage`, and `santankTextureSize`.
- Ownership: immutable bootstrap catalog. `mounts` is the source registry root, but its mutable `MountData[]` must not be exposed or duplicated; the target catalog composes the other mount definitions and exposes a read-only special-vehicle view. Eye positions and texture sizes are copied into immutable value data after the source initialization transform. `scutlixBaseDamage` remains a definition value, not per-entity combat state.
- Seam: `MountSpecialVehicleCatalogQuery.GetMountDefinition`, `GetScutlixPresentation`, and `GetSantankPresentation`-style pure reads keyed by a validated mount content ID. Combat, rendering, and vehicle adapters consume the view; none receives a mutable catalog array or writes back to it.
- Evidence: `D:\TRbackup\Version4\Terraria\Mount.cs:293-319`, initialization at `:649-651`, Scutlix eye/texture setup at `:1231-1246`, Santank setup at `:1756-1758`, and Scutlix readers at `:2794-2801` and `:4611-4618`. The report rows are the authoritative member inventory.
- Dependency impact: the catalog bootstrap must run before mount equip, special-vehicle presentation, and Scutlix projectile targeting. `MountRuntimeFrameAndFlightState` and the later catalog groups read the same validated definition ID but do not create a second registry. Projectile, lighting, and rendering ownership remains `integration-review`.
- Migration unit: introduce the immutable registry and special-vehicle view, assert the 64 source slots and transformed eye coordinates, then route one read-only Scutlix/Santank consumer through the query. Do not migrate mount runtime writes or combat effects in this step.
- Invariants and side effects: IDs are stable and in range; every registered slot has one definition; eye-position length and texture-size values match the source; no query allocates or mutates global state; projectile creation, dust, sound, and drawing remain explicit adapters.
- Evidence gap and blocking decision: the complete reader/writer matrix, mod/content registration phase, extension ID policy, and the exact current NLTX special-vehicle consumers are open. Integration review must choose the catalog composition root and whether dynamic content is frozen before simulation starts.
- Verification: not-run. Planned focused coverage is `MountCatalogDefinitionVerifier` for slot count, source values, transformed eye positions, immutable collection leakage, duplicate IDs, invalid IDs, and deterministic repeated reads.

### C03 - `MountDrillConstants`

- `status: proposed-design-recorded`
- Proposed types and paths: `Player/Mount/Definitions/MountDrillConstantsDefinition.cs` and `Player/Mount/Queries/MountDrillRulesQuery.cs` (proposed; no files created).
- Members completed: report rows 283-291: `drillDiodePoint1`, `drillDiodePoint2`, `drillTextureSize`, `drillTextureWidth`, `drillRotationChange`, `drillPickPower`, `drillPickTime`, `amountOfBeamsAtOnce`, and `maxDrillLength`.
- Ownership: immutable definition/query. The two diode points, texture dimensions, rotation step, pick power/time, beam batch limit, and maximum reach are rules/content values. `drillTextureSize` is initialized during the source catalog bootstrap and must be frozen into a value record before simulation; none of these values belongs in `DrillMountRuntimeComponent`.
- Seam: `MountDrillRulesQuery.GetGeometry`, `GetTiming`, and `GetPower`-style pure reads. `DrillMountSystem` consumes validated rules and emits explicit tile/projectile/effect intents; the query does not access `Main.rand`, tile storage, dust, or player state.
- Evidence: `D:\TRbackup\Version4\Terraria\Mount.cs:301-317` declares the members; `:1182` initializes `drillTextureSize`; `UpdateDrill` at `:2650-2672` advances runtime cooldown/rotation; `UseDrill` at `:2675-2764` reads `amountOfBeamsAtOnce`, `drillPickPower`, and `drillPickTime`; `Player.cs:16260` and `:17666` are the caller paths. Current NLTX has no matching drill rule owner in the searched `dome`/`src` C# surfaces.
- Dependency impact: the catalog must be available before drill runtime initialization and before drill geometry/cursor queries. The runtime component owns beam targets/cooldowns; tile selection, tile mutation, projectile creation, randomness, and dust remain explicit integration seams.
- Migration unit: add a frozen rules view, assert source defaults and legal ranges, then route one drill-system read through the query. Do not move `UpdateDrill`, `UseDrill`, tile mutation, or effect creation in this step.
- Invariants and side effects: `amountOfBeamsAtOnce` and `drillPickTime` are positive; `maxDrillLength` and texture dimensions are positive; diode points and rotation step are finite; repeated queries are deterministic; no mutable array or external service is exposed.
- Evidence gap and blocking decision: source call sites do not close every tile-cursor reader or content override path, and current NLTX has no complete drill mapping. Integration review must select the single catalog bootstrap and decide whether mod-provided drill rules are frozen at world/session start.
- Verification: not-run. Planned focused coverage is `MountDrillRulesVerifier` for source defaults, positive/range constraints, finite geometry, immutable values, invalid content IDs, and deterministic repeated reads.

### C04 - `MountSuperCartConstants`

- `status: proposed-design-recorded`
- Proposed types and paths: `Player/Mount/Definitions/MountSuperCartDefinition.cs` and `Player/Mount/Queries/MountSuperCartQuery.cs` (proposed; no files created).
- Members completed: report rows 317-321: `SuperCartRunSpeed`, `SuperCartDashSpeed`, `SuperCartAcceleration`, `SuperCartJumpHeight`, and `SuperCartJumpSpeed`.
- Ownership: immutable definition/query. These values are Super Cart rule overrides, not entity state. The dynamic `_shouldSuperCart` flag and the player `UsingSuperCart` qualification remain runtime/input concerns and must not be copied into the definition.
- Seam: `MountSuperCartQuery.GetMobilityOverride` and `GetJumpOverride`-style pure views, selected only after the runtime projection confirms cart and Super Cart eligibility. The query does not mutate velocity, jump state, collision, or player input.
- Evidence: `D:\TRbackup\Version4\Terraria\Mount.cs:369-377` declares the values; `:472-498` reads the run/dash/acceleration overrides; `:2421-2449` reads jump height/speed; `_shouldSuperCart` is reset at `:642` and set from cart/player qualification at `:4102-4105`; `Player.cs:3058-3070` defines `UsingSuperCart`. Current NLTX has `PlayerMountState.UsesSuperCartRules` at `src/Player/PlayerMountState.cs:29`, but no complete override projection or owner.
- Dependency impact: the definition is consumed by mobility and jump projections after cart qualification and before movement commit. It must not compete with `MountMovementAndAbilityCatalog` for ordinary mount values or create a second `UsingSuperCart` authority.
- Migration unit: freeze the five source values, add a pure override query, and route one projection read through it while preserving the existing eligibility owner. Do not migrate velocity integration or jump/collision writes.
- Invariants and side effects: override values are finite and positive where applicable; eligibility is evaluated separately; repeated queries are deterministic; the definition has no player/network/persistence writes.
- Evidence gap and blocking decision: the exact source writer for `UsingSuperCart`, all consumers of the projected speeds, and current NLTX movement integration are not closed. Integration review must select the single dynamic enablement and movement commit owner.
- Verification: not-run. Planned focused coverage is `MountSuperCartVerifier` for source values, qualification gating, fallback to ordinary mount values, finite ranges, and query non-mutation.

### C05 - `MountRuntimeFrameAndFlightState`

- `status: implemented-component-source; verificationStatus: not-verified`
- Component source saved at `src/Player/Mount/Components/MountRuntimeFrameAndFlightStateComponent.cs`. `MountRuntimeSystem` was not created because systems are outside this task.
- Members completed: report rows 293-303, 312-313, 315, and 316: `_data`, `_type`, `_flipDraw`, `_frame`, `_frameCounter`, `_frameExtra`, `_frameExtraCounter`, `_frameState`, `_flyTime`, `_idleTime`, `_idleTimeNext`, `_shouldSuperCart`, `_walkingGraceTimeLeft`, `_mountSpecificData`, and `_active`.
- Ownership: the component is the single authoritative owner for active/type/frame/flight lifecycle state and frame counters. `_data` is represented as a validated immutable definition/content ID or handle, never as a mutable `MountData` reference. `_mountSpecificData` is accounted for as a legacy heterogeneous carrier: the target has a discriminated variant handle, while drill payload and selective/extra-frame payload are owned by `DrillMountRuntime` and `MountVariantFlags`; no `object` field or parallel payload owner is introduced.
- Seam: `MountRuntimeSystem` consumes equip/dismount/input/velocity/definition qualification and commits a new component value through one owner. Queries read an immutable runtime view. Equip, dismount, player-size, buff, collision, lighting, sound, dust, and network/persistence effects leave through explicit commands or commit/projection ports.
- Evidence: `D:\TRbackup\Version4\Terraria\Mount.cs:321-367` declares the fields; `Reset` resets the core lifecycle at `:622-643`; `UpdateFrame` mutates frame, idle, grace, and orientation state at `:3089-4016`; `Flight` decrements `_flyTime` at `:2583-2593`; `ResetFlightTime` writes it at `:4497-4516`; `UpdateEffects` sets `_shouldSuperCart` from cart/player qualification at `:4097-4108`; `SetMount` initializes type/data/active/variant state at `:4782-4886`; `Dismount` clears active/variant state and calls reset at `:4728-4780`; Player callers include `:16355`, `:16536-16540`, and `:20520-20536`.
- Dependency impact: catalog definitions must be resolved before equip and frame/flight systems; fatigue/ability state is updated by a later system and must not be folded into this component. Current NLTX `PlayerMountStateComponent` already owns a partial type/flight/frame surface, so migration must adapt or replace that owner rather than add a second authoritative component. Player position, collision size, buffs, and effects remain `integration-review` seams.
- Migration unit: introduce a typed runtime state and a single transition system, map equip/dismount/reset and frame/flight reads through it, and keep legacy adapters as read-only compatibility boundaries until focused verifiers prove one writer. Do not migrate player geometry, lighting, dust, sound, or network fields in this step.
- Invariants and side effects: inactive state has no readable definition, type is invalid/unset, and no flight decrement occurs; active state has one valid definition ID; frame and extra-frame counters are finite; frame state is within the catalog's frame-state domain; flight and grace timers are non-negative; Super Cart enablement is derived from an explicit qualification input; runtime updates are deterministic for a supplied input snapshot and do not call external effect services.
- Evidence gap and blocking decision: the source `Reset` leaves some unrelated legacy fields outside this component's reset set, the complete reader/writer matrix for `_mountSpecificData` is heterogeneous, and current NLTX has an overlapping partial owner. Integration review must approve the single owner, reset semantics, variant payload handoff, and definition-ID serialization before implementation.
- Verification: not-verified. The affected Player project build failed before a successful artifact; planned focused coverage remains unrun.

### C06 - `MountFatigueAndAbilityState`

- `status: implemented-component-source; verificationStatus: not-verified`
- Component source saved at `src/Player/Mount/Components/MountFatigueAndAbilityStateComponent.cs`. `MountAbilitySystem` was not created because systems are outside this task.
- Members completed: report rows 304-311: `_fatigue`, `_fatigueMax`, `_abilityCharging`, `_abilityCharge`, `_abilityCooldown`, `_abilityDuration`, `_abilityActive`, and `_aiming`.
- Ownership: authoritative per-player resource and ability-transition state. Preserve `_fatigue` as a floating-point accumulated value with `_fatigueMax`; do not translate it to the current integer `FatigueRemaining` without a behavior decision. Ability charge/cooldown/duration are bounded timers/values, while active/charging/aiming are explicit state bits.
- Seam: `MountAbilitySystem` consumes a typed input snapshot, mount definition, frame/flight state, and validated effect/projectile ports, then commits only this component. `StartAbilityCharge`, `StopAbilityCharge`, `AbilityRecovery`, `FatigueRecovery`, `AimAbility`, and `ResetHeadPosition` become explicit transitions; projectile creation, sound, dust, and player-facing aim/frame effects are emitted as intents.
- Evidence: `D:\TRbackup\Version4\Terraria\Mount.cs:343-357` declares the fields; ability charge/start/stop/recovery at `:2503-2567`; fatigue recovery at `:2569-2581`; hover consumes flight and increases fatigue at `:2844-2898`; `UseAbility` sets drill active at `:2838-2840`; `UpdateEffects` clears active when the owned projectile is absent at `:4305-4310`; `AimAbility`/reset writes aiming at `:4522-4545`; Player invokes recovery at `:15102` and `:16239`. Current NLTX has floating fields in `src/Player/PlayerMountState.cs` and `PlayerMountComponent.cs`, but the active ECS owner uses integer `FatigueRemaining` and snapshots expose that integer.
- Dependency impact: movement/hover and frame projections read fatigue/ability state; drill runtime consumes the ability-active qualification; projectile and input adapters provide explicit commands. The component must not own projectile IDs, player mouse state, random sources, or external effects.
- Migration unit: first write a semantic comparison verifier for floating accumulated fatigue versus integer remaining fatigue; then introduce the typed state and route one recovery/ability transition through it. Do not change flight input consumption or snapshot schema until the comparison and single-owner decision are accepted.
- Invariants and side effects: `0 <= fatigue <= fatigueMax` when `fatigueMax > 0`; zero-max mounts never divide by zero; charge is within the definition maximum; cooldown/duration never become negative; active/charging transitions are qualified by mount type and cooldown; aiming resets deterministically; no transition directly spawns a projectile or mutates world/player effects.
- Evidence gap and blocking decision: complete `UseAbility` input ownership and ability-to-projectile lifecycle are not closed, and the current NLTX polarity/precision mismatch is a behavior blocker. Integration review must decide the canonical resource semantics before implementation or persistence migration.
- Verification: not-verified. The affected Player project build failed before a successful artifact; planned focused coverage remains unrun.

### C07 - `MountRuntimeIdentityAndFrameProjection`

- `status: proposed-design-recorded`
- Proposed type and path: `Player/Mount/Queries/MountRuntimeIdentityAndFrameProjectionQuery.cs` (proposed; no file created).
- Members completed: report rows 314, 322, and 324-334: `_debugDraw`, `_defaultDelegatesData`, `Active`, `Type`, `Frame`, `FlyTime`, `BodyFrame`, `RunningGraceTime`, `PlayerXOFfset`, `PlayerOffset`, `PlayerOffsetHitbox`, `PlayerHeadOffset`, and `HeightBoost`.
- Ownership: pure projection over `MountRuntimeFrameAndFlightStateComponent` plus immutable geometry/frame definitions. `Active`, type, frame, flight, and grace values come from the authoritative runtime snapshot; body/offset constants come from the definition; offset-hitbox calculations are bounded derived values. `_debugDraw` is a transient diagnostic sink and `_defaultDelegatesData` is a default adapter contract, so neither is stored or mutated by the query.
- Seam: `MountRuntimeIdentityAndFrameProjectionQuery.GetView(runtime, definition)` returns an immutable view. Inactive or invalid-definition behavior is explicit and matches the legacy zero/default contract. The legacy spelling `PlayerXOFfset` is preserved only at a compatibility adapter boundary; internal code uses a correctly named projection field.
- Evidence: `D:\TRbackup\Version4\Terraria\Mount.cs:363-379` declares the two legacy fields; property implementations are at `:387-441`; `Reset` initializes diagnostics at `:616-620`; frame/flight writers are in `UpdateFrame` and `Flight` at `:2583-2593` and `:3089-4016`. Current NLTX has only partial height-boost capability data (`MountCapabilityRegistry`) and no complete runtime geometry projection or mount snapshot.
- Dependency impact: geometry/collision and presentation systems consume the view after runtime state commit; mobility/ability projection may consume identity/type but must not write back. Network/persistence projections consume committed values only. Collision and player-size ownership remains `integration-review`.
- Migration unit: implement a pure view over existing typed state/definitions, add inactive/out-of-range/default tests, and route one read path through it. Keep diagnostics and delegate defaults on explicit adapter ports; do not migrate collision or presentation effects here.
- Invariants and side effects: repeated calls with identical inputs produce identical values; no state, catalog, list, delegate, collision, lighting, logging, or network mutation occurs; frame-index access never escapes `playerYOffsets` bounds; inactive offsets are zero; hitbox offset uses the same height-boost formula.
- Evidence gap and blocking decision: exact current NLTX geometry readers, collision-size commit order, `_debugDraw` lifetime, and default delegate exposure are not closed. Integration review must decide the compatibility adapter and the sole player hitbox/position owner before routing production callers.
- Verification: not-run. Planned focused coverage is `MountFrameProjectionVerifier` for identity values, inactive/default behavior, offset bounds, legacy spelling adapter, deterministic repeated reads, and reflection/side-effect checks showing no writes.

### C08 - `MountRuntimeMobilityAndAbilityProjection`

- `status: proposed-design-recorded`
- Proposed type and path: `Player/Mount/Queries/MountRuntimeMobilityAndAbilityProjectionQuery.cs` (proposed; no file created).
- Members completed: report rows 335-350: `RunSpeed`, `DashSpeed`, `Acceleration`, `AutoJump`, `BlockExtraJumps`, `IsConsideredASlimeMount`, `Cart`, `CanGrindRails`, `AnyTrackRider`, `CanUseWings`, `Delegations`, `AbilityCharging`, `AbilityActive`, `AbilityCharge`, `AllowDirectionChange`, and `DismountOnItemUse`.
- Ownership: pure mobility/qualification projection over the runtime frame/flight and fatigue/ability components plus immutable catalog definitions. It does not store speed, cart, ability, or delegate state and does not compete with the runtime components.
- Seam: `MountRuntimeMobilityAndAbilityProjectionQuery.GetView(runtime, resources, definition, superCartQualification)` returns a read-only view. `Delegations` is represented as a non-executable descriptor/adapter key; callback execution belongs to `IMountEffectPort` in C15. Movement, jump, rail, item-use, and dismount systems consume the view and submit commands rather than mutate through it.
- Evidence: `D:\TRbackup\Version4\Terraria\Mount.cs:443-613` contains all property implementations. Run-speed special cases include swim, type-specific bonuses, fatigue ratio, and Super Cart at `:447-476`; cart/rail/wing and default delegate behavior at `:520-581`; ability state/charge and direction/dismount qualification at `:584-612`. Current NLTX `PlayerControlSystem.cs:38-80` reads a narrower `MountCapabilityRegistry`; `PlayerMountStateComponent` exposes only a subset and currently performs flight/fatigue mutation.
- Dependency impact: this query depends on the C02-C06 catalogs/components and must be evaluated after authoritative state transitions but before movement/collision commit. It is consumed by Player movement, rail, ability, and item-use boundaries. It must not call collision, random, logging, network, persistence, sound, dust, projectile, or tile services.
- Migration unit: implement the pure view and source-backed formula tests, then route one movement read through it while retaining a compatibility adapter for the existing registry. Defer `Delegations` execution and collision/movement writes.
- Invariants and side effects: inactive cart/rail values remain false; inactive wing behavior preserves the legacy default only through an explicit compatibility rule; speed and acceleration are finite; fatigue ratio handles zero maximum explicitly; ability charge handles zero maximum without division by zero; query results are deterministic and have no writeback.
- Evidence gap and blocking decision: all formula callers, zero-max definitions, current registry equivalence, and dynamic Super Cart/fatigue writer ordering are not closed. Integration review must choose one mobility projection and decide whether current capability tables are replaced, adapted, or rejected per value.
- Verification: not-run. Planned focused coverage is `MountMobilityProjectionVerifier` for every special speed case, Super Cart override, fatigue ratio, inactive defaults, cart/rail/wing qualification, delegate descriptor non-execution, ability charge/cooldown, dismount qualification, and pure-query behavior.

### C09 - `MountGeometryAndOffsetCatalog`

- `status: proposed-design-recorded`
- Proposed types and paths: `Player/Mount/Definitions/MountGeometryAndOffsetCatalogDefinition.cs` and `Player/Mount/Queries/MountGeometryAndOffsetQuery.cs` (proposed; no files created).
- Members completed: report rows 210-217 and 268: `textureWidth`, `textureHeight`, `xOffset`, `yOffset`, `playerYOffsets`, `bodyFrame`, `playerHeadOffset`, `heightBoost`, and `playerXOffset`.
- Ownership: immutable per-mount geometry/content definition. `playerYOffsets` is copied into an immutable or defensively exposed sequence; no caller receives the source mutable array. `heightBoost` and offsets describe geometry but do not themselves mutate the player hitbox, position, shadows, or collision state.
- Seam: `MountGeometryAndOffsetQuery.GetGeometry(mountContentId)` returns a read-only geometry view consumed by frame, drawing, and collision adapters. Runtime `PlayerOffset` and `PlayerOffsetHitbox` remain pure projections over this view and committed runtime frame state; the collision adapter owns any fit/resize command.
- Evidence: `D:\TRbackup\Version4\Terraria\Mount.cs:147-161` declares the core geometry fields and `:263` declares `playerXOffset`; initialization assignments span `:655-2370`, including `SetAsRollerSkate`, `SetAsHorse`, `SetAsChillet`, and `SetAsMinecart` at `:2197-2369`; `GetHeightBoost` reads the catalog at `:2389-2398`; runtime offset properties read these values at `:401-441`; player/collision consumers include `Player.cs:2513`, `:3290`, `:3826`, `:13721`, `Main.cs:11789`, and `Mount.cs:4762-4777`, `:4829-4862`, `:4935-4977`. Current NLTX exposes only partial height-boost lookup in `MountCapabilityRegistry`.
- Dependency impact: geometry bootstrap precedes frame projection and player-size/collision integration. Ground/aerial/dash frame definitions consume the same mount content ID but do not duplicate geometry arrays. Network/persistence may serialize a content ID and committed geometry projection only after versioning is approved.
- Migration unit: freeze source geometry values, validate offset-array lengths against frame counts, add immutable-array leakage and bounds tests, and route one read-only geometry consumer through the query. Do not move player resize, collision fit, or shadow-position writes in this step.
- Invariants and side effects: texture dimensions and offset arrays are content-valid; every frame index used by `PlayerOffset` is bounded; inactive projection returns the documented default; source geometry arrays cannot be mutated through a query; query evaluation is deterministic and has no collision, player, lighting, or network side effects.
- Evidence gap and blocking decision: all mount-specific geometry overrides and complete current NLTX consumers are not closed, and source `SetMount`/`Dismount` interleave geometry with collision and shadow writes. Integration review must choose one geometry catalog and one player-size/position commit root before implementation.
- Verification: not-run. Planned focused coverage is `MountGeometryCatalogVerifier` for all source slots, dimensions, offsets, array immutability, frame bounds, default/inactive behavior, `GetHeightBoost`, and collision seam non-mutation.

### C10 - `MountGroundAnimationFrames`

- `status: proposed-design-recorded`
- Proposed types and paths: `Player/Mount/Definitions/MountGroundAnimationFramesDefinition.cs` and `Player/Mount/Queries/MountGroundAnimationFramesQuery.cs` (proposed; no files created).
- Members completed: report rows 239-245 and 252-255: `totalFrames`, `standingFrameStart`, `standingFrameCount`, `standingFrameDelay`, `runningFrameStart`, `runningFrameCount`, `runningFrameDelay`, `idleFrameStart`, `idleFrameCount`, `idleFrameDelay`, and `idleFrameLoop`.
- Ownership: immutable per-mount frame-sequence definition. The catalog describes ranges, delays, and loop policy; the runtime frame component owns current frame/counters. Idle trigger time and randomized `idleTimeNext` are runtime/system concerns and require an explicit clock/random port.
- Seam: `MountGroundAnimationFramesQuery.GetSequence(mountContentId, frameState)` returns a bounded read-only sequence for standing, running, or idle selection. `MountRuntimeSystem` consumes it with supplied velocity/time/random decisions and commits frame state; the query does not advance counters or choose random values.
- Evidence: `D:\TRbackup\Version4\Terraria\Mount.cs:205-237` declares the frame fields; initialization spans `:662-2376` and includes `SetAs*` assignments; `UpdateFrame` consumes idle values at `:3656-3749`, standing at `:3757-3764`, and running at `:3796-3818`; Player routes states at `Player.cs:20520-20536`. Current NLTX has no complete ground animation catalog or frame system.
- Dependency impact: geometry and frame/draw constants are prerequisites; aerial/water and dash definitions remain separate later groups. The runtime component stores the selected frame and counters, not catalog data. Presentation reads committed frame state after system update.
- Migration unit: freeze source sequences, validate non-negative ranges/delays and total-frame bounds, add deterministic sequence tests with injected idle decisions, and route one ground-frame read through the query. Do not migrate random timing, drawing, or effects in this step.
- Invariants and side effects: standing/running/idle ranges stay within `totalFrames`; zero-count idle is handled explicitly; delays are non-negative and zero-delay behavior is defined; loop policy is immutable; query evaluation is deterministic and side-effect-free for fixed inputs.
- Evidence gap and blocking decision: exact current NLTX frame consumers, source random distribution, and all mount-specific frame overrides are not closed. Integration review must choose clock/random ownership and the frame-system writer before implementation.
- Verification: not-run. Planned focused coverage is `MountGroundFrameVerifier` for all source definitions, range/delay bounds, idle loop/non-loop behavior, zero-count idle, injected random timing, frame reset on state change, and query non-mutation.

### C11 - `MountAerialAndWaterAnimationFrames`

- `status: proposed-design-recorded`
- Proposed types and paths: `Player/Mount/Definitions/MountAerialAndWaterAnimationFramesDefinition.cs` and `Player/Mount/Queries/MountAerialAndWaterAnimationFramesQuery.cs` (proposed; no files created).
- Members completed: report rows 246-251 and 256-258: `flyingFrameStart`, `flyingFrameCount`, `flyingFrameDelay`, `inAirFrameStart`, `inAirFrameCount`, `inAirFrameDelay`, `swimFrameStart`, `swimFrameCount`, and `swimFrameDelay`.
- Ownership: immutable aerial/water frame ranges and delays. The runtime frame system owns current frame/counters; velocity, flight-time/fatigue, wing/grapple state, liquid state, and mount-specific special cases are explicit inputs/rules outside the definition.
- Seam: `MountAerialAndWaterAnimationFramesQuery.GetSequence(mountContentId, frameState)` returns bounded data for flying, in-air, or swimming. `MountRuntimeSystem` applies the source-backed type-specific rules with supplied input and commits frame state; the query does not read Player, Collision, wings, grapple, randomness, or effects.
- Evidence: `D:\TRbackup\Version4\Terraria\Mount.cs:219-243` declares the members; initialization assignments span `:681-1801` and later `SetAs*` helpers; `UpdateFrame` reads flying at `:3823-3845`, in-air and type overrides at `:3847-3959`, and swim at `:3961-3978`; `UpdateFrame_Velociraptor` at `:4018-4037` rewrites state based on selective-flying data. Current NLTX has only a narrow frame eligibility table in `MountCapabilityRegistry`.
- Dependency impact: geometry and frame constants are prerequisites; ground and dash frame groups remain separate. The runtime component and C06 resource state provide current counters and fatigue inputs. Presentation consumes the committed frame view after update.
- Migration unit: freeze all three source sequences, validate ranges and zero-count semantics, add injected-input tests for flying/in-air/swim selection, and route one aerial/water frame read through the query. Do not migrate wing/grapple state, physics, or effects in this step.
- Invariants and side effects: frame ranges stay within total geometry frame bounds; zero-count sequences have explicit fallback behavior; delays are non-negative and zero-delay behavior is defined; query results are deterministic for fixed inputs and do not mutate state or external services.
- Evidence gap and blocking decision: exact liquid/wing/grapple readers, all type-specific frame rewrites, and current NLTX aerial consumers are not closed. Integration review must approve the runtime frame writer and the external input/override ports before implementation.
- Verification: not-run. Planned focused coverage is `MountAerialFrameVerifier` for range/delay bounds, zero-count fallback, flying/in-air/swim transitions, type-specific overrides, fatigue/flight inputs, selective-flying qualification, and query non-mutation.

### C12 - `MountDashAnimationFrames`

- `status: proposed-design-recorded`
- Proposed types and paths: `Player/Mount/Definitions/MountDashAnimationFramesDefinition.cs` and `Player/Mount/Queries/MountDashAnimationFramesQuery.cs` (proposed; no files created).
- Members completed: report rows 259-261: `dashingFrameStart`, `dashingFrameCount`, and `dashingFrameDelay`.
- Ownership: immutable dash frame range/delay definition. The runtime frame component owns current dash frame/counter; direction, `_flipDraw`, velocity, Player dash/dashDelay, and special mount overrides are external inputs and runtime rules.
- Seam: `MountDashAnimationFramesQuery.GetSequence(mountContentId)` returns a bounded dash sequence. `MountRuntimeSystem` applies forward/reverse stepping and commits state; the query does not read or mutate Player dash fields.
- Evidence: `D:\TRbackup\Version4\Terraria\Mount.cs:245-249` declares the members; initialization includes `:1284-1286`, `:1793-1795`, `:2057-2059`, `:2104-2106`, `:2158-2160`, `:2265-2267`, and `:2324-2326`; `UpdateFrame` reads them with signed movement at `:3980-4009` and applies type 62/63 dash-window frame override at `:4012-4015`; Player dash state is maintained outside Mount, including `Player.cs:12564-12999`.
- Dependency impact: geometry and runtime frame state are prerequisites; movement/ability catalog supplies dash eligibility and speed but does not duplicate the frame sequence. Presentation consumes committed frame state after the dash system.
- Migration unit: freeze dash definitions, validate range/delay values, add forward/reverse and zero-count tests with injected velocity/dash state, and route one dash-frame read through the query. Do not migrate Player dash state, velocity writes, or effects.
- Invariants and side effects: dash frame ranges are bounded; non-negative/zero delays have defined behavior; forward and reverse transitions are deterministic; query has no access to Player, physics, random, collision, or effects.
- Evidence gap and blocking decision: the complete dash writer, all mount-specific overrides, and current NLTX dash/frame integration are not closed. Integration review must choose the dash system owner and direction/flip compatibility policy before implementation.
- Verification: not-run. Planned focused coverage is `MountDashFrameVerifier` for all source sequences, forward/reverse stepping, zero-count/zero-delay behavior, `_flipDraw` interaction, Player dash-window overrides, frame reset, and query non-mutation.

### C13 - `MountMovementAndAbilityCatalog`

- `status: proposed-design-recorded`
- Proposed types and paths: `Player/Mount/Definitions/MountMovementAndAbilityCatalogDefinition.cs` and `Player/Mount/Queries/MountMovementAndAbilityCatalogQuery.cs` (proposed; no files created).
- Members completed: report rows 219-236: `flightTimeMax`, `usesHover`, `runSpeed`, `dashSpeed`, `swimSpeed`, `acceleration`, `jumpSpeed`, `jumpHeight`, `fallDamage`, `extraFall`, `fatigueMax`, `constantJump`, `blockExtraJumps`, `abilityChargeMax`, `abilityDuration`, `abilityCooldown`, `walkingGraceTimeMax`, and `dismountsOnItemUse`.
- Ownership: immutable per-mount movement/resource/ability definition. Maxima and defaults seed or bound C05/C06 runtime state; speeds, jump, fall, hover, constant-jump, extra-jump, grace, and dismount rules are read by projections/systems. No runtime counter, fatigue accumulator, velocity, jump result, or ability timer is stored here.
- Seam: `MountMovementAndAbilityCatalogQuery.GetDefinition(mountContentId)` returns a read-only rule view. C08 applies state- and input-dependent formulas, C05/C06 own committed runtime values, and movement/fall/input systems submit intents or commits through their own boundaries.
- Evidence: `D:\TRbackup\Version4\Terraria\Mount.cs:165-199` declares the fields; source initialization spans `:656-1766` and `SetAs*` helpers; `JumpHeight`/`JumpSpeed` apply velocity/state and Super Cart overrides at `:2400-2452`; `RunSpeed` and ability/dismount projections read related values at `:443-613`; ability timers and fatigue recovery use definitions at `:2531-2581`; current NLTX `MountCapabilityRegistry` exposes only a partial table at `dome/src/Terraria.Dome.Simulation/Player/Definitions/MountCapabilityRegistry.cs:72-386`, while snapshots carry only flight/fatigue remaining values.
- Dependency impact: this catalog is a prerequisite for runtime initialization, mobility projection, flight/fatigue and ability systems, frame grace rules, and item-use dismount qualification. It must be the single source for these definition values and must not duplicate C02-C04 special catalogs or C08 derived formulas.
- Migration unit: freeze all 64 source definition slots, validate finite/non-negative values and explicit zero semantics, compare each current registry value against the source table, and route one projection read through the query. Do not change runtime resource polarity, movement integration, fall damage, or snapshot schema in this step.
- Invariants and side effects: definitions are immutable after bootstrap; timer/resource maxima are non-negative; speed/acceleration values are finite; zero flight/fatigue/ability maxima are handled without division by zero; query reads do not mutate runtime state, input, physics, player, collision, network, persistence, or effects.
- Evidence gap and blocking decision: complete source readers for fall damage/extra fall, grace, and item-use dismount are not closed, and current NLTX capability tables intentionally cover only a subset. Integration review must approve value-by-value replacement/adaptation and the canonical resource polarity before implementation.
- Verification: not-run. Planned focused coverage is `MountMovementCatalogVerifier` for all source slots, value/default equivalence, zero-max behavior, finite ranges, Super Cart fallback separation, resource seeding, ability timer bounds, dismount/grace rules, immutable reads, and no query writeback.

### C14 - `MountVehicleAndPresentationCatalog`

- `status: proposed-design-recorded`
- Proposed types and paths: `Player/Mount/Definitions/MountVehicleAndPresentationCatalogDefinition.cs` and `Player/Mount/Queries/MountVehicleAndPresentationCatalogQuery.cs` (proposed; no files created).
- Members completed: report rows 218, 237-238, and 262-267: `buff`, `spawnDust`, `spawnDustNoGravity`, `Minecart`, `CanRideMinecartTracks`, `CanUseWings`, `lightColor`, `emitsLight`, and `delegations`.
- Ownership: immutable per-mount vehicle/presentation descriptor. Buff IDs, dust IDs/flags, cart/rail/wing capability, light color/emission, and delegate keys are definitions; they do not add buffs, spawn dust, emit light, invoke sounds, resize players, or mutate rail state.
- Seam: `MountVehicleAndPresentationCatalogQuery.GetDescriptor(mountContentId)` returns value data and non-executable delegate identifiers. Vehicle, buff, lighting, sound, dust, and drawing adapters consume the descriptor and emit explicit effect commands through C15; no mutable `MountDelegatesData` instance crosses the query boundary.
- Evidence: `D:\TRbackup\Version4\Terraria\Mount.cs:163`, `:201-203`, and `:251-261` declare the fields; initialization assigns values and delegate targets throughout `:653-2348`; buff consumption occurs at `:4516`, `:4738`, and `:4792-4811`; lighting reads at `:3142-3145`; Player/Minecart consumers include `Player.cs:11601-11683`, `:12659-12771`, `:17493-17531`, `:17955`, and mouth/hand reads at `:3241-3258`. Current NLTX has only partial cart/wing capability sets and no full presentation descriptor.
- Dependency impact: this catalog depends on immutable mount IDs and is consumed after committed runtime state by vehicle/presentation/effect adapters. C15 owns callback/effect translation; C09 geometry and C08 qualification views provide supporting data. Buff persistence, player size, rail movement, lighting, sound, dust, and network ownership remain integration seams.
- Migration unit: freeze descriptor values and delegate keys, validate source IDs/colors/defaults, route one read-only vehicle/presentation consumer through the query, and emit no direct effects. Do not move buff application, light emission, dust creation, sound, or collision/rail writes in this step.
- Invariants and side effects: descriptor reads are immutable and deterministic; invalid/inactive mounts have explicit defaults; light color is finite; dust/buff IDs are validated against their external registries; delegate identifiers are data-only; no query call reaches effect services.
- Evidence gap and blocking decision: complete current NLTX vehicle/presentation readers and the external ID registry/versioning policy are not closed. Integration review must choose adapter owners and effect ordering before descriptor consumers replace legacy callbacks.
- Verification: not-run. Planned focused coverage is `MountVehiclePresentationVerifier` for descriptor defaults, cart/rail/wing flags, buff/dust IDs, light values, delegate-key non-execution, inactive behavior, immutable reads, and effect-command boundary checks.

### C15 - `MountDelegateContract`

- `status: proposed-design-recorded`
- Proposed types and paths: `Player/Mount/Adapters/IMountEffectPort.cs` and `Player/Mount/Adapters/MountDelegateAdapter.cs` (proposed; no files created). A separate pose/resize query or command port may be split only if implementation scale requires it; the public contract remains explicit and typed.
- Members completed: report rows 202-209: `MinecartDust`, `MinecartJumpingSound`, `MinecartLandingSound`, `MinecartBumperSound`, `MouthPosition`, `HandPosition`, `PlayerSize`, and `DashDust`.
- Ownership: external adapter/port, not a component. Core mount state stores only an immutable delegate key/descriptor from C14. `MouthPosition` and `HandPosition` map to pure pose queries; `PlayerSize` maps to a validated resize result/command; minecart sound/dust and dash dust map to effect intents. No `Action<>`, `Dust`, `Player`, or delegate object is stored in authoritative ECS state.
- Seam: the owner system emits typed intents containing entity/player identity, committed mount content ID, tick/transition ID, position/size inputs, and effect key. `IMountEffectPort` translates them to audio, dust, lighting, resize, and pose adapters. Effect adapters execute after state/collision commit; they do not write mount components. Duplicate execution is controlled by a committed transition/effect ID, and failure/retry policy is an integration decision rather than hidden callback behavior.
- Evidence: `D:\TRbackup\Version4\Terraria\Mount.cs:96-126` declares delegate types, fields, and default bindings; specialized bindings are assigned at `:830-831`, `:919-921`, `:988-999`, `:1303-1304`, `:1392-1393`, `:1928-1930`, `:1983-1985`, `:2026-2029`, `:2072-2076`, `:2125-2129`, and `:2347-2348`; position/size readers are at `Player.cs:3235-3258` and `:17955`; minecart consumers are at `Player.cs:11601-11683`, `:17493-17531`; dash dust is invoked at `Player.cs:12659-12771`; source adapter behavior is in `DelegateMethods.cs:149-229` and `Minecart.cs:566-820`, `:1246-1269`.
- Dependency impact: C14 supplies data-only delegate keys; C05/C08 supply runtime qualification; collision/player systems own resize and rail commits; lighting, sound, dust, and projectile/tile adapters own external effects. The adapter must not become a second source of mount state or bypass the scheduler order.
- Migration unit: define value contracts and key-to-adapter registration, add one effect-intent path with a no-op verifier, and route one read-only pose/size consumer through a typed port. Do not migrate direct delegate invocation, random dust, audio, player resize, or rail mutation until the integration owner is approved.
- Invariants and side effects: ports receive immutable context values; pose queries are deterministic for fixed inputs; size results are validated before resize commands; effect intents are ordered after committed state; adapters are the only side-effect boundary; retries are explicit and idempotency is observable; a missing key fails closed with a diagnostic result rather than arbitrary callback invocation.
- Evidence gap and blocking decision: the exact effect registry, all current NLTX adapters, network-facing effect policy, and player-size commit owner are not closed. Integration review must decide whether effect failures are retried, dropped, or surfaced, and how client-only visual effects are separated from authoritative commands.
- Verification: not-run. Planned focused coverage is `MountDelegateAdapterVerifier` for key registration, pose/size result determinism, no third-party types in core contracts, effect ordering, duplicate suppression, missing-key behavior, failure/retry observability, and adapter non-writeback.

### C16 - `DrillMountRuntime`

- `status: implemented-component-source; verificationStatus: not-verified`
- Component source saved at `src/Player/Mount/Components/DrillMountRuntimeComponent.cs`; it contains a fixed eight-slot beam state with an externally read-only view, optional component-local `DrillTileTarget` values, and typed `Unknown`/block/wall purpose. `DrillMountSystem` was not created because systems are outside this task.
- Members completed: report rows 188-196: `curTileTarget`, `cooldown`, `lastPurpose`, `diodeRotationTarget`, `diodeRotation`, `outerRingRotation`, `beams`, `beamCooldown`, and `crosshairPosition`. The nested `beams` value has exactly eight slots, each carrying the three beam values from rows 188-190.
- Ownership: authoritative short-lived per-player drill state. Each beam is a fixed slot with an optional tile target, a non-negative cooldown, and a typed purpose (`Block` or `Wall`, preserving an explicit unknown value for invalid legacy data). The source sentinel `Point16.NegativeOne` is translated at the adapter boundary; it is not a valid target in the component. Rotations are finite angles, `crosshairPosition` is a value copied from an explicit aim command, and `beamCooldown` is a separate bounded global gate rather than an alias for a beam cooldown.
- Lifecycle: mounting type 8 creates eight idle beams with no target and zero per-beam cooldown. `UpdateDrill` decrements each cooldown, clears the target when a cooldown reaches zero, eases `diodeRotation` toward `diodeRotationTarget` with the source 0.85/0.15 blend, and decrements the global gate. Aim input updates the normalized diode target and crosshair value using the shortest angular path. Frame/mobility input advances `outerRingRotation` from horizontal velocity and wraps it to the source `[-pi, pi]` interval. Dismount/reset removes the component or returns it to the idle value; stale targets must never survive an equip transition.
- Seam: `DrillMountSystem` consumes `DrillAimCommand`, `DrillUseCommand`, actor identity, committed ability qualification, cursor input, velocity, and validated C03 drill rules. It asks an explicit `IDrillTargetQuery` for a block or wall target, reserves the first eligible bounded slot, and emits a typed `DrillTilePickIntent` or equivalent commit intent containing mount/player identity, target, purpose, pick power/time, and a transition/effect ID. `IMountEffectPort` receives dust/visual intents. Tile mutation, projectile allocation, randomness, logging, and network publication remain outside the component and system core.
- Evidence: `D:\TRbackup\Version4\Terraria\Mount.cs:20-58` declares `DrillBeam` and `DrillMountData`, including the eight-slot initialization and idle sentinel; `:2650-2673` performs cooldown/target cleanup, diode easing, and global cooldown decrement; `:2675-2766` gates use on type 8 and ability activity, selects block/wall targets, writes target/purpose/cooldown, and creates random dust; `:3033-3044` advances and wraps the outer ring from velocity; `:4674-4702` updates diode aim and crosshair; `:4873-4901` creates/reset-initializes drill state; `Player.cs:16258-16260`, `:17664-17666`, and `:23530-23533` establish movement, end-of-tick, and input callers. Current NLTX has no complete drill runtime owner in the searched `dome`/`src` surfaces.
- Dependency impact: C03 supplies immutable drill geometry/timing/power rules; C05/C06 supply mount identity and ability-active qualification; C15 owns dust/effect translation. Player movement supplies velocity and input, while world/tile and projectile systems own target validation, mutation, allocation, and commit. The source search found decrements but no authoritative `beamCooldown` writer, so this component must not invent a recharge rule or conflate the global gate with per-beam cooldowns.
- Migration unit: add the fixed-size typed state and idle/reset verifier; route equip, dismount, aim, and per-tick cooldown transitions through one writer; route one block/wall target path through an injected target query; emit a no-op drill intent before enabling tile or projectile commits. Keep random dust and all world/effect writes behind explicit ports, and record the unresolved `beamCooldown` policy before replacing the compatibility path.
- Invariants and side effects: the beam collection is always length eight; cooldowns and the global gate are non-negative and bounded by validated rule limits; a non-empty target requires a positive cooldown and a known purpose; inactive/non-drill mounts cannot accept aim/use commands; rotations are finite and normalized; repeated input cannot reserve the same slot twice in one committed transition; no component/query/system method accesses `Player`, `Main.rand`, `Dust`, tile storage, projectile arrays, or network/persistence state directly.
- Evidence gap and blocking decision: the source does not close the reader/owner for `beamCooldown` or the complete consumption path for `curTileTarget`/`lastPurpose`, and the current NLTX target-selection/tile mutation owner is missing. Integration review must choose the target-query, tile/projectile commit, and effect ports, define accepted-intent versus failed-intent state commit, and settle duplicate/retry identity before implementation.
- Verification: not-verified. The affected Player project build failed before a successful artifact; planned focused coverage remains unrun.

### C17 - `MountVariantFlags`

- `status: implemented-component-source; verificationStatus: not-verified`
- Component source saved at `src/Player/Mount/Components/MountVariantStateComponent.cs`; it uses an explicit discriminated state and does not expose `_mountSpecificData`, `object`, boxed payloads, or third-party callback types. `MountVariantSystem` was not created because systems are outside this task.
- Members completed: report rows 197-201: `BooleanMountData.boolean`, `SelectiveFlyingMountData.showFlyingFrames`, `SelectiveFlyingMountData.allowedToFly`, `ExtraFrameMountData.frame`, and `ExtraFrameMountData.frameCounter`.
- Ownership: one mutually exclusive variant state per mounted player with `None`, `Boolean`, `SelectiveFlying`, or `ExtraFrame` tags. `Boolean` stores the declared boolean value; `SelectiveFlying` stores the two declared flags; `ExtraFrame` stores the frame and counter. Drill state is C16 and is not a payload branch of this component. The tag and payload are changed only by `MountVariantSystem`; C05 owns active/type lifecycle and requests variant creation/clear through an explicit command.
- Lifecycle: equip type 35 creates a zeroed `ExtraFrame` state, type 54 creates `SelectiveFlying` with both flags false, and dismount/reset clears the variant. `allowedToFly` is refreshed from the explicit wing qualification input before flight/frame qualification. A pure query exposes the allowed-flight and frame flags; it cannot write the component. `Boolean` and `ExtraFrame` remain compatibility state until their missing consumers are confirmed, so no speculative frame or effect behavior is added.
- Seam: `MountVariantSystem` consumes `MountVariantEquipCommand`, `MountVariantResetCommand`, and validated wing/frame inputs, then commits a new tagged value. `MountVariantQuery` returns an immutable qualification view to C05/C08 and frame systems. Serialization and network projections consume the tag plus typed payload only after an explicit schema/version decision; no projection writes back to the component.
- Evidence: `D:\TRbackup\Version4\Terraria\Mount.cs:60-94` declares all three nested payload types and defaults; `:2454-2470` reads `allowedToFly` in `CanFly`; `:2603-2610` writes it from `wingsLogic`; `:4018-4037` uses it to choose Velociraptor flying/in-air state; `:4728-4740` clears `_mountSpecificData` on dismount; `:4877-4884` creates `ExtraFrameMountData` and `SelectiveFlyingMountData` on equip. The searched source has no construction/read/write use for `BooleanMountData`, no complete consumer for `showFlyingFrames`, and no direct consumer for `ExtraFrameMountData.frame`/`frameCounter`. Current NLTX `MountCapabilityRegistry.cs:152-159` accepts `allowedToFly` as a parameter but has no complete tagged variant owner.
- Compatibility note: `UpdateFrame` uses primitive boxed `bool` assignments to `_mountSpecificData` for other mount types at `Mount.cs:3429-3444` and `:3490-3505`. Those assignments are not instances of the report's `BooleanMountData`; the migration must either give them a separately named typed transition or retain an adapter, and must never silently map them to this component's `Boolean` tag.
- Dependency impact: C05 supplies active/type and equip/dismount lifecycle; C08 and frame queries consume the pure qualification view; C16 remains an independent drill component; Player wing state and presentation/frame systems provide explicit inputs. Network, persistence, player entity, effects, and boxed legacy compatibility remain integration seams.
- Migration unit: define the tag and typed payloads, add equip/reset and mutual-exclusion tests, route type 54 wing qualification through one variant writer, and route one read-only `CanFly`/frame qualification through `MountVariantQuery`. Do not serialize or delete legacy payloads until the unconsumed fields and unlisted boxed bool transitions have source-backed compatibility tests.
- Invariants and side effects: exactly one tag is active; payload defaults are `false`/`0` as in the source; `allowedToFly` is only true after an accepted wing qualification update; inactive mounts expose `None`; invalid tags/payload combinations fail closed; query calls are deterministic and non-mutating; no variant path adds buffs, changes player size, invokes collision, creates dust/sound/projectiles, accesses randomness, or publishes network/persistence output directly.
- Evidence gap and blocking decision: `BooleanMountData`, `showFlyingFrames`, and `ExtraFrameMountData` have incomplete source consumers, while other boxed bool state is not represented by report rows. Integration review must choose compatibility versus behavior ownership, define a tagged state schema and restore defaults, and decide how unlisted boxed bool transitions are represented without object state.
- Verification: not-verified. The affected Player project build failed before a successful artifact; planned focused coverage remains unrun.
