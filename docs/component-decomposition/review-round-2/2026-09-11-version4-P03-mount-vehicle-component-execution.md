# Version4 P03 坐骑与车辆组件执行计划

> 本文记录 P03 的实施顺序、依赖影响和真实 checkpoint。当前 session 已保存四个 ECS 组件源码；非组件代码仍未实现。

partitionId: P03  
sessionId: 1c308b3eb4f84a0987329c3385ba10ee  
inputReport: D:\TRbackup\NLTX\docs\migration\ledgers\authoritative-20-partitions\P03-Mount-Vehicle.md  
componentDesignPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\2026-09-11-version4-P03-mount-vehicle-component-design.md  
componentExecutionPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\2026-09-11-version4-P03-mount-vehicle-component-execution.md  
executionStatus: failed  
implementationStatus: partial  
verificationStatus: not-verified  
evidenceStatus: source-inventory-confirmed; Version4-source-confirmed; current-NLTX-partial  
nltxStatus: partial  
completedComponents: [MountRuntimeFrameAndFlightState, MountFatigueAndAbilityState, DrillMountRuntime, MountVariantFlags]  
currentComponent: none  
pendingComponents: [MountFrameAndDrawCatalog, MountSpecialVehicleCatalog, MountDrillConstants, MountSuperCartConstants, MountRuntimeIdentityAndFrameProjection, MountRuntimeMobilityAndAbilityProjection, MountGeometryAndOffsetCatalog, MountGroundAnimationFrames, MountAerialAndWaterAnimationFrames, MountDashAnimationFrames, MountMovementAndAbilityCatalog, MountVehicleAndPresentationCatalog, MountDelegateContract]  
lastCheckpointUtc: 2026-09-12T07:05:35.6354864Z  
evidence-gap: C05, C06, C16, and C17 component source is saved but unverified. The serial build of `src/Player/Terraria.Player.csproj` failed with five existing cross-project reference errors, so no successful compile evidence exists for the new components. Their systems, queries, definition catalogs, adapters, focused verifiers, runtime writer replacement, serialization, network, persistence, and behavior-equivalence evidence are still absent. C01-C04 and C07-C15 remain pending because their planned targets are definitions, queries, systems, or adapters outside the component-only scope.  
blocking-decision: Do not implement or claim any non-component target in this checkpoint. Existing PlayerMountComponent/PlayerMountState remain untouched compatibility skeletons until one-writer evidence and integration ownership are approved. Variant serialization, drill target/effect commit, beamCooldown ownership, and adapter retry semantics remain integration-review items. This retry session was claimed through the runner after the predecessor session failed; it continues the same component-only boundary without claiming the old session.  

## 1. Scope and non-goals

This plan covers only the 17 P03 leaf subsystems and their 163 report members. The current checkpoint modifies only four ECS component `.cs` files under `src/Player/Mount/Components` and these two progress documents. It does not modify systems, queries, commands, adapters, projections, tests, `.csproj` files, source generators, reports, indexes, network protocols, or persistence formats. It does not claim that current NLTX is behavior-equivalent to Version4.

### 1.1 Actual component checkpoint

The runner-bound session `1c308b3eb4f84a0987329c3385ba10ee` is continuing the component checkpoint created by predecessor session `2ff556cf95db4c3b98a147cc837f2863`; the following component sources are present:

- `src/Player/Mount/Components/MountRuntimeFrameAndFlightStateComponent.cs`: active/type/frame/flight and frame-state counters.
- `src/Player/Mount/Components/MountFatigueAndAbilityStateComponent.cs`: fractional fatigue and ability timers/flags.
- `src/Player/Mount/Components/DrillMountRuntimeComponent.cs`: bounded eight-slot drill beam state, component-local typed target/purpose, rotations, and crosshair position.
- `src/Player/Mount/Components/MountVariantStateComponent.cs`: `None`/`Boolean`/`SelectiveFlying`/`ExtraFrame` tagged payload state.

These files contain state and defaults only. No system is present to write transitions, no query is present to project the state, and no component registration, network, persistence, or effect metadata was invented without an existing framework contract. The verification attempt is `not-verified` because the affected project build failed before a successful artifact could be produced.

### 1.2 Verification evidence

