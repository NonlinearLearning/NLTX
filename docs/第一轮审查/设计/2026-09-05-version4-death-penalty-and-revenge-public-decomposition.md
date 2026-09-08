# 02 — DeathPenaltyAndRevenge 独立只读审查与 ECS 设计提示词

本文件必须与以下公共协议共同使用，开始任务前先完整读取该文件：
D:\TRbackup\NLTX\docs\第一轮审查\设计\2026-09-05-version4-public-decomposition-common-protocol.md

公共协议中的只读、并行、证据优先级、行号复核、状态枚举、事实/设计隔离、ECS 文件组织、副作用隔离、报告结构和 Integration Handoff 规则全部适用于本任务。若本任务包与公共协议出现冲突，以公共协议为准。

本任务属于已经固定的 19 个游戏模拟子系统。不得改名、合并、拆分或扩展固定清单；如果当前全量索引出现额外候选，只能记录为 boundary-challenge，交由最终整合会话处理。

## 任务元数据

- taskNumber: 02
- subsystemId: DeathPenaltyAndRevenge
- layer: authoritative-simulation
- currentNltxStatus: missing
- originalBoundary: 从 CombatAndStatus 与 SpawnLifecycleAndLoot 的交界处反向拆出
- relatedSubsystems: CombatAndStatus, PlayerGameplay, NpcAndTownSimulation, ItemContainerAndEconomy, PersistenceAndRecovery, NetworkSessionAndSectionStreaming, ClientPresentationAndTools
- reportPath: D:\TRbackup\NLTX\docs\第一轮审查\2026-09-05-version4-death-penalty-and-revenge-public-decomposition.md

## 专属目标

围绕 DeathPenaltyAndRevenge，基于真实 Version4 代码确定权威状态、写入根、生命周期、调用方向和跨域提交边界，并提出可实现但不落地的 ECS 拆分设计。该子系统的责任是：金币损失复仇 marker、过期、资格、重生尝试、敌人重建和 marker 同步。

“拆分为组件”只表示 proposed design。不得创建实际组件、System、Query、Command、Adapter、Projection、测试或项目文件。

## Version4 必须深读和重新定位的专属证据

- D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs:302 AddMarker；:310 构造/marker store；:317 CacheEnemy；:342 Reset；:353 Update；:360 CheckRespawns；:430 RemoveExpiredOrInvalidMarkers；:452 SendAllMarkersToPlayer。
- 同文件全部字段和 RevengeMarker 嵌套类型：identity、位置、NPC 目标、金币信息、过期时间、respawn lock、有效性和序列化。
- D:\TRbackup\Version4\Terraria\NPC.cs:6301 RevengeManager 初始化；:64542 死亡缓存/提交；:66518 重生检查和 NPC 重建。
- D:\TRbackup\Version4\Terraria\Main.cs:11451：每 tick 驱动 RevengeManager.Update；D:\TRbackup\Version4\Terraria\NetMessage.cs:2359：marker 复制。
- D:\TRbackup\Version4\Terraria\Player.cs：KillMe、死亡恢复和金币损失调用链；Item.cs、ItemDropRules、NPC.NewNPC：金币/物品和 NPC 结果。
- D:\TRbackup\Version4\Terraria\WorldFile.cs、PlayerFileData.cs、RemoteClient.cs、MessageBuffer.cs：持久化、连接和接收边界。

任务包中的路径和行号只是检索线索。必须重新读取文件、定位实际符号和调用上下文；报告使用实际行号，并标记 confirmed、partial、missing、unresolved、evidence-mismatch 或 version-drift。

## tModLoader 公开 API 交叉验证

- D:\TRbackup\tmodloader-api-docs-stable\class_mod_player.html:4270：ModPlayer.PreKill 或等价的公开死亡拦截边界。
- 按公共协议检索 NPC 生成/死亡、物品掉落、网络同步和世界/玩家存档页面；不得声称公开算法包含 CoinLossRevengeSystem。

不得用 tModLoader 文档替代 Version4 私有行为；必须记录实际 HTML 文件、页面标题、文档版本、成员锚点和仅用于何种边界交叉验证。

## Space Station 14 最小相关 ECS 参考

只读检索下列最小相关关键词和目录：
- death、dead、respawn、revive、damage、health
- spawn、despawn、entity lifecycle、player session、currency、wallet、loot、inventory、event、network state、replication

只记录实际读取的 Space Station 14 文件、类型/方法和参考用途（Component 粒度、System/Query 边界、Event/Command、关系、网络或持久化）。禁止复制代码、命名、目录结构和领域语义；若无直接对应证据，明确写无直接对应证据。

## 当前 NLTX 必须浅读核对

- 当前 NLTX 没有 marker 状态、重生 System、金币损失事务或 focused verifier。
- 必须分别核对 src/Combat、src/Player、src/Npc、src/Items 和 Test，不得因为已有死亡组件就标为已实现。

只能读取当前 src、Test、dome/src 和既有验证材料，不得修改；本轮并行审查不运行编译/测试命令，验证状态写为 not-run，历史真实输出只能写 existing-evidence。

## public-decomposition 专属问题

- marker 是世界级索引、实体级状态还是两者组合；marker store 的聚合边界和唯一写者。
- 玩家死亡事实、金币损失结果、marker 生命周期、NPC spawn commit 和客户端显示分别由谁拥有。
- respawn attempt lock 是 marker 生命周期字段、玩家状态还是独立事务 token；如何保证幂等。
- NPC 缓存数据与 NPC 实体 identity 如何分离；成功、失败、过期和无效删除的状态机。
- 网络复制和持久化只能读取不可变快照；NetworkId、PersistentEntityId、Player/NpcEntityId 和外部 SessionId 标记 integration-review。

必须完成成员/字段/方法盘点、读者/写者/生命周期/副作用表、权威状态所有权表、访问模式分组、Component/System/Query/Command/Adapter/Projection 接口契约、System 顺序、跨域依赖、ECS ID/关系建模、不拆分项和行为保持风险。所有新类型、路径和签名均标记 status: proposed；跨子系统类型标记 crossSubsystemOwner: integration-review。

## 专属交付重点

- 至少给出 DeathResolution 输入、CoinLoss 结果、RevengeMarker identity/lifecycle、Eligibility Query、Expiration System、Respawn Command/System、Replication/Persistence Adapter、Client Projection 的 proposed 设计。
- 必须画出死亡→金币→marker→资格→锁→NPC spawn commit→marker 结算→复制的真实/建议顺序，并标注 Version4 证据和未知处。

报告必须写入：
D:\TRbackup\NLTX\docs\第一轮审查\2026-09-05-version4-death-penalty-and-revenge-public-decomposition.md

报告必须包含 Version4 事实、完整参考补证、tModLoader 交叉验证、Space Station 14 参考、当前 NLTX 状态、proposed 设计、not-run 验证、证据缺口、blocking-decision 和 Integration Handoff。报告只能使用中文说明与英文稳定 ID，不能宣称迁移完成、行为等价或 API 兼容。

## 明确不拆分的专属对象

- 单个 RevengeMarker、单个 NPC 缓存条目、单个金币掉落、单个 respawn lock、单个死亡 Hook、单个网络消息、单个 UI marker、单个 NPC 类型、单个 ItemDropRule。

请说明每个对象为什么只是实例、策略、规则、查询、缓存、Adapter 或 Projection，而不是一级子系统。

## 完成条件

完成只读证据审查并生成唯一指定报告后，在最终回复中简要说明报告路径、实际读取的主要证据、本次未运行构建/测试、evidence-gap、blocking-decision 以及未修改生产代码。
