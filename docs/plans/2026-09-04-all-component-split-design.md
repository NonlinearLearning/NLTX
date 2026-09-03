# 全量 ECS 组件拆分设计

## 目标

依据 `docs/组件设计报告.md`，参考 `D:\TRbackup\Version4` 的字段来源和
`C:\Users\shan\Downloads\ECS\space-station-14-master` 的共享组件/领域系统组织方式，
把当前 Simulation 中仍然混合或重复的状态收敛为按领域归属的 ECS 组件，同时保持现有运行时
行为、协议语义、持久化语义和公开 API 的兼容性。

本设计中的“全量”指报告确认的全部组件边界都要完成审计和落地；对于当前已经满足边界的
组件，不重新创建同义类型，而是验证其权威来源和消费者，必要时只迁移路径或移除重复模型。

## 约束与非目标

- 不创建 `BaseEntityComponent`、`SpatialComponent`、`SpatialCoreComponent` 或通用资源/寿命组件。
- 不把 Player、NPC、Projectile 的不同生命周期、网络复制、液体规则或表现历史合并为公共状态。
- 不把 `ProjectileDamageComponent` 与 `HealthComponent` 合并。
- 不把 `EntityIdentityComponent`、协议 identity、账户 UUID、持久化 ID、ReplicationId 或
  `whoAmI` 当作同一个身份字段。
- 组件只承载状态；跨实体行为由 System、Command、Event 或 Adapter 负责。
- 目录按能力/领域优先；职责子目录仅在已有稳定边界下使用；每个同名 PascalCase 文件只保留
  一个核心公共类型。
- 迁移期间允许短期单向 Adapter，但禁止新旧权威字段双写或隐式互相覆盖。

## 目标拓扑

### 公共 Entity 能力

继续使用 `src/Share/Entity/Components` 中现有的：

- `LocationComponent`
- `VelocityComponent`
- `ColliderComponent`
- `DirectionComponent`
- `LiquidComponent`
- `EntityIdentityComponent`

几何查询只读取位置和碰撞几何，保持纯计算；不创建公共聚合组件。

### Player/NPC 战斗能力

- `HealthComponent`：当前生命和最大生命，仅附着于可受伤 Player/NPC。
- `DefenseComponent`：伤害计算的最终防御输入。
- `ImmunityComponent`：伤害资格和免疫窗口。
- `StatusEffectsComponent` 与 `TimedStatusEffect`：集合职责和单条效果记录分开。
- Player/NPC 的死亡、复活、消失、掉落和 tombstone 继续由各自生命周期组件处理；不创建
  通用 `MobStateComponent` 重新聚合不同生命周期。
- 仅当当前权威代码存在 Mana/Stamina 等资源时，才按资源语义分别拆分，不引入带类型分支的
  `GenericResourceComponent`。

### Projectile 能力

审计并收敛现有的 `ProjectileDefinitionComponent`、`ProjectileBehaviorComponent`、
`ProjectileLifetimeComponent`、`ProjectileOwnerComponent`、`ProjectileDamageComponent`、
`ProjectilePenetrationComponent` 和专用方向组件。网络 identity/update、液体策略、碰撞策略、
表现和历史状态继续留在 Projectile 或协议/表现边界。

### 输入、运动和朝向

输入组件表达控制意图，`VelocityComponent` 表达物理/AI/环境处理后的结果；Movement/Physics
消费前者并更新后者和位置。公共 `DirectionComponent` 只表达稳定的水平朝向，Projectile 轨迹、
NPC 垂直方向、Player 瞄准方向及表现方向各自保留。

### Item、Inventory、Hands、Equipment、Container

按静态定义、实例数量、库存组织、手部关系、穿戴关系、攻击能力和容纳关系分别审计并落地：

- `ItemDefinitionComponent`
- `StackableItemComponent`
- `InventoryComponent`
- `HandsComponent`
- `EquipmentComponent`
- `WeaponComponent`
- `ContainerComponent`

现有 `ItemStackComponent`、`ItemEquipmentStateComponent`、WorldItem 和 Chest 组件先做字段
映射，再决定保留、改名、拆分或通过 Adapter 迁移；不复制平行模型。

### Targeting、AI、身份和复制

- `TargetingComponent` 只表示动态目标关系。
- `NpcAiStateComponent` 只表示 AI 阶段、计时和内部状态。
- Owner、Target、Container 等关系必须使用带域/作用域的强类型引用。
- Registry 只负责 `EntityUuid`、当前 ECS Entity 和有效 typed projection 的登记、解析、冲突
  检测和清理，不拥有 Player/NPC/Projectile 状态。
- 网络复制游标、脏标记和快照键继续是 typed projection，不进入领域组件。

### 排除项

`RenderStateComponent`、`PresentationHistoryComponent`、`rotation`、`gfxOffY`、
`oldPosition`/`oldVelocity`、`oldPos`/`oldRot`/`oldSpriteDirection`、Tile 液体储存状态和
协议临时对象不进入服务器共享权威组件。

## 分阶段数据流

1. Spawn/commit System 创建领域实体并初始化其所需组件。
2. Input、AI、装备和环境 System 写入意图或领域输入状态。
3. Movement/Physics/Combat System 读取最小组件集合，写入位置、速度、生命或效果结果。
4. Lifecycle System 消费领域结果，决定 Player 复活、NPC 消失或 Projectile 结束。
5. Replication/Compatibility Adapter 从权威组件生成 typed projection；失败必须显式返回，不能
   静默绑定到新实体。
6. Presentation 层只从权威快照计算或缓存表现/历史数据，不反向写入服务器权威状态。

## 分阶段实施顺序

1. 建立逐字段映射、写者/读者/生命周期清单，确认现有重复类型和双写路径。
2. 完成 Player/NPC Health、Defense、Immunity、效果集合边界，并保留各自生命周期。
3. 完成 Projectile 定义、行为、寿命、归属、伤害、穿透和专用方向的权威来源收敛。
4. 收敛 InputIntent、Velocity、Location、Physics 和公共/专用方向的访问边界。
5. 完成 Item/Inventory/Hands/Equipment/Weapon/Container 的字段迁移、命令和事件适配。
6. 审计 Targeting、AI、Identity Registry、协议投影、复制和持久化的作用域与解析失败。
7. 补齐各领域边界测试，删除迁移完成后的重复权威字段和旧类型。

每个阶段都必须保持受影响项目可编译，并在阶段末运行对应的 focused verifier；不得把所有
迁移合并成一个无法定位回归的大 diff。

## 验收标准

- 空间查询和基础移动的消费者无需按 Player/NPC/Projectile 类型分支。
- Health、Defense、Immunity、Projectile Damage、Projectile Lifetime 和各类生命周期之间
  没有语义混用或双写权威来源。
- Item 定义、堆叠、库存、手部、装备、武器和容器的更新原因与所有权可独立追踪。
- 所有跨实体关系和外部标识带有明确类型和作用域；投影解析失败可区分并显式处理。
- 领域状态、复制状态、兼容状态和表现/历史状态没有循环依赖。
- 现有行为验证保持通过；新增测试覆盖边界值、拒绝、不变量、生命周期和迁移兼容性。
- 编译命令遵循 `AGENTS.md` 的 `BUILD-CONCURRENCY-1`，产物只位于 `Build/bin/` 和相关构建目录。

