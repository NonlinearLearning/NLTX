# System Decomposition Report: authoritative P20

taskSet: `authoritative-system-decomposition`  
taskId: `AUTH-SYS-P20`  
partitionId: `P20`  
sessionId: `0d1738f8ed3b4403b5beee258fd1b263`  
inputReport: `D:\TRbackup\NLTX\docs\migration\ledgers\authoritative-20-partitions\P20-World-Generation-Actions-Shapes.md`  
taskPrompt: `D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\design\2026-09-11-version4-authoritative-20-partition-prompts\2026-09-11-version4-P20-world-generation-actions-shapes-public-decomposition.md`  
outputReport: `D:\TRbackup\NLTX\docs\system-decomposition\reports\2026-09-18-system-decomposition-authoritative-P20-world-generation-actions-shapes.md`  
designStatus: `proposed`  
verificationStatus: `not-run`

## Scope and Evidence

This report covers only the claimed authoritative P20 partition: 12 leaf groups, 115 fields, 4 properties, and 119 total members under `WorldGenerationAndEcology`. The authoritative input inventory is the member manifest. The grouped index below reproduces all 119 `declaring type::member` entries so this report's scope can be checked without treating the candidate-component names as runtime ownership evidence.

Evidence is separated into four classes:

- **Version4 source facts:** local source at `D:\TRbackup\Version4`. The directory has no Git metadata, so a source commit/revision is unavailable. SHA256 for `Terraria\WorldGen.cs` is `A06A8463E39EA065441FF1EF702A787D3450ADAE5CD3065CF30BF76784D3EA1D`; `Terraria.WorldBuilding\Actions.cs` is `3125F2FBF0DFC87491140E8E60D7BC7121D5D95665EEB87C1BD16A9464FA1E44`.
- **CPG query evidence:** the read-only `ecs-system` CPG query API initialized the SQLite index at `D:\TRbackup\Version4-cpg-export\out-dop8-interproc.sqlite`. Manifest SHA256 is `6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`; project fingerprint is `521CBD4C11E7EB731256C232350A54099F431D0E59EBDF3C38880281F169C11B`; import status is `complete`, with 967 shards, 8,166,789 nodes, 71,038,907 edges, and 1,317 diagnostics. All 119 input members resolved to declaration endpoints across 66 types. Member-use scans covered all 967 shards: 111 complete, 8 partial, and 182 use facts. The index reports no `SourceSnapshotId` or per-file source hashes, so its correspondence to the current local source is **unknown**. A complete index query is scoped graph evidence; it does not prove runtime dispatch, alias effects, reflection, registration, or scheduler order.
- **Current NLTX evidence:** static source inspection under `D:\TRbackup\NLTX\src\NSSLC`. Exact searches found the P20 action command types at their declarations but no P20 command consumers in that source tree. This supports “no consumer found in searched source”, not proof that no external or generated consumer exists.
- **External organization reference:** `C:\Users\shan\Downloads\ECS\space-station-14-master`, `DungeonJob.cs`, was inspected only for generation-job organization (explicit dependencies, layer order, reservation tracking, suspend/resume). Its revision was not recorded. It is not evidence for Terraria behavior.

Key CPG results include one `WorldGen.cs` use of `StructureMap.CanPlace(Rectangle,int)`, two of `AddProtectedStructure`, 15 of `WorldGenRange.GetRandom`, and two of `GenAction.Output`. `GenAction.Apply` has no resolved call site and has query gaps. `GenShape.Perform` and `GenSearch.Find` are reached through `WorldUtils` virtual calls. The 8 partial zero-hit member queries carry `NoMatchingFactInScannedScope`; they must not be read as proof of no use.

### Complete 119-member scope

