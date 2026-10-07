# System Decomposition Report: authoritative P16

- partitionId: P16
- sessionId: b65120e249874094ac5069b99b2fb9c7
- taskSet: authoritative-system-decomposition
- inputReport: docs/migration/ledgers/authoritative-20-partitions/P16-World-Lifecycle-Housing-Metrics.md
- prompt: docs/component-decomposition/review-round-1/design/2026-09-11-version4-authoritative-20-partition-prompts/2026-09-11-version4-P16-world-lifecycle-housing-metrics-public-decomposition.md
- outputReport: docs/system-decomposition/reports/2026-09-18-system-decomposition-authoritative-P16-world-lifecycle-housing-metrics.md
- designStatus: proposed
- verificationStatus: not-run
- sourceModified: true

## Scope and Evidence

本报告只处理 authoritative P16 的 13 个叶子组、130 个字段和 3 个属性（共 133 项）。输入清单是成员范围的权威来源；它确认这些成员声明在 Version4 源树中，不证明完整读写闭包或现有 NSSLC 行为等价。下表逐组覆盖 claim 清单中的全部成员：

| Input group | Claimed members | Proposed behavior family / initial evidence |
|---|---|---|
| WorldGenBiomeBackgroundAndDistanceMetrics | TownManager, Manifest, tileReframeCount, treeBG1, treeBG2, treeBG3, treeBG4, corruptBG, jungleBG, snowBG, hallowBG, crimsonBG, desertBG, oceanBG, mushroomBG, underworldBG, oceanDistance, beachDistance, shimmerSafetyDistance, crimson, generatingRandomEvil | 世界环境/生物群系度量；TownManager、清单与随机感染状态不是同一度量不变量。 |
| WorldGenTileCountMetrics | tileCounts, totalEvil, totalBlood, totalGood, totalSolid, totalEvil2, totalBlood2, totalGood2, totalSolid2, tEvil, tBlood, tGood, totalX, totalD | 周期性分列扫描、累计值、百分比快照和网络通知。 |
| WorldLifecycleLoadAndTransformState | _transformingWorld, isGeneratingOrLoadingWorld, loadFailed, worldCleared, worldBackup, lastMaxTilesX, lastMaxTilesY | 加载/生成/变换屏障状态；文件、存档与世界存储协调未闭合。 |
| WorldLifecycleProgressionAndEventState | spawnEye, spawnHardBoss, shadowOrbSmashed, shadowOrbCount, altarCount, spawnMeteor | 进度/事件事实；写入者、消费顺序和最终 owner 需交 integration review。 |
| WorldLifecycleHousingAndSpawnPacingState | builtHouseWithNoFurniture, builtHouseWithNoLight, stopDrops, AllowedToSpreadInfections, destroyObject, npcSpawnDelay, npcSpawnPeriod | 住房结果、生成节奏和扩散/破坏策略混合；需按不变量拆分。 |
| WorldLifecycleTileMergeState | mergeUp, mergeDown, mergeLeft, mergeRight | Tile 邻接合并策略状态；Tile mutation/framing 的写入闭包需确认。 |
| WorldHousingCountersAndScoringState | prioritizedTownNPCType, numTileCount, maxTileCount, maxWallOut2, CountedTiles, lavaCount, iceCount, sandCount, rockCount, shroomCount, maxRoomTiles, maxRoomSize, roomTiles, numRoomTiles, hiScore | 住房扫描/评分暂存与优先 NPC 节奏；候选搜索的中间量不应作为 NPC 权威状态。 |
| WorldHousingRoomSearchState | roomX1, roomX2, roomY1, roomY2, canSpawn, houseTile, bestX, bestY, roomTorch, roomDoor, roomChair, roomTable, roomHasStinkbug, roomHasEchoStinkbug, LastFoundHouse, currentlyTryingToUseAlternateHousingSpot, sharedRoomX, _roomCheckStack, roomCheckFailureReason | 单次房间检查上下文、规则结果、反馈和候选坐标；实际路径具有共享可变状态。 |
| WorldHousingRuleAndDiagnosticState | WorldGenParam_Evil, cactusWaterWidth, cactusWaterHeight, cactusWaterLimit, mysticLogsEvent | 配置、植被规则和事件开关，不属于一个住房规则组件。 |
| WorldGenerationDimensionsState | meteorShowerCount, WorldSizeSmallX, WorldSizeSmallY, WorldSizeMediumX, WorldSizeMediumY, WorldSizeLargeX, WorldSizeLargeY, InfectionAndGrassSpreadOuterWorldBuffer | 尺寸目录/边界规则与流星计数异质；世界尺寸执行权归属待确认。 |
| WorldGenerationScratchState | trapDiag, gem, mossType, neonMossType | 生成 pass 的临时 scratch；生命周期与并发隔离未知，不建议仅凭字段新增持久 Component。 |
| WorldTerrainEffectsAndCaches | tileSolidBackup, ItemSpawnProtectionTime, _coatingColors, catTailDistance, TreeTops, BackgroundsCache, fossilBreak, ExploitDestroyQueue, hardModeWorldUpdates, growGrassUnderground, _isRainingBoulders, _SpawnThunderStorm_SafeSpots, BUBBLES_SOLID_STATE_FOR_HOUSING, grassSpread, heartPos, heartCount, strip_w, strip_h, bitStrip, _preventInfiniteRopeFraming | 持久化树冠样式、表现缓存、生成 scratch、天气/掉落和 Tile 规则混合；不能合并为通用环境 Component。 |
| WorldGenDerivedProperties | TransformingWorld, genRand, oceanLevel | 派生计数、共享随机流和纯数值派生三种不同读取语义。 |

Version4 源码树为 D:\TRbackup\Version4；没有记录该工作树的 Git commit 或全树 snapshot ID。只读 CPG 数据库 D:\TRbackup\Version4-cpg-export\out-dop8-interproc.sqlite 的 manifest SHA-256 为 6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364，project fingerprint 为 521CBD4C11E7EB731256C232350A54099F431D0E59EBDF3C38880281F169C11B，导入状态 complete、967 shards。使用 Find-CpgSymbols、Get-CpgTypeSurface -MaxItems 1000 核对后，Terraria.WorldGen direct surface 为 774 项，P16 清单 133 项全匹配，缺项 0，查询状态 complete。

**调用和使用查询。** CPG Find-CpgCallSites 在显式选择的 Terraria/WorldGen.cs、Terraria/Main.cs、Terraria/NPC.cs、Terraria.IO/WorldFile.cs、Terraria/NetMessage.cs、Terraria/Projectile.cs shards 中返回：StartRoomCheck 6 个、QuickFindHome 3 个、UpdateWorld 2 个、StartHardmode 1 个、TransformWorldOnBackgroundThread 2 个 call-site，查询状态 complete、未返回 gap。CountTiles 在所选范围命中 UpdateWorld 内 1 个调用点。Get-CpgCallableFacts 对这些方法为 partial，并含 CalleeEffectsNotExpanded；多项 Get-CpgMemberUses 虽查询完成，单个 fact 仍为 partial，AccessMode 有 Unknown，且 alias/callee effects 未展开。因此这些结果只确认索引范围内的端点和静态边，不代表所有入口、写入或运行时效果闭合。SetWorldSize 的受限调用查询为 partial，故补查 Version4 的 WorldFileData.SetWorldSize 与 WorldGen.SetWorldSize 源码。

CPG artifact 没有 SourceSnapshotId 或逐文件源码 hash；以上 CPG 事实不能证明行号绑定到现有源码快照。关键关系均回到当前 Version4 源码检查；源码行号在下文仅作定位。缺失的动态目标、接口回调闭包、多世界运行时入口和异常恢复一律记 partial / unknown。

初稿代码检查范围为 src/NSSLC/Component/WorldSession/WorldGeneration。该树已有生命周期、住房、注册表、TreeTops 和 metrics 数据类型；可见一个实际局部实现 WorldLayerMetricsSystem 及相应 Query。初稿搜索未找到 P16 类型被加载器、主循环或完整 Version4 adapter 调用的证据。后续执行已将世界加载 lifecycle 接入 WorldLoadRecoveryCoordinator，并从 WorldStorageCoordinatorFactory 注册 API catalog 和 LegacyWorldLoadRecoveryEffects。当前另有 `NSSLC.Tools.Simulation` headless finite-simulation composition root 调用注册 API 并物化其有限内容目录支持的 NPC；其 runtime NPC store 现保留存档 GivenName、housing relation 与 variation，但旧版 NPC type-name resolver 仍仅覆盖有限目录中的 Guide、Old Man、Zombie。此 caller 证明候选 session 在该有限宿主中的加载路径已接通，不证明完整游戏/server host 接线或旧行为等价。当前 `src` 下只有 WorldGeneration 与 Simulation 两个可执行工具入口，未发现完整游戏/server composition root。Bestiary 持久击杀数/发现 ID 现有 runtime snapshot owner，但 gameplay tracking 与网络/成就效果未接线；TileEntity 现由独立 storage adapter 在受 gate 的发布阶段将 type code 0–10 物化为 runtime 子类型并恢复已解码字段，placement hook 也可注册对应子类型；物化后还会按记录 ID 顺序派发 `OnWorldLoaded()`。当前这些实体的 Version4 callback 实现为空。Simulation 的假人/逻辑传感器有限更新会投影回 runtime；保存时在共享 I/O gate 内将 runtime TE snapshot 提交给 session owner，再由 session snapshot source 编码。完整 `PerformUpdates` 行为及逐次 runtime mutation 的领域提交仍未闭合。

参考项目 C:\Users\shan\Downloads\ECS\space-station-14-master 的 GridPreloaderComponent / GridPreloaderSystem 展示了将驻留列表放在 map entity 的 Component、由 System 接收 map lifecycle 事件并操作外部 map service 的结构；AtmosphereSystem.Processing 展示 System 管理有游标且可延后处理的批次。这些仅用于比较状态与执行边界，不能推断 Terraria 的生命周期、调度器、异步或住房语义。

## Prior Component Decomposition Reconciliation

- P16 专属 public-decomposition prompt 指定的历史产物 docs/component-decomposition/review-round-1/2026-09-11-version4-P16-world-lifecycle-housing-metrics-public-decomposition.md 不存在于预期路径；其具体 owner 决策因此为 evidence-gap，本报告不把旧分区标题当作已确认设计。
- 输入 inventory 的 13 组用于稳定限定成员，不是 13 个 Components 或 Systems 的直接方案。特别是 WorldGenBiomeBackgroundAndDistanceMetrics 同时含 TownManager、Manifest、生物群系背景和距离；WorldGenerationDimensionsState 同时含 meteorShowerCount 与尺寸目录；WorldTerrainEffectsAndCaches 混合存档状态、网络投影、事件临时数据和函数 scratch。这些组需按不变量再分。
- Component 的字段聚类仍有参考价值：住房搜索有可识别的临时上下文；Tile 计数有扫描游标和累计窗口；世界载入标记有生命周期。但实际 owner 要以写入者、完成提交和外部持久化/网络路径决定，而不是沿用 Component 名称。
- src/NSSLC 已有 WorldLoadLifecycleComponent、WorldGenerationLifecycleState、HousingScanStateComponent、TownHousingRegistryComponent、WorldTreeTopsStateComponent、WorldLayerMetricsComponent 等。初稿只确认 WorldLayerMetricsSystem 的 generation 匹配与 commit 为本模块局部代码；后续补入 lifecycle recovery coordinator、TreeTops load API catalog、旧运行时 projection 和 headless simulation host registration。该 registration 属有限范围的 host integration，不代表完整游戏宿主已采用新路径。住房最终 owner 仍需 integration-review；TownHousingRegistryComponent 的 status: proposed 与 crossSubsystemOwner: integration-review 保持适用。

## Conceptual Behaviors

| Behavior slice | Invariant / observable result | State and effects | Evidence |
|---|---|---|---|
| Load, settle and publish world readiness | 不允许在受门禁的加载/生成阶段运行通常的世界更新；成功读档还包含液体 settle、水检查及多个 NPC/天气收尾 | 全局 load flag、load failure、世界数据、液体状态、状态文本、NPC/世界初始化；失败和重入收尾未闭合 | WorldFile.LoadWorld 在文件解析/修复之后、settle 之前设置 isGeneratingOrLoadingWorld，settle 后才清除；WorldGen.UpdateWorld 与 Main 另有检查。WorldFile.cs:750, 781; WorldGen.cs:59400; Main.cs:11265 |
| Serialize against terrain transformation | 写档需要等正在运行的异步变换并取得相同 I/O 锁 | Atomic transform count、ThreadPool work、WorldFile.IOLock、主线程回调和存档字节 | WorldGen.TransformWorldOnBackgroundThread 在锁内调用 delegate，finally 减计数并入队 follow-up；WorldFile._SaveWorld 等计数归零后 TryEnter(IOLock)。WorldGen.cs:26103; WorldFile.cs:905 |
| Find, validate and assign NPC housing | 候选房间需满足扫描边界、房间规则、评分、特殊 NPC 资格和互住/占用条件；通过后才更新 NPC home/homeless 状态 | 共享搜索 scratch、读 Tile、NPC 写入、成就/反馈副作用、读取 TownRoomManager；住房结果不是纯 Query | QuickFindHome -> StartRoomCheck / RoomNeeds / ScoreRoom; WorldGen.cs:5310, 5361, 5699; 调用点包含 Main、NPC、WorldFile |
| Persist and rebuild housing registry | NPC 类型到房间的映射与快速 has-room 状态一致，读档时要与实际 tile 房间复核 | TownRoomManager 有锁、Save/Load/Clear；载入验证可移位 NPC 并驱逐 | TownRoomManager.cs:65, 102, 118; WorldFile.cs:3476, 3484, 1938 |
| Incremental tile/environment metrics | 每 30 次 world update 推进一列；完整扫描周期更新比例、重置累计并发送网络通知 | 横向游标、分层 tile 计数、biome totals、百分比、Skyblock 观察值、NetMessage.SendData(57) | WorldGen.UpdateWorld 调 CountTiles；CountTiles 在 X == 0 发布百分比并清累计。WorldGen.cs:59419, 59085, 59110 |
| World update orchestration and pacing | Main 的 world update 入口必须保留既有调用位置、次数和随机调用顺序 | 住房优先级、扩散状态、Wiring、TileEntity、Lunar、metrics、Liquid、天气与随机地形更新 | 两个 Main call-sites；Main.cs:11593, 11602; 顺序始于 WorldGen.cs:59408，余下效果跨多个系统 |
| Profile, dimension and environment state | 尺寸/距离/背景/TreeTops 派生及更新需与 world identity、tile 空间和序列化/net snapshot 对应 | Main.maxTiles*、随机流、背景/地形缓存、13 区域 TreeTops 状态、世界存档与网络字节 | WorldGen.SetWorldSize；TreeTopsInfo.Save/Load/SyncSend；WorldFile.cs:1423; NetMessage.cs:276 |

## State Ownership and Write Closure

| State family | Proposed authority and write closure | Direct readers / collaborators | Status and owner boundary |
|---|---|---|---|
| Load / transform / backup / clear flags | 由单个 world-session lifecycle owner 转换；WorldFile 作为读写 adapter 提交 parse/settle outcome；transform service 在开始/完成/失败点更新计数或状态。内部 phase 可以更早记录 load attempt，但 legacy gate 的兼容投影保持旧可观察时点。不要由 Query 修正生命周期。 | Main, WorldGen, WorldFile, Liquid settling, WorldFile.IOLock, main-thread callback queue | 直接 load 与 transform 写入已确认；失败后是否 reset load flag、world reset/clear 是否清所有字段为 unknown。crossSubsystemOwner: integration-review 处理世界存储、异步 host 和 session scope。 |
| Housing validation scratch and result | HousingValidationSystem 独占一次 Search/Score/commit 流；坐标、tile mask、评分游标放在明确的 call-scoped context。对外 Query 可读取不可变 RoomEvaluationResult；Query 不触发扫描副作用。NPC 住房字段只能由授权 assignment command/adapter 提交。 | NPC, Main, WorldFile 的验证/修复路径，Tile grid、住房规则、feedback、achievement、Town registry | 共享 scratch 与 NPC 结果提交在源码中可见；重入/并发与 interface feedback 动态目标为 partial。NPC housing 与 WorldGen 字段间的最终写 owner：crossSubsystemOwner: integration-review。 |
| Town room registry | 单独的 session-scoped registry owner 保存 resident key -> room、占用状态及 revision；持久化 adapter 做版本解码/写入，恢复验证走 HousingValidationSystem 后发 assignment command。 | WorldFile.SaveTownManager/LoadTownManager, WorldGen, NPC 查询与互住规则 | Version4 有按 npc type 索引的 room list 与快速 has-room flags；SetRoom(Point)、KickOut(int)、Save 对 list 使用 EntityCreationLock，但快速 flag 写入及 Load/Clear 不都在锁内，bounds/error 处理也未闭合；NSSLC resident identity/key mode 尚 unresolved。不得宣告新 registry 为完整 owner。 |
| Tile counts / biome metrics | 将扫描 cursor 与累计窗口放在独立 WorldTileMetricsSystem 的 world-scoped state；完整周期形成不可变 metrics snapshot；网络 message 通过 Projection/adapter 发出；Skyblock 写入经独立 observation port。 | WorldGen.UpdateWorld, Tile map, Skyblock, world biome/UI/network consumers | 30-tick cadence、X wrap、X=0 reset/send 在源码确认；具体计数数组算法细节、零总量、跨世界重置与消费端完整列表未闭合。Skyblock state 和 network schema：crossSubsystemOwner: integration-review。 |
| Background / biome / derived reads | 只读派生值由 Query；受 Main 或视觉系统维护的缓存保持原 owner，经版本化 snapshot 或 read API 暴露。genRand 需走 shared random port，不当纯 Query。 | Main/world settings, background draw/cache, WorldGen, random service, visual consumers | oceanLevel 算式可建纯 Query；TransformingWorld 读并发计数；genRand 返回共享可变随机源。BackgroundsCache owner/失效规则为 unknown。 |
| Progression, event, tile merge and terrain effects | 不给混合 P16 字段集中指定一个 WorldSystem。Progression flags、weather/event flags、Tile merge、tree style、exploit queue、spawn protection、temporary flood-fill scratch 各由对应 capability owner 接收。 | NPC/WorldProgression、Tile/Wiring、weather, item protection, Terraria WorldFile, NetMessage | P16 只标注接缝；上述跨域字段必须 crossSubsystemOwner: integration-review。完整静态写入闭包未查完。 |
| Dimensions, rules and scratch | 尺寸 catalog/派生边界按只读定义与 explicit SetWorldSize Command 拆分；无持久身份的 scratch 保留在单个生成 pass 调用局部，需跨 tick/恢复时才建 Component。 | WorldFileData, Main.maxTilesX/Y, generator passes, infection/grass spread | 三种尺寸数值在源码可见；尺寸分配的完整消费、地图分块和恢复行为 partial。meteor counter、evil seed rule 与尺寸不是同一 owner。 |

