# Version4 非权威组件拆分分区 P06：实体生命周期与归因 - public-decomposition 专属提示词

> 本文件只包含当前分区的专属任务内容，不复制公共提示词正文。
> 使用时：先提供公共提示词，再附加本文件；公共协议中的通用规则、证据门槛、报告结构、禁止项和 Integration Handoff 继续有效。

## 公共前置（仅引用）

- 公共提示词：`D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\design\2026-09-05-version4-public-decomposition-common-protocol.md`
- 必读技能：`D:\TRbackup\NLTX\.agents\skills\public-decomposition\SKILL.md`
- ECS 证据协议：`D:\TRbackup\NLTX\.agents\skills\public-decomposition\references\ecs-evidence-protocol.md`
- tModLoader 检索协议：`D:\TRbackup\NLTX\.agents\skills\public-decomposition\references\tmodloader-documentation-retrieval.md`
- 本文件不重复上述文件内容；执行前必须实际读取并遵守它们。
- 并行会话保障 skill：D:\\TRbackup\\NLTX\\.agents\\skills\\version4-non-authoritative-partition-session\\SKILL.md
- 并行领取/结算 runner：D:\\TRbackup\\NLTX\\Build\\Tools\\Invoke-Version4NonAuthoritativePartitionSession.ps1
- 必须先按 skill 使用 ClaimNext（或用户明确指定时使用 Claim -PartitionId P06），只写本文件的 outputReport，并用领取返回的同一 sessionId 完成 Complete/Fail/Abandon；不得手工编辑 ledger 或 lock。
- 本任务属于 20 个并行会话中的固定分区；currentTaskCount: 20。公共协议中遗留的 19 会话数量说明在本任务包中不适用。

## 任务元数据

- promptId: P06-non-authoritative-public-decomposition-20260911
- partitionId: P06
- inputPartitionReport: D:\TRbackup\NLTX\docs\migration\ledgers\non-authoritative-component-partitions\06-entity-lifecycle-attribution.md
- outputReport: D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-11-version4-non-authoritative-P06-entity-lifecycle-attribution-public-decomposition.md
- reportType: public-decomposition
- executionMode: independent-read-only-partition
- currentTaskCount: 20
- formalParentSubsystems: RuntimeComposition, SharedRuntimeMechanisms
- leafSubsystemCount: 3
- fieldCount: 43
- propertyCount: 1
- memberCount: 44
- sourceReportSha256: b944441d92d5103f218b766a3e354200794528a19e65a862e44c2b40a36f1196
- sourceInventoryStatus: source-inventory-confirmed; member-level owner/lifecycle/side-effect evidence still requires this session to close

## 专属范围锁定

- 本会话只审查输入分区报告中的 3 个叶子子系统和 44 条成员记录。以下清单是本会话唯一的成员范围；不得读取或复制其他 P 分区的成员清单。
- 细分子系统仍只是库存边界，不等价于一个组件；允许在本分区内部按访问模式拆成多个 proposed Component、System、Query、Command、Adapter 或 Projection。
- 来源序号、路径、行号和声明是检索线索；必须回到 Version4 重新定位，不得把输入报告直接当作已确认的读者、写者、生命周期或 owner 证据。

| 叶子子系统 | 父级 | 原章节 | 边界角色 | 字段 | 属性 | 成员 | 细分职责 |
|---|---|---|---|---:|---:|---:|---|
| `MainEntityPoolsAndWorldSlots` | `RuntimeComposition` | `4.1.25` | `runtime state` | 14 | 0 | 14 | 尘埃、星体、物品、NPC、投射物、容器和动画槽。 |
| `SharedEntitySourceAndAttribution` | `SharedRuntimeMechanisms` | `4.9.6` | `value object` | 22 | 1 | 23 | 实体来源链、死亡原因和归因上下文。 |
| `EntityIdentityAndMotionState` | `SharedRuntimeMechanisms` | `4.9.194` | `state` | 7 | 0 | 7 | 实体身份、位置、速度、方向和运动历史。 |

