# 实体定义与组件协作

## NSSLC ECS 组织入口

`Terraria.Entity.EcsServer` 是 NSSLC 自身的 ECS 组合边界。它接收一个由宿主提供的
Arch `World` 和官方 `Arch.System.ISystem<EcsServerTickContext>` 集合，内部使用官方
`Arch.System.Group<EcsServerTickContext>` 按注册顺序执行 `BeforeUpdate`、`Update` 和
`AfterUpdate`。它不创建服务器时钟、线程、网络连接或文件访问，也不替代宿主的外部 Tick
循环。

宿主负责：

- 创建或加载 Arch `World`；
- 决定每个 Tick 的编号和 `DeltaTime`；
- 将领域系统按明确的读写和提交顺序注册到 `EcsServer`；
- 在世界切换或关闭时调用 `Dispose`。

`EcsServer` 负责：

- 初始化已注册的 Arch 系统；
- 拒绝未初始化、非连续 Tick、重入和已释放状态；
- 只在完整的 `BeforeUpdate → Update → AfterUpdate` 流程成功后推进 `LastTick`；
- 发生系统异常后停止继续推进，等待宿主释放该 ECS 世界。

需要阶段顺序时使用 `EcsServer.Create` 和 `EcsServerPhase`。每个阶段内部由官方
`Arch.System.Group<EcsServerTickContext>` 按注册顺序运行；阶段只表达 NSSLC 的可见性和
顺序边界，不创建第二个服务器时钟或线程。

```csharp
EcsServer server = EcsServer.Create(
    world,
    [
      new(EcsServerPhase.Preparation, inputSystem),
      new(EcsServerPhase.Simulation, attributeTickSystem),
      new(EcsServerPhase.Simulation, gravityTickSystem),
      new(EcsServerPhase.Simulation, spatialSimulationCoordinatorSystem),
      new(EcsServerPhase.Commit, worldInteractionSystem)
    ]);
```

世界交互系统通过 Arch `World.Query` 消费实体上的 `WorldInteractionRequestComponent`，并把
结果写回 `WorldInteractionResultComponent`。它应在 `Commit` 阶段调用专用地图端口；地图
tile 数据、WorldFile 和网络 section 不迁移到 Arch Entity。

这个入口只组织 NSSLC 的 Arch 系统，不依赖 `MinimalImplementation`。尚未实现完整行为的
AI、Combat、Item、Liquid、Movement、Physics 和 World 系统不会因为注册入口存在而被伪造为
已接入；它们应在各自的系统合同明确后加入同一个官方 `Group`。

实体定义描述一种实体的初始组件组合；它不代表某个运行时实体，也不保存 Arch `World`、
运行时句柄或可变实例状态。实体实例仍由 ECS 运行时以身份和组件集合表达。

空间实体使用 `LocationComponent`、`Movement.VelocityComponent` 和
`Physics.ColliderComponent`。这三个类型已随物理大类归位：
`LocationComponent`、以及旧的 `PositionComponent`/`VelocityComponent` 迁移投影，
于 2026-11-01 移入 `Physics/Movement/`（命名空间保持不变）。
它们不再位于本目录，本域只保留身份、定义、运行时句柄与 ECS 入口。

```text
EntityDefinition（例如 Slime）
  ├─ LocationComponent
  ├─ Movement.VelocityComponent
  ├─ ColliderComponent
  ├─ LifeComponent
  ├─ TargetComponent
  └─ AiComponent（SlimeAiBehavior）
```

## 类型定义和运行时协作

| 层 | 责任 |
| --- | --- |
| `EntityDefinition` | 类型标识、初始组件工厂、必需组件和 AI 声明 |
| `EntityDefinitionBuilder` | 防止同一组件重复注册，检查定义级依赖 |
| `EntityDefinitionRegistry` | 注册和查找不可变定义；不保存实例 |
| `IEntityDefinitionHost<TEntity>` | 由服务器实现，把完整组件集合原子映射到 Arch 实体并登记身份 |
| `EntityDefinitionSystem<TEntity>` | 读取定义、创建独立组件值并调用宿主创建/销毁入口 |
| `AiEntitySystem` | 为带 `AiComponent` 的实体维护独立绑定，读取宿主快照并返回结果 |
| 运动、物理、战斗系统 | 读取组件和 AI 意图，按各自规则提交真实状态变化 |
| 调度层 | 安排感知、AI、移动/物理、战斗和同步阶段 |

