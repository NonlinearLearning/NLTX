# Version4 WorldGenerationAndEcology 公开拆分审查与 ECS 设计报告

## 0. 报告元数据与结论状态

| 字段 | 值 |
| --- | --- |
| `reportId` | `WorldGenerationAndEcology-20260905-version4-public-decomposition` |
| `subsystemId` | `WorldGenerationAndEcology` |
| `taskNumber` | `18` |
| `layer` | `authoritative-simulation` |
| `reportPath` | `D:\TRbackup\NLTX\docs\第一轮审查\2026-09-05-version4-world-generation-and-ecology-public-decomposition.md` |
| `evidenceStatus` | `partial` |
| `nltxStatus` | `partial` |
| `verificationStatus` | `not-run` |
| `readOnlyScope` | Version4、完整可编译参考、tModLoader 本地公开文档、Space Station 14 最小 ECS 参考、当前 NLTX 源码和既有规则材料 |
| `agentMode` | 本次未启动子代理；本报告由当前会话独立完成 |

本报告只生成设计和证据记录，不创建 Component、System、Query、Command、Adapter、Projection、测试或迁移文件。报告中所有新类型、新路径、接口和伪代码都属于 `status: proposed`；跨子系统共享类型都属于 `crossSubsystemOwner: integration-review`。

## 1. 执行摘要

Version4 的世界生成与生态责任并不是一个只负责“生成地形”的小模块，而是由 `Terraria.WorldGen`、`Terraria.WorldBuilding.WorldGenerator`、`Terraria.IO.WorldFile`、`Terraria.Liquid`、`Main.tile` 和若干 `GenVars`/住房状态共同形成的全局流程。它同时包含：世界清空和随机初始化、生成 Pass 注册与执行、Biome/感染/生态变换、结构和树、液体传播与交互、住房扫描、生成完成交接、存档加载恢复，以及与 Hardmode 长事务的相邻边界。

Version4 证据还存在重要的版本/裁剪事实：实际文件是 `D:\TRbackup\Version4\Terraria.IO\WorldFile.cs`，不是任务提示中的 `Terraria\WorldFile.cs`；`WorldGen.AddGenerationPass` 的两个核心重载位于 `WorldGen.cs:9175-9176` 且为空实现；`WorldGenerator.RunPass` 为空实现。完整可编译参考可以补证这些已存在成员的预期形状，但不能把完整参考的行为直接当成 Version4 已实现行为。因此，本报告对“生成 Pass 的注册链存在”与“完整 Pass 已被 Version4 执行”严格区分，后者为 `partial`，不能据此作行为等价声明。

当前 NLTX 的实际状态比任务包中的旧描述更完整：`dome` 中已经存在受限 `WorldGenerationPipeline`，并且有生成请求、阶段状态、确定性随机状态、检查点、阶段 trace、Tile/Liquid command batch、`TileChangeCommitSystem`、`LiquidChangeCommitSystem`、液体传播会话和有限验证。`WorldGenerationPipeline.GenerateCore` 依次运行受限地形、洞穴、Biome、矿石、结构、树、液体、frame 和验证路径。然而，这套流水线只支持受限规则和有限 Pass 集合，尚未证明覆盖 Version4 全量生成 Pass、完整住房扫描、完整液体运行时 solver、全量 `WorldFile` 交接或“生成 + 液体稳定 + 住房修复 + 持久化 + ready 发布”的统一屏障。因此当前结论仍为 `nltxStatus: partial`。

建议的拆分方向是把“生成计划”“单 Pass 可恢复状态”“Biome/生态权威状态”“住房纯查询与扫描结果”“生成命令”“Tile 提交端口”“持久化适配器”和“ready 屏障”分开，并把副作用集中到显式提交边界。`SceneMetrics` 只能作为派生查询或缓存，不能自动成为 Biome 权威状态；`PlaceTile`、`KillTile`、`Convert`、液体合并和网络/掉落副作用不能仅通过把代码搬到多个 System 来消除，必须先确定 Tile commit authority。

Hardmode 必须维持独立的 `WorldProgressionAndTransition` 归属。Version4 的 `StartHardmode` 和 `TransformWorldOnBackgroundThread` 是一个有保存锁、后台任务、主线程 follow-up、网络/公告副作用的长期事务，不应重新并入普通世界生成 Tick。

本报告最重要的未决架构问题有四个：

- Tile/TileEntity 的最终写入口是 `WorldStorage`、生成专用 `TileCommitPort`，还是统一 integration commit boundary；
- ready 是生成 Pass 完成、液体稳定、住房扫描完成，还是连持久化提交成功都必须满足；
- Hardmode 长事务的最终 owner 是 Progression、转换 command 编排器，还是独立 Transition/Persistence adapter；
- `WorldSectionId`、`TileCoordinate`、实体引用、持久化 ID、网络 ID、世界时间和 ready 值对象的共享 owner。

这些问题会改变依赖方向和提交语义，留给最终整合会话裁决。

## 2. 子系统范围与不负责内容

### 2.1 本报告负责的范围

本子系统的候选责任边界是：

1. 创建世界生成请求、生成计划、Pass 顺序、Pass 开关、Pass 版本和确定性随机上下文。
2. 执行地形、洞穴、Biome 表面、矿石、结构、树、感染和其他生成期生态变换，并把 Tile/Wall/Liquid/Structure 变更转换为显式 command。
3. 保存可暂停、失败、重试和检查点恢复所需的生成行为状态，但不把世界 Tile 快照误当成普通 ECS Component。
4. 计算 Biome/生态派生查询和更新权威生态状态；区分 `BiomeState` 与 `SceneMetrics`。
5. 生成住房扫描所需的纯资格查询、房间边界、需求判断、评分输入和待提交的住房结果。
6. 在生成完成、液体稳定、住房交接、持久化提交和对外 ready 发布之间建立显式依赖边界。
7. 为 WorldStorage、LiquidSimulation、NpcAndTownSimulation、WorldSession 和 server/client projection 提供稳定的 domain seam。

### 2.2 明确不负责的内容

- `WorldStorage` 的最终 Tile/TileEntity 存储实现、分块索引、存档字节格式的最终 owner；本报告只提出交接端口。
- `LiquidSimulation` 的完整运行期 `Liquid.UpdateLiquid` solver、玩家驱动液体更新和普通 Tick 调度；本报告只处理生成期液体 handoff。
- `WorldProgressionAndTransition` 的 Hardmode 长事务、矿石替换、后台世界转换、网络重同步和成就/公告；本报告只记录交接和隔离要求。
- `NpcAndTownSimulation` 的 NPC AI、TownManager 业务、最终住房分配和 NPC 生命周期；本报告提供住房查询和扫描结果边界。
- `SpatialSimulation`、碰撞、光照、渲染、音乐、UI、Scene effect 和客户端表现。
- tModLoader 扩展 API 的实现、外部 mod 的业务语义以及 Version4 私有实现之外的兼容承诺。
- 改动固定的 19 个子系统清单，或把其他候选域重新合并到本子系统。

## 3. 证据优先级、来源角色与方法

### 3.1 固定优先级

证据顺序为：

```text
Version4
→ 完整可编译参考源码
→ tModLoader v2026.07 本地公开 API 文档
→ Space Station 14 ECS 结构参考
```

Version4 决定真实行为、覆盖范围、字段和调用链；完整参考只补证 Version4 已存在文件/成员的删减或空实现；tModLoader 只交叉验证公开生命周期、存档、网络和扩展边界；Space Station 14 只参考组件粒度、System/Query、任务、取消、Tile 提交和迁移边界，不能推断 Terraria 语义。

### 3.2 本次实际读取的来源记录

| source | version/title | query or focus | evidence obtained | gaps / stop reason |
| --- | --- | --- | --- | --- |
| `D:\TRbackup\Version4` | Version4 目标源码 | `WorldGen`、`Liquid`、`WorldGenerator`、`GenPass`、`WorldFile` | 生成生命周期、Pass 编排、GenVars/住房/液体/Tile 写入、Hardmode 和存档交接 | 多个生成入口/Pass 基础成员为空实现；以 `partial` 停止，不把完整参考提升为目标行为 |
| `D:\TRbackup\无任何删减通过编译` | 完整可编译参考 | 与 Version4 同路径、同类型、同成员 | 补证 `RunPass`、Pass 应用、Pass 结果、Manifest、Snapshot、随机和 `AddGenerationPass` 的成员级行为 | 完整参考独有文件不加入 Version4 基线 |
| `D:\TRbackup\tmodloader-api-docs-stable\index.html` | `tModLoader: Main Page`，`tModLoader v2026.07` | `ModSystem`、`ModBiome`、`SceneMetrics`、`Tile`、`WorldGen`、`WorldFile`、`WorldFileData` | 公开生成 hook、Hardmode hook、加载/保存、网络方向、液体字段和派生 SceneMetrics 语义 | 不确认 Version4 私有实现或 NLTX 兼容性 |
| `C:\Users\shan\Downloads\ECS\space-station-14-master` | SS14 当前参考源码 | `DungeonSystem`、`DungeonJob`、`IDunGenLayer`、`TileSystem`、`TurfSystem`、地图管理/迁移 | 任务切片/取消/恢复、生成层、Tile history/dirty、纯查询、选择/持久化/迁移分界 | 无 Terraria 行为对应证据，只作结构参考 |
| `D:\TRbackup\NLTX\src`、`dome\src` | 当前工作树源码 | `WorldSession`、`WorldGeneration`、`WorldStorage`、`WorldBootstrap`、server ready/persistence | 受限生成 pipeline、command commit、液体会话、检查点和启动/持久化骨架 | 完整 Pass、住房执行、ready barrier、唯一写入口和格式兼容均未闭合 |

### 3.3 路径和版本偏差