来源成员的分区内序号线索范围：376..3698；序号不定义领域边界，必须逐条回到输入表和 Version4 覆盖。

## 分区专属目标

围绕“实体生命周期与归因”完成独立只读的 Version4 成员审查和 ECS 拆分设计。
- 围绕 Main 的实体池/世界槽、实体身份/运动状态和 EntitySource/死亡归因链，核对生成、注册、激活、更新、死亡、回收和槽位复用的真实生命周期。
- 回到 Terraria/Main.cs、实体基类/身份与来源相关源码及直接生成/销毁调用者，确认数组/池是否为权威存储，索引、实体引用、持久化 ID 与网络 ID 是否被混用。
- 把实体池管理、身份/运动 Component、来源/归因 Context、Spawn/Despawn System、死亡事件和历史快照区分为不同角色。
- 核对归因信息向战斗、掉落、死亡惩罚、网络、持久化和诊断的单向传播，并记录回收、重试、重复销毁和引用失效规则。

## 专属证据焦点

- 先完整读取本分区输入报告的成员表，再回到 D:\TRbackup\Version4 重新核对字段、属性、声明类型、初始化/注册、直接读者、直接写者、清理路径和副作用。
- 下列路径只作为本分区的检索种子；路径存在不等于成员语义已确认，必须按公共证据协议记录实际行号、符号、调用者、读者、写者、生命周期和 evidenceStatus。
  - `D:\TRbackup\Version4\Terraria.DataStructures/AEntitySource_OnHit.cs`
  - `D:\TRbackup\Version4\Terraria.DataStructures/AEntitySource_Tile.cs`
  - `D:\TRbackup\Version4\Terraria.DataStructures/EntitySource_ByItemSourceId.cs`
  - `D:\TRbackup\Version4\Terraria.DataStructures/EntitySource_ByProjectileSourceId.cs`
  - `D:\TRbackup\Version4\Terraria.DataStructures/EntitySource_ItemUse_WithAmmo.cs`
  - `D:\TRbackup\Version4\Terraria.DataStructures/EntitySource_ItemUse.cs`
  - `D:\TRbackup\Version4\Terraria.DataStructures/EntitySource_Mount.cs`
  - `D:\TRbackup\Version4\Terraria.DataStructures/EntitySource_OverfullChest.cs`
  - `D:\TRbackup\Version4\Terraria.DataStructures/EntitySource_Parent.cs`
  - `D:\TRbackup\Version4\Terraria.DataStructures/EntitySource_TileInteraction.cs`
  - `D:\TRbackup\Version4\Terraria.DataStructures/PlayerDeathReason.cs`
  - `D:\TRbackup\Version4\Terraria/Entity.cs`
  - `D:\TRbackup\Version4\Terraria/Main.cs`

- 输入分区抽取出的声明类型焦点：
  - `Terraria.DataStructures.AEntitySource_OnHit`
  - `Terraria.DataStructures.AEntitySource_Tile`
  - `Terraria.DataStructures.EntitySource_ByItemSourceId`
  - `Terraria.DataStructures.EntitySource_ByProjectileSourceId`
  - `Terraria.DataStructures.EntitySource_ItemUse`
  - `Terraria.DataStructures.EntitySource_ItemUse_WithAmmo`
  - `Terraria.DataStructures.EntitySource_Mount`
  - `Terraria.DataStructures.EntitySource_OverfullChest`
  - `Terraria.DataStructures.EntitySource_Parent`
  - `Terraria.DataStructures.EntitySource_TileInteraction`
  - `Terraria.DataStructures.PlayerDeathReason`
  - `Terraria.Entity`
  - `Terraria.Main`

- 若路径、类型或行号漂移，记录 version-drift 或 evidence-mismatch；不得静默沿用库存报告行号。Version4 是 Terraria 私有实现的事实来源，完整源码只补证 Version4 已存在文件，tModLoader 只交叉验证公开边界，SS14 只参考结构粒度。

