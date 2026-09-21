# Version4 P19 世界生成执行组件拆分设计

~~~yaml
partitionId: P19
sessionId: ca556245d09847cab6264716801a8095
handoffId: P19-world-generation-execution-authoritative-handoff-20260912
claimMode: manual
ledgerStatus: completed
inputReport: D:\TRbackup\NLTX\docs\migration\ledgers\authoritative-20-partitions\P19-World-Generation-Execution.md
componentDesignPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\2026-09-11-version4-P19-world-generation-execution-component-design.md
componentExecutionPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\2026-09-11-version4-P19-world-generation-execution-component-execution.md
designStatus: proposed
executionStatus: completed
implementationStatus: implemented
evidenceStatus: partial
nltxStatus: partial
currentNltxStatus: partial
verificationStatus: partially-verified
completedComponents: [WorldGenerationExecutionStateComponent, WorldGenerationProgressComponent, WorldGenerationPassSelectionComponent, WorldGenerationControlStateComponent, WorldGenerationHashMismatchPolicyComponent, WorldGenerationHashMismatchStateComponent, WorldGenerationSnapshotPolicyComponent, WorldGenerationAbortStateComponent, WorldGenerationGeneratorExecutionStateComponent, WorldGenerationExecutionCursorComponent, WorldGenerationPassResultComponent, WorldGenerationManifestComponent, WorldGenerationOptionSelectionComponent]
currentComponent: none
pendingComponents: []
lastCheckpointUtc: 2026-09-12T11:08:32Z
evidence-gap: All authorized P19 state components are implemented in src and compile in the affected WorldSession project; the existing WorldSession focused verifier also passes, but no P19-specific focused behavior verifier is available or run. Non-component execution, persistence, registry, support, thread, legacy-generator, P17, P18, P20, network and scheduler integration boundaries remain open.
blocking-decision: Implemented only state-owned components. Pass definitions, controller pass projections, generatingWorldOnThisThread, _generator, configuration, control lock, snapshots, persistence, registry, support types, P17 terrain metrics, P18 seed definitions, P20 action payloads and network projections remain outside this component-only session.
crossSubsystemOwner: integration-review
~~~

## 1. Scope and status

This document covers only the ten leaf subsystems under the formal parent
WorldGenerationAndEcology in the P19 authoritative report: execution lifecycle, generation
progress and pass state, controller control, generator execution, snapshots, manifests and pass
results, world-generation option definitions, the option registry, and three support types.
The authoritative inventory is 47 fields plus 37 properties, for 84 members.

This is a proposed decomposition and migration input. It is not a production implementation,
behavior-equivalence claim, API-compatibility closure, network closure, or persistence closure.
The 13 component state types listed in the current session verification are implemented in the
root `src` project. Deferred systems, queries, commands, adapters, projections, ports, paths and
interfaces remain proposed; existing NLTX files are evidence of partial coverage only.

All 13 authorized component-owned P19 state types have been written under `src` and the affected
WorldSession project compiles. Controller pass state, snapshot state, option registry and support
types remain non-component adapter/query/projection boundaries and are not created as
placeholders. The two P19 documents are updated together after each component checkpoint; a
checkpoint is not considered complete until both files contain the same ledger metadata.

## Current session verification

- Runner Handoff: `status=handed-off`, `partition=P19`, new session
  `ca556245d09847cab6264716801a8095`, exit code `0`.
- Serial build: `pwsh -NoProfile -File .\\Build\\Tools\\Invoke-SerialDotnet.ps1 build .\\src\\WorldSession\\Terraria.WorldSession.csproj /m:1 /nr:false /p:UseSharedCompilation=false /p:MSBuildNodeReuse=false /p:BuildInParallel=false`; exit code `0`, 0 warnings, 0 errors; artifact `Build/bin/Terraria.WorldSession/Debug/net10.0/Terraria.WorldSession.dll`.
- Existing focused verifier build: `pwsh -NoProfile -File .\\Build\\Tools\\Invoke-SerialDotnet.ps1 build .\\src\\WorldSessionFocusedVerifier\\Terraria.WorldSessionFocusedVerifier.csproj /m:1 /nr:false /p:UseSharedCompilation=false /p:MSBuildNodeReuse=false /p:BuildInParallel=false`; exit code `0`, 0 warnings, 0 errors; artifact `Build/bin/Terraria.WorldSessionFocusedVerifier/Debug/net10.0/Terraria.WorldSessionFocusedVerifier.dll`.
- Existing focused verifier run: `pwsh -NoProfile -File .\\Build\\Tools\\Invoke-SerialDotnet.ps1 run --project .\\src\\WorldSessionFocusedVerifier\\Terraria.WorldSessionFocusedVerifier.csproj --no-build --no-restore`; exit code `0`, eight existing assertions reported passed.
- P19-specific lifecycle, progress, control, snapshot, manifest, option-registry and support behavior verification: not available in the existing verifier and not run. This evidence does not establish full P19 behavior equivalence, network closure, persistence closure or integration completion.
- Runner Complete: `status=completed`, `partition=P19`, `sessionId=ca556245d09847cab6264716801a8095`, exit code `0`, lock released.

## 2. Evidence register

| Source | Evidence used | Fact supported | evidenceStatus |
|---|---|---|---|
| Version4 | D:\TRbackup\Version4\Terraria\WorldGen.cs:6267-6306 | asynchronous world-generation entry, lifecycle flags, callback and completion boundary | confirmed |
| Version4 | D:\TRbackup\Version4\Terraria\WorldGen.cs:10108-10147 | GenerateWorld sets execution flags, creates WorldGenerator, clears/resets/adds/disables passes, runs the generator, restores temporary state in finally | confirmed |
| Version4 | D:\TRbackup\Version4\Terraria\WorldGen.cs:10553 and 21674-21712 | pass registration, special-seed pass disabling, temporary state restoration and finish side effects | confirmed |
| Version4 | D:\TRbackup\Version4\Terraria.WorldBuilding\WorldGenerator.cs:1-589 | controller, pause/abort control, pass execution loop, control lock, progress bridge, pass results and world hash | confirmed; Version4 has stubs in selected methods |
| Full reference supplement | D:\TRbackup\无任何删减通过编译\Terraria.WorldBuilding\WorldGenerator.cs:1-589 | complete reference behavior for RunPass and controller lifecycle where the Version4 file is stubbed | full-reference-supplemented; not a replacement for Version4 coverage |
| Version4 | D:\TRbackup\Version4\Terraria.WorldBuilding\GenerationProgress.cs, GenPass.cs, GenPassResult.cs, WorldManifest.cs | progress formatting/clamping, pass definition/enable state, result fields and manifest metadata | confirmed |
| Version4 | D:\TRbackup\Version4\Terraria.WorldBuilding\WorldGenSnapshot.cs | snapshot metadata, GenVars reflection schema, matching passes, outdated check and snapshot file boundary | confirmed; restore implementation supplemented from full reference |
| Version4 | D:\TRbackup\Version4\Terraria.WorldBuilding\AWorldGenerationOption.cs, WorldGenerationOptions.cs, WorldGenConfiguration.cs | option enable transitions, static option catalog, configuration JSON roots and read-only option definitions | confirmed |
| Version4 | D:\TRbackup\Version4\Terraria.WorldBuilding\GenAction.cs and Terraria\WorldGen.cs:4009-4014 | action-chain support fields and tree-ground validation delegate boundary | confirmed; neighboring terrain/action ownership is outside P19 |
| Version4 | D:\TRbackup\Version4\Terraria.WorldGen.cs:4077, 4151 | WorldGen manifest and generating/loading lifecycle projection points | confirmed |
| tModLoader API mirror | D:\TRbackup\tmodloader-api-docs-stable\index.html, class_mod_system.html and class_gen_pass.html, tModLoader v2026.07 | public extension boundaries for PreWorldGen, ModifyWorldGenTasks, PostWorldGen, GenPass.Apply, Disable, Name, Weight and Enabled | confirmed-public-boundary; does not prove private Version4 ordering |
| SS14 organization reference | C:\Users\shan\Downloads\ECS\space-station-14-master\Content.Shared\Atmos\Components and Content.Server\Atmos\EntitySystems | map-level component ownership, system-owned mutation, command entry and derived projection organization | organization-only |
| Current NLTX | D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\WorldGeneration | typed world-generation state, cursor, pass snapshots, command batch, pipeline and trace exist, but the P19 controller/snapshot/options closure is partial | existing-evidence |
| Current NLTX | D:\TRbackup\NLTX\dome\Test\Terraria.Dome.GenerationCursor.Verification\Program.cs, Terraria.Dome.GenerationCommand.Verification\Program.cs, Terraria.Dome.WorldGeneration.Verification\Program.cs | focused verifier entry points exist | existing-evidence; not run in this session |

