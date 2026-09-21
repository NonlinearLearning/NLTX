# 世界交互与结构变更组件字段设计

## 1. 范围

本文是对
[`Version4世界交互与结构变更系统组件拆分报告`](../../component-decomposition/baseline/Version4世界交互与结构变更系统组件拆分报告.md)
的字段级设计。只定义组件及其值对象的字段和属性，不定义 System 算法、Command 处理、
网络协议、存档格式或测试。

设计依据：

- `D:\TRbackup\Version4\Terraria\Tile.cs`
- `D:\TRbackup\Version4\Terraria\WorldGen.cs`
- `D:\TRbackup\Version4\Terraria\Wiring.cs`
- `D:\TRbackup\Version4\Terraria.GameContent\PressurePlateHelper.cs`
- `D:\TRbackup\Version4\Terraria.DataStructures\TileEntity.cs`
- `D:\TRbackup\Version4\Terraria.DataStructures\TileEntitiesManager.cs`
- `D:\TRbackup\Version4\Terraria.GameContent.Tile_Entities\TELogicSensor.cs`、
  `TETeleportationPylon.cs`、`TEItemFrame.cs`、`TEFoodPlatter.cs`、`TEWeaponsRack.cs`、
  `TEDisplayDoll.cs`、`TEHatRack.cs`、`TETrainingDummy.cs`、
  `TEDeadCellsDisplayJar.cs`、`TELeashedEntityAnchorWithItem.cs`、`TEKiteAnchor.cs`、
  `TECritterAnchor.cs`
- SS14 参考目录 `C:\Users\shan\Downloads\ECS\space-station-14-master` 下的
  `Content.Shared/Wires`、`Content.Shared/Construction`、`Content.Shared/Interaction`，
  仅用于组件粒度和状态所有权的组织参考。

字段命名采用目标 C# API 的语义名称，不直接暴露 Terraria 的压缩位字段。`private` 字段和
只读属性表示集合由所属 System 维护，调用方不能取得内部可变集合的写权限。

## 2. 所有权和组合边界

```text
WorldStorage (第 3 子系统提供)
  TileCellComponent + TileFrameComponent + TileSignalTopologyComponent
  TileEntityAnchorComponent + TileEntityKindComponent
  TileEntityRuntimeIdComponent + TileEntityUpdateScheduleComponent
  TileEntity 专用状态组件

WorldInteraction (第 7 子系统拥有)
  MechanismCooldownComponent
  PressurePlateOccupancyComponent
  LogicSensorComponent

只读/临时数据
  TileObjectDefinition、WirePropagationContext、LogicGateWorkSet、
  LogicSensorEvaluationWorkSet、TeleportPylonProjectionCache、命令和变更记录
```

TileCell、TileFrame、TileSignalTopology 和 TileEntity 索引的权威存储仍由
`WorldStorage` 持有；它们在本设计中列出字段，是为了固定第 7 子系统读取和提交时使用的
数据契约。第 7 子系统不能再创建第二份 Tile 数组、`ByID`/`ByPosition` 字典或静态全局
状态。

## 3. 公共值对象

这些类型表达字段的语义，不是 ECS Component，不单独附着于实体。

### 3.1 TileCoordinate

```csharp
public readonly record struct TileCoordinate(int X, int Y);
```

| 成员 | 类型 | 读写 | 说明 |
| --- | --- | --- | --- |
| `X` | `int` | 只读 | Tile 横坐标；对应 `Point16.X` 的语义值 |
| `Y` | `int` | 只读 | Tile 纵坐标；对应 `Point16.Y` 的语义值 |

### 3.2 StoredItemState

```csharp
public readonly record struct StoredItemState(int Type, byte Prefix, int Stack)
{
  public bool IsEmpty => Type == 0 || Stack <= 0;
}
```

| 成员 | 类型 | 读写 | 说明 |
| --- | --- | --- | --- |
| `Type` | `int` | 只读 | 物品定义 ID；对应 `Item.type` |
| `Prefix` | `byte` | 只读 | 物品前缀；对应 `Item.prefix` |
| `Stack` | `int` | 只读 | 数量；序列化边界仍可按 Version4 的 `short/ushort` 兼容处理 |
| `IsEmpty` | `bool` | 派生只读 | `Type == 0` 或 `Stack <= 0` 时为空 |

