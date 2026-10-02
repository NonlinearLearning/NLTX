# System Decomposition Report: authoritative P16

- partitionId: P16
- sessionId: b65120e249874094ac5069b99b2fb9c7
- taskSet: authoritative-system-decomposition
- inputReport: docs/migration/ledgers/authoritative-20-partitions/P16-World-Lifecycle-Housing-Metrics.md
- prompt: docs/component-decomposition/review-round-1/design/2026-09-11-version4-authoritative-20-partition-prompts/2026-09-11-version4-P16-world-lifecycle-housing-metrics-public-decomposition.md
- outputReport: docs/system-decomposition/reports/2026-09-18-system-decomposition-authoritative-P16-world-lifecycle-housing-metrics.md
- designStatus: proposed
- verificationStatus: not-run
- sourceModified: false

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

当前代码检查范围为 src/NSSLC/Component/WorldSession/WorldGeneration。该树已有生命周期、住房、注册表、TreeTops 和 metrics 数据类型；可见一个实际局部实现 WorldLayerMetricsSystem 及相应 Query。搜索 P16 相关类型名未找到它们被加载器、主循环或完整 Version4 adapter 调用的证据。类型存在只说明当前 API/数据形状，不代表运行时已迁移或与旧行为等价。

参考项目 C:\Users\shan\Downloads\ECS\space-station-14-master 的 GridPreloaderComponent / GridPreloaderSystem 展示了将驻留列表放在 map entity 的 Component、由 System 接收 map lifecycle 事件并操作外部 map service 的结构；AtmosphereSystem.Processing 展示 System 管理有游标且可延后处理的批次。这些仅用于比较状态与执行边界，不能推断 Terraria 的生命周期、调度器、异步或住房语义。

## Prior Component Decomposition Reconciliation

- P16 专属 public-decomposition prompt 指定的历史产物 docs/component-decomposition/review-round-1/2026-09-11-version4-P16-world-lifecycle-housing-metrics-public-decomposition.md 不存在于预期路径；其具体 owner 决策因此为 evidence-gap，本报告不把旧分区标题当作已确认设计。
- 输入 inventory 的 13 组用于稳定限定成员，不是 13 个 Components 或 Systems 的直接方案。特别是 WorldGenBiomeBackgroundAndDistanceMetrics 同时含 TownManager、Manifest、生物群系背景和距离；WorldGenerationDimensionsState 同时含 meteorShowerCount 与尺寸目录；WorldTerrainEffectsAndCaches 混合存档状态、网络投影、事件临时数据和函数 scratch。这些组需按不变量再分。
- Component 的字段聚类仍有参考价值：住房搜索有可识别的临时上下文；Tile 计数有扫描游标和累计窗口；世界载入标记有生命周期。但实际 owner 要以写入者、完成提交和外部持久化/网络路径决定，而不是沿用 Component 名称。
- src/NSSLC 已有 WorldLoadLifecycleComponent、WorldGenerationLifecycleState、HousingScanStateComponent、TownHousingRegistryComponent、WorldTreeTopsStateComponent、WorldLayerMetricsComponent 等。WorldLayerMetricsSystem 的 generation 匹配与 commit 仅是本模块局部代码；住房/生命周期/TreeTops 未找到对应的实际端到端旧 API adapter/主循环注册证据。TownHousingRegistryComponent 自带 status: proposed 和 crossSubsystemOwner: integration-review，此标记与本次静态证据相符。

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
| TransformingWorld, genRand, oceanLevel | WorldLifecycleQuery.IsTransforming; IGenerationRandomSource.Next; WorldOceanLevelQuery.Evaluate(surface, rock) | genRand 是共享可变随机源，需要维持调用顺序，不是可任意重排的 Query。oceanLevel 算式是纯派生；NSSLC 目前还额外拒绝非有限输入，旧 API 的异常/非有限输入行为未建立兼容结论。 |

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

    WorldFile.ValidateLoadNPCs -> StartRoomCheck
      -> TownManager.HasRoom / KickOut -> NPC room correction or homeless write

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
- **Reset / destroy / rebuild / multi-world:** WorldGen 是大量 static 状态。完整 reset/clear/unload 及是否一次运行仅一 world 未闭合；没有证据证明字段或已有 NSSLC holder 已按 session 隔离。切换 world、重载失败、重复启动/取消、重新生成时的状态清除一律 unknown。
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
4. **Load failure/reset:** loadFailed 与 isGeneratingOrLoadingWorld 的异常、早退、retry、world clear/unload 状态图未闭合；特别是打开 gate 后 catch 的恢复路径为 unknown。
5. **Housing callback closure:** IRoomCheckFeedback 动态实现集合、异常副作用和临时 tileSolid[379] 的异常恢复未验证；call-site query 的 complete 不消除这些 gap。
6. **Housing authority:** TownManager 当前以 NPC type 索引；NSSLC TownHousingResidentKey 有多种 key mode 候选。NPC instance/type 的最终身份、world scope、持久化兼容和唯一写入者是 blocking integration decision。
7. **Static state and multi-world:** P16 字段多为 static；当前没有证据表明未来 session Component 实际注册、被调用或能支持多 world。状态重置、重入和所有静态读写都必须做迁移前清点。
8. **Metrics dependencies:** Tile array 完整扫描窗口、计数边界/防零分母、Skyblock observer 影响、message 57 消费端与服务端/客户端方向未闭合。不可把 Query status complete 当作这一行为闭包。
9. **TreeTops protocol:** 旧版本 <211 的 copy path、当前 save/load count 不匹配/截断输入、所有 set/randomize callers 和网络字节的接收端还需验证。
10. **Terrain/effect scratch ownership:** P16 的 weather/event/tile merge/exploit/flood-fill scratch 相互独立；本分区不含这些跨 owner 的最终业务决策和全闭包写入证据。
11. **Existing target code integration:** WorldLayerMetricsSystem、WorldOceanLevelQuery 等局部 API 已有源码；housing/lifecycle/TreeTops holders 也存在，但没有检查或证明它们覆盖旧 API caller、存档和网络并发路径。实际迁移与等价仍 unknown。

阻塞后续切换的决定：world session 的 authoritative entity/session key；load fail/cancel 的终态转换；housing identity 和 NPC/registry 双写策略；旧存档 key/schema 与新 Component 恢复映射；Main update 注入点及线程/随机顺序；metrics/network/Skyblock 提交合同。以上未定时只能实现隔离的候选逻辑，不能切换旧入口或宣告迁移完成。

## Verification Plan

以下仅是未来针对行为的验证计划；本会话没有执行任何构建、测试、verifier、运行时检查或协议 round-trip。verificationStatus: not-run。

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
