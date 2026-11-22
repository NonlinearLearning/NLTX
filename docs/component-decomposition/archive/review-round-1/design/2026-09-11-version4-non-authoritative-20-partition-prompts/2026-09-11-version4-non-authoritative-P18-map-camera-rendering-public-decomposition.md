# Version4 非权威组件拆分分区 P18：地图、相机与绘制 - public-decomposition 专属提示词

> 本文件只包含当前分区的专属任务内容，不复制公共提示词正文。
> 使用时：先提供公共提示词，再附加本文件；公共协议中的通用规则、证据门槛、报告结构、禁止项和 Integration Handoff 继续有效。

## 公共前置（仅引用）

- 公共提示词：`D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\design\2026-09-05-version4-public-decomposition-common-protocol.md`
- 必读技能：`D:\TRbackup\NLTX\.agents\skills\public-decomposition\SKILL.md`
- ECS 证据协议：`D:\TRbackup\NLTX\.agents\skills\public-decomposition\references\ecs-evidence-protocol.md`
- tModLoader 检索协议：`D:\TRbackup\NLTX\.agents\skills\public-decomposition\references\tmodloader-documentation-retrieval.md`
- 本文件不重复上述文件内容；执行前必须实际读取并遵守它们。
- 并行会话保障 skill：D:\\TRbackup\\NLTX\\.agents\\skills\\version4-non-authoritative-partition-session\\SKILL.md
- 并行领取/结算 runner：D:\\TRbackup\\NLTX\\Build\\Tools\\Invoke-Version4NonAuthoritativePartitionSession.ps1
- 必须先按 skill 使用 ClaimNext（或用户明确指定时使用 Claim -PartitionId P18），只写本文件的 outputReport，并用领取返回的同一 sessionId 完成 Complete/Fail/Abandon；不得手工编辑 ledger 或 lock。
- 本任务属于 20 个并行会话中的固定分区；currentTaskCount: 20。公共协议中遗留的 19 会话数量说明在本任务包中不适用。

## 任务元数据

- promptId: P18-non-authoritative-public-decomposition-20260911
- partitionId: P18
- inputPartitionReport: D:\TRbackup\NLTX\docs\migration\ledgers\non-authoritative-component-partitions\18-map-camera-rendering.md
- outputReport: D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-11-version4-non-authoritative-P18-map-camera-rendering-public-decomposition.md
- reportType: public-decomposition
- executionMode: independent-read-only-partition
- currentTaskCount: 20
- formalParentSubsystems: ClientPresentationAndTools, RuntimeComposition, SharedRuntimeMechanisms
- leafSubsystemCount: 29
- fieldCount: 274
- propertyCount: 57
- memberCount: 331
- sourceReportSha256: b944441d92d5103f218b766a3e354200794528a19e65a862e44c2b40a36f1196
- sourceInventoryStatus: source-inventory-confirmed; member-level owner/lifecycle/side-effect evidence still requires this session to close

## 专属范围锁定

- 本会话只审查输入分区报告中的 29 个叶子子系统和 331 条成员记录。以下清单是本会话唯一的成员范围；不得读取或复制其他 P 分区的成员清单。
- 细分子系统仍只是库存边界，不等价于一个组件；允许在本分区内部按访问模式拆成多个 proposed Component、System、Query、Command、Adapter 或 Projection。
- 来源序号、路径、行号和声明是检索线索；必须回到 Version4 重新定位，不得把输入报告直接当作已确认的读者、写者、生命周期或 owner 证据。