### 3.3 TileEntityRuntimeId 和 TileEntityKindId

```csharp
public readonly record struct TileEntityRuntimeId(int Value);

public readonly record struct TileEntityKindId(byte Value);
```

| 类型/成员 | 读写 | 说明 |
| --- | --- | --- |
| `TileEntityRuntimeId.Value` | 只读 | `TileEntity.ID` 的运行时/网络作用域 ID；不等同 ECS Entity ID、持久化 ID 或玩家 ID |
| `TileEntityKindId.Value` | 只读 | `TileEntitiesManager` 注册的类型 ID；不等同 Tile 图形类型 ID |

### 3.4 逻辑传感器枚举

```csharp
public enum LogicCheckType : byte
{
  None,
  Day,
  Night,
  PlayerAbove,
  Water,
  Lava,
  Honey,
  Liquid,
}
```

枚举值对应 Version4 `TELogicSensor.LogicCheckType`，不把时钟、玩家命中框或液体快照存入
组件。

## 4. Tile 结构组件

### 4.1 TileCellComponent

归属：`WorldStorage/Tiles`。一格 Tile 的基础权威状态；不包含邻域计算和行为。

```csharp
public sealed class TileCellComponent
{
  public ushort TileType { get; internal set; }
  public bool IsActive { get; internal set; }
  public ushort WallType { get; internal set; }
  public byte LiquidAmount { get; internal set; }
  public LiquidKind LiquidKind { get; internal set; }
}
```

| 成员 | 类型 | 读写 | 来源/约束 |
| --- | --- | --- | --- |
| `TileType` | `ushort` | 存储边界可写 | `Tile.type`；Tile 定义目录解释其内容 |
| `IsActive` | `bool` | 存储边界可写 | `Tile.active()`；不由碰撞查询缓存 |
| `WallType` | `ushort` | 存储边界可写 | `Tile.wall` |
| `LiquidAmount` | `byte` | 存储边界可写 | `Tile.liquid` |
| `LiquidKind` | `LiquidKind` | 存储边界可写 | `Tile.liquidType()` 的语义值；不与实体液体接触组件合并 |

`LiquidKind` 是世界存储使用的语义枚举（例如 water、lava、honey、shimmer）；具体枚举值由
第 3 子系统定义。

### 4.2 TileFrameComponent

归属：`WorldStorage/Tiles`。保存框架结果；值由 `TileFramingSystem` 根据邻域计算后提交。

```csharp
public sealed class TileFrameComponent
{
  public short TileFrameX { get; internal set; }
  public short TileFrameY { get; internal set; }
  public short WallFrameX { get; internal set; }
  public short WallFrameY { get; internal set; }
}
```

| 成员 | 类型 | 读写 | 来源/约束 |
| --- | --- | --- | --- |
| `TileFrameX` | `short` | 框架提交写 | `Tile.frameX` |
| `TileFrameY` | `short` | 框架提交写 | `Tile.frameY` |
| `WallFrameX` | `short` | 框架提交写 | `Tile.wallFrameX(...)` 的语义值 |
| `WallFrameY` | `short` | 框架提交写 | `Tile.wallFrameY(...)` 的语义值 |

### 4.3 TileSignalTopologyComponent

归属：`WorldStorage/Tiles`。只表达某格的线路和执行器拓扑，不保存一次传播的队列。

```csharp
public sealed class TileSignalTopologyComponent
{
  public bool HasWire1 { get; internal set; }
  public bool HasWire2 { get; internal set; }
  public bool HasWire3 { get; internal set; }
  public bool HasWire4 { get; internal set; }
  public bool HasActuator { get; internal set; }
  public bool IsActuated { get; internal set; }
}
```

| 成员 | 类型 | 读写 | 来源/约束 |
| --- | --- | --- | --- |
| `HasWire1` | `bool` | 结构提交写 | `Tile.wire()` |
| `HasWire2` | `bool` | 结构提交写 | `Tile.wire2()` |
| `HasWire3` | `bool` | 结构提交写 | `Tile.wire3()` |
| `HasWire4` | `bool` | 结构提交写 | `Tile.wire4()` |
| `HasActuator` | `bool` | 结构提交写 | `Tile.actuator()` |
| `IsActuated` | `bool` | 执行器提交写 | `Tile.inActive()`；只表示当前执行状态 |