共享/间接写入风险。QuickFindHome 临时写 Main.tileSolid[379]，然后恢复旧值，但未见 finally；异常是否泄漏变化需要失败路径验证。StartRoomCheck 接受 IRoomCheckFeedback，该接口调用可能进入 UI/粒子实现，不能根据 NoRoomCheckFeedback 的空实现推断其他实现无效果。CountTiles 对 Tile 缺失项会创建 Tile 并写 Skyblock sets，这不只是读取度量。WorldFile.LoadWorld 在 settle 区间打开 load flag，内部 catch 设 loadFailed；该 catch 路径是否总有统一 unload/recovery 清理未知。

## Boundary Role and Decision

| Proposed capability | Decision | Component / System / Query / Command / Adapter roles | Why / rejected alternative |
|---|---|---|---|
| World session load/transform lifecycle | separate | 一份 world-scoped lifecycle Component；WorldSessionLifecycleSystem 唯一推进状态；WorldLoadCommand / BeginTransformCommand 代表意图；Storage/Lock/ThreadPool/MainThreadQueue 经 Adapter/ports 接入；状态快照 Query 只读。 | 加载门禁、并发计数、失败和保存屏障有共同 lifecycle/invariant。拒绝把这些旗标分散给 Main、WorldFile 和住房/metrics Systems 写。必须先明确线程模型与失败复位。 |
| Housing search and validation | separate | HousingScanContext 为调用局部状态，验证和候选评分在 HousingValidationSystem；FindHome 返回结果；AssignHomeCommand 提交 NPC 字段；feedback、achievement 与 Tile access 通过显式结果/ports；RoomRequirementsQuery 只接受不可变输入。 | RoomNeeds/StartRoomCheck 写共享变量，QuickFindHome 改 NPC 和 tile solidity 并发出效果，不能标成纯 Query。拒绝“一次扫描就是纯 Query”或把 registry、NPC 状态和诊断集中到 housing 巨型 Component。 |
| Town housing registry | separate, owner proposed | Registry Component + TownHousingRegistrySystem 管唯一映射/快速索引；读取由 Query；存档版本由 persistence Adapter 负责。 | TownRoomManager 有独立持久化数据与锁，不等同扫描 scratch。最终 owner/key contract 归 crossSubsystemOwner: integration-review，未确认前仅提出 seam。 |
| Tile/biome counters | separate | world-scoped metrics Component 保存游标/累积；WorldTileMetricsSystem 以明确 cadence 扫描；metrics Query 返回 snapshot；NetMessage 与 Skyblock 走 Projection/Adapter。 | 它有 30 次更新 cadence、累计窗口、X=0 提交和网络副作用，不应在 read-only Query 或普通 HUD projection 里推进。拒绝把 counter 并入 WorldUpdateSystem 的未区分共享状态。 |
| World update host order | 对既有 orchestrator partial | 保持一个显式 WorldUpdate composition；将已经识别的 capability 调用委托给 Systems/ports；只有真实 scheduler registration 与隔离测试完成后才新增调度节点。 | 文件顺序不是调度合同。UpdateWorld 混合 wiring/liquid/weather/random/tile 更新，P16 不拥有全部行为。拒绝按本分区字段建一个平行 UpdateWorld scheduler。 |
| TreeTops persistent style | separate | 区域样式 Component/State；System 修改样式；Persistence Adapter 处理旧版本 (<211) 和当前格式；network Projection 保持 13 byte 协议。 | 有明确 13-area state、save/load 版本分支和网络写出，生命周期不同于 scratch。旧数据复制规则与所有写入点需补验证。 |
| Mixed effects/caches, merge, dimensions, progression | partial / keep in true owning capability | Tile merge 与 tile framing 同属 tile mutation owner；weather/spawn/progression 缓存分别保留其既有域；尺寸定义用 Query + explicit Command；随机/临时数组按 invocation scope；每个 snapshot/net/save adapter 做边界工作。 | P16 的源 inventory 子组不能证明单一 owner。拒绝 WorldGenerationEnvironmentSystem 总括组和以 field list 直接加 Component。各最终 owner crossSubsystemOwner: integration-review。 |

## System API and Legacy Behavior Mapping

| Legacy entry / behavior | Proposed API composition | Compatibility contract and unresolved behavior |
|---|---|---|
| WorldFile.LoadWorld 对 isGeneratingOrLoadingWorld / loadFailed 的读写 | WorldSessionLifecycleSystem 记录 load attempt -> persistence adapter decode/repair -> 在旧时点投影 legacy load gate -> liquid-settle command/adapter -> validation/finalize -> MarkReady 或 MarkLoadFailed | 保持标记可见时点、生成/载入期间的 UpdateWorld gate、进度文本/初始化副作用及版本返回码。当前读档异常在 flag 打开后发生时的清理行为为 unknown；恢复和取消契约先由 integration review 定义。 |
| WorldGen.StartHardmode -> TransformWorldOnBackgroundThread | BeginWorldTransform command -> transformation work port 在适当锁/执行上下文工作 -> completion/failure message -> main-thread follow-up adapter | 顺序至少是 hardMode state / item protection -> transform -> finally 释放 transforming gate -> main-thread callback。当前 initializeHardMode() 为空，真正地形变化为 unknown；不能用包裹它的并发机制证明效果。 |
| WorldFile._SaveWorld 与 IOLock | SaveWorldCommand -> lifecycle barrier await -> storage writer adapter 在一致锁/快照上提交 | 保持 save 不与 transformation 并行、云存档拒绝/回滚和临时文件行为。旧代码以循环等待直到 TransformingWorld 清零后再获取锁；新 await/barrier 与取消/异常等价未验证。 |
| StartRoomCheck, RoomNeeds, QuickFindHome, ScoreRoom | HousingValidationSystem.EvaluateRoom/FindBestRoom -> HousingEvaluationResult -> 条件满足时 AssignHomeCommand -> feedback/achievement adapter | 保持 world bounds、tile solidity 替代、扫描顺序、失败原因、NPC 特例、评分 tie-break 与旧 homeless 标志。RoomNeeds 写共享 scratch，所以不直接映射成对 legacy 全局状态的 Query。异常时 tile solidity 恢复和 feedback implementation 均为 blocker。 |
| WorldGen.TownManager / WorldFile.SaveTownManager / LoadTownManager | Registry Query/Set/KickOut -> TownHousingPersistenceAdapter.Write/Restore -> 加载后房间重验/assignment command | 保持数据版本、居民键、数组/list 快速状态一致、房间点坐标格式和驱逐次序。旧 key 以 NPC type 为核心，NSSLC resident key mode 未确定；不可假定实体 ID 可替代旧键。 |
| Main -> WorldGen.UpdateWorld -> CountTiles | 既有 world-update host 调用 metrics tick; WorldTileMetricsSystem.AdvanceColumn; 形成 snapshot 后 WorldMetricsProjection.SendLegacyMessage(57); Skyblock observer adapter | 保持 Main 调用位置、30 tick cadence、totalX 循环、列区段/权重、累计边界、零比例特殊处理、发送时点和 liquid/weather/tile 随机顺序。Net message 与 Skyblock 最终 owner 为 integration review。 |
| WorldGen.SetWorldSize / WorldFileData.SetWorldSize | WorldSizeCatalogQuery -> SetWorldDimensionsCommand -> world-storage owner 分配/提交边界 | 保持 small/medium/large 对应 4200x1200、6400x1800、8400x2400，及旧偏移/尺寸 metadata 语义。源查询有 partial 结果，需确认所有调用者与 world allocation 时点。 |
| WorldGen.TreeTops, TreeTopsInfo.Save/Load/SyncSend | TreeTopsSystem.Get/SetStyle -> snapshot Query; persistence adapter 对 <211 旧版复制/当前 count 格式解码；projection 写网络 byte 序列 | 保持 13-area 顺序、默认值、版本界限及 net message 字节顺序。外部样式写者与 load failure/short stream 行为还需检查。 |
| TransformingWorld, genRand, oceanLevel | WorldLifecycleQuery.IsTransforming; IGenerationRandomSource.Next; WorldOceanLevelQuery.Evaluate(surface, rock) | genRand 是共享可变随机源，需要维持调用顺序，不是可任意重排的 Query。oceanLevel 是纯派生；实现跟进已将旧 getter 接入该 Query，并去掉额外的有限值拒绝，以保留 Version4 算式的 IEEE 浮点结果；行为仍未运行验证。 |

## Call and Dependency DAG

以下是静态源码明确的有限路径，箭头描述可见调用/提交次序，不代表全程序 DAG 已闭合：

    Main.Update
      -> WorldGen.UpdateWorld [两个已索引 call-sites]
           -> lifecycle gate: isGeneratingOrLoadingWorld
           -> Wiring / TileEntity / lunar update
           -> totalD cadence -> CountTiles(totalX)
           -> Liquid cadence -> weather -> infection/tile/random passes

    NPC / Main -> QuickFindHome
      -> StartRoomCheck -> tile scan + IRoomCheckFeedback dispatch
      -> RoomNeeds -> special-NPC conditions -> ScoreRoom
      -> occupancy/CanNPCsLiveWithEachOther via TownManager
      -> NPC home/homeless writes + Achievement notification

    WorldFile.LoadWorld_Version2
      -> LoadWorld_LastMinuteFixes -> FixAgainstExploits
           -> StartRoomCheck(saved NPC home)
           -> TownManager.HasRoom(fallback room) -> StartRoomCheck(fallback)
           -> TownManager.KickOut -> NPC homeless write

    WorldFile.ValidateLoadNPCs -> bounded NPC-section read for stream validation

    WorldFile Save path -> TownManager.Save; Load path -> TownManager.Load

    NPC -> StartHardmode
      -> Main.hardMode / item-spawn protection
      -> TransformWorldOnBackgroundThread
           -> increment transforming count
           -> Task.Factory work under WorldFile.IOLock
           -> finally decrement count and enqueue main-thread follow-up
    WorldFile._SaveWorld -> wait until TransformingWorld is false -> TryEnter(IOLock)

    WorldFile.LoadWorld -> parse/version loader -> cleanup + settle/liquid checks
      -> clear isGeneratingOrLoadingWorld -> NPC / world-specific finalization
      -> exception catch -> loadFailed = true

边界说明：CPG 只确认选定 shards 内 CallTargets 端点；Get-CpgCallableFacts 表明 effects 未扩展。feedback 是接口分派，异步传入的 delegate 和 main-thread action 是动态入口。WorldFile 版本分支、配置/种子、网络 handler、反射注册及 reset/unload 不在当前静态闭包内，均保留 partial/unknown。不得依据类文件顺序、System 注册顺序或 SS14 示例推断帧调度顺序。

## Lifecycle and Side Effects

- **Create / load / activate:** 内部 lifecycle 可先记录 load attempt，但旧 isGeneratingOrLoadingWorld 的兼容投影需保持可观察时点；当前读档路径在 parse/repair 后、liquid settle 前置位，settle 后清除。只有 settle 与需要的 world validation 结束后才宣布 ready。输入数据版本、cloud save 早退、异常、重试/取消、旧 world 保留策略需作为明确转换测试。Version4 WorldFile.LoadWorld 在 cloud error 可直接置 loadFailed；内部 catch 也置该值，但在 settle 期间抛错是否清除 load flag 未证实。
- **Transform / save barrier:** transformation 的 Task/finally/main-thread 回调和 WorldFile I/O lock 是跨线程外部效果。新 owner 必须定义锁顺序、多个转换如何计数、失败是否仍触发 callback、save 是否可取消；当前只有 initializeHardMode 函数体空，真实 hardmode 转换效果 unknown。
- **Housing update / failure:** QuickFindHome 在扫描中临时覆写全局 Main.tileSolid[379] 并遍历 NPC/Tile；它最后恢复原值，但可见源码不是 finally。扫描失败/抛错、重入、反馈实现对 NPC/World 状态的回调行为需确认。候选结果只有在有效且 occupancy 允许时提交到 NPC；读档房间失效后会尝试 registry 位置并可能 KickOut。保留这条旧顺序直到有行为证据支持重排。
- **World update cadence:** Main 两个调用分支都有 WorldGen.UpdateWorld；UpdateWorld 先 lifecycle gate、更新 infection policy、Wiring/TileEntity/Lunar、计数、Liquid/天气，再按照 world update rate 作随机 tile/pass work。若提取 metrics System，必须仍在对应旧 tick 点推进；不得因新增 ECS system registration 改变随机调用序列或跨帧可见性。
- **Persistence / replication:** Town room map 经 WorldFile Save/Load，TreeTops 写入世界格式且有 version<211 fallback；TreeTops 也通过 NetMessage writer 写出。CountTiles 发送 NetMessage 57。存档与网络序列化是外部协议，要求 byte order、版本、加载默认值和读写失败保持；本报告无运行时/文件协议验证。
- **Reset / destroy / rebuild / multi-world:** WorldGen 是大量 static 状态。legacy `serverLoadWorldCallBack` 现将并发读档串行到 `Hooks.WorldLoaded()` 完成，并拒绝同线程递归进入；这只限定该入口的重叠执行。完整 reset/clear/unload、取消、重载失败后的所有静态状态清除及是否一次只运行一个 world 仍未闭合，也没有证据证明字段或已有 NSSLC holder 已按 session 隔离。
- **Reference comparison:** SS14 的 GridPreloader 将 map-scoped collection 放在其 Component 并由 System 订阅 round/map-load 事件；Atmosphere processing 保留 cursor 与续帧预算。它们提示应显式建 lifecycle 与长扫描状态，但 SS14 scheduler、存储和实体模型不能替换此处对 Terraria Main/WorldFile 调用顺序的取证。

## Integration Handoff

以下所有 owner 决定均交给 integration-review；P16 不把相邻能力的成员移入本分区，也不代其他分区确定最终 owner。

| Handoff seam | Required decision / evidence |
|---|---|
| WorldFile / Main / world session | 谁拥有唯一 Loading/Ready/Failed/Transforming 状态；error/cancel/retry/clear 的原子转换；异步 task 与 main-thread completion、IOLock 的锁顺序与 unload 收尾。 |
| NPC housing / Town registry | resident identity 是 NPC type、entity instance 还是其他 key；NPC home fields 与 registry 谁是权威；spawn/search/occupancy/WorldFile restore 的调用方；失败反馈和 achievement 是否在同一次 commit 中发生。 |
| Tile / biome / Skyblock / networking | CountTiles 对 tile 和 Skyblock collection 的写职责、metrics snapshot schema、message 57 的时点、多个 world/context 的 scan cursor 如何区分。 |
| World progression / events / weather | shadowOrb*, spawn*, meteorShowerCount, stopDrops, hardmode/infection flags 与对应 progression/event/weather owner 的一写者规则。 |
| Tile mutation / framing / exploit behavior | merge*, destroyObject, tileSolidBackup, ExploitDestroyQueue, bitStrip 的写者、线程访问与 reset 点。 |
| Background / scene / random / dimensions | 背景缓存失效与读者、WorldGen distances/metrics 与 SceneMetrics 的 snapshot contract、随机源所有者和调用顺序、WorldSize Command 对 world storage 的提交点。 |
| Persistence / network protocol | TownManager key 和 save compatibility；TreeTops <211 migration 与当前字节序；输出网络快照是否只读，不得回写 Component。 |