- Command: `pwsh -NoProfile -Command "& '.\\Build\\Tools\\Invoke-SerialDotnet.ps1' @('build', '.\\src\\Player\\Terraria.Player.csproj', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')"`
- Project: `src/Player/Terraria.Player.csproj`
- Exit code: `1`
- Result: `0` warnings, `5` errors; all reported errors are existing `PlayerMinionCapacityCommitSystem.cs` / `SubmitMinionCapacityDeltaCommand.cs` references to unavailable `Relationships`, `Projectile`, `EntityReference`, and `ProjectileIdentityComponent` types. The new mount component files produced no diagnostic in the retry.
- Expected output: `Build/bin/Terraria.Player/Debug/net10.0/Terraria.Player.dll`; no successful artifact is attributable to this build.
- No focused verifier or test code was created or run, per the component-only scope.

The proposed directory root follows the existing Player capability ownership while adding a meaningful Mount boundary:

```text
dome/src/Terraria.Dome.Simulation/Player/Mount/
  Definitions/
  Components/
  Queries/
  Systems/
  Adapters/
  Projections/
```

No `Shared/Components/`, `Common/`, or `Misc/` directory is proposed. Each same-named PascalCase file will contain one core public type when implementation is authorized. Exact namespace and project references must be checked before each commit.

## 2. Migration safeguards

- Preserve Version4 public names and semantics at adapters while introducing typed internal boundaries. Do not delete a legacy field until a focused verifier demonstrates one authoritative writer and complete read coverage.
- Do not double-write mount type, flight/fatigue resources, ability timers, or geometry. A compatibility adapter may translate, but it must not become a second authority.
- Keep definition/catalog data immutable after bootstrap. Any mod/content extension must have an explicit registration phase and deterministic ID validation.
- Keep entity identity, persistent player slot, network player slot, and mount content ID as separate values. Do not use a mount type as an entity or network identity.
- Commands carry intent; systems perform state transitions; queries are read-only; projections observe committed state only.
- Every step is independently revertible. Before implementation, record source path, target path, namespace, dependency impact, verifier, and rollback condition.

## 3. Proposed implementation sequence

| Step | Component | Proposed target path | Primary operation | Verification gate |
|---:|---|---|---|---|
| 1 | `MountFrameAndDrawCatalog` | `Player/Mount/Definitions/MountFrameAndDrawCatalogDefinition.cs`; `Player/Mount/Queries/MountFrameAndDrawQuery.cs` | extract constants and rat frame sequence as immutable catalog data | catalog default and mutation-leak verifier |
| 2 | `MountSpecialVehicleCatalog` | `Player/Mount/Definitions/MountSpecialVehicleCatalogDefinition.cs`; query | move Scutlix/Santank and mount table data behind catalog registration | special vehicle lookup verifier |
| 3 | `MountDrillConstants` | `Player/Mount/Definitions/MountDrillConstantsDefinition.cs`; query | isolate drill geometry and timing constants | constant range and source-value verifier |
| 4 | `MountSuperCartConstants` | `Player/Mount/Definitions/MountSuperCartDefinition.cs`; query | isolate super cart override values | override projection verifier |
| 5 | `MountRuntimeFrameAndFlightState` | `Player/Mount/Components/MountRuntimeFrameAndFlightStateComponent.cs`; `Player/Mount/Systems/MountRuntimeSystem.cs` | introduce typed owner for active/type/frame/flight lifecycle | equip, reset, frame, and flight transition verifier |
| 6 | `MountFatigueAndAbilityState` | `Player/Mount/Components/MountFatigueAndAbilityStateComponent.cs`; `Player/Mount/Systems/MountAbilitySystem.cs` | preserve fractional fatigue and timer semantics | flight/fatigue/ability verifier |
| 7 | `MountRuntimeIdentityAndFrameProjection` | `Player/Mount/Queries/MountRuntimeIdentityAndFrameProjectionQuery.cs` | replace direct property reads with pure snapshot queries | deterministic no-write projection verifier |
| 8 | `MountRuntimeMobilityAndAbilityProjection` | `Player/Mount/Queries/MountRuntimeMobilityAndAbilityProjectionQuery.cs` | calculate speed, cart, wing, and ability views | projection and edge-case verifier |
| 9 | `MountGeometryAndOffsetCatalog` | `Player/Mount/Definitions/MountGeometryAndOffsetCatalogDefinition.cs`; query | move geometry/offset definitions behind bounded read-only views | offset bounds and collision seam verifier |
| 10 | `MountGroundAnimationFrames` | `Player/Mount/Definitions/MountGroundAnimationFramesDefinition.cs`; query | preserve ground frame ranges and loop rules | frame sequence verifier |
| 11 | `MountAerialAndWaterAnimationFrames` | `Player/Mount/Definitions/MountAerialAndWaterAnimationFramesDefinition.cs`; query | preserve aerial/water frame ranges | flight/water frame verifier |
| 12 | `MountDashAnimationFrames` | `Player/Mount/Definitions/MountDashAnimationFramesDefinition.cs`; query | preserve dash frame ranges | dash frame verifier |
| 13 | `MountMovementAndAbilityCatalog` | `Player/Mount/Definitions/MountMovementAndAbilityCatalogDefinition.cs`; query | move movement, resource, and dismount capability data | catalog-to-projection verifier |
| 14 | `MountVehicleAndPresentationCatalog` | `Player/Mount/Definitions/MountVehicleAndPresentationCatalogDefinition.cs`; query | separate buff, dust, minecart, wing, and light descriptors | presentation descriptor verifier |
| 15 | `MountDelegateContract` | `Player/Mount/Adapters/IMountEffectPort.cs`; `Player/Mount/Adapters/MountDelegateAdapter.cs` | replace callback fields with explicit effect ports | side-effect ordering and failure verifier |
| 16 | `DrillMountRuntime` | `Player/Mount/Components/DrillMountRuntimeComponent.cs`; `Player/Mount/Systems/DrillMountSystem.cs` | bounded beam runtime and explicit tile/projectile intents | drill cooldown, duplicate, and commit verifier |
| 17 | `MountVariantFlags` | `Player/Mount/Components/MountVariantStateComponent.cs`; `Player/Mount/Systems/MountVariantSystem.cs` | replace object-typed variant state with a tagged value | variant lifecycle and serialization verifier |

