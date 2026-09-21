# Version4 非权威组件拆分分区 P07：战斗、伤害与状态效果 - public-decomposition 专属提示词

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
- 必须先按 skill 使用 ClaimNext（或用户明确指定时使用 Claim -PartitionId P07），只写本文件的 outputReport，并用领取返回的同一 sessionId 完成 Complete/Fail/Abandon；不得手工编辑 ledger 或 lock。
- 本任务属于 20 个并行会话中的固定分区；currentTaskCount: 20。公共协议中遗留的 19 会话数量说明在本任务包中不适用。

## 任务元数据

- promptId: P07-non-authoritative-public-decomposition-20260911
- partitionId: P07
- inputPartitionReport: D:\TRbackup\NLTX\docs\migration\ledgers\non-authoritative-component-partitions\07-combat-status-effects.md
- outputReport: D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-11-version4-non-authoritative-P07-combat-status-effects-public-decomposition.md
- reportType: public-decomposition
- executionMode: independent-read-only-partition
- currentTaskCount: 20
- formalParentSubsystems: SharedRuntimeMechanisms
- leafSubsystemCount: 6
- fieldCount: 91
- propertyCount: 6
- memberCount: 97
- sourceReportSha256: b944441d92d5103f218b766a3e354200794528a19e65a862e44c2b40a36f1196
- sourceInventoryStatus: source-inventory-confirmed; member-level owner/lifecycle/side-effect evidence still requires this session to close

## 专属范围锁定

- 本会话只审查输入分区报告中的 6 个叶子子系统和 97 条成员记录。以下清单是本会话唯一的成员范围；不得读取或复制其他 P 分区的成员清单。
- 细分子系统仍只是库存边界，不等价于一个组件；允许在本分区内部按访问模式拆成多个 proposed Component、System、Query、Command、Adapter 或 Projection。
- 来源序号、路径、行号和声明是检索线索；必须回到 Version4 重新定位，不得把输入报告直接当作已确认的读者、写者、生命周期或 owner 证据。

| 叶子子系统 | 父级 | 原章节 | 边界角色 | 字段 | 属性 | 成员 | 细分职责 |
|---|---|---|---|---:|---:|---:|---|
| `SharedCombatTargetingAndImmunity` | `SharedRuntimeMechanisms` | `4.9.34` | `query/state` | 15 | 3 | 18 | 目标、命中框、免疫和击杀尝试数据。 |
| `SharedHitTileTracking` | `SharedRuntimeMechanisms` | `4.9.36` | `state/query` | 17 | 0 | 17 | Tile 命中和挖掘追踪状态。 |
| `ItemTagEffectState` | `SharedRuntimeMechanisms` | `4.9.91` | `state/definition` | 13 | 1 | 14 | 鞭子标签效果、唯一标签效果和效果状态。 |
| `LockOnTargetingState` | `SharedRuntimeMechanisms` | `4.9.107` | `adapter/query` | 5 | 2 | 7 | 锁定范围、保持时间和目标选择状态。 |
| `SharedCombatTextState` | `SharedRuntimeMechanisms` | `4.9.119` | `presentation` | 23 | 0 | 23 | 伤害、治疗和暴击战斗文本状态。 |
| `SharedItemCombatAndDamageCapabilityState` | `SharedRuntimeMechanisms` | `4.9.125` | `definition/state` | 18 | 0 | 18 | 物品伤害、命中、恢复和职业伤害能力。 |

来源成员的分区内序号线索范围：1083..3870；序号不定义领域边界，必须逐条回到输入表和 Version4 覆盖。

## 分区专属目标

围绕“战斗、伤害与状态效果”完成独立只读的 Version4 成员审查和 ECS 拆分设计。
- 围绕目标选择/命中框/免疫、Tile 命中追踪、物品战斗能力、鞭子标签/唯一标签、锁定目标和战斗文本，核对战斗权威状态、资格 Query、提交 System 和表现 Projection。
- 回到相关 Item/Player/NPC/Projectile/CombatText/LockOn 源码及直接调用者，确认伤害、治疗、击杀、免疫、命中和效果状态的读者/写者及清理时机。
- 把目标/命中查询、伤害 Command、免疫 Component、状态效果 System、Tile hit 追踪、锁定输入和战斗文本输出分开设计。
- 核对服务端权威、客户端显示、网络同步、归因、掉落和死亡的边界；记录拒绝、重复命中、冷却、重试和系统顺序。

## 专属证据焦点

- 先完整读取本分区输入报告的成员表，再回到 D:\TRbackup\Version4 重新核对字段、属性、声明类型、初始化/注册、直接读者、直接写者、清理路径和副作用。
- 下列路径只作为本分区的检索种子；路径存在不等于成员语义已确认，必须按公共证据协议记录实际行号、符号、调用者、读者、写者、生命周期和 evidenceStatus。
  - `D:\TRbackup\Version4\Terraria.DataStructures/CachedProjectileCounterBuffTextHandler.cs`
  - `D:\TRbackup\Version4\Terraria.DataStructures/MultiPointHitbox.cs`
  - `D:\TRbackup\Version4\Terraria.DataStructures/NPCAimedTarget.cs`
  - `D:\TRbackup\Version4\Terraria.DataStructures/NPCDebuffImmunityData.cs`
  - `D:\TRbackup\Version4\Terraria.DataStructures/NPCKillAttempt.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Items/TagEffectState.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Items/UniqueTagEffect.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Items/WhipTagEffect.cs`
  - `D:\TRbackup\Version4\Terraria.GameInput/LockOnHelper.cs`
  - `D:\TRbackup\Version4\Terraria/CombatText.cs`
  - `D:\TRbackup\Version4\Terraria/HitTile.cs`
  - `D:\TRbackup\Version4\Terraria/Item.cs`

