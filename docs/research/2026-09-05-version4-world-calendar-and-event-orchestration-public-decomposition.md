# 05 — WorldCalendarAndEventOrchestration 子系统审查与 ECS 拆分设计

> subsystemId: WorldCalendarAndEventOrchestration  
> taskNumber: 05  
> evidenceStatus: partial  
> nltxStatus: partial  
> verificationStatus: not-run

## 1. 执行摘要

本报告只审查固定任务 WorldCalendarAndEventOrchestration，不扩展 19 个子系统清单。Version4 证据显示该责任面具有自己的状态写集、生命周期或跨域提交边界；但它不能因此被解释为已经迁移到 NLTX。报告中的所有 ECS 名称、路径和接口均为 proposed design。

当前边界：从 WorldSession 的一般时间更新中提升。任务包的旧状态为 partial，当前结论为 partial。本轮没有运行构建、测试或任何 compile-capable 命令，不能声称行为等价、迁移完成或 API 兼容。

## 2. 范围和不负责内容

负责本任务所列的权威事实、独立生命周期、资格/规则计算、显式 Command、受控提交、持久化和网络投影边界。不负责相邻子系统的最终 owner、共享基础类型、客户端 UI 事实、平台接口和未被 Version4 当前文件直接证明的完整实现。

相邻系统只能通过只读 view、Query、Command 或 Adapter 交接。跨两个以上子系统使用的 ID、快照和 commit port 标记 crossSubsystemOwner: integration-review。

## 3. 证据来源和角色

| 来源 | 证据 | 角色 | 状态 |
| --- | --- | --- | --- |
| Version4 | Terraria\Main.cs:12050、12972、13112-13118、13470-13564；Terraria\WorldFile.cs；Terraria\NetMessage.cs；Invasion/LanternNight/BirthdayParty 等事件调用点 | 真实字段、方法、调用链和副作用 | confirmed / partial |
| 完整可编译参考 | 仅在与 Version4 同路径、同签名的删减处补证；本任务未用补证替代 Version4 | 补证，不扩展基线 | partial |
| tModLoader | D:\TRbackup\tmodloader-api-docs-stable，v2026.07；任务包指定公开 API 页面 | 公开生命周期/网络/扩展边界交叉验证 | partial |
| Space Station 14 | 本任务没有找到与 Terraria 私有责任面直接对应的行为证据 | 仅保留 ECS 粒度参考 | missing |

tModLoader 页面不能证明 Version4 私有算法；Space Station 14 无直接对应证据，以下边界仅由 Version4 真实代码和 NLTX 项目约束决定。

## 4. Version4 真实代码事实

1. Main.UpdateTime 在 :12972 更新世界时间，并在 :13112-13118 进入事件相关更新；:13470-13564 处理昼夜边界、任务轮换、入侵/活动评估和城镇后果。
2. UpdateWeather 位于 Main.cs:12050，但天气事实与日历事件触发存在时序交接，不能把 WeatherState 直接等同 EventState。
3. 事件启动、结束、入侵进度和 NPC/城镇结果由多个 Main/NPC/WorldGen 路径消费；网络/存档是结果投影而非规则 owner。
4. WorldSession 拥有基础时钟事实的候选写集；Calendar 负责边界资格、事件实例和事件提交，最终 owner 仍需整合。

### 4.1 事实调用方向

```text
upstream input/fact
  -> Version4 owner state / eligibility calculation
  -> lifecycle or progression commit
  -> downstream entity/world mutation
  -> persistence/network/client projection
```

实际读者：WorldSession、NpcAndTownSimulation、SpawnLifecycleAndLoot、WorldGenerationAndEcology、WorldProgressionAndUnlocks、SimulationRuleOverrides。实际写者候选：WorldSession 提供时间；Calendar 唯一提交事件实例/边界；下游只消费 Command 或只读视图。confirmed 只证明当前成员和调用点，不证明理想化的单写者架构。

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

- proposed CalendarClockState，status: proposed；目标路径仅为设计提示
- proposed EventInstanceState，status: proposed；目标路径仅为设计提示
- proposed EventScheduleState，status: proposed；目标路径仅为设计提示
- proposed EventEligibilityQuery，status: proposed；目标路径仅为设计提示
- proposed EventCommitCommand，status: proposed；目标路径仅为设计提示
- proposed EventScheduleSystem，status: proposed；目标路径仅为设计提示
- proposed EventBoundarySystem，status: proposed；目标路径仅为设计提示
- proposed AnnouncementProjection，status: proposed；目标路径仅为设计提示
- proposed EventReplicationAdapter，status: proposed；目标路径仅为设计提示

最小契约：

- Component 只保存一个内聚概念的权威数据，不放网络 DTO、日志、时钟或 UI。
- Query 只读快照并返回资格/规则结果，不修改状态。
- System 读取输入并产出明确状态转换或 Command；失败原因、重试次数和幂等键可观察。
- Commit port 是唯一候选写入口；最终共享 owner 交由 integration-review。
- Adapter 负责 Version4 binary/network/外部类型转换；Projection 只输出不可变视图。

