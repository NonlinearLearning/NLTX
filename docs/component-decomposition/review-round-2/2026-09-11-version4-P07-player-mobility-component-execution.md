# Version4 P07 玩家移动、跳跃、抓钩与穿越组件执行计划

partitionId: P07
sessionId: 557fbd2f1ba042b49770c6351375f1c3
inputReport: D:\TRbackup\NLTX\docs\migration\ledgers\authoritative-20-partitions\P07-Player-Mobility.md
componentDesignPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\2026-09-11-version4-P07-player-mobility-component-design.md
componentExecutionPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\2026-09-11-version4-P07-player-mobility-component-execution.md
executionStatus: failed
implementationStatus: implemented
designStatus: proposed
evidenceStatus: partial
currentNltxStatus: partial
verificationStatus: source-saved-player-build-blocked-5-errors
completedComponents: [PlayerBeetleArmorState, PlayerSolarAndNebulaArmorState, PlayerMagnetAndUtilityAccessoryState, PlayerDashAndGroundTraversalState, PlayerRopeAndPulleyState, PlayerSlideAndCarpetTraversalState, PlayerWingsAndFlightState, PlayerJumpAvailabilityState, PlayerJumpExecutionState, PlayerJumpMobilityModifiers, PlayerGrappleAndRocketState]
currentComponent: none
pendingComponents: []
lastCheckpointUtc: 2026-09-12T10:50:38Z
evidence-gap: C01-C11 component sources are saved. The serial Terraria.Player build is blocked by five
existing errors in src/Player/Progression outside P07; no focused verifier ran, and the existing DLL is
older than this build attempt. Nebula helper semantics, BitsByte/shared-value closure, frame/effect
projections, Item/Equipment/Combat/Spatial/Projectile/Mount owners, network/persistence format,
scheduler ordering, and behavior-equivalence evidence remain open. C11 keeps rocketSoundDelay and
rocketFrame deferred to the proposed effect projection.
blocking-decision: Component-only implementation is allowed, but the affected Player project cannot
establish fresh compile evidence until its unrelated Progression references are repaired. Do not
implement or claim the proposed Systems, Queries, Projections, Ports, Adapters, tests, registration,
network, persistence, or runtime writer replacement.

## 1. 执行边界

本文件是 P07 的实施记录与剩余边界，不授权本轮修改 csproj、测试、Version4、权威报告或
ledger。组件类型和路径已按设计实际写入 `src`；Systems、Queries、Commands、Adapters、
Projections、运行时 writer、网络和持久化仍为 `status: proposed`。本文件不把未运行的编译、
测试、运行时、网络、持久化或行为等价验证描述为通过。

执行必须遵循：一个权威事实一个 writer；Query 只读；Projection 只向外输出；Adapter 负责
旧 API、协议、文件和外部类型；时钟、随机、日志、音频、粒子、Lighting、Gore、网络和
持久化位于显式 port/adapter；文件顺序不表达 runtime schedule。每完成一个叶子组，必须先
同步本文件与 design 文件的检查点元数据，再进入下一个叶子组。

## 2. Ordered implementation units

| unit | 叶子组 | proposed boundary | 单一 writer | 前置 focused evidence |
|---:|---|---|---|---|
| 0 | verifier harness | 在既有 P07/Player verifier 位置扩展窄场景；输出只放 `Build/` | verifier 只断言 | active build ownership、源序号和初始化断言 |
| 1 | PlayerBeetleArmorState | `PlayerBeetleArmorStateComponent` + `PlayerBeetleOrbKinematicsComponent` + frame projection | `PlayerBeetleArmorSystem`/orb system/frame projection | C01 甲虫 Buff、计时、随机运动、效果隔离和回滚 |
| 2 | PlayerSolarAndNebulaArmorState | solar state, shield kinematics, nebula resource | `PlayerSolarNebulaSystem` | C02 shield/dash/resource transition |
| 3 | PlayerMagnetAndUtilityAccessoryState | utility capability component/query | `PlayerUtilityCapabilitySystem` | C03 equipment/Buff-derived capability reset |
| 4 | PlayerDashAndGroundTraversalState | dash and ground traversal state | `PlayerGroundTraversalSystem` plus spatial commit port | C04 dash/stair/slope/out-of-range ordering |
| 5 | PlayerRopeAndPulleyState | rope eligibility and pulley state | `PlayerRopeAndPulleySystem` | C05 relation, pulley input and `meleeEnchant` handoff |
| 6 | PlayerSlideAndCarpetTraversalState | slide/carpet state and frame projection | `PlayerSlideAndCarpetSystem` | C06 ice/skate/carpet collision and timers |
| 7 | PlayerWingsAndFlightState | flight authority plus frame projection | `PlayerFlightSystem` | C07 wing time, logic, max and frame boundary |
| 8 | PlayerJumpAvailabilityState | eight extra-jump qualification pairs | `PlayerJumpQualificationSystem` | C08 reset/consume/refill per jump kind |
| 9 | PlayerJumpExecutionState | execution flags and pogo state | `PlayerJumpSystem` | C09 mutually exclusive execution and reset |
| 10 | PlayerJumpMobilityModifiers | down dash/auto jump/boost/fall | `PlayerJumpMobilityModifierSystem` | C10 modifier ordering and spatial commit |
| 11 | PlayerGrappleAndRocketState | grapple relation and rocket timers | `PlayerGrappleAndRocketSystem` | C11 20-slot relation, rocket release and projectile command |
| 12 | compatibility removal | legacy Player field adapters | one replacement writer per field | all 11 focused verifier sets and integration decisions |

## 3. Global implementation sequence

1. Capture source/target paths and dependency impact in the checkpoint record; do not move code in a
   planning-only session.
2. Add or extend a focused verifier and a compatibility read adapter while the old writer remains
   the only writer.
3. Introduce the smallest domain-first proposed component/system/query/command boundary. Keep Item,
   Buff, Projectile, Tile, Mount, Combat, network and persistence types behind explicit seams.
4. Route one legacy writer through the new owner only after source, lifecycle, reset, invalid input,
   duplicate command and early-return cases are covered.
5. Verify network/persistence projections and prediction/rollback behavior. Remove the compatibility
   writer only after the new writer is proven; rollback restores the prior single-writer boundary.
6. Compile-capable verification must inspect active `dotnet.exe` and `csc.exe` first and run
   serially through `Build/Tools/Invoke-SerialDotnet.ps1`. The attempted command and its blocking
   result are recorded in the verification section below.