## Migration Behavior Contract

后续实现若获授权，应按每个 legacy 行为建立具体 observation vector，至少比较：输入 world/session 和 NPC/Tile 状态、返回值/错误、authoritative state delta、房间结果/坐标、事件/achievement/网络/文件副作用、调用顺序和随机流消耗、失败后的恢复状态、重复调用幂等性及 world/session 隔离。API 可重组但必须观察等价，不要求方法数或签名一致。

建议先按依赖顺序切垂直行为：

1. 定义 world-session lifecycle 与 Storage/lock/main-thread port，验证成功、早退、异常、重试和 save/transform barrier，再切换 Main 的 update gate。
2. 以独立 housing scan context 实现只读 evaluation result 与 command commit；把 TownRegistry 的身份/保存协议作为单独决策，迁移 NPC/WorldFile callers 后再考虑删除旧共享 scratch。
3. 将列计数切成具有 cursor/window 的 metrics owner，以既有 UpdateWorld 节点触发，并通过独立 legacy projection 发出网络/Skyblock 结果；核准消息/调用节奏后再替换旧 counters。
4. 单独移植 TreeTops 的读写协议、世界版本兼容与网络 projection；环境缓存、尺寸规则、tile merge 和各种 scratch 不作为一个批次一并重写。

旧 API facade 若继续存在，必须是 adapter/协调入口，不能另持一份同一不变量的状态。每一垂直切片至少先并行比较观测结果，再逐消费者切换，保留受控回滚开关；删除旧实现只能在 owner 唯一且行为测试覆盖后另行授权。本报告没有执行、影子比较、回滚或删除。

## Evidence Gaps and Blocking Decisions

1. **Source identity:** 目标源码工作树 commit/hash 未记录；CPG manifest 未绑定当前 source snapshot 或逐文件 hash。已引用位置需在实现批次重新定位。
2. **Prior analysis gap:** P16 public-decomposition 指定产物缺失；若其包含与此报告冲突的决议，需要 integration review 先恢复证据而非猜测。
3. **Hardmode behavior:** WorldGen.initializeHardMode() 在目标源码是空函数体；真实转换写入为 unknown。当前只能确认包装器、锁、计数和 follow-up 调度，不得声称 hardmode 已映射完整。
4. **Load failure/reset:** Application recovery code models primary retry, backup check/restore/retry, cancellation, publication failure, terminal reset, and unexpected exceptions. A partially published world raises the compatibility gate before reset and releases it only after reset and lifecycle projection succeed. If the gate projection fails, recovery skips destructive reset and preserves `RequiresWorldReset`; failed reset or terminal projection also leaves the default runtime projection's gate raised. The legacy async callback is now serialized through its post-load hook, with same-thread recursive entry rejected. This remains source-level evidence: no full game/server registration root is present, host reset/finalization and external unload callbacks remain integration-owned, and no behavior tests were run. External runtime failures and unload outcomes remain unverified.
5. **Housing callback closure:** IRoomCheckFeedback 动态实现集合、异常副作用和临时 tileSolid[379] 的异常恢复未验证；call-site query 的 complete 不消除这些 gap。
6. **Housing authority:** TownManager 当前以 NPC type 索引；NSSLC TownHousingResidentKey 有多种 key mode 候选。NPC instance/type 的最终身份、world scope、持久化兼容和唯一写入者是 blocking integration decision。
7. **Static state and multi-world:** P16 字段多为 static；当前没有证据表明未来 session Component 实际注册、被调用或能支持多 world。callback gate 限制旧异步读档入口的并发重叠，不会把这些全局字段改为 session-scoped；其余状态重置、外部重入和静态读写仍需迁移前清点。
8. **Metrics dependencies:** Tile array 完整扫描窗口、计数边界/防零分母、Skyblock observer 影响、message 57 消费端与服务端/客户端方向未闭合。不可把 Query status complete 当作这一行为闭包。
9. **TreeTops protocol:** 旧版本 <211 的 copy path、当前 save/load count 不匹配/截断输入、所有 set/randomize callers 和网络字节的接收端还需验证。
10. **Terrain/effect scratch ownership:** P16 的 weather/event/tile merge/exploit/flood-fill scratch 相互独立；本分区不含这些跨 owner 的最终业务决策和全闭包写入证据。
11. **Existing target code integration:** WorldLayerMetricsSystem、WorldOceanLevelQuery 等局部 API 已有源码；housing/lifecycle/TreeTops holders 也存在，但没有检查或证明它们覆盖旧 API caller、存档和网络并发路径。实际迁移与等价仍 unknown。

阻塞后续切换的决定：world session 的 authoritative entity/session key；load fail/cancel 的终态转换；housing identity 和 NPC/registry 双写策略；旧存档 key/schema 与新 Component 恢复映射；Main update 注入点及线程/随机顺序；metrics/network/Skyblock 提交合同。以上未定时只能实现隔离的候选逻辑，不能切换旧入口或宣告迁移完成。

## Verification Plan

以下仅是未来针对行为的验证计划；最初设计分析阶段没有执行构建、测试、verifier、运行时检查或协议 round-trip。后续实现阶段的增量编译单独记于下方执行记录；没有行为验证，verificationStatus 仍为 not-run。

- **Lifecycle matrix:** cloud 不可用、missing world/autogen、版本拒绝、正常读档、版本修复、settle 正常/异常、取消/重试；断言状态转移、UpdateWorld gate、清理行为、错误码/进度副作用与 ready 时点。
- **Transform/save concurrency:** 多个并发 transformation、transform 抛错、follow-up 抛错、主线程排队、save 同时开始/等待/取消；断言计数非负/归零、IOLock 不交错、回调次数和存档快照边界。
- **Housing contract:** world edge 与 tile boundary、大小限制、椅/桌/门/光源/wall/stinkbug/gate 规则、special NPC 资格、评分 tie-break、住房互斥/共享规则、NPC homeless/home 更新、WorldFile restore 后修复与 KickOut；再以 feedback throw 覆盖 tileSolid[379] 恢复及不发生半提交。
- **Town registry persistence:** 空/多房间、重复 NPC key、非法/越界 key、截断/未知版本、load 后 room revalidation；确保快速索引与主映射一致，旧存档升级后无重复或房间错配。
- **Metrics cadence and protocol:** 连续更新 29/30/31 次、世界宽度边界和 X wrap、完整扫描中的 Tile/biome 计数、X=0 百分比/累计重置/单次 message 57；同时检查 zero solid 与 Skyblock observer 的现有结果。
- **Update order and randomness:** 比较 Main 两入口、Liquid/Wiring/TileEntity/weather/tile passes 的顺序及同 seed RNG 消耗，确保 System 接入没有延迟一帧或改变随机序列。
- **Dimensions and derived reads:** 三种世界尺寸、旧 metadata 和输入边界；oceanLevel 数值与旧算式的有效/非有限输入；随机来源调用必须保持主序列。
- **TreeTops format:** 13 个 area 全值；<211 copy、211 及后续版本读写、截断/错误 count、sync 13-byte 顺序与接收端一致；读档/发包不反向写权威状态。
- **Isolation/rebuild:** 重复 load/fail/retry/world clear/session destroy、两个 world 并行或切换、save/load/rebuild 后不串写 Housing/metrics/tree/event/scratch state。

通过上述必要的目标行为测试前，结论仅为 proposed 静态拆分；本报告完成或 runner 结算均不代表迁移成功、已编译、行为等价或运行时接入。

## 2026-10-04 Implementation Follow-up: World Load Lifecycle

本节补记本报告发布后的局部实现，不改写上述 2026-09-18 静态证据或将 P16 标为完成。

- `WorldStorageCoordinatorFactory.RegisterGeneratedWorldLoadHandlerWithRuntimeTileMap` 已把生成加载恢复协调器接到 legacy `Main.LoadWorld` 注册接口；发布顺序是 TileMap、取消检查、核心 session 标量/外观与 TownManager 绑定、Chest/Sign 投影、再次取消检查、宿主其余状态回调。`WorldLoadRecoveryCoordinator` 负责 gate、重试/备份、settle、finalize 和失败 reset。
- `LegacyWorldSessionProjection` 对 Optional Absent 的固定长度外观区段按 `WorldFile*Section.Empty` 的零值表形状投影；present 但长度不符仍拒绝。这样旧版本缺少 Additional Backgrounds 区段时不会仅因投影数组为空而失败。
- `LegacyWorldSessionProjection.PublishCoreState` 成功投影核心状态后，会把 `WorldGen.TownManager` 绑定到候选 session 的 `TownHousingRegistryComponent`，供后续受门控宿主回调访问；若后续发布失败，`WorldGen.clearWorld()` 会重建 TownManager，丢弃候选绑定。
- `LegacyWorldChestAndSignProjection` 在受门控发布阶段先从候选 session 的容器/标牌 snapshots 构造完整 runtime staging 数据，再检查 legacy 容量、坐标、重复 anchor 与活动物品类型，最后更新 `Main.chest`、Chest 坐标索引和 `Main.sign`。这两类状态不再要求宿主回调重复投影。
- 截至本节记录时，`TownRoomManager` 的 `HasRoom`/`HasRoomQuick`、房间分配与清除、住户枚举和 Save/Load 已转发到 `TownHousingRegistrySystem` 与 persistence adapter；`CanNPCsLiveWithEachOther` 还是 compatibility stub，旧规则依赖的 NPC housing-category/profile 数据尚未接入。后续互住规则实现见“2026-10-04 Implementation Follow-up: Town Housing Compatibility”。
- `WorldNpcLoadApi` 保留可空 `NetId`，不再把旧格式缺失值折叠成 `0`；`WorldNpcState` 同时保留 `LegacyTypeName`。这避免损失输入身份形式，但旧类型名到 runtime definition 的解析和完整 NPC 默认状态 materialization 仍需要宿主内容目录。
- 仍未闭合端到端接线：仓库搜索未发现生产组合入口调用该注册方法；宿主仍需投影 NPC、TileEntity、Bestiary 等状态并保持 NPC home 与 TownHousing registry 一致，也需提供宿主专属预结算清理和额外 finalization。旧版 `WorldFile.LoadWorld` 的 NPC/skyblock/slime-rain/WorldId 收尾序列已由标准 `LegacyWorldLoadRecoveryEffects` 执行，但 NPC legacy type-name 到 runtime ID、NPC spawn consumer、TileEntity runtime owner 和 Bestiary 的宿主适配仍未闭合。不能据此宣称 legacy `WorldFile.LoadWorld` 已在实际游戏宿主中运行新路径。
- 仅执行生产项目增量编译：`dotnet build src/NSSLC.Infrastructure/WorldStorage/NSSLC.Infrastructure.WorldStorage.csproj --no-restore -p:BuildProjectReferences=false --verbosity minimal`，退出码 `0`，警告 `0`、错误 `0`；程序集输出在 `Build/bin/NSSLC.Infrastructure.WorldStorage/Debug/net10.0/NSSLC.Infrastructure.WorldStorage.dll`。未运行测试、verifier、运行场景或协议 round-trip；行为验证状态仍为 `not-run`。
- `TownManager` 绑定接线后再次编译：完整 `dotnet build src/NSSLC.Infrastructure/WorldStorage/NSSLC.Infrastructure.WorldStorage.csproj --no-restore --verbosity minimal` 的依赖图在 `NSSLC.WorldGeneration` 成功生成（13 条警告）后，于 `ProjectileDerivedPropertiesQuery.cs(3,16)` 因找不到 `Terraria.Npc` 失败；随后目标项目命令 `dotnet build src/NSSLC.Infrastructure/WorldStorage/NSSLC.Infrastructure.WorldStorage.csproj --no-restore -p:BuildProjectReferences=false --verbosity minimal` 退出码 `0`，警告 `0`、错误 `0`，确认 WorldStorage 改动可编译。未运行测试；行为验证状态仍为 `not-run`。
- Chest/Sign 投影接入发布链后再次运行 `dotnet build src/NSSLC.Infrastructure/WorldStorage/NSSLC.Infrastructure.WorldStorage.csproj --no-restore -p:BuildProjectReferences=false --verbosity minimal`，退出码 `0`，警告 `0`、错误 `0`，程序集输出仍位于 `Build/bin/NSSLC.Infrastructure.WorldStorage/Debug/net10.0/NSSLC.Infrastructure.WorldStorage.dll`。未运行测试、verifier、运行场景或协议 round-trip；行为验证状态仍为 `not-run`。
- 保留旧 NPC identity 表示后分别构建 `src/NSSLC/Component/WorldStorage/Terraria.WorldStorage.csproj`、`src/NSSLC.Application/NSSLC.Application.csproj` 和 `src/NSSLC.Infrastructure/WorldStorage/NSSLC.Infrastructure.WorldStorage.csproj`，均使用 `--no-restore -p:BuildProjectReferences=false --verbosity minimal`：三个目标均退出码 `0`；依次为 7/0/0 条警告、0 错误；产物分别位于 `Build/bin/Terraria.WorldStorage/Debug/net10.0/`、`Build/bin/NSSLC.Application/Debug/net10.0/` 和 `Build/bin/NSSLC.Infrastructure.WorldStorage/Debug/net10.0/`。未运行测试；行为验证状态仍为 `not-run`。
- 失败清理路径复核发现生命周期投影原先在 TreeTops/活动会话投影之后才升起 legacy load gate；这些步骤若先失败，恢复协调器仍可能继续 reset，而世界更新门尚未确认升起。`IWorldLoadLifecycleProjection` 现在要求升门作为首个可失败操作前的发布、失败后保持升起；默认 runtime projection 按此顺序写 `WorldGen.isGeneratingOrLoadingWorld`，并仍在关联状态全部发布后才降门。分别增量构建 `src/NSSLC.Application/NSSLC.Application.csproj` 与 `src/NSSLC.Infrastructure/WorldStorage/NSSLC.Infrastructure.WorldStorage.csproj`，命令均为 `dotnet build <project> --no-restore -p:BuildProjectReferences=false --verbosity minimal`；两者退出码 `0`，各 0 警告、0 错误，产物位于对应的 `Build/bin/<assembly>/Debug/net10.0/`。未运行测试或运行时验证；`designStatus: proposed`、`verificationStatus: not-run` 不变。仓库仍无生产组合入口调用注册方法，宿主 NPC/TileEntity/Bestiary 投影与最终初始化回调仍未闭合。
- `WorldTileEntityLoadApi` 在提交阶段增加了旧加载器的边界过滤：只有 `world.tiles.load` 已提交后才读取地图尺寸，并丢弃不满足 `WorldGen.InWorld(x, y, 1)` 的 TileEntity anchors；旧 Version4 `WorldFile.LoadTileEntities` 对此类实体执行 Remove。Application 与 WorldStorage 生产项目各再次以 `dotnet build <project> --no-restore -p:BuildProjectReferences=false --verbosity minimal` 编译，退出码均为 `0`，各 0 警告、0 错误，产物仍位于对应 `Build/bin/<assembly>/Debug/net10.0/`。没有运行测试或场景。边界过滤不等于完整 TileEntity runtime materialization；实体 tile-validity 检查及运行时类型仍需宿主/TileEntity owner 接线。
- metrics 网络投影取消了旧入口不存在的 `Main.netMode == 2` 附加条件：`CountTiles` 在 `X == 0` 时无条件调用 `NetMessage.SendData(57)`，投影现在只依据该窗口事件决定是否发起相同调用。协议实现和各网络模式下的实际接收者仍未在运行时核验。`src/NSSLC.Infrastructure/WorldGeneration/NSSLC.WorldGeneration.csproj` 以 `dotnet build src/NSSLC.Infrastructure/WorldGeneration/NSSLC.WorldGeneration.csproj --no-restore -p:BuildProjectReferences=false --verbosity minimal` 增量编译通过，退出码 `0`，13 警告、0 错误；产物位于 `Build/bin/NSSLC.WorldGeneration/Debug/net10.0/NSSLC.WorldGeneration.dll`。未运行测试、verifier 或场景，行为验证状态保持 `not-run`。
- `WorldPressurePlateLoadApi` 已将存档 anchors 写入候选 session 的 `PressurePlates` registry。新增的 `LegacyWorldPressurePlateProjection` 在受门控发布且 TileMap/core/chest/sign 成功后暂存 legacy `Point -> bool[255]` 映射，再持 `PressurePlateHelper.EntityCreationLock` 重置并恢复映射，最后设置 `NeedsFirstUpdate`；空集合也保留旧加载器的首次更新语义。
- 当前 `WorldGen.clearWorld()` 增加 helper reset，使恢复失败时的 world reset 清除已投影 anchors；参考行为见 Version4 `Terraria/WorldGen.cs::clearWorld` 与 `Terraria.GameContent/PressurePlateHelper.cs::Reset/Update`。旧源码的 `MoveInto`、`MoveAwayFrom`、`PokeLocation` 函数体为空，因此本切片只迁移锚点/首次更新状态，不宣称压力板触发行为等价，也没有增加旧加载器未证明的坐标过滤。
- `WorldGeneration` 与 `WorldStorage` 生产项目分别以 `dotnet build <project> --no-restore -p:BuildProjectReferences=false --verbosity minimal` 编译，退出码均为 `0`；分别 13 警告/0 错误和 0 警告/0 错误，产物位于 `Build/bin/NSSLC.WorldGeneration/Debug/net10.0/NSSLC.WorldGeneration.dll` 与 `Build/bin/NSSLC.Infrastructure.WorldStorage/Debug/net10.0/NSSLC.Infrastructure.WorldStorage.dll`。未运行测试、verifier 或运行场景；实际游戏宿主接线仍未证实，`designStatus: proposed`、`verificationStatus: not-run` 保持不变。
- 继续接通旧版 `WorldFile.LoadWorld` 释门后的收尾顺序：`LegacyWorldLoadRecoveryEffects.FinalizeLoadedWorld` 依次初始化 NPC 萤火虫/蝴蝶概率、调用现有 `WorldGen.Skyblock.ScanTiles()`、在存档史莱姆雨计时为正时恢复无公告的活动状态，再按 WorldId 初始化洞穴怪物类型。`NPC.setFireFlyChance()` 保留旧版 `Main.rand` 与 `WorldGen.genRand` 的条件分支及调用顺序；`NPC.SetWorldSpecificMonstersByWorldID()` 使用局部 `UnifiedRandom(Main.worldID)` 并保留碰撞时两次重抽的旧逻辑。`Main.worldID` 派生自 `ActiveWorldFileData.WorldId`，而 `LegacyWorldSessionProjection.PublishCoreState` 在 finalization 前通过 `ApplyLoadedIdentity` 发布该 ID。史莱姆雨专用 runtime 方法只实现旧调用的 `announce: false` 路径，不扩展到网络公告；运行时 NPC/critters spawn 消费者仍未接入，故这些派生值的端到端使用仍未证明。
- `LegacyWorldLoadRecoveryEffects` 先执行上述旧收尾，再调用宿主 finalization callback；Factory 文档现要求该 callback 只处理额外 host-owned 工作。仓库内仍没有生产代码调用注册 API 的组合入口，外部自定义 `IWorldLoadRecoveryEffects` 也能绕过这套标准收尾；实际宿主接线和 callback 是否重复执行同一行为仍属 integration-review 缺口。
- NPC/TileEntity/Bestiary 不宜用现有占位 runtime 强行完成投影：`WorldNpcLoadApi` 只提交 candidate session 的 NPC state；编译中的 `Runtime/NPCID` 没有 legacy name resolver，`NPC` 也没有可用的 `SetDefaults`，现有 `NPC.NewNPC` 仅写入通用占位属性。`Terraria.Content` 已有 `INpcDefinitionQuery` 与 `IContentIdentityQuery`，但当前源码没有生产 `NpcDefinitionCatalog` 构造/注入点，runtime NPC 也未消费这些查询；因此它们只是可能接入的 identity/definition seam，不是当前 materializer。旧 `WorldFile.cs` 虽含 `SetDefaults` 调用，但 `NSSLC.Infrastructure.WorldStorage.csproj` 未编译该文件。运行时 `TileEntity.Read` 明确抛出 `NotSupportedException`，`Main.BestiaryTracker` 仍是动态占位。没有实际内容目录和 runtime materializer 时物化这些状态会丢 NPC 默认行为、实体扩展数据或 bestiary 身份，故保留 host adapter 缺口，不用空实现或通用 NPC 伪装完成。
- 本次仅执行两个受影响生产项目增量编译：`dotnet build src/NSSLC.Infrastructure/WorldGeneration/NSSLC.WorldGeneration.csproj --no-restore -p:BuildProjectReferences=false --verbosity minimal` 退出码 `0`，13 警告/0 错误，产物 `Build/bin/NSSLC.WorldGeneration/Debug/net10.0/NSSLC.WorldGeneration.dll`；随后 `dotnet build src/NSSLC.Infrastructure/WorldStorage/NSSLC.Infrastructure.WorldStorage.csproj --no-restore -p:BuildProjectReferences=false --verbosity minimal` 退出码 `0`，0 警告/0 错误，产物 `Build/bin/NSSLC.Infrastructure.WorldStorage/Debug/net10.0/NSSLC.Infrastructure.WorldStorage.dll`。按用户要求没有运行测试、verifier、运行场景或协议 round-trip；`designStatus: proposed`、`verificationStatus: not-run` 保持不变。
- 本轮只读复核 `WorldLoadRecoveryCoordinator.FinishTerminalFailure`、`RecoverUnexpectedFailure`、`WorldLoadLifecycleSystem` 与 `RuntimeWorldLoadLifecycleProjection`：部分发布后的 reset 路径先升 load gate，完成 reset 后将 lifecycle 标记为 cleared/unpublished，并由 terminal projection 在关联 flags/session publication 后降门；reset 或投影失败会保留升门状态。无须再改 `WorldGen.clearWorld()` 的 gate 写入。该结论是源码控制流证据，不是运行验证；外部注册入口、宿主 callbacks、unload/re-entry 仍未闭合，`verificationStatus: not-run` 不变。
- 随后复核 API 重试隔离：`IWorldLoadApi.PrepareLoad` 契约禁止修改 authoritative state 或 owner context；`Build/Tools/WorldLoadApiGenerator/WorldLoadApiCatalogGenerator.cs` 先发出完整 preparation 循环，再发出 commit 循环（生成器源码 917、922 行）；当前生成 catalog 含 27 个 API。Commit 失败会被标成 `WorldLoadApiStage.Commit` 并终止当前候选。发现 Prepare 失败时若先前 prepared 数据的 `DiscardPrepared` 抛错，执行结果虽带 `CleanupException`，恢复协调器仍会把同一个候选交给主档重试/备份重试。现新增 `RequiresCandidateDiscard`：提交失败或准备资源清理失败均终止重试并丢弃 unpublished candidate；只有 publication 已开始或其状态不确定时才执行宿主 world reset。改动位于 `WorldRecoveryOutcome` 与 `WorldLoadRecoveryCoordinator`。本次只做静态源码复核，没有运行测试、verifier、构建或场景；恢复路径和异常副作用仍需宿主运行验证，`designStatus: proposed`、`verificationStatus: not-run` 保持不变。
- 上述候选隔离改动后，仅增量构建受影响项目 `src/NSSLC.Application/NSSLC.Application.csproj`：`dotnet build src/NSSLC.Application/NSSLC.Application.csproj --no-restore -p:BuildProjectReferences=false --verbosity minimal`，退出码 `0`，0 警告/0 错误，产物位于 `Build/bin/NSSLC.Application/Debug/net10.0/NSSLC.Application.dll`。该构建只确认编译；按要求没有运行测试、verifier 或场景，行为验证仍为 `not-run`。
- 继续追踪旧异步入口发现 `WorldGen.serverLoadWorldCallBack()` 仍直接抛出 `NotSupportedException`，因此即使宿主注册了 `Main.LoadWorld` handler，该调用链也到不了恢复协调器。现将该入口改为调用 `NSSLC.WorldGeneration.IO.WorldFile.LoadWorld()`；handler 成功后才播放旧加载音效并触发 `Hooks.WorldLoaded()`，失败时不触发成功收尾。Owner projection 将其已实现的 session 时间/天气/进度字段直接写入 runtime，因此该路径不再调用旧 `SetOngoingToTemps()` 暂存器；但 `WorldQuestLoadApi` 保存的 `CultistDelay` 尚无 runtime projection，仍需宿主 adapter 接手。静态源码复核确认该入口连到 `WorldFile.LoadWorld -> Main.LoadWorld -> registered handler`；仓库仍没有生产组合入口注册该 handler，NPC/TileEntity/Bestiary runtime materializer 和宿主 callbacks 仍未闭合。本项没有运行测试、verifier、构建或场景，`designStatus: proposed`、`verificationStatus: not-run` 保持不变。
- 复核 metrics 清理路径：`Main.ResetWorldStorage` 切换为新 `WorldSessionRestoreState` 后调用 `WorldGen.ResetWorldTileMetrics`；该路径清掉 TileCounts、30 tick cadence、scheduled/next column、累计窗口及 published snapshot，`LegacyWorldTileMetricsProjection.Reset` 同时清零旧 totals、百分比、`totalX` 与 `totalD`。成功加载发布也先切换到候选 session 再重置其 metrics。以上静态调用链覆盖了 Version4 `clearWorld` 对 `totalX/totalD` 的重置要求，因此此处无需重复添加游标复位逻辑；未运行行为验证。
- 生命周期复核发现 `WorldGen.clearWorld()` 将 `worldCleared` 设为 true，但成功的 `GenerateWorld()` 没有结束该状态。现在只有 `_generator.GenerateWorld()` 返回成功、`Finish()` 完成且 `finally` 中的临时状态恢复正常返回后，才将其清为 false；生成失败或任何异常路径仍保留清世界结果。该状态当前还用于恢复 coordinator 确认 reset 已完成。本项仅做静态控制流复核，未运行测试、构建或场景，`verificationStatus: not-run` 不变。

