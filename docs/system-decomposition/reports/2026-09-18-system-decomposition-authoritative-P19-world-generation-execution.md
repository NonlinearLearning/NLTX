# System Decomposition Report: authoritative P19

partitionId: P19  
taskId: AUTH-SYS-P19  
claimMemberCount: 84  
sessionId: 33c1c3a45612439da49380669be73114  
designStatus: proposed  
verificationStatus: not-run

## Scope and Evidence

This report covers only P19, the ten claimed leaf groups under WorldGenerationAndEcology: 47 fields and 37 properties. It proposes System boundaries from static evidence; it does not implement them. P17 terrain and GenVars facts, P18 seed definitions, and P20 action/shape payloads are excluded as owners and appear only as integration handoffs.

| P19 leaf group | Member count | Complete claimed member roster |
|---|---:|---|
| WorldGenerationExecutionState | 6 | generatingWorld, generatingWorldOnThisThread, _generator, SmallConsecutivesFound, SmallConsecutivesEliminated, placingTraps |
| WorldGenerationProgressAndPassState | 13 | _message, _value, _totalWeightedProgress, TotalWeight, CurrentPassWeight, Name, Weight, Message, MessageNoFormatting, Value, TotalWeightedProgress, TotalProgress, Enabled |
| WorldGenerationControllerPassState | 7 | _previousManifest, _snapshots, OnPassesLoaded, _generator, Passes, CurrentPass, LastCompletedPass |
| WorldGenerationControllerPauseAndHashState | 7 | _paused, PauseAfterPass, PauseOnHashMismatch, PausedDueToHashMismatch, SnapshotFrequency, Paused, QueuedAbort |
| WorldGenerationGeneratorExecutionState | 11 | _passes, _seed, _configuration, _progress, _controller, _controlLock, _currentPass, CurrentGenerationProgress, CurrentController, _hashTime, PassResults |
| WorldGenerationSnapshotState | 13 | SerializerSettings, fieldsAndProperties, _dataOffset, _matchingPasses, SnapshotFolderSuffix, Extension, _snapshotSizeCache, Manifest, Path, GenVarsJson, GenPassResults, Outdated, PathForActiveWorld |
| WorldGenerationManifestAndPassResults | 8 | GenPassResults, SerializerSettings, DurationMs, Hash, Skipped, Version, GitSHA, FinalHash |
| WorldGenerationOptionBaseState | 12 | _enabled, AutoGenEnabled, _biomeRoot, _passRoot, Enabled, KeyName, ServerConfigName, SpecialSeedNames, SpecialSeedValues, Description, Title, Texture |
| WorldGenerationOptionRegistry | 3 | _options, _powerPermissionsLineHeader, Options |
| WorldGenerationSupportTypes | 4 | NextAction, OutputData, Instance, IsGroundValid |

The source directory D:\TRbackup\Version4 has no Git metadata, so a commit identity is unavailable. These SHA-256 values identify the inspected Version4 files for this report:

| Source file | SHA-256 |
|---|---|
| Terraria/Main.cs | 66DBE1D1E6FB89A24A512309BB039DFEC7C06D4678D2A22D7ADECBAD3AFD6520 |
| Terraria/WorldGen.cs | A06A8463E39EA065441FF1EF702A787D3450ADAE5CD3065CF30BF76784D3EA1D |
| Terraria.WorldBuilding/WorldGenerator.cs | 917FFA37464C607EC4A205EAB0E0454A30AF606BCD15DE3500F160F969BF9774 |
| Terraria.WorldBuilding/WorldGenSnapshot.cs | 241077E18B9F8BDB69822EC2F1056BEB555189A6E07CB657FCFADA6D4084965C |
| Terraria.WorldBuilding/WorldManifest.cs | 51B1B5A7079C4BBD30B426891F691529623FE4A58714C2A0AC5A8EE46A2FB962 |
| Terraria.WorldBuilding/WorldGenerationOptions.cs | 0AC2FF3D0870D4A827F5B7A6240F0FA25370407105F1CC1C69B23F31CBB5C2A8 |
| Terraria.WorldBuilding/AWorldGenerationOption.cs | 800309DB63733EFC5D22976B10184F1580284C890F1A0C6C7577EC19C01213C1 |
| Terraria.IO/WorldFile.cs | 92DF79BB7AE89393327AE9EF6B7B78762909E5B2E197C0F3FCCFE709E152F289 |

Evidence sources and limits:

