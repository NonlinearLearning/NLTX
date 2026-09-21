
# P18 map, camera, lighting, and drawing proposed component design

partitionId: P18
sessionId: 8c92202ba8c44e9c8bf9ceb7ab7928f7
inputReport: D:\TRbackup\NLTX\docs\migration\ledgers\non-authoritative-component-partitions\18-map-camera-rendering.md
componentDesignPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\non-authoritative\2026-09-11-version4-non-authoritative-P18-map-camera-rendering-component-design.md
componentExecutionPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\non-authoritative\2026-09-11-version4-non-authoritative-P18-map-camera-rendering-component-execution.md
handoffId: P18-session-rebind-20260912
previousSessionId: 5b200601f3fa45fa9deba0555a93acf5
previousDocumentSessionId: f1bd5d48d87c427d86104571ac6c167d
handoffReason: runner Handoff transferred the failed manual P18 claim to the new session; component checkpoints and verification claims were preserved.
runnerSettlementStatus: completed
runnerSettlementSessionId: 8c92202ba8c44e9c8bf9ceb7ab7928f7
runnerSettlementExitCode: 0
runnerSettlementLockReleased: true
designStatus: proposed
executionStatus: planned
implementationStatus: completed
verificationStatus: independently-verified
completedComponents: [C01.CameraUiTransformStateComponent, C02.SceneScanInputComponent, C03.SceneMetricsAggregateComponent, C04.SceneVisualProjectionComponent, C06.LightingCoordinatorComponent, C07.LightMapCacheComponent, C07.LegacyLightingCacheComponent, C08.SpriteAnimationComponent, C08.TileAnimationRegistryComponent, C09.DrawCommandWorksetComponent, C09.SpriteBatchStateComponent, C10.WorldDrawingProjectionComponent, C10.VertexStripWorksetComponent, C11.ShaderParameterComponent, C11.ShaderFamilyParameterComponent, C12.ShaderRegistryComponent, C13.WorldMapSnapshotComponent, C13.MapTileSnapshotComponent, C14.MapOverlayComponent, C14.MapInvalidationQueueComponent, C16.MapEncodingCatalogComponent]; source saved and independently verified
currentComponent: none; Component subset complete after C01 component-level input invariant validation
pendingComponents: C05.CaptureAndCameraSupportAdapter (deferred; adapter-only); C12.ShaderRegistryAdapter and non-Component roles (excluded); C15.MapPersistenceAdapter (deferred; adapter/projection-only)
lastCheckpointUtc: 2026-09-12T08:35:04.5086298Z
evidence-gap: missing first-round P18 research report; Version4 partial/stubbed bodies; current NLTX P18 implementation match absent; cross-partition readers, writers, IDs, persistence, network, and scheduler ownership unresolved
blocking-decision: integration-review must select final owners for world tiles, entity and weather readers, player/UI camera inputs, map persistence, network map snapshots, shader assets, particle/audio handoff, and render ordering

implementationCheckpoint: Component source for C01-C04, C06-C14, and C16 is saved under src2; C01 component invariant validation was updated after handoff; serial build and focused verifier passed.
deferredNonComponentWork: C05 adapter/projection work, C12 registry adapter/query/system work, and C15 persistence adapter/projection work remain excluded from this Component-only session.
implementationFiles: proposed src2/ClientPresentation/MapCameraRendering/CameraUiTransformInput.cs; proposed src2/ClientPresentation/MapCameraRendering/CameraUiTransformStateComponent.cs; proposed src2/ClientPresentation/MapCameraRendering/CameraUiTransformSystem.cs; proposed src2/ClientPresentation/MapCameraRendering/CameraTransformSnapshot.cs; proposed src2/ClientPresentation/MapCameraRendering/CameraTransformQuery.cs; proposed src2/ClientPresentation/MapCameraRendering/UiScaleProjection.cs; proposed src2/ClientPresentation/MapCameraRendering/MapTileSnapshotValue.cs; proposed src2/ClientPresentation/MapCameraRendering/MapTileSnapshotComponent.cs; proposed src2/ClientPresentation/MapCameraRendering/WorldMapSnapshotComponent.cs; proposed src2/ClientPresentation/MapCameraRendering/WorldMapQuery.cs; proposed src2/ClientPresentation/MapCameraRendering/MapTileSnapshotSystem.cs; proposed src2/ClientPresentation/MapCameraRendering/MapEncodingCatalogComponent.cs; proposed src2/ClientPresentation/MapCameraRendering/MapEncodingCatalogQuery.cs; proposed src2/ClientPresentation/MapCameraRendering/MapCodecAdapter.cs; proposed src2/ClientPresentation/MapCameraRendering/SceneAudioProjectionQuery.cs; proposed src2/ClientPresentation/MapCameraRendering/LightingMode.cs; proposed src2/ClientPresentation/MapCameraRendering/LightingFrameSnapshot.cs; proposed src2/ClientPresentation/MapCameraRendering/LightingSample.cs; proposed src2/ClientPresentation/MapCameraRendering/LightingCoordinatorComponent.cs; proposed src2/ClientPresentation/MapCameraRendering/LightingModeSystem.cs; proposed src2/ClientPresentation/MapCameraRendering/LightingFrameSwapSystem.cs; proposed src2/ClientPresentation/MapCameraRendering/LightingQuery.cs; proposed src2/ClientPresentation/MapCameraRendering/IRandomSampleSource.cs; proposed src2/ClientPresentation/MapCameraRendering/LightMapCellInput.cs; proposed src2/ClientPresentation/MapCameraRendering/LightMapSample.cs; proposed src2/ClientPresentation/MapCameraRendering/LightMapCacheComponent.cs; proposed src2/ClientPresentation/MapCameraRendering/LegacyLightingCacheComponent.cs; proposed src2/ClientPresentation/MapCameraRendering/LightMapQuery.cs; proposed src2/ClientPresentation/MapCameraRendering/LegacyLightingProjection.cs; proposed src2/ClientPresentation/MapCameraRendering/TileLightScannerQuery.cs; proposed src2/ClientPresentation/MapCameraRendering/LightMapScanSystem.cs
implementationEvidence: post-handoff C01 revision passed the affected-library build and existing P18 verifier through the serial wrapper; no behavior-equivalence, network-closure, persistence-closure, or cross-partition owner claim is made.
verificationCommands: build src2/ClientPresentation/MapCameraRendering/Terraria.MapCameraRendering.csproj exit=0 warnings=0 errors=0 artifact=Build/bin/Terraria.MapCameraRendering/Debug/net10.0/Terraria.MapCameraRendering.dll; build src2/ClientPresentation/MapCameraRenderingVerification/Terraria.MapCameraRenderingVerification.csproj --no-restore exit=0 warnings=0 errors=0 artifact=Build/bin/Terraria.MapCameraRenderingVerification/Debug/net10.0/Terraria.MapCameraRenderingVerification.dll; run verifier --no-build --no-restore exit=0 output=P18 verifier passed.
implementationGap: C05/C15 adapter-only work and C12 non-Component roles remain deferred/excluded; persistence and graphics adapter behavior remains local to src2 and is not behavior-equivalence evidence.
implementationFilesAdditional2: proposed src2/ClientPresentation/MapCameraRendering/ShaderValueKind.cs; proposed src2/ClientPresentation/MapCameraRendering/ShaderParameterValue.cs; proposed src2/ClientPresentation/MapCameraRendering/ShaderHandle.cs; proposed src2/ClientPresentation/MapCameraRendering/ShaderDefinition.cs; proposed src2/ClientPresentation/MapCameraRendering/ShaderRegistryComponent.cs; proposed src2/ClientPresentation/MapCameraRendering/ShaderRegistryBuildSystem.cs; proposed src2/ClientPresentation/MapCameraRendering/ShaderLookupQuery.cs; proposed src2/ClientPresentation/MapCameraRendering/ShaderParameterComponent.cs; proposed src2/ClientPresentation/MapCameraRendering/ShaderFamilyParameterComponent.cs; proposed src2/ClientPresentation/MapCameraRendering/ShaderParameterSystem.cs; proposed src2/ClientPresentation/MapCameraRendering/IShaderAssetSource.cs; proposed src2/ClientPresentation/MapCameraRendering/ShaderAssetAdapter.cs; proposed src2/ClientPresentation/MapCameraRendering/IShaderParameterSink.cs; proposed src2/ClientPresentation/MapCameraRendering/ShaderApplyResult.cs; proposed src2/ClientPresentation/MapCameraRendering/ShaderApplyAdapter.cs; proposed src2/ClientPresentation/MapCameraRendering/MapInvalidationCommand.cs; proposed src2/ClientPresentation/MapCameraRendering/MapInvalidationQueueComponent.cs; proposed src2/ClientPresentation/MapCameraRendering/MapInvalidationQueueSystem.cs; proposed src2/ClientPresentation/MapCameraRendering/MapClockSnapshot.cs; proposed src2/ClientPresentation/MapCameraRendering/IMapClockSource.cs; proposed src2/ClientPresentation/MapCameraRendering/MapClockQuery.cs; proposed src2/ClientPresentation/MapCameraRendering/MapPingProjection.cs; proposed src2/ClientPresentation/MapCameraRendering/MapPylonProjection.cs; proposed src2/ClientPresentation/MapCameraRendering/MapOverlayComponent.cs; proposed src2/ClientPresentation/MapCameraRendering/MapOverlaySystem.cs; proposed src2/ClientPresentation/MapCameraRendering/MapPersistenceFailureKind.cs; proposed src2/ClientPresentation/MapCameraRendering/MapPersistenceSnapshot.cs; proposed src2/ClientPresentation/MapCameraRendering/MapPersistenceResult.cs; proposed src2/ClientPresentation/MapCameraRendering/MapLoadResult.cs; proposed src2/ClientPresentation/MapCameraRendering/MapSaveProjection.cs; proposed src2/ClientPresentation/MapCameraRendering/MapPersistenceAdapter.cs
implementationFilesAdditional: proposed src2/ClientPresentation/MapCameraRendering/SpriteFrameSnapshot.cs; proposed src2/ClientPresentation/MapCameraRendering/SpriteAnimationComponent.cs; proposed src2/ClientPresentation/MapCameraRendering/AnimationAdvanceSystem.cs; proposed src2/ClientPresentation/MapCameraRendering/TileAnimationKey.cs; proposed src2/ClientPresentation/MapCameraRendering/TileAnimationDefinition.cs; proposed src2/ClientPresentation/MapCameraRendering/TileAnimationRegistryComponent.cs; proposed src2/ClientPresentation/MapCameraRendering/AnimationRegistrySystem.cs; proposed src2/ClientPresentation/MapCameraRendering/DrawCommand.cs; proposed src2/ClientPresentation/MapCameraRendering/DrawCommandInput.cs; proposed src2/ClientPresentation/MapCameraRendering/DrawCommandWorksetComponent.cs; proposed src2/ClientPresentation/MapCameraRendering/DrawCommandBuildSystem.cs; proposed src2/ClientPresentation/MapCameraRendering/SpriteBatchStateInput.cs; proposed src2/ClientPresentation/MapCameraRendering/SpriteBatchStateComponent.cs; proposed src2/ClientPresentation/MapCameraRendering/ISpriteBatchSink.cs; proposed src2/ClientPresentation/MapCameraRendering/SpriteBatchSubmissionResult.cs; proposed src2/ClientPresentation/MapCameraRendering/SpriteBatchSubmitAdapter.cs; proposed src2/ClientPresentation/MapCameraRendering/WorldDrawingInput.cs; proposed src2/ClientPresentation/MapCameraRendering/EntityShadowSnapshot.cs; proposed src2/ClientPresentation/MapCameraRendering/WorldDrawingProjectionComponent.cs; proposed src2/ClientPresentation/MapCameraRendering/WorldDrawingProjectionSystem.cs; proposed src2/ClientPresentation/MapCameraRendering/VertexStripPoint.cs; proposed src2/ClientPresentation/MapCameraRendering/VertexStripWorksetComponent.cs; proposed src2/ClientPresentation/MapCameraRendering/VertexStripBuildSystem.cs; proposed src2/ClientPresentation/MapCameraRendering/IVertexSink.cs; proposed src2/ClientPresentation/MapCameraRendering/VertexSubmitAdapter.cs

This is a non-authoritative second-round design proposal. It is a design and evidence handoff only.
The 29 leaf subsystems and 331 source members are inventory inputs. No proposed component, system,
query, command, adapter, projection, interface, path, file, migration, compatibility closure,
behavior-equivalence claim, API closure, network closure, persistence closure, or verification
result is being claimed.

## 1. Scope and exclusions

In scope:

- client camera, UI scale, visual offset, viewport, matrix, and map projection state;
- scene scan inputs, geometry thresholds, aggregate metrics, scene visual projections, and their
  explicit invalidation/lifecycle boundaries;
- new and legacy lighting coordination, light-map caches, tile-light query state, and frame
  swapping;
- animation/frame state, transient draw commands, batch submission state, vertex strips, entity
  shadow snapshots, horizon/particle drawing payloads, and explicit render scheduling;
- shader parameters, shader family state, registry lookup, asset/graphics adapters, and missing
  resource behavior;
- world-map tile snapshots, map update queues, overlays, pings, map I/O, and encoding catalogs.

Excluded:

- authoritative world tile, wall, liquid, weather, NPC, player, projectile, entity, and item
  simulation state;
- final ownership of camera input, entity identity, player scene perspective, network protocol,
  world-file persistence, external asset loading, audio, particles, and UI command handling;
- any current NLTX implementation claim. Adjacent current NLTX files such as TileMapStore,
  TileMapLayout, TileCellState, TileLightingDefinition, TileDefinition, and
  ContentPresentationIndex are existing foundations observed during research, not P18 migration
  outputs and not evidence that the proposed design exists;
- any first-round P18 output report. The first-round report named by the earlier prompt is absent.

Every excluded shared boundary is proposed only as crossSubsystemOwner: integration-review.

## 2. Evidence and status

The input inventory is:
docs\migration\ledgers\non-authoritative-component-partitions\18-map-camera-rendering.md
Source SHA-256: b944441d92d5103f218b766a3e354200794528a19e65a862e44c2b40a36f1196.
It contains 29 leaf subsystems, 274 fields, 57 properties, and 331 records. The source sequence
number is an inventory key only; it never defines runtime order.

