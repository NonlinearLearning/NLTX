# P20 世界生成动作、条件、形状与结构规划执行计划

## Execution Metadata

~~~yaml
documentType: component-execution
partitionId: P20
sessionId: 5249467530774ccd893d62a6da3139ac
runnerSettlementStatus: failed
runnerSettlementSessionId: 5249467530774ccd893d62a6da3139ac
runnerSettlementExitCode: 5
runnerSettlementLockReleased: true
inputReport: D:\TRbackup\NLTX\docs\migration\ledgers\authoritative-20-partitions\P20-World-Generation-Actions-Shapes.md
componentDesignPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\2026-09-11-version4-p20-world-generation-actions-shapes-component-design.md
executionStatus: blocked
implementationStatus: partial
verificationStatus: c12-required-generation-id-build-passed; full-worldsession-build-passed; existing-focused-verifier-passed; semantic-integration-not-verified
designCompletedComponents:
  - C01-WorldTileMergeCullStateQuery
  - C02-WorldGenerationTileSetActionsCommand
  - C03-WorldGenerationWallMutationActionsCommand
  - C04-WorldGenerationTilePlacementAndPaintActionsCommand
  - C05-WorldGenerationLiquidAndNeighborActionsCommand
  - C06-WorldGenerationTileScanAndControlActionsCommand
  - C07-WorldGenerationTileFramingAndDebugActionsCommand
  - C08-WorldGenerationConditionsAndSearchesQuery
  - C09-WorldGenerationShapeDataDefinitionQuery
  - C10-WorldGenerationShapeModifierStateDefinitionQuery
  - C11-WorldGenerationTileWallConditionStateQuery
  - C12-WorldStructurePlanningAndMasksComponent
completedComponents:
  - C01-WorldTileMergeCullStateQuery
  - C02-WorldGenerationTileSetActionsCommand
  - C03-WorldGenerationWallMutationActionsCommand
  - C04-WorldGenerationTilePlacementAndPaintActionsCommand
  - C05-WorldGenerationLiquidAndNeighborActionsCommand
  - C06-WorldGenerationTileScanAndControlActionsCommand
  - C07-WorldGenerationTileFramingAndDebugActionsCommand
  - C08-WorldGenerationConditionsAndSearchesQuery
  - C09-WorldGenerationShapeDataDefinitionQuery
  - C10-WorldGenerationShapeModifierStateDefinitionQuery
  - C11-WorldGenerationTileWallConditionStateQuery
currentComponent: C12-WorldStructurePlanningAndMasksComponent
pendingComponents:
  - C12-WorldStructurePlanningAndMasksComponent
lastCheckpointUtc: 2026-09-12T11:23:12.1866675Z
evidence-gap:
  - Implementation project and canonical ECS world owner are not selected for these proposed boundaries.
  - Version4 call-site, persistence, network, retry, and scheduler contracts are incomplete; current bodies and fallback behavior must be reconciled before implementation.
  - Cross-partition ownership remains open with P01, P16, P17, P19, WorldStorage, and WorldInteraction.
  - Version4 call-site order, storage ownership, combined paint atomicity, callback failure behavior, liquid type semantics, neighbor ordering, random stream policy, and commit failure semantics remain unverified for implementation.
  - C06 focused verification covers filter copying, deterministic accumulation/reset, and explicit seam mapping; callback exception/cancellation behavior, bounds validity, pass cleanup, duplicate execution, and P19 scheduling remain unverified.
  - C07 source boundary is saved, but focused build/run is not verified because the affected project currently fails on unrelated duplicate LakePlacementCapacityDefinition, LakePlacementHistoryComponent, and LakePlacementHistorySnapshot declarations outside P20.
  - Historical checkpoint only: C08 source and verifier assertions were saved while a rebuild was blocked by `src/WorldSession/WorldGeneration/Systems/HellChestLootCycleSystem.cs(16)` calling a missing single-argument `TryAdvanceAfterSuccessfulPlacement` overload outside P20. That earlier C08 artifact predates the final fixture-coordinate edit and cannot prove the final verifier run.
  - Historical checkpoint only: C09 source and verifier assertions were saved while the required serial build for `src/WorldSessionFocusedVerifier/Terraria.WorldSessionFocusedVerifier.csproj` exited 1 with 0 warnings and 2 errors because `src/WorldSession/WorldGeneration/Systems/HellChestLootCycleSystem.cs(16)` called a missing single-argument `TryAdvanceAfterSuccessfulPlacement` overload outside P20.
  - Historical checkpoint only: C10 source and verifier assertions were saved while the same unrelated HellChest compile error prevented a current build. Explicit random-input and fallback modifier behavior remain limited to the boundary contract until the affected project can build.
  - Historical checkpoint only: C11 source and verifier assertions were saved while the same unrelated HellChest compile error prevented a current build. Exact legacy predicate composition and missing-neighbor distinction remain integration evidence gaps.
  - Historical checkpoint only: the required no-build verifier command was run through `Build/Tools/Invoke-SerialDotnet.ps1` and exited `-532462766` at the old C08 `IsSolid` assertion. Its `Build/bin/Terraria.WorldSessionFocusedVerifier` artifact predates the C09-C11 source and verifier timestamps, so this run is stale evidence and does not verify the current checkpoint.
  - Exact persistence/network representation, retry and duplicate delivery policy, scheduler barriers, and owner-system selection remain open across the proposed boundaries.
  - C12 source is saved at `src/WorldSession/WorldGeneration/Structures/WorldStructurePlanningAndMasksComponent.cs`; copied planned/protected rectangle state and a required non-negative generation identity are implemented, with no canonical owner or runtime writer.
  - Existing `StructureReservationIntent` and `InMemoryStructureReservationAdapter` provide generation-scoped reservation evidence, but C12 does not yet have confirmed canonical world-scoped GenerationId binding, DungeonSide ownership, WorldGenRange scaling inputs, reset/completion barriers, persistence/recovery format, network projection, or concurrency policy.
  - Current fresh verification: the serial `WorldSession` build exited 0 with 0 warnings and 0 errors, the serial `WorldSessionFocusedVerifier` build exited 0 with 0 warnings and 0 errors, and the corrected `run --project ... --no-build --no-restore` verifier exited 0 with the existing C01/C07/C09/C10/C14 success lines. Artifacts are under `Build/bin/`; the earlier HellChest error is historical and no P20 source change was made to resolve it.
  - Previous runner settlement: `Fail -PartitionId P20 -SessionId 5d6a6899d72f4b6da04009ec7429ae84` returned `status: failed` with process exit code `5` and `lockReleased: true`; that session recorded the unresolved canonical integration contract.
  - Retry session `1cf3a2e17cde4a0dbc98543899d0b1f8` was claimed through the official runner with `-Retry` after P19 handoff completion. P19's completed documents still identify P20 action payloads, scheduler barriers, WorldFile persistence/recovery, and network projection as `integration-review`; no new C12 owner evidence is available.
  - Retry decision: preserve the existing partial component boundary, do not add a second StructureMap owner or guessed range/side behavior, and settle this retry as failed after current verification because the same canonical integration contract remains unresolved.
  - Retry verification: `Invoke-SerialDotnet.ps1 build ./src/WorldSession/Terraria.WorldSession.csproj --no-restore -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false` exited `0` with `0` warnings and `0` errors; `Build/bin/Terraria.WorldSession/Debug/net10.0/Terraria.WorldSession.dll` exists. The focused verifier build with the same serial wrapper exited `0` with `0` warnings and `0` errors; the corrected `run --project ./src/WorldSessionFocusedVerifier/Terraria.WorldSessionFocusedVerifier.csproj --no-build --no-restore ...` exited `0` and printed the existing C01/C07/C09/C10/C14 success lines. These commands do not verify C12 owner, lifecycle, concurrency, persistence, recovery, range, side, or network semantics.
  - Final retry settlement: official runner `Fail -PartitionId P20 -SessionId 1cf3a2e17cde4a0dbc98543899d0b1f8 -ResultExitCode 1` returned `status=failed`, runner exit code `5`, and `lockReleased=true`.
  - New retry evidence: P17's settled component design explicitly maps `GenVars.structures` to a `StructureReservationAdapter` behind `IStructureReservationCommitPort`, with a generation-session reservation index and no direct save/network evidence. This confirms that the existing reservation adapter is an integration seam, not evidence for a second canonical C12 `StructureMap` owner.
  - New retry decision: keep the C12 component limited to copied plan/protection collections and generation identity. Do not add lock ownership, reservation writer, save/network state, or guessed range/side definitions while P16/P19 lifecycle and integration contracts remain open.
  - Final retry settlement: official runner `Fail -PartitionId P20 -SessionId 5249467530774ccd893d62a6da3139ac -ResultExitCode 1` returned `status=failed`, runner exit code `5`, and `lockReleased=true` at `2026-09-12T11:23:12.1866675Z`.