- 任务提示给出的 `D:\TRbackup\Version4\Terraria\WorldFile.cs` 不存在；实际证据为 `D:\TRbackup\Version4\Terraria.IO\WorldFile.cs`，标记 `evidence-mismatch`，以下均使用实际路径。
- 任务包关于“当前 NLTX 没有 generation executor、确定性 Pass 或 Tile commit”的描述与现场源码不一致，属于 `version-drift`。现场存在受限 executor、确定性状态、命令提交和有限验证，但不能据此声称完整 Version4 实现。
- 根目录 `D:\TRbackup\NLTX\约束\公共拆分约束.md` 和 `D:\TRbackup\NLTX\docs\flowstate\README.md` 当前未找到；已读取仓库实际存在的 `dome\docs\flowstate\README.md` 作为有限上下文，不把缺失文件当成设计事实。

## 4. Version4 真实代码证据

下表中的状态只表示源码证据的充分程度，不表示迁移状态。

| ID | 实际证据 | 读者 / 写者 | 生命周期与副作用 | 状态 |
| --- | --- | --- | --- | --- |
| `V4-WG-01` | `D:\TRbackup\Version4\Terraria\WorldGen.cs:6267-6304`，`worldGenCallback` 调用 `GenerateWorld`，成功后调用 `WorldFile.SaveNewWorld`；`CreateNewWorld` 设置 `generatingWorld`、用世界种子初始化 `UnifiedRandom`、设置 `isGeneratingOrLoadingWorld` 并启动生成 | World creation caller、server/world file 读取流程；`WorldGen` 写生成标志和随机源 | 新世界创建、异步生成、成功保存；失败不进入成功保存路径 | `confirmed` |
| `V4-WG-02` | `WorldGen.cs:6336-6384` 的 `serverLoadWorldCallBack` 处理加载失败重试、`.bak` 备份恢复和完成回调；`WorldGen.cs:6387-6664` 的 `clearWorld` 清空注册表、TileEntity、Wiring、地图、NPC/事件、液体和生成状态 | `WorldFile`/服务器加载者读；`clearWorld` 写全局世界和生成状态 | 加载前重置、失败恢复和重试；涉及全局状态，非纯函数 | `confirmed` |
| `V4-WG-03` | `WorldGen.cs:10108-10147` 的 `GenerateWorld` 创建配置、调用 hook、创建 `WorldGenerator`、执行 `clearWorld`、`Reset`、`AddPasses`、special-seed 禁用和 `_generator.GenerateWorld()`，最后 `Finish`；`finally` 恢复临时状态 | `CreateNewWorld`、服务器生成调用；`WorldGen`/`WorldGenerator` 写生成状态和 Pass 容器 | 生成事务入口；异常和临时 Tile 状态恢复 | `partial` |
| `V4-WG-04` | `WorldGen.cs:10148-10552` 的 `Reset` 根据 seed/special seed 重建随机源、`StructureMap`、表层/岩层/海滩/雪地/丛林/湖泊/Dungeon/矿石等 GenVars，并 `Liquid.ReInit`；`WorldGen.cs:10553` 起的 `AddPasses` 注册 Terrain、Biome、Cave、Dungeon、Liquid、Structure、Tree、Cleanup 等大量 Pass | 生成入口读配置和种子；`Reset`/`AddPasses` 写 GenVars/Pass 列表 | 每次新世界生成前初始化；随机消费和 Pass 顺序是行为敏感点 | `confirmed`（初始化/注册存在） |
| `V4-WG-05` | `WorldGen.cs:9175-9176` 的两个 `AddGenerationPass` 核心重载为空；同文件 `WorldGen.cs:10553` 起仍有大量调用 | `AddPasses` 写入的目标应是 `_generator` Pass 列表；当前成员体未提供完整执行证据 | 不能仅由调用点证明 Pass 已注册/已执行 | `partial`，并带 `evidence-gap` |
| `V4-WG-06` | `D:\TRbackup\Version4\Terraria.WorldBuilding\WorldGenerator.cs:24-260` 的 `Controller` 持有 Pass、当前/完成 Pass、暂停、hash mismatch、snapshot 频率和 Abort；`GenerateWorld` 在 `:291-339` 编排；`RunPass` 在 `:341-343` 为空 | 生成入口读 Controller；`WorldGenerator` 写 Pass 状态和 Manifest 结果 | 应有暂停、Abort、Pass 进度和失败边界，但当前 Version4 的 Pass 执行链被裁剪 | `partial` |
| `V4-WG-07` | `WorldGen.cs:5310-5360` `RoomNeeds`；`:5361` 起 `QuickFindHome`；`:5466-5481` `CountTileTypesInArea`；`:5510-5641` `ScoreRoom`；`:5642-5670` 房间边界；`:5687-5697` 房间判断；`:5699-5772` `StartRoomCheck` 栈遍历和边界/大小检查；`:5773` 起 `CheckRoom` 检查固体、门/栅栏、墙和特殊条件 | Town NPC/住房逻辑读房间 Tile、NPC home 坐标；WorldGen 写静态房间扫描状态和 NPC 住房相关字段 | 纯资格计算与写入/踢出住房混合在静态全局中；加载后还会再次恢复 | `confirmed`（逻辑存在），`partial`（独立边界未形成） |
| `V4-WG-08` | `WorldGen.cs:9987-10017` 的 `BiomeTileCheck` 扫描约 50 Tile 半径并按 Tile/Wall 类型判定；`WorldGen.cs:59400` 起 `UpdateWorld` 读取 Hardmode、生态更新条件并运行 TileEntity、液体和生态更新 | 玩家/生态/生成查询读 Tile；`BiomeTileCheck` 不等于权威 Biome 状态 | 查询可能被频繁调用；`SceneMetrics`/Biome 结果必须区分派生值与权威事实 | `confirmed`（查询行为），`partial`（权威归属） |
| `V4-WG-09` | 生成结束交接位于 `WorldGen.cs:21654-21671` 附近：遍历液体调用 `Liquid.LiquidCheck`，准备额外 spawn、清理 Town NPC 位置、更新 Angler 任务和进度；`:21704-21711` `Finish` 更新 `WorldFileMetadata`、发布世界生成通知并清理状态 | 生成器写生成尾部状态；WorldFile、NPC、液体和通知系统读取 | 这是生成完成到运行期的交接，不是单个 Pass 的尾部计算 | `confirmed`，但最终 ready 屏障 `unresolved` |
| `V4-WG-10` | `WorldGen.cs:21718` 起 `LiquidInteractionsCleanup` 直接读取/写入 Tile 液体类型和数量，处理合并与特殊 Tile；`WorldGen.cs:4454` `EmptyLiquid`、`:4478` `PlaceLiquid` | 生成、Liquid、Tile 逻辑读写 `Main.tile` 液体字段 | 直接 Tile mutation、液体交互和潜在破坏副作用 | `confirmed` |
| `V4-WG-11` | `Liquid.cs:28` `numLiquid`、`:34` `quickSettle`；`QuickWater` `:111` 起；`UpdateLiquid` `:1015-1189`；`AddWater` `:1190-1234`；`LiquidCheck` `:1238-1321`；`CreateLiquidMergeTile` `:1322-1323` 为空；合并规则 `:1324-1395`；液体检查 `:1396-1426` | Liquid 更新读玩家数量、工作集、Tile 液体；写液体队列、Tile、网络变化，可能调用 `KillTile` | 运行期 solver、生成期检查和合并共用全局状态；当前目标实现有裁剪 | `partial` |
| `V4-WG-12` | `WorldGen.cs:26084-26102` 的 `StartHardmode` 设置 `Main.hardMode`、保护物品、启动后台转换并在主线程后续发布公告/成就/网络区段；`:26103-26127` `TransformWorldOnBackgroundThread` 使用 `_transformingWorld` 和 `WorldFile.IOLock` | Progression/世界转换读写 Tile、Hardmode、存档锁和网络状态 | 长事务、后台线程、保存互斥、主线程 follow-up；不属于普通生成 Tick | `confirmed`（边界），owner 归属 `integration-review` |
| `V4-WG-13` | `WorldGen.cs:47823` 区域 `Convert`、`:47843` 单 Tile/Wall `Convert`；`:50970` `PlaceTile`；`:52561` `KillTile`；`:67228-67310` `WaterCheck`；`:67311` 起 `ClearPendingLiquid` | WorldGen、交互、Progression、Liquid 和结构逻辑读写 `Main.tile`；`PlaceTile/KillTile` 还触发 frame、掉落、TileEntity、Wiring、网络等 | 这是 Tile 写入根和副作用汇聚点；直接搬迁会改变顺序和失败语义 | `confirmed`，唯一 authority `unresolved` |
| `V4-WF-01` | 实际路径 `D:\TRbackup\Version4\Terraria.IO\WorldFile.cs:658-867` 的 `LoadWorld`：自动生成分支解析选项、调用 `WorldGen.GenerateWorld`，成功后 `SaveNewWorld`；加载后 `:750-781` 调用 `Liquid.QuickWater`、`WaterCheck`、`UpdateLiquid` 循环并清理加载/生成标志 | World bootstrap 读写 WorldGen、Liquid 和存档状态 | 自动生成与加载恢复共享交接；液体稳定发生在 ready 之前的候选路径 | `confirmed` |
| `V4-WF-02` | `WorldFile.cs:870-932` `SaveNewWorld`/`SaveWorld`/`_SaveWorld`；`:933` 起 `InternalSaveWorld` 使用临时文件、验证、备份和写入；`:1465` 写入 `WorldGen.Manifest.Serialize()`；`:1469` 起保存 Tile/液体 | WorldFile 读取所有持久化域并写文件、备份、锁；生成成功路径触发保存 | 原子性、重试、备份和 I/O 锁必须在 Adapter 交接中可测试 | `confirmed` |
| `V4-WF-03` | `WorldFile.cs:1938-1953` 加载后对 Town NPC 再调用 `WorldGen.StartRoomCheck`，尝试 `TownManager.HasRoom`，失败则 `KickOut` 并设置 homeless | WorldFile/NPC/Town 读住房扫描；WorldFile 写加载后的 NPC 住房状态 | 住房不是只在生成 Pass 内发生，而是加载恢复交接的一部分 | `confirmed` |

## 5. 完整参考源码补证表

完整参考只用于补证 Version4 已存在的同路径/同成员，不提升 Version4 的覆盖状态。

