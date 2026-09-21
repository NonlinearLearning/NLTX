# Version4 P16 世界生命周期、住房、指标与环境缓存组件执行计划

partitionId: P16
sessionId: e9e33a27331a40d3a159985a54bbf8eb
inputReport: D:\TRbackup\NLTX\docs\migration\ledgers\authoritative-20-partitions\P16-World-Lifecycle-Housing-Metrics.md
designPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\2026-09-11-version4-P16-world-lifecycle-housing-metrics-component-design.md
executionPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\2026-09-11-version4-P16-world-lifecycle-housing-metrics-component-execution.md
executionStatus: failed
implementationStatus: partial
verificationStatus: partially-verified
evidenceStatus: partial
designCompletedComponents: [C01, C02, C03, C04, C05, C06, C07, C08, C09, C10, C11, C12, C13]
completedComponents: [C03-WorldLoadLifecycleComponent, C03-WorldDimensionCompatibilityState, C03-WorldTransformTransactionComponent, C04-WorldProgressionEventStateComponent, C05-WorldHousingBuildDiagnosticState, C05-WorldNpcSpawnPacingState, C05-WorldDropSuppressionContext, C05-WorldObjectDestructionContext, C06-WorldTileMergePassContext, C06-WorldTileReframeCounter, C07-HousingScanBudgetDefinition, C07-HousingScanVisitedTilesContext, C07-HousingMaterialCounterSnapshot, C08-HousingRoomSearchContext, C08-HousingRoomTileClassificationContext, C08-HousingRoomRequirementResult, C08-HousingRoomDiagnosticFlags, C09-HousingWaterRuleDefinition, C09-CactusWaterEligibilityQuery, C09-WorldEvilRuleDefinition, C10-WorldSizeCatalogDefinition, C10-WorldDimensionSelectionQuery, C10-WorldSpreadBoundaryDefinition, C10-WorldMeteorGeometryQuery, C11-TrapGenerationDiagnosticScratch, C11-GemSelectionPassScratch, C11-MossPalettePassScratch, C11-WorldGenerationRandomPort, C12-WorldTerrainTileOverrideScope, C12-WorldHardmodeTilePolicyState, C12-WorldGrassSpreadState, C12-CrimsonHeartPlacementScratch, C12-WorldTerrainRuleDefinition, C12-WorldFossilShatterScope, C12-WorldBoulderRainState, C12-WorldStormSafeSpotScratch, C12-WorldTreeTopsStateComponent, C12-WorldBackgroundFlashCacheProjection, C12-WorldTerrainDestructionQueueComponent, C12-WorldTerrainCoatingStateComponent, C13-WorldOceanLevelQuery]
currentComponent: C12-WorldTerrainEffectsAndCaches
pendingComponents: [C03-WorldFileRecoveryAdapter, C03-WorldClearCoordinator, C03-TransformingWorldQueryIntegration, C04-WorldSpawnIntentQueue, C04-WorldProgressionPersistenceProjection, C04-WorldProgressionNetworkProjection, C04-WorldEventEffectAdapter, C05-WorldInfectionPolicyOwnerIntegration, C05-WorldNpcSpawnEffectAdapter, C05-WorldHousingDiagnosticProjection, C06-TileMergeQuery, C06-WorldTileFrameCommitPort, C08-WorldHousingRoomSearchState, C09-MysticLogsEventAdapter, C12-WorldTerrainEffectsAndCaches, C13-WorldGenerationRandomSourceAdapter, C01-WorldGenBiomeBackgroundAndDistanceMetrics, C02-WorldGenTileCountMetrics]
lastCheckpointUtc: 2026-09-12T12:30:07.9622729Z
evidence-gap: C03 的三个状态组件已隔离落地，但 WorldFile recovery、clear coordinator、volatile guard bridge、TransformingWorld query、取消/调度/IOLock 和旧尺寸清理调用图仍未闭合。C04 的 shadow-orb/altar 跨 tick 事实组件已落地，但 spawn intent、WorldFile/network projection、effect acknowledgment、事件并发和完整 round-trip 仍未闭合。C05 的住房诊断、NPC 节拍、掉落抑制和物体销毁 guard 已落地；感染策略发现已有近义 `WorldInfectionPolicyStateComponent`，但 owner 复用、UpdateWorld phase 和 NPC/effect adapter 仍需核对。C07 的扫描预算、访问上下文、材质计数快照、房间访问上下文、评分快照和优先级状态已以隔离组件落地；C08 的 room search context、房间 tile 分类上下文、不可变 requirement result 和诊断 flags 也已以隔离组件落地，但 C07/C08 的完整房间搜索与 flood-fill、RoomNeeds 资格查询、评分、房间失败原因与 diagnostics projection、alternate-spot、住房 assignment、NPC/TownManager key 映射、NPC/Town/network/progression effects 和运行时 parity 仍未闭合。C09 仙人掌规则/query、C10 尺寸 catalog/meteor geometry、C11 scratch/random port、C12 地形状态/缓存和 C13 ocean-level query 的既有 evidence gaps 继续有效；本次受影响的 WorldSession 串行构建已通过，但 P16 focused verification 仍未执行。
blocking-decision: C03 新组件只保存加载/清理事实、上一轮尺寸 footprint 和嵌套变换计数；不得在组件内实现 WorldFile、清理协调、跨线程发布、IOLock、取消或主线程 follow-up。C04 新组件只保存 shadow-orb/altar committed facts；不复制已有 Calendar pending-event 状态，不实现 spawn consumer、WorldFile/NetMessage projection 或 effect acknowledgment。C05 新组件只保存诊断、节拍和局部 scope；已有 `WorldInfectionPolicyStateComponent` 不在本分区重复创建，感染策略 owner 复用和 UpdateWorld 接线继续 pending。C07 新增住房扫描/房间评分组件只保存显式输入、短生命周期上下文和不可变结果；不得在组件内实现扫描、评分、资格查询、feedback、NPC/TownManager 关系、网络或持久化副作用。C08 新增的 room search context、房间 tile 分类上下文、不可变 requirement result 和诊断 flags 只保存一次尝试的递归/候选状态、去重分类状态、资格结果与两个 diagnostic bits；不得在组件内实现 flood-fill、RoomNeeds 查询、失败原因 projection、alternate-spot effect、住房 assignment 或 NPC/Town/network/progression effects。C09 WorldEvilRuleDefinition、C10 catalog/meteor geometry、C11 scratch/random port、C12 terrain state/cache 与 C13 ocean query 均保持无 legacy writer、无持久化/网络副作用；不得因局部实现或 focused verifier 证据替代完整运行时 parity 验证。所有外部 adapter、System、Query、Command、Projection、测试和 runtime parity 继续 pending；P16 总体 verificationStatus 保持 not-run。

## 1. 执行边界

状态覆盖说明：文件前部 `evidence-gap` 与 `blocking-decision` 保留了早期计划阶段的历史措辞；以顶部 metadata 和第 10 节 Terminal Checkpoint 为当前状态来源。当前 P16 已完成局部隔离验证，但未完成运行时迁移、存档/网络兼容或行为等价验证。

本节记录设计阶段的迁移顺序和边界约束；实际已执行的隔离实现只在第 9 节 implementation
checkpoint 中记录。除第 9 节列出的 `src/` 隔离实现外，本轮没有修改 `Test/`、`dome/`、
Version4、权威报告或 ledger；未进入 implementation checkpoint 的路径、类型和接口仍为
`status: proposed`。

全局不变量：

- 一个权威状态只有一个 owner writer；兼容 adapter 可以读旧字段，但不能 dual-write。
- Query 只读并返回值/快照；Projection 只向外输出；Adapter 承担外部类型、文件和协议。
- pass scratch 在 pass 开始分配或清零，在成功/失败/取消路径统一释放或归还；不进入世界存档。
- 文件路径、Markdown 顺序和成员来源序号都不定义运行时执行顺序。
- 每个组件完成后，先同步本文件和 design 文档的 checkpoint，再进入下一个组件。

## 2. 提议执行顺序

| Checkpoint | 边界 | 迁移前置 | focused verifier 主题 |
|---|---|---|---|
| C01 | 背景/距离/注册表交接 | world evil、TownManager、Manifest owner 决策 | pure query、key/SaveLoad adapter |
| C02 | tile count/threshold metrics | count refresh 与网络 authority 证据 | snapshot determinism、packet projection |
| C03 | load/transform lifecycle | WorldFile wait/failure 和后台 transform 事务 | transition/rollback/cleanup |
| C04 | progression/event | save fields 与 event command owner | event idempotency、round-trip |
| C05 | housing/spawn pacing | infection/terrain/NPC spawn owner | pacing transitions、side-effect ordering |
| C06 | tile merge | tile merge caller and clear boundary | directional flags and reset |
| C07 | housing counters/scoring | scan budget and score invariants | bounds, duplicate points, deterministic score |
| C08 | room search | NPC relation key and QuickFindHome write-back | room qualification, failure, NPC commit |
| C09 | housing rules/diagnostic | constants and MysticLogs event owner | pure rule query, diagnostic isolation |
| C10 | dimensions | actual world bounds owner and meteor event handoff | catalog values, boundary calculations |
| C11 | generation scratch | every pass allocation/clear owner | no persistence, no cross-pass leakage |
| C12 | terrain effects/caches | TreeTops persistence/network and queue owners | effect order, cache invalidation, queue cleanup |
| C13 | derived properties | transform/random/ocean input ports | deterministic getter/query and no write-back |

## 3. Planned file and adapter sequence

1. Add only the smallest proposed domain-first type for the current checkpoint, one public type per same-named file.
2. Add a focused verifier before routing any legacy write. The verifier must fail if a second writer exists or if a Query mutates input.
3. Route one old reader/writer through the owner System or Adapter while preserving old names behind a compatibility boundary.
4. Verify state transition, reset, error, persistence/network projection, and cross-subsystem handoff for that checkpoint.
5. Record command, project, exit code, warning/error counts and artifact path only when a later compile-capable gate is explicitly run under the repository serial-dotnet policy.
6. Remove the compatibility path only after the relevant verifier and integration review are green. This P16 session does not run those commands.