blocking-decision:
  - C01, C08, C09, C10, and C11 are implemented as pure explicit-snapshot/value query boundaries; C02-C07 are implemented as transient query/projection boundaries; C12 is a partial component boundary with required non-negative generation identity and remains blocked for canonical owner implementation.
  - Do not activate dual writers or persist command payloads. Approve world scope, owner systems, commit ordering, and compatibility window before coding.
  - C05 is a pure immutable liquid/neighbor intent boundary, C06 is a pass-local scan/control boundary, C07 is a framing/diagnostic intent boundary with no active runtime adapter, and C08 is a pure copied-snapshot condition/search boundary with no writer.
  - The current Version4 WorldBuilding bodies contain placeholder behavior, so the declaration inventory and fallback/reference source cannot prove authoritative behavior.
  - C01 lacks an approved immutable tile-neighborhood snapshot, merge-rule input, edge policy, framing revision, and NLTX framing owner. C09-C11 lack approved shape snapshot, aliasing, predicate-composition, and explicit random-input contracts.
  - C02-C07 lack approved tile, wall, liquid, framing, pass-control, and diagnostic owners. C12 lacks a canonical world aggregate and generation binding, reset, concurrency, persistence, recovery, and network contracts.
  - C01-C11 source and focused verifier assertions exist, and the partial C12 source exists. The current fresh WorldSessionFocusedVerifier build exited 0 with 0 warnings and 0 errors; its corrected no-build run exited 0 and printed the existing C01/C07/C09/C10/C14 success lines. No C12-specific verifier was added or run.
executionGate:
  status: blocked
  blockedBefore: none
  observedAtUtc: 2026-09-12T10:51:38.7445647Z
  sourceEvidence: D:\TRbackup\Version4\Terraria.WorldBuilding\Actions.cs; D:\TRbackup\Version4\Terraria.WorldBuilding\Conditions.cs; D:\TRbackup\Version4\Terraria.WorldBuilding\Modifiers.cs; D:\TRbackup\Version4\Terraria.WorldBuilding\ModShapes.cs; D:\TRbackup\Version4\Terraria.WorldBuilding\Searches.cs; D:\TRbackup\Version4\Terraria\WorldGen.cs
  requiredBeforeResume: approved immutable tile/neighborhood snapshots, merge/framing owner contract, P17/P19/P16/P01 handoff contracts, command commit ports, scheduler barriers, random stream policy, and a focused verifier project
  sourceChanges: src/WorldSession/WorldGeneration/Queries/WorldTileMergeCullStateQuery.cs; src/WorldSession/WorldGeneration/Actions/WorldGenerationTileSetActionsCommand.cs; src/WorldSession/WorldGeneration/Actions/WorldGenerationWallMutationActionsCommand.cs; src/WorldSession/WorldGeneration/Actions/WorldGenerationTilePlacementAndPaintActionsCommand.cs; src/WorldSession/WorldGeneration/Actions/WorldGenerationLiquidAndNeighborActionsCommand.cs; src/WorldSession/WorldGeneration/Actions/WorldGenerationTileScanAndControlActionsCommand.cs; src/WorldSession/WorldGeneration/Actions/WorldGenerationTileFramingAndDebugActionsCommand.cs; src/WorldSession/WorldGeneration/Queries/WorldGenerationConditionsAndSearchesQuery.cs; src/WorldSession/WorldGeneration/Shapes/WorldGenerationShapeDataDefinitionQuery.cs; src/WorldSession/WorldGeneration/Shapes/WorldGenerationShapeModifierStateDefinitionQuery.cs; src/WorldSession/WorldGeneration/Queries/WorldGenerationTileWallConditionStateQuery.cs; src/WorldSession/WorldGeneration/Structures/WorldStructurePlanningAndMasksComponent.cs; src/WorldSession/Terraria.WorldSession.csproj; src/WorldSessionFocusedVerifier/Program.cs; src/WorldSessionFocusedVerifier/Terraria.WorldSessionFocusedVerifier.csproj
   verification: the full `src/WorldSession/Terraria.WorldSession.csproj` build exited 0 with 0 warnings and 0 errors and produced the WorldSession DLL under `Build/bin/`; the existing WorldSessionFocusedVerifier build exited 0 with 0 warnings and 0 errors, and its `run --project ... --no-build --no-restore` invocation exited 0. C12 has no focused verifier; semantic integration remains not-verified.