- `WorldTileMergeCullState` (8): `Terraria.WorldGen.TileMergeCullCache::CullTop`, `::CullBottom`, `::CullLeft`, `::CullRight`, `::CullTopLeft`, `::CullTopRight`, `::CullBottomLeft`, `::CullBottomRight`.
- `WorldGenerationTileSetActions` (12): `Terraria.WorldBuilding.Actions.ClearTile::_frameNeighbors`; `Actions.HalfBlock::_value`; `Actions.SetTile::_type`, `::_doFraming`, `::_doNeighborFraming`, `::_clearTile`; `Actions.SetTileKeepWall::_type`, `::_doFraming`, `::_doNeighborFraming`; `Actions.SetSlope::_slope`; `Actions.SetHalfTile::_halfTile`; `Actions.SwapSolidTile::_type`.
- `WorldGenerationWallMutationActions` (7): `Terraria.WorldBuilding.Actions.ClearWall::_frameNeighbors`; `Actions.SetWall::_type`, `::_doFraming`, `::_doNeighborFraming`, `::_clearTile`; `Actions.PlaceWall::_type`, `::_neighbors`.
- `WorldGenerationTilePlacementAndPaintActions` (5): `Terraria.WorldBuilding.Actions.SetTilePaint::paintID`; `Actions.SetWallPaint::paintID`; `Actions.SetTileAndWallPaint::paintID`; `Actions.PlaceTile::_type`, `::_style`.
- `WorldGenerationLiquidAndNeighborActions` (3): `Terraria.WorldBuilding.Actions.SetLiquid::_type`, `::_value`; `Actions.Smooth::_applyToNeighbors`.
- `WorldGenerationTileScanAndControlActions` (7): `Terraria.WorldBuilding.Actions.ContinueWrapper::_action`; `Actions.Count::_count`; `Actions.Scanner::_count`; `Actions.TileScanner::_tileIds`, `::_tileCounts`; `Actions.Custom::_perUnit`; `Actions.UpdateBounds::_bounds`.
- `WorldGenerationTileFramingAndDebugActions` (3): `Terraria.WorldBuilding.Actions.DebugDraw::_color`, `::_spriteBatch`; `Actions.SetFrames::_frameNeighbors`.
- `WorldGenerationConditionsAndSearches` (11): `Terraria.WorldBuilding.Conditions.IsTile::_types`; `Conditions.BoolCheck::_theBool`; `Conditions.InWorld::_fluff`; `Terraria.WorldBuilding.GenSearch::NOT_FOUND`, `::_conditions`; `Terraria.WorldBuilding.Searches.Left::_maxDistance`; `Searches.Right::_maxDistance`; `Searches.Down::_maxDistance`; `Searches.Up::_maxDistance`; `Searches.Rectangle::_width`, `::_height`.
- `WorldGenerationShapeData` (10): `Terraria.WorldBuilding.GenModShape::_data`; `GenShape::_quitOnFail`; `Terraria.WorldBuilding.ModShapes.OuterOutline::POINT_OFFSETS`, `::_useDiagonals`, `::_useInterior`; `ModShapes.InnerOutline::POINT_OFFSETS`, `::_useDiagonals`; `Terraria.WorldBuilding.ShapeData::_points`, `::Count`; `Terraria.WorldBuilding.GenBase::_tiles`.
- `WorldGenerationShapeModifierState` (22): `Terraria.WorldBuilding.Modifiers.ShapeScale::_scale`; `Modifiers.Expand::_xExpansion`, `::_yExpansion`; `Modifiers.RadialDither::_innerRadius`, `::_outerRadius`; `Modifiers.Blotches::_minX`, `::_minY`, `::_maxX`, `::_maxY`, `::_chance`; `Modifiers.InShape::_shapeData`; `Modifiers.NotInShape::_shapeData`; `Modifiers.Checkerboard::_percentile`; `Modifiers.RectangleMask::_xMin`, `::_yMin`, `::_xMax`, `::_yMax`; `Modifiers.Offset::_xOffset`, `::_yOffset`; `Modifiers.Dither::_failureChance`; `Modifiers.Flip::_flipX`, `::_flipY`.
- `WorldGenerationTileWallConditionState` (20): `Terraria.WorldBuilding.Modifiers.Conditions::_conditions`; `Modifiers.OnlyWalls::_types`; `Modifiers.OnlyTiles::_types`; `Modifiers.IsTouching::DIRECTIONS`, `::_useDiagonals`, `::_tileIds`; `Modifiers.NotTouching::DIRECTIONS`, `::_useDiagonals`, `::_tileIds`; `Modifiers.IsTouchingAir::DIRECTIONS`, `::_useDiagonals`; `Modifiers.SkipTiles::_types`; `Modifiers.HasLiquid::_liquidType`, `::_liquidLevel`; `Modifiers.NoLiquid::_liquidType`; `Modifiers.SkipWalls::_types`; `Modifiers.IsAboveHeight::_y`, `::_inclusive`; `Modifiers.IsBelowHeight::_y`, `::_inclusive`.
- `WorldStructurePlanningAndMasks` (11): `Terraria.WorldBuilding.DungeonSide::Left`, `::Right`; `Terraria.WorldBuilding.StructureMap::_structures`, `::_protectedStructures`, `::_lock`; `Terraria.WorldBuilding.WorldGenRange::Empty`, `::Minimum`, `::Maximum`, `::ScaleWith`, `::ScaledMinimum`, `::ScaledMaximum`.

