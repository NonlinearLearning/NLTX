# Version4 P19 世界生成执行组件执行计划

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

## Implementation checkpoint register

All component-owned P19 state is implemented in `src`; the affected project build and the
existing WorldSession focused verifier have been recorded. `verificationStatus` remains
`partially-verified` because no P19-specific focused behavior verifier is available or run.

## Current session verification

- Runner Handoff: `status=handed-off`, `partition=P19`, new session
  `ca556245d09847cab6264716801a8095`, exit code `0`.
- Serial build: `pwsh -NoProfile -File .\\Build\\Tools\\Invoke-SerialDotnet.ps1 build .\\src\\WorldSession\\Terraria.WorldSession.csproj /m:1 /nr:false /p:UseSharedCompilation=false /p:MSBuildNodeReuse=false /p:BuildInParallel=false`; exit code `0`, 0 warnings, 0 errors; artifact `Build/bin/Terraria.WorldSession/Debug/net10.0/Terraria.WorldSession.dll`.
- Existing focused verifier build: `pwsh -NoProfile -File .\\Build\\Tools\\Invoke-SerialDotnet.ps1 build .\\src\\WorldSessionFocusedVerifier\\Terraria.WorldSessionFocusedVerifier.csproj /m:1 /nr:false /p:UseSharedCompilation=false /p:MSBuildNodeReuse=false /p:BuildInParallel=false`; exit code `0`, 0 warnings, 0 errors; artifact `Build/bin/Terraria.WorldSessionFocusedVerifier/Debug/net10.0/Terraria.WorldSessionFocusedVerifier.dll`.
- Existing focused verifier run: `pwsh -NoProfile -File .\\Build\\Tools\\Invoke-SerialDotnet.ps1 run --project .\\src\\WorldSessionFocusedVerifier\\Terraria.WorldSessionFocusedVerifier.csproj --no-build --no-restore`; exit code `0`, eight existing assertions reported passed.
- P19-specific lifecycle, progress, control, snapshot, manifest, option-registry and support behavior verification: not available in the existing verifier and not run. This evidence does not establish full P19 behavior equivalence, network closure, persistence closure or integration completion.
- Runner Complete: `status=completed`, `partition=P19`, `sessionId=ca556245d09847cab6264716801a8095`, exit code `0`, lock released.

| Component | Source path | Implemented state | Deferred dependency |
|---|---|---|---|
| WorldGenerationExecutionStateComponent | `src/WorldSession/WorldGeneration/Execution/WorldGenerationExecutionStateComponent.cs` | GeneratingWorld, SmallConsecutivesFound, SmallConsecutivesEliminated, PlacingTraps | lifecycle/metric/phase systems; P17/P20 handoff |
| WorldGenerationProgressComponent | `src/WorldSession/WorldGeneration/Progress/WorldGenerationProgressComponent.cs` | MessageNoFormatting, Message, Value, TotalWeightedProgress, TotalWeight, CurrentPassWeight, TotalProgress | progress system, pass definition and presentation projection |
| WorldGenerationPassSelectionComponent | `src/WorldSession/WorldGeneration/Progress/WorldGenerationPassSelectionComponent.cs` | immutable pass-id enabled map | pass registration and special-seed selection |
| WorldGenerationControlStateComponent | `src/WorldSession/WorldGeneration/Controller/WorldGenerationControlStateComponent.cs` | Paused and PauseAfterPassId | control command/system and lock adapter |
| WorldGenerationHashMismatchPolicyComponent | `src/WorldSession/WorldGeneration/Controller/WorldGenerationHashMismatchPolicyComponent.cs` | PauseOnHashMismatch | hash query/system |
| WorldGenerationHashMismatchStateComponent | `src/WorldSession/WorldGeneration/Controller/WorldGenerationHashMismatchStateComponent.cs` | PausedDueToHashMismatch | mismatch and resume control |
| WorldGenerationSnapshotPolicyComponent | `src/WorldSession/WorldGeneration/Controller/WorldGenerationSnapshotPolicyComponent.cs` | SnapshotFrequency | snapshot scheduler and persistence |
| WorldGenerationAbortStateComponent | `src/WorldSession/WorldGeneration/Controller/WorldGenerationAbortStateComponent.cs` | AbortQueued | abort command and finish system |
| WorldGenerationGeneratorExecutionStateComponent | `src/WorldSession/WorldGeneration/Execution/WorldGenerationGeneratorExecutionStateComponent.cs` | Seed | deterministic random and pass execution |
| WorldGenerationExecutionCursorComponent | `src/WorldSession/WorldGeneration/Execution/WorldGenerationExecutionCursorComponent.cs` | CurrentPassId | pass execution cleanup and controller query |
| WorldGenerationPassResultComponent | `src/WorldSession/WorldGeneration/Persistence/WorldGenerationPassResultComponent.cs` | DurationMs, Hash, Skipped | ordered result commit and manifest persistence |
| WorldGenerationManifestComponent | `src/WorldSession/WorldGeneration/Persistence/WorldGenerationManifestComponent.cs` | Version, GitSHA | WorldFile/manifest persistence |
| WorldGenerationOptionSelectionComponent | `src/WorldSession/WorldGeneration/Options/WorldGenerationOptionSelectionComponent.cs` | Enabled, AutoGenEnabled | option system, P18 seed semantics and presentation |

