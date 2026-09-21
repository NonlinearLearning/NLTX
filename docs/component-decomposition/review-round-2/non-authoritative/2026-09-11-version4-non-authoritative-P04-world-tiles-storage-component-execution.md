# Version4 P04 World Tiles and Storage: Proposed Execution Plan

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

## Execution Boundary

This execution record includes the authorized Component-only implementation checkpoint. The ten
Component files listed below were created under src2; no project-file change, test code, test
execution, runtime parity check, or behavior-equivalence verification was performed.

The only source inventory for this plan is
`D:\TRbackup\NLTX\docs\migration\ledgers\non-authoritative-component-partitions\04-world-tiles-storage.md`:
23 leaf groups, 310 fields, 10 properties, and 320 members. Source numbers are trace identifiers;
they do not define implementation or scheduler order.

## Proposed Target Layout

The following paths and namespaces are candidates and must be checked against the ECS file and
component naming constraints before implementation. Each proposed public type gets one same-named
PascalCase file. The current NLTX partial models are migration inputs, not evidence that these
targets already exist.

| Capability | Proposed directory | Proposed namespace | Proposed core owners |
|---|---|---|---|
| geometry and capacity | `src2\WorldStorage\Geometry\` | `NLTX.WorldStorage.Geometry` | `WorldGeometryComponent`, `WorldCapacityLimitsComponent`, `WorldGeometryInitializationSystem` |
| content catalogs | `src2\Content\` | `NLTX.Content` | `TileBehaviorCatalogComponent`, `TileLightingCatalogComponent`, `TileFramingCatalogComponent`, `WallMetadataCatalogComponent` |
| map and cell storage | `src2\WorldStorage\Tiles\` | `NLTX.WorldStorage.Tiles` | `TileMapStoreComponent`, `TileCellMaterialComponent`, `TileCellLiquidComponent`, `TileCellFrameBitComponent` |
| tile mutation and work | `src2\WorldInteraction\Tiles\` | `NLTX.WorldInteraction.Tiles` | `TileCellMutationSystem`, `LiquidPropagationSystem`, `TileFramingSystem`, `TileDirtySectionSystem` |
| placement and queries | `src2\WorldInteraction\Placement\` | `NLTX.WorldInteraction.Placement` | `TilePlacementValue`, `TilePlacementCommandSystem`, `TileAnchorQuery`, `TilePlacementPreviewQuery` |
| TileEntity state | `src2\WorldInteraction\TileEntities\` and `src2\WorldStorage\TileEntities\` | `NLTX.WorldInteraction.TileEntities` and `NLTX.WorldStorage.TileEntities` | `TileEntityRegistryStore`, identity/anchor/kind/update components, kind systems |
| framing and signs | `src2\WorldInteraction\Tiles\Framing\` and `src2\WorldStorage\Signs\` | `NLTX.WorldInteraction.Tiles.Framing` and `NLTX.WorldStorage.Signs` | `TileFramingLookupCatalog`, `WorldSignRecord`, `WorldSignStore` |
| snapshots and external protocols | `src2\WorldStorage\Snapshots\` and `src2\Adapters\` | `NLTX.WorldStorage.Snapshots` and `NLTX.Adapters` | snapshot payloads, persistence/network/placement/tile codecs |
| client paint | `src2\Client\TilePaint\` | `NLTX.Client.TilePaint` | `TilePaintRenderCacheAdapter`, variation keys, color projection |

`crossSubsystemOwner: integration-review` applies to any proposed target that crosses Player,
NPC, Item, Projectile, Wiring, networking, persistence, content registration, or scheduler
boundaries.

## Source-to-Target Mapping

The following mapping covers every P04 inventory member. It is a migration map, not a claim that
the target exists. Where one source field is a packed constant, cache, callback, or external
object, the target role deliberately says `Adapter`, `Projection`, `definition`, or `transient`
rather than forcing it into an authoritative Component.

### MainWorldGeometryAndCapacity (11)

| Source | Source member | Proposed target role/file |
|---:|---|---|
| 174 | `Terraria.Main.leftWorld` | Component -> `WorldGeometryComponent.cs` |
| 175 | `Terraria.Main.rightWorld` | Component -> `WorldGeometryComponent.cs` |
| 176 | `Terraria.Main.topWorld` | Component -> `WorldGeometryComponent.cs` |
| 177 | `Terraria.Main.bottomWorld` | Component -> `WorldGeometryComponent.cs` |
| 178 | `Terraria.Main.maxTilesX` | derived Component field -> `WorldGeometryComponent.cs`; calculated by `WorldGeometryInitializationSystem.cs` |
| 179 | `Terraria.Main.maxTilesY` | derived Component field -> `WorldGeometryComponent.cs`; calculated by `WorldGeometryInitializationSystem.cs` |
| 180 | `Terraria.Main.maxSectionsX` | derived Component field -> `WorldGeometryComponent.cs`; calculated by `WorldGeometryInitializationSystem.cs` |
| 181 | `Terraria.Main.maxSectionsY` | derived Component field -> `WorldGeometryComponent.cs`; calculated by `WorldGeometryInitializationSystem.cs` |
| 182 | `Terraria.Main.maxDustToDraw` | Projection -> `RenderCapacityProjection.cs` |
| 183 | `Terraria.Main.maxNetPlayers` | Component -> `WorldCapacityLimitsComponent.cs` |
| 184 | `Terraria.Main.maxNPCs` | Component -> `WorldCapacityLimitsComponent.cs` |

### MainCameraAndLiquidState (5)

| Source | Source member | Proposed target role/file |
|---:|---|---|
| 193 | `Terraria.Main.invBottom` | Projection -> `ClientInventoryLayoutProjection.cs` |
| 194 | `Terraria.Main.cameraX` | Projection -> `ClientCameraViewStateProjection.cs` |
| 195 | `Terraria.Main.cameraY` | Projection -> `ClientCameraViewStateProjection.cs` |
| 196 | `Terraria.Main.liquidAlpha` | Projection -> `LiquidPresentationStateProjection.cs` |
| 197 | `Terraria.Main.waterStyle` | Projection -> `LiquidPresentationStateProjection.cs` |

### MainTileFrameAndCatchMetadata (5)

| Source | Source member | Proposed target role/file |
|---:|---|---|
| 369 | `Terraria.Main.tileSand` | Component -> `TileSurfaceTagCatalogComponent.cs` |
| 370 | `Terraria.Main.tileFlame` | Component -> `TileHazardTagCatalogComponent.cs` |
| 371 | `Terraria.Main.npcCatchable` | Component -> `NpcCatchabilityCatalogComponent.cs` |
| 372 | `Terraria.Main.tileFrame` | Component -> `TileFrameRuntimeStateComponent.cs` |
| 373 | `Terraria.Main.tileFrameCounter` | Component -> `TileFrameRuntimeStateComponent.cs` |

### MainWorldMapAndTileStore (2)

| Source | Source member | Proposed target role/file |
|---:|---|---|
| 374 | `Terraria.Main.Map` | Projection -> `WorldMapProjection.cs` |
| 375 | `Terraria.Main.tile` | Store/Component -> `TileMapStoreComponent.cs` |

### MainWallAndGlobalTileMetadata (6)

| Source | Source member | Proposed target role/file |
|---:|---|---|
| 255 | `Terraria.Main.musicFade` | Projection -> `WorldMusicFadeProjection.cs` |
| 262 | `Terraria.Main.wallHouse` | Component -> `WallMetadataCatalogComponent.cs` |
| 263 | `Terraria.Main.wallDungeon` | Component -> `WallMetadataCatalogComponent.cs` |
| 264 | `Terraria.Main.wallLight` | Component -> `WallMetadataCatalogComponent.cs` |
| 265 | `Terraria.Main.wallBlend` | Component -> `WallMetadataCatalogComponent.cs` |
| 281 | `Terraria.Main.wallLargeFrames` | Component -> `WallFramingCatalogComponent.cs` |

### MainTileBehaviorAndInteractionMetadata (20)

| Source | Source member | Proposed target role/file |
|---:|---|---|
| 257 | `Terraria.Main.tileMergeDirt` | Component -> `TileBehaviorCatalogComponent.cs` |
| 258 | `Terraria.Main.tileCut` | Component -> `TileBehaviorCatalogComponent.cs` |
| 259 | `Terraria.Main.tileAlch` | Component -> `TileBehaviorCatalogComponent.cs` |
| 266 | `Terraria.Main.tileStone` | Component -> `TileBehaviorCatalogComponent.cs` |
| 267 | `Terraria.Main.tileAxe` | Component -> `TileBehaviorCatalogComponent.cs` |
| 268 | `Terraria.Main.tileHammer` | Component -> `TileBehaviorCatalogComponent.cs` |
| 269 | `Terraria.Main.tileWaterDeath` | Component -> `TileBehaviorCatalogComponent.cs` |
| 270 | `Terraria.Main.tileLavaDeath` | Component -> `TileBehaviorCatalogComponent.cs` |
| 271 | `Terraria.Main.tileTable` | Component -> `TileBehaviorCatalogComponent.cs` |
| 276 | `Terraria.Main.tileSolidTop` | Component -> `TileBehaviorCatalogComponent.cs` |
| 277 | `Terraria.Main.tileSolid` | Component -> `TileBehaviorCatalogComponent.cs` |
| 278 | `Terraria.Main.tileBouncy` | Component -> `TileBehaviorCatalogComponent.cs` |
| 279 | `Terraria.Main.tileOreFinderPriority` | Component -> `TileBehaviorCatalogComponent.cs` |
| 282 | `Terraria.Main.tileRope` | Component -> `TileBehaviorCatalogComponent.cs` |
| 285 | `Terraria.Main.tileNoAttach` | Component -> `TileBehaviorCatalogComponent.cs` |
| 286 | `Terraria.Main.tileNoFail` | Component -> `TileBehaviorCatalogComponent.cs` |
| 292 | `Terraria.Main.tileGlowMask` | Component -> `TileLightingCatalogComponent.cs` |
| 293 | `Terraria.Main.tileContainer` | Component -> `TileBehaviorCatalogComponent.cs` |
| 294 | `Terraria.Main.tileSign` | Component -> `TileBehaviorCatalogComponent.cs` |
| 295 | `Terraria.Main.tileMerge` | Component -> `TileBehaviorCatalogComponent.cs` |

### MainTileLightingAndFrameMetadata (15)

| Source | Source member | Proposed target role/file |
|---:|---|---|
| 256 | `Terraria.Main.tileLighted` | Component -> `TileLightingCatalogComponent.cs` |
| 260 | `Terraria.Main.tileShine` | Component -> `TileLightingCatalogComponent.cs` |
| 261 | `Terraria.Main.tileShine2` | Component -> `TileLightingCatalogComponent.cs` |
| 272 | `Terraria.Main.tileBlockLight` | Component -> `TileLightingCatalogComponent.cs` |
| 273 | `Terraria.Main.tileNoSunLight` | Component -> `TileLightingCatalogComponent.cs` |
| 274 | `Terraria.Main.tileDungeon` | Component -> `TileEnvironmentTagCatalogComponent.cs` |
| 275 | `Terraria.Main.tileSpelunker` | Component -> `TileEnvironmentTagCatalogComponent.cs` |
| 280 | `Terraria.Main.tileLargeFrames` | Component -> `TileFramingCatalogComponent.cs` |
| 283 | `Terraria.Main.tileBrick` | Component -> `TileSurfaceTagCatalogComponent.cs` |
| 284 | `Terraria.Main.tileMoss` | Component -> `TileSurfaceTagCatalogComponent.cs` |
| 287 | `Terraria.Main.tileCracked` | Component -> `TileSurfaceTagCatalogComponent.cs` |
| 288 | `Terraria.Main.tileObsidianKill` | Component -> `TileHazardTagCatalogComponent.cs` |
| 289 | `Terraria.Main.tileFrameImportant` | Component -> `TileFramingCatalogComponent.cs` |
| 290 | `Terraria.Main.tilePile` | Component -> `TileFramingCatalogComponent.cs` |
| 291 | `Terraria.Main.tileBlendAll` | Component -> `TileFramingCatalogComponent.cs` |

### TileCellMaterialAndLiquidState (9)

| Source | Source member | Proposed target role/file |
|---:|---|---|
| 788 | `Terraria.Tile.type` | Component -> `TileCellMaterialComponent.cs` |
| 789 | `Terraria.Tile.wall` | Component -> `TileCellMaterialComponent.cs` |
| 790 | `Terraria.Tile.liquid` | Component -> `TileCellLiquidComponent.cs` |
| 812 | `Terraria.Tile.Liquid_Water` | definition -> `LiquidKindDefinition.cs` |
| 813 | `Terraria.Tile.Liquid_Lava` | definition -> `LiquidKindDefinition.cs` |
| 814 | `Terraria.Tile.Liquid_Honey` | definition -> `LiquidKindDefinition.cs` |
| 815 | `Terraria.Tile.Liquid_Shimmer` | definition -> `LiquidKindDefinition.cs` |
| 816 | `Terraria.Tile.NeitherLavaOrHoney` | Adapter -> `TileLiquidPackingCodec.cs` |
| 817 | `Terraria.Tile.EitherLavaOrHoney` | Adapter -> `TileLiquidPackingCodec.cs` |

### TileCellFrameAndBitState (21)

| Source | Source member | Proposed target role/file |
|---:|---|---|
| 791 | `Terraria.Tile.sTileHeader` | Component -> `TileCellFrameBitComponent.cs` |
| 792 | `Terraria.Tile.bTileHeader` | Component -> `TileCellFrameBitComponent.cs` |
| 793 | `Terraria.Tile.bTileHeader2` | Component -> `TileCellFrameBitComponent.cs` |
| 794 | `Terraria.Tile.bTileHeader3` | Component -> `TileCellFrameBitComponent.cs` |
| 795 | `Terraria.Tile.frameX` | Component -> `TileCellFrameBitComponent.cs` |
| 796 | `Terraria.Tile.frameY` | Component -> `TileCellFrameBitComponent.cs` |
| 797 | `Terraria.Tile.Bit0` | Adapter -> `TileCellBitCodec.cs` |
| 798 | `Terraria.Tile.Bit1` | Adapter -> `TileCellBitCodec.cs` |
| 799 | `Terraria.Tile.Bit2` | Adapter -> `TileCellBitCodec.cs` |
| 800 | `Terraria.Tile.Bit3` | Adapter -> `TileCellBitCodec.cs` |
| 801 | `Terraria.Tile.Bit4` | Adapter -> `TileCellBitCodec.cs` |
| 802 | `Terraria.Tile.Bit5` | Adapter -> `TileCellBitCodec.cs` |
| 803 | `Terraria.Tile.Bit6` | Adapter -> `TileCellBitCodec.cs` |
| 804 | `Terraria.Tile.Bit7` | Adapter -> `TileCellBitCodec.cs` |
| 805 | `Terraria.Tile.Bit15` | Adapter -> `TileCellBitCodec.cs` |
| 806 | `Terraria.Tile.Type_Solid` | Adapter -> `TileCellBitCodec.cs` |
| 807 | `Terraria.Tile.Type_Halfbrick` | Adapter -> `TileCellBitCodec.cs` |
| 808 | `Terraria.Tile.Type_SlopeDownRight` | Adapter -> `TileCellBitCodec.cs` |
| 809 | `Terraria.Tile.Type_SlopeDownLeft` | Adapter -> `TileCellBitCodec.cs` |
| 810 | `Terraria.Tile.Type_SlopeUpRight` | Adapter -> `TileCellBitCodec.cs` |
| 811 | `Terraria.Tile.Type_SlopeUpLeft` | Adapter -> `TileCellBitCodec.cs` |

### SharedTileAnchorAndReachQueries (18)

| Source | Source member | Proposed target role/file |
|---:|---|---|
| 1033 | `Terraria.DataStructures.AnchorData.type` | value -> `AnchorSpec.cs` |
| 1034 | `Terraria.DataStructures.AnchorData.tileCount` | value -> `AnchorSpec.cs` |
| 1035 | `Terraria.DataStructures.AnchorData.checkStart` | value -> `AnchorSpec.cs` |
| 1036 | `Terraria.DataStructures.AnchorData.Empty` | value factory -> `AnchorSpec.cs` |
| 1037 | `AnchoredEntitiesCollection.IndexPointPair.index` | value -> `AnchorIndexEntry.cs` |
| 1038 | `AnchoredEntitiesCollection.IndexPointPair.coords` | value -> `AnchorIndexEntry.cs` |
| 1039 | `AnchoredEntitiesCollection._anchoredNPCs` | Store -> `AnchorIndexStore.cs` |
| 1040 | `AnchoredEntitiesCollection._anchoredPlayers` | Store -> `AnchorIndexStore.cs` |
| 1263 | `Terraria.DataStructures.Point16.X` | value -> existing/proposed `TileCoordinate.cs` |
| 1264 | `Terraria.DataStructures.Point16.Y` | value -> existing/proposed `TileCoordinate.cs` |
| 1265 | `Terraria.DataStructures.Point16.Zero` | value constant -> `TileCoordinate.cs` |
| 1266 | `Terraria.DataStructures.Point16.NegativeOne` | value constant -> `TileCoordinate.cs` |
| 1329 | `TileReachCheckSettings.TileRangeMultiplier` | value -> `TileReachPolicy.cs` |
| 1330 | `TileReachCheckSettings.TileReachLimit` | value -> `TileReachPolicy.cs` |
| 1331 | `TileReachCheckSettings.OverrideXReach` | value -> `TileReachPolicy.cs` |
| 1332 | `TileReachCheckSettings.OverrideYReach` | value -> `TileReachPolicy.cs` |
| 1333 | `TileReachCheckSettings.Simple` | value preset -> `TileReachPolicy.cs` |
| 1334 | `TileReachCheckSettings.Pylons` | value preset -> `TileReachPolicy.cs`; owner integration-review |

### SharedTileFramingAndSignState (16)

| Source | Source member | Proposed target role/file |
|---:|---|---|
| 3078 | `Terraria.Framing.BlockStyle.top` | value -> `FrameNeighborMask.cs` |
| 3079 | `Terraria.Framing.BlockStyle.bottom` | value -> `FrameNeighborMask.cs` |
| 3080 | `Terraria.Framing.BlockStyle.left` | value -> `FrameNeighborMask.cs` |
| 3081 | `Terraria.Framing.BlockStyle.right` | value -> `FrameNeighborMask.cs` |
| 3082 | `Terraria.Framing.selfFrame8WayLookup` | Component -> `TileFramingLookupCatalog.cs` |
| 3083 | `Terraria.Framing.wallFrameLookup` | Component -> `TileFramingLookupCatalog.cs` |
| 3084 | `Terraria.Framing.frameSize8Way` | Component -> `TileFramingLookupCatalog.cs` |
| 3085 | `Terraria.Framing.wallFrameSize` | Component -> `TileFramingLookupCatalog.cs` |
| 3086 | `Terraria.Framing.blockStyleLookup` | Component -> `TileFramingLookupCatalog.cs` |
| 3087 | `Terraria.Framing.phlebasTileFrameNumberLookup` | Component -> `TileFramingLookupCatalog.cs` |
| 3088 | `Terraria.Framing.lazureTileFrameNumberLookup` | Component -> `TileFramingLookupCatalog.cs` |
| 3089 | `Terraria.Framing.centerWallFrameLookup` | Component -> `TileFramingLookupCatalog.cs` |
| 3487 | `Terraria.Sign.maxSigns` | Component -> `WorldSignCapacityPolicy.cs` |
| 3488 | `Terraria.Sign.x` | Component -> `WorldSignRecord.cs` |
| 3489 | `Terraria.Sign.y` | Component -> `WorldSignRecord.cs` |
| 3490 | `Terraria.Sign.text` | Component -> `WorldSignRecord.cs` |

### SharedTileSnapshots (19)

| Source | Source member | Proposed target role/file |
|---:|---|---|
| 2967 | `TileSnapshot.TileStruct._type` | value -> `TileCellSnapshotPayload.cs` |
| 2968 | `TileSnapshot.TileStruct._wall_bTileHeader3_packed` | value -> `TileCellSnapshotPayload.cs` |
| 2969 | `TileSnapshot.TileStruct._sTileHeader` | value -> `TileCellSnapshotPayload.cs` |
| 2970 | `TileSnapshot.TileStruct._frameX` | value -> `TileCellSnapshotPayload.cs` |
| 2971 | `TileSnapshot.TileStruct._frameY` | value -> `TileCellSnapshotPayload.cs` |
| 2972 | `TileSnapshot.TileStruct._liquid` | value -> `TileCellSnapshotPayload.cs` |
| 2973 | `TileSnapshot.TileStruct._bTileHeader` | value -> `TileCellSnapshotPayload.cs` |
| 2974 | `TileSnapshot.TileStruct._i0` | codec storage -> `TileCellSnapshotPayload.cs` |
| 2975 | `TileSnapshot.TileStruct._i1` | codec storage -> `TileCellSnapshotPayload.cs` |
| 2976 | `TileSnapshot.TileStruct._i2` | codec storage -> `TileCellSnapshotPayload.cs` |
| 2977 | `TileSnapshot.TileStruct._liquidNames` | Adapter -> `TileSnapshotCodec.cs` |
| 2978 | `TileSnapshot._worldFile` | Adapter -> `TileSnapshotPersistenceAdapter.cs` |
| 2979 | `TileSnapshot._tiles` | transient Projection -> `TileSnapshotBuffer.cs` |
| 2980 | `TileSnapshot._tileEntities` | transient Projection -> `TileEntitySnapshotBuffer.cs`; owner integration-review |
| 2981 | `TileSnapshot._chests` | Adapter -> `ChestSnapshotPort.cs`; owner integration-review |
| 2982 | `TileSnapshot._tempStream` | Adapter resource -> `TileSnapshotSerializationAdapter.cs` |
| 2983 | `TileSnapshot._tempWriter` | Adapter resource -> `TileSnapshotSerializationAdapter.cs` |
| 2984 | `TileSnapshot._tempReader` | Adapter resource -> `TileSnapshotSerializationAdapter.cs` |
| 3941 | `TileSnapshot.Context` | command context -> `TileSnapshotContext.cs` |

### TileEntityRegistryAndBaseState (14)

| Source | Source member | Proposed target role/file |
|---:|---|---|
| 1299 | `TileEntitiesManager._nextEntityID` | Store allocator -> `TileEntityRegistryStore.cs` |
| 1300 | `TileEntitiesManager._types` | Store kind registry -> `TileEntityKindRegistry.cs` |
| 1301 | `TileEntity.manager` | dependency port -> `TileEntityRegistryStore.cs` |
| 1302 | `TileEntity.MaxEntitiesPerChunk` | Component policy -> `TileEntityCapacityPolicy.cs` |
| 1303 | `TileEntity.EntityCreationLock` | lifecycle synchronization -> `TileEntityLifecycleSystem.cs` |
| 1304 | `TileEntity.UpdateEntities` | Component -> `TileEntityUpdateScheduleComponent.cs` |
| 1305 | `TileEntity.ByID` | Store runtime index -> `TileEntityRegistryStore.cs` |
| 1306 | `TileEntity.ByPosition` | Store anchor index -> `TileEntityRegistryStore.cs` |
| 1307 | `TileEntity.TileEntitiesNextID` | allocator -> `TileEntityLifecycleSystem.cs` |
| 1308 | `TileEntity.ID` | Component -> `TileEntityRuntimeIdentityComponent.cs` |
| 1309 | `TileEntity.Position` | Component -> `TileEntityAnchorComponent.cs` |
| 1310 | `TileEntity.type` | Component -> `TileEntityKindComponent.cs` |
| 1311 | `TileEntity.RequiresUpdates` | Component -> `TileEntityUpdateScheduleComponent.cs` |
| 1312 | `TileEntityType<T>.EntityTypeID` | Catalog -> `TileEntityKindCatalog.cs`; owner integration-review |

### TileEntityDisplayAndInventoryState (24)

| Source | Source member | Proposed target role/file |
|---:|---|---|
| 2191 | `TEDeadCellsDisplayJar.item` | Component -> `SingleItemDisplayComponent.cs` |
| 2192 | `TEDisplayDoll.DisplayDollPose.Pose` | value/component -> `DisplayPoseComponent.cs` |
| 2193 | `TEDisplayDoll.DisplayDollPose.ItemAnimationPercent` | value -> `DisplayPoseState.cs` |
| 2194 | `TEDisplayDoll.DisplayDollPose.ItemAimRadians` | value -> `DisplayPoseState.cs` |
| 2195 | `TEDisplayDoll.MyTileID` | definition -> `TileEntityFootprintDefinition.cs` |
| 2196 | `TEDisplayDoll.entityTileWidth` | definition -> `TileEntityFootprintDefinition.cs` |
| 2197 | `TEDisplayDoll.entityTileHeight` | definition -> `TileEntityFootprintDefinition.cs` |
| 2198 | `TEDisplayDoll._dollPlayer` | Adapter -> `DisplayDollPlayerProjectionAdapter.cs`; owner integration-review |
| 2199 | `TEDisplayDoll._equip` | Component -> `DisplayDollInventoryComponent.cs` |
| 2200 | `TEDisplayDoll._dyes` | Component -> `DisplayDollInventoryComponent.cs` |
| 2201 | `TEDisplayDoll._misc` | Component -> `DisplayDollInventoryComponent.cs` |
| 2202 | `TEDisplayDoll._pose` | Component -> `DisplayPoseComponent.cs` |
| 2203 | `TEDisplayDoll.SupportedUseStylePoses` | definition -> `DisplayDollPoseCatalog.cs` |
| 2204 | `TEDisplayDoll._projectileDummy` | Adapter -> `DisplayDollProjectileProjectionAdapter.cs`; owner integration-review |
| 2205 | `TEFoodPlatter.item` | Component -> `SingleItemDisplayComponent.cs` |
| 2206 | `TEHatRack.MyTileID` | definition -> `TileEntityFootprintDefinition.cs` |
| 2207 | `TEHatRack.entityTileWidth` | definition -> `TileEntityFootprintDefinition.cs` |
| 2208 | `TEHatRack.entityTileHeight` | definition -> `TileEntityFootprintDefinition.cs` |
| 2209 | `TEHatRack._dollPlayer` | Adapter -> `HatRackPlayerProjectionAdapter.cs`; owner integration-review |
| 2210 | `TEHatRack._items` | Component -> `HatRackInventoryComponent.cs` |
| 2211 | `TEHatRack._dyes` | Component -> `HatRackInventoryComponent.cs` |
| 2212 | `TEItemFrame.item` | Component -> `SingleItemDisplayComponent.cs` |
| 2231 | `TEWeaponsRack.item` | Component -> `SingleItemDisplayComponent.cs` |
| 2232 | `TEWeaponsRack.MyTileID` | definition -> `TileEntityFootprintDefinition.cs` |

### TileEntityAnchorAndSensorState (11)

| Source | Source member | Proposed target role/file |
|---:|---|---|
| 2189 | `TECritterAnchor._myEntityID` | Component -> `LeashedEntityAnchorRuntimeComponent.cs`; owner integration-review |
| 2190 | `TECritterAnchor.CritterPrototypes` | definition/Adapter -> `CritterPrototypeCatalog.cs`; owner integration-review |
| 2213 | `TEKiteAnchor._myEntityID` | Component -> `LeashedEntityAnchorRuntimeComponent.cs`; owner integration-review |
| 2215 | `TELogicSensor.playerBox` | transient -> `LogicSensorEvaluationScratch.cs` |
| 2216 | `TELogicSensor.tripPoints` | command queue -> `LogicSensorTripQueue.cs` |
| 2217 | `TELogicSensor.markedIDsForRemoval` | deferred-removal queue -> `LogicSensorSystem.cs` |
| 2218 | `TELogicSensor.inUpdateLoop` | update guard -> `LogicSensorSystem.cs` |
| 2219 | `TELogicSensor.playerBoxFilled` | transient -> `LogicSensorEvaluationScratch.cs` |
| 2220 | `TELogicSensor.logicCheck` | Component -> `LogicSensorComponent.cs` |
| 2221 | `TELogicSensor.On` | Component -> `LogicSensorComponent.cs` |
| 2222 | `TELogicSensor.CountedData` | Component -> `LogicSensorComponent.cs` |

### TileEntityWorldInteractionState (9)

| Source | Source member | Proposed target role/file |
|---:|---|---|
| 2214 | `TELeashedEntityAnchorWithItem.itemType` | Component -> `LeashedEntityAnchorComponent.cs`; external Item owner integration-review |
| 2223 | `TETeleportationPylon.MyTileID` | definition -> `TileEntityFootprintDefinition.cs`; pylon owner integration-review |
| 2224 | `TETeleportationPylon.entityTileWidth` | definition -> `TileEntityFootprintDefinition.cs` |
| 2225 | `TETeleportationPylon.entityTileHeight` | definition -> `TileEntityFootprintDefinition.cs` |
| 2226 | `TETrainingDummy.playerBoxes` | transient -> `TrainingDummyEvaluationScratch.cs` |
| 2227 | `TETrainingDummy.playerBoxFilled` | transient -> `TrainingDummyEvaluationScratch.cs` |
| 2228 | `TETrainingDummy.npcSlotsFull` | transient -> `TrainingDummyEvaluationScratch.cs` |
| 2229 | `TETrainingDummy.npc` | Component -> `TrainingDummyInteractionComponent.cs`; external NPC owner integration-review |
| 2230 | `TETrainingDummy.activationRetryCooldown` | Component -> `TrainingDummyInteractionComponent.cs`; clock/retry owner integration-review |

### SharedTilePlacementAnchorAndHookModules (20)

| Source | Source member | Proposed target role/file |
|---:|---|---|
| 2792 | `AnchorDataModule.top` | value -> `TilePlacementAnchorDefinition.cs` |
| 2793 | `AnchorDataModule.bottom` | value -> `TilePlacementAnchorDefinition.cs` |
| 2794 | `AnchorDataModule.left` | value -> `TilePlacementAnchorDefinition.cs` |
| 2795 | `AnchorDataModule.right` | value -> `TilePlacementAnchorDefinition.cs` |
| 2796 | `AnchorDataModule.wall` | value -> `TilePlacementAnchorDefinition.cs` |
| 2797 | `AnchorTypesModule.tileValid` | Component -> `TilePlacementAnchorCatalog.cs` |
| 2798 | `AnchorTypesModule.tileInvalid` | Component -> `TilePlacementAnchorCatalog.cs` |
| 2799 | `AnchorTypesModule.tileAlternates` | Component -> `TilePlacementAnchorCatalog.cs` |
| 2800 | `AnchorTypesModule.wallValid` | Component -> `TilePlacementAnchorCatalog.cs` |
| 2801 | `LiquidDeathModule.water` | value -> `LiquidPlacementPolicy.cs` |
| 2802 | `LiquidDeathModule.lava` | value -> `LiquidPlacementPolicy.cs` |
| 2803 | `LiquidPlacementModule.water` | value -> `LiquidPlacementPolicy.cs` |
| 2804 | `LiquidPlacementModule.lava` | value -> `LiquidPlacementPolicy.cs` |
| 2805 | `TileObjectAlternatesModule.data` | Component -> `TileObjectStyleCatalog.cs` |
| 2834 | `TileObjectSubTilesModule.data` | Component -> `TileObjectStyleCatalog.cs` |
| 2835 | `TilePlacementHooksModule.check` | Adapter -> `PlacementHookAdapter.cs` |
| 2836 | `TilePlacementHooksModule.postPlaceEveryone` | Adapter -> `PlacementHookAdapter.cs` |
| 2837 | `TilePlacementHooksModule.postPlaceMyPlayer` | Adapter -> `PlacementHookAdapter.cs` |
| 2838 | `TilePlacementHooksModule.placeOverride` | Adapter -> `PlacementHookAdapter.cs` |
| 2839 | `TilePlacementHooksModule.getStyleMethod` | Adapter -> `PlacementHookAdapter.cs` |

### SharedTileObjectPreviewState (25)

| Source | Source member | Proposed target role/file |
|---:|---|---|
| 1313 | `TileObjectPreviewData._type` | transient Projection -> `TileObjectPreviewProjection.cs` |
| 1314 | `TileObjectPreviewData._style` | transient Projection -> `TileObjectPreviewProjection.cs` |
| 1315 | `TileObjectPreviewData._alternate` | transient Projection -> `TileObjectPreviewProjection.cs` |
| 1316 | `TileObjectPreviewData._random` | transient Projection -> `TileObjectPreviewProjection.cs`; randomness owner integration-review |
| 1317 | `TileObjectPreviewData._active` | transient Projection -> `TileObjectPreviewProjection.cs` |
| 1318 | `TileObjectPreviewData._size` | transient Projection -> `TileObjectPreviewProjection.cs` |
| 1319 | `TileObjectPreviewData._coordinates` | transient Projection -> `TileObjectPreviewProjection.cs` |
| 1320 | `TileObjectPreviewData._objectStart` | transient Projection -> `TileObjectPreviewProjection.cs` |
| 1321 | `TileObjectPreviewData._data` | transient Projection -> `TileObjectPreviewGridProjection.cs` |
| 1322 | `TileObjectPreviewData._dataSize` | transient Projection -> `TileObjectPreviewGridProjection.cs` |
| 1323 | `TileObjectPreviewData._percentValid` | Query result -> `TilePlacementValidityView.cs` |
| 1324 | `TileObjectPreviewData.placementCache` | transient cache -> `TileObjectPreviewCache.cs` |
| 1325 | `TileObjectPreviewData.randomCache` | transient cache -> `TileObjectPreviewCache.cs`; randomness owner integration-review |
| 1326 | `TileObjectPreviewData.None` | value constant -> `TileObjectPreviewProjection.cs` |
| 1327 | `TileObjectPreviewData.ValidSpot` | value constant -> `TileObjectPreviewProjection.cs` |
| 1328 | `TileObjectPreviewData.InvalidSpot` | value constant -> `TileObjectPreviewProjection.cs` |
| 3702 | `TileObjectPreviewData.Active` | Query facade -> `TilePlacementPreviewQuery.cs` |
| 3703 | `TileObjectPreviewData.Type` | Query facade -> `TilePlacementPreviewQuery.cs` |
| 3704 | `TileObjectPreviewData.Style` | Query facade -> `TilePlacementPreviewQuery.cs` |
| 3705 | `TileObjectPreviewData.Alternate` | Query facade -> `TilePlacementPreviewQuery.cs` |
| 3706 | `TileObjectPreviewData.Random` | Query facade -> `TilePlacementPreviewQuery.cs` |
| 3707 | `TileObjectPreviewData.Size` | Query facade -> `TilePlacementPreviewQuery.cs` |
| 3708 | `TileObjectPreviewData.Coordinates` | Query facade -> `TilePlacementPreviewQuery.cs` |
| 3709 | `TileObjectPreviewData.ObjectStart` | Query facade -> `TilePlacementPreviewQuery.cs` |
| 3710 | `TileObjectPreviewData.this[]` | Query facade -> `TileObjectPreviewGridQuery.cs` |

### SharedTileObjectPlacementValueState (16)

| Source | Source member | Proposed target role/file |
|---:|---|---|
| 1208 | `PlacementDetails.tileType` | Command value -> `TilePlacementValue.cs` |
| 1209 | `PlacementDetails.tileStyle` | Command value -> `TilePlacementValue.cs` |
| 1210 | `PlacementHook.hook` | Adapter input/result -> `PlacementHookAdapter.cs` |
| 1211 | `PlacementHook.badReturn` | value -> `PlacementHookResult.cs` |
| 1212 | `PlacementHook.badResponse` | value -> `PlacementHookResult.cs` |
| 1213 | `PlacementHook.processedCoordinates` | value -> `PlacementHookResult.cs` |
| 1214 | `PlacementHook.Empty` | value constant -> `PlacementHookResult.cs` |
| 1215 | `PlacementHook.Response_AllInvalid` | value constant -> `PlacementHookResult.cs` |
| 3509 | `TileObject.xCoord` | value -> `TileObjectPlacementValue.cs` |
| 3510 | `TileObject.yCoord` | value -> `TileObjectPlacementValue.cs` |
| 3511 | `TileObject.type` | value -> `TileObjectPlacementValue.cs` |
| 3512 | `TileObject.style` | value -> `TileObjectPlacementValue.cs` |
| 3513 | `TileObject.alternate` | value -> `TileObjectPlacementValue.cs` |
| 3514 | `TileObject.random` | value -> `TileObjectPlacementValue.cs`; randomness owner integration-review |
| 3515 | `TileObject.Empty` | value constant -> `TileObjectPlacementValue.cs` |
| 3516 | `TileObject.objectPreview` | Projection reference -> `TileObjectPlacementValue.cs` |

### SharedTilePlacementCoordinateAndDrawModules (14)

| Source | Source member | Proposed target role/file |
|---:|---|---|
| 2813 | `TileObjectCoordinatesModule.width` | Projection -> `TileObjectCoordinatesProjection.cs` |
| 2814 | `TileObjectCoordinatesModule.heights` | Projection -> `TileObjectCoordinatesProjection.cs` |
| 2815 | `TileObjectCoordinatesModule.padding` | Projection -> `TileObjectCoordinatesProjection.cs` |
| 2816 | `TileObjectCoordinatesModule.paddingFix` | Projection -> `TileObjectCoordinatesProjection.cs` |
| 2817 | `TileObjectCoordinatesModule.styleWidth` | Projection -> `TileObjectCoordinatesProjection.cs` |
| 2818 | `TileObjectCoordinatesModule.styleHeight` | Projection -> `TileObjectCoordinatesProjection.cs` |
| 2819 | `TileObjectCoordinatesModule.calculated` | transient state -> `TileObjectCoordinatesProjection.cs` |
| 2820 | `TileObjectCoordinatesModule.drawStyleOffset` | Projection -> `TileObjectCoordinatesProjection.cs` |
| 2821 | `TileObjectCoordinatesModule.drawFrameOffsets` | Projection -> `TileObjectCoordinatesProjection.cs` |
| 2822 | `TileObjectDrawModule.xOffset` | Projection -> `TileObjectDrawProjection.cs` |
| 2823 | `TileObjectDrawModule.yOffset` | Projection -> `TileObjectDrawProjection.cs` |
| 2824 | `TileObjectDrawModule.flipHorizontal` | Projection -> `TileObjectDrawProjection.cs` |
| 2825 | `TileObjectDrawModule.flipVertical` | Projection -> `TileObjectDrawProjection.cs` |
| 2826 | `TileObjectDrawModule.stepDown` | Projection -> `TileObjectDrawProjection.cs` |

### SharedTilePlacementBaseAndStyleModules (14)

| Source | Source member | Proposed target role/file |
|---:|---|---|
| 2806 | `TileObjectBaseModule.width` | definition -> `TileObjectBaseDefinition.cs` |
| 2807 | `TileObjectBaseModule.height` | definition -> `TileObjectBaseDefinition.cs` |
| 2808 | `TileObjectBaseModule.origin` | definition -> `TileObjectBaseDefinition.cs` |
| 2809 | `TileObjectBaseModule.direction` | definition -> `TileObjectBaseDefinition.cs` |
| 2810 | `TileObjectBaseModule.randomRange` | definition -> `TileObjectBaseDefinition.cs`; randomness owner integration-review |
| 2811 | `TileObjectBaseModule.flattenAnchors` | definition -> `TileObjectBaseDefinition.cs` |
| 2812 | `TileObjectBaseModule.specificRandomStyles` | definition -> `TileObjectBaseDefinition.cs` |
| 2827 | `TileObjectStyleModule.style` | definition -> `TileObjectStyleDefinition.cs` |
| 2828 | `TileObjectStyleModule.horizontal` | definition -> `TileObjectStyleDefinition.cs` |
| 2829 | `TileObjectStyleModule.styleWrapLimit` | definition -> `TileObjectStyleDefinition.cs` |
| 2830 | `TileObjectStyleModule.styleMultiplier` | definition -> `TileObjectStyleDefinition.cs` |
| 2831 | `TileObjectStyleModule.styleLineSkip` | definition -> `TileObjectStyleDefinition.cs` |
| 2832 | `TileObjectStyleModule.styleWrapLimitVisualOverride` | definition -> `TileObjectStyleDefinition.cs` |
| 2833 | `TileObjectStyleModule.styleLineSkipVisualoverride` | definition -> `TileObjectStyleDefinition.cs` |

### SharedTilePaintRenderTargetState (12)

| Source | Source member | Proposed target role/file |
|---:|---|---|
| 2544 | `ARenderTargetHolder.Target` | client Adapter -> `TilePaintRenderCacheAdapter.cs` |
| 2545 | `ARenderTargetHolder._wasPrepared` | client Adapter lifecycle -> `TilePaintRenderCacheAdapter.cs` |
| 2546 | `TreeTopRenderTargetHolder.Key` | client Adapter binding -> `TilePaintRenderCacheAdapter.cs` |
| 2548 | `TileRenderTargetHolder.Key` | client Adapter binding -> `TilePaintRenderCacheAdapter.cs` |
| 2549 | `CageTopRenderTargetHolder.Key` | client Adapter binding -> `TilePaintRenderCacheAdapter.cs` |
| 2550 | `WallRenderTargetHolder.Key` | client Adapter binding -> `TilePaintRenderCacheAdapter.cs` |
| 2561 | `TilePaintSystemV2._cageTopRenders` | client cache -> `TilePaintRenderCacheAdapter.cs` |
| 2562 | `TilePaintSystemV2._tilesRenders` | client cache -> `TilePaintRenderCacheAdapter.cs` |
| 2563 | `TilePaintSystemV2._wallsRenders` | client cache -> `TilePaintRenderCacheAdapter.cs` |
| 2564 | `TilePaintSystemV2._treeTopRenders` | client cache -> `TilePaintRenderCacheAdapter.cs` |
| 2565 | `TilePaintSystemV2._treeBranchRenders` | client cache -> `TilePaintRenderCacheAdapter.cs` |
| 2566 | `TilePaintSystemV2._requests` | client transient queue -> `TilePaintRenderCacheAdapter.cs` |

### SharedTilePaintVariationAndColorState (14)

| Source | Source member | Proposed target role/file |
|---:|---|---|
| 2547 | `TreeBranchTargetHolder.Key` | value -> `TreeFoliageVariationKey.cs` |
| 2551 | `TileVariationkey.TileType` | value -> `TilePaintVariationKey.cs` |
| 2552 | `TileVariationkey.TileStyle` | value -> `TilePaintVariationKey.cs` |
| 2553 | `TileVariationkey.PaintColor` | value -> `TilePaintVariationKey.cs` |
| 2554 | `WallVariationKey.WallType` | value -> `WallPaintVariationKey.cs` |
| 2555 | `WallVariationKey.PaintColor` | value -> `WallPaintVariationKey.cs` |
| 2556 | `TreeFoliageVariantKey.TextureIndex` | value -> `TreeFoliageVariationKey.cs` |
| 2557 | `TreeFoliageVariantKey.TextureStyle` | value -> `TreeFoliageVariationKey.cs` |
| 2558 | `TreeFoliageVariantKey.PaintColor` | value -> `TreeFoliageVariationKey.cs` |
| 2559 | `CageTopVariationkey.CageStyle` | value -> `CageTopVariationKey.cs` |
| 2560 | `CageTopVariationkey.PaintColor` | value -> `CageTopVariationKey.cs` |
| 3506 | `TileColorCache.Color` | Projection -> `TileColorCacheProjection.cs` |
| 3507 | `TileColorCache.FullBright` | Projection -> `TileColorCacheProjection.cs` |
| 3508 | `TileColorCache.Invisible` | Projection -> `TileColorCacheProjection.cs` |

## Proposed Implementation Sequence

Each phase is gated by focused tests and an integration-review checkpoint. A phase may not claim
completion until its owner, readers, writers, lifecycle, and external effects are explicit.

1. **Boundary lock and inventory gate.** Re-read the P04 inventory, create an immutable source
   number manifest for the implementation branch, resolve the missing public-split constraint,
   and approve all `crossSubsystemOwner: integration-review` entries. No source migration occurs
   in this phase.
2. **Geometry and catalog ports.** Introduce proposed geometry/capacity and content catalog
   ports alongside the existing `src\Content` and `src\WorldStorage` partial types. Validate
   dimensions, registry lengths, section calculations, and ownership. Keep old readers on the
   compatibility facade until the new query contract is proven.
3. **Canonical Tile cell store.** Implement the proposed cell material, liquid, frame/bit payloads
   and codecs. Establish `TileCellMutationSystem` as the only writer for coupled cell changes.
   Add mutation commands, dirty-section events, and deterministic cell invariant tests before
   moving callers.
4. **Liquid and framing work.** Port liquid queues/buffers and framing lookup behavior behind
   explicit systems. Verify neighbor effects, map/section dirty output, FrameImportant behavior,
   and failure/retry ownership. WorldGen and Wiring remain external ports until integration review.
5. **Anchors and placement.** Port coordinate/reach values, anchor catalogs, style/base/coordinate
   definitions, preview queries, and placement results. Then route placement through one command
   transaction and a compatibility `PlacementHookAdapter`; no preview may write the world.
6. **TileEntity registry and lifecycle.** Implement kind registration, runtime allocator, ID and
   position indexes, update scheduling, create/move/remove transactions, and deferred-removal
   semantics. Validate index consistency after every lifecycle operation.
7. **TileEntity kind components.** Migrate display/inventory, leashed anchors, sensors, pylons,
   and training-dummy state behind external Item/Player/NPC/Projectile ports. Run server-only
   update tests and network projection tests before removing compatibility reads.
8. **Signs and snapshot transaction.** Implement sign records and frame lookup owners, then
   snapshot payloads and persistence/network adapters. Restore in a transaction that rebuilds
   cell state, TileEntity indexes, signs/chests, liquid work, section dirtiness, and resync
   projections in the approved order.
9. **Client projections and paint cache.** Migrate camera/map/sign/preview/draw/color projections
   and client-only paint caches. Confirm disposal and cache invalidation do not mutate simulation
   state or persistence output.
10. **Compatibility removal and acceptance.** Remove old aliases only after static owner checks,
    focused tests, affected-project serial verification, and integration-review sign-off. Record
    the final ownership and any deferred evidence in the migration plan.

## Single-Write Owner Contract

| State or effect | Proposed single writer | Allowed readers | Explicitly forbidden |
|---|---|---|---|
| geometry and section dimensions | `WorldGeometryInitializationSystem` | all P04 queries/systems through a read port | direct Main-style writes from systems |
| catalog arrays and definitions | `CatalogInitializationSystem` / content loader | collision, framing, placement, liquid, entity queries | treating a per-cell field as catalog authority |
| material/liquid/frame/packed Tile state | `TileCellMutationSystem` | `TileCellQuery`, liquid/framing/projection systems | direct setter or packed-header writes from any other system |
| liquid propagation queue | `LiquidPropagationSystem` | mutation system and diagnostics | using a pure Query to propagate or enqueue hidden work |
| frame work and neighboring frame output | `TileFramingSystem` through mutation commands | map/render/network projections | direct neighboring Tile writes in a renderer or adapter |
| placement commit | `TilePlacementCommandSystem` then mutation owner | preview/reach queries | preview, hook delegate, or client projection mutating cells |
| TileEntity indexes and runtime IDs | `TileEntityRegistryStore` and `TileEntityLifecycleSystem` | lifecycle/query/update systems | separate `ByID`/`ByPosition` shadow registries |
| TileEntity update queue | `TileEntityUpdateSystem` | kind systems and lifecycle system | client execution of server-authoritative Update |
| kind-specific state | corresponding kind component system | kind query and projections | generic base registry owning item/player/NPC objects |
| signs | `WorldSignStore` | sign queries and projections | embedding sign text in Tile cell authority |
| snapshot buffers and restore | `TileSnapshotPersistenceAdapter` via restore command | transaction coordinator | restore code directly bypassing mutation/index owners |
| network/save/UI/render output | respective Projection or Adapter | external consumers | output code feeding values back as hidden authority |

## Field and Component Invariants

- A `TileCoordinate` is the only coordinate passed to cell commands. Bounds are checked against
  `WorldGeometryComponent` before a mutation is accepted.
- Material, wall, liquid, frame, and packed flags are exposed as a coherent read view. The
  mutation system reads the prior value, applies one command, and publishes one committed change
  with derived dirty/frame/liquid work.
- Bit constants and liquid packing constants are codec rules. They are not independently mutable
  Components and may not be rewritten by callers.
- Section membership is derived from geometry. A section dirty mark is idempotent and is emitted
  after the cell commit, never before a failed mutation.
- TileEntity creation inserts runtime ID and anchor indexes atomically. A failed insert rolls back
  every index and does not leak an external reference.
- Moving a TileEntity removes the old position before inserting the new position; duplicate
  positions and duplicate runtime IDs are rejected.
- Deferred removals are applied only after the update pass; iteration never mutates the active
  update collection.
- Display inventories own item slots as domain values or opaque external handles. They do not own
  Item system lifecycle or network identity.
- Preview, coordinate, draw, camera, map, color, and render-target values are transient or
  one-way projections. They cannot be serialized as authoritative world state.
- Snapshot restore is atomic from the caller's perspective: on failure, no partial new registry,
  liquid queue, sign, or section state is exposed.

## Compatibility and Double-Write Strategy

Compatibility is a temporary migration mechanism, not a second authority.

1. Add read-only facades around existing partial NLTX stores and components. New systems read the
   facade while old callers continue using the old API.
2. For each state family, route old write calls into the new single owner first. If a legacy
   mirror is required for an un-migrated caller, the owner emits an explicit compatibility event
   after commit; no caller writes both stores independently.
3. During a bounded shadow period, compare old serialized/projection output with the new output
   using deterministic test fixtures. Shadow comparison must not send network messages, write
   files, mutate graphics, or alter world state.
4. Keep one-way adapters for Version4-shaped APIs. Mark aliases with an expiration checkpoint and
   an owner. Do not expose packed fields as public mutable references.
5. Remove mirrors only after all readers are moved, static owner checks pass, focused tests pass,
   and integration-review approves the removal. If a mismatch is found, stop promotion and use
   the rollback procedure below.

No double-write is planned for client paint targets, camera, map, preview, or UI state. Those are
projections and caches. No double-write is planned for TileEntity IDs or snapshot restore; those
need one transaction owner from their first implementation checkpoint.

## Network, Persistence and Snapshot Migration

### Network

- Define proposed protocol DTOs separately from Components. A network DTO may carry coordinate,
  committed cell payload, sign record, TileEntity kind/anchor, or projection version, but it does
  not expose internal stores or runtime object references.
- `NetworkProjectionSystem` reads committed changes after mutation/lifecycle transactions. It
  batches dirty sections and explicit TileEntity lifecycle changes, preserving the protocol's
  ordering and retry semantics behind an Adapter.
- Runtime TileEntity IDs, persistent IDs, and network IDs remain separate. If Version4 sends its
  runtime ID, the adapter must document the session scope and reject stale IDs.
- Client network receipt submits validated commands to the same mutation and lifecycle owners;
  packet handlers never write the store directly. Server-authoritative TileEntity updates are not
  executed on clients.
- Network failure is reported as an explicit adapter result. A failed send does not roll back a
  committed simulation mutation unless the approved protocol explicitly defines a transactional
  acknowledgment; the default plan is retryable projection state.

### Persistence

- `PersistenceProjectionSystem` reads stable committed views and serializes versioned DTOs through
  a persistence Adapter. It never serializes object graphs containing active locks, graphics
  resources, delegates, streams, or external runtime references.
- Catalog definitions are referenced by stable content/type identifiers and version metadata;
  runtime registry IDs are not used as persistent identity without an integration decision.
- Signs, Tile cells, sections, TileEntities, chests, and liquid work state have explicit format
  boundaries. The missing chest and world-file owner is `crossSubsystemOwner: integration-review`.

### Snapshot and Restore

The proposed restore transaction is:

1. Acquire a restore command context and validate snapshot version, geometry, bounds, and content
   registry compatibility.
2. Decode Tile cell payloads through the proposed codecs into staging buffers.
3. Quiesce or replace liquid work, framing work, network projections, and TileEntity update
   scheduling according to an explicit server/world lifecycle port.
4. Commit cell payloads through `TileCellMutationSystem` or its approved bulk-restore command.
5. Rebuild TileEntity kind, runtime, and anchor indexes through `TileEntityLifecycleSystem`; do
   not copy old runtime IDs unless the identity policy explicitly permits it.
6. Restore signs and chest records through their owners, then rebuild section dirty state and
   liquid work from committed cells.
7. Recompute required frames/map projections and emit one explicit network resync projection on
   the server when required by the protocol.
8. Publish the restore-complete event, release temporary streams/buffers, and resume scheduling.

Any failure before publish discards staging and leaves the prior world visible. If the current
Version4 restore path has a different order, integration-review must record the compatibility
decision and add a regression test before implementation proceeds.

## Proposed System Order for Implementation

The implementation must encode dependencies rather than rely on file order:

1. `WorldGeometryInitializationSystem`
2. `CatalogInitializationSystem`
3. `TileMapAllocationSystem`
4. `TileEntityRegistryInitializationSystem`
5. `WorldSnapshotLoadSystem` or `NetworkSectionLoadSystem`
6. `TileCellMutationSystem`
7. `LiquidPropagationSystem`
8. `TileFramingSystem`
9. `TileEntityLifecycleSystem`
10. `TileEntityUpdateSystem`
11. `LogicSensorSystem`, `TrainingDummySystem`, and other kind systems
12. `TilePlacementCommandSystem` and interaction command systems
13. `NetworkProjectionSystem` and `PersistenceProjectionSystem`
14. `WorldMapProjectionSystem`, sign/preview/draw/camera projections
15. `TilePaintRenderCacheAdapter` at client render lifecycle boundaries

The order is proposed and has `crossSubsystemOwner: integration-review` until Liquid, WorldGen,
Wiring, networking, persistence, and client owners approve it. Any retry, clock, random, logging,
or UI effect must be injected through an explicit port and tested as an ordered effect.

## Rollback Procedure and Conditions

Rollback is required if any of the following occurs: a packed Tile invariant changes, a liquid or
frame neighbor result diverges, a TileEntity index is inconsistent, a snapshot restore exposes
partial state, a network/persistence identity collides, or a client projection changes simulation
state.

1. Stop promotion at the current phase and retain the last known-good compatibility facade.
2. Disable the new reader path with the phase feature flag or dependency registration switch;
   route reads back to the old facade, while preserving committed data ownership if already
   migrated.
3. If a state family has not crossed its commit boundary, discard staging buffers and pending
   commands. Do not delete broad build/source directories or reset unrelated work.
4. If a state family has crossed its commit boundary, run the owner-defined reverse adapter or
   restore the versioned persisted snapshot; never reconstruct packed bits by ad hoc field copies.
5. Reconcile runtime TileEntity indexes and clear/rebuild liquid, frame, map, and network queues
   through their owners.
6. Capture the failing fixture, source member IDs, phase, and effect trace; add a focused test and
   update the integration decision before retrying.
7. Remove compatibility mirrors only after the rollback window closes and acceptance evidence is
   recorded.

Rollback does not authorize reverting unrelated user changes or deleting existing workspace
artifacts. The current session required no rollback; the Component-only source changes compiled
successfully and no runtime migration was attempted.

## Focused Verifier and Static-Check Plan

No verifier has run. The planned gates are:

- **Inventory gate:** parse the P04 report and assert the 23 group names, 310 fields, 10
  properties, 320 members, unique source numbers, and exact source member names.
- **Mapping gate:** parse both second-round documents and assert every input source number appears
  once in each mapping matrix, with matching group and member, and all metadata fields are equal.
- **Owner gate:** static analysis must find one writer for packed Tile fields, one registry/index
  owner, one sign store, one snapshot restore boundary, and no authoritative simulation reference
  to `RenderTarget2D`.
- **Geometry/catalog gate:** bounds, section derivation, catalog array lengths, content-generation
  initialization, and cleanup are deterministic.
- **Tile gate:** clear/reset, material, wall, liquid, bit, slope, frame, dirty-section, and
  neighbor effects preserve all invariants and commit atomically.
- **Placement gate:** reach, anchors, liquid rules, styles, preview purity, hook adapter failure,
  and one-command placement are covered.
- **TileEntity gate:** create/move/remove, duplicate detection, deferred removal, update ordering,
  per-kind state, and external-reference release are covered.
- **Persistence/network gate:** versioned round trips, isolated IDs, stale packet rejection,
  restore transaction failure, resync emission, and retry behavior are covered.
- **Projection gate:** map/sign/camera/preview/draw/color projections are one-way; paint cache
  creation, invalidation, and disposal do not change simulation or save output.
- **Repository gate:** after implementation, run the affected project only from the repository
  root through `Build\Tools\Invoke-SerialDotnet.ps1`, with the selected SDK and
  `-p:UseSharedCompilation=false`, `-m:1`, `-nr:false`, `-p:MSBuildNodeReuse=false`, and
  `-p:BuildInParallel=false`. Then run the focused verifier with `--no-build --no-restore`,
  inspect warning/error counts, and verify artifacts under `Build\bin\`. No raw `dotnet`
  command, solution-wide build, or parallel compile is permitted.

The serial repository build gate was run for the affected src2 project and passed with 0 warnings
and 0 errors. No focused verifier or test was run, and the two second-round documents remain
excluded from the user-forbidden `git diff --check`.

## Risks and Evidence Gaps

- The missing P04 first-round report prevents relying on a completed public-decomposition trace;
  this plan uses the input inventory and directly inspected Version4 evidence only.
- The missing `约束\公共拆分约束.md` prevents a final public-split compliance decision.
- The existing NLTX models overlap portions of the proposed boundaries. Their actual readers and
  writers must be inspected during implementation before adding or renaming files.
- `Tile` packed headers and setters are coupled; field-by-field migration can silently change
  wire/save formats.
- Liquid propagation, framing, WorldGen, Wiring, map queues, and network dirty collections have
  effect ordering that is not closed by declarations alone.
- TileEntity per-kind fields refer to Item, Player, Projectile, NPC, and leashed-entity domains;
  their ownership, serialization, and network identity are cross-partition decisions.
- Snapshot restore includes TileEntity and Chest state plus stream resources and can emit server
  resynchronization. Partial restore is a high-severity failure.
- Client caches may hold graphics resources and must not be moved into shared simulation storage.
- The affected project is `src2\\Terraria.NonAuthoritative.Persistence.csproj` targeting `net10.0`.
  No test project was selected or run because this task forbids test changes and test execution.

## Integration Handoff

For the remaining deferred work, integration-review must:

1. approve the six capability lanes and reconcile proposed paths with current NLTX files;
2. decide final owners for geometry/catalogs, Tile mutation/codecs, liquid/framing queues,
   placement hooks, TileEntity IDs/indexes, signs, chests, snapshots, and paint projections;
3. settle all external IDs and protocol DTOs independently from coordinates and runtime IDs;
4. approve the proposed scheduler and restore order with cross-partition owners;
5. provide the missing public-split constraint or record an explicit exception;
6. select affected projects and focused verifier projects;
7. for the remaining deferred work, create C# files only after the owners and boundaries above
   are approved, then run the repository-mandated serial verification.

The companion design matrix is at
`D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\non-authoritative\2026-09-11-version4-non-authoritative-P04-world-tiles-storage-component-design.md`.

## Component Implementation Checkpoint 1

- component: `WorldGeometryComponent`
- implementationStatus: `implemented-in-source; verification pending`
- actualComponentFile: `D:\TRbackup\NLTX\src2\WorldStorage\Geometry\WorldGeometryComponent.cs`
- sourceChange: added a sealed component for world bounds, tile dimensions, and section dimensions;
  validation is limited to the component's own value invariants
- dependencyEvidence: no external project type, registration API, mutation owner, or effect port is
  required by the component
- verificationStatus: `not-run`; no compile-capable command has been run yet
- evidence-gap: Version4 declares the geometry fields, but the src2 initialization lifecycle,
  section formula authority, and compatibility readers are not closed
- blocking-decision: `crossSubsystemOwner: integration-review` must confirm the geometry producer
  before any caller migration or derived-value recalculation
- excludedThisCheckpoint: no non-Component code or test code was added or modified

## Component Implementation Checkpoint 10

- component: `TileEntityUpdateScheduleComponent`
- implementationStatus: `implemented-in-source; verification pending`
- actualComponentFile: `D:\TRbackup\NLTX\src2\WorldStorage\TileEntities\TileEntityUpdateScheduleComponent.cs`
- sourceChange: added a sealed component containing the entity-level `RequiresUpdates` state
- dependencyEvidence: Version4's global `UpdateEntities` collection is intentionally not stored in
  this component because its scheduler ownership and iteration lifecycle are unresolved
- verificationStatus: `not-run`; no compile-capable command has been run yet
- evidence-gap: update collection ownership, snapshot timing, deferred removal, and execution
  boundaries are not closed
- blocking-decision: `crossSubsystemOwner: integration-review` must approve the scheduler owner
  and deferred-removal lifecycle before the global collection is implemented
- excludedThisCheckpoint: no non-Component code or test code was added or modified

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
- sourceChange: added a sealed component with a byte `KindId` property matching the Version4
  TileEntity `type` field
- dependencyEvidence: no kind registry, registration key, per-kind behavior, or external identity
  was added
- verificationStatus: `not-run`; no compile-capable command has been run yet
- evidence-gap: kind registration and persistent/network identity separation remain unresolved
- blocking-decision: `crossSubsystemOwner: integration-review` must approve the kind catalog owner
  before lookup or registration code is introduced
- excludedThisCheckpoint: no non-Component code or test code was added or modified

## Component Implementation Checkpoint 8

- component: `TileEntityAnchorComponent`
- implementationStatus: `implemented-in-source; verification pending`
- actualComponentFile: `D:\TRbackup\NLTX\src2\WorldStorage\TileEntities\TileEntityAnchorComponent.cs`
- sourceChange: added a sealed component with signed 16-bit anchor coordinates matching the
  Version4 `Point16` coordinate width
- dependencyEvidence: no Point16 dependency, footprint definition, position index, placement
  behavior, or movement system was added
- verificationStatus: `not-run`; no compile-capable command has been run yet
- evidence-gap: coordinate identity and multi-tile footprint ownership remain unverified
- blocking-decision: `crossSubsystemOwner: integration-review` must settle the coordinate value and
  anchor-index owner before lifecycle or placement code consumes this component
- excludedThisCheckpoint: no non-Component code or test code was added or modified

## Component Implementation Checkpoint 7

- component: `TileEntityRuntimeIdentityComponent`
- implementationStatus: `implemented-in-source; verification pending`
- actualComponentFile: `D:\TRbackup\NLTX\src2\WorldStorage\TileEntities\TileEntityRuntimeIdentityComponent.cs`
- sourceChange: added a sealed component with a non-negative `RuntimeId` property
- dependencyEvidence: no allocator, index, persistent ID, network ID, or entity object reference
  was added
- verificationStatus: `not-run`; no compile-capable command has been run yet
- evidence-gap: runtime ID allocation and identity lifetimes are not closed
- blocking-decision: `crossSubsystemOwner: integration-review` must approve the runtime identity
  lifecycle and independent persistent/network identity policies
- excludedThisCheckpoint: no non-Component code or test code was added or modified

## Component Implementation Checkpoint 6

- component: `TileEntityCapacityPolicyComponent`
- implementationStatus: `implemented-in-source; verification pending`
- actualComponentFile: `D:\TRbackup\NLTX\src2\WorldStorage\TileEntities\TileEntityCapacityPolicyComponent.cs`
- sourceChange: added a sealed component containing a positive `MaxEntitiesPerChunk` value
- dependencyEvidence: no chunk store, allocator, synchronization boundary, registry, or lifecycle
  code was added
- verificationStatus: `not-run`; no compile-capable command has been run yet
- evidence-gap: the source constant is confirmed, but capacity scope and overflow semantics remain
  unresolved
- blocking-decision: `crossSubsystemOwner: integration-review` must approve the consumer and
  overflow policy before registry implementation
- excludedThisCheckpoint: no non-Component code or test code was added or modified

## Component Implementation Checkpoint 5

- component: `TileCellFrameBitComponent`
- implementationStatus: `implemented-in-source; verification pending`
- actualComponentFile: `D:\TRbackup\NLTX\src2\WorldStorage\Tiles\TileCellFrameBitComponent.cs`
- sourceChange: added a sealed component with `ushort`, `byte`, and `short` properties matching
  the Version4 packed-header and frame-coordinate storage fields
- dependencyEvidence: no bit codec, slope query, framing system, mutation system, or persistence
  adapter was added
- verificationStatus: `not-run`; no compile-capable command has been run yet
- evidence-gap: field widths are confirmed, but packed semantics and the single writer are not
  proven in src2
- blocking-decision: `crossSubsystemOwner: integration-review` must approve the codec and mutation
  owner before any update surface is exposed
- excludedThisCheckpoint: no non-Component code or test code was added or modified

## Component Implementation Checkpoint 4

- component: `TileCellLiquidComponent`
- implementationStatus: `implemented-in-source; verification pending`
- actualComponentFile: `D:\TRbackup\NLTX\src2\WorldStorage\Tiles\TileCellLiquidComponent.cs`
- sourceChange: added a sealed component with liquid type code and amount properties; liquid type
  is constrained to the four Version4 values
- dependencyEvidence: no propagation queue, header codec, framing operation, or external liquid
  system is implemented or referenced
- verificationStatus: `not-run`; no compile-capable command has been run yet
- evidence-gap: amount and kind are inventoried separately in Version4, but the src2 packed-bit
  codec and canonical mutation owner are not proven
- blocking-decision: `crossSubsystemOwner: integration-review` must settle liquid encoding and
  mutation ownership before legacy writes are adapted
- excludedThisCheckpoint: no non-Component code or test code was added or modified

## Component Implementation Checkpoint 3

- component: `TileCellMaterialComponent`
- implementationStatus: `implemented-in-source; verification pending`
- actualComponentFile: `D:\TRbackup\NLTX\src2\WorldStorage\Tiles\TileCellMaterialComponent.cs`
- sourceChange: added a sealed component with `ushort` tile and wall material properties matching
  the Version4 field types
- dependencyEvidence: active state, liquid, frame, and packed-header semantics are intentionally
  not duplicated in this component
- verificationStatus: `not-run`; no compile-capable command has been run yet
- evidence-gap: the material fields are inventoried, but the canonical write owner and active-bit
  coupling are not proven in src2
- blocking-decision: `crossSubsystemOwner: integration-review` must approve the mutation owner
  before legacy writes can be adapted
- excludedThisCheckpoint: no non-Component code or test code was added or modified

## Component Implementation Checkpoint 2

- component: `WorldCapacityLimitsComponent`
- implementationStatus: `implemented-in-source; verification pending`
- actualComponentFile: `D:\TRbackup\NLTX\src2\WorldStorage\Geometry\WorldCapacityLimitsComponent.cs`
- sourceChange: added a sealed component for positive maximum network-player and NPC capacities;
  no allocator, registry, or external NPC object is stored
- dependencyEvidence: the component has no dependency outside BCL primitives
- verificationStatus: `not-run`; no compile-capable command has been run yet
- evidence-gap: the Version4 capacity fields are confirmed, but src2 allocation semantics and the
  NPC sentinel-slot convention are not closed
- blocking-decision: `crossSubsystemOwner: integration-review` must settle the capacity convention
  before an external storage owner consumes these values
- excludedThisCheckpoint: no non-Component code or test code was added or modified