~~~

This document is an implementation checkpoint, not a completion claim. C01 and C08-C11 have
source boundaries saved; the C12 source boundary is partial and only stores confirmed structure
collections. C12 remains blocked for canonical ownership and lifecycle integration. All paths remain
subject to the repository ECS file-organization rules and the integration decisions listed above.

## 1. Preconditions And Non-Goals

Before implementation begins, obtain decisions for the world entity or aggregate that owns
generation state, the P17/P19/P16 handoff contracts, tile storage and framing ownership,
persistence and network representation, DungeonBounds ownership, and the legacy compatibility
window. Implementation must use explicit command/query/commit seams and must not infer runtime
order from Markdown or source file order.

This implementation session creates only the explicitly scoped P20 source and focused verifier
files under src, and updates only the two P20 Markdown checkpoints. It does not change the
authoritative report, other partition documents, Version4 source, or ledger.

## 2. Proposed File Organization

| Proposed type | Proposed path | Role |
| --- | --- | --- |
| WorldTileMergeCullStateQuery | src/WorldSession/WorldGeneration/Queries/WorldTileMergeCullStateQuery.cs | Pure culling result over explicit tile/framing input |
| WorldGenerationTileSetActionsCommand | src/WorldSession/WorldGeneration/Actions/WorldGenerationTileSetActionsCommand.cs | Tile set/clear/shape intent payload |
| WorldGenerationWallMutationActionsCommand | src/WorldSession/WorldGeneration/Actions/WorldGenerationWallMutationActionsCommand.cs | Wall set/clear/place intent payload |
| WorldGenerationTilePlacementAndPaintActionsCommand | src/WorldSession/WorldGeneration/Actions/WorldGenerationTilePlacementAndPaintActionsCommand.cs | Placement and paint intent payload |
| WorldGenerationLiquidAndNeighborActionsCommand | src/WorldSession/WorldGeneration/Actions/WorldGenerationLiquidAndNeighborActionsCommand.cs | Liquid/smoothing intent payload |
| WorldGenerationTileScanAndControlActionsCommand | src/WorldSession/WorldGeneration/Actions/WorldGenerationTileScanAndControlActionsCommand.cs | Scan/count/custom action execution-local payload |
| WorldGenerationTileFramingAndDebugActionsCommand | src/WorldSession/WorldGeneration/Actions/WorldGenerationTileFramingAndDebugActionsCommand.cs | Frame intent plus diagnostic projection request |
| WorldGenerationConditionsAndSearchesQuery | src/WorldSession/WorldGeneration/Queries/WorldGenerationConditionsAndSearchesQuery.cs | Pure tile/world qualification and directional search |
| WorldGenerationShapeDataDefinitionQuery | src/WorldSession/WorldGeneration/Shapes/WorldGenerationShapeDataDefinitionQuery.cs | Immutable point-set definition and shape-data query |
| WorldGenerationShapeModifierStateDefinitionQuery | src/WorldSession/WorldGeneration/Shapes/WorldGenerationShapeModifierStateDefinitionQuery.cs | Immutable modifier definitions and transformations |
| WorldGenerationTileWallConditionStateQuery | src/WorldSession/WorldGeneration/Queries/WorldGenerationTileWallConditionStateQuery.cs | Pure tile/wall/liquid/height/contact qualification |
| WorldStructurePlanningAndMasksComponent | src/WorldSession/WorldGeneration/Structures/WorldStructurePlanningAndMasksComponent.cs | World-scoped structure planning authority |

Keep one core public type per same-named PascalCase file. Existing namespaces and public APIs must
remain stable until a separate migration decision authorizes change. Do not add generic
Shared/Components/, Common, Misc, or Manager directories.

## 3. Planned Sequence And Rollback Units

| Step | Component | Planned unit | Rollback trigger |
| ---: | --- | --- | --- |
| 1 | C01 culling query | Establish explicit culling input/output and invalidation seam. | Cache result changes without proven framing input or query writes state. |
| 2 | C02 tile set actions | Translate tile set/clear/shape fields into commands and one tile commit owner. | Tile replacement, framing, or neighbor order differs. |
| 3 | C03 wall mutation actions | Add wall command and commit path with tile-clearing policy explicit. | Wall/tile ordering or neighbor handling becomes implicit. |
| 4 | C04 placement/paint actions | Separate placement and paint intent from tile storage and rendering effects. | Paint leaks into authority or placement style is lost. |
| 5 | C05 liquid/neighbor actions | Route liquid and smoothing intent to the approved liquid/framing owner. | P01/P17 liquid ownership or ordering remains ambiguous. |
| 6 | C06 scan/control actions | Keep counters, dictionaries, delegates, and callbacks execution-local. | Mutable work state survives a pass or becomes a world component. |
| 7 | C07 framing/debug actions | Commit framing through a port and debug draw through a one-way projection. | Diagnostic code mutates generation authority or replication. |
| 8 | C08 conditions/searches | Implement pure qualification and directional search over snapshots. | Query reads globals, writes tiles, or changes search termination. |
| 9 | C09 shape data | Introduce immutable point-set/value semantics and count query. | Point-set aliasing or coordinate semantics drift. |
| 10 | C10 shape modifiers | Preserve modifier parameters behind immutable definitions and explicit random input. | Randomness, mutation, or modifier ordering is hidden. |
| 11 | C11 tile/wall conditions | Implement pure condition predicates with explicit neighbor/liquid inputs. | Direction order, diagonal policy, or height inclusivity differs. |
| 12 | C12 structure planning | Establish one world-scoped owner, protected-overlap rules, range/mask seam, and snapshot adapter. | Persistence, lock, overlap, or unique-owner policy is unresolved. |

