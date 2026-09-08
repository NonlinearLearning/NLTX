# 01 — LiquidSimulation 独立只读审查与 ECS 设计提示词

本文件必须与以下公共协议共同使用，开始任务前先完整读取该文件：
D:\TRbackup\NLTX\docs\第一轮审查\设计\2026-09-05-version4-public-decomposition-common-protocol.md

公共协议中的只读、并行、证据优先级、行号复核、状态枚举、事实/设计隔离、ECS 文件组织、副作用隔离、报告结构和 Integration Handoff 规则全部适用于本任务。若本任务包与公共协议出现冲突，以公共协议为准。

本任务属于已经固定的 19 个游戏模拟子系统。不得改名、合并、拆分或扩展固定清单；如果当前全量索引出现额外候选，只能记录为 boundary-challenge，交由最终整合会话处理。

## 任务元数据

- taskNumber: 01
- subsystemId: LiquidSimulation
- layer: authoritative-simulation
- currentNltxStatus: missing
- originalBoundary: 从 SpatialSimulation 反向拆出
- relatedSubsystems: SpatialSimulation, WorldStorage, WorldGenerationAndEcology, PersistenceAndRecovery, NetworkSessionAndSectionStreaming, ClientPresentationAndTools
- reportPath: D:\TRbackup\NLTX\docs\第一轮审查\2026-09-05-version4-liquid-simulation-public-decomposition.md

## 专属目标

围绕 LiquidSimulation，基于真实 Version4 代码确定权威状态、写入根、生命周期、调用方向和跨域提交边界，并提出可实现但不落地的 ECS 拆分设计。该子系统的责任是：液体数量/类型状态、排队流动、液体反应以及 Tile、网络和存档变更发布。

“拆分为组件”只表示 proposed design。不得创建实际组件、System、Query、Command、Adapter、Projection、测试或项目文件。

## Version4 必须深读和重新定位的专属证据

- D:\TRbackup\Version4\Terraria\Liquid.cs:56 附近的 NetSendLiquid；:88 附近的 ReInit；:1015 附近的 UpdateLiquid；:1190 附近的 AddWater；:1427 附近的 DelWater。
- D:\TRbackup\Version4\Terraria\LiquidBuffer.cs：液体更新队列、入队/消费/清空和去重逻辑。
- D:\TRbackup\Version4\Terraria\WorldGen.cs:15306、:20118：世界生成阶段驱动液体更新。
- D:\TRbackup\Version4\Terraria\WorldFile.cs:775：世界加载后的液体处理；D:\TRbackup\Version4\Terraria\NetMessage.cs：液体网络序列化/发布。
- D:\TRbackup\Version4\Terraria\Tile.cs 及相关 Tile 类型：液体数量、类型和跳过更新字段的真实持有者。
- D:\TRbackup\Version4\Terraria\Collision.cs、Player.cs、NPC.cs、Projectile.cs：液体接触读取者；LiquidRenderer 及 Terraria.GameContent.Liquid：客户端投影。

任务包中的路径和行号只是检索线索。必须重新读取文件、定位实际符号和调用上下文；报告使用实际行号，并标记 confirmed、partial、missing、unresolved、evidence-mismatch 或 version-drift。

## tModLoader 公开 API 交叉验证

- D:\TRbackup\tmodloader-api-docs-stable\struct_tile.html:250-274：LiquidAmount、LiquidType、SkipLiquid 的公开字段语义。
- 按公共协议从本地索引检索 Tile 网络同步、世界存档和 ModSystem 生命周期页面；只记录实际页面和锚点。

不得用 tModLoader 文档替代 Version4 私有行为；必须记录实际 HTML 文件、页面标题、文档版本、成员锚点和仅用于何种边界交叉验证。

## Space Station 14 最小相关 ECS 参考

只读检索下列最小相关关键词和目录：
- liquid、fluid、puddle、solution、reagent、tile reaction
- grid/map、entity system、component、network state、serialization

只记录实际读取的 Space Station 14 文件、类型/方法和参考用途（Component 粒度、System/Query 边界、Event/Command、关系、网络或持久化）。禁止复制代码、命名、目录结构和领域语义；若无直接对应证据，明确写无直接对应证据。

## 当前 NLTX 必须浅读核对

- 当前 NLTX 无液体求解器或 focused verifier；液体接触只在 SpatialSimulation 相关模型中出现。
- 不要把现有 LiquidComponent 或液体接触字段直接等同于 Version4 的流动权威状态。

只能读取当前 src、Test、dome/src 和既有验证材料，不得修改；本轮并行审查不运行编译/测试命令，验证状态写为 not-run，历史真实输出只能写 existing-evidence。

## public-decomposition 专属问题

- 谁拥有 Tile 液体数量/类型的最终写入；SpatialSimulation 是否只能通过纯 Query 读取。
- LiquidBuffer 应是内部工作集、独立队列组件还是 Command 输入；不得把队列误判为权威状态根。
- 流动资格计算、液体反应和 Tile 提交是否应拆为不同 System；哪些字段必须同一事务更新。
- 世界加载后的液体重建、网络不可变快照、持久化快照和 LiquidRenderer Projection 的方向。
- TileCoordinate、WorldSectionId、LiquidTypeId、NetworkId 和 EntityReference 的跨子系统 owner 只能标记 integration-review。

必须完成成员/字段/方法盘点、读者/写者/生命周期/副作用表、权威状态所有权表、访问模式分组、Component/System/Query/Command/Adapter/Projection 接口契约、System 顺序、跨域依赖、ECS ID/关系建模、不拆分项和行为保持风险。所有新类型、路径和签名均标记 status: proposed；跨子系统类型标记 crossSubsystemOwner: integration-review。

## 专属交付重点

- 至少给出 LiquidState、FlowWork/Queue、Reaction、Commit、ContactQuery、Persistence/Network Adapter、Renderer Projection 的 proposed 边界和替代方案。
- 报告必须把液体状态写集、读取者、调用顺序、失败/重试和 verifier 场景分开。

报告必须写入：
D:\TRbackup\NLTX\docs\第一轮审查\2026-09-05-version4-liquid-simulation-public-decomposition.md

报告必须包含 Version4 事实、完整参考补证、tModLoader 交叉验证、Space Station 14 参考、当前 NLTX 状态、proposed 设计、not-run 验证、证据缺口、blocking-decision 和 Integration Handoff。报告只能使用中文说明与英文稳定 ID，不能宣称迁移完成、行为等价或 API 兼容。

## 明确不拆分的专属对象

- LiquidRenderer、LiquidBuffer、单个液体类型、单个反应、单个 Tile、单个 NetModule、单个碰撞查询、单个渲染快照。

请说明每个对象为什么只是实例、策略、规则、查询、缓存、Adapter 或 Projection，而不是一级子系统。

## 完成条件

完成只读证据审查并生成唯一指定报告后，在最终回复中简要说明报告路径、实际读取的主要证据、本次未运行构建/测试、evidence-gap、blocking-decision 以及未修改生产代码。