四个线路字段使用 `HasWire1` 至 `HasWire4`，避免在尚未确认颜色映射时把 Version4 的 wire
编号错误命名为颜色。颜色语义只存在于传播上下文或内容定义。

## 5. 世界交互运行时组件

### 5.1 MechanismCooldownComponent

归属：`WorldInteraction/Wiring`，附着于世界或区段运行时实体。它替代 `Wiring` 的机关
数组和三类炮冷却静态字段；不保存传播队列。

```csharp
public sealed class MechanismCooldownComponent
{
  private readonly List<MechanismCooldownEntry> _entries = new();

  public IReadOnlyList<MechanismCooldownEntry> Entries => _entries;
  public int CannonCooldownTicks { get; internal set; }
  public int BunnyCannonCooldownTicks { get; internal set; }
  public int SnowballCannonCooldownTicks { get; internal set; }
}

public readonly record struct MechanismCooldownEntry(
  TileCoordinate Position,
  int RemainingTicks);
```

| 成员 | 类型 | 读写 | 来源/约束 |
| --- | --- | --- | --- |
| `_entries` | `List<MechanismCooldownEntry>` | 仅所属 System | `Wiring._mechX`、`_mechY`、`_mechTime` 的按记录表示；不得由调用方直接替换 |
| `Entries` | `IReadOnlyList<MechanismCooldownEntry>` | 只读属性 | 传播和机关更新的输入视图 |
| `CannonCooldownTicks` | `int` | 所属 System 写 | `cannonCoolDown` |
| `BunnyCannonCooldownTicks` | `int` | 所属 System 写 | `bunnyCannonCoolDown` |
| `SnowballCannonCooldownTicks` | `int` | 所属 System 写 | `snowballCannonCoolDown` |
| `MechanismCooldownEntry.Position` | `TileCoordinate` | 只读 | 机关所在坐标 |
| `MechanismCooldownEntry.RemainingTicks` | `int` | 只读值 | 机关下一次处理前的剩余 Tick |

`running`、`CurrentUser`、`_currentWireColor`、泵计数、传送端点和线路遍历集合不属于本组件。

### 5.2 PressurePlateOccupancyComponent

归属：`WorldInteraction/PressurePlates`，附着于世界或压力板索引实体。它只保存当前有效
占用关系和需要处理的板，不保存玩家旧位置。

```csharp
public sealed class PressurePlateOccupancyComponent
{
  private readonly Dictionary<TileCoordinate, HashSet<EntityReference>> _occupantsByPlate = new();

  public IReadOnlyDictionary<TileCoordinate, IReadOnlySet<EntityReference>> OccupantsByPlate { get; }
  public bool NeedsFirstUpdate { get; internal set; }
}
```

| 成员 | 类型 | 读写 | 来源/约束 |
| --- | --- | --- | --- |
| `_occupantsByPlate` | `Dictionary<TileCoordinate, HashSet<EntityReference>>` | 仅所属 System | 替代 `PressurePlatesPressed`；键是压力板坐标，值是有效实体引用集合 |
| `OccupantsByPlate` | 只读字典视图 | 只读属性 | 不返回内部可变字典或集合 |
| `NeedsFirstUpdate` | `bool` | 所属 System 写 | 替代 `PressurePlateHelper.NeedsFirstUpdate` |

`PlayerLastPosition` 和复用的 `pressurePlateBounds` 是系统级 scratch/cache，不放入该组件；
玩家实体仅通过现有 `LocationComponent` 和碰撞几何被读取。

## 6. TileEntity 身份和调度组件

### 6.1 TileEntityAnchorComponent

归属：`WorldStorage/TileEntities`。表达 TileEntity 的锚定原点；多格尺寸由
`TileEntityKind`/内容定义决定。

```csharp
public sealed class TileEntityAnchorComponent
{
  public TileCoordinate Origin { get; internal set; }
}
```

| 成员 | 类型 | 读写 | 来源/约束 |
| --- | --- | --- | --- |
| `Origin` | `TileCoordinate` | 放置/移动提交写 | `TileEntity.Position`；是多格 TileEntity 的逻辑原点 |

