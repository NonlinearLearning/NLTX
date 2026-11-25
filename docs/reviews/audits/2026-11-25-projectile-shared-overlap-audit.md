# Projectile 域与共享系统的职责重叠审计

| 项 | 值 |
| --- | --- |
| 文档 ID | `AUDIT-2026-11-25-PROJECTILE-SHARED-OVERLAP` |
| 状态 | `open`（只出审计与判定，未改代码） |
| 触发 | 用户质疑：`ProjectileAiAdapter` 这类「数据融合模型」的职责已被共享系统解决，Projectile 下还有多少同类 |
| 范围 | `src/NSSLC/Projectile/**` 全部 60 个文件，对照 `src/NSSLC/Share/**` 九个共享域 |
| 判据 | 「被共享系统解决」= 共享域已提供同职责的类型/协议，且 Projectile 侧的实现没有超出该协议的额外语义 |

## 1. 结论摘要

Projectile 域 60 个文件中，**7 处**存在「共享域已提供等价能力，Projectile 侧另行实现」的重叠；其中 **3 处**是直接重复（可删或可并入），**2 处**是算法重复（可抽共享），**2 处**是第二真源（必须收敛）。

| # | 模型 | 重叠的共享能力 | 判定 |
| --- | --- | --- | --- |
| 1 | `ProjectileAiSnapshotProvider` | `IAiSnapshotProvider<T>` + `INpcAiSnapshotContributor` 组合模式 | 直接重复 |
| 2 | `ProjectileTargetAcquisitionSystem` | `NpcAiTargetSnapshotContributor.FindNearestFriendlyTarget` | 算法重复 |
| 3 | `ProjectileBehaviorCatalog` + `ProjectileBehaviorProfile` | `AiBehaviorCatalog<TIntent>` + `AiBehaviorDeclaration<TIntent>` | 直接重复 |
| 4 | `ProjectileRuntimeRegistry._terminated` | `ProjectileDispositionStateComponent.Terminated` | 第二真源 |
| 5 | `ProjectileHitAdmissionResult` | `DamageRejectionReason`（`TargetImmune`/`SourceUnavailable`/`SourceInactive`） | 重复错误码 |
| 6 | `ProjectileSpawnRequests` | 无（迁移期兼容入口，理由已消失） | 过期残留 |
| 7 | `ProjectileAiAdapter.AttachedLastUpdate` | 无（零读者） | 死值 |

## 2. 逐项证据

### 2.1 `ProjectileAiSnapshotProvider` —— 共享投影协议的第二个实现

**共享侧**：`Share/AI/Query/IAiSnapshotProvider.cs` 定义协议；`Share/AI/System/AiRequirements.cs` 用 `Components` 声明行为要读什么；NPC 侧把投影拆成 4 个 `INpcAiSnapshotContributor`（movement / target / life / world），由 `NpcAiSnapshotContributors.CreateDefault` 组装。

**Projectile 侧**：`Projectile/System/ProjectileAiSnapshotProvider.cs:35-66` 在一个方法里内联四个 `if (requiredComponents.Contains(...))` 分支，等价于 NPC 的四个 contributor 压平成一个类。

**差异**：Projectile 版多了 `_resolver.TryResolve` 解析目标句柄、以及「目标死亡/隐身则不写事实」的降级逻辑（`:71-83`）。这部分是**领域特有的**，共享协议不覆盖。

**判定**：投影**机制**重复，投影**语义**不重复。处置方向不是删除，而是把「按 `requiredComponents` 分派」这层换成共享的 contributor 组装形态，让 Projectile 只贡献自己的两个 fact 类型（`ProjectileTargetFact`、`ProjectileHomingCapabilityComponent`）。

### 2.2 `ProjectileTargetAcquisitionSystem` —— 与 NPC 目标选择同算法

**NPC 侧**：`NpcAiSnapshotContributors.cs:112-151`，Query `EntityFactionComponent + LifeComponent + LocationComponent`，过滤自身/阵营/存活，`Vector2.DistanceSquared` 取最近，同时算可见性。

**Projectile 侧**：`ProjectileTargetAcquisitionSystem.cs:98-150`，同样的 Query 形态、同样的距离平方比较、同样的阵营与存活过滤，额外排除 owner 句柄。

**差异**：阵营判据相反（NPC 找 `Faction.Friendly`，投射物找「不等于我这边的」）；Projectile 写 `ProjectileTargetComponent` 组件，NPC 只投影 fact 不写组件。

**判定**：「最近敌对/友方实体」这个空间查询算法在仓里存在两份。可抽一个共享的 `NearestEntitySelector`（输入：原点、半径、阵营判据、排除句柄；输出：候选），两侧只保留自己的判据与落点。

### 2.3 `ProjectileBehaviorCatalog` / `ProjectileBehaviorProfile` —— 与共享行为目录同构

**共享侧**：`AiBehaviorCatalog<TIntent>` 按 `Type` 键、`Seal()` 后拒绝新增、重复注册抛异常；`AiBehaviorDeclaration<TIntent>` 承载 `BehaviorType` + 工厂 + `Reads`。

**Projectile 侧**：`ProjectileBehaviorCatalog` 按 `AiBehaviorProfileId` 键、`Seal()` 后拒绝、重复注册抛异常、额外做「同一 BehaviorId 绑定不同 BehaviorType 则拒绝」（`:28-36`）；`ProjectileBehaviorProfile` 承载 `BehaviorId`/`ProfileId`/`BehaviorType`/`Requirements`/`MaximumSpeed`/`TurnRate`/`Range`。

