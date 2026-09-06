# 07 — WorldProgressionAndUnlocks 子系统审查与 ECS 拆分设计

> subsystemId: WorldProgressionAndUnlocks  
> taskNumber: 07  
> evidenceStatus: partial  
> nltxStatus: partial  
> verificationStatus: not-run

## 1. 执行摘要

本报告只审查固定任务 WorldProgressionAndUnlocks，不扩展 19 个子系统清单。Version4 证据显示该责任面具有自己的状态写集、生命周期或跨域提交边界；但它不能因此被解释为已经迁移到 NLTX。报告中的所有 ECS 名称、路径和接口均为 proposed design。

当前边界：从内容/图鉴表现元数据中提升。任务包的旧状态为 partial，当前结论为 partial。本轮没有运行构建、测试或任何 compile-capable 命令，不能声称行为等价、迁移完成或 API 兼容。

## 2. 范围和不负责内容

负责本任务所列的权威事实、独立生命周期、资格/规则计算、显式 Command、受控提交、持久化和网络投影边界。不负责相邻子系统的最终 owner、共享基础类型、客户端 UI 事实、平台接口和未被 Version4 当前文件直接证明的完整实现。

相邻系统只能通过只读 view、Query、Command 或 Adapter 交接。跨两个以上子系统使用的 ID、快照和 commit port 标记 crossSubsystemOwner: integration-review。

## 3. 证据来源和角色

| 来源 | 证据 | 角色 | 状态 |
| --- | --- | --- | --- |
| Version4 | Terraria.GameContent\Bestiary\BestiaryUnlocksTracker.cs:5-56；NPCKillsTracker.cs:23-46；Terraria\Main.cs:1023、3355、14001-14004、14167-14170；Terraria.IO\WorldFile.cs | 真实字段、方法、调用链和副作用 | confirmed / partial |
| 完整可编译参考 | 仅在与 Version4 同路径、同签名的删减处补证；本任务未用补证替代 Version4 | 补证，不扩展基线 | partial |
| tModLoader | D:\TRbackup\tmodloader-api-docs-stable，v2026.07；任务包指定公开 API 页面 | 公开生命周期/网络/扩展边界交叉验证 | partial |
| Space Station 14 | 本任务没有找到与 Terraria 私有责任面直接对应的行为证据 | 仅保留 ECS 粒度参考 | missing |

tModLoader 页面不能证明 Version4 私有算法；Space Station 14 无直接对应证据，以下边界仅由 Version4 真实代码和 NLTX 项目约束决定。

## 4. Version4 真实代码事实

1. BestiaryUnlocksTracker.cs:5-56 具备 tracker 初始化、保存、加载、重置、校验和玩家同步路径，具有持久化生命周期。
2. NPCKillsTracker.cs:23-46 记录击杀事实并更新 tracker；这不是 UI-only 计算。
3. Main.cs:1023 持有 tracker，:3355 初始化；:14001-14004、:14167-14170 将进度用于动物学家/城镇 NPC 资格。
4. WorldFile 的 Bestiary/世界进度路径与平台 Achievement/Social 投影必须分离；平台接口不拥有权威 unlock fact。

### 4.1 事实调用方向

```text
upstream input/fact
  -> Version4 owner state / eligibility calculation
  -> lifecycle or progression commit
  -> downstream entity/world mutation
  -> persistence/network/client projection
```

实际读者：NpcAndTownSimulation、WorldCalendarAndEventOrchestration、WorldSession、ContentCatalog、PersistenceAndRecovery、NetworkSession。实际写者候选：NPC kill/event result、world progression commit；客户端和平台只能投影。confirmed 只证明当前成员和调用点，不证明理想化的单写者架构。

## 5. 成员、读者、写者和生命周期