The controller pass projection, snapshot, option registry, support types, thread-local flag,
legacy generator handle, configuration JSON, serializer settings, file paths, timing, locks,
queries, commands, adapters, projections and tests remain intentionally unimplemented because
they are outside the component-only scope.

## 1. Plan contract

This file records the implementation sequence and the component checkpoints actually applied in
this session. It records the current WorldSession build and existing focused verifier evidence;
it does not claim that P19-specific behavior, network replay, save/load replay or behavior
equivalence has been verified. The current verification status is:

~~~text
executionStatus: in-progress
implementationStatus: implemented
verificationStatus: partially-verified
~~~

The implementation must process only P19 members, preserve Version4 public namespaces and
behavior through explicit adapters, record source/target paths and dependency impact before any
move, and establish a focused verifier before replacing a writer. New names and paths in this
plan have status: proposed.

## 2. Checkpoint 1: execution state

Completed design unit: WorldGenerationExecutionState.

Proposed target boundary:

- WorldGeneration/Execution/WorldGenerationExecutionStateComponent.cs
- WorldGeneration/Execution/WorldGenerationExecutionSystem.cs
- WorldGeneration/Execution/WorldGenerationThreadExecutionAdapter.cs
- WorldGeneration/Execution/WorldGenerationGeneratorHandleAdapter.cs
- WorldGeneration/Execution/WorldGenerationExecutionCommand.cs

Implementation order:

1. Add a focused verifier for normal start, abort, exception cleanup, thread-affinity cleanup,
   per-run generator handle lifetime, metric reset and placing-traps phase transitions.
2. Define the run identity and a single lifecycle writer. Keep the current NLTX typed state as
   an input/reference; do not silently equate its stage with Version4 generatingWorld.
3. Introduce the thread and legacy-generator adapters behind ports. No WorldGenerator instance
   or thread-static flag may be stored as a broad ECS component.
4. Route terrain metric updates through explicit commands and hand trap behavior to the P20
   integration boundary.
5. Only after the verifier is green may the compatibility writer be retired.

Focused evidence required before the next unit: per-member writer matrix, cleanup trace for
normal/abort/exception paths, and a verifier result that distinguishes P17 metric calculation
from P19 metric commit. No implementation or verifier run has occurred.

## 3. Checkpoint 2: progress and pass state

Completed design unit: WorldGenerationProgressAndPassState.

Proposed target boundary:

- WorldGeneration/Progress/WorldGenerationProgressComponent.cs
- WorldGeneration/Progress/WorldGenerationProgressSystem.cs
- WorldGeneration/Progress/WorldGenerationPassDefinition.cs
- WorldGeneration/Progress/WorldGenerationPassSelectionSystem.cs
- WorldGeneration/Progress/WorldGenerationProgressProjection.cs

Implementation order:

1. Establish a pure progress query for Message, MessageNoFormatting, Value and TotalProgress,
   including percent formatting, clamping and zero-total behavior.
2. Define a pass catalog value for Name and Weight and a separate selection state for Enabled.
3. Route special-seed disable decisions through an explicit selection command and preserve the
   public GenPass.Disable compatibility adapter.