### 6.2 TileEntityKindComponent

```csharp
public sealed class TileEntityKindComponent
{
  public TileEntityKindId Kind { get; internal set; }
}
```

| 成员 | 类型 | 读写 | 来源/约束 |
| --- | --- | --- | --- |
| `Kind` | `TileEntityKindId` | 类型注册时写 | `TileEntity.type`/`TileEntitiesManager` 注册 ID；不等同 Tile 图形类型 |

### 6.3 TileEntityRuntimeIdComponent

```csharp
public sealed class TileEntityRuntimeIdComponent
{
  public TileEntityRuntimeId Id { get; internal set; }
}
```

| 成员 | 类型 | 读写 | 来源/约束 |
| --- | --- | --- | --- |
| `Id` | `TileEntityRuntimeId` | 分配时写，一经分配不可变 | `TileEntity.ID`；只由 `TileEntityRuntimeIdAllocator` 分配 |

ECS Entity ID、持久化 ID、网络客户端 ID、账户 ID 和世界 GUID 均不能写入 `Id`。

### 6.4 TileEntityUpdateScheduleComponent

```csharp
public sealed class TileEntityUpdateScheduleComponent
{
  public bool RequiresUpdates { get; internal set; }
}
```

| 成员 | 类型 | 读写 | 来源/约束 |
| --- | --- | --- | --- |
| `RequiresUpdates` | `bool` | 注册/卸载时写 | 替代 `TileEntity.RequiresUpdates`；为 `true` 时由 `TileEntityStore` 纳入更新调度 |

`UpdateEntities` 列表、UpdateStart/UpdateEnd 回调和调度顺序属于 `TileEntityRuntimeSystem`/
`TileEntityStore`，不复制成组件字段。

## 7. TileEntity 专用状态组件

### 7.1 LogicSensorComponent

归属：`WorldInteraction/TileEntities`，仅附着逻辑传感器 TileEntity。

```csharp
public sealed class LogicSensorComponent
{
  public LogicCheckType CheckType { get; internal set; }
  public bool IsOn { get; internal set; }
  public int CountedData { get; internal set; }
}
```

| 成员 | 类型 | 读写 | 来源/约束 |
| --- | --- | --- | --- |
| `CheckType` | `LogicCheckType` | 放置/配置时写 | `TELogicSensor.logicCheck` |
| `IsOn` | `bool` | 传感器 System 写 | `TELogicSensor.On`；对应 Tile frame 的状态由结构提交派生 |
| `CountedData` | `int` | 传感器 System 写 | `TELogicSensor.CountedData` |

玩家矩形缓存、液体快照、`tripPoints`、待删除 ID 和 `inUpdateLoop` 不属于组件；它们属于
传感器评估批次的临时工作集。

### 7.2 ItemFrameComponent

```csharp
public sealed class ItemFrameComponent
{
  public StoredItemState Item { get; internal set; }
}
```

| 成员 | 类型 | 读写 | 来源/约束 |
| --- | --- | --- | --- |
| `Item` | `StoredItemState` | 放置/取出/破坏提交写 | `TEItemFrame.item` 的持久化字段 `type/prefix/stack` |

### 7.3 WeaponRackComponent

```csharp
public sealed class WeaponRackComponent
{
  public StoredItemState Item { get; internal set; }
}
```

| 成员 | 类型 | 读写 | 来源/约束 |
| --- | --- | --- | --- |
| `Item` | `StoredItemState` | 放置/取出/破坏提交写 | `TEWeaponsRack.item` 的持久化字段 `type/prefix/stack` |

### 7.4 FoodPlatterComponent

```csharp
public sealed class FoodPlatterComponent
{
  public StoredItemState Item { get; internal set; }
}
```

| 成员 | 类型 | 读写 | 来源/约束 |
| --- | --- | --- | --- |
| `Item` | `StoredItemState` | 放置/取出/破坏提交写 | `TEFoodPlatter.item` 的持久化字段 `type/prefix/stack` |

### 7.5 DeadCellsDisplayJarComponent

```csharp
public sealed class DeadCellsDisplayJarComponent
{
  public StoredItemState Item { get; internal set; }
}
```

