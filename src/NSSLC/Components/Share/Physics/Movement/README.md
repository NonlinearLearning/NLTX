# Movement 组件与意图

`Movement` 保存实体的速度、移动配置、重力配置和本 tick 的移动请求。它不读取 Arch
存储，不拥有世界对象，也不直接写位置。

> 位置：原 `Share/Movement/`，2026-11-01 整体并入 `Share/Physics/Movement/`。
> C# 命名空间保持 `Terraria.Movement` 不变，目录只表达归属。
>
> 2026-11-01 另有三个空间状态类型从 `Share/Entity/` 移入本目录，命名空间同样未变：
> `LocationComponent`（`Terraria.Entity`，权威位置）、
> `CombatVelocityComponent.cs` 内的 `Terraria.Combat.VelocityComponent`、
> `CombatPositionComponent.cs` 内的 `Terraria.Combat.PositionComponent`。
> 后两者是 `[Obsolete]` 兼容投影，当前零消费者，只作迁移提示，不应再加入新原型。
> 文件名加 `Combat` 前缀是为了与本目录的 `VelocityComponent.cs`
> （`Terraria.Movement.VelocityComponent`）区分。
>
> 同日 `CombatPositionComponent.cs`（`Terraria.Combat.PositionComponent`）已删除。
> 位置权威只有 `LocationComponent`，保留第二套过时投影没有价值。

## 组件职责

| 类型 | 职责 | 写入者 |
| --- | --- | --- |
| `LocationComponent` | 权威世界坐标（碰撞体原点） | 空间提交系统 |
| `VelocityComponent` | 当前空间速度 | 空间提交系统 |
| `MovementCapabilityComponent` | 速度上限、加速度、跳跃和移动模式 | 定义/能力系统 |
| `MovementIntent` | 输入或 AI 在指定 tick 的请求 | 输入/AI/战斗效果 |
| `GravityCapabilityComponent` | 是否参与重力及能否切换方向 | 定义/能力系统 |
| `GravityConfigurationComponent` | 基础重力和默认方向 | 定义/创建 |
| `GravityDirectionStateComponent` | 没有覆盖时的方向选择 | 重力输入系统 |
| `GravityModifierCollectionComponent` | 带来源和期限的重力修饰器 | 效果生命周期系统 |
| `GravityStateComponent` | 当前帧求值后的重力投影 | `GravityModifierSystem` |

`MovementIntent` 是一次性输入，不是成功的移动结果。它带有来源、来源 revision、tick 和
优先级，系统必须在执行时重新检查实体、能力、tick 和世界版本。

## 与 AI 的协作

```text
PlayerInput / AiEntitySystem / CombatEffect
        ↓ MovementIntent
MovementIntentResolver
        ↓ 一个已选择的请求
Physics.SpatialSimulationSystem
        ↓ SpatialSimulationResult
Physics.SpatialSimulationCoordinatorSystem
        ↓ Location + Velocity + Contact
```

AI 只能返回意图，不能取得 Arch 组件的可写引用。击退、爆炸和冲刺同样先产生意图或外部
力请求，由空间提交边界统一写入速度和位置。

## 重力

`GravityModifierCalculator` 是显式输入的确定性求值器。`GravityModifierSystem` 管理修饰器
生命周期并生成 `GravityStateComponent`。移动系统读取该投影，不能把装备、网络和移动系统
都作为重力状态的并列写入者。

## 迁移规则

当前服务器仍有 Player、NPC 和 Projectile 专属移动 owner。接入时使用适配器把这些 owner
的空间状态转换为 `SpatialEntitySnapshot<TEntity>`，验证后再交给空间系统；不要让旧 owner
和 ECS 组件同时推进同一实体的位置。