## 4. Checkpoint C01: PlayerBeetleArmorState

### 4.1 Source and target plan

| source boundary | proposed target | dependency impact |
|---|---|---|
| `Terraria.Player.beetleOrbs`, `beetleCounter`, `beetleCountdown`, `beetleDefense`, `beetleOffense`, `beetleBuff` | `src/Player/Armor/PlayerBeetleArmorStateComponent.cs` and `PlayerBeetleArmorSystem.cs`; namespace `Terraria.Player.Armor`; `status: proposed` | consumes Equipment/Buff facts; emits Buff and Combat commands; no direct Item or Combat mutation |
| `Terraria.Player.beetlePos`, `beetleVel` | `src/Player/Armor/PlayerBeetleOrbKinematicsComponent.cs` and `PlayerBeetleOrbSystem.cs`; `status: proposed` | consumes player velocity and injected random samples; emits immutable visual/Combat facts; no Projectile identity |
| `Terraria.Player.beetleFrame`, `beetleFrameCounter` | `src/Player/Armor/PlayerBeetleArmorFrameProjection.cs`; `status: proposed` | consumes committed Beetle state; emits renderer snapshot only |

### 4.2 Member execution matrix

| seq | member | migration action | focused check |
|---:|---|---|---|
| 488 | `beetleOrbs` | Translate to `BeetleOrbCount`; preserve 0..3 and Buff tier ordering; old field read adapter only | tier transition, invalid Buff removal, duplicate command |
| 489 | `beetleCounter` | Translate to deterministic timer; preserve defense threshold/offense decay | tick-by-tick counter transition |
| 490 | `beetleCountdown` | Keep transient countdown separate from orb count; do not persist without evidence | tier decrease reset and spawn reset |
| 491 | `beetleDefense` | Recompute from set/effect facts; do not make it a durable equipment flag | reset-before-derive and Buff acceptance |
| 492 | `beetleOffense` | Recompute from set/effect facts; Combat consumes a command/result | reset-before-derive and damage ordering |
| 493 | `beetleBuff` | Keep as derived Buff-present projection of Buff owner | add/remove/clear and no independent writer |
| 512 | `beetlePos` | Move fixed length-3 value state; clear inactive tail slots | inactive zeroing, stable length, rollback |
| 513 | `beetleVel` | Move fixed length-3 value state; replace `Main.rand` with injected random port | deterministic sample replay, damping and velocity subtraction |
| 514 | `beetleFrame` | Move to one-way frame projection | wrap/reset and no authority writeback |
| 515 | `beetleFrameCounter` | Move to one-way frame projection | frame cadence and reset |

### 4.3 Effect, failure and rollback contract

- Buff add/remove, Combat result, random sampling, Dust, Lighting, Gore, audio, network and save
  operations are explicit commands/ports. A Query cannot perform any of them.
- Unknown equipment/Buff definition, invalid orb tier, stale tick or duplicate command fails closed;
  it must not invent a tier or apply a second effect.
- The pre-tick snapshot includes all C01 component values and the injected random sample identity.
  A rollback restores the snapshot and discards uncommitted effect commands. Commit IDs prevent
  duplicate Buff/Combat/effect output.
- If the focused verifier finds changed threshold, timer, orb, frame, reset or side-effect ordering,
  retain the legacy writer and remove only the new adapter. Never enable dual-write.

### 4.4 Focused verifier set

`BeetleBuffTierTransition`, `BeetleCounterTiming`, `BeetleOrbKinematics`, `BeetleEffectsIsolation`,
`BeetleFrameProjection`, and `BeetleRollbackAndDuplicateCommand` are planned and not run. Each must
record the exact project, serial command, exit code, warning/error counts and `Build/bin` artifact
path if it later runs.

### 4.5 C01 checkpoint log

- Design checkpoint: complete for `PlayerBeetleArmorState`; all 10 report members mapped in the
  design document with source sequence, type, proposed owner, lifecycle and current NLTX evidence.
- Component implementation: saved in the actual implementation checkpoint below; verification:
  `source-saved-player-build-blocked-5-errors`.
- Blocking decision: `integration-review` for Buff/Equipment source, Combat effect ownership, random
  and presentation ports, network replication and persistence policy.
- Next checkpoint: `PlayerSolarAndNebulaArmorState`.

### 4.6 Actual component implementation checkpoint

- Saved component sources:
  - `src/Player/Armor/PlayerBeetleArmorStateComponent.cs`
  - `src/Player/Armor/PlayerBeetleOrbKinematicsComponent.cs`
- Implemented state: the six armor/Buff facts (`BeetleOrbCount`, `BeetleCounter`,
  `BeetleCountdown`, `HasDefenseSet`, `HasOffenseSet`, `BeetleBuffActive`) and fixed-capacity
  three-slot orb `Positions`/`Velocities` arrays.
- Defaults: scalar state uses the source-compatible C# zero defaults; the two arrays are allocated
  at capacity 3 and start with zero `Vector2` values. Frame fields 514-515 remain a deferred
  presentation projection.
- Dependency impact: Buff/Equipment, Combat, random sampling, presentation, network, persistence,
  and Spatial ownership remain external. No System, Query, Command, Adapter, Projection, test, or
  registration code was added.
- Verification: `source-saved-player-build-blocked-5-errors`; no focused verifier ran.

## 5. Checkpoint C02: PlayerSolarAndNebulaArmorState

### 5.1 Source/target and dependency impact

| source boundary | proposed target | dependency impact |
|---|---|---|
| Solar shield count/counter/dash flags | `src/Player/Armor/PlayerSolarArmorStateComponent.cs`; `Terraria.Player.Armor`; `status: proposed` | Equipment/Buff input; Buff command and Spatial/Combat output |
| Solar shield position/velocity arrays | `src/Player/Armor/PlayerSolarShieldKinematicsComponent.cs`; `status: proposed` | fixed 3-slot value state; renderer/Spatial snapshot only |
| Nebula levels/counter | `src/Player/Armor/PlayerNebulaResourceStateComponent.cs`; `status: proposed` | Resource/Combat projection; helper evidence is partial |

### 5.2 Member execution matrix