## 2026-10-04 Implementation Follow-up: Town Housing Compatibility

本节继续补齐住房互住兼容边界；不改变 P16 全分区状态，也不将其标为完成。

- Version4 `TownRoomManager.CanNPCsLiveWithEachOther(NPC, NPC)` 直接比较两个 `housingCategory`；整数重载先从 `ContentSamples.NpcsByNetId` 解析，找不到时返回 `true`。`NPC.SetDefaults` 将类别初始化为 `0`，仅将城镇宠物类型 `637、638、656、670、678–684` 设为 `1`。这些 ID 与当前 `NPCID.Sets.IsTownPet` 列表一致；Version4 `ContentSamples.Initialize` 为 `-65` 至 `NPCID.Count - 1` 的 NetId 建立样本。
- 运行时 `NPC` 新增 legacy `housingCategory` 字段；`NPC.NewNPC` 依据相同 `IsTownPet` 集合初始化为 `0/1`。`TownRoomManager` 实现了原来的两个重载，按类别不相等判断是否可同住，并对 Version4 样本范围外的整数键保留允许行为。
- 此实现闭合当前 WorldGen 生成路径的 profile 读取，但未接入生产 NPC definition catalog，也没有将恢复 session 中的 `WorldNpcState` 物化到 runtime NPC；宿主载入 NPC 时仍须按定义恢复 `housingCategory`。当前 runtime 的整数兼容查询也以 Version4 连续 NetId 样本范围和内置 `IsTownPet` 集合为准，尚未接入宿主扩展的样本/内容覆盖。
- 本次只检查源码与调用签名，没有运行测试、构建、verifier 或场景；行为验证仍为 `not-run`，P16 的 owner / runtime integration 缺口继续保持开放。

## 2026-10-04 Implementation Follow-up: Derived Ocean Level

- Version4 `WorldGen.oceanLevel` 与当前旧 getter 使用 `(worldSurface + rockLayer) / 2.0 + 40.0`。旧 getter 现委托 `WorldOceanLevelQuery.Evaluate`，共享项目 Query 只读取显式参数并执行该算式；移除此前额外的 `double.IsFinite` 拒绝，确保 NaN/Infinity 也按原算术传播。
- 只读静态检查确认 WorldGen 的消费者仍通过 `oceanLevel` 读取，派生计算没有缓存、共享写入或附加副作用。没有运行测试、构建或场景；`verificationStatus: not-run` 不变。

## 2026-10-04 Implementation Follow-up: World Load Exception Flag

- `NSSLC.WorldGeneration.IO.WorldFile.LoadWorld()` 现在在分发至 `Main.LoadWorld()` 时捕获 handler 异常，保存至 `LastThrownLoadException` 并设置 `WorldGen.loadFailed = true`，以匹配旧版 WorldFile 捕获加载异常、由 `serverLoadWorldCallBack()` 根据失败标志跳过成功收尾的行为；其他成功/失败结果仍由恢复协调器发布。加载正常返回后，音效和 `Hooks.WorldLoaded()` 仍只有在 `loadFailed == false` 时执行。此处不替代宿主 reset。
- 进一步核对注册 handler 的终态观察器后发现，`resultObserver` 在成功发布且释门后调用；其异常原先会逸出并被上述 runtime facade 误记为加载失败，令已提交世界处于 `loadFailed == true`。现在将结果观察失败转交可选 `exceptionObserver`，并把通知器本身失败写入 `Trace`，不再改变已完成的恢复结果；相应观察器职责与异常隔离已写入 Factory 契约。
- 静态对照 Version4 `WorldFile.LoadWorld()` catch 路径和 `WorldGen.serverLoadWorldCallBack()` 成功收尾条件，复核了 `Runtime.WorldFile.LoadWorld -> Main.LoadWorld -> registered handler -> resultObserver` 的异常边界。仓库仍无生产组合入口注册 handler；NPC、TileEntity、Bestiary runtime materializer 和宿主回调继续未闭合。未运行测试、构建、verifier 或场景；`designStatus: proposed`、`verificationStatus: not-run` 保持不变。

## 2026-10-04 Implementation Follow-up: Load Failure Observer Boundary

- handler 在存储门获取前遇到异常时，如果配置了 `exceptionObserver`，此前会报告后正常返回而不设置 legacy `WorldGen.loadFailed`。现在先标记失败，再通知 observer；因此取消提供器或 I/O gate 获取错误不会被误当作成功加载。
- `Runtime.WorldFile.LoadWorld()` 的异常捕获范围现在也包括随机源初始化、菜单背景锁和季节前置钩子；这些准备步骤失败时同样记录 `LastThrownLoadException` 并设置 `loadFailed`，使 `serverLoadWorldCallBack()` 跳过成功收尾。
- Factory 将 exception observer 调用收束到 `NotifyExceptionObserver`。无论它观察的是 gate/recovery 异常还是结果观察器异常，observer 自身抛错都会作为诊断聚合异常写入 `Trace`，不会覆盖原错误或改变已计算的恢复状态。源码搜索确认 observer 没有其他直接调用点。
- 只做了静态控制流和调用点检查；按要求未运行测试、构建、verifier 或场景。未闭合项仍包括生产宿主注册入口与 NPC、TileEntity、Bestiary runtime materializer；`designStatus: proposed`、`verificationStatus: not-run` 保持不变。

## 2026-10-04 Implementation Follow-up: Runtime World Reset Arrays

- Version4 `WorldGen.clearWorld()` 会清空整个 `Main.countsAsHostForGameplay`，并对 400 个物品槽将 `Main.timeItemSlotCannotBeReusedFor` 归零。当前 `MainHost.ResetWorldStorage()` 重建物品数组，却保留这两组静态数组；现已在重建时按对应范围清零，恢复失败触发 clearWorld 后不会把旧世界的 host 标记或物品槽冷却带入新会话。
- 静态调用链为 `WorldLoadRecoveryCoordinator.FinishTerminalFailure -> LegacyWorldLoadRecoveryEffects.ResetWorld -> WorldGen.clearWorld -> Main.ResetWorldStorage`；只做源码检查，未运行测试、构建或场景。该补充不闭合动态 NPC/Bestiary owner 或生产宿主注册；`designStatus: proposed`、`verificationStatus: not-run` 保持不变。
## 2026-10-04 Implementation Follow-up: NPC Lifecycle Reset State

- Version4 `WorldGen.clearWorld()` 清空 `NPC.ShimmeredTownNPCs`，调用 `NPC.ClearFoundActiveNPCs()`，并在重建 NPC slots 时将 `spawnSlotProtected` 逐项置零。Version4 `NPC.cs` 将前两者定义为 `bool[NPCID.Count]`，slot protection 定义为 `int[InitData.MaxNPCs]`；`InitData.MaxNPCs` 为 200。
- 当前 runtime 原先把这些静态状态声明为 `dynamic` 占位。现将其改为相同维度/元素类型的数组，实现 `ClearFoundActiveNPCs()`，并在 `WorldGen.clearWorld()` / `MainHost.ResetWorldStorage()` 中清理。它们因此随部分发布后的恢复 reset 一起清空，不再遗留跨会话 NPC 搜索或槽保护标记。
- 证据来源：`D:/TRbackup/Version4/Terraria/WorldGen.cs:6554,6563,6642`、`D:/TRbackup/Version4/Terraria/NPC.cs:5959,6297,7119`、`D:/TRbackup/Version4/Terraria/InitData.cs:5`。本轮只检查源码与差异，没有运行测试、构建或场景；Bestiary/Pylon/CreativePower 等仍无本地运行时 owner，生产宿主注册仍未闭合，`designStatus: proposed`、`verificationStatus: not-run` 保持不变。
## 2026-10-04 Implementation Follow-up: NPC Daily Reset Flags

- Version4 `WorldGen.clearWorld()` 调用 `NPC.ResetBadgerHatTime()`；该方法清除 `EoCKilledToday` 与 `WoFKilledToday`。当前 runtime 原先将方法留作动态空占位，现实现这两个布尔状态及精确清零，并把调用接回 `clearWorld()`。
- Version4 的两个置位写入分别位于 NPC boss 事件流程；当前 runtime 尚无对应事件写入/消费实现。本补充只迁移 reset 效果，不宣称相关事件行为已迁移。证据：`D:/TRbackup/Version4/Terraria/WorldGen.cs:6401`、`NPC.cs:6468,6470,65432-65437,65502-65508`。只检查源码，未运行测试、构建或场景；`designStatus: proposed`、`verificationStatus: not-run` 不变。