## 4. C01 checkpoint plan: `WorldGenBiomeBackgroundAndDistanceMetrics`

### Proposed boundaries

- `WorldBackgroundStyleDefinition` and `WorldBiomeDistanceDefinition`: immutable values/definitions, no world writes.
- `WorldBackgroundStyleQuery` and `WorldBiomeDistanceQuery`: pure calculations over an explicit world snapshot.
- `WorldEvilSelectionPassContext` and `WorldEvilSelectionState`: random selection and authoritative commit, with injected random source.
- `TownHousingRegistryAdapter` and `WorldManifestPersistenceAdapter`: external boundaries; both remain `crossSubsystemOwner: integration-review` until formats and key semantics are approved.
- `WorldTerrainFrameEffectState`: receives explicit tile reframe command; it is not owned by the background query.

### Planned sequence

1. Characterize background IDs, distance constants, evil selection, and reframe call sites from Version4.
2. Define immutable input/output snapshots and an adapter contract for TownRoomManager/WorldManifest without changing runtime code.
3. Establish one owner for `crimson` and `generatingRandomEvil`; document failure rollback and random source lifetime.
4. Add pure-query and adapter-focused verifier cases, then route one legacy read at a time.
5. Keep WorldFile and TownRoomManager compatibility adapters until Save/Load and key-mode evidence is complete.

### Completion gate

C01 may be marked design-complete only when the two documents contain the member mapping, owner table, evidence gap, verifier plan and explicit integration-review decisions. Implementation remains not started and verification remains not run.

## 4.1 C02 checkpoint plan: `WorldGenTileCountMetrics`

### Proposed boundaries

- `WorldTileCountPassContext` owns the mutable `tileCounts` array, second-pass accumulators, and scan cursor/interval state for one explicit scan lifetime.
- `WorldTileCountSnapshot` owns immutable totals after the pass closes; it must not expose the mutable array or become a save format without evidence.
- `WorldInfectionThresholdQuery` derives the three threshold bytes from a closed snapshot and explicit clamp/zero-denominator policy.
- `WorldInfectionThresholdNetworkProjection` serializes only server-authoritative `tGood`, `tEvil`, and `tBlood`; it does not accept client writes.
- `WorldSceneMetricQuery` remains an adapter over instance-local `SceneMetrics`, with tick/center invalidation independent of world totals.

### Planned sequence

1. Characterize every write to the `tileCounts` array and the `total*2 -> total*` close operation, including reset and load-time reads.
2. Define a bounded accumulator and immutable snapshot with a monotonically identified scan revision; retain the legacy fields behind an adapter.
3. Specify zero-denominator, integer-to-byte rounding, minimum-nonzero behavior, overflow and duplicate-revision policy from Version4 evidence.
4. Add deterministic snapshot and threshold tests before connecting the network projection; test packet order and server/client authority separately.
5. Prove that local `SceneMetrics` scans do not share mutable storage or invalidate world totals, then migrate one consumer at a time.

### Completion gate

C02 is design-complete for this session. Implementation remains not started; verification remains not run until an affected project and focused verifier are explicitly selected.

## 4.2 C03 checkpoint plan: `WorldLifecycleLoadAndTransformState`

### Proposed boundaries

- `WorldLoadLifecycleComponent` and owner system model loading/generation/ready/failed/clearing while keeping the legacy volatile guard behind a compatibility adapter.
- `WorldTransformTransactionComponent` and owner system model the increment/decrement activity count; `TransformingWorld` is a read-only Query.
- `WorldDimensionCompatibilityState` retains the previous allocation footprint needed by room edge checks and clear cleanup; it is not the world dimension authority.
- `WorldFileRecoveryAdapter` owns file errors, backup existence, copy/delete and retry decisions; it does not write ECS state directly.
- `WorldClearCoordinator` emits explicit reset commands to tile, NPC, liquid, TownManager, map and event owners.

### Planned sequence

1. Characterize all guard reads and writes, including the WorldFile settle window and CreateNewWorld background task.
2. Characterize transform nesting, IOLock ownership, main-thread follow-up and save waiting; specify cancellation and exception behavior before implementation.
3. Define state transitions and compatibility publication for the legacy booleans and previous-dimension footprint.
4. Add transition, concurrency, recovery and cleanup verifiers before routing load/transform calls.
5. Migrate one lifecycle boundary at a time; retain legacy adapters until all guard consumers and save/load paths are covered.

### Completion gate

C03 is design-complete for this session. No source implementation, build, test, or runtime behavior claim is made.

## 4.3 C04 checkpoint plan: `WorldLifecycleProgressionAndEventState`

### Proposed boundaries

- `WorldProgressionEventStateComponent` owns the committed cross-tick facts `shadowOrbSmashed`, `shadowOrbCount`, and `altarCount`.
- `WorldSpawnIntentQueue` carries bounded, validated `spawnEye`, `spawnHardBoss` (0-3), and `spawnMeteor` intents for one event window; consumers acknowledge or retain them according to an explicit failure policy.
- `WorldProgressionPersistenceProjection` preserves the proven WorldFile field order and widths; it must not serialize an in-flight mutation.
- `WorldProgressionNetworkProjection` is limited to packet fields with evidence and is server-authoritative.
- `WorldEventEffectAdapter` performs NPC/boss/meteor spawn and external chat/achievement effects; it cannot mutate progression directly.

### Planned sequence

1. Map every event producer, consumer, clear/reset path and WorldFile version branch.
2. Replace unconstrained event ints/bools with validated commands and bounded intent states while preserving the legacy adapter.
3. Define an atomic progression update for shadow-orb threshold behavior and the exact acknowledgment point for each effect.
4. Add round-trip, idempotency, failure-retention, server-authority and duplicate-tick verifiers.
5. Route one event consumer at a time; only remove legacy writes after persistence/network and effect verifiers pass.

### Completion gate

C04 is design-complete for this session. Implementation remains not started; verification remains not run.

## 4.4 C05 checkpoint plan: `WorldLifecycleHousingAndSpawnPacingState`

### Proposed boundaries

- `WorldHousingBuildDiagnosticState` records build diagnostics and publishes immutable diagnostic output; it does not own NPC housing assignments.
- `WorldInfectionSpreadPolicyState` is recomputed from explicit world/creative-power inputs at the start of each world update.
- `WorldNpcSpawnPacingState` owns the delay/period clock; NPC spawn is an external effect adapter with an explicit acknowledgment policy.
- `WorldDropSuppressionContext` and `WorldObjectDestructionContext` are scoped operation contexts with `finally` cleanup, not long-lived world flags.

### Planned sequence

1. Map UpdateWorld's policy, mechanism, liquid, tile, housing and spawn subphases and identify effect ports.
2. Characterize all early returns and exceptions around `stopDrops` and `destroyObject`; define scoped reset behavior before routing writes.
3. Introduce immutable policy and diagnostic snapshots, then a pacing decision function with an explicit world-update revision.
4. Add reset, tick, blocked-spawn, nested-operation and side-effect-order verifiers.
5. Route one legacy flag at a time; keep the old fields behind compatibility adapters until all consumers are covered.

### Completion gate

C05 is design-complete for this session. Implementation remains not started; verification remains not run.

## 4.5 C06 checkpoint plan: `WorldLifecycleTileMergeState`

### Proposed boundaries

- `WorldTileMergePassContext` contains the four directional flags for one framing call and nested cosmetic calls.
- `TileMergeQuery` is a pure query over explicit neighbors and flags; it preserves the Version4 opposite-direction mapping.
- `WorldTileFrameCommitPort` owns tile/frame/network writes, while `WorldTileReframeCounter` owns recursion guard increments and finally cleanup.
- No merge flag is persisted, replicated or retained across ticks.

### Planned sequence

1. Record every flag set/read/reset site and the opposite-direction mapping.
2. Define a value snapshot and scoped context API with explicit save/restore for recursive cosmetic framing.
3. Add the all-combination query matrix and reset/reentrancy verifiers before routing TileFrame.
4. Compare against existing Dome query/test evidence without treating it as newly executed evidence.
5. Route root framing, recursive framing, and commit/network effects separately; remove the legacy fields only after the context leak and parity gates pass.

### Completion gate

C06 is design-complete for this session. Implementation remains not started; verification remains not run.

## 4.6 C07 checkpoint plan: `WorldHousingCountersAndScoringState`

### Proposed boundaries

- `HousingScanBudgetDefinition` supplies `maxTileCount` and `maxWallOut2`; it is immutable input and has no tile/NPC writes.
- `HousingScanVisitedTilesContext` owns one `countTiles`/`nextCount` recursion, including point de-duplication, bounded counter and five material counters; `countDirtTiles` must not silently reuse it.
- `HousingMaterialCounterSnapshot` closes the scan and exposes immutable lava/ice/sand/rock/shroom counts.
- `HousingRoomVisitedTilesContext` owns one `StartRoomCheck` room bitmap/bounds lifetime; `HousingRoomScoreSnapshot` owns score, shared-room and best-coordinate output.
- `HousingAssignmentPriorityState` carries validated NPC selection intent toward C08; it does not own the NPC relation or TownManager key.

### Planned sequence

1. Map all `countTiles`, `nextCount`, `countDirtTiles`, `ScoreRoom` and generation callers, including reset, nested and early-return paths.
2. Define separate scoped scan and room-score contexts with explicit clear/close behavior; preserve `maxTileCount` cutoffs, duplicate-point handling and material counting order.
3. Define occupancy short-circuit, pollution penalty, candidate tie policy and `IRoomCheckFeedback` output without allowing a Query to mutate `WorldGen` or NPC state.
4. Add bounded-scan, consecutive-scan, room-score and deterministic-candidate verifiers before routing any legacy writer.
5. Publish only immutable score/material snapshots and a priority command to C08; retain the old static facade until the maxWallOut2 writer and score parity owners are approved.

### Completion gate

