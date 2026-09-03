# Entity 派生类同质组件拆分设计草案

> 本文是设计草案，不是执行计划。它定义目标组件模型、状态所有权和接口边界，
> 不要求在本轮修改 C#，也不规定提交顺序。

## 1. 设计目标与非目标

### 目标

- 将参考 `Entity`、`Player`、`Projectile`、`NPC` 的公共概念映射为可组合、可查询的 ECS 状态。
- 让位置、速度、碰撞几何和水平朝向等真正同质的数据由公共组件表达。
- 让 Player、NPC、Projectile 的生命周期、网络复制、AI、所有者和伤害语义保持领域隔离。
- 分离权威状态、派生查询、兼容投影和表现数据，避免继承式字段重新聚合为巨型组件。
- 为 Movement、Combat、Lifecycle、Replication 建立低耦合 seam。

### 非目标

- 不把所有参考类字段逐一搬入 ECS。
- 不创建 `BaseEntityComponent`、`GenericLifetimeComponent`、公共 `WetComponent` 或公共
  `NetUpdateComponent`。
- 不把 Player、NPC、Projectile 的行为塞回组件方法或继承层次。
- 不在本草案中删除现有组件、改动生成路径或运行测试。

## 2. 当前上下文

参考继承关系来自只读目录：

```text
D:\TRbackup\Version4\Terraria\Entity.cs
    ├── Player.cs
    ├── Projectile.cs
    └── NPC.cs
```

当前权威运行时是 `dome/src/Terraria.Dome.Simulation` 的 Arch ECS。Player、NPC 和两类
Projectile 均已通过组件组合创建，公共组件主要位于：

```text
dome/src/Terraria.Dome.Simulation/Components/Entity/
```

设计依据上一份审查报告中的字段盘点：

- `position`、`velocity`、`width`、`height` 是跨三类实体共享的空间概念。
- 水平 `direction` 可以共享，但 Projectile 的特殊方向、NPC 的垂直方向和表现用
  `spriteDirection` 不应被强行统一。
- Player/NPC 的生命状态同质；Projectile 的 `damage` 是攻击输出，不是生命状态。
- `active`、`timeLeft`、`netUpdate` 虽然同名或相近，但生命周期和所有权不同。

## 3. 公共组件命名约束

`src/Share/Entity/Components` 已经定义了跨模块公共组件的规范名称；后续设计保持这些
名称，不再为同一概念创建第二套公共命名：

| 公共规范名称 | 语义 | 当前 Simulation 同义旧名 | 设计处理 |
| --- | --- | --- | --- |
| `LocationComponent` | 世界位置 X/Y | `TransformComponent` | 公共名称保持 `LocationComponent`；旧名只在迁移/适配边界出现 |
| `VelocityComponent` | 每 tick 速度 X/Y | `VelocityComponent` | 名称和路径保持不变 |
| `ColliderComponent` | Hitbox 宽高 | `ColliderComponent` | 名称和路径保持不变 |
| `DirectionComponent` | 水平朝向 | `FacingComponent` | 公共名称保持 `DirectionComponent`；Projectile 特殊方向另行适配 |
| `LiquidComponent` | 液体接触及计时基础状态 | Player/NPC 分域环境状态、Projectile 液体策略 | 名称保持，但不把实体特有规则塞入公共组件 |
| `EntityIdentityComponent` | 进程内实体 UUID | 各域 Identity/Replication 字段 | 名称保持；网络、持久化、账户 ID 继续分离 |

规范文件位置：

```text
D:\TRbackup\NLTX\src\Share\Entity\Components
```

`dome/src/Terraria.Dome.Simulation/Components/Entity` 中已经存在的同义类型，在实际迁移
设计中应视为待收敛的旧名，而不是继续新增公共类型。该命名收敛不改变本草案对生命周期、
网络复制和领域行为的隔离判断。

## 4. 目标组件拓扑

```text
                         ┌────────────────────────────┐
                         │ Arch Entity + typed Handle │
                         └──────────────┬─────────────┘
                                        │
              ┌─────────────────────────┼─────────────────────────┐
              v                         v                         v
     ┌────────────────┐        ┌────────────────┐        ┌────────────────────┐
     │ Spatial values │        │ Shared combat │        │ Identity boundary  │
     │ Location       │        │ Health         │        │ EntityUuid root    │
     │ Velocity       │        │ Defense        │        │ Registry + Adapter │
     │ Collider       │        │ Immunity       │        │ typed projections  │
     │ Direction(X)   │        │ Buff policy    │        │                    │
     └───────┬────────┘        └───────┬────────┘        └──────────┬─────────┘
             v                         v                          v
       Geometry /              Combat / damage              Protocol / persistence /
       movement systems        systems                      compatibility adapters
```

