# 07 — WorldProgressionAndUnlocks 独立只读审查与 ECS 设计提示词

本文件必须与以下公共协议共同使用，开始任务前先完整读取该文件：
D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\design\2026-09-05-version4-public-decomposition-common-protocol.md

公共协议中的只读、并行、证据优先级、行号复核、状态枚举、事实/设计隔离、ECS 文件组织、副作用隔离、报告结构和 Integration Handoff 规则全部适用于本任务。若本任务包与公共协议出现冲突，以公共协议为准。

本任务属于已经固定的 19 个游戏模拟子系统。不得改名、合并、拆分或扩展固定清单；如果当前全量索引出现额外候选，只能记录为 boundary-challenge，交由最终整合会话处理。

## 任务元数据

- taskNumber: 07
- subsystemId: WorldProgressionAndUnlocks
- layer: authoritative-simulation
- currentNltxStatus: missing
- originalBoundary: 从内容/图鉴表现元数据中提升
- relatedSubsystems: NpcAndTownSimulation, WorldSession, PersistenceAndRecovery, ExternalBoundaries, ContentCatalog
- reportPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-05-version4-world-progression-and-unlocks-public-decomposition.md

## 专属目标

围绕 WorldProgressionAndUnlocks，基于真实 Version4 代码确定权威状态、写入根、生命周期、调用方向和跨域提交边界，并提出可实现但不落地的 ECS 拆分设计。该子系统的责任是：会改变权威资格的持久化发现、击杀、目击、交谈和解锁事实。

“拆分为组件”只表示 proposed design。不得创建实际组件、System、Query、Command、Adapter、Projection、测试或项目文件。

## Version4 必须深读和重新定位的专属证据

- D:\TRbackup\Version4\Terraria.GameContent.Bestiary\BestiaryUnlocksTracker.cs:5-56：tracker 初始化、保存、加载、重置、校验和玩家同步。
- D:\TRbackup\Version4\Terraria.GameContent.Bestiary\NPCKillsTracker.cs:23-46：击杀登记和更新广播。
- D:\TRbackup\Version4\Terraria\Main.cs:1023、:3355：BestiaryTracker 持有和初始化；:14001-14004、:14167-14170：进度影响动物学家/城镇 NPC 资格。
- D:\TRbackup\Version4\Terraria.IO\WorldFile.cs：Bestiary/世界进度的持久化和网络路径；Achievement/Social 代码只作排除或投影对照。

任务包中的路径和行号只是检索线索。必须重新读取文件、定位实际符号和调用上下文；报告使用实际行号，并标记 confirmed、partial、missing、unresolved、evidence-mismatch 或 version-drift。

## tModLoader 公开 API 交叉验证

- D:\TRbackup\tmodloader-api-docs-stable\class_mod_system.html:322-335：世界存档/同步扩展边界。
- 按索引检索 ModNPC 生成资格、Bestiary、世界数据和网络接收页面；不得把 API 页面当私有 tracker 实现。

不得用 tModLoader 文档替代 Version4 私有行为；必须记录实际 HTML 文件、页面标题、文档版本、成员锚点和仅用于何种边界交叉验证。

## Space Station 14 最小相关 ECS 参考

只读检索下列最小相关关键词和目录：
- discovery/unlock component、achievement/progression、player profile
- event subscriber、persistent state、eligibility query、network replication

只记录实际读取的 Space Station 14 文件、类型/方法和参考用途（Component 粒度、System/Query 边界、Event/Command、关系、网络或持久化）。禁止复制代码、命名、目录结构和领域语义；若无直接对应证据，明确写无直接对应证据。

## 当前 NLTX 必须浅读核对

- 当前 NLTX 只有 src/Content/ContentIdentityCatalog.cs、ContentPresentationIndex.cs 等身份/显示元数据，没有进度存储、事件订阅、资格 Query 或投影。
- 必须核对 Test 中是否有进度验证；没有就保持 not-run/missing。

只能读取当前 src、Test、dome/src 和既有验证材料，不得修改；本轮并行审查不运行编译/测试命令，验证状态写为 not-run，历史真实输出只能写 existing-evidence。

## public-decomposition 专属问题

- 哪些发现事实真正改变 NPC/世界资格，哪些只是客户端图鉴或平台成就投影。
- Tracker 的持久化 ID、版本迁移、重置、校验和玩家同步如何拆分。
- 击杀/目击/交谈事件由谁发布，Unlock System 是否只写自身聚合，NPC 只读资格 Query。
- Persistence、Network 和 Client Projection 是否都读取同一不可变进度快照。

必须完成成员/字段/方法盘点、读者/写者/生命周期/副作用表、权威状态所有权表、访问模式分组、Component/System/Query/Command/Adapter/Projection 接口契约、System 顺序、跨域依赖、ECS ID/关系建模、不拆分项和行为保持风险。所有新类型、路径和签名均标记 status: proposed；跨子系统类型标记 crossSubsystemOwner: integration-review。

## 专属交付重点

- 提出 UnlockFact、ProgressionAggregate、UnlockEvent、EligibilityQuery、ProgressionCommitSystem、Persistence/Replication Adapter、Client Projection 的 proposed 设计。
- 报告必须覆盖击杀登记、保存加载、坏数据校验、加入同步、NPC 资格消费和平台成就排除。

报告必须写入：
D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-05-version4-world-progression-and-unlocks-public-decomposition.md

报告必须包含 Version4 事实、完整参考补证、tModLoader 交叉验证、Space Station 14 参考、当前 NLTX 状态、proposed 设计、not-run 验证、证据缺口、blocking-decision 和 Integration Handoff。报告只能使用中文说明与英文稳定 ID，不能宣称迁移完成、行为等价或 API 兼容。

## 明确不拆分的专属对象

- 单个 bestiary entry、单个 achievement notification、单个击杀计数、单个 UI 图标、Social API。

请说明每个对象为什么只是实例、策略、规则、查询、缓存、Adapter 或 Projection，而不是一级子系统。

## 完成条件

完成只读证据审查并生成唯一指定报告后，在最终回复中简要说明报告路径、实际读取的主要证据、本次未运行构建/测试、evidence-gap、blocking-decision 以及未修改生产代码。