4. Have the progress system commit current-pass weight and completed-pass weight only at explicit
   pass boundaries.
5. Verify disabled-pass totals, pass order and snapshot matching before replacing Version4
   progress/pass writers.

Implemented component:

- `src/WorldSession/WorldGeneration/Progress/WorldGenerationProgressComponent.cs`
  stores `MessageNoFormatting`, clamped `Value`, `TotalWeightedProgress`, `TotalWeight` and
  `CurrentPassWeight`, and exposes deterministic `Message` and `TotalProgress` projections.
  It has no I/O, localization, scheduler, query or command behavior.

Dependency impact: the component is independent of `GenPass`, `WorldGenerator`, JSON, thread
state and persistence. Pass definition and pass selection remain separate boundaries. The
component is implemented and included in the current serial build; P19-specific progress
behavior remains unverified.

## 4. Checkpoint 3: controller pass state

Completed design unit: WorldGenerationControllerPassState.

Proposed target boundary:

- WorldGeneration/Controller/WorldGenerationControllerPassStateComponent.cs
- WorldGeneration/Controller/WorldGenerationControllerSystem.cs
- WorldGeneration/Controller/WorldGenerationPassCheckpointQuery.cs
- WorldGeneration/Persistence/WorldGenerationSnapshotIndexAdapter.cs

Implementation order:

1. Define a stable run-scoped pass identity and a read-only schedule query.
2. Bind the legacy generator handle through an adapter; do not store WorldGenerator in ECS state.
3. Commit CurrentPass before pass execution and LastCompletedPass only after PassResults commit.
4. Move snapshot lookup/deletion behind a control-gate-aware index adapter.
5. Emit OnPassesLoaded as an explicit compatibility event after registration, then verify reset,
   abort and exception cleanup of the schedule cursor and snapshot index.

Focused evidence required before the next unit: seven-member reader/writer matrix, stable pass
identity, pass cursor transition trace and snapshot-index cleanup. No implementation or verifier
run has occurred.

## 5. Checkpoint 4: controller pause and hash state

Completed design unit: WorldGenerationControllerPauseAndHashState.

Proposed target boundary:

- WorldGeneration/Controller/WorldGenerationControlStateComponent.cs
- WorldGeneration/Controller/WorldGenerationControlCommand.cs
- WorldGeneration/Controller/WorldGenerationControlSystem.cs
- WorldGeneration/Controller/WorldGenerationHashQuery.cs
- WorldGeneration/Controller/WorldGenerationControlLockAdapter.cs

Implementation order:

1. Model pause, pause-after-pass, hash-mismatch policy/state, snapshot frequency and queued
   abort as separate control values.
2. Route all changes through commands and one control system; preserve Paused setter behavior in
   a legacy adapter.
3. Put the Version4 control lock behind a proposed adapter and test lock contention/failure
   without blocking core state.
4. Compute hashes from a stable world snapshot and inject timing/UI/logging effects.
5. Verify pause, resume, mismatch restore, abort and cleanup transitions before replacing the
   controller writer.

Focused evidence required before the next unit: seven-member control matrix, command ordering,
hash mismatch trace, lock contention behavior and UI side-effect isolation. No implementation or
verifier run has occurred.

Implemented components:

- `src/WorldSession/WorldGeneration/Controller/WorldGenerationControlStateComponent.cs`
  stores pause state and a stable `PauseAfterPassId`; a paused run clears the deferred pass pause.
- `src/WorldSession/WorldGeneration/Controller/WorldGenerationHashMismatchPolicyComponent.cs`
  stores `PauseOnHashMismatch`.
- `src/WorldSession/WorldGeneration/Controller/WorldGenerationHashMismatchStateComponent.cs`
  stores `PausedDueToHashMismatch` independently from pause intent.
- `src/WorldSession/WorldGeneration/Controller/WorldGenerationSnapshotPolicyComponent.cs`
  stores the validated Version4 snapshot frequency values without file or snapshot behavior.
- `src/WorldSession/WorldGeneration/Controller/WorldGenerationAbortStateComponent.cs`
  stores the queued abort intent.

The control command, lock, hash calculation, snapshot adapter, UI and cleanup behavior remain
unimplemented non-component dependencies. These components are implemented and compile-verified,
but their P19-specific control behavior remains unverified.

