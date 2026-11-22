# 19 — SpawnLifecycleAndLoot 独立只读审查与 ECS 设计提示词

本文件必须与以下公共协议共同使用，开始任务前先完整读取该文件：
D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\design\2026-09-05-version4-public-decomposition-common-protocol.md

公共协议中的只读、并行、证据优先级、行号复核、状态枚举、事实/设计隔离、ECS 文件组织、副作用隔离、报告结构和 Integration Handoff 规则全部适用于本任务。若本任务包与公共协议冲突，以公共协议为准。

本任务属于已经固定的 19 个游戏模拟子系统。不得改名、合并、拆分或扩展固定清单；如果当前全量索引出现额外候选，只能记录为 boundary-challenge，交由最终整合会话处理。

## 任务元数据

- taskNumber: 19
- subsystemId: SpawnLifecycleAndLoot
- layer: authoritative-simulation
- currentNltxStatus: missing
- originalBoundary: 初始生命周期/掉落责任；与 NPC、Projectile、Item、DeathPenaltyAndRevenge 交界
- relatedSubsystems: NpcAndTownSimulation, ProjectileSimulation, ItemContainerAndEconomy, DeathPenaltyAndRevenge, WorldStorage, NetworkSessionAndSectionStreaming
- reportPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-05-version4-spawn-lifecycle-and-loot-public-decomposition.md

## 专属目标

围绕 SpawnLifecycleAndLoot，基于真实 Version4 代码确定权威状态、写入根、生命周期、调用方向和跨域提交边界，并提出可实现但不落地的 ECS 拆分设计。该子系统的责任是：跨域实体生成、清理、销毁、掉落发布和结构性结果提交。

“拆分为组件”只表示 proposed design。不得创建实际组件、System、Query、Command、Adapter、Projection、测试或项目文件。

## Version4 必须深读和重新定位的专属证据

- D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules\CommonDrop.cs：声明式掉落规则及其执行/报告接口，按实际行号重新定位。
- D:\TRbackup\Version4\Terraria\NPC.cs：NewNPC、CheckDead、DropEoWLoot/NPCLoot（约 :64726、:64747）和实体销毁路径。
- D:\TRbackup\Version4\Terraria\Player.cs:11984-11990 OnKillNPC；Projectile.cs:12514 等击杀结果入口；EquipmentLoadout.cs:54-64 物品掉落。
- MessageBuffer.cs:3100 等网络生成；WorldFile.cs：NPC/Item/TileEntity 保存；CoinLossRevengeSystem.cs:190：复仇 NPC spawn 交接。
- 完整参考 ItemDropRules/NPC/Projectile 只补证 Version4 已存在的明确删减，不扩展覆盖基线。

任务包中的路径和行号只是检索线索。必须重新读取文件、定位实际符号和调用上下文；报告使用实际行号，并标记 confirmed、partial、missing、unresolved、evidence-mismatch 或 version-drift。

## tModLoader 公开 API 交叉验证

- D:\TRbackup\tmodloader-api-docs-stable\class_global_n_p_c.html：NPC 生成、击杀和掉落公开扩展边界。
- class_mod_n_p_c.html、class_recipe.html、class_mod_packet.html：结果生成、物品和同步边界；实际页面、标题和锚点必须记录。

不得用 tModLoader 文档替代 Version4 私有行为；必须记录实际 HTML 文件、页面标题、文档版本、成员锚点和仅用于何种边界交叉验证。

## Space Station 14 最小相关 ECS 参考

只读检索下列最小相关关键词和目录：
- spawn、despawn、delete、cleanup、loot、drop、entity lifecycle
- entity factory、event/command、component/system、inventory/currency result、replication

只记录实际读取的 Space Station 14 文件、类型/方法和参考用途（Component 粒度、System/Query 边界、Event/Command、关系、网络或持久化）。禁止复制代码、命名、目录结构和领域语义；若无直接对应证据，明确写无直接对应证据。

## 当前 NLTX 必须浅读核对

- 当前 NLTX 没有统一 spawn/cleanup transaction 或 focused verifier；src/Npc、src/Projectile、src/Items 的局部模型不能替代跨域提交器。
- 必须把 declarative ItemDropRule 与生命周期协调器分开；若当前索引出现额外 Wiring/Mount 候选，不得改变固定 19 任务。

只能读取当前 src、Test、dome/src 和既有验证材料，不得修改；本轮并行审查不运行编译/测试命令，验证状态写为 not-run，历史真实输出只能写 existing-evidence。

## public-decomposition 专属问题

- 生成请求、实体分配、初始化、激活、销毁、清理和结果发布的唯一 owner。
- 掉落规则只计算候选结果，ItemContainerAndEconomy 是否负责物品/金币提交。
- NPC/Projectile spawn 与 DeathPenalty 重建、网络复制、WorldStorage 持久化的命令方向。
- 失败、重复请求、slot/ID 回收、部分结果提交和客户端投影如何保证幂等。
- 自然生成、事件生成、复仇重建和网络 spawn 是否需要不同策略但共享同一提交边界。

必须完成成员/字段/方法盘点、读者/写者/生命周期/副作用表、权威状态所有权表、访问模式分组、Component/System/Query/Command/Adapter/Projection 接口契约、System 顺序、跨域依赖、ECS ID/关系建模、不拆分项和行为保持风险。所有新类型、路径和签名均标记 status: proposed；跨子系统类型标记 crossSubsystemOwner: integration-review。

## 专属交付重点

- 提出 SpawnRequest、EntityLifecycleState、CleanupWork、LootDecision/Result、SpawnCommitSystem、DespawnSystem、LootCommitPort、Replication/Persistence Adapter 的 proposed 设计。
- 报告必须给出自然生成、NPC 死亡掉落、Projectile 销毁、复仇重建、网络 spawn 和 focused verifier 场景。

报告必须写入：
D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-05-version4-spawn-lifecycle-and-loot-public-decomposition.md

报告必须包含 Version4 事实、完整参考补证、tModLoader 交叉验证、Space Station 14 参考、当前 NLTX 状态、proposed 设计、not-run 验证、证据缺口、blocking-decision 和 Integration Handoff。报告只能使用中文说明与英文稳定 ID，不能宣称迁移完成、行为等价或 API 兼容。

## 明确不拆分的专属对象

- 单个 ItemDropRule、单个 NPC spawn、单个金币/物品掉落、单个实体 slot、单个网络消息、单个死亡 Hook。

请说明每个对象为什么只是实例、策略、规则、查询、缓存、Adapter 或 Projection，而不是一级子系统。

## 完成条件

完成只读证据审查并生成唯一指定报告后，在最终回复中简要说明报告路径、实际读取的主要证据、本次未运行构建/测试、evidence-gap、blocking-decision 以及未修改生产代码。