## 2026-10-04 Implementation Follow-up: First-Save Lifecycle Owner

- Version4 `WorldFile.LoadWorld()` 的自动生成顺序是先建立新世界 metadata，运行 `WorldGen.GenerateWorld()`，成功后调用 `SaveNewWorld()`，再从目标路径读回刚保存的世界（`D:/TRbackup/Version4/Terraria.IO/WorldFile.cs:664-714`）。这使首次持久化成功成为加载生命周期的一部分，生成返回成功本身不足以结束该流程。
- 当前 `NSSLC.WorldGeneration.IO.WorldFile.SaveNewWorld()` 与 `SaveWorld()` 仍抛 `NotSupportedException`；当前 `Main` 没有 Version4 `autoGen` 入口或 metadata 创建流程。`RegisterGeneratedWorldLoadHandler` 的当前调用方是 `NSSLC.Tools.Simulation`；未找到完整游戏/server production composition root。
- 可见的旧式 writer 位于 `src/NSSLC.Infrastructure/WorldStorage/WorldFile/WorldFile.cs`，但 `NSSLC.Infrastructure.WorldStorage.csproj` 的显式 Compile 项只包含 section decoder/encoder/validator、tile codecs 与新 adapter，没有编译这个 monolith。`LoadedWorldPersistenceSnapshotSource` 现实现 `IWorldPersistenceSnapshotSource`，并由 `NSSLC.Tools.Simulation.CreateSnapshotCoordinator` 用于已加载 session 的 autosave；它要求 session 已发布且 lifecycle 为 `Completed`，不能捕获生成中 runtime world 的首次保存快照。`GeneratedWorldDocumentProjection.Create` 的唯一调用点仍是 `NSSLC.Tools.WorldGeneration.WorldPersistenceRoundTrip`，其输入是已生成缓冲区，不是可持续反映 runtime 后续变更的活动世界 owner。
- 因此没有把已加载 session 的旧快照接到首次保存，也没有将离线生成投影伪装成活动世界快照。安全闭合该流程仍需要宿主提供当前权威运行时快照 source、metadata/目标路径来源，并在同一个组合根中共享 transform/save I/O gate、注册 load 与 save handlers，再把首次保存成功接回读档流程；这些生产 owner/caller 当前不在仓库。该跨系统接线保留 `crossSubsystemOwner: integration-review`。
- 首次保存子边界只做源码、项目编译项和调用点静态检查；该子边界没有修改生产代码，也未运行测试、构建、verifier 或场景。`designStatus: proposed`、`verificationStatus: not-run` 保持不变。

## 2026-10-04 Implementation Follow-up: Reset Completion Marker

- `WorldStorageCoordinatorFactory.ResetLegacyWorld()` 仅在 `WorldGen.clearWorld()` 返回后检查 `worldCleared`，并以此生成 reset 成功结果。此前 `ResetGenerationState()` 在 clearWorld 后续 `Manifest` / `TownManager` 替换、维度更新、Tree shake 清理、metrics 归零和 `Liquid.ReInit()` 之前就将该标记设为 true；若后续清理抛错，标记会先于完整 reset 发布。
- 现将 `worldCleared = true` 移至 `clearWorld()` 末尾、`Liquid.ReInit()` 成功之后。Version4 `clearWorld()` 也在完成 tile/entity/liquid 清理及 `Manifest` 替换后才置位（`分类参考/WorldGeneration/Version4/Terraria/WorldGen.cs:6654-6667`）。这样同步异常路径不会由该标记提前宣称 reset 成功；它仍不证明运行时宿主清理 callback 的行为等价。
- 只做源码控制流复核，未运行测试、构建、verifier 或场景；`designStatus: proposed`、`verificationStatus: not-run` 保持不变。

## 2026-10-04 Implementation Follow-up: Current Load Diagnostic

- `Runtime.WorldFile.LoadWorld()` 只在 catch 中写 `LastThrownLoadException`；同一 facade 后续成功加载会保留上次异常，令诊断值与当前 `loadFailed` 结果不一致。全仓静态搜索确认当前运行时该字段没有其他消费者。
- 现于每次加载入口先清空该字段；本次异常仍由 catch 重新写入，且继续设置 `WorldGen.loadFailed`。因此字段表示最近一次调用的异常，而不是跨调用累积的历史异常。
- 只检查源码和字段读写点；没有运行测试、构建、verifier 或场景，`designStatus: proposed`、`verificationStatus: not-run` 保持不变。

## 2026-10-04 Implementation Follow-up: Angler Quest Load Projection

- `WorldQuestLoadApi` 已把 `AnglerWhoFinishedToday` 和 `AnglerQuest` 写入 `WorldNpcHistoryStateComponent`，但当前 runtime `Main` 没有对应接收字段，session commit 因而丢弃了这两项已解码数据。Version4 将这两项保存在 `Main.anglerWhoFinishedToday` / `Main.anglerQuest`；`WorldFile.ResetTemps()` 另清空每日完成名单并复位 `anglerQuestFinished`，但不重置 `anglerQuest`。Version4 源码中该 reset 的唯一调用点是 `WorldGen.clearWorld()`，所以普通 load commit 不额外改动这个未持久化布尔值。
- `MainHost` 现提供对应 legacy runtime 字段；`LegacyWorldSessionProjection.PublishCoreState()` 在受 load gate 保护的 commit 中复制名单并发布任务索引；`ResetGenerationState()` 清空每日名单和完成标记，保留旧版未清零的任务索引语义。
- 此 projection 只闭合这两项已存在的 quest runtime 字段，不补写尚无当前 runtime owner/consumer 的 `CultistRitual.delay`。静态核对了 Version4 load/reset 读写点与当前 session->Main 调用顺序；未运行测试、构建、verifier 或场景，`designStatus: proposed`、`verificationStatus: not-run` 保持不变。

## 2026-10-04 Implementation Follow-up: Angler Quest Transient Reset

- 在 Angler Quest load projection 后新增 `Main.anglerQuestFinished = false`。该布尔量不在存档区段中，而 Version4 `WorldFile.ResetTemps()` 会在清世界时清零；新的 session 发布覆盖了持久化的每日完成者名单和任务索引，但不会默认经过完整旧式 `clearWorld` 调用链。显式清零使跨 session 发布不会继承上一世界的临时完成标记。
- 此变更补充前一节“Angler Quest Load Projection”记录：当时描述的是普通 session commit 尚未改动该标记的状态；本 follow-up 将它纳入受 load gate 保护的发布。`CultistRitual.delay` 仍无当前 runtime owner/consumer，不作占位投影。
- 仅静态核对 Version4 `ResetTemps` / load call sites 与当前 projection 顺序；没有运行测试、构建、verifier 或场景，`designStatus: proposed`、`verificationStatus: not-run` 保持不变。

## 2026-10-04 Implementation Follow-up: Cultist Ritual Load Owner Boundary

- Version4 `WorldFile.LoadWorld_Version2` 将 `CultistDelay` 解码到临时字段，成功 `SetOngoingToTemps()` 时写入 `CultistRitual.delay`（`Terraria.IO/WorldFile.cs:114,165,2154`）。该值由 `Terraria.GameContent.Events.CultistRitual.UpdateTime()` 每次 world time update 递减并在到期时调度祭坛检查/生成；`NPC` 的远古信徒祭坛被摧毁时调用 `TabletDestroyed()` 将延迟设为 43200（`Terraria.GameContent.Events/CultistRitual.cs:21-47,49-55`；`Terraria/Main.cs:13112`；`Terraria/NPC.cs:37528`）。
- 当前加载 API 已把该字段放入候选 session 的 `WorldTimeWeatherState.CultistDelay`，但当前 runtime 源码没有 `CultistRitual` owner、world-time 调用点或 NPC 祭坛行为；普通 `Main` scalar 镜像无法恢复上述运行语义。未新增占位 owner，也未把 session 数值伪称为已接入的运行状态。该行为闭合仍需包含 world-time 与 NPC 祭坛调用方的 runtime/host integration owner。
- 本轮只静态搜索当前 runtime 与逐段核对 Version4 调用闭包；未运行测试、构建、verifier 或场景。`designStatus: proposed`、`verificationStatus: not-run` 不变。

## 2026-10-04 Implementation Follow-up: Per-Reset Completion Marker

- `WorldStorageCoordinatorFactory.ResetLegacyWorld()` 以 `WorldGen.worldCleared` 判断本次 `clearWorld()` 是否成功；该标记此前只在清理函数末尾置 `true`，如果先前清理成功后下一次清理中途抛错，旧 `true` 会错误证明新一轮 reset 完成。
- `WorldGen.clearWorld()` 现于首个可失败清理动作前先将 `worldCleared` 置 `false`，仅在所有 reset 操作（包括 `Liquid.ReInit()`）正常返回后再置 `true`。这样多次载入/恢复失败的 reset 尝试各自拥有独立成功结果；生命周期 coordinator 仍以 load gate 保护 reset。
- 静态核对了 `ResetLegacyWorld -> clearWorld` 调用关系及两个标记写入点；未运行测试、构建、verifier 或场景，`designStatus: proposed`、`verificationStatus: not-run` 保持不变。

## 2026-10-04 Implementation Follow-up: Loaded File Metadata Projection

- `WorldMetadataLoadApi` 将 file metadata 区段提交到 `LoadedWorldSession.Metadata`，但发布路径此前只投影 descriptor identity，未将已读到的 revision/favorite 写回活动 `WorldFileData`。Version4 在读取 WorldFile metadata 后把其 `Revision` / `IsFavorite` 保存在活动 `WorldFileData.Metadata`；旧格式无 metadata 区段时则由宿主元数据扫描使用当前设置默认值。
- `LoadedWorldSession.Metadata` 现在以 nullable 区分“文件没有 metadata 区段”和“区段显式值”；有区段时 `LegacyWorldSessionProjection` 把两字段写入 runtime `WorldFileData`，缺失时保留宿主在加载前提供的值。没有改动路径、cloud identity 或 host file discovery。
- Runtime `WorldFileData` 新增 `MetadataRevision` / `MetadataIsFavorite` 及 `ApplyLoadedMetadata`，作为本地运行时 file-data owner 的显式映射；该数据仍不属于 `WorldSessionRestoreState`。
- 静态对照了 Version4 `WorldFile.GetAllMetadata`/`FileMetadata.Read`、当前 decoder 与 session publication 调用点；没有运行测试、构建、verifier 或场景，`designStatus: proposed`、`verificationStatus: not-run` 保持不变。

## 2026-10-04 Implementation Follow-up: Storage Gate Lease Contract

- `RegisterGeneratedWorldLoadHandler` 把 `IWorldStorageIoGate.Enter()` lease 覆盖到完整 recovery 调用，并在恢复结果对外通知前释放。外层入口将 acquisition 失败映射为 `loadFailed`；因此 port 需要保证 `Enter` 抛错时没有残留持锁，也需要保证成功 lease 按 adapter 线程约束释放时不抛错，避免已完成发布在释锁异常时被误报为普通 load failure。
- `IWorldStorageIoGate` 现明确要求上述异常边界以及 lease 的幂等释放语义。当前 `WorldStorageIoGate` 的 semaphore / monitor lease 以 `Interlocked.Exchange` 至多释放一次；monitor 变体仍需在获取线程释放，遵守该约束时退出不抛错。
- 只检查了 port 与内置 adapter 的控制流及调用作用域；没有运行测试、构建、verifier 或场景，`designStatus: proposed`、`verificationStatus: not-run` 保持不变。

## 2026-10-04 Implementation Follow-up: World Manifest Restoration

- 当前 `WorldSpawnLoadApi` 已把存档中的 `WorldManifestJson` 保留在候选 `LoadedWorldSession`，但会话发布未恢复 `WorldGen.Manifest`；而 `WorldGen.clearWorld()` 会先将其重置为空 manifest。Version4 `WorldFile.LoadWorld_Version2()` 在读取 spawn/seed 相关尾部数据时执行 `WorldGen.Manifest = WorldManifest.Deserialize(versionNumber < 299 ? "" : reader.ReadString())`（`D:/TRbackup/Version4/Terraria.IO/WorldFile.cs:2567`）。当前生成器以 `WorldGen.Manifest.GenPassResults` 暴露 pass 历史，快照保存/恢复也读写该对象。
- `LegacyWorldSessionProjection.PublishCoreState()` 现将候选 session 中的 JSON 反序列化至当前 generation owner；缺少 manifest 时传空串，与旧版早期文件回退为空 manifest 的语义一致。该发布发生于受 load gate 保护的 session commit，在恢复后的 finalization 之前，失败恢复清理仍会再次重置 owner。
- CreativePowers API 当前只接受新生成世界的空 payload 并将 session 标记为无 powers；仓库没有该 session flag 的运行时消费者/owner，因此没有创建占位字段或声称完成 CreativePowers runtime 恢复。
- 只静态核对了解码→session→发布链、旧版读档赋值、clearWorld reset 及 manifest 当前消费者；按要求未运行测试、构建、verifier 或场景。`designStatus: proposed`、`verificationStatus: not-run` 保持不变。

## 2026-10-04 Implementation Follow-up: Reset Entity Slot Identity

- 失败恢复会走 `WorldGen.clearWorld() -> Main.ResetWorldStorage()` 重建 runtime arrays。对照 Version4 `WorldGen.clearWorld()` 的循环，旧路径为每个 item slot 和 projectile slot 重建对象后写入对应 `whoAmI`，并为每个 dust slot 写入 `dustIndex`（`D:/TRbackup/Version4/Terraria/WorldGen.cs:6622-6648`）；Version4 `Main` 初始化也将整个 6001 项 dust 表按数组下标编号（`D:/TRbackup/Version4/Terraria/Main.cs:3463`）。当前 reset 虽正确编号 NPC，却曾为 item/projectile/dust 使用默认编号零的新对象。
- `MainHost.ResetWorldStorage()` 现按各自数组下标初始化 `WorldItem.whoAmI`、`Projectile.whoAmI` 与 `Dust.dustIndex`，使 clear/reset 后槽身份与数组位置一致；数组范围保持当前运行时既有的 400/1000/6001 项。
- 只静态核对了旧 reset/初始化赋值与当前 runtime 字段声明、重建路径；没有运行测试、构建、verifier 或场景。`designStatus: proposed`、`verificationStatus: not-run` 保持不变。

## 2026-10-04 Implementation Follow-up: Legacy Quest Defaults

- `ReadQuests()` 对 WorldFile 88–94 返回 `null`，因可选 Quest API 不执行，候选 session 的 `CultistDelay` 会错误保留为零；Version4 在早期文件没有该字段时明确设为 86400（`D:/TRbackup/Version4/Terraria.IO/WorldFile.cs:2246-2254`）。版本 95–106 虽有 Quest 前缀，但 Version4 会从已读取的 invasion type/size 调用 `Main.FakeLoadInvasionStart()`，恢复 `invasionSizeStart`，当前 decoder 原先一直将它置零（`D:/TRbackup/Version4/Terraria.IO/WorldFile.cs:2233-2243`；`D:/TRbackup/Version4/Terraria/Main.cs:12686-12712`）。
- Decoder 现将 progression snapshot 传给 Quest 解码：版本 88–94 生成空 quest defaults（含 CultistDelay 86400），版本 95–106 用同一旧版公式按入侵类型与大小计算 `invasionSizeStart`；版本 107+ 仍直接读取存档值。这样缺段/旧字段默认值进入候选 session，之后由既有 `WorldQuestLoadApi` 一次性提交。
- 只静态核对了旧版分支、当前 decoder/API/session 写入链与公式；按要求没有运行测试、构建、verifier 或场景。CultistDelay 的运行时仪式更新与 NPC 祭坛调用者仍缺 owner，`designStatus: proposed`、`verificationStatus: not-run` 不变。

## 2026-10-04 Implementation Follow-up: Early Load Setup Exception Flags

- `RunGeneratedWorldLoad()` 在创建 runtime bindings 或 recovery effects 时也可能抛错；这发生于 `WorldLoadRecoveryCoordinator.Recover()` 开始之前，因而不会进入 lifecycle projection。该 catch 原先只设置 `WorldGen.loadFailed`，`worldBackup` 和 `worldCleared` 会保留上一次加载的静态镜像。
- catch 现在同时从本次新建的 `WorldLoadLifecycleComponent` 投影 `worldBackup` 与 `worldCleared`。若 recovery 已启动，后续 `RecoverUnexpectedFailure()` 仍负责 gate、重置和最终 lifecycle 投影；该修正不提前清理已发布的 active world。
- 静态检查了 bindings/effects 创建顺序、catch 路径及 `RecoverUnexpectedFailure()` 的投影调用；未运行测试、构建、verifier 或场景。生产宿主仍需在正确的上层组合入口注册 `RegisterGeneratedWorldLoadHandler`，NPC/TileEntity/Bestiary runtime materializer 与 Cultist ritual owner 仍未闭合；`designStatus: proposed`、`verificationStatus: not-run` 不变。

## 2026-10-04 Implementation Follow-up: Load Recovery Host Composition Boundary

