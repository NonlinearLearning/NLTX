# Version4 非权威组件拆分分区 P04：地块、液体与世界存储 - public-decomposition 专属提示词

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
- 必须先按 skill 使用 ClaimNext（或用户明确指定时使用 Claim -PartitionId P04），只写本文件的 outputReport，并用领取返回的同一 sessionId 完成 Complete/Fail/Abandon；不得手工编辑 ledger 或 lock。
- 本任务属于 20 个并行会话中的固定分区；currentTaskCount: 20。公共协议中遗留的 19 会话数量说明在本任务包中不适用。

## 任务元数据

- promptId: P04-non-authoritative-public-decomposition-20260911
- partitionId: P04
- inputPartitionReport: D:\TRbackup\NLTX\docs\migration\ledgers\non-authoritative-component-partitions\04-world-tiles-storage.md
- outputReport: D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-11-version4-non-authoritative-P04-world-tiles-storage-public-decomposition.md
- reportType: public-decomposition
- executionMode: independent-read-only-partition
- currentTaskCount: 20
- formalParentSubsystems: RuntimeComposition, SharedRuntimeMechanisms, WorldStorage
- leafSubsystemCount: 23
- fieldCount: 310
- propertyCount: 10
- memberCount: 320
- sourceReportSha256: b944441d92d5103f218b766a3e354200794528a19e65a862e44c2b40a36f1196
- sourceInventoryStatus: source-inventory-confirmed; member-level owner/lifecycle/side-effect evidence still requires this session to close

## 专属范围锁定

- 本会话只审查输入分区报告中的 23 个叶子子系统和 320 条成员记录。以下清单是本会话唯一的成员范围；不得读取或复制其他 P 分区的成员清单。
- 细分子系统仍只是库存边界，不等价于一个组件；允许在本分区内部按访问模式拆成多个 proposed Component、System、Query、Command、Adapter 或 Projection。
- 来源序号、路径、行号和声明是检索线索；必须回到 Version4 重新定位，不得把输入报告直接当作已确认的读者、写者、生命周期或 owner 证据。