| 成员 | 类型 | 读写 | 来源/约束 |
| --- | --- | --- | --- |
| `Item` | `StoredItemState` | 放置/取出/破坏提交写 | `TEDeadCellsDisplayJar.item`；`FitsJar` 资格由 Query 判断 |

这四个组件共享 `StoredItemState` 值对象，但不合并为一个带 Tile 类型分支的巨型组件。实体
只挂载与自身 TileEntity 种类对应的一个组件。

### 7.6 DisplayDollComponent

```csharp
public sealed class DisplayDollComponent
{
  private readonly StoredItemState[] _equipment = new StoredItemState[9];
  private readonly StoredItemState[] _dyes = new StoredItemState[9];
  private readonly StoredItemState[] _miscellaneous = new StoredItemState[1];

  public IReadOnlyList<StoredItemState> Equipment => _equipment;
  public IReadOnlyList<StoredItemState> Dyes => _dyes;
  public IReadOnlyList<StoredItemState> Miscellaneous => _miscellaneous;
  public byte Pose { get; internal set; }
  public bool ContainsItems { get; }
}
```

| 成员 | 类型 | 读写 | 来源/约束 |
| --- | --- | --- | --- |
| `_equipment` | `StoredItemState[9]` | 仅所属 System | `TEDisplayDoll._equip`；装备槽位 0-8 |
| `Equipment` | `IReadOnlyList<StoredItemState>` | 只读属性 | 对外只读视图，不能替换数组 |
| `_dyes` | `StoredItemState[9]` | 仅所属 System | `TEDisplayDoll._dyes`；染料槽位 0-8 |
| `Dyes` | `IReadOnlyList<StoredItemState>` | 只读属性 | 对外只读视图 |
| `_miscellaneous` | `StoredItemState[1]` | 仅所属 System | `TEDisplayDoll._misc` |
| `Miscellaneous` | `IReadOnlyList<StoredItemState>` | 只读属性 | 对外只读视图 |
| `Pose` | `byte` | 人偶 System 写 | `TEDisplayDoll._pose`；网络/存档兼容字段 |
| `ContainsItems` | `bool` | 派生只读属性 | 任一装备、染料或 misc 槽非空 |

`_dollPlayer`、`SupportedUseStylePoses`、投射物 dummy、动画百分比和瞄准弧度属于表现或计算
辅助，不进入权威组件。

### 7.7 HatRackComponent

```csharp
public sealed class HatRackComponent
{
  private readonly StoredItemState[] _items = new StoredItemState[2];
  private readonly StoredItemState[] _dyes = new StoredItemState[2];

  public IReadOnlyList<StoredItemState> Items => _items;
  public IReadOnlyList<StoredItemState> Dyes => _dyes;
  public bool ContainsItems { get; }
}
```

| 成员 | 类型 | 读写 | 来源/约束 |
| --- | --- | --- | --- |
| `_items` | `StoredItemState[2]` | 仅所属 System | `TEHatRack._items`；两个帽子槽 |
| `Items` | `IReadOnlyList<StoredItemState>` | 只读属性 | 对外只读视图 |
| `_dyes` | `StoredItemState[2]` | 仅所属 System | `TEHatRack._dyes`；两个染料槽 |
| `Dyes` | `IReadOnlyList<StoredItemState>` | 只读属性 | 对外只读视图 |
| `ContainsItems` | `bool` | 派生只读属性 | 任一帽子或染料槽非空 |

### 7.8 TrainingDummyComponent

```csharp
public sealed class TrainingDummyComponent
{
  public EntityReference Npc { get; internal set; } = EntityReference.None;
  public int ActivationRetryCooldownTicks { get; internal set; }
  public bool IsActive => !Npc.IsEmpty;
}
```

| 成员 | 类型 | 读写 | 来源/约束 |
| --- | --- | --- | --- |
| `Npc` | `EntityReference`（Scope=`Npc`） | 激活/停用 System 写 | `TETrainingDummy.npc`；不能直接保存裸数组下标 |
| `ActivationRetryCooldownTicks` | `int` | 训练假人 System 写 | `TETrainingDummy.activationRetryCooldown` |
| `IsActive` | `bool` | 派生只读属性 | `Npc` 非空时为 `true` |

