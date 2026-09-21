# P20 世界生成动作、条件、形状与结构规划组件设计

## Document Metadata

~~~yaml
documentType: component-design
designStatus: proposed
partitionId: P20
sessionId: 5249467530774ccd893d62a6da3139ac
runnerSettlementStatus: failed
runnerSettlementSessionId: 5249467530774ccd893d62a6da3139ac
runnerSettlementExitCode: 5
runnerSettlementLockReleased: true
inputReport: D:\TRbackup\NLTX\docs\migration\ledgers\authoritative-20-partitions\P20-World-Generation-Actions-Shapes.md
outputDesignPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\2026-09-11-version4-p20-world-generation-actions-shapes-component-design.md
outputExecutionPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\2026-09-11-version4-p20-world-generation-actions-shapes-component-execution.md
evidenceStatus: partial
nltxStatus: partial
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
  - Version4 declaration evidence closes the 119-member inventory but not every reader, writer, creation/cleanup path, scheduler owner, persistence path, or network path.
  - Current Version4 WorldBuilding bodies contain placeholders for some methods; the no-deletion compiled snapshot is fallback/reference evidence and has known version drift.
  - P17 owns terrain facts and biome/world-generation terrain state; P19 owns generation execution/control and pass scheduling; P16 owns broader world lifecycle and handoff; P01 owns liquid runtime/wiring/spatial/death/teleport concerns.
  - Tile storage/framing/liquid replication, DungeonBounds ownership, generation snapshot/recovery, save/load, and network projection require integration review.
  - Version4 call-site order, storage ownership, combined paint atomicity, callback failure behavior, liquid type semantics, neighbor ordering, random stream policy, and commit failure semantics remain unverified for implementation.
  - C06 focused verification covers filter copying, deterministic accumulation/reset, and explicit seam mapping; callback exception/cancellation behavior, bounds validity, pass cleanup, duplicate execution, and P19 scheduling remain unverified.
  - C07 source boundary is saved, but focused build/run is not verified because the affected project currently fails on unrelated duplicate LakePlacementCapacityDefinition, LakePlacementHistoryComponent, and LakePlacementHistorySnapshot declarations outside P20.
  - Historical checkpoint only: C08 source and verifier assertions were saved while a rebuild was blocked by `src/WorldSession/WorldGeneration/Systems/HellChestLootCycleSystem.cs(16)` calling a missing single-argument `TryAdvanceAfterSuccessfulPlacement` overload outside P20. That earlier C08 artifact predates the final fixture-coordinate edit and cannot prove the final verifier run.
  - Historical checkpoint only: C09 source and verifier assertions were saved while the complete serial build exited 1 with 0 warnings and 2 errors because `src/WorldSession/WorldGeneration/Systems/HellChestLootCycleSystem.cs(16)` called a missing single-argument `TryAdvanceAfterSuccessfulPlacement` overload outside P20. A diagnostic serial build excluding only that file via `-p:DefaultItemExcludes=**/HellChestLootCycleSystem.cs` exited 0 with 0 warnings and 0 errors and produced the affected project artifacts under `Build/bin/`.
  - Historical checkpoint only: C10 source and verifier assertions were saved with the same diagnostic build and no-build focused verifier pass; explicit random-input and fallback modifier behavior remain limited to the boundary contract until the affected project can build without the unrelated external error.
  - Historical checkpoint only: C11 source and verifier assertions were saved with the same diagnostic build and no-build focused verifier pass; exact legacy predicate composition and missing-neighbor distinction remain integration evidence gaps.
  - Historical checkpoint only: the no-build focused verifier was run through `Build/Tools/Invoke-SerialDotnet.ps1` after the diagnostic build and exited 0. Its output reached the existing focused success lines, including the C09-C11 calls in the main verifier flow. This proves the diagnostic artifact, not a full-project build with the excluded external file.
  - Exact persistence/network representation, retry and duplicate delivery policy, scheduler barriers, and owner-system selection remain open across the proposed boundaries.
  - C12 now has a partial component boundary at `src/WorldSession/WorldGeneration/Structures/WorldStructurePlanningAndMasksComponent.cs`; the file copies planned and protected structure rectangles into read-only collections and requires a non-negative generation identity without claiming StructureMap ownership.
  - Existing `StructureReservationIntent` and `InMemoryStructureReservationAdapter` evidence supports a generation-scoped reservation identity, but the C12 canonical owner still does not have independently confirmed canonical world-scoped GenerationId binding, DungeonSide value ownership, WorldGenRange scaling inputs, reset/completion barriers, persistence/recovery format, network projection, or concurrency policy.
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
  - C01, C08, C09, C10, and C11 are implemented as pure explicit-snapshot/value query boundaries; C02-C07 are implemented as transient query/projection boundaries; C12 has a partial component source with required non-negative generation identity and remains blocked for canonical owner implementation.
  - Actions and short-lived work items remain Command payloads; Queries and Definitions cannot write world state or read hidden global state.
  - Structure planning is the only proposed durable authority in this partition; the C12 source is a partial state boundary only. Persistence, concurrency, range/side semantics, and unique ownership remain integration-review until a world-scoped owner is approved.
  - C12 stores copied `PlannedStructures` and `ProtectedStructures` `WorldGenerationRectangle` values, records a required non-negative generation identity, exposes the lists through `IReadOnlyList`, and has no registration attribute because the affected project uses plain component classes and no component registration framework was found in `src`.
  - C05 emits liquid/neighbor intent only, C06 emits pass-local scan/control work items, and C07 emits framing/diagnostic intent without activating runtime storage, persistence, networking, rendering, or a second cross-partition owner. C08 evaluates copied predicates and searches over explicit snapshots without any writer.
  - The implementation gate stopped before C01: the current Version4 WorldBuilding bodies contain placeholder behavior, and the declaration inventory alone cannot establish authoritative runtime semantics.
  - C01 lacks an approved immutable tile-neighborhood snapshot, merge-rule input, edge policy, framing revision, and NLTX framing owner. Implementing the query would invent its public contract and culling behavior.
  - C11 uses explicit predicate composition and neighborhood facts at this boundary, but exact legacy missing-neighbor/error distinction remains partial. C10 uses explicit random inputs at this boundary, but random stream/version integration remains open. C02-C07 lack approved tile, wall, liquid, framing, pass-control, and diagnostic owners. C12 has a partial source boundary for copied structure collections, but canonical ownership remains blocked by missing world aggregate, reset, concurrency, persistence, recovery, and network contracts.
  - P20 source and focused-verifier files now exist for C01-C11, and the partial C12 component source now exists. The current fresh WorldSession and existing WorldSessionFocusedVerifier builds and no-build run passed; the verifier does not cover C12 and no C12-specific verifier was added.
executionGate:
  status: blocked
  blockedBefore: none
  observedAtUtc: 2026-09-12T10:51:38.7445647Z
  sourceEvidence: D:\TRbackup\Version4\Terraria.WorldBuilding\Actions.cs; D:\TRbackup\Version4\Terraria.WorldBuilding\Conditions.cs; D:\TRbackup\Version4\Terraria.WorldBuilding\Modifiers.cs; D:\TRbackup\Version4\Terraria.WorldBuilding\ModShapes.cs; D:\TRbackup\Version4\Terraria.WorldBuilding\Searches.cs; D:\TRbackup\Version4\Terraria\WorldGen.cs
  requiredBeforeResume: approved immutable tile/neighborhood snapshots, merge/framing owner contract, P17/P19/P16/P01 handoff contracts, command commit ports, scheduler barriers, random stream policy, and a focused verifier project
  sourceChanges: src/WorldSession/WorldGeneration/Queries/WorldTileMergeCullStateQuery.cs; src/WorldSession/WorldGeneration/Actions/WorldGenerationTileSetActionsCommand.cs; src/WorldSession/WorldGeneration/Actions/WorldGenerationWallMutationActionsCommand.cs; src/WorldSession/WorldGeneration/Actions/WorldGenerationTilePlacementAndPaintActionsCommand.cs; src/WorldSession/WorldGeneration/Actions/WorldGenerationLiquidAndNeighborActionsCommand.cs; src/WorldSession/WorldGeneration/Actions/WorldGenerationTileScanAndControlActionsCommand.cs; src/WorldSession/WorldGeneration/Actions/WorldGenerationTileFramingAndDebugActionsCommand.cs; src/WorldSession/WorldGeneration/Queries/WorldGenerationConditionsAndSearchesQuery.cs; src/WorldSession/WorldGeneration/Shapes/WorldGenerationShapeDataDefinitionQuery.cs; src/WorldSession/WorldGeneration/Shapes/WorldGenerationShapeModifierStateDefinitionQuery.cs; src/WorldSession/WorldGeneration/Queries/WorldGenerationTileWallConditionStateQuery.cs; src/WorldSession/WorldGeneration/Structures/WorldStructurePlanningAndMasksComponent.cs; src/WorldSession/Terraria.WorldSession.csproj; src/WorldSessionFocusedVerifier/Program.cs; src/WorldSessionFocusedVerifier/Terraria.WorldSessionFocusedVerifier.csproj
  verification: the complete WorldSession build exited 0 with 0 warnings and 0 errors; the existing WorldSessionFocusedVerifier build exited 0 with 0 warnings and 0 errors; its no-build run exited 0 and printed the existing C01/C07/C09/C10/C14 success lines. C12 has no focused verifier; owner/lifecycle/persistence/network semantics remain not-verified.
~~~

