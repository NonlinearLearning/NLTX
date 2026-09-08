# 16 — CombatAndStatus 子系统审查与 ECS 拆分设计

> subsystemId: CombatAndStatus  
> taskNumber: 16  
> evidenceStatus: partial  
> nltxStatus: confirmed  
> verificationStatus: not-run

## 1. 执行摘要

本报告只审查固定任务 CombatAndStatus，不扩展 19 个子系统清单。Version4 证据显示该责任面具有自己的状态写集、生命周期或跨域提交边界；但它不能因此被解释为已经迁移到 NLTX。报告中的 ECS 名称、路径和接口均为 proposed design。

当前边界：初始战斗责任；DeathPenaltyAndRevenge 只接收死亡/金币结果。本轮没有运行构建、测试或任何 compile-capable 命令，不能声称行为等价、迁移完成或 API 兼容。

## 2. 范围和不负责内容

负责本任务所列的权威事实、独立生命周期、资格/规则计算、显式 Command、受控提交、持久化和网络投影边界。不负责相邻子系统的最终 owner、共享基础类型、客户端 UI 事实、平台接口和未被 Version4 当前文件直接证明的完整实现。相邻系统只能通过只读 view、Query、Command 或 Adapter 交接；共享 ID、快照和 commit port 标记 crossSubsystemOwner: integration-review。

## 3. 证据来源和角色

| 来源 | 证据 | 角色 | 状态 |
| --- | --- | --- | --- |
| Version4 | Terraria\Player.cs:22572、生命/免疫/伤害字段；NPC.cs StrikeNPC/CheckDead；Projectile.cs 命中/穿透；Collision.cs CanHit；MessageBuffer.cs:1315、1420；当前 src/Combat 与 Test/Terraria.Combat.Verification | 真实字段、方法、调用链和副作用 | confirmed / partial |
| 完整可编译参考 | 仅在同路径、同签名的删减处补证；不替代 Version4 | 补证，不扩展基线 | partial |
| tModLoader | D:\TRbackup\tmodloader-api-docs-stable，v2026.07；任务包指定公开 API 页面 | 公开生命周期/网络/扩展边界 | partial |
| Space Station 14 | 没有找到与 Terraria 私有责任面直接对应的行为证据 | 仅 ECS 粒度参考 | missing |

tModLoader 不能证明 Version4 私有算法；Space Station 14 无直接对应证据，以下边界仅由 Version4 真实代码和 NLTX 项目约束决定。

## 4. Version4 真实代码事实

1. Player.KillMe 在 Player.cs:22572 形成玩家死亡结算入口；生命、免疫和伤害字段由 Player/Combat 逻辑读取和更新。
2. NPC StrikeNPC/CheckDead、Projectile hit/penetration 和 MessageBuffer:1315、1420 网络伤害入口形成多来源 DamageRequest，但最终 health/death writer 必须收口。
3. Collision.CanHit/CanHitWithCheck 只提供空间资格；状态效果、免疫、死亡结果和 replication/presentation 应分离。
4. 当前 NLTX 已有 src/Combat 的 DamageRequest、DamageResolutionSystem、Health/Immunity 等模型和 Combat verifier 源码；静态覆盖可标 confirmed，但本轮验证状态仍是 not-run。

### 4.1 事实调用方向

```text
upstream input/fact
  -> owner state / eligibility calculation
  -> lifecycle or domain commit
  -> downstream entity/world mutation
  -> persistence/network/client projection
```

主要读者：PlayerGameplay、NpcAndTownSimulation、ProjectileSimulation、DeathPenaltyAndRevenge、SpawnLifecycleAndLoot、SpatialSimulation。主要写者候选：DamageResolutionSystem owns damage/health result；death/loot/revenge consume event；projection read-only。confirmed 只证明当前成员和调用点，不证明理想化的单写者架构。

## 5. 成员、读者、写者和生命周期