## 专属审查问题

- 实体槽位、激活标志、身份、位置/速度历史和来源上下文谁拥有写入权？槽位索引是否只是运行时句柄而非实体身份？
- 生成来源、伤害来源、死亡原因和归因快照的生命周期是否一致；哪些是事件 payload，哪些需要持久化或网络复制？
- 实体回收与池复用如何防止陈旧引用、重复提交和跨帧可见性错误？是否需要显式 Despawn/Recycle System 顺序？
- 与战斗、NPC、物品、投射物、死亡惩罚和网络会话共享的 EntityReference/Attribution 类型如何交给整合审查？

## 专属不拆分边界

- 不要把所有实体池、身份、运动历史、来源和死亡原因塞进一个 UniversalEntityComponent。
- 不要把数组索引、槽位、临时来源链、死亡事件或缓存引用误判为持久化实体 ID。
- 不要在本分区宣布跨域 EntityId、NetworkId、PersistentId 或 Attribution owner；统一写 crossSubsystemOwner: integration-review。

专属跨域提醒：重点记录与空间、战斗、NPC、物品、掉落、死亡惩罚、网络和持久化的 integration-risk；所有身份/来源共享候选仅提交 candidate owner。

## 输出要求

- 将完整研究报告只写入：D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-11-version4-non-authoritative-P06-entity-lifecycle-attribution-public-decomposition.md
- 不要写入输入分区报告、公共提示词、其他分区报告、源代码、测试、项目文件或迁移文件。
- 报告必须逐条覆盖本分区成员，给出成员归属表、状态所有权、Component/System/Query/Command/Adapter/Projection 边界、接口契约、依赖方向、顺序约束、不拆分项、兼容策略、evidence-gap、blocking-decision、focused verifier 计划和 Integration Handoff。
- 本轮只读审查不运行编译型命令；没有实际执行的 verifier 必须写 verificationStatus: not-run。
- proposed 类型、路径和接口必须明确标记 proposed；不得写成当前 NLTX 已存在或已经迁移。
- 跨两个或以上分区使用的类型、接口、ID、快照、值对象和 System 顺序只能提出候选，必须写 crossSubsystemOwner: integration-review，不得在本报告单方面裁决。
- 不得修改任何生产代码、测试、项目文件、生成器、公共协议、分区报告或其他并行会话输出。

## 分区完成检查

- [ ] 逐条覆盖本分区全部 44 条成员，成员事实来自输入表但关键语义已回到 Version4 复核。
- [ ] 只使用本分区叶子清单；没有复制、重新设计或宣布其他 P 分区成员。
- [ ] Component 只承载内聚权威状态；派生值、缓存、查询、短期 payload、日志、网络/存档/UI 输出按正确角色隔离。
- [ ] 所有共享候选写 crossSubsystemOwner: integration-review；没有把 proposed 设计写成已实现能力。
- [ ] 未执行的构建、测试和 verifier 均明确 verificationStatus: not-run；证据缺口、冲突和 blocking-decision 已单列。
- [ ] 输出文件路径唯一为：D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-11-version4-non-authoritative-P06-entity-lifecycle-attribution-public-decomposition.md

## 会话结束回报

最终消息只回报以下字段：
- partitionId: P06
- outputReport: D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-11-version4-non-authoritative-P06-entity-lifecycle-attribution-public-decomposition.md
- evidenceSourcesRead: 实际读取的 Version4 文件/类型、必要的完整源码补证、tModLoader 页面和 SS14 参考
- evidenceGaps: 仍缺失或冲突的关键证据
- blockingDecisions: 需要最终整合会话裁决的 owner/顺序/事务边界
- verificationStatus: not-run
- productionCodeModified: no

本提示词只定义本分区的专属范围和审查抓手；公共协议负责共同证据规则、报告结构、禁止项和 Integration Handoff。
