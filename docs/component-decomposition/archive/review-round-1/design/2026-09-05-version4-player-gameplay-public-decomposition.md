# 11 — PlayerGameplay 独立只读审查与 ECS 设计提示词

本文件必须与以下公共协议共同使用，开始任务前先完整读取该文件：
D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\design\2026-09-05-version4-public-decomposition-common-protocol.md

公共协议中的只读、并行、证据优先级、行号复核、状态枚举、事实/设计隔离、ECS 文件组织、副作用隔离、报告结构和 Integration Handoff 规则全部适用于本任务。若本任务包与公共协议冲突，以公共协议为准。

本任务属于已经固定的 19 个游戏模拟子系统。不得改名、合并、拆分或扩展固定清单；如果当前全量索引出现额外候选，只能记录为 boundary-challenge，交由最终整合会话处理。

## 任务元数据

- taskNumber: 11
- subsystemId: PlayerGameplay
- layer: authoritative-simulation
- currentNltxStatus: partial
- originalBoundary: 初始玩家聚合责任；CombatAndStatus、ItemContainerAndEconomy、SpatialSimulation、ProjectileSimulation 等与其交接
- relatedSubsystems: CombatAndStatus, ItemContainerAndEconomy, SpatialSimulation, ProjectileSimulation, WorldSession, IntentAndInteraction
- reportPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-05-version4-player-gameplay-public-decomposition.md

## 专属目标

围绕 PlayerGameplay，基于真实 Version4 代码确定权威状态、写入根、生命周期、调用方向和跨域提交边界，并提出可实现但不落地的 ECS 拆分设计。该子系统的责任是：玩家资源、装备、物品使用、生命、能力、实体生命周期和玩家状态转换。

“拆分为组件”只表示 proposed design。不得创建实际组件、System、Query、Command、Adapter、Projection、测试或项目文件。

## Version4 必须深读和重新定位的专属证据

- D:\TRbackup\Version4\Terraria\Player.cs：Update（约 :156）、Update(int)（约 :14789）、ItemCheckWrapped（:19603）、KillMe（:22572）、OnKillNPC（:11988）以及玩家字段。
- D:\TRbackup\Version4\Terraria\Main.cs:11420-11472：玩家在世界 Tick 中的更新位置；MessageBuffer.cs：入站输入写入玩家状态。
- Player.cs 中生命/死亡、装备、背包、物品使用、坐骑、钓鱼、召唤、重生和移动状态的读写者。
- D:\TRbackup\Version4\Terraria.IO\PlayerFileData.cs、WorldFile.cs：玩家文件和与世界状态交互的存档路径。

任务包中的路径和行号只是检索线索。必须重新读取文件、定位实际符号和调用上下文；报告使用实际行号，并标记 confirmed、partial、missing、unresolved、evidence-mismatch 或 version-drift。

## tModLoader 公开 API 交叉验证

- D:\TRbackup\tmodloader-api-docs-stable\class_mod_player.html：公开玩家生命周期、伤害、物品、输入和更新扩展；必须从本地索引重新定位实际标题、版本和成员锚点。
- 按公共协议检索 ModPlayer.PreKill、ModifyHurt、PostUpdate、SaveData/LoadData 等页面，只作公开边界交叉验证。

不得用 tModLoader 文档替代 Version4 私有行为；必须记录实际 HTML 文件、页面标题、文档版本、成员锚点和仅用于何种边界交叉验证。

## Space Station 14 最小相关 ECS 参考

只读检索下列最小相关关键词和目录：
- player entity、player session、input intent、equipment、inventory、health
- component composition、system/query、session/entity relation、network state、persistence

只记录实际读取的 Space Station 14 文件、类型/方法和参考用途（Component 粒度、System/Query 边界、Event/Command、关系、网络或持久化）。禁止复制代码、命名、目录结构和领域语义；若无直接对应证据，明确写无直接对应证据。

## 当前 NLTX 必须浅读核对

- 当前 NLTX src/Player 已有身份、生命、装备、背包、物品使用等多个状态组件，但没有完整 orchestrated execution chain。
- 检查 src/Player、src/Physics、src/Items、src/Combat、Test 和 dome/src；不得把其他子系统状态重新合并进 PlayerGameplay。

只能读取当前 src、Test、dome/src 和既有验证材料，不得修改；本轮并行审查不运行编译/测试命令，验证状态写为 not-run，历史真实输出只能写 existing-evidence。

## public-decomposition 专属问题

- 玩家实体身份、输入意图、生命/恢复、装备/背包、能力和移动状态如何按访问模式拆分。
- 死亡事实由 CombatAndStatus 提交还是 PlayerGameplay 拥有生命周期结果；金币、钓鱼、传送和投射物如何通过命令交接。
- 玩家实体 ID、持久化玩家 ID、网络/Session ID 如何分离并标记 integration-review。
- Update、ItemCheck、KillMe 等巨型方法如何分解为 System，同时保留权威写入顺序和副作用隔离。
- 哪些玩家字段是权威状态、派生缓存、兼容字段或客户端 Projection。

必须完成成员/字段/方法盘点、读者/写者/生命周期/副作用表、权威状态所有权表、访问模式分组、Component/System/Query/Command/Adapter/Projection 接口契约、System 顺序、跨域依赖、ECS ID/关系建模、不拆分项和行为保持风险。所有新类型、路径和签名均标记 status: proposed；跨子系统类型标记 crossSubsystemOwner: integration-review。

## 专属交付重点

- 提出 PlayerIdentity、PlayerLifecycle、PlayerVital、PlayerEquipment、PlayerInventory、PlayerUse、PlayerAbility、InputIntent 等 proposed 组件及 System/Query/Command 边界。
- 报告必须给出玩家更新、输入验证、移动/战斗/物品交接、死亡、重生和存档投影的 focused verifier 设计。

报告必须写入：
D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-05-version4-player-gameplay-public-decomposition.md

报告必须包含 Version4 事实、完整参考补证、tModLoader 交叉验证、Space Station 14 参考、当前 NLTX 状态、proposed 设计、not-run 验证、证据缺口、blocking-decision 和 Integration Handoff。报告只能使用中文说明与英文稳定 ID，不能宣称迁移完成、行为等价或 API 兼容。

## 明确不拆分的专属对象

- 单个玩家字段、单个 Buff、单个按键、单个 UI 控件、单个网络 packet、完整 Player 巨型组件。

请说明每个对象为什么只是实例、策略、规则、查询、缓存、Adapter 或 Projection，而不是一级子系统。

## 完成条件

完成只读证据审查并生成唯一指定报告后，在最终回复中简要说明报告路径、实际读取的主要证据、本次未运行构建/测试、evidence-gap、blocking-decision 以及未修改生产代码。