| Source | Use and evidence status |
|---|---|
| P19 input ledger and claimed prompt | Authoritative member boundary and evidence questions; inventory is confirmed as input scope, not owner evidence. |
| Version4 WorldGen.cs, WorldGenerator.cs, WorldGenSnapshot.cs, WorldManifest.cs, WorldGenerationOptions.cs, AWorldGenerationOption.cs, WorldGenConfiguration.cs, Main.cs, WorldFile.cs | Current source declarations and selected control-flow observations were read directly. The selected high-level call flow is confirmed where explicitly cited below; open dispatch and uninspected paths remain partial or unknown. |
| Read-only CPG Query API at D:\TRbackup\NLTX\.agents\skills\ecs-system\tools\CpgEvidence.ps1 | SQLite database D:\TRbackup\Version4-cpg-export\out-dop8-interproc.sqlite; import complete; 967 shards, 8,166,789 nodes, 71,038,907 edges, 1,317 diagnostics; manifest SHA-256 6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364; project fingerprint 521CBD4C11E7EB731256C232350A54099F431D0E59EBDF3C38880281F169C11B; SourceSnapshotId is null. |
| Complete reference mirror at D:\TRbackup\无任何删减通过编译\Terraria.WorldBuilding | Used only to inspect fuller bodies corresponding to Version4 stubs. Supplementary behavior is not promoted to confirmed Version4 behavior. |
| SS14 reference project | Content.Shared/Hands/EntitySystems/SharedHandsSystem.cs and Content.Shared/Containers/ExitContainerOnMoveSystem.cs were read for organization only. They show partial/shared System organization and event subscription, not WorldGen behavior or ownership evidence. |
| Current NLTX target at D:\TRbackup\NLTX\src\NSSLC | Read-only searches show partial P19 state representation; no complete P19 run/controller/snapshot/persistence/registry System was established. |

CPG evidence is static and index-bound. A complete query means the selected indexed scope returned within its budget; it does not bind the index to the Version4 file snapshot, close unselected callers, expand callee effects, prove alias behavior, or establish scheduler order. The CPG source export paths were not established as the same source snapshot as D:\TRbackup\Version4.

Selected Query API results:

| Query | Result | Interpretation |
|---|---|---|
| Find-CpgSymbols for CreateNewWorld, WorldGen.GenerateWorld, WorldGenerator.GenerateWorld, WorldGenSnapshot, WorldGenerator, and WorldManifest | complete for the exact symbol identities in their declaration paths | Confirms indexed declarations only. |
| Find-CpgCallSites for CreateNewWorld in Main.cs, WorldGen.cs, and WorldFile.cs | complete, one site in Main.cs | Current Main.cs source confirms Main calls CreateNewWorld at line 2514. |
| Find-CpgCallSites for WorldGen.GenerateWorld in Main.cs, WorldGen.cs, and WorldFile.cs | complete, two sites: WorldFile.cs and WorldGen.cs | Current source confirms WorldFile auto-generation and worldGenCallback routes. |
| Find-CpgCallSites for WorldGenerator.GenerateWorld in WorldGen.cs and WorldGenerator.cs | complete, one site in WorldGen.cs | Current source confirms the static wrapper invokes the instance generator. |
| Get-CpgCallableFacts for CreateNewWorld, both GenerateWorld methods | partial; gaps include CalleeEffectsNotExpanded | Direct call/effect closure is not available from these results. |
| Get-CpgMemberUses for generatingWorld | complete in Main.cs, WorldGen.cs, and WorldFile.cs; 16 uses, 3 assignment-left writes, 13 Unknown | Does not establish a unique writer or full consumer set. |
| Get-CpgMemberUses for _passes, _currentPass, PassResults, _generator | complete in selected WorldGen/WorldGenerator paths; 4, 4, 14, and 3 uses respectively | _currentPass has two assignment-left writes; all _passes and PassResults uses and two _currentPass uses are Unknown. _generator has one assignment-left write and two Unknown uses. |
| Get-CpgMemberUses for _snapshots and _previousManifest | complete in selected Controller-related paths; 5 and 1 uses | _snapshots uses are Unknown; one _previousManifest assignment-left write is visible. |
| Get-CpgMemberUses for OnPassesLoaded | partial, zero matched uses in the selected WorldGenerator.cs and WorldGen.cs scope | NoMatchingFactInScannedScope is not evidence of no subscriber. |
| Find-CpgCallSites for WorldGenSnapshot.Create, Restore, TryResetToSnapshot, and WorldManifest.Serialize | Create and Restore have one Controller call site each; TryResetToSnapshot has a Controller call site; Serialize has WorldFile and snapshot call sites | Snapshot and manifest relations are source-checked; full effects and inbound paths remain open. |
| Find-CpgCallSites for WorldGenerationOptions.Register, SelectOption, GetOptionFromSeedText | Register has ten indexed self-registration sites; SelectOption and GetOptionFromSeedText each have Main.cs and WorldFile.cs sites | Confirms selected indexed calls only; enabled-state event subscribers and other callers are not closed. |