其中 `Spatial Core` 是公共组件集合而不是新的聚合组件；图中的分组只表示访问关系，
不表示要创建一个名为 `SpatialCoreComponent` 的容器。

## 5. 组件设计

### 5.1 公共空间组件

#### `LocationComponent`

**权威数据**：实体世界坐标的 X/Y，语义对应参考 `Entity.position`。公共规范名称保持
为 `LocationComponent`，不使用新的 `TransformComponent` 公共名称。

**接口草案**：

```csharp
public struct LocationComponent
{
  public float X;
  public float Y;
}
```

**所有权**：Spawn System 初始化；Movement、碰撞和恢复 System 修改；Geometry Query、
Replication Projection 只读。

**不包含**：中心点、Hitbox、距离等派生值；这些由 Query 计算，避免缓存失效问题。

#### `VelocityComponent`

**权威数据**：每 tick 世界速度，对应参考 `Entity.velocity`。

**接口草案**：

```csharp
public struct VelocityComponent
{
  public float X;
  public float Y;
}
```

**所有权**：Player 控制、NPC 行为、Projectile 行为和碰撞响应分别写入；Movement System
  只负责按统一规则消费速度并推进 Transform。

**不包含**：加速度、重力规则、摩擦或实体类型分支。它们属于 Physics/Movement 或领域 System。

#### `ColliderComponent`

**权威数据**：碰撞宽高，对应参考 `Entity.width/height`。

**接口草案**：

```csharp
public readonly record struct ColliderComponent(float Width, float Height);
```

**所有权**：实体定义和 Spawn System；Geometry Query、Tile Collision、目标判定只读。

**不包含**：碰撞结果、斜坡接触、穿透策略或碰撞回调。

#### `DirectionComponent`

**权威数据**：水平朝向，值域为 -1 或 1，对应参考 `Entity.direction` 的主要用途。
公共规范名称保持为 `DirectionComponent`。

**接口草案**：

```csharp
public struct DirectionComponent
{
  public int Horizontal;
}
```

**所有权**：Player 输入、NPC 行为和需要水平朝向的通用行为 System。

**边界**：

- Projectile 的特殊方向字段不纳入公共同质组件；公共名称保持 `DirectionComponent`。
- NPC 的 `DirectionY`、`OldDirectionY` 留在 NPC Movement 状态。
- `spriteDirection` 属于表现/投影，不成为模拟朝向的别名。

### 5.2 共享战斗组件

#### `HealthComponent`

**适用实体**：Player、NPC。

**权威数据**：Current、Maximum；由 Combat/Damage/Respawn/Death System 维护。

**接口草案**：

```csharp
public struct HealthComponent
{
  public int Current;
  public int Maximum;
  
}
```

**不适用实体**：Projectile。投射物的 `ProjectileDamageComponent` 表示它造成的伤害，
  不能通过挂载 Health 伪造可受伤实体。

**不变量**：

- `0 <= Current <= Maximum`。
- Player 的死亡可进入 respawn；NPC 的死亡通常进入 despawn/掉落；两者由不同生命周期
  System 消费相同的 Health 结果。

#### `DefenseComponent`

**适用实体**：Player、NPC。

**设计**：保留一个小型防御值组件；Player 装备系统和 NPC 定义分别负责提供/重建其值，
  Combat System 只读取最终权威值。

**不包含**：装备来源、NPC 难度缩放、伤害减免策略。这些属于输入定义或纯 Policy。

#### `ImmunityComponent` 与 Buff 组件

免疫时间和 Buff 集合可以在 Combat 层复用基础机制，但免疫键空间、玩家专属增益、NPC
专属抗性和 Projectile 命中冷却仍分域。共享的是纯策略或容器接口，不是所有状态字段。

## 6. 同质组件的访问边界

### 6.1 `EntityGeometryQuery`

**输入**：`LocationComponent`、`ColliderComponent`。

**输出**：中心点、Hitbox、边缘点、距离和方向等派生值。

**约束**：纯函数、无组件写入、无全局可变缓存。调用方不需要知道实体是 Player、NPC 还是
Projectile。

### 6.2 `MovementSystem`

**输入**：Entity、Transform、Velocity，以及由上游系统准备的碰撞/物理上下文。

**输出**：更新 Transform，必要时发出碰撞/接触结果。

**约束**：不读取实体专属生命周期或定义组件来分支公共几何逻辑；实体差异由上游领域
System 先写入速度或意图。

### 6.3 `HealthComponent` 的共享边界

`HealthComponent` 是 Player 与 NPC 之间唯一明确的共享战斗状态候选。Combat System 只依赖
Current/Maximum；伤害来源、死亡后处理和复活/消失行为不属于该组件。