## 6. Checkpoint 5: generator execution state

Completed design unit: WorldGenerationGeneratorExecutionState.

Proposed target boundary:

- WorldGeneration/Execution/WorldGenerationGeneratorExecutionStateComponent.cs
- WorldGeneration/Execution/WorldGenerationPassExecutionSystem.cs
- WorldGeneration/Execution/WorldGenerationRunContext.cs
- WorldGeneration/Execution/WorldGenerationStaticBridgeProjection.cs
- WorldGeneration/Execution/WorldGenerationHashTimingAdapter.cs

Implementation order:

1. Define a run context for seed and typed configuration, and reference the pass schedule rather
   than copying it into a broad component.
2. Make PassExecutionSystem the single writer for CurrentPass and the result commit boundary.
3. Inject progress, controller, control-lock, clock and deterministic-random ports.
4. Publish CurrentGenerationProgress and CurrentController only through a compatibility projection,
   clearing both on every terminal path.
5. Verify skipped/failed/pass-result ordering, cursor cleanup, deterministic seed behavior and
   timing isolation before replacing the legacy generator loop.

Focused evidence required before the next unit: 11-member reader/writer matrix, run-context
lifetime trace, one-writer result ordering and static bridge cleanup. No implementation or
verifier run has occurred.

Implemented components:

- `src/WorldSession/WorldGeneration/Execution/WorldGenerationGeneratorExecutionStateComponent.cs`
  stores the per-run deterministic seed only.
- `src/WorldSession/WorldGeneration/Execution/WorldGenerationExecutionCursorComponent.cs`
  stores the stable current-pass identity and exposes whether a pass is active.

The pass schedule, configuration, progress/controller handles, control lock, static compatibility
bridge, timing cache and pass-result list remain adapter, projection or query boundaries. These
components are implemented and compile-verified, but pass-execution behavior remains unverified.

## 7. Checkpoint 6: snapshot state

Completed design unit: WorldGenerationSnapshotState.

Proposed target boundary:

- WorldGeneration/Persistence/WorldGenerationSnapshotDto.cs
- WorldGeneration/Persistence/WorldGenerationSnapshotPersistenceAdapter.cs
- WorldGeneration/Persistence/WorldGenerationSnapshotConsistencyQuery.cs
- WorldGeneration/Persistence/WorldGenerationGenVarsProjectionAdapter.cs
- WorldGeneration/Persistence/WorldGenerationSnapshotRestoreCommand.cs

Implementation order:

1. Define a stable snapshot DTO containing manifest projection, serialized GenVars payload,
   schedule identity and tile payload metadata.
2. Keep JSON settings and reflection schema inside an adapter; reject unsupported or unknown
   mutable live members according to an explicit compatibility policy.
3. Implement path, extension, offset and size-cache behavior behind a storage port.
4. Add pure history/outdated checks and validate version, GitSHA, pass names and skipped state
   before indexing a snapshot.
5. Restore only through a command that coordinates WorldGen reset, tile restore and NPC cleanup;
   verify malformed, stale, mismatched and successful restore cases.

Focused evidence required before the next unit: 13-member persistence matrix, serialized file
layout, stale/history decision trace and restore rollback behavior. No implementation or
verifier run has occurred.

## 8. Checkpoint 7: manifest and pass results

Completed design unit: WorldGenerationManifestAndPassResults.

Proposed target boundary:

- WorldGeneration/Persistence/WorldGenerationManifestProjection.cs
- WorldGeneration/Persistence/WorldGenerationPassResultComponent.cs
- WorldGeneration/Persistence/WorldGenerationPassResultCommitSystem.cs
- WorldGeneration/Persistence/WorldGenerationManifestPersistenceAdapter.cs
- WorldGeneration/Persistence/WorldGenerationManifestQuery.cs

Implementation order:

1. Make PassResultCommitSystem the only writer of ordered pass results.
2. Record duration through a clock port and hash through the stable-world hash query; keep both
   diagnostic/integrity data separate from mutable world authority.
3. Derive skipped state from pass selection at the result boundary and derive FinalHash from the
   final result.
4. Capture Version and GitSHA at run reset/start and serialize only through the manifest adapter.
5. Verify result order, empty/null hash behavior, clone/deserialize failure handling and the
   WorldFile save/load boundary before replacing the legacy Manifest writer.

