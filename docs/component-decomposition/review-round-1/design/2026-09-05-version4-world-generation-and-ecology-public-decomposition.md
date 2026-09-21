# 18 — WorldGenerationAndEcology 独立只读审查与 ECS 设计提示词

本文件必须与以下公共协议共同使用，开始任务前先完整读取该文件：
D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\design\2026-09-05-version4-public-decomposition-common-protocol.md

公共协议中的只读、并行、证据优先级、行号复核、状态枚举、事实/设计隔离、ECS 文件组织、副作用隔离、报告结构和 Integration Handoff 规则全部适用于本任务。若本任务包与公共协议冲突，以公共协议为准。

本任务属于已经固定的 19 个游戏模拟子系统。不得改名、合并、拆分或扩展固定清单；如果当前全量索引出现额外候选，只能记录为 boundary-challenge，交由最终整合会话处理。

## 任务元数据

- taskNumber: 18
- subsystemId: WorldGenerationAndEcology
- layer: authoritative-simulation
- currentNltxStatus: partial
- originalBoundary: 初始世界生成/生态责任；WorldProgressionAndTransition、WorldStorage、LiquidSimulation 与其交接
- relatedSubsystems: WorldStorage, WorldProgressionAndTransition, WorldSession, LiquidSimulation, SpatialSimulation, NpcAndTownSimulation
- reportPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-05-version4-world-generation-and-ecology-public-decomposition.md

## 专属目标

围绕 WorldGenerationAndEcology，基于真实 Version4 代码确定权威状态、写入根、生命周期、调用方向和跨域提交边界，并提出可实现但不落地的 ECS 拆分设计。该子系统的责任是：世界生成计划、Biome/生态变换、住房扫描和世界 ready 状态转换。

“拆分为组件”只表示 proposed design。不得创建实际组件、System、Query、Command、Adapter、Projection、测试或项目文件。

## Version4 必须深读和重新定位的专属证据

- D:\TRbackup\Version4\Terraria\WorldGen.cs：GenerateWorld（约 :6272）、AddGenerationPass/MicroBiomes（约 :20799）、Biome/Housing/生态变换以及 PlaceTile/KillTile。
- WorldGen.cs:26084 StartHardmode 作为独立长事务边界；:59408-59415 生态传播/规则覆写读取。
- D:\TRbackup\Version4\Terraria\WorldFile.cs:716 世界生成调用、:775 液体处理、:1941 TownManager/住房加载等恢复交接。
- Terraria.GameContent.Biomes\*、Terraria.WorldBuilding\*、Liquid.cs、Tile/Collision：生成、环境和存储写集。

任务包中的路径和行号只是检索线索。必须重新读取文件、定位实际符号和调用上下文；报告使用实际行号，并标记 confirmed、partial、missing、unresolved、evidence-mismatch 或 version-drift。

## tModLoader 公开 API 交叉验证

- D:\TRbackup\tmodloader-api-docs-stable\class_mod_system.html:139-140：世界生成任务和可修改 pass。
- 按索引检索 class_mod_biome.html、世界加载/卸载、PostUpdateWorld 和 Tile/结构页面；SceneMetrics 只作派生查询参考。

不得用 tModLoader 文档替代 Version4 私有行为；必须记录实际 HTML 文件、页面标题、文档版本、成员锚点和仅用于何种边界交叉验证。

## Space Station 14 最小相关 ECS 参考

只读检索下列最小相关关键词和目录：
- map generation、procgen、biome、ecology、tile/grid、world state
- generation pass、deterministic system、query、commit barrier、serialization

只记录实际读取的 Space Station 14 文件、类型/方法和参考用途（Component 粒度、System/Query 边界、Event/Command、关系、网络或持久化）。禁止复制代码、命名、目录结构和领域语义；若无直接对应证据，明确写无直接对应证据。

## 当前 NLTX 必须浅读核对

- 当前 NLTX 有 src/WorldSession/WorldGeneration 生命周期和部分 world state，但没有 generation executor、确定性 pass、Tile commit 或 focused verifier。
- 不得把 WorldProgressionAndTransition 的 Hardmode 事务重新并入常规生成 Tick。

只能读取当前 src、Test、dome/src 和既有验证材料，不得修改；本轮并行审查不运行编译/测试命令，验证状态写为 not-run，历史真实输出只能写 existing-evidence。

## public-decomposition 专属问题

- 生成计划、pass、随机种子、生态变换、住房扫描和 ready 状态的生命周期与写者。
- WorldStorage 是唯一 Tile/TileEntity 写入口吗；LiquidSimulation、WorldInteraction 和 Progression 如何提交。
- Biome/SceneMetrics 哪些是纯 Query，哪些是权威世界事实；如何保证确定性和重跑。
- 生成失败、预算/暂停、恢复、版本迁移和网络/存档边界。
- 生成阶段与液体、NPC、事件和 Hardmode 的显式顺序约束。

必须完成成员/字段/方法盘点、读者/写者/生命周期/副作用表、权威状态所有权表、访问模式分组、Component/System/Query/Command/Adapter/Projection 接口契约、System 顺序、跨域依赖、ECS ID/关系建模、不拆分项和行为保持风险。所有新类型、路径和签名均标记 status: proposed；跨子系统类型标记 crossSubsystemOwner: integration-review。

## 专属交付重点

- 提出 GenerationPlan、PassState、Biome/EcologyState、HousingQuery、GenerationCommand、TileCommitPort、ReadyTransition、Persistence Adapter 的 proposed 设计。
- 报告必须覆盖新世界生成、重跑/失败、Hardmode 过渡交接、住房扫描、液体更新和 focused verifier。

报告必须写入：
D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-05-version4-world-generation-and-ecology-public-decomposition.md

报告必须包含 Version4 事实、完整参考补证、tModLoader 交叉验证、Space Station 14 参考、当前 NLTX 状态、proposed 设计、not-run 验证、证据缺口、blocking-decision 和 Integration Handoff。报告只能使用中文说明与英文稳定 ID，不能宣称迁移完成、行为等价或 API 兼容。

## 明确不拆分的专属对象

- 单个 Biome、单个生成 pass、SceneMetrics 缓存、单个 Tile、单个随机数、单个世界生成 UI。

请说明每个对象为什么只是实例、策略、规则、查询、缓存、Adapter 或 Projection，而不是一级子系统。

## 完成条件

完成只读证据审查并生成唯一指定报告后，在最终回复中简要说明报告路径、实际读取的主要证据、本次未运行构建/测试、evidence-gap、blocking-decision 以及未修改生产代码。
