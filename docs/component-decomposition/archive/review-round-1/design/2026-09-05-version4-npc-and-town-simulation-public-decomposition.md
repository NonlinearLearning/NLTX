# 12 — NpcAndTownSimulation 独立只读审查与 ECS 设计提示词

本文件必须与以下公共协议共同使用，开始任务前先完整读取该文件：
D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\design\2026-09-05-version4-public-decomposition-common-protocol.md

公共协议中的只读、并行、证据优先级、行号复核、状态枚举、事实/设计隔离、ECS 文件组织、副作用隔离、报告结构和 Integration Handoff 规则全部适用于本任务。若本任务包与公共协议冲突，以公共协议为准。

本任务属于已经固定的 19 个游戏模拟子系统。不得改名、合并、拆分或扩展固定清单；如果当前全量索引出现额外候选，只能记录为 boundary-challenge，交由最终整合会话处理。

## 任务元数据

- taskNumber: 12
- subsystemId: NpcAndTownSimulation
- layer: authoritative-simulation
- currentNltxStatus: partial
- originalBoundary: 初始 NPC/城镇权威责任；SpawnLifecycleAndLoot、WorldProgressionAndUnlocks、DeathPenaltyAndRevenge 与其交接
- relatedSubsystems: SpawnLifecycleAndLoot, WorldProgressionAndUnlocks, WorldCalendarAndEventOrchestration, CombatAndStatus, SpatialSimulation, DeathPenaltyAndRevenge
- reportPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-05-version4-npc-and-town-simulation-public-decomposition.md

## 专属目标

围绕 NpcAndTownSimulation，基于真实 Version4 代码确定权威状态、写入根、生命周期、调用方向和跨域提交边界，并提出可实现但不落地的 ECS 拆分设计。该子系统的责任是：NPC AI、目标关系、实体生命周期、自然/城镇生成资格、住房和城镇规则。

“拆分为组件”只表示 proposed design。不得创建实际组件、System、Query、Command、Adapter、Projection、测试或项目文件。

## Version4 必须深读和重新定位的专属证据

- D:\TRbackup\Version4\Terraria\NPC.cs：UpdateNPC（约 :76711）、NewNPC 调用、CheckDead/NPCLoot/Drop 路径、townNPC/housing 字段和 AI 状态。
- D:\TRbackup\Version4\Terraria\Main.cs:11511-11524：NPC 更新；D:\TRbackup\Version4\Terraria\WorldGen.cs:4263、:4630、:5119 等住房扫描和 TownManager。
- NPC.cs:6301、:64542、:66518：RevengeManager 初始化、死亡/缓存和重生边界，作为 DeathPenaltyAndRevenge 交接证据。
- D:\TRbackup\Version4\Terraria.IO\WorldFile.cs、D:\TRbackup\Version4\Terraria\MessageBuffer.cs：TownManager/NPC 保存、加载、网络 spawn 和状态接收。

任务包中的路径和行号只是检索线索。必须重新读取文件、定位实际符号和调用上下文；报告使用实际行号，并标记 confirmed、partial、missing、unresolved、evidence-mismatch 或 version-drift。

## tModLoader 公开 API 交叉验证

- D:\TRbackup\tmodloader-api-docs-stable\class_mod_n_p_c.html：公开 NPC 生命周期和扩展边界；必须按索引定位真实标题、版本和成员。
- 按索引检索 ModSystem/GlobalNPC 的生成、掉落、AI、住宅和网络页面；不能用 Hook 替代 NPC 真实调用链。

不得用 tModLoader 文档替代 Version4 私有行为；必须记录实际 HTML 文件、页面标题、文档版本、成员锚点和仅用于何种边界交叉验证。

## Space Station 14 最小相关 ECS 参考

只读检索下列最小相关关键词和目录：
- mob entity、AI、targeting、health、spawn/despawn、mind/session
- component state、entity system、query、event、replication、prototype

只记录实际读取的 Space Station 14 文件、类型/方法和参考用途（Component 粒度、System/Query 边界、Event/Command、关系、网络或持久化）。禁止复制代码、命名、目录结构和领域语义；若无直接对应证据，明确写无直接对应证据。

## 当前 NLTX 必须浅读核对

- 当前 NLTX 有 src/Npc、src/Town 的实体和城镇状态组件，但没有 AI、住房 Query、生成调度和完整提交链。
- 必须区分 NPC 实体状态、Town/住房查询、SpawnLifecycle、Death/Revenge 和 Loot 的 owner。

只能读取当前 src、Test、dome/src 和既有验证材料，不得修改；本轮并行审查不运行编译/测试命令，验证状态写为 not-run，历史真实输出只能写 existing-evidence。

## public-decomposition 专属问题

- NPC 实例身份、AI 行为状态、目标关系、生命周期和城镇住房状态如何拆分。
- 自然生成、城镇生成、复仇重建和网络 spawn 的资格与提交谁拥有；NPC 只读哪些 Query。
- AI style、单个 Town NPC、住房结果是机制、关系还是一级 System。
- NPC slot、PersistentEntityId、NetworkId 和玩家/marker 关系如何建模。
- 死亡、掉落、清理和重建是否存在重复写者或循环依赖。

必须完成成员/字段/方法盘点、读者/写者/生命周期/副作用表、权威状态所有权表、访问模式分组、Component/System/Query/Command/Adapter/Projection 接口契约、System 顺序、跨域依赖、ECS ID/关系建模、不拆分项和行为保持风险。所有新类型、路径和签名均标记 status: proposed；跨子系统类型标记 crossSubsystemOwner: integration-review。

## 专属交付重点

- 提出 NpcIdentity、NpcLifecycle、NpcBehavior、NpcTarget、Town/HousingRelation、SpawnEligibilityQuery、NpcUpdateSystem、NpcSpawnCommitSystem 的 proposed 设计。
- 报告必须覆盖 AI tick、住房扫描、死亡/掉落交接、重生、网络复制和 focused verifier。

报告必须写入：
D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-05-version4-npc-and-town-simulation-public-decomposition.md

报告必须包含 Version4 事实、完整参考补证、tModLoader 交叉验证、Space Station 14 参考、当前 NLTX 状态、proposed 设计、not-run 验证、证据缺口、blocking-decision 和 Integration Handoff。报告只能使用中文说明与英文稳定 ID，不能宣称迁移完成、行为等价或 API 兼容。

## 明确不拆分的专属对象

- 单个 NPC 类型、单个 AI style、单个 Town NPC、单个住房房间、单个 spawn 消息、单个掉落规则。

请说明每个对象为什么只是实例、策略、规则、查询、缓存、Adapter 或 Projection，而不是一级子系统。

## 完成条件

完成只读证据审查并生成唯一指定报告后，在最终回复中简要说明报告路径、实际读取的主要证据、本次未运行构建/测试、evidence-gap、blocking-decision 以及未修改生产代码。
