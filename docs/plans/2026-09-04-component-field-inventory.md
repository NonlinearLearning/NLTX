# ECS 组件字段归属基线

本清单是 `2026-09-04-all-component-split-implementation.md` 的 Task 1 证据。
它记录本轮开始时的字段权威归属、重复模型和迁移动作；不把名称相近但生命周期不同的字段
强行合并。

## 已符合目标且不新增同义类型

| 能力 | 当前权威类型 | 写者 | 消费者 | 动作 |
| --- | --- | --- | --- | --- |
| 世界位置 | `EntityEcs.Components.LocationComponent` | Spawn、Movement、Physics、领域行为 | Geometry、Collision、Replication | 保留 |
| 速度 | `EntityEcs.Components.VelocityComponent` | Input/AI、Physics、Projectile behavior | Movement、Collision | 保留 |
| 碰撞几何 | `EntityEcs.Components.ColliderComponent` | Definition、Spawn | Geometry、Collision、Targeting | 保留 |
| 水平朝向 | `EntityEcs.Components.DirectionComponent` | Player/NPC 控制与行为 | Movement、Replication | 保留；Projectile 继续使用专用方向 |
| 实体液体接触 | `EntityEcs.Components.LiquidComponent` | 领域环境系统 | 领域液体规则 | 保留；不与 Tile 液体合并 |
| 实体身份根 | `EntityEcs.Components.EntityIdentityComponent` | 权威 spawn/commit | Registry、诊断 | 保留；不与协议/账户/持久化 ID 合并 |

## Player/NPC Combat 与效果

| 概念 | 当前类型和字段 | 当前写者/消费者 | 问题 | 迁移动作 |
| --- | --- | --- | --- | --- |
| 生命 | `HealthComponent.Current/Maximum` | Damage、regen、Player/NPC spawn、lifecycle | 语义正确，但构造与突变未统一保证边界不变量 | 保留并补足不变量测试；不与生命周期合并 |
| 防御 | `DefenseComponent.Value` | spawn、damage calculation | 与 `NpcCombatStateComponent.Defense/BaseDefense` 重复 | 以 `DefenseComponent` 为最终防御权威；把 NPC 定义/基准防御保留为 NPC 专属定义或基准字段 |
| NPC 最大生命 | `HealthComponent.Maximum` 与 `NpcCombatStateComponent.MaximumHealth/BaseLifeMax` | spawn、NPC combat/snapshot | 同一生命上限存在重复权威候选 | `HealthComponent` 为运行时生命权威；NPC 的 base 值保留为定义/重算输入，不作为第二个当前最大生命 |
| NPC 攻击、击退和伤害资格 | `NpcCombatStateComponent` | NPC combat/AI | 属于 NPC 专属能力，不能误合并为 Health 或 Defense | 保留在 NPC 域；移除其中与通用 Health/Defense 重复的当前值 |
| 伤害免疫 | `ImmunityComponent`、`HitImmunityComponent` | Damage resolution、projectile hit policy | Player/NPC 短免疫与 projectile/local/static cooldown 键空间不同 | `ImmunityComponent` 用于被击资格；`HitImmunityComponent` 保持 Projectile 命中冷却专属 |
| Mana | `ManaComponent` | Player 资源/再生系统 | 已独立于 Health | 保留；不创建 GenericResource |
| 效果集合/单项 | `StatusEffectsComponent`、`TimedStatusEffect` | StatusEffects Systems | 语义正确 | 保留，补齐集合与单项到期边界测试 |
| 生命周期 | `PlayerLifecycleComponent`、`NpcLifecycleComponent` | Player/NPC lifecycle systems | 终止和恢复语义不同 | 保持分域；不创建 MobStateComponent |

## Projectile

| 概念 | 当前类型 | 当前写者/消费者 | 问题 | 迁移动作 |
| --- | --- | --- | --- | --- |
| 生成时定义 | `ProjectileDefinitionComponent` | 两种 spawn system、behavior/collision/replication readers | 部分 behavior effect 直接使用 `with` 重写 definition 的 knockback，破坏“定义不可变”边界 | 将运行时可衰减的攻击状态迁入运行时 damage/ability 组件；definition 只保留生成配置 |
| 行为状态 | `ProjectileBehaviorComponent` 与 `ProjectileBehaviorState` | Projectile behavior systems | 已与寿命分离 | 保留；TargetId 仅作行为局部状态，不取代 Targeting 关系 |
| 寿命 | `ProjectileLifetimeComponent` | Projectile tick/despawn | 已与行为分离 | 保留；结束原因保持 Projectile 生命周期语义 |
| Player/NPC 所有者 | `ProjectileOwnerComponent`、`NpcProjectileOwnerComponent` | spawn、owner hit rules | 所有者与当前目标必须保持独立 | 保留分域 Owner 组件；禁止复用为 Targeting |
| 伤害 | `ProjectileDamageComponent` | spawn、behavior effect、hit resolution | 已与目标 Health 分离；应承载所有运行时攻击值变化 | 保留并把运行时攻击衰减从 definition 移入此边界或其专用能力组件 |
| 穿透 | `ProjectilePenetrationComponent` | collision/behavior effects | 已独立 | 保留 |
| 专用方向 | `ProjectileDirectionComponent` | spawn、behavior | 与公共水平 Direction 生命周期不同 | 保留 |
| 协议复制 | NetworkIdentity/NetworkUpdate components | protocol/replication | 不属于领域状态 | 保持分离 |