The sequence is a dependency plan, not a runtime execution claim. The scheduler contract must be implemented separately and tested; source file order is irrelevant.

## 4. Dependency and compatibility plan

### Source-to-target impact

The source boundary is primarily `D:\TRbackup\Version4\Terraria\Mount.cs`, with Player, Main, Projectile, Collision, and delegate callers. The target boundary is `dome/src/Terraria.Dome.Simulation/Player/Mount` plus explicitly reviewed adapters in Player, Spatial, Projectile, World, Server, and Protocol. No source path is moved in this planning session.

### Compatibility window

1. Add read-only definition/query types and compare them against source-backed registry values.
2. Add one runtime component owner at a time while the existing adapter remains the only external compatibility entry point.
3. Route one writer through the new system and assert that the old path is not also writing.
4. Add snapshot/network/persistence fields only after the owner and versioning policy are approved.
5. Remove compatibility reads only after focused verifier evidence covers normal, invalid, reset, duplicate, and recovery paths.

### Cross-subsystem handoff

The following dependencies are intentionally not assigned by this P03 plan: Player entity lifecycle, player position and collider, movement/collision commit, tile storage, projectile allocator, item/buff ownership, network input/replication, persistence, lighting, sound, and dust. Each must be an explicit port or integration-reviewed owner.

## 5. Planned verification commands

No compile-capable command is run in this design session. Future implementation sessions must run from the repository root through `Build/Tools/Invoke-SerialDotnet.ps1`, inspect `dotnet.exe`/`csc.exe` first, use one affected project at a time, and record exit code, warning/error counts, and `Build/bin/` artifact paths. Planned shape only:

```powershell
pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 `
  test .\dome\Test\<affected-verifier>\<affected-verifier>.csproj `
  -m:1 -nr:false -p:UseSharedCompilation=false `
  -p:MSBuildNodeReuse=false -p:BuildInParallel=false