The Version4 source hash and the CPG manifest hash identify separate artifacts. This report does not claim that the CPG index represents the exact current Version4 snapshot.

## Prior Component Decomposition Reconciliation

The same-partition Component design and execution documents at docs/component-decomposition/review-round-2/2026-09-11-version4-P19-world-generation-execution-component-design.md and docs/component-decomposition/review-round-2/2026-09-11-version4-P19-world-generation-execution-component-execution.md describe 13 component-owned state types, previous builds, and an existing verifier. Those are historical records from a different session and path framing; this report did not rerun their commands and does not treat them as evidence that current NSSLC behavior is complete or migrated.

Current target searches under src/NSSLC find corresponding execution, cursor, generator seed, progress, pass-selection, pause/control, hash mismatch, snapshot policy, manifest, pass-result, and option-selection components. Files include Execution/WorldGenerationExecutionStateComponent.cs, Execution/WorldGenerationExecutionCursorComponent.cs, Execution/WorldGenerationGeneratorExecutionStateComponent.cs, Progress/WorldGenerationProgressComponent.cs, Progress/WorldGenerationPassSelectionComponent.cs, Controller/WorldGenerationControlStateComponent.cs, Controller/WorldGenerationHashMismatchPolicyComponent.cs, Controller/WorldGenerationHashMismatchStateComponent.cs, Controller/WorldGenerationSnapshotPolicyComponent.cs, Persistence/WorldGenerationManifestComponent.cs, Persistence/WorldGenerationPassResultComponent.cs, and Options/WorldGenerationOptionSelectionComponent.cs.

The target also contains generation-domain Systems for narrower capabilities. A search of src/NSSLC/Component/WorldSession/WorldGeneration/Systems did not find an end-to-end run lifecycle, pass executor, Controller owner, WorldGenSnapshot persistence adapter, or option-catalog owner covering this P19 slice. This scoped search supports partial coverage only. The historical WorldGenerationAbortStateComponent is not present under the searched target tree; the current QueuedAbort owner is unknown. Current component presence does not establish writers, lifecycle, callback integration, or one System per invariant.

## Conceptual Behaviors

1. Run lifecycle and world preparation. Main or WorldFile starts a run; WorldGen sets run flags, prepares configuration, constructs a generator, clears/resets the world, registers and filters passes, runs them, applies finish behavior, and clears temporary run flags.
2. Ordered pass execution and progress. WorldGenerator selects the next pass by the current PassResults count, exposes a current-pass cursor, computes total enabled weight, and appends one result before invoking the Controller completion hook. The progress formatting/clamping behavior is present in GenerationProgress, but per-pass updates are not confirmed because Version4 RunPass is stubbed.
3. Run control and checkpoint history. Controller exposes pause/resume, pause-after-pass, abort, hash policy, snapshot cadence, pass views, reset, and snapshot operations under the generator control lock. These state transitions are interdependent with current pass/result count.
4. Manifest and durable snapshot interchange. Manifest/pass results are serialized by WorldFile and reused in WorldGenSnapshot. Snapshot create/load/restore crosses JSON reflection, tile snapshot, file I/O, temporary world state, GenVars, manifest, NPC cleanup, and debug output; it is an effectful restore path, not a read-only projection.
5. Option/configuration catalog. WorldGenerationOptions registers static option instances and resolves seed text/server flags; AWorldGenerationOption.Enabled raises a state-change event. WorldGenConfiguration parses embedded JSON roots consumed by pass/micro-biome generation. A read that returns mutable option instances is not a pure Query.
6. Generation support payloads. GenAction.NextAction/OutputData and OptionStorage<T>.Instance are support contracts. CheckTreeSettings.IsGroundValid is a delegate boundary into terrain/tree rules, not a P19-owned tree algorithm.

## State Ownership and Write Closure

The proposed owner column is a design hypothesis. Confirmed source writes and unresolved writers are separated; CPG access modes remain Unknown except where an assignment-left span was identified.