| 状态/行为组 | 归类 | 读者 | 写者 | 生命周期 | 副作用 | 状态 |
| --- | --- | --- | --- | --- | --- | --- |
| 核心事实/实例状态 | 权威状态候选 | 本责任面和下游只读系统 | Version4 owner 与少数跨域调用者 | 初始化、Tick、恢复、卸载 | 世界/实体状态 | partial |
| 资格/规则计算 | Query 或策略 | owner system、下游 | 无权威写入 | 请求/边界 | 应保持纯、确定 | partial |
| 生命周期/事务状态 | 行为状态 | coordinator、commit system | 单一协调器候选 | 请求到完成/失败/重试 | spawn/despawn/save/net | partial |
| 网络/存档值 | Adapter 输入/Projection | 外部边界 | Adapter | 加载、保存、同步 | I/O、wire bytes | partial |
| UI/平台结果 | Projection | 客户端/平台 | 外部投影 | 事件或帧 | UI/平台副作用 | excluded |

失败、重复请求、幂等和部分提交不能隐藏在 Query 中，必须由 System/Command/Adapter 返回可观察结果。

## 6. proposed ECS 边界

以下均为 status: proposed，不是当前已存在实现：

- proposed UnlockFactComponent，status: proposed；目标路径仅为设计提示
- proposed ProgressionAggregate，status: proposed；目标路径仅为设计提示
- proposed UnlockEvent，status: proposed；目标路径仅为设计提示
- proposed UnlockEligibilityQuery，status: proposed；目标路径仅为设计提示
- proposed ProgressionCommitSystem，status: proposed；目标路径仅为设计提示
- proposed UnlockPersistenceAdapter，status: proposed；目标路径仅为设计提示
- proposed UnlockReplicationProjection，status: proposed；目标路径仅为设计提示
- proposed AchievementClientProjection，status: proposed；目标路径仅为设计提示

最小契约：

- Component 只保存一个内聚概念的权威数据，不放网络 DTO、日志、时钟或 UI。
- Query 只读快照并返回资格/规则结果，不修改状态。
- System 读取输入并产出明确状态转换或 Command；失败原因、重试次数和幂等键可观察。
- Commit port 是唯一候选写入口；最终共享 owner 交由 integration-review。
- Adapter 负责 Version4 binary/network/外部类型转换；Projection 只输出不可变视图。

| 边界 | Interface | Implementation | Seam | Depth / Leverage / Locality |
| --- | --- | --- | --- | --- |
| 资格 | proposed IWorldProgressionAndUnlocksEligibilityQuery | 纯字段/快照计算 | fake read view | medium / high / domain |
| 生命周期 | proposed IWorldProgressionAndUnlocksLifecycleSystem | 显式状态机 | command recorder | deep / high / domain |
| 提交 | proposed IWorldProgressionAndUnlocksCommitPort | owner adapter | atomic result | deep / very high / cross-domain |
| 网络/存档 | proposed WorldProgressionAndUnlocksReplicationAdapter / PersistenceAdapter | 不可变快照编解码 | in-memory stream/golden bytes | medium / high / adapter |
| 客户端 | proposed WorldProgressionAndUnlocksProjection | 只读视图 | fake snapshot | shallow / medium / client |

## 7. 调用方向和 System 顺序

建议顺序（均为 proposed，不是 Version4 已实现顺序）：

```text
input/load/death/world boundary
  -> validation and eligibility Query
  -> state/lifecycle System
  -> Command construction
  -> WorldProgressionAndUnlocksCommitSystem
  -> downstream entity/world result
  -> persistence/replication Projection
```

本任务的交接顺序为：kill/observation fact -> deduplicated UnlockEvent -> progression commit -> NPC eligibility query -> save/join snapshot -> platform projection。失败时保留旧权威状态；只有 commit port 成功才产生持久化/网络快照。Retry 必须有最大次数、幂等键和诊断结果。

## 8. 持久化、网络和客户端边界

Persistence Adapter（status: proposed）只接受已提交 snapshot，保留 Version4 字段顺序、版本分支、坏数据拒绝和失败不覆盖旧状态语义。Replication Projection（status: proposed）只从 committed revision 生成不可变消息；入站先转为权限和版本验证过的 Command，客户端不能直接写权威状态。客户端/平台 Projection（status: proposed）可以落后，但不能成为 Simulation owner。