The full reference tree is used only to fill evidence gaps where the Version4 file contains a
stub. It cannot promote a Version4 behavior claim to confirmed when the Version4 call path or
integration boundary remains incomplete.

## 3. Boundary decision

P19 is an execution/control and persistence-boundary slice, not a second owner for world facts.
The proposed direction is:

~~~text
generation request and external rules
  -> WorldGenerationLifecycleSystem
  -> pass definitions and enabled-pass selection
  -> WorldGenerationPassExecutionSystem
  -> progress, pass result and deterministic hash projections
  -> snapshot/manifest persistence adapters
  -> finish and WorldFile integration
~~~

The authoritative runtime state is kept in narrow capability boundaries:

- lifecycle and run-scoped execution flags belong to the execution system;
- progress and pass availability are distinct because progress is mutable telemetry while pass
  name/weight are definitions and Enabled is a selection decision;
- pause, hash-mismatch and abort are explicit control state, changed through commands under a
  control-gate adapter;
- snapshots and manifests are persistence projections, never an alternate authority for live
  tiles or GenVars;
- option definitions and the option registry are catalog/projection boundaries; the semantics of
  special seeds remains an integration with P18;
- reflection, file paths, localization, assets, locks, clocks, logging and UI are adapters or
  deferred effects, not fields in authoritative ECS state.

No proposed component may combine world terrain facts from P17, seed-definition facts from P18,
or action payload semantics from P20. Cross-partition reads use a query or explicit command and
are recorded as cross-subsystem findings.

## 4. Explicit system order

The scheduler must encode this order; file or directory order has no runtime meaning.

1. WorldGenerationRequestAdapter validates the generation request and receives read-only rule,
   configuration, seed and action inputs from the integration boundary.
2. WorldGenerationLifecycleSystem begins one run, creates the run identity, sets the global
   lifecycle projection, and creates the proposed execution state.
3. WorldGenerationConfigurationAdapter loads and validates biome/pass configuration without
   exposing JObject values to the core state.
4. WorldGenerationPassRegistrationSystem creates pass definitions and the ordered pass schedule.
5. WorldGenerationPassSelectionSystem applies special-seed pass enable/disable decisions through
   an explicit command; P18 owns seed meaning and P19 owns schedule application.
6. WorldGenerationProgressSystem initializes total weight and begins the current pass.
7. WorldGenerationPassExecutionSystem acquires the control gate, runs one enabled pass, emits
   progress updates, and produces a pass result.
8. WorldGenerationHashSystem computes the deterministic tile hash after the pass result boundary;
   wall-clock duration comes from a proposed clock port and is never used as authority.
9. WorldGenerationControllerSystem applies pause-after-pass, hash-mismatch, reset, abort and
   resume commands. Snapshot creation/restoration is serialized through the same control gate.
10. WorldGenerationSnapshotPersistenceAdapter and WorldManifestPersistenceAdapter serialize or
    restore projections only after the controller has established a stable pass boundary.
11. WorldGenerationFinishSystem restores temporary changes, publishes completion, clears lifecycle
    flags and hands the result to the WorldFile integration boundary.

The order between liquid settling, WorldFile loading and generation completion remains a
cross-subsystem integration risk. P19 records the boundary but does not claim ownership of the
liquid or world-file algorithms.

## 5. Checkpoint 1: WorldGenerationExecutionState

### 5.1 Proposed boundary

Proposed target paths, each with status: proposed:

- dome/src/Terraria.Dome.Simulation/WorldGeneration/Execution/WorldGenerationExecutionStateComponent.cs
- dome/src/Terraria.Dome.Simulation/WorldGeneration/Execution/WorldGenerationExecutionSystem.cs
- dome/src/Terraria.Dome.Simulation/WorldGeneration/Execution/WorldGenerationThreadExecutionAdapter.cs
- dome/src/Terraria.Dome.Simulation/WorldGeneration/Execution/WorldGenerationGeneratorHandleAdapter.cs
- dome/src/Terraria.Dome.Simulation/WorldGeneration/Execution/WorldGenerationExecutionCommand.cs

WorldGenerationExecutionStateComponent is the proposed owner of run lifecycle flags, phase
markers and generation metrics. Thread affinity and the legacy WorldGenerator object do not enter
the component as opaque global state; they cross the explicit thread and handle adapters. P17
terrain logic may calculate small-consecutive values, but only an explicit command can commit
those values to the P19 execution state. Trap-placement behavior remains a cross-subsystem
integration boundary with P20.

### 5.2 Member ownership table

| Source member | C# type | Classification | Proposed owner/seam | Readers | Writers and lifecycle | Side effect |
|---|---|---|---|---|---|---|
| generatingWorld | bool | Component | WorldGenerationExecutionStateComponent; WorldGenerationLifecycleSystem | lifecycle query, world-generation UI adapter, WorldFile integration | LifecycleSystem on CreateNewWorld/GenerateWorld start and finally completion/exception | gates external lifecycle visibility; no I/O in component |
| generatingWorldOnThisThread | bool | Adapter | WorldGenerationThreadExecutionAdapter | pass execution and thread-affinity query | ExecutionSystem enters/leaves the worker scope; cleared in finally | thread-local effect boundary |
| _generator | WorldGenerator | Adapter | WorldGenerationGeneratorHandleAdapter | controller and execution adapters | LifecycleSystem creates and releases a handle per run | retains legacy object; must not become ECS authority |
| SmallConsecutivesFound | int | Component plus Command input | WorldGenerationExecutionStateComponent; WorldGenerationMetricCommitSystem | diagnostics and integration query | explicit metric command from terrain pass; reset at run start | diagnostic projection only; calculation belongs to P17 review |
| SmallConsecutivesEliminated | int | Component plus Command input | WorldGenerationExecutionStateComponent; WorldGenerationMetricCommitSystem | diagnostics and integration query | explicit metric command from terrain pass; reset at run start | diagnostic projection only; calculation belongs to P17 review |
| placingTraps | bool | Component plus Command | WorldGenerationExecutionStateComponent; WorldGenerationPhaseControlSystem | pass qualification and trap integration query | phase command around trap placement; cleared on pass completion/finally | controls phase visibility; trap effects belong to P20/integration review |

### 5.3 Invariants and evidence gap

- generatingWorld and generatingWorldOnThisThread cannot be cleared before the callback has
  completed its finally path, including exception and abort paths.