- 按优先级继续检查世界加载恢复接线。当前 `NSSLC.WorldGeneration` 只引用 WorldSession 与 SpatialSimulation，`NSSLC.Infrastructure.WorldStorage` 引用 Application 与 WorldGeneration；`NSSLC.Tools.WorldGeneration` 与 `NSSLC.Tools.Simulation` 都引用两个基础项目。静态调用点核对发现后者的 `Program` 确实调用 `RegisterGeneratedWorldLoadHandlerWithRuntimeTileMap`，并在 finalization callback 中恢复其 runtime NPC store；前者只做文档/section roundtrip。故“没有任何 caller”不再准确。
- `NSSLC.Tools.Simulation` 是 headless finite-simulation host，不是完整游戏/server host。它使用 session、tile map、core state、container/sign/pressure plate projections 和一份只含 Guide、Old Man、Zombie 的 simulation content catalog；现仅精确映射这三种 Version4 legacy type name，其他 NPC 名称会因缺少受支持的 definition 而拒绝物化。TileEntity runtime projection 已接入 WorldStorage 发布链并派发 Version4 中现有的空 `OnWorldLoaded` 回调；训练假人和 logic sensor 有限行为已由 `ActiveTileEntityTickPhase` 实现，其已提交状态同步到 runtime。该 host 的 save preparation 现于共享 I/O gate 内将 runtime TE snapshot 提交到 session owner，再从 session 捕获存档快照。该 host 仍未实现完整游戏宿主生命周期、Version4 全量 `PerformUpdates` 行为或每次 runtime mutation 后立即提交到 session。此 caller 让加载恢复在该有限 simulation 范围可达，但不能作为所有 world state 已物化的证据。
- 本次继续在该宿主中加入 Version4 旧名称 `Guide`、`Old Man`、`Zombie` 到现有三条 content definition 的显式映射（Version4 `NPCID.cs:10538,10551,10565`），并将 NPC hydration 改为要求已发布 session，先构建候选 slot store、全部身份/类型与 slot 验证通过后再一次替换发布。未支持的名称或缺少 definition 仍返回 invalid-data，且失败不会留下部分 runtime NPC store。终态结果只有在旧世界已清空或仍有 reset 未完成时才清空 host store；发布前普通读取失败及仅丢弃候选 session 的失败保留旧 active NPC。setup/recovery exception observer 不清理，以免没有终态 lifecycle 证据时误清运行时 NPC。该 map 只覆盖 headless simulation content，不替代 Version4 全量 `NPCID.FromLegacyName`。
- 完整游戏宿主仍需在正确组合入口复用 recovery registration，并提供全量 NPC definition/catalog、TileEntity update behavior 与逐次 runtime-to-session commit、Bestiary gameplay/network owner、宿主特定清理及最终收尾；现有 `OnWorldLoaded` 调度仅覆盖源码中为空的 callback。本段为项目依赖、调用点和源码路径的静态证据，不代表运行时行为等价或验证完成。
- 未运行测试、构建、verifier 或场景；`designStatus: proposed`、`verificationStatus: not-run` 保持不变。

## 2026-10-04 Implementation Follow-up: Storage Gate Acquisition Failure Flags

- `RegisterGeneratedWorldLoadHandler` 的外层 catch 覆盖 cancellation-token provider 和 I/O gate acquisition；这些异常可能先于 `RunGeneratedWorldLoad()` 的 session/lifecycle 创建，因此不会经过其 per-load flag 投影。此前 catch 只设置 `loadFailed`，造成 `worldBackup` / `worldCleared` 暂留前一次加载的值。
- 现在跟踪 `RunGeneratedWorldLoad()` 是否已返回：在它返回前抛错时，外层 catch 将 `worldBackup` / `worldCleared` 投影为新 attempt 的默认 false；若异常来自 recovery 返回后的 lease 释放，则保留 recovery 已投影的两个结果标记，只报告 `loadFailed`。这保留了 `IWorldStorageIoGate` 关于 lease release 不抛错的契约，同时避免释放异常覆盖已完成的 reset/backup 结果。
- 只静态检查了 handler 的 provider、acquire、recovery、dispose 与 catch 顺序；未运行测试、构建、verifier 或场景。`designStatus: proposed`、`verificationStatus: not-run` 保持不变。

## 2026-10-04 Implementation Follow-up: Simulation NPC Housing Hydration

- `WorldNpcLoadApi` 已将存档的 GivenName、homeless、home 坐标、town variation 与 `homelessDespawn` 保存在 `WorldNpcState`；headless simulation 的 `RuntimeNpcEntity.Hydrate` 现把 GivenName 原样投到 `NpcGivenNameComponent`，把住房关系投到 `TownHousingRelationStateComponent`，把 variation 投到 `NpcPresentationStateComponent`。空名称仍为空，不触发名字随机生成。Homeless 或含负数的未分配 home 坐标不成为当前住房关系；原始值仍在 `SavedState`。`homelessDespawn` 归住房组件，不再误填 `NpcSpawnAndCritterStateComponent.SpawnedFromStatue`。对应旧版读档字段见 Version4 `Terraria.IO/WorldFile.cs:2979-2988`。
- 该 simulation catalog 目前仅支持 Guide、Old Man、Zombie；这三者的旧版默认 `housingCategory` 均为 0，且 `lookForHomeTimeout` 默认值为 0。宿主扩充 NPC definitions 前，hydration 不推断宠物类别或未建模的 profile。Variation 缺失时使用旧版默认 0。GivenName 组件保留原字符串（null 转为空串），但尚未提供旧版 `FullName` / `GivenOrTypeName` 在空 GivenName 时回退到本地化类型名的查询；字段默认及名称回退见 Version4 `Terraria/NPC.cs:6411-6413,6580-6615,8121,8149,8221-8223`。
- 这项修改只恢复候选 NPC 的住房关系字段，不实现 TownHousingRegistry 与实体关系的重验，也不替代旧版存档读取后的房间修复、KickOut、住房规则和反馈闭包。失败回滚仍由既有 runtime NPC store 的候选构建/一次性替换与 world-load 终态观察器负责；完整游戏/server host、全量 NPC catalog、TileEntity update/save-back lifecycle 与 Bestiary owner 仍未闭合。
- 只静态对照了 Version4 `WorldFile.LoadNPCs` / `NPC.SetDefaults`、当前 `WorldNpcLoadApi`、housing/presentation 组件及 simulation hydration 调用路径；未运行测试、构建、verifier 或场景。`designStatus: proposed`、`verificationStatus: not-run` 保持不变。

## 2026-10-04 Implementation Follow-up: Finalization Cancellation Boundaries

- 标准 `FinalizeLoadedWorld` 先执行 NPC spawn 概率初始化、Skyblock tile scan、可选 slime-rain restore、WorldId NPC 初始化，再调用宿主 finalization。此前仅在第一个步骤前检查取消；若取消在同步步骤中到达，后续独立收尾仍会继续，最终才由 recovery coordinator 检出并 reset。
- 现在每个同步收尾步骤返回后立即检查同一 cancellation token；已开始且无 token 入口的步骤不能被中断，但取消后不会启动后续收尾或 host callback。返回 Canceled 仍交给 coordinator 的现有失败清理路径。
- 仅静态核对了步骤顺序与 coordinator 对 finalization 取消的 reset 分支；未运行测试、构建、verifier 或场景。`designStatus: proposed`、`verificationStatus: not-run` 保持不变。

## 2026-10-04 Implementation Follow-up: TileEntity Anchor Validation

- Version4 `WorldFile.LoadTileEntities` 在读入并按 anchor 替换重复实体后，会移除越界或 `TileEntity.manager.CheckValidTile` 拒绝的实体（`Terraria.IO/WorldFile.cs:3400-3435`）。`WorldTileEntityLoadApi` 现于 tile section 提交后读取 anchor `TileCellState`；WorldFile 类型码 0–7 按对应 legacy entity 的 active、tile type 与 frame 规则验证，明确无效的实体不写入 session store。该规则由 Infrastructure validator 提供，Application API 不直接依赖运行时静态 Tile。
- 类型码 8–10 的 anchor validator 返回 Unknown 并保留记录；Version4 `TEDeadCellsDisplayJar`、`TEKiteAnchor` 与 `TECritterAnchor` 的 tile-validity 来源为占位体，不能据此判有效或无效。类型码 8 的扩展读写为空；9、10 继承空的 leashed-anchor serializer。codec 只接受已注册类型码 0–10，其余值拒绝。
- 当前切片尚未实现 TileEntity runtime 类型 materialization 或 `OnWorldLoaded` callbacks。只静态对照了 Version4 的 anchor 检查顺序、各已实现 `IsTileValidForEntity` 与当前 tile/session commit 顺序；未运行测试、构建、verifier 或场景，`designStatus: proposed`、`verificationStatus: not-run` 不变。

## 2026-10-04 Implementation Follow-up: TileEntity Load Identity and Duplicate Anchors

- Version4 `TileEntity.WriteInner` 会写入持久化 ID，但 `TileEntity.ReadInner` 读出的值随后在 `WorldFile.LoadTileEntities` 被覆盖为输入序号；`TileEntitiesNextID` 设为 section 原始计数。故 runtime 身份由记录顺序产生，而非存档 ID。加载时发现重复 anchor 会移除此前记录并添加当前记录；之后才过滤出界或无效 anchor，因而被过滤记录仍计入 `NextId`。
- `WorldFileTileEntityCodec.Decode` 现消费但忽略存档 ID，以记录序号建立 runtime ID，并把每个 anchor 的值替换为最后一条记录。类型码 8–10 使用目标源码中的空扩展 serializer；生成编码不再为类型码 8 插入虚构的 item tuple。`WorldTileEntitiesPrepared` 将实体列表与原始 `NextId` 一起传到 commit；`TileEntityStore.Replace` 原子构建新索引后保留该计数，即使 anchor 过滤留下 ID 空洞。manager 注册的 Kite/Critter Anchor 现在可以进入候选 session，但其 tile validity 仍为 Unknown，runtime materializer 仍缺失。
- 解码不再拒绝 Logic Sensor 的原始枚举 byte（Version4 值 7 是 Liquid）、负 item type（旧 `Item.netDefaults` 将其作为特殊物品编码），或 Doll/Hat Rack 未使用的高位；旧读路径保留前者并不检查这些位。锚点规则和应用过滤后的 session 数据仍保持显式。
- 只静态检查了 Version4 `Terraria.DataStructures/TileEntity.cs:172-214`、`Terraria.IO/WorldFile.cs:3385-3444`、`TileEntitiesManager.RegisterAll`、各已解码类型的 extension-data 读写、`TELogicSensor.cs:399-406`、`TEDisplayDoll.cs:153-205`、`TEHatRack.cs:82-109`、`Item.cs:1030-1070`、`TEDeadCellsDisplayJar.cs:15-26`、`TELeashedEntityAnchor.cs:5-12` 与 Kite/Critter Anchor 声明，以及当前 codec→API→store 调用链。未运行测试、构建、verifier 或场景；TileEntity runtime materialization、`OnWorldLoaded` 反馈及完整宿主接线仍未闭合，`designStatus: proposed`、`verificationStatus: not-run` 不变。

## 2026-10-04 Implementation Follow-up: Fail-Closed World Reset Gate

- `FinishTerminalFailure` 与 `RecoverUnexpectedFailure` 在已发布或发布状态不确定时，都会先升起 lifecycle load gate 并投影到宿主，再调用 `ResetWorld`。此前即使该投影失败也继续清理活动世界，无法确认世界更新已暂停。
- 现在 gate projection 失败会阻止后续 destructive reset；恢复继续保留 `RequiresWorldReset` 与 gate-raised lifecycle 状态，并报告 cleanup failure。异常路径的终态投影仍会再尝试同步这些状态，但不能将重试成功当作 reset 已完成。默认 `RuntimeWorldLoadLifecycleProjection` 会先升起兼容 gate，完成其余 flags/session 投影后才降门。
- 只静态核对了两个 reset 调用点、`WorldLoadLifecycleSystem.BeginWorldReset`、默认 runtime projection 的升降门顺序与返回结果；没有运行测试、构建、verifier 或场景。宿主自定义 projection 及 failure-injection 行为仍未验证；`designStatus: proposed`、`verificationStatus: not-run` 不变。

## 2026-10-04 Implementation Follow-up: Serialize Terminal Load Observation

- 注册 handler 原先只在 `RunGeneratedWorldLoad` 内持有 `recoveryLock`；退出锁并释放 I/O gate 后才调用 `resultObserver`。当前 simulation host 的 observer 会在 reset/cleared 结果上清空 runtime NPC store。另一轮 load 可在这段间隙先发布并 hydration 新世界，随后被前一轮 observer 清空。
- handler 现把每轮 load 与其 terminal observer 放在同一 per-registration lock 范围内，但仍在 observer 前释放 I/O gate。这样下一轮 load 会等宿主终态清理完成，observer 仍可自行调用需要该 I/O gate 的操作。observer 异常仍按原规则只作诊断，不回滚已完成的 load 结果。
- 只静态核对了注册 handler 的锁/gate/observer 顺序及 `NSSLC.Tools.Simulation.Program` 的 store reset/hydration 调用点；没有运行测试、构建、verifier 或场景。锁外部的 unload/clear 调用和完整游戏宿主仍需 integration review；`designStatus: proposed`、`verificationStatus: not-run` 不变。

## 2026-10-04 Implementation Follow-up: Publish Host NPC State Under the Load Gate

- Version4 `WorldFile.LoadWorld` 在解码与修复完成后才升起 `isGeneratingOrLoadingWorld`，随后执行液体 settle，降门后才运行 spawn chance、Skyblock、slime-rain 和 world-specific NPC finalization（`D:/TRbackup/Version4/Terraria.IO/WorldFile.cs:747-788`）。新路径的 `WorldLoadRecoveryCoordinator` 在候选完整且校验后升门，再由 `PublishRuntimeWorldState` 调用宿主剩余状态发布；门保持升起直到 settle 完成。
- Simulation host 原先把存档 NPC 物化放在 `finalizeLoadedWorld`，因此该 store 在降门后才切换；成功 reset 时，`resultObserver` 也在降门后才清理宿主 NPC store。现将候选构建/原子替换移到发布回调，并新增可选 `resetHostWorldState` effect：全局 legacy reset 成功后、终态 lifecycle 投影释放 load gate 前清理宿主 NPC store。reset 未完成时门继续升起；observer 仅记录结果，不再承担一致性清理。
- 候选 NPC 类型/身份验证失败仍由 `RuntimeNpcStore.Hydrate` 的先构建后替换保证旧 store 不被部分覆盖；发布失败、settle 失败或 finalization 失败会走 gate-raised reset，清空已发布的 Simulation store。宿主 reset effect 失败时 coordinator 保留 reset-required 与升起的 gate。
- 静态核对了 Version4 settle/finalization 顺序、当前 recovery publish/reset 调用顺序及 Simulation NPC store 替换点；未运行测试、构建、verifier 或场景。完整游戏/server host 的 NPC/TileEntity/Bestiary runtime lifecycle 与最终行为等价仍待 integration review；`designStatus: proposed`、`verificationStatus: not-run` 不变。

## 2026-10-04 Implementation Follow-up: WorldFile Transient Reset Ownership

- Version4 `WorldGen.clearWorld()` 调用 `WorldFile.ResetTemps()`；该方法除清理文件解析临时量外，还清空 `Main.anglerWhoFinishedToday` 并复位 `Main.anglerQuestFinished`（`D:/TRbackup/Version4/Terraria.IO/WorldFile.cs:627-641`，调用点 `WorldGen.cs:6472`）。当前生成 runtime facade 原先让 `ResetTemps()` 抛 `NotSupportedException`，而 `ResetGenerationState()` 直接清理这两个 Main 字段。
- runtime facade 现实现其实际持有状态对应的两项清理，`WorldGen.clearWorld()` 显式调用它们；从 `ResetGenerationState()` 移除重复赋值。失败恢复仍经 `WorldGen.clearWorld()` 在 lifecycle gate 下执行。当前 runtime facade 未实现其余旧文件解析临时量，`SetTempToOngoing()` / `SetOngoingToTemps()` 仍由存储宿主负责；不据此宣称完整旧 `WorldFile` 临时状态迁移。
- 静态搜索确认新 reset 方法只有 `clearWorld()` 调用点，候选 session 发布仍由 `LegacyWorldSessionProjection` 写入 Angler 名单及任务索引；旧版 reset 不清任务索引，保持该语义。没有运行测试、构建、verifier 或场景；`designStatus: proposed`、`verificationStatus: not-run` 不变。

## 2026-10-04 Implementation Follow-up: Reset Attempt Flags Before Load Preparation