The design remains the authority for ownership and compatibility boundaries. C01 and C08-C11 are
implemented as pure explicit-snapshot/value boundaries without new runtime writers. C12 has a
partial source boundary for copied structure collections and required generation identity but
remains blocked for canonical owner integration; unresolved cross-partition owners and Version4
behavior gaps are retained below and are not silently promoted to runtime contracts. The earlier
HellChest failure is retained as historical checkpoint evidence; the current fresh complete build
and existing focused verifier run are recorded above, and no C12-specific verifier exists.

## 1. Scope And Design Summary

P20 covers the WorldGenerationAndEcology formal parent and exactly twelve leaf groups from the
authoritative report: tile and wall mutation actions, placement and paint actions, liquid and
neighbor actions, scan/control actions, framing/debug actions, conditions/searches, shape data,
shape modifiers, tile/wall conditions, tile merge culling, and structure planning/masks. The report
contains 115 fields and 4 properties, for 119 members in total.

The proposed decomposition uses four different boundaries instead of turning every source class
into a persistent ECS component:

1. Short-lived generation actions become explicit Command payloads consumed by owner systems and
   committed through tile, wall, liquid, framing, or world mutation ports.
2. Conditions, searches, culling, scans, and shape operations become pure Query boundaries over
   explicit immutable tile/world snapshots. Scan accumulation is an execution-local accumulator
   owned by a scan system, not a durable query component.
3. Shape data, shape modifiers, DungeonSide, and WorldGenRange are immutable Definition/value
   inputs or query results. They do not own generation execution or global state.
4. StructureMap is a proposed world-scoped planning Component with one owner system. Its lock,
   lists, serialization annotations, and registration API stay behind a seam until persistence,
   concurrency, and cross-partition ownership are approved.

No target source path is created in this planning session. Proposed organization is domain-first
and intentionally avoids generic Shared/Components/ or Manager directories:

| Capability | Proposed directory |
| --- | --- |
| Commands | src/WorldSession/WorldGeneration/Actions/ |
| Queries | src/WorldSession/WorldGeneration/Queries/ |
| Shapes and definitions | src/WorldSession/WorldGeneration/Shapes/ |
| Structures and masks | src/WorldSession/WorldGeneration/Structures/ |

Existing namespaces, public APIs, and external type boundaries remain unchanged during a future
compatibility phase unless a separate implementation decision approves an API change.

## 2. Evidence Baseline

| Evidence source | Location | Fact used | Evidence status |
| --- | --- | --- | --- |
| Authoritative member inventory | D:\TRbackup\NLTX\docs\migration\ledgers\authoritative-20-partitions\P20-World-Generation-Actions-Shapes.md | 12 groups, source sequence range 2042..2598, exact declaration type/path/line/member/type, 119 members | confirmed for inventory; partial for ownership |
| Current Version4 declarations | D:\TRbackup\Version4\Terraria.WorldBuilding\Actions.cs, Conditions.cs, GenBase.cs, GenSearch.cs, Searches.cs, GenShape.cs, GenModShape.cs, ShapeData.cs, ModShapes.cs, Modifiers.cs, StructureMap.cs, WorldGenRange.cs; D:\TRbackup\Version4\Terraria\WorldGen.cs | Declaration shape, static/global coupling, merge-cache locations, and lifecycle anchors | confirmed for declarations; partial for behavior |
| Fallback/reference implementation | D:\TRbackup\无任何删减通过编译\Terraria.WorldBuilding and D:\TRbackup\无任何删减通过编译\Terraria\WorldGen.cs | Actual action, condition, search, shape, modifier, scan, structure, and range behavior used to formulate seams and verifier cases | fallback/reference only; known version drift |
| World-generation lifecycle | D:\TRbackup\Version4\Terraria\WorldGen.cs:6267-6305, 10096-10186 | Callback/save/completion order, asynchronous entry, dungeon setup, reset and GenVars.structures initialization | source evidence; NLTX owner partial |
| Tile merge culling | D:\TRbackup\Version4\Terraria\WorldGen.cs:4018-4065, 71537-71556 | Eight directional cull flags and culling calculation/query boundary | declaration/behavior partial; invalidation owner missing |
| Public API cross-reference | D:\TRbackup\tmodloader-api-docs-stable\index.html, local mirror header tModLoader v2026.07 | Public API-shaped action, condition, search, shape, modifier and structure boundaries | organization reference only |
| ECS organization cross-reference | C:\Users\shan\Downloads\ECS\space-station-14-master\Content.Server\Wires\WiresSystem.cs; Content.Shared\Wires\SharedWiresComponent.cs | Component/System/event/query separation and explicit effect ownership | organization reference only |
| Existing NLTX boundaries | src\WorldSession\WorldGeneration, src\WorldStorage\TileMapStore.cs, src\WorldStorage\TileCellState.cs, src\WorldInteraction\Tiles\TileCellComponent.cs, src\WorldInteraction\Structures\StructureFootprintComponent.cs | Existing world-generation, tile-storage, interaction, and structure boundaries to integrate with | partial implementation evidence |

The current Version4 snapshot contains placeholder bodies such as return new bool (); and
return new Point (). The fallback is therefore used only to recover likely effect ordering and
validation behavior. It is not silently promoted to authoritative Version4 semantics. Fallback
DungeonSide.None, extra StructureMap members, and non-placeholder WorldGenRange.ScaleValue
behavior are version-drift evidence and remain in the evidence gap.

## 3. Component Map And Review Order

| Id | Leaf group | Proposed boundary | Classification | Members |
| --- | --- | --- | --- | ---: |
| C01 | WorldTileMergeCullState | WorldTileMergeCullStateQuery | derived Query | 8 |
| C02 | WorldGenerationTileSetActions | WorldGenerationTileSetActionsCommand | Command payload | 12 |
| C03 | WorldGenerationWallMutationActions | WorldGenerationWallMutationActionsCommand | Command payload | 7 |
| C04 | WorldGenerationTilePlacementAndPaintActions | WorldGenerationTilePlacementAndPaintActionsCommand | Command payload | 5 |
| C05 | WorldGenerationLiquidAndNeighborActions | WorldGenerationLiquidAndNeighborActionsCommand | Command payload | 3 |
| C06 | WorldGenerationTileScanAndControlActions | WorldGenerationTileScanAndControlActionsCommand | execution-local work item | 7 |
| C07 | WorldGenerationTileFramingAndDebugActions | WorldGenerationTileFramingAndDebugActionsCommand | Command plus diagnostic Projection | 3 |
| C08 | WorldGenerationConditionsAndSearches | WorldGenerationConditionsAndSearchesQuery | pure Query | 11 |
| C09 | WorldGenerationShapeData | WorldGenerationShapeDataDefinitionQuery | immutable Definition plus Query | 10 |
| C10 | WorldGenerationShapeModifierState | WorldGenerationShapeModifierStateDefinitionQuery | immutable Definition plus Query | 22 |
| C11 | WorldGenerationTileWallConditionState | WorldGenerationTileWallConditionStateQuery | pure Query | 20 |
| C12 | WorldStructurePlanningAndMasks | WorldStructurePlanningAndMasksComponent | world-scoped planning Component | 11 |

This is review/checkpoint order, not runtime order. Runtime order must be expressed by explicit
scheduler barriers and ownership contracts. The intended data direction is:

~~~text
World seed/config + P17 terrain snapshot + P19 pass input
        |
        v
explicit Conditions/Searches/Shape/Modifier Queries
        |
        v
generation intent -> Command payloads -> owner Systems -> CommitPorts
        |                                  |
        |                                  +--> tile/wall/liquid/framing storage
        |                                  +--> structure planning Component (C12 owner)
        v
read-only results, events, snapshots and diagnostic projections
        |
        +--> P19 pass control / P16 lifecycle handoff / network and persistence adapters
~~~

Queries never write Components. Projections never become authority. Commands carry intent and
short-lived parameters; owner systems determine validation, ordering, duplicate handling and
effect delivery.

## 4. Cross-Partition Ownership

| Boundary | P20 responsibility | Owner requiring integration review |
| --- | --- | --- |
| Terrain facts, tile identity and biome state | Consume an explicit read snapshot and emit tile mutation intent | P17 |
| Generation pass scheduling and barriers | Expose command/query contracts and completion evidence | P19 |
| World reset, save/completion callbacks and broader lifecycle | Publish/read lifecycle events and snapshots | P16 |
| Liquid simulation, wiring, spatial, death and teleport behavior | Emit liquid intent only; do not own runtime liquid simulation | P01 |
| Tile storage and framing | Use a CommitPort/adapter; no direct storage-array authority in P20 | WorldStorage / WorldInteraction integration review |
| Network replication, save/load and recovery | Consume committed snapshots or events; no direct legacy-field replication | integration review |
| DungeonBounds | Accept an opaque value/adapter at the action boundary until owner is identified | integration review |

## 5. Checkpoint Ledger

Detailed member ownership, interface contracts, side-effect registration, and focused verifiers
are appended below one component at a time. A component is reviewed only after its section is
saved in both this document and the execution document and the metadata checkpoint is advanced.