- _generator is a per-run handle and must not be shared as a static mutable ECS component.
- metrics are reset on a new run and cannot be updated by a query or by a persistence adapter.
- placingTraps is a phase flag, not a definition of trap behavior or trap action payload.
- The current NLTX WorldGenerationStateComponent and lifecycle snapshot cover generation identity,
  stage and completion, but do not prove this complete Version4 execution/controller mapping;
  currentNltxStatus therefore remains partial.

## 6. Initial allocation register

The following inventory is the fixed P19 scope. Component-owned state rows are implemented;
non-component adapter, query and projection boundaries remain planned and deferred.

| Leaf subsystem | Fields | Properties | Total | Planned capability boundary | Checkpoint status |
|---|---:|---:|---:|---|---|
| WorldGenerationExecutionState | 6 | 0 | 6 | execution lifecycle, thread and phase state | component implemented; thread/handle adapters deferred |
| WorldGenerationProgressAndPassState | 7 | 6 | 13 | progress component, pass definition and availability | progress component implemented; selection pending |
| WorldGenerationControllerPassState | 4 | 3 | 7 | pass controller and checkpoint index | no component-owned state; query/adapter deferred |
| WorldGenerationControllerPauseAndHashState | 1 | 6 | 7 | control state and explicit control commands | component state implemented; command/query/adapter deferred |
| WorldGenerationGeneratorExecutionState | 10 | 1 | 11 | generator run context and execution cursor | component state implemented; adapter/projection deferred |
| WorldGenerationSnapshotState | 7 | 6 | 13 | snapshot persistence adapter and read-only query | no component-owned state; persistence/query deferred |
| WorldGenerationManifestAndPassResults | 2 | 6 | 8 | manifest/result persistence projection | component metadata implemented; persistence deferred |
| WorldGenerationOptionBaseState | 4 | 8 | 12 | option definition, selection state and client projection | selection component implemented; definition/presentation deferred |
| WorldGenerationOptionRegistry | 2 | 1 | 3 | catalog registry adapter and read-only enumeration | no component-owned state; registry deferred |
| WorldGenerationSupportTypes | 4 | 0 | 4 | action-chain and tree-validation support seams | no component-owned state; P17/P20 seams deferred |

### 6.1 Implemented component paths

The following component-owned state has been written under `src/WorldSession/WorldGeneration`:

| Component | Source path | State represented | Deferred dependency impact |
|---|---|---|---|
| WorldGenerationExecutionStateComponent | `Execution/WorldGenerationExecutionStateComponent.cs` | lifecycle flag, consecutive metrics and trap phase flag | lifecycle system, P17 metric commit and P20 trap integration |
| WorldGenerationProgressComponent | `Progress/WorldGenerationProgressComponent.cs` | message, clamped value, weighted progress and total progress | progress system, query and presentation projection |
| WorldGenerationPassSelectionComponent | `Progress/WorldGenerationPassSelectionComponent.cs` | stable pass-id to enabled-state selection | pass definition, special-seed selection and scheduler |
| WorldGenerationControlStateComponent | `Controller/WorldGenerationControlStateComponent.cs` | pause intent and pause-after-pass identity | control command/system and lock adapter |
| WorldGenerationHashMismatchPolicyComponent | `Controller/WorldGenerationHashMismatchPolicyComponent.cs` | hash-mismatch pause policy | deterministic hash query and control system |
| WorldGenerationHashMismatchStateComponent | `Controller/WorldGenerationHashMismatchStateComponent.cs` | hash-mismatch pause cause | hash mismatch system and resume command |
| WorldGenerationSnapshotPolicyComponent | `Controller/WorldGenerationSnapshotPolicyComponent.cs` | validated snapshot frequency | snapshot scheduler and persistence adapter |
| WorldGenerationAbortStateComponent | `Controller/WorldGenerationAbortStateComponent.cs` | queued abort intent | abort command and finish system |
| WorldGenerationGeneratorExecutionStateComponent | `Execution/WorldGenerationGeneratorExecutionStateComponent.cs` | run seed | deterministic random adapter and pass executor |
| WorldGenerationExecutionCursorComponent | `Execution/WorldGenerationExecutionCursorComponent.cs` | current pass identity | pass execution system and controller query |
| WorldGenerationPassResultComponent | `Persistence/WorldGenerationPassResultComponent.cs` | one pass duration, hash and skipped state | ordered result commit and manifest projection |
| WorldGenerationManifestComponent | `Persistence/WorldGenerationManifestComponent.cs` | version and Git SHA metadata | manifest persistence and WorldFile boundary |
| WorldGenerationOptionSelectionComponent | `Options/WorldGenerationOptionSelectionComponent.cs` | option Enabled and AutoGenEnabled state | option system, P18 seed mapping and presentation projection |

No component was created for controller pass projections, snapshot bytes/paths, JSON settings,
option definitions/registry, action-chain support, tree validation, thread-local state, the
legacy `WorldGenerator` handle, or other adapter/query/projection members.

### Planned member names for later checkpoints

- WorldGenerationProgressAndPassState: _message, _value, _totalWeightedProgress, TotalWeight,
  CurrentPassWeight, Name, Weight, Message, MessageNoFormatting, Value, TotalWeightedProgress,
  TotalProgress, Enabled.
- WorldGenerationControllerPassState: _previousManifest, _snapshots, OnPassesLoaded, _generator,
  Passes, CurrentPass, LastCompletedPass.
- WorldGenerationControllerPauseAndHashState: _paused, PauseAfterPass, PauseOnHashMismatch,
  PausedDueToHashMismatch, SnapshotFrequency, Paused, QueuedAbort.
- WorldGenerationGeneratorExecutionState: _passes, _seed, _configuration, _progress, _controller,
  _controlLock, _currentPass, CurrentGenerationProgress, CurrentController, _hashTime, PassResults.
- WorldGenerationSnapshotState: SerializerSettings, fieldsAndProperties, _dataOffset,
  _matchingPasses, SnapshotFolderSuffix, Extension, _snapshotSizeCache, Manifest, Path, GenVarsJson,
  GenPassResults, Outdated, PathForActiveWorld.
- WorldGenerationManifestAndPassResults: GenPassResults, SerializerSettings, DurationMs, Hash,
  Skipped, Version, GitSHA, FinalHash.
- WorldGenerationOptionBaseState: _enabled, AutoGenEnabled, _biomeRoot, _passRoot, Enabled,
  KeyName, ServerConfigName, SpecialSeedNames, SpecialSeedValues, Description, Title, Texture.
- WorldGenerationOptionRegistry: _options, _powerPermissionsLineHeader, Options.
- WorldGenerationSupportTypes: NextAction, OutputData, Instance, IsGroundValid.

## 7. Checkpoint 2: WorldGenerationProgressAndPassState

### 7.1 Proposed boundary

Proposed target paths, each with status: proposed:

- dome/src/Terraria.Dome.Simulation/WorldGeneration/Progress/WorldGenerationProgressComponent.cs
- dome/src/Terraria.Dome.Simulation/WorldGeneration/Progress/WorldGenerationProgressSystem.cs
- dome/src/Terraria.Dome.Simulation/WorldGeneration/Progress/WorldGenerationPassDefinition.cs
- dome/src/Terraria.Dome.Simulation/WorldGeneration/Progress/WorldGenerationPassSelectionSystem.cs
- dome/src/Terraria.Dome.Simulation/WorldGeneration/Progress/WorldGenerationProgressProjection.cs