| 叶子子系统 | 父级 | 原章节 | 边界角色 | 字段 | 属性 | 成员 | 细分职责 |
|---|---|---|---|---:|---:|---:|---|
| `MainWorldGeometryAndCapacity` | `RuntimeComposition` | `4.1.14` | `runtime state` | 11 | 0 | 11 | 世界边界、Tile 尺寸、区段和实体容量。 |
| `MainCameraAndLiquidState` | `RuntimeComposition` | `4.1.16` | `runtime state` | 5 | 0 | 5 | 相机坐标、液体透明度、样式和缓冲区。 |
| `MainTileFrameAndCatchMetadata` | `RuntimeComposition` | `4.1.23` | `runtime state` | 5 | 0 | 5 | Tile 砂土、火焰、可捕获标记和帧缓存。 |
| `MainWorldMapAndTileStore` | `RuntimeComposition` | `4.1.24` | `runtime state` | 2 | 0 | 2 | 世界地图和全局 Tile 存储引用。 |
| `MainWallAndGlobalTileMetadata` | `RuntimeComposition` | `4.1.43` | `catalog reference` | 6 | 0 | 6 | Main 的墙体、全局合并和音乐淡出元数据。 |
| `MainTileBehaviorAndInteractionMetadata` | `RuntimeComposition` | `4.1.48` | `catalog reference` | 20 | 0 | 20 | Main 的 Tile 交互、碰撞、放置和行为元数据。 |
| `MainTileLightingAndFrameMetadata` | `RuntimeComposition` | `4.1.49` | `catalog reference` | 15 | 0 | 15 | Main 的 Tile 光照、框架、砖墙和表现元数据。 |
| `TileCellMaterialAndLiquidState` | `WorldStorage` | `4.5.2` | `authoritative snapshot` | 9 | 0 | 9 | 单格 Tile 类型、墙体和液体材料状态。 |
| `TileCellFrameAndBitState` | `WorldStorage` | `4.5.3` | `authoritative snapshot` | 21 | 0 | 21 | 单格 Tile 帧坐标、头位和形状位状态。 |
| `SharedTileAnchorAndReachQueries` | `SharedRuntimeMechanisms` | `4.9.31` | `query/state` | 18 | 0 | 18 | 锚点、可达性、坐标和值对象支持。 |
| `SharedTileFramingAndSignState` | `SharedRuntimeMechanisms` | `4.9.32` | `state/query` | 16 | 0 | 16 | Tile 框架和告示牌状态。 |
| `SharedTileSnapshots` | `SharedRuntimeMechanisms` | `4.9.33` | `snapshot/query` | 18 | 1 | 19 | Tile 快照值和只读投影。 |
| `TileEntityRegistryAndBaseState` | `SharedRuntimeMechanisms` | `4.9.72` | `state` | 14 | 0 | 14 | TileEntity 基类、注册表、类型标识和实体 ID。 |
| `TileEntityDisplayAndInventoryState` | `SharedRuntimeMechanisms` | `4.9.73` | `state/presentation` | 24 | 0 | 24 | 展示架、物品框、食物盘和展示容器状态。 |
| `TileEntityAnchorAndSensorState` | `SharedRuntimeMechanisms` | `4.9.74` | `state/query` | 11 | 0 | 11 | 逻辑传感器和生物/风筝锚点状态。 |
| `TileEntityWorldInteractionState` | `SharedRuntimeMechanisms` | `4.9.75` | `state/query` | 9 | 0 | 9 | 训练假人、传送晶塔和带物品的实体锚点交互状态。 |
| `SharedTilePlacementAnchorAndHookModules` | `SharedRuntimeMechanisms` | `4.9.121` | `definition/query` | 20 | 0 | 20 | 锚点、液体规则、交替 Tile 和放置 hook 模块。 |
| `SharedTileObjectPreviewState` | `SharedRuntimeMechanisms` | `4.9.128` | `query/snapshot` | 16 | 9 | 25 | TileObject 放置预览、缓存和有效性百分比。 |
| `SharedTileObjectPlacementValueState` | `SharedRuntimeMechanisms` | `4.9.129` | `value object/query` | 16 | 0 | 16 | TileObject 坐标、样式、placement hook 和放置结果值。 |
| `SharedTilePlacementCoordinateAndDrawModules` | `SharedRuntimeMechanisms` | `4.9.186` | `definition/query` | 14 | 0 | 14 | Tile 放置坐标、绘制模块和几何输出。 |
| `SharedTilePlacementBaseAndStyleModules` | `SharedRuntimeMechanisms` | `4.9.187` | `definition/query` | 14 | 0 | 14 | Tile 放置基础、样式和共享几何输入模块。 |
| `SharedTilePaintRenderTargetState` | `SharedRuntimeMechanisms` | `4.9.218` | `presentation state` | 12 | 0 | 12 | TilePaintSystemV2 的渲染目标、缓存集合和绘制请求状态。 |
| `SharedTilePaintVariationAndColorState` | `SharedRuntimeMechanisms` | `4.9.219` | `definition/state` | 14 | 0 | 14 | Tile/墙/树/笼样式变体键与颜色缓存状态。 |

来源成员的分区内序号线索范围：174..3941；序号不定义领域边界，必须逐条回到输入表和 Version4 覆盖。

## 分区专属目标

围绕“地块、液体与世界存储”完成独立只读的 Version4 成员审查和 ECS 拆分设计。
- 围绕世界尺寸/容量、Main 的 Tile/Wall 元数据、单格材料/液体/帧位、TileObject 放置、TileEntity、Tile 快照和 TilePaintSystemV2，确认权威世界存储与编辑/渲染投影的边界。
- 回到 Terraria/Tile.cs、Terraria/Main.cs、TileObjectData、TileEntity、TilePaintSystemV2、WorldGen/液体调用者，核对单格字段的读写、帧更新、放置 hook、清理和序列化。
- 分别评估 TileCell、Tile framing、液体状态、锚点/可达性 Query、TileEntity registry/实例、放置 Command、快照 Projection 和绘制缓存。
- 核对 Tile 写入对液体、机关、地图、网络区段、持久化和客户端绘制的提交顺序；显式区分实体引用、持久化 ID、网络 ID 和外部 ID。

