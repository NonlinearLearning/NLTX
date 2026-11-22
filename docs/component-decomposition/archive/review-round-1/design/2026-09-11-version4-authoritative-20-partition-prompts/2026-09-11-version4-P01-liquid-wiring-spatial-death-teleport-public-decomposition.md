# P01 基础模拟、液体、机关、空间、死亡惩罚与传送 - public-decomposition 分区专属提示词

> 本文件只包含当前分区的专属任务内容，不复制公共提示词正文。
> 使用时：先提供公共提示词，再附加本文件；公共协议中的通用规则、证据门槛、报告结构、禁止项和交接格式继续有效。

## 公共前置（仅引用）

- 公共提示词：`D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\design\2026-09-05-version4-public-decomposition-common-protocol.md`
- 必读技能：`D:\TRbackup\NLTX\.agents\skills\public-decomposition\SKILL.md`
- ECS 证据协议：`D:\TRbackup\NLTX\.agents\skills\public-decomposition\references\ecs-evidence-protocol.md`
- tModLoader 检索协议：`D:\TRbackup\NLTX\.agents\skills\public-decomposition\references\tmodloader-documentation-retrieval.md`
- 本文件不重复上述文件内容；执行前必须实际读取并遵守它们。
- 本任务包覆盖权威报告的 P01-P20 共 20 个并行分区；本文件的 `currentTaskCount: 20`、分区范围和唯一输出路径优先于公共协议中遗留的 19 会话数量说明。

## 任务元数据

- promptId: P01-public-decomposition-20260911
- partitionId: P01
- inputPartitionReport: D:\TRbackup\NLTX\docs\migration\ledgers/Version4权威模拟系统字段属性逐成员源码声明-20分区\P01-Liquid-Wiring-Spatial-Death-Teleport.md
- outputReport: D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-11-version4-P01-liquid-wiring-spatial-death-teleport-public-decomposition.md
- reportType: public-decomposition
- executionMode: independent-read-only-partition
- currentTaskCount: 20
- formalParentSubsystems: LiquidSimulation, WiringAndMechanisms, SpatialSimulation, DeathPenaltyAndRevenge, TeleportationAndTraversal
- leafSubsystemCount: 14
- fieldCount: 111
- propertyCount: 2
- memberCount: 113

## 专属范围锁定

本会话只审查输入分区报告中的 14 个叶子子系统。以下清单是本会话唯一的成员范围；不得读取或复制作其他 P 分区的成员清单：
- `LiquidFlowBudgetAndPanicState`
- `LiquidCellWorkItemState`
- `LiquidBufferQueueState`
- `LiquidChangePublication`
- `WiringPropagationAndGateState`
- `WiringTeleportAndPumpState`
- `WiringMechanismCooldowns`
- `CollisionQueryCache`
- `CollisionContactAndHurtResults`
- `RevengeMarkerExpirationAndIdentityState`
- `RevengeMarkerEnemyContextState`
- `RevengeMarkerValueAndRespawnState`
- `RevengeRegistryAndCache`
- `TeleportPylonRegistry`

这是多父级聚合分区；报告必须分别保持 LiquidSimulation、WiringAndMechanisms、SpatialSimulation、DeathPenaltyAndRevenge、TeleportationAndTraversal 的责任边界，不因同属基础模拟而合并。

跨分区发现必须在报告中写成 `cross-subsystem finding`、`integration-risk` 或 `crossSubsystemOwner: integration-review`，不得把相邻分区成员重新纳入本分区。

## 分区专属目标

围绕“基础模拟、液体、机关、空间、死亡惩罚与传送”完成独立只读的 Version4 成员审查和 ECS 拆分设计。先读取输入分区报告中的完整成员表，再回到 Version4 实际源码核对关键字段、属性、方法、初始化路径、读写者、写入者、生命周期和副作用；所有结论必须区分源码事实、当前 NLTX 状态、proposed 设计和验证结果。

本任务的输出是研究报告，不是运行时实现；报告必须保持公共提示词规定的英文稳定 ID、证据状态、Integration Handoff 和未完成声明。

## 专属证据焦点

- D:\TRbackup\Version4\Terraria\Liquid.cs：液体预算、工作项、缓冲和变更发布的真实读写与 Tick 生命周期。
- D:\TRbackup\Version4\Terraria\Wiring.cs 及其直接调用者：电线传播、机关门、泵和传送的提交边界。
- D:\TRbackup\Version4\Terraria\Collision.cs、Tile/WorldGen 相关调用：碰撞查询缓存、接触和受伤结果的权威性。
- D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs：复仇标记、金币惩罚、过期和注册缓存。
- Version4 中传送水晶塔/旅行资格的实际类型、注册、索引、网络和世界加载调用；不得由类型名猜路径。
- Main.cs、WorldGen.cs、MessageBuffer.cs、NetMessage.cs 中与上述领域的 Tick、清理、网络和存档交接。

如果专属焦点中的路径、类型或历史行号发生漂移，必须按公共协议重新定位并记录 `version-drift` 或 `evidence-mismatch`，不得静默沿用旧行号。

## 专属审查问题

- Liquid 的工作队列、预算计数、停滞/panic 状态是否同生命周期，哪些只是短生命周期 Command payload。
- Wiring 的传播状态、机关冷却、泵/传送状态是否有唯一写入根，是否会与 WorldInteraction 或 Teleportation 争夺 owner。
- Collision 查询缓存与接触/受伤结果如何区分权威结果、派生查询和失效缓存。
- 死亡惩罚复仇标记的实体引用、持久化标识、网络标识和注册缓存如何分离。
- 液体、机关、空间、复仇和传送之间哪些关系必须交由 integration-review。

## 专属不拆分边界

- 单个液体工作项、单根电线、单个机关、单个碰撞结果、单个复仇标记、单个水晶塔注册项。
- 不要把 Liquid、Wiring、Collision、DeathPenalty 或 Teleportation 重新合成一个基础组件。

## 输出要求

将完整研究报告只写入：`D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-11-version4-P01-liquid-wiring-spatial-death-teleport-public-decomposition.md`
- 不要写入输入分区报告、公共提示词、其他分区报告、源代码、测试、项目文件或迁移文件。
- 报告必须逐条覆盖本分区成员，给出成员归属表、状态所有权、组件/System/Query/Command/Adapter/Projection 边界、接口契约、依赖方向、顺序约束、不拆分项、兼容策略、evidence-gap、blocking-decision、focused verifier 计划和 Integration Handoff。
- 本轮只读审查不运行编译型命令；没有实际执行的 verifier 必须写 `verificationStatus: not-run`。
- 完成前检查：本报告只包含本分区组名；没有复制其他 P 分区成员；没有把 proposed 类型写成已存在实现；没有宣布跨分区 owner。

## 会话执行结束条件

输出文件存在且只由本会话写入后，在最终消息中报告：P01、输出路径、实际读取的 Version4 证据、关键 evidence-gap、blocking-decision、erificationStatus，以及未修改生产代码。
