# 16 — CombatAndStatus 独立只读审查与 ECS 设计提示词

本文件必须与以下公共协议共同使用，开始任务前先完整读取该文件：
D:\TRbackup\NLTX\docs\第一轮审查\2026-09-05-version4-public-decomposition-common-protocol.md

公共协议中的只读、并行、证据优先级、行号复核、状态枚举、事实/设计隔离、ECS 文件组织、副作用隔离、报告结构和 Integration Handoff 规则全部适用于本任务。若本任务包与公共协议冲突，以公共协议为准。

本任务属于已经固定的 19 个游戏模拟子系统。不得改名、合并、拆分或扩展固定清单；如果当前全量索引出现额外候选，只能记录为 boundary-challenge，交由最终整合会话处理。

## 任务元数据

- taskNumber: 16
- subsystemId: CombatAndStatus
- layer: authoritative-simulation
- currentNltxStatus: confirmed
- originalBoundary: 初始战斗责任；DeathPenaltyAndRevenge 只接收死亡/金币结果
- relatedSubsystems: PlayerGameplay, NpcAndTownSimulation, ProjectileSimulation, DeathPenaltyAndRevenge, ItemContainerAndEconomy, SpatialSimulation
- reportPath: D:\TRbackup\NLTX\docs\research\2026-09-05-version4-combat-and-status-public-decomposition.md

## 专属目标

围绕 CombatAndStatus，基于真实 Version4 代码确定权威状态、写入根、生命周期、调用方向和跨域提交边界，并提出可实现但不落地的 ECS 拆分设计。该子系统的责任是：伤害资格、结算、免疫、死亡、归因和状态效果转换。

“拆分为组件”只表示 proposed design。不得创建实际组件、System、Query、Command、Adapter、Projection、测试或项目文件。

## Version4 必须深读和重新定位的专属证据

- D:\TRbackup\Version4\Terraria\Player.cs：KillMe（约 :22572）、生命/免疫/伤害字段和玩家战斗调用。
- D:\TRbackup\Version4\Terraria\NPC.cs：StrikeNPC、CheckDead、免疫和死亡/掉落边界；MessageBuffer.cs:1315、:1420：网络伤害入口。
- D:\TRbackup\Version4\Terraria\Projectile.cs：命中、穿透和伤害提交；Collision.cs：CanHit/受伤 Tile 查询。
- 当前 NLTX：D:\TRbackup\NLTX\src\Combat、src\StatusEffects、D:\TRbackup\NLTX\Test\Terraria.Combat.Verification；只能读取已有代码和历史验证。

任务包中的路径和行号只是检索线索。必须重新读取文件、定位实际符号和调用上下文；报告使用实际行号，并标记 confirmed、partial、missing、unresolved、evidence-mismatch 或 version-drift。

## tModLoader 公开 API 交叉验证

- D:\TRbackup\tmodloader-api-docs-stable\class_mod_player.html：ModPlayer 伤害、死亡和状态扩展；按索引重新定位实际成员。
- class_global_n_p_c.html、class_mod_projectile.html：NPC/Projectile 命中边界，仅作公开契约交叉验证。

不得用 tModLoader 文档替代 Version4 私有行为；必须记录实际 HTML 文件、页面标题、文档版本、成员锚点和仅用于何种边界交叉验证。

## Space Station 14 最小相关 ECS 参考

只读检索下列最小相关关键词和目录：
- damage、health、status effect、death、stun、immunity
- damage system、event/command、component state、entity query、replication

只记录实际读取的 Space Station 14 文件、类型/方法和参考用途（Component 粒度、System/Query 边界、Event/Command、关系、网络或持久化）。禁止复制代码、命名、目录结构和领域语义；若无直接对应证据，明确写无直接对应证据。

## 当前 NLTX 必须浅读核对

- 当前索引将 CombatAndStatus 标为 confirmed，但本会话不得重新声称迁移完成；必须区分历史 existing-evidence 与本次 verificationStatus=not-run。
- 读取现有 Combat System 和 verifier，指出仍未覆盖的全局编排、Adapter、NPC 交接或行为等价风险。

只能读取当前 src、Test、dome/src 和既有验证材料，不得修改；本轮并行审查不运行编译/测试命令，验证状态写为 not-run，历史真实输出只能写 existing-evidence。

## public-decomposition 专属问题

- 伤害请求、资格 Query、结算、免疫、生命状态、死亡事实和状态效果转换的唯一写者。
- 死亡事实何时交给 DeathPenaltyAndRevenge、SpawnLifecycleAndLoot、PlayerGameplay；Combat 不拥有金币 marker 或 NPC 重建。
- NPC/Player/Projectile 的伤害来源、归因、网络输入和客户端战斗文本如何隔离。
- 现有 NLTX System/组件是否满足 controlled commit 和 focused verifier；哪些仍是 partial。
- 状态效果叠加、免疫窗口、死亡幂等和失败边界如何表达。

必须完成成员/字段/方法盘点、读者/写者/生命周期/副作用表、权威状态所有权表、访问模式分组、Component/System/Query/Command/Adapter/Projection 接口契约、System 顺序、跨域依赖、ECS ID/关系建模、不拆分项和行为保持风险。所有新类型、路径和签名均标记 status: proposed；跨子系统类型标记 crossSubsystemOwner: integration-review。

## 专属交付重点

- 提出 DamageRequest、DamageEligibilityQuery、DamageResolutionSystem、Health/Immunity/Status Components、DeathResultEvent、Replication/Presentation Projection 的 proposed 边界。
- 报告必须引用现有 Test/Terraria.Combat.Verification 的实际测试和历史输出（若读取到），并明确本次未运行。

报告必须写入：
D:\TRbackup\NLTX\docs\research\2026-09-05-version4-combat-and-status-public-decomposition.md

报告必须包含 Version4 事实、完整参考补证、tModLoader 交叉验证、Space Station 14 参考、当前 NLTX 状态、proposed 设计、not-run 验证、证据缺口、blocking-decision 和 Integration Handoff。报告只能使用中文说明与英文稳定 ID，不能宣称迁移完成、行为等价或 API 兼容。

## 明确不拆分的专属对象

- 单个伤害数字、单个 Buff、单个 NPC 命中、单个 CombatText、单个 PreKill Hook、DeathPenalty marker。

请说明每个对象为什么只是实例、策略、规则、查询、缓存、Adapter 或 Projection，而不是一级子系统。

## 完成条件

完成只读证据审查并生成唯一指定报告后，在最终回复中简要说明报告路径、实际读取的主要证据、本次未运行构建/测试、evidence-gap、blocking-decision 以及未修改生产代码。