Each step is independently revertible only after the old path remains available and the focused
verifier confirms equivalence. Legacy declarations are not removed by adding a proposed boundary.
No dual-active writer is allowed.

## 4. Common Verifier Contract

Any compile-capable command must run serially from the repository root through
Build/Tools/Invoke-SerialDotnet.ps1, with -p:UseSharedCompilation=false, and its artifact must be
under Build/bin/. The current focused build and stale no-build run are recorded in the verification
gate below; neither is accepted as current behavior verification.

Every implementation step must include focused checks for deterministic calculations,
side-effect isolation, default/invariant behavior, duplicate/retry handling, compatibility reads,
source-sequence coverage, and the relevant persistence/network boundary. Verifiers must run with
--no-build --no-restore after the affected project has been built serially.

## 5. Checkpoint Ledger

Detailed execution records are appended in component order. The component design document and this
plan must be saved together before advancing currentComponent.

| Id | Boundary | Status |
| --- | --- | --- |
| C01 | WorldTileMergeCullStateQuery | complete-plan |
| C02 | WorldGenerationTileSetActionsCommand | complete-plan |
| C03 | WorldGenerationWallMutationActionsCommand | complete-plan |
| C04 | WorldGenerationTilePlacementAndPaintActionsCommand | complete-plan |
| C05 | WorldGenerationLiquidAndNeighborActionsCommand | complete-plan |
| C06 | WorldGenerationTileScanAndControlActionsCommand | complete-plan |
| C07 | WorldGenerationTileFramingAndDebugActionsCommand | complete-plan |
| C08 | WorldGenerationConditionsAndSearchesQuery | complete-plan |
| C09 | WorldGenerationShapeDataDefinitionQuery | complete-plan |
| C10 | WorldGenerationShapeModifierStateDefinitionQuery | complete-plan |
| C11 | WorldGenerationTileWallConditionStateQuery | complete-plan |
| C12 | WorldStructurePlanningAndMasksComponent | complete-plan |
## 6. Component Execution Checkpoints

### C01 - WorldTileMergeCullStateQuery

status: implemented-boundary; implementationStatus: complete-for-boundary; verificationStatus: focused-passed.

1. Freeze an input record containing the tile neighborhood, merge region, framing revision, edge
   policy, and any generation rule that affects culling. Do not pass a live global tile array.
2. Map source sequences 2409-2416 to the eight result positions in the design table. Keep the
   source declaration and public compatibility projection unchanged during the first step.
3. Add a pure query seam and an owner-side invalidation adapter. The adapter may cache only after
   the framing owner and invalidation key are approved.
4. Add focused truth-table cases for empty, boundary, diagonal, stale-revision, and changed-frame
   inputs. Check query purity by comparing the snapshot and side-effect recorder before and after.
5. Keep the result out of persistence and replication authority. A rollback removes the proposed
   query adapter and restores the legacy read path; no tile data is deleted.

Dependencies: WorldStorage/WorldInteraction framing snapshot contract, P17 tile facts, and P19
pass barrier. API seam: immutable snapshot in, eight-value result out. Effect ordering: read
snapshot, calculate, then let the caller decide whether to submit a framing or merge command.
Duplicate policy: equal input revision may reuse an externally owned result; a stale revision must
be rejected or recalculated, never silently committed. Focused verifier: passed for revision
rejection, directional culling, equal visibility, missing-neighbor policy, and result revision
preservation.

Implementation checkpoint: `src/WorldSession/WorldGeneration/Queries/WorldTileMergeCullStateQuery.cs`
provides the explicit `TileMergeNeighborhood`, `CullRules`, `TileMergeRegion`, and eight-field
`CullResult` boundary. It rejects a revision mismatch, applies the documented missing-neighbor
policy, returns all-false results when invisible blocks are explicitly shown, and performs no
storage, cache, logging, random, network, or global-state access. The dedicated P20 verifier and
serial build passed; framing invalidation ownership, persistence, replication, and runtime
call-site equivalence remain unverified.
### C02 - WorldGenerationTileSetActionsCommand

status: implemented-boundary; implementationStatus: complete-for-boundary; verificationStatus: focused-passed.

1. Preserve source mapping 2048, 2050-2054, 2059-2061, 2065-2066, and 2076 exactly once.
   Represent the operation kind and values as a short-lived command; do not expose the legacy
   private fields as persistent component members.
2. Define the TileCommitPort input and the separate FrameRequestPort output. The owner system must
   make clear whether clear-before-set, wall preservation, local framing, and neighbor framing occur
   before or after the tile write.
3. Add the legacy action adapter only after constructor defaults and source call sites are closed.
   A failed commit must return an explicit failure result and must not retry a non-idempotent command
   implicitly.
4. Add focused tests for each source action, invalid tile/slope values, keep-wall behavior, frame
   flags, duplicate command IDs, and no-write-on-validation-failure.
5. Rollback by disabling the adapter and retaining the original action path; do not remove legacy
   methods or delete tile data.

Dependencies: C01 culling result if framing uses it, P17 tile/terrain facts, WorldStorage tile
commit ownership, and P19 pass scheduling. API seam: immutable command in, commit result plus
optional framing intent out. Effect order is validate, write tile, then enqueue explicitly requested
framing/neighbor work, subject to the approved storage contract. The implementation is a transient
immutable command only; it does not write tiles or enqueue effects. Focused verification passed for
source-default preservation and pre-commit invalid-slope rejection. Tile registry validation,
duplicate delivery, commit failure, and framing order remain unverified.