| 状态/行为组 | 归类 | 读者 | 写者 | 生命周期 | 副作用 | 状态 |
| --- | --- | --- | --- | --- | --- | --- |
| 核心事实/实例状态 | 权威状态候选 | 本责任面和下游只读系统 | Version4 owner 与跨域调用者 | 初始化、Tick、恢复、卸载 | 世界/实体状态 | partial |
| 资格/规则计算 | Query/策略 | owner system、下游 | 无权威写入 | 请求/边界 | 应保持纯、确定 | partial |
| 生命周期/事务状态 | 行为状态 | coordinator、commit system | 单一协调器候选 | 请求到完成/失败/重试 | 实体/世界/网络 | partial |
| 网络/存档值 | Adapter 输入/Projection | 外部边界 | Adapter | 加载、保存、同步 | I/O、wire bytes | partial |
| UI/平台结果 | Projection | 客户端/平台 | 外部投影 | 事件或帧 | UI/平台副作用 | excluded |

失败、重复请求、幂等和部分提交不能隐藏在 Query 中，必须由 System/Command/Adapter 返回可观察结果。

## 6. proposed ECS 边界

以下均为 status: proposed，不是当前已存在实现：

- proposed DamageRequest，status: proposed；目标路径仅为设计提示
- proposed DamageEligibilityQuery，status: proposed；目标路径仅为设计提示
- proposed DamageResolutionSystem，status: proposed；目标路径仅为设计提示
- proposed HealthComponent，status: proposed；目标路径仅为设计提示
- proposed ImmunityComponent，status: proposed；目标路径仅为设计提示
- proposed StatusEffectStateComponent，status: proposed；目标路径仅为设计提示
- proposed DeathResultEvent，status: proposed；目标路径仅为设计提示
- proposed CombatReplicationProjection，status: proposed；目标路径仅为设计提示

契约要求：

- Component 只保存一个内聚概念的权威数据，不放网络 DTO、日志、时钟或 UI。
- Query 只读快照并返回资格/规则结果，不修改状态。
- System 读取输入并产出状态转换或 Command；失败原因、重试次数和幂等键可观察。
- Commit port 是唯一候选写入口，最终共享 owner 交由 integration-review。
- Adapter 负责 Version4 binary/network/外部类型转换；Projection 只输出不可变视图。

| 边界 | Interface | Implementation | Seam | Depth / Leverage / Locality |
| --- | --- | --- | --- | --- |
| 资格 | proposed ICombatAndStatusEligibilityQuery | 纯字段/快照计算 | fake read view | medium / high / domain |
| 生命周期 | proposed ICombatAndStatusLifecycleSystem | 显式状态机 | command recorder | deep / high / domain |
| 提交 | proposed ICombatAndStatusCommitPort | owner adapter | atomic result | deep / very high / cross-domain |
| 网络/存档 | proposed CombatAndStatusReplicationAdapter / PersistenceAdapter | 不可变快照编解码 | in-memory stream/golden bytes | medium / high / adapter |
| 客户端 | proposed CombatAndStatusProjection | 只读视图 | fake snapshot | shallow / medium / client |

## 7. 调用方向和 System 顺序

建议顺序（均为 proposed，不是 Version4 已实现顺序）：

```text
input/load/world/entity boundary
  -> validation and eligibility Query
  -> state/lifecycle System
  -> Command construction
  -> CombatAndStatusCommitSystem
  -> downstream result
  -> persistence/replication Projection
```

本任务的交接顺序为：input/hit -> eligibility -> damage calculation -> immunity/status -> health commit -> death result -> loot/revenge handoff -> replication/presentation。失败时保留旧权威状态；只有 commit port 成功才产生持久化/网络快照。Retry 必须有最大次数、幂等键和诊断结果。

## 8. 持久化、网络和客户端边界

Persistence Adapter（status: proposed）只接受已提交 snapshot，保留 Version4 字段顺序、版本分支、坏数据拒绝和失败不覆盖旧状态语义。Replication Projection（status: proposed）只从 committed revision 生成不可变消息；入站先转为权限和版本验证过的 Command，客户端不能直接写权威状态。客户端 Projection（status: proposed）可以落后，但不能成为 Simulation owner。