| 叶子子系统 | 父级 | 原章节 | 边界角色 | 字段 | 属性 | 成员 | 细分职责 |
|---|---|---|---|---:|---:|---:|---|
| `MainCameraAndUiScaleState` | `RuntimeComposition` | `4.1.4` | `presentation state` | 8 | 0 | 8 | 相机对象、视图矩阵和 UI 缩放状态。 |
| `MainCameraAndVisualOffsets` | `RuntimeComposition` | `4.1.7` | `presentation state` | 8 | 0 | 8 | 相机插值、手持/头饰偏移和屏幕平移数据。 |
| `MainSceneMetricsState` | `RuntimeComposition` | `4.1.20` | `derived/query` | 4 | 0 | 4 | 相机和玩家场景指标缓存。 |
| `SharedSceneScanSettings` | `SharedRuntimeMechanisms` | `4.9.7` | `query input` | 4 | 0 | 4 | 场景扫描中心、可视区域和玩家视角输入。 |
| `SharedSceneVisualState` | `SharedRuntimeMechanisms` | `4.9.8` | `presentation state` | 6 | 0 | 6 | 场景光照衰减、天气表现强度和视觉过渡状态。 |
| `SharedCaptureAndCameraSupport` | `SharedRuntimeMechanisms` | `4.9.9` | `presentation` | 5 | 1 | 6 | 截图、无人机相机和钻头调试绘制支持。 |
| `LightMapCacheState` | `SharedRuntimeMechanisms` | `4.9.76` | `cache` | 5 | 8 | 13 | 光照图颜色和掩码缓存。 |
| `TileLightScannerState` | `SharedRuntimeMechanisms` | `4.9.77` | `query` | 1 | 0 | 1 | Tile 光照扫描和随机扫描辅助状态。 |
| `DrawAnimationAndFrameState` | `SharedRuntimeMechanisms` | `4.9.80` | `presentation/state` | 18 | 2 | 20 | 动画、绘制动画和精灵帧状态。 |
| `DrawCommandAndBatchState` | `SharedRuntimeMechanisms` | `4.9.81` | `presentation/command` | 20 | 0 | 20 | 绘制数据命令和 SpriteBatch 批处理状态。 |
| `WorldDrawingAuxiliaryState` | `SharedRuntimeMechanisms` | `4.9.82` | `presentation` | 10 | 0 | 10 | 粒子编排、Tile 绘制、地平线和辅助渲染状态。 |
| `EntityShadowPresentationState` | `SharedRuntimeMechanisms` | `4.9.108` | `presentation/snapshot` | 6 | 1 | 7 | 实体阴影的位置、旋转和表现快照。 |
| `SharedShaderBaseParameterState` | `SharedRuntimeMechanisms` | `4.9.120` | `presentation` | 18 | 3 | 21 | 基础 shader、屏幕参数和效果参数缓存。 |
| `SharedLightingCoordinatorState` | `SharedRuntimeMechanisms` | `4.9.130` | `state/query` | 16 | 3 | 19 | 新旧光照引擎切换、活动引擎和逐帧光照状态。 |
| `SharedLegacyLightingState` | `SharedRuntimeMechanisms` | `4.9.131` | `state/query` | 19 | 1 | 20 | 旧光照扫描、临时光源和旧光照图状态。 |
| `SharedSceneScanAccumulatorState` | `SharedRuntimeMechanisms` | `4.9.171` | `query/cache` | 8 | 5 | 13 | 场景扫描计数、位置快照、最近实体和扫描缓存。 |
| `SharedSceneTileAggregateState` | `SharedRuntimeMechanisms` | `4.9.175` | `query/cache` | 0 | 19 | 19 | 场景 Tile、液体、实体和资源聚合计数。 |
| `SharedShaderFamilyDataState` | `SharedRuntimeMechanisms` | `4.9.182` | `presentation` | 19 | 1 | 20 | Hair、Misc、Armor 等 shader 家族数据和参数。 |
| `SharedShaderRegistryAndLookupState` | `SharedRuntimeMechanisms` | `4.9.183` | `presentation/query` | 9 | 0 | 9 | GameShaders、ShaderDataSet 注册、索引和查找状态。 |
| `SharedSceneZoneGeometryAndThresholdState` | `SharedRuntimeMechanisms` | `4.9.203` | `query/input` | 12 | 0 | 12 | 场景扫描窗口、层高、阈值和区域几何输入。 |
| `MapOverlayAndLayerPresentation` | `ClientPresentationAndTools` | `4.11.12` | `presentation` | 12 | 0 | 12 | 地图覆盖层、Ping 和传送晶塔图层表现。 |
| `WorldMapState` | `ClientPresentationAndTools` | `4.11.13` | `presentation/state` | 4 | 1 | 5 | 世界地图尺寸和地图持久状态。 |
| `MapTileUpdateQueueState` | `ClientPresentationAndTools` | `4.11.16` | `presentation/state` | 3 | 0 | 3 | 地图区域更新队列、锁和待处理更新状态。 |
| `CameraMatrixAndViewportState` | `ClientPresentationAndTools` | `4.11.19` | `presentation` | 8 | 8 | 16 | 相机视口、缩放、平移和图形变换矩阵状态。 |
| `VertexStripAndColorState` | `ClientPresentationAndTools` | `4.11.20` | `presentation` | 12 | 1 | 13 | 顶点条、顶点声明、索引和顶点颜色状态。 |
| `MapTileCellState` | `ClientPresentationAndTools` | `4.11.24` | `snapshot state` | 3 | 3 | 6 | 单个地图 Tile 的类型、光照、颜色和更新标志。 |
| `MapIoRuntimeState` | `ClientPresentationAndTools` | `4.11.28` | `adapter state` | 4 | 0 | 4 | 地图文件锁、场景缓存、雪量和压缩运行状态。 |
| `MapEncodingHeaderBitCatalogState` | `ClientPresentationAndTools` | `4.11.33` | `adapter state` | 24 | 0 | 24 | 地图编码 Header 位布局、保留位和颜色位定义。 |
| `MapEncodingOptionLimitState` | `ClientPresentationAndTools` | `4.11.34` | `adapter state` | 8 | 0 | 8 | 地图编码绘制循环、选项上限、渐变上限和区块尺寸。 |