## 输入、运动与方向

| 概念 | 当前类型 | 动作 |
| --- | --- | --- |
| Player 控制意图 | `ControlInputComponent`、`PlayerControlStateComponent`、`MovementIntentComponent` | 保持与 Velocity 分离；明确写者链 |
| NPC 运动意图 | `NpcMovementIntentSystem` 与领域 movement state | 保持与 Velocity 分离 |
| 物理结果 | `VelocityComponent`、`PhysicsStateComponent` | Movement/Physics 负责位置和速度结果，不把输入写回结果字段 |
| 公共/专用方向 | `DirectionComponent`、`ProjectileDirectionComponent`、NPC 专属状态 | 保持不同领域含义 |

## Item、Inventory、Equipment、Container

| 概念 | 当前类型 | 当前边界判断 | 迁移动作 |
| --- | --- | --- | --- |
| 静态定义 | `ItemDefinition` 及 `Items/Definitions/*` | 定义已与实例数量分离 | 为 ECS 实例建立明确 `ItemDefinitionComponent` 投影前，先确认其不复制 registry 权威 |
| 堆叠 | `ItemStack`、`ItemStackComponent` | 数量与类型仍打包在值对象中 | 保持值对象；仅在实例 ECS 需要独立数量写入时引入 `StackableItemComponent` |
| 实例变体 | `ItemInstanceStateComponent` | prefix、variant、dye、name override 与定义/堆叠分离 | 保留 |
| 角色库存 | `InventoryComponent` | 40 槽、选择槽、revision 和实例状态属于 Player inventory 组织 | 保留；不把 Chest 作为 Player Inventory |
| 穿戴 | `ItemEquipmentStateComponent` 与 Player equipment inventory | 穿戴槽位和装备效果属于独立关系 | 收敛为明确 Equipment 语义，但不把 weapon use/cooldown 混入 |
| 武器/使用 | `ItemUseStateComponent`、ItemUseDefinition、ItemCombatDefinition | 攻击能力和使用进度不能与穿戴关系合并 | 保持/收敛为 Weapon/Use 边界 |
| 容器 | `ChestInventoryComponent`、ChestDefinition/Access | 普通容器容量与角色库存语义不同 | 抽取最小 Container 能力仅在多个非角色容器真正共享时进行；Chest 继续归 WorldObjects |
| Hands | 未见独立权威手部组件 | 现有“选择槽”不自动等于手部关系 | 先查明当前操作语义；无独立读写者则不虚构 HandsComponent |

## Targeting、AI、关系与身份

| 概念 | 当前类型 | 问题 | 迁移动作 |
| --- | --- | --- | --- |
| NPC 目标 | `Components/NPC/NpcTargetComponent` 与 `Npc/Components/NpcTargetComponent` | 两个同名同域类型并存，spawn 路径同时创建两者 | 选择一个域归属（NPC），迁移读取者并删除重复类型 |
| NPC AI | `Components/AI/NpcAiStateComponent` 与 `NpcBehaviorStateComponent` | AI 计时/行为与领域行为配置需分清 | 前者仅保存阶段/计时；后者保持 NPC 专属策略状态 |
| Projectile 行为局部 target | `ProjectileBehaviorState.TargetId` | 是行为内部数值，不等于跨域实体 Targeting | 保留为局部 AI 数据或替换为明确 typed handle，不能升级成公共关系 |
| Owner | Player/NPC projectile owner components | 固定生成关系 | 保持独立于 Target |
| 身份和复制 | Entity UUID、Player/NPC/Projectile protocol/replication components | 作用域各异 | Registry/adapter 只做单向投影和显式失败；不合并字段 |

## 首批实施优先级

1. NPC `HealthComponent`/`DefenseComponent` 与 `NpcCombatStateComponent` 的重复运行时字段。
2. Projectile definition 被运行时 behavior 改写的路径。
3. 两套 NPC Target 组件的重复附着与消费者迁移。
4. Item/Inventory/Container/Hands 的字段审计后，只实现有独立读写者支持的最小边界。