The listed counts sum to 119 (115 fields and 4 properties), matching the claim input. Field/member names identify inventory only; they do not establish complete readers, writers, or behavior.

## Prior Component Decomposition Reconciliation

The P20 input ledger and same-partition component material correctly treat the 12 leaves as candidate state/API groupings, not proven runtime owners. Preserve the useful seams but do not create one scheduled System per leaf:

- The seven directional cull values fit a read-only Query result. NLTX already has `WorldTileMergeCullStateQuery` taking neighbor visibility, a framing revision, and missing-neighbor/show-invisible rules. That is a more explicit input boundary than a mutable cache, but producer, invalidation, and all consumers are not closed.
- Tile, wall, placement/paint, liquid/neighbor, and framing/debug groups are command payload candidates. NLTX has corresponding readonly command records, but this inspection found no P20 dispatcher/consumer. The records alone do not prove tile mutation, ordering, framing, or effect ownership.
- Scan/control action fields combine counting, scanner state, continuation, custom callbacks, and bounds. Their shared runtime contract and side effects are **unknown**; keep this as a partial boundary until source behavior and mutation owners are recovered.
- Conditions/searches, shape data, modifiers, and tile/wall conditions fit explicit-input Query/Definition APIs. NLTX has snapshot-backed condition/search queries and immutable shape/modifier calculation APIs. Their visible implementations establish current NLTX behavior only; they cannot fill in the Version4 stubs or prove value equivalence.
- The NLTX `WorldStructurePlanningAndMasksComponent` contains copied planned/protected rectangle lists keyed by `GenerationId`, with no writer found in the searched P20 source. The separate `IStructureReservationCommitPort` is used by `UndergroundDesertPlacementCommitSystem` for a generation-scoped overlap check and larva command projection. It is not shown to implement P20 `StructureMap` behavior or to write the P20 component.

## Conceptual Behaviors

The inventory names and constructor-held fields suggest these behavior families, but many bodies in the local Version4 tree are stubs, so exact input/output/state semantics are **unknown**:

