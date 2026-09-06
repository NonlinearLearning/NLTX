# 05 — WorldCalendarAndEventOrchestration 独立只读审查与 ECS 设计提示词

本文件必须与以下公共协议共同使用，开始任务前先完整读取该文件：
D:\TRbackup\NLTX\docs\第一轮审查\2026-09-05-version4-public-decomposition-common-protocol.md

公共协议中的只读、并行、证据优先级、行号复核、状态枚举、事实/设计隔离、ECS 文件组织、副作用隔离、报告结构和 Integration Handoff 规则全部适用于本任务。若本任务包与公共协议出现冲突，以公共协议为准。

本任务属于已经固定的 19 个游戏模拟子系统。不得改名、合并、拆分或扩展固定清单；如果当前全量索引出现额外候选，只能记录为 boundary-challenge，交由最终整合会话处理。

## 任务元数据

- taskNumber: 05
- subsystemId: WorldCalendarAndEventOrchestration
- layer: authoritative-simulation
- currentNltxStatus: partial
- originalBoundary: 从 WorldSession 的一般时间更新中提升
- relatedSubsystems: WorldSession, NpcAndTownSimulation, SpawnLifecycleAndLoot, WorldGenerationAndEcology, SimulationRuleOverrides
- reportPath: D:\TRbackup\NLTX\docs\research\2026-09-05-version4-world-calendar-and-event-orchestration-public-decomposition.md

## 专属目标

围绕 WorldCalendarAndEventOrchestration，基于真实 Version4 代码确定权威状态、写入根、生命周期、调用方向和跨域提交边界，并提出可实现但不落地的 ECS 拆分设计。该子系统的责任是：日历边界、事件资格、事件实例和提交后的活动事实。

“拆分为组件”只表示 proposed design。不得创建实际组件、System、Query、Command、Adapter、Projection、测试或项目文件。

## Version4 必须深读和重新定位的专属证据

- D:\TRbackup\Version4\Terraria\Main.cs:12972 UpdateTime；:13112-13118 事件 UpdateTime 调用；:13268 UpdateTime_StartDay；:13470 UpdateTime_StartDay。
- Main.cs:13470-13564：昼夜边界、渔夫任务轮换、入侵/事件评估和城镇生成触发。
- Main.cs:12050 UpdateWeather、:11346 调用天气；相关事件类、Invasion、DD2、LanternNight、BirthdayParty、Sandstorm 和 Cultist 调用链。
- WorldFile.cs、NetMessage.cs、NPC.cs：事件状态保存、同步、刷怪和城镇后果。

任务包中的路径和行号只是检索线索。必须重新读取文件、定位实际符号和调用上下文；报告使用实际行号，并标记 confirmed、partial、missing、unresolved、evidence-mismatch 或 version-drift。

## tModLoader 公开 API 交叉验证

- D:\TRbackup\tmodloader-api-docs-stable\class_mod_system.html:253-259：PostUpdateTime/PostUpdateWorld 顺序边界。
- 按索引检索事件、世界更新、入侵和网络公告的公开生命周期；只作交叉验证。

不得用 tModLoader 文档替代 Version4 私有行为；必须记录实际 HTML 文件、页面标题、文档版本、成员锚点和仅用于何种边界交叉验证。

## Space Station 14 最小相关 ECS 参考

只读检索下列最小相关关键词和目录：
- round timer、event scheduler、game rule、event proposal、event state
- random eligibility、system order、network snapshot、admin/player session

只记录实际读取的 Space Station 14 文件、类型/方法和参考用途（Component 粒度、System/Query 边界、Event/Command、关系、网络或持久化）。禁止复制代码、命名、目录结构和领域语义；若无直接对应证据，明确写无直接对应证据。

## 当前 NLTX 必须浅读核对

- 当前 NLTX 有 WorldSession 中的部分事件状态，但无统一日历事件调度器、资格 Query 或 focused verifier。
- 事件状态存在不等于事件生命周期已实现。

只能读取当前 src、Test、dome/src 和既有验证材料，不得修改；本轮并行审查不运行编译/测试命令，验证状态写为 not-run，历史真实输出只能写 existing-evidence。

## public-decomposition 专属问题

- 时间推进、昼夜边界、定时器、随机资格和事件实例是否应分成状态组件与调度 System。
- 事件资格读取哪些 WorldSession、Progression、Biome、NPC 和 RuleOverride 快照。
- 事件启动/停止、失败重试、公告和刷怪后果的提交边界。
- 如何避免事件系统与 WorldSession、Npc/Town、Spawn/Loot 重复写事件或刷怪状态。

必须完成成员/字段/方法盘点、读者/写者/生命周期/副作用表、权威状态所有权表、访问模式分组、Component/System/Query/Command/Adapter/Projection 接口契约、System 顺序、跨域依赖、ECS ID/关系建模、不拆分项和行为保持风险。所有新类型、路径和签名均标记 status: proposed；跨子系统类型标记 crossSubsystemOwner: integration-review。

## 专属交付重点

- 提出 CalendarClock、EventEligibilityQuery、EventInstanceState、EventScheduleSystem、EventCommitCommand、Announcement/Replication Projection 的 proposed 设计。
- 报告必须包含至少日夜边界、入侵/活动启动、事件结束和服务器/客户端投影的 verifier 场景。

报告必须写入：
D:\TRbackup\NLTX\docs\research\2026-09-05-version4-world-calendar-and-event-orchestration-public-decomposition.md

报告必须包含 Version4 事实、完整参考补证、tModLoader 交叉验证、Space Station 14 参考、当前 NLTX 状态、proposed 设计、not-run 验证、证据缺口、blocking-decision 和 Integration Handoff。报告只能使用中文说明与英文稳定 ID，不能宣称迁移完成、行为等价或 API 兼容。

## 明确不拆分的专属对象

- 单个事件类、单个随机数、单个公告消息、单个 NPC spawn、客户端天气/音乐表现。

请说明每个对象为什么只是实例、策略、规则、查询、缓存、Adapter 或 Projection，而不是一级子系统。

## 完成条件

完成只读证据审查并生成唯一指定报告后，在最终回复中简要说明报告路径、实际读取的主要证据、本次未运行构建/测试、evidence-gap、blocking-decision 以及未修改生产代码。