| Evidence | Observation | Status |
|---|---|---|
| Version4 Terraria/Main.cs:124-138, 230-295 | GameViewMatrix, UI scale, camera instance, lerp counters, visual offsets, CurrentPan, and BlackFadeIn are declared. | confirmed declaration; readers and writers are partial |
| Version4 Terraria/Main.cs:640-646, 11640-11653 | Player/camera SceneMetrics, separate-camera selection, camera pan/lerp update, and SceneState update are visible. | confirmed call-site slice; final scheduling is partial |
| Version4 Terraria/Main.cs:12265-12318 | Scene scan input, capture guard, drone tracking, Scan call, black-fade teleport, lighting clear, and transition paths are visible. | partial; CaptureManager and some bodies are stubbed |
| Version4 Terraria/SceneMetrics.cs:196-224, 231-317 | Scan invalidation uses update count or center; reset, tile/NPC scans, aggregate calculation, zone calculation, and player effects are present in the slice. | partial; cross-domain readers and effect ownership need closure |
| Version4 Terraria/SceneMetricsScanSettings.cs and SceneState.cs | Scan settings and visual decay/weather transition fields are declarations; SceneState calls MapHelper.CaptureSceneState. | confirmed declaration; sound and transition side effects partial |
| Version4 Terraria.Graphics.Light/LightingEngine.cs, LegacyLighting.cs, LightMap.cs, TileLightScanner.cs | New/legacy engines, active/working maps, scanners, temporary lights, and light-map fields are present. | partial; many Version4 methods are stubbed |
| Version4 Terraria/Lighting.cs:29-63, 92-129 | Lighting mode selection/reset and active color/brightness reads are visible. | confirmed for shown paths; server boundary requires integration |
| Version4 Terraria.DataStructures/DrawAnimation*.cs, SpriteFrame.cs | Frame counters and sprite frame coordinates are declared. | confirmed declaration; update callers are partial |
| Version4 Terraria.DataStructures/DrawData.cs and SpriteBatchBeginner.cs | Draw payload and SpriteBatch state are transient presentation structures. | confirmed declaration; submission call chain partial |
| Version4 Terraria.DataStructures/EntityShadowInfo.cs and GameContent.Drawing files | Shadow snapshot, horizon, particle settings, and draw logging fields are declared. | partial; TimeLogger and audio/particle handoff unresolved |
| Version4 Terraria.Graphics.Shaders files | Shader parameter families, effect handles, image assets, registries, and item/hair lookup dictionaries exist. | partial; asset lifetime and registration order need closure |
| Version4 Terraria.Graphics/Camera.cs and SpriteViewMatrix.cs | Camera position/size and lazy matrix cache fields/properties exist. | partial; Rebuild and ShouldRebuild are stubbed in Version4 |
| Version4 Terraria.Graphics/VertexColors.cs and VertexStrip.cs | Vertex colors, custom vertex data, declarations, indices, and temporary geometry caches exist. | confirmed declaration; graphics-device ownership partial |
| Version4 Terraria.Map/MapHelper.cs:89-120 | Scene area/snowiness derivation, map save guard, cloud/map-enabled guard, Monitor.TryEnter, file I/O, logging, and release paths are visible. | partial; persistence protocol belongs to P14 integration review |
| Version4 Terraria.Map/WorldMap.cs, MapTile.cs, MapUpdateQueue.cs | WorldMap dimensions/tiles, packed MapTile flags/color, and area update queue declarations exist; construction/update bodies are stubbed in this slice. | partial |
| Version4 Terraria.Map/MapOverlayDrawContext.cs, PingMapLayer.cs, TeleportPylonsMapLayer.cs | Overlay transform state, ping time/duration, ping storage, and pylon border data are declared. Ping uses DateTime.Now. | partial; clock is an explicit adapter boundary |
| tModLoader v2026.07 class_camera.html, class_world_map.html, class_map_helper.html | Public camera and map APIs cross-check position/size, tile updates, section/update consumption, and save/reset boundaries. | supplemental API cross-check; not private Version4 proof |
| tModLoader v2026.07 class_scene_metrics.html, class_lighting.html, class_light_map.html | Public docs cross-check scan/threshold concepts and that client lighting is not server-synchronized gameplay state. | supplemental API cross-check |
| tModLoader v2026.07 class_shader_data.html, struct_draw_data.html | Public docs cross-check shader base/apply boundary and transient draw payload shape. | supplemental API cross-check |
| SS14 client camera/mapping/render files | Structural comparison supports separating client presentation, map source, overlay, and rendering systems. | structural reference only; no semantics copied |
| Complete-reference source D:\TRbackup\无任何删减通过编译 | Richer camera/rendering/map call chains exist for files also present in Version4. | supplemental/partial only; not a Version4 fact |
| Current NLTX src/Test/dome/src search | No matching P18 camera/map/lighting/shader/presentation implementation was found. | absence-of-match only; not a capability claim |
| Missing source material | The first-round P18 research report and the old referenced 约束\公共拆分约束.md are absent. | missing evidence |

Evidence handling rule: confirmed means only the listed declaration or call slice was read; partial
means behavior or ownership is not closed; supplemental never overrides Version4; missing means the
proposal must carry an explicit evidence gap.

## 3. Proposed capability units

The following 16 units are proposed boundaries, not existing types or files.

| ID | Proposed role | Main input leaves | Proposed state/behavior boundary |
|---|---|---|---|
| C01 | proposed CameraUiTransformState | MainCameraAndUiScaleState; MainCameraAndVisualOffsets; CameraMatrixAndViewportState | proposed camera/UI transform state, pan/lerp, offset tables, viewport and matrix cache |
| C02 | proposed SceneScanInputAndGeometry | SharedSceneScanSettings; SharedSceneZoneGeometryAndThresholdState | proposed scan window, perspective, geometry thresholds, and pure region qualification |
| C03 | proposed SceneMetricsAggregateCache | MainSceneMetricsState; SharedSceneScanAccumulatorState; SharedSceneTileAggregateState | proposed scan accumulator and derived tile/liquid/entity aggregate cache |
| C04 | proposed SceneVisualProjection | SharedSceneVisualState | proposed visual decay, weather intensity, transition flags, and sound-token projection |
| C05 | proposed CaptureAndCameraSupportAdapter | SharedCaptureAndCameraSupport | proposed capture query, drone-camera tracking adapter, and drill debug draw projection |
| C06 | proposed LightingCoordinatorState | SharedLightingCoordinatorState | proposed mode, active engine selection, frame-light inputs, active/working map swap |
| C07 | proposed LightMapAndLegacyLightingCache | LightMapCacheState; TileLightScannerState; SharedLegacyLightingState | proposed legacy scan/cache state and light-map buffers; never authoritative gameplay state |
| C08 | proposed SpriteAnimationAndFrameState | DrawAnimationAndFrameState | proposed frame counters, tile animation registry, deferred add/remove worksets |
| C09 | proposed DrawCommandBatchWorkset | DrawCommandAndBatchState | proposed transient draw command and SpriteBatch submission workset |
| C10 | proposed WorldDrawingProjection | WorldDrawingAuxiliaryState; EntityShadowPresentationState; VertexStripAndColorState | proposed horizon/particle/tile draw projection, entity shadow snapshot, vertex scratch data |
| C11 | proposed ShaderParameterState | SharedShaderBaseParameterState; SharedShaderFamilyDataState | proposed CPU-side shader parameter cache and family-specific binding state |
| C12 | proposed ShaderRegistryAdapter | SharedShaderRegistryAndLookupState | proposed registry, lookup, asset registration, and shader-handle adapter |
| C13 | proposed WorldMapAndTileSnapshot | WorldMapState; MapTileCellState | proposed client map dimensions and tile visual snapshot, with world tile source external |
| C14 | proposed MapOverlayAndUpdateQueue | MapOverlayAndLayerPresentation; MapTileUpdateQueueState | proposed map transform/overlay state, pings/pylons, and queued area invalidation |
| C15 | proposed MapPersistenceAdapter | MapIoRuntimeState | proposed file/cloud/lock/zlib adapter state; no persistence I/O in ECS systems |
| C16 | proposed MapEncodingCatalog | MapEncodingHeaderBitCatalogState; MapEncodingOptionLimitState | proposed immutable encoding constants and bounded decode/encode options |

## 4. Leaf-to-unit assignment and boundaries

The following table assigns every leaf to a proposed role. The exact 331-record ledger is included
in section 8 and in the paired proposed execution plan.

| Leaf subsystem | Member count | Source sequence ranges | Proposed unit | Authority classification |
|---|---:|---|---|---|
| MainCameraAndUiScaleState | 8 | 10-17 | proposed C01 | client presentation state |
| MainCameraAndVisualOffsets | 8 | 61-68 | proposed C01 | client presentation state |
| MainSceneMetricsState | 4 | 236-239 | proposed C03 | derived/cache holder |
| SharedSceneScanSettings | 4 | 3475-3478 | proposed C02 | query input |
| SharedSceneVisualState | 6 | 3479-3484 | proposed C04 | client visual projection |
| SharedCaptureAndCameraSupport | 6 | 1115-1118, 2633, 3872 | proposed C05 | external/presentation adapter |
| LightMapCacheState | 13 | 2673-2677, 3876-3883 | proposed C07 | render cache |
| TileLightScannerState | 1 | 2678 | proposed C07 | deterministic query helper with injected randomness |
| DrawAnimationAndFrameState | 20 | 1096-1101, 1293-1298, 2991-2996, 3700-3701 | proposed C08 | transient presentation state |
| DrawCommandAndBatchState | 20 | 1102-1114, 1286-1292 | proposed C09 | transient command/workset |
| WorldDrawingAuxiliaryState | 10 | 1526-1535 | proposed C10 | presentation/projection |
| EntityShadowPresentationState | 7 | 1119-1124, 3688 | proposed C10 | entity-derived snapshot |
| SharedShaderBaseParameterState | 21 | 2713-2730, 3887-3889 | proposed C11 | client render cache and external handle boundary |
| SharedLightingCoordinatorState | 19 | 2664-2672, 3306-3312, 3972-3974 | proposed C06 | client lighting coordinator |
| SharedLegacyLightingState | 20 | 2645-2663, 3875 | proposed C07 | legacy render cache |
| SharedSceneScanAccumulatorState | 13 | 3433, 3468-3474, 3979-3982, 4022 | proposed C03 | derived scan accumulator |
| SharedSceneTileAggregateState | 19 | 3983-3986, 3989-3999, 4017-4019, 4021 | proposed C03 | derived aggregate cache |
| SharedShaderFamilyDataState | 20 | 2685-2688, 2695-2701, 2705-2712, 3886 | proposed C11 | family-specific render parameters |
| SharedShaderRegistryAndLookupState | 9 | 2689-2694, 2702-2704 | proposed C12 | registry/lookup adapter state |
| SharedSceneZoneGeometryAndThresholdState | 12 | 3429-3432, 3434-3441 | proposed C02 | query geometry and thresholds |
| MapOverlayAndLayerPresentation | 12 | 4250-4255, 4262-4267 | proposed C14 | client overlay state |
| WorldMapState | 5 | 4268-4271, 4535 | proposed C13 | map snapshot/cache |
| MapTileUpdateQueueState | 3 | 4259-4261 | proposed C14 | explicit invalidation workset |
| CameraMatrixAndViewportState | 16 | 4194-4201, 4523-4530 | proposed C01 | client transform cache |
| VertexStripAndColorState | 13 | 4202-4213, 4531 | proposed C10 | graphics scratch/projection |
| MapTileCellState | 6 | 4256-4258, 4532-4534 | proposed C13 | visual tile snapshot |
| MapIoRuntimeState | 4 | 4245-4247, 4249 | proposed C15 | file/compression adapter runtime |
| MapEncodingHeaderBitCatalogState | 24 | 4215-4238 | proposed C16 | immutable compatibility catalog |
| MapEncodingOptionLimitState | 8 | 4214, 4239-4244, 4248 | proposed C16 | bounded adapter options |

Boundary rule: a leaf is not itself a component. A proposed component holds only cohesive state;
a proposed system owns transitions; a proposed query is read-only; a proposed command is the
submission boundary; a proposed adapter owns external effects; a proposed projection emits client,
network, or persistence output.

## 5. Proposed contracts, fields, invariants, and ownership

### 5.1 Proposed C01 CameraUiTransformState

Proposed state includes UI scale wanted/used, camera transform references, pan and lerp progress,
visual offset tables, viewport position/size, zoom/translation and cached matrices. It must keep
unscaled and scaled coordinates distinguishable, reject non-finite scale or zoom, and rebuild
derived matrices only after an explicit invalidation. proposed CameraUiTransformSystem reads
player/input/capture camera intents and writes the proposed C01 state. proposed
CameraMatrixQuery is pure. proposed UiScaleProjection emits UI-space transforms. GraphicsDevice
ownership is an adapter concern. Camera state is not authoritative world position.

Readers: proposed scene-scan input query, proposed render scheduling system, proposed map projection
system, and UI projection. Writers: the single proposed CameraUiTransformSystem. Initialization
creates a neutral transform; frame update applies input and camera commands; cleanup releases only
external matrix/device handles through the proposed graphics adapter.

### 5.2 Proposed C02 SceneScanInputAndGeometry

Proposed fields preserve scan rectangle, biome center, NPC-scan option, perspective token, fixed
screen size, padding, scan size, town-NPC rectangle, thresholds, and height-zone booleans.
Invariants: scan rectangles are bounded and normalized; thresholds are explicit configuration;
perspective player/entity references are read-only snapshots; no query changes world state.
proposed SceneScanInputSystem builds inputs from camera/UI and integration-owned world geometry.
proposed SceneZoneQuery calculates qualification without writing. Readers are proposed C03 and
proposed C04; writers are the proposed scan-input system only.

Tile, liquid, weather, NPC, and player readers are crossSubsystemOwner: integration-review.

### 5.3 Proposed C03 SceneMetricsAggregateCache

Proposed C03 holds scan timestamp/revision, world center/tile center, best ore data, banner and
closest-NPC derived data, tile/liquid count arrays, perspective snapshot, and named aggregate
counts. Invariants: a scan either fully replaces a revision or is discarded; aggregate counters
reset before accumulation; values are tagged with source world revision and scan center; no
render projection writes them. proposed SceneMetricsScanSystem reads proposed C02 plus immutable
world/entity/liquid/weather snapshots and writes C03. proposed SceneMetricsQuery reads C03 only.
Player effects currently visible in SceneMetrics require a proposed command to the integration owner,
not a hidden write from the query.

C03 is derived/cache state, not gameplay authority. Readers include proposed scene visual,
lighting, UI, and world-event projections; writers are the proposed scan system.

### 5.4 Proposed C04 SceneVisualProjection

Proposed C04 stores light decay, outside-weather visual intensity, transition suppression, and
sound-slot tokens. Sound token use is an outbound projection. The proposed SceneVisualSystem reads
C03 and weather inputs, writes only C04, and sends proposed audio projection commands through an
adapter. It must not own weather authority or mutate a sound service from a query. A failed audio
submission is isolated from scene aggregate state and is retryable only at the adapter boundary.

### 5.5 Proposed C05 CaptureAndCameraSupportAdapter

Proposed C05 separates capture state, drone tracked-projectile reference/type, and drill debug
draw payload. CaptureManager.IsCapturing is partial in Version4; proposed CaptureQuery returns an
explicit unknown/disabled result until the external capture provider closes the contract. The
proposed drone adapter reads an integration-owned projectile snapshot, never a mutable projectile
object. Drill debug output is a client projection. Capture and debug failure must not mutate camera
or world authority.

### 5.6 Proposed C06 LightingCoordinatorState

Proposed C06 owns lighting mode, global brightness, off-screen tile budget, active engine handle,
per-frame light input lists, active and working map references, and coordinator constants. Invariant:
only one active map is published for a frame; working data cannot be read by render submission until
the proposed LightingFrameSwapSystem commits it; mode changes invalidate both maps and scanner
work. proposed LightingModeSystem selects new/legacy mode; proposed LightingQuery reads active
light color/brightness. Lighting is client presentation and must never be used as synchronized
gameplay truth.

### 5.7 Proposed C07 LightMapAndLegacyLightingCache

Proposed C07 contains LightMap dimensions/decay and color/mask buffers, legacy jagged lighting cells,
temporary light flags, scanner/random state, and legacy mode. Buffers are bounded by explicit width
and height; array index validation is mandatory; random state is injected for deterministic tests;
clear/reset is idempotent. proposed LightMapScanSystem reads tile/light snapshots and writes working
cache. proposed LegacyLightingProjection provides read-only colors. New/legacy internals stay
separate even though both are in C07; they are not a single authoritative lighting component.

### 5.8 Proposed C08 SpriteAnimationAndFrameState

Proposed C08 owns frame counters, frame count, ticks per frame, ping-pong/not-actually-animating
flags, sprite padding/current row and column, and tile animation registries plus deferred add/remove
lists. Invariants: frame count and tick rate are positive or an explicit disabled value; deferred
mutation is applied at a single commit point; animation coordinates are value snapshots. proposed
AnimationAdvanceSystem advances counters from a supplied tick delta. proposed AnimationRegistrySystem
commits queued additions/removals. No entity or tile authority is changed by frame advancement.

### 5.9 Proposed C09 DrawCommandBatchWorkset

Proposed C09 is transient: texture/resource token, position, destination/source rectangle, color,
rotation, origin, scale, effects, shader index, rotation flag, destination mode, null rectangle,
and SpriteBatch state. It is cleared after submission or failure. Invariants: a command cannot
escape its frame lease; shader index is a handle validated by proposed C12; external graphics
objects are opaque adapter tokens; batch begin/end ownership is single-writer. proposed
DrawCommandBuildSystem creates commands; proposed SpriteBatchSubmitAdapter performs effectful
submission; render submission cannot write tile, entity, player, or map authority.

### 5.10 Proposed C10 WorldDrawingProjection

Proposed C10 contains horizon blend/color data, draw-data list, particle orchestration payload,
shadow snapshot, vertex colors/custom vertices/index buffers, and temporary position/rotation caches.
It is a projection/workset, not simulation. Entity shadow data is keyed by an integration-owned
entity snapshot; it does not own body frame or entity rotation. proposed HorizonAndTileProjection,
proposed EntityShadowProjection, and proposed VertexStripBuildSystem write separate worksets inside
the proposed capability boundary. proposed RenderSubmitSystem consumes them in explicit order.
Time logging, particles, audio, and GraphicsDevice are adapters or projections.

### 5.11 Proposed C11 ShaderParameterState