| 边界 | Interface | Implementation | Seam | Depth / Leverage / Locality |
| --- | --- | --- | --- | --- |
| 资格 | proposed IWorldCalendarAndEventOrchestrationEligibilityQuery | 纯字段/快照计算 | fake read view | medium / high / domain |
| 生命周期 | proposed IWorldCalendarAndEventOrchestrationLifecycleSystem | 显式状态机 | command recorder | deep / high / domain |
| 提交 | proposed IWorldCalendarAndEventOrchestrationCommitPort | owner adapter | atomic result | deep / very high / cross-domain |
| 网络/存档 | proposed WorldCalendarAndEventOrchestrationReplicationAdapter / PersistenceAdapter | 不可变快照编解码 | in-memory stream/golden bytes | medium / high / adapter |
| 客户端 | proposed WorldCalendarAndEventOrchestrationProjection | 只读视图 | fake snapshot | shallow / medium / client |

## 7. 调用方向和 System 顺序

建议顺序（均为 proposed，不是 Version4 已实现顺序）：

```text
input/load/death/world boundary
  -> validation and eligibility Query
  -> state/lifecycle System
  -> Command construction
  -> WorldCalendarAndEventOrchestrationCommitSystem
  -> downstream entity/world result
  -> persistence/replication Projection
```

本任务的交接顺序为：WorldSession clock snapshot -> day/night boundary query -> event eligibility -> event commit -> NPC/spawn/progression commands -> world/network announcement projection。失败时保留旧权威状态；只有 commit port 成功才产生持久化/网络快照。Retry 必须有最大次数、幂等键和诊断结果。

## 8. 持久化、网络和客户端边界

Persistence Adapter（status: proposed）只接受已提交 snapshot，保留 Version4 字段顺序、版本分支、坏数据拒绝和失败不覆盖旧状态语义。Replication Projection（status: proposed）只从 committed revision 生成不可变消息；入站先转为权限和版本验证过的 Command，客户端不能直接写权威状态。客户端/平台 Projection（status: proposed）可以落后，但不能成为 Simulation owner。

关键缺口：全部事件的启动/结束调用图、事件字段存档与网络恢复、事件与 Hardmode/天气 override 的 writer 竞争。

## 9. 当前 NLTX 状态

当前 NLTX 有 WorldSession、WorldProgression 和 NPC event/spawn 局部模型，但没有经本轮验证的统一 CalendarClock/EventSchedule 提交根；判为 partial。

当前可作为边界证据的路径：dome/src/Terraria.Dome.Simulation/World/WorldProgressionState.cs、World/Systems/WorldProgressionSystem.cs、NPC event/spawn 系统可作为交接证据；不把它们改写成 Calendar 已完成。

这些路径最多证明局部类型/命令/策略存在，不证明完整 Version4 调用链、主运行时接线或行为等价。不能用目录顺序决定执行顺序。

## 10. focused verifier 计划

下列 verifier 均为 status: proposed，本轮不创建、不运行：

| verifier ID | 目标 | 核心断言 |
| --- | --- | --- |
| WORLDCALENDARANDEVENTORCHESTRATION-STATE | 权威状态 | 只有指定 owner 能写；旧值、revision 和重复命令可审计 |
| WORLDCALENDARANDEVENTORCHESTRATION-ELIGIBILITY | 纯资格 Query | 相同快照相同结果；Query 不写状态 |
| WORLDCALENDARANDEVENTORCHESTRATION-LIFECYCLE | 生命周期 | 成功、失败、过期、取消、重复请求和恢复状态明确 |
| WORLDCALENDARANDEVENTORCHESTRATION-COMMIT | 受控提交 | 非法输入整批拒绝，不发生隐式部分成功 |
| WORLDCALENDARANDEVENTORCHESTRATION-PERSISTENCE | 存档恢复 | 版本、坏数据、回滚和旧状态保留 |
| WORLDCALENDARANDEVENTORCHESTRATION-REPLICATION | 网络投影 | 只从 committed snapshot 发送；入站无权限不能写 |
| WORLDCALENDARANDEVENTORCHESTRATION-CROSS | 跨域顺序 | WorldSession clock snapshot -> day/night boundary query -> event eligibility -> event commit -> NPC/spawn/progression commands -> world/network announcement projection 可由 trace recorder 观察 |

未来实施时才允许按 AGENTS.md 通过串行包装器运行受影响 verifier；本轮未运行 dotnet，因此没有 exit code、warning/error count 或 artifact path。

## 11. 不拆分项

- 单个节日/入侵：只是实例、规则条目、Hook、缓存、Adapter 或 Projection，不拥有完整状态、生命周期和跨域提交边界。
- 单个昼夜边界：只是实例、规则条目、Hook、缓存、Adapter 或 Projection，不拥有完整状态、生命周期和跨域提交边界。
- 单个 announcement：只是实例、规则条目、Hook、缓存、Adapter 或 Projection，不拥有完整状态、生命周期和跨域提交边界。
- 单个 NPC 生成策略：只是实例、规则条目、Hook、缓存、Adapter 或 Projection，不拥有完整状态、生命周期和跨域提交边界。
- 单个事件布尔值和单个天气效果：只是实例、规则条目、Hook、缓存、Adapter 或 Projection，不拥有完整状态、生命周期和跨域提交边界。