## 专属证据焦点

- 先完整读取本分区输入报告的成员表，再回到 D:\TRbackup\Version4 重新核对字段、属性、声明类型、初始化/注册、直接读者、直接写者、清理路径和副作用。
- 下列路径只作为本分区的检索种子；路径存在不等于成员语义已确认，必须按公共证据协议记录实际行号、符号、调用者、读者、写者、生命周期和 evidenceStatus。
  - `D:\TRbackup\Version4\Terraria.DataStructures/AnchorData.cs`
  - `D:\TRbackup\Version4\Terraria.DataStructures/AnchoredEntitiesCollection.cs`
  - `D:\TRbackup\Version4\Terraria.DataStructures/PlacementDetails.cs`
  - `D:\TRbackup\Version4\Terraria.DataStructures/PlacementHook.cs`
  - `D:\TRbackup\Version4\Terraria.DataStructures/Point16.cs`
  - `D:\TRbackup\Version4\Terraria.DataStructures/TileEntitiesManager.cs`
  - `D:\TRbackup\Version4\Terraria.DataStructures/TileEntity.cs`
  - `D:\TRbackup\Version4\Terraria.DataStructures/TileEntityType.cs`
  - `D:\TRbackup\Version4\Terraria.DataStructures/TileObjectPreviewData.cs`
  - `D:\TRbackup\Version4\Terraria.DataStructures/TileReachCheckSettings.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Tile_Entities/TECritterAnchor.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Tile_Entities/TEDeadCellsDisplayJar.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Tile_Entities/TEDisplayDoll.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Tile_Entities/TEFoodPlatter.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Tile_Entities/TEHatRack.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Tile_Entities/TEItemFrame.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Tile_Entities/TEKiteAnchor.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Tile_Entities/TELeashedEntityAnchorWithItem.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Tile_Entities/TELogicSensor.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Tile_Entities/TETeleportationPylon.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Tile_Entities/TETrainingDummy.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Tile_Entities/TEWeaponsRack.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent/TilePaintSystemV2.cs`
  - `D:\TRbackup\Version4\Terraria.Modules/AnchorDataModule.cs`
  - `D:\TRbackup\Version4\Terraria.Modules/AnchorTypesModule.cs`
  - `D:\TRbackup\Version4\Terraria.Modules/LiquidDeathModule.cs`
  - `D:\TRbackup\Version4\Terraria.Modules/LiquidPlacementModule.cs`
  - `D:\TRbackup\Version4\Terraria.Modules/TileObjectAlternatesModule.cs`
  - `D:\TRbackup\Version4\Terraria.Modules/TileObjectBaseModule.cs`
  - `D:\TRbackup\Version4\Terraria.Modules/TileObjectCoordinatesModule.cs`
  - `D:\TRbackup\Version4\Terraria.Modules/TileObjectDrawModule.cs`
  - `D:\TRbackup\Version4\Terraria.Modules/TileObjectStyleModule.cs`
  - `D:\TRbackup\Version4\Terraria.Modules/TileObjectSubTilesModule.cs`
  - `D:\TRbackup\Version4\Terraria.Modules/TilePlacementHooksModule.cs`
  - `D:\TRbackup\Version4\Terraria.Utilities/TileSnapshot.cs`
  - `D:\TRbackup\Version4\Terraria/Framing.cs`
  - `D:\TRbackup\Version4\Terraria/Main.cs`
  - `D:\TRbackup\Version4\Terraria/Sign.cs`
  - `D:\TRbackup\Version4\Terraria/Tile.cs`
  - `D:\TRbackup\Version4\Terraria/TileColorCache.cs`
  - `D:\TRbackup\Version4\Terraria/TileObject.cs`