Progress telemetry and pass definitions are deliberately separate concepts. The proposed
WorldGenerationProgressComponent owns the current pass value, weighted totals and message
template. WorldGenerationPassDefinition owns Name and Weight as immutable inputs for a run.
Enabled is a selection result owned by WorldGenerationPassSelectionSystem; it is consumed by the
execution scheduler and projected to diagnostics. No progress query may mutate the component.

### 7.2 Member ownership table

| Source member | C# type | Classification | Proposed owner/seam | Readers | Writers and lifecycle | Side effect |
|---|---|---|---|---|---|---|
| GenerationProgress._message | string | Component state | WorldGenerationProgressComponent | progress projection and pass UI adapter | ProgressSystem sets formatted template through explicit progress command; reset at run start | localization/UI projection only |
| GenerationProgress._value | double | Component state | WorldGenerationProgressComponent | TotalProgress query and UI projection | ProgressSystem clamps every update to [0,1]; reset at pass start/end | none in core |
| GenerationProgress._totalWeightedProgress | double | Component state | WorldGenerationProgressComponent | TotalProgress query and pause diagnostics | ProgressSystem updates after completed enabled passes | none in core |
| GenerationProgress.TotalWeight | double | Component state | WorldGenerationProgressComponent | weighted progress query and execution scheduler | PassSelection/ProgressSystem computes enabled-pass sum at run start and reset | none in core |
| GenerationProgress.CurrentPassWeight | double | Component state | WorldGenerationProgressComponent | TotalProgress query | ProgressSystem sets from the current enabled pass before execution | none in core |
| GenPass.Name | string | Definition | WorldGenerationPassDefinition | pass selection, diagnostics, public hook adapter | pass registration creates it once per run; no runtime mutation | localization/diagnostic label at adapter |
| GenPass.Weight | double | Definition | WorldGenerationPassDefinition | total-weight calculation and manifest/result diagnostics | pass registration creates it; configuration/hook adapter supplies input | none in core |
| GenerationProgress.Message | string | Derived property | WorldGenerationProgressQuery and presentation projection | UI/status consumers | read formats _message with clamped Value; write becomes an explicit SetMessage command | formatting and localization boundary |
| GenerationProgress.MessageNoFormatting | string | Compatibility projection | WorldGenerationProgressQuery/LegacyProgressAdapter | legacy UI and diagnostics | read/write routed through explicit message command; cannot bypass owner | legacy API compatibility |
| GenerationProgress.Value | double | Derived read plus command input | WorldGenerationProgressQuery and ProgressSystem | pass implementation, UI and scheduler | setter semantics become a clamping command handled by ProgressSystem | no I/O |
| GenerationProgress.TotalWeightedProgress | double | Command-backed state | WorldGenerationProgressSystem | TotalProgress query and Controller projection | only ProgressSystem writes after pass completion/reset | none in core |
| GenerationProgress.TotalProgress | double | Pure query | WorldGenerationProgressQuery | UI, diagnostics and controller progress bridge | recomputes from Value, CurrentPassWeight, total weighted progress and TotalWeight; no writer | none |
| GenPass.Enabled | bool | Selection state | WorldGenerationPassSelectionComponent/PassSelectionSystem | execution loop, weight query, snapshot matching and public hook adapter | selection system initializes true and applies explicit Disable command for special seeds | affects scheduled work and downstream snapshot compatibility |

### 7.3 Invariants and evidence gap

- Message formatting must preserve the Version4 percent-template behavior without placing
  localization or UI calls in the component.
- Value is always clamped to [0,1]; TotalProgress returns zero when TotalWeight is zero.
- TotalWeight includes only enabled passes, while TotalWeightedProgress includes completed
  enabled passes; disabled passes do not silently contribute weight.
- Name and Weight remain pass-definition inputs and are not writable progress telemetry.
- Enabled is not inferred from file order; the selection system records the explicit disable reason
  and emits the schedule used by the execution system.
- Current NLTX has world-generation stages, snapshots and traces, but no proven Version4
  progress/pass selection closure. This checkpoint therefore remains proposed with
  currentNltxStatus: partial.

## 8. Checkpoint 3: WorldGenerationControllerPassState

### 8.1 Proposed boundary

Proposed target paths, each with status: proposed:

- dome/src/Terraria.Dome.Simulation/WorldGeneration/Controller/WorldGenerationControllerPassStateComponent.cs
- dome/src/Terraria.Dome.Simulation/WorldGeneration/Controller/WorldGenerationControllerSystem.cs
- dome/src/Terraria.Dome.Simulation/WorldGeneration/Controller/WorldGenerationPassCheckpointQuery.cs
- dome/src/Terraria.Dome.Simulation/WorldGeneration/Persistence/WorldGenerationSnapshotIndexAdapter.cs

This component owns the controller pass-facing view and the run-scoped index that associates
completed passes with snapshots. It does not own snapshot bytes, file I/O, pass behavior, or the
legacy generator object. Passes, CurrentPass and LastCompletedPass are projections or query
results over the explicit pass schedule and result cursor.

### 8.2 Member ownership table

| Source member | C# type | Classification | Proposed owner/seam | Readers | Writers and lifecycle | Side effect |
|---|---|---|---|---|---|---|
| Controller._previousManifest | WorldManifest | Projection input | WorldGenerationPreviousManifestProjection/Adapter | snapshot matching and reset controller | ControllerSystem captures a read-only manifest projection before reset; replaced per run | persistence metadata read, no live-world write |
| Controller._snapshots | Dictionary<GenPass, WorldGenSnapshot> | Registry/cache adapter | WorldGenerationSnapshotIndexAdapter | controller snapshot lookup and deletion | SnapshotPersistenceAdapter inserts/removes under control gate; cleared at run end | file deletion/creation delegated to persistence port |
| Controller.OnPassesLoaded | Action<Controller> | Event/compatibility adapter | WorldGenerationPassScheduleLoadedEventAdapter | legacy debug UI and external hook bridge | PassRegistrationSystem emits one event after schedule construction | callback/UI side effect at adapter |
| Controller._generator | WorldGenerator | Adapter handle | WorldGenerationGeneratorHandleAdapter | controller pass projection and execution bridge | LifecycleSystem binds the current run handle; unbound after finalization | legacy object access isolated behind port |
| Controller.Passes | List<GenPass> | Query/projection | WorldGenerationPassScheduleQuery | controller/UI, snapshot matcher and public hook adapter | read-only view of pass definitions and selection state; no direct writer | exposes compatibility view only |
| Controller.CurrentPass | GenPass | Derived query | WorldGenerationCurrentPassQuery | progress/UI, controller commands and diagnostics | derived from execution cursor while a pass is active; cleared at boundary | none |
| Controller.LastCompletedPass | GenPass | Derived query | WorldGenerationLastCompletedPassQuery | reset/snapshot selection and diagnostics | derived from committed result count and schedule; no direct writer | none |

### 8.3 Invariants and evidence gap

- The snapshot index is keyed by a stable pass identity for the run; equality and schedule
  revision must be explicit rather than relying on mutable object identity.
- LastCompletedPass cannot advance before the corresponding PassResult commit, and CurrentPass
  must be cleared after the pass boundary even on failure or abort.
- OnPassesLoaded is an event seam, not a hidden second writer for schedule state.
- _previousManifest is read-only input to matching/reset decisions; it cannot overwrite the
  current manifest without an explicit restore command.
- Version4 has a static Controller bridge and selected stubs in the P19 reference path. The full
  reference supplements call behavior, but the NLTX controller/snapshot index is not closed;
  currentNltxStatus: partial remains required.

## 9. Checkpoint 4: WorldGenerationControllerPauseAndHashState

### 9.1 Proposed boundary