| seq | member | migration action | focused check |
|---:|---|---|---|
| 494 | `solarShields` | preserve Buff tier 0..3 and invalid-tier removal | tier/reset |
| 495 | `solarCounter` | preserve interval timer and grant reset | tick/threshold |
| 496 | `solarShieldPos` | preserve fixed three slots and inactive clearing | orbit/length |
| 497 | `solarShieldVel` | preserve target velocity and deterministic update | velocity/order |
| 498 | `solarDashing` | keep dash phase separate from final velocity writer | start/end/duplicate |
| 499 | `solarDashConsumedFlare` | keep one-time guard in dash command transaction | one-time consumption |
| 500 | `nebulaLevelLife` | map to derived resource level, not life authority | helper/stat projection |
| 501 | `nebulaLevelMana` | map to derived resource level, not mana authority | helper/reset |
| 502 | `nebulaManaCounter` | keep transient until reader/writer evidence is recovered | lifecycle/unknown helper |
| 503 | `nebulaLevelDamage` | map to Combat modifier projection | level/damage ordering |

### 5.3 C02 verifier and rollback

Planned verifiers are `SolarShieldTierAndReset`, `SolarShieldOrbit`, `SolarDashConsumption`,
`NebulaLevelBoundary`, `SolarEffectIsolation`, and `SolarRollbackAndProjection`; none has run.
The checkpoint rolls back to the legacy single writer if tier, orbit, dash, resource projection or
effect ordering changes. The Version4 Nebula helper remains incomplete, so no tier algorithm was
added to the component.

### 5.4 C02 verifier set and checkpoint log

- Design checkpoint: complete for `PlayerSolarAndNebulaArmorState`; all 10 report members are mapped
  with proposed owner, source evidence, current NLTX state and integration seams.
- Component implementation: saved below; verification: `source-saved-player-build-blocked-5-errors`.
- Blocking decision: `integration-review`, especially the empty Nebula helper and Solar dash stub.
- Next checkpoint: `PlayerMagnetAndUtilityAccessoryState`.

### 5.5 Actual component implementation checkpoint: C02

- Saved component sources:
  - `src/Player/Armor/PlayerSolarArmorStateComponent.cs`
  - `src/Player/Armor/PlayerSolarShieldKinematicsComponent.cs`
  - `src/Player/Armor/PlayerNebulaResourceStateComponent.cs`
- Implemented state: Solar `ShieldCount`, `SolarCounter`, `IsSolarDashing`,
  `SolarDashConsumedFlare`; fixed-capacity length-3 shield `Positions`/`Velocities`; Nebula
  `LifeLevel`, `ManaLevel`, `NebulaManaCounter`, and `DamageLevel`.
- Defaults: scalar fields use C# zero defaults; shield arrays have capacity 3 and start with zero
  `Vector2` values. Dust, shader, Lighting, Combat, Buff, Resource, network and persistence effects
  remain external. The Version4 Nebula helper is still incomplete, so no tier algorithm was added.
- Verification: `source-saved-player-build-blocked-5-errors`; no focused verifier ran.

## 6. Checkpoint C03: PlayerMagnetAndUtilityAccessoryState

### 6.1 Source/target and dependency impact

| source boundary | proposed target | dependency impact |
|---|---|---|
| eight Version4 utility/capability fields | `src/Player/Accessories/PlayerUtilityCapabilityComponent.cs`; namespace `Terraria.Player.Accessories`; `status: proposed` | Equipment/Buff facts in; Items/WorldInteraction, Resource, Jump, NPC targeting and Combat snapshots/commands out |
| `hasDeadCellsDownDash` execution handoff | `PlayerJumpQualificationQuery`; `crossSubsystemOwner: integration-review` | Utility publishes qualification only; Jump/Mobility owns execution and velocity |
| `inferno` effect handoff | `IPlayerCombatCommandPort` and `IPlayerLightingPort`; `crossSubsystemOwner: integration-review` | utility fact only; Combat/lighting own side effects and `infernoCounter` remains outside this partition |

### 6.2 Member execution matrix

| seq | member | migration action | focused check |
|---:|---|---|---|
| 504 | `manaMagnet` | clear/recompute from Item/Buff facts; Item pickup owns effect | reset and range |
| 505 | `lifeMagnet` | clear/recompute from Buff/equipment facts; health pickup owns effect | reset and range |
| 506 | `treasureMagnet` | clear/recompute from equipment facts; catalog owns range constant | reset and range |
| 507 | `chiselSpeed` | expose tool-speed capability through query | tool timing |
| 508 | `lifeForce` | expose life-capacity modifier input, not health authority | resource projection |
| 509 | `hasDeadCellsDownDash` | publish qualification; Jump/Mobility owns execution | airborne/mount/down guard |
| 510 | `calmed` | publish targeting/environment capability; final owner integration-review | target query/reset |
| 511 | `inferno` | publish Combat/environment capability; effect ports own output | damage/light/local-remote |

### 6.3 C03 effect, failure and rollback contract

Reset derived facts before applying source definitions. Unknown definitions, stale equipment revisions
and duplicate commands fail closed. Pickup, tool mutation, health, NPC target, Combat, Lighting,
network and persistence operations are ports/adapters; the capability Query is pure. Snapshot the
eight booleans plus source revision before prediction, and discard uncommitted effect commands on
rollback. Keep the legacy writer if any reset, range, timing, qualification or effect order changes.

### 6.4 C03 verifier set and checkpoint log

Planned verifiers are `UtilityResetAndRecompute`, `MagnetPickupQuery`, `ChiselToolTiming`,
`LifeForceProjection`, `DownDashQualification`, and `CalmedAndInfernoEffects`; none has run.

- Design checkpoint: complete for `PlayerMagnetAndUtilityAccessoryState`; all 8 report members are
  mapped with proposed owner, source evidence, current NLTX state and integration seams.
- Component implementation: saved below; verification: `source-saved-player-build-blocked-5-errors`.
- Blocking decision: `integration-review` for Item/Buff/World/Combat/NPC ownership and network/save
  policy.
- Next checkpoint: `PlayerDashAndGroundTraversalState`.

### 6.5 Actual component implementation checkpoint: C03

- Saved component source: `src/Player/Accessories/PlayerUtilityCapabilityComponent.cs`.
- Implemented state: `ManaMagnet`, `LifeMagnet`, `TreasureMagnet`, `ChiselSpeed`, `LifeForce`,
  `HasDeadCellsDownDash`, `Calmed`, and `Inferno`.
- Dependency impact: the component contains only derived capability facts. Item pickup, tool timing,
  life resources, Jump execution, NPC targeting, Combat damage, Lighting, network, and persistence
  remain external owners; no System or Query was added.
- Verification: `source-saved-player-build-blocked-5-errors`; no focused verifier ran.

## 7. Checkpoint C04: PlayerDashAndGroundTraversalState