| P19 group | Proposed owner or boundary | Source-observed writes and readers | Evidence and unresolved closure |
|---|---|---|---|
| WorldGenerationExecutionState | WorldGenerationLifecycleSystem owns run lifecycle and run-scoped execution flags. | WorldGen.CreateNewWorld sets generatingWorld before scheduling; WorldGen.GenerateWorld sets it and thread-local state on entry and clears these in finally. WorldGen also writes _generator during generator construction. | generatingWorld has other indexed uses in Main and WorldGen; selected CPG writes are not complete. generatingWorldOnThisThread, trap/metric fields, task failures before body entry, and external writers remain partial/unknown. |
| WorldGenerationProgressAndPassState | WorldGenerationPassExecutionSystem owns mutable progress and active pass selection; immutable pass identity/name/weight definition is a catalog input. | WorldGenerator calculates TotalWeight, selects _currentPass from the result count, and clears it after the pass. Current source exposes GenerationProgress mutators and GenPass.Enabled private-set state. | RunPass is a stub, so progress Start/End and actual pass enable side effects are unknown. Pass list population/ordering and all Enabled writers need registration/option closure. |
| WorldGenerationControllerPassState | Keep as the control-facing pass view on the same execution capability; snapshot index is an adapter-owned reference. | Passes and CurrentPass project WorldGenerator collections/cursor; LastCompletedPass derives from PassResults.Count. Controller SetGenerator and OnPassesLoaded are stubs in Version4. | CPG finds a write to _previousManifest and Unknown _snapshots uses in selected paths; no subscriber is established. Runtime lifetime and collection aliasing are partial. |
| WorldGenerationControllerPauseAndHashState | WorldGenerationControlSystem owns control transitions, coordinated with the pass owner through one lock/commit port. | Version4 exposes mutable control properties and lock helpers; source includes direct reset, snapshot and run-to-pass paths. | Pause, hash mismatch, abort, OnPaused and OnPassCompleted interactions are incomplete or stubbed. Do not infer thread safety or command idempotency from property declarations. |
| WorldGenerationGeneratorExecutionState | WorldGenerationPassExecutionSystem owns the pass/result cursor invariant and run context; config/random/clock services are adapters. | WorldGenerator.GenerateWorld loops synchronously; under _controlLock it reads PassResults.Count, assigns _currentPass, appends RunPass result, calls OnPassCompleted, then clears _currentPass. | Version4 RunPass returns a placeholder result. Current pass collection population, failure handling, locks, shared static progress/controller and any alternate result writers need further closure. |
| WorldGenerationSnapshotState | SnapshotPersistenceAdapter handles encoding, file lifecycle and validated restore requests; the live world owner remains the writer of restored authoritative state. | Controller calls WorldGenSnapshot.Create/Restore; current source shows manifest clone, GenVars JSON, TileSnapshot and binary file operations. Restore resets WorldGen state and manifest, applies GenVars/tile data, deactivates NPCs, and emits UI text. | Version4 serializer converter bodies are stubs. TileSnapshot and reflection effects cross P17/world storage; file naming, schema compatibility, cache lifecycle, error atomicity, and restore rollback are unknown. |
| WorldGenerationManifestAndPassResults | PassExecutionSystem commits ordered results; WorldManifestProjection/SerializerAdapter handles persistence output. | Result list count controls pass progression and is the source for LastCompletedPass/FinalHash. WorldFile writes Manifest.Serialize and reads WorldManifest.Deserialize. | CPG sees Serialize call sites in WorldFile and snapshot. Write closure of result fields, save version behavior, failure recovery and other readers are partial. |
| WorldGenerationOptionBaseState | WorldGenerationOptionSelectionSystem owns mutable selection; immutable labels/keys/seed match rules are definitions. | Enabled setter changes _enabled and synchronously calls OnEnabledStateChanged and static OnOptionStateChanged; Main and WorldFile select options, and WorldFile applies AutoGenEnabled entries. | Event subscribers, callbacks, option effects and full writer paths are not closed. Definition fields include localization/assets, which require external adapters. |
| WorldGenerationOptionRegistry | WorldGenerationOptionCatalog owns registration and lookup; expose an immutable catalog query rather than the mutable list alias. | Static initialization registers ten options; Register rejects duplicate generic storage and appends to _options; Options returns the list as IEnumerable. | Plugins/mod hooks, initialization ordering, duplicate registration failure handling, and whether consumers mutate returned option instances remain unknown. |
| WorldGenerationSupportTypes | Keep as payload/adapter contracts; final owner depends on adjacent P17/P20 behavior. | GenAction chains NextAction/OutputData; OptionStorage<T>.Instance is static singleton storage; IsGroundValid is a delegate. | Callbacks and consumers are not closed here. P17 terrain and P20 action/shape ownership must go to integration review. |

The ownership invariant for the execution core is proposed as: exactly one WorldGenerationPassExecutionSystem advances the pass cursor and commits each corresponding ordered result, while the control owner requests state transitions through a shared synchronization boundary. A second writer to PassResults or direct mutation of the active pass list would violate this invariant. Source and query evidence do not yet prove that the current code has a unique writer.

## Boundary Role and Decision