Projectile 的 `damage`、穿透和过期状态是反例，不能因为都出现在战斗流程中就并入 Health。

### 6.4 其他系统的排除原则

生命周期、AI、复制、所有者和表现系统可以读取公共组件，但不因此获得将其专属状态
提升为公共组件的权限。凡是只有一个实体域读写、或终止/恢复语义不一致的字段，继续留在
领域组件中。

## 7. 统一身份根与非同质标识边界

身份不是可机械合并的同质字段。统一的是服务器权威实体的根身份和追踪入口；协议、账户、
持久化与高频索引仍保留各自类型、作用域和生命周期。

### 7.1 权威身份根

`EntityIdentityComponent` 位于 `D:\TRbackup\NLTX\src\Share\Entity\Components`，是所有
持续参与服务器权威模拟的实体（Player、NPC、Projectile 及其他权威实体）的必选公共状态。
其规范字段为 `EntityUuid`（内部可由 128 位 `Guid` 承载）：

- 仅由服务器权威 Spawn/Commit 流程生成；零值非法，不接受客户端、协议或存档指定的值。
- 在单个实体实例生命周期内保持不变；实例销毁、重生、重建、重新发射或断线重连后重建时，
  生成新的 `EntityUuid`。
- 用于日志、诊断、内部事件关联和跨系统追踪；不编码实体类型、时间或网络槽位。
- 预测实体、纯表现实体、插值副本和协议解码临时对象不挂载该组件，可使用独立的
  `PredictionEntityId` 或表现层句柄。

### 7.2 Registry 与 typed projection

服务器身份 Registry 只登记 `EntityUuid`、当前 Arch `Entity` 和有效投影，不拥有领域状态。
每个边界 Adapter/索引维护自己的 typed projection，并通过 Registry 解析到根身份：

| 旧概念/字段 | 新语义与范围 | 处理方式 |
| --- | --- | --- |
| Arch `Entity` | 当前进程 ECS 行引用 | 临时运行时句柄，不作身份根 |
| `PlayerHandle` / `NpcHandle` | Player/NPC 域高频索引 | 保留强类型，经 Registry 映射到 `EntityUuid` |
| Projectile `Identity` | Terraria 协议内投射物配对键 | 重命名为带作用域的 `ProjectileProtocolIdentity`，由协议 Adapter 管理 |
| Projectile UUID | 旧投射物关联字段 | 不再作为第二权威 UUID；需要跨系统关联时映射到 `EntityUuid`，临时/协议用途留在 Adapter |
| `ReplicationId` | 会话/域内复制游标与快照键 | 保留各域 typed projection，不替代 `EntityUuid` |
| Account UUID | 跨连接账户身份 | 保留 `AccountUuid`，通过 Player 持久化身份映射当前 `EntityUuid` |
| Persistent Entity ID | 跨会话存档对象身份 | 独立于运行时 UUID；恢复时建立到新 `EntityUuid` 的映射 |
| `whoAmI` | Terraria 数组/连接槽兼容索引 | 仅在 Compatibility Adapter 使用，不进入 ECS 权威状态 |

禁止用裸 `int` 或裸 `Guid` 表达跨域关系。所有者、目标、复制游标和协议引用必须携带类型与
作用域；投影失效、槽位复用或作用域不匹配时返回显式失败，绝不静默绑定到新实体。

### 7.3 网络、持久化与迁移方向

- `EntityUuid` 默认不进入现有 Terraria 网络包。网络继续发送协议规定的 typed projection；
  未来新增实体引用必须版本化并声明作用域、权限和有效期。
- `EntityUuid` 不作为默认存档主键。持久化恢复先读取 `PersistentEntityId`，再创建新运行时实体
  并登记 `PersistentEntityId -> EntityUuid` 映射；临时 Projectile 不写持久化身份。
- 迁移只允许单向投影：旧协议/兼容字段 → typed projection → `EntityUuid`，或
  `EntityUuid` → typed projection → 旧协议字段。禁止新旧字段双向隐式同步；按域验证后移除旧核心写路径。

### 7.4 解析失败契约

Registry/Adapter 必须区分 `NotFound`、`Expired`、`ScopeMismatch` 和 `Conflict` 等 typed 失败：

- 客户端命令：拒绝并记录安全审计事件；
- 服务器内部事件：隔离/死信并暴露诊断指标；
- 网络快照：丢弃无法解析的投影，等待完整快照或重同步；
- 持久化恢复：标记引用修复失败，禁止自动绑定到新实体。

## 8. 表现与历史数据设计

以下字段不进入当前权威模拟组件：

```text
rotation
gfxOffY
oldPosition / oldVelocity
oldPos / oldRot / oldSpriteDirection
```