Focused evidence required before the next unit: eight-member manifest/result matrix, single result
writer trace, hash/timing policy trace and WorldFile ordering evidence. No implementation or
verifier run has occurred.

## 10. Checkpoint 8: option base state

Completed design unit: WorldGenerationOptionBaseState.

Proposed target boundary:

- WorldGeneration/Options/WorldGenerationOptionDefinition.cs
- WorldGeneration/Options/WorldGenerationOptionSelectionComponent.cs
- WorldGeneration/Options/WorldGenerationOptionSystem.cs
- WorldGeneration/Options/WorldGenerationConfigurationAdapter.cs
- WorldGeneration/Options/WorldGenerationOptionPresentationProjection.cs

Implementation order:

1. Create immutable definition/catalog values for key, server name and special-seed mapping;
   keep P18 as the semantic owner of the seed catalog.
2. Create run-scoped selection state for Enabled and AutoGenEnabled with explicit precedence and
   one transition event.
3. Load biome/pass configuration through an adapter and expose typed reads only.
4. Resolve Description, Title and Texture through presentation adapters, not authority state.
5. Verify transition events, automatic selection, mapping-length validation, configuration
   defaults and localization/asset isolation before replacing the legacy option base.

Focused evidence required before the next unit: 12-member option/config matrix, event transition
trace, configuration validation and P18 seed-mapping handoff. No implementation or verifier run
has occurred.

Implemented component:

- `src/WorldSession/WorldGeneration/Options/WorldGenerationOptionSelectionComponent.cs`
  stores the run-scoped `Enabled` and `AutoGenEnabled` state. It has no option definition,
  configuration, event, localization, asset or special-seed semantics.

The option catalog, configuration adapter and presentation projection remain unimplemented
non-component dependencies. All authorized component implementation units are now written and
compile-verified; option behavior remains unverified.

## 11. Checkpoint 9: option registry

Completed design unit: WorldGenerationOptionRegistry.

Proposed target boundary:

- WorldGeneration/Options/WorldGenerationOptionRegistryAdapter.cs
- WorldGeneration/Options/WorldGenerationOptionCatalogQuery.cs
- WorldGeneration/Options/WorldGenerationOptionRegistrationCommand.cs
- WorldGeneration/Options/WorldGenerationSeedConfigParser.cs

Implementation order:

1. Register the fixed built-in option set through one adapter and reject duplicate type
   registration.
2. Return an immutable catalog view rather than the mutable static list.
3. Parse the seed_ server configuration prefix through a dedicated adapter and validate
   normalized option keys and clamped values.
4. Keep generic OptionStorage instance lookup as the next support unit and keep P18 as the seed
   semantic owner.
5. Verify initialization order, duplicate registration, read-only enumeration, reset isolation
   and config parsing before replacing the static registry.

Focused evidence required before the next unit: three-member registry matrix, registration
trace, read-only catalog behavior and seed_ parser edge cases. No implementation or verifier run
has occurred.

## 12. Checkpoint 10: support types

Completed design unit: WorldGenerationSupportTypes.

Proposed target boundary:

- WorldGeneration/Support/WorldGenerationActionChainAdapter.cs
- WorldGeneration/Support/WorldGenerationShapeOutputProjection.cs
- WorldGeneration/Support/WorldGenerationOptionStorageAdapter.cs
- WorldGeneration/Support/WorldGenerationTreeGroundQuery.cs

Implementation order:

1. Keep GenAction chain links and ShapeData output behind an explicit P20 action/shape seam.
2. Add cycle/termination protection to chain traversal and convert output into explicit tile or
   structure commands.
3. Keep generic option storage as a one-time registry adapter separate from selection state.
4. Route tree-ground validity through a pure deterministic query and leave P17 terrain facts
   outside this partition.
5. Verify chain termination, output ownership, duplicate option registration and tree-query
   determinism before replacing support writers.

Focused evidence required for final review: four-member support matrix, chain traversal trace,
output command ownership, option-storage isolation and tree-query purity. No implementation or
verifier run has occurred.

## 13. Ordered implementation units