Proposed decision: separate run lifecycle, pass execution, mutable run control, option selection/catalog, and effectful persistence responsibilities; keep their per-run state changes coordinated rather than splitting by individual fields or by every legacy method.

| Proposed capability | Boundary choice | Responsibility and exclusion |
|---|---|---|
| WorldGenerationLifecycleSystem | separate | Accept start request; coordinate run setup, configuration, generator/pass composition, finish, and finally cleanup. Does not own terrain-generation algorithms or manifest file format. |
| WorldGenerationPassExecutionSystem | separate, with one ordered commit point | Own current pass, progress mutations, ordered result append, and pass-loop state. Does not queue each synchronous pass through a new asynchronous command buffer. |
| WorldGenerationControlSystem | partial with the execution coordinator until the lock/commit contract is proven | Own pause/resume, pause-after-pass, abort, hash mismatch and reset requests. It must not independently write CurrentPass or PassResults. If integration proves an independent schedule and explicit synchronization contract, revisit separate. |
| SnapshotPersistenceAdapter and WorldManifestSerializerAdapter | adapter/projection, not independent scheduler Systems by default | Encode/decode and perform I/O; validate restore data, then ask the live-world owner to commit it. They never directly become a second authoritative world writer. |
| WorldGenerationOptionCatalog and WorldGenerationOptionSelectionSystem | separate definition/query and selection responsibilities | Catalog owns registered option definitions; selection owner commits enabled/autogen selection. A query must not return a writable alias as if immutable. |
| Generation support payloads | keep as data/delegate contracts until P17/P20 integration review | Do not create Systems around one action link, output payload, singleton storage slot, or predicate delegate. |

Reject one monolithic WorldGenerationSystem because it would mix asynchronous lifecycle, synchronous ordered pass commits, pause/hash control, world mutation, serialization/file I/O, option selection, UI and cross-partition terrain/actions. Reject a System per member or per legacy method because no independent invariant or schedule boundary is demonstrated. The SS14 examples are used only to compare organization: a partial System can share a capability, while an event-specific System can own a narrow reaction; neither determines P19 boundaries.

## System API and Legacy Behavior Mapping

These are proposed conceptual APIs. They are not present or verified in NSSLC by this report.

| Legacy entry or surface | Proposed composition | Behavior contract to retain | Evidence status |
|---|---|---|---|
| Main world-creation route to WorldGen.CreateNewWorld | StartWorldGeneration command -> WorldGenerationLifecycleSystem -> asynchronous run handle; progress is a projection/query | Set up active-world seed/random/UI state, then schedule generation; preserve callback and task failure semantics, which are not fully known. | Main and WorldGen source edge confirmed; Task scheduler/failure closure partial. |
| WorldGen.worldGenCallback | Lifecycle coordinator -> PassExecutionSystem -> SaveNewWorld only on success -> completion callback | Retain sound/menu/callback ordering and success condition. Callback exception behavior is not fully tested or proven. | Static source path confirmed; side effects are direct-source facts only. |
| WorldFile.LoadWorld auto-generation route | OptionCatalog query + selection command -> WorldGenerationLifecycleSystem -> SaveNewWorld on true result | Preserve copied-seed and text-seed branches, autogen flags, option application, and save conditional. The load path after failed generation must retain existing error behavior. | Selected WorldFile source and CPG call site confirmed; other load/save paths remain open. |
| WorldGen.GenerateWorld | RunWorldGeneration request with progress/controller input -> configuration adapter -> lifecycle setup -> pass executor -> finish/cleanup | Retain configuration hook, generator creation, clear/reset/pass order, result and finally cleanup. Do not claim RunPass behavior from the placeholder body. | Control-flow order confirmed in source; pass effects and hook closure partial. |
| WorldGenerator.GenerateWorld and RunPass | Synchronous ExecutePassLoop API; result commit occurs before Controller completion handling | Preserve PassResults-count ordering, abort/pause checks, control lock, current-pass visibility, result append and clear order. Do not introduce delayed queue semantics without proving same visibility. | Loop structure confirmed; RunPass and controller callbacks are stubs in Version4. |
| Controller pause/reset/snapshot calls | PauseWorld, ResumeWorld, AbortWorld, RunToPass, ResetWorld and RestoreSnapshot requests routed through ControlSystem and SnapshotPersistenceAdapter | Preserve lock behavior, branch order, existing direct vs deferred visibility, error and pause result. Exact success/failure values need source/behavior confirmation. | Public control surfaces exist; several implementations are stubs and dynamic entry points are not closed. |
| WorldManifest Serialize/Deserialize and WorldFile load/save | WorldManifestProjection -> versioned WorldFile adapter; snapshot adapter reuses the same format only after compatibility agreement | Preserve binary record position/version gate, nullable final hash, pass-result order, load fallback and save exception behavior. | Serialize call sites and direct code observed; full schema/recovery compatibility not verified. |
| WorldGenerationOptions and AWorldGenerationOption | WorldGenerationOptionCatalog Query + explicit select/apply ServerConfig command; seed matching receives normalized input | Preserve registration uniqueness, text/value matching, Reset-before-Select behavior, AutoGenEnabled application, synchronous state-change callbacks, and errors. | Static registration and selected callers confirmed; subscriber closure unknown. |
| Generation support members | P20 action/shape API, P17 terrain predicate Query and catalog storage adapter after integration review | Preserve chain/output ownership and predicate inputs; do not silently move P17/P20 facts into P19. | Member inventory confirmed; consumers and target composition unknown. |