- 输入分区抽取出的声明类型焦点：
  - `Terraria.DataStructures.AnchorData`
  - `Terraria.DataStructures.AnchoredEntitiesCollection`
  - `Terraria.DataStructures.AnchoredEntitiesCollection.IndexPointPair`
  - `Terraria.DataStructures.PlacementDetails`
  - `Terraria.DataStructures.PlacementHook`
  - `Terraria.DataStructures.Point16`
  - `Terraria.DataStructures.TileEntitiesManager`
  - `Terraria.DataStructures.TileEntity`
  - `Terraria.DataStructures.TileEntityType<T>`
  - `Terraria.DataStructures.TileObjectPreviewData`
  - `Terraria.DataStructures.TileReachCheckSettings`
  - `Terraria.Framing`
  - `Terraria.Framing.BlockStyle`
  - `Terraria.GameContent.Tile_Entities.TECritterAnchor`
  - `Terraria.GameContent.Tile_Entities.TEDeadCellsDisplayJar`
  - `Terraria.GameContent.Tile_Entities.TEDisplayDoll`
  - `Terraria.GameContent.Tile_Entities.TEDisplayDoll.DisplayDollPose`
  - `Terraria.GameContent.Tile_Entities.TEFoodPlatter`
  - `Terraria.GameContent.Tile_Entities.TEHatRack`
  - `Terraria.GameContent.Tile_Entities.TEItemFrame`
  - `Terraria.GameContent.Tile_Entities.TEKiteAnchor`
  - `Terraria.GameContent.Tile_Entities.TELeashedEntityAnchorWithItem`
  - `Terraria.GameContent.Tile_Entities.TELogicSensor`
  - `Terraria.GameContent.Tile_Entities.TETeleportationPylon`
  - `Terraria.GameContent.Tile_Entities.TETrainingDummy`
  - `Terraria.GameContent.Tile_Entities.TEWeaponsRack`
  - `Terraria.GameContent.TilePaintSystemV2`
  - `Terraria.GameContent.TilePaintSystemV2.ARenderTargetHolder`
  - `Terraria.GameContent.TilePaintSystemV2.CageTopRenderTargetHolder`
  - `Terraria.GameContent.TilePaintSystemV2.CageTopVariationkey`
  - `Terraria.GameContent.TilePaintSystemV2.TileRenderTargetHolder`
  - `Terraria.GameContent.TilePaintSystemV2.TileVariationkey`
  - `Terraria.GameContent.TilePaintSystemV2.TreeBranchTargetHolder`
  - `Terraria.GameContent.TilePaintSystemV2.TreeFoliageVariantKey`
  - `Terraria.GameContent.TilePaintSystemV2.TreeTopRenderTargetHolder`
  - `Terraria.GameContent.TilePaintSystemV2.WallRenderTargetHolder`
  - `Terraria.GameContent.TilePaintSystemV2.WallVariationKey`
  - `Terraria.Main`
  - `Terraria.Modules.AnchorDataModule`
  - `Terraria.Modules.AnchorTypesModule`
  - `Terraria.Modules.LiquidDeathModule`
  - `Terraria.Modules.LiquidPlacementModule`
  - `Terraria.Modules.TileObjectAlternatesModule`
  - `Terraria.Modules.TileObjectBaseModule`
  - `Terraria.Modules.TileObjectCoordinatesModule`
  - `Terraria.Modules.TileObjectDrawModule`
  - `Terraria.Modules.TileObjectStyleModule`
  - `Terraria.Modules.TileObjectSubTilesModule`
  - `Terraria.Modules.TilePlacementHooksModule`
  - `Terraria.Sign`
  - `Terraria.Tile`
  - `Terraria.TileColorCache`
  - `Terraria.TileObject`
  - `Terraria.Utilities.TileSnapshot`
  - `Terraria.Utilities.TileSnapshot.TileStruct`

- 若路径、类型或行号漂移，记录 version-drift 或 evidence-mismatch；不得静默沿用库存报告行号。Version4 是 Terraria 私有实现的事实来源，完整源码只补证 Version4 已存在文件，tModLoader 只交叉验证公开边界，SS14 只参考结构粒度。

