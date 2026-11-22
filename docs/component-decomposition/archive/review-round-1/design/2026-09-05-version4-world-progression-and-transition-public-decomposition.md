# 06 — WorldProgressionAndTransition 独立只读审查与 ECS 设计提示词

本文件必须与以下公共协议共同使用，开始任务前先完整读取该文件：
D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\design\2026-09-05-version4-public-decomposition-common-protocol.md

公共协议中的只读、并行、证据优先级、行号复核、状态枚举、事实/设计隔离、ECS 文件组织、副作用隔离、报告结构和 Integration Handoff 规则全部适用于本任务。若本任务包与公共协议出现冲突，以公共协议为准。

本任务属于已经固定的 19 个游戏模拟子系统。不得改名、合并、拆分或扩展固定清单；如果当前全量索引出现额外候选，只能记录为 boundary-challenge，交由最终整合会话处理。

## 任务元数据

- taskNumber: 06
- subsystemId: WorldProgressionAndTransition
- layer: authoritative-simulation
- currentNltxStatus: partial
- originalBoundary: 从 WorldSession/WorldGenerationAndEcology 提升
- relatedSubsystems: WorldGenerationAndEcology, WorldSession, WorldStorage, ExternalBoundaries, SimulationRuleOverrides
- reportPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-05-version4-world-progression-and-transition-public-decomposition.md

## 专属目标

围绕 WorldProgressionAndTransition，基于真实 Version4 代码确定权威状态、写入根、生命周期、调用方向和跨域提交边界，并提出可实现但不落地的 ECS 拆分设计。该子系统的责任是：Hardmode 等长周期世界过渡的资格、计划、提交屏障、重同步和后续规则切换。

“拆分为组件”只表示 proposed design。不得创建实际组件、System、Query、Command、Adapter、Projection、测试或项目文件。

## Version4 必须深读和重新定位的专属证据

- D:\TRbackup\Version4\Terraria\WorldGen.cs:26084 StartHardmode 及其周边：世界模式改变、保护、后台转换、结束清理。
- D:\TRbackup\Version4\Terraria\NPC.cs:65959：NPC/事件路径触发 WorldGen.StartHardmode。
- WorldGen 中 Hardmode 任务、感染/矿阶/生态变更和区段重同步调用；Main.cs、NetMessage.cs 中结果发布。
- WorldFile.cs：Hardmode/世界进度存档、验证和加载恢复。

任务包中的路径和行号只是检索线索。必须重新读取文件、定位实际符号和调用上下文；报告使用实际行号，并标记 confirmed、partial、missing、unresolved、evidence-mismatch 或 version-drift。

## tModLoader 公开 API 交叉验证

- D:\TRbackup\tmodloader-api-docs-stable\class_mod_system.html:139-140：可修改的 Hardmode 任务序列。
- 按公共协议检索 OnWorldLoad、SaveWorldData、PostUpdateWorld 和网络恢复边界。

不得用 tModLoader 文档替代 Version4 私有行为；必须记录实际 HTML 文件、页面标题、文档版本、成员锚点和仅用于何种边界交叉验证。

## Space Station 14 最小相关 ECS 参考

只读检索下列最小相关关键词和目录：
- round transition、game rule transition、migration/transaction
- world generation pass、commit barrier、state machine、rollback、network resync

只记录实际读取的 Space Station 14 文件、类型/方法和参考用途（Component 粒度、System/Query 边界、Event/Command、关系、网络或持久化）。禁止复制代码、命名、目录结构和领域语义；若无直接对应证据，明确写无直接对应证据。

## 当前 NLTX 必须浅读核对

- 当前 NLTX 有 HardMode、矿阶和感染等 WorldSession 状态，也有 WorldGeneration 生命周期，但没有 transition executor、计划/提交屏障或 verifier。
- 状态字段不能替代长事务执行链。

只能读取当前 src、Test、dome/src 和既有验证材料，不得修改；本轮并行审查不运行编译/测试命令，验证状态写为 not-run，历史真实输出只能写 existing-evidence。

## public-decomposition 专属问题

- Hardmode 触发事实、过渡计划、Tile/生态修改、世界规则提交和区段重同步如何分阶段。
- 后台/预算/暂停/失败重试和恢复如何表达，哪些副作用隔离到 Port。
- WorldGenerationAndEcology 负责 pass 执行还是只提供生成能力；WorldStorage 谁拥有 Tile 提交。
- ExternalBoundaries 何时读取不可变 transition result，如何保证一次提交不重复应用。

必须完成成员/字段/方法盘点、读者/写者/生命周期/副作用表、权威状态所有权表、访问模式分组、Component/System/Query/Command/Adapter/Projection 接口契约、System 顺序、跨域依赖、ECS ID/关系建模、不拆分项和行为保持风险。所有新类型、路径和签名均标记 status: proposed；跨子系统类型标记 crossSubsystemOwner: integration-review。

## 专属交付重点

- 至少提出 ProgressionTransitionState、TransitionPlan、EligibilityQuery、Generation/Mutation Command、CommitBarrier、Resync Adapter/Projection 的 proposed 设计。
- 报告必须明确 Hardmode 过渡的真实调用链和 verifier：资格、部分失败、重试、幂等、恢复和网络一致性。

报告必须写入：
D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-05-version4-world-progression-and-transition-public-decomposition.md

报告必须包含 Version4 事实、完整参考补证、tModLoader 交叉验证、Space Station 14 参考、当前 NLTX 状态、proposed 设计、not-run 验证、证据缺口、blocking-decision 和 Integration Handoff。报告只能使用中文说明与英文稳定 ID，不能宣称迁移完成、行为等价或 API 兼容。

## 明确不拆分的专属对象

- HardMode 布尔字段、单个感染 Tile、单个生成 pass、单个网络消息、单个背景效果。

请说明每个对象为什么只是实例、策略、规则、查询、缓存、Adapter 或 Projection，而不是一级子系统。

## 完成条件

完成只读证据审查并生成唯一指定报告后，在最终回复中简要说明报告路径、实际读取的主要证据、本次未运行构建/测试、evidence-gap、blocking-decision 以及未修改生产代码。
