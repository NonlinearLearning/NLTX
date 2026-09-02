# 游戏 ECS 文件组织

> 历史参考文档：本文件保留早期 ECS 组织原则与示例。
> 仓库当前正式约束以 [ECS文件组织设计约束.md](ECS文件组织设计约束.md) 为准。

## 目标

本规范只组织游戏 ECS 代码。它不涵盖 ECS 运行时、网络、渲染、UI、持久化、构建、
工具或资源管线。

目录导航的首要问题应是“这是哪一项游戏规则”，而不是“这是什么技术类别”。因此，
第一层目录按游戏领域命名；每个领域内部使用统一的 ECS 子目录。

## 标准目录

```text
src/
  <领域>/
    Components/
    Systems/
    Events/
    Commands/
    Snapshots/
    Definitions/
```

`<领域>` 使用游戏语义命名，例如 `Player`、`Npc`、`Projectile`、`Tiles`、`Combat`、
`Movement`、`Physics` 或 `Inventory`。目录和 C# 类型使用 PascalCase。

只有领域已经拥有相应文件时，才创建该子目录。不要为了目录完整性创建空目录。

## 子目录职责

| 目录 | 放置内容 | 生命周期和可变性 |
| --- | --- | --- |
| `Components/` | 实体或世界的持续运行时状态 | 随 simulation tick 改变 |
| `Systems/` | 读取状态、处理事件、计算规则并写出结果的逻辑 | 代码固定，按调度执行 |
| `Events/` | 已发生的一次性游戏事实 | 短暂存在，不作为持续状态 |
| `Commands/` | 尚未提交的确定性状态变更意图 | 由系统产生，提交后失效 |
| `Snapshots/` | 系统计算使用的稳定只读输入视图 | 在 tick 边界创建、替换或失效 |
| `Definitions/` | 类型、原型或静态规则的不可变定义 | 跨实例复用，通常不随 tick 改变 |

一个目录中的文件应具有单一职责。不要把 `DamageEvent` 放进 `Components/`，也不要把
`TileChangeCommand` 放进 `Events/`。

## 领域优先的规则

不要建立全局技术目录：

```text
src/
  Components/
  Systems/
  Events/
  Commands/
  Snapshots/
```

这种结构会将同一玩法的状态、行为和消息分散到多个位置。修改 `Tiles`、`Combat` 或
`Projectile` 时，维护者无法只在一个领域内完成理解、修改和验证。

应采用领域优先的结构：

```text
src/
  Tiles/
    Components/
    Systems/
    Events/
    Commands/
    Snapshots/
    Definitions/

  Combat/
    Components/
    Systems/
    Events/
    Commands/

  Projectile/
    Components/
    Systems/
    Events/
```

## 共享组件的归属

实体不通过 `PlayerEntity`、`NpcEntity`、`ProjectileEntity` 的继承体系共享游戏状态。
实体由组件组合而成；共享组件应放在其表达的游戏能力领域中。

| 组件 | 推荐位置 | 原因 |
| --- | --- | --- |
| `PlayerInputComponent` | `Player/Components/` | 仅表达玩家控制 |
| `NpcAiComponent` | `Npc/Components/` | 仅表达 NPC 行为状态 |
| `ProjectileOwnerComponent` | `Projectile/Components/` | 仅表达投射物与发射者关系 |
| `HealthComponent` | `Combat/Components/` | 表达可受伤的战斗能力，可由多个实体拥有 |
| `DamageModifierComponent` | `Combat/Components/` | 表达伤害结算规则，而非实体类别 |
| `VelocityComponent` | `Movement/Components/` | 表达移动能力，可由玩家、NPC 和投射物拥有 |
| `ColliderComponent` | `Physics/Components/` | 表达碰撞能力，可由任何可碰撞实体拥有 |
| `TransformComponent` | `Entity/Components/` | 最低层实体空间状态，不属于特定玩法 |
| `LifetimeComponent` | `Entity/Components/` 或 `Time/Components/` | 依其主要语义选择实体生命周期或时间规则 |

放置组件时，按以下顺序判断：

1. 组件是否只属于一种实体类别？若是，放入该实体类别领域。
2. 组件是否表达多个实体共享的一项明确游戏能力？若是，放入该能力领域。
3. 组件是否几乎适用于所有实体，且不表达特定玩法？若是，放入 `Entity` 领域。

不要建立泛化的 `Shared/Components/` 目录。它会逐渐变成“暂时不知道放哪里”的堆积处。

## Definitions

`Definitions/` 存放静态玩法定义。它回答“某种东西是什么、初始规则是什么”，而不是
“这个实体此刻处于什么状态”。定义通常通过 Id 被实例状态引用，并由多个实例共享。

例如：

```text
Tiles/
  Definitions/
    TileDefinition.cs
    TileMaterialDefinition.cs
    TileDropDefinition.cs

  Components/
    TileStateComponent.cs
    TileMiningProgressComponent.cs
```

`TileDefinition` 可表达 Tile 的固定碰撞、硬度和掉落规则；某一格 Tile 的 frame、挖掘
进度和是否已被移除，则属于运行时状态。

`Definitions/` 只在存在稳定、跨实例复用的静态规则时创建。若当前迁移仍通过兼容层读取
旧 Terraria 静态表，不要仅为了完整性提前创建该目录。

## Tile 领域示例

下例覆盖确定性 Tile 修改链：

```text
src/
  Tiles/
    Components/
      TileStateComponent.cs
      TileMiningProgressComponent.cs
    Systems/
      TileMiningSystem.cs
      TileChangeCommitSystem.cs
    Events/
      TileBrokenEvent.cs
      TilePlacedEvent.cs
    Commands/
      TileChangeCommand.cs
    Snapshots/
      TileReadSnapshot.cs
      TileWorldSnapshot.cs
    Definitions/
      TileDefinition.cs
```

其数据流为：

```text
TileReadSnapshot
  -> TileMiningSystem
  -> TileChangeCommand
  -> TileChangeCommitSystem
  -> 新的 TileWorldSnapshot
```

`TileBrokenEvent` 表示已提交的事实；`TileChangeCommand` 表示尚待提交的意图。二者不可互换。

## 创建新领域的条件

满足下列任一条件时，可以从既有领域中提取新领域：

- 该规则拥有自己的状态、系统和事件，并且可独立理解。
- 维护者修改该规则时，持续需要绕过原目录中的无关文件。
- 该规则的命名、状态不变量和测试场景已稳定。

不要因为只有一个孤立类型就建立领域。也不要将只有一个临时用途的类型提升为跨领域抽象。

## 文件命名

- 一个核心公开类型一个文件，文件名与类型名一致。
- Component 使用 `Component` 后缀，例如 `HealthComponent.cs`。
- System 使用 `System` 后缀，例如 `DamageResolutionSystem.cs`。
- Event 使用 `Event` 后缀，例如 `DamageRequestedEvent.cs`。
- Command 使用 `Command` 后缀，例如 `TileChangeCommand.cs`。
- Snapshot 使用 `Snapshot` 后缀，例如 `TileReadSnapshot.cs`。
- 静态定义使用 `Definition` 后缀，例如 `NpcDefinition.cs`。

具体 C# 格式和命名仍以 `约束/Google-CSharp-Style-Guide-约束.md` 为准。