Proposed C11 separates scalar/vector parameters, image asset tokens, custom texture tokens, sampler
state tokens, image scales, effect parameter wrappers, cached parameter values, shader asset/pass
tokens, family-specific armor/hair/misc values, projection-matrix selection, shader-specific data,
and disabled state. Invariants: parameter writes are staged until Apply; missing assets produce an
explicit bind failure or deterministic fallback; no shader object crosses the core state boundary.
proposed ShaderParameterSystem stages values; proposed ShaderApplyAdapter applies them to a graphics
effect. Misc image loading is an external asset operation and must be skipped or reported on a
dedicated server.

### 5.12 Proposed C12 ShaderRegistryAdapter

Proposed C12 owns armor/hair registries, lookup dictionaries, counts, and misc-name lookup only as
client registry state. proposed ShaderRegistryBuildSystem registers definitions once per content
revision and validates duplicate keys. proposed ShaderLookupQuery is read-only and returns a
proposed ShaderHandle, not an external Effect. proposed ShaderAssetAdapter resolves handles and
lifetime. Registry rebuild invalidates dependent C11 bindings; no item or player authority is
mutated.

### 5.13 Proposed C13 WorldMapAndTileSnapshot

Proposed C13 stores map dimensions, black edge width, and tile visual snapshots. MapTile Type,
Light, packed flags, changed/queued bits, and Color remain view/cache data unless integration review
proves a different authority. The proposed MapTileSnapshotSystem consumes a read-only world-tile
projection and writes a new map revision. proposed WorldMapQuery reads bounded coordinates only.
MapTile snapshots never write back to authoritative TileMapStore or world Tile state.

World tile coordinate/value objects and tile revision ownership are crossSubsystemOwner:
integration-review. Persistent world ID and map-file identity are not the same as an entity ID.

### 5.14 Proposed C14 MapOverlayAndUpdateQueue

Proposed C14 owns map transform/clipping/opacity, ping positions and time values, pylon border
presentation, and area invalidation queues/lock lease state. DateTime.Now must be replaced at the
edge by an injected proposed ClockAdapter; it must not be read in a pure query. proposed
MapInvalidationCommand is submitted by world-tile projection; proposed MapOverlaySystem consumes
queue entries and updates overlay snapshots. Queue overflow has an explicit coalescing or rejection
policy and never silently drops authoritative tile mutations.

### 5.15 Proposed C15 MapPersistenceAdapter

Proposed C15 isolates I/O lock, scene-area/snowiness save context, and zlib decompressor state.
proposed MapPersistenceAdapter owns file/cloud policy, stream lifetime, lock acquisition, compression
failure conversion, logging, atomic replacement, and retry. proposed MapSaveProjection reads a
stable C13 snapshot and writes no ECS state. Persistence is outside render-frame critical state;
failure leaves the previous durable map and reports a typed result.

### 5.16 Proposed C16 MapEncodingCatalog

Proposed C16 is immutable compatibility data: header bit positions, reserved/unused bits, option
limits, draw budget, gradients, liquid types, and chunk size. Invariants: no duplicate bit masks,
reserved bits cannot be emitted as active values, limits are positive and bounded, and the catalog
version is explicit. proposed MapEncodingQuery is pure; proposed MapCodecAdapter owns binary read/write.
The catalog does not own map tiles or world persistence identity.

## 6. IDs, boundaries, and dependency direction

Separate identity domains:

| Identity | Proposed meaning | Owner status |
|---|---|---|
| Entity ID | entity/player/projectile identity used to reference snapshots | crossSubsystemOwner: integration-review; not C01/C03/C10 authority |
| Persistent ID | world/save/map document identity and revision | crossSubsystemOwner: integration-review with P14 |
| Network ID | connection-scoped entity/map section/snapshot identity and sequence | crossSubsystemOwner: integration-review with P13/P14 |
| External ID | asset key, shader handle, GraphicsDevice/effect token, audio SlotId, file/cloud key | proposed adapters only; final external contract integration-review |
| Tile coordinate/value | world coordinate plus tile revision/value snapshot | crossSubsystemOwner: integration-review with P04/P14 |

Dependency direction is one-way:
world/entity/player/weather/content snapshots -> proposed scene scan and camera input queries ->
proposed C03/C04/C06/C07 caches -> proposed draw/shader/map projections -> proposed graphics,
clock, audio, network, and persistence adapters. No projection or cache writes upstream authority.
The proposed C01 camera transform can be read by C02 and C14 but does not own player movement.

## 7. Explicit schedule and side-effect ownership

Proposed frame schedule, subject to integration review:

1. proposed FrameInputSnapshotSystem captures input, camera intent, world revision, and clock value.
2. proposed CameraUiTransformSystem updates C01 and publishes a transform revision.
3. proposed SceneScanInputSystem updates C02 from camera and world geometry snapshots.
4. proposed SceneMetricsScanSystem resets and fills C03; proposed SceneMetricsQuery remains pure.
5. proposed SceneVisualSystem updates C04 and emits only proposed audio projection intents.
6. proposed LightingModeSystem selects C06; proposed LightMapScanSystem fills C07 working buffers;
   proposed LightingFrameSwapSystem publishes active buffers.
7. proposed AnimationAdvanceSystem and proposed AnimationRegistrySystem update C08 at their commit
   point.
8. proposed DrawCommandBuildSystem, proposed WorldDrawingProjectionSystem, and proposed
   ShaderParameterSystem build C09-C11 worksets from immutable snapshots.
9. proposed MapTileSnapshotSystem and proposed MapOverlaySystem consume map revisions and C14 queue
   entries; proposed map projection must not block render submission on file I/O.
10. proposed SpriteBatchSubmitAdapter, proposed ShaderApplyAdapter, and proposed VertexSubmitAdapter
    submit C09-C12/C10 in explicit layer order.
11. proposed FramePresentationProjection publishes client/UI/map snapshots; proposed MapSaveProjection
    runs on a persistence schedule from a stable snapshot, never as an implicit render side effect.

Effect ownership:

- clock reads: proposed ClockAdapter;
- graphics device, textures, effects, sampler/blend/rasterizer state: proposed GraphicsAdapter;
- file/cloud/compression/locks/logging: proposed MapPersistenceAdapter;
- audio SlotId and particle publication: proposed AudioAdapter and proposed ParticleAdapter;
- network snapshots: proposed NetworkProjectionAdapter;
- randomness: injected proposed RandomSource;
- all retries: the adapter or command owner that can identify idempotency.

## 8. Complete proposed member assignment ledger

The following ledger contains every one of the 331 member records from the validated P18 input report.
Each row has a proposed unit assignment. The source path and line are evidence locations only. The
last column does not assert that any target type or file exists.

