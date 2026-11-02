# 世界交互能力

本目录提供实体访问世界的共享能力边界和 NSSLC 的最小可运行世界 owner。它使用 Arch 的
实体句柄定位调用者，但不把地图拆成“一 tile 一个实体”。

`WorldRuntime` 组合 `WorldRuntimeStateComponent` 与 `WorldTileStorageComponent`，实现
`IWorldInteractionWorld` 和 `ISpatialWorldQuery`。因此基础运行可以直接读取、放置、破坏
tile，推进显式 ECS Tick，并为移动系统提供实心 tile 碰撞和液体接触查询。持久化、网络
广播、掉落、多格物体、斜坡和完整液体传播仍由外部 adapter 负责，不能从这个最小 owner
推断已实现。

## 职责分层（2026-11-01）

本域按角色再分三层。目录只表达职责，全部文件的 C# 命名空间仍为 `Terraria.World`：

```text
World/
  Contract/   # World 定义的接口：IWorldInteractionWorld、IWorldTileGrid
  Model/      # World 定义的数据模型：坐标、tile 事实、结果、mutation、时间推进值、运动结果
  Solver/     # 纯运算：运动、时间推进、tile 转移、mutation 归并、掉落表、索引、权限、物品
  *.cs        # 更新驱动：WorldRuntime、WorldTickSystem、WorldInteractionSystem、存储与组件
```

### 运算与更新的边界

- `Solver/` 中的类型只做计算。输入是 `Model/` 的值与 `Contract/` 的只读端口；它们不持有锁、
  不写存储、不推进 revision、不读 Arch 世界。
- 更新驱动只做四件事：加锁、判断时机、写存储、推进 revision 并记录 mutation 和管理队列。
- 本域对外暴露的接口一律返回 `Model/` 中定义的模型。例外是 `ISpatialWorldQuery` 与
  `ISpatialWorldRevisionSource`：这两个端口属于 `Terraria.Physics`，由本域实现，因此 `Move`
  和 `GetLiquidContact` 返回 Physics 侧结果。域内运算结果仍是 `WorldTileMotionResult` 这类
  World 模型，由 `WorldRuntime` 投影过去。

| 运算类型 | 承担的规则 | 调用方 |
| --- | --- | --- |
| `WorldTileMotionSolver` | 分轴移动、二分回退、接地吸附、世界边界钳制 | `WorldRuntime.Move` |
| `WorldTimeAdvanceSolver` | tick/delta/revision 校验、昼夜推进、epoch 递增 | `WorldRuntimeStateComponent`、`WorldRuntime` |
| `WorldTileTransitionRules` | 破坏/放置/液体写入后的 tile 值，可否破坏或放置 | `WorldRuntime` |
| `WorldMutationRules` | 变更是否记录、抵消或归并 | `WorldRuntime.RecordMutationUnsafe` |
| `WorldTileDropRules` | 单 tile 掉落表 | `WorldRuntime.TryBreakTile` |
| `WorldTileIndexRules` | 边界判断与行主序索引 | `WorldTileStorageComponent` |
| `WorldInteractionPermissionRules` | 操作到权限的映射、权限位校验 | `WorldInteractionSystem`、`WorldInteractionCapabilityComponent` |
| `WorldItemRules` | 物品合法性、可否变更、空槽选择、revision 递增 | `WorldItemStore` |

行为等价以确定性转录比对为依据：改造前后 195 行输出逐字节相同。

## 协作关系

```text
Arch Entity
  └─ WorldInteractionCapabilityComponent
       └─ WorldInteractionSystem.TryAcquire
            └─ WorldInteractionAccess（短期令牌）
                 └─ IWorldInteractionWorld
                      └─ WorldRuntime / 外部权威世界所有者
```

## Arch World 请求流

需要在服务器 Tick 中处理世界交互时，实体同时附加：

```text
WorldInteractionCapabilityComponent
WorldInteractionRequestComponent
WorldInteractionResultComponent
```

宿主把 `WorldInteractionSystem` 注册到 `EcsServerPhase.Commit`：

