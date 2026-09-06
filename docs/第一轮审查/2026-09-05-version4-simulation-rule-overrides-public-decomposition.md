# 08 — SimulationRuleOverrides 独立只读审查与 ECS 设计提示词

本文件必须与以下公共协议共同使用，开始任务前先完整读取该文件：
D:\TRbackup\NLTX\docs\第一轮审查\2026-09-05-version4-public-decomposition-common-protocol.md

公共协议中的只读、并行、证据优先级、行号复核、状态枚举、事实/设计隔离、ECS 文件组织、副作用隔离、报告结构和 Integration Handoff 规则全部适用于本任务。若本任务包与公共协议出现冲突，以公共协议为准。

本任务属于已经固定的 19 个游戏模拟子系统。不得改名、合并、拆分或扩展固定清单；如果当前全量索引出现额外候选，只能记录为 boundary-challenge，交由最终整合会话处理。

## 任务元数据

- taskNumber: 08
- subsystemId: SimulationRuleOverrides
- layer: authoritative-simulation
- currentNltxStatus: missing
- originalBoundary: 从 ContentCatalog/客户端 Creative 元数据中提升
- relatedSubsystems: WorldSession, WorldGenerationAndEcology, WorldCalendarAndEventOrchestration, PlayerGameplay, PersistenceAndRecovery, NetworkSessionAndSectionStreaming
- reportPath: D:\TRbackup\NLTX\docs\research\2026-09-05-version4-simulation-rule-overrides-public-decomposition.md

## 专属目标

围绕 SimulationRuleOverrides，基于真实 Version4 代码确定权威状态、写入根、生命周期、调用方向和跨域提交边界，并提出可实现但不落地的 ECS 拆分设计。该子系统的责任是：Journey/Creative 权限控制的规则覆写、权限校验和每 Tick 不可变规则快照。

“拆分为组件”只表示 proposed design。不得创建实际组件、System、Query、Command、Adapter、Projection、测试或项目文件。

## Version4 必须深读和重新定位的专属证据

- D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowerManager.cs:92-110：Power 注册和权威规则类别；:132-197：保存/加载、权限和玩家同步路径。
- D:\TRbackup\Version4\Terraria\Main.cs:3159-3175：UpdateTimeRate 读取时间 Power；:11364-11374：世界难度覆写。
- D:\TRbackup\Version4\Terraria\WorldGen.cs:59408-59415：生态传播读取停止/覆写 Power；相关刷怪、天气和生态调用点。
- D:\TRbackup\Version4\Terraria\WorldFile.cs:3284、:3517-3525：Creative powers 校验、保存和加载。

任务包中的路径和行号只是检索线索。必须重新读取文件、定位实际符号和调用上下文；报告使用实际行号，并标记 confirmed、partial、missing、unresolved、evidence-mismatch 或 version-drift。

## tModLoader 公开 API 交叉验证

- D:\TRbackup\tmodloader-api-docs-stable\class_mod_system.html:253-259：规则在世界 Tick 中被消费的阶段交叉验证。
- 按索引检索世界/玩家存档、网络接收和权限相关公开页面；不把 UI 排序当作权威证据。

不得用 tModLoader 文档替代 Version4 私有行为；必须记录实际 HTML 文件、页面标题、文档版本、成员锚点和仅用于何种边界交叉验证。

## Space Station 14 最小相关 ECS 参考

只读检索下列最小相关关键词和目录：
- permissions、rule override、game rule、admin command
- immutable snapshot、configuration component、system ordering、network/persistence adapter

只记录实际读取的 Space Station 14 文件、类型/方法和参考用途（Component 粒度、System/Query 边界、Event/Command、关系、网络或持久化）。禁止复制代码、命名、目录结构和领域语义；若无直接对应证据，明确写无直接对应证据。

## 当前 NLTX 必须浅读核对

- 当前 NLTX 的 src/Content/ContentPresentationIndex.cs 仅表达 Creative 显示元数据；没有权限、覆写状态、命令、恢复或复制闭环。
- 不得因为存在 Creative 内容名或排序就标为 partial/confirmed。

只能读取当前 src、Test、dome/src 和既有验证材料，不得修改；本轮并行审查不运行编译/测试命令，验证状态写为 not-run，历史真实输出只能写 existing-evidence。

## public-decomposition 专属问题

- Power 定义、权限、玩家/世界作用域、当前值和过期/恢复状态如何分离。
- 覆写命令何时验证、何时提交；Tick 开始采样的 immutable RuleSnapshot 如何防止中途双写。
- 时间、天气、生态、刷怪率和难度覆写分别由哪些下游 System 读取。
- Persistence/Network 只传输权威快照；外部权限/session ID 如何隔离。

必须完成成员/字段/方法盘点、读者/写者/生命周期/副作用表、权威状态所有权表、访问模式分组、Component/System/Query/Command/Adapter/Projection 接口契约、System 顺序、跨域依赖、ECS ID/关系建模、不拆分项和行为保持风险。所有新类型、路径和签名均标记 status: proposed；跨子系统类型标记 crossSubsystemOwner: integration-review。

## 专属交付重点

- 提出 RuleOverrideState、PermissionQuery、OverrideCommand、RuleSnapshotSystem、Persistence/Replication Adapter 的 proposed 设计。
- 报告必须说明一个 Power、一个玩家权限、一个世界覆写和一个非法命令的 focused verifier 计划。

报告必须写入：
D:\TRbackup\NLTX\docs\research\2026-09-05-version4-simulation-rule-overrides-public-decomposition.md

报告必须包含 Version4 事实、完整参考补证、tModLoader 交叉验证、Space Station 14 参考、当前 NLTX 状态、proposed 设计、not-run 验证、证据缺口、blocking-decision 和 Integration Handoff。报告只能使用中文说明与英文稳定 ID，不能宣称迁移完成、行为等价或 API 兼容。

## 明确不拆分的专属对象

- 单个 CreativePower、UI 排序项、按钮/菜单、单个权限消息、客户端 slider。

请说明每个对象为什么只是实例、策略、规则、查询、缓存、Adapter 或 Projection，而不是一级子系统。

## 完成条件

完成只读证据审查并生成唯一指定报告后，在最终回复中简要说明报告路径、实际读取的主要证据、本次未运行构建/测试、evidence-gap、blocking-decision 以及未修改生产代码。