| ID | 补证来源 | 补证内容 | 对当前结论的限制 | 状态 |
| --- | --- | --- | --- | --- |
| `REF-WG-01` | `D:\TRbackup\无任何删减通过编译\Terraria.WorldBuilding\WorldGenerator.cs:418-577` | 完整 `GenerateWorld`/`RunPass`；每个 Pass 以 `_seed` 初始化 `UnifiedRandom`；调用 `GenerationProgress.Start/End`；异常报告；写 `GenPassResult.Name/DurationMs/RandNext` | 只能说明空实现应补的行为形状，不能证明 Version4 已有这些行为 | `partial` |
| `REF-WG-02` | `...\Terraria.WorldBuilding\GenPass.cs:1-42` | `Enable`、`Disable` 和 `Apply` 调用抽象 `ApplyPass` | Version4 的 `GenPass` 只有部分契约；Pass 可禁用/调用顺序是行为风险点 | `partial` |
| `REF-WG-03` | `...\Terraria.WorldBuilding\GenPassResult.cs:1-39` | `Name`、`DurationMs`、`RandNext`、`Hash`、`Skipped` 和 `Matches` | 不能把完整结果 Manifest 当成 Version4 当前可验证输出 | `partial` |
| `REF-WG-04` | `...\Terraria.WorldBuilding\GenerationProgress.cs:1-100` | 进度限制、权重累计、`Start`/`End` 生命周期 | Version4 当前生成完成交接仍受空 Pass/裁剪影响 | `partial` |
| `REF-WG-05` | `...\Terraria.WorldBuilding\WorldManifest.cs:9-65` | `GenPassResults`、`FinalHash`、JSON 序列化/反序列化/Clone | 是 Manifest 设计补证，不是 NLTX 持久化兼容证据 | `partial` |
| `REF-WG-06` | `...\Terraria.WorldBuilding\WorldGenSnapshot.cs:15` 起 | 通过 JSON converter 保存/恢复 `GenVars`；snapshot 包含 Manifest、GenVars 和 Tile 状态 | 大快照不应机械变成普通 ECS Component；需单独的 checkpoint/projection | `partial` |
| `REF-WG-07` | `...\Terraria.WorldGen.cs:10373-10393` | `AddGenerationPass` 通过 `_generator.Append(...)` 注册 `PassLegacy`/`GenPass` | 直接补足 `WorldGen.cs:9175-9176` 的空体预期；目标 Version4 仍标记 `partial` | `partial` |
| `REF-WG-08` | `...\Terraria.WorldBuilding\StructureMap.cs:9-74` | 结构保护范围和 Tile 资格检查；锁保护结构集合 | 支持结构提交边界设计，不决定 NLTX 最终 Tile owner | `partial` |
| `REF-WG-09` | `...\Terraria.WorldBuilding\WorldGenerationOptions.cs:9-123` | Special Seed 注册、选择、重置、种子文本解析和自动生成标志 | 只补生成选项生命周期；当前 NLTX 仍通过 `EnsureSupportedRules` 限制规则 | `partial` |

## 6. tModLoader 公开 API 交叉验证

本节的来源是本地 Doxygen 镜像，不属于 Version4 权威实现。首页 `D:\TRbackup\tmodloader-api-docs-stable\index.html:8` 标题为 `tModLoader: Main Page`，`:29` 显示 `tModLoader v2026.07`。

| 公开边界 | 实际文档证据 | 交叉验证结论 | 状态 |
| --- | --- | --- | --- | --- |
| 生成 Pass 扩展与 Hardmode Pass | `D:\TRbackup\tmodloader-api-docs-stable\class_mod_system.html:139-140` 的 `ModifyHardmodeTasks(List<GenPass>)`；`:163-164` 的 `ModifyWorldGenTasks(List<GenPass>, ref double)`；`ModifyWorldGenTasks` 说明锚点 `class_mod_system.html#a6d3537099fd511aa551fa4d28052387b`；Hardmode 说明锚点 `#ab3224849ab5d16570434c13e66a4eb9e` | 生成 Pass 与 Hardmode Pass 是不同生命周期；设计上不能把 Hardmode 重新并入普通生成计划 | `confirmed`（公开边界） |
| 世界加载/保存 hook | `class_mod_system.html:131-133` `LoadWorldData`，实际锚点 `#a12097aab73db65bd17b2bde505e1022d`；`:334` `SaveWorldData`；详细说明约 `:603-630`、`:1078-1104` | 扩展数据必须在存档 Adapter/Projection 边界处理，不能渗入核心生成 Component | `confirmed`（公开边界） |
| 网络方向 | `class_mod_system.html:166-170`；`NetReceive` 锚点 `#a144ef598fa0b3bcc2a0ac6bd1c5467aa`，说明约 `:839-862`；`NetSend` 说明约 `:872-896` | `NetReceive` 是客户端接收世界数据的公开边界；权威生成状态仍应由 server 持有，客户端只接收投影 | `confirmed`（公开边界） |
| 世界更新和生成后 hook | `class_mod_system.html` 的 `PostUpdateWorld` 约 `:258`、`PostWorldGen` 约 `:262` | 生成后回调和运行期世界更新是不同阶段，支持 ready barrier 与普通 Tick 分离 | `confirmed`（公开边界） |
| Biome 与 Scene effect | `class_mod_biome.html:105-106` `IsBiomeActive`，锚点 `#a440b792825b471829288a02628f58499`；`:108-109` `IsSceneEffectActive`，锚点 `#a4b610d384cce4a6d03d875a45f118dc4` | `IsSceneEffectActive` 自动转发 `IsBiomeActive` 的结果；公开场景效果不等于权威世界 Biome 状态 | `confirmed`（公开边界） |
| SceneMetrics | `class_scene_metrics.html:110` `ScanAndExportToMain`；详细成员锚点为 `#aad9cd2adbe69564e7db8052770c159f5` | 只能作为扫描得到的派生指标/缓存；不能替代 `BiomeEcologyState` 权威事实 | `confirmed`（公开边界） |
| Tile 液体字段 | `struct_tile.html:250-253` `LiquidAmount`，说明锚点 `#a420edd17ed083bfa38db1bfb5a8f5dc7`；`:256-259` `LiquidType`；`:272-274` `SkipLiquid` | 液体状态与 Tile 紧密耦合；提交边界必须显式处理 amount/type/skip 和失效顺序 | `confirmed`（公开字段） |
| WorldGen 生成、Tile、住房、Hardmode | `class_world_gen.html:661` `GenerateWorld`；`:920` `KillTile`；`:1222` `PlaceTile`；`:1557` `StartHardmode`；`:1563` `StartRoomCheck`；`PlaceTile` 详细说明约 `:2689-2752`，`KillTile` 约 `:2423-2474` | 文档明确 `PlaceTile` 返回值不完全可靠，应检查实际 Tile；运行期成功变更需通过 `NetMessage.SendTileSquare` 同步；Tile 操作伴随 frame/掉落/网络边界 | `confirmed`（公开边界） |
| WorldFile 与 WorldFileData | `class_world_file.html:162` `LoadWorld`、`:207-213` `SaveWorld`、`:228` `ValidateWorld`、`:240` `OnWorldLoad`；`class_world_file_data.html:199` `IsHardMode`、`:214` `UniqueId`、`:217` `WorldGeneratorVersion`、`:248-251` `Seed`/`SeedText` | 存档元数据、生成版本、seed、Hardmode 和加载回调都需要稳定持久化/投影边界 | `confirmed`（公开边界） |

这里的 `confirmed` 仅表示公开文档明确说明了边界，不表示 Version4 或 NLTX 已实现 tModLoader API 兼容。

## 7. Space Station 14 最小 ECS 参考

SS14 仅用于结构粒度和副作用隔离参考，不能推断 Terraria 的房间、Biome、Tile 或生成语义。

| 实际文件 | 类型/方法 | 可参考的结构 | 不可推断的部分 |
| --- | --- | --- | --- |
| `C:\Users\shan\Downloads\ECS\space-station-14-master\Content.Server\Procedural\DungeonSystem.cs:24-254` | `DungeonSystem`、`JobQueue`、取消令牌、生成任务入队 | 长任务拥有明确的队列、取消和完成边界；可对应生成暂停/恢复 seam | 不证明 Terraria Pass 顺序或世界生成算法 |
| `...\Content.Server\Procedural\DungeonJob\DungeonJob.cs:27-325` | `DungeonJob`、显式 seed、目标 Grid、配置、保留 Tile 集合、分层执行、`ValidateResume` | 生成工作可切片、暂停、恢复、取消，并把随机源与任务状态显式化 | 不复制 `DungeonJob` 命名、层语义或任务模型 |
| `...\Content.Shared\Procedural\IDunGenLayer.cs:4` 起 | `IDunGenLayer` | 生成层契约可以启发 `Pass` seam | 不等同于 Version4 `GenPass` 行为 |
| `...\Content.Shared\Maps\TileSystem.cs:21-303` | `TileSystem`、Replace/History/Dirty 处理 | Tile 变更后的 history、chunk、dirty、替换和后处理有集中边界 | 不决定 NLTX 的 `WorldStorage` owner 或 Terraria 掉落语义 |
| `...\Content.Shared\Maps\TurfSystem.cs:110-204` | `IsTileBlocked`、`IsSpace` | 资格判断与写入分离；Query 可保持纯 | 不把 Terraria `Collision` 或住房条件映射成同名语义 |
| `...\Content.Server\Maps\GameMapManager.cs:16-217` | 地图选择、配置、随机源、持久化选择和日志依赖 | 选择状态、随机源、日志和外部配置可由 Adapter/Query 分开 | 不推断 Terraria 世界选择规则 |
| `...\Content.Server\Maps\MapMigrationSystem.cs:7-73` | 迁移事件、旧/新 prototype、删除/重命名映射 | 版本迁移应是显式 adapter/event，而不是隐式修改生成状态 | 不证明 NLTX 或 Version4 存档格式可以直接迁移 |
| `...\Content.Shared\Maps\GameMapPrototype.cs:18-66` | `GameMapPrototype`、MapPath、Grid、Stations、Persistence | 地图定义和运行时实例状态可分离 | 不复制 SS14 的 prototype、命名或目录结构 |