Queries are only pure when they return immutable data and do not touch clocks, caches, events, random state, UI, or world state. WorldGenerationOptions.Options exposes instances whose Enabled setter has synchronous effects, so it cannot be treated as an immutable pure query until copied or wrapped. Pause, reset, snapshot restore, and option selection are state-changing requests, not queries.

## Call and Dependency DAG

Source-observed edges:

| Edge | Evidence | Status |
|---|---|---|
| Main.CreateWorld -> WorldGen.CreateNewWorld | Main.cs:2514; CPG selected scope has one exact CallTargets site | confirmed for this selected call site |
| WorldGen.CreateNewWorld -> Task.Factory.StartNew -> worldGenCallback | WorldGen.cs:6285-6304 | confirmed in current method body; scheduler semantics and all starts unknown |
| worldGenCallback -> WorldGen.GenerateWorld -> conditional WorldFile.SaveNewWorld -> afterGeneration callback | WorldGen.cs:6267-6283 | confirmed direct order |
| WorldFile.LoadWorld autoGen -> WorldGen.GenerateWorld -> conditional SaveNewWorld | WorldFile.cs:658-720; CPG selected WorldFile site | confirmed direct route |
| WorldGen.GenerateWorld -> load config -> process config hook -> create WorldGenerator -> clearWorld -> Reset -> AddPasses -> DisablePassesForSpecialSeeds -> generator.GenerateWorld -> Finish -> finally restore temporary state and clear flags | WorldGen.cs:10108-10147 | confirmed statement order; indirect hook/pass effects partial |
| WorldGenerator.GenerateWorld -> Controller.SetGenerator -> pass loop -> RunPass -> append PassResults -> Controller.OnPassCompleted -> clear current pass | WorldGenerator.cs:291-343 | control structure confirmed; three invoked behavior bodies are stubs in Version4 |
| Controller snapshot methods -> WorldGenSnapshot.Create/Restore | WorldGenerator.cs:100-260; CPG sees Create/Restore and TryResetToSnapshot call sites | calls confirmed in selected source; full lifecycle and caller closure partial |
| WorldManifest.Serialize -> WorldFile save record and WorldGenSnapshot file; WorldFile load record -> WorldManifest.Deserialize | WorldFile.cs:1465, 2567; WorldGenSnapshot.cs:126-131; CPG selected call sites | confirmed source paths, format compatibility unknown |
| Main and WorldFile -> WorldGenerationOptions.GetOptionFromSeedText/SelectOption | Main.cs:2500-2504; WorldFile.cs:698-706; selected CPG call sites | confirmed selected paths; other UI/config/event callers unknown |

The indexed WorldGenerator type surface contains 16 direct members. This confirms a declaration surface, not all members or a complete runtime graph. WorldGen.cs has an indexed source size of about 9.3 GB; relevant CPG queries used the SQLite index with zero shard bytes scanned. The WorldGen.GenerateWorld and CreateNewWorld callable facts are partial with CalleeEffectsNotExpanded. TryCreateSnapshot has no CPG call site in the selected scope and is partial; current source and supplementary reference must be consulted instead of treating zero as no callers.

Proposed composition order, subject to integration review:

1. Resolve active world identity and seed input.
2. Resolve the option catalog and commit selection before generation configuration is read.
3. Load and process configuration, then create the run-scoped generator and progress/control context.
4. Clear/reset world generation state, register the ordered pass list, then apply disabled-pass rules.
5. Execute passes synchronously in established order; publish each result before completion/hash/snapshot/pause decisions.
6. Finish generation, restore temporary global state and clear run markers in a finally path.
7. Persist the world only when the existing caller observes a successful result, then invoke completion behavior at the existing point.

Only the local WorldGenerator loop and direct method statement order support sequence claims. No ECS scheduler phase, barrier, global pass-registration order, parallel-system permission, or multi-world scheduler contract was observed. Proposed edges are not a proven runtime DAG.