`EntityUuid` 是一次权威实体实例的身份根，由服务器创建流程生成；它不等同于 Arch
`Entity`、网络槽位或存档身份。`EntityRuntimeHandle` 是带有 `WorldScope` 和 `Generation`
的运行时投影：`WorldScope` 防止上一世界的引用进入当前世界，`Generation` 防止同一槽位
复用后旧引用解析到新实体。实体销毁或世界切换时，宿主必须使旧 handle 失效。
`EntityIdentityComponent` 只保存 `EntityUuid`，不保存 Arch 句柄或网络字段。

实体定义系统只管理**组成契约**。它不在每个 tick 更新位置，不运行 AI，也不处理碰撞和
伤害。组件保存状态，系统维护规则；一个位置组件可以被 AI、战斗、视野和网络系统读取，
但实际位置由运动/物理能力按明确提交阶段更新。

## 创建定义

```csharp
using Terraria.Ai.Examples;
using Terraria.Combat;

EntityDefinition slime = new EntityDefinitionBuilder(new EntityDefinitionId("slime"))
    .AddValue(Terraria.Entity.LocationComponent.At(0, 0))
    .AddValue(Terraria.Movement.VelocityComponent.Stationary)
    .AddValue(new ExampleTargetComponent(default, false, false, 0))
    .AddValue(new ExampleMovementComponent(default, false, false))
    .WithAi<SlimeAiBehavior>()
    .Require<Terraria.Entity.LocationComponent>()
    .Require<ExampleTargetComponent>()
    .Require<ExampleMovementComponent>()
    .Build();

EntityComponentSet initial = slime.CreateComponents();
// IEntityDefinitionHost<TEntity>.Create 在这里按 Values 写入 Arch；本层不依赖 Arch。
```

`Add` 接收工厂，每次 `CreateComponents()` 都会创建独立值。定义中不保存上一次创建的组件
实例，避免多个实体共享可变数组、集合或运行时状态。`AddValue` 适合不可变值或原型默认值；
可变组件应使用 `Add(() => new Component(...))`。

`Require<T>` 是定义级组合检查。如果行为需要从多个组件投影出某个 AI 输入，定义应声明
这些源组件；`AiSnapshotRequirements` 只描述快照 Provider 必须提供的值。AI 运行时仍会检查
实际快照，组件被移除或禁用时不会用默认值掩盖错误。

也可以在定义构建时把已知的 AI 需求加入同一份组合契约：

```csharp
AiRequirements slimeInputs = AiRequirements.Empty
    .Require<ExampleMovementComponent>()
    .Require<ExampleTargetComponent>();

EntityDefinition slime = new EntityDefinitionBuilder(new EntityDefinitionId("slime"))
    .AddValue(Terraria.Entity.LocationComponent.At(0, 0))
    .AddValue(Terraria.Movement.VelocityComponent.Stationary)
    .AddValue(new ExampleMovementComponent(default, false, false))
    .AddValue(new ExampleTargetComponent(default, false, false, 0))
    .WithAi<SlimeAiBehavior>(slimeInputs)
    .Build();
```

这里的 `AiRequirements` 是快照输入的显式契约，不会创建行为实例或把规划器放进实体定义。
宿主可以从多个权威组件生成这些投影；运行时 `AiBinding` 仍会再次检查每个 `AiSnapshot`。

## 每帧协作

```text
Arch 查询实体组件
      ↓ 复制只读输入
AiSnapshot
      ↓
AiEntitySystem.Calculate(entity, component, snapshot, dt)
      ↓
AiEntityResult<TEntity, TIntent>
      ↓ 保留实体键和意图顺序
Movement / Physics / Combat 系统校验并提交
      ↓
下一阶段读取新的 `LocationComponent` 等权威状态
```