- `Runtime.WorldFile.LoadWorld()` 在进入 `Main.LoadWorld()` handler 前还会初始化随机源、锁定菜单背景并运行节日钩子。这些准备步骤抛错时原 catch 会设 `loadFailed`，但没有新的 coordinator lifecycle projection，故 `worldBackup` / `worldCleared` 可能保留上轮镜像。
- 每次 facade load 入口现将 `loadFailed`、`worldBackup`、`worldCleared` 置为新 attempt 默认值；后续恢复协调器仍负责投影本轮 backup/reset 结果，前置步骤异常则只把本轮 `loadFailed` 置回 true。此写入位于恢复 handler 外层，因此覆盖 coordinator 尚未启动的准备失败路径。
- 静态核对了 `WorldFile.LoadWorld()` 准备步骤、catch 与 handler 调用顺序，以及 coordinator 按 lifecycle 投影的标记路径；没有运行测试、构建、verifier 或场景。旧 facade 外部直接写入 lifecycle 字段的调用者未作动态核验；`designStatus: proposed`、`verificationStatus: not-run` 不变。

## 2026-10-04 Implementation Follow-up: Publish and Reset Persisted Bestiary State

- `WorldBestiaryLoadApi` 已将 kill counts、seen NPC IDs、chatted NPC IDs 解码到候选 session 的 `WorldNpcHistoryStateComponent`，此前没有 runtime owner 接收它们。现新增 `WorldBestiaryTracker` 保存不可变快照，并提供对应查询；发布前复制数据、拒绝负 kill count、去重 seen/chatted ID，随后以单次 volatile state 替换提交。
- `LegacyWorldSessionProjection.PublishCoreState()` 在受 load gate 保护的 session commit 中发布上述三组状态；`WorldGen.clearWorld()` 调用 tracker reset。因而未发布候选失败继续保留旧活动世界数据；发布开始后的 settle/finalize 失败则由 gate-raised reset 清除候选 Tracker state。
- 该 owner 只承接现有存档状态及读取，不实现 Version4 的 gameplay kill/sight/chat tracking、achievement、player-join network synchronization 或完整 Bestiary tracker API。完整游戏/server host 对这些行为的接线仍需 integration review。只静态核对 API→session→projection 与 clear/reset 调用顺序；没有运行测试、构建、verifier 或场景，`designStatus: proposed`、`verificationStatus: not-run` 不变。

## 2026-10-04 Implementation Follow-up: TileEntity Typed Runtime Gap (Historical Finding)

- 静态检查发现当前 `Runtime/TileEntity.cs::Register()` 对所有 placement hook 都只创建基类 `TileEntity`；`TryGetAt<T>()` 随后对该基类对象执行 `as T`。`GameContent.Tile_Entities` 下的 TE 兼容类型没有继承该基类，runtime `PerformUpdates()` 也为空。因此 WorldGen 中按 `TEItemFrame`、`TEWeaponsRack` 等类型读取实体的调用，不能由当前注册表返回这些兼容类型；把 session 的 `TileEntitySnapshot` 复制进这个基类字典也不会接通类型化读取、持久内容或更新行为。
- `WorldTileEntityLoadApi` 仍只把已校验快照提交到 session 的 `TileEntityStore`；完整 materializer 必须建立 type-code 到具体 runtime owner 的映射，并恢复各类型字段/物品及 update 生命周期。当前源码没有可直接复用的完整 TE runtime 类，故保留此 integration gap，不添加仅复制快照的假性接线。静态证据未运行测试、构建、verifier 或场景；`designStatus: proposed`、`verificationStatus: not-run` 不变。

## 2026-10-04 Implementation Follow-up: TileEntity Runtime Publication

- 当前 `Runtime/TileEntity.cs` 的 placement hooks 已注册 TE 子类型；若同一 anchor 之前已有基类占位记录，类型化注册保留原 ID 并用具体类型替换。runtime ID 单调递增，并在 load restore 时接续 session 保存的原始 `NextId`，因此 anchor 校验过滤留下的 ID 空洞不会导致新放置实体复用已有 ID。
- 新增 `LegacyWorldTileEntityProjection`，在 load gate 内、tile map 发布之后构造候选 TE runtime 列表，再一次替换 runtime 字典。映射覆盖 Version4 type code 0–10：训练假人 NPC index、item frame/weapons rack/food platter item、logic sensor 字段、display doll 19 个 item slot 与 pose、hat rack 4 个 slot，以及无额外 payload 的 pylon/jar/kite/critter 类型。anchor、唯一 ID/位置、物品槽数量和 NextId 在替换前核验；取消或解码形状不一致不会部分替换旧 runtime 表。World reset 已由 `Main.ResetWorldStorage()` 经 `TileEntity.Clear()` 清除此 owner。
- `WorldFile.LoadTileEntities` 在所有实体加入索引后逐个派发 `OnWorldLoaded()`；目标源码中基类及 Kite/Critter 共用 override 均为空。runtime projection 现于候选列表原子替换后按原始记录 ID 顺序派发该回调；取消或回调异常继续交给 coordinator 的 gated recovery reset。此处只复现已有空回调的调用合同，不代表新增了 callback gameplay effects。训练假人/logic sensor update 行为与 runtime TE 后续修改回写 `TileEntityStore` 仍未实现，因此不是完整持久化闭环；类型 8–10 的锚点合法性仍保留 validator 的 Unknown 决策。完整游戏/server host 仍需接入 update owner 和存档 snapshot source。
- 只静态对照 `WorldFile.LoadTileEntities` 的索引后回调顺序、Version4 `TileEntity.OnWorldLoaded` 及两个 anchor override 的空实现，以及 recovery 对 publication failure 的 gated reset 路径；未运行测试、构建、verifier 或场景。`designStatus: proposed`、`verificationStatus: not-run` 保持不变。

## 2026-10-04 Implementation Follow-up: TileEntity Runtime Save Snapshot

- `NSSLC.Tools.Simulation` 的 save source 先前始终从 `TileEntityStore` 编码加载时快照，即使 legacy runtime entity 已被放置或修改也不会进入保存结果。runtime `TileEntity` 现在在 owner thread 上捕获不可变、按 ID 排序的 typed `TileEntityStoreSnapshot`，包含原始 `NextId`、物品槽、NPC index、logic sensor 状态及 display doll pose；未知子类型、重复 ID/anchor 和越界 ID 会 fail closed。
- 初版 `LoadedWorldPersistenceSnapshotSource` 曾直接接收 runtime TE snapshot source；后续收口改为由 save preparation 在共享 I/O gate 内提交 runtime 状态到 session owner，再由 session source 编码。该先后变化在 2026-10-04 `TileEntity Runtime Snapshot Commit Under Save Gate` follow-up 记录。
- 该接线覆盖有限 Simulation host 的 WorldFile 保存快照，不代表完整游戏/server host 的所有 placement、item mutation 与 remove 入口都已映射为领域命令；其余宿主必须在 owner thread 提供同类 gated reconciliation，并继续落实实际 TE update owner。`PerformUpdates` 的完整旧行为、类型 8–10 tile validity 与 `OnWorldLoaded` 非空效果仍未闭合。
- 只做了源码/调用链静态检查；没有运行测试、构建、verifier 或场景。行为验证仍 `not-run`，`designStatus: proposed` 不变。

## 2026-10-04 Implementation Follow-up: TileEntity Runtime Snapshot Commit Under Save Gate

- 直接从 runtime 为 WorldFile 编码会绕过 `TileEntityStore` 领域 owner；将快照同步放在 Program 调用 Save 前又会留下变换/I/O 屏障之间的竞态。现新增 `IWorldSaveSnapshotPreparation`，`WorldSaveSnapshotCoordinator.Save` 在等待 transform 静止并持有共享 I/O gate 后、调用 session snapshot source 前执行准备步骤。
- Simulation 的 `LegacyWorldTileEntitySavePreparation` 在创建它的 owner thread 捕获 runtime TE 表，校验 session 已 settled/published、tile map 尺寸、WorldFile 边界、实体数量和 `NextId` 单调性，然后通过 `TileEntityStore.CommitRuntimeSnapshot` 原子替换领域状态并重建更新 schedule。替换保留同 ID logic sensor 的非持久 counted-data；取消/验证失败不会提交候选快照，并令本次保存失败。后续编码只从 session `TileEntityStore` 读取。
- 若后续编码或文件写入失败，runtime reconciliation 已提交到内存中的 session；这反映实际 runtime owner 状态，但不代表文件保存成功。该方案闭合 Simulation 的 save-boundary TE 持久化；逐次 runtime mutation 的即时领域提交、完整 `PerformUpdates`、完整游戏宿主注册仍未实现。
- Version4 `TEDeadCellsDisplayJar.IsTileValidForEntity`、`TEKiteAnchor.IsTileValidForEntity` 与 `TECritterAnchor.IsTileValidForEntity` 在当前 `D:/TRbackup/Version4` 源树都是 `return new bool()` 占位体；全盘检索未找到这些类的第二份源码。因此类型码 8–10 validator 继续返回 `Unknown`，不从 tile type/frame dimensions 猜出有效性规则。
- 只静态核对了 `WorldSaveSnapshotCoordinator.Save` 的 wait→gate→prepare→capture 顺序、Simulation 调用点、TileEntityStore replace/index/schedule 和 Version4 有效性方法源码；未运行测试、构建、verifier 或场景。`designStatus: proposed`、`verificationStatus: not-run` 保持不变。

## 2026-10-04 Implementation Follow-up: Restore Signed TileEntity Item Codes

- Version4 `Item.netDefaults` 将 TileEntity 有符号 `Int16` 记录码 `-1..-18`、`-19..-24`、`-25..-48` 分段映射到具体物品默认值（`Terraria/Item.cs:1030-1177`）；ItemFrame、WeaponsRack 和 FoodPlatter 直接以 `Int16` 存取 type/prefix/stack。此前 TileEntity projection 把负码原样赋给 runtime `Item.type`，运行行为与旧 `netDefaults` 不同；codec 保存时也因 `Type <= 0` 拒绝该记录。
- runtime `Item.netDefaults` 现在复现三个负码区间的映射，TE `RestoreItem` 对负 type 调用该 API；WorldFile encoder 允许非空 item 保留 signed-short 负 type，同时校验类型/stack 可编码范围。runtime 快照在旧加载语义下捕获映射后的正物品 ID；纯 session 编解码路径则仍可 round-trip 原始负码。
- 只静态对照 Version4 `Item.netDefaults` 与三个 TE `ReadExtraData`/`WriteExtraData` 字节顺序，并检查当前 projection→runtime item→snapshot→codec 路径；没有运行测试、构建、verifier 或场景。`verificationStatus: not-run`、`designStatus: proposed` 保持不变。

## 2026-10-04 Implementation Follow-up: Restore Signed TileEntity Item Prefixes

- Version4 的 ItemFrame、WeaponsRack、FoodPlatter 及带物品的其他 TileEntity 均按 `netDefaults(type) -> Prefix(prefix) -> stack` 恢复物品。`LegacyWorldTileEntityProjection.RestoreItem` 先前只在正数、目录范围内的类型分支调用 `Prefix`，因此负数网络类型码虽映射到正确物品，存档前缀仍会丢失。
- 现在所有分支都在完成类型默认值恢复后应用同一 prefix 规则，再恢复 stack；超出当前 PrefixID 范围的原始 byte 仍按既有兼容路径保留。负数类型的默认值映射与 Version4 `Item.netDefaults` 区间公式逐项相符。
- 静态核对了 Version4 三类物品 TE 的读取顺序、`Item.netDefaults` 的负数映射和当前 projection 顺序；没有运行测试、构建、verifier 或场景。`verificationStatus: not-run`、`designStatus: proposed` 不变。

## 2026-10-04 Implementation Follow-up: Restore Legacy Chest Item States

- Version4 `WorldFile.LoadChests` 接受有符号 stack：负值仍读取 item type/prefix，并把运行时 stack 规范为 `1`；item type 通过 `Item.netDefaults` 恢复，之后应用 prefix。当前 decoder 保留了有符号 stack 和 Int32 type，但 document validator 拒绝负 type，load API 与 runtime projection 还会拒绝负 stack；runtime projection 也没有应用 prefix 属性。
- 现允许持久化 DTO 保留负 stack 与有符号 type；运行时投影对正 stack 保持旧顺序 `netDefaults -> stack -> Prefix`，对负 stack保持 `netDefaults -> Prefix -> stack=1`。仍校验 stack 的 signed-short 范围、正 stack 的空 type，以及 runtime catalog 可恢复的正 type 范围；超出 PrefixID 范围的 byte 按现有兼容策略保留。
- 静态对照了 Version4 `SaveChests`/`LoadChests` 的写读规则、WorldFile decoder/validator/encoder、`WorldChestLoadApi` 和 legacy chest projection；未运行测试、构建、verifier 或场景。`verificationStatus: not-run`、`designStatus: proposed` 不变。

## 2026-10-04 Implementation Follow-up: Logic Sensor Edge and Removal Effects

- Version4 `TELogicSensor.Update` 对 Day/Night 只在状态升为 ON 时加入 tripwire；PlayerAbove 与 Water/Lava/Honey/Liquid 在任一状态变化时触发。已激活的后一组 sensor 被 `Kill` 移除时也会再触发一次。Simulation 的 `ActiveTileEntityTickPhase` 此前只处理升为 ON 的变化，且无移除效果。
- 现按旧分支规则处理两类状态边沿，并在移除已激活的 PlayerAbove/液体 sensor 时、从 session/runtime 索引删除前触发当前 `LogicSensorWiringSwitch`。store 的 `MutationRevision` 也会覆盖仅改变非持久 counted-data 的提交，而方法布尔结果继续只表示持久 ON 状态是否改变。
- 当前 wiring adapter 只遍历已建 wire mask 并切换 actuator 标记；未接入完整 `Wiring.HitSwitch` 设备效果、PlayerAbove 的传送屏蔽或网络 tile-square 广播，因此这是 Simulation host 的行为补齐，不代表完整游戏/server parity。只静态对照 Version4 `TELogicSensor.Update/ChangeState/Kill` 与当前 tick/store/projection 调用顺序；未运行测试、构建、verifier 或场景，`verificationStatus: not-run`、`designStatus: proposed` 保持不变。

## 2026-10-04 Implementation Follow-up: Reject Saves During Incomplete Load Recovery

- 发布后的 liquid settle/finalization 失败会要求 world reset 并保持 load gate；恢复 handler 随后释放共享 I/O gate。此前 `LoadedWorldPersistenceSnapshotSource` 只检查 session complete/published，未检查其 lifecycle 仍处于 loading、failed 或 `RequiresWorldReset`，所以后续 save 可在 reset 未完成时捕获该 session。失败 reset 过程中，runtime lifecycle projection 也会因 session 仍标记 published 而重新投影候选 session。
- `RuntimeWorldLoadLifecycleProjection` 现不会在 `RequiresWorldReset` 时发布/重新激活候选 session；`LoadedWorldPersistenceSnapshotSource.Capture` 仅接受已完成加载的 lifecycle，并拒绝全局 load gate、loading、load-failed 或待 reset 状态；TileEntity save preparation 使用相同的 session lifecycle 条件，避免拒绝保存前先提交运行时 TE 快照。`IWorldPersistenceSnapshotSource` 契约也明确要求各宿主的 snapshot source 拒绝未就绪世界。成功路径仍在 gate 释放、恢复正常后允许捕获；失败路径不能仅因 I/O lease 已释放就视为世界可保存。
- 只静态核对了 `FinishTerminalFailure` / `RecoverUnexpectedFailure` 的 reset 失败状态、兼容 gate 与 I/O gate 释放顺序、runtime lifecycle projection 和 snapshot capture 前置条件；按要求未运行测试、构建、verifier 或场景。外部自定义 snapshot source 仍需遵守同一 session-ready 契约；`designStatus: proposed`、`verificationStatus: not-run` 保持不变。

## 2026-10-04 Implementation Follow-up: Preserve a Load Gate Across Failed Attempts

- 当一次已发布加载的 reset 未完成时，协调器将旧 session 标记为 `RequiresWorldReset` 并保留全局 gate。下一次 handler 会新建候选 session；如果新尝试在发布前失败，其 lifecycle 原本没有 reset 要求，终态 projection 会按该 session 的 `IsGeneratingOrLoadingWorld=false` 降低旧 gate，让未清理的 runtime 可继续更新。
- `WorldLoadRecoveryCoordinator` 现在可接收 attempt 开始前已有 gate 的事实，并记录在该 session lifecycle。默认 runtime projection 在新候选尚未发布且没有完成 reset 时继续升起/保留 gate；候选成功发布并 settle 后，或 reset 成功并清除世界后，才允许释放。未继承 gate 的普通解析失败仍按原逻辑降门。
- 静态对照了每次生成候选 session 的 handler、`FinishTerminalFailure` 的未发布/已发布分支及 Version4 同样使用 `Task.Factory.StartNew(serverLoadWorldCallBack)` 的调用方式；本项修复跨 attempt gate 所有权，不改变旧异步入口。没有运行测试、构建、verifier 或场景；`designStatus: proposed`、`verificationStatus: not-run` 保持不变。

## 2026-10-04 Implementation Follow-up: Runtime NPC Encounter Reset