| Source sequence | Leaf | Member kind | Declaring type | Member | Source location | Declared C# type | Proposed role | Evidence status |
|---:|---|---|---|---|---|---|---|---|
| 10 | MainCameraAndUiScaleState | field | Terraria.Main | GameViewMatrix | D:\TRbackup\Version4\Terraria\Main.cs:124 | Terraria.Graphics.SpriteViewMatrix | proposed C01 CameraUiTransformState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 11 | MainCameraAndUiScaleState | field | Terraria.Main | _uiScaleMatrix | D:\TRbackup\Version4\Terraria\Main.cs:126 | Matrix | proposed C01 CameraUiTransformState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 12 | MainCameraAndUiScaleState | field | Terraria.Main | _uiScaleWanted | D:\TRbackup\Version4\Terraria\Main.cs:128 | float | proposed C01 CameraUiTransformState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 13 | MainCameraAndUiScaleState | field | Terraria.Main | _uiScaleUsed | D:\TRbackup\Version4\Terraria\Main.cs:130 | float | proposed C01 CameraUiTransformState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 14 | MainCameraAndUiScaleState | field | Terraria.Main | SettingPlayWhenUnfocused | D:\TRbackup\Version4\Terraria\Main.cs:132 | bool | proposed C01 CameraUiTransformState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 15 | MainCameraAndUiScaleState | field | Terraria.Main | ReversedUpDownArmorSetBonuses | D:\TRbackup\Version4\Terraria\Main.cs:134 | bool | proposed C01 CameraUiTransformState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 16 | MainCameraAndUiScaleState | field | Terraria.Main | instance | D:\TRbackup\Version4\Terraria\Main.cs:136 | Terraria.Main | proposed C01 CameraUiTransformState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 17 | MainCameraAndUiScaleState | field | Terraria.Main | Camera | D:\TRbackup\Version4\Terraria\Main.cs:138 | Terraria.Graphics.Camera | proposed C01 CameraUiTransformState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 61 | MainCameraAndVisualOffsets | field | Terraria.Main | cameraLerp | D:\TRbackup\Version4\Terraria\Main.cs:230 | float | proposed C01 CameraUiTransformState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 62 | MainCameraAndVisualOffsets | field | Terraria.Main | cameraLerpTimer | D:\TRbackup\Version4\Terraria\Main.cs:232 | int | proposed C01 CameraUiTransformState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 63 | MainCameraAndVisualOffsets | field | Terraria.Main | cameraLerpTimeToggle | D:\TRbackup\Version4\Terraria\Main.cs:234 | int | proposed C01 CameraUiTransformState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 64 | MainCameraAndVisualOffsets | field | Terraria.Main | OffsetsNPCOffhand | D:\TRbackup\Version4\Terraria\Main.cs:236 | Vector2[] | proposed C01 CameraUiTransformState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 65 | MainCameraAndVisualOffsets | field | Terraria.Main | OffsetsPlayerOnhand | D:\TRbackup\Version4\Terraria\Main.cs:245 | Vector2[] | proposed C01 CameraUiTransformState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 66 | MainCameraAndVisualOffsets | field | Terraria.Main | OffsetsPlayerHeadgear | D:\TRbackup\Version4\Terraria\Main.cs:269 | Vector2[] | proposed C01 CameraUiTransformState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 67 | MainCameraAndVisualOffsets | field | Terraria.Main | CurrentPan | D:\TRbackup\Version4\Terraria\Main.cs:293 | Vector2 | proposed C01 CameraUiTransformState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 68 | MainCameraAndVisualOffsets | field | Terraria.Main | BlackFadeIn | D:\TRbackup\Version4\Terraria\Main.cs:295 | int | proposed C01 CameraUiTransformState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 236 | MainSceneMetricsState | field | Terraria.Main | _cameraSceneMetrics | D:\TRbackup\Version4\Terraria\Main.cs:640 | Terraria.SceneMetrics | proposed C03 SceneMetricsAggregateCache | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 237 | MainSceneMetricsState | field | Terraria.Main | _playerSceneMetrics | D:\TRbackup\Version4\Terraria\Main.cs:642 | Terraria.SceneMetrics | proposed C03 SceneMetricsAggregateCache | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 238 | MainSceneMetricsState | field | Terraria.Main | _usingSeparateCameraSceneMetrics | D:\TRbackup\Version4\Terraria\Main.cs:644 | bool | proposed C03 SceneMetricsAggregateCache | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 239 | MainSceneMetricsState | field | Terraria.Main | SceneState | D:\TRbackup\Version4\Terraria\Main.cs:646 | Terraria.SceneState | proposed C03 SceneMetricsAggregateCache | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 3475 | SharedSceneScanSettings | field | Terraria.SceneMetricsScanSettings | VisualScanArea | D:\TRbackup\Version4\Terraria\SceneMetricsScanSettings.cs:7 | Rectangle? | proposed C02 SceneScanInputAndGeometry | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 3476 | SharedSceneScanSettings | field | Terraria.SceneMetricsScanSettings | BiomeScanCenterPositionInWorld | D:\TRbackup\Version4\Terraria\SceneMetricsScanSettings.cs:9 | Vector2 | proposed C02 SceneScanInputAndGeometry | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 3477 | SharedSceneScanSettings | field | Terraria.SceneMetricsScanSettings | ScanNPCPositions | D:\TRbackup\Version4\Terraria\SceneMetricsScanSettings.cs:11 | bool | proposed C02 SceneScanInputAndGeometry | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 3478 | SharedSceneScanSettings | field | Terraria.SceneMetricsScanSettings | PerspectivePlayer | D:\TRbackup\Version4\Terraria\SceneMetricsScanSettings.cs:13 | Terraria.Player | proposed C02 SceneScanInputAndGeometry | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 3479 | SharedSceneVisualState | field | Terraria.SceneState | airLightDecay | D:\TRbackup\Version4\Terraria\SceneState.cs:16 | float | proposed C04 SceneVisualProjection | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 3480 | SharedSceneVisualState | field | Terraria.SceneState | solidLightDecay | D:\TRbackup\Version4\Terraria\SceneState.cs:18 | float | proposed C04 SceneVisualProjection | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 3481 | SharedSceneVisualState | field | Terraria.SceneState | outsideWeatherEffectIntensity | D:\TRbackup\Version4\Terraria\SceneState.cs:20 | float | proposed C04 SceneVisualProjection | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 3482 | SharedSceneVisualState | field | Terraria.SceneState | _strongBlizzardSound | D:\TRbackup\Version4\Terraria\SceneState.cs:22 | SlotId | proposed C04 SceneVisualProjection | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 3483 | SharedSceneVisualState | field | Terraria.SceneState | _insideBlizzardSound | D:\TRbackup\Version4\Terraria\SceneState.cs:24 | SlotId | proposed C04 SceneVisualProjection | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 3484 | SharedSceneVisualState | field | Terraria.SceneState | skipTransitions | D:\TRbackup\Version4\Terraria\SceneState.cs:26 | bool | proposed C04 SceneVisualProjection | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 1115 | SharedCaptureAndCameraSupport | field | Terraria.DataStructures.DrillDebugDraw | point | D:\TRbackup\Version4\Terraria.DataStructures\DrillDebugDraw.cs:7 | Vector2 | proposed C05 CaptureAndCameraSupportAdapter | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 1116 | SharedCaptureAndCameraSupport | field | Terraria.DataStructures.DrillDebugDraw | color | D:\TRbackup\Version4\Terraria.DataStructures\DrillDebugDraw.cs:9 | Color | proposed C05 CaptureAndCameraSupportAdapter | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 1117 | SharedCaptureAndCameraSupport | field | Terraria.DataStructures.DroneCameraTracker | _trackedProjectile | D:\TRbackup\Version4\Terraria.DataStructures\DroneCameraTracker.cs:7 | Terraria.Projectile | proposed C05 CaptureAndCameraSupportAdapter | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 1118 | SharedCaptureAndCameraSupport | field | Terraria.DataStructures.DroneCameraTracker | _lastTrackedType | D:\TRbackup\Version4\Terraria.DataStructures\DroneCameraTracker.cs:9 | int | proposed C05 CaptureAndCameraSupportAdapter | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 2633 | SharedCaptureAndCameraSupport | field | Terraria.Graphics.Capture.CaptureManager | Instance | D:\TRbackup\Version4\Terraria.Graphics.Capture\CaptureManager.cs:9 | Terraria.Graphics.Capture.CaptureManager | proposed C05 CaptureAndCameraSupportAdapter | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 3872 | SharedCaptureAndCameraSupport | property | Terraria.Graphics.Capture.CaptureManager | IsCapturing | D:\TRbackup\Version4\Terraria.Graphics.Capture\CaptureManager.cs:15 | bool | proposed C05 CaptureAndCameraSupportAdapter | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 2673 | LightMapCacheState | field | Terraria.Graphics.Light.LightMap | _colors | D:\TRbackup\Version4\Terraria.Graphics.Light\LightMap.cs:9 | Vector3[] | proposed C07 LightMapAndLegacyLightingCache | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 2674 | LightMapCacheState | field | Terraria.Graphics.Light.LightMap | _mask | D:\TRbackup\Version4\Terraria.Graphics.Light\LightMap.cs:11 | Terraria.Graphics.Light.LightMaskMode[] | proposed C07 LightMapAndLegacyLightingCache | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 2675 | LightMapCacheState | field | Terraria.Graphics.Light.LightMap | _random | D:\TRbackup\Version4\Terraria.Graphics.Light\LightMap.cs:13 | Terraria.Utilities.FastRandom | proposed C07 LightMapAndLegacyLightingCache | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 2676 | LightMapCacheState | field | Terraria.Graphics.Light.LightMap | DEFAULT_WIDTH | D:\TRbackup\Version4\Terraria.Graphics.Light\LightMap.cs:15 | int | proposed C07 LightMapAndLegacyLightingCache | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 2677 | LightMapCacheState | field | Terraria.Graphics.Light.LightMap | DEFAULT_HEIGHT | D:\TRbackup\Version4\Terraria.Graphics.Light\LightMap.cs:17 | int | proposed C07 LightMapAndLegacyLightingCache | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 3876 | LightMapCacheState | property | Terraria.Graphics.Light.LightMap | Width | D:\TRbackup\Version4\Terraria.Graphics.Light\LightMap.cs:19 | int | proposed C07 LightMapAndLegacyLightingCache | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 3877 | LightMapCacheState | property | Terraria.Graphics.Light.LightMap | Height | D:\TRbackup\Version4\Terraria.Graphics.Light\LightMap.cs:21 | int | proposed C07 LightMapAndLegacyLightingCache | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 3878 | LightMapCacheState | property | Terraria.Graphics.Light.LightMap | LightDecayThroughAir | D:\TRbackup\Version4\Terraria.Graphics.Light\LightMap.cs:23 | float | proposed C07 LightMapAndLegacyLightingCache | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 3879 | LightMapCacheState | property | Terraria.Graphics.Light.LightMap | LightDecayThroughSolid | D:\TRbackup\Version4\Terraria.Graphics.Light\LightMap.cs:25 | float | proposed C07 LightMapAndLegacyLightingCache | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 3880 | LightMapCacheState | property | Terraria.Graphics.Light.LightMap | LightDecayThroughCrackedBrick | D:\TRbackup\Version4\Terraria.Graphics.Light\LightMap.cs:27 | float | proposed C07 LightMapAndLegacyLightingCache | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 3881 | LightMapCacheState | property | Terraria.Graphics.Light.LightMap | LightDecayThroughWater | D:\TRbackup\Version4\Terraria.Graphics.Light\LightMap.cs:29 | Vector3 | proposed C07 LightMapAndLegacyLightingCache | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 3882 | LightMapCacheState | property | Terraria.Graphics.Light.LightMap | LightDecayThroughHoney | D:\TRbackup\Version4\Terraria.Graphics.Light\LightMap.cs:31 | Vector3 | proposed C07 LightMapAndLegacyLightingCache | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 3883 | LightMapCacheState | property | Terraria.Graphics.Light.LightMap | this[] | D:\TRbackup\Version4\Terraria.Graphics.Light\LightMap.cs:33 | Vector3 | proposed C07 LightMapAndLegacyLightingCache | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 2678 | TileLightScannerState | field | Terraria.Graphics.Light.TileLightScanner | _random | D:\TRbackup\Version4\Terraria.Graphics.Light\TileLightScanner.cs:13 | Terraria.Utilities.FastRandom | proposed C07 LightMapAndLegacyLightingCache | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 1096 | DrawAnimationAndFrameState | field | Terraria.DataStructures.DrawAnimation | Frame | D:\TRbackup\Version4\Terraria.DataStructures\DrawAnimation.cs:8 | int | proposed C08 SpriteAnimationAndFrameState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 1097 | DrawAnimationAndFrameState | field | Terraria.DataStructures.DrawAnimation | FrameCount | D:\TRbackup\Version4\Terraria.DataStructures\DrawAnimation.cs:10 | int | proposed C08 SpriteAnimationAndFrameState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 1098 | DrawAnimationAndFrameState | field | Terraria.DataStructures.DrawAnimation | TicksPerFrame | D:\TRbackup\Version4\Terraria.DataStructures\DrawAnimation.cs:12 | int | proposed C08 SpriteAnimationAndFrameState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 1099 | DrawAnimationAndFrameState | field | Terraria.DataStructures.DrawAnimation | FrameCounter | D:\TRbackup\Version4\Terraria.DataStructures\DrawAnimation.cs:14 | int | proposed C08 SpriteAnimationAndFrameState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 1100 | DrawAnimationAndFrameState | field | Terraria.DataStructures.DrawAnimationVertical | PingPong | D:\TRbackup\Version4\Terraria.DataStructures\DrawAnimationVertical.cs:8 | bool | proposed C08 SpriteAnimationAndFrameState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 1101 | DrawAnimationAndFrameState | field | Terraria.DataStructures.DrawAnimationVertical | NotActuallyAnimating | D:\TRbackup\Version4\Terraria.DataStructures\DrawAnimationVertical.cs:10 | bool | proposed C08 SpriteAnimationAndFrameState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 1293 | DrawAnimationAndFrameState | field | Terraria.DataStructures.SpriteFrame | PaddingX | D:\TRbackup\Version4\Terraria.DataStructures\SpriteFrame.cs:8 | int | proposed C08 SpriteAnimationAndFrameState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 1294 | DrawAnimationAndFrameState | field | Terraria.DataStructures.SpriteFrame | PaddingY | D:\TRbackup\Version4\Terraria.DataStructures\SpriteFrame.cs:10 | int | proposed C08 SpriteAnimationAndFrameState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 1295 | DrawAnimationAndFrameState | field | Terraria.DataStructures.SpriteFrame | _currentColumn | D:\TRbackup\Version4\Terraria.DataStructures\SpriteFrame.cs:12 | byte | proposed C08 SpriteAnimationAndFrameState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 1296 | DrawAnimationAndFrameState | field | Terraria.DataStructures.SpriteFrame | _currentRow | D:\TRbackup\Version4\Terraria.DataStructures\SpriteFrame.cs:14 | byte | proposed C08 SpriteAnimationAndFrameState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 1297 | DrawAnimationAndFrameState | field | Terraria.DataStructures.SpriteFrame | ColumnCount | D:\TRbackup\Version4\Terraria.DataStructures\SpriteFrame.cs:16 | byte | proposed C08 SpriteAnimationAndFrameState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 1298 | DrawAnimationAndFrameState | field | Terraria.DataStructures.SpriteFrame | RowCount | D:\TRbackup\Version4\Terraria.DataStructures\SpriteFrame.cs:18 | byte | proposed C08 SpriteAnimationAndFrameState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 2991 | DrawAnimationAndFrameState | field | Terraria.Animation | _animations | D:\TRbackup\Version4\Terraria\Animation.cs:8 | System.Collections.Generic.List<Terraria.Animation> | proposed C08 SpriteAnimationAndFrameState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 2992 | DrawAnimationAndFrameState | field | Terraria.Animation | _temporaryAnimations | D:\TRbackup\Version4\Terraria\Animation.cs:10 | System.Collections.Generic.Dictionary<Terraria.DataStructures.Point16, Terraria.Animation> | proposed C08 SpriteAnimationAndFrameState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 2993 | DrawAnimationAndFrameState | field | Terraria.Animation | _awaitingRemoval | D:\TRbackup\Version4\Terraria\Animation.cs:12 | System.Collections.Generic.List<Terraria.DataStructures.Point16> | proposed C08 SpriteAnimationAndFrameState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 2994 | DrawAnimationAndFrameState | field | Terraria.Animation | _awaitingAddition | D:\TRbackup\Version4\Terraria\Animation.cs:14 | System.Collections.Generic.List<Terraria.Animation> | proposed C08 SpriteAnimationAndFrameState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 2995 | DrawAnimationAndFrameState | field | Terraria.Animation | _coordinates | D:\TRbackup\Version4\Terraria\Animation.cs:16 | Terraria.DataStructures.Point16 | proposed C08 SpriteAnimationAndFrameState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 2996 | DrawAnimationAndFrameState | field | Terraria.Animation | _tileType | D:\TRbackup\Version4\Terraria\Animation.cs:18 | ushort | proposed C08 SpriteAnimationAndFrameState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 3700 | DrawAnimationAndFrameState | property | Terraria.DataStructures.SpriteFrame | CurrentColumn | D:\TRbackup\Version4\Terraria.DataStructures\SpriteFrame.cs:20 | byte | proposed C08 SpriteAnimationAndFrameState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 3701 | DrawAnimationAndFrameState | property | Terraria.DataStructures.SpriteFrame | CurrentRow | D:\TRbackup\Version4\Terraria.DataStructures\SpriteFrame.cs:32 | byte | proposed C08 SpriteAnimationAndFrameState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 1102 | DrawCommandAndBatchState | field | Terraria.DataStructures.DrawData | texture | D:\TRbackup\Version4\Terraria.DataStructures\DrawData.cs:8 | Texture2D | proposed C09 DrawCommandBatchWorkset | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 1103 | DrawCommandAndBatchState | field | Terraria.DataStructures.DrawData | position | D:\TRbackup\Version4\Terraria.DataStructures\DrawData.cs:10 | Vector2 | proposed C09 DrawCommandBatchWorkset | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 1104 | DrawCommandAndBatchState | field | Terraria.DataStructures.DrawData | destinationRectangle | D:\TRbackup\Version4\Terraria.DataStructures\DrawData.cs:12 | Rectangle | proposed C09 DrawCommandBatchWorkset | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 1105 | DrawCommandAndBatchState | field | Terraria.DataStructures.DrawData | sourceRect | D:\TRbackup\Version4\Terraria.DataStructures\DrawData.cs:14 | Rectangle? | proposed C09 DrawCommandBatchWorkset | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 1106 | DrawCommandAndBatchState | field | Terraria.DataStructures.DrawData | color | D:\TRbackup\Version4\Terraria.DataStructures\DrawData.cs:16 | Color | proposed C09 DrawCommandBatchWorkset | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 1107 | DrawCommandAndBatchState | field | Terraria.DataStructures.DrawData | rotation | D:\TRbackup\Version4\Terraria.DataStructures\DrawData.cs:18 | float | proposed C09 DrawCommandBatchWorkset | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 1108 | DrawCommandAndBatchState | field | Terraria.DataStructures.DrawData | origin | D:\TRbackup\Version4\Terraria.DataStructures\DrawData.cs:20 | Vector2 | proposed C09 DrawCommandBatchWorkset | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 1109 | DrawCommandAndBatchState | field | Terraria.DataStructures.DrawData | scale | D:\TRbackup\Version4\Terraria.DataStructures\DrawData.cs:22 | Vector2 | proposed C09 DrawCommandBatchWorkset | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 1110 | DrawCommandAndBatchState | field | Terraria.DataStructures.DrawData | effect | D:\TRbackup\Version4\Terraria.DataStructures\DrawData.cs:24 | SpriteEffects | proposed C09 DrawCommandBatchWorkset | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 1111 | DrawCommandAndBatchState | field | Terraria.DataStructures.DrawData | shader | D:\TRbackup\Version4\Terraria.DataStructures\DrawData.cs:26 | int | proposed C09 DrawCommandBatchWorkset | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 1112 | DrawCommandAndBatchState | field | Terraria.DataStructures.DrawData | ignorePlayerRotation | D:\TRbackup\Version4\Terraria.DataStructures\DrawData.cs:28 | bool | proposed C09 DrawCommandBatchWorkset | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 1113 | DrawCommandAndBatchState | field | Terraria.DataStructures.DrawData | useDestinationRectangle | D:\TRbackup\Version4\Terraria.DataStructures\DrawData.cs:30 | bool | proposed C09 DrawCommandBatchWorkset | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 1114 | DrawCommandAndBatchState | field | Terraria.DataStructures.DrawData | nullRectangle | D:\TRbackup\Version4\Terraria.DataStructures\DrawData.cs:32 | Rectangle? | proposed C09 DrawCommandBatchWorkset | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 1286 | DrawCommandAndBatchState | field | Terraria.DataStructures.SpriteBatchBeginner | sortMode | D:\TRbackup\Version4\Terraria.DataStructures\SpriteBatchBeginner.cs:9 | SpriteSortMode | proposed C09 DrawCommandBatchWorkset | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 1287 | DrawCommandAndBatchState | field | Terraria.DataStructures.SpriteBatchBeginner | blendState | D:\TRbackup\Version4\Terraria.DataStructures\SpriteBatchBeginner.cs:11 | BlendState | proposed C09 DrawCommandBatchWorkset | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 1288 | DrawCommandAndBatchState | field | Terraria.DataStructures.SpriteBatchBeginner | samplerState | D:\TRbackup\Version4\Terraria.DataStructures\SpriteBatchBeginner.cs:13 | SamplerState | proposed C09 DrawCommandBatchWorkset | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 1289 | DrawCommandAndBatchState | field | Terraria.DataStructures.SpriteBatchBeginner | depthStencilState | D:\TRbackup\Version4\Terraria.DataStructures\SpriteBatchBeginner.cs:15 | DepthStencilState | proposed C09 DrawCommandBatchWorkset | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 1290 | DrawCommandAndBatchState | field | Terraria.DataStructures.SpriteBatchBeginner | rasterizerState | D:\TRbackup\Version4\Terraria.DataStructures\SpriteBatchBeginner.cs:17 | RasterizerState | proposed C09 DrawCommandBatchWorkset | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 1291 | DrawCommandAndBatchState | field | Terraria.DataStructures.SpriteBatchBeginner | effect | D:\TRbackup\Version4\Terraria.DataStructures\SpriteBatchBeginner.cs:19 | Effect | proposed C09 DrawCommandBatchWorkset | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 1292 | DrawCommandAndBatchState | field | Terraria.DataStructures.SpriteBatchBeginner | transformMatrix | D:\TRbackup\Version4\Terraria.DataStructures\SpriteBatchBeginner.cs:21 | Matrix | proposed C09 DrawCommandBatchWorkset | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 1526 | WorldDrawingAuxiliaryState | field | Terraria.GameContent.Drawing.HorizonHelper | _horizonBlendState | D:\TRbackup\Version4\Terraria.GameContent.Drawing\HorizonHelper.cs:14 | BlendState | proposed C10 WorldDrawingProjection | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 1527 | WorldDrawingAuxiliaryState | field | Terraria.GameContent.Drawing.HorizonHelper | MoonColors | D:\TRbackup\Version4\Terraria.GameContent.Drawing\HorizonHelper.cs:22 | Color[] | proposed C10 WorldDrawingProjection | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 1528 | WorldDrawingAuxiliaryState | field | Terraria.GameContent.Drawing.NextHorizonRenderer | _drawData | D:\TRbackup\Version4\Terraria.GameContent.Drawing\NextHorizonRenderer.cs:16 | System.Collections.Generic.List<Terraria.DataStructures.DrawData> | proposed C10 WorldDrawingProjection | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 1529 | WorldDrawingAuxiliaryState | field | Terraria.GameContent.Drawing.ParticleOrchestraSettings | PositionInWorld | D:\TRbackup\Version4\Terraria.GameContent.Drawing\ParticleOrchestraSettings.cs:8 | Vector2 | proposed C10 WorldDrawingProjection | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 1530 | WorldDrawingAuxiliaryState | field | Terraria.GameContent.Drawing.ParticleOrchestraSettings | MovementVector | D:\TRbackup\Version4\Terraria.GameContent.Drawing\ParticleOrchestraSettings.cs:10 | Vector2 | proposed C10 WorldDrawingProjection | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 1531 | WorldDrawingAuxiliaryState | field | Terraria.GameContent.Drawing.ParticleOrchestraSettings | UniqueInfoPiece | D:\TRbackup\Version4\Terraria.GameContent.Drawing\ParticleOrchestraSettings.cs:12 | int | proposed C10 WorldDrawingProjection | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 1532 | WorldDrawingAuxiliaryState | field | Terraria.GameContent.Drawing.ParticleOrchestraSettings | IndexOfPlayerWhoInvokedThis | D:\TRbackup\Version4\Terraria.GameContent.Drawing\ParticleOrchestraSettings.cs:14 | byte | proposed C10 WorldDrawingProjection | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 1533 | WorldDrawingAuxiliaryState | field | Terraria.GameContent.Drawing.TileDrawingBase | DrawOwnBlacks | D:\TRbackup\Version4\Terraria.GameContent.Drawing\TileDrawingBase.cs:10 | bool | proposed C10 WorldDrawingProjection | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 1534 | WorldDrawingAuxiliaryState | field | Terraria.GameContent.Drawing.TileDrawingBase | FlushLogData | D:\TRbackup\Version4\Terraria.GameContent.Drawing\TileDrawingBase.cs:12 | Terraria.TimeLogger.TimeLogData | proposed C10 WorldDrawingProjection | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 1535 | WorldDrawingAuxiliaryState | field | Terraria.GameContent.Drawing.TileDrawingBase | DrawCallLogData | D:\TRbackup\Version4\Terraria.GameContent.Drawing\TileDrawingBase.cs:14 | Terraria.TimeLogger.TimeLogData | proposed C10 WorldDrawingProjection | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 1119 | EntityShadowPresentationState | field | Terraria.DataStructures.EntityShadowInfo | Position | D:\TRbackup\Version4\Terraria.DataStructures\EntityShadowInfo.cs:7 | Vector2 | proposed C10 WorldDrawingProjection | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 1120 | EntityShadowPresentationState | field | Terraria.DataStructures.EntityShadowInfo | Rotation | D:\TRbackup\Version4\Terraria.DataStructures\EntityShadowInfo.cs:9 | float | proposed C10 WorldDrawingProjection | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 1121 | EntityShadowPresentationState | field | Terraria.DataStructures.EntityShadowInfo | Origin | D:\TRbackup\Version4\Terraria.DataStructures\EntityShadowInfo.cs:11 | Vector2 | proposed C10 WorldDrawingProjection | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 1122 | EntityShadowPresentationState | field | Terraria.DataStructures.EntityShadowInfo | Direction | D:\TRbackup\Version4\Terraria.DataStructures\EntityShadowInfo.cs:13 | int | proposed C10 WorldDrawingProjection | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 1123 | EntityShadowPresentationState | field | Terraria.DataStructures.EntityShadowInfo | GravityDirection | D:\TRbackup\Version4\Terraria.DataStructures\EntityShadowInfo.cs:15 | int | proposed C10 WorldDrawingProjection | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 1124 | EntityShadowPresentationState | field | Terraria.DataStructures.EntityShadowInfo | BodyFrameIndex | D:\TRbackup\Version4\Terraria.DataStructures\EntityShadowInfo.cs:17 | int | proposed C10 WorldDrawingProjection | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 3688 | EntityShadowPresentationState | property | Terraria.DataStructures.EntityShadowInfo | HeadgearOffset | D:\TRbackup\Version4\Terraria.DataStructures\EntityShadowInfo.cs:19 | Vector2 | proposed C10 WorldDrawingProjection | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 2713 | SharedShaderBaseParameterState | field | Terraria.Graphics.Shaders.ScreenShaderData | _uColor | D:\TRbackup\Version4\Terraria.Graphics.Shaders\ScreenShaderData.cs:9 | Vector3 | proposed C11 ShaderParameterState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 2714 | SharedShaderBaseParameterState | field | Terraria.Graphics.Shaders.ScreenShaderData | _uSecondaryColor | D:\TRbackup\Version4\Terraria.Graphics.Shaders\ScreenShaderData.cs:11 | Vector3 | proposed C11 ShaderParameterState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 2715 | SharedShaderBaseParameterState | field | Terraria.Graphics.Shaders.ScreenShaderData | _uOpacity | D:\TRbackup\Version4\Terraria.Graphics.Shaders\ScreenShaderData.cs:13 | float | proposed C11 ShaderParameterState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 2716 | SharedShaderBaseParameterState | field | Terraria.Graphics.Shaders.ScreenShaderData | _globalOpacity | D:\TRbackup\Version4\Terraria.Graphics.Shaders\ScreenShaderData.cs:15 | float | proposed C11 ShaderParameterState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 2717 | SharedShaderBaseParameterState | field | Terraria.Graphics.Shaders.ScreenShaderData | _uIntensity | D:\TRbackup\Version4\Terraria.Graphics.Shaders\ScreenShaderData.cs:17 | float | proposed C11 ShaderParameterState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 2718 | SharedShaderBaseParameterState | field | Terraria.Graphics.Shaders.ScreenShaderData | _uTargetPosition | D:\TRbackup\Version4\Terraria.Graphics.Shaders\ScreenShaderData.cs:19 | Vector2 | proposed C11 ShaderParameterState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 2719 | SharedShaderBaseParameterState | field | Terraria.Graphics.Shaders.ScreenShaderData | _uDirection | D:\TRbackup\Version4\Terraria.Graphics.Shaders\ScreenShaderData.cs:21 | Vector2 | proposed C11 ShaderParameterState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 2720 | SharedShaderBaseParameterState | field | Terraria.Graphics.Shaders.ScreenShaderData | _uProgress | D:\TRbackup\Version4\Terraria.Graphics.Shaders\ScreenShaderData.cs:23 | float | proposed C11 ShaderParameterState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 2721 | SharedShaderBaseParameterState | field | Terraria.Graphics.Shaders.ScreenShaderData | _uImageOffset | D:\TRbackup\Version4\Terraria.Graphics.Shaders\ScreenShaderData.cs:25 | Vector2 | proposed C11 ShaderParameterState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 2722 | SharedShaderBaseParameterState | field | Terraria.Graphics.Shaders.ScreenShaderData | _uAssetImages | D:\TRbackup\Version4\Terraria.Graphics.Shaders\ScreenShaderData.cs:27 | Asset<Texture2D>[] | proposed C11 ShaderParameterState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 2723 | SharedShaderBaseParameterState | field | Terraria.Graphics.Shaders.ScreenShaderData | _uCustomImages | D:\TRbackup\Version4\Terraria.Graphics.Shaders\ScreenShaderData.cs:29 | Texture2D[] | proposed C11 ShaderParameterState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 2724 | SharedShaderBaseParameterState | field | Terraria.Graphics.Shaders.ScreenShaderData | _samplerStates | D:\TRbackup\Version4\Terraria.Graphics.Shaders\ScreenShaderData.cs:31 | SamplerState[] | proposed C11 ShaderParameterState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 2725 | SharedShaderBaseParameterState | field | Terraria.Graphics.Shaders.ScreenShaderData | _imageScales | D:\TRbackup\Version4\Terraria.Graphics.Shaders\ScreenShaderData.cs:33 | Vector2[] | proposed C11 ShaderParameterState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 2726 | SharedShaderBaseParameterState | field | Terraria.Graphics.Shaders.ScreenShaderData | uImageSize | D:\TRbackup\Version4\Terraria.Graphics.Shaders\ScreenShaderData.cs:40 | Terraria.Graphics.Shaders.ShaderData.EffectParameter<Vector2>[] | proposed C11 ShaderParameterState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 2727 | SharedShaderBaseParameterState | field | Terraria.Graphics.Shaders.ShaderData.EffectParameter<T> | _setValue | D:\TRbackup\Version4\Terraria.Graphics.Shaders\ShaderData.cs:14 | System.Action<T> | proposed C11 ShaderParameterState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 2728 | SharedShaderBaseParameterState | field | Terraria.Graphics.Shaders.ShaderData.EffectParameter<T> | _cachedParameters | D:\TRbackup\Version4\Terraria.Graphics.Shaders\ShaderData.cs:16 | System.Runtime.CompilerServices.ConditionalWeakTable<Terraria.Graphics.Shaders.ShaderData.EffectParameter, object> | proposed C11 ShaderParameterState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 2729 | SharedShaderBaseParameterState | field | Terraria.Graphics.Shaders.ShaderData | _shader | D:\TRbackup\Version4\Terraria.Graphics.Shaders\ShaderData.cs:24 | Asset<Effect> | proposed C11 ShaderParameterState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 2730 | SharedShaderBaseParameterState | field | Terraria.Graphics.Shaders.ShaderData | _passName | D:\TRbackup\Version4\Terraria.Graphics.Shaders\ShaderData.cs:26 | string | proposed C11 ShaderParameterState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 3887 | SharedShaderBaseParameterState | property | Terraria.Graphics.Shaders.ScreenShaderData | Intensity | D:\TRbackup\Version4\Terraria.Graphics.Shaders\ScreenShaderData.cs:42 | float | proposed C11 ShaderParameterState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 3888 | SharedShaderBaseParameterState | property | Terraria.Graphics.Shaders.ScreenShaderData | CombinedOpacity | D:\TRbackup\Version4\Terraria.Graphics.Shaders\ScreenShaderData.cs:44 | float | proposed C11 ShaderParameterState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 3889 | SharedShaderBaseParameterState | property | Terraria.Graphics.Shaders.ShaderData | Shader | D:\TRbackup\Version4\Terraria.Graphics.Shaders\ShaderData.cs:28 | Effect | proposed C11 ShaderParameterState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 2664 | SharedLightingCoordinatorState | field | Terraria.Graphics.Light.LightingEngine.PerFrameLight | Position | D:\TRbackup\Version4\Terraria.Graphics.Light\LightingEngine.cs:15 | Point | proposed C06 LightingCoordinatorState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 2665 | SharedLightingCoordinatorState | field | Terraria.Graphics.Light.LightingEngine.PerFrameLight | Color | D:\TRbackup\Version4\Terraria.Graphics.Light\LightingEngine.cs:17 | Vector3 | proposed C06 LightingCoordinatorState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 2666 | SharedLightingCoordinatorState | field | Terraria.Graphics.Light.LightingEngine | AREA_PADDING | D:\TRbackup\Version4\Terraria.Graphics.Light\LightingEngine.cs:26 | int | proposed C06 LightingCoordinatorState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 2667 | SharedLightingCoordinatorState | field | Terraria.Graphics.Light.LightingEngine | NON_VISIBLE_PADDING | D:\TRbackup\Version4\Terraria.Graphics.Light\LightingEngine.cs:28 | int | proposed C06 LightingCoordinatorState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 2668 | SharedLightingCoordinatorState | field | Terraria.Graphics.Light.LightingEngine | _perFrameLights | D:\TRbackup\Version4\Terraria.Graphics.Light\LightingEngine.cs:30 | System.Collections.Generic.List<Terraria.Graphics.Light.LightingEngine.PerFrameLight> | proposed C06 LightingCoordinatorState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 2669 | SharedLightingCoordinatorState | field | Terraria.Graphics.Light.LightingEngine | _oldPerFrameLights | D:\TRbackup\Version4\Terraria.Graphics.Light\LightingEngine.cs:32 | System.Collections.Generic.List<Terraria.Graphics.Light.LightingEngine.PerFrameLight> | proposed C06 LightingCoordinatorState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 2670 | SharedLightingCoordinatorState | field | Terraria.Graphics.Light.LightingEngine | _tileScanner | D:\TRbackup\Version4\Terraria.Graphics.Light\LightingEngine.cs:34 | Terraria.Graphics.Light.TileLightScanner | proposed C06 LightingCoordinatorState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 2671 | SharedLightingCoordinatorState | field | Terraria.Graphics.Light.LightingEngine | _activeLightMap | D:\TRbackup\Version4\Terraria.Graphics.Light\LightingEngine.cs:36 | Terraria.Graphics.Light.LightMap | proposed C06 LightingCoordinatorState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 2672 | SharedLightingCoordinatorState | field | Terraria.Graphics.Light.LightingEngine | _workingLightMap | D:\TRbackup\Version4\Terraria.Graphics.Light\LightingEngine.cs:38 | Terraria.Graphics.Light.LightMap | proposed C06 LightingCoordinatorState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 3306 | SharedLightingCoordinatorState | field | Terraria.Lighting | DEFAULT_GLOBAL_BRIGHTNESS | D:\TRbackup\Version4\Terraria\Lighting.cs:12 | float | proposed C06 LightingCoordinatorState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 3307 | SharedLightingCoordinatorState | field | Terraria.Lighting | BLIND_GLOBAL_BRIGHTNESS | D:\TRbackup\Version4\Terraria\Lighting.cs:14 | float | proposed C06 LightingCoordinatorState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 3308 | SharedLightingCoordinatorState | field | Terraria.Lighting | OffScreenTiles | D:\TRbackup\Version4\Terraria\Lighting.cs:16 | int | proposed C06 LightingCoordinatorState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 3309 | SharedLightingCoordinatorState | field | Terraria.Lighting | _mode | D:\TRbackup\Version4\Terraria\Lighting.cs:19 | Terraria.Graphics.Light.LightMode | proposed C06 LightingCoordinatorState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 3310 | SharedLightingCoordinatorState | field | Terraria.Lighting | NewEngine | D:\TRbackup\Version4\Terraria\Lighting.cs:21 | Terraria.Graphics.Light.LightingEngine | proposed C06 LightingCoordinatorState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 3311 | SharedLightingCoordinatorState | field | Terraria.Lighting | LegacyEngine | D:\TRbackup\Version4\Terraria\Lighting.cs:23 | Terraria.Graphics.Light.LegacyLighting | proposed C06 LightingCoordinatorState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 3312 | SharedLightingCoordinatorState | field | Terraria.Lighting | _activeEngine | D:\TRbackup\Version4\Terraria\Lighting.cs:25 | Terraria.Graphics.Light.ILightingEngine | proposed C06 LightingCoordinatorState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 3972 | SharedLightingCoordinatorState | property | Terraria.Lighting | GlobalBrightness | D:\TRbackup\Version4\Terraria\Lighting.cs:27 | float | proposed C06 LightingCoordinatorState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 3973 | SharedLightingCoordinatorState | property | Terraria.Lighting | Mode | D:\TRbackup\Version4\Terraria\Lighting.cs:29 | Terraria.Graphics.Light.LightMode | proposed C06 LightingCoordinatorState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 3974 | SharedLightingCoordinatorState | property | Terraria.Lighting | UsingNewLighting | D:\TRbackup\Version4\Terraria\Lighting.cs:63 | bool | proposed C06 LightingCoordinatorState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 2645 | SharedLegacyLightingState | field | Terraria.Graphics.Light.LegacyLighting.LightingSwipeData | InnerLoop1Start | D:\TRbackup\Version4\Terraria.Graphics.Light\LegacyLighting.cs:16 | int | proposed C07 LightMapAndLegacyLightingCache | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 2646 | SharedLegacyLightingState | field | Terraria.Graphics.Light.LegacyLighting.LightingSwipeData | InnerLoop1End | D:\TRbackup\Version4\Terraria.Graphics.Light\LegacyLighting.cs:18 | int | proposed C07 LightMapAndLegacyLightingCache | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 2647 | SharedLegacyLightingState | field | Terraria.Graphics.Light.LegacyLighting.LightingSwipeData | InnerLoop2Start | D:\TRbackup\Version4\Terraria.Graphics.Light\LegacyLighting.cs:20 | int | proposed C07 LightMapAndLegacyLightingCache | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 2648 | SharedLegacyLightingState | field | Terraria.Graphics.Light.LegacyLighting.LightingSwipeData | InnerLoop2End | D:\TRbackup\Version4\Terraria.Graphics.Light\LegacyLighting.cs:22 | int | proposed C07 LightMapAndLegacyLightingCache | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 2649 | SharedLegacyLightingState | field | Terraria.Graphics.Light.LegacyLighting.LightingSwipeData | JaggedArray | D:\TRbackup\Version4\Terraria.Graphics.Light\LegacyLighting.cs:24 | Terraria.Graphics.Light.LegacyLighting.LightingState[][] | proposed C07 LightMapAndLegacyLightingCache | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 2650 | SharedLegacyLightingState | field | Terraria.Graphics.Light.LegacyLighting.LightingState | R | D:\TRbackup\Version4\Terraria.Graphics.Light\LegacyLighting.cs:37 | float | proposed C07 LightMapAndLegacyLightingCache | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 2651 | SharedLegacyLightingState | field | Terraria.Graphics.Light.LegacyLighting.LightingState | R2 | D:\TRbackup\Version4\Terraria.Graphics.Light\LegacyLighting.cs:39 | float | proposed C07 LightMapAndLegacyLightingCache | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 2652 | SharedLegacyLightingState | field | Terraria.Graphics.Light.LegacyLighting.LightingState | G | D:\TRbackup\Version4\Terraria.Graphics.Light\LegacyLighting.cs:41 | float | proposed C07 LightMapAndLegacyLightingCache | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 2653 | SharedLegacyLightingState | field | Terraria.Graphics.Light.LegacyLighting.LightingState | G2 | D:\TRbackup\Version4\Terraria.Graphics.Light\LegacyLighting.cs:43 | float | proposed C07 LightMapAndLegacyLightingCache | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 2654 | SharedLegacyLightingState | field | Terraria.Graphics.Light.LegacyLighting.LightingState | B | D:\TRbackup\Version4\Terraria.Graphics.Light\LegacyLighting.cs:45 | float | proposed C07 LightMapAndLegacyLightingCache | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 2655 | SharedLegacyLightingState | field | Terraria.Graphics.Light.LegacyLighting.LightingState | B2 | D:\TRbackup\Version4\Terraria.Graphics.Light\LegacyLighting.cs:47 | float | proposed C07 LightMapAndLegacyLightingCache | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 2656 | SharedLegacyLightingState | field | Terraria.Graphics.Light.LegacyLighting.LightingState | CrackedLight | D:\TRbackup\Version4\Terraria.Graphics.Light\LegacyLighting.cs:49 | bool | proposed C07 LightMapAndLegacyLightingCache | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 2657 | SharedLegacyLightingState | field | Terraria.Graphics.Light.LegacyLighting.LightingState | StopLight | D:\TRbackup\Version4\Terraria.Graphics.Light\LegacyLighting.cs:51 | bool | proposed C07 LightMapAndLegacyLightingCache | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 2658 | SharedLegacyLightingState | field | Terraria.Graphics.Light.LegacyLighting.LightingState | WetLight | D:\TRbackup\Version4\Terraria.Graphics.Light\LegacyLighting.cs:53 | bool | proposed C07 LightMapAndLegacyLightingCache | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 2659 | SharedLegacyLightingState | field | Terraria.Graphics.Light.LegacyLighting.LightingState | HoneyLight | D:\TRbackup\Version4\Terraria.Graphics.Light\LegacyLighting.cs:55 | bool | proposed C07 LightMapAndLegacyLightingCache | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 2660 | SharedLegacyLightingState | field | Terraria.Graphics.Light.LegacyLighting | MAX_TEMP_LIGHTS | D:\TRbackup\Version4\Terraria.Graphics.Light\LegacyLighting.cs:58 | int | proposed C07 LightMapAndLegacyLightingCache | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 2661 | SharedLegacyLightingState | field | Terraria.Graphics.Light.LegacyLighting | _tileScanner | D:\TRbackup\Version4\Terraria.Graphics.Light\LegacyLighting.cs:60 | Terraria.Graphics.Light.TileLightScanner | proposed C07 LightMapAndLegacyLightingCache | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 2662 | SharedLegacyLightingState | field | Terraria.Graphics.Light.LegacyLighting | _swipeRandom | D:\TRbackup\Version4\Terraria.Graphics.Light\LegacyLighting.cs:64 | Terraria.Utilities.FastRandom | proposed C07 LightMapAndLegacyLightingCache | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 2663 | SharedLegacyLightingState | field | Terraria.Graphics.Light.LegacyLighting | _lightMap | D:\TRbackup\Version4\Terraria.Graphics.Light\LegacyLighting.cs:66 | Terraria.Graphics.Light.LightMap | proposed C07 LightMapAndLegacyLightingCache | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 3875 | SharedLegacyLightingState | property | Terraria.Graphics.Light.LegacyLighting | Mode | D:\TRbackup\Version4\Terraria.Graphics.Light\LegacyLighting.cs:68 | int | proposed C07 LightMapAndLegacyLightingCache | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 3433 | SharedSceneScanAccumulatorState | field | Terraria.SceneMetrics | BestOreType | D:\TRbackup\Version4\Terraria\SceneMetrics.cs:20 | int | proposed C03 SceneMetricsAggregateCache | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 3468 | SharedSceneScanAccumulatorState | field | Terraria.SceneMetrics | CanPlayCreditsRoll | D:\TRbackup\Version4\Terraria\SceneMetrics.cs:90 | bool | proposed C03 SceneMetricsAggregateCache | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 3469 | SharedSceneScanAccumulatorState | field | Terraria.SceneMetrics | NPCBannerBuff | D:\TRbackup\Version4\Terraria\SceneMetrics.cs:92 | bool[] | proposed C03 SceneMetricsAggregateCache | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 3470 | SharedSceneScanAccumulatorState | field | Terraria.SceneMetrics | hasBanner | D:\TRbackup\Version4\Terraria\SceneMetrics.cs:94 | bool | proposed C03 SceneMetricsAggregateCache | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 3471 | SharedSceneScanAccumulatorState | field | Terraria.SceneMetrics | ClosestNPCPosition | D:\TRbackup\Version4\Terraria\SceneMetrics.cs:96 | Vector2[] | proposed C03 SceneMetricsAggregateCache | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 3472 | SharedSceneScanAccumulatorState | field | Terraria.SceneMetrics | _dummyPlayer | D:\TRbackup\Version4\Terraria\SceneMetrics.cs:98 | Terraria.Player | proposed C03 SceneMetricsAggregateCache | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 3473 | SharedSceneScanAccumulatorState | field | Terraria.SceneMetrics | _tileCounts | D:\TRbackup\Version4\Terraria\SceneMetrics.cs:100 | int[] | proposed C03 SceneMetricsAggregateCache | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 3474 | SharedSceneScanAccumulatorState | field | Terraria.SceneMetrics | _liquidCounts | D:\TRbackup\Version4\Terraria\SceneMetrics.cs:102 | int[] | proposed C03 SceneMetricsAggregateCache | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 3979 | SharedSceneScanAccumulatorState | property | Terraria.SceneMetrics | LastScanTime | D:\TRbackup\Version4\Terraria\SceneMetrics.cs:104 | uint | proposed C03 SceneMetricsAggregateCache | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 3980 | SharedSceneScanAccumulatorState | property | Terraria.SceneMetrics | Center | D:\TRbackup\Version4\Terraria\SceneMetrics.cs:106 | Vector2 | proposed C03 SceneMetricsAggregateCache | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 3981 | SharedSceneScanAccumulatorState | property | Terraria.SceneMetrics | TileCenter | D:\TRbackup\Version4\Terraria\SceneMetrics.cs:108 | Point | proposed C03 SceneMetricsAggregateCache | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 3982 | SharedSceneScanAccumulatorState | property | Terraria.SceneMetrics | BestOrePosition | D:\TRbackup\Version4\Terraria\SceneMetrics.cs:110 | Point | proposed C03 SceneMetricsAggregateCache | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 4022 | SharedSceneScanAccumulatorState | property | Terraria.SceneMetrics | PerspectivePlayer | D:\TRbackup\Version4\Terraria\SceneMetrics.cs:190 | Terraria.Player | proposed C03 SceneMetricsAggregateCache | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 3983 | SharedSceneTileAggregateState | property | Terraria.SceneMetrics | ShimmerTileCount | D:\TRbackup\Version4\Terraria\SceneMetrics.cs:112 | int | proposed C03 SceneMetricsAggregateCache | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 3984 | SharedSceneTileAggregateState | property | Terraria.SceneMetrics | EvilTileCount | D:\TRbackup\Version4\Terraria\SceneMetrics.cs:114 | int | proposed C03 SceneMetricsAggregateCache | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 3985 | SharedSceneTileAggregateState | property | Terraria.SceneMetrics | HolyTileCount | D:\TRbackup\Version4\Terraria\SceneMetrics.cs:116 | int | proposed C03 SceneMetricsAggregateCache | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 3986 | SharedSceneTileAggregateState | property | Terraria.SceneMetrics | HoneyBlockCount | D:\TRbackup\Version4\Terraria\SceneMetrics.cs:118 | int | proposed C03 SceneMetricsAggregateCache | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 3989 | SharedSceneTileAggregateState | property | Terraria.SceneMetrics | SandTileCount | D:\TRbackup\Version4\Terraria\SceneMetrics.cs:124 | int | proposed C03 SceneMetricsAggregateCache | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 3990 | SharedSceneTileAggregateState | property | Terraria.SceneMetrics | MushroomTileCount | D:\TRbackup\Version4\Terraria\SceneMetrics.cs:126 | int | proposed C03 SceneMetricsAggregateCache | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 3991 | SharedSceneTileAggregateState | property | Terraria.SceneMetrics | SnowTileCount | D:\TRbackup\Version4\Terraria\SceneMetrics.cs:128 | int | proposed C03 SceneMetricsAggregateCache | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 3992 | SharedSceneTileAggregateState | property | Terraria.SceneMetrics | WaterCandleCount | D:\TRbackup\Version4\Terraria\SceneMetrics.cs:130 | int | proposed C03 SceneMetricsAggregateCache | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 3993 | SharedSceneTileAggregateState | property | Terraria.SceneMetrics | PeaceCandleCount | D:\TRbackup\Version4\Terraria\SceneMetrics.cs:132 | int | proposed C03 SceneMetricsAggregateCache | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 3994 | SharedSceneTileAggregateState | property | Terraria.SceneMetrics | ShadowCandleCount | D:\TRbackup\Version4\Terraria\SceneMetrics.cs:134 | int | proposed C03 SceneMetricsAggregateCache | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 3995 | SharedSceneTileAggregateState | property | Terraria.SceneMetrics | PartyMonolithCount | D:\TRbackup\Version4\Terraria\SceneMetrics.cs:136 | int | proposed C03 SceneMetricsAggregateCache | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 3996 | SharedSceneTileAggregateState | property | Terraria.SceneMetrics | MeteorTileCount | D:\TRbackup\Version4\Terraria\SceneMetrics.cs:138 | int | proposed C03 SceneMetricsAggregateCache | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 3997 | SharedSceneTileAggregateState | property | Terraria.SceneMetrics | BloodTileCount | D:\TRbackup\Version4\Terraria\SceneMetrics.cs:140 | int | proposed C03 SceneMetricsAggregateCache | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 3998 | SharedSceneTileAggregateState | property | Terraria.SceneMetrics | JungleTileCount | D:\TRbackup\Version4\Terraria\SceneMetrics.cs:142 | int | proposed C03 SceneMetricsAggregateCache | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 3999 | SharedSceneTileAggregateState | property | Terraria.SceneMetrics | DungeonTileCount | D:\TRbackup\Version4\Terraria\SceneMetrics.cs:144 | int | proposed C03 SceneMetricsAggregateCache | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 4017 | SharedSceneTileAggregateState | property | Terraria.SceneMetrics | GraveyardTileCount | D:\TRbackup\Version4\Terraria\SceneMetrics.cs:180 | int | proposed C03 SceneMetricsAggregateCache | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 4018 | SharedSceneTileAggregateState | property | Terraria.SceneMetrics | DesertSandTileCount | D:\TRbackup\Version4\Terraria\SceneMetrics.cs:182 | int | proposed C03 SceneMetricsAggregateCache | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 4019 | SharedSceneTileAggregateState | property | Terraria.SceneMetrics | OceanSandTileCount | D:\TRbackup\Version4\Terraria\SceneMetrics.cs:184 | int | proposed C03 SceneMetricsAggregateCache | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 4021 | SharedSceneTileAggregateState | property | Terraria.SceneMetrics | TownNPCCount | D:\TRbackup\Version4\Terraria\SceneMetrics.cs:188 | int | proposed C03 SceneMetricsAggregateCache | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 2685 | SharedShaderFamilyDataState | field | Terraria.Graphics.Shaders.ArmorShaderData | _uColor | D:\TRbackup\Version4\Terraria.Graphics.Shaders\ArmorShaderData.cs:10 | Vector3 | proposed C11 ShaderParameterState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 2686 | SharedShaderFamilyDataState | field | Terraria.Graphics.Shaders.ArmorShaderData | _uSecondaryColor | D:\TRbackup\Version4\Terraria.Graphics.Shaders\ArmorShaderData.cs:12 | Vector3 | proposed C11 ShaderParameterState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 2687 | SharedShaderFamilyDataState | field | Terraria.Graphics.Shaders.ArmorShaderData | _uSaturation | D:\TRbackup\Version4\Terraria.Graphics.Shaders\ArmorShaderData.cs:14 | float | proposed C11 ShaderParameterState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 2688 | SharedShaderFamilyDataState | field | Terraria.Graphics.Shaders.ArmorShaderData | _uTargetPosition | D:\TRbackup\Version4\Terraria.Graphics.Shaders\ArmorShaderData.cs:16 | Vector2 | proposed C11 ShaderParameterState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 2695 | SharedShaderFamilyDataState | field | Terraria.Graphics.Shaders.HairShaderData | _uColor | D:\TRbackup\Version4\Terraria.Graphics.Shaders\HairShaderData.cs:10 | Vector3 | proposed C11 ShaderParameterState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 2696 | SharedShaderFamilyDataState | field | Terraria.Graphics.Shaders.HairShaderData | _uSecondaryColor | D:\TRbackup\Version4\Terraria.Graphics.Shaders\HairShaderData.cs:12 | Vector3 | proposed C11 ShaderParameterState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 2697 | SharedShaderFamilyDataState | field | Terraria.Graphics.Shaders.HairShaderData | _uSaturation | D:\TRbackup\Version4\Terraria.Graphics.Shaders\HairShaderData.cs:14 | float | proposed C11 ShaderParameterState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 2698 | SharedShaderFamilyDataState | field | Terraria.Graphics.Shaders.HairShaderData | _uOpacity | D:\TRbackup\Version4\Terraria.Graphics.Shaders\HairShaderData.cs:16 | float | proposed C11 ShaderParameterState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 2699 | SharedShaderFamilyDataState | field | Terraria.Graphics.Shaders.HairShaderData | _uImage | D:\TRbackup\Version4\Terraria.Graphics.Shaders\HairShaderData.cs:18 | Asset<Texture2D> | proposed C11 ShaderParameterState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 2700 | SharedShaderFamilyDataState | field | Terraria.Graphics.Shaders.HairShaderData | _shaderDisabled | D:\TRbackup\Version4\Terraria.Graphics.Shaders\HairShaderData.cs:20 | bool | proposed C11 ShaderParameterState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 2701 | SharedShaderFamilyDataState | field | Terraria.Graphics.Shaders.HairShaderData | _uTargetPosition | D:\TRbackup\Version4\Terraria.Graphics.Shaders\HairShaderData.cs:22 | Vector2 | proposed C11 ShaderParameterState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 2705 | SharedShaderFamilyDataState | field | Terraria.Graphics.Shaders.MiscShaderData | _uColor | D:\TRbackup\Version4\Terraria.Graphics.Shaders\MiscShaderData.cs:11 | Vector3 | proposed C11 ShaderParameterState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 2706 | SharedShaderFamilyDataState | field | Terraria.Graphics.Shaders.MiscShaderData | _uSecondaryColor | D:\TRbackup\Version4\Terraria.Graphics.Shaders\MiscShaderData.cs:13 | Vector3 | proposed C11 ShaderParameterState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 2707 | SharedShaderFamilyDataState | field | Terraria.Graphics.Shaders.MiscShaderData | _uImage0 | D:\TRbackup\Version4\Terraria.Graphics.Shaders\MiscShaderData.cs:15 | Asset<Texture2D> | proposed C11 ShaderParameterState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 2708 | SharedShaderFamilyDataState | field | Terraria.Graphics.Shaders.MiscShaderData | _uImage1 | D:\TRbackup\Version4\Terraria.Graphics.Shaders\MiscShaderData.cs:17 | Asset<Texture2D> | proposed C11 ShaderParameterState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 2709 | SharedShaderFamilyDataState | field | Terraria.Graphics.Shaders.MiscShaderData | _uImage2 | D:\TRbackup\Version4\Terraria.Graphics.Shaders\MiscShaderData.cs:19 | Asset<Texture2D> | proposed C11 ShaderParameterState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 2710 | SharedShaderFamilyDataState | field | Terraria.Graphics.Shaders.MiscShaderData | _useProjectionMatrix | D:\TRbackup\Version4\Terraria.Graphics.Shaders\MiscShaderData.cs:21 | bool | proposed C11 ShaderParameterState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 2711 | SharedShaderFamilyDataState | field | Terraria.Graphics.Shaders.MiscShaderData | _shaderSpecificData | D:\TRbackup\Version4\Terraria.Graphics.Shaders\MiscShaderData.cs:23 | Vector4 | proposed C11 ShaderParameterState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 2712 | SharedShaderFamilyDataState | field | Terraria.Graphics.Shaders.MiscShaderData | _customSamplerState | D:\TRbackup\Version4\Terraria.Graphics.Shaders\MiscShaderData.cs:25 | SamplerState | proposed C11 ShaderParameterState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 3886 | SharedShaderFamilyDataState | property | Terraria.Graphics.Shaders.HairShaderData | ShaderDisabled | D:\TRbackup\Version4\Terraria.Graphics.Shaders\HairShaderData.cs:24 | bool | proposed C11 ShaderParameterState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 2689 | SharedShaderRegistryAndLookupState | field | Terraria.Graphics.Shaders.ArmorShaderDataSet | _shaderData | D:\TRbackup\Version4\Terraria.Graphics.Shaders\ArmorShaderDataSet.cs:8 | System.Collections.Generic.List<Terraria.Graphics.Shaders.ArmorShaderData> | proposed C12 ShaderRegistryAdapter | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 2690 | SharedShaderRegistryAndLookupState | field | Terraria.Graphics.Shaders.ArmorShaderDataSet | _shaderLookupDictionary | D:\TRbackup\Version4\Terraria.Graphics.Shaders\ArmorShaderDataSet.cs:10 | System.Collections.Generic.Dictionary<int, int> | proposed C12 ShaderRegistryAdapter | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 2691 | SharedShaderRegistryAndLookupState | field | Terraria.Graphics.Shaders.ArmorShaderDataSet | _shaderDataCount | D:\TRbackup\Version4\Terraria.Graphics.Shaders\ArmorShaderDataSet.cs:12 | int | proposed C12 ShaderRegistryAdapter | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 2692 | SharedShaderRegistryAndLookupState | field | Terraria.Graphics.Shaders.GameShaders | Armor | D:\TRbackup\Version4\Terraria.Graphics.Shaders\GameShaders.cs:7 | Terraria.Graphics.Shaders.ArmorShaderDataSet | proposed C12 ShaderRegistryAdapter | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 2693 | SharedShaderRegistryAndLookupState | field | Terraria.Graphics.Shaders.GameShaders | Hair | D:\TRbackup\Version4\Terraria.Graphics.Shaders\GameShaders.cs:9 | Terraria.Graphics.Shaders.HairShaderDataSet | proposed C12 ShaderRegistryAdapter | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 2694 | SharedShaderRegistryAndLookupState | field | Terraria.Graphics.Shaders.GameShaders | Misc | D:\TRbackup\Version4\Terraria.Graphics.Shaders\GameShaders.cs:11 | System.Collections.Generic.Dictionary<string, Terraria.Graphics.Shaders.MiscShaderData> | proposed C12 ShaderRegistryAdapter | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 2702 | SharedShaderRegistryAndLookupState | field | Terraria.Graphics.Shaders.HairShaderDataSet | _shaderData | D:\TRbackup\Version4\Terraria.Graphics.Shaders\HairShaderDataSet.cs:10 | System.Collections.Generic.List<Terraria.Graphics.Shaders.HairShaderData> | proposed C12 ShaderRegistryAdapter | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 2703 | SharedShaderRegistryAndLookupState | field | Terraria.Graphics.Shaders.HairShaderDataSet | _shaderLookupDictionary | D:\TRbackup\Version4\Terraria.Graphics.Shaders\HairShaderDataSet.cs:12 | System.Collections.Generic.Dictionary<int, short> | proposed C12 ShaderRegistryAdapter | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 2704 | SharedShaderRegistryAndLookupState | field | Terraria.Graphics.Shaders.HairShaderDataSet | _shaderDataCount | D:\TRbackup\Version4\Terraria.Graphics.Shaders\HairShaderDataSet.cs:14 | byte | proposed C12 ShaderRegistryAdapter | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 3429 | SharedSceneZoneGeometryAndThresholdState | field | Terraria.SceneMetrics | AssumedConstantScreenSize | D:\TRbackup\Version4\Terraria\SceneMetrics.cs:12 | Point | proposed C02 SceneScanInputAndGeometry | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 3430 | SharedSceneZoneGeometryAndThresholdState | field | Terraria.SceneMetrics | ZoneScanPadding | D:\TRbackup\Version4\Terraria\SceneMetrics.cs:14 | int | proposed C02 SceneScanInputAndGeometry | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 3431 | SharedSceneZoneGeometryAndThresholdState | field | Terraria.SceneMetrics | ZoneScanSize | D:\TRbackup\Version4\Terraria\SceneMetrics.cs:16 | Point | proposed C02 SceneScanInputAndGeometry | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 3432 | SharedSceneZoneGeometryAndThresholdState | field | Terraria.SceneMetrics | TownNPCRectSize | D:\TRbackup\Version4\Terraria\SceneMetrics.cs:18 | Vector2 | proposed C02 SceneScanInputAndGeometry | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 3434 | SharedSceneZoneGeometryAndThresholdState | field | Terraria.SceneMetrics | SnowTileMax | D:\TRbackup\Version4\Terraria\SceneMetrics.cs:22 | int | proposed C02 SceneScanInputAndGeometry | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 3435 | SharedSceneZoneGeometryAndThresholdState | field | Terraria.SceneMetrics | MushroomTileThreshold | D:\TRbackup\Version4\Terraria\SceneMetrics.cs:24 | int | proposed C02 SceneScanInputAndGeometry | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 3436 | SharedSceneZoneGeometryAndThresholdState | field | Terraria.SceneMetrics | BelowSurface | D:\TRbackup\Version4\Terraria\SceneMetrics.cs:26 | bool | proposed C02 SceneScanInputAndGeometry | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 3437 | SharedSceneZoneGeometryAndThresholdState | field | Terraria.SceneMetrics | ZoneSkyHeight | D:\TRbackup\Version4\Terraria\SceneMetrics.cs:28 | bool | proposed C02 SceneScanInputAndGeometry | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 3438 | SharedSceneZoneGeometryAndThresholdState | field | Terraria.SceneMetrics | ZoneOverworldHeight | D:\TRbackup\Version4\Terraria\SceneMetrics.cs:30 | bool | proposed C02 SceneScanInputAndGeometry | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 3439 | SharedSceneZoneGeometryAndThresholdState | field | Terraria.SceneMetrics | ZoneDirtLayerHeight | D:\TRbackup\Version4\Terraria\SceneMetrics.cs:32 | bool | proposed C02 SceneScanInputAndGeometry | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 3440 | SharedSceneZoneGeometryAndThresholdState | field | Terraria.SceneMetrics | ZoneRockLayerHeight | D:\TRbackup\Version4\Terraria\SceneMetrics.cs:34 | bool | proposed C02 SceneScanInputAndGeometry | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 3441 | SharedSceneZoneGeometryAndThresholdState | field | Terraria.SceneMetrics | ZoneUnderworldHeight | D:\TRbackup\Version4\Terraria\SceneMetrics.cs:36 | bool | proposed C02 SceneScanInputAndGeometry | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 4250 | MapOverlayAndLayerPresentation | field | Terraria.Map.MapOverlayDrawContext | _mapPosition | D:\TRbackup\Version4\Terraria.Map\MapOverlayDrawContext.cs:11 | Vector2 | proposed C14 MapOverlayAndUpdateQueue | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 4251 | MapOverlayAndLayerPresentation | field | Terraria.Map.MapOverlayDrawContext | _mapOffset | D:\TRbackup\Version4\Terraria.Map\MapOverlayDrawContext.cs:13 | Vector2 | proposed C14 MapOverlayAndUpdateQueue | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 4252 | MapOverlayAndLayerPresentation | field | Terraria.Map.MapOverlayDrawContext | _clippingRect | D:\TRbackup\Version4\Terraria.Map\MapOverlayDrawContext.cs:15 | Rectangle? | proposed C14 MapOverlayAndUpdateQueue | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 4253 | MapOverlayAndLayerPresentation | field | Terraria.Map.MapOverlayDrawContext | _mapScale | D:\TRbackup\Version4\Terraria.Map\MapOverlayDrawContext.cs:17 | float | proposed C14 MapOverlayAndUpdateQueue | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 4254 | MapOverlayAndLayerPresentation | field | Terraria.Map.MapOverlayDrawContext | _drawScale | D:\TRbackup\Version4\Terraria.Map\MapOverlayDrawContext.cs:19 | float | proposed C14 MapOverlayAndUpdateQueue | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 4255 | MapOverlayAndLayerPresentation | field | Terraria.Map.MapOverlayDrawContext | _opacity | D:\TRbackup\Version4\Terraria.Map\MapOverlayDrawContext.cs:21 | float | proposed C14 MapOverlayAndUpdateQueue | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 4262 | MapOverlayAndLayerPresentation | field | Terraria.Map.PingMapLayer.Ping | Position | D:\TRbackup\Version4\Terraria.Map\PingMapLayer.cs:15 | Vector2 | proposed C14 MapOverlayAndUpdateQueue | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 4263 | MapOverlayAndLayerPresentation | field | Terraria.Map.PingMapLayer.Ping | Time | D:\TRbackup\Version4\Terraria.Map\PingMapLayer.cs:17 | System.DateTime | proposed C14 MapOverlayAndUpdateQueue | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 4264 | MapOverlayAndLayerPresentation | field | Terraria.Map.PingMapLayer | PING_DURATION_IN_SECONDS | D:\TRbackup\Version4\Terraria.Map\PingMapLayer.cs:26 | double | proposed C14 MapOverlayAndUpdateQueue | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 4265 | MapOverlayAndLayerPresentation | field | Terraria.Map.PingMapLayer | PING_FRAME_RATE | D:\TRbackup\Version4\Terraria.Map\PingMapLayer.cs:28 | double | proposed C14 MapOverlayAndUpdateQueue | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 4266 | MapOverlayAndLayerPresentation | field | Terraria.Map.PingMapLayer | _pings | D:\TRbackup\Version4\Terraria.Map\PingMapLayer.cs:30 | SlotVector<Terraria.Map.PingMapLayer.Ping> | proposed C14 MapOverlayAndUpdateQueue | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 4267 | MapOverlayAndLayerPresentation | field | Terraria.Map.TeleportPylonsMapLayer | BorderSize | D:\TRbackup\Version4\Terraria.Map\TeleportPylonsMapLayer.cs:16 | int | proposed C14 MapOverlayAndUpdateQueue | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 4268 | WorldMapState | field | Terraria.Map.WorldMap | MaxWidth | D:\TRbackup\Version4\Terraria.Map\WorldMap.cs:12 | int | proposed C13 WorldMapAndTileSnapshot | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 4269 | WorldMapState | field | Terraria.Map.WorldMap | MaxHeight | D:\TRbackup\Version4\Terraria.Map\WorldMap.cs:14 | int | proposed C13 WorldMapAndTileSnapshot | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 4270 | WorldMapState | field | Terraria.Map.WorldMap | BlackEdgeWidth | D:\TRbackup\Version4\Terraria.Map\WorldMap.cs:16 | int | proposed C13 WorldMapAndTileSnapshot | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 4271 | WorldMapState | field | Terraria.Map.WorldMap | _tiles | D:\TRbackup\Version4\Terraria.Map\WorldMap.cs:18 | Terraria.Map.MapTile[,] | proposed C13 WorldMapAndTileSnapshot | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 4535 | WorldMapState | property | Terraria.Map.WorldMap | this[] | D:\TRbackup\Version4\Terraria.Map\WorldMap.cs:20 | Terraria.Map.MapTile | proposed C13 WorldMapAndTileSnapshot | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 4259 | MapTileUpdateQueueState | field | Terraria.Map.MapUpdateQueue | MAX_QUEUED_UPDATES | D:\TRbackup\Version4\Terraria.Map\MapUpdateQueue.cs:11 | int | proposed C14 MapOverlayAndUpdateQueue | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 4260 | MapTileUpdateQueueState | field | Terraria.Map.MapUpdateQueue | _areaUpdateQueue | D:\TRbackup\Version4\Terraria.Map\MapUpdateQueue.cs:13 | System.Collections.Generic.List<Rectangle> | proposed C14 MapOverlayAndUpdateQueue | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 4261 | MapTileUpdateQueueState | field | Terraria.Map.MapUpdateQueue | _lock | D:\TRbackup\Version4\Terraria.Map\MapUpdateQueue.cs:15 | object | proposed C14 MapOverlayAndUpdateQueue | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 4194 | CameraMatrixAndViewportState | field | Terraria.Graphics.SpriteViewMatrix | _zoom | D:\TRbackup\Version4\Terraria.Graphics\SpriteViewMatrix.cs:8 | Vector2 | proposed C01 CameraUiTransformState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 4195 | CameraMatrixAndViewportState | field | Terraria.Graphics.SpriteViewMatrix | _translation | D:\TRbackup\Version4\Terraria.Graphics\SpriteViewMatrix.cs:10 | Vector2 | proposed C01 CameraUiTransformState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 4196 | CameraMatrixAndViewportState | field | Terraria.Graphics.SpriteViewMatrix | _zoomMatrix | D:\TRbackup\Version4\Terraria.Graphics\SpriteViewMatrix.cs:12 | Matrix | proposed C01 CameraUiTransformState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 4197 | CameraMatrixAndViewportState | field | Terraria.Graphics.SpriteViewMatrix | _transformationMatrix | D:\TRbackup\Version4\Terraria.Graphics\SpriteViewMatrix.cs:14 | Matrix | proposed C01 CameraUiTransformState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 4198 | CameraMatrixAndViewportState | field | Terraria.Graphics.SpriteViewMatrix | _normalizedTransformationMatrix | D:\TRbackup\Version4\Terraria.Graphics\SpriteViewMatrix.cs:16 | Matrix | proposed C01 CameraUiTransformState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 4199 | CameraMatrixAndViewportState | field | Terraria.Graphics.SpriteViewMatrix | _graphicsDevice | D:\TRbackup\Version4\Terraria.Graphics\SpriteViewMatrix.cs:22 | GraphicsDevice | proposed C01 CameraUiTransformState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 4200 | CameraMatrixAndViewportState | field | Terraria.Graphics.SpriteViewMatrix | PixelPerfectOffset | D:\TRbackup\Version4\Terraria.Graphics\SpriteViewMatrix.cs:24 | float | proposed C01 CameraUiTransformState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 4201 | CameraMatrixAndViewportState | field | Terraria.Graphics.SpriteViewMatrix | PixelPerfectSafeZoomLevelStep | D:\TRbackup\Version4\Terraria.Graphics\SpriteViewMatrix.cs:26 | float | proposed C01 CameraUiTransformState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 4523 | CameraMatrixAndViewportState | property | Terraria.Graphics.Camera | UnscaledPosition | D:\TRbackup\Version4\Terraria.Graphics\Camera.cs:8 | Vector2 | proposed C01 CameraUiTransformState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 4524 | CameraMatrixAndViewportState | property | Terraria.Graphics.Camera | UnscaledSize | D:\TRbackup\Version4\Terraria.Graphics\Camera.cs:10 | Vector2 | proposed C01 CameraUiTransformState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 4525 | CameraMatrixAndViewportState | property | Terraria.Graphics.Camera | ScaledPosition | D:\TRbackup\Version4\Terraria.Graphics\Camera.cs:12 | Vector2 | proposed C01 CameraUiTransformState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 4526 | CameraMatrixAndViewportState | property | Terraria.Graphics.Camera | ScaledSize | D:\TRbackup\Version4\Terraria.Graphics\Camera.cs:14 | Vector2 | proposed C01 CameraUiTransformState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 4527 | CameraMatrixAndViewportState | property | Terraria.Graphics.Camera | GameViewMatrix | D:\TRbackup\Version4\Terraria.Graphics\Camera.cs:46 | Terraria.Graphics.SpriteViewMatrix | proposed C01 CameraUiTransformState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 4528 | CameraMatrixAndViewportState | property | Terraria.Graphics.Camera | Center | D:\TRbackup\Version4\Terraria.Graphics\Camera.cs:50 | Vector2 | proposed C01 CameraUiTransformState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 4529 | CameraMatrixAndViewportState | property | Terraria.Graphics.SpriteViewMatrix | Translation | D:\TRbackup\Version4\Terraria.Graphics\SpriteViewMatrix.cs:28 | Vector2 | proposed C01 CameraUiTransformState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 4530 | CameraMatrixAndViewportState | property | Terraria.Graphics.SpriteViewMatrix | TransformationMatrix | D:\TRbackup\Version4\Terraria.Graphics\SpriteViewMatrix.cs:40 | Matrix | proposed C01 CameraUiTransformState | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 4202 | VertexStripAndColorState | field | Terraria.Graphics.VertexColors | TopLeftColor | D:\TRbackup\Version4\Terraria.Graphics\VertexColors.cs:7 | Color | proposed C10 WorldDrawingProjection | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 4203 | VertexStripAndColorState | field | Terraria.Graphics.VertexColors | TopRightColor | D:\TRbackup\Version4\Terraria.Graphics\VertexColors.cs:9 | Color | proposed C10 WorldDrawingProjection | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 4204 | VertexStripAndColorState | field | Terraria.Graphics.VertexColors | BottomLeftColor | D:\TRbackup\Version4\Terraria.Graphics\VertexColors.cs:11 | Color | proposed C10 WorldDrawingProjection | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 4205 | VertexStripAndColorState | field | Terraria.Graphics.VertexColors | BottomRightColor | D:\TRbackup\Version4\Terraria.Graphics\VertexColors.cs:13 | Color | proposed C10 WorldDrawingProjection | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 4206 | VertexStripAndColorState | field | Terraria.Graphics.VertexStrip.CustomVertexInfo | Position | D:\TRbackup\Version4\Terraria.Graphics\VertexStrip.cs:17 | Vector2 | proposed C10 WorldDrawingProjection | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 4207 | VertexStripAndColorState | field | Terraria.Graphics.VertexStrip.CustomVertexInfo | Color | D:\TRbackup\Version4\Terraria.Graphics\VertexStrip.cs:19 | Color | proposed C10 WorldDrawingProjection | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 4208 | VertexStripAndColorState | field | Terraria.Graphics.VertexStrip.CustomVertexInfo | TexCoord | D:\TRbackup\Version4\Terraria.Graphics\VertexStrip.cs:21 | Vector3 | proposed C10 WorldDrawingProjection | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 4209 | VertexStripAndColorState | field | Terraria.Graphics.VertexStrip.CustomVertexInfo | _vertexDeclaration | D:\TRbackup\Version4\Terraria.Graphics\VertexStrip.cs:23 | VertexDeclaration | proposed C10 WorldDrawingProjection | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 4210 | VertexStripAndColorState | field | Terraria.Graphics.VertexStrip | _vertices | D:\TRbackup\Version4\Terraria.Graphics\VertexStrip.cs:35 | Terraria.Graphics.VertexStrip.CustomVertexInfo[] | proposed C10 WorldDrawingProjection | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 4211 | VertexStripAndColorState | field | Terraria.Graphics.VertexStrip | _indices | D:\TRbackup\Version4\Terraria.Graphics\VertexStrip.cs:37 | short[] | proposed C10 WorldDrawingProjection | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 4212 | VertexStripAndColorState | field | Terraria.Graphics.VertexStrip | _temporaryPositionsCache | D:\TRbackup\Version4\Terraria.Graphics\VertexStrip.cs:39 | System.Collections.Generic.List<Vector2> | proposed C10 WorldDrawingProjection | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 4213 | VertexStripAndColorState | field | Terraria.Graphics.VertexStrip | _temporaryRotationsCache | D:\TRbackup\Version4\Terraria.Graphics\VertexStrip.cs:41 | System.Collections.Generic.List<float> | proposed C10 WorldDrawingProjection | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 4531 | VertexStripAndColorState | property | Terraria.Graphics.VertexStrip.CustomVertexInfo | VertexDeclaration | D:\TRbackup\Version4\Terraria.Graphics\VertexStrip.cs:25 | VertexDeclaration | proposed C10 WorldDrawingProjection | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 4256 | MapTileCellState | field | Terraria.Map.MapTile | Type | D:\TRbackup\Version4\Terraria.Map\MapTile.cs:5 | ushort | proposed C13 WorldMapAndTileSnapshot | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 4257 | MapTileCellState | field | Terraria.Map.MapTile | Light | D:\TRbackup\Version4\Terraria.Map\MapTile.cs:7 | byte | proposed C13 WorldMapAndTileSnapshot | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 4258 | MapTileCellState | field | Terraria.Map.MapTile | _extraData | D:\TRbackup\Version4\Terraria.Map\MapTile.cs:9 | byte | proposed C13 WorldMapAndTileSnapshot | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 4532 | MapTileCellState | property | Terraria.Map.MapTile | IsChanged | D:\TRbackup\Version4\Terraria.Map\MapTile.cs:11 | bool | proposed C13 WorldMapAndTileSnapshot | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 4533 | MapTileCellState | property | Terraria.Map.MapTile | UpdateQueued | D:\TRbackup\Version4\Terraria.Map\MapTile.cs:30 | bool | proposed C13 WorldMapAndTileSnapshot | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 4534 | MapTileCellState | property | Terraria.Map.MapTile | Color | D:\TRbackup\Version4\Terraria.Map\MapTile.cs:49 | byte | proposed C13 WorldMapAndTileSnapshot | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 4245 | MapIoRuntimeState | field | Terraria.Map.MapHelper | IOLock | D:\TRbackup\Version4\Terraria.Map\MapHelper.cs:80 | object | proposed C15 MapPersistenceAdapter | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 4246 | MapIoRuntimeState | field | Terraria.Map.MapHelper | sceneArea | D:\TRbackup\Version4\Terraria.Map\MapHelper.cs:82 | Rectangle | proposed C15 MapPersistenceAdapter | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 4247 | MapIoRuntimeState | field | Terraria.Map.MapHelper | sceneSnowiness | D:\TRbackup\Version4\Terraria.Map\MapHelper.cs:84 | float | proposed C15 MapPersistenceAdapter | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 4249 | MapIoRuntimeState | field | Terraria.Map.MapHelper | zlibDecompress | D:\TRbackup\Version4\Terraria.Map\MapHelper.cs:88 | ZlibCodec | proposed C15 MapPersistenceAdapter | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 4215 | MapEncodingHeaderBitCatalogState | field | Terraria.Map.MapHelper | HeaderEmpty | D:\TRbackup\Version4\Terraria.Map\MapHelper.cs:20 | int | proposed C16 MapEncodingCatalog | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 4216 | MapEncodingHeaderBitCatalogState | field | Terraria.Map.MapHelper | HeaderTile | D:\TRbackup\Version4\Terraria.Map\MapHelper.cs:22 | int | proposed C16 MapEncodingCatalog | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 4217 | MapEncodingHeaderBitCatalogState | field | Terraria.Map.MapHelper | HeaderWall | D:\TRbackup\Version4\Terraria.Map\MapHelper.cs:24 | int | proposed C16 MapEncodingCatalog | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 4218 | MapEncodingHeaderBitCatalogState | field | Terraria.Map.MapHelper | HeaderWater | D:\TRbackup\Version4\Terraria.Map\MapHelper.cs:26 | int | proposed C16 MapEncodingCatalog | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 4219 | MapEncodingHeaderBitCatalogState | field | Terraria.Map.MapHelper | HeaderLava | D:\TRbackup\Version4\Terraria.Map\MapHelper.cs:28 | int | proposed C16 MapEncodingCatalog | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 4220 | MapEncodingHeaderBitCatalogState | field | Terraria.Map.MapHelper | HeaderHoney | D:\TRbackup\Version4\Terraria.Map\MapHelper.cs:30 | int | proposed C16 MapEncodingCatalog | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 4221 | MapEncodingHeaderBitCatalogState | field | Terraria.Map.MapHelper | HeaderHeavenAndHell | D:\TRbackup\Version4\Terraria.Map\MapHelper.cs:32 | int | proposed C16 MapEncodingCatalog | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 4222 | MapEncodingHeaderBitCatalogState | field | Terraria.Map.MapHelper | HeaderBackground | D:\TRbackup\Version4\Terraria.Map\MapHelper.cs:34 | int | proposed C16 MapEncodingCatalog | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 4223 | MapEncodingHeaderBitCatalogState | field | Terraria.Map.MapHelper | Header2_ReadHeader3Bit | D:\TRbackup\Version4\Terraria.Map\MapHelper.cs:36 | int | proposed C16 MapEncodingCatalog | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 4224 | MapEncodingHeaderBitCatalogState | field | Terraria.Map.MapHelper | Header2Color1 | D:\TRbackup\Version4\Terraria.Map\MapHelper.cs:38 | int | proposed C16 MapEncodingCatalog | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 4225 | MapEncodingHeaderBitCatalogState | field | Terraria.Map.MapHelper | Header2Color2 | D:\TRbackup\Version4\Terraria.Map\MapHelper.cs:40 | int | proposed C16 MapEncodingCatalog | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 4226 | MapEncodingHeaderBitCatalogState | field | Terraria.Map.MapHelper | Header2Color3 | D:\TRbackup\Version4\Terraria.Map\MapHelper.cs:42 | int | proposed C16 MapEncodingCatalog | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 4227 | MapEncodingHeaderBitCatalogState | field | Terraria.Map.MapHelper | Header2Color4 | D:\TRbackup\Version4\Terraria.Map\MapHelper.cs:44 | int | proposed C16 MapEncodingCatalog | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 4228 | MapEncodingHeaderBitCatalogState | field | Terraria.Map.MapHelper | Header2Color5 | D:\TRbackup\Version4\Terraria.Map\MapHelper.cs:46 | int | proposed C16 MapEncodingCatalog | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 4229 | MapEncodingHeaderBitCatalogState | field | Terraria.Map.MapHelper | Header2ShimmerBit | D:\TRbackup\Version4\Terraria.Map\MapHelper.cs:48 | int | proposed C16 MapEncodingCatalog | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 4230 | MapEncodingHeaderBitCatalogState | field | Terraria.Map.MapHelper | Header2_UnusedBit8 | D:\TRbackup\Version4\Terraria.Map\MapHelper.cs:50 | int | proposed C16 MapEncodingCatalog | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 4231 | MapEncodingHeaderBitCatalogState | field | Terraria.Map.MapHelper | Header3_ReservedForHeader4Bit | D:\TRbackup\Version4\Terraria.Map\MapHelper.cs:52 | int | proposed C16 MapEncodingCatalog | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 4232 | MapEncodingHeaderBitCatalogState | field | Terraria.Map.MapHelper | Header3_UnusudBit2 | D:\TRbackup\Version4\Terraria.Map\MapHelper.cs:54 | int | proposed C16 MapEncodingCatalog | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 4233 | MapEncodingHeaderBitCatalogState | field | Terraria.Map.MapHelper | Header3_UnusudBit3 | D:\TRbackup\Version4\Terraria.Map\MapHelper.cs:56 | int | proposed C16 MapEncodingCatalog | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 4234 | MapEncodingHeaderBitCatalogState | field | Terraria.Map.MapHelper | Header3_UnusudBit4 | D:\TRbackup\Version4\Terraria.Map\MapHelper.cs:58 | int | proposed C16 MapEncodingCatalog | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 4235 | MapEncodingHeaderBitCatalogState | field | Terraria.Map.MapHelper | Header3_UnusudBit5 | D:\TRbackup\Version4\Terraria.Map\MapHelper.cs:60 | int | proposed C16 MapEncodingCatalog | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 4236 | MapEncodingHeaderBitCatalogState | field | Terraria.Map.MapHelper | Header3_UnusudBit6 | D:\TRbackup\Version4\Terraria.Map\MapHelper.cs:62 | int | proposed C16 MapEncodingCatalog | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 4237 | MapEncodingHeaderBitCatalogState | field | Terraria.Map.MapHelper | Header3_UnusudBit7 | D:\TRbackup\Version4\Terraria.Map\MapHelper.cs:64 | int | proposed C16 MapEncodingCatalog | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 4238 | MapEncodingHeaderBitCatalogState | field | Terraria.Map.MapHelper | Header3_UnusudBit8 | D:\TRbackup\Version4\Terraria.Map\MapHelper.cs:66 | int | proposed C16 MapEncodingCatalog | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 4214 | MapEncodingOptionLimitState | field | Terraria.Map.MapHelper | drawLoopMilliseconds | D:\TRbackup\Version4\Terraria.Map\MapHelper.cs:18 | int | proposed C16 MapEncodingCatalog | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 4239 | MapEncodingOptionLimitState | field | Terraria.Map.MapHelper | maxTileOptions | D:\TRbackup\Version4\Terraria.Map\MapHelper.cs:68 | int | proposed C16 MapEncodingCatalog | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 4240 | MapEncodingOptionLimitState | field | Terraria.Map.MapHelper | maxWallOptions | D:\TRbackup\Version4\Terraria.Map\MapHelper.cs:70 | int | proposed C16 MapEncodingCatalog | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 4241 | MapEncodingOptionLimitState | field | Terraria.Map.MapHelper | maxLiquidTypes | D:\TRbackup\Version4\Terraria.Map\MapHelper.cs:72 | int | proposed C16 MapEncodingCatalog | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 4242 | MapEncodingOptionLimitState | field | Terraria.Map.MapHelper | maxSkyGradients | D:\TRbackup\Version4\Terraria.Map\MapHelper.cs:74 | int | proposed C16 MapEncodingCatalog | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 4243 | MapEncodingOptionLimitState | field | Terraria.Map.MapHelper | maxDirtGradients | D:\TRbackup\Version4\Terraria.Map\MapHelper.cs:76 | int | proposed C16 MapEncodingCatalog | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 4244 | MapEncodingOptionLimitState | field | Terraria.Map.MapHelper | maxRockGradients | D:\TRbackup\Version4\Terraria.Map\MapHelper.cs:78 | int | proposed C16 MapEncodingCatalog | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
| 4248 | MapEncodingOptionLimitState | field | Terraria.Map.MapHelper | MapChunkSize | D:\TRbackup\Version4\Terraria.Map\MapHelper.cs:86 | int | proposed C16 MapEncodingCatalog | proposed member assignment; source readers/writers and lifecycle are partial until implementation evidence closes |
## 9. Evidence gaps and blocking decisions