Space Station 14 无直接对应 Version4 世界生成行为证据；以下边界仅由 Version4 真实代码和 NLTX 项目约束决定。

## 8. 成员、字段、方法、读写者与生命周期盘点

### 8.1 Version4 和参考侧：成员、字段、方法、读者、写者与生命周期盘点表

| 成员/状态 | 声明处 | 主要读者 | 主要写者 | 生命周期 | 状态种类与候选归属 |
| --- | --- | --- | --- | --- | --- |
| `generatingWorld`、`isGeneratingOrLoadingWorld` | `WorldGen.cs:6267-6304`、`:10108-10147`、`WorldFile.cs:750-781` | World update、WorldFile、server callback | WorldGen/Create/Load/finally | 创建、加载、生成、完成/失败清理 | 权威生命周期状态；候选 `WorldSession`/`ReadyTransition`，非表现字段 |
| `GenVars` 表层、岩层、Dungeon、矿石、Biome 统计等 | `WorldGen.cs:10148-10552`、`clearWorld:6387-6664` | 生成 Pass、Biome、结构、住房、存档 | `Reset`、各 Pass、cleanup | 每次生成重置，完成后进入 Manifest/snapshot | 生成期权威状态；候选 `GenerationPlan`/`BiomeEcologyState`，不能一股脑成为巨型 Component |
| `_generator`、Pass 列表、Controller | `WorldGen.cs:10108-10147`、`WorldGenerator.cs:24-343` | `AddPasses`、GenerateWorld、progress/controller | `AddGenerationPass`、`RunPass`/GenerateWorld | 生成事务内 | 行为编排状态；候选 `GenerationPlan`/`PassState` |
| `Manifest`/`GenPassResult` | `WorldFile.cs:1465`；完整参考 `WorldManifest.cs:9-65` | WorldFile、验证、调试/trace | 生成器和存档 | Pass 完成到保存/加载 | snapshot/持久化投影；候选 `GenerationManifestProjection`，不能作为运行期 authority |
| 房间边界、`roomTiles`、需求计数、NPC home/homeless | `WorldGen.cs:5310-5773`、`WorldFile.cs:1938-1953` | Town/NPC、WorldFile、生成结束 | `StartRoomCheck`、`ScoreRoom`、加载恢复 | 生成、加载后恢复、NPC 住房变化 | 混合权威/派生；查询与 assignment 必须分离 |
| `Main.tile` 的 Tile/Wall/frame/liquid | `WorldGen.cs:47823-52561`、`Liquid.cs:1190-1426` | 几乎所有生成、Liquid、交互、生态和存档 | `PlaceTile`、`KillTile`、`Convert`、Liquid | 生成、运行期、Hardmode、保存 | 世界存储权威；最终 owner `unresolved` |
| `numLiquid`、液体工作集、`quickSettle` | `Liquid.cs:28-34`、`:1015-1321` | `UpdateLiquid`、`LiquidCheck`、WorldFile | Liquid solver、WaterCheck | 运行期与加载交接 | LiquidSimulation 行为状态；生成期只是 handoff，不是全 solver |
| `Main.hardMode`、`_transformingWorld`、`WorldFile.IOLock` | `WorldGen.cs:26084-26127` | Progression、WorldFile、网络/公告 | StartHardmode、后台转换和主线程 follow-up | 一次 Hardmode 长事务 | `WorldProgressionAndTransition`；本报告不重新归入生成 |

### 8.2 当前 NLTX

| 成员/状态 | 实际声明处 | 已观察到的读者/写者 | 生命周期 | 状态与缺口 |
| --- | --- | --- | --- | --- |
| `WorldPreparationState`、`WorldGenerationLifecycleState` | `D:\TRbackup\NLTX\src\WorldSession\WorldGeneration\WorldPreparationState.cs:3`；`WorldGenerationLifecycleState.cs:3-15` | WorldSession 和 legacy flag query | Loading/Generating/Ready/Failed/Unloading | `confirmed` 为状态模型；不证明有完整 ready barrier |
| `WorldDescriptorState`、`WorldRulesState` | `src\WorldSession\WorldGeneration\WorldDescriptorState.cs:3`；`WorldRulesState.cs:3` | world bootstrap/session/生成请求 | 世界创建、加载和恢复 | `confirmed` 为数据模型；跨域持久化 owner 未定 |
| `WorldEcologyScheduleState`、`TownHousingRegistry` | `src\WorldSession\WorldGeneration\WorldEcologyScheduleState.cs:3`；`TownHousingRegistry.cs:5` | session/生态和 Town 相关调用点 | 运行期调度和住房 registry | `partial`；存在状态但未证实本子系统拥有执行和最终写入口 |
| `WorldGenerationRequest` | `dome\src\Terraria.Dome.Simulation\WorldGeneration\WorldGenerationRequest.cs:7-307`，seed/rule/profile 属性 `:145-156`、`:268-303` | `WorldBootstrap` 和 `WorldGenerationPipeline` | 一次生成请求 | `confirmed` 为请求模型；`EnsureSupportedRules` 只支持受限规则 |
| `WorldGenerationPipeline.GenerateCore` | `dome\src\Terraria.Dome.Simulation\WorldGeneration\WorldGenerationPipeline.cs:9-425` | `WorldBootstrap:133-139`、trace 调用者 | 受限生成一次性执行 | `partial`：有 executor，但不是 Version4 全量 Pass 等价实现 |
| `WorldGenerationStateComponent` | `...\WorldGeneration\Components\WorldGenerationStateComponent.cs:3-61` | Pipeline、Pass、commit、validation | Created 到 Validated；序列号递增 | `confirmed`：有阶段和 sequence；取消/失败持久化边界仍有限 |
| `WorldGenerationPassState`、`WorldGenerationRuntimeState` | `...\WorldGeneration\WorldGenerationPassState.cs:8-74`；`WorldGenerationRuntimeState.cs:5-53` | 生成 Pass/随机/command/checkpoint | 当前 Pass、cursor、random、command、failure | `partial`：骨架存在；完整恢复和所有 Pass 接入未证实 |
| `WorldGenerationCheckpoint` | `...\WorldGeneration\Components\WorldGenerationCheckpoint.cs:6-42` | 液体会话/生成恢复调用者 | snapshot 与状态/cursor/runtime 一致性 | `confirmed` 为检查点契约；不等于全量 WorldFile recovery |
| `WorldGenerationSystemOrder` | `...\WorldGeneration\Systems\WorldGenerationSystemOrder.cs:4-45` | Pipeline/调度读取 | 生成阶段固定顺序 | `confirmed` 为局部顺序；必须与跨域最终调度合并 |
| `TileChangeCommitSystem` | `...\World\Systems\TileChangeCommitSystem.cs:8-222` | Pipeline、Legacy commit boundary | command 排序、sequence/bounds/frame、`WorldGrid.TrySetTile` | `confirmed` 为局部 Tile authority；是否是所有 NLTX writer 的唯一入口 `blocking-decision` |
| `LiquidChangeCommitSystem` | `...\World\Systems\LiquidChangeCommitSystem.cs:7-82` 附近 | Liquid pipeline/session | type/amount/sequence/section version、`WorldGrid.TrySetLiquid` | `confirmed` 为局部提交；不等于运行期 Liquid solver authority |
| `LiquidPropagationSystem`、`LiquidPropagationSession` | `...\Liquid\Systems\LiquidPropagationSystem.cs:13-170`；`WorldGeneration\LiquidPropagationSession.cs:9-157` | Pipeline | 生成期 work item、merge、command、checkpoint/restore | `partial`：有限生成期传播已存在，完整运行期语义未证实 |
| `WorldGenerationHousingState` 与 Housing query/policy 文件 | `...\WorldGeneration\WorldGenerationHousingState.cs:3-25` 及同目录 `Housing*Query/Policy/Decision` | 当前可见为规则/查询调用者 | 房间扫描值模型和资格判断 | `partial`：未发现完整住房扫描执行系统接入 `GenerateCore` |
| `WorldGenerationLifecycleQuery` | `...\WorldGeneration\WorldGenerationLifecycleQuery.cs:3-11` | legacy flag 转换者 | 从旧 flag 生成 lifecycle snapshot | `confirmed` 为纯转换；不是 ready barrier |
| `WorldBootstrap`、server ready、persistence | `dome\src\Terraria.Dome.Server\Startup\WorldBootstrap.cs:20-139`；`ServerHostState.cs:56-106`；`DomeServer.cs:212,310,779`；`Persistence\WorldSaveCoordinator.cs:7-101` | server host、bootstrap、snapshot/恢复 | Create/Restore/Load、host ready、保存恢复 | `partial`：server ready 与 world ready 尚未建立同一屏障 |
| `TileMapStore` | `src\WorldStorage\TileMapStore.cs:3-8` | 当前可见仅有 Tile 数组/layout 字段和查询属性 | storage 初始化/布局 | `missing` 作为全 NLTX 唯一写入口证据；不能据此宣布 WorldStorage authority |

## 9. 权威状态所有权判定