### 7.1 Source/target and dependency impact

| source boundary | proposed target | dependency impact |
|---|---|---|
| `dashType`, `dash`, `dashTime`, `timeSinceLastDashStarted`, `dashDelay` | `src/Player/Mobility/PlayerDashStateComponent.cs` and `PlayerDashSystem.cs`; namespace `Terraria.Player.Mobility`; `status: proposed` | input/equipment facts in; dash intent/effect commands out; Spatial owns final velocity |
| `stairFall`, `sloping`, `accRunSpeed`, `powerrun`, `runningOnSand` | `src/Player/Mobility/PlayerGroundTraversalStateComponent.cs`; `status: proposed`; `crossSubsystemOwner: integration-review` | Tile/Collision queries in; one spatial movement commit out |
| `outOfRange` | `src/Player/Mobility/PlayerOutOfRangeStateComponent.cs`; `status: proposed`; `crossSubsystemOwner: integration-review` | section visibility state only; local authority must remain live |
| `flapSound` | `src/Player/Mobility/PlayerFlightEffectProjection.cs`; `status: proposed`; `crossSubsystemOwner: integration-review` | flight facts in; audio command out |

### 7.2 Member execution matrix

| seq | member | migration action | focused check |
|---:|---|---|---|
| 524 | `stairFall` | derive from fall-through/grapple/gravity and collision result | slope/stair reset |
| 525 | `outOfRange` | isolate remote section short-circuit from local authority | section recovery |
| 532 | `sloping` | commit collision result fact for current tick only | stale flag prevention |
| 544 | `dashType` | preserve equipment/mount/solar behavior key | capability precedence |
| 545 | `dash` | preserve active mode separate from type | mode start/end |
| 546 | `dashTime` | preserve directional double-tap window | input edge/window |
| 547 | `timeSinceLastDashStarted` | preserve elapsed timer and clamp | reset/clamp |
| 548 | `dashDelay` | preserve positive/negative dash phase | phase/early return |
| 551 | `accRunSpeed` | derive speed parameter, never final velocity | mount/equipment modifiers |
| 582 | `powerrun` | derive from Tile catalog snapshot | surface modifier |
| 583 | `runningOnSand` | derive from converted sand classification | surface modifier |
| 584 | `flapSound` | move to effect gate/projection | one sound per transition |

### 7.3 C04 effect, failure and rollback contract

Dash input, Tile/Collision queries, Spatial commit, section visibility, sound, dust, lighting and
network operations are explicit seams. Unknown dash type, stale section, invalid tile result or
duplicate input fails closed. Snapshot dash/traversal state with input and world-section revision
before prediction; discard uncommitted effects on rollback. Keep the legacy writer when phase,
collision, speed, remote short-circuit or effect order changes.

### 7.4 C04 verifier set and checkpoint log

Planned verifiers are `DashDoubleTapAndPhase`, `DashMovementOwnership`, `SlopeAndStairTraversal`,
`SurfaceRunModifiers`, `OutOfRangeSectionShortCircuit`, and `FlapEffectGate`; none has run.

- Design checkpoint: complete for `PlayerDashAndGroundTraversalState`; all 12 report members are
  mapped with proposed owner, source evidence, current NLTX state and integration seams.
- Component implementation: saved in the actual implementation checkpoint below; verification:
  `source-saved-player-build-blocked-5-errors`.
- Blocking decision: `integration-review` for Spatial/Collision, Network/Visibility, Mount, Audio and
  the dash/solar/equipment precedence contract.
- Next checkpoint: `PlayerRopeAndPulleyState`.

## 8. Checkpoint C05: PlayerRopeAndPulleyState

### 8.1 Source/target and dependency impact

| source boundary | proposed target | dependency impact |
|---|---|---|
| `ropeCount`, `cordage`, `gem`, `gemCount`, `ownedLargeGems` | `src/Player/Mobility/PlayerRopeStateComponent.cs` and `PlayerRopeAndPulleySystem.cs`; namespace `Terraria.Player.Mobility`; `status: proposed` | Inventory/Item and Tile rope queries in; qualification facts out |
| `pulleyDir`, `pulley` | `src/Player/Mobility/PlayerPulleyStateComponent.cs`; `status: proposed`; `crossSubsystemOwner: integration-review` | input/collision facts in; Spatial position/velocity commit out |
| `pulleyFrame`, `pulleyFrameCounter` | `src/Player/Mobility/PlayerPulleyFrameProjection.cs`; `status: proposed` | committed pulley facts in; renderer snapshot out |
| `meleeEnchant` | Combat/Status-owned projection; `crossSubsystemOwner: integration-review` | Buff input in; Combat result/network/save out |

### 8.2 Member execution matrix

| seq | member | migration action | focused check |
|---:|---|---|---|
| 541 | `ropeCount` | preserve 10-tick rope grace and zero gating | lifecycle |
| 552 | `cordage` | recompute equipment rope capability | reset/qualification |
| 553 | `gem` | preserve -1 and catalog selected-gem projection | scan/index |
| 554 | `gemCount` | preserve periodic inventory scan counter | 10-tick scan |
| 555 | `ownedLargeGems` | preserve BitsByte reset and bits | bitset/catalog |
| 556 | `meleeEnchant` | hand off to Combat/Status; P07 is not owner | one writer/clear |
| 557 | `pulleyDir` | preserve pulley phase/direction 1/2 | rope/obstacle |
| 558 | `pulley` | preserve active relation state | enter/exit/cleanup |
| 559 | `pulleyFrame` | move to frame projection | wrap/no authority |
| 560 | `pulleyFrameCounter` | move to frame projection cadence | threshold/reset |

### 8.3 C05 effect, failure and rollback contract

Inventory scans, Tile/Collision reads, Projectile relation validation, Spatial commit, Combat status,
audio/frame/network/save operations are explicit seams. Invalid rope/gem/target or stale inventory
revision fails closed. Snapshot rope/pulley state plus inventory/world revisions before prediction;
discard uncommitted effects on rollback. Keep legacy writer if any relation, scan, direction, collision,
Combat or frame order changes.

### 8.4 C05 verifier set and checkpoint log

Planned verifiers are `RopeGraceLifecycle`, `GemScanProjection`, `MeleeEnchantHandoff`,
`PulleyDiscoveryAndDirection`, `PulleyMovementOwnership`, and `PulleyFrameProjection`; none has run.

