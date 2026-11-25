# NSSLC 共享组件领域

`src/NSSLC` 是 `Terraria.Components.csproj` 的共享组件与领域契约源码根。ECS 领域按
归属分两层落位：端别无关的共享能力域收在 `Share/`（`AI`、`Attributes`、`Combat`、
`Entity`、`Inventory`、`Item`、`Physics`），其余领域直接位于项目根目录；`Components/`
收容层不再存在。目录只表达领域归属，
C# 命名空间保持原样，目录移动不改变公共 API。这里的代码遵循参考 ECS 项目中稳定的
分层方式：组件是数据，定义是原型，系统拥有规则，运行时存储负责实体和组件关联。需要
Arch 运行时的交互系统会把 Arch 句柄限制在明确的适配入口；它不复制最新客户端的业务
命名、客户端生命周期或平台对象。

## 目录

### `Share/` 下的共享能力域

| 目录 | 内容 |
| --- | --- |
| [AI](Share/AI/README.md) | HTN/GOAP 统一计算契约、AI 绑定和有序意图 |
| [Attributes](Share/Attributes/README.md) | 基础属性、来源分类、限时属性修饰器、Buff 与状态效果 |
| [Combat](Share/Combat/README.md) | 生命、伤害、减伤、无敌窗口与战斗状态组件 |
| [Entity](Share/Entity/README.md) | 实体定义、组件组合、初始化契约、实体生命周期边界和 Arch 组织入口 |
| [Inventory](Share/Inventory/README.md) | 广义库存能力、槽位规则、跨实体物品转移、手持槽与物品使用（门闸、弹药、通道） |
| [Item](Share/Item/README.md) | 物品定义、无实体物品堆和实体化物品状态 |
| [Physics](Share/Physics/README.md) | 碰撞体、空间查询与模拟、接触事实、液体和运动/重力 |

### 项目根下的领域

| 目录 | 内容 |
| --- | --- |
| [Chat](Chat/) | 聊天请求、命令分派、有界历史和广播队列 |
| [Npc](Npc/) | NPC AI 事实、意图、引导行为和运行时适配 |
| [Player](Player/) | 玩家输入请求、战斗结果、死亡与重生状态 |
| [Projectile](Projectile/) | 投射物行为、生命周期、AI 与战斗接线 |
| [World](World/README.md) | 世界运行时、Tile 存储、交互请求和投影合同，以及刷怪、会话与 Tile 实体三个语义子域 |
| [WorldItem](WorldItem/) | 世界物品存储、请求队列和 tick 系统 |

### `World/ComponentSystem/` 下的语义子域

刷怪、会话和 Tile 实体不独立成项目根领域，而是 `World/ComponentSystem/` 下的语义子域：
它们与世界运行时共享同一权威世界边界，按 [ECS 文件组织设计约束](../../Context/架构约束/ECS文件组织设计约束.md)
§3.4 视为独立领域再分。

| 子域 | 内容 | 命名空间 |
| --- | --- | --- |
| [EntitySpawn](World/ComponentSystem/EntitySpawn/) | 刷怪预算、区域规则、随机数和刷怪事实 | `Terraria.EntitySpawn` |
| [Session](World/ComponentSystem/Session/) | 会话身份、在线玩家投影和只读玩家来源端口 | `Terraria.Session` |
| [TileEntity](World/ComponentSystem/TileEntity/) | 箱子、告示牌等 Tile 实体记录与请求/结果队列 | `Terraria.World` |

`Share/Attributes/Buff`、`Share/Attributes/Contribution`、`Share/Physics/CollisionGeometry`、
`Share/Physics/ContactFact`、`Share/Physics/Liquid`、`Share/Physics/Movement`、
`Share/Physics/SpatialQuery` 和 `Share/Physics/SpatialSimulation` 是领域内按稳定职责划分的
子目录。`Movement` 与 `Liquid` 的命名空间分别为 `Terraria.Movement` 和 `Terraria.Liquid`，
与父目录 `Terraria.Physics` 不同；这是目录与命名空间解耦的既定事实，不在目录移动时顺手修正。
`Share/Inventory` 同理：物品使用相关文件已并入该目录，命名空间仍是
`Terraria.Share.Items`（使用档案）与 `Terraria.Share.Player`（请求、门闸状态与事实）。