关键缺口：Version4 全部 damage source、RNG/crit 时点、status effect persistence、网络权限、Combat verifier 本轮未执行。

## 9. 当前 NLTX 状态

当前 NLTX Combat 边界有较完整组件、系统和 verifier 源码，按任务包/既有索引为 confirmed；不把本轮未运行误写成 independently-verified。

当前可作为边界证据的路径：src/Combat、src/StatusEffects、dome/src/Terraria.Dome.Simulation/Combat/ 与 Test/Terraria.Combat.Verification。

这些路径最多证明局部类型/命令/策略存在，不证明完整 Version4 调用链、主运行时接线或行为等价。不能用目录顺序决定执行顺序。

## 10. focused verifier 计划

下列 verifier 均为 status: proposed，本轮不创建、不运行：

| verifier ID | 目标 | 核心断言 |
| --- | --- | --- |
| COMBATANDSTATUS-STATE | 权威状态 | 只有指定 owner 能写；旧值、revision 和重复命令可审计 |
| COMBATANDSTATUS-ELIGIBILITY | 纯资格 Query | 相同快照相同结果；Query 不写状态 |
| COMBATANDSTATUS-LIFECYCLE | 生命周期 | 成功、失败、过期、取消、重复请求和恢复状态明确 |
| COMBATANDSTATUS-COMMIT | 受控提交 | 非法输入整批拒绝，不发生隐式部分成功 |
| COMBATANDSTATUS-PERSISTENCE | 存档恢复 | 版本、坏数据、回滚和旧状态保留 |
| COMBATANDSTATUS-REPLICATION | 网络投影 | 只从 committed snapshot 发送；入站无权限不能写 |
| COMBATANDSTATUS-CROSS | 跨域顺序 | input/hit -> eligibility -> damage calculation -> immunity/status -> health commit -> death result -> loot/revenge handoff -> replication/presentation 可由 trace recorder 观察 |

未来实施时才允许按 AGENTS.md 通过串行包装器运行受影响 verifier；本轮未运行 dotnet，因此没有 exit code、warning/error count 或 artifact path。

## 11. 不拆分项

- 单个伤害类型：只是实例、规则条目、Hook、缓存、Adapter 或 Projection，不拥有完整状态、生命周期和跨域提交边界。
- 单个 buff/debuff：只是实例、规则条目、Hook、缓存、Adapter 或 Projection，不拥有完整状态、生命周期和跨域提交边界。
- 单个免疫 flag：只是实例、规则条目、Hook、缓存、Adapter 或 Projection，不拥有完整状态、生命周期和跨域提交边界。
- 单个 hit packet：只是实例、规则条目、Hook、缓存、Adapter 或 Projection，不拥有完整状态、生命周期和跨域提交边界。
- 单个 death hook：只是实例、规则条目、Hook、缓存、Adapter 或 Projection，不拥有完整状态、生命周期和跨域提交边界。
- 单个碰撞资格查询：只是实例、规则条目、Hook、缓存、Adapter 或 Projection，不拥有完整状态、生命周期和跨域提交边界。

## 12. 兼容策略、风险和未决事项

兼容策略：保留 Version4 字段/序列化顺序和调用时点；先建立只读 legacy facade，再迁移单一写集；以 golden bytes、状态机 trace 和错误边界锁定行为；分别建模 EntityId、PersistentEntityId、NetworkId、Player/Npc ID、WorldSectionId 和外部 SessionId。

主要风险：

- RISK-16-COMBATANDSTATUS-DOUBLE-WRITE：旧 writer 与 proposed owner 并存，导致状态漂移。
- RISK-16-COMBATANDSTATUS-ORDER：系统顺序改变，使下游在不同阶段观察事实。
- RISK-16-COMBATANDSTATUS-PROJECTION：网络/存档/UI 反向写入权威状态。
- RISK-16-COMBATANDSTATUS-PARTIAL：只迁移局部模型却宣称完整 Version4 行为。

