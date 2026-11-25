# Projectile 领域

这个目录保存 Projectile 的原子组件、按内容族切分的不可变定义与行为目录、共享 AI 行为适配
和 Arch Tick 适配器。它不保存网络协议、表现状态、Tile 存储或长期 Projectile 聚合
对象；组件本身不持有 Arch `World`，生命周期/候选/提交系统只在明确边界接收宿主提供的世界。

## 组合模型

基础状态使用 `Projectile*Component` 表达；追踪、反弹和重力使用能力组件是否存在表达：

```text
ProjectileHomingCapabilityComponent + ProjectileTargetComponent（被 ProjectileTargetFact 投影）
ProjectileBouncePolicyComponent（内容参数）
ProjectilePenetrationStateComponent（Remaining / PendingConsumed / Consumed / SuppressesDamage）
GravityCapabilityComponent + GravityStateComponent
SubstepCapabilityComponent（共享物理层，投射物按 extraUpdates + 1 装配）
```

查询直接使用精确的 Arch 组件组合。不要把这些能力重新包装成布尔字段或能力列表。

穿透预算是**弹体自身的"生命值"**，命中与反弹共用同一份计数：维护第二个"还能反弹几次"
的计数器必然与它漂移。`Remaining = -1` 表示无限；`PendingConsumed` 是本 tick 已入队、
等伤害回执的预扣；回执为 `Applied` 才转成 `Consumed`，被拒则回滚预扣——不回滚就会出现
"打空也扣穿透"。

## 内容定义与能力装配

内容定义是一组按内容族切分的 section（`IProjectileSection`），装配退化为对 section 列表的
两遍折叠（`ProjectileComposition`）：先 `Collect` 声明跨族参数，再 `Contribute` 构造组件。
section 只持标量与值对象，运行时组件一律在 `Contribute` 里构造——内容是默认值，不是当前值。

| 内容族（section） | 装配结果 |
| --- | --- |
| `ProjectileBasicsSection` | 身份、行为选择、寿命、碰撞策略与碰撞能力、攻击引用、属性集；碰撞体与取整尺寸以标量形式供生成请求使用 |
| `ProjectileGravitySection` | `GravityCapabilityComponent` + `GravityConfigurationComponent` + `GravityStateComponent`；关闭时移动模式保持 `Flight`，开启时切到 `Free` 以便重力积分生效 |
| `ProjectileLiquidSection` | `LiquidInteractionCapabilityComponent` + `LiquidContactComponent`（后者缺失时空间协调系统不会回写液体接触事实） |
| `ProjectilePenetrationSection` | 生成时由 `FromLimit` 把 `MaximumHits` 快照成 `Remaining`；`SuppressesDamage` 决定打空后是否哑火。上限的唯一来源是本节，内容侧不再写 `AttributeKind.ProjectilePenetrationLimit` |
| `ProjectileBounceSection`（可缺省） | `ProjectileBouncePolicyComponent`；运行时次数归穿透预算，不再单独计数 |
| `ProjectileHomingSection`（可缺省） | `ProjectileHomingCapabilityComponent`；转向率同时经参数块进入 `MovementCapabilityComponent.acceleration`，这是唯一真实存在的跨族读数，也是两遍折叠的理由 |
| `BasicsSection.ExtraUpdates` | `SubstepCapabilityComponent(extraUpdates + 1)`；高速弹靠子步避免穿墙 |

运动必需的前置能力组由 `ProjectileComposition.AddSpatialPrerequisites` 统一添加，折叠结束后
`EnsureSpatialPrerequisites` 逐项校验，缺项在建服期抛异常并带上内容 id 与缺失类型名——
不再让"弹体不动"这种症状离原因十步远。

内容定义来自 LVCS 的 `projectile` 内容族（`NSSLC.Server/Content/Projectile`）：
`ProjectileCodec` 只做与批次有关的事——反序列化、`id` / `type` 必填、重复 id 判定；
族字段的校验与转换由 `IProjectileSectionReader` 承担，读取器逐个登记在
`ProjectileSectionReaders`，因此新增一族不改 codec。`ProjectileCatalog` 在 `Seal()` 后作为
不可变目录传入系统，装配在 `ProjectileRecipe` 构造时完成一次。定义里不放行为逻辑，
行为仍由共享 AI 行为类实现。

## 行为模型

`AiBehaviorSelectionComponent` 只保存 `BehaviorId` 和 `ProfileId`。Profile 是不可变参数，
`AiRequirements` 是行为输入声明，Planner 和运行时状态由共享 `AiBinding` 管理。

行为看到的事实由按源域拆开的 contributor 投影：位置、速度、追踪能力、目标各一份，
provider 自己不认识任何一种事实，只在行为声明过某项事实时调用对应 contributor。
同一事实被两个 contributor 认领会在构造期失败。

`ProjectileRecipe` 和 `ProjectileCatalog` 保存定义级默认值与查找结果，不创建实体、
不持有 Arch `World`，也不保存运行时可变状态。