**重叠面**：`BehaviorType` 与 `Requirements` 已被 `AiBehaviorDeclaration` 承载；`Seal` + 重复拒绝的目录语义已被 `AiBehaviorCatalog` 承载。

**不重叠面**：`MaximumSpeed`/`TurnRate`/`Range` 是**内容参数**，共享域没有对应物；profile 维度的键（同一行为多套参数）也是共享目录没有的。

**判定**：目录**机制**重复，参数**内容**不重复。处置方向：目录并入 `AiBehaviorCatalog`，只把「profile → 参数三元组」留成 Projectile 侧的不可变表。

### 2.4 `ProjectileRuntimeRegistry._terminated` —— 终止状态的第二个真源

**事实链**：`ProjectileLifecycleSystem.CommitTermination`（`:251-268`）先写 `ProjectileDispositionStateComponent(Terminated)`，再 `Remove` 注册表，再 `MarkTerminated`。而 `TryTerminate`（`:176-189`）判断"已终止"读的是**组件**，`CommitTermination` 判断"已终止"读的是**组件**，只有 `:165` 与 `:238` 在实体已销毁、解析不到时才回退读 `_terminated`。

**问题**：实体销毁后组件随之消失，此时确实只剩 `_terminated` 能回答"这是已终止还是陈旧句柄"。所以它不是纯冗余——但它是**注册表层的状态**，而 `EntityRuntimeRegistry.Remove` 已经保留了 `_generations`（代际推进）。`_terminated` 是第二个"句柄历史"表。

**判定**：第二真源，但收敛方式需要设计（把终止历史并入共享注册表的代际语义，或明确它是注册表扩展而非 Projectile 私有）。**不建议直接删**。

### 2.5 `ProjectileHitAdmissionResult` —— 与共享 `DamageRejectionReason` 语义重叠

| Projectile 枚举值 | 共享 `DamageRejectionReason` 对应值 |
| --- | --- |
| `ProjectileUnavailable` | `SourceUnavailable` |
| `ProjectileInactive` | `SourceInactive` |
| `TargetImmune` | `TargetImmune`（同名同义） |
| `Accepted` | `None` |

**判定**：这是[错误处理约束](../cr/CR-2026-11-25-error-handling-constraint.md) §「单一码注册表」要清理的典型样本——同一语义在两个域各有一套码。且该枚举**零测试引用**（`grep` 全仓 Test 零命中），生产侧只在 `ProjectileDamageRequestSystem.cs:83` 与 `ProjectileHitResolutionSystem` 内部流转。

### 2.6 `ProjectileSpawnRequests` —— 迁移期兼容入口

**自述**：`ProjectileSpawnRequests.cs:11-13` 写「迁移期的旧投射物实现位于另一个程序集，需要一条不依赖内部约定的入队路径」。

**现状**：生产侧唯一调用者是同程序集的 `ProjectileItemUseSpawnSystem.cs:109`；另有两处测试调用。跨程序集的旧实现已不存在。

**判定**：存在理由已消失。可把 `Enqueue` 收回 `internal`，测试改走同程序集可见性，或保留公开但删掉过时注释。

### 2.7 `ProjectileAiAdapter.AttachedLastUpdate` —— 死值

`ProjectileAiAdapter.cs:54` 声明并在 `:125` 自增，全仓（src + Test）零读取。

## 3. 不成立的候选（避免误删）

| 候选 | 为什么不是重叠 |
| --- | --- |
| `ProjectileId` vs 共享 `EntityId` | 同形（readonly record struct + string + 校验）但异义：`EntityId` 是实体身份，`ProjectileId` 是内容定义键。合并会让内容键获得实体语义 |
| `ProjectileSpawnQueueComponent` / `ProjectileTerminationQueueComponent` | 共享层有 10 个同形 tick 队列组件，但**没有**通用 `TickScopedQueue<T>` 基类。这是「形态重复待抽取」，不是「已被共享系统解决」 |
| `ProjectileLifecycleStatus` | 领域状态机（`Deferred`/`AlreadyTerminated`/`StaleReference`），共享 `DamageProcessStatus` 语义范围不同 |
| `ProjectileAiAdapter` 本体 | 已确认是运行期活跃组件（`Program.cs:720` 注册），承载缺陷 B5 修复（弹体意图恒 `Stop`）。它**不是**数据融合模型，是意图提交器 |

## 4. 建议处置顺序

| 序 | 动作 | 风险 | 前置 |
| --- | --- | --- | --- |
| 1 | 删 `AttachedLastUpdate` | 无 | 无 |
| 2 | `ProjectileSpawnRequests` 收回 internal 并改注释 | 低 | 确认无跨程序集调用者 |
| 3 | `ProjectileHitAdmissionResult` 并入 `DamageRejectionReason` | 中 | 先补测试（当前零覆盖） |
| 4 | 目标选择抽共享 `NearestEntitySelector` | 中 | NPC 与 Projectile 两侧同时改 |
| 5 | 快照投影改 contributor 形态 | 中高 | 先补 Projectile 快照测试 |
| 6 | 行为目录并入 `AiBehaviorCatalog` | 高 | 需先厘清 profile 参数与 declaration 的边界 |
| 7 | `_terminated` 收敛 | 高 | 需设计共享注册表的终止历史语义 |

## 5. 与内容组合分解计划的关系

[Projectile 内容组合分解计划](../plans/2026-11-25-projectile-content-composition-decomposition-plan.md)的范围是**内容定义与装配链路**，其 §排除范围明确写了「AI 行为与 Profile」。因此本审计的 7 项**都不在该计划的范围内**——计划已完成不代表这些重叠已消除。本审计是它们的 canonical 记录。