evidence-gap：Version4 全部 damage source、RNG/crit 时点、status effect persistence、网络权限、Combat verifier 本轮未执行。

blocking-decision：

- BD-CS-01 health/death owner；BD-CS-02 status effect writer；BD-CS-03 damage result 与 loot/revenge 的 event schema。
- 这些问题会改变权威 owner、事务边界或必须保持的 System 顺序，本报告不替代最终整合裁决。

## 13. Integration Handoff

```text
subsystemId: CombatAndStatus
taskNumber: 16
reportPath: D:\TRbackup\NLTX\docs\第一轮审查\2026-09-05-version4-combat-and-status-public-decomposition.md

evidenceStatus: partial
nltxStatus: confirmed
verificationStatus: not-run

confirmedOwners:
- Version4 中由上述真实成员/调用点直接证明的核心事实和生命周期入口；最终 ECS owner 仅在没有跨域冲突时可候选确认。
- Terraria\Player.cs:22572、生命/免疫/伤害字段；NPC.cs StrikeNPC/CheckDead；Projectile.cs 命中/穿透；Collision.cs CanHit；MessageBuffer.cs:1315、1420；当前 src/Combat 与 Test/Terraria.Combat.Verification

proposedTypes:
- proposed DamageRequest, status: proposed
- proposed DamageEligibilityQuery, status: proposed
- proposed DamageResolutionSystem, status: proposed
- proposed HealthComponent, status: proposed
- proposed ImmunityComponent, status: proposed
- proposed StatusEffectStateComponent, status: proposed
- proposed DeathResultEvent, status: proposed
- proposed CombatReplicationProjection, status: proposed

sharedTypesForIntegrationReview:
- EntityId、PersistentEntityId、NetworkId、WorldSectionId、WorldTickSnapshot、CombatAndStatusResult、CombatAndStatusCommitPort；candidate owner 未裁决。

crossSubsystemReaders:
- PlayerGameplay、NpcAndTownSimulation、ProjectileSimulation、DeathPenaltyAndRevenge、SpawnLifecycleAndLoot、SpatialSimulation

crossSubsystemWriters:
- DamageResolutionSystem owns damage/health result；death/loot/revenge consume event；projection read-only

orderingConstraints:
- input/hit -> eligibility -> damage calculation -> immunity/status -> health commit -> death result -> loot/revenge handoff -> replication/presentation
- commit 成功后才允许 persistence/replication projection。

boundaryChallenges:
- 初始战斗责任；DeathPenaltyAndRevenge 只接收死亡/金币结果
- 任务包状态与当前局部模型可能存在 version-drift；不能从局部文件推断完整迁移。

evidenceGaps:
- Version4 全部 damage source、RNG/crit 时点、status effect persistence、网络权限、Combat verifier 本轮未执行

blockingDecisions:
- BD-CS-01 health/death owner；BD-CS-02 status effect writer；BD-CS-03 damage result 与 loot/revenge 的 event schema

notImplemented:
- 本报告未创建或修改 Component、System、Query、Command、Adapter、Projection、测试或项目文件。
- 本报告不声明当前 NLTX 行为等价或 API 兼容。

verifierPlan:
- COMBATANDSTATUS-STATE、ELIGIBILITY、LIFECYCLE、COMMIT、PERSISTENCE、REPLICATION、CROSS，全部 status: proposed；本轮 verificationStatus: not-run。
```

## 14. 最终声明

本报告是基于 Version4、完整参考源码、tModLoader 公开文档和有限 ECS 结构参考形成的子系统边界与 ECS 拆分设计。它不是迁移完成报告，不是行为等价证明，不是 API 兼容证明，也不是当前 NLTX 已实现能力的声明。

本报告只写入任务指定的唯一报告文件，未修改生产代码、测试代码、dome/src、Version4、完整参考源码、tModLoader 文档或其他共享审查材料。本轮未运行构建或测试，验证状态为 not-run。
