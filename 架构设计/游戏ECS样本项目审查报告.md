# 游戏 ECS 样本项目审查报告

## 范围

审查目录：`C:\Users\shan\Downloads\ECS`

审查对象：

- Space Station 14
- Veloren
- Valence
- Terasology

本报告仅审查游戏 ECS 的文件组织：领域、组件、系统、事件、命令、快照和静态定义。
不将 ECS 运行时、网络、渲染、UI、持久化、构建、工具或工作区工程组织作为结论依据。

## 结论

四个样本虽采用不同语言和 ECS 框架，但共同证明了以下方向：

1. 游戏代码的第一层目录应优先表达领域或玩法能力。
2. Component、System、Event 等是领域内部的职责分类，而不是长期全局导航入口。
3. 多个实体共享的组件，应按其表达的游戏能力归属，而不是按第一个使用它的实体类别归属。
4. 共享模拟规则、服务端专有规则和客户端专有规则可以分开实现，但仍应围绕同一游戏领域聚合。
5. 系统执行顺序应有显式注册或调度依据，不能依赖目录或文件枚举顺序。

据此，本仓库采用以下目录标准：

```text
<领域>/
  Components/
  Systems/
  Events/
  Commands/
  Snapshots/
  Definitions/
```

当前正式约束见 [ECS文件组织设计约束.md](ECS文件组织设计约束.md)；本报告保留为样本审查记录。

## Space Station 14

### 证据

SS14 的 `Doors` 是完整的游戏领域切片：

```text
Content.Shared/Doors/
  Components/
    AirlockComponent.cs
    DoorComponent.cs
    DoorBoltComponent.cs
  Systems/
    SharedAirlockSystem.cs
    SharedDoorSystem.cs
  DoorEvents.cs

Content.Server/Doors/
  Systems/
    AirlockSystem.cs
    DoorSystem.cs

Content.Client/Doors/
  AirlockSystem.cs
  DoorSystem.cs
```

`Content.Shared/Doors/Systems/SharedAirlockSystem.cs` 订阅 `AirlockComponent` 的游戏事件，
协调门的开闭、栓锁、电源和撬门等规则。这说明其目录首先服务于“门”这个玩法领域，
再在领域内部区分 Components、Systems 和 Events。

### 可采纳结论

- 使用领域优先目录，例如 `Tiles`、`Combat`、`Inventory`、`Projectile`。
- 将某领域的状态、规则、事件和命令就近存放。
- 当同一领域需要多运行时实现时，保持领域名一致，而不是按全局技术目录切散。

### 不直接采用的部分

- SS14 的顶层领域数量很大；本仓库不应在没有实际实现时预创建同等粒度的空目录。
- 本次规范不纳入其 Client、Server、数据库或资源目录组织。

## Veloren

### 证据

Veloren 将共享游戏组件和共享游戏系统分置：

```text
common/src/comp/
  health.rs
  inventory.rs
  melee.rs
  physics.rs
  position.rs
  stats.rs
  velocity.rs

common/systems/src/
  buff.rs
  controller.rs
  melee.rs
  phys.rs
  projectile.rs
  stats.rs
```

其 `common/systems/src/lib.rs` 明确声明调度依赖，例如 controller、角色行为、属性、
物理、物理事件与投射物系统之间的先后关系。服务端的游戏专有系统还包含地形、战利品、
宠物和传送等领域。

### 可采纳结论

- `Health`、`Velocity`、`Collider` 等不是 Player、Npc 或 Projectile 的专属状态，应按
  `Combat`、`Movement`、`Physics` 等共享游戏能力归属。
- 游戏系统的执行顺序必须由显式调度关系表达。
- 跨实体共享不要求建立实体继承树。

### 不直接采用的部分

- Veloren 的全局 `comp` 与 `systems` 横向布局适用于其已有的 Rust crate 边界，但会削弱
  单一玩法领域的局部性；本仓库不采用为顶层目录标准。

## Valence

### 证据

Valence 以游戏能力为模块边界，例如：

```text
valence_entity/
valence_inventory/
valence_equipment/
valence_weather/
valence_world_border/
```

`valence_entity` 内部同时保存实体状态、查询、生命周期逻辑和 Plugin 注册；
`valence_server` 则以 `movement.rs`、`action.rs`、`interact_block.rs`、`teleport.rs` 等
文件组织游戏行为。`EntityPlugin` 明确设置实体初始化、追踪数据更新和变更清除的阶段。

### 可采纳结论

- 一项游戏能力应内聚其相关的状态和规则。
- 系统阶段要具名并显式配置，例如输入、模拟、提交和清理。
- 稳定能力可有清晰装配点，但装配机制本身不属于本次游戏文件组织规范。

### 不直接采用的部分

- 每项能力拆为独立 Rust crate 的粒度不适合当前尚无 C# 项目的仓库状态。
- 本次规范不纳入 Plugin、协议或服务端工程结构。

## Terasology

### 证据

Terasology 的游戏功能由模块承载；模块内部组合组件、系统、事件、实体定义和玩法逻辑。
引擎层还定义了 ComponentSystem 生命周期和系统注册机制。

### 可采纳结论

- 游戏功能应形成可独立理解的模块，而不是依附于全局技术目录。
- Component、System、Event、Definition 可以共同构成一个玩法领域的完整实现。

### 不直接采用的部分

- 本次规范不采用其引擎级的反射注册、全局 Context 或 ECS runtime 目录。

## 共享组件审查结论

下列放置方式能维持领域局部性：

```text
Entity/Components/TransformComponent.cs
Movement/Components/VelocityComponent.cs
Physics/Components/ColliderComponent.cs
Combat/Components/HealthComponent.cs
Player/Components/PlayerInputComponent.cs
Npc/Components/NpcAiComponent.cs
Projectile/Components/ProjectileOwnerComponent.cs
```

该结构避免两种退化：

```text
src/Components/
src/Systems/
src/Events/
```

以及：

```text
Shared/Components/
```

前者将一个玩法切散；后者会变成语义不清的收容目录。

## Definitions 审查结论

静态定义在样本中以 prototype、registry、数据表、类型常量或资源定义等不同形式出现。
它们的共同性质是：多个运行时实例复用同一份稳定规则，而系统根据类型或定义 Id 查询它。

因此，本规范将其统一为领域内的 `Definitions/`：

```text
Npc/Definitions/NpcDefinition.cs
Projectile/Definitions/ProjectileDefinition.cs
Tiles/Definitions/TileDefinition.cs
```

实例当前生命值、位置、AI 计时器、剩余穿透数和 Tile 挖掘进度属于 `Components/`；
固定最大生命值、基础伤害、掉落表、碰撞规则和默认参数属于 `Definitions/`。

## 采用边界

本结论只规定游戏 ECS 文件的归属规则。它不要求当前创建任何 ECS 文件、空目录、
项目文件或新的抽象层。实际实现应在出现稳定领域职责时按需创建目录，并遵循本仓库的
C# 风格和构建约束。