| 状态 | Version4 事实 owner | 当前 NLTX 观察 | proposed owner / 交接 | 读者 | 写者 | 决策状态 |
| --- | --- | --- | --- | --- | --- | --- |
| 生成生命周期、阶段、失败 | `WorldGen` flags + `WorldGenerator.Controller` | `WorldSession` lifecycle + Dome generation state | `WorldGenerationStateComponent` 与 `ReadyTransition`（`status: proposed`）；跨域发布由 `WorldSession`/integration review 决定 | Bootstrap、update gate、server、persistence | Preparation、Pass、validation、failure/recovery | `partial` |
| Seed、seed variant、规则、Pass 版本 | `Reset`、`WorldGenerationOptions`、GenVars | `WorldGenerationRequest`、`WorldSeedComponent`、random snapshot | `GenerationPlan`、`PassState`、`DeterministicRandomPort`（均 `status: proposed`） | Pass、manifest、replay verifier | Preparation/plan | `partial` |
| Tile/Wall/TileEntity | `Main.tile` 和 WorldGen/Liquid/TileEntity | 局部 `WorldGrid` + `TileChangeCommitSystem` | `TileCommitPort`（`status: proposed`）连接最终 owner；候选 `WorldStorage` 或 integration commit | 生成、液体、交互、Progression、存档 | WorldStorage、WorldInteraction、Liquid、Progression、生成 | `blocking-decision` |
| 生成期液体 | Tile 液体字段 + `Liquid` work set | `LiquidPropagationSession` + Liquid commit | `LiquidHandoffSystem`（`status: proposed`）产生交接，运行期 authority 留给 LiquidSimulation | Liquid、validation、ready | Liquid source/propagation、运行期 solver | `partial` |
| Biome/感染/生态权威事实 | GenVars、Tile/Wall 和变换逻辑 | 多个 world generation policy/query；尚未证实统一 authority | `BiomeEcologyState`（`status: proposed`）保存事实，`BiomeQuery`/`SceneMetricsProjection`（`status: proposed`）只读派生 | 生态更新、NPC、场景投影、存档 | BiomeEcologySystem、Progression conversion command | `blocking-decision` |
| 住房房间资格和 assignment | WorldGen room scan + Town/NPC fields；加载时再次检查 | Housing state/rule/query 模型存在，执行接入未证实 | `HousingQuery` + `HousingScanSystem`（`status: proposed`）；最终 NPC/Town assignment owner 留 `integration-review` | Town/NPC、WorldFile/恢复、ready | Housing scan、Town assignment、玩家/事件变化 | `partial` |
| 生成完成 Manifest/hash | `WorldGenerator` + `WorldFile` | Trace/checkpoint/persistence snapshot 骨架 | `GenerationManifestProjection`（`status: proposed`）、`PersistenceAdapter`（`status: proposed`） | 存档、诊断、replay verifier | Pass runner/persistence adapter | `partial` |
| Ready | Version4 由生成 callback、液体处理、加载恢复和 server 流程共同暗示，未见单一字段 | server host `IsReady` 已有，但不是 world barrier | `ReadyTransition`（`status: proposed`），最终 owner `integration-review` | WorldSession、server、网络、普通 Tick gate | 生成/液体/住房/持久化 barrier | `blocking-decision` |
| Hardmode/长期转换 | `WorldProgressionAndTransition` 相邻边界：`StartHardmode`/后台转换 | 当前 NLTX 没有可据此宣布已接入本子系统的完整 owner | `WorldProgressionAndTransition` 或明确的 transition adapter（`status: proposed`）；不进入普通生成 Tick | Progression、WorldFile、网络、生态 | Progression transaction/adapter | `blocking-decision` |

## 10. proposed ECS 拆分

以下是设计候选，不是当前 NLTX 实现。每个项目均显式标记 `status: proposed`。

### 10.1 proposed Component / 状态模块

| proposed 类型 | suggested path（`status: proposed`） | 内聚状态 | 不应包含 | owner / 关系 |
| --- | --- | --- | --- | --- |
| `GenerationPlan` | `dome/src/Terraria.Dome.Simulation/WorldGeneration/Plan/GenerationPlan.cs`（`status: proposed`） | GenerationId、world metadata、seed/variant、rule snapshot、random stream version、按顺序的 Pass descriptor、weight、enabled/disabled、plan version | Tile 数组、I/O writer、UI progress、运行期 Liquid work set | WorldGeneration 独占；引用 `WorldSectionId` 等共享类型时 `crossSubsystemOwner: integration-review` |
| `PassState` | `.../WorldGeneration/State/PassState.cs`（`status: proposed`） | PassId、pass version、cursor、random snapshot、budget、pause/abort、failure、command batch reference、checkpoint id | 其他 Pass 的 mutable state、全量世界快照、网络连接 | WorldGeneration；每个生成实例/Pass 关系，而非继承树 |
| `BiomeEcologyState` | `.../WorldGeneration/Ecology/BiomeEcologyState.cs`（`status: proposed`） | 权威 Biome tag、感染/转换 revision、surface/rock facts、spread schedule、mutation budget、必要的统计版本 | SceneMetrics 缓存、播放器音乐、渲染效果、NPC AI | WorldGeneration/生态事实；跨到 Progression 的转换输入须 `integration-review` |
| `HousingScanState` | `.../WorldGeneration/Housing/HousingScanState.cs`（`status: proposed`） | 当前扫描 cursor、候选房间、规则版本、扫描 revision、失败原因、待交接 assignment | Town NPC 实体完整状态、持久化 writer、UI 文本 | WorldGeneration 只拥有扫描过程；最终居民 assignment `crossSubsystemOwner: integration-review` |
| `GenerationRandomState` | `.../WorldGeneration/Random/GenerationRandomState.cs`（`status: proposed`） | seed、stream version、PassId、cursor、可序列化随机状态 | 全局静态 RNG、时间、网络、隐式调用计数 | WorldGeneration；与每个 `PassState` 一对一关系 |
| `ReadyTransition` | `src/WorldSession/WorldGeneration/ReadyTransition.cs`（`status: proposed`） | generation revision、liquid stable、housing complete、persistence committed、failure、publication revision | server socket 状态、渲染 ready、未经确认的 host boolean | 最终 `crossSubsystemOwner: integration-review`；候选 WorldSession 承载生命周期 |

Tile grid snapshot、Manifest 和大型 GenVars snapshot 不建议作为巨型 ECS Component；它们应作为 checkpoint/持久化值或不可变读取快照，由系统显式传递。

### 10.2 proposed System / Query / Command / Adapter / Projection

| 类型 | proposed path | 输入 | 输出/副作用 | seam、depth、leverage、locality |
| --- | --- | --- | --- | --- |
| `WorldPreparationSystem` | `.../WorldGeneration/Systems/WorldPreparationSystem.cs`（`status: proposed`） | bootstrap request、world metadata、rules、previous failure | 初始化 generation lifecycle、清理旧 session、发出 plan request；不直接写存档 | seam：WorldBootstrap → WorldGeneration；depth 中等、leverage 高、locality 高 |
| `GenerationPlanSystem` | `.../WorldGeneration/Systems/GenerationPlanSystem.cs`（`status: proposed`） | request、rules、pass registry、seed | `GenerationPlan` 和按序 Pass descriptor；记录禁用原因 | seam：版本化 Pass registry；depth 深、leverage 高、locality 高 |
| `GenerationPassSystem` | `.../WorldGeneration/Systems/GenerationPassSystem.cs`（`status: proposed`） | plan、PassState、immutable `WorldGridSnapshot`、random port | `GenerationCommand`，更新 cursor/PassState；暂停/取消可重入 | seam：Pass execution vs command commit；depth 深、leverage 高、locality 高 |
| `BiomeEcologySystem` | `.../WorldGeneration/Systems/BiomeEcologySystem.cs`（`status: proposed`） | committed terrain snapshot、Biome rules、生态 schedule | 更新 `BiomeEcologyState`，产生 conversion/Tile commands；不读取 SceneMetrics 作为 authority | seam：生态规则 vs WorldStorage；depth 深、leverage 高、locality 中高 |
| `HousingScanSystem` | `.../WorldGeneration/Systems/HousingScanSystem.cs`（`status: proposed`） | immutable Tile snapshot、`HousingQuery`、NPC home candidates | `HousingScanState`、房间结果、assignment command/event；不直接踢出 NPC | seam：查询结果 → NpcAndTown；depth 中等、leverage 中高、locality 高 |
| `LiquidHandoffSystem` | `.../WorldGeneration/Systems/LiquidHandoffSystem.cs`（`status: proposed`） | committed Tile/Liquid snapshot、generation liquid work set、LiquidSimulation port | 液体稳定/未稳定结果、handoff command、失败原因 | seam：生成期传播 → LiquidSimulation；depth 深、leverage 高、locality 中 |
| `PersistenceCommitSystem` | `.../WorldGeneration/Systems/PersistenceCommitSystem.cs`（`status: proposed`） | generation snapshot、Manifest、WorldStorage snapshot、adapter result | 原子保存结果、备份/恢复状态；不把 I/O 异常吞掉 | seam：domain snapshot → `PersistenceAdapter`；depth 深、leverage 高、locality 低但可测 |
| `ReadyTransitionSystem` | `src/WorldSession/WorldGeneration/Systems/ReadyTransitionSystem.cs`（`status: proposed`） | lifecycle、pass validation、liquid handoff、housing result、persistence result | `ReadyTransition` 状态和一次性 ready publication event | seam：所有 barrier 输入；depth 深、leverage 极高、locality 中，必须集成审查 |
| `HousingQuery` | `.../WorldGeneration/Housing/HousingQuery.cs`（`status: proposed`） | Tile snapshot、bounds、housing rules、candidate home point | room bounds、needs、blocking reason、score inputs；纯函数、无写入 | seam：可重复纯计算；depth 深、leverage 高、locality 高 |
| `BiomeQuery` | `.../WorldGeneration/Ecology/BiomeQuery.cs`（`status: proposed`） | `BiomeEcologyState`、Tile snapshot、坐标/实体上下文 | Biome/scene eligibility；不修改 state | seam：权威事实 vs 派生查询；depth 中高、leverage 高、locality 高 |
| `GenerationCommand` | `.../WorldGeneration/Commands/GenerationCommand.cs`（`status: proposed`） | coordinate/section、source Pass、sequence、expected section version、kind、payload | Tile/Wall/Liquid/Structure/Frame/Conversion mutation intent；不可隐式执行 | seam：命令可排序、验证、重放；depth 深、leverage 极高、locality 高 |
| `TileCommitPort` | `.../WorldGeneration/Ports/TileCommitPort.cs`（`status: proposed`） | 已验证 command envelope、section version、generation revision | commit result、applied/rejected/deferred commands；可记录失败和重试 | seam：核心生成与 WorldStorage/integration；depth 极深、leverage 极高、locality 由整合决定 |
| `DeterministicRandomPort` | `.../WorldGeneration/Ports/DeterministicRandomPort.cs`（`status: proposed`） | seed、stream version、PassId、cursor | next value、new cursor、snapshot/restore；不读 wall clock 或全局 RNG | seam：重跑/暂停/恢复；depth 深、leverage 高、locality 高 |
| `PersistenceAdapter` | `.../WorldGeneration/Adapters/PersistenceAdapter.cs`（`status: proposed`） | domain snapshot、Manifest、format version | save/load/validate/recover result，临时文件/备份由 adapter 管理 | seam：I/O 隔离；depth 极深、leverage 高、locality 低但必须显式 failure/retry |
| `GenerationProgressProjection` | `.../WorldGeneration/Projections/GenerationProgressProjection.cs`（`status: proposed`） | PassState、progress、trace | UI/日志/网络进度视图；只读输出 | seam：表现/协议边界；depth 浅、leverage 中、locality 高 |
| `WorldReadyProjection` | `.../WorldGeneration/Projections/WorldReadyProjection.cs`（`status: proposed`） | ReadyTransition 已提交状态 | server/client ready snapshot；禁止反向写模拟状态 | seam：WorldSession/server/client；depth 中、leverage 高、locality 高 |

