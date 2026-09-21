# 根 src ECS 组件拆分设计

## 决策与范围

本记录取代此前仅适用于 `dome/` 的实施目标。依据
[`组件设计报告`](../../component-decomposition/baseline/组件设计报告.md)，组件实现在根目录 `src/`；`dome/`、`Test/`、
构建策略与验证程序不在本次变更范围。用户明确要求不运行构建或测试。

现有 `src/Share/Entity` 的 `LocationComponent`、`VelocityComponent`、
`DirectionComponent`、`ColliderComponent`、`LiquidComponent` 和
`EntityIdentityComponent` 是已确认的共享基线，保持原路径和 API，不创建同义聚合组件。

## 证据与边界

| 来源字段/状态 | 新的权威组件 | 主要写者 | 归属 |
| --- | --- | --- | --- |
| `Player.statLife`、`NPC.life` | `HealthComponent` | Combat System | Combat |
| `NPC.defense`、Item 防御值 | `DefenseComponent` | Combat/装备 System | Combat |
| `Player.immune`、`immuneTime` | `ImmunityComponent` | Combat System | Combat |
| `Player.control*` | `InputIntentComponent` | Player Input System | Player |
| `NPC.target`、`aiStyle`、`ai` | `TargetingComponent`、`NpcAiStateComponent` | NPC AI System | Npc |
| `Projectile.type`、`ai`、`timeLeft`、`owner`、`damage`、`penetrate` | 七个 Projectile 组件 | Projectile Systems | Projectile |
| `Item.type`、`stack`、`maxStack`、装备/使用字段 | Item、库存和装备组件 | Item/Inventory Systems | Items |
| `Chest.item`、`maxItems` | `ContainerComponent` | Container System | Items |
| buff 类型和倒计时 | `StatusEffectsComponent`、`TimedStatusEffect` | Status Effect System | StatusEffects |

以上字段来源于报告所列 `D:\TRbackup\Version4` 类型；SS14 仅作为“按能力目录与组件/System
边界组织”的结构参考，不复制其代码、命名或领域模型。tModLoader API 文档无法确认私有运行
顺序，因此本次只建立无副作用的权威状态模型，不声称改变原版运行时语义。

## 源码拓扑与依赖方向

```text
src/Share/Entity                 existing identity and spatial vocabulary
src/Relationships                EntityReference
src/Combat                       health, defense, immunity
src/Physics                      collision policy and result
src/Player                       input intent and player lifecycle
src/Npc                          targeting, AI and NPC lifecycle
src/Projectile                   projectile definition, behavior and lifecycle
src/Items                        item, inventory, hands, equipment and container
src/StatusEffects                timed effect collection

Player / Npc / Projectile / Items / StatusEffects -> Relationships
all project references point only to lower-level vocabulary; no domain project
references another domain project.
```

Every listed directory contains actual report-supported source types. Components are placed
directly in their small domain directory; no empty `Components/`, `Systems/`, or generic
shared catch-all directory is introduced. `Relationships` uses a scoped `EntityReference`
rather than exposing a naked `Guid` across domains.

## Ownership contracts

- `HealthComponent`, `DefenseComponent`, and `ImmunityComponent` are independent mutable
  combat inputs/results. They do not encode Player or NPC death transitions.
- `PlayerLifecycleComponent`, `NpcLifecycleComponent`, and `ProjectileLifetimeComponent`
  deliberately remain separate. A general `MobStateComponent` is not created.
- `PlayerAimComponent`, `NpcDirectionComponent`, and `ProjectileDirectionComponent` express
  their domain-specific direction state; they do not overload the shared horizontal
  `DirectionComponent`.
- Projectile definition, behavior, owner, damage, penetration, lifetime, and trajectory
  direction have one component each. Network identity, presentation history, and replication
  cursors are excluded.
- `InventoryComponent` describes a player inventory layout; `ContainerComponent` describes
  generic capacity and contents; `HandsComponent` is only the currently held relation.
- `TimedStatusEffect` is one value record. `StatusEffectsComponent` owns its collection.
- Collision policy/result is separate from the shared collider geometry. Systems will own
  collision calculation and writes when they are introduced later.

## Migration, impact, and rollback

There are no non-shared root `src` types to move: the source side is new files only. Existing
shared Entity source is not modified. The new projects depend on no external ECS runtime and
introduce no system scheduling, I/O, clocks, randomness, protocol conversion, persistence, or
rendering state. Consumers can adopt one component at a time without dual-writing legacy fields.

Rollback is limited to deleting the new project files and directories introduced by this change;
no existing source API or `dome/` behavior is changed. Compilation and tests are intentionally
not run at the user's direction, so this design is recorded as implementation-unverified.