Proposed target paths, each with status: proposed:

- dome/src/Terraria.Dome.Simulation/WorldGeneration/Controller/WorldGenerationControlStateComponent.cs
- dome/src/Terraria.Dome.Simulation/WorldGeneration/Controller/WorldGenerationControlCommand.cs
- dome/src/Terraria.Dome.Simulation/WorldGeneration/Controller/WorldGenerationControlSystem.cs
- dome/src/Terraria.Dome.Simulation/WorldGeneration/Controller/WorldGenerationHashQuery.cs
- dome/src/Terraria.Dome.Simulation/WorldGeneration/Controller/WorldGenerationControlLockAdapter.cs

The control component stores pause intent and observable control state. Commands express pause,
resume, pause-after-pass, snapshot-frequency and abort requests. The lock adapter serializes
controller operations with pass execution; it is an effect port, not component data. HashWorld is
a deterministic query over a stable world snapshot, while timing, UI, sleep and logging remain
adapters.

### 9.2 Member ownership table

| Source member | C# type | Classification | Proposed owner/seam | Readers | Writers and lifecycle | Side effect |
|---|---|---|---|---|---|---|
| Controller._paused | bool | Component state | WorldGenerationControlStateComponent | execution loop and control query | ControlSystem commits Pause/Resume; reset at run start and finalization | controls scheduling |
| Controller.PauseAfterPass | GenPass | Command-backed control state | WorldGenerationControlStateComponent and PauseAfterPassCommand | execution loop and diagnostics | ControlSystem sets or clears by stable pass identity; setting Paused clears it | changes future scheduling |
| Controller.PauseOnHashMismatch | bool | Policy state | WorldGenerationHashMismatchPolicyComponent | result/control systems | initialized from run policy; changed only by explicit policy command | chooses failure response |
| Controller.PausedDueToHashMismatch | bool | Derived control state | WorldGenerationHashMismatchStateComponent | UI/diagnostic query and resume gate | HashMismatchSystem sets when a mismatch pauses the run; Resume clears | notification/projection |
| Controller.SnapshotFrequency | SnapshotFrequency | Policy/command state | WorldGenerationSnapshotPolicyComponent | snapshot scheduler and controller | ControlSystem sets from explicit command; default None per run | triggers persistence adapter |
| Controller.Paused | bool | Compatibility property | WorldGenerationControlQuery/LegacyControllerAdapter | legacy controller clients | get reads state; set becomes Pause/Resume command with Version4 clearing semantics | legacy API bridge |
| Controller.QueuedAbort | bool | Command-backed terminal intent | WorldGenerationAbortStateComponent | execution loop and finish system | Abort command sets once; terminal commit consumes and clears at run end | stops future pass execution |

### 9.3 Control sequence and evidence gap

1. A control command enters the control system and attempts the lock adapter.
2. Pause or abort is committed before the next pass boundary; a current pass is not mutated by a
   query.
3. On pass completion, the hash query runs against the committed world, then the mismatch system
   may restore a previous snapshot, set Paused and set PausedDueToHashMismatch.
4. Snapshot creation/deletion/reset and restore are executed under the same control gate.
5. UI visibility, sleep, chat, logging and main-thread section refresh are emitted after state
   commit through adapters.

Hash mismatch, pause-after-pass and queued abort have different causes and must not collapse into
one boolean. The Version4 control lock and full-reference OnPassCompleted behavior are evidenced,
but current NLTX has no complete control/snapshot integration; currentNltxStatus remains partial.

## 10. Checkpoint 5: WorldGenerationGeneratorExecutionState

### 10.1 Proposed boundary

Proposed target paths, each with status: proposed:

- dome/src/Terraria.Dome.Simulation/WorldGeneration/Execution/WorldGenerationGeneratorExecutionStateComponent.cs
- dome/src/Terraria.Dome.Simulation/WorldGeneration/Execution/WorldGenerationPassExecutionSystem.cs
- dome/src/Terraria.Dome.Simulation/WorldGeneration/Execution/WorldGenerationRunContext.cs
- dome/src/Terraria.Dome.Simulation/WorldGeneration/Execution/WorldGenerationStaticBridgeProjection.cs
- dome/src/Terraria.Dome.Simulation/WorldGeneration/Execution/WorldGenerationHashTimingAdapter.cs

The proposed run context contains only stable seed/configuration references and execution
identifiers. Pass schedule and progress are read through their explicit boundaries, control lock
is an adapter, and PassResults is a projection into the Manifest result owner. Static
CurrentGenerationProgress and CurrentController are compatibility projections with one active run
at a time; they are not global mutable component authority.

### 10.2 Member ownership table

| Source member | C# type | Classification | Proposed owner/seam | Readers | Writers and lifecycle | Side effect |
|---|---|---|---|---|---|---|
| WorldGenerator._passes | List<GenPass> | Schedule reference | WorldGenerationPassScheduleQuery/RunContextAdapter | pass executor and controller projections | PassRegistrationSystem appends definitions before execution; no duplicate component copy | legacy list compatibility |
| WorldGenerator._seed | int | Run definition | WorldGenerationRunContextComponent | deterministic random adapter and pass executor | LifecycleSystem sets once per run; immutable until run end | seeds random stream |
| WorldGenerator._configuration | WorldGenConfiguration | Configuration adapter | WorldGenerationConfigurationAdapter | pass executor | configuration adapter loads once and exposes typed pass config | JSON/file access outside core |
| WorldGenerator._progress | GenerationProgress | State handle/projection | WorldGenerationProgressComponent through ProgressAdapter | pass executor and public progress bridge | ProgressSystem owns values; handle cleared at finalization | UI/legacy projection |
| WorldGenerator._controller | Controller | Control handle | WorldGenerationControllerHandleAdapter | executor and controller system | LifecycleSystem binds one controller per run; released at finalization | legacy API access |
| WorldGenerator._controlLock | object | Synchronization adapter | WorldGenerationControlLockAdapter | pass executor and controller commands | adapter owns lock lifetime and try-operate semantics | synchronization effect |
| WorldGenerator._currentPass | GenPass | Execution cursor | WorldGenerationExecutionCursorComponent | controller query, progress and diagnostics | PassExecutionSystem sets before RunPass and clears in finally | drives schedule |
| WorldGenerator.CurrentGenerationProgress | GenerationProgress | Static compatibility projection | WorldGenerationStaticBridgeProjection | public hooks and diagnostics | bridge publishes active progress at start and null at finalization | global compatibility visibility |
| WorldGenerator.CurrentController | Controller | Static compatibility projection | WorldGenerationStaticBridgeProjection | public hooks and diagnostics | bridge publishes active controller at start and null at finalization | global compatibility visibility |
| WorldGenerator._hashTime | Stopwatch | Timing adapter/cache | WorldGenerationHashTimingAdapter | diagnostics and snapshot cadence query | HashSystem starts/stops/reset through injected clock/timer port | wall-clock measurement |
| WorldGenerator.PassResults | List<GenPassResult> | Result projection | WorldGenerationPassResultProjection | controller, manifest persistence and diagnostics | ResultCommitSystem appends once after RunPass; Manifest owner persists | persistence/trace projection |

### 10.3 Execution contract and evidence gap

- PassExecutionSystem owns the one transition from current pass to committed result and clears the
  cursor on all exit paths.
- A pass receives typed configuration and a deterministic random stream; it cannot write the
  controller, manifest or persistence adapter directly.
- Static bridge values are published only for legacy compatibility and are cleared even on abort or
  exception. They cannot be queried as authoritative ECS state.
