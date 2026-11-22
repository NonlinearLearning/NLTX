# 15 — FishingAndCatchSimulation 独立只读审查与 ECS 设计提示词

本文件必须与以下公共协议共同使用，开始任务前先完整读取该文件：
D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\design\2026-09-05-version4-public-decomposition-common-protocol.md

公共协议中的只读、并行、证据优先级、行号复核、状态枚举、事实/设计隔离、ECS 文件组织、副作用隔离、报告结构和 Integration Handoff 规则全部适用于本任务。若本任务包与公共协议冲突，以公共协议为准。

本任务属于已经固定的 19 个游戏模拟子系统。不得改名、合并、拆分或扩展固定清单；如果当前全量索引出现额外候选，只能记录为 boundary-challenge，交由最终整合会话处理。

## 任务元数据

- taskNumber: 15
- subsystemId: FishingAndCatchSimulation
- layer: authoritative-simulation
- currentNltxStatus: partial
- originalBoundary: 从 ProjectileSimulation 与 ItemContainerAndEconomy 交界提升
- relatedSubsystems: ProjectileSimulation, ItemContainerAndEconomy, PlayerGameplay, WorldSession, WorldGenerationAndEcology, LiquidSimulation, SpawnLifecycleAndLoot
- reportPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-05-version4-fishing-and-catch-simulation-public-decomposition.md

## 专属目标

围绕 FishingAndCatchSimulation，基于真实 Version4 代码确定权威状态、写入根、生命周期、调用方向和跨域提交边界，并提出可实现但不落地的 ECS 拆分设计。该子系统的责任是：钓鱼资格、浮标时序、钓获判定以及 Item/NPC 结果请求。

“拆分为组件”只表示 proposed design。不得创建实际组件、System、Query、Command、Adapter、Projection、测试或项目文件。

## Version4 必须深读和重新定位的专属证据

- D:\TRbackup\Version4\Terraria\Projectile.cs:25365：进入 AI_061_FishingBobber；:34073-34074：AI_061_FishingBobber 和交付方法空体；:47784：交付调用点。
- D:\TRbackup\无任何删减通过编译\Terraria\Projectile.cs:51236、:51496：只有路径、类型、签名和调用邻域匹配时才补证，不能扩大 Version4 基线。
- D:\TRbackup\Version4\Terraria\Main.cs:1612-1674、:3359-3364：FishDropsDB 初始化和任务鱼轮换。
- Player.cs、Liquid.cs、WorldSession、Biome/Scene 查询和 Item/NPC 生成路径：资格和结果跨域证据。

任务包中的路径和行号只是检索线索。必须重新读取文件、定位实际符号和调用上下文；报告使用实际行号，并标记 confirmed、partial、missing、unresolved、evidence-mismatch 或 version-drift。

## tModLoader 公开 API 交叉验证

- D:\TRbackup\tmodloader-api-docs-stable\class_mod_player.html:2604-2632：ModifyFishingAttempt 资格阶段；:1636-1692：CatchFish/结果阶段，必须实际复核锚点。
- 按索引检索鱼饵、掉落、NPC/Item 生成和网络页面；公开 API 不替代 Version4 空实现补证。

不得用 tModLoader 文档替代 Version4 私有行为；必须记录实际 HTML 文件、页面标题、文档版本、成员锚点和仅用于何种边界交叉验证。

## Space Station 14 最小相关 ECS 参考

只读检索下列最小相关关键词和目录：
- fishing、catch、interaction、random outcome、reagent/fluid contact
- component state machine、eligibility query、event/command、loot result、inventory transaction

只记录实际读取的 Space Station 14 文件、类型/方法和参考用途（Component 粒度、System/Query 边界、Event/Command、关系、网络或持久化）。禁止复制代码、命名、目录结构和领域语义；若无直接对应证据，明确写无直接对应证据。

## 当前 NLTX 必须浅读核对

- 当前 NLTX 有 src/Player/PlayerFishingCapabilityState.cs 和 src/Content/FishingDropRuleCatalog.cs，但无 bobber 状态机、环境资格、鱼饵事务和结果提交。
- 状态/规则元数据不能替代 Fishing System；必须核对 dome/src/Fishing（如存在）只能作为现状证据。

只能读取当前 src、Test、dome/src 和既有验证材料，不得修改；本轮并行审查不运行编译/测试命令，验证状态写为 not-run，历史真实输出只能写 existing-evidence。

## public-decomposition 专属问题

- 浮标是 Projectile 载体还是钓获事务的实体；资格、时序、结果和提交如何分 System。
- 液体、天气、时间、Biome、任务、玩家能力和内容掉落规则的只读输入。
- Item 结果和 NPC 结果分别由 ItemContainerAndEconomy、Spawn/Loot 提交，本子系统只发结果 Command。
- 空实现与完整参考的证据状态、source-gap、version-drift 和行为风险。
- 鱼饵消耗、失败、重复钓获、网络请求和客户端提示如何保持幂等。

必须完成成员/字段/方法盘点、读者/写者/生命周期/副作用表、权威状态所有权表、访问模式分组、Component/System/Query/Command/Adapter/Projection 接口契约、System 顺序、跨域依赖、ECS ID/关系建模、不拆分项和行为保持风险。所有新类型、路径和签名均标记 status: proposed；跨子系统类型标记 crossSubsystemOwner: integration-review。

## 专属交付重点

- 提出 FishingAttemptState、BobberTiming、FishingEligibilityQuery、CatchDecision、CatchResultCommand、Item/NPC outcome Adapter/Projection 的 proposed 设计。
- 报告必须覆盖有鱼/无鱼、资格拒绝、鱼饵消耗、Item/NPC 结果、网络和 focused verifier 场景。

报告必须写入：
D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-05-version4-fishing-and-catch-simulation-public-decomposition.md

报告必须包含 Version4 事实、完整参考补证、tModLoader 交叉验证、Space Station 14 参考、当前 NLTX 状态、proposed 设计、not-run 验证、证据缺口、blocking-decision 和 Integration Handoff。报告只能使用中文说明与英文稳定 ID，不能宣称迁移完成、行为等价或 API 兼容。

## 明确不拆分的专属对象

- 浮标 Projectile 实例、单个鱼种、单个掉落规则、单个鱼饵字段、单个 UI bobber、单个 API Hook。

请说明每个对象为什么只是实例、策略、规则、查询、缓存、Adapter 或 Projection，而不是一级子系统。

## 完成条件

完成只读证据审查并生成唯一指定报告后，在最终回复中简要说明报告路径、实际读取的主要证据、本次未运行构建/测试、evidence-gap、blocking-decision 以及未修改生产代码。