Evidence gaps, synchronized with the execution plan:

- The first-round P18 public-decomposition report is absent, so this second-round proposal relies on
  the validated inventory, directly read Version4 files, supplemental public documentation, and
  structural references.
- Version4 has stubbed or incomplete Camera.Rebuild/ShouldRebuild, CaptureManager, WorldMap
  construction, queue updates, parts of lighting, and persistence behavior. No omitted behavior is
  inferred.
- The inventory gives declaration locations, not a complete member-level reader/writer graph,
  initialization order, cleanup graph, or failure semantics. Rows are assigned to proposed roles
  but their final owner remains evidence-gap where noted.
- Current NLTX does not contain a matching P18 implementation. Existing adjacent TileMap and
  lighting definitions are not migration results.
- Tile, entity, weather, player, UI, particle, audio, network, persistence, and render scheduler
  boundaries cross other partitions and cannot be finalized in this session.
- The old common constraint file 约束\公共拆分约束.md referenced by historical material is absent.

Blocking decisions:

- final owner of MapTile/WorldMap snapshot versus authoritative world Tile and persistence schema;
- final camera input and scene perspective owner across player, input, UI, drone, and capture flows;
- final SceneMetrics effect/event boundary and tile/liquid/NPC/weather snapshot contract;
- final new/legacy lighting selection and active/working buffer scheduling contract;
- final shader asset lifetime, network handle policy, and graphics-device thread ownership;
- final map network snapshot, section revision, and map-file compatibility version;
- final audio/particle/logging ports and render layer ordering.

