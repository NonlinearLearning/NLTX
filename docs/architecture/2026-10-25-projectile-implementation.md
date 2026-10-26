# Projectile 领域实现说明

- 日期：2026-10-25
- 适用项目：`src/NSSLC/Terraria.Components.csproj`
- 实现状态：基础组件、AI 适配、生命周期、实体碰撞候选、Projectile Combat 局部命中提交、穿透/免疫和寿命提交已实现；宿主仍需装配完整 Tick 顺序
- 验证状态：项目构建和脱离生产目录的 Arch/Combat 冒烟通过；尚未声明完整运行时行为等价

## 1. 实现目标

本次实现把 Projectile 设计文档中的第一层可复用合同落到 NSSLC 共享组件项目：

```text
组件组合
  → 行为键与不可变 Profile
  → AiSystem<ProjectileAiIntent>
  → 脱离 ECS 的 ProjectileSimulationSystem
  → ProjectileLifetimeTickSystem / ProjectileCollisionSystem
  → DamageAdmission → DamageMitigation → DamageCommit
  → ProjectileHitResolution → ProjectileLifecycle termination
```

代码没有复制外部客户端的聚合对象、固定数组、静态全局状态、网络字段或方法命名。Projectile
实例由 Arch Entity 上的组件组合表达；行为计算只返回值类型意图，Arch 写入仍由领域适配器
和空间协调器负责。

## 2. 文件组织

新增文件位于 `src/NSSLC/Components/Share/Projectile/`，目录按 Projectile 领域组织，
没有建立横向的全局 `Systems` 或 `Data` 收容目录：

| 文件族 | 作用 |
| --- | --- |
| `Projectile*Component.cs` | 实体能力、关系和持续状态 |
| `ProjectileDefinition*.cs` | 不可变内容定义和定义目录 |
| `ProjectileBehaviorId.cs`、`ProjectileBehaviorProfileId.cs` | 内容定义键 |
| `ProjectileBehaviorProfile.cs`、`ProjectileBehaviorCatalog.cs` | 不可变行为参数和解析目录 |
| `ProjectileAiIntent.cs`、`ProjectileTargetFact.cs` | AI 输入/输出值 |
| `ProjectileStraightAiBehavior.cs`、`ProjectileHomingAiBehavior.cs` | 共享 AI 系统的行为实现 |
| `ProjectileAiRegistration.cs` | Profile 校验和 AI 注册适配 |
| `ProjectileMotionResult.cs` | 脱离 ECS 的一次性运动计算输出；Simulation 直接接收查询边界提供的组件值 |
| `ProjectileSimulationSystem.cs` | 纯 Projectile 运动和寿命计算 |
| `ProjectileLifecycleSystem.cs`、`ProjectileRuntimeRegistry.cs` | spawn、generation handle、owner 关系、终止和 Arch 结构提交 |
| `ProjectileCollisionSystem.cs` | 在 Arch 查询边界读取当前组件，执行 Projectile 策略过滤并组装一次性 hit candidate；AABB 几何交由 Physics |
| `DamageAdmissionSystem.cs`、`DamageMitigationSystem.cs`、`DamageCommitSystem.cs` | 命中准入、减伤计算和目标 `LifeComponent` 唯一常规写入 |
| `ProjectileCombatCommitSystem.cs` | 在 Commit 阶段消费 Projectile 候选；读取 Player 防御/保护窗口和 Attributes，按 Combat owner 提交 Life，再交回 Projectile 穿透/免疫 |
| `ProjectileHitResolutionSystem.cs`、`ProjectileLifetimeTickSystem.cs`、`ProjectileTerminationCommitSystem.cs` | accepted receipt 消费、属性阈值读取、穿透/免疫、寿命推进和 PendingTerminate 收尾 |

## 3. 组件契约

基础组件如下：