它们的候选归属是 Presentation、插值缓存或 Compatibility Projection。只有在明确以下
契约后，才可独立设计 `MotionHistoryComponent` 或表现组件：

1. 采样频率是 tick 还是 Projectile extra update；
2. 写者和清理者是谁；
3. 是否参与网络或持久化；
4. 失效条件和恢复行为是什么；
5. 是否存在只读消费者而没有模拟写者。

在契约缺失时，表现字段只能从当前权威状态计算或由表现层自行缓存。

## 9. 液体状态设计

参考 `Entity` 的 `wet`、`lavaWet`、`honeyWet`、`shimmerWet` 是名称上的公共字段，
但三个实体域的规则不同：

- Player：环境接触影响控制、移动、呼吸和状态效果。
- NPC：环境接触与移动速度、免疫和行为规则相关。
- Projectile：通常由定义中的 `LiquidPolicy`、`IgnoreWater` 和碰撞策略决定。

因此不设计公共 `WetComponent`。共享点放在只读环境查询或液体策略接口，实体域分别保存
需要持续变化的接触状态。

## 10. 接口契约摘要

| 边界 | 最小输入 | 输出 | 禁止事项 |
| --- | --- | --- | --- |
| Geometry Query | Location + Collider | 纯几何值 | 写组件、缓存派生字段 |
| Movement System | Location + Velocity + 物理上下文 | Location 变更/接触结果 | 读取并分支所有领域生命周期 |
| Combat System | Damage Command + Health/Defense/Immunity | Health 变更/事件/命令 | 把来源 damage 当目标 Health |
| 公共身份根 | `EntityIdentityComponent.EntityUuid` | 运行时实体身份 | 不能承载网络、账户或持久化语义 |
| 身份 Registry | 根身份 + 当前 ECS Entity | 登记、解析、冲突检测、清理 | 不拥有领域状态，不返回静默猜测结果 |
| Identity Adapter | 旧字段/协议/持久化 projection | typed projection ↔ `EntityUuid` | 不允许新旧身份双写 |

## 11. 不变量与设计审查标准

### 不变量

- Entity 只有在拥有对应领域生命周期组件时，才由该领域 System 判断活动状态。
- `LocationComponent` 与 `ColliderComponent` 的单位和坐标原点必须一致。
- `HealthComponent` 只附着于可受伤 Player/NPC；Projectile 不附着。
- `EntityUuid`、ReplicationId、协议 identity、账户 UUID、Persistent Entity ID 和 Arch Entity 不可互换。
- `EntityUuid` 仅由服务器权威 Spawn/Commit 创建；零值和客户端指定值非法。
- Registry/Adapter 解析失败必须显式返回 typed 错误，禁止槽位复用后的静默重绑定。
- Query 不得产生结构变化或隐式写入。
- Component 不跨实体执行行为；跨实体操作通过 System、Command 或 Event。

### 接受标准

- 公共组件的消费者无需 `if Player/NPC/Projectile` 类型分支即可完成空间查询和基础移动。
- 所有持续参与权威模拟的实体均可通过 `EntityUuid` 在 Registry 中稳定追踪；预测/表现对象不冒充权威实体。
- Player、NPC、Projectile 生命周期仍可表达各自的终止、恢复、tombstone 和复制顺序。
- 任何表现/兼容字段都能指出权威来源、失效条件和单向数据流。
- 没有新增巨型基础组件、无类型关系字段或循环依赖。

## 12. 保留项与未决问题

以下问题不在本设计中擅自决定：

1. `rotation` 是否需要服务器权威化，还是仅由客户端表现计算。
2. `oldPosition/oldVelocity` 是否需要跨网络复制，及其 tick/extra-update 采样契约。
3. Player 与 NPC 是否需要共同的“活动资格”纯查询；若需要，查询必须适配已有分域
   生命周期组件，不能引入公共可变状态。
4. 液体接触状态是否需要统一只读 Environment Query；当前先保留各域策略。

## 13. 设计结论

目标模型不是“把 Entity 重新拆成更多文件”，而是只识别三类结果：

1. **真正同质的公共状态**：Location、Velocity、Collider、水平 Direction，以及 Player/NPC
   共享的 Health。
2. **明确排除的相似概念**：生命周期、AI、所有者、伤害、复制、液体规则和表现历史，
   因生命周期或读写者不同而不构成同质组件。
3. **单向派生/外部边界**：Geometry Query、Compatibility Adapter 和表现历史缓存。

该设计保留当前 ECS 已有的良好组件粒度，只为公共访问模式提供稳定 seam，不通过继承或
“通用组件”重新聚合彼此不兼容的生命周期和协议语义。