```csharp
WorldRuntime world = new(width: 256, height: 128);
WorldInteractionSystem worldSystem = new(ecsWorld, world);
WorldTickSystem worldTick = new(ecsWorld, world);
EcsServer server = EcsServer.Create(
    ecsWorld,
    [
      new(EcsServerPhase.Simulation, worldTick),
      new(EcsServerPhase.Commit, worldSystem)
    ]);
```

请求由提交阶段通过 Arch `World.Query` 消费。每个实体每次更新最多处理一个仍处于
`IsPending` 状态的请求；系统完成后把 `WorldInteractionResultComponent` 写回，并将请求
标记为已消费。未知操作、实体权限不足和世界端口拒绝都会写入 `Rejected` 结果。

```csharp
Entity player = ecsWorld.Create(
    new WorldInteractionCapabilityComponent(
        WorldInteractionPermission.ReadTile |
        WorldInteractionPermission.BreakTile),
    WorldInteractionRequestComponent.BreakTile(
        requestId: 10,
        tick: 42,
        coordinate: new WorldTileCoordinate(100, 80)),
    new WorldInteractionResultComponent());
```

请求组件只保存一次操作的 Tick、坐标、类型和请求编号；系统只消费当前 Tick 的请求，旧请求
会写入拒绝结果。组件不保存 tile 数组、WorldFile、地图副本或长期 Arch 实体引用。实体即使
缺少 `WorldInteractionCapabilityComponent`，也会收到明确的权限拒绝结果并结束请求。
`IWorldInteractionWorld` 是 tile/wall/liquid 数据、坐标规则、掉落、dirty section 和广播
的权威端口；`WorldRuntime` 只覆盖基础 tile、碰撞和液体接触行为。Arch `World` 管理的是
交互实体和请求状态，不是整张密集 tile map。

`WorldInteractionCapabilityComponent` 是实体的授权声明。它只保存
`WorldInteractionPermission` 位：读取方块、破坏方块和放置方块。实体没有这个组件时，
`TryAcquire` 返回 `false`，不能取得访问令牌。

`WorldInteractionSystem` 是共享 API 的检查入口。每次读取或修改都会再次检查：

- Arch `Entity` 属于创建该系统的 Arch `World`；
- 实体仍然存活；
- 能力组件仍然存在；
- 能力组件仍包含本次操作需要的权限；
- 令牌中的权限没有超出取得令牌时的权限。

因此移除组件、收紧权限或销毁实体后，旧令牌不会继续授权操作。Arch 的实体句柄也不会
从 `WorldInteractionAccess` 公开给调用方，调用方只能使用系统提供的世界 API。

## 使用方式

```csharp
using Arch.Core;
using Terraria.World;

using ArchWorld = Arch.Core.World;

using ArchWorld ecsWorld = ArchWorld.Create();
Entity player = ecsWorld.Create(
    new WorldInteractionCapabilityComponent(
        WorldInteractionPermission.ReadTile |
        WorldInteractionPermission.BreakTile |
        WorldInteractionPermission.PlaceTile));

WorldInteractionSystem interactions = new(ecsWorld, worldPort);
if (interactions.TryAcquire(
        player,
        WorldInteractionPermission.BreakTile,
        out WorldInteractionAccess access)) {
  WorldInteractionResult result = interactions.TryBreakTile(
      access,
      new WorldTileCoordinate(100, 80));
}
```

没有能力组件的实体不能取得令牌：

```csharp
Entity projectile = ecsWorld.Create();
bool canInteract = interactions.TryAcquire(
    projectile,
    WorldInteractionPermission.BreakTile,
    out _); // false
```

## 世界端口的职责

`IWorldInteractionWorld` 是权威世界的适配端口。服务器实现它时，应把读取和修改转发到
现有世界所有者，例如 `WorldOwner` 的快照读取、地形规则和提交入口。适配器负责处理世界
版本、坐标边界、方块规则、掉落物、脏区块和网络广播；能力组件不绕过这些规则，也不保存
第二份地形。

读取 API 返回值快照。破坏和放置 API 返回一次操作结果；结果只表示世界端口是否接受了该
操作，跨系统效果仍按服务器已有的提交顺序处理。