`AiEntitySystem` 只维护 `entity → AiBinding` 的运行时关联，不保存位置、速度或其他实体
组件。它不依赖 Arch 的 `Entity` 类型，宿主可以用完整的 Arch 实体句柄或带作用域/generation
的自有键作为 `TEntity`。实体槽位复用、世界切换和实体删除前必须先 `Detach`；不要把旧绑定
留给新的实体实例。

如果组件移除是通过查询发现的，可以调用 `TryDetach`；它在绑定已不存在时返回 `false`，避免
把生命周期清理错误误判为实体计算失败。绑定的输入要求可通过 `GetRequiredComponents` 交给
`AiSnapshot` Provider，Provider 不持有 AI 规划器。

建议阶段顺序是：

```text
感知/快照 → AI 计算 → 移动和物理 → 战斗/交互 → 网络同步
```

AI 产生的 `MoveTo`、`Jump`、`BeginAttack` 等是请求数据，不是已经成功的状态。运动、物理和
战斗系统必须重新检查实体有效性、目标版本、范围、碰撞和资源。真实执行结果在下一 tick
投影回快照；AI 不直接写 `LocationComponent`。

## 生命周期

```csharp
AiEntitySystem<MyEntityKey, ExampleAiIntent> aiEntities = new(aiSystem);
aiEntities.Attach(entityKey, aiComponent);

AiEntityResult<MyEntityKey, ExampleAiIntent> result =
    aiEntities.Calculate(entityKey, aiComponent, snapshot, deltaTime);

// 移除 AiComponent、销毁实体或卸载世界：先消费停止意图，再释放绑定。
AiEntityResult<MyEntityKey, ExampleAiIntent> stopped = aiEntities.Detach(entityKey);
```

行为声明改变时不要继续复用旧绑定：

```csharp
AiEntityResult<MyEntityKey, ExampleAiIntent> stopped =
    aiEntities.Rebind(entityKey, newAiComponent);
// 先消费 stopped.Result.Intents，再继续用新声明 Calculate。
```

世界卸载可以先调用 `StopAll()`，消费每个结果，再调用 `Dispose()`。`StopAll()` 不保证实体
遍历顺序；如果提交顺序重要，宿主应按自己的调度顺序排序结果。`Dispose()` 是资源清理入口，
不再返回新的结果。

## 不应合并的职责

- 位置、速度和碰撞状态属于实体组件，不应放进 AI 系统。
- AI 系统不应持有 Arch `World`、实体组件引用或运动/战斗服务。
- 实体定义不应变成处理所有实体行为的总控系统。
- Arch Archetype 是存储和查询布局，不等同于业务实体类型；多个定义可能具有相同组件集合。

实体定义和 AI 协调代码仍然不依赖 Arch。`EcsServer` 是同目录提供的显式 Arch.System
组织适配器：宿主把需要运行的 `ISystem<EcsServerTickContext>` 按顺序传入，由服务器宿主
拥有时钟并调用 `Initialize`/`Update`。它只组织已经注册的系统，不自动发现 Share 系统，
也不负责网络、文件、持久化或服务器后台线程。结构变化由同目录的
`EcsServerCommandBuffer` 记录并在宿主确定的边界播放。

一个最小系统可以直接通过 Arch 查询组件：

```csharp
using Arch.Core;
using Arch.System;
using Terraria.Entity;
using Terraria.Movement;

public sealed class MovementUpdateSystem : BaseSystem<World, EcsServerTickContext>
{
  public MovementUpdateSystem(World world) : base(world) { }

  public override void Update(in EcsServerTickContext context)
  {
    var query = new QueryDescription().WithAll<MovementCapabilityComponent,
        VelocityComponent>();
    context.World.Query(in query, (ref MovementCapabilityComponent capability,
        ref VelocityComponent velocity) =>
    {
      if (capability.Enabled)
      {
        // 读取输入并生成空间意图；实际速度写入由空间提交系统负责。
      }
    });
  }
}
```

上例只展示组织边界和查询入口；真实移动应使用 `SpatialSimulationSystem` 与
`SpatialSimulationCoordinatorSystem` 的快照/提交合同，不应在示例回调中重复实现空间规则。