关键缺口：坏数据校验细节、玩家加入同步完整路径、世界级/玩家级 unlock owner、重复击杀和版本迁移语义。

## 9. 当前 NLTX 状态

当前树存在 src/Content、NPC progression/definition 与 dome 的世界进度局部模型，但未见经本轮验证的 Bestiary unlock aggregate；判为 partial。

当前可作为边界证据的路径：dome/src/Terraria.Dome.Simulation/World/WorldProgressionState.cs、NPC qualification 系统和内容目录可提供只读交接；不能声称已迁移 BestiaryTracker。

这些路径最多证明局部类型/命令/策略存在，不证明完整 Version4 调用链、主运行时接线或行为等价。不能用目录顺序决定执行顺序。

## 10. focused verifier 计划

下列 verifier 均为 status: proposed，本轮不创建、不运行：

| verifier ID | 目标 | 核心断言 |
| --- | --- | --- |
| WORLDPROGRESSIONANDUNLOCKS-STATE | 权威状态 | 只有指定 owner 能写；旧值、revision 和重复命令可审计 |
| WORLDPROGRESSIONANDUNLOCKS-ELIGIBILITY | 纯资格 Query | 相同快照相同结果；Query 不写状态 |
| WORLDPROGRESSIONANDUNLOCKS-LIFECYCLE | 生命周期 | 成功、失败、过期、取消、重复请求和恢复状态明确 |
| WORLDPROGRESSIONANDUNLOCKS-COMMIT | 受控提交 | 非法输入整批拒绝，不发生隐式部分成功 |
| WORLDPROGRESSIONANDUNLOCKS-PERSISTENCE | 存档恢复 | 版本、坏数据、回滚和旧状态保留 |
| WORLDPROGRESSIONANDUNLOCKS-REPLICATION | 网络投影 | 只从 committed snapshot 发送；入站无权限不能写 |
| WORLDPROGRESSIONANDUNLOCKS-CROSS | 跨域顺序 | kill/observation fact -> deduplicated UnlockEvent -> progression commit -> NPC eligibility query -> save/join snapshot -> platform projection 可由 trace recorder 观察 |

未来实施时才允许按 AGENTS.md 通过串行包装器运行受影响 verifier；本轮未运行 dotnet，因此没有 exit code、warning/error count 或 artifact path。

## 11. 不拆分项

- 单个 unlock fact：只是实例、规则条目、Hook、缓存、Adapter 或 Projection，不拥有完整状态、生命周期和跨域提交边界。
- 单个 NPC 击杀计数：只是实例、规则条目、Hook、缓存、Adapter 或 Projection，不拥有完整状态、生命周期和跨域提交边界。
- 单个 Bestiary UI 条目：只是实例、规则条目、Hook、缓存、Adapter 或 Projection，不拥有完整状态、生命周期和跨域提交边界。
- 单个平台 achievement：只是实例、规则条目、Hook、缓存、Adapter 或 Projection，不拥有完整状态、生命周期和跨域提交边界。
- 单个资格 bool：只是实例、规则条目、Hook、缓存、Adapter 或 Projection，不拥有完整状态、生命周期和跨域提交边界。

## 12. 兼容策略、风险和未决事项

兼容策略：保留 Version4 字段/序列化顺序和调用时点；先建立只读 legacy facade，再迁移单一写集；以 golden bytes、状态机 trace 和错误边界锁定行为；分别建模 EntityId、PersistentEntityId、NetworkId、Player/Npc ID、WorldSectionId 和外部 SessionId。

主要风险：

- RISK-07-WORLDPROGRESSIONANDUNLOCKS-DOUBLE-WRITE：旧 writer 与 proposed owner 并存，导致状态漂移。
- RISK-07-WORLDPROGRESSIONANDUNLOCKS-ORDER：系统顺序改变，使下游在不同阶段观察事实。
- RISK-07-WORLDPROGRESSIONANDUNLOCKS-PROJECTION：网络/存档/UI 反向写入权威状态。
- RISK-07-WORLDPROGRESSIONANDUNLOCKS-PARTIAL：只迁移局部模型却宣称完整 Version4 行为。