| Id | Boundary | Design checkpoint | Execution checkpoint |
| --- | --- | --- | --- |
| C01 | WorldTileMergeCullStateQuery | complete-design | complete-plan |
| C02 | WorldGenerationTileSetActionsCommand | complete-design | complete-plan |
| C03 | WorldGenerationWallMutationActionsCommand | complete-design | complete-plan |
| C04 | WorldGenerationTilePlacementAndPaintActionsCommand | complete-design | complete-plan |
| C05 | WorldGenerationLiquidAndNeighborActionsCommand | complete-design | complete-plan |
| C06 | WorldGenerationTileScanAndControlActionsCommand | complete-design | complete-plan |
| C07 | WorldGenerationTileFramingAndDebugActionsCommand | complete-design | complete-plan |
| C08 | WorldGenerationConditionsAndSearchesQuery | complete-design | complete-plan |
| C09 | WorldGenerationShapeDataDefinitionQuery | complete-design | complete-plan |
| C10 | WorldGenerationShapeModifierStateDefinitionQuery | complete-design | complete-plan |
| C11 | WorldGenerationTileWallConditionStateQuery | complete-design | complete-plan |
| C12 | WorldStructurePlanningAndMasksComponent | complete-design | complete-plan |

## 6. Global Compatibility And Verification Policy

The compatibility phase preserves original declarations, public APIs, namespaces and external
types. A future adapter may translate legacy action invocation into a command, but it cannot create
a second active writer. A legacy read projection may remain until a focused verifier proves that
the new owner is authoritative and the old writer is disabled. No field is deleted merely because
the proposed type exists.

Every future implementation batch must verify source sequence and declaration mapping exactly once,
command/query side-effect isolation, default and invariant behavior, duplicate/retry handling,
one visible owner for tile/wall/liquid/framing/storage/persistence/network effects, committed
snapshot projection, and explicit scheduler barriers. Actual verification remains not-run for
this design-only session. No compile-capable command is authorized or required here.
## 7. Component Checkpoints

### C01 - WorldTileMergeCullStateQuery

status: proposed. The eight source fields are a directional derived result, not durable ECS
authority. The query receives an immutable tile-neighborhood snapshot, the requested merge region,
the framing revision, and explicit edge policy. It returns a value containing the eight cull
decisions. It does not cache internally; if a cache is later needed, its owner must be the framing
system and its invalidation key must be explicit.

#### Member ownership

| Source sequence | Source type/member | Type | Proposed ownership |
| ---: | --- | --- | --- |
| 2409 | Terraria.WorldGen.TileMergeCullCache.CullTop | bool | WorldTileMergeCullStateQuery result: top direction |
| 2410 | Terraria.WorldGen.TileMergeCullCache.CullBottom | bool | WorldTileMergeCullStateQuery result: bottom direction |
| 2411 | Terraria.WorldGen.TileMergeCullCache.CullLeft | bool | WorldTileMergeCullStateQuery result: left direction |
| 2412 | Terraria.WorldGen.TileMergeCullCache.CullRight | bool | WorldTileMergeCullStateQuery result: right direction |
| 2413 | Terraria.WorldGen.TileMergeCullCache.CullTopLeft | bool | WorldTileMergeCullStateQuery result: top-left direction |
| 2414 | Terraria.WorldGen.TileMergeCullCache.CullTopRight | bool | WorldTileMergeCullStateQuery result: top-right direction |
| 2415 | Terraria.WorldGen.TileMergeCullCache.CullBottomLeft | bool | WorldTileMergeCullStateQuery result: bottom-left direction |
| 2416 | Terraria.WorldGen.TileMergeCullCache.CullBottomRight | bool | WorldTileMergeCullStateQuery result: bottom-right direction |

#### Contract

| Dimension | Proposed contract |
| --- | --- |
| Entity/world scope | Query-scoped value for one tile/merge region; no independent entity |
| State category | Derived result/cache candidate; declaration defaults are false, but semantic defaults require behavior verification |
| Lifecycle | Compute after an explicit tile/framing snapshot; invalidate on relevant tile, wall, frame, bounds, or merge-rule revision |
| Interface | WorldTileMergeCullStateQuery.Calculate(snapshot, region, rules) returns eight booleans and an evidence revision |
| Implementation | Pure directional eligibility calculation; no Main, WorldGen, clock, random, logging, storage or network access |
| Seam | Framing snapshot provider and CommitPort consumer are external; cull query is replaceable by a truth-table verifier |
| Depth | Deep enough to hide direction and edge-policy arithmetic; shallow public result with no storage authority |
| Leverage | Shared by tile merge/framing consumers without exposing tile-array internals |
| Locality | src/WorldSession/WorldGeneration/Queries/WorldTileMergeCullStateQuery.cs |
| Persistence/network | Not persisted and not replicated as authority; a client-facing view must be a one-way projection of committed tile state |
| Side effects | None; invalidation notification belongs to the framing owner |

#### Risks and evidence

The current Version4 declaration and WorldGen locations establish the fields and culling
boundary, but not the complete read/write schedule or invalidation owner. The fallback behavior is
reference evidence only. WorldStorage and WorldInteraction may own the framing revision. P17 owns
terrain facts, while P19 owns pass scheduling. Evidence gap remains open for cache lifetime,
concurrent readers, save/load, and replication.

#### Focused verifier plan

Not run. Before implementation, compare all eight outputs for empty, edge, fully merged,
single-neighbor, diagonal, stale-revision, and changed-framing cases. Assert identical explicit
inputs are deterministic, query calls have no writes, stale revisions are rejected or recomputed
according to the selected policy, and a repeated query does not mutate a cache. Verify that no
persistence or network serializer treats the result as authority.
### C02 - WorldGenerationTileSetActionsCommand

status: implemented-boundary. These twelve fields are constructor/operation parameters for short-lived
generation actions. They are not world state. The proposed command preserves the distinction
between clear, set, keep-wall, slope, half-tile, and solid-swap intent; a TileCommitPort is the
only proposed write seam.

#### Member ownership

| Source sequence | Source type/member | Type | Proposed ownership |
| ---: | --- | --- | --- |
| 2048 | Terraria.WorldBuilding.Actions.ClearTile._frameNeighbors | bool | ClearTile command frame-neighbor option |
| 2050 | Terraria.WorldBuilding.Actions.HalfBlock._value | bool | HalfBlock command value |
| 2051 | Terraria.WorldBuilding.Actions.SetTile._type | ushort | SetTile command tile type |
| 2052 | Terraria.WorldBuilding.Actions.SetTile._doFraming | bool | SetTile command local framing option |
| 2053 | Terraria.WorldBuilding.Actions.SetTile._doNeighborFraming | bool | SetTile command neighbor-framing option |
| 2054 | Terraria.WorldBuilding.Actions.SetTile._clearTile | bool | SetTile command clear-before-set option |
| 2059 | Terraria.WorldBuilding.Actions.SetTileKeepWall._type | ushort | SetTileKeepWall command tile type |
| 2060 | Terraria.WorldBuilding.Actions.SetTileKeepWall._doFraming | bool | SetTileKeepWall local framing option |
| 2061 | Terraria.WorldBuilding.Actions.SetTileKeepWall._doNeighborFraming | bool | SetTileKeepWall neighbor-framing option |
| 2065 | Terraria.WorldBuilding.Actions.SetSlope._slope | int | SetSlope command slope value |
| 2066 | Terraria.WorldBuilding.Actions.SetHalfTile._halfTile | bool | SetHalfTile command value |
| 2076 | Terraria.WorldBuilding.Actions.SwapSolidTile._type | ushort | SwapSolidTile replacement type |

#### Contract

| Dimension | Proposed contract |
| --- | --- |
| Entity/world scope | One generation invocation and target coordinate/shape; never a durable entity component |
| State category | Transient Command payload; source private fields are compatibility mapping, not authority |
| Lifecycle | Construct from explicit generation intent, validate at command boundary, commit once, then release |
| Interface | Tile mutation command with operation kind, tile type/value, target, and explicit framing/neighbor policy |
| Implementation | Owner system translates command to TileCommitPort calls and emits a framing request only when requested |
| Seam | TileCommitPort, FrameRequestPort, and a legacy action adapter; no direct Main.tile or WorldGen mutation |
| Depth | Hide action-specific parameter combinations and validation while keeping effect ordering visible |
| Leverage | All tile generation passes use one ordered write path; P17 supplies facts and P19 supplies pass context |
| Locality | src/WorldSession/WorldGeneration/Actions/WorldGenerationTileSetActionsCommand.cs |
| Defaults/invariants | Type/value/range defaults must match source constructors; slope and tile IDs require explicit validation; keep-wall must not silently clear wall |
| Persistence/network | Commands are not persisted or replicated; committed tile snapshots are the only projection input |
| Side effects | Tile write and optional framing are registered at the owner system; logging/retry policy is outside the payload |

#### Risks and verifier plan

The authoritative report confirms the fields and source declarations, but call-site order,
constructor defaults, tile-array write ownership, and failure behavior remain partial. The current
boundary preserves the observed Version4 defaults for `ClearTile`, `SetTile`, and
`SetTileKeepWall`, carries the source operation distinctions, rejects negative slopes before any
owner can commit, and performs no I/O or mutation. Focused verification passed for these cases;
tile registry validation, duplicate delivery, commit failure, and framing order remain unverified.
### C03 - WorldGenerationWallMutationActionsCommand

status: implemented-boundary. The seven fields describe wall mutation intent and framing policy. They remain
transient Command data. The owner system must make the tile-clearing side effect explicit instead
of allowing a wall action to reach storage through an implicit global helper.

#### Member ownership