```text
BehaviorId/ProfileId
        ↓ catalog validation
AiSystem<ProjectileAiIntent>
        ↓ component facts at the query boundary
ProjectileAiIntent
        ↓
ProjectileMovementIntentSystem → MovementIntentRequestSystem
        ↓
PhysicsContactBuffer → CombatAttackIntentBuffer → Combat admission/commit → penetration/immunity → lifecycle termination
```

穿透上限和命中免疫 Tick 是 Attributes 的数值输入；Projectile 只声明免疫能力并保存目标免疫 list 和 accepted hit 计数；终止请求进入 world-scoped `ProjectileTerminationBuffer`。

终止历史归本域：`ProjectileTerminationJournal` 记住已经终止过的句柄，用于把「重复终止」与
「陈旧句柄」分开。共享注册表只回答「句柄指向谁」，不背这份历史——实体销毁后组件消失，
只有历史还在。免疫 list 按 `EntityRuntimeHandle` 的 world scope/generation
区分目标，同一目标刷新窗口，不同目标同时保留。

Projectile 行为属于共享 AI 系统的一种实现，不建立独立的 `ProjectileBehaviorSystem`。AI 不
直接写 `LocationComponent`、`VelocityComponent`、目标 `LifeComponent` 或实体结构。

## 模拟与提交

位移与子步只有一处实现：`SubstepCapabilityComponent`（内容 `extraUpdates + 1`）由共享
`SpatialSimulationCoordinatorSystem` 读取，按子步细分本帧位移，因此"子步"只提高瓦片碰撞精度、
不改变总位移。Projectile 域不再保留第二套脱离 ECS 的位移实现——那套代码没有生产消费者，
却一直让读者以为子步归它管。

`SpatialSimulationCoordinatorSystem` 是位置、速度和接触事实的唯一写者，并在提交边界重新校验
实体身份、generation、disposition 和 world revision。命中、生命、穿透、免疫和实体销毁分别由
Combat、Projectile 状态 owner 和生命周期 owner 提交。

Projectile 碰撞入口消费共享 `PhysicsContactBuffer` 的几何事实；通用 Collider 几何、Layer/Mask、接触点和法线由共享 Physics 计算。Projectile 入口只处理自身碰撞策略、Owner 排除、命中免疫和重复目标抑制，再向 `CombatAttackIntentBuffer` 追加攻击定义键、目标句柄和接触事实。系统不保存 Projectile 或目标的聚合快照，也不在 Projectile 实体上保存 DamagePayload。

已实现的 Arch/Combat 入口包括 `ProjectileLifecycleSystem`、`PhysicsContactCaptureSystem`、
`ProjectileLifetimeTickSystem`、`ProjectileTerminationCommitSystem`、共享 Combat 请求收集与
结算系统，以及 `ProjectileHitResolutionSystem`。Projectile 接触意图由 `CombatAttackRequestSystem` 转换成共享 `DamageRequest`；共享 `DamageCommitSystem` 是目标生命的伤害写入点，Projectile 结果系统只处理
穿透、命中免疫和终止。

按阶段排列的完整入口：

| 阶段 | 系统 | 职责 |
| --- | --- | --- |
| Preparation | `ProjectileItemUseSpawnSystem` | 消费本帧使用事实，按 `ItemUse.Shoot` 入队生成请求（队列按 tick 归属，必须与生成同阶段） |
| Preparation | `ProjectileSpawnRequestSystem` / `ProjectileLifecycleSystem` | 生成弹体并装配内容定义 |
| Preparation | `ProjectileTargetAcquisitionSystem` → `ProjectileAiAdapter` | 锁定目标并算出本帧速度意图，覆盖默认 `Stop` 意图 |
| Simulation | `ProjectileMovementIntentSystem` → `MovementIntentRequestSystem` | 刷新并装入本帧位移意图 |
| Simulation | `SpatialSimulationCoordinatorSystem` | 唯一位移写者；产出实体接触事实与瓦片接触事实 |
| Simulation | `ProjectileTileResolutionSystem` | 消费瓦片接触：按 `ConsumeOrder` 判预算 → 反射速度 + 入位移意图，或入终止队列 |
| Simulation | `ProjectileDamageRequestSystem` | 消费实体接触，预扣预算后入 `CombatAttackIntentQueue` |
| Commit | `ProjectileDamageResultSystem` | `Applied` → 确认扣减并可终止；`Rejected` → 回滚预扣 |
| Commit | `ProjectileTerminationCommitSystem` | 按句柄去重、取原因优先级，提交销毁 |

宿主需要把这些系统按 `EcsServerPhase` 注册，并为同一世界的 owner、NPC 和 Player
注册统一的 `IEntityRuntimeResolver`；世界级队列（`TileContactQueueComponent`、
`PhysicsContactQueueComponent`、`MovementIntentRequestQueueComponent`）必须是**单例**，
同一组件出现两个宿主实体时 `SingletonComponent<T>` 会直接抛错。实现细节与行为证据见：

- [Projectile 实现说明](../../../docs/reviews/audits/2026-10-25-projectile-implementation.md)：证据范围与行为参考
- 领域模型、读写集与阶段顺序以本文件为准；本轮内容、射击接缝与预算状态机见
  [投射物 P0–P4 CR](../../../docs/cr/CR-2026-11-24-projectile-p0-p1-content-and-shoot-seam.md)