evidence-gap：坏数据校验细节、玩家加入同步完整路径、世界级/玩家级 unlock owner、重复击杀和版本迁移语义。

blocking-decision：

- BD-WPU-01 unlock 是玩家状态、世界状态还是两级 aggregate；BD-WPU-02 NPC 资格 read view owner；BD-WPU-03 platform result 是否允许反馈。
- 这些问题会改变权威 owner、事务边界或必须保持的 System 顺序，本报告不替代最终整合裁决。

## 13. Integration Handoff

```text
subsystemId: WorldProgressionAndUnlocks
taskNumber: 07
reportPath: D:\TRbackup\NLTX\docs\research\2026-09-05-version4-world-progression-and-unlocks-public-decomposition.md

evidenceStatus: partial
nltxStatus: partial
verificationStatus: not-run

confirmedOwners:
- Version4 中由上述真实成员/调用点直接证明的核心事实和生命周期入口；最终 ECS owner 仅在没有跨域冲突时可候选确认。
- Terraria.GameContent\Bestiary\BestiaryUnlocksTracker.cs:5-56；NPCKillsTracker.cs:23-46；Terraria\Main.cs:1023、3355、14001-14004、14167-14170；Terraria.IO\WorldFile.cs

proposedTypes:
- proposed UnlockFactComponent, status: proposed
- proposed ProgressionAggregate, status: proposed
- proposed UnlockEvent, status: proposed
- proposed UnlockEligibilityQuery, status: proposed
- proposed ProgressionCommitSystem, status: proposed
- proposed UnlockPersistenceAdapter, status: proposed
- proposed UnlockReplicationProjection, status: proposed
- proposed AchievementClientProjection, status: proposed

sharedTypesForIntegrationReview:
- EntityId、PersistentEntityId、NetworkId、WorldSectionId、WorldTickSnapshot、WorldProgressionAndUnlocksResult、WorldProgressionAndUnlocksCommitPort；candidate owner 未裁决。

crossSubsystemReaders:
- NpcAndTownSimulation、WorldCalendarAndEventOrchestration、WorldSession、ContentCatalog、PersistenceAndRecovery、NetworkSession

crossSubsystemWriters:
- NPC kill/event result、world progression commit；客户端和平台只能投影

orderingConstraints:
- kill/observation fact -> deduplicated UnlockEvent -> progression commit -> NPC eligibility query -> save/join snapshot -> platform projection
- commit 成功后才允许 persistence/replication projection。

boundaryChallenges:
- 从内容/图鉴表现元数据中提升
- 任务包状态与当前局部模型可能存在 version-drift；不能从局部文件推断完整迁移。

evidenceGaps:
- 坏数据校验细节、玩家加入同步完整路径、世界级/玩家级 unlock owner、重复击杀和版本迁移语义

blockingDecisions:
- BD-WPU-01 unlock 是玩家状态、世界状态还是两级 aggregate；BD-WPU-02 NPC 资格 read view owner；BD-WPU-03 platform result 是否允许反馈

notImplemented:
- 本报告未创建或修改 Component、System、Query、Command、Adapter、Projection、测试或项目文件。
- 本报告不声明当前 NLTX 行为等价或 API 兼容。

verifierPlan:
- WORLDPROGRESSIONANDUNLOCKS-STATE、ELIGIBILITY、LIFECYCLE、COMMIT、PERSISTENCE、REPLICATION、CROSS，全部 status: proposed；本轮 verificationStatus: not-run。
```

## 14. 最终声明

本报告是基于 Version4、完整参考源码、tModLoader 公开文档和有限 ECS 结构参考形成的子系统边界与 ECS 拆分设计。它不是迁移完成报告，不是行为等价证明，不是 API 兼容证明，也不是当前 NLTX 已实现能力的声明。

本报告只写入任务指定的唯一报告文件，未修改生产代码、测试代码、dome/src、Version4、完整参考源码、tModLoader 文档或其他共享审查材料。本轮未运行构建或测试，验证状态为 not-run。