| Unit | Component scope | Proposed source/target boundary | Single writer | Evidence gate |
|---:|---|---|---|---|
| 1 | WorldGenerationExecutionState | Version4 WorldGen lifecycle -> WorldGeneration/Execution | WorldGenerationLifecycleSystem and explicit metric/phase commands | lifecycle cleanup and adapter ownership |
| 2 | WorldGenerationProgressAndPassState | GenerationProgress and GenPass -> WorldGeneration/Progress | WorldGenerationProgressSystem and pass selection system | formatting, clamping, weights, disabled pass |
| 3 | WorldGenerationControllerPassState | Controller pass fields -> WorldGeneration/Controller | WorldGenerationControllerSystem | pass list/current/last-completed and snapshot-index handoff |
| 4 | WorldGenerationControllerPauseAndHashState | pause/hash/abort fields -> WorldGeneration/Controller | WorldGenerationControlSystem | control lock, pause/resume, hash mismatch and abort |
| 5 | WorldGenerationGeneratorExecutionState | WorldGenerator run context -> WorldGeneration/Execution | WorldGenerationPassExecutionSystem | current pass, pass results, config and static bridge closure |
| 6 | WorldGenerationSnapshotState | WorldGenSnapshot -> WorldGeneration/Persistence | WorldGenerationSnapshotPersistenceAdapter | serialization, stale detection, restore and file isolation |
| 7 | WorldGenerationManifestAndPassResults | WorldManifest/GenPassResult -> WorldGeneration/Persistence | WorldManifestPersistenceAdapter and result commit | order, hashes, timing, version/GitSHA and WorldFile boundary |
| 8 | WorldGenerationOptionBaseState | AWorldGenerationOption/config -> WorldGeneration/Options | WorldGenerationOptionSystem | enable transitions, catalog definition and external projection |
| 9 | WorldGenerationOptionRegistry | static option list -> WorldGeneration/Options | WorldGenerationOptionRegistryAdapter | one-time registration, read-only enumeration and reset isolation |
| 10 | WorldGenerationSupportTypes | GenAction, OptionStorage and tree check -> WorldGeneration/Support | capability-specific adapters/queries | no action payload leakage, storage isolation and pure tree query |

Every unit records a source path, target path and dependency impact before implementation. The
runtime scheduler owns order explicitly; the filesystem does not.

## 14. Side-effect ports and adapters

The planned implementation will use proposed ports for clock/timing, deterministic hash input,
control locking, snapshot storage, manifest storage, progress/UI notification, localization,
asset lookup and legacy callback/API compatibility. Core components and queries cannot perform
file I/O, locking, thread management, logging, localization, asset loading or UI updates.

The persistence plan is projection-first: serialize a stable DTO from live authority, validate
version/GitSHA/pass compatibility, then issue a restore command. A snapshot or manifest read
cannot directly mutate the live world. A failed restore must report a reason and leave the
current authority unchanged.

## 15. Verification and acceptance gates

The current session ran compile-capable commands only through
`Build/Tools/Invoke-SerialDotnet.ps1`, against the affected projects, with serial MSBuild and
`UseSharedCompilation=false`. The exact build and no-build verifier commands and their results
are recorded in `Current session verification` above.

Before implementation can be called complete, run focused verifiers for lifecycle, progress,
control, pass execution, snapshot restore, manifest persistence, option registration and support
queries. Record command, project, exit code, warning/error counts and Build/bin artifact path in
the implementation session. Run verifier commands with --no-build after the affected project
build. Network, WorldFile and liquid-order evidence must be recorded separately.

Current verificationStatus is partially-verified. The existing verifier output is evidence only
for its eight existing assertions; it is not evidence of complete P19 lifecycle, persistence,
network or behavior-equivalence verification.

## 16. Integration handoff and stop conditions

Stop the implementation sequence and hand off to integration-review if ownership of P17 terrain
metrics, P18 seed definitions, P20 action payloads, WorldFile/liquid ordering, public hook
compatibility or client presentation cannot be proven. Do not solve that uncertainty by moving
members between authoritative partitions.

The execution session may call Complete only after both P19 documents are updated, checked
read-only, and the runner confirms the original partition/session ownership. If the work is
actually blocked, use the runner's Fail operation with the original session ID and an
evidence-based failure message. Never edit the ledger manually or generate another session ID.
