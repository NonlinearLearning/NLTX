# Version4 P04 World Tiles and Storage: Proposed Component Design

partitionId: P04
sessionId: 7714d0b0d753445aa529fb1b38578679
inputReport: D:\TRbackup\NLTX\docs\migration\ledgers\non-authoritative-component-partitions\04-world-tiles-storage.md
componentDesignPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\non-authoritative\2026-09-11-version4-non-authoritative-P04-world-tiles-storage-component-design.md
componentExecutionPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\non-authoritative\2026-09-11-version4-non-authoritative-P04-world-tiles-storage-component-execution.md
designStatus: proposed
executionStatus: completed
implementationStatus: completed
verificationStatus: serial-build-passed; existing-verifier-failed-before-P04-checks; behavior-not-verified
completedComponents: P04 inventory coverage and six proposed capability-boundary checkpoints completed as design-only checkpoints; Component checkpoints completed for WorldGeometryComponent, WorldCapacityLimitsComponent, TileCellMaterialComponent, TileCellLiquidComponent, TileCellFrameBitComponent, TileEntityCapacityPolicyComponent, TileEntityRuntimeIdentityComponent, TileEntityAnchorComponent, TileEntityKindComponent, and TileEntityUpdateScheduleComponent
currentComponent: Deferred-boundary review complete; no further independent Component owner is closed
pendingComponents: catalog Components without a closed src2 registration/array owner; TileMapStore and frame-runtime state with unresolved canonical storage; display/inventory/interaction Components with external Item, Player, NPC, or Projectile ownership; all non-Component roles remain deferred
lastCheckpointUtc: 2026-09-12T08:57:12Z
src2CodeModified: yes
verificationCommand: pwsh -NoProfile -Command "& '.\Build\Tools\Invoke-SerialDotnet.ps1' -DotnetArguments @('build', '.\src2\Terraria.NonAuthoritative.Persistence.csproj', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')"
verificationResult: exitCode=0; warnings=0; errors=0; artifact=Build\bin\Terraria.NonAuthoritative.Persistence\Debug\net10.0\Terraria.NonAuthoritative.Persistence.dll
aggregateVerifierCommand: pwsh -NoProfile -Command "& '.\Build\Tools\Invoke-SerialDotnet.ps1' -DotnetArguments @('run', '--project', '.\src2\Verification\Terraria.NonAuthoritative.Persistence.Verification.csproj', '--no-build', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')"
aggregateVerifierResult: exitCode=1; System.InvalidOperationException at src2\Verification\Program.cs:326; diagnostics must be sanitized before projection; the existing verifier failed before the P04 tile-header check, so no P04-specific behavior result is available
handoffId: p04-world-tiles-storage-rebind-20260912
previousSessionId: bd37778d060641eeaa9f2700ed00af6c
handoffStatus: completed; old session was rejected from further settlement and the active session is 7714d0b0d753445aa529fb1b38578679
evidence-gap: P04 first-round public-decomposition report is absent; 约束\公共拆分约束.md is absent; complete per-member readers, writers, lifecycle and parity evidence is not closed for all 320 members; exact Version4 build/runtime integration and cross-partition ownership are unverified; geometry initialization and canonical section derivation owner are not closed; the existing aggregate verifier fails at src2\Verification\Program.cs:326 before the P04 tile-specific check
blocking-decision: integration-review must decide final owners for shared Main metadata, canonical Tile mutation/bit packing, TileEntity identity and persistence/network projections, placement hook execution, snapshot restore ordering, and the global System scheduler before remaining deferred work or caller migration; the existing aggregate verifier boundary must be repaired outside this Component-only task before P04 behavior can be verified

## Status Boundary

This is a second-round, non-authoritative, proposed design. Every type, file, namespace,
Component, System, Query, Command, Adapter, Projection, ID boundary, and scheduler order below
is a candidate only. Nothing in this document claims that NLTX already contains the proposed
implementation, that Version4 has been migrated, or that behavior is equivalent.

The input report is a source inventory. It is the only member-range authority for this session:
23 leaf groups, 310 fields, 10 properties, and 320 total members. The original source inventory
reports source numbers that are not a runtime order and must not be used to infer execution order.

## Scope and Exclusions

In scope:

- world geometry, section capacity, the Tile map store, map/client views, and Tile/Wall catalog
  metadata represented by the P04 inventory;
- per-cell material, wall, liquid, packed headers, slopes, frames, and mutation invariants;
- anchors, reach settings, TileObject definitions, previews, placement values, hooks, and draw
  projections;
- TileEntity registry, runtime scheduling, anchor identity, display/inventory state, sensors,
  pylon and training-dummy interaction state;
- framing lookup state, signs, tile snapshots, persistence/network projections, and paint caches;
- proposed ownership contracts for initialization, update, cleanup, persistence, networking, and
  client/UI projection.

Remaining out of scope after this Component-only implementation:

- all source edits under `src`, `Test`, Version4, and any C# or project file outside the ten
  Component files recorded below;
- first-round P04 research, authoritative ECS design, final cross-partition ownership, or API
  compatibility approval;
- Player, NPC, Item, Projectile, Wiring, WorldGen, Liquid propagation, networking, save-file,
  rendering, or UI implementations outside the P04 member boundary. P04 records only the seams
  that those domains must use;
- claiming that the existing NLTX files are a complete or behavior-equivalent migration.

## Evidence and Facts

The following are the absolute Version4 evidence paths read for this design:

- `D:\TRbackup\Version4\Terraria\Main.cs`
- `D:\TRbackup\Version4\Terraria\Tile.cs`
- `D:\TRbackup\Version4\Terraria\Framing.cs`
- `D:\TRbackup\Version4\Terraria\Sign.cs`
- `D:\TRbackup\Version4\Terraria\TileObject.cs`
- `D:\TRbackup\Version4\Terraria\TileColorCache.cs`
- `D:\TRbackup\Version4\Terraria.Utilities\TileSnapshot.cs`
- `D:\TRbackup\Version4\Terraria.GameContent\TilePaintSystemV2.cs`
- `D:\TRbackup\Version4\Terraria\Liquid.cs`
- `D:\TRbackup\Version4\Terraria\LiquidBuffer.cs`
- `D:\TRbackup\Version4\Terraria\WorldGen.cs`
- `D:\TRbackup\Version4\Terraria.DataStructures\AnchorData.cs`
- `D:\TRbackup\Version4\Terraria.DataStructures\AnchoredEntitiesCollection.cs`
- `D:\TRbackup\Version4\Terraria.DataStructures\PlacementDetails.cs`
- `D:\TRbackup\Version4\Terraria.DataStructures\PlacementHook.cs`
- `D:\TRbackup\Version4\Terraria.DataStructures\Point16.cs`
- `D:\TRbackup\Version4\Terraria.DataStructures\TileEntitiesManager.cs`
- `D:\TRbackup\Version4\Terraria.DataStructures\TileEntity.cs`
- `D:\TRbackup\Version4\Terraria.DataStructures\TileEntityType.cs`
- `D:\TRbackup\Version4\Terraria.DataStructures\TileObjectPreviewData.cs`
- `D:\TRbackup\Version4\Terraria.DataStructures\TileReachCheckSettings.cs`
- `D:\TRbackup\Version4\Terraria.GameContent.Tile_Entities\` and the twelve P04 TileEntity
  source files under that directory.

The local public API evidence was read from `D:\TRbackup\tmodloader-api-docs-stable` at version
`v2026.07`, including `struct_tile.html`, `class_tile_entity.html`, `struct_tile_object.html`,
`class_tile_object_data.html`, `class_main.html`, `class_world_gen.html`, `class_sign.html`,
`class_framing.html`, `class_world_map.html`, and `struct_tile_reach_check_settings.html`.
Those documents describe public boundaries only; they do not prove Version4 implementation
parity.

Confirmed Version4 facts used here:

- `Main.tile` is the two-dimensional Tile store; `Main.Map` is a separate map reference, and
  world and section dimensions are derived from Main geometry fields.
- `Tile` combines material, liquid, frame, and packed-bit state. Clear/reset and liquid/bit
  setters can update several packed members together, so a single proposed Tile mutation owner
  is required.
- `Liquid` and `LiquidBuffer` maintain queues, checking flags, and network-change collections;
  liquid propagation is not a read-only query.
- `WorldGen.TileFrame`, `SquareTileFrame`, `SquareWallFrame`, `RangeFrame`, and related methods
  can change frames, liquid handling, map queues, and neighboring cells.
- `TileEntity` combines registry indexes, runtime identity, position, update scheduling,
  lifecycle, serialization, and callbacks. Runtime ID, coordinate, persistent ID, network ID,
  and external entity/item/NPC IDs must remain distinct.
- `TELogicSensor` can change Tile frames, invoke Wiring, send network messages, and defer removal.
- `TileSnapshot` includes Tile, TileEntity, Chest, stream, and restore concerns; restore can
  trigger server-side tile resynchronization.
- `TilePaintSystemV2` stores graphics targets and render requests, which are client cache state,
  not simulation authority.

## Current NLTX State

The following existing files are relevant partial models, not proof of a completed migration:

- `src\WorldStorage\TileMapStore.cs`, `TileMapLayout.cs`, `TileCellState.cs`, liquid work and
  scheduler/buffer/dirty-section types, `TileEntityStore.cs`, `TileEntityRecord.cs`,
  `TileEntityId.cs`, `TileEntityTypeId.cs`, `TileEntityUpdateSchedule.cs`, `WorldSignStore.cs`,
  `WorldSignState.cs`, `WorldContainerStore.cs`, `WorldChestState.cs`, and `WorldSectionState.cs`;
- `src\WorldInteraction\Tiles\TileCellComponent.cs`, `TileFrameComponent.cs`,
  `TileLiquidWorkStateComponent.cs`, `TileSignalTopologyComponent.cs`, `LiquidKind.cs`, and
  `TileCoordinate.cs`;
- `src\WorldInteraction\TileEntities\` display, anchor, item-frame, rack, sensor, dummy,
  pylon-adjacent, identity, persistence, and update-schedule component files;
- `src\Content\TileDefinition.cs`, `TileDefinitionCatalog.cs`,
  `ITileDefinitionQuery.cs`, `TileCollisionDefinition.cs`, `TileEnvironmentDefinition.cs`, and
  `TileFramingDefinition.cs`.

The proposed design must integrate with those partial models only after an implementation pass
proves their fields and ownership. Existing names are not treated as final owners, and no
duplicate component is authorized by this document.

## Proposed Capability Boundaries

The 23 leaf groups are proposed to participate in six capability lanes. A lane can contain a
small number of cohesive components plus explicit systems and projections; a leaf group is not
automatically a component.

| Lane | Proposed boundary | Proposed target area | Authority rule |
|---|---|---|---|
| A | world geometry, capacity, catalogs, map and camera views | `src2\WorldStorage\Geometry\`, `src2\Content\`, `src2\WorldStorage\Map\`, `src2\Client\WorldView\` | geometry/store/catalog owners expose read ports; camera/map/audio values are projections where appropriate |
| B | Tile cell material, liquid, frame, and packed bits | `src2\WorldStorage\Tiles\`, `src2\WorldInteraction\Tiles\` | one proposed `TileCellMutationSystem` owns coherent cell writes and emits dirty/frame/liquid commands |
| C | anchors, reach, TileObject definitions, previews, placement and draw | `src2\WorldInteraction\Placement\`, `src2\WorldInteraction\Tiles\Queries\` | definitions and previews are values/projections; placement commands are the only world mutation entry |
| D | TileEntity registry, identity, display/inventory and interactions | `src2\WorldInteraction\TileEntities\`, `src2\WorldStorage\TileEntities\` | registry, position index, update queue, and per-kind state have separate owners |
| E | framing, signs, snapshots, persistence and network projections | `src2\WorldInteraction\Tiles\Framing\`, `src2\WorldStorage\Signs\`, `src2\WorldStorage\Snapshots\`, `src2\Adapters\` | framing/sign state is separate from cell state; snapshot/serialization never becomes simulation authority |
| F | paint variation keys, color cache and render targets | `src2\Client\TilePaint\` | client-only adapter/cache/projection; no RenderTarget2D or request list in simulation components |

All target names and paths in this table are `proposed`. Cross-partition owners are
`crossSubsystemOwner: integration-review`.

## Proposed Contracts

### Lane A: Geometry, Capacity, Catalogs, Map and Views

Proposed components are `WorldGeometryComponent`, `WorldCapacityLimitsComponent`,
`TileBehaviorCatalogComponent`, `TileLightingCatalogComponent`, `TileFramingCatalogComponent`,
`WallMetadataCatalogComponent`, and `TileFrameRuntimeStateComponent`. They hold immutable or
world-session-scoped definitions and bounded geometry values. They do not own per-cell Tile data.

Proposed `WorldGeometryInitializationSystem` validates non-negative bounds, derives tile and
section counts, allocates the store, and publishes a geometry-ready event. Proposed
`CatalogInitializationSystem` loads catalog arrays once per content generation and rejects arrays
whose lengths do not match the content registry. Proposed `WorldMapProjectionSystem` and
`CameraViewProjectionSystem` publish map/camera views; they do not write the authoritative Tile
store. Readers are Tile queries, framing, placement, collision, liquid, network, and persistence
ports. Writers are initialization and explicit catalog reload commands only.

`invBottom`, `cameraX`, `cameraY`, `liquidAlpha`, `waterStyle`, and `musicFade` are proposed
client/UI or presentation projections unless integration-review proves a simulation reader. They
must not be placed in a universal world component by name alone.

### Lane B: Tile Cells and Liquid/Frame Mutation

Proposed `TileCellMaterialComponent` contains tile type and wall type; proposed
`TileCellLiquidComponent` contains liquid amount/type state; proposed `TileCellFrameBitComponent`
contains frame coordinates and packed flags. The proposed `TileCellMutationSystem` is the single
write owner for all coupled clear/reset/type/liquid/header/bit/slope transitions. Proposed
`LiquidPropagationSystem`, `TileFramingSystem`, and `TileDirtySectionSystem` consume explicit
commands/events after mutation. Proposed `TileCellQuery` is read-only and returns a consistent
cell view.

Invariants: a cell is addressed by one `TileCoordinate`; packed header changes preserve all
unrelated bits; liquid amount/type combinations obey the Version4 codec; frame changes and
liquid changes publish the required dirty/neighbor effects; no system writes a packed field
directly; section indexes are derived from the geometry owner.

### Lane C: Anchors and Placement

Proposed values include `AnchorSpec`, `TileReachPolicy`, `TilePlacementDefinition`,
`TileObjectStyleDefinition`, `TileObjectCoordinatesProjection`, `TileObjectDrawProjection`, and
`TilePlacementValue`. Proposed `TileAnchorQuery` and `TilePlacementPreviewQuery` are pure
queries. Proposed `TilePlacementCommandSystem` validates reach, anchors, liquid policy, styles,
hooks, and target cells, then submits one atomic placement mutation to the Tile mutation owner.
Proposed `PlacementHookAdapter` is the only boundary to callback/delegate behavior. Preview and
draw values are transient and may be discarded each frame; they cannot mutate the world.

### Lane D: TileEntity Registry and Kind State

Proposed `TileEntityRegistryStore` owns type registration and indexes; proposed
`TileEntityRuntimeIdentityComponent`, `TileEntityAnchorComponent`, `TileEntityKindComponent`,
and `TileEntityUpdateScheduleComponent` own the corresponding cohesive values. Proposed
`TileEntityLifecycleSystem` allocates IDs, inserts/removes all indexes transactionally, and
publishes lifecycle events. Proposed `TileEntityUpdateSystem` snapshots the update queue before
calling kind systems and applies deferred removals after the update pass. `TELogicSensor` gets an
explicit command/event adapter for frame, Wiring, and network effects.

Display/inventory state is proposed as per-kind components such as
`DisplayDollInventoryComponent`, `HatRackInventoryComponent`, `SingleItemDisplayComponent`,
`DisplayPoseComponent`, `TrainingDummyInteractionComponent`, and
`LeashedEntityAnchorComponent`. `Item`, `Player`, `Projectile`, `NPC`, and `LeashedCritter`
objects remain behind adapters or opaque external references. A TileEntity position is an anchor,
not a persistent or network ID.

### Lane E: Framing, Signs, Snapshots and Projections

Proposed `TileFramingLookupCatalog` and `FrameNeighborMask` hold lookup/definition state;
`TileFramingSystem` calculates frame outputs and emits cell mutations through the Tile owner.
Proposed `WorldSignStore` owns sign coordinate/text records. `maxSigns` is capacity policy, not
sign content. Proposed `TileSnapshotProjection` reads a stable world view; proposed
`TileSnapshotPersistenceAdapter` serializes/restores via a transaction boundary and must restore
TileEntity indexes, chests, signs, liquid queues, and section dirtiness in an explicit order.
Snapshot buffers and BinaryReader/BinaryWriter are adapter-owned resources.

### Lane F: Paint and Render Cache

Proposed immutable variation values are `TilePaintVariationKey`, `WallPaintVariationKey`,
`TreeFoliageVariationKey`, and `CageTopVariationKey`; proposed `TileColorCacheProjection`
contains color/full-bright/invisible output. Proposed `TilePaintRenderCacheAdapter` owns
`RenderTarget2D`, holder dictionaries, preparation flags, and request lists. It is created and
disposed on the client graphics lifecycle and is never persisted, networked, or read by the
authoritative simulation.

## IDs, Ownership and Side Effects

| Identity | Meaning | Proposed owner | Must not be confused with |
|---|---|---|---|
| Tile coordinate | `(x,y)` cell address, including `Point16`-style value | `TileCoordinate` value/query | TileEntity runtime ID, section ID, persistent ID |
| TileEntity runtime ID | process/world-session registry key (`ID`, `TileEntitiesNextID`) | `TileEntityRuntimeIdentityComponent` and registry store | coordinate, type ID, network ID |
| TileEntity kind/type ID | registered kind (`type`, `EntityTypeID`) | `TileEntityKindCatalog` | runtime ID or external NPC/item ID |
| TileEntity anchor | position and multi-tile footprint | `TileEntityAnchorComponent` plus shape definition | persistent ID or network ID |
| Persistent ID | save-file identity, if required by integration | persistence adapter; candidate only | runtime ID and network ID |
| Network ID | protocol identity, if required by the protocol | network projection/adapter; candidate only | runtime ID and persistent ID |
| External entity/item/NPC ID | reference into Player, NPC, Item, Projectile or leashed-entity systems | external adapter; `crossSubsystemOwner: integration-review` | any Tile or TileEntity identity |
| World section | spatial dirty/streaming partition | `WorldSectionState`/section owner; candidate only | Tile coordinate and TileEntity identity |

Clock, randomness, logging, file I/O, network I/O, graphics resources, Wiring calls, and UI
effects are proposed ports. Components remain deterministic data. Systems state the ordering and
failure/retry behavior; adapters own effect execution and report failures as explicit results.

## Proposed Scheduler Contract

This order is a candidate integration contract, not an existing runtime fact:

1. `WorldGeometryInitializationSystem` establishes geometry and sections.
2. `CatalogInitializationSystem` loads tile, wall, framing, liquid, placement, and entity-kind
   definitions.
3. `TileMapAllocationSystem` allocates/loads cell storage and initializes the single mutation
   owner.
4. `TileEntityRegistryInitializationSystem` registers kinds and clears runtime indexes.
5. `WorldSnapshotLoadSystem` or network section load submits validated load commands.
6. `TileCellMutationSystem` applies queued cell changes atomically and emits dirty/frame/liquid
   work.
7. `LiquidPropagationSystem` consumes liquid work and emits further cell mutation commands.
8. `TileFramingSystem` consumes frame work, updates neighbor frames, and emits map/section
   projections.
9. `TileEntityLifecycleSystem` applies create/remove/move commands after cell changes.
10. `TileEntityUpdateSystem` runs server-authoritative updates and records deferred removal/effect
    commands; clients do not execute authoritative TileEntity `Update`.
11. `InteractionCommandSystem` evaluates reach/anchor/placement commands and submits world
    mutations.
12. `NetworkProjectionSystem` and `PersistenceProjectionSystem` serialize committed changes.
13. `MapAndClientViewProjectionSystem` updates map, camera, sign, preview, and paint views.
14. `TilePaintRenderCacheAdapter` prepares/releases graphics resources at client render time.

Any integration change to this order requires an explicit owner decision and focused regression
coverage. It must not be inferred from source file order.

## Per-Member Proposed Ownership Matrix

The following matrix is the required 320-member coverage. Each row has a proposed role and target
owner. `field` and `property` retain the input report's source declaration kind. The source number
is a trace key only; it is not an execution order.

### MainWorldGeometryAndCapacity (11)

| Source | Kind | Member | Proposed role and owner |
|---:|---|---|---|
| 174 | field | `Main.leftWorld` | proposed Component `WorldGeometryComponent` in `src2\WorldStorage\Geometry\WorldGeometryComponent.cs` |
| 175 | field | `Main.rightWorld` | proposed Component `WorldGeometryComponent` in `src2\WorldStorage\Geometry\WorldGeometryComponent.cs` |
| 176 | field | `Main.topWorld` | proposed Component `WorldGeometryComponent` in `src2\WorldStorage\Geometry\WorldGeometryComponent.cs` |
| 177 | field | `Main.bottomWorld` | proposed Component `WorldGeometryComponent` in `src2\WorldStorage\Geometry\WorldGeometryComponent.cs` |
| 178 | field | `Main.maxTilesX` | proposed derived field of `WorldGeometryComponent`; calculated by proposed `WorldGeometryInitializationSystem` |
| 179 | field | `Main.maxTilesY` | proposed derived field of `WorldGeometryComponent`; calculated by proposed `WorldGeometryInitializationSystem` |
| 180 | field | `Main.maxSectionsX` | proposed derived field of `WorldGeometryComponent`; calculated by proposed `WorldGeometryInitializationSystem` |
| 181 | field | `Main.maxSectionsY` | proposed derived field of `WorldGeometryComponent`; calculated by proposed `WorldGeometryInitializationSystem` |
| 182 | field | `Main.maxDustToDraw` | proposed Projection `RenderCapacityProjection` in `src2\Client\WorldView\RenderCapacityProjection.cs` |
| 183 | field | `Main.maxNetPlayers` | proposed Component `WorldCapacityLimitsComponent` in `src2\WorldStorage\Geometry\WorldCapacityLimitsComponent.cs` |
| 184 | field | `Main.maxNPCs` | proposed Component `WorldCapacityLimitsComponent` in `src2\WorldStorage\Geometry\WorldCapacityLimitsComponent.cs` |

### MainCameraAndLiquidState (5)

| Source | Kind | Member | Proposed role and owner |
|---:|---|---|---|
| 193 | field | `Main.invBottom` | proposed Projection `ClientInventoryLayoutProjection` in `src2\Client\WorldView\ClientInventoryLayoutProjection.cs` |
| 194 | field | `Main.cameraX` | proposed Projection `ClientCameraViewStateProjection` in `src2\Client\WorldView\ClientCameraViewStateProjection.cs` |
| 195 | field | `Main.cameraY` | proposed Projection `ClientCameraViewStateProjection` in `src2\Client\WorldView\ClientCameraViewStateProjection.cs` |
| 196 | field | `Main.liquidAlpha` | proposed Projection `LiquidPresentationStateProjection` in `src2\Client\WorldView\LiquidPresentationStateProjection.cs` |
| 197 | field | `Main.waterStyle` | proposed Projection `LiquidPresentationStateProjection` in `src2\Client\WorldView\LiquidPresentationStateProjection.cs` |

### MainTileFrameAndCatchMetadata (5)

| Source | Kind | Member | Proposed role and owner |
|---:|---|---|---|
| 369 | field | `Main.tileSand` | proposed Component `TileSurfaceTagCatalogComponent` in `src2\Content\TileSurfaceTagCatalogComponent.cs` |
| 370 | field | `Main.tileFlame` | proposed Component `TileHazardTagCatalogComponent` in `src2\Content\TileHazardTagCatalogComponent.cs` |
| 371 | field | `Main.npcCatchable` | proposed Component `NpcCatchabilityCatalogComponent` in `src2\Content\NpcCatchabilityCatalogComponent.cs` |
| 372 | field | `Main.tileFrame` | proposed Component `TileFrameRuntimeStateComponent` in `src2\WorldInteraction\Tiles\TileFrameRuntimeStateComponent.cs` |
| 373 | field | `Main.tileFrameCounter` | proposed Component `TileFrameRuntimeStateComponent` in `src2\WorldInteraction\Tiles\TileFrameRuntimeStateComponent.cs` |

### MainWorldMapAndTileStore (2)

| Source | Kind | Member | Proposed role and owner |
|---:|---|---|---|
| 374 | field | `Main.Map` | proposed Projection `WorldMapProjection` in `src2\WorldStorage\Map\WorldMapProjection.cs` |
| 375 | field | `Main.tile` | proposed Store/Component `TileMapStoreComponent` in `src2\WorldStorage\Tiles\TileMapStoreComponent.cs` |

### MainWallAndGlobalTileMetadata (6)

| Source | Kind | Member | Proposed role and owner |
|---:|---|---|---|
| 255 | field | `Main.musicFade` | proposed Projection `WorldMusicFadeProjection` in `src2\Client\WorldView\WorldMusicFadeProjection.cs` |
| 262 | field | `Main.wallHouse` | proposed Component `WallMetadataCatalogComponent` in `src2\Content\WallMetadataCatalogComponent.cs` |
| 263 | field | `Main.wallDungeon` | proposed Component `WallMetadataCatalogComponent` in `src2\Content\WallMetadataCatalogComponent.cs` |
| 264 | field | `Main.wallLight` | proposed Component `WallMetadataCatalogComponent` in `src2\Content\WallMetadataCatalogComponent.cs` |
| 265 | field | `Main.wallBlend` | proposed Component `WallMetadataCatalogComponent` in `src2\Content\WallMetadataCatalogComponent.cs` |
| 281 | field | `Main.wallLargeFrames` | proposed Component `WallFramingCatalogComponent` in `src2\Content\WallFramingCatalogComponent.cs` |

### MainTileBehaviorAndInteractionMetadata (20)

| Source | Kind | Member | Proposed role and owner |
|---:|---|---|---|
| 257 | field | `Main.tileMergeDirt` | proposed Component `TileBehaviorCatalogComponent` in `src2\Content\TileBehaviorCatalogComponent.cs` |
| 258 | field | `Main.tileCut` | proposed Component `TileBehaviorCatalogComponent` in `src2\Content\TileBehaviorCatalogComponent.cs` |
| 259 | field | `Main.tileAlch` | proposed Component `TileBehaviorCatalogComponent` in `src2\Content\TileBehaviorCatalogComponent.cs` |
| 266 | field | `Main.tileStone` | proposed Component `TileBehaviorCatalogComponent` in `src2\Content\TileBehaviorCatalogComponent.cs` |
| 267 | field | `Main.tileAxe` | proposed Component `TileBehaviorCatalogComponent` in `src2\Content\TileBehaviorCatalogComponent.cs` |
| 268 | field | `Main.tileHammer` | proposed Component `TileBehaviorCatalogComponent` in `src2\Content\TileBehaviorCatalogComponent.cs` |
| 269 | field | `Main.tileWaterDeath` | proposed Component `TileBehaviorCatalogComponent` in `src2\Content\TileBehaviorCatalogComponent.cs` |
| 270 | field | `Main.tileLavaDeath` | proposed Component `TileBehaviorCatalogComponent` in `src2\Content\TileBehaviorCatalogComponent.cs` |
| 271 | field | `Main.tileTable` | proposed Component `TileBehaviorCatalogComponent` in `src2\Content\TileBehaviorCatalogComponent.cs` |
| 276 | field | `Main.tileSolidTop` | proposed Component `TileBehaviorCatalogComponent` in `src2\Content\TileBehaviorCatalogComponent.cs` |
| 277 | field | `Main.tileSolid` | proposed Component `TileBehaviorCatalogComponent` in `src2\Content\TileBehaviorCatalogComponent.cs` |
| 278 | field | `Main.tileBouncy` | proposed Component `TileBehaviorCatalogComponent` in `src2\Content\TileBehaviorCatalogComponent.cs` |
| 279 | field | `Main.tileOreFinderPriority` | proposed Component `TileBehaviorCatalogComponent` in `src2\Content\TileBehaviorCatalogComponent.cs` |
| 282 | field | `Main.tileRope` | proposed Component `TileBehaviorCatalogComponent` in `src2\Content\TileBehaviorCatalogComponent.cs` |
| 285 | field | `Main.tileNoAttach` | proposed Component `TileBehaviorCatalogComponent` in `src2\Content\TileBehaviorCatalogComponent.cs` |
| 286 | field | `Main.tileNoFail` | proposed Component `TileBehaviorCatalogComponent` in `src2\Content\TileBehaviorCatalogComponent.cs` |
| 292 | field | `Main.tileGlowMask` | proposed Component `TileLightingCatalogComponent` in `src2\Content\TileLightingCatalogComponent.cs` |
| 293 | field | `Main.tileContainer` | proposed Component `TileBehaviorCatalogComponent` in `src2\Content\TileBehaviorCatalogComponent.cs` |
| 294 | field | `Main.tileSign` | proposed Component `TileBehaviorCatalogComponent` in `src2\Content\TileBehaviorCatalogComponent.cs` |
| 295 | field | `Main.tileMerge` | proposed Component `TileBehaviorCatalogComponent` in `src2\Content\TileBehaviorCatalogComponent.cs` |

### MainTileLightingAndFrameMetadata (15)

| Source | Kind | Member | Proposed role and owner |
|---:|---|---|---|
| 256 | field | `Main.tileLighted` | proposed Component `TileLightingCatalogComponent` in `src2\Content\TileLightingCatalogComponent.cs` |
| 260 | field | `Main.tileShine` | proposed Component `TileLightingCatalogComponent` in `src2\Content\TileLightingCatalogComponent.cs` |
| 261 | field | `Main.tileShine2` | proposed Component `TileLightingCatalogComponent` in `src2\Content\TileLightingCatalogComponent.cs` |
| 272 | field | `Main.tileBlockLight` | proposed Component `TileLightingCatalogComponent` in `src2\Content\TileLightingCatalogComponent.cs` |
| 273 | field | `Main.tileNoSunLight` | proposed Component `TileLightingCatalogComponent` in `src2\Content\TileLightingCatalogComponent.cs` |
| 274 | field | `Main.tileDungeon` | proposed Component `TileEnvironmentTagCatalogComponent` in `src2\Content\TileEnvironmentTagCatalogComponent.cs` |
| 275 | field | `Main.tileSpelunker` | proposed Component `TileEnvironmentTagCatalogComponent` in `src2\Content\TileEnvironmentTagCatalogComponent.cs` |
| 280 | field | `Main.tileLargeFrames` | proposed Component `TileFramingCatalogComponent` in `src2\Content\TileFramingCatalogComponent.cs` |
| 283 | field | `Main.tileBrick` | proposed Component `TileSurfaceTagCatalogComponent` in `src2\Content\TileSurfaceTagCatalogComponent.cs` |
| 284 | field | `Main.tileMoss` | proposed Component `TileSurfaceTagCatalogComponent` in `src2\Content\TileSurfaceTagCatalogComponent.cs` |
| 287 | field | `Main.tileCracked` | proposed Component `TileSurfaceTagCatalogComponent` in `src2\Content\TileSurfaceTagCatalogComponent.cs` |
| 288 | field | `Main.tileObsidianKill` | proposed Component `TileHazardTagCatalogComponent` in `src2\Content\TileHazardTagCatalogComponent.cs` |
| 289 | field | `Main.tileFrameImportant` | proposed Component `TileFramingCatalogComponent` in `src2\Content\TileFramingCatalogComponent.cs` |
| 290 | field | `Main.tilePile` | proposed Component `TileFramingCatalogComponent` in `src2\Content\TileFramingCatalogComponent.cs` |
| 291 | field | `Main.tileBlendAll` | proposed Component `TileFramingCatalogComponent` in `src2\Content\TileFramingCatalogComponent.cs` |

### TileCellMaterialAndLiquidState (9)

| Source | Kind | Member | Proposed role and owner |
|---:|---|---|---|
| 788 | field | `Tile.type` | proposed Component `TileCellMaterialComponent` in `src2\WorldStorage\Tiles\TileCellMaterialComponent.cs` |
| 789 | field | `Tile.wall` | proposed Component `TileCellMaterialComponent` in `src2\WorldStorage\Tiles\TileCellMaterialComponent.cs` |
| 790 | field | `Tile.liquid` | proposed Component `TileCellLiquidComponent` in `src2\WorldStorage\Tiles\TileCellLiquidComponent.cs` |
| 812 | field | `Tile.Liquid_Water` | proposed definition `LiquidKindDefinition` in `src2\WorldStorage\Tiles\LiquidKindDefinition.cs` |
| 813 | field | `Tile.Liquid_Lava` | proposed definition `LiquidKindDefinition` in `src2\WorldStorage\Tiles\LiquidKindDefinition.cs` |
| 814 | field | `Tile.Liquid_Honey` | proposed definition `LiquidKindDefinition` in `src2\WorldStorage\Tiles\LiquidKindDefinition.cs` |
| 815 | field | `Tile.Liquid_Shimmer` | proposed definition `LiquidKindDefinition` in `src2\WorldStorage\Tiles\LiquidKindDefinition.cs` |
| 816 | field | `Tile.NeitherLavaOrHoney` | proposed Adapter `TileLiquidPackingCodec` in `src2\Adapters\Tiles\TileLiquidPackingCodec.cs` |
| 817 | field | `Tile.EitherLavaOrHoney` | proposed Adapter `TileLiquidPackingCodec` in `src2\Adapters\Tiles\TileLiquidPackingCodec.cs` |

### TileCellFrameAndBitState (21)

| Source | Kind | Member | Proposed role and owner |
|---:|---|---|---|
| 791 | field | `Tile.sTileHeader` | proposed Component `TileCellFrameBitComponent` in `src2\WorldStorage\Tiles\TileCellFrameBitComponent.cs` |
| 792 | field | `Tile.bTileHeader` | proposed Component `TileCellFrameBitComponent` in `src2\WorldStorage\Tiles\TileCellFrameBitComponent.cs` |
| 793 | field | `Tile.bTileHeader2` | proposed Component `TileCellFrameBitComponent` in `src2\WorldStorage\Tiles\TileCellFrameBitComponent.cs` |
| 794 | field | `Tile.bTileHeader3` | proposed Component `TileCellFrameBitComponent` in `src2\WorldStorage\Tiles\TileCellFrameBitComponent.cs` |
| 795 | field | `Tile.frameX` | proposed Component `TileCellFrameBitComponent` in `src2\WorldStorage\Tiles\TileCellFrameBitComponent.cs` |
| 796 | field | `Tile.frameY` | proposed Component `TileCellFrameBitComponent` in `src2\WorldStorage\Tiles\TileCellFrameBitComponent.cs` |
| 797 | field | `Tile.Bit0` | proposed Adapter `TileCellBitCodec` in `src2\Adapters\Tiles\TileCellBitCodec.cs` |
| 798 | field | `Tile.Bit1` | proposed Adapter `TileCellBitCodec` in `src2\Adapters\Tiles\TileCellBitCodec.cs` |
| 799 | field | `Tile.Bit2` | proposed Adapter `TileCellBitCodec` in `src2\Adapters\Tiles\TileCellBitCodec.cs` |
| 800 | field | `Tile.Bit3` | proposed Adapter `TileCellBitCodec` in `src2\Adapters\Tiles\TileCellBitCodec.cs` |
| 801 | field | `Tile.Bit4` | proposed Adapter `TileCellBitCodec` in `src2\Adapters\Tiles\TileCellBitCodec.cs` |
| 802 | field | `Tile.Bit5` | proposed Adapter `TileCellBitCodec` in `src2\Adapters\Tiles\TileCellBitCodec.cs` |
| 803 | field | `Tile.Bit6` | proposed Adapter `TileCellBitCodec` in `src2\Adapters\Tiles\TileCellBitCodec.cs` |
| 804 | field | `Tile.Bit7` | proposed Adapter `TileCellBitCodec` in `src2\Adapters\Tiles\TileCellBitCodec.cs` |
| 805 | field | `Tile.Bit15` | proposed Adapter `TileCellBitCodec` in `src2\Adapters\Tiles\TileCellBitCodec.cs` |
| 806 | field | `Tile.Type_Solid` | proposed Adapter `TileCellBitCodec` in `src2\Adapters\Tiles\TileCellBitCodec.cs` |
| 807 | field | `Tile.Type_Halfbrick` | proposed Adapter `TileCellBitCodec` in `src2\Adapters\Tiles\TileCellBitCodec.cs` |
| 808 | field | `Tile.Type_SlopeDownRight` | proposed Adapter `TileCellBitCodec` in `src2\Adapters\Tiles\TileCellBitCodec.cs` |
| 809 | field | `Tile.Type_SlopeDownLeft` | proposed Adapter `TileCellBitCodec` in `src2\Adapters\Tiles\TileCellBitCodec.cs` |
| 810 | field | `Tile.Type_SlopeUpRight` | proposed Adapter `TileCellBitCodec` in `src2\Adapters\Tiles\TileCellBitCodec.cs` |
| 811 | field | `Tile.Type_SlopeUpLeft` | proposed Adapter `TileCellBitCodec` in `src2\Adapters\Tiles\TileCellBitCodec.cs` |

### SharedTileAnchorAndReachQueries (18)

| Source | Kind | Member | Proposed role and owner |
|---:|---|---|---|
| 1033 | field | `AnchorData.type` | proposed value `AnchorSpec` in `src2\WorldInteraction\Placement\AnchorSpec.cs` |
| 1034 | field | `AnchorData.tileCount` | proposed value `AnchorSpec` in `src2\WorldInteraction\Placement\AnchorSpec.cs` |
| 1035 | field | `AnchorData.checkStart` | proposed value `AnchorSpec` in `src2\WorldInteraction\Placement\AnchorSpec.cs` |
| 1036 | field | `AnchorData.Empty` | proposed value factory on `AnchorSpec`; no mutable world state |
| 1037 | field | `AnchoredEntitiesCollection.IndexPointPair.index` | proposed value `AnchorIndexEntry` in `src2\WorldInteraction\Placement\AnchorIndexEntry.cs` |
| 1038 | field | `AnchoredEntitiesCollection.IndexPointPair.coords` | proposed value `AnchorIndexEntry` in `src2\WorldInteraction\Placement\AnchorIndexEntry.cs` |
| 1039 | field | `AnchoredEntitiesCollection._anchoredNPCs` | proposed Store `AnchorIndexStore` in `src2\WorldInteraction\Placement\AnchorIndexStore.cs` |
| 1040 | field | `AnchoredEntitiesCollection._anchoredPlayers` | proposed Store `AnchorIndexStore` in `src2\WorldInteraction\Placement\AnchorIndexStore.cs` |
| 1263 | field | `Point16.X` | proposed value `TileCoordinate.X` in `src2\WorldInteraction\Tiles\TileCoordinate.cs` |
| 1264 | field | `Point16.Y` | proposed value `TileCoordinate.Y` in `src2\WorldInteraction\Tiles\TileCoordinate.cs` |
| 1265 | field | `Point16.Zero` | proposed value constant on `TileCoordinate` |
| 1266 | field | `Point16.NegativeOne` | proposed value constant on `TileCoordinate` |
| 1329 | field | `TileReachCheckSettings.TileRangeMultiplier` | proposed value `TileReachPolicy` in `src2\WorldInteraction\Placement\TileReachPolicy.cs` |
| 1330 | field | `TileReachCheckSettings.TileReachLimit` | proposed value `TileReachPolicy` in `src2\WorldInteraction\Placement\TileReachPolicy.cs` |
| 1331 | field | `TileReachCheckSettings.OverrideXReach` | proposed value `TileReachPolicy` in `src2\WorldInteraction\Placement\TileReachPolicy.cs` |
| 1332 | field | `TileReachCheckSettings.OverrideYReach` | proposed value `TileReachPolicy` in `src2\WorldInteraction\Placement\TileReachPolicy.cs` |
| 1333 | field | `TileReachCheckSettings.Simple` | proposed value preset on `TileReachPolicy` |
| 1334 | field | `TileReachCheckSettings.Pylons` | proposed value preset on `TileReachPolicy`; pylon owner is `crossSubsystemOwner: integration-review` |

### SharedTileFramingAndSignState (16)

| Source | Kind | Member | Proposed role and owner |
|---:|---|---|---|
| 3078 | field | `Framing.BlockStyle.top` | proposed value `FrameNeighborMask` in `src2\WorldInteraction\Tiles\Framing\FrameNeighborMask.cs` |
| 3079 | field | `Framing.BlockStyle.bottom` | proposed value `FrameNeighborMask` in `src2\WorldInteraction\Tiles\Framing\FrameNeighborMask.cs` |
| 3080 | field | `Framing.BlockStyle.left` | proposed value `FrameNeighborMask` in `src2\WorldInteraction\Tiles\Framing\FrameNeighborMask.cs` |
| 3081 | field | `Framing.BlockStyle.right` | proposed value `FrameNeighborMask` in `src2\WorldInteraction\Tiles\Framing\FrameNeighborMask.cs` |
| 3082 | field | `Framing.selfFrame8WayLookup` | proposed Component `TileFramingLookupCatalog` in `src2\WorldInteraction\Tiles\Framing\TileFramingLookupCatalog.cs` |
| 3083 | field | `Framing.wallFrameLookup` | proposed Component `TileFramingLookupCatalog` in `src2\WorldInteraction\Tiles\Framing\TileFramingLookupCatalog.cs` |
| 3084 | field | `Framing.frameSize8Way` | proposed Component `TileFramingLookupCatalog` in `src2\WorldInteraction\Tiles\Framing\TileFramingLookupCatalog.cs` |
| 3085 | field | `Framing.wallFrameSize` | proposed Component `TileFramingLookupCatalog` in `src2\WorldInteraction\Tiles\Framing\TileFramingLookupCatalog.cs` |
| 3086 | field | `Framing.blockStyleLookup` | proposed Component `TileFramingLookupCatalog` in `src2\WorldInteraction\Tiles\Framing\TileFramingLookupCatalog.cs` |
| 3087 | field | `Framing.phlebasTileFrameNumberLookup` | proposed Component `TileFramingLookupCatalog` in `src2\WorldInteraction\Tiles\Framing\TileFramingLookupCatalog.cs` |
| 3088 | field | `Framing.lazureTileFrameNumberLookup` | proposed Component `TileFramingLookupCatalog` in `src2\WorldInteraction\Tiles\Framing\TileFramingLookupCatalog.cs` |
| 3089 | field | `Framing.centerWallFrameLookup` | proposed Component `TileFramingLookupCatalog` in `src2\WorldInteraction\Tiles\Framing\TileFramingLookupCatalog.cs` |
| 3487 | field | `Sign.maxSigns` | proposed Component `WorldSignCapacityPolicy` in `src2\WorldStorage\Signs\WorldSignCapacityPolicy.cs` |
| 3488 | field | `Sign.x` | proposed Component `WorldSignRecord` in `src2\WorldStorage\Signs\WorldSignRecord.cs` |
| 3489 | field | `Sign.y` | proposed Component `WorldSignRecord` in `src2\WorldStorage\Signs\WorldSignRecord.cs` |
| 3490 | field | `Sign.text` | proposed Component `WorldSignRecord` in `src2\WorldStorage\Signs\WorldSignRecord.cs` |

### SharedTileSnapshots (19)

| Source | Kind | Member | Proposed role and owner |
|---:|---|---|---|
| 2967 | field | `TileSnapshot.TileStruct._type` | proposed value `TileCellSnapshotPayload` in `src2\WorldStorage\Snapshots\TileCellSnapshotPayload.cs` |
| 2968 | field | `TileSnapshot.TileStruct._wall_bTileHeader3_packed` | proposed value `TileCellSnapshotPayload` in `src2\WorldStorage\Snapshots\TileCellSnapshotPayload.cs` |
| 2969 | field | `TileSnapshot.TileStruct._sTileHeader` | proposed value `TileCellSnapshotPayload` in `src2\WorldStorage\Snapshots\TileCellSnapshotPayload.cs` |
| 2970 | field | `TileSnapshot.TileStruct._frameX` | proposed value `TileCellSnapshotPayload` in `src2\WorldStorage\Snapshots\TileCellSnapshotPayload.cs` |
| 2971 | field | `TileSnapshot.TileStruct._frameY` | proposed value `TileCellSnapshotPayload` in `src2\WorldStorage\Snapshots\TileCellSnapshotPayload.cs` |
| 2972 | field | `TileSnapshot.TileStruct._liquid` | proposed value `TileCellSnapshotPayload` in `src2\WorldStorage\Snapshots\TileCellSnapshotPayload.cs` |
| 2973 | field | `TileSnapshot.TileStruct._bTileHeader` | proposed value `TileCellSnapshotPayload` in `src2\WorldStorage\Snapshots\TileCellSnapshotPayload.cs` |
| 2974 | field | `TileSnapshot.TileStruct._i0` | proposed value `TileCellSnapshotPayload` codec storage |
| 2975 | field | `TileSnapshot.TileStruct._i1` | proposed value `TileCellSnapshotPayload` codec storage |
| 2976 | field | `TileSnapshot.TileStruct._i2` | proposed value `TileCellSnapshotPayload` codec storage |
| 2977 | field | `TileSnapshot.TileStruct._liquidNames` | proposed Adapter `TileSnapshotCodec` in `src2\Adapters\Snapshots\TileSnapshotCodec.cs` |
| 2978 | field | `TileSnapshot._worldFile` | proposed Adapter `TileSnapshotPersistenceAdapter` in `src2\Adapters\Snapshots\TileSnapshotPersistenceAdapter.cs` |
| 2979 | field | `TileSnapshot._tiles` | proposed transient Projection `TileSnapshotBuffer` in `src2\WorldStorage\Snapshots\TileSnapshotBuffer.cs` |
| 2980 | field | `TileSnapshot._tileEntities` | proposed transient Projection `TileEntitySnapshotBuffer`; identity owner is `crossSubsystemOwner: integration-review` |
| 2981 | field | `TileSnapshot._chests` | proposed Adapter `ChestSnapshotPort`; Chest owner is `crossSubsystemOwner: integration-review` |
| 2982 | field | `TileSnapshot._tempStream` | proposed Adapter resource in `TileSnapshotSerializationAdapter` |
| 2983 | field | `TileSnapshot._tempWriter` | proposed Adapter resource in `TileSnapshotSerializationAdapter` |
| 2984 | field | `TileSnapshot._tempReader` | proposed Adapter resource in `TileSnapshotSerializationAdapter` |
| 3941 | property | `TileSnapshot.Context` | proposed scoped `TileSnapshotContext` command context; never a world component |

### TileEntityRegistryAndBaseState (14)

| Source | Kind | Member | Proposed role and owner |
|---:|---|---|---|
| 1299 | field | `TileEntitiesManager._nextEntityID` | proposed Store `TileEntityRegistryStore` allocator state |
| 1300 | field | `TileEntitiesManager._types` | proposed Store `TileEntityKindRegistry` |
| 1301 | field | `TileEntity.manager` | proposed dependency port on `TileEntityRegistryStore` |
| 1302 | field | `TileEntity.MaxEntitiesPerChunk` | proposed Component `TileEntityCapacityPolicy` |
| 1303 | field | `TileEntity.EntityCreationLock` | proposed synchronization boundary of `TileEntityLifecycleSystem`; not component data |
| 1304 | field | `TileEntity.UpdateEntities` | proposed Component `TileEntityUpdateScheduleComponent` |
| 1305 | field | `TileEntity.ByID` | proposed runtime index in `TileEntityRegistryStore` |
| 1306 | field | `TileEntity.ByPosition` | proposed anchor index in `TileEntityRegistryStore` |
| 1307 | field | `TileEntity.TileEntitiesNextID` | proposed `TileEntityRuntimeIdAllocator` in `TileEntityLifecycleSystem` |
| 1308 | field | `TileEntity.ID` | proposed Component `TileEntityRuntimeIdentityComponent` |
| 1309 | field | `TileEntity.Position` | proposed Component `TileEntityAnchorComponent` |
| 1310 | field | `TileEntity.type` | proposed Component `TileEntityKindComponent` |
| 1311 | field | `TileEntity.RequiresUpdates` | proposed Component `TileEntityUpdateScheduleComponent` |
| 1312 | field | `TileEntityType<T>.EntityTypeID` | proposed `TileEntityKindCatalog`; final registry owner is `crossSubsystemOwner: integration-review` |

### TileEntityDisplayAndInventoryState (24)

| Source | Kind | Member | Proposed role and owner |
|---:|---|---|---|
| 2191 | field | `TEDeadCellsDisplayJar.item` | proposed Component `SingleItemDisplayComponent` |
| 2192 | field | `TEDisplayDoll.DisplayDollPose.Pose` | proposed value/component `DisplayPoseComponent` |
| 2193 | field | `TEDisplayDoll.DisplayDollPose.ItemAnimationPercent` | proposed value `DisplayPoseState` |
| 2194 | field | `TEDisplayDoll.DisplayDollPose.ItemAimRadians` | proposed value `DisplayPoseState` |
| 2195 | field | `TEDisplayDoll.MyTileID` | proposed definition `TileEntityFootprintDefinition` |
| 2196 | field | `TEDisplayDoll.entityTileWidth` | proposed definition `TileEntityFootprintDefinition` |
| 2197 | field | `TEDisplayDoll.entityTileHeight` | proposed definition `TileEntityFootprintDefinition` |
| 2198 | field | `TEDisplayDoll._dollPlayer` | proposed Adapter `DisplayDollPlayerProjectionAdapter`; external Player owner is integration-review |
| 2199 | field | `TEDisplayDoll._equip` | proposed Component `DisplayDollInventoryComponent` |
| 2200 | field | `TEDisplayDoll._dyes` | proposed Component `DisplayDollInventoryComponent` |
| 2201 | field | `TEDisplayDoll._misc` | proposed Component `DisplayDollInventoryComponent` |
| 2202 | field | `TEDisplayDoll._pose` | proposed Component `DisplayPoseComponent` |
| 2203 | field | `TEDisplayDoll.SupportedUseStylePoses` | proposed definition `DisplayDollPoseCatalog` |
| 2204 | field | `TEDisplayDoll._projectileDummy` | proposed Adapter `DisplayDollProjectileProjectionAdapter`; external Projectile owner is integration-review |
| 2205 | field | `TEFoodPlatter.item` | proposed Component `SingleItemDisplayComponent` |
| 2206 | field | `TEHatRack.MyTileID` | proposed definition `TileEntityFootprintDefinition` |
| 2207 | field | `TEHatRack.entityTileWidth` | proposed definition `TileEntityFootprintDefinition` |
| 2208 | field | `TEHatRack.entityTileHeight` | proposed definition `TileEntityFootprintDefinition` |
| 2209 | field | `TEHatRack._dollPlayer` | proposed Adapter `HatRackPlayerProjectionAdapter`; external Player owner is integration-review |
| 2210 | field | `TEHatRack._items` | proposed Component `HatRackInventoryComponent` |
| 2211 | field | `TEHatRack._dyes` | proposed Component `HatRackInventoryComponent` |
| 2212 | field | `TEItemFrame.item` | proposed Component `SingleItemDisplayComponent` |
| 2231 | field | `TEWeaponsRack.item` | proposed Component `SingleItemDisplayComponent` |
| 2232 | field | `TEWeaponsRack.MyTileID` | proposed definition `TileEntityFootprintDefinition` |

### TileEntityAnchorAndSensorState (11)

| Source | Kind | Member | Proposed role and owner |
|---:|---|---|---|
| 2189 | field | `TECritterAnchor._myEntityID` | proposed Component `LeashedEntityAnchorRuntimeComponent`; external entity ID owner is integration-review |
| 2190 | field | `TECritterAnchor.CritterPrototypes` | proposed definition `CritterPrototypeCatalog` behind a leashed-entity Adapter |
| 2213 | field | `TEKiteAnchor._myEntityID` | proposed Component `LeashedEntityAnchorRuntimeComponent`; external entity ID owner is integration-review |
| 2215 | field | `TELogicSensor.playerBox` | proposed transient state `LogicSensorEvaluationScratch` owned by `LogicSensorSystem` |
| 2216 | field | `TELogicSensor.tripPoints` | proposed command queue `LogicSensorTripQueue` |
| 2217 | field | `TELogicSensor.markedIDsForRemoval` | proposed deferred-removal queue in `LogicSensorSystem` |
| 2218 | field | `TELogicSensor.inUpdateLoop` | proposed update-pass guard in `LogicSensorSystem` |
| 2219 | field | `TELogicSensor.playerBoxFilled` | proposed transient evaluation state in `LogicSensorSystem` |
| 2220 | field | `TELogicSensor.logicCheck` | proposed Component `LogicSensorComponent` |
| 2221 | field | `TELogicSensor.On` | proposed Component `LogicSensorComponent` |
| 2222 | field | `TELogicSensor.CountedData` | proposed Component `LogicSensorComponent` |

### TileEntityWorldInteractionState (9)

| Source | Kind | Member | Proposed role and owner |
|---:|---|---|---|
| 2214 | field | `TELeashedEntityAnchorWithItem.itemType` | proposed Component `LeashedEntityAnchorComponent`; external Item type is an opaque adapter value |
| 2223 | field | `TETeleportationPylon.MyTileID` | proposed definition `TileEntityFootprintDefinition`; pylon catalog owner is integration-review |
| 2224 | field | `TETeleportationPylon.entityTileWidth` | proposed definition `TileEntityFootprintDefinition` |
| 2225 | field | `TETeleportationPylon.entityTileHeight` | proposed definition `TileEntityFootprintDefinition` |
| 2226 | field | `TETrainingDummy.playerBoxes` | proposed transient state `TrainingDummyEvaluationScratch` owned by `TrainingDummySystem` |
| 2227 | field | `TETrainingDummy.playerBoxFilled` | proposed transient state `TrainingDummyEvaluationScratch` |
| 2228 | field | `TETrainingDummy.npcSlotsFull` | proposed transient state `TrainingDummyEvaluationScratch` |
| 2229 | field | `TETrainingDummy.npc` | proposed Component `TrainingDummyInteractionComponent`; external NPC identity is integration-review |
| 2230 | field | `TETrainingDummy.activationRetryCooldown` | proposed Component `TrainingDummyInteractionComponent`; clock/retry port is integration-review |

### SharedTilePlacementAnchorAndHookModules (20)

| Source | Kind | Member | Proposed role and owner |
|---:|---|---|---|
| 2792 | field | `AnchorDataModule.top` | proposed value `TilePlacementAnchorDefinition` |
| 2793 | field | `AnchorDataModule.bottom` | proposed value `TilePlacementAnchorDefinition` |
| 2794 | field | `AnchorDataModule.left` | proposed value `TilePlacementAnchorDefinition` |
| 2795 | field | `AnchorDataModule.right` | proposed value `TilePlacementAnchorDefinition` |
| 2796 | field | `AnchorDataModule.wall` | proposed value `TilePlacementAnchorDefinition` |
| 2797 | field | `AnchorTypesModule.tileValid` | proposed Component `TilePlacementAnchorCatalog` |
| 2798 | field | `AnchorTypesModule.tileInvalid` | proposed Component `TilePlacementAnchorCatalog` |
| 2799 | field | `AnchorTypesModule.tileAlternates` | proposed Component `TilePlacementAnchorCatalog` |
| 2800 | field | `AnchorTypesModule.wallValid` | proposed Component `TilePlacementAnchorCatalog` |
| 2801 | field | `LiquidDeathModule.water` | proposed value `LiquidPlacementPolicy` |
| 2802 | field | `LiquidDeathModule.lava` | proposed value `LiquidPlacementPolicy` |
| 2803 | field | `LiquidPlacementModule.water` | proposed value `LiquidPlacementPolicy` |
| 2804 | field | `LiquidPlacementModule.lava` | proposed value `LiquidPlacementPolicy` |
| 2805 | field | `TileObjectAlternatesModule.data` | proposed Component `TileObjectStyleCatalog` |
| 2834 | field | `TileObjectSubTilesModule.data` | proposed Component `TileObjectStyleCatalog` |
| 2835 | field | `TilePlacementHooksModule.check` | proposed Adapter `PlacementHookAdapter` |
| 2836 | field | `TilePlacementHooksModule.postPlaceEveryone` | proposed Adapter `PlacementHookAdapter` |
| 2837 | field | `TilePlacementHooksModule.postPlaceMyPlayer` | proposed Adapter `PlacementHookAdapter` |
| 2838 | field | `TilePlacementHooksModule.placeOverride` | proposed Adapter `PlacementHookAdapter` |
| 2839 | field | `TilePlacementHooksModule.getStyleMethod` | proposed Adapter `PlacementHookAdapter`; delegate execution is an effect boundary |

### SharedTileObjectPreviewState (25)

| Source | Kind | Member | Proposed role and owner |
|---:|---|---|---|
| 1313 | field | `TileObjectPreviewData._type` | proposed transient Projection `TileObjectPreviewProjection` |
| 1314 | field | `TileObjectPreviewData._style` | proposed transient Projection `TileObjectPreviewProjection` |
| 1315 | field | `TileObjectPreviewData._alternate` | proposed transient Projection `TileObjectPreviewProjection` |
| 1316 | field | `TileObjectPreviewData._random` | proposed transient Projection `TileObjectPreviewProjection`; randomness port is integration-review |
| 1317 | field | `TileObjectPreviewData._active` | proposed transient Projection `TileObjectPreviewProjection` |
| 1318 | field | `TileObjectPreviewData._size` | proposed transient Projection `TileObjectPreviewProjection` |
| 1319 | field | `TileObjectPreviewData._coordinates` | proposed transient Projection `TileObjectPreviewProjection` |
| 1320 | field | `TileObjectPreviewData._objectStart` | proposed transient Projection `TileObjectPreviewProjection` |
| 1321 | field | `TileObjectPreviewData._data` | proposed transient Projection `TileObjectPreviewGridProjection` |
| 1322 | field | `TileObjectPreviewData._dataSize` | proposed transient Projection `TileObjectPreviewGridProjection` |
| 1323 | field | `TileObjectPreviewData._percentValid` | proposed Query result `TilePlacementValidityView` |
| 1324 | field | `TileObjectPreviewData.placementCache` | proposed transient `TileObjectPreviewCache` |
| 1325 | field | `TileObjectPreviewData.randomCache` | proposed transient `TileObjectPreviewCache`; randomness owner is integration-review |
| 1326 | field | `TileObjectPreviewData.None` | proposed value constant on `TileObjectPreviewProjection` |
| 1327 | field | `TileObjectPreviewData.ValidSpot` | proposed value constant on `TileObjectPreviewProjection` |
| 1328 | field | `TileObjectPreviewData.InvalidSpot` | proposed value constant on `TileObjectPreviewProjection` |
| 3702 | property | `TileObjectPreviewData.Active` | proposed read-only Query facade `TilePlacementPreviewQuery` |
| 3703 | property | `TileObjectPreviewData.Type` | proposed read-only Query facade `TilePlacementPreviewQuery` |
| 3704 | property | `TileObjectPreviewData.Style` | proposed read-only Query facade `TilePlacementPreviewQuery` |
| 3705 | property | `TileObjectPreviewData.Alternate` | proposed read-only Query facade `TilePlacementPreviewQuery` |
| 3706 | property | `TileObjectPreviewData.Random` | proposed read-only Query facade `TilePlacementPreviewQuery` |
| 3707 | property | `TileObjectPreviewData.Size` | proposed read-only Query facade `TilePlacementPreviewQuery` |
| 3708 | property | `TileObjectPreviewData.Coordinates` | proposed read-only Query facade `TilePlacementPreviewQuery` |
| 3709 | property | `TileObjectPreviewData.ObjectStart` | proposed read-only Query facade `TilePlacementPreviewQuery` |
| 3710 | property | `TileObjectPreviewData.this[]` | proposed read-only Query facade `TileObjectPreviewGridQuery` |

### SharedTileObjectPlacementValueState (16)

| Source | Kind | Member | Proposed role and owner |
|---:|---|---|---|
| 1208 | field | `PlacementDetails.tileType` | proposed Command input `TilePlacementValue` |
| 1209 | field | `PlacementDetails.tileStyle` | proposed Command input `TilePlacementValue` |
| 1210 | field | `PlacementHook.hook` | proposed Adapter result/input `PlacementHookAdapter` |
| 1211 | field | `PlacementHook.badReturn` | proposed value `PlacementHookResult` |
| 1212 | field | `PlacementHook.badResponse` | proposed value `PlacementHookResult` |
| 1213 | field | `PlacementHook.processedCoordinates` | proposed value `PlacementHookResult` |
| 1214 | field | `PlacementHook.Empty` | proposed value constant on `PlacementHookResult` |
| 1215 | field | `PlacementHook.Response_AllInvalid` | proposed value constant on `PlacementHookResult` |
| 3509 | field | `TileObject.xCoord` | proposed value `TileObjectPlacementValue` |
| 3510 | field | `TileObject.yCoord` | proposed value `TileObjectPlacementValue` |
| 3511 | field | `TileObject.type` | proposed value `TileObjectPlacementValue` |
| 3512 | field | `TileObject.style` | proposed value `TileObjectPlacementValue` |
| 3513 | field | `TileObject.alternate` | proposed value `TileObjectPlacementValue` |
| 3514 | field | `TileObject.random` | proposed value `TileObjectPlacementValue`; randomness port is integration-review |
| 3515 | field | `TileObject.Empty` | proposed value constant on `TileObjectPlacementValue` |
| 3516 | field | `TileObject.objectPreview` | proposed Projection reference `TileObjectPreviewProjection` |

### SharedTilePlacementCoordinateAndDrawModules (14)

| Source | Kind | Member | Proposed role and owner |
|---:|---|---|---|
| 2813 | field | `TileObjectCoordinatesModule.width` | proposed Projection `TileObjectCoordinatesProjection` |
| 2814 | field | `TileObjectCoordinatesModule.heights` | proposed Projection `TileObjectCoordinatesProjection` |
| 2815 | field | `TileObjectCoordinatesModule.padding` | proposed Projection `TileObjectCoordinatesProjection` |
| 2816 | field | `TileObjectCoordinatesModule.paddingFix` | proposed Projection `TileObjectCoordinatesProjection` |
| 2817 | field | `TileObjectCoordinatesModule.styleWidth` | proposed Projection `TileObjectCoordinatesProjection` |
| 2818 | field | `TileObjectCoordinatesModule.styleHeight` | proposed Projection `TileObjectCoordinatesProjection` |
| 2819 | field | `TileObjectCoordinatesModule.calculated` | proposed transient state `TileObjectCoordinatesProjection` |
| 2820 | field | `TileObjectCoordinatesModule.drawStyleOffset` | proposed Projection `TileObjectCoordinatesProjection` |
| 2821 | field | `TileObjectCoordinatesModule.drawFrameOffsets` | proposed Projection `TileObjectCoordinatesProjection` |
| 2822 | field | `TileObjectDrawModule.xOffset` | proposed Projection `TileObjectDrawProjection` |
| 2823 | field | `TileObjectDrawModule.yOffset` | proposed Projection `TileObjectDrawProjection` |
| 2824 | field | `TileObjectDrawModule.flipHorizontal` | proposed Projection `TileObjectDrawProjection` |
| 2825 | field | `TileObjectDrawModule.flipVertical` | proposed Projection `TileObjectDrawProjection` |
| 2826 | field | `TileObjectDrawModule.stepDown` | proposed Projection `TileObjectDrawProjection` |

### SharedTilePlacementBaseAndStyleModules (14)

| Source | Kind | Member | Proposed role and owner |
|---:|---|---|---|
| 2806 | field | `TileObjectBaseModule.width` | proposed definition `TileObjectBaseDefinition` |
| 2807 | field | `TileObjectBaseModule.height` | proposed definition `TileObjectBaseDefinition` |
| 2808 | field | `TileObjectBaseModule.origin` | proposed definition `TileObjectBaseDefinition` |
| 2809 | field | `TileObjectBaseModule.direction` | proposed definition `TileObjectBaseDefinition` |
| 2810 | field | `TileObjectBaseModule.randomRange` | proposed definition `TileObjectBaseDefinition`; randomness port is integration-review |
| 2811 | field | `TileObjectBaseModule.flattenAnchors` | proposed definition `TileObjectBaseDefinition` |
| 2812 | field | `TileObjectBaseModule.specificRandomStyles` | proposed definition `TileObjectBaseDefinition` |
| 2827 | field | `TileObjectStyleModule.style` | proposed definition `TileObjectStyleDefinition` |
| 2828 | field | `TileObjectStyleModule.horizontal` | proposed definition `TileObjectStyleDefinition` |
| 2829 | field | `TileObjectStyleModule.styleWrapLimit` | proposed definition `TileObjectStyleDefinition` |
| 2830 | field | `TileObjectStyleModule.styleMultiplier` | proposed definition `TileObjectStyleDefinition` |
| 2831 | field | `TileObjectStyleModule.styleLineSkip` | proposed definition `TileObjectStyleDefinition` |
| 2832 | field | `TileObjectStyleModule.styleWrapLimitVisualOverride` | proposed definition `TileObjectStyleDefinition` |
| 2833 | field | `TileObjectStyleModule.styleLineSkipVisualoverride` | proposed definition `TileObjectStyleDefinition` |

### SharedTilePaintRenderTargetState (12)

| Source | Kind | Member | Proposed role and owner |
|---:|---|---|---|
| 2544 | field | `ARenderTargetHolder.Target` | proposed client Adapter `TilePaintRenderCacheAdapter` |
| 2545 | field | `ARenderTargetHolder._wasPrepared` | proposed client Adapter lifecycle state `TilePaintRenderCacheAdapter` |
| 2546 | field | `TreeTopRenderTargetHolder.Key` | proposed client Adapter binding to `TreeFoliageVariationKey` |
| 2548 | field | `TileRenderTargetHolder.Key` | proposed client Adapter binding to `TilePaintVariationKey` |
| 2549 | field | `CageTopRenderTargetHolder.Key` | proposed client Adapter binding to `CageTopVariationKey` |
| 2550 | field | `WallRenderTargetHolder.Key` | proposed client Adapter binding to `WallPaintVariationKey` |
| 2561 | field | `TilePaintSystemV2._cageTopRenders` | proposed client cache in `TilePaintRenderCacheAdapter` |
| 2562 | field | `TilePaintSystemV2._tilesRenders` | proposed client cache in `TilePaintRenderCacheAdapter` |
| 2563 | field | `TilePaintSystemV2._wallsRenders` | proposed client cache in `TilePaintRenderCacheAdapter` |
| 2564 | field | `TilePaintSystemV2._treeTopRenders` | proposed client cache in `TilePaintRenderCacheAdapter` |
| 2565 | field | `TilePaintSystemV2._treeBranchRenders` | proposed client cache in `TilePaintRenderCacheAdapter` |
| 2566 | field | `TilePaintSystemV2._requests` | proposed client transient request queue in `TilePaintRenderCacheAdapter` |

### SharedTilePaintVariationAndColorState (14)

| Source | Kind | Member | Proposed role and owner |
|---:|---|---|---|
| 2547 | field | `TreeBranchTargetHolder.Key` | proposed value `TreeFoliageVariationKey` |
| 2551 | field | `TileVariationkey.TileType` | proposed value `TilePaintVariationKey` |
| 2552 | field | `TileVariationkey.TileStyle` | proposed value `TilePaintVariationKey` |
| 2553 | field | `TileVariationkey.PaintColor` | proposed value `TilePaintVariationKey` |
| 2554 | field | `WallVariationKey.WallType` | proposed value `WallPaintVariationKey` |
| 2555 | field | `WallVariationKey.PaintColor` | proposed value `WallPaintVariationKey` |
| 2556 | field | `TreeFoliageVariantKey.TextureIndex` | proposed value `TreeFoliageVariationKey` |
| 2557 | field | `TreeFoliageVariantKey.TextureStyle` | proposed value `TreeFoliageVariationKey` |
| 2558 | field | `TreeFoliageVariantKey.PaintColor` | proposed value `TreeFoliageVariationKey` |
| 2559 | field | `CageTopVariationkey.CageStyle` | proposed value `CageTopVariationKey` |
| 2560 | field | `CageTopVariationkey.PaintColor` | proposed value `CageTopVariationKey` |
| 3506 | field | `TileColorCache.Color` | proposed Projection `TileColorCacheProjection` |
| 3507 | field | `TileColorCache.FullBright` | proposed Projection `TileColorCacheProjection` |
| 3508 | field | `TileColorCache.Invisible` | proposed Projection `TileColorCacheProjection` |

## Lifecycle and Invariant Summary

| Boundary | Initialization | Normal update | Cleanup | Persistence/network |
|---|---|---|---|---|
| Geometry/catalog | validate content generation and derive dimensions | read-only catalog queries | dispose/reject generation-owned arrays | serialize definitions only where protocol requires; catalog version is explicit |
| Tile store/cell | allocate or load sections with default cells | mutation owner applies commands; liquid/framing consume events | flush projections, clear queues, release sections | cell codec and section projection; no direct array alias across adapters |
| Placement | load definitions and hook ports | pure preview then command validation and atomic commit | discard previews and temporary geometry | commands/results are protocol-specific projections |
| TileEntity | register kinds, clear indexes, allocate runtime IDs | lifecycle and kind systems update server state; deferred deletes commit after pass | remove from all indexes and release external references | separate runtime/persistent/network identities; per-kind serializers |
| Sign/framing | load lookup catalog and sign capacity | framing system writes through Tile mutation owner; sign store updates records | clear sign records and lookup caches | sign records and frame outputs have independent projections |
| Snapshot | create scoped buffers/adapters | read stable view; restore only through transaction command | dispose streams and clear buffers | persistence adapter controls format; network resync is an explicit projection |
| Paint | create client graphics adapter | prepare/request/cache targets on client | dispose targets on graphics lifecycle | never persisted or authoritative/networked |

## Evidence Gaps and Blocking Decisions

Evidence gaps are intentionally explicit:

- No P04 first-round report exists at
  `D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-11-version4-non-authoritative-P04-world-tiles-storage-public-decomposition.md`.
  This second-round document does not recreate it or treat it as evidence.
- `D:\TRbackup\NLTX\约束\公共拆分约束.md` was not present. The design therefore uses the
  available repository and session rules and marks public-split compliance as requiring review.
- The inventory confirms declarations and paths, but a complete member-by-member reader/writer,
  lifecycle, failure, retry, and parity trace is not closed for every member. Rows remain
  proposed and must be checked during implementation.
- Cross-partition owners for Player/NPC/Item/Projectile/Wiring, network identity, persistence
  identity, section scheduling, and global scheduler order are not settled.
- No focused test, runtime test, or behavior-equivalence verification was performed in this
  session. The affected src2 project was compiled serially after the Component changes.

Blocking decisions for integration-review:

1. Select the canonical NLTX owner for geometry/catalog arrays and reconcile it with existing
   `src\Content` and `src\WorldStorage` types without duplicate authority.
2. Approve one Tile mutation owner and one packed-bit/liquid codec before any split of `Tile`.
3. Define persistent and network TileEntity IDs independently from runtime IDs and coordinates.
4. Define the transaction and resynchronization order for snapshot restore, TileEntity indexes,
   chests, signs, liquid queues, section dirtiness, and network output.
5. Decide whether hook delegates are compatibility adapters or first-class domain commands, and
   define failure/retry semantics.
6. Approve the scheduler order above against the owners of Liquid, WorldGen, Wiring, networking,
   rendering, and content systems.

## Focused Verifier Plan

No verifier was run. The eventual focused plan is:

- inventory verifier: parse the P04 input and assert 23 unique groups, 310 fields, 10 properties,
  320 members, and one proposed row per source number;
- static owner verifier: assert only the proposed Tile mutation owner writes packed cell fields,
  only the registry owner mutates TileEntity indexes, and no simulation project references
  `RenderTarget2D`;
- invariant tests: clear/reset/type/liquid/bit/frame transitions, slope encoding, section dirty
  propagation, and liquid work queue behavior;
- lifecycle tests: TileEntity create/move/remove/update/deferred-remove transactions and index
  consistency;
- placement tests: anchor/reach/liquid policy/preview/command/hook failure boundaries;
- snapshot tests: round-trip payloads, isolated IDs, restore ordering, cleanup, and resync command
  emission;
- projection tests: sign/map/camera/paint projections are one-way and graphics caches are
  disposed without changing simulation state;
- serial repository build/test only after code exists, using the required wrapper and affected
  project from the repository root. The serial build passed with 0 warnings and 0 errors; tests
  were not run because this task forbids test changes and test execution was not required.

## Integration Handoff

The execution companion is
`D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\non-authoritative\2026-09-11-version4-non-authoritative-P04-world-tiles-storage-component-execution.md`.
This document now records the authorized Component-only implementation checkpoint below;
`crossSubsystemOwner: integration-review` still applies to every shared type, ID, snapshot,
external reference, and scheduler edge named in this document.

## Component Implementation Checkpoint 1

- component: `WorldGeometryComponent`
- implementationStatus: `implemented-in-source; verification pending`
- actualComponentFile: `D:\TRbackup\NLTX\src2\WorldStorage\Geometry\WorldGeometryComponent.cs`
- sourceChange: added a sealed component for world bounds, tile dimensions, and section dimensions;
  the constructor rejects non-finite bounds, reversed bounds, non-positive tile dimensions, and
  negative section dimensions
- dependencyEvidence: component uses only BCL primitives and owns no initialization, registry,
  mutation, projection, persistence, network, or scheduler behavior
- verificationStatus: `not-run`; serial affected-project build has not yet been executed
- evidence-gap: the Version4 geometry fields and defaults are confirmed in `Main.cs`, but their
  canonical src2 producer, resize lifecycle, and section derivation contract remain unverified
- blocking-decision: `crossSubsystemOwner: integration-review` must confirm the geometry
  initialization owner and whether section counts are supplied or derived before callers are moved
- excludedThisCheckpoint: no System, Query, Command, Adapter, Projection, Registry, Scheduler,
  test, or project-file code was added

## Component Implementation Checkpoint 10

- component: `TileEntityUpdateScheduleComponent`
- implementationStatus: `implemented-in-source; verification pending`
- actualComponentFile: `D:\TRbackup\NLTX\src2\WorldStorage\TileEntities\TileEntityUpdateScheduleComponent.cs`
- sourceChange: added a sealed component for the entity-level `RequiresUpdates` state
- dependencyEvidence: the component does not contain Version4's global `UpdateEntities` list;
  that collection is a scheduler-owned set and remains deferred rather than being modeled as
  entity state without an owner
- verificationStatus: `not-run`; serial affected-project build remains pending
- evidence-gap: update collection ownership, snapshot timing, deferred removal, and server/client
  execution boundaries are not closed
- blocking-decision: `crossSubsystemOwner: integration-review` must approve the update scheduler
  owner and deferred-removal lifecycle before the global collection is implemented
- excludedThisCheckpoint: no System, Query, Command, Adapter, Projection, Registry, Scheduler,
  test, or project-file code was added

## Component-Only Implementation Inventory

The following ten Component files are the only P04 source files modified by this implementation
session:

- `D:\TRbackup\NLTX\src2\WorldStorage\Geometry\WorldGeometryComponent.cs`
- `D:\TRbackup\NLTX\src2\WorldStorage\Geometry\WorldCapacityLimitsComponent.cs`
- `D:\TRbackup\NLTX\src2\WorldStorage\Tiles\TileCellMaterialComponent.cs`
- `D:\TRbackup\NLTX\src2\WorldStorage\Tiles\TileCellLiquidComponent.cs`
- `D:\TRbackup\NLTX\src2\WorldStorage\Tiles\TileCellFrameBitComponent.cs`
- `D:\TRbackup\NLTX\src2\WorldStorage\TileEntities\TileEntityCapacityPolicyComponent.cs`
- `D:\TRbackup\NLTX\src2\WorldStorage\TileEntities\TileEntityRuntimeIdentityComponent.cs`
- `D:\TRbackup\NLTX\src2\WorldStorage\TileEntities\TileEntityAnchorComponent.cs`
- `D:\TRbackup\NLTX\src2\WorldStorage\TileEntities\TileEntityKindComponent.cs`
- `D:\TRbackup\NLTX\src2\WorldStorage\TileEntities\TileEntityUpdateScheduleComponent.cs`

No catalog, TileMap store, codec, mutation, lifecycle, scheduler, registry, adapter, projection,
test, or project-file source was added.

## Component Implementation Checkpoint 9

- component: `TileEntityKindComponent`
- implementationStatus: `implemented-in-source; verification pending`
- actualComponentFile: `D:\TRbackup\NLTX\src2\WorldStorage\TileEntities\TileEntityKindComponent.cs`
- sourceChange: added a sealed component for the Version4 byte TileEntity kind identifier
- dependencyEvidence: the component stores only the kind value; kind registration, lookup, and
  per-kind behavior remain outside the Component
- verificationStatus: `not-run`; serial affected-project build remains pending
- evidence-gap: Version4 confirms the byte field, but the stable catalog owner and relationship to
  persistent/network kind identifiers are not closed
- blocking-decision: `crossSubsystemOwner: integration-review` must approve kind registration and
  identity separation before lookup code is introduced
- excludedThisCheckpoint: no System, Query, Command, Adapter, Projection, Registry, Scheduler,
  test, or project-file code was added

## Component Implementation Checkpoint 8

- component: `TileEntityAnchorComponent`
- implementationStatus: `implemented-in-source; verification pending`
- actualComponentFile: `D:\TRbackup\NLTX\src2\WorldStorage\TileEntities\TileEntityAnchorComponent.cs`
- sourceChange: added a sealed component containing the signed 16-bit TileEntity anchor origin
  coordinates
- dependencyEvidence: the component represents an anchor value only; footprint definitions,
  position indexing, bounds checks, and movement ownership remain outside the component
- verificationStatus: `not-run`; serial affected-project build remains pending
- evidence-gap: Version4 confirms a `Point16`-shaped position, but src2 coordinate value identity,
  footprint semantics, and anchor-index lifecycle are not closed
- blocking-decision: `crossSubsystemOwner: integration-review` must settle the coordinate value and
  multi-tile footprint owner before index or placement code consumes this component
- excludedThisCheckpoint: no System, Query, Command, Adapter, Projection, Registry, Scheduler,
  test, or project-file code was added

## Component Implementation Checkpoint 7

- component: `TileEntityRuntimeIdentityComponent`
- implementationStatus: `implemented-in-source; verification pending`
- actualComponentFile: `D:\TRbackup\NLTX\src2\WorldStorage\TileEntities\TileEntityRuntimeIdentityComponent.cs`
- sourceChange: added a sealed component for the non-negative process/world-session runtime ID
- dependencyEvidence: runtime identity is kept separate from coordinate, kind, persistent, and
  network identity; no registry or ID allocator was added
- verificationStatus: `not-run`; serial affected-project build remains pending
- evidence-gap: Version4 confirms the integer runtime ID, but allocation, reuse, persistence, and
  network identity policies are explicitly not closed
- blocking-decision: `crossSubsystemOwner: integration-review` must settle runtime-ID lifecycle
  and its separation from persistent/network IDs before a registry consumes this component
- excludedThisCheckpoint: no System, Query, Command, Adapter, Projection, Registry, Scheduler,
  test, or project-file code was added

## Component Implementation Checkpoint 6

- component: `TileEntityCapacityPolicyComponent`
- implementationStatus: `implemented-in-source; verification pending`
- actualComponentFile: `D:\TRbackup\NLTX\src2\WorldStorage\TileEntities\TileEntityCapacityPolicyComponent.cs`
- sourceChange: added a sealed component for the positive per-chunk TileEntity capacity policy;
  the Version4 value is represented as component state without implementing chunk allocation
- dependencyEvidence: component uses only a primitive value and owns no registry, lock, allocator,
  lifecycle, or external TileEntity reference
- verificationStatus: `not-run`; serial affected-project build remains pending
- evidence-gap: Version4 confirms `MaxEntitiesPerChunk`, but chunk identity, capacity scope, and
  overflow behavior are not closed in src2
- blocking-decision: `crossSubsystemOwner: integration-review` must define the capacity policy
  consumer and overflow handling before registry code is introduced
- excludedThisCheckpoint: no System, Query, Command, Adapter, Projection, Registry, Scheduler,
  test, or project-file code was added

## Component Implementation Checkpoint 5

- component: `TileCellFrameBitComponent`
- implementationStatus: `implemented-in-source; verification pending`
- actualComponentFile: `D:\TRbackup\NLTX\src2\WorldStorage\Tiles\TileCellFrameBitComponent.cs`
- sourceChange: added a sealed component for the four Version4 header storage values and frame
  coordinates, preserving the source field widths
- dependencyEvidence: bit constants, slope interpretation, packed codec behavior, framing, and
  mutation are intentionally excluded from the component
- verificationStatus: `not-run`; serial affected-project build remains pending
- evidence-gap: Version4 confirms the storage fields, but header-bit semantics and the one-write
  mutation owner are not closed; public setters would risk an unverified packed invariant
- blocking-decision: `crossSubsystemOwner: integration-review` must approve the bit codec and
  mutation owner before this component receives replacement/update APIs
- excludedThisCheckpoint: no System, Query, Command, Adapter, Projection, Registry, Scheduler,
  test, or project-file code was added

## Component Implementation Checkpoint 4

- component: `TileCellLiquidComponent`
- implementationStatus: `implemented-in-source; verification pending`
- actualComponentFile: `D:\TRbackup\NLTX\src2\WorldStorage\Tiles\TileCellLiquidComponent.cs`
- sourceChange: added a sealed component for the Tile liquid type code and amount; it accepts only
  the four Version4 liquid type values and does not perform propagation or packing
- dependencyEvidence: the component is BCL-only and keeps liquid propagation, header packing, and
  neighbor effects outside the Component
- verificationStatus: `not-run`; serial affected-project build remains pending
- evidence-gap: Version4 stores liquid amount separately from liquid-kind header bits, but the
  canonical src2 codec and mutation owner are not closed
- blocking-decision: `crossSubsystemOwner: integration-review` must approve liquid kind encoding and
  the single mutation owner before any compatibility setter is added
- excludedThisCheckpoint: no System, Query, Command, Adapter, Projection, Registry, Scheduler,
  test, or project-file code was added

## Component Implementation Checkpoint 3

- component: `TileCellMaterialComponent`
- implementationStatus: `implemented-in-source; verification pending`
- actualComponentFile: `D:\TRbackup\NLTX\src2\WorldStorage\Tiles\TileCellMaterialComponent.cs`
- sourceChange: added a sealed component containing the Version4 tile and wall material type values
  as `ushort` properties
- dependencyEvidence: the component contains no active-bit, liquid, frame, packed-header, map, or
  mutation behavior; those boundaries remain separate or deferred
- verificationStatus: `not-run`; serial affected-project build remains pending
- evidence-gap: Version4 confirms the two field types, but canonical material mutation ownership
  and the active-bit relationship are not closed in src2
- blocking-decision: `crossSubsystemOwner: integration-review` must approve the Tile cell mutation
  owner before any mutable compatibility API is introduced
- excludedThisCheckpoint: no System, Query, Command, Adapter, Projection, Registry, Scheduler,
  test, or project-file code was added

## Component Implementation Checkpoint 2

- component: `WorldCapacityLimitsComponent`
- implementationStatus: `implemented-in-source; verification pending`
- actualComponentFile: `D:\TRbackup\NLTX\src2\WorldStorage\Geometry\WorldCapacityLimitsComponent.cs`
- sourceChange: added a sealed component that stores positive maximum network-player and NPC
  capacities and rejects invalid limits
- dependencyEvidence: component uses only BCL primitives and does not allocate or register entities
- verificationStatus: `not-run`; serial affected-project build remains pending
- evidence-gap: the Version4 `maxNetPlayers` and `maxNPCs` fields are confirmed, but the src2
  capacity producer and the relationship between NPC capacity and external NPC storage are not
  closed
- blocking-decision: `crossSubsystemOwner: integration-review` must confirm whether NPC capacity
  includes a sentinel slot before any storage allocation is connected
- excludedThisCheckpoint: no System, Query, Command, Adapter, Projection, Registry, Scheduler,
  test, or project-file code was added