C07 is design-complete only when the 15-member mapping, scoped owner table, C08 handoff, evidence gaps and focused verifier plan are recorded. The isolated implementation of the scan budget, visited-tiles context, material snapshot, room context, score snapshot, and priority state is recorded below; the scan/score owner integration remains pending and verification remains not run.

## 4.7 C08 checkpoint plan: `WorldHousingRoomSearchState`

### Proposed boundaries

- `HousingRoomSearchContext` owns one `StartRoomCheck` attempt, including bounds, `roomTiles`, `houseTile`, stack and failure reason; it is discarded after the attempt.
- `HousingRoomRequirementQuery` consumes an immutable tile-category snapshot and returns the four requirement flags plus `CanSpawn` without writing global flags.
- `HousingRoomCandidateSnapshot` and `HousingRoomDiagnosticProjection` carry score/candidate and diagnostic output separately.
- `HousingAssignmentCommand` is the only proposed commit seam for NPC home, homeless and homeless-despawn changes; NPC/Town/network effects remain adapters.
- `HousingAlternateSpotScope` and `HousingTileSolidityOverrideScope` restore guard/candidate state in `finally` across recursive `QuickFindHome` attempts.

### Planned sequence

1. Characterize `StartRoomCheck`, `CheckRoom`, `RoomNeeds`, `ScoreRoom`, `QuickFindHome`, alternate housing recursion and all feedback consumers.
2. Define immutable room input/output records and a scoped search context that resets all observed flags, bounds and stacks at each attempt.
3. Define a single assignment commit and an explicit adapter boundary for NPC home fields, TownRoomManager and network/progression projections.
4. Add room qualification, failure-reason precedence, fallback order, occupancy, key-mode and exception-restoration verifiers.
5. Route the legacy facade through the owner system one operation at a time; stop at the `TownHousingResidentKey` versus `PersistentEntityId` integration review instead of dual-writing.

### Completion gate

C08 is design-complete only when the 19-member mapping, QuickFindHome side-effect order, key conflict, adapter ownership and focused verifier plan are recorded. Isolated result/state components are implemented below; full search, query, assignment, adapter, and projection integration remains pending and verification remains not run.

## 4.8 C09 checkpoint plan: `WorldHousingRuleAndDiagnosticState`

### Proposed boundaries

- `WorldEvilRuleDefinition` and its validated command own the `-1/0/1` generation request; they do not own committed `crimson` state.
- `HousingWaterRuleDefinition` is the immutable cactus width/height/limit catalog; `CactusWaterEligibilityQuery` receives explicit liquid and world snapshots and preserves integer threshold semantics.
- `MysticLogsEventAdapter` owns the Version4 event object, clock, stump scan and NPC spawner effects; `MysticLogsEventProjection` is one-way diagnostic output.

### Planned sequence

1. Close all evil parameter writers and generation branches, all cactus rule callers, and the MysticLogs lifecycle callback graph.
2. Validate input values and define the pure cactus decision over clipped liquid windows without calling placement methods.
3. Add an explicit event lifecycle port for StartWorld, StartNight, UpdateTime, FallenLogDestroyed and WorldClear with failure/reset behavior.
4. Add value-domain, integer-division, event-order, clock and effect-isolation verifiers.
5. Keep the legacy constants/object behind adapters until C01/C04 authority and event side-effect ownership are approved.

### Completion gate

C09 is design-complete only when the five-member mapping, immutable rule boundaries, event adapter lifecycle, evidence gaps and verifier plan are recorded. Implementation remains not started and verification remains not run.

## 4.9 C10 checkpoint plan: `WorldGenerationDimensionsState`

### Proposed boundaries

- `WorldSizeCatalogDefinition` contains the three fixed size profiles; `WorldDimensionSelectionQuery` returns a profile without writing Main allocation state.
- `WorldSpreadBoundaryDefinition` remains a definition-only seam until a Version4 consumer for the 10-tile buffer is found.
- `WorldMeteorGeometryQuery` handles explicit dimension/surface constraints; `MeteorProgressionHandoff` sends `meteorShowerCount` to the C04 progression owner and does not duplicate it.

### Planned sequence

1. Map Main auto-create/CLI selection, `clearWorld`, WorldFile edge/dimension I/O and all meteor initialization, decrement, clear and temporary capture paths.
2. Define one size catalog and compatibility mapping while preserving current literal assignments behind an adapter.
3. Characterize tile/world-unit boundary conversion and previous-dimension cleanup before adding any dimension writer.
4. Add catalog, boundary, load mismatch, meteor geometry and C04 handoff verifiers; treat the unconsumed spread buffer as blocked evidence.
5. Route one dimension consumer at a time and leave WorldFile format unchanged until version/round-trip evidence is complete.

### Completion gate

C10 is design-complete only when the eight-member mapping, catalog/boundary seam, C04 meteor handoff, unresolved spread consumer and verifier plan are recorded. Implementation remains not started and verification remains not run.

## 4.10 C11 checkpoint plan: `WorldGenerationScratchState`

### Proposed boundaries

- `TrapGenerationDiagnosticScratch` is scoped to trap generation; no reset or external projection is invented while its consumer remains unobserved.
- `GemSelectionPassScratch` is cleared at `gemCave` entry and consumed by `randGem`/`randGemTile` within that pass.
- `MossPalettePassScratch` preserves the observed distinction between full `randMoss(false)` and neon-only `randMoss(true)` calls.
- `WorldGenerationRandomPort` supplies an explicit stream/adapter while preserving Version4 draw order; none of these types is persistent or networked.

### Planned sequence

1. Map each scratch allocation, reset, read and GenVars/Spread/tile effect consumer by pass.
2. Define pass leases and finally cleanup, retaining `justNeon` palette retention and the observed gem selection order.
3. Add trap, gem, moss and random-stream verifiers, including repeated passes and exception/cancellation behavior.
4. Route one pass through the scoped context while preserving legacy fields behind compatibility adapters.
5. Do not add a generic scratch component or clear `trapDiag` until its producer/consumer contract is confirmed.

### Completion gate

C11 is design-complete only when the four-member mapping, three scratch seams, random port, no-persistence rule and evidence gaps are recorded. Implementation remains not started and verification remains not run.

## 4.11 C12 checkpoint plan: `WorldTerrainEffectsAndCaches`

### Proposed boundaries

- Split temporary tile-solid backup, fossil guard, hardmode/grass policy, boulder/heart event state, destruction queue and frame/rope guards by lifetime and writer.
- Keep `WorldTreeTopsStateComponent` with explicit WorldFile/NetMessage adapters; keep `WorldBackgroundFlashCacheProjection` as independently invalidated derived cache.
- Keep coating lists, storm safe spots, heart positions and bit-strip data in pass/update scratch; keep cat-tail/item/housing/strip declarations as Definitions until consumers are proven.

### Planned sequence

1. Map Reset/Restore, UpdateWorld phases, recursive effects, queue producers/consumer, TreeTops save/sync, background flash tick and all unknown declaration references.
2. Specify effect ordering, queue flush/retention, exception cleanup and C05/C06/C01 handoffs before creating any terrain owner.
3. Add tile-solid restore, recursion, policy, queue, TreeTops round-trip/network and cache invalidation verifiers.
4. Route one effect boundary at a time; keep unknown queue producers and unconsumed declarations deferred rather than inventing behavior.
5. Remove legacy fields only after producer closure, single-writer checks and focused parity gates are approved.

### Completion gate

C12 is design-complete only when the 20-member mapping, lifetime-based terrain boundaries, TreeTops/cache split, unknown-consumer record and verifier plan are recorded. Implementation remains not started and verification remains not run.

## 4.12 C13 checkpoint plan: `WorldGenDerivedProperties`

### Proposed boundaries

- `TransformingWorldQuery` reads the C03 transaction owner; it never mutates or caches the busy state.
- `WorldGenerationRandomSourceAdapter` bridges the existing Main.rand owner to explicit pass ports and does not create a second RNG owner.
- `WorldOceanLevelQuery` calculates the exact double formula from explicit surface/rock-layer inputs and has no write-back.

### Planned sequence

1. Map transform callers, nested count updates, WorldFile save wait, Main.rand initialization and all oceanLevel consumers.
2. Define query/adapter contracts and cross-check C03 transaction, C10 dimensions and C12 random/effect handoffs.
3. Add transform count, random identity/draw-order and ocean double-precision verifiers before routing any derived reader.
4. Keep compatibility getters until cancellation, random stream and worldSurface/rockLayer authority are closed.
5. Treat all three members as derived/adapter boundaries; never add them to persistent ECS state.

### Completion gate

C13 is design-complete only when the three-member mapping, query/adapter contracts, cross-partition handoffs, evidence gaps and focused verifier plan are recorded. Implementation remains not started and verification remains not run.

## 5. Rollback and failure policy

- If a verifier shows changed background selection, distance boundary, random sequence, TownManager key mapping, or Manifest round-trip, retain the legacy route and stop that checkpoint.
- If persistence or network format is unavailable, mark the adapter blocked; do not invent a version or packet layout.
- If a component needs an external owner from another partition, emit a handoff record with member names, source sequence, proposed seam and `crossSubsystemOwner: integration-review`; do not change that partition's document.
- `Fail` is reserved for an actual unrecoverable session blocker; a design evidence gap alone is recorded in this plan and does not call ledger `Fail`.

## 6. Verification command policy for later implementation

During the original design-only phase no compile-capable command was run. The later implementation
checkpoint inspected active `dotnet.exe`/`csc.exe` processes and used only the repository wrapper
from the root, as shown below:

```powershell
pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 `
  build .\src\<affected-project>\<affected-project>.csproj `
  -m:1 -nr:false -p:UseSharedCompilation=false `
  -p:MSBuildNodeReuse=false -p:BuildInParallel=false