服务器诊断日志不是 ECS 领域状态，已迁到
[NSSLC.Infrastructure/Diagnostics](../NSSLC.Infrastructure/Diagnostics/README.md)，由
`NSSLC.Infrastructure.Diagnostics.csproj` 编译；通信域已迁到
[NSSLC.Network/Communication](../NSSLC.Network/Communication/)。本表只列
`Terraria.Components.csproj` 编译的 ECS 领域。

## 领域内目录

有 `*System.cs` 的领域，根目录只放主系统文件（`*System.cs`）和本 `README.md`，其余源码
按四类职责归档：

| 目录 | 内容 |
| --- | --- |
| `Component/` | 组件：挂在实体或世界上、跨 tick 持续的状态 |
| `Command/` | 命令：请求、意图、命令/请求队列及其种类标识 |
| `Query/` | 查询：只读查询接口与只读端口 |
| `System/` | 系统附属文件：规则、求解器、运行时与存储服务、定义与编目、事实/结果/快照、枚举与值对象 |

没有 `*System.cs` 的目录（`Share/Item`、`World/ComponentSystem/Session`、`Share/Physics`、
`Share/Physics/Liquid`、`Share/Physics/SpatialQuery`、`Share/Attributes/Contribution`）
保持扁平，不建空的四类目录。`Share/AI/BasicFramework/Goap`、
`Share/AI/BasicFramework/Htn` 是移植库，`Share/AI/BasicFramework/Examples` 是示例集，三者保留来源结构。
规则正文见 [ECS 文件组织设计约束](../../Context/架构约束/ECS文件组织设计约束.md) §3.4。

## 一次实体更新

```text
EntityDefinition
  → EntityFactory / ECS runtime
  → 组件查询与快照
  → AiEntitySystem.Calculate
  → 运动、物理、战斗系统消费意图
  → 组件权威状态提交
  → 下一阶段和下一 tick 查询新状态
```

Inventory 是共享的持有能力。`InventoryComponent` 通过槽位定义组合出背包、饰品栏、武器
弹药栏、猪猪存钱罐和箱子；`InventorySystem` 负责类别匹配、堆叠、拾取、取出和跨库存移动。
槽位可以保存轻量 `ItemStack` 值，也可以保存带 `EntityUuid` 的完整物品实体；后者保留武器
的弹药库存和其他实例组件。组件保存槽位状态，系统负责跨实体写入，网络、存档和 UI 位于
后续适配边界。

玩家装备不再有单独的 `PlayerEquipmentComponent`。物品所在的武器、护甲或饰品槽由
`InventoryComponent` 的槽位定义和物品归属关系唯一表达；装备效果提交给
`AttributeSetComponent` 时才产生属性修饰器。

位置组件不属于 AI 系统。AI 和其他系统读取位置的值或只读投影，运动/物理系统按调度契约
提交实际位置。AI 返回的是 `MoveTo`、`Jump`、`BeginAttack` 等请求数据，不能直接修改
`LocationComponent`。

`Terraria.Combat.VelocityComponent` 仅保留为旧调用方的过渡投影，已标记为过时，并转换到
`LocationComponent` 与 `Movement.VelocityComponent`。新的实体定义和系统不应再把它作为
权威组件加入组合。同源的 `Terraria.Combat.PositionComponent` 已于 2026-11-01 删除：它同样
是零消费者的过时投影，而位置已有 `LocationComponent` 唯一权威，留下迁移壳只会让后来者误以为
存在第二条位置写入路径。