1. **Cull derivation:** derive up to eight neighbor-edge cull flags from center and neighboring tile visibility, with missing-neighbor and invisible-block policies. Version4 `WorldGen.cs` has a cull-cache path that inspects neighboring tile state and changes frame values; invalidation and all readers remain unknown.
2. **Generation action composition:** configure per-coordinate operations for tile set/clear/slope, wall set/clear, tile placement and paint, liquid, smoothing, scanning/counting, framing, and debug drawing. `WorldUtils.Gen(origin, shape, action)` calls `shape.Perform(origin, action)`. The action `Apply` methods in `Actions.cs` return a default bool stub, so write sets, output values, continuation rules, per-unit call order, exception behavior, and side effects are unknown.
3. **Conditions and searches:** evaluate predicates over tiles/world bounds and locate positions in a direction or rectangle. `WorldUtils.Find` calls `search.Find(origin)` and maps `GenSearch.NOT_FOUND` to `false`; the concrete search and condition behavior is stubbed or dynamically dispatched in the inspected source.
4. **Shape traversal and transformation:** represent shape points and derive outlines, masks, offsets, scale, dither, blotches, and flips, then allow a shape to drive an action. `GenShape.Perform`, `ModShapes` implementations, and modifier application contain stubs; point enumeration, ordering, random draws, and quit-on-failure semantics are unknown.
5. **Structure eligibility and reservation:** `StructureMap.CanPlace` checks world bounds, intersections with protected rectangles, and active tiles against the selected validity array. `AddProtectedStructure` inflates the rectangle and adds it to both `_structures` and `_protectedStructures`, under `_lock`. The caller's eligibility check and later add are separate calls; atomicity between them is not established.
6. **Generation range and side data:** `WorldGenRange.GetRandom` is used by `WorldGen.cs`; `ScaleValue` returns a default int stub in the local source. Effective scaled limits and random-result behavior are unknown.

Observed state kinds are mixed: immutable/configuration-like action and modifier parameters; per-call query inputs/results; scanner/search mutable fields; derived cull flags; and world-scoped structure lists/range values. The only fully readable P20 authoritative mutation evidence is for `StructureMap` lists and their lock. Do not infer action write sets from names.

## State Ownership and Write Closure

| State or effect | Version4 evidence | Proposed owner/composition | Closure |
|---|---|---|---|
| `GenVars.structures` / `StructureMap` | `WorldGen.cs` assigns a new map at world-gen setup. `CanPlace` reads protected rectangles and active tiles; `AddProtectedStructure` writes both lists under the map lock. | One generation-scoped structure reservation System should own validate-and-reserve and publish immutable snapshots/Query results. Terrain tile reads enter through an explicit snapshot/reader adapter. | **partial**: `CanPlace` followed by `AddProtectedStructure` is not a single demonstrated transaction; other map readers/writers and full lifecycle are open. |
| Tile/wall/liquid/paint/framing world state | Field names and public `WorldUtils.Gen` composition identify intended action families, but concrete `Apply` bodies are stubs. | Emit ordered commands to the authoritative terrain/framing owners selected by P17/P19 integration review. Keep the P20 coordinator from writing those components directly. | **unknown**: readers, all indirect writes, event/effect emission, rollback and unique owner are not established. |
| Conditions, searches, shapes, modifiers | Inputs and abstract/virtual calls are visible; implementation bodies are stubbed. | Stateless Query/Definition APIs over immutable tile/world snapshots and explicit random inputs. Queries return values only. | **partial**: purity and cache invalidation can be designed, but compatibility and all runtime consumers remain unknown. |
| Cull flags and frame revisions | Source path computes neighbor flags then updates frame values; NLTX Query takes a revisioned neighborhood snapshot. | Query derives cull flags; framing owner controls revision changes and invalidation. | **partial**: revision producer, all readers/writers and barrier with framing are unknown. |
| Random stream and `WorldGenRange` | `GetRandom` has 15 indexed use facts; `ScaleValue` is stubbed. | Pass a generation-owned random stream or captured random input through the execution API; do not let Query secretly consume ambient RNG. | **unknown**: random draw count/order, retry and resume behavior are unverified. |
| Debug drawing and arbitrary custom callback | `_spriteBatch`, color and delegate fields are inventoried; `Apply` is stubbed. | Isolate presentation/debug and callback effects behind adapters invoked by the generation coordinator. | **unknown**: thread affinity, callback lifetime, effects, and whether these run in authoritative generation are unclosed. |

No unique owner is confirmed for tile mutations, frame updates, world-scoped reservation lifecycle, or persistence. The proposed reservation owner must serialize eligibility plus insertion or expose a transactional/coordination contract; the old pair of separately locked calls is not evidence of atomic commit.

## Boundary Role and Decision

`keep` retains a cohesive API/definition boundary without inventing a scheduler node. `partial` retains a mixed or incompletely evidenced boundary pending source closure. `separate` proposes an independent owner because state mutation and query roles differ; it remains proposed, not implemented.