| Source sequence | Source type/member | Type | Proposed ownership |
| ---: | --- | --- | --- |
| 2049 | Terraria.WorldBuilding.Actions.ClearWall._frameNeighbors | bool | ClearWall neighbor-framing option |
| 2055 | Terraria.WorldBuilding.Actions.SetWall._type | ushort | SetWall wall type |
| 2056 | Terraria.WorldBuilding.Actions.SetWall._doFraming | bool | SetWall local framing option |
| 2057 | Terraria.WorldBuilding.Actions.SetWall._doNeighborFraming | bool | SetWall neighbor-framing option |
| 2058 | Terraria.WorldBuilding.Actions.SetWall._clearTile | bool | SetWall clear-tile side-effect option |
| 2072 | Terraria.WorldBuilding.Actions.PlaceWall._type | ushort | PlaceWall wall type |
| 2073 | Terraria.WorldBuilding.Actions.PlaceWall._neighbors | bool | PlaceWall neighbor handling option |

#### Contract

| Dimension | Proposed contract |
| --- | --- |
| Entity/world scope | One target coordinate/shape and generation invocation; no durable wall-action entity |
| State category | Transient wall Command; source fields are compatibility data |
| Lifecycle | Construct, validate wall type and target, commit wall mutation, emit explicit tile/framing intents, release |
| Interface | WallMutationCommand with operation kind, wall type, clear-tile policy, local-frame policy and neighbor policy |
| Implementation | WallMutationOwnerSystem calls WallCommitPort and, only when requested, TileCommitPort and FrameRequestPort |
| Seam | WallCommitPort plus explicit TileCommitPort/FrameRequestPort; no direct global tile access |
| Depth | Encapsulate wall action parameter combinations while exposing effect order and failure |
| Leverage | Reuses one wall path for generation passes and leaves runtime wall replication to its owner |
| Locality | src/WorldSession/WorldGeneration/Actions/WorldGenerationWallMutationActionsCommand.cs |
| Defaults/invariants | clearTile and neighbor flags must retain source defaults; wall writes cannot silently mutate tile state |
| Persistence/network | Command is neither persisted nor replicated; committed wall/tile snapshots feed projections |
| Side effects | Wall write, optional tile clear, and framing are separately registered and ordered at the owner system |

#### Risks and verifier plan

Version4 declarations establish all seven fields, but complete action call sites, constructor
defaults, tile-clearing semantics, wall storage owner, and neighbor framing schedule are partial.
Fallback behavior is reference-only. The current boundary preserves the observed defaults for
clear-wall, set-wall, and place-wall, keeps tile clearing explicit, and performs no I/O or state
mutation. Focused verification passed for these invariants; invalid types, duplicate commands,
partial commit failure, and exact owner-system side-effect order remain unverified.
### C04 - WorldGenerationTilePlacementAndPaintActionsCommand

status: implemented-boundary. The five fields combine placement parameters with tile and wall paint
parameters. The proposal keeps these as transient commands but separates the target effect:
placement is a world mutation, paint is a tile/wall attribute mutation, and neither is a rendering
authority.

#### Member ownership

| Source sequence | Source type/member | Type | Proposed ownership |
| ---: | --- | --- | --- |
| 2067 | Terraria.WorldBuilding.Actions.SetTilePaint.paintID | byte | SetTilePaint command paint identifier |
| 2068 | Terraria.WorldBuilding.Actions.SetWallPaint.paintID | byte | SetWallPaint command paint identifier |
| 2069 | Terraria.WorldBuilding.Actions.SetTileAndWallPaint.paintID | byte | combined tile/wall paint command paint identifier |
| 2070 | Terraria.WorldBuilding.Actions.PlaceTile._type | ushort | PlaceTile command tile type |
| 2071 | Terraria.WorldBuilding.Actions.PlaceTile._style | int | PlaceTile command style |

#### Contract

| Dimension | Proposed contract |
| --- | --- |
| Entity/world scope | One target tile/shape and generation invocation; no durable placement entity |
| State category | Transient placement/paint Command payload |
| Lifecycle | Construct from explicit intent, validate target/type/style/paint, commit authority, emit framing or presentation event, release |
| Interface | PlacementPaintCommand with operation kind, target, type/style or paint ID, and explicit framing policy |
| Implementation | Owner system calls approved tile/wall/paint CommitPorts; render/client code consumes a one-way projection |
| Seam | TilePlacementCommitPort, TilePaintCommitPort, WallPaintCommitPort, and FrameRequestPort |
| Depth | Hide legacy action constructors while preserving separate placement, tile paint, and wall paint effects |
| Leverage | Shared generation passes can submit one typed intent without coupling to storage or rendering |
| Locality | src/WorldSession/WorldGeneration/Actions/WorldGenerationTilePlacementAndPaintActionsCommand.cs |
| Defaults/invariants | Paint ID, style and type validation must follow source behavior; combined paint must define atomicity or explicit partial-failure semantics |
| Persistence/network | Persist/replicate committed tile and wall attributes only through their owners; commands and render objects are excluded |
| Side effects | Authority commit first; framing and presentation notifications follow the approved commit order; no SpriteBatch in the command |

#### Risks and verifier plan

The source inventory establishes the five fields, but does not close placement collision rules,
style interpretation, paint range validation, combined-operation atomicity, or network projection.
The current boundary preserves the four operation kinds, paint identifiers, tile type, and the
Version4 default style without exposing rendering or storage types. Focused verification passed
for operation separation, paint mapping, and style preservation; placement collision, paint range,
combined atomicity, duplicate delivery, and network projection remain unverified.
### C05 - WorldGenerationLiquidAndNeighborActionsCommand

status: implemented-boundary. SetLiquid and Smooth carry liquid and neighbor-smoothing intent. They must remain
commands because the values describe a requested mutation, not durable liquid authority. P01 owns
runtime liquid wiring/simulation concerns; P20 exposes only an explicit liquid commit or handoff
port and does not claim the simulation.

#### Member ownership

| Source sequence | Source type/member | Type | Proposed ownership |
| ---: | --- | --- | --- |
| 2074 | Terraria.WorldBuilding.Actions.SetLiquid._type | int | SetLiquid liquid type |
| 2075 | Terraria.WorldBuilding.Actions.SetLiquid._value | byte | SetLiquid liquid amount |
| 2078 | Terraria.WorldBuilding.Actions.Smooth._applyToNeighbors | bool | Smooth neighbor-application option |

#### Contract

| Dimension | Proposed contract |
| --- | --- |
| Entity/world scope | One target cell/shape and generation pass; no persistent liquid-action entity |
| State category | Transient liquid/neighbor Command payload |
| Lifecycle | Validate explicit liquid type/level and target, submit intent, receive commit/handoff result, release |
| Interface | LiquidNeighborCommand with operation kind, liquid type/level, target, and neighbor policy |
| Implementation | Owner system submits LiquidCommitPort or P01 handoff and a separate smoothing/framing request |
| Seam | LiquidCommitPort, NeighborSmoothingPort, and generation-pass barrier |
| Depth | Hide legacy liquid action details while making cross-subsystem ownership and ordering visible |
| Leverage | Allows generation to request liquid changes without embedding runtime liquid simulation |
| Locality | src/WorldSession/WorldGeneration/Actions/WorldGenerationLiquidAndNeighborActionsCommand.cs |
| Defaults/invariants | Liquid range, type support, empty-cell behavior, and neighbor ordering require source call-site evidence |
| Persistence/network | Command is not persisted or replicated; P01/runtime owner decides committed liquid snapshot and replication |
| Side effects | Liquid commit and smoothing are explicit effect registrations; no hidden global liquid reads or writes |

#### Risks and verifier plan

The report confirms three fields, while P01 owns related runtime liquid behavior. The main unresolved
facts are liquid type/value validation, whether Smooth is part of generation framing or simulation,
and retry/duplicate semantics across the handoff. Fallback action bodies are reference evidence
only. The focused verifier passed operation separation, target/type/level mapping, default liquid
level, and explicit neighbor policy without accessing runtime liquid state. Planned integration checks
cover all liquid types and levels, zero/full values,
out-of-range input, blocked cells, smooth-neighbor on/off, order relative to tile/wall changes,
duplicate handoff, and no P20 ownership of runtime liquid state. Liquid type/value validation, Smooth
ownership, handoff ordering, and duplicate/retry semantics remain evidence gaps.

Implementation checkpoint: `src/WorldSession/WorldGeneration/Actions/WorldGenerationLiquidAndNeighborActionsCommand.cs`
maps source sequences 2074, 2075, and 2078 to immutable `SetLiquid` and `Smooth` intent values.
`SetLiquid` retains `liquidLevel = byte.MaxValue`; `Smooth` retains `applyToNeighbors = false`.
The command performs no liquid simulation, tile-array access, storage, networking, persistence,
logging, random access, or P01 state mutation.
### C06 - WorldGenerationTileScanAndControlActionsCommand

status: implemented-boundary. This group contains execution control and scan accumulators, not persistent
world state. Ref<int>, dictionaries, delegates, nested actions, and DungeonBounds references must
live in a pass-local execution context. The proposed name is a command/work-item boundary; scan
results are returned as explicit values or events and are not stored implicitly in a query.

#### Member ownership

| Source sequence | Source type/member | Type | Proposed ownership |
| ---: | --- | --- | --- |
| 2042 | Terraria.WorldBuilding.Actions.ContinueWrapper._action | Terraria.WorldBuilding.GenAction | execution-local continuation command |
| 2043 | Terraria.WorldBuilding.Actions.Count._count | Terraria.Ref<int> | execution-local count sink/port |
| 2044 | Terraria.WorldBuilding.Actions.Scanner._count | Terraria.Ref<int> | execution-local scan count sink/port |
| 2045 | Terraria.WorldBuilding.Actions.TileScanner._tileIds | ushort[] | immutable scan filter input |
| 2046 | Terraria.WorldBuilding.Actions.TileScanner._tileCounts | Dictionary<ushort, int> | execution-local scan accumulator |
| 2047 | Terraria.WorldBuilding.Actions.Custom._perUnit | Terraria.WorldBuilding.GenBase.CustomPerUnitAction | explicit callback adapter scoped to one pass |
| 2062 | Terraria.WorldBuilding.Actions.UpdateBounds._bounds | Terraria.GameContent.Generation.Dungeon.DungeonBounds | opaque bounds command value pending owner resolution |