## 12. 兼容策略、风险和未决事项

兼容策略：保留 Version4 字段/序列化顺序和调用时点；先建立只读 legacy facade，再迁移单一写集；以 golden bytes、状态机 trace 和错误边界锁定行为；分别建模 EntityId、PersistentEntityId、NetworkId、Player/Npc ID、WorldSectionId 和外部 SessionId。

主要风险：

- RISK-05-WORLDCALENDARANDEVENTORCHESTRATION-DOUBLE-WRITE：旧 writer 与 proposed owner 并存，导致状态漂移。
- RISK-05-WORLDCALENDARANDEVENTORCHESTRATION-ORDER：系统顺序改变，使下游在不同阶段观察事实。
- RISK-05-WORLDCALENDARANDEVENTORCHESTRATION-PROJECTION：网络/存档/UI 反向写入权威状态。
- RISK-05-WORLDCALENDARANDEVENTORCHESTRATION-PARTIAL：只迁移局部模型却宣称完整 Version4 行为。

evidence-gap：全部事件的启动/结束调用图、事件字段存档与网络恢复、事件与 Hardmode/天气 override 的 writer 竞争。

blocking-decision：

- BD-WCE-01 时间提交槽位；BD-WCE-02 事件实例 owner 与 NPC spawn owner；BD-WCE-03 事件快照是否跨域聚合。
- 这些问题会改变权威 owner、事务边界或必须保持的 System 顺序，本报告不替代最终整合裁决。

## 13. Integration Handoff

```text
subsystemId: WorldCalendarAndEventOrchestration
taskNumber: 05
reportPath: D:\TRbackup\NLTX\docs\research\2026-09-05-version4-world-calendar-and-event-orchestration-public-decomposition.md

evidenceStatus: partial
nltxStatus: partial
verificationStatus: not-run

confirmedOwners:
- Version4 中由上述真实成员/调用点直接证明的核心事实和生命周期入口；最终 ECS owner 仅在没有跨域冲突时可候选确认。
- Terraria\Main.cs:12050、12972、13112-13118、13470-13564；Terraria\WorldFile.cs；Terraria\NetMessage.cs；Invasion/LanternNight/BirthdayParty 等事件调用点

proposedTypes:
- proposed CalendarClockState, status: proposed
- proposed EventInstanceState, status: proposed
- proposed EventScheduleState, status: proposed
- proposed EventEligibilityQuery, status: proposed
- proposed EventCommitCommand, status: proposed
- proposed EventScheduleSystem, status: proposed
- proposed EventBoundarySystem, status: proposed
- proposed AnnouncementProjection, status: proposed
- proposed EventReplicationAdapter, status: proposed

sharedTypesForIntegrationReview:
- EntityId、PersistentEntityId、NetworkId、WorldSectionId、WorldTickSnapshot、WorldCalendarAndEventOrchestrationResult、WorldCalendarAndEventOrchestrationCommitPort；candidate owner 未裁决。

crossSubsystemReaders:
- WorldSession、NpcAndTownSimulation、SpawnLifecycleAndLoot、WorldGenerationAndEcology、WorldProgressionAndUnlocks、SimulationRuleOverrides

crossSubsystemWriters:
- WorldSession 提供时间；Calendar 唯一提交事件实例/边界；下游只消费 Command 或只读视图

orderingConstraints:
- WorldSession clock snapshot -> day/night boundary query -> event eligibility -> event commit -> NPC/spawn/progression commands -> world/network announcement projection
- commit 成功后才允许 persistence/replication projection。

boundaryChallenges:
- 从 WorldSession 的一般时间更新中提升
- 任务包状态与当前局部模型可能存在 version-drift；不能从局部文件推断完整迁移。

evidenceGaps:
- 全部事件的启动/结束调用图、事件字段存档与网络恢复、事件与 Hardmode/天气 override 的 writer 竞争

blockingDecisions:
- BD-WCE-01 时间提交槽位；BD-WCE-02 事件实例 owner 与 NPC spawn owner；BD-WCE-03 事件快照是否跨域聚合

notImplemented:
- 本报告未创建或修改 Component、System、Query、Command、Adapter、Projection、测试或项目文件。
- 本报告不声明当前 NLTX 行为等价或 API 兼容。

verifierPlan:
- WORLDCALENDARANDEVENTORCHESTRATION-STATE、ELIGIBILITY、LIFECYCLE、COMMIT、PERSISTENCE、REPLICATION、CROSS，全部 status: proposed；本轮 verificationStatus: not-run。
```

## 14. 最终声明

本报告是基于 Version4、完整参考源码、tModLoader 公开文档和有限 ECS 结构参考形成的子系统边界与 ECS 拆分设计。它不是迁移完成报告，不是行为等价证明，不是 API 兼容证明，也不是当前 NLTX 已实现能力的声明。

本报告只写入任务指定的唯一报告文件，未修改生产代码、测试代码、dome/src、Version4、完整参考源码、tModLoader 文档或其他共享审查材料。本轮未运行构建或测试，验证状态为 not-run。