```text
ProjectileOwnerComponent
ProjectileSourceMetadataComponent
AiBehaviorSelectionComponent
ProjectileBehaviorStateComponent
ProjectileKinematicsStateComponent
ProjectileLifetimeStateComponent
ProjectilePenetrationStateComponent
ProjectileCollisionPolicyComponent
DamagePayloadComponent
ProjectileHitImmunityStateComponent
ProjectileDispositionStateComponent
ProjectileTerminationRequestComponent
```

可选能力独立组合：

```text
ProjectileHomingCapabilityComponent
ProjectileBounceCapabilityComponent
ProjectileBounceStateComponent
GravityCapabilityComponent
GravityStateComponent
```

组件不保存 Arch `World`、Arch `Entity`、Planner、服务、回调或可写目标引用。`Kinematics`
只保存有限的 `ExtraUpdates` 和当前子步，不复制 `VelocityComponent`。`HitImmunity` 使用
`EntityRuntimeHandle` 的 world scope/generation 比较，不建立全局静态免疫表。

`ProjectilePenetrationStateComponent` 现在只记录 accepted hit 计数；有效上限由
`AttributeKind.ProjectilePenetrationLimit` 计算，负值表示无限。`ProjectileHitImmunityStateComponent`
是 Projectile 的免疫能力组件，list 按目标 handle/generation 保存多个独立到期 Tick；同一目标刷新、
不同目标并存，免疫时长由 `AttributeKind.ProjectileHitImmunityTicks` 计算。
`ProjectileDispositionStateComponent` 的生命周期状态为 `Active`、`PendingTerminate`、
`Terminated`。这些状态的提交由 Projectile 生命周期 owner 执行，本次纯模拟不会直接改变它。

`AttributeSetComponent` 的 `AttributeKind.Defense`、`AttributeKind.DamageReduction` 和
`AttributeKind.Endurance` 是服务器保存的属性事实；`ProtectionWindowComponent` 是独立的共享
Combat 状态。Projectile Combat 从 Attributes 解析减伤，在攻击类型、伤害 profile 与防御公式
确定前，不宣称已经复刻客户端的具体防御公式。

## 4. 行为目录和 AI

`AiBehaviorSelectionComponent` 只选择行为和参数：

```text
BehaviorId  → Projectile 行为内容键
ProfileId   → 不可变 Profile 键
```

`ProjectileBehaviorProfile` 保存行为类型、`AiRequirements`、最大速度、转向速度和范围等
不可变值。它不保存实体身份、世界、目标句柄或 planner 运行态。`ProjectileBehaviorCatalog`
在注册阶段检查 Profile 键唯一，并拒绝同一 `BehaviorId` 绑定多个不兼容的 C# 行为类型。

AI 使用 NSSLC 已有的 `AiSystem<ProjectileAiIntent>` 和 `AiBinding<TIntent>`：

- `ProjectileStraightAiBehavior` 读取速度并返回保持速度意图；
- `ProjectileHomingAiBehavior` 读取位置、速度、追踪能力和当前 Tick 的目标事实，返回受转向速度限制的目标速度；
- AI 不写 `LocationComponent`、`VelocityComponent`、目标 `LifeComponent` 或 Arch 结构；
- 输入不满足 `AiRequirements` 时由共享绑定返回 `MissingComponents`；
- `ProjectileAiRegistration` 只负责把经过目录校验的行为工厂注册到共享 AI 系统。
- `ProjectileDefinition` 和 `ProjectileDefinitionCatalog` 保存定义级默认值与查找结果，不创建
  实体、不持有 Arch `World`，也不保存运行时可变状态。

当前行为类中的 Profile 是显式构造输入，便于宿主为不同 Profile 创建独立绑定；每次绑定仍
由 `AiSystem` 产生新行为实例，不共享可变 planner 状态。

## 5. ProjectileSimulationSystem

`ProjectileSimulationSystem.Simulate` 是脱离 ECS 的确定性计算边界，输入为查询边界提供的组件值：

```text
SpatialFrame
Location、Velocity、Collider、Kinematics、Lifetime、CollisionPolicy、Disposition
ProjectileAiIntent
TileCollisionCapabilityComponent
GravityCapabilityComponent
GravityStateComponent
deltaTime
```