#### Contract

| Dimension | Proposed contract |
| --- | --- |
| Entity/world scope | One pass execution context; no persistent ECS component for Ref, Dictionary, delegate or callback |
| State category | Transient work item, query accumulator, or command adapter; not world authority |
| Lifecycle | Create at pass start, consume during iteration, emit result, clear at pass end or failure |
| Interface | ScanControlWorkItem with explicit input snapshot, continuation policy, count/result sink, and bounds value |
| Implementation | P19 pass system owns iteration and cancellation; P20 supplies pure accumulation and adapter contracts |
| Seam | CountResultPort, CustomPerUnitAdapter, DungeonBoundsAdapter, and pass cancellation barrier |
| Depth | Hide legacy callback mechanics while exposing ownership, cancellation, and cleanup |
| Leverage | Avoids leaking mutable dictionaries and delegate invocation into persistent ECS state |
| Locality | src/WorldSession/WorldGeneration/Actions/WorldGenerationTileScanAndControlActionsCommand.cs |
| Defaults/invariants | Empty filters and counters, callback exceptions, bounds validity, and continuation semantics require source call-site evidence |
| Persistence/network | No work item or callback is persisted or replicated; only committed scan results/snapshots may project outward |
| Side effects | Counter writes and callback invocation are registered at the pass owner; P20 query code remains deterministic |

#### Risks and verifier plan

The authoritative declaration establishes seven fields but not pass scheduling, callback ownership,
exception behavior, or DungeonBounds lifecycle. Ref<int> and Dictionary are particularly unsafe
as durable components because they hide aliasing and mutable ownership. The focused verifier passed
filter copying, deterministic matched-tile accumulation, reset isolation, and explicit continuation,
count-sink, custom-callback, tile-scanner, and bounds seam mapping. Callback exception and
cancellation, bounds validity, pass cleanup, duplicate execution, and no state surviving the pass
remain planned integration checks.

Implementation checkpoint: `src/WorldSession/WorldGeneration/Actions/WorldGenerationTileScanAndControlActionsCommand.cs`
maps source sequences 2042-2047 and 2062 to immutable operation intent plus pass-local adapter
interfaces. `TileCountAccumulator` copies and de-duplicates filter IDs, records only selected tile
types, returns a frozen count snapshot, and resets explicitly. The command does not persist
`Ref<int>`, `Dictionary`, `GenAction`, delegates, or `DungeonBounds`, and performs no world-array,
network, persistence, logging, clock, random, or runtime state access.
### C07 - WorldGenerationTileFramingAndDebugActionsCommand

status: implemented-boundary. SetFrames is a framing command; DebugDraw is a one-way diagnostic projection
request. Color and SpriteBatch must not enter generation authority or a persistent component. The
proposed boundary therefore has a framing command and a diagnostic adapter behind one source group,
with no debug-to-world feedback path.

#### Member ownership

| Source sequence | Source type/member | Type | Proposed ownership |
| ---: | --- | --- | --- |
| 2063 | Terraria.WorldBuilding.Actions.DebugDraw._color | Color | diagnostic projection input, converted at the rendering edge |
| 2064 | Terraria.WorldBuilding.Actions.DebugDraw._spriteBatch | SpriteBatch | diagnostic rendering adapter dependency, never core state |
| 2077 | Terraria.WorldBuilding.Actions.SetFrames._frameNeighbors | bool | framing command neighbor policy |

#### Contract

| Dimension | Proposed contract |
| --- | --- |
| Entity/world scope | One generation operation or diagnostic frame; no persistent DebugDraw entity |
| State category | Framing Command plus one-way diagnostic Projection request |
| Lifecycle | Validate frame request, commit frame work, publish optional diagnostic record, release rendering dependency |
| Interface | FrameRequest with neighbor policy; DiagnosticDrawRequest with stable color/value data, not SpriteBatch |
| Implementation | FramingOwnerSystem writes through FrameRequestPort; DiagnosticProjectionAdapter translates to client/render services |
| Seam | FrameRequestPort and DiagnosticSink; no render service in domain or query code |
| Depth | Hide rendering API conversion while preserving frame commit order and projection direction |
| Leverage | Keeps debug instrumentation useful without coupling world-generation authority to a client renderer |
| Locality | src/WorldSession/WorldGeneration/Actions/WorldGenerationTileFramingAndDebugActionsCommand.cs |
| Defaults/invariants | Neighbor framing default and frame invalidation must match source behavior; diagnostic output cannot alter command results |
| Persistence/network | Frame state belongs to tile/framing owner; diagnostics are optional local/client projection and not authority |
| Side effects | Framing commit and diagnostic draw are separate registered effects; SpriteBatch is adapter-only |

#### Risks and verifier plan

The source inventory confirms three fields, but framing ownership and DebugDraw call sites are not
closed. The tModLoader action page supports a public debug-draw type, not Version4 private
lifecycle. The C07 source boundary is saved, but its focused verifier could not be built because
the affected project currently contains unrelated duplicate LakePlacement declarations. Planned
checks cover frame-neighbor false/true, frame commit
ordering, invalidation, missing diagnostic sink, color conversion, render failure, repeated draw,
and proof that diagnostic output cannot mutate a tile or structure snapshot.

Implementation checkpoint: `src/WorldSession/WorldGeneration/Actions/WorldGenerationTileFramingAndDebugActionsCommand.cs`
maps source sequences 2063, 2064, and 2077 to separate `DebugDraw` and `SetFrames` operation kinds.
It uses `ColorRgba` and an explicit `IDiagnosticSink`; `SpriteBatch` is not part of the core
boundary, and diagnostic commands cannot carry framing-neighbor intent. Build and runtime
verification remain not-verified due to the unrelated project errors recorded above.
Implementation checkpoint: `src/WorldSession/WorldGeneration/Queries/WorldGenerationConditionsAndSearchesQuery.cs`.
The C08 boundary maps source sequences 2081-2083, 2096-2097, and 2286-2291 to copied condition
values, explicit `TileWorldSnapshot` facts, immutable `NotFound`/`NOT_FOUND` values, and pure
directional/rectangle search definitions. `TileWorldSnapshot` defensively copies its tile map;
condition and search evaluation has no `Main`, `WorldGen`, clock, random, logging, or write-back
dependency. The verifier RED build first failed on the missing C08 type as expected, and a later
build passed after C08 source was added. The final verifier fixture was then corrected to provide a
coordinate outside the source `IsSolid` ten-tile fluff boundary, but the required rebuild was
blocked by the unrelated `HellChestLootCycleSystem.cs(16)` error, so runtime verification remains
not-verified.
### C08 - WorldGenerationConditionsAndSearchesQuery

status: implemented-boundary. Conditions and directional searches are pure qualification over an explicit
world/tile snapshot. The source sentinel and condition arrays are query inputs/results, not global
mutable state. Search execution must return a value and a termination reason rather than mutating
the input snapshot.

#### Member ownership

| Source sequence | Source type/member | Type | Proposed ownership |
| ---: | --- | --- | --- |
| 2081 | Terraria.WorldBuilding.Conditions.IsTile._types | ushort[] | immutable tile-type predicate input |
| 2082 | Terraria.WorldBuilding.Conditions.BoolCheck._theBool | bool | boolean predicate input |
| 2083 | Terraria.WorldBuilding.Conditions.InWorld._fluff | int | explicit world-boundary predicate parameter |
| 2096 | Terraria.WorldBuilding.GenSearch.NOT_FOUND | Point | immutable not-found result sentinel/value |
| 2097 | Terraria.WorldBuilding.GenSearch._conditions | Terraria.WorldBuilding.GenCondition[] | immutable search predicate list |
| 2286 | Terraria.WorldBuilding.Searches.Left._maxDistance | int | left-search bound |
| 2287 | Terraria.WorldBuilding.Searches.Right._maxDistance | int | right-search bound |
| 2288 | Terraria.WorldBuilding.Searches.Down._maxDistance | int | down-search bound |
| 2289 | Terraria.WorldBuilding.Searches.Up._maxDistance | int | up-search bound |
| 2290 | Terraria.WorldBuilding.Searches.Rectangle._width | int | rectangle-search width |
| 2291 | Terraria.WorldBuilding.Searches.Rectangle._height | int | rectangle-search height |

#### Contract

| Dimension | Proposed contract |
| --- | --- |
| Entity/world scope | Query invocation over one immutable world/tile snapshot; no entity or durable component |
| State category | Pure Query definition and result; sentinel is value semantics |
| Lifecycle | Build predicates/search bounds, evaluate once or repeatedly against the same snapshot, discard result |
| Interface | EvaluateCondition(snapshot, point, predicate) and Search(snapshot, origin, searchDefinition) return bool/point/result reason |
| Implementation | Pure coordinate checks, type checks, boolean checks, and directional/rectangle search; no global reads |
| Seam | TileSnapshotReader supplies immutable facts; a verifier can replace it with a table fixture |
| Depth | Hide scan order and boundary arithmetic; expose explicit bounds and result reason |
| Leverage | All generation eligibility and search consumers share deterministic qualification semantics |
| Locality | src/WorldSession/WorldGeneration/Queries/WorldGenerationConditionsAndSearchesQuery.cs |
| Defaults/invariants | Empty condition list, max distance/width/height validation, world-edge inclusivity, and NOT_FOUND equality require source confirmation |
| Persistence/network | No query state is persisted or replicated; only caller-selected results may influence a command |
| Side effects | None; queries cannot log, publish events, mutate arrays, or advance random state |