### 10.3 proposed command payload 最小契约

下列只是不可直接编译的设计草图，整体 `status: proposed`：

```text
status: proposed
GenerationCommand {
  generationId
  sequence
  sourcePassId
  sourcePassVersion
  sectionId                 // crossSubsystemOwner: integration-review
  expectedSectionVersion
  coordinate                // crossSubsystemOwner: integration-review
  kind: Tile | Wall | Liquid | Frame | Structure | Conversion
  payload
}

status: proposed
TileCommitPort.Commit(
  generationRevision,
  IReadOnlyList<GenerationCommand> commands,
  CommitPolicy policy)
  -> TileCommitResult
```

命令生成系统只能读取不可变 snapshot 和自己的状态；命令提交系统负责 bounds、sequence、source、section version、冲突策略、幂等性和错误返回。Query 不修改 authority，Projection 不反向写 simulation，Adapter 不把外部 BinaryReader/BinaryWriter 类型带入核心状态。

## 11. 调用方向与 System 顺序

### 11.1 调用方向

```text
WorldBootstrap
  → WorldPreparationSystem [proposed]
  → GenerationPlanSystem [proposed]
  → GenerationPassSystem [proposed]
      → GenerationCommand [proposed]
      → TileCommitPort [proposed]
          → integration commit / WorldStorage [owner unresolved]
  → BiomeEcologySystem [proposed]
      → TileCommitPort [proposed]
  → HousingScanSystem [proposed]
      → HousingQuery [proposed]
      → NpcAndTown assignment boundary [integration-review]
  → LiquidHandoffSystem [proposed]
      → LiquidSimulation handoff
  → PersistenceCommitSystem [proposed]
      → PersistenceAdapter [proposed]
  → ReadyTransitionSystem [proposed]
      → WorldSession / server / client projections
```

反向方向应被禁止：

- `WorldReadyProjection` 不写 `WorldGenerationState`；
- `SceneMetricsProjection` 不写 `BiomeEcologyState`；
- `PersistenceAdapter` 不直接调用生成 Pass；
- `LiquidSimulation` 不绕过约定的 Tile commit boundary 修改生成期 authoritative snapshot；
- `WorldProgressionAndTransition` 不以普通生成 Tick 的隐式副作用改变 `GenerationPlan`。

### 11.2 对现有局部 Pipeline 的显式顺序

当前 `WorldGenerationSystemOrder` 已声明以下局部序列：`dome/src/Terraria.Dome.Simulation/WorldGeneration/Systems/WorldGenerationSystemOrder.cs:4-45`。

```text
TerrainBase
→ CaveCarving
→ BiomeSurface
→ OrePlacement
→ StructurePlacement
→ TreePlacement
→ LiquidSource
→ LiquidPropagation
→ TileFrame
→ TileChangeCommit
→ Validation
```

这只能作为当前 Dome 局部证据。完整设计应将跨域顺序写成调度约束，而不是依赖文件或目录顺序：

1. `WorldPreparationSystem`（`status: proposed`）先于任何 Pass，完成旧世界清理、generation revision 和生命周期进入 Generating。
2. `GenerationPlanSystem`（`status: proposed`）先于 Pass，完成 seed、random stream version、Pass registry、禁用规则和权重。
3. `GenerationPassSystem`（`status: proposed`）按 plan 执行；每个 Pass 先读 snapshot，再发 command，再由 commit boundary 提交。
4. `BiomeEcologySystem`（`status: proposed`）只能在所需 Terrain/Cave/Structure 事实已提交后运行；Biome 查询不能抢先读未提交 Tile。
5. `HousingScanSystem`（`status: proposed`）必须在结构放置、frame 和必要墙体提交后运行，扫描结果再交给 NPC/Town owner。
6. `LiquidHandoffSystem`（`status: proposed`）必须在液体源和相关 Tile 提交后运行；未稳定时不得进入 ready。
7. `PersistenceCommitSystem`（`status: proposed`）必须在 generation validation、液体 handoff、住房结果的输入版本确定后运行；失败必须阻止 ready。
8. `ReadyTransitionSystem`（`status: proposed`）只能在最终选定的 barrier 全部成功、revision 一致且 publication 幂等时发布 ready。

Hardmode 的顺序是独立图：`StartHardmode` 请求 → Progression owner 校验 → 后台转换事务 → Tile/存档 commit → 主线程网络/公告 follow-up。它不应成为上述生成序列的第九步。

## 12. 持久化、网络与客户端投影边界

### 12.1 持久化

Version4 `WorldFile.LoadWorld`、`SaveWorld`、`InternalSaveWorld` 和 `SaveWorldHeader/SaveWorldTiles` 证明生成结果、Manifest、WorldFile metadata、Tile/液体、住房恢复和备份重试存在同一存档生命周期，但这不意味着它们都应属于生成 Component。

建议的 `PersistenceAdapter`（`status: proposed`）输入是版本化 domain snapshot：`WorldDescriptorState`、WorldRules、`BiomeEcologyState`、validated Tile/Liquid snapshot、住房交接结果和 Manifest。Adapter 内部才处理 BinaryReader/BinaryWriter、临时文件、备份、校验、I/O lock、重试和 recovery。保存成功必须返回明确的 `commitRevision`，而不是仅返回 boolean。

`WorldGenerationCheckpoint`（当前 NLTX 已有）适合生成暂停/恢复；它不应自动等价于正式 WorldFile。正式存档需要独立的 format version、迁移策略、校验失败策略和旧版本兼容策略。完整参考 `WorldGenSnapshot` 的 GenVars/Tile snapshot 可作为证据，但不能绕过最终 `WorldStorage` owner 决策。

### 12.2 网络

权威 server 应只发布已提交的 generation revision、WorldFile metadata、Manifest 摘要、Tile/section snapshot 和 ready 状态。客户端应通过 `WorldReadyProjection`（`status: proposed`）和相应的 tile/world data projection 获取状态，不能将客户端 `NetReceive` 结果写回生成 authority。

Version4/tModLoader 公开边界表明网络世界数据和 `ModSystem.NetReceive/NetSend` 是单独生命周期。`PlaceTile` 的公开说明还要求运行期变更通过 tile-square 同步；因此把 command commit、network notification 和本地 Tile mutation 混成一个 Query 会破坏失败和重试可见性。

### 12.3 客户端和 UI

生成进度、Pass 名称、暂停、Abort、失败原因和 ready 状态都应由 Projection 输出。当前 Version4 `GenerationProgress`/Controller 的行为可以作为兼容观察点，但 UI 不应拥有 Pass 状态，WorldGen UI 也不是一级子系统。日志、指标、hash 和 trace 也是 projection/diagnostic adapter，不是 authoritative Component。

## 13. 当前 NLTX 映射与差距

| 责任 | 当前 NLTX 对应 | 已确认能力 | 尚未闭合的差距 | 状态 |
| --- | --- | --- | --- | --- |
| 世界生成请求/元数据 | `WorldGenerationRequest.cs:7-307`、`WorldBootstrap.cs:133-139` | metadata、rules、seed variant、random stream version、terrain/biome profile 可进入 pipeline | 规则被 `EnsureSupportedRules` 限制，不能说明全量 special seed/Version4 配置可用 | `partial` |
| 受限 Pass 执行 | `WorldGenerationPipeline.cs:9-425`、多个 `Systems/*` 和 `Legacy*` | 真实存在 Terrain/Cave/Biome/Ore/Structure/Tree/Liquid/Frame/Validation 受限链 | 未证明 Version4 全量 Pass 集合、Pass 依赖、随机消费和 Legacy side effect 全部覆盖 | `partial` |
| 阶段/sequence | `WorldGenerationStateComponent.cs:3-61`、`WorldGenerationStage.cs` | GenerationId、Stage、NextSequence、Validated 完成态 | 失败/取消/重试和跨系统 transaction semantics 仍需设计 | `partial` |
| Pass state/checkpoint | `WorldGenerationPassState.cs:8-47`、`Components/WorldGenerationCheckpoint.cs:6-42`、`WorldGenerationRuntimeState.cs` | snapshot、random、cursor、commands、runtime 一致性骨架 | 未证明所有 Pass 都可暂停恢复，正式 WorldFile recovery 也不同 | `partial` |
| Tile commit | `World/Systems/TileChangeCommitSystem.cs:8-222`、Legacy commit boundary | command 校验、排序、bounds/sequence、`WorldGrid.TrySetTile`，也处理 frame | `WorldStorage` 是否全局唯一 writer 未证实；其他 `WorldGrid`/interaction/progression writer 需盘点 | `blocking-decision` |
| 液体生成传播 | `WorldGeneration/LiquidPropagationSession.cs:9-157`、`Liquid/Systems/LiquidPropagationSystem.cs:13-170`、Liquid commit | 生成期 work item、merge、预算、checkpoint/restore | 不是 Version4 `Liquid.UpdateLiquid` 全运行期 solver，ready handoff 未统一 | `partial` |
| Biome/生态 | `BiomeSurfaceSystem`、大量 `Legacy*` policy/query、`WorldEcologyScheduleState` | 有局部 surface 和 conversion/selection 规则骨架 | 未证明一个权威 `BiomeEcologyState`、所有生态写者和 SceneMetrics 分层 | `partial` |
| 住房 | `WorldGenerationHousingState.cs:3-25`、`Housing*Query/Policy/Decision`、`src/WorldSession` registry | 房间规则和值模型广泛存在 | 未发现完整住房扫描 executor 接入 `GenerateCore`，也未确定 Town/NPC assignment owner | `partial` |
| ready | `WorldSession` lifecycle；`ServerHostState.MarkReady`、`DomeServer.IsReady` | host ready 和 legacy lifecycle conversion 存在 | host ready 不等于生成、液体、住房、持久化均成功 | `blocking-decision` |
| 存档 | `WorldBootstrap`、`dome/src/Terraria.Dome.Server/Persistence/*` | Dome world/state persistence、Create/Restore/Load 骨架 | 不能等同 Version4 `WorldFile` 格式、header、Manifest 和 recovery 兼容 | `partial` |
| Hardmode | 当前生成目录有若干 Hardmode policy/状态候选，但未证明跨域完成 owner | 可在设计上保持独立 | 不应塞入普通 generation Tick；最终 Progression/adapter owner 待裁决 | `blocking-decision` |