来源成员的分区内序号线索范围：10..4535；序号不定义领域边界，必须逐条回到输入表和 Version4 覆盖。

## 分区专属目标

围绕“地图、相机与绘制”完成独立只读的 Version4 成员审查和 ECS 拆分设计。
- 围绕 Main 相机/UI scale/visual offset、SceneMetrics、场景扫描、光照图、TileLight、绘制命令/批处理、shader registry、地图状态/编码/更新队列和相机矩阵，核对模拟输入、渲染缓存与客户端投影。
- 回到 Main、SceneMetrics、Lighting、Map、Camera、Draw、Shader 相关源码及绘制调用者，确认帧生命周期、缓存失效、地图 I/O、渲染提交和设备/资源边界。
- 分别评估 camera/view state、scene scan Query、light map cache、draw command/batch、shader Adapter、map persistent state、map update queue 和 tile visual snapshot。
- 核对地图存储与世界 Tile、相机与 UI/输入、场景指标与实体/天气、绘制与粒子/音频的只读/投影方向，记录渲染失败与重建策略。

## 专属证据焦点

- 先完整读取本分区输入报告的成员表，再回到 D:\TRbackup\Version4 重新核对字段、属性、声明类型、初始化/注册、直接读者、直接写者、清理路径和副作用。
- 下列路径只作为本分区的检索种子；路径存在不等于成员语义已确认，必须按公共证据协议记录实际行号、符号、调用者、读者、写者、生命周期和 evidenceStatus。
  - `D:\TRbackup\Version4\Terraria.DataStructures/DrawAnimation.cs`
  - `D:\TRbackup\Version4\Terraria.DataStructures/DrawAnimationVertical.cs`
  - `D:\TRbackup\Version4\Terraria.DataStructures/DrawData.cs`
  - `D:\TRbackup\Version4\Terraria.DataStructures/DrillDebugDraw.cs`
  - `D:\TRbackup\Version4\Terraria.DataStructures/DroneCameraTracker.cs`
  - `D:\TRbackup\Version4\Terraria.DataStructures/EntityShadowInfo.cs`
  - `D:\TRbackup\Version4\Terraria.DataStructures/SpriteBatchBeginner.cs`
  - `D:\TRbackup\Version4\Terraria.DataStructures/SpriteFrame.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Drawing/HorizonHelper.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Drawing/NextHorizonRenderer.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Drawing/ParticleOrchestraSettings.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Drawing/TileDrawingBase.cs`
  - `D:\TRbackup\Version4\Terraria.Graphics.Capture/CaptureManager.cs`
  - `D:\TRbackup\Version4\Terraria.Graphics.Light/LegacyLighting.cs`
  - `D:\TRbackup\Version4\Terraria.Graphics.Light/LightingEngine.cs`
  - `D:\TRbackup\Version4\Terraria.Graphics.Light/LightMap.cs`
  - `D:\TRbackup\Version4\Terraria.Graphics.Light/TileLightScanner.cs`
  - `D:\TRbackup\Version4\Terraria.Graphics.Shaders/ArmorShaderData.cs`
  - `D:\TRbackup\Version4\Terraria.Graphics.Shaders/ArmorShaderDataSet.cs`
  - `D:\TRbackup\Version4\Terraria.Graphics.Shaders/GameShaders.cs`
  - `D:\TRbackup\Version4\Terraria.Graphics.Shaders/HairShaderData.cs`
  - `D:\TRbackup\Version4\Terraria.Graphics.Shaders/HairShaderDataSet.cs`
  - `D:\TRbackup\Version4\Terraria.Graphics.Shaders/MiscShaderData.cs`
  - `D:\TRbackup\Version4\Terraria.Graphics.Shaders/ScreenShaderData.cs`
  - `D:\TRbackup\Version4\Terraria.Graphics.Shaders/ShaderData.cs`
  - `D:\TRbackup\Version4\Terraria.Graphics/Camera.cs`
  - `D:\TRbackup\Version4\Terraria.Graphics/SpriteViewMatrix.cs`
  - `D:\TRbackup\Version4\Terraria.Graphics/VertexColors.cs`
  - `D:\TRbackup\Version4\Terraria.Graphics/VertexStrip.cs`
  - `D:\TRbackup\Version4\Terraria.Map/MapHelper.cs`
  - `D:\TRbackup\Version4\Terraria.Map/MapOverlayDrawContext.cs`
  - `D:\TRbackup\Version4\Terraria.Map/MapTile.cs`
  - `D:\TRbackup\Version4\Terraria.Map/MapUpdateQueue.cs`
  - `D:\TRbackup\Version4\Terraria.Map/PingMapLayer.cs`
  - `D:\TRbackup\Version4\Terraria.Map/TeleportPylonsMapLayer.cs`
  - `D:\TRbackup\Version4\Terraria.Map/WorldMap.cs`
  - `D:\TRbackup\Version4\Terraria/Animation.cs`
  - `D:\TRbackup\Version4\Terraria/Lighting.cs`
  - `D:\TRbackup\Version4\Terraria/Main.cs`
  - `D:\TRbackup\Version4\Terraria/SceneMetrics.cs`
  - `D:\TRbackup\Version4\Terraria/SceneMetricsScanSettings.cs`
  - `D:\TRbackup\Version4\Terraria/SceneState.cs`