- Design checkpoint: complete for `PlayerRopeAndPulleyState`; all 10 report members are mapped with
  proposed/deferred classification and `crossSubsystemOwner` where required.
- Component implementation: saved in the actual implementation checkpoint below; verification:
  `source-saved-player-build-blocked-5-errors`.
- Blocking decision: `integration-review` for Inventory/Projectile/Spatial/Combat and protocol policy.
- Next checkpoint: `PlayerSlideAndCarpetTraversalState`.

### 8.5 Actual component implementation checkpoint: C05

- Saved component sources:
  - `src/Player/Mobility/PlayerRopeStateComponent.cs`
  - `src/Player/Mobility/PlayerPulleyStateComponent.cs`
- Implemented state: rope grace counter, cordage capability, selected gem with source default `-1`,
  gem scan counter, byte-backed large-gem bit payload, pulley direction, and pulley-active state.
- Type/dependency gap: the affected Player project does not expose a reusable `Terraria.BitsByte`
  value type. `OwnedLargeGems` therefore stores the same eight-bit payload as `byte` until the
  shared value boundary is resolved; no new helper or protocol type was created.
- Deferred state: `meleeEnchant` remains Combat/Status-owned; pulley frame fields remain a
  presentation projection. Inventory, Tile, Projectile, Spatial, network, and persistence behavior
  is not implemented here.
- Verification: `source-saved-player-build-blocked-5-errors`; no focused verifier ran.

## 9. Checkpoint C06: PlayerSlideAndCarpetTraversalState

### 9.1 Source/target and dependency impact

| source boundary | proposed target | dependency impact |
|---|---|---|
| `sliding`, `slideDir`, `iceSkate`, `spikedBoots` | `src/Player/Mobility/PlayerSlideStateComponent.cs` and `PlayerSlideSystem.cs`; namespace `Terraria.Player.Mobility`; `status: proposed` | Equipment/Tile/Collision input; Spatial movement intent out |
| `carpet`, `canCarpet`, `carpetTime` | `src/Player/Mobility/PlayerCarpetTraversalStateComponent.cs`; `status: proposed` | Jump/Wing/Grapple/Mount exclusion facts in; Spatial intent out |
| `carpetFrame`, `carpetFrameCounter` | `src/Player/Mobility/PlayerCarpetFrameProjection.cs`; `status: proposed` | committed traversal facts in; renderer snapshot out |
| `snowBallLauncherInteractionCooldown` | Item/WorldInteraction-owned command state; `crossSubsystemOwner: integration-review` | item command path only; no movement writer |

### 9.2 Member execution matrix

| seq | member | migration action | focused check |
|---:|---|---|---|
| 562 | `sliding` | preserve wall/ground traversal state and reset | wall slide/reset |
| 563 | `slideDir` | preserve collision-derived direction | direction/geometry |
| 564 | `snowBallLauncherInteractionCooldown` | keep in Item interaction seam | cooldown owner |
| 565 | `iceSkate` | derive slippery-surface capability | acceleration |
| 566 | `carpet` | derive carpet capability and exclusions | qualification |
| 567 | `spikedBoots` | derive level from equipment; no Item payload copy | thresholds |
| 568 | `carpetFrame` | move to output projection | fixed range/reset |
| 569 | `carpetFrameCounter` | move to output projection cadence | counter/rollback |
| 570 | `canCarpet` | preserve one-use qualification | consume/restore |
| 571 | `carpetTime` | preserve 300 tick active timer | timer/ground reset |

### 9.3 C06 effect, failure and rollback contract

Tile/Collision reads, equipment definitions, Jump/Wing/Grapple/Mount facts, Spatial commit, Item
interaction, frame/audio/network/save operations are explicit seams. Invalid surface, stale exclusion
or duplicate activation fails closed. Snapshot slide/carpet state and input before prediction; discard
uncommitted effects on rollback. Keep the legacy writer if movement, timer, qualification or frame
ordering changes.

### 9.4 C06 verifier set and checkpoint log

Planned verifiers are `WallSlideDirectionAndReset`, `IceSkateSurfaceModifier`,
`CarpetQualificationAndExclusion`, `CarpetTimerAndFrame`, `SnowballInteractionHandoff`, and
`SlideCarpetSpatialOwnership`; none has run.

- Design checkpoint: complete for `PlayerSlideAndCarpetTraversalState`; all 10 report members are
  mapped with proposed/deferred classification and explicit cross-partition seams.
- Component implementation: saved in the actual implementation checkpoint below; verification:
  `source-saved-player-build-blocked-5-errors`.
- Blocking decision: `integration-review` for Item/Spatial/Jump/Wing/Grapple/Mount ordering.
- Next checkpoint: `PlayerWingsAndFlightState`.

### 9.5 Actual component implementation checkpoint: C06

- Saved component sources:
  - `src/Player/Mobility/PlayerSlideStateComponent.cs`
  - `src/Player/Mobility/PlayerCarpetTraversalStateComponent.cs`
- Implemented state: sliding/direction/ice-skate/spiked-boot facts and carpet capability,
  re-use qualification, and active remaining time.
- Deferred state: `snowBallLauncherInteractionCooldown` remains Item/WorldInteraction-owned;
  carpet frame fields remain presentation projection state. Components do not write spatial position,
  velocity, Tile data, Item state, audio, network, or persistence.
- Verification: `source-saved-player-build-blocked-5-errors`; no focused verifier ran.

## 10. Checkpoint C07: PlayerWingsAndFlightState

### 10.1 Source/target and dependency impact

| source boundary | proposed target | dependency impact |
|---|---|---|
| `wingTime`, `wingTimeMax` | reconcile `dome/.../PlayerFlightStateComponent.cs` with `src/Player/Mobility/PlayerFlightStateComponent.cs`; namespace `Terraria.Player.Mobility`; `status: proposed` | Equipment definition and Jump/Carpet/Grapple/Mount facts in; flight consumption out |
| `wings`, `wingsLogic` | `src/Player/Mobility/PlayerFlightDefinitionComponent.cs`; `status: proposed` | catalog revision and equipment facts in; flight/frame queries out |
| `wingFrame`, `wingFrameCounter` | `src/Player/Mobility/PlayerWingFrameProjection.cs`; `status: proposed` | committed flight facts in; renderer/audio snapshot out |

### 10.2 Member execution matrix