- 输入分区抽取出的声明类型焦点：
  - `Terraria.CombatText`
  - `Terraria.DataStructures.CachedProjectileCounterBuffTextHandler`
  - `Terraria.DataStructures.MultiPointHitbox`
  - `Terraria.DataStructures.NPCAimedTarget`
  - `Terraria.DataStructures.NPCDebuffImmunityData`
  - `Terraria.DataStructures.NPCKillAttempt`
  - `Terraria.GameContent.Items.TagEffectState`
  - `Terraria.GameContent.Items.UniqueTagEffect`
  - `Terraria.GameContent.Items.WhipTagEffect`
  - `Terraria.GameInput.LockOnHelper`
  - `Terraria.HitTile`
  - `Terraria.HitTile.HitTileObject`
  - `Terraria.Item`

- 若路径、类型或行号漂移，记录 version-drift 或 evidence-mismatch；不得静默沿用库存报告行号。Version4 是 Terraria 私有实现的事实来源，完整源码只补证 Version4 已存在文件，tModLoader 只交叉验证公开边界，SS14 只参考结构粒度。

## 专属审查问题

- 目标筛选、命中框、免疫和伤害结果分别是 Query、权威状态还是一次性事件？免疫生命周期与实体/效果生命周期是否一致？
- Item combat capability、鞭子标签、唯一效果、恢复和职业伤害是否共享写者，还是应拆为能力定义、实例状态和效果 System？
- Lock-on 的输入/目标缓存和 CombatText 的文本/动画状态是否应留在表现或输入边界，如何避免反向写战斗事实？
- 伤害、Tile 命中、死亡、掉落、网络快照和客户端文本之间的提交顺序与跨子系统 owner 如何证据化？

## 专属不拆分边界

- 不要把目标查询、免疫、伤害能力、状态效果、Tile 命中和战斗文本重新合成 CombatComponent。
- 不要把单次 HitResult、CombatText 条目、锁定候选、标签缓存或冷却计时误升格为长期权威状态。
- 不要以 UI 文本或客户端效果推断服务端伤害 owner；表现只能是单向 Projection。

专属跨域提醒：重点记录与实体归因、空间碰撞、玩家输入、物品、NPC、掉落、网络和 UI 的 integration-risk；伤害/效果/归因共享候选标 crossSubsystemOwner: integration-review。

## 输出要求

- 将完整研究报告只写入：D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-11-version4-non-authoritative-P07-combat-status-effects-public-decomposition.md
- 不要写入输入分区报告、公共提示词、其他分区报告、源代码、测试、项目文件或迁移文件。
- 报告必须逐条覆盖本分区成员，给出成员归属表、状态所有权、Component/System/Query/Command/Adapter/Projection 边界、接口契约、依赖方向、顺序约束、不拆分项、兼容策略、evidence-gap、blocking-decision、focused verifier 计划和 Integration Handoff。
- 本轮只读审查不运行编译型命令；没有实际执行的 verifier 必须写 verificationStatus: not-run。
- proposed 类型、路径和接口必须明确标记 proposed；不得写成当前 NLTX 已存在或已经迁移。
- 跨两个或以上分区使用的类型、接口、ID、快照、值对象和 System 顺序只能提出候选，必须写 crossSubsystemOwner: integration-review，不得在本报告单方面裁决。
- 不得修改任何生产代码、测试、项目文件、生成器、公共协议、分区报告或其他并行会话输出。

## 分区完成检查

- [ ] 逐条覆盖本分区全部 97 条成员，成员事实来自输入表但关键语义已回到 Version4 复核。
- [ ] 只使用本分区叶子清单；没有复制、重新设计或宣布其他 P 分区成员。
- [ ] Component 只承载内聚权威状态；派生值、缓存、查询、短期 payload、日志、网络/存档/UI 输出按正确角色隔离。
- [ ] 所有共享候选写 crossSubsystemOwner: integration-review；没有把 proposed 设计写成已实现能力。
- [ ] 未执行的构建、测试和 verifier 均明确 verificationStatus: not-run；证据缺口、冲突和 blocking-decision 已单列。
- [ ] 输出文件路径唯一为：D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-11-version4-non-authoritative-P07-combat-status-effects-public-decomposition.md

## 会话结束回报

最终消息只回报以下字段：
- partitionId: P07
- outputReport: D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-11-version4-non-authoritative-P07-combat-status-effects-public-decomposition.md
- evidenceSourcesRead: 实际读取的 Version4 文件/类型、必要的完整源码补证、tModLoader 页面和 SS14 参考
- evidenceGaps: 仍缺失或冲突的关键证据
- blockingDecisions: 需要最终整合会话裁决的 owner/顺序/事务边界
- verificationStatus: not-run
- productionCodeModified: no

本提示词只定义本分区的专属范围和审查抓手；公共协议负责共同证据规则、报告结构、禁止项和 Integration Handoff。