Implementation checkpoint: `src/WorldSession/WorldGeneration/Actions/WorldGenerationTileSetActionsCommand.cs`
maps the twelve C02 members to operation-specific immutable values: clear-neighbor framing,
half-block, set-tile and keep-wall framing/clear policies, slope, half-tile, and solid-swap type.
No storage, network, persistence, logging, random, or global tile access is performed.
### C03 - WorldGenerationWallMutationActionsCommand

status: implemented-boundary; implementationStatus: complete-for-boundary; verificationStatus: focused-passed.

1. Map source sequences 2049, 2055-2058, and 2072-2073 exactly once into a transient wall
   operation record. Keep clear-tile as an explicit effect request, not an implementation shortcut.
2. Obtain the approved WallCommitPort contract and define whether wall commit returns the prior
   wall/tile state needed for rollback or emits a compensating command.
3. Implement the owner sequence as validate wall intent, commit wall, optionally clear tile, then
   enqueue local/neighbor framing according to the approved contract. The command must never call
   storage directly.
4. Add focused checks for all flag combinations, invalid wall IDs, protected/occupied tiles,
   duplicate command IDs, commit failure and compensating behavior. Include a side-effect trace
   assertion for ordering.
5. Rollback by disabling the wall adapter and restoring the legacy wall action path; do not remove
   the source action declarations or alter unrelated tile storage.

Dependencies: C02 TileCommitPort contract, WorldStorage/WorldInteraction wall/framing owner, P17
tile/wall facts, and P19 pass scheduling. No persistence or network work is part of this step.
Focused verifier passed for default preservation and explicit tile-clear/neighbor intent. Invalid
types, duplicate delivery, partial commit failure, and exact owner-system side-effect order remain
unverified.

Implementation checkpoint: `src/WorldSession/WorldGeneration/Actions/WorldGenerationWallMutationActionsCommand.cs`
maps the seven C03 members to immutable clear-wall, set-wall, and place-wall operation values.
`ClearWall` cannot implicitly clear a tile; `SetWall` exposes clear-tile as explicit intent; and
`PlaceWall` preserves the source neighbor-handling default. The command performs no storage,
network, persistence, logging, random, or global tile access.
### C04 - WorldGenerationTilePlacementAndPaintActionsCommand

status: implemented-boundary; implementationStatus: complete-for-boundary; verificationStatus: focused-passed.

1. Map source sequences 2067-2071 exactly once. Keep placement, tile paint, wall paint, and
   combined paint as distinct operation kinds even if the command record shares a common envelope.
2. Agree on TilePlacementCommitPort and paint-port contracts, including target occupancy, style
   interpretation, valid paint values, and whether combined tile/wall paint commits atomically.
3. Route successful authority commits to an explicit framing request and a one-way presentation
   event. Do not pass SpriteBatch or rendering services into the domain command.
4. Add focused tests for each operation, invalid type/style/paint, occupied targets, duplicate
   delivery, combined-operation failure and rollback/compensation. Record the side-effect trace.
5. Rollback by disabling the new adapter and restoring legacy placement/paint actions. No render
   state or committed tile/wall data is deleted by rollback.

Dependencies: C02 tile command seam, C03 wall command seam, WorldStorage/WorldInteraction paint
owner, P17 tile facts, and P19 pass context. Persistence and network remain integration review.
Focused verifier passed for operation separation, paint mapping, default style, and no rendering
dependency. Placement collision, paint range, combined atomicity, duplicate delivery, and
network projection remain unverified.

Implementation checkpoint: `src/WorldSession/WorldGeneration/Actions/WorldGenerationTilePlacementAndPaintActionsCommand.cs`
maps the five C04 members to distinct tile-paint, wall-paint, combined-paint, and tile-placement
intent values. `PlaceTile` retains `style = 0`; the command contains no `SpriteBatch`, storage,
network, persistence, logging, random, or global tile access.
### C05 - WorldGenerationLiquidAndNeighborActionsCommand

status: implemented-boundary; implementationStatus: complete-for-boundary; verificationStatus: focused-passed.

1. Map source sequences 2074, 2075, and 2078 exactly once into a transient command envelope.
   Keep liquid type, amount, and neighbor smoothing independent in the source-to-target table.
2. Obtain P01 and WorldStorage/WorldInteraction decisions for LiquidCommitPort, replication,
   smoothing ownership, and the generation-to-runtime handoff barrier.
3. Validate the command using explicit world and tile snapshots, then submit the intent. Do not
   call liquid simulation, global arrays, networking, or persistence from the command.
4. Add focused cases for zero/full/out-of-range levels, supported/unsupported liquid types,
   neighbor smoothing, blocked cells, duplicate delivery, retry after handoff failure, and ordered
   interaction with C02-C04 commands.
5. Rollback by disabling the handoff adapter and restoring the legacy generation action path. Do
   not delete liquid state or alter P01 runtime ownership.

Implementation checkpoint: `src/WorldSession/WorldGeneration/Actions/WorldGenerationLiquidAndNeighborActionsCommand.cs`
maps source sequences 2074, 2075, and 2078 to immutable `SetLiquid` and `Smooth` intent values.
The focused verifier passed target/type/level/default-neighbor assertions. Liquid type range,
runtime handoff, smoothing ownership, duplicate/retry behavior, commit ordering, and integration
with P01/P17/P19 remain unverified.

Dependencies: C02-C04 command seams, P01 liquid owner, P17 terrain facts, P19 pass barrier, and
WorldStorage/WorldInteraction tile/framing owner. No runtime adapter is enabled by this checkpoint.
### C06 - WorldGenerationTileScanAndControlActionsCommand

status: implemented-boundary; implementationStatus: complete-for-boundary; verificationStatus: focused-passed.

1. Map source sequences 2042-2047 and 2062 exactly once. Define an execution context owned by the
   generation pass; keep Ref<int>, Dictionary, GenAction, delegates, and DungeonBounds out of a
   durable component.