No blocking decision is silently resolved here. All shared candidates remain
crossSubsystemOwner: integration-review.

## 10. Focused verifier plan

These are plans only and are not executed in this session:

- coverage verifier: parse the P18 inventory and assert 331 unique source sequences, 29 leaves,
  274 fields, and 57 properties; compare both proposed documents' ledgers;
- camera verifier: finite scale/zoom, scaled versus unscaled coordinates, pan/lerp boundary,
  matrix invalidation, viewport resize, and camera teleport transition;
- scene verifier: scan revision/center invalidation, complete reset before aggregation, threshold
  boundaries, perspective snapshot immutability, and no hidden writes from queries;
- lighting verifier: active versus working map isolation, new/legacy mode transition, cache bounds,
  clear idempotency, deterministic injected randomness, and brightness clamping;
- presentation verifier: animation tick and deferred registry mutation, draw command frame lease,
  sprite batch ownership, vertex/index bounds, shadow snapshot immutability;
- shader verifier: duplicate registry key rejection, lookup miss behavior, staged Apply ordering,
  missing-asset fallback, server-side no-op, and handle lifetime;
- map verifier: packed MapTile bit round-trip, bounds, changed/queued state, queue overflow,
  ping expiry with injected clock, overlay transform, and pylon projection;
- persistence/network verifier: file lock/retry/atomic failure, encoding compatibility,
  immutable map snapshot publication, separate IDs, and no client render cache in authority;