| P20 leaf | Decision | Proposed role | Reason |
|---|---|---|---|
| `WorldTileMergeCullState` | keep | Query | The eight outputs are derived from an explicit neighborhood; invalidation belongs with the framing owner. |
| `WorldGenerationTileSetActions` | keep | Command payload | Preserve operation parameters; dispatch to a single terrain write owner. |
| `WorldGenerationWallMutationActions` | keep | Command payload | Wall writes and framing options need the same ordered commit boundary, not a new leaf System. |
| `WorldGenerationTilePlacementAndPaintActions` | keep | Command payload | Placement/paint is a command family; commit ownership crosses into terrain and placement subsystems. |
| `WorldGenerationLiquidAndNeighborActions` | keep | Command payload | Neighbor effects require explicit ordered commits and cannot be hidden in a Query. |
| `WorldGenerationTileScanAndControlActions` | partial | Query plus operation-local control/callback contract | Count, continuation, custom action, and bounds have distinct state/effect risks that the stubs do not resolve. |
| `WorldGenerationTileFramingAndDebugActions` | partial | Framing command plus debug/presentation adapter | Framing write ownership and debug effect lifecycle differ; split only after source and integration evidence. |
| `WorldGenerationConditionsAndSearches` | keep | Query/Definition | Evaluate from a caller-owned snapshot and return search result/sentinel mapping. |
| `WorldGenerationShapeData` | keep | Definition/Query | Immutable points and shape traversal calculation do not require their own System. |
| `WorldGenerationShapeModifierState` | keep | Definition/Query | Modifier parameters feed pure transforms; no independent lifecycle is established. |
| `WorldGenerationTileWallConditionState` | keep | Definition/Query | Predicate definitions should not own mutable tile state. |
| `WorldStructurePlanningAndMasks` | separate | Structure reservation owner System plus read-only Query | The list mutation invariant differs from shape/action calculations. Owner identity and world lifetime still need integration review. |

Rejected: one System per leaf, because grouping fields by ledger row does not establish independently scheduled work; a single P20 System that writes terrain, frames, debug presentation, and structure state, because it would combine owners and effects without a proven transaction; and treating the existing reservation adapter as the P20 owner, because its observed caller and generation-scoped overlap rule are narrower.

## System API and Legacy Behavior Mapping

| Legacy entry or state | Proposed ECS-facing composition | Compatibility/evidence note |
|---|---|---|
| `WorldUtils.Gen(origin, shape, action)` -> `GenShape.Perform` -> `GenAction.Apply` | `ShapeTraversalQuery` returns ordered candidate coordinates and execution policy; `GenerationActionExecutionSystem` consumes them and emits ordered tile/wall/liquid/framing commands. | Keep the public facade as an adapter during migration. The current source call edge is visible, but dynamic dispatch and action effects are **unknown**. Do not claim the proposed query preserves legacy point order. |
| `GenAction.Output(ShapeData)` | Map to an explicit immutable shape/result value passed to the next query or action stage. | CPG found two uses; the exact caller closure and output alias/lifetime need source confirmation. |
| `Conditions` / `GenCondition.CheckValidity`; `Modifiers.*` | `WorldGenerationConditionsAndSearchesQuery` over `ITileSnapshotReader`; separate shape transforms consume immutable definitions and explicit random values. | Preserve search sentinel mapping and boundary/fluff semantics after they can be recovered. Existing NLTX snapshot APIs are not proof of equivalence. |
| `WorldUtils.Find` / `GenSearch.Find` | Read-only `TileSearchQuery` returns position plus `Found`/termination reason; a compatibility adapter maps not-found back to `NOT_FOUND` and `false`. | Source proves only that the facade maps the sentinel to false; concrete search behavior and no-match versus boundary distinction are **unknown**. |
| `WorldGenRange.GetRandom` / `ScaleValue` | `WorldGenRangeQuery` takes the range definition and explicit generation/random input, returning value and random-stream result. | Draw count, scaling and invalid-range behavior are **unknown** until source behavior is recovered. |
| `StructureMap.CanPlace` | Read-only `StructurePlacementQuery` accepts world bounds, protected reservations, candidate rectangle, padding, and tile-validity snapshot. | Current code includes bounds, protected overlap, and active-tile validity; preserve the legacy `validTiles` overload and padding semantics. |
| `StructureMap.AddProtectedStructure` | `WorldStructureReservationSystem` accepts a typed reserve command and atomically validates/inserts; query/projection exposes a stable snapshot. | The current method inflates and writes two lists under one lock, but cross-call atomicity with `CanPlace` is not established. Preserve original public facade until callers and persistence are closed. |
| `TileMergeCullCache` fields | `WorldTileMergeCullStateQuery.Calculate(neighborhood, region, rules)` returns eight cull flags and framing revision. | NLTX has this explicit query already; parity with source culling and cache invalidation remains **unknown**. |
| action leaf types | Existing NLTX readonly command records remain payload candidates; a generation coordinator orders them and delegates to approved terrain/framing owners. | No consumer was found for those records in the searched source tree. Their declaration is not a migrated entry point. |