| seq | member | migration action | focused check |
|---:|---|---|---|
| 650 | `wingTime` | preserve finite resource consumption and clamp | consume/restore |
| 651 | `wings` | preserve catalog definition key | definition revision |
| 652 | `wingsLogic` | preserve separate behavior logic key | logic override |
| 653 | `wingTimeMax` | preserve capacity and restore boundary | max/landing |
| 654 | `wingFrame` | move to presentation projection | frame/no writeback |
| 655 | `wingFrameCounter` | move to presentation projection cadence | cadence/reset |

### 10.3 C07 effect, failure and rollback contract

Equipment definition, Jump/Carpet/Grapple/Mount qualification, Spatial commit, frame/audio, network
and persistence are explicit seams. Invalid definition or duplicate consume fails closed. Snapshot
flight state and definition revision before prediction; frame/effect output is discarded on rollback.
The existing Flight component is a partial migration input, not a second authority.

### 10.4 C07 verifier set and checkpoint log

Planned verifiers are `FlightResourceConsumption`, `WingDefinitionBoundary`, `FlightResetAndRestore`,
`WingFrameProjection`, `FlightEffectIsolation`, and `FlightRollbackAndAuthority`; none has run.

- Design checkpoint: complete for `PlayerWingsAndFlightState`; all 6 report members are mapped and
  the existing NLTX partial component is explicitly reconciled rather than duplicated.
- Component implementation: saved in the actual implementation checkpoint below; verification:
  `source-saved-player-build-blocked-5-errors`.
- Blocking decision: `integration-review` for Equipment/Jump/Carpet/Grapple/Mount/Spatial and
  client presentation/network policy.
- Next checkpoint: `PlayerJumpAvailabilityState`.

### 10.5 Actual component implementation checkpoint: C07

- Saved component sources:
  - `src/Player/Mobility/PlayerFlightStateComponent.cs`
  - `src/Player/Mobility/PlayerFlightDefinitionComponent.cs`
- Implemented state: `WingTime`, `WingTimeMax`, `Wings`, and `WingsLogic`.
- Deferred state: `wingFrame` and `wingFrameCounter` remain presentation projection state. No flight
  consumption, gravity, liquid, mount, Spatial, audio, network, persistence, or frame behavior was
  added.
- Verification: `source-saved-player-build-blocked-5-errors`; no focused verifier ran.

## 11. Checkpoint C08: PlayerJumpAvailabilityState

### 11.1 Source/target and dependency impact

| source boundary | proposed target | dependency impact |
|---|---|---|
| 9 `hasJumpOption_*` fields | `src/Player/Jump/PlayerJumpAvailabilityComponent.cs`; namespace `Terraria.Player.Jump`; `status: proposed` | Equipment/Buff/ability facts in; pure qualification Query out |
| 9 `canJumpAgain_*` fields | same Component, Jump-owned writer | consume/refresh commands; no direct Spatial write |

### 11.2 Member execution matrix

| pair | members | migration action | focused check |
|---|---|---|---|
| Cloud | `hasJumpOption_Cloud`, `canJumpAgain_Cloud` | keep qualification separate from consumable window | qualify/consume/refresh |
| Sandstorm | `hasJumpOption_Sandstorm`, `canJumpAgain_Sandstorm` | same pair contract | qualify/consume/refresh |
| Blizzard | `hasJumpOption_Blizzard`, `canJumpAgain_Blizzard` | same pair contract | qualify/consume/refresh |
| Fart | `hasJumpOption_Fart`, `canJumpAgain_Fart` | same pair contract; effects external | consume/effect dedupe |
| Sail | `hasJumpOption_Sail`, `canJumpAgain_Sail` | same pair contract | qualify/consume/refresh |
| Unicorn | `hasJumpOption_Unicorn`, `canJumpAgain_Unicorn` | same pair contract | qualify/consume/refresh |
| Santank | `hasJumpOption_Santank`, `canJumpAgain_Santank` | same pair contract | qualify/consume/refresh |
| WallOfFleshGoat | `hasJumpOption_WallOfFleshGoat`, `canJumpAgain_WallOfFleshGoat` | same pair contract | qualify/consume/refresh |
| Basilisk | `hasJumpOption_Basilisk`, `canJumpAgain_Basilisk` | same pair contract | qualify/consume/refresh |

The report contains 18 members across these nine named pairs. Every pair is one source capability
plus one availability window; no pair may be collapsed into one boolean.

### 11.3 C08 effect, failure and rollback contract

Equipment/Buff input, pure qualification, Jump consume/refresh commands, Spatial motion, mount/wing/
carpet/grapple exclusion, network/save projection and visual effects are explicit seams. Unknown kind,
stale source revision or duplicate command fails closed. Snapshot all pairs plus command key before
prediction and discard uncommitted effects on rollback. Legacy writer remains until verifier evidence.

### 11.4 C08 verifier set and checkpoint log

Planned verifiers are `JumpQualificationMatrix`, `ExtraJumpConsumeOnce`, `RefreshDoubleJumpsLifecycle`,
`JumpPriorityAndExclusion`, and `ExtraJumpProjection`; none has run.

- Design checkpoint: complete for `PlayerJumpAvailabilityState`; all 18 report members are mapped
  as qualification or consumable availability with explicit owner and lifecycle.
- Component implementation: saved in the actual implementation checkpoint below; verification:
  `source-saved-player-build-blocked-5-errors`.
- Blocking decision: `integration-review` for extra-jump definitions, execution priority, networking,
  persistence and effects.
- Next checkpoint: `PlayerJumpExecutionState`.

### 11.5 Actual component implementation checkpoint: C08

- Saved component source: `src/Player/Jump/PlayerJumpAvailabilityComponent.cs`.
- Implemented state: all 18 qualification/window fields, with each `Has*Option` capability separate
  from its corresponding `CanJumpAgain*` consumable window.
- Dependency impact: Equipment/Buff qualification, Jump consume/refresh, exclusion facts, Spatial
  motion, effects, network, and persistence remain external; no Query or execution behavior was added.
- Verification: `source-saved-player-build-blocked-5-errors`; no focused verifier ran.

## 12. Checkpoint C09: PlayerJumpExecutionState

### 12.1 Source/target and dependency impact

| source boundary | proposed target | dependency impact |
|---|---|---|
| Down Dash and 9 extra-jump execution flags | `src/Player/Jump/PlayerJumpExecutionComponent.cs` and `PlayerJumpSystem.cs`; namespace `Terraria.Player.Jump`; `status: proposed` | availability Query/input in; movement intent/effect facts out |
| Pogo tricks flag | same Jump execution boundary; `crossSubsystemOwner: integration-review` for Mount | Mount input in; pose/effect projection out |