2. Obtain P19 scheduler contracts for continuation, cancellation, callback exception handling, and
   pass cleanup. Obtain an integration decision for DungeonBounds ownership and representation.
3. Implement explicit count/result sinks and a deterministic tile-count accumulator. Callback
   invocation must pass through an adapter that records failure and does not hide retries.
4. Add focused tests for filter copies, count isolation, reset between passes, continuation stop,
   callback failure, cancellation, bounds rejection, duplicate execution, and cleanup after error.
5. Rollback by removing the proposed work-item adapter and restoring legacy pass-local actions.
   Never serialize the accumulator or delete an in-progress world snapshot.

Implementation checkpoint: `src/WorldSession/WorldGeneration/Actions/WorldGenerationTileScanAndControlActionsCommand.cs`
maps source sequences 2042-2047 and 2062 to pass-local intent, explicit adapter interfaces, and a
defensively copied tile-count accumulator. Focused verification passed filter copying, matched-tile
counting, reset isolation, and seam mapping. Callback exception/cancellation, bounds validation,
pass cleanup, duplicate execution, and P19 scheduler integration remain unverified.

Dependencies: P19 pass execution/control, P17 tile facts, C08 condition query, and integration
review for DungeonBounds. No runtime adapter or durable component is enabled by this checkpoint.
### C07 - WorldGenerationTileFramingAndDebugActionsCommand

status: implemented-boundary; implementationStatus: complete-for-boundary; verificationStatus: not-verified; build-blocked-unrelated-hellchest.

1. Map source sequences 2063, 2064, and 2077 exactly once. Replace SpriteBatch in the proposed
   core boundary with a diagnostic sink/adapter input; retain the legacy API only at the edge.
2. Obtain WorldStorage/WorldInteraction framing ownership and the invalidation contract. Define
   whether a frame request is synchronous, queued, or coalesced.
3. Commit frame work through FrameRequestPort, then publish an optional diagnostic record. The
   diagnostic path must be lossy/optional and must not participate in authority success.
4. Add focused tests for neighbor framing, invalidation, coalescing, missing sink, color conversion,
   render failure, repeated draw, and no mutation of tile/structure snapshots.
5. Rollback by disabling the framing/debug adapter and restoring legacy SetFrames/DebugDraw calls.
   Do not remove frame data or use debug output as a recovery source.

Implementation checkpoint: `src/WorldSession/WorldGeneration/Actions/WorldGenerationTileFramingAndDebugActionsCommand.cs`
maps source sequences 2063, 2064, and 2077 to separate `DebugDraw` and `SetFrames` operation kinds.
The boundary uses `ColorRgba` and an explicit `IDiagnosticSink`; `SpriteBatch` is excluded from the
core boundary, and diagnostic commands cannot carry framing-neighbor intent. Focused verification
is not-verified because the affected project fails with unrelated LakePlacement duplicate-type
errors outside P20.

Dependencies: C01 culling query, C02-C05 mutation seams, WorldStorage/WorldInteraction framing
owner, and P19 pass barrier. No runtime framing or diagnostic adapter is enabled by this checkpoint.
### C08 - WorldGenerationConditionsAndSearchesQuery

status: implemented-boundary; implementationStatus: complete-for-boundary; verificationStatus: not-verified; build-blocked-unrelated-hellchest.

1. Map source sequences 2081-2083, 2096-2097, and 2286-2291 exactly once. Copy predicate
   arrays into immutable query inputs or define ownership of any read-only view.
2. Establish the TileSnapshotReader contract and a deterministic coordinate/bounds policy. Do not
   read Main, WorldGen, current time, random state, or mutable global sentinel data.
3. Implement condition evaluation and directional/rectangle search as pure calculations. Return
   explicit not-found and termination values; do not use search failure to write a fallback tile.
4. Add focused tests for type lists, boolean checks, world bounds, compound conditions, distance
   zero/negative, all directions, rectangle dimensions, edges, not-found and repeated evaluation.
5. Rollback by restoring legacy condition/search invocation through an adapter. No tile or world data
   is deleted because this boundary is query-only.

Implementation checkpoint: `src/WorldSession/WorldGeneration/Queries/WorldGenerationConditionsAndSearchesQuery.cs` and
the C08 assertions in `src/WorldSessionFocusedVerifier/Program.cs`. The production query is pure,
defensively copies map/predicate inputs, preserves source direction and rectangle iteration order,
and exposes `SearchTerminationReason` alongside the immutable not-found value. No tile or world
writer is enabled. Dependencies: P17 terrain/tile snapshot, C06 scan context, C01 culling input
where relevant, and P19 pass control. C08 source compilation succeeded in a serial build with 0
warnings and 0 errors; final focused run is not-verified because the current project is blocked by
the unrelated `HellChestLootCycleSystem.cs(16)` compile error.
### C09 - WorldGenerationShapeDataDefinitionQuery

status: implemented-boundary; implementationStatus: complete-for-boundary; verificationStatus: not-verified; build-blocked-unrelated-hellchest.

1. Map source sequences 2093, 2098, 2281-2285, 2292, 2571, and 2585 exactly once. Replace
   live HashSet and Tile[,] references with owned immutable point data and explicit tile snapshots.
2. Agree on Point16/value representation and whether compatibility constructors copy or wrap source
   sets. Preserve unique-point count semantics and explicit diagonal/interior options.
3. Implement count and outline queries as pure functions. Shape application must produce C02-C07
   commands rather than mutate a tile array from the query.
4. Add focused tests for duplicate points, empty/one-point sets, outline modes, point-offset order,
   quit-on-fail policy, snapshot isolation, deterministic repeat, and no write-back.
5. Rollback by restoring the legacy shape adapter and retaining the original source set ownership.
   No world tile data is removed by rollback.