## Lifecycle and Side Effects

- Create/start: CreateNewWorld updates active-world UI/seed flags before starting a task. The direct Task.Factory.StartNew target is observed; cancellation, retries, duplicate starts, and behavior if task scheduling fails are unknown.
- Prepare/run: GenerateWorld loads configuration, invokes a hook, constructs the generator, clears/resets global world state, adds and disables passes, and invokes the generator. World state, random source, GenVars, UI, and pass rules are shared effects.
- Pass: the generator loop checks abort and pause, acquires _controlLock, sets the pass cursor, locks the pass object, appends the result, calls OnPassCompleted, clears the cursor and continues. Version4 RunPass and the Controller callback bodies prevent a complete claim about progress, exceptions, hashes, or pause transitions.
- Complete/error: WorldGen.GenerateWorld restores temporary state and clears execution and special-seed flags in finally. WorldFile.SaveWorld catches save exceptions, displays an error, then rethrows. Callback failures, failures before entering GenerateWorld, complete task error propagation and retry/reset outcomes remain unknown.
- Pause/control: source exposes control-lock operations and a loop that continues while paused. Version4 OnPaused is empty, so UI refresh, sleeping/yield behavior, lock contention and prompt responsiveness are unknown.
- Snapshot: current source creates a world-specific folder, serializes Manifest and GenVars state, saves tile snapshot data, and tracks file size. Restore loads tile data, restores temporary state, resets WorldGen, replaces manifest, applies GenVars, restores tiles, deactivates NPCs and emits UI text. The reference mirror supplies full code for some converter methods, but that does not close the current stub or external TileSnapshot behavior.
- Reset/unload/rebuild: Controller reset paths call WorldGen restore/reset and update progress. Snapshot deletion uses file deletion and clears a static cache. The exact run teardown, active-world switch, unload, and cache invalidation behavior is partial.
- Cross-world/thread lifetime: generatingWorldOnThisThread is thread-static while most generation context, random source, manifest and option storage are static. Multi-world concurrency and isolation are unknown; a target design must not assume concurrent worlds without an integration decision.
- Network/presentation: no P19 network replication contract was established. UI, sound, logging, localization and callbacks remain adapter effects.

## Integration Handoff

| Handoff | Shared boundary | Required decision |
|---|---|---|
| P17, crossSubsystemOwner: integration-review | GenVars, tile grid, tree-ground predicate, tile snapshot and other terrain state | Define the authoritative world/terrain writer, snapshot snapshot-version validation, reset/restore transaction and tree Query boundary. |
| P18, crossSubsystemOwner: integration-review | Seed definitions, translated seed text, secret-seed matching and seed-derived options | Define which seed identity is input, how selection is committed and when generated world options become immutable for a run. |
| P20, crossSubsystemOwner: integration-review | GenAction chain and ShapeData payload | Decide action-chain lifetime, output ownership, pass invocation, failure and ordering contract. |
| WorldFile/Main world lifecycle, crossSubsystemOwner: integration-review | Active world metadata, autogen/loading branch, SaveNewWorld, binary record and callback | Agree save conditional, record compatibility, failure/retry, and who ends or replaces a world-generation session. |
| UI/event/config extension surface, crossSubsystemOwner: integration-review | OnPassesLoaded, OnOptionStateChanged, hooks, progress/debug UI and server configuration | Discover subscribers and extension hooks; preserve synchronous visibility and error behavior. |
| Snapshot storage, crossSubsystemOwner: integration-review | WorldGenSnapshot files, reflection serializer, tile snapshot and static size cache | Specify schema/version validation, paths, corruption handling, atomic restore/rollback, deletion and multi-world ownership. |
| Scheduler/thread/network, crossSubsystemOwner: integration-review | Task scheduling, thread-static marker, global progress/controller/random and any external command/network endpoint | Decide single-run exclusivity, synchronization/affinity, async completion and whether any state is replicated. |

## Migration Behavior Contract

This is an acceptance plan, not a migration result. Tests below must run only in a later authorized implementation task and through the real proposed owner/composition.