#### Risks and verifier plan

The declaration inventory closes the eleven members, while complete search iteration and boundary
semantics are only partially evidenced. NOT_FOUND is a public mutable static in the source
inventory and must become an immutable value boundary without changing compatibility behavior until
approved. Verification is not-run. Planned checks cover each condition, empty/compound predicates,
negative and zero bounds, world edges, all four directions, rectangle inclusion, not-found identity,
determinism, and proof of no snapshot mutation.
### C09 - WorldGenerationShapeDataDefinitionQuery

status: implemented-boundary. Shape data is an immutable point-set definition plus query-facing count and
outline operations. The source fields data, points, quit-on-fail, offsets, and tile access property
must not become a mutable world component. The current tile-array property is a global bridge and
is therefore an adapter input in the proposed design.

#### Member ownership

| Source sequence | Source type/member | Type | Proposed ownership |
| ---: | --- | --- | --- |
| 2093 | Terraria.WorldBuilding.GenModShape._data | Terraria.WorldBuilding.ShapeData | immutable shape-data input/view |
| 2098 | Terraria.WorldBuilding.GenShape._quitOnFail | bool | shape execution policy input |
| 2281 | Terraria.WorldBuilding.ModShapes.OuterOutline.POINT_OFFSETS | int[] | immutable outline-neighbor definition |
| 2282 | Terraria.WorldBuilding.ModShapes.OuterOutline._useDiagonals | bool | outer-outline query policy |
| 2283 | Terraria.WorldBuilding.ModShapes.OuterOutline._useInterior | bool | outer-outline inclusion policy |
| 2284 | Terraria.WorldBuilding.ModShapes.InnerOutline.POINT_OFFSETS | int[] | immutable outline-neighbor definition |
| 2285 | Terraria.WorldBuilding.ModShapes.InnerOutline._useDiagonals | bool | inner-outline query policy |
| 2292 | Terraria.WorldBuilding.ShapeData._points | HashSet<Terraria.DataStructures.Point16> | immutable point-set storage with defensive ownership |
| 2571 | Terraria.WorldBuilding.GenBase._tiles | Terraria.Tile[,] | external tile snapshot/adapter input, not component state |
| 2585 | Terraria.WorldBuilding.ShapeData.Count | int | derived count query over the point set |

#### Contract

| Dimension | Proposed contract |
| --- | --- |
| Entity/world scope | Definition/query scoped to one shape operation; no independent world entity |
| State category | Immutable Definition, value set and pure Query |
| Lifecycle | Construct from copied/owned points, evaluate outline/count, discard or retain as immutable catalog value |
| Interface | ShapeDataDefinition(points) and ShapeDataQuery.Count/Outline(snapshot, policy) return immutable values |
| Implementation | Defensive copy or immutable set; explicit point offsets and quit-on-fail policy; tile access supplied as snapshot |
| Seam | PointSet value boundary, TileSnapshotReader, and shape-output result; no live Terraria.Tile[,] dependency in core |
| Depth | Hide set ownership and outline neighbor arithmetic without hiding output ordering or failure policy |
| Leverage | Shape modifiers and generation passes reuse the same geometry value semantics |
| Locality | src/WorldSession/WorldGeneration/Shapes/WorldGenerationShapeDataDefinitionQuery.cs |
| Defaults/invariants | Duplicate points collapse as source HashSet does; count equals unique points; diagonal and interior flags remain explicit |
| Persistence/network | Shape data is definition/snapshot input; no live point-set or tile array is replicated; persistence requires an approved versioned format |
| Side effects | None in definition/query; shape application emits later mutation Commands |

#### Risks and verifier plan

The report confirms ten members, but aliasing behavior, point ordering, outline boundary treatment,
and tile-array lifetime are not fully closed. The fallback is reference evidence; it does not
establish the authoritative Version4 contract. This boundary closes point-set aliasing and output
ordering; live tile-array lifetime remains outside the core. Focused verification is pending. Planned checks cover
duplicate-point collapse, count, defensive ownership, empty sets, diagonal/interior outlines,
point offsets, quit-on-fail propagation, tile snapshot isolation, deterministic repeated queries,
and no mutation of the input set or snapshot.

Implementation checkpoint: `src/WorldSession/WorldGeneration/Shapes/WorldGenerationShapeDataDefinitionQuery.cs`.
The C09 boundary stores unique shape points in an immutable set and exposes a deterministic
X-then-Y ordered snapshot. `ShapePoint` preserves Point16-style 16-bit coordinate storage;
`OuterOutline` and `InnerOutline` use immutable cardinal/diagonal offset definitions, deduplicate
their output, preserve the explicit `ShapeExecutionPolicy`, and perform no action invocation or
tile/global-state access. The source file and existing C09 verifier assertions are saved; the
affected project has not yet been rebuilt or run after this checkpoint.
### C10 - WorldGenerationShapeModifierStateDefinitionQuery

status: implemented-boundary. The 22 fields are modifier parameters and immutable direction tables, not
persistent world state. Modifiers operate on an explicit shape value and return a new value or
command-ready result. Dither and radial operations must receive an explicit random decision/input
so a pure Query does not consume hidden global randomness.

#### Member ownership

| Source sequence | Source type/member | Type | Proposed ownership |
| ---: | --- | --- | --- |
| 2239 | Terraria.WorldBuilding.Modifiers.ShapeScale._scale | int | immutable scale parameter |
| 2240 | Terraria.WorldBuilding.Modifiers.Expand._xExpansion | int | immutable horizontal expansion |
| 2241 | Terraria.WorldBuilding.Modifiers.Expand._yExpansion | int | immutable vertical expansion |
| 2242 | Terraria.WorldBuilding.Modifiers.RadialDither._innerRadius | double | immutable inner radius |
| 2243 | Terraria.WorldBuilding.Modifiers.RadialDither._outerRadius | double | immutable outer radius |
| 2244 | Terraria.WorldBuilding.Modifiers.Blotches._minX | int | immutable blotch minimum X |
| 2245 | Terraria.WorldBuilding.Modifiers.Blotches._minY | int | immutable blotch minimum Y |
| 2246 | Terraria.WorldBuilding.Modifiers.Blotches._maxX | int | immutable blotch maximum X |
| 2247 | Terraria.WorldBuilding.Modifiers.Blotches._maxY | int | immutable blotch maximum Y |
| 2248 | Terraria.WorldBuilding.Modifiers.Blotches._chance | double | immutable blotch chance |
| 2249 | Terraria.WorldBuilding.Modifiers.InShape._shapeData | Terraria.WorldBuilding.ShapeData | immutable inclusion shape input |
| 2250 | Terraria.WorldBuilding.Modifiers.NotInShape._shapeData | Terraria.WorldBuilding.ShapeData | immutable exclusion shape input |
| 2254 | Terraria.WorldBuilding.Modifiers.Checkerboard._percentile | int | immutable checkerboard percentile |
| 2272 | Terraria.WorldBuilding.Modifiers.RectangleMask._xMin | int | immutable rectangle minimum X |
| 2273 | Terraria.WorldBuilding.Modifiers.RectangleMask._yMin | int | immutable rectangle minimum Y |
| 2274 | Terraria.WorldBuilding.Modifiers.RectangleMask._xMax | int | immutable rectangle maximum X |
| 2275 | Terraria.WorldBuilding.Modifiers.RectangleMask._yMax | int | immutable rectangle maximum Y |
| 2276 | Terraria.WorldBuilding.Modifiers.Offset._xOffset | int | immutable X offset |
| 2277 | Terraria.WorldBuilding.Modifiers.Offset._yOffset | int | immutable Y offset |
| 2278 | Terraria.WorldBuilding.Modifiers.Dither._failureChance | double | immutable dither failure chance |
| 2279 | Terraria.WorldBuilding.Modifiers.Flip._flipX | bool | immutable horizontal flip policy |
| 2280 | Terraria.WorldBuilding.Modifiers.Flip._flipY | bool | immutable vertical flip policy |

#### Contract

| Dimension | Proposed contract |
| --- | --- |
| Entity/world scope | Query over one immutable shape value and explicit modifier context; no entity |
| State category | Immutable modifier Definition and pure transformation Query |
| Lifecycle | Construct definition, apply in explicit caller order, return new shape/result, discard definition or retain catalog |
| Interface | Apply(shape, definition, explicitRandomInput, worldBounds) returns shape/result without mutation |
| Implementation | Scale/expand/outline/dither/blotch/mask/offset/flip transformations over C09 values |
| Seam | Shape value input, random-decision port, world-bounds input, and command output boundary |
| Depth | Centralize geometry transformation while exposing modifier order and random policy |
| Leverage | Generation passes can compose modifiers without inheriting mutable GenAction behavior |
| Locality | src/WorldSession/WorldGeneration/Shapes/WorldGenerationShapeModifierStateDefinitionQuery.cs |
| Defaults/invariants | Radius ordering, chance range, percentile range, rectangle inclusivity and flip/offset semantics require source confirmation |
| Persistence/network | Definitions may be versioned catalog data; transformed shape and random decisions are not network authority |
| Side effects | None in Query; random adapter supplies values, and later systems submit mutation commands |