- static checks: capability-first paths, one core public type per file, explicit scheduler edges,
  no external graphics/file types in core state, and every shared edge marked
  crossSubsystemOwner: integration-review.

No compile-capable command, C# migration, test, static verifier, build, or behavior-equivalence
verification has run. verificationStatus remains not-run.

## 11. Integration handoff

partitionId: P18
sessionId: 8c92202ba8c44e9c8bf9ceb7ab7928f7
inputReport: D:\TRbackup\NLTX\docs\migration\ledgers\non-authoritative-component-partitions\18-map-camera-rendering.md
componentDesignPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\non-authoritative\2026-09-11-version4-non-authoritative-P18-map-camera-rendering-component-design.md
componentExecutionPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\non-authoritative\2026-09-11-version4-non-authoritative-P18-map-camera-rendering-component-execution.md
evidenceStatus: existing source-inventory and partial Version4 evidence; first-round report missing
verificationStatus: not-run
productionCodeModified: no
proposedComponents: proposed C01-C16 as listed in section 3
proposedSystems: proposed camera, scan, metrics, lighting, animation, draw, shader, map, and persistence systems as listed
proposedQueries: proposed camera, scene, lighting, shader, map, and encoding queries as listed
proposedAdapters: proposed graphics, capture, clock, audio, particle, shader-asset, map-persistence, codec, and network adapters
sharedTypesForIntegrationReview: Entity ID, persistent ID, network ID, external asset/shader handle, tile coordinate/value, scene snapshot, camera transform, map revision
crossSubsystemOwner: integration-review
notImplemented: no production code, C#, test, csproj, source generator, migration, build, test, or behavior-equivalence verification was created or run

This document is a proposed non-authoritative design. It is not an implementation report, migration
completion report, behavior-equivalence proof, API compatibility proof, network closure, persistence
closure, or claim about current NLTX capability.