Implementation checkpoint: `src/WorldSession/WorldGeneration/Shapes/WorldGenerationShapeDataDefinitionQuery.cs`.
The boundary owns a defensive immutable unique-point set, a deterministic X-then-Y point snapshot,
Point16-style 16-bit coordinate storage, immutable cardinal/diagonal outline offsets, and pure
`Count`, `OuterOutline`, and `InnerOutline` calculations. Outline results are deduplicated, carry
the explicit `ShapeExecutionPolicy`, and never invoke actions or access live tiles/globals. The
existing C09 assertions in `src/WorldSessionFocusedVerifier/Program.cs` cover duplicate collapse,
defensive input ownership, outline modes, policy propagation, full-block inner exclusion, and
repeat determinism. The current focused build was attempted and is blocked by the unrelated
`HellChestLootCycleSystem.cs(16)` error. The no-build verifier was run only against a stale
pre-C09 artifact and is not accepted as current evidence.

Dependencies: C01 culling query, C08 condition/search query, C10 modifier query, P17 tile snapshot,
and P19 execution owner. No runtime shape adapter or tile writer is enabled by this checkpoint.
### C10 - WorldGenerationShapeModifierStateDefinitionQuery

status: implemented-boundary; implementationStatus: complete-for-boundary; verificationStatus: not-verified; build-blocked-unrelated-hellchest.

1. Map source sequences 2239-2250, 2254, and 2272-2280 exactly once. Define immutable modifier
   records and keep ShapeData references owned by C09 value boundaries.
2. Agree on explicit random-decision input and seed/stream policy for dither, blotches and radial
   operations. No modifier query may read a global random generator.
3. Implement each transformation as a pure operation over an input/output shape value. Require the
   caller to express modifier order; do not hide order in a mutable action chain.
4. Add focused tests for identity, ranges, shape inclusion/exclusion, rectangle/offset/flip,
   random boundary values, deterministic repeated calls, supplied-random stream consumption, and
   input immutability.
5. Rollback by restoring legacy modifier adapters. Do not delete definitions or mutate existing
   shape data during rollback.

Implementation checkpoint: `src/WorldSession/WorldGeneration/Shapes/WorldGenerationShapeModifierStateDefinitionQuery.cs`.
The boundary maps all 22 source parameters to immutable records and returns new C09 values for every
modifier. Random values are explicit inputs; no global random source or shape mutation is used. The
focused build is blocked by the unrelated HellChest error, and the no-build run is stale; verification
remains not-verified.

Dependencies: C09 shape data boundary, C08 conditions/searches, P17 world bounds, P19 generation
execution, and explicit random adapter.
### C11 - WorldGenerationTileWallConditionStateQuery

status: implemented-boundary; implementationStatus: complete-for-boundary; verificationStatus: not-verified; build-blocked-unrelated-hellchest.

1. Map source sequences 2251-2253, 2255-2260, 2261-2262, and 2263-2271 exactly once. Store
   arrays as immutable inputs and preserve source direction order until a verifier approves any
   normalization.
2. Define TileNeighborhoodSnapshot fields for tile type, wall type, liquid type/level, coordinate,
   world bounds, and neighboring cells. Do not let the query read the global tile array.
3. Implement pure predicates and explicit reason codes. Compose conditions in caller order and keep
   diagonal and inclusive choices visible in the input definition.
4. Add focused tests for all type lists, contact directions, diagonal policy, air/not-touching,
   liquid and wall filters, height equality, out-of-world behavior, duplicate IDs, repeated calls,
   and no writes.
5. Rollback by restoring legacy modifier adapters. No tile, wall, liquid, or world state is deleted
   because this boundary is query-only.

Implementation checkpoint: `src/WorldSession/WorldGeneration/Queries/WorldGenerationTileWallConditionStateQuery.cs`.
The boundary maps all twenty source members to copied type lists, immutable cardinal-first direction
offsets, explicit `TileNeighborhoodSnapshot` facts, `ConditionDefinition` values, and
`ConditionResult` failure reasons. It implements allow/skip filters, contact and air-contact checks,
liquid and height predicates, caller-ordered compound evaluation, and explicit missing/out-of-world
neighbor behavior without reading or writing global tile state. The focused verifier assertions are
saved. The current focused build is blocked by the unrelated `HellChestLootCycleSystem.cs(16)`
error; the no-build run used a stale pre-C09 artifact and is not accepted as current evidence.

Dependencies: C08 conditions/searches, C09 shape data, P17 terrain/tile snapshot, P01 liquid facts,
and P19 pass control. No runtime tile/wall/liquid adapter is enabled by this checkpoint.
### C12 - WorldStructurePlanningAndMasksComponent

status: partial-boundary; implementationStatus: partial; verificationStatus: required-generation-id-component-build-passed; full-worldsession-build-passed; existing-focused-verifier-passed; semantic-integration-not-verified.

1. Map source sequences 2084-2085, 2293-2295, 2316-2319, and 2597-2598 exactly once. The saved
   partial source maps the confirmed StructureMap collection state to copied read-only
   `PlannedStructures` and `ProtectedStructures` values and records a required non-negative `GenerationId`
   from the existing reservation identity boundary. Keep DungeonSide and WorldGenRange as
   value/definition inputs; isolate StructureMap lists behind one world-scoped owner.
2. Obtain the canonical world-generation aggregate and P19/P16 reset/completion barriers. Decide
   whether structure plans are saved, recovered, networked, or only generation-session scoped.
3. Define StructurePlanCommitPort with explicit overlap, protected-overlap, duplicate, ordering,
   concurrency, and failure results. The lock must be an adapter detail; no raw object lock crosses
   the domain boundary.
4. Define RangeScaleQuery with explicit world-area/width inputs and rounding policy. Preserve
   source declarations while fallback formulas remain only reference evidence.
5. Add focused tests for registration/protection/overlap, duplicate and concurrent commands, reset
   and abort cleanup, deterministic snapshots, range modes, empty range, rounding, side values,
   persistence round trip, recovery, and one-way client projection.
6. Rollback by disabling the new owner and restoring the legacy StructureMap adapter. Do not
   discard saved structure plans or alter unrelated world lifecycle state during rollback.

The partial component source is saved, including a required non-negative generation identity backed
by the existing structure-reservation identity convention, but canonical implementation is not complete.
The authoritative report confirms the member inventory, while the canonical world aggregate,
canonical world-scoped generation binding, reset/completion barriers, persistence/recovery contract, network
projection, concurrency policy, and unique `StructureMap` owner are not established. Current
`WorldGenRange` scaling is a placeholder, while fallback scaling and extra `DungeonSide`/`StructureMap`
members are version-drift evidence. Creating a second owner or guessing lock/range behavior is
blocked by the evidence protocol.