### 12.2 Member execution matrix

| seq | member | migration action | focused check |
|---:|---|---|---|
| 1206 | `isPerformingJump_DownDash` | preserve active Down Dash execution and cancel/reset | lifecycle/guard |
| 1210 | `isPerformingJump_Cloud` | preserve Cloud execution flag | selection/visual |
| 1213 | `isPerformingJump_Sandstorm` | preserve Sandstorm execution flag | selection/visual |
| 1216 | `isPerformingJump_Blizzard` | preserve Blizzard execution flag | selection/modifier |
| 1219 | `isPerformingJump_Fart` | preserve Fart execution flag | selection/effect |
| 1222 | `isPerformingJump_Sail` | preserve Sail execution flag | selection/random effect |
| 1225 | `isPerformingJump_Unicorn` | preserve Unicorn execution flag | selection/effect |
| 1228 | `isPerformingJump_Santank` | preserve Santank execution flag | selection/effect |
| 1231 | `isPerformingJump_WallOfFleshGoat` | preserve Goat execution flag | selection/effect |
| 1234 | `isPerformingJump_Basilisk` | preserve Basilisk execution flag | selection/effect |
| 1235 | `isPerformingPogostickTricks` | keep Mount seam explicit | mount/pose |

### 12.3 C09 effect, failure and rollback contract

Jump qualification, execution commit, Spatial intent, Mount, Presentation, random, Dust/Gore/shader,
audio, network and persistence are explicit seams. Unknown kind, conflicting flags or duplicate
command fails closed. Snapshot execution state and command key before prediction; discard effects on
rollback. Keep legacy writer if priority or clearing changes.

### 12.4 C09 verifier set and checkpoint log

Planned verifiers are `ExtraJumpExecutionExclusive`, `DownDashExecutionLifecycle`,
`JumpVisualEffectIsolation`, `PogostickMountBoundary`, and `ExecutionRollbackAndProjection`; none has
run.

- Design checkpoint: complete for `PlayerJumpExecutionState`; all 11 report members are mapped with
  one Jump execution writer and explicit Mount/Presentation seams.
- Component implementation: saved in the actual implementation checkpoint below; verification:
  `source-saved-player-build-blocked-5-errors`.
- Blocking decision: `integration-review` for Jump priority, Spatial commit, Mount pose and effect/
  network/persistence policy.
- Next checkpoint: `PlayerJumpMobilityModifiers`.

### 12.5 Actual component implementation checkpoint: C09

- Saved component source: `src/Player/Jump/PlayerJumpExecutionComponent.cs`.
- Implemented state: the 11 execution flags for Down Dash, the nine named extra jumps, and
  `IsPerformingPogostickTricks`.
- Dependency impact: Jump execution owns future flag transitions; Spatial, Mount, Presentation,
  effects, network, and persistence remain external. No Jump System, Mount seam, or effect port was
  added.
- Verification: `source-saved-player-build-blocked-5-errors`; no focused verifier ran.

## 13. Checkpoint C10: PlayerJumpMobilityModifiers

### 13.1 Source/target and dependency impact

| source boundary | proposed target | dependency impact |
|---|---|---|
| `downDashTime`, `autoJump`, `justJumped`, `jumpSpeedBoost`, `extraFall` | `src/Player/Jump/PlayerJumpMobilityModifiersComponent.cs` and `PlayerJumpMobilityModifierSystem.cs`; namespace `Terraria.Player.Jump`; `status: proposed` | Equipment/Buff and Jump facts in; immutable Spatial modifier output out |
| final gravity/fall/velocity application | Spatial/Collision integration port; `crossSubsystemOwner: integration-review` | P07 never becomes second position/velocity writer |

### 13.2 Member execution matrix

| seq | member | migration action | focused check |
|---:|---|---|---|
| 1207 | `downDashTime` | preserve active-tick counter and reset | lifecycle |
| 1236 | `autoJump` | derive input policy from equipment/effects | hold/release |
| 1237 | `justJumped` | preserve current-tick fact and clear boundary | one-tick ordering |
| 1238 | `jumpSpeedBoost` | preserve additive jump parameter | arithmetic/reset |
| 1239 | `extraFall` | preserve additive fall parameter and Spatial handoff | gravity/handoff |

### 13.3 C10 effect, failure and rollback contract

Equipment/Buff calculation, Jump execution, modifier calculation and Spatial/Collision commit are
explicit seams. Invalid or non-finite values, stale source revisions and duplicate commands fail
closed. Snapshot all five values plus source/command revision; recompute derived values on rollback;
keep legacy writer if arithmetic or order differs.

### 13.4 C10 verifier set and checkpoint log

Planned verifiers are `DownDashTimerLifecycle`, `AutoJumpInputPolicy`, `JustJumpedTickFact`,
`JumpModifierCalculation`, and `SpatialModifierHandoff`; none has run.

- Design checkpoint: complete for `PlayerJumpMobilityModifiers`; all 5 report members are mapped as
  transient/derived modifiers with explicit Spatial handoff.
- Component implementation: saved in the actual implementation checkpoint below; verification:
  `source-saved-player-build-blocked-5-errors`.
- Blocking decision: `integration-review` for Equipment/Jump/Spatial/Flight/Rocket ordering.
- Next checkpoint: `PlayerGrappleAndRocketState`.

### 13.5 Actual component implementation checkpoint: C10

- Saved component source: `src/Player/Jump/PlayerJumpMobilityModifiersComponent.cs`.
- Implemented state: `DownDashTime`, `AutoJump`, `JustJumped`, `JumpSpeedBoost`, and `ExtraFall`.
- Dependency impact: the component stores jump modifiers and current-tick facts only. Equipment/Buff
  derivation, Jump ordering, Spatial/Collision application, Flight/Rocket interaction, network, and
  persistence remain external; the component does not write position, velocity, or gravity.
- Verification: `source-saved-player-build-blocked-5-errors`; no focused verifier ran.

## 14. Checkpoint C11: PlayerGrappleAndRocketState

### 14.1 Source/target and dependency impact

