# NSSLC ECS 服务器组织者

`Terraria.Entity.EcsServer` 是 NSSLC 的 Arch 运行时组织边界。它使用
`Arch.System.Group<EcsServerTickContext>`
保存一组已注册的 `ISystem<EcsServerTickContext>`，并按构造时的顺序调用每个系统的
`Initialize`、`BeforeUpdate`、`Update` 和 `AfterUpdate`。

需要明确阶段时，使用 `EcsServer.Create` 和 `EcsServerSystemRegistration`。阶段按照
`EcsServerPhase` 的顺序执行，同一阶段内仍按注册顺序执行：

```csharp
EcsServer server = EcsServer.Create(
    world,
    [
      new(EcsServerPhase.Preparation, inputSystem),
      new(EcsServerPhase.Simulation, attributeSystem),
      new(EcsServerPhase.Simulation, gravitySystem),
      new(EcsServerPhase.Simulation, spatialSimulationCoordinatorSystem),
      new(EcsServerPhase.Projection, replicationSystem)
    ]);
```

旧的 `new EcsServer(world, systems)` 构造方式仍可使用，它把传入系统全部放入
`Simulation` 阶段。

## 边界

`EcsServer` 拥有传入的 Arch `World` 引用，但默认不负责销毁它。只有创建时传入
`ownsWorld: true`，服务器释放时才会释放该 World。服务器宿主拥有时钟、Tick 频率、
线程和停止流程；调用 `Update` 时传入本轮的 Tick 与 `deltaTime`。

```text
服务器宿主时钟
    ↓ EcsServer.Update(tick, deltaTime)
Arch.System.Group
    ↓ 按注册顺序
NSSLC 领域系统
    ↓ Arch 查询 / 计算 / 提交
Arch World 组件状态
```

## 是否需要系统组织层

Arch 的 `World` 不会自动更新组件。组件只是附着在实体上的状态；`World.Query` 只负责按
组件组合找到实体并执行查询回调，也不会替服务器决定执行频率、系统顺序或提交时机。

因此 NSSLC 的权威服务器需要一个**系统组织层**，但不需要一个把所有规则合并在一起的总
系统。当前的 `EcsServer` 使用官方 `Arch.System.Group<EcsServerTickContext>` 作为这一层：

```text
服务器宿主时钟
    ↓
EcsServer.Update(tick, deltaTime)
    ↓
Arch.System.Group
    ├─ 属性/效果生命周期系统
    ├─ 重力与移动准备系统
    ├─ AI 感知与意图系统
    ├─ 空间模拟与提交系统
    └─ 战斗、物品和同步系统
```

这里的“系统组织”表示注册、生命周期和阶段顺序，不表示每个组件必须拥有一个系统：

- 有规则、持续状态或明确副作用的组件组合，才需要对应系统读取或写入；
- 只保存数据、由其他系统消费的组件不需要单独的更新系统；
- 背包转移、拾取、放置等请求驱动操作可以在请求提交阶段调用领域 API，不必每个 Tick 扫描；
- 一个系统应拥有清晰的组件读写责任，避免多个系统同时成为同一字段的权威写入者。

如果需要更细的阶段，可以把多个官方 `Group<EcsServerTickContext>` 嵌套注册到外层
`EcsServer`；这仍然是同一个组织入口，不会创建第二套服务器时钟或后台循环。

它不会：

- 创建后台循环或定时器；
- 从 Arch World 自动发现并注册系统；
- 处理网络包、世界文件、存档或客户端表现；
- 把所有领域规则合并成一个系统；
- 替代各领域的提交不变量。

## 生命周期

```csharp
using Arch.Core;
using Terraria.Entity;
using Terraria.Attributes;
using Terraria.Movement;

World world = World.Create();
EcsServer server = new(
    world,
    [new AttributeTickSystem(world), new GravityTickSystem(world)]);

server.Initialize();
server.Update(tick: 0, deltaTime: 1f / 60f);
server.Update(tick: 1, deltaTime: 1f / 60f);
server.Dispose();
```

`Initialize` 只能成功调用一次。`Update` 要求 Tick 非负；首次更新后，后续 Tick 必须每次递增
一个逻辑步，时间步必须为有限正数。宿主如果发生追赶或跳过，必须先按自己的时钟策略补齐
逻辑更新，不能让限时属性和重力修饰器在一次调用中悄悄少推进多个逻辑步。
系统更新发生异常后，组织者进入故障状态，后续更新会被拒绝；宿主应释放该组织者并按自己的
生命周期策略创建新的运行时。`Dispose` 幂等，并负责释放注册系统；World 是否释放由
`ownsWorld` 决定。

## 系统职责

一个系统负责一项内聚的组件状态变化或领域计算。它可以直接使用 `context.World` 做 Arch
查询，也可以调用已有的快照/提交系统：

```csharp
public sealed class MovementUpdateSystem : BaseSystem<World, EcsServerTickContext>
{
  public MovementUpdateSystem(World world) : base(world) { }

  public override void Update(in EcsServerTickContext context)
  {
    var query = new QueryDescription().WithAll<MovementCapabilityComponent>();
    context.World.Query(in query, (ref MovementCapabilityComponent movement) =>
    {
      // 读取本 Tick 的输入，生成意图或准备空间快照。
    });
  }
}
```

当前已提供两个可以直接注册到 Group 的 Tick 适配器：

- `AttributeTickSystem`：查询 `AttributeSetComponent`，推进限时属性修饰器；
- `GravityTickSystem`：查询完整重力组合，推进修饰器并写入 `GravityStateComponent`。

这两个适配器只负责把已有计算规则接到 Arch 查询。它们不会推断装备、Buff、输入、世界
查询或网络状态，也不会替代 `SpatialSimulationSystem` 的空间提交边界。

`EcsServer` 不规定所有系统都必须每 Tick 修改组件。请求型操作可以由服务器在独占提交阶段
直接调用领域系统；持续模拟系统则由宿主在每轮调用 `Update`。添加/移除组件和销毁实体属于
结构变化，应由宿主按 Arch 的访问约定安排在明确的提交边界。`EcsServerCommandBuffer`
封装 Arch `CommandBuffer`，用于记录一轮更新中的创建、销毁、添加、移除和设置操作；宿主在
确认本轮查询结束后调用 `Playback`，不得把同一个缓冲区重复播放。

## 与现有服务器循环的关系

`EcsServer` 是 NSSLC 的 Arch 组织适配器，不是 `MinimalImplementation` 的替代品。若宿主
已有服务器循环，应在该循环的一个明确阶段调用 `EcsServer.Update`，不要同时再启动第二个
权威循环推进相同的 World。阶段内的系统顺序由传入 Group 的顺序决定；文件名和目录顺序不
构成调度依据。