计算规则：

1. 拒绝无效空间版本、非正时间步、非 Active Projectile 和非有限 AI 速度。
2. 寿命为零时返回 `Expired`，不再推进位置。
3. 根据 `ProjectileAiIntent` 选择保持速度或调整速度。
4. 在启用重力时消费 `GravityStateComponent`，不直接改变重力组件。
5. 按 `ExtraUpdates + 1` 执行有限子步，每一步通过 `ISpatialWorldQuery.Move` 查询 Tile。
6. Tile 返回非有限结果时拒绝本次计算；发生 Tile 碰撞时停止剩余子步。
7. 返回 `ProjectileMotionResult`、子步状态和 `Expired/Applied` 状态；寿命仍由 `ProjectileLifetimeTickSystem` 唯一推进。

返回结果只携带值，不代表已经提交。宿主必须在提交边界再次检查 Projectile 身份、generation、
世界 revision 和当前 disposition，然后由 `SpatialSimulationCoordinatorSystem` 写入位置、
速度、接地和液体接触。Projectile Simulation 不处理实体 broadphase、Combat Life、穿透
消费、掉落或删除实体。

## 6. 仍需宿主接入的边界

以下工作保留给后续明确 owner 的系统或宿主装配：

- `BehaviorId`/`ProfileId` 与实体定义的完整 hydration；
- Projectile AI 结果到 Arch 能力状态的提交适配器；
- Bounce 意图到空间速度结果的提交；
- 真实服务器 Tick 注册、NPC/Player runtime handle 注册和跨阶段冲突重捕获；
- Tile 碰撞后的反弹/终止策略、NPC 专属战斗规则和完整 Combat intent。

这些边界不通过在组件中加入 World、服务引用或“万能 Projectile 系统”解决。生命周期和
命中代码已按明确读写集实现；宿主需把系统按 Preparation → Simulation → Commit 注册到
`EcsServer`，并保证结构变更只发生在查询边界之后。

建议顺序为：

```text
Preparation → ProjectileLifetimeTickSystem
Simulation  → shared AI / ProjectileSimulation adapter
              → SpatialSimulationCoordinatorSystem
              → ProjectileCollisionSystem（捕获碰撞事实并生成候选）
Commit      → DamageAdmission / DamageMitigation / DamageCommit / ProjectileCombatCommitSystem
              → ProjectileHitResolutionSystem
              → ProjectileTerminationCommitSystem
```

## 7. 验证

执行命令：

```text
dotnet build src/NSSLC/Terraria.Components.csproj --no-restore \
  -p:BaseOutputPath=Build/bin/ \
  -p:BaseIntermediateOutputPath=Build/obj/
```

结果：退出码 `0`，`0` 个警告，`0` 个错误。生成物位于：

```text
src/NSSLC/Build/bin/Debug/net10.0/Terraria.Components.dll
```

本次构建验证了组件、AI 适配、生命周期、命中提交和 Projectile 纯模拟代码可以在当前项目中
编译。脱离生产目录的 `buff-smoke` Arch 冒烟覆盖了 owner handle 创建、AABB 候选、Damage
admission/mitigation/commit、Player 防御保护窗口、Attributes 减伤、accepted receipt、穿透
耗尽和终止后的旧 handle 拒绝，输出为：

```text
Buff/NPC smoke verification passed.
Projectile ECS smoke verification passed.
```

冒烟命令为：

```text
dotnet run --project .agent-workplace/buff-smoke/Smoke.csproj --no-restore
```

本次冒烟还验证了多目标免疫并存、同目标窗口刷新、旧 generation 不命中、属性修饰改变
穿透上限、保护窗口拒绝时不消费 accepted hit，以及免疫记录在独立生命周期组件中维护。

这不证明完整 Tile sweep、AI 宿主绑定、NPC/Player 全部生命周期或旧客户端行为等价；这些仍
需要宿主级行为测试。