| source boundary | proposed target | dependency impact |
|---|---|---|
| `grappling[20]`, `grapCount` | `src/Player/Grapple/PlayerGrappleRelationComponent.cs` and `PlayerGrappleSystem.cs`; namespace `Terraria.Player.Grapple`; `status: proposed` | Projectile relation validation in; grapple movement/cleanup commands out |
| rocket resource/phase/release fields | `src/Player/Grapple/PlayerRocketStateComponent.cs` and `PlayerRocketSystem.cs`; `status: proposed` | Jump/Flight/Carpet facts in; Spatial/Projectile/effect commands out |
| `rocketBoots`, `vanityRocketBoots` | `src/Player/Grapple/PlayerRocketCapabilityComponent.cs`; `status: proposed`; equipment owner integration-review | Equipment revision in; capability/effect queries out |
| `rocketSoundDelay`, `rocketFrame` | `src/Player/Grapple/PlayerRocketEffectProjection.cs`; `status: proposed` | committed rocket facts in; audio/renderer snapshot out |

### 14.2 Member execution matrix

| seq | member | migration action | focused check |
|---:|---|---|---|
| 1246 | `grappling` | preserve fixed 20 legacy Projectile slot relation | identity/slot/reuse |
| 1247 | `grapCount` | preserve valid relation count and cleanup | count/clear |
| 1248 | `rocketTime` | preserve current rocket resource | consume/restore |
| 1249 | `rocketTimeMax` | preserve capacity separate from current resource | definition/reset |
| 1250 | `rocketDelay` | preserve active rocket phase timer | phase/early return |
| 1251 | `rocketDelay2` | preserve visual/effect cadence timer | cadence |
| 1252 | `rocketSoundDelay` | move sound gate to effect projection | one sound/rollback |
| 1253 | `rocketRelease` | preserve release edge gate | input/duplicate |
| 1254 | `rocketFrame` | move frame flag to effect projection | frame/reset |
| 1255 | `rocketBoots` | preserve functional capability level | equipment/qualification |
| 1256 | `vanityRocketBoots` | preserve appearance/effect level separately | vanity/functional split |
| 1257 | `canRocket` | preserve airborne one-use qualification | exclusion/exhaustion |

### 14.3 C11 effect, failure and rollback contract

Projectile relation, equipment definition, Jump/Flight/Carpet qualification, Spatial commit, rocket
resource, audio/renderer, network, persistence and child Projectile commands are explicit seams. Stale
slot, invalid owner/type, exhausted resource, unknown boot definition or duplicate command fails closed.
Snapshot relation and rocket state with source revisions; discard uncommitted Projectile/effect output
on rollback. Keep the legacy writer if any identity, cleanup, timing or effect order changes.

### 14.4 C11 verifier set and checkpoint log

Planned verifiers are `GrappleRelation20Slot`, `GrappleProjectileIdentityBoundary`, `GrappleCleanup`,
`RocketResourceAndDelay`, `RocketReleaseAndQualification`, `RocketEquipmentProjection`, and
`RocketEffectIsolationAndRollback`; none has run.

- Design checkpoint: complete for `PlayerGrappleAndRocketState`; all 12 report members are mapped,
  including the fixed relation boundary and separate rocket capability/effect projections.
- Component implementation: saved in the actual implementation checkpoint below; verification:
  `source-saved-player-build-blocked-5-errors`.
- Blocking decision: `integration-review` for Projectile identity/lifecycle, Equipment, Jump/Flight,
  Spatial, Presentation, network and persistence.

### 14.5 Actual component implementation checkpoint: C11

- Saved component sources:
  - `src/Player/Grapple/PlayerGrappleRelationComponent.cs`
  - `src/Player/Grapple/PlayerRocketStateComponent.cs`
  - `src/Player/Grapple/PlayerRocketCapabilityComponent.cs`
- Implemented state: a fixed 20-slot `int` Projectile relation initialized to `-1` with `Count`;
  rocket time, capacity, phase/effect delay, release, and qualification state; and functional versus
  vanity boot levels. `RocketTimeMax` retains the source default `7`.
- Deferred state: `rocketSoundDelay` and `rocketFrame` remain effect projection state. Slot attach,
  removal, cleanup, Projectile identity validation, rocket movement, effects, network, and persistence
  are not implemented in components.
- Verification: `source-saved-player-build-blocked-5-errors`; no focused verifier ran.

## 15. Prediction, rollback and compatibility policy

The implementation must keep a tick/version on every input-derived command, reject stale or duplicate
commands, and snapshot authoritative state before prediction. A network or persistence adapter may
serialize only committed snapshots. Legacy public field names and namespaces remain available through
read adapters until replacement ownership is verified; no adapter may create a second writer.

## 16. Verification result and settlement

The required compile-capable command was executed after the active-build check using the wrapper's
named argument-array form (the wrapper otherwise treats `-p:...` as an ambiguous script parameter):

```powershell
pwsh -NoProfile -Command '$dotnetArguments = [string[]]@("build", ".\src\Player\Terraria.Player.csproj", "-m:1", "-nr:false", "-p:UseSharedCompilation=false", "-p:MSBuildNodeReuse=false", "-p:BuildInParallel=false"); & ".\Build\Tools\Invoke-SerialDotnet.ps1" -DotnetArguments $dotnetArguments; exit $LASTEXITCODE'
```

- Project: `src/Player/Terraria.Player.csproj`; exit code: `1`; warnings: `0`; errors: `5`.
- Blocking errors are in existing `src/Player/Progression` sources: missing
  `Terraria.Relationships`, missing `Terraria.Projectile`, missing `EntityReference`, and missing
  `ProjectileIdentityComponent`.
- Expected artifact: `D:\TRbackup\NLTX\Build\bin\Terraria.Player\Debug\net10.0\Terraria.Player.dll`.
  The path exists with timestamp `2026-09-12T02:04:19Z`, before this attempt, so it is stale and
  is not counted as a successful output. No `--no-build --no-restore` focused verifier was run.

The component-only implementation is saved for all C01-C11 groups, but the manual partition session
is settled as failed because the affected project cannot be compile-verified without changing
unrelated Progression sources. The deferred Systems, Queries, Commands, Adapters, Projections,
effects, network, persistence, and runtime writer replacement remain outside this session. The
authoritative P07 runner was settled with `Fail` using session
`557fbd2f1ba042b49770c6351375f1c3`; it returned exit code `1` and `lockReleased: true`.

## 17. Completion criteria

The component implementation portion is complete when all 112 members have one proposed/deferred
classification, one owner or explicit `crossSubsystemOwner: integration-review`, a source/target path,
dependency impact, compatibility rule and focused verifier, and every implementable component state is
saved under `src`. Overall session completion additionally requires both Markdown self-checks, the
repository build result, and the original P07 runner settlement. Systems, Queries, Commands, Adapters,
Projections, effects, network and persistence are intentionally not claimed as implemented here.