结论：当前 NLTX 已有“受限 executor + 局部提交”的可用设计素材，但尚未形成可以声称 Version4 全量行为等价的闭合子系统。

## 14. focused verifier 设计（本轮不执行）

以下是建议的 focused verifier 设计，所有新脚本/测试路径均为 `status: proposed`，不代表文件已经创建。

| verifier | proposed 位置 | 核心断言 | 失败时必须暴露 | 状态 |
| --- | --- | --- | --- | --- |
| `GenerationDeterminismVerifier` | `Test/WorldGeneration/GenerationDeterminismVerifier.cs`（`status: proposed`） | 相同 metadata/rules/seed/stream version 的两次生成产生相同 Pass trace、command sequence、final snapshot/hash | 首个不同 Pass、随机 cursor、command sequence 或 Tile 差异 | `not-run` |
| `PassOrderAndDependencyVerifier` | `Test/WorldGeneration/PassOrderAndDependencyVerifier.cs`（`status: proposed`） | Terrain→Cave→Biome→Ore→Structure→Tree→Liquid→Frame→Commit→Validation 的显式前置关系；禁用 Pass 不改变非法依赖 | 读取未提交 snapshot、Pass 顺序漂移、禁用后隐式写者 | `not-run` |
| `TileCommitAuthorityVerifier` | `Test/WorldGeneration/TileCommitAuthorityVerifier.cs`（`status: proposed`） | 所有生成 Tile/Wall/frame command 经过唯一选定的 commit seam；重复 sequence、越界、旧 section version 被拒绝 | 绕过 commit 的 writer、重复应用、部分提交和 rollback 语义 | `not-run` |
| `LiquidBudgetCheckpointVerifier` | `Test/WorldGeneration/LiquidBudgetCheckpointVerifier.cs`（`status: proposed`） | 不同 budget 切片、暂停/恢复和 checkpoint/restore 产生相同稳定结果；pending work 清空才可 handoff | 液体 merge 不确定、恢复后重复 command、稳定条件缺失 | `not-run` |
| `HousingQueryVerifier` | `Test/WorldGeneration/HousingQueryVerifier.cs`（`status: proposed`） | 房间边界、墙、门、椅子、桌子、光源、阻挡和 occupied 条件为纯、确定、边界安全；加载恢复与生成扫描使用同一规则版本 | Query 写 state、房间遍历越界、Town assignment 与资格查询混合 | `not-run` |
| `ReadyBarrierVerifier` | `Test/WorldGeneration/ReadyBarrierVerifier.cs`（`status: proposed`） | 生成、validation、液体稳定、住房完成、persistence commit 的所有排列中，任一失败都不能发布 ready；重复 publication 幂等 | host ready 提前发布、revision 不一致、失败后仍可 update simulation | `not-run` |
| `PersistenceRecoveryVerifier` | `Test/WorldGeneration/PersistenceRecoveryVerifier.cs`（`status: proposed`） | 临时文件、校验失败、备份恢复、重复保存和 generation checkpoint 恢复不破坏 authority | 半写文件、旧 Manifest、I/O 异常吞掉、恢复后状态漂移 | `not-run` |
| `HardmodeIsolationVerifier` | `Test/WorldGeneration/HardmodeIsolationVerifier.cs`（`status: proposed`） | Hardmode transition 不会作为普通 generation Pass/Tick 运行；保存锁、后台工作、主线程 follow-up 和网络 publication 顺序显式 | generation state 被 Hardmode 写入、长事务被重复启动、网络提前同步 | `not-run` |

建议未来增加一个非编译的证据扫描器（`Build/Tools/Verify-WorldGenerationBoundaries.ps1`，`status: proposed`），只检查：生产代码中的 Tile direct write 数量和调用者、生成 Pass 注册/执行覆盖、ready publication 调用者、WorldFile/Manifest 字段交接、Hardmode 调度入口。它不能替代运行时 verifier，也不能凭文本匹配宣布行为等价。

## 15. 本次验证状态

```text
verificationStatus: not-run
```

本轮只执行了只读文件读取、符号/行号检索、目标路径存在性检查和工作树状态检查。没有运行：

- `dotnet restore`、`dotnet build`、`dotnet test`、`dotnet run`、`dotnet msbuild` 或其他 compile-capable 命令；
- focused verifier、测试、脚本生成器或源代码生成器；
- 任何会写入 `Build/bin`、`Build/obj`、测试结果或源目录的命令。

因此本报告不提供编译、测试、运行时行为、性能或 API 兼容证据。源码存在、设计完整和历史材料都不能改写本轮的 `not-run` 状态。

## 16. 不拆分项

| 对象 | 不提升为一级子系统的理由 |
| --- | --- |
| 单个 Biome | 是 `BiomeEcologyState` 的一种规则/配置或 Query 实例；生命周期和写入边界由生态系统统一管理，单个 Biome 不拥有 ready、存档或 Tile commit |
| 单个生成 Pass | 是 `GenerationPlan` 中的策略/执行实例；只有 Pass 之间的状态、顺序、随机和 command seam 才构成系统边界 |
| `SceneMetrics` 缓存 | 是派生扫描结果，具有明确失效条件；不能成为 Biome authority，也不应拥有生成生命周期 |
| 单个 Tile | 是 WorldGrid/WorldStorage 的数据实例；独立拆成 ECS 子系统会把坐标、版本、frame、液体和结构不变量分散 |
| 单个随机数 | 是 `DeterministicRandomPort`/`PassState` 的一次值或 cursor；没有独立生命周期，不能作为 Component owner |
| 世界生成 UI | 是 `GenerationProgressProjection` 的表现投影；不能写 Pass、pause、abort 或 ready authority |

## 17. 兼容策略与行为保持风险

### 17.1 兼容策略

1. 在没有完整 V4 Pass 执行证据前，保留旧入口的 facade/adapter 形状；内部逐步将直接写入转换为 command，不先删除 `WorldGen`/`WorldFile` 兼容入口。
2. 保留 seed 文本、seed variant、generator version、world unique ID、WorldId、尺寸、surface/rock layer、spawn、Dungeon 和 special seed 的明确版本化字段。
3. 保持 Pass 的显式顺序、启用/禁用规则、每 Pass 随机初始化/消费策略和 checkpoint cursor；不能用“按文件顺序执行”替代调度契约。
4. `PlaceTile`/`KillTile`/`Convert` 的返回值、实际 Tile 检查、frame、TileEntity、掉落、Wiring、网络和 liquid side effect 必须在 adapter/command contract 中逐项保留或明确改变；tModLoader 文档明确指出 `PlaceTile` 返回值不完全可靠，不能只相信 boolean。
5. 生成成功、液体稳定、住房恢复和保存成功的 callback 顺序要保留为可验证事件；加载恢复后的住房重扫不能因为生成期已有扫描而被删除。
6. WorldFile 格式采用显式 format version 和迁移 adapter；Version4 Manifest、Header、Tile/Liquid 和 backup/retry 先以 golden fixture 记录，再做字段级迁移。
7. Hardmode 继续由 `WorldProgressionAndTransition` 负责；生成系统只接收明确的 progression/transition command 或最终结果，不持有后台转换线程和 `WorldFile.IOLock`。

### 17.2 主要行为风险

- 把 `Main.tile` 直接写入改为延迟 command 可能改变同一 Pass 内的读后写可见性、frame 触发时机、Liquid 合并和结构保护。
- 把 `BiomeTileCheck` 或 SceneMetrics 结果提升为 authority 可能造成查询缓存失效、加载后重算与生成期事实不一致。
- 将多个 Legacy Pass 合并成一个“Biome/生态 System”可能改变随机消费、边界扫描、失败重试和结构保护范围。
- 将 `Liquid.UpdateLiquid` 和生成期 `LiquidPropagationSession` 合并会混淆 budget、玩家输入、网络发布、quick settle 和 ready 条件。
- 把住房扫描结果直接写入 Town/NPC 状态可能绕过 Version4 加载后的 `StartRoomCheck`、`TownManager.HasRoom`、KickOut/homeless 交接。
- 把 server host `IsReady` 当作 world ready 会让尚未持久化或液体未稳定的世界对外开放。
- 以完整参考补空实现但未确认目标 Version4 构建差异，会产生错误的 API/行为兼容假设。
- 让生成、Liquid、Progression 或 WorldInteraction 各自拥有 Tile 写入口，会使 sequence、section version、rollback、network sync 和 save snapshot 失去单一不变量。

## 18. evidence-gap 与 blocking-decision

### 18.1 普通 evidence-gap

