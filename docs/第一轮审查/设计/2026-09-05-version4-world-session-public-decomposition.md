# 04 — WorldSession 独立只读审查与 ECS 设计提示词

本文件必须与以下公共协议共同使用，开始任务前先完整读取该文件：
D:\TRbackup\NLTX\docs\第一轮审查\设计\2026-09-05-version4-public-decomposition-common-protocol.md

公共协议中的只读、并行、证据优先级、行号复核、状态枚举、事实/设计隔离、ECS 文件组织、副作用隔离、报告结构和 Integration Handoff 规则全部适用于本任务。若本任务包与公共协议出现冲突，以公共协议为准。

本任务属于已经固定的 19 个游戏模拟子系统。不得改名、合并、拆分或扩展固定清单；如果当前全量索引出现额外候选，只能记录为 boundary-challenge，交由最终整合会话处理。

## 任务元数据

- taskNumber: 04
- subsystemId: WorldSession
- layer: authoritative-simulation
- currentNltxStatus: partial
- originalBoundary: 初始世界数据根；与 WorldCalendarAndEventOrchestration、WorldStorage 分边界
- relatedSubsystems: WorldCalendarAndEventOrchestration, WorldProgressionAndTransition, SimulationRuleOverrides, WorldStorage, RuntimeComposition
- reportPath: D:\TRbackup\NLTX\docs\第一轮审查\2026-09-05-version4-world-session-public-decomposition.md

## 专属目标

围绕 WorldSession，基于真实 Version4 代码确定权威状态、写入根、生命周期、调用方向和跨域提交边界，并提出可实现但不落地的 ECS 拆分设计。该子系统的责任是：持久世界规则、时间/天气事实以及会话级 ready/readiness 状态。

“拆分为组件”只表示 proposed design。不得创建实际组件、System、Query、Command、Adapter、Projection、测试或项目文件。

## Version4 必须深读和重新定位的专属证据

- D:\TRbackup\Version4\Terraria\Main.cs:11193 DoUpdate、:11411 DoUpdateInWorld、:12050 UpdateWeather、:12972 UpdateTime、:11344-11355 世界更新门控。
- D:\TRbackup\Version4\Terraria\Main.cs:11420-11472：玩家、计数、刷怪和实体更新的世界 Tick 顺序。
- D:\TRbackup\Version4\Terraria\WorldFile.cs:658、:878-933：世界加载/保存与临时状态恢复；WorldFileData.cs：世界文件状态和异常。
- Main.cs 中 world readiness、dayTime、weatherCounter、事件门控和 server/client 分支的实际读写者。

任务包中的路径和行号只是检索线索。必须重新读取文件、定位实际符号和调用上下文；报告使用实际行号，并标记 confirmed、partial、missing、unresolved、evidence-mismatch 或 version-drift。

## tModLoader 公开 API 交叉验证

- D:\TRbackup\tmodloader-api-docs-stable\class_mod_system.html:253-259：时间后更新/世界更新边界。
- D:\TRbackup\tmodloader-api-docs-stable\class_mod_system.html:1020-1044：PreUpdateEntities 作为阶段交叉验证；按索引补查世界加载/存档。

不得用 tModLoader 文档替代 Version4 私有行为；必须记录实际 HTML 文件、页面标题、文档版本、成员锚点和仅用于何种边界交叉验证。

## Space Station 14 最小相关 ECS 参考

只读检索下列最小相关关键词和目录：
- game state、round/session state、world time、weather、server readiness
- system scheduling、snapshot、persistence、network state

只记录实际读取的 Space Station 14 文件、类型/方法和参考用途（Component 粒度、System/Query 边界、Event/Command、关系、网络或持久化）。禁止复制代码、命名、目录结构和领域语义；若无直接对应证据，明确写无直接对应证据。

## 当前 NLTX 必须浅读核对

- 当前 NLTX 有 src/WorldSession/WorldSessionComponents.cs 等状态记录，但 ownership、执行链和 verifier 未闭合。
- 不得因为存在 WorldTimeWeatherState 或 WorldEventProgressState 就把 WorldSession 标为 confirmed。

只能读取当前 src、Test、dome/src 和既有验证材料，不得修改；本轮并行审查不运行编译/测试命令，验证状态写为 not-run，历史真实输出只能写 existing-evidence。

## public-decomposition 专属问题

- WorldSession 最小权威写集是什么；时间、天气、事件、Hardmode 和 Creative override 的边界。
- ready/loading/unloading 状态由谁驱动；RuntimeComposition 只编排还是写世界状态。
- 服务器、单机和客户端读取哪些事实；网络/存档只消费快照还是能触发恢复命令。
- WorldSession 与 WorldCalendarAndEventOrchestration 的时钟和边界事件如何避免双写。

必须完成成员/字段/方法盘点、读者/写者/生命周期/副作用表、权威状态所有权表、访问模式分组、Component/System/Query/Command/Adapter/Projection 接口契约、System 顺序、跨域依赖、ECS ID/关系建模、不拆分项和行为保持风险。所有新类型、路径和签名均标记 status: proposed；跨子系统类型标记 crossSubsystemOwner: integration-review。

## 专属交付重点

- 至少提出 WorldRules/Time/Weather、SessionReadiness、WorldTickSnapshot、WorldStateCommit Port、Persistence/Replication Adapter 的 proposed 方案。
- 报告必须给出 DoUpdate→DoUpdateInWorld→world state→downstream systems 的真实顺序和 focused verifier 计划。

报告必须写入：
D:\TRbackup\NLTX\docs\第一轮审查\2026-09-05-version4-world-session-public-decomposition.md

报告必须包含 Version4 事实、完整参考补证、tModLoader 交叉验证、Space Station 14 参考、当前 NLTX 状态、proposed 设计、not-run 验证、证据缺口、blocking-decision 和 Integration Handoff。报告只能使用中文说明与英文稳定 ID，不能宣称迁移完成、行为等价或 API 兼容。

## 明确不拆分的专属对象

- 单个 Main 静态字段、单个天气效果、单个事件类型、单个保存字段、客户端天气粒子。

请说明每个对象为什么只是实例、策略、规则、查询、缓存、Adapter 或 Projection，而不是一级子系统。

## 完成条件

完成只读证据审查并生成唯一指定报告后，在最终回复中简要说明报告路径、实际读取的主要证据、本次未运行构建/测试、evidence-gap、blocking-decision 以及未修改生产代码。