- Duration is diagnostic metadata from an injected timing port. It does not affect pass order,
  hash authority or retry behavior.
- The Version4 execution loop and hash algorithm are confirmed; RunPass and controller details are
  full-reference-supplemented where Version4 is stubbed. Current NLTX has a pipeline and pass
  snapshots but not this complete generator closure, so currentNltxStatus remains partial.

## 11. Checkpoint 6: WorldGenerationSnapshotState

### 11.1 Proposed boundary

Proposed target paths, each with status: proposed:

- dome/src/Terraria.Dome.Simulation/WorldGeneration/Persistence/WorldGenerationSnapshotDto.cs
- dome/src/Terraria.Dome.Simulation/WorldGeneration/Persistence/WorldGenerationSnapshotPersistenceAdapter.cs
- dome/src/Terraria.Dome.Simulation/WorldGeneration/Persistence/WorldGenerationSnapshotConsistencyQuery.cs
- dome/src/Terraria.Dome.Simulation/WorldGeneration/Persistence/WorldGenerationGenVarsProjectionAdapter.cs
- dome/src/Terraria.Dome.Simulation/WorldGeneration/Persistence/WorldGenerationSnapshotRestoreCommand.cs

Snapshot bytes and serialized GenVars are persistence projections. The live world, generation
state and pass schedule remain authoritative elsewhere. Serializer settings, reflection member
selection, path construction, offsets and file-size caches remain inside adapters. Outdated is a
pure consistency query and cannot write back to the live world.

### 11.2 Member ownership table

| Source member | C# type | Classification | Proposed owner/seam | Readers | Writers and lifecycle | Side effect |
|---|---|---|---|---|---|---|
| SnapshotGenVars.SerializerSettings | JsonSerializerSettings | Persistence adapter configuration | WorldGenerationGenVarsProjectionAdapter | GenVars serializer | adapter initializes immutable settings per serializer revision | JSON serialization |
| SnapshotGenVars.fieldsAndProperties | Dictionary<string, MemberInfo> | Reflection schema cache | WorldGenerationGenVarsProjectionAdapter | GenVars serializer/deserializer | adapter builds from allowed public writable/static members; invalidated by schema revision | reflection and compatibility |
| WorldGenSnapshot._dataOffset | int | Persistence metadata | WorldGenerationSnapshotDto/Adapter | tile payload loader/resaver | adapter records offset after manifest and GenVars payload; recalculated on resave | binary file positioning |
| WorldGenSnapshot._matchingPasses | List<GenPass> | Consistency cache | WorldGenerationSnapshotConsistencyQuery | Outdated and history matcher | built when snapshot is created/loaded against the current schedule | no live-world mutation |
| WorldGenSnapshot.SnapshotFolderSuffix | string | Persistence definition | WorldGenerationSnapshotPathAdapter | path resolver and cleanup | immutable format definition | filesystem path construction |
| WorldGenSnapshot.Extension | string | Persistence definition | WorldGenerationSnapshotPathAdapter | reader/enumerator and cleanup | immutable format definition | filesystem path construction |
| WorldGenSnapshot._snapshotSizeCache | IDictionary<string,long> | Diagnostic cache | WorldGenerationSnapshotSizeCacheAdapter | disk-usage diagnostics | adapter updates on create/read/resave/delete and clears per world | filesystem metadata |
| WorldGenSnapshot.Manifest | WorldManifest | Serialized projection | WorldGenerationSnapshotDto | history match and restore command | snapshot adapter clones the current manifest; restore emits a command, not a direct write | persistence |
| WorldGenSnapshot.Path | string | Adapter state | WorldGenerationSnapshotPathAdapter | read/delete/resave operations | adapter assigns validated path during create/read | file I/O |
| WorldGenSnapshot.GenVarsJson | string | Serialized projection payload | WorldGenerationGenVarsProjectionAdapter | restore and resave | adapter serializes at stable checkpoint and validates before restore | reflection/JSON |
| WorldGenSnapshot.GenPassResults | List<GenPassResult> | Read-only projection | WorldGenerationSnapshotResultQuery | controller and consistency checks | derives from snapshot Manifest; no independent writer | none |
| WorldGenSnapshot.Outdated | bool | Pure query | WorldGenerationSnapshotConsistencyQuery | controller snapshot selection | compares version, GitSHA, pass enablement and result history; no writer | none |
| WorldGenSnapshot.PathForActiveWorld | string | Derived adapter query | WorldGenerationSnapshotPathAdapter | load/create/delete-all | derives from active world path and suffix; no component state | filesystem path construction |

### 11.3 Persistence sequence and evidence gap

1. At a stable pass boundary, the adapter clones Manifest, serializes eligible GenVars and records
   the matching pass schedule.
2. It writes manifest text, GenVars JSON and tile snapshot bytes through an explicit storage port,
   recording the data offset only in the DTO/adapter.
3. Load validates the file, manifest history, pass names, version/GitSHA and enabled/skipped
   correspondence before indexing the snapshot.
4. Restore obtains the tile payload, resets temporary generation state through a command, applies
   validated projections and emits NPC/world cleanup effects through adapters.
5. Resave updates format metadata and serialized projections while preserving tile payload bytes.

Version4 confirms the file layout, active-world path, manifest/GenVars/tile restore sequence and
outdated checks; JSON converter behavior, snapshot history loading and resave are supplemented by
the complete reference. Current NLTX has stage snapshots but no proven WorldGenSnapshot file
closure, so currentNltxStatus remains partial.

## 12. Checkpoint 7: WorldGenerationManifestAndPassResults

### 12.1 Proposed boundary

Proposed target paths, each with status: proposed:

- dome/src/Terraria.Dome.Simulation/WorldGeneration/Persistence/WorldGenerationManifestProjection.cs
- dome/src/Terraria.Dome.Simulation/WorldGeneration/Persistence/WorldGenerationPassResultComponent.cs
- dome/src/Terraria.Dome.Simulation/WorldGeneration/Persistence/WorldGenerationPassResultCommitSystem.cs
- dome/src/Terraria.Dome.Simulation/WorldGeneration/Persistence/WorldGenerationManifestPersistenceAdapter.cs
- dome/src/Terraria.Dome.Simulation/WorldGeneration/Persistence/WorldGenerationManifestQuery.cs

The result commit system is the sole writer for the ordered pass-result projection. The Manifest
projection adapter owns serialization and WorldFile transfer; Manifest metadata is not copied into
live world authority. Version and GitSHA identify the generation algorithm/configuration revision;
FinalHash is derived from the last committed result and is not independently writable.

### 12.2 Member ownership table

| Source member | C# type | Classification | Proposed owner/seam | Readers | Writers and lifecycle | Side effect |
|---|---|---|---|---|---|---|
| WorldManifest.GenPassResults | List<GenPassResult> | Ordered result projection | WorldGenerationPassResultProjection/Component | controller, snapshot consistency, WorldFile adapter | PassResultCommitSystem appends exactly once after each pass boundary; reset at run start | persistence/diagnostic output |
| WorldManifest.SerializerSettings | JsonSerializerSettings | Persistence adapter configuration | WorldGenerationManifestPersistenceAdapter | Serialize, Deserialize and Clone adapter | immutable serializer revision; no runtime component writer | JSON serialization |
| GenPassResult.DurationMs | int | Diagnostic result data | WorldGenerationPassResultComponent | progress diagnostics, snapshot cadence and trace | ResultCommitSystem records injected clock duration after pass exit | clock/diagnostic effect |
| GenPassResult.Hash | uint? | Determinism/integrity result data | WorldGenerationPassResultComponent | mismatch query, snapshot and manifest adapter | HashSystem writes after stable pass completion when debugging/hash policy enables it | deterministic world hash |
| GenPassResult.Skipped | bool | Result/selection projection | WorldGenerationPassResultComponent | schedule history, snapshot compatibility and diagnostics | PassExecutionSystem sets from pass Enabled state; immutable after commit | affects persistence matching |
| WorldManifest.Version | string | Persistence identity | WorldGenerationManifestComponent | load/mismatch query and WorldFile adapter | ManifestSystem captures current game version at run reset/start | external version metadata |
| WorldManifest.GitSHA | string | Persistence identity | WorldGenerationManifestComponent | load/mismatch query and snapshot history | ManifestSystem captures source revision at run reset/start | external build metadata |
| WorldManifest.FinalHash | uint? | Derived property | WorldGenerationManifestQuery | snapshot creation, debug control and load validation | query returns last GenPassResult.Hash or null; never directly written | none |

