# Projectile 领域

这个目录保存 Projectile 的原子组件、不可变定义与行为目录、共享 AI 行为适配、Arch Tick
适配器和脱离 ECS 的纯模拟。它不保存网络协议、表现状态、Tile 存储或长期 Projectile 聚合
对象；组件本身不持有 Arch `World`，生命周期/候选/提交系统只在明确边界接收宿主提供的世界。

## 组合模型

基础状态使用 `Projectile*Component` 表达；追踪、反弹和重力使用能力组件是否存在表达：

```text
ProjectileHomingCapabilityComponent + ProjectileTargetFact input
ProjectileBounceCapabilityComponent + ProjectileBounceStateComponent
GravityCapabilityComponent + GravityStateComponent
```

查询直接使用精确的 Arch 组件组合。不要把这些能力重新包装成布尔字段或能力列表。

## 行为模型

`AiBehaviorSelectionComponent` 只保存 `BehaviorId` 和 `ProfileId`。Profile 是不可变参数，
`AiRequirements` 是行为输入声明，Planner 和运行时状态由共享 `AiBinding` 管理。

`ProjectileDefinition` 和 `ProjectileDefinitionCatalog` 保存定义级默认值与查找结果，不创建
实体、不持有 Arch `World`，也不保存运行时可变状态。

```text
BehaviorId/ProfileId
        ↓ catalog validation
AiSystem<ProjectileAiIntent>
        ↓ component facts at the query boundary
ProjectileAiIntent
        ↓
ProjectileSimulationSystem
        ↓
PhysicsContactBuffer → CombatAttackIntentBuffer → Combat admission/commit → penetration/immunity → lifecycle termination
```

穿透上限和命中免疫 Tick 是 Attributes 的数值输入；Projectile 只声明免疫能力并保存目标免疫 list 和 accepted hit 计数；终止请求进入 world-scoped `ProjectileTerminationBuffer`。免疫 list 按 `EntityRuntimeHandle` 的 world scope/generation
区分目标，同一目标刷新窗口，不同目标同时保留。

Projectile 行为属于共享 AI 系统的一种实现，不建立独立的 `ProjectileBehaviorSystem`。AI 不
直接写 `LocationComponent`、`VelocityComponent`、目标 `LifeComponent` 或实体结构。

## 模拟与提交

`ProjectileSimulationSystem` 只计算：

- AI 速度意图；
- 有限 `ExtraUpdates` 子步；
- Gravity 投影；
- Tile 查询和碰撞结果；
- 当前寿命事实检查；寿命递减由 `ProjectileLifetimeTickSystem` 唯一负责；
- 带空间 revision 的结果值。

它返回 `ProjectileSimulationResult`，不直接修改 Arch。宿主在提交边界重新校验实体身份、
generation、disposition 和 world revision，然后由 `SpatialSimulationCoordinatorSystem` 统一
写入位置、速度和接触状态。命中、生命、穿透、免疫和实体销毁分别由 Combat、Projectile
状态 owner 和生命周期 owner 提交。

Projectile 碰撞入口消费共享 `PhysicsContactBuffer` 的几何事实；通用 Collider 几何、Layer/Mask、接触点和法线由共享 Physics 计算。Projectile 入口只处理自身碰撞策略、Owner 排除、命中免疫和重复目标抑制，再向 `CombatAttackIntentBuffer` 追加攻击定义键、目标句柄和接触事实。系统不保存 Projectile 或目标的聚合快照，也不在 Projectile 实体上保存 DamagePayload。

已实现的 Arch/Combat 入口包括 `ProjectileLifecycleSystem`、`PhysicsContactCaptureSystem`、
`ProjectileLifetimeTickSystem`、`ProjectileTerminationCommitSystem`、共享 Combat 请求收集与
结算系统，以及 `ProjectileHitResolutionSystem`。Projectile 接触意图由 `CombatAttackRequestSystem` 转换成共享 `DamageRequest`；共享 `DamageCommitSystem` 是目标生命的伤害写入点，Projectile 结果系统只处理
穿透、命中免疫和终止。宿主需要把这些系统按 `EcsServerPhase` 注册，并为同一世界的 owner、NPC
和 Player 注册统一的 `IEntityRuntimeResolver`。完整字段、阶段、失败和后续接入顺序见：

- [Projectile ECS 系统设计](../../../docs/architecture/2026-10-24-projectile-ecs-system-design.md)
- [Projectile 实现说明](../../../docs/reviews/audits/2026-10-25-projectile-implementation.md)



