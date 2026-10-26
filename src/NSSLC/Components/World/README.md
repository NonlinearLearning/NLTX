# 世界交互能力

本目录提供实体访问世界的共享能力边界。它使用 Arch 的实体句柄定位调用者，但不把地图
数组、文件对象或服务器世界所有者放进组件。

## 协作关系

```text
Arch Entity
  └─ WorldInteractionCapabilityComponent
       └─ WorldInteractionSystem.TryAcquire
            └─ WorldInteractionAccess（短期令牌）
                 └─ IWorldInteractionWorld
                      └─ 权威世界所有者 / 规则 / 掉落 / 广播
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
WorldInteractionSystem worldSystem = new(ecsWorld, worldPort);
EcsServer server = EcsServer.Create(
    ecsWorld,
    [new(EcsServerPhase.Commit, worldSystem)]);
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
会写入拒绝结果。组件不保存 tile 数组、WorldFile、地图
副本或长期 Arch 实体引用。`IWorldInteractionWorld` 仍是 tile/wall/liquid 数据、坐标规则、
掉落、dirty section 和广播的权威端口。Arch `World` 管理的是交互实体和请求状态，不是整张
密集 tile map。

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