### 12.3 Invariants and evidence gap

- Result order is the execution order, not file order or completion time order.
- A skipped pass has no authority to emit a new world mutation; its result remains visible for
  schedule/history matching.
- Duration does not influence retry, pass selection or deterministic hash.
- Hash is nullable when hashing/debugging is disabled; FinalHash is null for an empty result list.
- WorldFile writes Manifest only at the explicit save boundary and reads it before load-time liquid
  settling completes; this ordering is an integration contract, not a reason to make Manifest a
  live-world component.
- Version4 confirms manifest declarations and WorldFile write/read locations. Full-reference
  result matching and complete RunPass data are supplementary where Version4 stubs or omits
  behavior. Current NLTX has trace/result concepts but no proven WorldFile manifest closure;
  currentNltxStatus remains partial.

## 13. Checkpoint 8: WorldGenerationOptionBaseState

### 13.1 Proposed boundary

Proposed target paths, each with status: proposed:

- dome/src/Terraria.Dome.Simulation/WorldGeneration/Options/WorldGenerationOptionDefinition.cs
- dome/src/Terraria.Dome.Simulation/WorldGeneration/Options/WorldGenerationOptionSelectionComponent.cs
- dome/src/Terraria.Dome.Simulation/WorldGeneration/Options/WorldGenerationOptionSystem.cs
- dome/src/Terraria.Dome.Simulation/WorldGeneration/Options/WorldGenerationConfigurationAdapter.cs
- dome/src/Terraria.Dome.Simulation/WorldGeneration/Options/WorldGenerationOptionPresentationProjection.cs

Option definition, mutable run selection, configuration and presentation are separate boundaries.
The option system owns the transition event caused by Enabled changes. KeyName,
ServerConfigName, special-seed name/value pairs and AutoGenEnabled are definition/selection inputs;
P18 remains authoritative for the meaning and catalog of special seeds. JObject roots stay inside
the configuration adapter, while LocalizedText and Asset<Texture2D> are client/presentation
projections.

### 13.2 Member ownership table

| Source member | C# type | Classification | Proposed owner/seam | Readers | Writers and lifecycle | Side effect |
|---|---|---|---|---|---|---|
| AWorldGenerationOption._enabled | bool | Run selection component | WorldGenerationOptionSelectionComponent | generation rules, pass selection and UI query | OptionSystem commits explicit Enable/Disable command per run | triggers option-state event |
| AWorldGenerationOption.AutoGenEnabled | bool | Automatic selection policy | WorldGenerationOptionAutoSelectionPolicy | world creation option loader | OptionSystem reads definition/config and sets selection before generation | changes initial option selection |
| WorldGenConfiguration._biomeRoot | JObject | Configuration adapter state | WorldGenerationConfigurationAdapter | typed biome configuration query | adapter loads embedded JSON once; no core writer | embedded resource I/O and parsing |
| WorldGenConfiguration._passRoot | JObject | Configuration adapter state | WorldGenerationConfigurationAdapter | typed pass configuration query | adapter loads embedded JSON once; no core writer | embedded resource I/O and parsing |
| AWorldGenerationOption.Enabled | bool | Command-backed property | WorldGenerationOptionQuery/LegacyOptionAdapter | rules, UI, public hooks | setter becomes OptionStateChanged command; transition only when value differs | event/UI projection |
| AWorldGenerationOption.KeyName | string | Definition identity | WorldGenerationOptionDefinitionCatalog | option registry and seed mapping | immutable definition supplied by option catalog; protected legacy access via adapter | none |
| AWorldGenerationOption.ServerConfigName | string | Definition/config identity | WorldGenerationOptionDefinitionCatalog | server config and selection mapping | immutable definition supplied by catalog | config integration |
| AWorldGenerationOption.SpecialSeedNames | string[] | Cross-system mapping definition | WorldGenerationSeedMappingProjection | seed parser and integration review | catalog adapter supplies immutable copy; P18 owns seed semantics | parsing/config boundary |
| AWorldGenerationOption.SpecialSeedValues | int[] | Cross-system mapping definition | WorldGenerationSeedMappingProjection | seed parser and integration review | catalog adapter supplies immutable copy; P18 owns value semantics | parsing/config boundary |
| AWorldGenerationOption.Description | LocalizedText | Presentation projection | WorldGenerationOptionPresentationProjection | UI adapter | localization adapter resolves per locale; option authority stores only stable key | localization I/O |
| AWorldGenerationOption.Title | LocalizedText | Presentation projection | WorldGenerationOptionPresentationProjection | UI adapter and server diagnostics | localization adapter resolves per locale; not generation authority | localization I/O |
| AWorldGenerationOption.Texture | Asset<Texture2D> | Presentation projection | WorldGenerationOptionPresentationProjection | world creation UI | asset adapter loads client resource; never read by generation core | asset loading/rendering |

### 13.3 Invariants and evidence gap

- Enabled transitions emit at most one state-change event per actual value change and do not invoke
  UI, localization or asset loading from the authority writer.
- AutoGenEnabled selects an initial option only; it cannot silently override an explicit user/seed
  command without a recorded precedence rule.
- SpecialSeedNames and SpecialSeedValues must have a deterministic mapping and matching lengths;
  semantic interpretation and the complete seed catalog remain a P18 handoff.
- Configuration reads are immutable after run initialization; malformed or missing sections use an
  explicit default/error policy and never leak JObject into ECS components.
- Current NLTX uses typed world-rule snapshots and request flags, which is useful existing
  evidence, but its option catalog/UI/config registration is not proven. currentNltxStatus remains
  partial.

## 14. Checkpoint 9: WorldGenerationOptionRegistry

### 14.1 Proposed boundary

Proposed target paths, each with status: proposed:

- dome/src/Terraria.Dome.Simulation/WorldGeneration/Options/WorldGenerationOptionRegistryAdapter.cs
- dome/src/Terraria.Dome.Simulation/WorldGeneration/Options/WorldGenerationOptionCatalogQuery.cs
- dome/src/Terraria.Dome.Simulation/WorldGeneration/Options/WorldGenerationOptionRegistrationCommand.cs
- dome/src/Terraria.Dome.Simulation/WorldGeneration/Options/WorldGenerationSeedConfigParser.cs

The registry adapter owns one-time registration and the run/session reset boundary. The catalog
query returns an immutable/read-only view. The seed_ prefix is a parser definition for server
configuration input, not a permission system or a seed-definition authority. Generic
OptionStorage instances remain a separate support seam in the next checkpoint.

### 14.2 Member ownership table