- Version4 `WorldGenerator.RunPass`、`GenPass` 部分契约、`AddGenerationPass` 和 `CreateLiquidMergeTile` 等成员被裁剪为空，无法仅凭目标源码确认完整执行语义。
- 完整参考只能对 Version4 已存在成员补证，不能补入 Version4 主覆盖基线，也不能证明当前 NLTX 已实现完整行为。
- 当前 NLTX 有大量生成 policy/query/legacy 文件和受限 pipeline，但未证明全量 Pass 已通过同一 executor、同一提交 authority 和同一 ready barrier。
- `WorldGenerationHousingState` 及 Housing query/policy 模型存在，但尚未证实一个完整的住房扫描 executor 在生成 pipeline 和加载恢复中复用同一规则。
- `dome` persistence 能力存在，但 `WorldFile.V319`/Dome format 与 Version4 WorldFile header/Tile/Manifest/API 的兼容关系未证实。
- 全仓所有 Tile/TileEntity/Liquid/Wall writer 的完整静态盘点尚未完成；当前 `TileMapStore` 证据不足以证明它是唯一控制写入口。
- tModLoader 页面是公开边界交叉验证，不提供 Version4 私有调用顺序、NLTX API 兼容或行为等价证明。
- SS14 仅提供结构模式，没有 Terraria 世界生成、液体、住房或 Hardmode 的直接对应证据。

### 18.2 blocking-decision A：Tile commit authority

**问题**：`WorldStorage` 是否是所有最终 Tile/Wall/TileEntity 写入的唯一入口，还是生成阶段可以拥有独立 commit seam？

| 选项 | 解释 | 影响 |
| --- | --- | --- |
| A | `WorldStorage` 是唯一最终 Tile/TileEntity 写入口；WorldGeneration、LiquidSimulation、WorldInteraction 和 Progression 只能提交统一 envelope | 依赖方向最集中；需要让所有现有 writer 迁移到 Storage API，改造面最大但 authority 最清晰 |
| B | 生成阶段保留专用 `TileCommitPort`，最终由 WorldStorage 落盘；运行期 writer 使用另一个明确 integration seam | 便于生成 checkpoint/批处理；需要定义生成 commit 与运行期 commit 的一致性、revision 和合并规则 |
| C | WorldGeneration 只产生 command，多个域经统一 integration commit 编排器提交 | 最能隔离生成算法和存储，但需要最终整合会话拥有统一 command envelope、冲突、排序和失败事务语义 |

本报告不裁决 A/B/C。当前 NLTX 的 `TileChangeCommitSystem` 证明有局部 authority，但不足以证明全域唯一 authority。

### 18.3 blocking-decision B：Ready transition barrier

**问题**：哪些成功条件共同决定世界可以从 Generating/Loading 进入 Ready？

| 选项 | 解释 | 影响 |
| --- | --- | --- |
| A | 生成 Pass 和基本 validation 完成即可 Ready | 启动最快；液体、住房和持久化必须在 Ready 后继续，容易暴露半完成世界 |
| B | 生成完成 + 液体稳定 + 住房扫描完成后 Ready | 语义更接近可游玩世界；需要把 LiquidSimulation、NpcAndTown 和 WorldSession 纳入 barrier |
| C | 生成、液体、住房、持久化全部提交成功后 Ready | 对外语义最强、可恢复性最好；启动 latency、I/O failure 和重试策略最复杂 |

本报告建议至少把 B/C 的差异显式化，但不替最终整合会话选择。

### 18.4 blocking-decision C：Hardmode owner

**问题**：如何保留 Version4 `StartHardmode` 的长期事务和生成边界？

| 选项 | 解释 | 影响 |
| --- | --- | --- |
| A | 全部归 `WorldProgressionAndTransition`；WorldGeneration 只提供生成期数据和查询 | owner 清楚，生成生命周期最干净；Progression 需要接管转换、保存锁、网络和生态输入 |
| B | WorldGeneration 只产生结构/生态转换 command，Progression 承担事务提交 | command 可重放和验证；需要定义 Hardmode command 与普通 generation command 的不同生命周期 |
| C | 长期后台转换由独立 Persistence/Transition adapter 编排，Progression 持有业务状态 | 适合长事务和 I/O/线程隔离；容易形成新的隐式总控，必须定义回调、锁和失败恢复 owner |

三者都不能把 `StartHardmode` 作为普通生成 Tick；最终 owner 留待整合。

### 18.5 blocking-decision D：共享类型 owner

以下类型会被两个或以上子系统使用，只能提出候选，不能由本报告宣布最终 owner：

| shared type（`status: proposed`） | candidate owner | consumers / writers | cross-subsystem owner | 未决影响 |
| --- | --- | --- | --- | --- |
| `WorldSectionId` | WorldStorage 或 integration protocol | WorldGeneration、WorldStorage、LiquidSimulation、网络 section | `crossSubsystemOwner: integration-review` | 决定 section version、锁、命令排序和快照粒度 |
| `TileCoordinate` | WorldStorage/Spatial 或 integration value package | 生成、液体、住房、交互、网络 | `crossSubsystemOwner: integration-review` | 决定边界检查、坐标系、负值/世界外语义 |
| `EntityReference` | ECS integration | Housing、NPC、WorldSession、网络 | `crossSubsystemOwner: integration-review` | 决定运行时 EntityId 与持久化 ID 是否可混用 |
| `PersistentEntityId` | Persistence/Entity integration | WorldFile、NPC/Town、恢复、网络映射 | `crossSubsystemOwner: integration-review` | 决定加载恢复和迁移关联 |
| `NetworkId` | Transport/integration | server ready、Tile sync、NPC、客户端 projection | `crossSubsystemOwner: integration-review` | 防止协议类型渗入核心生成状态 |
| `WorldTime` | WorldSession/Time integration | 生态、Progression、事件、存档 | `crossSubsystemOwner: integration-review` | 防止生成/生态误读 wall clock 或 Tick 计数 |
| `ReadyState` | WorldSession/integration | Generation、Liquid、Housing、Persistence、server | `crossSubsystemOwner: integration-review` | 决定 host ready 与 world ready 是否同一值对象 |
| `TileCommitEnvelope` | integration commit | Generation、Liquid、WorldInteraction、Progression、WorldStorage | `crossSubsystemOwner: integration-review` | 决定 command schema、revision、retry、rollback 和 authority |

以上每一项均为 `crossSubsystemOwner: integration-review`。

## 19. Integration Handoff

```text
subsystemId: WorldGenerationAndEcology
taskNumber: 18
reportPath: D:\TRbackup\NLTX\docs\第一轮审查\2026-09-05-version4-world-generation-and-ecology-public-decomposition.md

evidenceStatus:
nltxStatus: partial
verificationStatus: not-run

confirmedOwners:
- Version4 可确认的世界生成生命周期、生成 Pass 编排、GenVars 生成态、住房扫描和生成结束液体交接
- 当前 NLTX 已存在的受限 WorldGenerationPipeline、命令提交和液体传播骨架，但不代表 Version4 全量等价

proposedTypes:
- proposed GenerationPlan
- proposed PassState
- proposed BiomeEcologyState
- proposed HousingQuery
- proposed GenerationCommand
- proposed TileCommitPort
- proposed ReadyTransition
- proposed PersistenceAdapter

sharedTypesForIntegrationReview:
- WorldSectionId
- TileCoordinate
- EntityReference
- PersistentEntityId
- NetworkId
- WorldTime
- ReadyState
- TileCommitEnvelope

crossSubsystemReaders:
- WorldStorage
- WorldProgressionAndTransition
- WorldSession
- LiquidSimulation
- SpatialSimulation
- NpcAndTownSimulation

crossSubsystemWriters:
- WorldStorage
- WorldProgressionAndTransition
- LiquidSimulation
- WorldInteractionAndStructures
- PersistenceAndRecovery

orderingConstraints:
- World reset before pass execution
- deterministic seed initialization before pass execution
- biome conversion after required terrain passes
- housing scan after structure placement
- liquid handoff before ready transition
- persistence commit and ready publication require explicit barrier
- Hardmode transition remains outside ordinary generation Tick

boundaryChallenges:
- WorldStorage 是否唯一 Tile/TileEntity 写入口
- BiomeState 与 SceneMetrics 的权威性区分
- Ready 状态是否由生成、液体稳定、住房修复和持久化共同决定
- Hardmode 长事务的最终 owner
- 当前受限 NLTX pipeline 与 Version4 全量 Pass 集合的覆盖差异

evidenceGaps:
- Version4 多个 WorldBuilding 实现被裁剪为空体
- 完整参考源码只能对 Version4 已存在成员补证
- 当前 NLTX 虽有受限 generation pipeline，但完整 Pass、住房、ready、持久化和跨域执行闭环尚未证实
- `WorldFile.cs` 任务路径与实际 `Terraria.IO/WorldFile.cs` 不一致
- 根目录 `约束/公共拆分约束.md` 与 `docs/flowstate/README.md` 当前不存在；仅 `dome/docs/flowstate/README.md` 存在
- 当前报告本轮未运行 focused verifier

blockingDecisions:
- Tile commit authority
- Ready transition barrier
- Hardmode owner
- Shared identity/coordinate type owners

notImplemented:
- 当前 NLTX 尚未证明存在 Version4 全量生成 Pass 执行链
- 尚未证明所有 Tile/TileEntity 写者都经由唯一 WorldStorage commit authority
- 尚未证明完整住房扫描、液体稳定、持久化提交和 Ready 发布形成同一屏障
- 尚未证明 Version4 WorldFile 格式/API 兼容

verifierPlan:
- 建议 GenerationDeterminismVerifier、PassOrderAndDependencyVerifier、TileCommitAuthorityVerifier、LiquidBudgetCheckpointVerifier、HousingQueryVerifier、ReadyBarrierVerifier、PersistenceRecoveryVerifier 和 HardmodeIsolationVerifier；均为 proposed 设计，不代表已经执行
```

## 20. 最终声明

本报告是基于 Version4、完整参考源码、tModLoader 公开文档和有限 ECS 结构参考形成的子系统边界与 ECS 拆分设计。它不是迁移完成报告，不是行为等价证明，不是 API 兼容证明，也不是当前 NLTX 已实现能力的声明。

本次只生成本报告，没有修改生产代码、测试代码、Version4、完整参考源码、tModLoader 文档、Space Station 14 参考源码、索引或其他子系统报告；没有启动子代理；没有运行构建或测试。