#### Risks and verifier plan

The declaration inventory confirms all 22 parameters. The current Version4 bodies do not close
all transformation behavior, so the compiled fallback is reference-only. Dither, blotches and
radial behavior can consume randomness; the implementation must record explicit random input and
stream/version policy. Focused verification is pending. Planned checks cover identity/defaults, scale,
expansion, shape inclusion/exclusion, rectangle and offset boundaries, flip combinations, radius
ordering, chance/percentile validation, deterministic supplied-random results, modifier order, and
no mutation of input shape.

Implementation checkpoint: `src/WorldSession/WorldGeneration/Shapes/WorldGenerationShapeModifierStateDefinitionQuery.cs`.
The C10 boundary maps all 22 source parameters to immutable definition records and returns new C09
shape values for scale, expansion, radial dither, blotches, shape inclusion/exclusion, checkerboard,
rectangle masking, offset, dither, and flip. Randomness is supplied through explicit values and
records; no global random source, tile array, action callback, or output mutation is used. The
 focused verifier assertions are saved; a current serial build is blocked by the unrelated `HellChestLootCycleSystem.cs(16)` error, so this boundary remains source-complete but not verified.
### C11 - WorldGenerationTileWallConditionStateQuery

status: implemented-boundary. These twenty fields parameterize pure conditions and contact checks. They are
not mutable world state. The query receives an immutable tile/wall/liquid/height neighborhood and
returns eligibility. Direction arrays are immutable definitions; diagonal policy, type lists,
liquid levels, and height inclusivity remain explicit inputs.

#### Member ownership

| Source sequence | Source type/member | Type | Proposed ownership |
| ---: | --- | --- | --- |
| 2251 | Terraria.WorldBuilding.Modifiers.Conditions._conditions | Terraria.WorldBuilding.GenCondition[] | immutable compound condition list |
| 2252 | Terraria.WorldBuilding.Modifiers.OnlyWalls._types | ushort[] | wall-type allow-list |
| 2253 | Terraria.WorldBuilding.Modifiers.OnlyTiles._types | ushort[] | tile-type allow-list |
| 2255 | Terraria.WorldBuilding.Modifiers.IsTouching.DIRECTIONS | int[] | immutable contact direction table |
| 2256 | Terraria.WorldBuilding.Modifiers.IsTouching._useDiagonals | bool | contact diagonal policy |
| 2257 | Terraria.WorldBuilding.Modifiers.IsTouching._tileIds | ushort[] | touching tile-type allow-list |
| 2258 | Terraria.WorldBuilding.Modifiers.NotTouching.DIRECTIONS | int[] | immutable contact direction table |
| 2259 | Terraria.WorldBuilding.Modifiers.NotTouching._useDiagonals | bool | negative-contact diagonal policy |
| 2260 | Terraria.WorldBuilding.Modifiers.NotTouching._tileIds | ushort[] | not-touching tile-type allow-list |
| 2261 | Terraria.WorldBuilding.Modifiers.IsTouchingAir.DIRECTIONS | int[] | immutable air-contact direction table |
| 2262 | Terraria.WorldBuilding.Modifiers.IsTouchingAir._useDiagonals | bool | air-contact diagonal policy |
| 2263 | Terraria.WorldBuilding.Modifiers.SkipTiles._types | ushort[] | skipped tile-type list |
| 2264 | Terraria.WorldBuilding.Modifiers.HasLiquid._liquidType | int | required liquid type |
| 2265 | Terraria.WorldBuilding.Modifiers.HasLiquid._liquidLevel | int | minimum/required liquid level |
| 2266 | Terraria.WorldBuilding.Modifiers.NoLiquid._liquidType | int | excluded liquid type |
| 2267 | Terraria.WorldBuilding.Modifiers.SkipWalls._types | ushort[] | skipped wall-type list |
| 2268 | Terraria.WorldBuilding.Modifiers.IsAboveHeight._y | int | height threshold |
| 2269 | Terraria.WorldBuilding.Modifiers.IsAboveHeight._inclusive | bool | above-height equality policy |
| 2270 | Terraria.WorldBuilding.Modifiers.IsBelowHeight._y | int | height threshold |
| 2271 | Terraria.WorldBuilding.Modifiers.IsBelowHeight._inclusive | bool | below-height equality policy |

#### Contract

| Dimension | Proposed contract |
| --- | --- |
| Entity/world scope | Query invocation over an explicit tile neighborhood and world height context |
| State category | Immutable predicate definitions and pure Query results |
| Lifecycle | Construct predicate definitions, evaluate against snapshot, discard or retain immutable definitions |
| Interface | Evaluate(snapshot, point, conditionDefinition) returns a bool and optional reason code |
| Implementation | Type-list, liquid, wall, height and contact checks over explicit facts |
| Seam | TileNeighborhoodSnapshot, immutable direction table, and condition result reason |
| Depth | Centralize contact/height edge semantics while keeping caller-selected composition explicit |
| Leverage | Shared generation eligibility without exposing Main.tile or mutable modifier arrays |
| Locality | src/WorldSession/WorldGeneration/Queries/WorldGenerationTileWallConditionStateQuery.cs |
| Defaults/invariants | Empty lists, duplicate IDs, diagonal ordering, liquid comparison, and inclusive thresholds require source confirmation |
| Persistence/network | No condition definition or query result is authority; only committed world changes may replicate |
| Side effects | None; no logging, random, storage, liquid simulation, or event publication |

#### Risks and verifier plan

The authoritative declaration confirms all twenty members, including three direction tables, but
does not close exact neighbor ordering or predicate composition. The fallback is reference-only.
Focused verification is pending. Planned checks cover allow/skip lists, empty lists, duplicate IDs,
diagonal on/off, contact and air contact, not-touching, liquid type/level, wall filters, above and
below thresholds with inclusive true/false, out-of-world neighbors, deterministic results, and
no snapshot mutation.

Implementation checkpoint: `src/WorldSession/WorldGeneration/Queries/WorldGenerationTileWallConditionStateQuery.cs`.
The C11 boundary maps all twenty source members to copied type lists, immutable cardinal-first
direction offsets, explicit `TileNeighborhoodSnapshot` facts, `ConditionDefinition` values, and
`ConditionResult` failure reasons. It implements allow/skip filters, contact and air-contact checks,
liquid and height predicates, caller-ordered compound evaluation, and explicit missing/out-of-world
neighbor behavior without reading or writing global tile state. The source file and focused verifier
assertions are saved. A current serial build is blocked by the unrelated `HellChestLootCycleSystem.cs(16)`
error, and the only no-build run used a stale pre-C09 artifact; therefore C11 is not verified.
### C12 - WorldStructurePlanningAndMasksComponent

status: partial-boundary; implementationStatus: partial; verificationStatus: required-generation-id-component-build-passed; full-worldsession-build-passed; existing-focused-verifier-passed; semantic-integration-not-verified.
This is the only P20 boundary proposed as durable world-scoped authority. It
contains structure registration/protection and range/mask value inputs, but it is not authorized
to become the canonical owner until world scope, concurrency, persistence, recovery, and
cross-partition consumers are approved. DungeonSide constants and WorldGenRange are definitions or
value semantics, not independent entities. The saved partial boundary only copies the two confirmed
structure collections into read-only `WorldGenerationRectangle` lists; it does not implement the
world owner, locking, range calculations, side constants, or lifecycle behavior.

#### Member ownership

| Source sequence | Source type/member | Type | Proposed ownership |
| ---: | --- | --- | --- |
| 2084 | Terraria.WorldBuilding.DungeonSide.Left | short | immutable dungeon-side definition value |
| 2085 | Terraria.WorldBuilding.DungeonSide.Right | short | immutable dungeon-side definition value |
| 2293 | Terraria.WorldBuilding.StructureMap._structures | List<Rectangle> | world-scoped planned structure collection behind owner system |
| 2294 | Terraria.WorldBuilding.StructureMap._protectedStructures | List<Rectangle> | world-scoped protected structure collection behind owner system |
| 2295 | Terraria.WorldBuilding.StructureMap._lock | object | concurrency adapter/seam, not domain data |
| 2316 | Terraria.WorldBuilding.WorldGenRange.Empty | WorldGenRange | immutable empty range value |
| 2317 | Terraria.WorldBuilding.WorldGenRange.Minimum | int | immutable range minimum |
| 2318 | Terraria.WorldBuilding.WorldGenRange.Maximum | int | immutable range maximum |
| 2319 | Terraria.WorldBuilding.WorldGenRange.ScaleWith | Terraria.WorldBuilding.WorldGenRange.ScalingMode | immutable range scaling definition |
| 2597 | Terraria.WorldBuilding.WorldGenRange.ScaledMinimum | int | derived scaled-minimum query |
| 2598 | Terraria.WorldBuilding.WorldGenRange.ScaledMaximum | int | derived scaled-maximum query |

#### Source checkpoint

- Implemented source: `src/WorldSession/WorldGeneration/Structures/WorldStructurePlanningAndMasksComponent.cs`.
- Implemented confirmed state: copied `PlannedStructures` and `ProtectedStructures` values exposed as `IReadOnlyList<WorldGenerationRectangle>`, plus a required non-negative `GenerationId` that binds the component state to a generation scope.
- Registration: none; no component registration attribute or registration framework was found in the affected `src` project, and no guessed registration key was added.
- Deferred source members: DungeonSide constants, WorldGenRange values/properties, StructureMap lock, canonical world-scoped generation binding, and all owner/lifecycle/persistence/network behavior remain blocked by the evidence gap.
- Dependency impact: reuses the existing `WorldGenerationRectangle` value type and the generation identity convention evidenced by `StructureReservationIntent`; no System, Query, Command, Adapter, Projection, interface, test, project, or configuration file was added or modified.