- 输入分区抽取出的声明类型焦点：
  - `Terraria.Animation`
  - `Terraria.DataStructures.DrawAnimation`
  - `Terraria.DataStructures.DrawAnimationVertical`
  - `Terraria.DataStructures.DrawData`
  - `Terraria.DataStructures.DrillDebugDraw`
  - `Terraria.DataStructures.DroneCameraTracker`
  - `Terraria.DataStructures.EntityShadowInfo`
  - `Terraria.DataStructures.SpriteBatchBeginner`
  - `Terraria.DataStructures.SpriteFrame`
  - `Terraria.GameContent.Drawing.HorizonHelper`
  - `Terraria.GameContent.Drawing.NextHorizonRenderer`
  - `Terraria.GameContent.Drawing.ParticleOrchestraSettings`
  - `Terraria.GameContent.Drawing.TileDrawingBase`
  - `Terraria.Graphics.Camera`
  - `Terraria.Graphics.Capture.CaptureManager`
  - `Terraria.Graphics.Light.LegacyLighting`
  - `Terraria.Graphics.Light.LegacyLighting.LightingState`
  - `Terraria.Graphics.Light.LegacyLighting.LightingSwipeData`
  - `Terraria.Graphics.Light.LightingEngine`
  - `Terraria.Graphics.Light.LightingEngine.PerFrameLight`
  - `Terraria.Graphics.Light.LightMap`
  - `Terraria.Graphics.Light.TileLightScanner`
  - `Terraria.Graphics.Shaders.ArmorShaderData`
  - `Terraria.Graphics.Shaders.ArmorShaderDataSet`
  - `Terraria.Graphics.Shaders.GameShaders`
  - `Terraria.Graphics.Shaders.HairShaderData`
  - `Terraria.Graphics.Shaders.HairShaderDataSet`
  - `Terraria.Graphics.Shaders.MiscShaderData`
  - `Terraria.Graphics.Shaders.ScreenShaderData`
  - `Terraria.Graphics.Shaders.ShaderData`
  - `Terraria.Graphics.Shaders.ShaderData.EffectParameter<T>`
  - `Terraria.Graphics.SpriteViewMatrix`
  - `Terraria.Graphics.VertexColors`
  - `Terraria.Graphics.VertexStrip`
  - `Terraria.Graphics.VertexStrip.CustomVertexInfo`
  - `Terraria.Lighting`
  - `Terraria.Main`
  - `Terraria.Map.MapHelper`
  - `Terraria.Map.MapOverlayDrawContext`
  - `Terraria.Map.MapTile`
  - `Terraria.Map.MapUpdateQueue`
  - `Terraria.Map.PingMapLayer`
  - `Terraria.Map.PingMapLayer.Ping`
  - `Terraria.Map.TeleportPylonsMapLayer`
  - `Terraria.Map.WorldMap`
  - `Terraria.SceneMetrics`
  - `Terraria.SceneMetricsScanSettings`
  - `Terraria.SceneState`

