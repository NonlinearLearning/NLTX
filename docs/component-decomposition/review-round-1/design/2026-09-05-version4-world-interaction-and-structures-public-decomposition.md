# 10 — WorldInteractionAndStructures 独立只读审查与 ECS 设计提示词

本文件必须与以下公共协议共同使用，开始任务前先完整读取该文件：
D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\design\2026-09-05-version4-public-decomposition-common-protocol.md

公共协议中的只读、并行、证据优先级、行号复核、状态枚举、事实/设计隔离、ECS 文件组织、副作用隔离、报告结构和 Integration Handoff 规则全部适用于本任务。若本任务包与公共协议出现冲突，以公共协议为准。

本任务属于已经固定的 19 个游戏模拟子系统。不得改名、合并、拆分或扩展固定清单；如果当前全量索引出现额外候选，只能记录为 boundary-challenge，交由最终整合会话处理。

## 任务元数据

- taskNumber: 10
- subsystemId: WorldInteractionAndStructures
- layer: authoritative-simulation
- currentNltxStatus: partial
- originalBoundary: 初始世界交互责任面；线路机制若在索引中另列，只记录边界挑战
- relatedSubsystems: WorldStorage, IntentAndInteraction, SpatialSimulation, SpawnLifecycleAndLoot, NetworkSessionAndSectionStreaming
- reportPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-05-version4-world-interaction-and-structures-public-decomposition.md

## 专属目标

围绕 WorldInteractionAndStructures，基于真实 Version4 代码确定权威状态、写入根、生命周期、调用方向和跨域提交边界，并提出可实现但不落地的 ECS 拆分设计。该子系统的责任是：已验证的放置、破坏、线路、机关和结构变更命令，并向 WorldStorage 提交原子结果。

“拆分为组件”只表示 proposed design。不得创建实际组件、System、Query、Command、Adapter、Projection、测试或项目文件。

## Version4 必须深读和重新定位的专属证据

- D:\TRbackup\Version4\Terraria\Wiring.cs：HitSwitch（约 :267）、Actuate（:395）、CheckMech（:464）、XferWater、CheckLogicGate 及触发传播调用。
- D:\TRbackup\Version4\Terraria\WorldGen.cs：PlaceTile/KillTile/PlaceLiquid 等结构写入路径；Collision.cs:2549-2588 触发开关。
- D:\TRbackup\Version4\Terraria\TileEntity.cs、GameContent.Tile_Entities\*、Chest.cs、Tile.cs：结构宿主、容器和 Tile 写集。
- D:\TRbackup\Version4\Terraria\MessageBuffer.cs:903-910 等入站机关/结构命令；WorldFile.cs 保存/加载结构。
- 完整参考 D:\TRbackup\无任何删减通过编译\Terraria\Wiring.cs：仅在 Version4 同签名删减可闭合时补证。

任务包中的路径和行号只是检索线索。必须重新读取文件、定位实际符号和调用上下文；报告使用实际行号，并标记 confirmed、partial、missing、unresolved、evidence-mismatch 或 version-drift。

## tModLoader 公开 API 交叉验证

- D:\TRbackup\tmodloader-api-docs-stable\class_mod_tile_entity.html:505-540：放置、更新、同步和删除清理边界。
- D:\TRbackup\tmodloader-api-docs-stable\class_wiring.html:113：CheckMech 限流和 HitSwitchAndSync 同步语义；实际标题/版本/锚点必须记录。

不得用 tModLoader 文档替代 Version4 私有行为；必须记录实际 HTML 文件、页面标题、文档版本、成员锚点和仅用于何种边界交叉验证。

## Space Station 14 最小相关 ECS 参考

只读检索下列最小相关关键词和目录：
- interaction、construction、tile/entity、device network、signal、wiring
- command validation、transaction、event、component/system、network state

只记录实际读取的 Space Station 14 文件、类型/方法和参考用途（Component 粒度、System/Query 边界、Event/Command、关系、网络或持久化）。禁止复制代码、命名、目录结构和领域语义；若无直接对应证据，明确写无直接对应证据。

## 当前 NLTX 必须浅读核对

- 当前 NLTX 有 src/WorldInteraction 和 WorldStorage/Pylon 等部分组件，但没有统一 intent→validation→storage commit 链。
- 如发现 WiringAndMechanisms 作为额外候选，不得改变固定 19 任务；仅记录 boundary-challenge。

只能读取当前 src、Test、dome/src 和既有验证材料，不得修改；本轮并行审查不运行编译/测试命令，验证状态写为 not-run，历史真实输出只能写 existing-evidence。

## public-decomposition 专属问题

- 外部输入、Intent 验证、结构规则 Query、Tile/TileEntity/Chest 变更 Command 和原子提交如何分层。
- 线路传播、机械冷却、逻辑门、泵与触发结果是否共享工作集；谁拥有提交写集。
- WorldStorage 是否唯一写 Tile/TileEntity，Spawn/Loot 只接收已提交结果。
- 网络入站消息和客户端 UI 如何隔离，失败、拒绝和重试如何可见。

必须完成成员/字段/方法盘点、读者/写者/生命周期/副作用表、权威状态所有权表、访问模式分组、Component/System/Query/Command/Adapter/Projection 接口契约、System 顺序、跨域依赖、ECS ID/关系建模、不拆分项和行为保持风险。所有新类型、路径和签名均标记 status: proposed；跨子系统类型标记 crossSubsystemOwner: integration-review。

## 专属交付重点

- 提出 InteractionIntent、StructureMutationRequest、WiringWorkState、ValidationQuery、StructureCommitSystem、Storage Port、Network Projection 的 proposed 设计。
- 报告必须给出放置/破坏、线路触发、TileEntity 生命周期和网络命令的 focused verifier 计划。

报告必须写入：
D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-05-version4-world-interaction-and-structures-public-decomposition.md

报告必须包含 Version4 事实、完整参考补证、tModLoader 交叉验证、Space Station 14 参考、当前 NLTX 状态、proposed 设计、not-run 验证、证据缺口、blocking-decision 和 Integration Handoff。报告只能使用中文说明与英文稳定 ID，不能宣称迁移完成、行为等价或 API 兼容。

## 明确不拆分的专属对象

- 单个压力板、单个逻辑门、单个 TileEntity、单个网络消息、单个 Wiring Hook、单个 Tile 写入。

请说明每个对象为什么只是实例、策略、规则、查询、缓存、Adapter 或 Projection，而不是一级子系统。

## 完成条件

完成只读证据审查并生成唯一指定报告后，在最终回复中简要说明报告路径、实际读取的主要证据、本次未运行构建/测试、evidence-gap、blocking-decision 以及未修改生产代码。