重力属性使用 `Share/Physics/Movement/Gravity*` 类型表达。`GravityConfigurationComponent` 保存基础值，
`GravityModifierCollectionComponent` 保存带来源和时限的活动修饰器，
`GravityModifierCalculator` 计算本帧投影，`GravityModifierSystem` 管理时限和方向切换。
重力方向是离散状态，方向切换资格是能力，二者不应通过普通数值加减表示。移动、碰撞、跳跃
和网络系统仍由宿主消费 `GravityStateComponent`，当前共享层没有替换原版玩家运行时。

属性修饰使用 `Share/Attributes/AttributeSetComponent` 作为能力门槛。组件内部以 `List` 保存基础
值和活动修饰器，并通过 `AttributeCalculator` 按加算、乘算、覆盖的固定顺序计算当前值；
`AttributeSystem` 负责来源移除和时限推进。Buff、装备、饰品、武器、Boss、敌怪、NPC 和
友好生物只是来源分类，不构成实体继承树。Buff 的周期行为和战斗系统的受击结算仍由各自
领域负责。Player 不拥有装备槽或 Buff/Debuff 数值的私有状态组件；服务器的
`AttributeSetComponent` 保存 Player 基础防御和基础抗性，受击免疫窗口由共享
`ImmunityWindowComponent` 按 6 个伤害通道分槽保存（NPC 侧按攻击者分槽），Inventory 与 Buff
的数值贡献由 `AttributeModifier` 聚合，免疫窗口由 Combat 规则消费。生命池不进入属性系统；当前生命、最大生命、最小生命和存活状态由
`Share/Combat/LifeComponent` 单独承载。`LifeRegeneration` 仅可表示恢复速率贡献，不能表示生命池
本身。

## 风格和命名

- 公开类型使用 PascalCase，文件名与核心类型一致；私有字段使用 `_camelCase`。
- 使用明确的领域名：`EntityDefinition`、`AiBinding`、`AiEntitySystem`，不使用客户端特有的
  状态缩写或按网络包编号命名。
- 组件只保留实体状态；工厂、注册表和绑定对象属于运行时边界，不能伪装成组件字段。
- 定义和绑定不依赖 Arch 类型。服务器宿主负责把值集合和快照适配到具体 ECS。
- 可变实例不在定义之间共享；每次创建使用独立工厂或独立值副本。

## 权威边界

本项目中的组件和定义可以被多个运行时使用，但不提供平台 I/O、网络发送或隐藏的全局
实体写入。`InventorySystem` 是显式的 Arch 交互入口，负责其声明的库存不变量。实体删除、
组件移除和世界卸载时，宿主先停止相关运行态，消费清理结果，再释放绑定和索引。

`Share/Entity/EcsServer` 提供可选的 Arch.System 组织边界：它保存一个 Arch `World` 和一个
按注册顺序执行的 `Group<EcsServerTickContext>`，由服务器宿主显式调用 `Initialize` 和
`Update`。它不创建服务器时钟、后台任务、网络连接或持久化流程。未实现 Arch.System 适配器
的领域仍由宿主通过各自的共享 API 调用；加入 `EcsServer` 不会自动让所有已注册系统运行。

`World/WorldInteractionSystem` 是已接入的局部适配器：它在 `Commit` 阶段通过 Arch
`World.Query` 消费实体上的世界交互请求，并将结果写回组件，再调用专用地图端口。它不把
tile map、WorldFile 或网络 section 复制到 Arch Entity；完整世界行为仍需按领域合同继续接入。

Arch 不会因为实体拥有某个组件就自动更新它。`World` 和查询负责存储、筛选及访问组件，
NSSLC 的组织者负责把已经注册的系统按阶段组合起来。组件是数据能力，不要求“一组件一系统”；
持续模拟由相应系统更新，请求型库存、拾取和交互操作在明确的请求提交阶段处理。

实体身份使用 `EntityUuid` 作为一次实例的根身份，`EntityRuntimeHandle` 作为带世界作用域和
generation 的短生命周期投影。Arch `Entity` 只在宿主适配层使用，不从共享实体组件公开。