| Behavior case | Required observation vector | Current evidence |
|---|---|---|
| New world success and failure | task/callback result, pass order, final manifest/hash, menu and sound calls, world saved exactly under the existing success condition, all temporary flags cleared | Entry and conditional save order confirmed; pass work, exception branch and complete callback outcome unknown |
| Autogen on missing file | seed parsing branch, option selection/event, generated result, save conditional, subsequent load/error path | Selected branch confirmed; behavior equivalence not tested |
| Enabled/disabled pass sequence | exact ordered pass identity, skipped flag, progress weight, result append order, current-pass visibility | Loop structure confirmed; RunPass result/progress behavior is a Version4 stub |
| Pause/resume/abort/run-to-pass | state transitions, lock outcomes, next pass, return values, UI visibility, bounded responsiveness and idempotency | Public control surfaces exist; OnPaused and several Controller methods are stubs |
| Hash comparison and snapshot cadence | hash input/output, mismatch pause decision, snapshot timing, stale-history deletion, manifest comparison | Reference mirror only supplements missing Controller behavior; target Version4 behavior partial |
| Snapshot create/restore/delete | manifest and GenVars bytes, tile snapshot, NPC active flags, temporary state, cache/file lifecycle, failure rollback and world identity | Create/Restore statement effects visible; converter and external tile snapshot behavior incomplete |
| Manifest round-trip and save failure | binary record version gate, ordering, nullable fields, fallback behavior, user-visible save error and rethrow | Serializer call sites/source observed; compatibility and recovery not verified |
| Option catalog and selection | registration uniqueness/order, normalized seed matching, Reset-then-Select order, AutoGenEnabled, event calls, duplicate/error behavior | Registration and selected call sites observed; subscribers and runtime behavior unverified |
| Multi-world/session boundary | concurrent run rejection or isolation, thread affinity, active-world switching, cancellation and cleanup | Static/shared fields observed; supported behavior unknown |

## Evidence Gaps and Blocking Decisions

1. Unique writer closure is incomplete. CPG selected member-use results include Unknown access and alias gaps; OnPassesLoaded returned no selected facts; dynamic callbacks, property side effects and readers outside selected paths remain unknown.
2. The CPG SourceSnapshotId is null and per-file source binding is absent. The current Version4 directory is not a Git checkout. The recorded local file hashes identify inspected files but do not prove that all CPG shards represent them.
3. WorldGenerator.RunPass, Controller.SetGenerator, OnPaused, OnPassCompleted, UpdatePreviousManifest, ReportException, SetDebugWorldGenUIVisibility and WorldGenSnapshot converter methods contain stubs in Version4. These block full execution/control/snapshot behavior claims.
4. The complete reference mirror is supplementary only. Its fuller pass/controller/converter code cannot prove the same dispatch, integration or error behavior in this Version4 tree.
5. Pass list population, pass registration order, special-seed disable rules, all pass effects, and option-driven pass changes need complete registration/caller search. P17/P18/P20 ownership stays at integration-review.
6. Restore and reset touch tiles, GenVars, manifest, NPC active state, random/global state and files. Atomicity, corruption handling, rollback, persistence schema evolution and multi-world isolation are unknown.
7. The current NSSLC component presence is partial. No current source/runtime verification was performed for lifecycle, pass progression, pause/hash/abort, snapshot restore, manifest save/load or option callback composition.
8. World-file load failures, task scheduling failures, callback exceptions, cancellation, unload and retry paths have not been closed.
9. No observed network ownership or scheduler/access-set data proves phase, barriers, thread affinity or parallel safety.

Blocking integration decisions: authoritative ownership of terrain/GenVars restoration; seed/secret-option commit boundary; pass action/output ownership; shared manifest/save version contract; callback/extension registration; snapshot atomicity and scope; and one-run versus multi-world scheduling.

## Verification Plan

verificationStatus: not-run. No build, test, run, behavior verifier or source mutation was performed in this task. Verification must be designed against the real System composition before implementation is considered complete:

- Focus the WorldGenerationPassExecutionSystem tests on stable pass order, disabled passes, one result commit per pass, cursor clear on success/error, progress weight/math, and no duplicate completion callback.
- Focus the control tests on pause/resume/abort/pause-after-pass transitions under the shared control gate, lock contention, retry/idempotency and same-tick visibility.
- Focus lifecycle tests on every return/exception path clearing global/run flags, preserving callback/save order and ensuring failed runs do not commit success-only persistence.
- Focus snapshot tests on manifest and GenVars round-trip, tile snapshot restore, NPC cleanup, outdated snapshot rejection, corrupt/truncated file handling, rollback, cache deletion and world identity.
- Focus option tests on registration order and duplicates, seed text/value normalization, Reset/Select ordering, server autogen flags, event subscribers and effects visible to the next run.
- Add integration tests across P17/P18/P20 and WorldFile for world state ownership, seed input, pass/action order, save/load versioning and snapshot commit.
- Add scheduler/thread/session tests for duplicate starts, cancellation, callback failure, unload, active-world replacement and explicit single-world or multi-world semantics.

Run only after the implementation task authorizes verification. Passing compilation or local component tests alone will not establish behavior equivalence or migration success.