```

Focused checks before compile verification are planned as read-only PowerShell/document checks for member coverage, proposed path uniqueness, status fields, and source/target mapping. The user-requested `git diff --check` command is not run.

## 6. Rollback and completion conditions

Rollback a step if the focused verifier finds a second writer, altered reset/ordering behavior, an unbounded collection, an ID collision, a projection writeback, or an effect that cannot be made explicit. Restore the prior adapter boundary, retain the evidence, and do not advance the next component.

The P03 implementation plan is complete only when every member has one owner or an explicit deferred/evidence-gap, all cross-subsystem owners are reviewed, source/target paths are recorded, focused verifiers cover state transitions and pure queries, and serial build/test evidence is available. This document currently records none of those implementation or verification results.

## 7. Incremental component checkpoint

### C01 - `MountFrameAndDrawCatalog`

- `status: planned-boundary-recorded`
- Target: `Player/Mount/Definitions/MountFrameAndDrawCatalogDefinition.cs` and `Player/Mount/Queries/MountFrameAndDrawQuery.cs` (proposed).
- Source members: report rows 269-278 and 323.
- Dependency impact: frame selection and presentation readers depend on this catalog; no runtime component depends on a mutable array.
- Migration unit: add immutable catalog and focused verifier, then route one read path through the query.
- Rollback: remove the query adapter and retain the legacy read path if defaults or frame sequence differ.
- Verification: not-run.

### C02 - `MountSpecialVehicleCatalog`

- `status: planned-boundary-recorded`
- Target: `Player/Mount/Definitions/MountSpecialVehicleCatalogDefinition.cs` and `Player/Mount/Queries/MountSpecialVehicleCatalogQuery.cs` (proposed; no files created).
- Source members: report rows 279-282 and 292, covering the `mounts` registry, Scutlix eye positions and texture size, Scutlix base damage, and Santank texture size.
- Dependency impact: bootstrap precedes equip and special-vehicle reads; Scutlix combat/presentation adapters consume immutable views. No runtime component or later definition group may expose a mutable `MountData[]` or create a parallel registry.
- Migration unit: build the immutable registry, validate 64 slots and transformed eye positions, add a focused catalog verifier, then route one read-only Scutlix/Santank consumer through the query. Leave combat, rendering, and runtime writes at their existing adapters until ownership is approved.
- Rollback: remove the query adapter and restore the prior read path if slot IDs, eye-coordinate transformation, texture sizes, or source defaults differ; retain the evidence and do not advance runtime migration.
- Verification: not-run. Planned assertions cover slot count, duplicate/invalid IDs, source values, immutable array exposure, transformed eye positions, and deterministic reads.

### C03 - `MountDrillConstants`

- `status: planned-boundary-recorded`
- Target: `Player/Mount/Definitions/MountDrillConstantsDefinition.cs` and `Player/Mount/Queries/MountDrillRulesQuery.cs` (proposed; no files created).
- Source members: report rows 283-291, covering diode points, drill texture dimensions, rotation step, pick power/time, beam batch limit, and maximum drill length.
- Dependency impact: the frozen rules view precedes drill runtime initialization and cursor/geometry queries. It must not own beam targets, cooldowns, random values, tile storage, dust, or projectile allocation.
- Migration unit: create the immutable rules view, validate source defaults and ranges, add the focused rules verifier, then route one drill-system read through the query. Leave `UpdateDrill`/`UseDrill` behavior and external commits at their existing adapters until ownership is approved.
- Rollback: remove the query adapter and retain the legacy constants if defaults, texture initialization, or timing/power semantics differ; do not advance the drill runtime component.
- Verification: not-run. Planned assertions cover source defaults, positive/range constraints, finite geometry, immutable values, invalid content IDs, and deterministic reads.

### C04 - `MountSuperCartConstants`

- `status: planned-boundary-recorded`
- Target: `Player/Mount/Definitions/MountSuperCartDefinition.cs` and `Player/Mount/Queries/MountSuperCartQuery.cs` (proposed; no files created).
- Source members: report rows 317-321, covering Super Cart run/dash speed, acceleration, jump height, and jump speed.
- Dependency impact: the override query is consumed after cart/Super Cart qualification and before movement/jump commit. It must not own `_shouldSuperCart`, player input, velocity, collision, or persistence.
- Migration unit: freeze the five values, add a pure override query, and route one mobility projection read through it while retaining the current eligibility owner.
- Rollback: remove the query adapter and keep the legacy override path if any source value or qualification fallback differs; do not migrate movement writes.
- Verification: not-run. Planned assertions cover source values, qualification gating, ordinary-value fallback, finite ranges, and query non-mutation.

### C05 - `MountRuntimeFrameAndFlightState`

- `status: implemented-component-source; verificationStatus: not-verified`
- Component source: `src/Player/Mount/Components/MountRuntimeFrameAndFlightStateComponent.cs`. The planned `MountRuntimeSystem` remains unimplemented because systems are outside this task.
- Source members: report rows 293-303, 312-313, 315, and 316. `_data` becomes a validated definition ID/handle; `_mountSpecificData` becomes an explicit variant handoff rather than an `object` component field.
- Dependency impact: catalog resolution precedes equip/frame/flight transitions; fatigue/ability is a separate later owner. The existing NLTX `PlayerMountStateComponent` must be adapted or replaced, never double-written. Geometry, collision, buff, presentation, network, and persistence boundaries remain integration-reviewed.
- Migration unit: add the typed runtime component and one transition system, route equip/dismount/reset and frame/flight reads through it, and retain a read-only compatibility adapter until one-writer evidence exists.
- Rollback: remove the new runtime adapter and restore the prior mount-state path if reset, frame, flight, or definition-ID semantics differ; do not proceed to fatigue/ability migration with two runtime owners.
- Verification: not-verified. The affected Player project build failed before a successful artifact; planned focused assertions remain unrun.

### C06 - `MountFatigueAndAbilityState`

- `status: implemented-component-source; verificationStatus: not-verified`
- Component source: `src/Player/Mount/Components/MountFatigueAndAbilityStateComponent.cs`. The planned `MountAbilitySystem` remains unimplemented because systems are outside this task.
- Source members: report rows 304-311. Preserve Version4 floating accumulated fatigue and separate it from ability charge/cooldown/duration/active/aiming state.
- Dependency impact: hover/movement and frame projections read this component; drill runtime consumes ability-active qualification; projectile and input adapters provide explicit commands. No projectile IDs, mouse state, randomness, or effect service belongs in the component.
- Migration unit: add a semantic comparison verifier for Version4 fatigue versus current NLTX integer remaining fatigue, then introduce the typed state and route one recovery/ability transition through it. Defer flight-input and snapshot changes until the polarity/precision decision is approved.
- Rollback: retain the current adapter and stop migration if fractional fatigue, zero-max behavior, charge limits, or ability-active lifecycle differs; do not create a second resource owner.
- Verification: not-verified. The affected Player project build failed before a successful artifact; planned focused assertions remain unrun.

### C07 - `MountRuntimeIdentityAndFrameProjection`

- `status: planned-boundary-recorded`
- Target: `Player/Mount/Queries/MountRuntimeIdentityAndFrameProjectionQuery.cs` (proposed; no file created).
- Source members: report rows 314, 322, and 324-334. `_debugDraw` and `_defaultDelegatesData` are recorded as legacy diagnostic/adapter data, not mutable query state.
- Dependency impact: the pure view combines typed runtime state with immutable definitions for identity, frame, and offsets. Geometry/collision/presentation consume it after state commit; network/persistence observe committed views only.
- Migration unit: add the pure view and inactive/out-of-range/default tests, route one read path through it, and keep diagnostics/delegate defaults behind explicit adapters. Do not migrate hitbox or presentation writes in this step.
- Rollback: remove the query adapter and restore direct reads if zero/default semantics, frame bounds, or the public `PlayerXOFfset` compatibility mapping differ; do not route collision through an unverified projection.
- Verification: not-run. Planned assertions cover identity values, inactive/default behavior, offset bounds, legacy spelling compatibility, deterministic repeated reads, and no writes/effects from the query.

### C08 - `MountRuntimeMobilityAndAbilityProjection`

- `status: planned-boundary-recorded`
- Target: `Player/Mount/Queries/MountRuntimeMobilityAndAbilityProjectionQuery.cs` (proposed; no file created).
- Source members: report rows 335-350, covering speed/acceleration, jump and slime qualification, cart/rail/wing qualification, delegate view, ability state/charge, direction change, and item-use dismount.
- Dependency impact: the pure query depends on C02-C06 and is evaluated after state transitions but before movement/collision commit. `Delegations` must be a non-executable descriptor; effect execution belongs to C15.
- Migration unit: implement source-backed pure formulas and route one movement read through the query while retaining a compatibility adapter for the current registry. Defer movement writes, collision, effects, and callback execution.
- Rollback: remove the query adapter and retain existing capability reads if any special speed, inactive default, fatigue ratio, or zero-max behavior differs; do not replace the registry wholesale without per-value evidence.
- Verification: not-run. Planned assertions cover all special speed cases, Super Cart override, fatigue ratio, inactive defaults, cart/rail/wing qualification, non-executing delegate descriptors, ability charge/cooldown, dismount qualification, and no query writeback.

### C09 - `MountGeometryAndOffsetCatalog`

- `status: planned-boundary-recorded`
- Target: `Player/Mount/Definitions/MountGeometryAndOffsetCatalogDefinition.cs` and `Player/Mount/Queries/MountGeometryAndOffsetQuery.cs` (proposed; no files created).
- Source members: report rows 210-217 and 268, covering texture dimensions, x/y offsets, per-frame player Y offsets, body/head frame offsets, height boost, and player X offset.
- Dependency impact: immutable geometry bootstrap precedes frame and collision integration; animation definitions consume the same ID and must not duplicate arrays. Query output cannot mutate player size, position, shadows, or collision.
- Migration unit: freeze source geometry values, validate array lengths and bounds, add immutable-array tests, and route one geometry consumer through the query. Leave player resize and collision writes at the reviewed integration boundary.
- Rollback: remove the geometry query adapter if source slot values, SetAs* overrides, array lengths, or inactive defaults differ; do not migrate collision-size writes.
- Verification: not-run. Planned assertions cover source slots, dimensions, offsets, array immutability, frame bounds, default/inactive behavior, height boost, and collision seam non-mutation.

### C10 - `MountGroundAnimationFrames`

- `status: planned-boundary-recorded`
- Target: `Player/Mount/Definitions/MountGroundAnimationFramesDefinition.cs` and `Player/Mount/Queries/MountGroundAnimationFramesQuery.cs` (proposed; no files created).
- Source members: report rows 239-245 and 252-255, covering total frames, standing/running ranges and delays, idle range/delay, and loop policy.
- Dependency impact: geometry and frame/draw catalogs are prerequisites; runtime frame state owns current counters. Idle random/time selection must be injected and cannot be hidden in the definition/query.
- Migration unit: freeze source sequences, validate ranges/delays and total-frame bounds, add injected idle-decision tests, and route one ground-frame read through the query. Leave random timing, drawing, and effects outside this migration unit.
- Rollback: remove the query adapter if any source range, zero-count idle, loop behavior, delay, or state-reset semantics differ; do not migrate the runtime frame writer.
- Verification: not-run. Planned assertions cover all source definitions, range/delay bounds, idle loop/non-loop, zero-count idle, injected random timing, state-change reset, and query non-mutation.

### C11 - `MountAerialAndWaterAnimationFrames`

- `status: planned-boundary-recorded`
- Target: `Player/Mount/Definitions/MountAerialAndWaterAnimationFramesDefinition.cs` and `Player/Mount/Queries/MountAerialAndWaterAnimationFramesQuery.cs` (proposed; no files created).
- Source members: report rows 246-251 and 256-258, covering flying, in-air, and swimming frame ranges and delays.
- Dependency impact: geometry/frame catalogs precede this query; runtime state owns counters; velocity, wing/grapple, liquid, flight/fatigue, and type-specific rewrites are explicit inputs and must not be hidden in definitions.
- Migration unit: freeze the three sequences, validate ranges and zero-count fallback, add injected-input tests, and route one aerial/water read through the query. Leave physics, wing/grapple state, and effects outside this unit.
- Rollback: remove the query adapter if range, delay, zero-count, or special-state behavior differs; do not migrate aerial frame writes.
- Verification: not-run. Planned assertions cover range/delay bounds, zero-count fallback, flying/in-air/swim transitions, type-specific overrides, flight/fatigue inputs, selective-flying qualification, and query non-mutation.

### C12 - `MountDashAnimationFrames`

- `status: planned-boundary-recorded`
- Target: `Player/Mount/Definitions/MountDashAnimationFramesDefinition.cs` and `Player/Mount/Queries/MountDashAnimationFramesQuery.cs` (proposed; no files created).
- Source members: report rows 259-261, covering dash frame start/count/delay.
- Dependency impact: runtime frame state owns counters; Player dash/dashDelay, velocity direction, `_flipDraw`, and special mount overrides remain explicit inputs/rules. Movement/ability definitions provide eligibility and speed without duplicating frame data.
- Migration unit: freeze dash definitions, validate ranges/delays, add forward/reverse injected-input tests, and route one dash-frame read through the query. Leave Player dash and velocity writes outside this unit.
- Rollback: remove the query adapter if source sequence, reverse stepping, zero-delay behavior, or short-window overrides differ; do not migrate dash state ownership.
- Verification: not-run. Planned assertions cover all source sequences, forward/reverse stepping, zero-count/zero-delay, `_flipDraw`, Player dash-window override, reset, and query non-mutation.

### C13 - `MountMovementAndAbilityCatalog`

- `status: planned-boundary-recorded`
- Target: `Player/Mount/Definitions/MountMovementAndAbilityCatalogDefinition.cs` and `Player/Mount/Queries/MountMovementAndAbilityCatalogQuery.cs` (proposed; no files created).
- Source members: report rows 219-236, covering flight/hover, speed/acceleration/jump/fall, fatigue/resource maxima, constant/extra-jump rules, ability timers, walking grace, and item-use dismount.
- Dependency impact: the immutable catalog seeds/bounds C05/C06 and feeds C08; it must not duplicate special catalogs or derived formulas and must not own runtime counters, velocity, jump results, or ability state.
- Migration unit: freeze 64 source definition slots, validate finite/non-negative values and zero semantics, compare current registry values source-by-source, and route one projection read through the query. Leave resource polarity, movement integration, fall damage, and snapshots unchanged until reviewed.
- Rollback: remove the query adapter if any source value, default, zero-max rule, or current registry comparison differs; do not replace the capability registry wholesale or alter runtime resource semantics in this unit.
- Verification: not-run. Planned assertions cover all source slots, value/default equivalence, zero-max behavior, finite ranges, resource seeding, ability bounds, grace/dismount rules, immutable reads, and query non-mutation.

### C14 - `MountVehicleAndPresentationCatalog`

- `status: planned-boundary-recorded`
- Target: `Player/Mount/Definitions/MountVehicleAndPresentationCatalogDefinition.cs` and `Player/Mount/Queries/MountVehicleAndPresentationCatalogQuery.cs` (proposed; no files created).
- Source members: report rows 218, 237-238, and 262-267, covering buff/dust descriptors, cart/rail/wing flags, lighting, and delegate data.
- Dependency impact: immutable descriptors are consumed after runtime state commit by vehicle/presentation/effect adapters. C15 owns delegate translation; no mutable delegate object or direct effect service crosses the query boundary.
- Migration unit: freeze source descriptor values and delegate keys, validate external IDs/colors/defaults, route one read-only consumer through the query, and emit no effects. Leave buff, lighting, dust, sound, size, rail, and network writes at reviewed adapters.
- Rollback: remove the descriptor adapter if IDs, defaults, light values, or inactive behavior differ; do not replace legacy callbacks or effect commits in this unit.
- Verification: not-run. Planned assertions cover descriptor defaults, cart/rail/wing flags, buff/dust IDs, light values, delegate-key non-execution, inactive behavior, immutable reads, and effect-command boundaries.

### C15 - `MountDelegateContract`

- `status: planned-boundary-recorded`
- Target: `Player/Mount/Adapters/IMountEffectPort.cs` and `Player/Mount/Adapters/MountDelegateAdapter.cs` (proposed; no files created).
- Source members: report rows 202-209, covering minecart dust/sounds, mouth/hand position overrides, player-size override, and dash dust.
- Dependency impact: C14 supplies data-only keys; runtime systems emit typed intents; adapters execute after state/collision commit and never write mount components. No `Action<>`, `Dust`, `Player`, or third-party delegate object enters core state.
- Migration unit: define typed value contracts and key registration, add a no-op effect-intent verifier, and route one pose/size read through a port. Leave direct callbacks, random dust, audio, resize, and rail writes at reviewed integration boundaries.
- Rollback: remove the adapter and restore legacy invocation if context mapping, effect order, missing-key behavior, or duplicate suppression differs; do not migrate external effects without an idempotency decision.
- Verification: not-run. Planned assertions cover registration, deterministic pose/size results, no third-party types in core contracts, effect order, duplicate suppression, missing-key behavior, failure/retry visibility, and no adapter writeback.

### C16 - `DrillMountRuntime`

- `status: implemented-component-source; verificationStatus: not-verified`
- Component source: `src/Player/Mount/Components/DrillMountRuntimeComponent.cs`. The public beam collection is backed by an `Array.AsReadOnly` view; the planned `DrillMountSystem` remains unimplemented because systems are outside this task.
- Source members: report rows 188-196: `curTileTarget`, `cooldown`, `lastPurpose`, `diodeRotationTarget`, `diodeRotation`, `outerRingRotation`, `beams`, `beamCooldown`, and `crosshairPosition`; the nested beam state is bounded to eight slots.
- Dependency impact: C03 provides immutable drill rules; C05/C06 provide mount identity and ability-active qualification; C15 owns dust and visual effect translation. Player input/velocity, target selection, tile mutation, projectile allocation, randomness, and network/persistence are explicit integration dependencies and are not moved by this step.
- Implementation sequence: (1) define a typed fixed-eight beam value and explicit optional tile target/purpose values; (2) create the component only on drill equip and clear it on dismount/reset; (3) route the source cooldown cleanup, target clearing, diode easing, global-gate decrement, aim normalization, and outer-ring wrapping through one `DrillMountSystem` writer; (4) inject a target-query port for block and wall commands and emit typed drill intents with a transition/effect ID; (5) keep tile/projectile commits and dust behind adapters until accepted-intent, retry, and `beamCooldown` ownership are approved.
- Compatibility constraints: map `Point16.NegativeOne` to an empty optional target; map `lastPurpose` values 0/1 to `Block`/`Wall` and reject unknown values with visible diagnostics; never expose a mutable `DrillBeam[]` or an object payload to callers; preserve the source eight-slot selection order and the distinction between global and per-beam cooldown gates.
- Rollback: remove the component writer and retain the compatibility adapter if slot order, target-clearing timing, angle normalization, source cooldown polarity, or ability gating differs; stop before enabling tile/projectile effects if the global gate writer, intent commit result, or retry identity remains unresolved.
- Verification: not-verified. The affected Player project build failed before a successful artifact; planned focused assertions remain unrun.

### C17 - `MountVariantFlags`

- `status: implemented-component-source; verificationStatus: not-verified`
- Component source: `src/Player/Mount/Components/MountVariantStateComponent.cs`. The planned `MountVariantSystem` remains unimplemented because systems are outside this task.
- Source members: report rows 197-201: `BooleanMountData.boolean`, `SelectiveFlyingMountData.showFlyingFrames`, `SelectiveFlyingMountData.allowedToFly`, `ExtraFrameMountData.frame`, and `ExtraFrameMountData.frameCounter`.
- Dependency impact: C05 owns active/type and equip/dismount lifecycle; C08 and frame queries consume the pure variant view; C16 remains a separate drill component. Player wing input, frame presentation, network, persistence, and the unlisted primitive boxed-bool transitions are explicit integration dependencies.
- Implementation sequence: (1) define a `None`/`Boolean`/`SelectiveFlying`/`ExtraFrame` tag with typed payloads; (2) initialize and clear it only from the variant owner during equip/dismount/reset; (3) route type 54 wing qualification through one writer and expose a pure query for `CanFly`/frame qualification; (4) preserve the source false/zero defaults and reject invalid tag/payload combinations; (5) add compatibility coverage for the declared-but-unconsumed fields and separately name the boxed bool behavior at types 44/45 before deleting any legacy path.
- Compatibility constraints: no `object` field, boxed payload, or `BooleanMountData` alias for the unrelated primitive bool assignments; no speculative use of `showFlyingFrames`, `frame`, or `frameCounter`; drill state is not nested into the variant component; snapshots and network projections use typed tag/payload values only after schema/version approval.
- Rollback: remove the new writer and keep a read-only compatibility adapter if type 35/54 initialization, `allowedToFly` updates, inactive defaults, reset timing, or legacy boxed-bool behavior differs; do not serialize or remove legacy payloads while their consumers remain unverified.
- Verification: not-verified. The affected Player project build failed before a successful artifact; planned focused assertions remain unrun.