### Proposed System surface

- **Queries:** `ShapeTraversalQuery`, `ShapeModifierQuery`, `WorldGenerationConditionsAndSearchesQuery`, `WorldStructurePlacementQuery`, and the existing revision-aware cull Query. All inputs are immutable snapshots/definitions; no hidden mutation, global tile access, or implicit random draws.
- **Commands:** retain the current action payload families for tile, wall, placement/paint, liquid/neighbor, framing, and structure reservation. Commands identify generation scope and preserve source order; validation, rejection, partial-commit, and retry results must be explicit once recovered.
- **Systems:** one `GenerationActionExecutionSystem` coordinates the action stream but does not claim terrain state ownership; one `WorldStructureReservationSystem` owns reservation invariant if integration review confirms the world-scoped boundary. Do not create a scheduler node for each pure Query or modifier.
- **Adapters/projections:** adapt the legacy mutable `Main.tile`/random stream and static `GenVars` entry points to explicit world snapshot, random input, command sink, and legacy result/sentinel. Project read-only structure and generation results outward; projection must not write domain state.

All proposed APIs remain design names. Current NLTX source contains some matching type names but no evidence that the full proposed composition is connected or behavior-equivalent.

## Call and Dependency DAG

### Observed edges

```text
WorldGen generation setup -> GenVars.structures = new StructureMap()
WorldGen call site -> StructureMap.CanPlace(area, padding)
WorldGen call sites -> StructureMap.AddProtectedStructure(area)
WorldUtils.Gen(origin, shape, action) -> shape.Perform(origin, action) [virtual]
WorldUtils.Find(origin, search, out result) -> search.Find(origin) [virtual]
WorldUtils.Find -> compare result with GenSearch.NOT_FOUND -> bool
WorldGen callers -> WorldGenRange.GetRandom(random)
```

The first three are supported by inspected `WorldGen.cs` and `StructureMap.cs` call sites. CPG contributes the limited counts recorded in Scope and Evidence. The DAG is not closed: no resolved `GenAction.Apply` caller was returned; shape/search dispatch is abstract/virtual; lambda registrations, indirect calls, external mod/reflection entry points, and whole-pass scheduler relationships remain **partial** or **unknown**.

### Proposed composition edges

```text
P19 generation execution / phase input
  -> explicit world snapshot + generation random input
  -> ConditionsAndSearchesQuery
  -> ShapeData / ShapeModifier Queries
  -> ordered candidate points + action payload sequence
  -> GenerationActionExecutionSystem
  -> P17 terrain / liquid / framing command owners [integration-review]

placement candidate + tile snapshot
  -> StructurePlacementQuery
  -> WorldStructureReservationSystem (validate + reserve as one owner operation)
  -> read-only reservation snapshot for later generation queries
```