#### Contract

| Dimension | Proposed contract |
| --- | --- |
| Entity/world scope | One world-generation aggregate owns structure plans; range and side are value definitions |
| State category | Structure lists are candidate authoritative planning state; lock is an adapter, range values are Definition/Query |
| Lifecycle | Initialize at world-generation reset, register/query structures through one owner, snapshot before save/recovery, clear at world completion or abort |
| Interface | StructurePlanCommand, StructurePlanQuery, RangeScaleQuery, and immutable snapshot/projection |
| Implementation | WorldStructurePlanningOwnerSystem validates overlap/protection and serializes committed plan changes; range/side calculators stay pure |
| Seam | StructurePlanCommitPort, concurrency adapter, persistence snapshot adapter, network projection adapter |
| Depth | Hide list/lock mechanics and overlap rules while exposing explicit ownership and commit results |
| Leverage | Gives generation passes one planned-structure authority without claiming tile placement or lifecycle ownership |
| Locality | src/WorldSession/WorldGeneration/Structures/WorldStructurePlanningAndMasksComponent.cs |
| Defaults/invariants | Protected overlap, duplicate registration, order, empty range, scaling mode, integer rounding and side values require source/version confirmation |
| Persistence/network | Snapshot format, save/load, recovery, replication and unique ownership are integration-review; no raw list/lock is exposed |
| Side effects | Commit, persistence snapshot and projection are explicit adapters; queries and value definitions have none |

#### Risks and verifier plan

The current Version4 declaration confirms the two lists, lock, range values, properties, and side
constants. The existing `StructureReservationIntent` confirms that the surrounding reservation
boundary can carry a non-negative `GenerationId`, but it does not establish canonical world-scoped
C12 binding
or make its adapter the `StructureMap` owner. The fallback exposes additional StructureMap methods
and a DungeonSide.None value, so those are version-drift evidence rather than current API facts.
Current range scaling is a placeholder while fallback contains scaling formulas; exact
world-area/width inputs and rounding remain unresolved. The component now records a required
non-negative generation identity while retaining the confirmed copied structure collections. The canonical
world aggregate, reset/completion barriers, persistence/recovery contract, network projection,
concurrency policy, and unique `StructureMap` owner are not established, so the component remains
partial and the implementation gate remains blocked.
The fresh WorldSession build and existing focused verifier pass, but c12-verifier-not-run remains
because no verifier may be added in this task; semantic integration is still unverified. Planned
checks cover protected/unprotected overlap,
duplicate and concurrent registration, deterministic snapshot order, reset/abort cleanup, empty
range, each scaling mode and rounding, side-value compatibility, persistence round trip, recovery,
and one-way network projection.
## 8. Complete Member Coverage Audit

The following source-sequence partition is the audit key for all 119 report members. Every sequence
appears once in the component sections above; no sequence is assigned to two proposed boundaries.

| Component | Source sequences | Count | verificationStatus |
| --- | --- | ---: | --- |
| C01 WorldTileMergeCullStateQuery | 2409-2416 | 8 | not-run |
| C02 WorldGenerationTileSetActionsCommand | 2048, 2050-2054, 2059-2061, 2065-2066, 2076 | 12 | not-run |
| C03 WorldGenerationWallMutationActionsCommand | 2049, 2055-2058, 2072-2073 | 7 | not-run |
| C04 WorldGenerationTilePlacementAndPaintActionsCommand | 2067-2071 | 5 | not-run |
| C05 WorldGenerationLiquidAndNeighborActionsCommand | 2074-2075, 2078 | 3 | not-run |
| C06 WorldGenerationTileScanAndControlActionsCommand | 2042-2047, 2062 | 7 | not-run |
| C07 WorldGenerationTileFramingAndDebugActionsCommand | 2063-2064, 2077 | 3 | not-run |
| C08 WorldGenerationConditionsAndSearchesQuery | 2081-2083, 2096-2097, 2286-2291 | 11 | not-run |
| C09 WorldGenerationShapeDataDefinitionQuery | 2093, 2098, 2281-2285, 2292, 2571, 2585 | 10 | source-complete; build-blocked; not-verified |
| C10 WorldGenerationShapeModifierStateDefinitionQuery | 2239-2250, 2254, 2272-2280 | 22 | source-complete; build-blocked; not-verified |
| C11 WorldGenerationTileWallConditionStateQuery | 2251-2253, 2255-2271 | 20 | source-complete; build-blocked; not-verified |
| C12 WorldStructurePlanningAndMasksComponent | 2084-2085, 2293-2295, 2316-2319, 2597-2598 | 11 | partial-boundary; required-generation-id-component-build-passed; existing-focused-verifier-passed; semantic-integration-not-verified |
| Total | 2042..2598, non-contiguous | 119 | partial; C09-C11 source-complete; C12 blocked |

### Checkpoint evidence history

Each row records the decision retained when the component checkpoint was saved. The historical
record is design evidence, not proof that a verifier ran.

| Component | evidence-gap at checkpoint | blocking-decision at checkpoint | verificationStatus |
| --- | --- | --- | --- |
| C01 | Cull readers, cache lifetime and invalidation owner were partial | Query only; no cache authority or replication | not-run |
| C02 | Tile call-site order, defaults and TileCommitPort owner were partial | Keep all tile actions as transient commands | not-run |
| C03 | Wall/tile clear ordering and WallCommitPort owner were partial | Separate wall, tile-clear and framing seams | not-run |
| C04 | Placement style, paint range and combined atomicity were partial | Keep placement/paint out of rendering and persistence commands | not-run |
| C05 | Liquid validation and P01 handoff ordering were partial | P20 emits intent only; P01 owns runtime liquid | not-run |
| C06 | Pass lifetime, callback failure and DungeonBounds owner were partial | Ref, dictionary and delegate remain execution-local | not-run |
| C07 | Framing owner and DebugDraw call sites were partial | Debug output is one-way; SpriteBatch stays at adapter edge | not-run |
| C08 | Search boundary and NOT_FOUND compatibility semantics were partial | Pure snapshot query; no global reads or write-back | not-run |
| C09 | Point aliasing, outline order and tile-array lifetime were partial | Immutable shape value; Tile array remains outside the pure query boundary | source-complete; not-verified |
| C10 | Modifier formulas, random stream and order were partial | Explicit random input; no hidden global randomness | source-complete; not-verified |
| C11 | Direction ordering and inclusive/contact semantics were partial | Pure tile/wall/liquid/height query over explicit facts; missing neighbors are explicit | source-complete; not-verified |
| C12 | Persistence, concurrency, range formulas, side values and unique owner remain partial; only copied structure collections and required non-negative generation identity are implemented | Structure authority remains blocked until integration approval and current Version4 semantics are confirmed | partial-boundary; required-generation-id-component-build-passed; existing-focused-verifier-passed; semantic-integration-not-verified |

## 9. Side-Effect Registry And Integration Handoff

| Effect or boundary | P20 contract | Handoff owner/status |
| --- | --- | --- |
| Tile and wall writes | C02-C04 commands, ordered commit ports, explicit failure result | WorldStorage/WorldInteraction and P17 integration-review |
| Liquid and smoothing | C05 intent only, no runtime simulation | P01 plus P17 integration-review |
| Scan/callback execution | C06 pass-local work item and explicit result sink | P19 execution owner; DungeonBounds integration-review |
| Framing/cache invalidation | C01 and C07 query/commands with explicit revision | WorldStorage/WorldInteraction integration-review |
| Conditions/searches | C08 and C11 pure queries over snapshots | P17 snapshot provider; P19 caller |
| Shapes/modifiers | C09-C10 immutable values and pure transformations | P19 execution owner; explicit random adapter |
| Structures/masks | C12 proposed world-scoped planning authority and snapshot seam | P16/P19 lifecycle and integration-review |
| Persistence/recovery | Read committed snapshots only; no raw lists, locks, commands or queries | P16/integration-review |
| Network/client projection | One-way projection from committed snapshots/events | integration-review |
| Scheduler order | Explicit barriers only; document order is not runtime order | P19/integration-review |

## 10. Compatibility, Rollback And Acceptance

Compatibility keeps the Version4 source declarations, public APIs, namespaces, and external types
unchanged until a future implementation batch proves equivalence. A legacy adapter may translate
one source action into one command, but it must not activate a second writer. Query and Definition
boundaries cannot mutate legacy state. StructureMap remains behind an adapter until persistence,
concurrency, and world ownership are approved.

Each checkpoint can roll back by disabling only its proposed adapter and restoring the previous
legacy path. Rollback must not delete tile, wall, liquid, structure, save, or network data. A
future implementation may advance only after its focused verifier proves defaults, edge behavior,
effect order, duplicate/retry policy, cleanup, and compatibility projection.

This P20 session has implemented C01, C08, C09, C10, and C11 as explicit pure query/value boundaries
and has saved a partial C12 component boundary for the two confirmed structure collections. It has
not completed canonical C12 ownership, runtime owner integration, persistence, network, or
behavior-equivalence checks. The evidence-backed current state is:

~~~yaml
executionStatus: blocked
implementationStatus: partial
verificationStatus: c12-required-generation-id-build-passed; full-worldsession-build-passed; existing-focused-verifier-passed; semantic-integration-not-verified
~~~