- `RuntimeNpcStore` 持有跨 NPC 的 `NpcDamageTrackingSystem`，其中保存活动/最近结束的 encounter、encounter ID 与当前 tick。读档发布会用候选 session 的 NPC 列表替换 store；失败恢复则经 `resetHostWorldState` 调用 `RuntimeNpcStore.Reset()`。此前这两个切换都只换 NPC slot store，没有清除 encounter tracker，可能把前一世界的 encounter 状态带到新 session。
- 现于 `RuntimeNpcStore.Hydrate()` 完成候选 NPC staging 并提交新 slot store 后清空 damage tracking；`Reset()` 也同时清空 tracker，与 Version4 `WorldGen.clearWorld()` 调用 `NPCDamageTracker.Reset()` 的生命周期效果对齐。未在候选校验成功前改动现有 tracker。
- 静态检查了当前唯一加载宿主 `NSSLC.Tools.Simulation` 的 hydrate/reset 接线、`NpcDamageTrackingSystem.Reset()` 清理字段，以及 Version4 clear-world 调用点；没有运行测试、构建、verifier 或场景。该项只闭合 Simulation 的 runtime NPC encounter 生命周期，production composition root 与完整 NPC 行为仍未闭合；`designStatus: proposed`、`verificationStatus: not-run` 保持不变。

## 2026-10-04 Evidence Correction: NPC Housing Load Repair

- 更正上方旧调用 DAG：Version4 `WorldFile.ValidateLoadNPCs()` 只读取并跳过 NPC section 数据以验证流位置，不调用住房检查。实际修复发生在 `LoadWorld_Version2 -> LoadWorld_LastMinuteFixes -> FixAgainstExploits`：对活动、非 homeless 的 town NPC（跳过 type 368 与 37）检查已存 home；失败时查 TownManager 中的 fallback room 并重试；仍失败时 `KickOut` 并设置 homeless。源码位置：`D:/TRbackup/Version4/Terraria.IO/WorldFile.cs:1913-1950,3025-3058`。
- 当前 Simulation 的 `RuntimeNpcStore.Hydrate()` 保留 session 中的 housing relation，但没有执行上述 post-load repair。虽有 `WorldGen.StartRoomCheck()` 和 `TownHousingRegistry`，当前 host hydration 回调运行在 `LegacyWorldLoadRecoveryEffects.PublishLoadedWorld()` 捕获 dimension compatibility snapshot 之前，而检查器读取该尺寸状态；若直接调用旧 wrapper，会有使用旧尺寸边界的风险。正确闭合还需在候选 NPC、住房 registry 与 runtime slot store 间定义一次性修复/提交，并先保证 room check 使用候选世界尺寸。
- 本次只更正报告调用证据并记录实现边界；没有改动住房或 load source，也没有运行测试、构建、verifier 或场景。此房屋修复仍属 `crossSubsystemOwner: integration-review`；`designStatus: proposed`、`verificationStatus: not-run` 保持不变。

## 2026-10-04 Implementation Follow-up: Reject Saves from Replaced World Sessions

- 加载发布会将 `WorldStorageCoordinatorFactory.ActiveGeneratedWorldSession` 和 `Main.ActiveWorldSession` 切换到新 session，但旧 `LoadedWorldSession.IsPublished` 与已完成 lifecycle 保持不变。此前已绑定旧 session 的 `LoadedWorldPersistenceSnapshotSource` 只检查这些历史状态，仍可能在后续世界加载完成后从旧 session 编码并保存。
- 快照源现在要求显式提供当前 active-session 查询，并在捕获前以引用身份确认自身 session 仍是当前发布世界；Simulation 宿主绑定到 factory 的 active-session owner。该检查与 ready lifecycle 校验共同拒绝过期 session。
- 仅静态追踪了发布时 active-session 替换、Simulation 保存 coordinator 持有的 session 和快照捕获前置条件；未运行测试、构建、verifier 或场景。其他宿主必须向同一 snapshot source 提供其权威 active-session 查询；`designStatus: proposed`、`verificationStatus: not-run` 保持不变。

## 2026-10-04 Implementation Follow-up: Serialize the Legacy Async Load Callback

- `WorldGen.serverLoadWorld()` 对每次请求都调用 `Task.Factory.StartNew(serverLoadWorldCallBack)`；回调先进入 `Runtime.WorldFile.LoadWorld()` 重置三项全局结果标记，再进入 factory 内部恢复锁。该锁不能保护入口重置或成功后 `Hooks.WorldLoaded()`，所以并发 callback 可重叠静态世界生命周期并相互覆盖标记。
- `serverLoadWorldCallBack()` 现在以 WorldGen 生命周期门串行覆盖读档入口、结果标记和成功后的 `WorldLoaded` hook；同线程递归 callback 会明确失败，其他并发请求等待当前 callback 完成。锁在 `finally` 释放，异常不会永久占住入口。
- 静态检查了唯一 runtime `WorldFile.LoadWorld()` 调用点、异步 task 创建点和锁覆盖区间；没有运行测试、构建、verifier 或场景。该门只序列化此 legacy callback，不涵盖绕过它的外部 reset/unload/generation，也不建立 multi-world 支持；`designStatus: proposed`、`verificationStatus: not-run` 保持不变。

## 2026-10-05 Implementation Follow-up: NPC Housing Post-Load Repair

- `RuntimeNpcStore.Hydrate()` 现在在构造 runtime NPC 候选时执行有限 Simulation 宿主的 post-load housing repair：跳过非 town、已 homeless、legacy type 368 和 Old Man type 37；按 `homeTileX, homeTileY - 1` 验证原房间；失败后按 NPC type 查询候选 session 的 TownHousing registry 并重试；仍失败时保留旧版 fallback 坐标行为、移除房间登记并标记 homeless。有效 fallback 只修正 home 坐标，不重复提交 registry assignment。
- `WorldGen.IsHousingRoomValidAt()` 复用 `HousingValidationSystem` 与原有 max-room-tile/max-room-size、60 格最小房间、TileID 数量及 10 格边缘配置；边界宽高来自候选 session descriptor，并要求已投影的 `Main.tile` 二维数组尺寸吻合。它不读取尚未刷新的 `lastMaxTilesY`，也不触碰旧版共享房间 scratch。现有 `StartRoomCheck()` 仍使用旧尺寸兼容状态并共用同一规则 evaluator。
- 所有候选 NPC 解析和 runtime 物化成功后，修复过的不可变 NPC 状态才替换未发布 session 的 NPC slot，驱逐结果再提交到该 session 的 TownHousing registry，最后替换 runtime NPC store。候选构建失败不会提前替换活动 runtime store；同一 NPC type 的多实例按旧循环顺序暂存已驱逐 key，避免在后续房间无效时重新读取已被先前 `KickOut` 移除的 fallback。房间 tile 来源仍是 legacy `Main.tile` 的 infrastructure adapter，最终 housing owner/key 决策仍属 integration-review。
- 覆盖限于当前 `NSSLC.Tools.Simulation` host 与其有限 NPC catalog；type-name resolver 仅支持 Guide、Old Man、Zombie，且 Old Man 属 legacy 豁免，因此当前只有其余已支持的 town NPC 能进入实际修复分支。这不证明完整游戏/server host、全量 NPC、住房 feedback/achievement effects 或旧行为等价已闭合。
- 只静态对照了 Version4 `FixAgainstExploits` 顺序、候选 tile map 发布尺寸、住房 evaluator、TownHousing type-key registry 及 Simulation hydration 提交路径；未运行测试、构建、verifier 或场景。`designStatus: proposed`、`verificationStatus: not-run` 保持不变。

## 2026-10-05 Implementation Follow-up: Serialize Runtime World Lifecycle Operations

- `NSSLC.Tools.Simulation.Program` 原先直接调用 `Main.LoadWorld()`，绕过 `WorldFile.LoadWorld()` 对 load result flags 的初始化和 `WorldGen.serverLoadWorldCallBack()` 的串行门及成功后 `Hooks.WorldLoaded()`。当前唯一仓库宿主已切到 `serverLoadWorldCallBack()`，使其进入与 legacy 异步入口相同的加载生命周期。
- `_worldLifecycleGate` 现在也保护 `WorldGen.clearWorld()`、`WorldGen.Reset()`、完整 `WorldGen.GenerateWorld()` 调用、`WorldCreation.Create()` 的尺寸/种子初始化与生成过程、`WorldGenerator.TryReset()` 的 manifest/状态重置，以及 `WorldGenSnapshot.Restore()`。加载/恢复期间在同一线程内调用 `clearWorld()` 等嵌套操作可重入；跨线程的 clear/reset/generation/snapshot mutation 使用 `Monitor.TryEnter`，gate 忙时立即拒绝，`WorldGenerator.TryReset()` 返回 `false`，其余入口抛 `InvalidOperationException`，避免调用线程等待正在执行的 load hook。异步读档 callback 仍在共享 gate 上串行等待。`WorldGen.GenerateWorld()` 在取得 gate 后才升起生成标记，并在释放 gate 前完成原有 finally 清理。
- 生命周期门控复核发现 `clearWorld()` 只持有 lifecycle mutex，没有在 reset 清单执行期间升起 `isGeneratingOrLoadingWorld`；若清理中途抛错，后续 `UpdateWorld()` 仍可开始。现由 `RunWorldLifecycleMutation` 和 `TryRunWorldLifecycleMutation` 在整个 mutation 期间升门，成功后恢复进入前的门状态，异常则保持门升起；因此 `clearWorld()`、`Reset()`、`WorldGenSnapshot.Restore()` 与 `WorldGenerator.TryReset()` 的整段状态变更都受保护。`GenerateWorldCore` 另记录本次 clear 是否完成，避免无条件 finally 覆盖失败 reset 留下的门。静态调用路径包括 recovery effect 经 `WorldStorageCoordinatorFactory.ResetPublishedWorld -> ResetLegacyWorld -> WorldGen.clearWorld()`，以及 `WorldGen.GenerateWorld -> clearWorld()`。
- `WorldGen.UpdateWorld()` 现在也尝试获取 `_worldLifecycleGate` 并覆盖完整更新；门忙时跳过该次 world tick。读档 callback 的 `lock` 会等待已经进入的 tick 结束，clear/reset/generation/snapshot mutation 仍按既有策略 fail fast；同线程 tick callback 若重入生命周期操作会被拒绝，加载 hook 内重入 update 会跳过，因此显式使用这些入口的操作不再与该更新方法重叠。全仓库当前没有 `WorldGen.UpdateWorld()` 的调用点，完整 host 的真实 update 调度和任何绕过这些入口的静态写入仍需 integration review。
- 只静态检查了门的互斥/重入控制流、失败后保门路径及调用点搜索，未运行测试、构建、verifier 或场景；`designStatus: proposed`、`verificationStatus: not-run` 保持不变。
- 静态调用搜索确认当前 repo 的读档入口经 `WorldFile.LoadWorld() -> Main.LoadWorld()`；生成入口 `WorldCreation.Create()`、`WorldUtils` 与存档快照恢复均进入上述共享门，reset 也覆盖完整的 `WorldGenerator.TryReset()` action。此改动不提供独立 unload API，不约束仓库外对 `Main.LoadWorld()`、公开静态字段或其他 legacy API 的直接调用，也不证明完整游戏/server host 已采用该组合根。
- 只静态核对了入口调用链、gate 的嵌套调用/锁顺序、生成标记与 finally 清理位置；未运行测试、构建、verifier 或场景。`designStatus: proposed`、`verificationStatus: not-run` 保持不变。

## 2026-10-05 Implementation Follow-up: Commit Pre-Publication Tile Recovery

- 加载恢复重新补入缺失基础矿层的旧兼容行为：当 Copper、Iron、Silver、Gold 中任一层缺失时，按旧版 `CountTileTypesInWorld(7,166,6,167,9,168,8,169)` 统计并选择占优类型，结果同时写入 session 权威规则状态与 `WorldGen.SavedOreTiers`；Cobalt、Mythril、Adamantite 沿用载入值。
- 类型 127/504 的临时 tile 清理发生在 session 尚未完成发布时，直接改 `Main.tile` 会使后续保存从 `TileMapStore` 捕获旧值。现在候选清理阶段绑定 tile mutation observer，收集 `KillTile` 及 tile framing 写入的坐标，通过仅接受完整待发布 session 的候选提交入口更新 `TileMapStore`，再从 owner 重新投影规范化 tile；退出时解除 observer。取消或失败仍由 recovery coordinator reset 并丢弃候选 session。
- 液体 settle 之前及过程中，`Liquid.QuickWater(2)`、两次 `WorldGen.WaterCheck()`、每次 `Liquid.UpdateLiquid()` 和 pre-release callback 的 tile property 写入现在由 active-session observer 收集，并在每个 settle 阶段后回写 `TileMapStore` 再继续；取消/异常仍走已有 reset 路径。保存快照只从该 store 读取，因此 runtime 液体变化不再滞留在 `Main.tile`。
- 候选 tile 提交复用 runtime→owner 字段映射和完整坐标预校验，不允许在 session 已 active/published 后使用。该路径仍依赖加载门保持升起、runtime tile map 与 session 尺寸相同，以及该阶段在 owner thread 执行；其他 `KillTile` 间接副作用不因此成为 session tile owner 的一部分。
- 旧 `WorldFile.LoadWorld` 源码顺序是矿层恢复、`ConvertOldTileEntities`、`ClearTempTiles`，之后才升起全局 load gate；当前组合根为保护运行时发布而先投影 gate，再执行候选 tile 清理。因而 `KillTile` 看到的全局 gate 与旧路径不同，可能改变受 gate 控制的音效/掉落等副作用。`ConvertOldTileEntities` 的 mannequin、weapons-rack 转换仍未接入 session owner；其中 tile 49 的 `frameX/frameY == -1` 修复现已随候选 tile 清理一并提交到 owner。
- 对旧转换的进一步源码核对确认，`NSSLC.Infrastructure.WorldStorage.csproj` 关闭默认编译项且未包含旧 `WorldFile.cs`，所以其中 `ConvertOldTileEntities` 不会自动运行。mannequin 路径依赖 Version4 `Main.Initialize_Items` 构建的 `Item.headType/bodyType/legType` 反查表；当前 generated runtime 没有构建这些表，Simulation catalog 目前只声明 bow、arrow、gel 三项。weapons-rack 路径的 `TEWeaponsRack.TryPlacing` 还会执行 `WorldGen.RangeFrame` 并可能创建 WorldItem/发送网络消息，而 generated runtime 未提供该入口；Version4 `TileEntity.Place` 当前源码本身也是占位实现。因此后续应由 TileMap/TileEntity session owner 执行旧格式数据转换，并通过明确的 armor-slot resolver 与 host side-effect seam 衔接；不能把调用 legacy `ConvertOldTileEntities` 当作已完成迁移。
- 只静态检查了 `KillTile`/tile setter 写入方式、observer 绑定和候选生命周期条件、liquid settle 调用顺序、tile owner 提交与 snapshot source 的读取关系；没有运行测试、构建、verifier 或场景。该增量不闭合完整 server host 接线、其他 legacy `Main.tile` 写入、reset 后全局字段清理或版本行为等价；`designStatus: proposed`、`verificationStatus: not-run` 保持不变。

## 2026-10-05 Implementation Follow-up: Migrate Legacy Mannequin and Weapons-Rack State Before Projection

- `WorldStorageCoordinatorFactory.PublishRuntimeWorldState()` 现在先调用 `LegacyWorldTileEntityMigration.Apply()`，然后才投影 TileMap 和 TileEntity runtime。迁移扫描候选 session 的 `TileMapStore`，按旧格式转换 mannequin 类型 128/269 到 display-doll 类型 470、weapons-rack 类型 334 到 471，并创建相应 session `TileEntityStore` 快照；不再把转换留给未编译的 `WorldFile.ConvertOldTileEntities()`。
- 转换先在临时字典和列表中暂存完整 tile/frame 变化与 TileEntity；核验新旧 anchor/ID 后才提交两个 owner，并刷新该 session 的 TileEntity update schedule。新实体 ID 从旧 `LoadTileEntities` 使用的 record count 之后递增。mannequin 装备槽按 Version4 `TEDisplayDoll.SetInventoryFromMannequin` 的 `frame / 100` 与 `Item.headType/bodyType/legType` 映射恢复；当前 generated runtime 未填充的非空槽会拒绝候选加载，不会降成空装备。weapons-rack 按旧转换读取下排编码 item type 和 prefix。
- 适用范围是当前 `RegisterGeneratedWorldLoadHandlerWithRuntimeTileMap()` 路径；仓库外完整游戏/server host 和使用自定义 publication callback 的宿主尚未证明接入。此实现直接提交 session owner 数据和 tile frames，没有复现 `TEWeaponsRack.TryPlacing()` 的 `WorldGen.RangeFrame`、世界物品回退或网络消息；对应 host side-effect seam 仍为 `crossSubsystemOwner: integration-review`。没有支持 mannequin armor-slot 数据的宿主 catalog 时，含已装备 mannequin 的世界会明确加载失败，直至 resolver 可提供对应 item type。
- 只静态对照 Version4 `WorldFile.ConvertOldTileEntities`、`TEDisplayDoll.SetInventoryFromMannequin`、`TEWeaponsRack.TryPlacing` 与候选 projection/owner 提交顺序；未运行测试、构建、verifier 或场景。此增量闭合当前 Simulation runtime-tile-map 组合中的持久化迁移路径，不证明完整副作用等价或所有 host 已接线；`designStatus: proposed`、`verificationStatus: not-run` 保持不变。