| Source member | C# type | Classification | Proposed owner/seam | Readers | Writers and lifecycle | Side effect |
|---|---|---|---|---|---|---|
| WorldGenerationOptions._options | List<AWorldGenerationOption> | Registry adapter state | WorldGenerationOptionRegistryAdapter | catalog query, seed parser and option system | RegistryAdapter registers each option once during initialization and exposes a read-only copy | static registration state |
| WorldGenerationOptions._powerPermissionsLineHeader | string | Parser definition | WorldGenerationSeedConfigParser | server config flag parser | immutable constant; no runtime writer | config parsing |
| WorldGenerationOptions.Options | IEnumerable<AWorldGenerationOption> | Read-only catalog query | WorldGenerationOptionCatalogQuery | UI, seed parser, rules and public compatibility adapter | query returns read-only enumeration; cannot append or mutate registry | none in core |

### 14.3 Invariants and evidence gap

- Duplicate registration fails deterministically and cannot replace an existing option instance.
- Registry initialization order is explicit in the registration plan, but runtime system order is
  controlled by scheduler dependencies, never source file order.
- Options returns no mutable list handle; callers can request selection commands through the option
  system but cannot edit registry ownership.
- Server config parsing recognizes only the explicit seed_ prefix and validates option keys and
  clamped flag values through an adapter.
- Version4 confirms static registration, generic instance storage, seed-text lookup and server
  config parsing behavior. Current NLTX has rule snapshots but no proven static option registry
  closure; currentNltxStatus remains partial.

## 15. Checkpoint 10: WorldGenerationSupportTypes

### 15.1 Proposed boundary

Proposed target paths, each with status: proposed:

- dome/src/Terraria.Dome.Simulation/WorldGeneration/Support/WorldGenerationActionChainAdapter.cs
- dome/src/Terraria.Dome.Simulation/WorldGeneration/Support/WorldGenerationShapeOutputProjection.cs
- dome/src/Terraria.Dome.Simulation/WorldGeneration/Support/WorldGenerationOptionStorageAdapter.cs
- dome/src/Terraria.Dome.Simulation/WorldGeneration/Support/WorldGenerationTreeGroundQuery.cs

These support types are seams around existing capabilities, not a generic shared component
bucket. GenAction remains a chain of action behavior whose payload/output is handed to the P20
action boundary. OptionStorage is a registry implementation detail and is never exposed as a
mutable global component. IsGroundValid is a pure tree-ground qualification delegate; P17 owns
the tree/terrain facts and P19 only provides the query seam.

### 15.2 Member ownership table

| Source member | C# type | Classification | Proposed owner/seam | Readers | Writers and lifecycle | Side effect |
|---|---|---|---|---|---|---|
| GenAction.NextAction | GenAction | Command-chain link | WorldGenerationActionChainAdapter | shape/action executor and P20 action integration | chain builder links actions before execution; link is immutable during one action run | action execution delegated to P20 |
| GenAction.OutputData | ShapeData | Output projection | WorldGenerationShapeOutputProjection | shape executor and command collector | action writes output through explicit output command; consumer reads projection | tile/shape command generation delegated |
| WorldGenerationOptions.OptionStorage<T>.Instance | T | Registry instance adapter | WorldGenerationOptionStorageAdapter | typed option lookup and registry adapter | registration creates one instance; reset clears run selection, not catalog identity | static storage compatibility |
| WorldGen.CheckTreeSettings.IsGroundValid | GroundValidTest | Pure Query/delegate | WorldGenerationTreeGroundQuery | CheckTreeWithSettings and tree growth systems | caller supplies a deterministic predicate for each tree profile; no mutable writer | reads terrain definitions only |

### 15.3 Invariants and evidence gap

- Action-chain links terminate and cannot form an accidental cycle; chain traversal has an
  explicit maximum or visited-set policy.
- ShapeData output is not committed directly by GenAction; it becomes an explicit tile/structure
  command owned by the relevant integration system.
- OptionStorage<T>.Instance is initialized once per catalog type and cannot be replaced by a
  second registration; its selection state is separate from instance identity.
- IsGroundValid is deterministic for the provided ground type and profile and does not mutate
  tree, tile or generation state.
- Current NLTX has TreeCheckSettingsQuery and tree-ground suitability queries, which is
  existing-evidence, but no proven GenAction or option-storage closure. currentNltxStatus remains
  partial.

## 16. Non-split and compatibility decisions

- Do not mechanically copy all WorldGenerator fields into one component. Progress, pass
  definitions, controller control, generator context and persistence projections have different
  writers and lifetimes.
- Do not make WorldGenSnapshot or WorldManifest a second world authority. They are serialized
  projections and restore requests; restore is an explicit command handled by the controller.
- Do not place JObject, JsonSerializerSettings, LocalizedText, Asset<Texture2D>, file paths,
  Stopwatch or lock objects in authoritative ECS state.
- Do not turn a static option list or OptionStorage<T>.Instance into a globally mutable component.
  The registry adapter owns registration; a read-only catalog query exposes it.
- Do not move GenAction payload semantics into P19. Preserve an adapter boundary and hand off
  action parameters to P20.
- Preserve public namespace/API behavior through adapters until focused verifiers demonstrate a
  single replacement writer. No double-writer compatibility window is allowed for authority.
- Proposed files follow capability-first organization under WorldGeneration/Execution,
  Progress, Controller, Persistence, Options and Support. One public core type per same-named file;
  proposed file order does not define runtime order.

## 17. Focused verification plan

The verification plan remains migration-gated. The current session has only the build and existing
WorldSession verifier evidence recorded in the current session verification section; P19-specific behavior verification has
not run.

- Lifecycle: start, normal completion, abort, exception and repeated start; verify flags and
  thread-local state are cleared exactly once.
- Pass/progress: message percent formatting, Value clamping, zero TotalWeight, weighted progress,
  disabled passes and explicit pass order.
- Control: pause/resume, pause-after-pass, queued abort, hash mismatch and lock contention.
- Execution: one writer for CurrentPass and PassResults, pass timing injection, deterministic
  hash calculation and failed-pass behavior.
- Snapshot: stable serialization, manifest/version/GitSHA mismatch, disabled-pass matching,
  GenVars restoration, tile snapshot restoration, stale path rejection and no write-back from
  Outdated queries.
- Manifest: result order, nullable hash, final-hash derivation, duration source and WorldFile
  save/load boundary.
- Options: one-time registration, Enabled transition event, read-only enumeration, special-seed
  mapping handoff, localization/texture projection and no static-state leakage between runs.
- Support: action-chain termination, ShapeData ownership, OptionStorage initialization and
  tree-ground query purity.
- Integration: liquid settle ordering, WorldFile generation/load order, public tModLoader hook
  boundaries and P17/P18/P20 handoffs.

The verifier must report source member, proposed owner, writer, lifecycle and evidence status for
each row; a compile-only pass is insufficient. No C# build, test, network replay or persistence
replay was run here, so the current `verificationStatus` is `partially-verified`; P19-specific
behavior, network and persistence replay remain unverified.

## 18. Integration handoff

crossSubsystemOwner: integration-review is required for:

- the relation between P17 terrain/forest metrics and SmallConsecutivesFound,
  SmallConsecutivesEliminated, placingTraps and IsGroundValid;
- P18 special-seed definitions versus P19 option Enabled and pass disabling;
- P20 GenAction payloads, trap behavior and action-chain output;
- WorldFile manifest persistence, liquid settling and load-time recovery;
- network/client presentation of progress, pause state, option text and snapshot diagnostics;
- replacement of Version4 static bridges and legacy public callbacks.

These are integration risks, not reasons to assign another partition's facts to P19. Until each
handoff is verified, P19 remains partial and proposed.
