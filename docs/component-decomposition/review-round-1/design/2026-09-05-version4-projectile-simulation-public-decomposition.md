# 13 — ProjectileSimulation 独立只读审查与 ECS 设计提示词

本文件必须与以下公共协议共同使用，开始任务前先完整读取该文件：
D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\design\2026-09-05-version4-public-decomposition-common-protocol.md

公共协议中的只读、并行、证据优先级、行号复核、状态枚举、事实/设计隔离、ECS 文件组织、副作用隔离、报告结构和 Integration Handoff 规则全部适用于本任务。若本任务包与公共协议冲突，以公共协议为准。

本任务属于已经固定的 19 个游戏模拟子系统。不得改名、合并、拆分或扩展固定清单；如果当前全量索引出现额外候选，只能记录为 boundary-challenge，交由最终整合会话处理。

## 任务元数据

- taskNumber: 13
- subsystemId: ProjectileSimulation
- layer: authoritative-simulation
- currentNltxStatus: partial
- originalBoundary: 初始投射物责任；FishingAndCatchSimulation、LeashedEntitySimulation、CombatAndStatus 与其分边界
- relatedSubsystems: FishingAndCatchSimulation, LeashedEntitySimulation, CombatAndStatus, SpatialSimulation, SpawnLifecycleAndLoot, PlayerGameplay
- reportPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-05-version4-projectile-simulation-public-decomposition.md

## 专属目标

围绕 ProjectileSimulation，基于真实 Version4 代码确定权威状态、写入根、生命周期、调用方向和跨域提交边界，并提出可实现但不落地的 ECS 拆分设计。该子系统的责任是：投射物轨迹、碰撞、生命周期和专用投射物状态机。

“拆分为组件”只表示 proposed design。不得创建实际组件、System、Query、Command、Adapter、Projection、测试或项目文件。

## Version4 必须深读和重新定位的专属证据

- D:\TRbackup\Version4\Terraria\Projectile.cs:25365：Update/AI dispatch 进入专用 AI style；全文件字段、生命周期、碰撞和伤害读写。
- Projectile.cs 中 NewProjectile、Kill、penetrate、timeLeft、owner、damage、AI 数组和实体 slot 的调用链。
- Projectile.cs:34073-34074、:47784：Fishing bobber 入口/空实现/交付调用，交给 FishingAndCatchSimulation。
- D:\TRbackup\Version4\Terraria\Terraria.GameContent\LeashedEntity.cs：独立 leashed registry/section 生命周期，不要吞并。
- D:\TRbackup\Version4\Terraria\Main.cs:11420-11472、MessageBuffer.cs：投射物更新和网络 spawn/receive。

任务包中的路径和行号只是检索线索。必须重新读取文件、定位实际符号和调用上下文；报告使用实际行号，并标记 confirmed、partial、missing、unresolved、evidence-mismatch 或 version-drift。

## tModLoader 公开 API 交叉验证

- D:\TRbackup\tmodloader-api-docs-stable\class_mod_projectile.html：公开投射物更新、命中、kill、AI 扩展边界；实际锚点必须重新定位。
- 按索引检索 ModPlayer/GlobalNPC 伤害、碰撞和网络页面，仅作公开契约交叉验证。

不得用 tModLoader 文档替代 Version4 私有行为；必须记录实际 HTML 文件、页面标题、文档版本、成员锚点和仅用于何种边界交叉验证。

## Space Station 14 最小相关 ECS 参考

只读检索下列最小相关关键词和目录：
- projectile、ballistic、lifetime、collision、damage
- entity component、system/query、spawn/despawn、network state、prediction

只记录实际读取的 Space Station 14 文件、类型/方法和参考用途（Component 粒度、System/Query 边界、Event/Command、关系、网络或持久化）。禁止复制代码、命名、目录结构和领域语义；若无直接对应证据，明确写无直接对应证据。

## 当前 NLTX 必须浅读核对

- 当前 NLTX 有 src/Projectile 的定义组件和 dome 投射物系统，但生命周期、轨迹、碰撞提交和 verifier 未闭合。
- Fishing 和 LeashedEntity 是独立任务；不得把其状态复回 Projectile 巨型组件。

只能读取当前 src、Test、dome/src 和既有验证材料，不得修改；本轮并行审查不运行编译/测试命令，验证状态写为 not-run，历史真实输出只能写 existing-evidence。

## public-decomposition 专属问题

- 投射物 identity、owner、lifetime、trajectory、collision、penetration 和 damage 如何按共同读写者拆分。
- 专用 AI style 是策略还是状态；何时需要 typed System，何时保留兼容数据。
- NewProjectile、Kill、penetration、碰撞命中与 Combat、Spawn/Loot、Network 的命令方向。
- 实体引用、网络 ID、slot、持久化字段和客户端预测/投影如何隔离。
- 投射物销毁、掉落、命中和网络复制的顺序是否有隐式依赖。

必须完成成员/字段/方法盘点、读者/写者/生命周期/副作用表、权威状态所有权表、访问模式分组、Component/System/Query/Command/Adapter/Projection 接口契约、System 顺序、跨域依赖、ECS ID/关系建模、不拆分项和行为保持风险。所有新类型、路径和签名均标记 status: proposed；跨子系统类型标记 crossSubsystemOwner: integration-review。

## 专属交付重点

- 提出 ProjectileIdentity、TrajectoryState、Lifetime/Penetration、CollisionQuery、ProjectileUpdateSystem、HitCommand、Spawn/KillPort、ReplicationProjection 的 proposed 设计。
- 报告必须明确 Fishing/LeashedEntity 的边界，并设计轨迹、命中、穿透、销毁和网络一致性 verifier。

报告必须写入：
D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-05-version4-projectile-simulation-public-decomposition.md

报告必须包含 Version4 事实、完整参考补证、tModLoader 交叉验证、Space Station 14 参考、当前 NLTX 状态、proposed 设计、not-run 验证、证据缺口、blocking-decision 和 Integration Handoff。报告只能使用中文说明与英文稳定 ID，不能宣称迁移完成、行为等价或 API 兼容。

## 明确不拆分的专属对象

- 单个 AI style、单个投射物类型、单个 damage 字段、单个 spawn packet、单个渲染 trail。

请说明每个对象为什么只是实例、策略、规则、查询、缓存、Adapter 或 Projection，而不是一级子系统。

## 完成条件

完成只读证据审查并生成唯一指定报告后，在最终回复中简要说明报告路径、实际读取的主要证据、本次未运行构建/测试、evidence-gap、blocking-decision 以及未修改生产代码。