Dependencies: C08-C11 query/value boundaries, P17 terrain/world dimensions, P19 generation
scheduler, P16 lifecycle/save/recovery, WorldStorage/WorldInteraction structure consumers, and
integration review for persistence/network ownership. The C12-specific focused verifier is not run
because no verifier may be added in this task. The fresh WorldSession compilation and existing
  focused verifier both passed with 0 warnings and 0 errors; the verifier does not cover C12
structure-owner semantics.
## 7. Final Execution Handoff

### Complete source-sequence coverage

The implementation sequence must use the following immutable audit key. Every member is mapped once
in the design document and must remain mapped once during implementation.

| Component | Source sequences | Count | Planned verifier |
| --- | --- | ---: | --- |
| C01 | 2409-2416 | 8 | culling truth table and invalidation |
| C02 | 2048, 2050-2054, 2059-2061, 2065-2066, 2076 | 12 | tile command defaults/order |
| C03 | 2049, 2055-2058, 2072-2073 | 7 | wall/tile-clear/framing order |
| C04 | 2067-2071 | 5 | placement/paint atomicity |
| C05 | 2074-2075, 2078 | 3 | liquid handoff and smoothing |
| C06 | 2042-2047, 2062 | 7 | pass-local cleanup and callback failure |
| C07 | 2063-2064, 2077 | 3 | frame projection and diagnostic isolation |
| C08 | 2081-2083, 2096-2097, 2286-2291 | 11 | pure condition/search matrix |
| C09 | 2093, 2098, 2281-2285, 2292, 2571, 2585 | 10 | shape value and outline matrix |
| C10 | 2239-2250, 2254, 2272-2280 | 22 | modifier and explicit-random matrix |
| C11 | 2251-2253, 2255-2271 | 20 | contact/liquid/height matrix |
| C12 | 2084-2085, 2293-2295, 2316-2319, 2597-2598 | 11 | structure/range/snapshot matrix |
| Total | 12 components, non-contiguous range 2042..2598 | 119 | partial; C09-C11 source-complete; C12 blocked |

### Integration decisions required before implementation

1. P17 must provide immutable terrain/tile/liquid facts and identify which tile/wall state is
   authoritative.
2. P19 must provide pass-local execution, cancellation, continuation, scheduler barriers and
   generation command delivery semantics.
3. P16 must decide world aggregate ownership, reset/completion callbacks, persistence and recovery
   for structure plans and generation snapshots.
4. P01 must own runtime liquid/wiring/spatial/death/teleport behavior; C05 cannot become a second
   liquid authority.
5. WorldStorage and WorldInteraction must assign tile/wall/framing storage, cache invalidation,
   paint and structure-consumer seams.
6. Integration review must decide DungeonBounds ownership, network/client projection, duplicate
   command policy and versioned save formats.

### Rollback matrix

| Boundary | Rollback unit | Data safety rule |
| --- | --- | --- |
| C01/C07 | remove query/cache or framing/debug adapter | Keep committed tile state; never restore from diagnostics |
| C02-C04 | disable tile/wall/placement/paint adapters | Do not delete committed tile/wall data; retain legacy read path |
| C05 | disable liquid handoff adapter | P01 runtime state remains authoritative |
| C06 | remove pass-local work-item adapter | Clear only execution-local accumulators after pass termination |
| C08/C11 | remove query adapters | No world state is changed because these are pure |
| C09/C10 | remove shape/modifier adapters | Retain immutable definitions and legacy shape behavior |
| C12 | disable structure owner adapter | Preserve saved/recovery snapshots; restore legacy StructureMap access |

### Verification gate

Historical verification checkpoint before the unrelated HellChest source was resolved by another
session: the affected project was identified and the following exact serial build was run through
`Build/Tools/Invoke-SerialDotnet.ps1` from the repository root:

```powershell
$serialArgs = @('build','./src/WorldSession/Terraria.WorldSession.csproj','--no-restore','-m:1','-nr:false','-p:UseSharedCompilation=false','-p:MSBuildNodeReuse=false','-p:BuildInParallel=false')
& .\Build\Tools\Invoke-SerialDotnet.ps1 @serialArgs
```

Result for the full affected project `src/WorldSession/Terraria.WorldSession.csproj`: exit code `1`,
0 warnings, 1 error. The error is the unrelated `src/WorldSession/WorldGeneration/Systems/HellChestLootCycleSystem.cs:16`
call to a missing single-argument `TryAdvanceAfterSuccessfulPlacement` overload. No source was
changed to bypass that error. A diagnostic serial build excluding only that source file exited `0`,
with 0 warnings and 0 errors, and produced
`Build/bin/Terraria.WorldSession/Debug/net10.0/Terraria.WorldSession.dll`. No C12-specific verifier
was run because this task forbids adding or modifying verifier/test code.

Focused verifiers must cover source-sequence closure, deterministic pure queries, side-effect
registration, defaults/invariants, owner uniqueness, duplicate/retry behavior, cleanup, persistence
round trip, network projection, and explicit scheduler barriers. Until those checks and integration
decisions exist, verificationStatus remains not-verified.

### Final status

This implementation checkpoint has completed C01-C11 and saved a partial C12 component boundary
with required non-negative generation identity. C12 remains pending for canonical owner, canonical
world-scoped generation binding, lifecycle, persistence, network, range, side, and concurrency
semantics; no runtime tile,
wall, liquid, framing, shape, or structure writer has been
activated by this checkpoint. The authoritative report and other partition documents remain
unchanged; the current manual runner session is being settled as failed because the remaining
canonical integration contract is outside the authorized component-only scope.

~~~yaml
executionStatus: blocked
implementationStatus: partial
verificationStatus: c12-required-generation-id-build-passed; full-worldsession-build-passed; existing-focused-verifier-passed; semantic-integration-not-verified
~~~