These edges are a proposed design DAG, not observed execution order. The order of declarations, files, or generation pass construction in a source file is not used to assert a scheduler edge. P17/P19 ownership, command acceptance barrier, commit visibility, retries, and cancellation handoff must be agreed before the DAG is executable.

## Lifecycle and Side Effects

- **Create/activate:** `WorldGen.cs` creates `GenVars.structures` during generation setup. The structure map is a static `GenVars` slot, while `StructureMap` itself holds protected/ordinary lists and a lock. Scope across two simultaneous worlds or resumed generations is **unknown**.
- **Update/commit:** `CanPlace` reads under `_lock`; `AddProtectedStructure` mutates both lists under `_lock`. The two separate calls leave a race window between check and add. The in-memory NLTX adapter also checks then appends without an observed lock, and the shown caller only covers underground-desert larva projection; it must not be substituted as a global P20 transaction.
- **Effects:** the action fields indicate possible tile/wall/paint/liquid/frame writes, random decisions, counters, custom callbacks, and graphics drawing, but concrete effects and order are **unknown** because relevant `Apply`, `Perform`, `Find`, `CheckValidity`, modifier, and range-scaling bodies are stubs. Do not use those empty bodies to assert “no side effects”.
- **Reset/end/unload:** setup replacement is observed; full reset, end, destroy, unload, failure rollback and multi-world behavior for the P20 state are **unknown**. The proposed System must receive lifecycle commands from the agreed generation/world owner and clear only its own generation scope.
- **Snapshot/persistence:** `StructureMap`'s two lists have `[JsonProperty]`; `WorldGenSnapshot` serializes public static `GenVars` members except `[JsonIgnore]` members and calls reset/deserialize on restore. However snapshot converter methods (`CanConvert`, `ReadJson`, `WriteJson`) are stubbed in this checkout. Actual persistence and restored reservation semantics are **unknown**. Network replication is not evidenced for these members.
- **Scheduler and suspension:** P20 call-site evidence shows direct calls but does not establish phase barriers, retry, cancellation, suspend/resume, or cross-thread execution. SS14 `DungeonJob.cs` demonstrates an explicit job/layer/reservation structure only as an organizational comparison; it does not establish Terraria behavior.

## Integration Handoff

Unresolved cross-partition decisions carry `crossSubsystemOwner: integration-review`:

- **P19 World Generation Execution:** select the generation action coordinator, command sequence/barrier, random stream and query/command error policy. P20 does not infer runtime order from declaration or pass-file order.
- **P17 World Terrain/Biomes/GenVars:** choose the unique authoritative writer for tiles, walls, liquid, paint, frames and culling revision. P20 action/query code must send commands or consume snapshots rather than double-write terrain state.
- **P16 World Lifecycle/Housing/Metrics and P01 world/session boundary:** assign generation scope identity, create/reset/end/unload ownership, multi-world isolation, and persistence/recovery boundary. `generationId` in one NLTX reservation path does not settle these contracts.
- **Reservation consumers:** decide whether the P20 reservation invariant is global to a generation or local to a placement subsystem; define atomic eligibility-plus-reserve, padding/valid-tile policy, duplicate/retry behavior, snapshot consistency, and cleanup. The existing underground-desert port is not evidence of the global owner.
- **External API boundary:** enumerate mod/reflection/configuration entry points and decide facade lifetime/deletion gate. Static CPG results do not close this surface.

Until these are resolved, proposed types may be reviewed but no new scheduler dependency or authoritative owner should be treated as final.

## Migration Behavior Contract

No behavior equivalence is claimed. The executable Version4 baseline for most action/shape/condition/search logic is unavailable in this checkout because method bodies are stubs; expected old results for those operations are **unknown** and must be recovered before differential acceptance can be specified.

When a behavior-capable baseline exists, compare old and proposed compositions on the same world, generation ID, random stream, candidate order and tile snapshots. The observation vector should include:

- ordered candidate coordinates, shape points/output data and quit/continue result;
- each tile/wall/liquid/paint/slope/half-block/frame delta and any neighbor-frame deltas;
- scanner counts, bounds, custom callback invocations and action return/continuation result;
- condition/search result, position, not-found sentinel and boundary behavior;
- random values consumed and produced, range endpoints, retry/resume sequence and deterministic seed replay;
- `CanPlace` result and reason at world edges, against protected rectangles, active valid/invalid tiles, padding, and overlapping concurrent reservations;
- structure-list snapshot before/after commit, no partial double-list update, generation-scope reset and restore;
- exception/rejection behavior, event/effect output, and whether commands become visible before or after the agreed phase barrier.

Candidate focused verifiers are: pure/deterministic Query and no-write checks; each action command's exact write-set and ordering check; shape/search boundary and sentinel checks; random draw-count/replay checks; reservation atomicity/overlap/idempotency checks; lifecycle reset/multi-world/snapshot restoration checks; and one integration verifier proving P19 dispatches through the single terrain/reservation owners. All are **planned only**. None ran for this report.

Migration sequence proposal: first recover authoritative method bodies or a trusted executable baseline; then close call/write/lifecycle edges; port pure snapshot Queries and Definitions; route command families through the integration-approved terrain owner while preserving order and facade behavior; establish transactional structure reservation ownership and lifecycle; run the focused verifiers against an agreed observation vector; remove legacy facades only after inbound callers and external entry points are closed. Completing this document does not satisfy any step's behavior gate.

## Evidence Gaps and Blocking Decisions

1. **Source/index identity:** the CPG manifest has no source snapshot ID or per-file hash. Local source hashes identify only inspected files; matching the full CPG database to this checkout is **unknown**.
2. **Stubbed behavior/effects:** `Actions.*.Apply`, `GenShape.Perform`/`ModShapes`, `GenSearch.Find`/concrete searches, condition checks, modifier application, and `WorldGenRange.ScaleValue` contain default-return/no-op bodies. Their outputs, writes, random usage, ordering and exception behavior are **unknown**.
3. **Inbound/dynamic closure:** 8 member queries are partial; `GenAction.Apply` lacks a resolved call site; shape/search dispatch is virtual. Reflection/mod loader registrations, callbacks, generated calls and other external entry points are **unknown**.
4. **NLTX connection:** current action command declarations have no consumer found in the searched `src\NSSLC` tree. The P20 structure component has no writer found there. The reservation adapter serves an observed, narrower underground-desert path. Runtime wiring and unique owners are **unknown**.
5. **State/lifecycle:** complete readers/writers for tile, wall, liquid, frames, scanner state, culling caches and structure lists; world/generation scope; unload/reset/failure cleanup; concurrent reservations; multi-world behavior; snapshot and network contracts remain **unknown**.
6. **Scheduling/compatibility:** action ordering, phase barriers, random sequence, retries, command visibility, error policy and public extension compatibility are **unknown**. P17/P19/P16/P01 decisions require `crossSubsystemOwner: integration-review`.

Blocking decisions before implementation are: restore or otherwise establish a trusted legacy behavior baseline; name the unique terrain and reservation writers; define command ordering/commit visibility and random ownership; define world lifecycle/persistence scope; and inventory external API entry points. If any evidence is unavailable, keep it explicitly `unknown` and do not mark the design as implemented.

## Verification Plan

**Performed for this report:** read-only CPG queries and targeted static source inspection; complete member inventory reconciliation (119/119); report path/status/content static review and whitespace check. These checks establish document consistency only.

**Not performed by instruction:** no build, test, run, publish, behavior verifier, production-source edit, test edit, project-file edit, input-ledger edit or prompt edit. `verificationStatus: not-run`.

Before implementation acceptance, run the focused verifiers listed in Migration Behavior Contract only after the legacy baseline and cross-partition owner contracts are available. Acceptance must prove the new owner and composition against the agreed observation vector; a runner `Complete`, compile result, local structure check, or retained facade alone is not migration success.

Settlement of this report task means only that the proposed P20 document was delivered. It does not mean that a System was implemented, runtime wiring exists, behavior is equivalent, or migration succeeded.