```

The actual project, warning/error counts, exit code and `Build/bin/` artifact must be recorded only after execution. Tests must use `--no-build --no-restore` after the affected project has been built serially.

## 7. Checkpoint log

### C01 `WorldGenBiomeBackgroundAndDistanceMetrics`

- Design state: complete for this planned session.
- Proposed seams: pure background/distance Query; evil selection pass; TownManager adapter; Manifest adapter; terrain frame command.
- Integration review: `TownManager`, `Manifest`, `crimson` persistence owner and `tileReframeCount` handoff remain open.
- Verification: not run.
- Implementation: not started.

### C02 `WorldGenTileCountMetrics`

- Design state: complete for this planned session.
- Proposed seams: bounded tile-count pass context; immutable totals snapshot; pure threshold Query; server-only network Projection; isolated SceneMetrics adapter.
- Integration review: network threshold authority, packet revision/duplicate policy, WorldFile count scope, and SceneMetrics storage boundary remain open.
- Verification: not run.
- Implementation: not started.

### C03 `WorldLifecycleLoadAndTransformState`

- Design state: complete for this planned session.
- Proposed seams: load lifecycle owner; transform transaction counter; read-only busy Query; dimension compatibility snapshot; WorldFile recovery adapter; clear coordinator.
- Integration review: cancellation/exception propagation, scheduler/IOLock ports, Main guard bridge, and backup retry ownership remain open.
- Verification: not run.
- Implementation: not started.

### C04 `WorldLifecycleProgressionAndEventState`

- Design state: complete for this planned session.
- Proposed seams: progression facts; bounded spawn intent; effect adapter with acknowledgment; WorldFile persistence Projection; server network Projection.
- Integration review: event authority, `spawnHardBoss` value contract, byte/int format compatibility, effect failure policy and save/event concurrency remain open.
- Verification: not run.
- Implementation: not started.

### C05 `WorldLifecycleHousingAndSpawnPacingState`

- Design state: complete for this planned session.
- Proposed seams: build diagnostic snapshot; per-update infection policy; NPC pacing state; scoped drop suppression; scoped object destruction context.
- Integration review: UpdateWorld phase owner, creative power input, NPC spawn acknowledgment, terrain effect ports and nested reset behavior remain open.
- Verification: not run.
- Implementation: not started.

### C06 `WorldLifecycleTileMergeState`

- Design state: complete for this planned session.
- Proposed seams: scoped directional merge context; pure TileMergeQuery; frame commit port; reframe recursion counter.
- Existing evidence: Dome `TileMergeQuery` and verifier references were inspected but not executed in this session.
- Integration review: all framing entry points, exception cleanup, frame/network effect ownership and exact Version4 parity remain open.
- Verification: not run.
- Implementation: not started.

### C07 `WorldHousingCountersAndScoringState`

- Design state: complete for this planned session.
- Proposed seams: bounded recursive scan context; immutable material counter snapshot; room visited-tile context; room score snapshot; validated housing priority state.
- Source evidence: `WorldGen.countTiles` clears counters and `CountedTiles`, stops at world/terrain/liquid boundaries or `maxTileCount`, deduplicates points, and counts five material categories; `ScoreRoom` clears `sharedRoomX`, rejects occupied rooms with `hiScore = -1`, applies category/material penalties, and commits only a higher valid candidate.
- Migration impact: legacy `WorldGen.cs:4193-4219,4237` fields remain behind compatibility adapters; C08 receives snapshots/commands rather than mutable dictionaries, bitsets, or direct NPC writes.
- Integration review: `maxWallOut2` writer closure, `IRoomCheckFeedback` ownership, all nested scan callers, score tie semantics, and the NPC/TownManager key conflict remain open.
- Verification: not run.
- Implementation: not started.

### C08 `WorldHousingRoomSearchState`

- Design state: complete for this planned session.
- Proposed seams: scoped room search context; pure RoomNeeds eligibility result; candidate snapshot; diagnostic projection; HousingAssignmentCommand; alternate-spot and tile-solidity scopes.
- Source evidence: `StartRoomCheck` resets room bitmap/classification/flags and delegates bounded recursive checks; `RoomNeeds` computes four required categories; `QuickFindHome` tries fallback locations and writes NPC home/homeless state; `TownRoomManager` persists NPC-type keyed room pairs.
- Migration impact: retain the legacy facade behind a compatibility adapter; pass immutable room/score snapshots to C08 and commit NPC/Town/network effects once through an explicit owner.
- Integration review: `TownHousingResidentKey` versus `PersistentEntityId`, full NPC authority, TownManager Save/Load mapping, feedback ownership, and exception-safe restoration of `Main.tileSolid[379]` remain open.
- Verification: not run.
- Implementation: not started.

### C09 `WorldHousingRuleAndDiagnosticState`

- Design state: complete for this planned session.
- Proposed seams: validated evil parameter command/definition; immutable cactus water rule; pure cactus eligibility query; MysticLogs lifecycle adapter and one-way event projection.
- Source evidence: `WorldGenParam_Evil` is set at the main input boundary and consumed by world-generation branches; cactus constants drive bounded liquid checks with integer threshold semantics; `MysticLogFairiesEvent` owns load/night/update/clear state and NPC-spawner effects.
- Migration impact: keep event object and static fields behind adapters; pass explicit liquid/world snapshots into the cactus Query and send placement/event effects through owners.
- Integration review: non-CLI evil writers, complete cactus call graph, clock/NPC spawner ports, event failure policy, and authority relative to C01/C04 remain open.
- Verification: the design-phase verifier was not run; the isolated implementation build and the
  existing regression verifier are recorded in section 9, while a P16 focused verifier is still
  missing.
- Implementation: isolated `WorldEvilRuleDefinition` and cactus rule/query implemented; MysticLogs boundary
  remains not started because its external event object, clock, NPC spawner, and reset owner are unresolved.

### C10 `WorldGenerationDimensionsState`

- Design state: complete for this planned session.
- Proposed seams: immutable size catalog; pure dimension selection; explicit spread boundary policy; meteor geometry Query; C04 progression handoff.
- Source evidence: catalog values are declared in `WorldGen`; Main auto-create/CLI assigns matching dimensions; WorldFile reads/writes edge and max tile dimensions; spread buffer has no observed consumer; `meteorShowerCount` is initialized, decremented, cleared, and temporarily captured/restored.
- Migration impact: do not replace Main literals or add WorldFile fields until one dimension owner and file compatibility mapping are approved; do not duplicate the meteor counter in C10.
- Integration review: dimension allocation owner, spread consumer, C04 meteor authority, WorldFile version format, and C03 previous-dimension cleanup remain open.
- Verification: the design-phase verifier was not run; the isolated implementation build and the
  existing regression verifier is recorded in section 9, while a P16 focused verifier is still
  missing. The latest regression run failed in the pre-existing WorldLayerMetrics verifier assertion;
  it is not evidence against the isolated C09 Definition, so `verificationStatus` remains `not-run`.
- Implementation: isolated size catalog/selection/boundary definition implemented; dimension,
  spread and meteor boundaries not started.

### C11 `WorldGenerationScratchState`

- Design state: complete for this planned session.
- Proposed seams: trap diagnostic scratch; gem selection scratch; moss palette scratch; explicit random stream port; no persistent/network component.
- Source evidence: `trapDiag` is written by trap-generation branches with no observed consumer; `gemCave` clears/selects `gem`, which `randGem` consumes; `randMoss(false)` refreshes full and neon palettes while `randMoss(true)` refreshes neon only, and `setMoss`/`neonMossBiome` consume the respective palette.
- Migration impact: retain pass-local lifetime and exact random/draw order; keep unobserved `trapDiag` clear behavior unresolved rather than inventing a reset.
- Integration review: trap diagnostic consumer, random source ownership, GenVars moss effect owner, Spread/tile placement port, and exception cleanup remain open.
- Verification: not run.
- Implementation: not started.

### C12 `WorldTerrainEffectsAndCaches`

- Design state: complete for this planned session.
- Proposed seams: tile-solid restore scope; per-effect terrain state; destruction queue; TreeTops authoritative state plus WorldFile/network adapters; BackgroundsCache projection; scoped safe-spots/heart/coating/bit-strip scratch; immutable rule definitions.
- Source evidence: Reset/RestoreTemporaryStateChanges manage `tileSolidBackup`; UpdateWorld and recursive effects use phase-local flags/counters; storm safe spots are cleared/rebuilt; Crimson hearts are reset/collected/placed; TreeTops has explicit Save/Load/SyncSend; BackgroundsCache is updated from background styles and tick-decayed.
- Migration impact: keep TreeTops persistence/network boundary separate from cache and scratch; do not add behavior for declarations with no observed consumers; keep `destroyObject` handoff with C05.
- Integration review: queue producer, rope guard read, bitStrip/constant consumers, effect ordering, TreeTops authority/version mapping, and exception cleanup remain open.
- Verification: not run.
- Implementation: not started.

### C13 `WorldGenDerivedProperties`

- Design state: complete for this planned session.
- Proposed seams: read-only TransformingWorld query; Main.rand-backed random source adapter; pure ocean-level query over explicit surface/rock-layer inputs.
- Source evidence: `TransformingWorld` reads the C03 counter; background transforms increment/decrement with `Interlocked` and `finally`; WorldFile save waits on the getter; `genRand` returns `Main.rand`; `oceanLevel` uses the exact double formula and is consumed by `oceanDepths`.
- Migration impact: keep transaction, random, and world-surface owners outside this checkpoint; preserve draw order and avoid caching or writing derived values.
- Integration review: full transform caller/cancellation graph, Main.rand stream ownership, worldSurface/rockLayer load authority, and cross-checks with C03/C10/C12 remain open.
- Verification: the design-phase verifier was not run; the isolated implementation build and the
  existing regression verifier are recorded in section 9, while a P16 focused verifier is still
  missing.
- Implementation: isolated ocean-level query implemented; transform/random boundaries not started.

## 8. Current status

All 13 checkpoints are design-plan complete for this session. The implementation phase has completed isolated C03-C07 state units, C08 room search context, room tile classification context, immutable requirement result, and diagnostic flags, C09 WorldEvilRuleDefinition and cactus rule/query, C10 size catalog/selection/boundary definition, C11 trap/gem/moss scratch and explicit random port, four local C12 terrain states plus the terrain rule definition and fossil nested guard, and C13 ocean-level query; no legacy writer, public API, registration key, persistence format, or network path was replaced. Current metadata is `currentComponent: C08-WorldHousingRoomSearchState`, `executionStatus: in-progress`, `implementationStatus: in-progress`, and `verificationStatus: not-run`.

## 9. Implementation Checkpoint (2026-09-12)

### C09 `WorldHousingRuleAndDiagnosticState` - cactus water rule/query

- Saved source files before advancing:
  - `src/WorldSession/WorldGeneration/Housing/HousingWaterRuleDefinition.cs`
  - `src/WorldSession/WorldGeneration/Housing/CactusWaterGridSnapshot.cs`
  - `src/WorldSession/WorldGeneration/Housing/CactusWaterEligibilityResult.cs`
  - `src/WorldSession/WorldGeneration/Housing/CactusWaterEligibilityQuery.cs`
- Core behavior: fixed Version4 cactus dimensions and limit; defensive copy of the liquid grid;
  clipped half-open scan window; `byte.MaxValue` liquid scale; integer division before the strict
  limit comparison; `result.ExceedsLimit && (!remixWorld || candidateY <= worldSurfaceY)` remix/world-
  surface decision; no external side effects.
- Correction checkpoint: Version4 `GrowCactus` uses
  `(!Main.remixWorld || !(j > Main.worldSurface)) && num3 / 255 > cactusWaterLimit`; the Query now
  preserves that polarity so a remix-world candidate above the surface is the only bypass.
- Dependency impact: new files are contained by `Terraria.WorldSession`; no existing source,
  project file, registration key, public API, network protocol, persistence format, or other
  partition document was changed.
- Verification: the affected WorldSession project build completed with exit code `0`, `0` warnings,
  and `0` errors; the existing regression verifier also completed with exit code `0`. Neither covers
  the four new C09 types, so the P16 focused verifier remains not run.
- Remaining C09 work: evil request validation/owner and MysticLogs adapter remain blocked by
  unresolved authority and lifecycle/effect-port evidence. C10 meteor/spread, C11-C12, C13
  transform/random, and C01-C08 remain pending.

### C10 `WorldGenerationDimensionsState` - size catalog/selection/boundary definition

- Saved source files before advancing:
  - `src/WorldSession/WorldGeneration/Passes/WorldSizeProfile.cs`
  - `src/WorldSession/WorldGeneration/Passes/WorldSizeCatalogDefinition.cs`
  - `src/WorldSession/WorldGeneration/Passes/WorldDimensionSelectionQuery.cs`
  - `src/WorldSession/WorldGeneration/Passes/WorldSpreadBoundaryDefinition.cs`
- Core behavior: exact Version4 small/medium/large dimensions, legacy `0/1/2` profile selection,
  width threshold mapping, and an explicit immutable outer-world buffer definition with value `10`.
  Selection is pure and has no `Main`/WorldFile writes.
- Dependency impact: new files are contained by `Terraria.WorldSession`; no current dimension writer,
  WorldFile compatibility path, meteor counter, spread consumer, or other partition document was
  changed.
- Verification: the affected WorldSession project build completed with exit code `0`, `0` warnings,
  and `0` errors; the existing regression verifier also completed with exit code `0`. It does not
  cover the four new C10 types, so the P16 focused verifier remains not run.
- Remaining C10 work: dimension allocation/edge conversion, spread consumer closure, meteor progress handoff,
  spawn tile/solid/platform/density scan, Collision/meteor effect and runtime call-graph integration remain pending.

### C10 `WorldGenerationDimensionsState` - WorldMeteorGeometryQuery

- Saved source files before advancing:
  - `src/WorldSession/WorldGeneration/Passes/WorldMeteorGeometryQuery.cs`
  - `src/WorldSession/WorldGeneration/Passes/WorldMeteorSpawnGeometry.cs`
- Saved focused verifier changes:
  - `src/WorldTerrainC12FocusedVerifier/Terraria.WorldTerrainC12FocusedVerifier.csproj`
  - `src/WorldTerrainC12FocusedVerifier/Program.cs`
- Core behavior: pure explicit-input Query preserving the Version4 ordinary start expression
  `(int)(worldSurface * 0.3)` and underground cast/division order `(int)(worldSurface + rockLayer) / 2`;
  horizontal candidate range `[150, maxTilesX - 150)`, vertical range
  `[verticalMinInclusive, maxTilesY)`, `maxTilesX * 0.08` spawn exclusion distance, and strict
  `>`/`<` exclusion boundaries. `WorldMeteorSpawnGeometry` is a read-only `record struct` with bounded
  candidate and exclusion checks.
- Dependency impact: added two `Terraria.WorldSession` source files and included them in the existing focused
  verifier. No Main, Tile, Projectile, Collision, random source, `meteorShowerCount`, WorldFile, network,
  registration key, or public API was changed; no meteor effect is committed by this unit.
- Focused verifier build command:
  `pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 build .\src\WorldTerrainC12FocusedVerifier\Terraria.WorldTerrainC12FocusedVerifier.csproj -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false`
  completed with exit code `0`, `0` warnings, and `0` errors. Artifact:
  `Build/bin/WorldTerrainC12FocusedVerifier/Debug/net10.0/WorldTerrainC12FocusedVerifier.dll`.
- Focused verifier run command:
  `pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 run --project .\src\WorldTerrainC12FocusedVerifier\Terraria.WorldTerrainC12FocusedVerifier.csproj --no-build --no-restore -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false`
  completed with exit code `0` and printed `WorldMeteorGeometryQuery C10 focused verifier passed.`
- Remaining work: tile-solid/platform/density adapter, random coordinate selection, Collision and actual
  `meteor(...)` effect, `meteorShowerCount` progression/clear behavior, C04 handoff, dimension owner,
  WorldFile compatibility and runtime behavior-equivalence verification. `implementationStatus` for this
  isolated unit is `implemented-isolated`; P16 overall `verificationStatus` remains `not-run`.

| checkpoint | saved UTC | completed/current transition | actual source files | implementationStatus | verificationStatus |
|---|---|---|---|---|---|
| C09 cactus rule/query | 2026-09-12T01:27:00.0000000Z | C09 sub-unit completed -> C10 catalog current | 4 new files under `src/WorldSession/WorldGeneration/Housing/` | implemented-isolated | not-run |
| C10 size catalog/selection/boundary | 2026-09-12T01:31:25.0000000Z | C10 sub-unit completed -> C13 ocean-level query current | 4 new files under `src/WorldSession/WorldGeneration/Passes/` | implemented-isolated | not-run |
| C12 local terrain units and rule definition | 2026-09-11T21:06:35.9293109Z | C12 local state/rule units completed -> C12 terrain effects and caches current | 5 new files under `src/WorldSession/WorldGeneration/Terrain/` | implemented-isolated | not-run |
| C12 fossil shatter scope | 2026-09-11T21:48:00.1297466Z | fossil nested-guard unit completed -> C12 terrain effects and caches remains current | `src/WorldSession/WorldGeneration/Terrain/WorldFossilShatterScope.cs`; `src/WorldTerrainC12FocusedVerifier/` | implemented-isolated | focused-pass; P16 not-run |
| C12 storm safe-spot scratch | 2026-09-11T22:08:59.8419482Z | storm safe-spot scratch completed -> C12 terrain effects and caches remains current | `src/WorldSession/WorldGeneration/Terrain/WorldStormSafeSpot.cs`; `src/WorldSession/WorldGeneration/Terrain/WorldStormSafeSpotScratch.cs`; `src/WorldTerrainC12FocusedVerifier/` | implemented-isolated | focused-pass; P16 not-run |
| C12 boulder rain state | 2026-09-11T22:28:54.6170029Z | boulder transition state completed -> C12 terrain effects and caches remains current | `src/WorldSession/WorldGeneration/Terrain/WorldBoulderRainState.cs`; `src/WorldSession/WorldGeneration/Terrain/WorldBoulderRainTransition.cs`; `src/WorldTerrainC12FocusedVerifier/` | implemented-isolated | focused-pass; P16 not-run |
| C12 TreeTops state | 2026-09-11T22:45:05.4284341Z | TreeTops state/snapshot completed -> C12 terrain effects and caches remains current | `src/WorldSession/WorldGeneration/Terrain/TreeTops/WorldTreeTopsStateComponent.cs`; `src/WorldSession/WorldGeneration/Terrain/TreeTops/WorldTreeTopsStateSnapshot.cs`; `src/WorldTerrainC12FocusedVerifier/` | implemented-isolated | focused-pass; P16 not-run |
| C12 background flash cache projection | 2026-09-11T23:00:02.4786147Z | background variation/flash projection completed -> C12 terrain effects and caches remains current | `src/WorldSession/WorldGeneration/Metrics/WorldBackgroundFlashCacheProjection.cs`; `src/WorldTerrainC12FocusedVerifier/` | implemented-isolated | focused-pass; P16 not-run |
| C13 ocean-level query | 2026-09-11T18:01:25.3576027Z | C13 sub-unit completed -> no current component | 1 new file under `src/WorldSession/WorldGeneration/Metrics/` | implemented-isolated | not-run |

### C12 `WorldTerrainEffectsAndCaches` - terrain rule definition and fossil shatter scope

- Saved source file before advancing:
  - `src/WorldSession/WorldGeneration/Terrain/WorldTerrainRuleDefinition.cs`
- Core behavior: preserves the Version4 declaration constants `ItemSpawnProtectionTime=18000`,
  `catTailDistance=8`, `BUBBLES_SOLID_STATE_FOR_HOUSING=true`, `strip_w=200`, and `strip_h=50`
  as PascalCase public constants in a read-only definition.
- Dependency impact: the new file is contained by `Terraria.WorldSession`; it does not connect to
  `WorldGen`, Tile, housing, save, network, strip, or bit-strip effects because no consumer/owner
  boundary is confirmed.
- Verification: no P16 focused verifier covers this definition; `verificationStatus` remains `not-run`.
- Additional saved source file before advancing:
  `src/WorldSession/WorldGeneration/Terrain/WorldFossilShatterScope.cs`.
- `WorldFossilShatterScope` enforces the confirmed fossil tile gate (`404`), rejects nested active
  attempts, and releases its local guard idempotently on `Dispose`. It does not perform Tile kill,
  random selection, drops, network messaging, or any other terrain effect.
- P16 focused verifier files:
  `src/WorldTerrainC12FocusedVerifier/Terraria.WorldTerrainC12FocusedVerifier.csproj` and
  `src/WorldTerrainC12FocusedVerifier/Program.cs`. The verifier covers fossil tile admission,
  nested rejection, idempotent disposal, and re-entry; its build and run both passed.
- Remaining C12 work: fossil effect adapters plus boulder/storm/queue/TreeTops/background/coating/
  bit-strip/rope boundaries remain pending until their writers, lifecycle, effect ordering, and
  adapters are evidenced.
- Additional saved source files before advancing:
  `src/WorldSession/WorldGeneration/Terrain/WorldStormSafeSpot.cs` and
  `src/WorldSession/WorldGeneration/Terrain/WorldStormSafeSpotScratch.cs`.
- `WorldStormSafeSpot` preserves explicit rectangle geometry with half-open containment and overflow-
  resistant boundary arithmetic. `WorldStormSafeSpotScratch` clears and rebuilds current-update safe
  spots without exposing a mutable list. It does not own player filtering, velocity projection,
  random sampling, lightning spawning, Tile, or network effects.
- The isolated verifier build and run both passed; see the verification checkpoint below.
- Remaining C12 work: fossil effect adapters plus boulder/storm lightning/queue/TreeTops/background/
  coating/bit-strip/rope boundaries remain pending until their writers, lifecycle, effect ordering,
  and adapters are evidenced.
- Additional saved source files before advancing:
  `src/WorldSession/WorldGeneration/Terrain/WorldBoulderRainState.cs` and
  `src/WorldSession/WorldGeneration/Terrain/WorldBoulderRainTransition.cs`.
- `WorldBoulderRainState` preserves the confirmed `SpawnFallingObjects` seed/storm state transition,
  no-surface early return, clear-world reset, and falling-to-inactive progression intent. The intent
  is not an achievement side effect; random position selection, Collision, Projectile, Tile and
  network effects remain outside this unit.
- The isolated verifier build and run passed the boulder start/end, notification, early-return and
  reset cases.
- Remaining C12 work: boulder spawn/achievement adapters plus fossil effect, storm lightning, queue,
  TreeTops/background/coating/bit-strip/rope boundaries remain pending until their writers, lifecycle,
  effect ordering, and adapters are evidenced.

### C12 `WorldTerrainEffectsAndCaches` - WorldTreeTopsStateComponent

- Saved source files before advancing:
  - `src/WorldSession/WorldGeneration/Terrain/TreeTops/WorldTreeTopsStateComponent.cs`
  - `src/WorldSession/WorldGeneration/Terrain/TreeTops/WorldTreeTopsStateSnapshot.cs`
- `WorldTreeTopsStateComponent` follows the confirmed Version4 `TreeTopsInfo.AreaId.Count=13` and
  stores 13 `int` variation slots. It provides explicit area reads/writes and rejects area ids outside
  `0..12`; it does not guess a legal style range for any area.
- `WorldTreeTopsStateSnapshot` copies all 13 slots and exposes a read-only wrapper. Later component
  mutations do not alter an existing snapshot, and callers cannot write through `IList<int>` to the
  snapshot storage.
- This unit owns only in-memory TreeTops state. WorldFile `Save/Load`, the pre-211 fallback,
  `NetMessage.SyncSend/SyncReceive`, random style selection, Tree FX, and server authority remain
  deferred to evidenced adapters/owners; no legacy API, registration key, persistence format, or
  network path was changed.
- The focused verifier covers the 13-area count, area 0/12 reads and writes, snapshot isolation,
  range rejection, and read-only exposure. Build and no-build run passed. TreeTops adapters, format
  round-trip, and runtime behavior equivalence remain unverified.

### C12 `WorldTerrainEffectsAndCaches` - WorldBackgroundFlashCacheProjection

- Saved source file before advancing:
  - `src/WorldSession/WorldGeneration/Metrics/WorldBackgroundFlashCacheProjection.cs`
- `WorldBackgroundFlashCacheProjection` stores 13 background variation values and 13 flash values,
  matching the confirmed `BackgroundChangeFlashInfo` layout. `UpdateCache` accepts an explicit style
  snapshot, starts the changed area's flash at `1f` outside the game menu, and does not start it in the
  menu. `UpdateFlashValues` decays every area by `0.05f` and clamps to `[0,1]`.
- Reads validate the area id. The projection has no `WorldGen`, TreeTops, XNA, rendering, persistence,
  or network dependency and does not become the background-style authoritative writer.
- The focused verifier covers style invalidation, menu behavior, tick decay, zero clamp, 13-area shape,
  and invalid inputs; build and no-build run passed. The legacy `WorldGen.setBG`/`Main` scheduling and
  render adapter remain unimplemented.

### C13 `WorldGenDerivedProperties` - ocean-level query

- Saved source file before advancing:
  - `src/WorldSession/WorldGeneration/Metrics/WorldOceanLevelQuery.cs`
- Core behavior: accepts explicit `worldSurface` and `rockLayer` inputs, rejects non-finite values,
  and returns `(worldSurface + rockLayer) / 2.0 + 40.0` without caching or write-back. This preserves
  the Version4 `oceanLevel` getter's double-precision calculation while leaving `worldSurface` and
  `rockLayer` ownership outside C13.
- Dependency impact: the new file is contained by `Terraria.WorldSession`; no ocean-depth consumer,
  world-surface owner, `Main`, `WorldFile`, network path, registration key, or other partition
  document was changed.
- Verification: the affected WorldSession project build completed with exit code `0`, `0` warnings,
  and `0` errors; the existing regression verifier also completed with exit code `0`. It does not
  cover the new C13 type, so the P16 focused verifier remains not run.
- Remaining C13 work: `TransformingWorldQuery` and `WorldGenerationRandomSourceAdapter` remain
  blocked by transform caller/cancellation, random-stream ownership, and cross-pass draw-order evidence.

### C12 `WorldTerrainEffectsAndCaches` - WorldTerrainDestructionQueueComponent

- Actual source saved before advancing:
  - `src/WorldSession/WorldGeneration/Terrain/WorldTerrainDestructionQueueComponent.cs`
- `WorldTerrainDestructionQueueComponent` stores the ordered `Queue<TilePosition>` state for the
  Version4 `ExploitDestroyQueue` member. Its public state/lifecycle surface is `Count`, `IsEmpty`,
  `Enqueue`, `TryDequeue`, and `Clear`; duplicate positions are preserved in enqueue order.
- The component is intentionally limited to state ownership. It does not implement the unresolved
  enqueue producer, `destroyObject` gate, tile framing, network publication, persistence, deduplication,
  or retry behavior. It also does not expose the Version4/XNA `Point` type.
- Dependency impact: only `Terraria.WorldSession` source was extended, reusing the existing
  `Terraria.WorldGeneration.Components.TilePosition`; no legacy API, registration key, WorldFile,
  network protocol, System, Command, Query, Adapter, Projection, test, or other partition document
  was changed.
- `implementationStatus` for this isolated unit is `implemented-isolated`; the queue producer, flush
  owner, effect ordering, duplicate/failure policy, and lifecycle window remain pending/blocking.
- Verification: not run for this new file. No test or focused verifier was added because this task
  forbids test changes; P16 overall `verificationStatus` remains `not-run`.

### C12 `WorldTerrainEffectsAndCaches` - WorldTerrainCoatingStateComponent

- Actual source saved before advancing:
  - `src/WorldSession/WorldGeneration/Terrain/WorldTerrainCoatingStateComponent.cs`
- `WorldTerrainCoatingStateComponent` owns current coating colors as copied `ColorRgba` values. Its
  state/lifecycle surface is `Count`, `Colors`, `Replace`, and `Clear`; callers cannot mutate the
  component through the exposed read-only wrapper.
- The component intentionally does not inspect tiles, implement `fullbright`/`invisible` predicates,
  convert to XNA `Color`, invoke paint effects, frame tiles, publish network state, or persist data.
  Those behaviors remain pending at the external coating owner/adapter seam.
- Dependency impact: only `Terraria.WorldSession` source was extended. No legacy API, registration
  key, WorldFile, network protocol, System, Command, Query, Adapter, Projection, test, or other
  partition document was changed.
- `implementationStatus` for this isolated unit is `implemented-isolated`; tile predicate, borrow
  lifetime, effect consumer, XNA adapter, and runtime parity remain pending. Verification is not run
  for this new file; P16 overall `verificationStatus` remains `not-run`.

### C03 `WorldLifecycleLoadAndTransformState` - isolated state components

- Actual source saved before advancing:
  - `src/WorldSession/WorldGeneration/WorldLoadLifecycleComponent.cs`
  - `src/WorldSession/WorldGeneration/WorldDimensionCompatibilityState.cs`
  - `src/WorldSession/WorldGeneration/WorldTransformTransactionComponent.cs`
- `WorldLoadLifecycleComponent` stores the four confirmed Version4 facts:
  `IsGeneratingOrLoadingWorld`, `LoadFailed`, `WorldCleared`, and `WorldBackup`. Its methods only
  update local state; they do not perform WorldFile I/O, backup copy/delete, clear coordination, or
  cross-thread publication.
- `WorldDimensionCompatibilityState` stores the previous tile allocation footprint
  `LastMaxTilesX`/`LastMaxTilesY`, preserves the zero-initialized no-previous-world state, and exposes
  explicit capture/clear operations. It does not replace current world bounds or invent persistence.
- `WorldTransformTransactionComponent` stores non-negative nested `ActiveCount`, monotonic `Revision`,
  and derived `IsTransforming`. `Begin`/`End` enforce local count pairing; scheduler, cancellation,
  I/O lock, `Interlocked` publication, and main-thread follow-up remain outside the component.
- Dependency impact: only three `Terraria.WorldSession` component files were added. No System, Query,
  Command, Adapter, Projection, test, project file, legacy writer, persistence format, network path,
  or other partition source was modified.
- These isolated units are `implemented-isolated`. WorldFile recovery, clear coordination, volatile
  guard compatibility, TransformingWorld query integration, and runtime behavior equivalence remain
  pending. No focused verifier was added, so P16 `verificationStatus` remains `not-run`.

### C04 `WorldLifecycleProgressionAndEventState` - committed progression facts

- Actual source saved before advancing:
  - `src/WorldSession/WorldProgression/WorldProgressionEventStateComponent.cs`
- `WorldProgressionEventStateComponent` stores the confirmed cross-tick Version4 facts
  `ShadowOrbSmashed`, `ShadowOrbCount`, and `AltarCount`; `Replace` rejects negative counts and
  `Reset` clears the local state for one world.
- The component does not duplicate spawn intents from `Calendar.PendingWorldEventStateComponent`,
  implement shadow-orb threshold effects, write NPC/achievement state, perform WorldFile Save/Load,
  publish network state, or acknowledge external effects. Those remain C04 owner and integration-review
  responsibilities.
- Dependency impact: only one `Terraria.WorldSession` progression component file was added. Existing
  Calendar state, System, Query, Command, Adapter, Projection, test, project, persistence, and network
  code were not modified.
- This isolated unit is `implemented-isolated`; event commands, concurrent snapshots, persistence/network
  round-trip, and effect failure policy remain unresolved. P16 `verificationStatus` remains `not-run`.

### C05 `WorldLifecycleHousingAndSpawnPacingState` - diagnostics, pacing and scoped state

- Actual source saved before advancing:
  - `src/WorldSession/WorldGeneration/WorldHousingBuildDiagnosticState.cs`
  - `src/WorldSession/WorldGeneration/WorldNpcSpawnPacingState.cs`
  - `src/WorldSession/WorldGeneration/WorldDropSuppressionContext.cs`
  - `src/WorldSession/WorldGeneration/WorldObjectDestructionContext.cs`
- `WorldHousingBuildDiagnosticState` stores accumulated missing-furniture and missing-light diagnostics
  and exposes explicit `Reset`; it does not write housing relations or diagnostic projections.
- `WorldNpcSpawnPacingState` stores `NpcSpawnDelay`/`NpcSpawnPeriod`, rejects negative values, and
  supports explicit replacement/reset; it does not advance time, spawn NPCs, or decide invasion/eclipse
  blocking.
- `WorldDropSuppressionContext` represents the local `stopDrops` scope with nesting depth, while
  `WorldObjectDestructionContext` represents `destroyObject` recursive admission with one active guard.
  Both only enforce local pairing/reset and perform no drops, Tile, object destruction, framing, network,
  or logging effects.
- Existing `src/WorldSession/WorldProgression/WorldInfectionPolicyStateComponent.cs` is a near-equivalent
  state type for `AllowedToSpreadInfections`; no duplicate component was created. Its owner reuse,
  creative-power input, and UpdateWorld phase remain in integration review.
- Dependency impact: only four `Terraria.WorldSession` component/context files were added. The existing
  infection state, System, Query, Command, Adapter, Projection, test, project, and other partition source
  were not modified.
- These isolated units are `implemented-isolated`; UpdateWorld scheduling, NPC/effect adapters,
  diagnostic projection, exception/cancellation call graph, and runtime parity remain unresolved.
  P16 `verificationStatus` remains `not-run`.

### C07 `WorldHousingCountersAndScoringState` - HousingScanBudgetDefinition

- Actual source saved before advancing: `src/WorldSession/WorldGeneration/Housing/HousingScanBudgetDefinition.cs`.
- `HousingScanBudgetDefinition` stores the two scan-budget inputs `MaxTileCount` and `MaxWallOut2`,
  rejecting zero and negative values. It does not read Tile, mutate counters, write NPC/TownManager,
  or perform recursive scanning.
- Dependency impact: only one `Terraria.WorldSession` housing Definition was added. No System, Query,
  Command, Adapter, Projection, test, project file, or existing `HousingScanStateComponent` aggregate
  was modified. The scan call graph, single writer for `maxWallOut2`, and runtime parity remain open;
  this isolated unit is `implemented-isolated` and P16 `verificationStatus` remains `not-run`.

### C07 `WorldHousingCountersAndScoringState` - HousingScanVisitedTilesContext

- Actual source saved before advancing: `src/WorldSession/WorldGeneration/Housing/HousingScanVisitedTilesContext.cs`.
- `HousingScanVisitedTilesContext` owns one `countTiles`/`nextCount` attempt's `TilePosition` de-duplication
  set, `NumTileCount` budget counter, and five material counters `LavaCount`, `IceCount`, `SandCount`,
  `RockCount`, and `ShroomCount`. `Clear` discards the scan state; `TryAdd` rejects an exhausted budget
  or duplicate position.
- It receives immutable `HousingScanBudgetDefinition`, does not read Tile/wall/liquid, recurse, mutate
  `WorldGen`/NPC/TownManager, or expose its set. The separate `countDirtTiles` context, exception/cancel
  lifetime, and snapshot close remain owner-review work.
- Dependency impact: only one housing context file was added. No existing aggregate component, System,
  Query, Command, Adapter, Projection, test, project file, or other partition source was modified. This
  unit is `implemented-isolated`; P16 `verificationStatus` remains `not-run`.

### C07 `WorldHousingCountersAndScoringState` - HousingMaterialCounterSnapshot

- Actual source saved before advancing: `src/WorldSession/WorldGeneration/Housing/HousingMaterialCounterSnapshot.cs`.
- `HousingMaterialCounterSnapshot` is the immutable close-out snapshot for one housing scan. It stores the five
  material counts `LavaCount`, `IceCount`, `SandCount`, `RockCount`, and `ShroomCount`, and rejects negative
  inputs. It does not reuse or clear the next scan context and has no Tile, WorldGen, NPC, TownManager,
  persistence, or network effects.
- Dependency impact: only one immutable housing snapshot type was added. No scan context, legacy
  `HousingScanStateComponent` aggregate, System, Query, Command, Adapter, Projection, test, project file, or
  other partition source was modified. The snapshot builder, counting order, exception/cancel close, and
  generation-pass consumer remain owner-review work. This unit is `implemented-isolated`; P16
  `verificationStatus` remains `not-run`.

### C07 `WorldHousingCountersAndScoringState` - HousingRoomVisitedTilesContext

- Actual source saved before advancing: `src/WorldSession/WorldGeneration/Housing/HousingRoomVisitedTilesContext.cs`.
- `HousingRoomVisitedTilesContext` stores one room-check attempt's `MaxRoomTiles`, `MaxRoomSize`, de-duplicated
  `TilePosition` set, `NumRoomTiles`, and `RoomX1`/`RoomX2`/`RoomY1`/`RoomY2` bounds. `TryAdd` rejects
  duplicate points, an exhausted tile budget, or a point that would reach the room-size span limit; `Clear`
  clears the visited set and counters and restores the start-position bounds.
- It does not read Tile/wall, perform flood fill, publish feedback, evaluate furniture eligibility, or mutate
  NPC/TownManager state, and it does not expose a mutable collection. Finally-based exception/cancel lifetime,
  open-gate/solid-wall/too-small/too-big precedence, and exact Version4 `BitSet2D` mapping remain with the
  `HousingRoomSearchSystem` owner. This unit is `implemented-isolated`; P16 `verificationStatus` remains
  `not-run`.

### C07 `WorldHousingCountersAndScoringState` - HousingRoomScoreSnapshot

- Actual source saved before advancing: `src/WorldSession/WorldGeneration/Housing/HousingRoomScoreSnapshot.cs`.
- `HousingRoomScoreSnapshot` is an immutable room-score result. It stores `NumRoomTiles`, permits the Version4
  `hiScore = -1` occupancy short-circuit through `HighScore`, stores paired optional `BestX`/`BestY` candidate
  coordinates, and stores the optional `SharedRoomX` outcome; a one-sided candidate coordinate is rejected.
- It does not score rooms, query occupancy, call feedback, write NPC home state, or produce
  TownManager/network/progression effects, nor does it place score scratch in persistence/network projections.
  The single score writer, pollution penalty, tie policy, and candidate publication remain owner-review work.
  This unit is `implemented-isolated`; P16 `verificationStatus` remains `not-run`.

### C07 `WorldHousingCountersAndScoringState` - HousingAssignmentPriorityState

- Actual source saved before advancing: `src/WorldSession/WorldGeneration/Housing/HousingAssignmentPriorityState.cs`.
- `HousingAssignmentPriorityState` stores only the optional `PrioritizedTownNpcType` intent for one housing
  decision window. Replacement rejects negative values and `Clear` represents no priority. It does not own an
  NPC entity, TownManager key, housing relation, or network/progression effect.
- Dependency impact: only one housing state type was added. Existing `TownHousingResidentKey`,
  `TownHousingAssignmentComponent`, and `WorldEcologyScheduleState` were not modified, and no C08 relation
  commit was added. NPC type semantics, key-mode mapping, assignment command, and the owner remain
  `crossSubsystemOwner: integration-review`. This unit is `implemented-isolated`; P16
  `verificationStatus` remains `not-run`.

### C08 `WorldHousingRoomSearchState` - HousingRoomTileClassificationContext

- Actual source saved before advancing: `src/WorldSession/WorldGeneration/Housing/HousingRoomTileClassificationContext.cs`.
- `HousingRoomTileClassificationContext` owns the `houseTile`-style tile-type classification set in a fixed-size boolean
  array and tracks `TileTypeCount` and `ClassifiedTileTypeCount`. `Contains` and `Mark` validate the configured tile-type
  range; `Mark` de-duplicates classifications, and `Clear` resets the set and count.
- The component does not read Tile state, perform housing eligibility, implement room search, or publish diagnostic
  projections. It has no NPC, TownManager, network, or persistence effects. Tile-type sources, scan call graph, and
  `houseTile` lifetime remain with the pending C08 search owner.
- Dependency impact: only the housing component file was added. No System, Query, Command, Adapter, Projection, test,
  project file, or existing housing aggregate was modified. This isolated unit is `implemented-isolated`; complete room
  search, eligibility query, and runtime parity remain unverified.

### C08 `WorldHousingRoomSearchState` - HousingRoomSearchContext

- Actual source saved before advancing: `src/WorldSession/WorldGeneration/Housing/HousingRoomSearchContext.cs`.
- `HousingRoomSearchContext` organizes one room-check attempt's short-lived state. It creates and owns
  `HousingRoomVisitedTilesContext`, `HousingRoomTileClassificationContext`, and `HousingRoomDiagnosticFlags`, and stores
  `LastFoundHouse`, `IsTryingAlternateHousingSpot`, `SharedRoomX`, plus a recursive `TilePosition` check stack.
- `PushRoomCheck`/`TryPopRoomCheck` expose only stack state; `Clear` resets the stack, candidate fields, and three specialized
  child contexts. The type does not read Tile state, perform flood-fill, calculate RoomNeeds, commit scoring/housing
  relations, or publish diagnostic/network/persistence effects.
- Dependency impact: only the housing context file was added. No System, Query, Command, Adapter, Projection, test, project
  file, or other partition source was modified. This isolated unit is `implemented-isolated`; the search owner,
  failure-reason projection, alternate-spot scope, and runtime parity remain unverified.

### C08 `WorldHousingRoomSearchState` - HousingRoomRequirementResult

- Actual source saved before advancing: `src/WorldSession/WorldGeneration/Housing/HousingRoomRequirementResult.cs`.
- `HousingRoomRequirementResult` is the immutable output of one room-eligibility decision. It stores the five boolean
  results in Version4 order: `HasTorch`, `HasDoor`, `HasChair`, `HasTable`, and `CanSpawn`; `false` means that the
  requirement is not satisfied and does not mean that scanning has not occurred.
- The result type does not read `houseTile`, execute `RoomNeeds` rules, write legacy flags, or produce NPC, TownManager,
  network, or persistence effects. Requirement registries, query logic, and legacy-facade compatibility writes remain
  pending C08 Query/System work.
- Dependency impact: only the housing result file was added. No System, Query, Command, Adapter, Projection, test,
  project file, or other partition source was modified. This isolated unit is `implemented-isolated`; P16
  `verificationStatus` remains `not-run`.

### C08 `WorldHousingRoomSearchState` - HousingRoomDiagnosticFlags

- Actual source saved before advancing: `src/WorldSession/WorldGeneration/Housing/HousingRoomDiagnosticFlags.cs`.
- `HousingRoomDiagnosticFlags` owns `HasStinkbug` and `HasEchoStinkbug`. `MarkStinkbug` and `MarkEchoStinkbug` set one
  flag at a time, `Replace` replaces both values, and `Clear` resets both flags to `false`.
- The component does not emit a diagnostic projection, affect spawning, read NPC/TownManager state, or perform room search,
  feedback, network, or persistence effects. Flag production conditions, consumers, and failure-reason projection remain
  pending C08 owner and external-adapter work.
- Dependency impact: only the housing component file was added. No System, Query, Command, Adapter, Projection, test,
  project file, or other partition source was modified. This isolated unit is `implemented-isolated`; P16
  `verificationStatus` remains `not-run`.

### Verification checkpoint - isolated P16 units

- Affected project build:
  `pwsh -NoProfile -Command "& '.\Build\Tools\Invoke-SerialDotnet.ps1' -DotnetArguments @('build', '.\src\WorldSession\Terraria.WorldSession.csproj', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')"`
  latest run completed with exit code `0`, `0` warnings, and `0` errors. Artifact:
  `Build/bin/Terraria.WorldSession/Debug/net10.0/Terraria.WorldSession.dll`.
- P16 fossil focused verifier build:
  `pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 build .\src\WorldTerrainC12FocusedVerifier\Terraria.WorldTerrainC12FocusedVerifier.csproj -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false`
  completed with exit code `0`, `0` warnings, and `0` errors. Artifact:
  `Build/bin/WorldTerrainC12FocusedVerifier/Debug/net10.0/WorldTerrainC12FocusedVerifier.dll`.
- P16 fossil focused verifier run:
  `pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 run --project .\src\WorldTerrainC12FocusedVerifier\Terraria.WorldTerrainC12FocusedVerifier.csproj --no-build --no-restore -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false`
  completed with exit code `0` and printed `WorldFossilShatterScope C12 focused verifier passed.`
- P16 storm safe-spot focused verifier uses the same isolated project. Its build completed with exit
  code `0`, `0` warnings, and `0` errors; artifact:
  `Build/bin/WorldTerrainC12FocusedVerifier/Debug/net10.0/WorldTerrainC12FocusedVerifier.dll`.
  The no-build run completed with exit code `0` and printed
  `WorldStormSafeSpotScratch C12 focused verifier passed.`
- P16 boulder state focused verifier uses the same isolated project. Its build completed with exit code
  `0`, `0` warnings, and `0` errors; the no-build run completed with exit code `0` and printed
  `WorldBoulderRainState C12 focused verifier passed.` The artifact is
  `Build/bin/WorldTerrainC12FocusedVerifier/Debug/net10.0/WorldTerrainC12FocusedVerifier.dll`.
- P16 TreeTops state focused verifier uses the same isolated project. Its build completed with exit
  code `0`, `0` warnings, and `0` errors; the no-build run completed with exit code `0` and printed
  `WorldTreeTopsStateComponent C12 focused verifier passed.` The artifact is
  `Build/bin/WorldTerrainC12FocusedVerifier/Debug/net10.0/WorldTerrainC12FocusedVerifier.dll`.
- P16 background flash cache focused verifier uses the same isolated project. Its build completed
  with exit code `0`, `0` warnings, and `0` errors; the no-build run completed with exit code `0` and
  printed `WorldBackgroundFlashCacheProjection C12 focused verifier passed.` The artifact is
  `Build/bin/WorldTerrainC12FocusedVerifier/Debug/net10.0/WorldTerrainC12FocusedVerifier.dll`.
- Existing regression verifier:
  `pwsh -NoProfile -Command "& '.\Build\Tools\Invoke-SerialDotnet.ps1' -DotnetArguments @('run', '--project', '.\Test\Terraria.WorldSession.WorldGeneration.Verification\Terraria.WorldSession.WorldGeneration.Verification.csproj', '--no-build', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')"`
  latest attempt ran without compiler output for approximately three minutes while its verifier
  process remained CPU-active, so the session owner interrupted it; the attempt ended with exit code
  `1` and did not produce a verifier diagnostic. A prior recorded attempt failed at
  `Test/Terraria.WorldSession.WorldGeneration.Verification/Program.cs:380` on the pre-existing
  `WorldLayerMetrics` state-preservation assertion. This verifier does not cover the new P16 types and
  does not close the P16 full runtime, persistence/network, or behavior-equivalence verification gate.

## 10. Terminal Checkpoint (2026-09-12)

- The valid session for this checkpoint is `e9e33a27331a40d3a159985a54bbf8eb`. The older
  `1b3b5585d92d44b4ab883e4ad66332dd` in earlier document text is historical and must not be used
  for runner settlement.
- The C12 component boundary review is complete. `WorldTerrainTileOverrideScope`,
  `WorldHardmodeTilePolicyState`, `WorldGrassSpreadState`, `CrimsonHeartPlacementScratch`,
  `WorldTerrainRuleDefinition`, `WorldFossilShatterScope`, `WorldBoulderRainState`,
  `WorldStormSafeSpotScratch`, `WorldTreeTopsStateComponent`,
  `WorldBackgroundFlashCacheProjection`, `WorldTerrainDestructionQueueComponent`, and
  `WorldTerrainCoatingStateComponent` are recorded as isolated entries in `completedComponents`.
- Source search across `src/WorldSession` and the existing verifier found no confirmed consumer for
  `bitStrip` or `_preventInfiniteRopeFraming`. Queue search found only the isolated component's own
  `Enqueue` method and no external producer. No duplicate component, giant aggregate, or guessed
  frame/queue owner was added.
- No `src/` source was added or modified at this checkpoint. Remaining C12 work is a producer/consumer,
  effect-owner, WorldFile/NetMessage adapter, XNA adapter, or runtime-authority boundary and is outside
  the safe component-only scope for this session.
- Verification results:
  - `pwsh -NoProfile -Command "& '.\Build\Tools\Invoke-SerialDotnet.ps1' -DotnetArguments @('build', '.\src\WorldSession\Terraria.WorldSession.csproj', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')"`: exit code `0`, `0` warnings, `0` errors; artifact `Build/bin/Terraria.WorldSession/Debug/net10.0/Terraria.WorldSession.dll`.
  - `pwsh -NoProfile -Command "& '.\Build\Tools\Invoke-SerialDotnet.ps1' -DotnetArguments @('build', '.\src\WorldTerrainC12FocusedVerifier\Terraria.WorldTerrainC12FocusedVerifier.csproj', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')"`: exit code `0`, `0` warnings, `0` errors; artifact `Build/bin/Terraria.WorldTerrainC12FocusedVerifier/Debug/net10.0/Terraria.WorldTerrainC12FocusedVerifier.dll`.
  - The verifier was run serially with `run --project ... --no-build --no-restore`, exit code `0`. It reported passing fossil scope, boulder state, TreeTops state, background flash cache, storm safe-spot scratch, and meteor geometry scenarios.
- These are isolated-slice results and do not prove complete P16 runtime migration, persistence/network
  compatibility, or behavior equivalence. The partition terminal state is therefore
  `executionStatus: failed`, `implementationStatus: partial`, and `verificationStatus: partially-verified`;
  the failure is due to remaining work requiring unresolved external owners/adapters/systems/queries/
  projections and the unclosed runtime call graph.