玩家命中框集合、`playerBoxFilled` 和 `npcSlotsFull` 是批次缓存，不属于组件。

### 7.9 LeashedEntityAnchorComponent

归属：`WorldInteraction/TileEntities`，附着于风筝、动物等带物品的系绳锚点。Kite 与 Critter
的差异由 `TileEntityKindComponent` 和内容原型目录表达。

```csharp
public sealed class LeashedEntityAnchorComponent
{
  public int ItemType { get; internal set; }
  public bool HasItem => ItemType > 0;
}
```

| 成员 | 类型 | 读写 | 来源/约束 |
| --- | --- | --- | --- |
| `ItemType` | `int` | 插入/掉落提交写 | `TELeashedEntityAnchorWithItem.itemType` |
| `HasItem` | `bool` | 派生只读属性 | `ItemType > 0` |

`CritterPrototypes`、风筝/动物原型和实际生成的 LeashedEntity 属于内容定义或实体生命周
期，不放入该组件。没有物品的抽象 `TELeashedEntityAnchor` 不额外创建空组件。

## 8. 不设计为 Component 的候选项

下列名称在报告中出现，但按 `public-decomposition` 的所有权规则明确排除：

| 名称 | 排除理由 | 正确归属 |
| --- | --- | --- |
| `PlaceTileIntent`、`BreakTileIntent`、`WireTriggerIntent` | 单次请求，不是实体持续状态 | Command/Intent |
| `WirePropagationContext` | 当前操作者、颜色、运行标志只在一次传播有效 | `WiringInteractionSystem` 局部上下文 |
| `WireTraversalWorkSet`、`LogicGateWorkSet`、`WireEffectAccumulator` | 队列、去重集合、泵/传送聚合是一次传播 scratch | System 私有工作集 |
| `LogicSensorEvaluationWorkSet` | 玩家矩形、待触发点、待删除 ID 只跨一个 Update 批次 | `LogicSensorSystem` 私有工作集 |
| `TeleportPylonProjectionCache` | 从 TileEntity 索引可重建，属于投影缓存 | Projection |
| `StructureChangeRecord` | 提交后的不可变输出，不反向驱动权威状态 | Snapshot/Projection 输入 |
| `TileObjectDefinitionCatalog` | 锚点、尺寸、合法 Tile 和 placement hook 是只读定义 | Content Catalog |
| `TileEntityStore` 的 `ByID`、`ByPosition`、`UpdateEntities` | 全局索引/调度容器，不是单个实体字段 | WorldStorage service |
| `CurrentUser` 全局静态字段 | 隐式因果来源会污染嵌套或并发传播 | `InteractionActor`/传播上下文字段 |

## 9. 组合示例

以下仅说明字段组合，不规定实体创建或系统执行顺序：

| 实体/存储行 | 组件组合 |
| --- | --- |
| 普通可布线 Tile | `TileCellComponent` + `TileFrameComponent` + `TileSignalTopologyComponent` |
| 逻辑传感器 TileEntity | `TileEntityAnchorComponent` + `TileEntityKindComponent` + `TileEntityRuntimeIdComponent` + `TileEntityUpdateScheduleComponent` + `LogicSensorComponent` |
| 物品框 | `TileEntityAnchorComponent` + `TileEntityKindComponent` + `TileEntityRuntimeIdComponent` + `ItemFrameComponent` |
| 展示人偶 | `TileEntityAnchorComponent` + `TileEntityKindComponent` + `TileEntityRuntimeIdComponent` + `DisplayDollComponent` |
| 训练假人 | `TileEntityAnchorComponent` + `TileEntityKindComponent` + `TileEntityRuntimeIdComponent` + `TileEntityUpdateScheduleComponent` + `TrainingDummyComponent` |
| 压力板索引 | `PressurePlateOccupancyComponent` |
| 世界/区段机关调度 | `MechanismCooldownComponent` |

同一实体可以同时具有 Tile 结构组件和 TileEntity 专用组件，但不能把 Tile、线路传播队列、
网络投影和所有 TileEntity 种类再聚合成一个 `WorldInteractionComponent` 或
`TileEntityPayloadComponent`。