## 专属审查问题

- 单格 Tile 的材料、墙体、液体、帧位和 bit 状态是否共享同一生命周期/写者？哪些应拆成内聚 Component，哪些是派生 framing 或缓存？
- TileEntity registry、类型标识、实例状态和世界锚点如何分离；展示架、传感器、晶塔和训练假人是否各有独立 owner？
- 放置预览、放置结果、placement hook、Tile 快照和绘制 target 是否是 Query/Command/Projection，而不是权威世界存储？
- Tile 修改如何向液体、机关、地图、网络和存档发布变更；失败、回滚和幂等性由哪个 System 或 Adapter 保证？

## 专属不拆分边界

- 不要把单格 Tile、TileEntity、放置预览、Paint 缓存、地图快照和液体传播合并为一个 WorldTileComponent。
- 不要把 TileEntity 类型 ID、网络/存档标识或单次 placement result 直接当成通用共享组件。
- 不要让 Snapshot、绘制缓存或 Query 反向写入权威 Tile 状态；放置与 framing 的写入必须通过显式 System/Command。

专属跨域提醒：重点记录与液体/机关、世界生成、网络区段、地图、UI 和持久化的 integration-risk；TileCoordinate、TileSnapshot、TileEntityReference、WorldSection 等共享候选必须 crossSubsystemOwner: integration-review。

## 输出要求

- 将完整研究报告只写入：D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-11-version4-non-authoritative-P04-world-tiles-storage-public-decomposition.md
- 不要写入输入分区报告、公共提示词、其他分区报告、源代码、测试、项目文件或迁移文件。
- 报告必须逐条覆盖本分区成员，给出成员归属表、状态所有权、Component/System/Query/Command/Adapter/Projection 边界、接口契约、依赖方向、顺序约束、不拆分项、兼容策略、evidence-gap、blocking-decision、focused verifier 计划和 Integration Handoff。
- 本轮只读审查不运行编译型命令；没有实际执行的 verifier 必须写 verificationStatus: not-run。
- proposed 类型、路径和接口必须明确标记 proposed；不得写成当前 NLTX 已存在或已经迁移。
- 跨两个或以上分区使用的类型、接口、ID、快照、值对象和 System 顺序只能提出候选，必须写 crossSubsystemOwner: integration-review，不得在本报告单方面裁决。
- 不得修改任何生产代码、测试、项目文件、生成器、公共协议、分区报告或其他并行会话输出。

## 分区完成检查

- [ ] 逐条覆盖本分区全部 320 条成员，成员事实来自输入表但关键语义已回到 Version4 复核。
- [ ] 只使用本分区叶子清单；没有复制、重新设计或宣布其他 P 分区成员。
- [ ] Component 只承载内聚权威状态；派生值、缓存、查询、短期 payload、日志、网络/存档/UI 输出按正确角色隔离。
- [ ] 所有共享候选写 crossSubsystemOwner: integration-review；没有把 proposed 设计写成已实现能力。
- [ ] 未执行的构建、测试和 verifier 均明确 verificationStatus: not-run；证据缺口、冲突和 blocking-decision 已单列。
- [ ] 输出文件路径唯一为：D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-11-version4-non-authoritative-P04-world-tiles-storage-public-decomposition.md

## 会话结束回报

最终消息只回报以下字段：
- partitionId: P04
- outputReport: D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-11-version4-non-authoritative-P04-world-tiles-storage-public-decomposition.md
- evidenceSourcesRead: 实际读取的 Version4 文件/类型、必要的完整源码补证、tModLoader 页面和 SS14 参考
- evidenceGaps: 仍缺失或冲突的关键证据
- blockingDecisions: 需要最终整合会话裁决的 owner/顺序/事务边界
- verificationStatus: not-run
- productionCodeModified: no

本提示词只定义本分区的专属范围和审查抓手；公共协议负责共同证据规则、报告结构、禁止项和 Integration Handoff。