- 若路径、类型或行号漂移，记录 version-drift 或 evidence-mismatch；不得静默沿用库存报告行号。Version4 是 Terraria 私有实现的事实来源，完整源码只补证 Version4 已存在文件，tModLoader 只交叉验证公开边界，SS14 只参考结构粒度。

## 专属审查问题

- 相机位置、视口、UI 缩放、视觉偏移和矩阵是否是客户端表现状态，哪些输入来自玩家/世界？谁可写？
- SceneMetrics、TileLight、LightMap、shader lookup 和 draw batch 是 Query、缓存、Adapter 还是 Projection；失效条件是什么？
- WorldMap、MapTile、Map I/O、更新队列和编码 header 哪些需要持久化，哪些只是视图缓存？
- 地图/相机/绘制与 Tile、天气、实体、网络、UI、粒子之间的交接和跨分区 owner 如何标记？

## 专属不拆分边界

- 不要把 camera、scene scan、lighting、map storage、draw command 和 shader registry 合成 RenderComponent。
- 不要把单次 DrawCommand、SpriteBatch、光照缓存、地图 Tile update、shader parameter 或 scene aggregate 当作权威模拟状态。
- 不要让 Projection/Render cache 反向写 Tile、实体或事件权威状态；绘制顺序必须由显式渲染调度表达。

专属跨域提醒：重点记录与 Tile/液体、世界事件、实体、UI、网络地图投影、音频粒子和持久化的 integration-risk；WorldMap/TileSnapshot/CameraState/ShaderHandle 候选统一 crossSubsystemOwner: integration-review。

## 输出要求

- 将完整研究报告只写入：D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-11-version4-non-authoritative-P18-map-camera-rendering-public-decomposition.md
- 不要写入输入分区报告、公共提示词、其他分区报告、源代码、测试、项目文件或迁移文件。
- 报告必须逐条覆盖本分区成员，给出成员归属表、状态所有权、Component/System/Query/Command/Adapter/Projection 边界、接口契约、依赖方向、顺序约束、不拆分项、兼容策略、evidence-gap、blocking-decision、focused verifier 计划和 Integration Handoff。
- 本轮只读审查不运行编译型命令；没有实际执行的 verifier 必须写 verificationStatus: not-run。
- proposed 类型、路径和接口必须明确标记 proposed；不得写成当前 NLTX 已存在或已经迁移。
- 跨两个或以上分区使用的类型、接口、ID、快照、值对象和 System 顺序只能提出候选，必须写 crossSubsystemOwner: integration-review，不得在本报告单方面裁决。
- 不得修改任何生产代码、测试、项目文件、生成器、公共协议、分区报告或其他并行会话输出。

## 分区完成检查

- [ ] 逐条覆盖本分区全部 331 条成员，成员事实来自输入表但关键语义已回到 Version4 复核。
- [ ] 只使用本分区叶子清单；没有复制、重新设计或宣布其他 P 分区成员。
- [ ] Component 只承载内聚权威状态；派生值、缓存、查询、短期 payload、日志、网络/存档/UI 输出按正确角色隔离。
- [ ] 所有共享候选写 crossSubsystemOwner: integration-review；没有把 proposed 设计写成已实现能力。
- [ ] 未执行的构建、测试和 verifier 均明确 verificationStatus: not-run；证据缺口、冲突和 blocking-decision 已单列。
- [ ] 输出文件路径唯一为：D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-11-version4-non-authoritative-P18-map-camera-rendering-public-decomposition.md

## 会话结束回报

最终消息只回报以下字段：
- partitionId: P18
- outputReport: D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-11-version4-non-authoritative-P18-map-camera-rendering-public-decomposition.md
- evidenceSourcesRead: 实际读取的 Version4 文件/类型、必要的完整源码补证、tModLoader 页面和 SS14 参考
- evidenceGaps: 仍缺失或冲突的关键证据
- blockingDecisions: 需要最终整合会话裁决的 owner/顺序/事务边界
- verificationStatus: not-run
- productionCodeModified: no

本提示词只定义本分区的专属范围和审查抓手；公共协议负责共同证据规则、报告结构、禁止项和 Integration Handoff。
