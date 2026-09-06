# 03 — LeashedEntitySimulation 独立只读审查与 ECS 设计提示词

本文件必须与以下公共协议共同使用，开始任务前先完整读取该文件：
D:\TRbackup\NLTX\docs\第一轮审查\2026-09-05-version4-public-decomposition-common-protocol.md

公共协议中的只读、并行、证据优先级、行号复核、状态枚举、事实/设计隔离、ECS 文件组织、副作用隔离、报告结构和 Integration Handoff 规则全部适用于本任务。若本任务包与公共协议出现冲突，以公共协议为准。

本任务属于已经固定的 19 个游戏模拟子系统。不得改名、合并、拆分或扩展固定清单；如果当前全量索引出现额外候选，只能记录为 boundary-challenge，交由最终整合会话处理。

## 任务元数据

- taskNumber: 03
- subsystemId: LeashedEntitySimulation
- layer: authoritative-simulation
- currentNltxStatus: missing
- originalBoundary: 从 ProjectileSimulation 反向拆出；锚点 TileEntity 仍归 WorldStorage/WorldInteractionAndStructures
- relatedSubsystems: ProjectileSimulation, WorldStorage, NetworkSessionAndSectionStreaming, SpatialSimulation
- reportPath: D:\TRbackup\NLTX\docs\research\2026-09-05-version4-leashed-entity-simulation-public-decomposition.md

## 专属目标

围绕 LeashedEntitySimulation，基于真实 Version4 代码确定权威状态、写入根、生命周期、调用方向和跨域提交边界，并提出可实现但不落地的 ECS 拆分设计。该子系统的责任是：leashed entity registry、section 激活、生成/销毁、更新、锚点关系和网络流式同步。

“拆分为组件”只表示 proposed design。不得创建实际组件、System、Query、Command、Adapter、Projection、测试或项目文件。

## Version4 必须深读和重新定位的专属证据

- D:\TRbackup\Version4\Terraria.GameContent\LeashedEntity.cs:48 Registry.RegisterAll；:94 SectionEntityList；:198 Clear；:234 UpdateEntities；:239 _UpdateEntities；:263 section update；:18 附近 NetModule.Sync。
- D:\TRbackup\Version4\Terraria\Main.cs:3347 registry 初始化；:11572 UpdateEntities 驱动；D:\TRbackup\Version4\Terraria\WorldGen.cs:6665 清理路径。
- D:\TRbackup\Version4\Terraria.GameContent.LeashedEntities\*：约二十个注册 prototype；检查每个 prototype 是定义还是生命周期成员。
- D:\TRbackup\Version4\Terraria.GameContent.Tile_Entities\TELeashedEntityAnchor.cs 与 TELeashedEntityAnchorWithItem.cs：锚点宿主、销毁和存储关系。
- 相关 section、实体 identity、NetModule、WorldSections、RemoteClient 和 MessageBuffer 调用点。

任务包中的路径和行号只是检索线索。必须重新读取文件、定位实际符号和调用上下文；报告使用实际行号，并标记 confirmed、partial、missing、unresolved、evidence-mismatch 或 version-drift。

## tModLoader 公开 API 交叉验证

- D:\TRbackup\tmodloader-api-docs-stable\class_mod_packet.html:94、:139：客户端/服务器方向和 relay 语义。
- 按公共协议检索 ModTileEntity 宿主校验、更新、同步和删除清理页面；不声称存在公开 ModLeashedEntity API。

不得用 tModLoader 文档替代 Version4 私有行为；必须记录实际 HTML 文件、页面标题、文档版本、成员锚点和仅用于何种边界交叉验证。

## Space Station 14 最小相关 ECS 参考

只读检索下列最小相关关键词和目录：
- entity lifecycle、component、system、map/grid、spatial partition、network state、replication
- spawn、despawn、attach、anchor、relationship、serialization

只记录实际读取的 Space Station 14 文件、类型/方法和参考用途（Component 粒度、System/Query 边界、Event/Command、关系、网络或持久化）。禁止复制代码、命名、目录结构和领域语义；若无直接对应证据，明确写无直接对应证据。

## 当前 NLTX 必须浅读核对

- 当前 NLTX 无 leashed-entity registry、section 生命周期、anchor 关系或 focused verifier。
- 检查 src/Projectile、src/WorldStorage、src/Share/Entity 和 Test 中是否已有可复用身份模型，但不得创建代码。

只能读取当前 src、Test、dome/src 和既有验证材料，不得修改；本轮并行审查不运行编译/测试命令，验证状态写为 not-run，历史真实输出只能写 existing-evidence。

## public-decomposition 专属问题

- registry、prototype definition、active section list 和实体实例状态如何分离。
- section 激活/停用是否是查询结果、缓存还是权威生命周期；谁能触发生成和销毁。
- 锚点 TileEntity 只提供宿主事实还是拥有 leashed entity；如何表达实体关系和持久化 ID。
- NetModule.Sync 的输入是否为不可变快照；低层网络类型如何隔离在 Adapter。
- 从 ProjectileSimulation 拆出的边界是否保持 projectile-like 运动机制而不共享巨型组件。

必须完成成员/字段/方法盘点、读者/写者/生命周期/副作用表、权威状态所有权表、访问模式分组、Component/System/Query/Command/Adapter/Projection 接口契约、System 顺序、跨域依赖、ECS ID/关系建模、不拆分项和行为保持风险。所有新类型、路径和签名均标记 status: proposed；跨子系统类型标记 crossSubsystemOwner: integration-review。

## 专属交付重点

- 至少提出 Registry/Definition、LeashedEntityState、SectionActivation、Spawn/Despawn Command、Update System、Anchor Relation、Replication Adapter、Persistence Adapter 的 proposed 设计。
- 报告必须列出 registry 初始化、section 更新、清理、网络同步的真实顺序，以及独立 verifier 的激活/销毁/断线场景。

报告必须写入：
D:\TRbackup\NLTX\docs\research\2026-09-05-version4-leashed-entity-simulation-public-decomposition.md

报告必须包含 Version4 事实、完整参考补证、tModLoader 交叉验证、Space Station 14 参考、当前 NLTX 状态、proposed 设计、not-run 验证、证据缺口、blocking-decision 和 Integration Handoff。报告只能使用中文说明与英文稳定 ID，不能宣称迁移完成、行为等价或 API 兼容。

## 明确不拆分的专属对象

- 单个 leashed critter prototype、单个 anchor TileEntity、单个 NetModule、单个 section list、单个网络包、单个渲染对象。

请说明每个对象为什么只是实例、策略、规则、查询、缓存、Adapter 或 Projection，而不是一级子系统。

## 完成条件

完成只读证据审查并生成唯一指定报告后，在最终回复中简要说明报告路径、实际读取的主要证据、本次未运行构建/测试、evidence-gap、blocking-decision 以及未修改生产代码。
