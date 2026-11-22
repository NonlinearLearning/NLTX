# NSSLC ECS 服务器组织者

`Terraria.Entity.EcsServer` 是 NSSLC 的 Arch 运行时组织边界。它使用
`Arch.System.Group<EcsServerTickContext>`
保存一组已注册的 `ISystem<EcsServerTickContext>`，并按验证后的稳定调度顺序调用每个系统的
`Initialize`、`BeforeUpdate`、`Update` 和 `AfterUpdate`。

需要明确阶段时，使用 `EcsServer.Create` 和 `EcsServerSystemRegistration`。阶段按照
`EcsServerPhase` 的顺序执行；同一阶段内先验证 `Before` / `After` 依赖，再进行稳定拓扑排序。
没有依赖的节点保持原注册顺序，因此增加一条局部依赖不会重排无关系统：

```csharp
EcsServer server = EcsServer.Create(
    world,
    [
      new(EcsServerPhase.Preparation, "input.capture", inputSystem),
      new(EcsServerPhase.Simulation, "attributes.rebuild", attributeSystem,
          after: ["player.input.apply"],
          reason: "Same-tick equipment input must be visible to attribute readers."),
      new(EcsServerPhase.Simulation, "movement.gravity", gravitySystem),
      new(EcsServerPhase.Simulation, "movement.spatial", spatialSimulationCoordinatorSystem,
          after: ["movement.gravity"]),
      new(EcsServerPhase.Projection, "network.replication", replicationSystem)
    ]);
```

Key 由组合根定义，是稳定、唯一且区分实例的 registration identity；不能用类名代替同类
系统的多个实例。`After` 表示依赖节点先执行，`Before` 表示当前节点先执行，依赖只允许
指向同一 phase 的显式 key。启动时会拒绝重复 key、缺失 key、跨 phase 依赖和环，并在异常
消息中给出 phase、节点、边和可诊断的环路径。`OrderedRegistrationKeys` 可用于启动日志和
快照验证；它不是第二套调度入口。

旧的 `new EcsServer(world, systems)` 构造方式仍可使用，它把传入系统全部放入
`Simulation` 阶段。

## 边界

`EcsServer` 拥有传入的 Arch `World` 引用，但默认不负责销毁它。只有创建时传入
`ownsWorld: true`，服务器释放时才会释放该 World。服务器宿主拥有时钟、Tick 频率、
线程和停止流程；调用 `Update` 时传入本轮的 Tick 与 `deltaTime`。

```text
服务器宿主时钟
    ↓ EcsServer.Update(tick, deltaTime)
固定 phase contract
    ↓ Preparation → Simulation → Commit → Projection
phase 内稳定拓扑排序
    ↓
Arch.System.Group
    ↓
NSSLC 领域系统
    ↓ Arch 查询 / 计算 / 提交
Arch World 组件状态
```

phase 之间的先后不是伪装成 dependency edge 的字符串关系，而是 `EcsServerPhase` 的固定
合同。网络收包和 ingress 在 `Preparation` 完成，`Simulation` 读取其事实，`Commit` 完成
权威写入，`Projection` 只观察已提交状态。若一轮 Tick 使用 `EcsServerCommandBuffer`，
`Commit` 结束时播放一次结构变更；进入 `Projection` 前必须已经播放，不能让投影越过该
提交边界。

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
- `WorldTickSystem`：接收 `EcsServerTickContext`，推进 `WorldRuntime` 的世界时间和版本；
  通常注册在 `Simulation`，并把 `WorldInteractionSystem` 放在 `Commit`。

这些适配器只负责把领域状态接到 Arch 的 Tick 生命周期。它们不会推断装备、Buff、输入或
网络状态，也不会替代 `SpatialSimulationSystem` 的空间提交边界。

`EcsServer` 不规定所有系统都必须每 Tick 修改组件。请求型操作可以由服务器在独占提交阶段
直接调用领域系统；持续模拟系统则由宿主在每轮调用 `Update`。添加/移除组件和销毁实体属于
结构变化，应由宿主按 Arch 的访问约定安排在明确的提交边界。`EcsServerCommandBuffer`
封装 Arch `CommandBuffer`，用于记录一轮更新中的创建、销毁、添加、移除和设置操作；当前
`EcsServer.Update` 在 `Commit` 完成后、`Projection` 开始前播放这一轮缓冲区，并拒绝同一
缓冲区的重复播放。没有 `Projection` 系统时，更新尾部仍会完成一次兜底播放。

需要在同一 Tick 创建实体时，定义宿主可以实现 `IDeferredEntityDefinitionHost<TEntity>`。
`EntityDefinitionSystem<TEntity>.SpawnDeferred` 返回 `EcsServerEntityCreationTicket`：ticket 的
`BufferedEntity` 只允许提交到当前 command buffer，`Resolve()` 只能在 playback 后使用；声明了
`EntityIdentityComponent` 的实体会先分配 runtime handle，待 playback 完成后再登记真实 Arch
实体。命令被丢弃、创建后又被销毁或 playback 失败时，ticket 会释放预分配的 handle，不把
buffered entity 泄漏到跨域事实或网络投影中。服务器 NPC 与投射物生成路径使用这一边界，因此
它们在 Commit 后才进入后续 Simulation/Projection 可见范围；启动初始化和通信 ingress 仍可使用
同步宿主入口作为明确的低层适配器。

## 与现有服务器循环的关系

`EcsServer` 是 NSSLC 的 Arch 组织适配器，不是 `MinimalImplementation` 的替代品。若宿主
已有服务器循环，应在该循环的一个明确阶段调用 `EcsServer.Update`，不要同时再启动第二个
权威循环推进相同的 World。阶段内的系统顺序由依赖图和稳定注册序决定；文件名和目录顺序
不构成调度依据。
