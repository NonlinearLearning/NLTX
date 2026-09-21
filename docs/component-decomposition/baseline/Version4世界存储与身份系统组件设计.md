# Version4 世界存储与身份系统组件设计

## 1. 文档定位

本文是 [Version4 世界存储与身份系统组件拆分报告](Version4世界存储与身份系统组件拆分报告.md) 的字段与属性设计稿。它只定义世界存储根及其内聚状态组件的字段、属性和值类型，不定义方法、System、Query、Command、Adapter、网络 DTO、存档 DTO 或测试。

字段归属以 `D:\TRbackup\Version4` 的实际存储形态为依据：`Main.tile`、`Main.player/npc/projectile/item`、`projectileIdentity`、`Main.chest`、`Main.sign`、`TileEntity.ByID/ByPosition/UpdateEntities` 和 `WorldSections.data`。详细读写路径、生命周期和证据状态见拆分报告。SS14 仅支持边界组织结论，不决定下列字段语义。

本文取代 [世界会话与进度系统组件设计](世界会话与进度系统组件设计.md) 第 6 节中针对 `WorldStorageRoot` 的简化交叉边界。旧节保留为当时的会话设计记录；新实现应以本文的组件集合、分离的 Chest/Sign 存储、投射物 identity 索引和 TileEntity 更新索引为准。

### 字段设计规则

- 一个组件只保存一个共同生命周期、共同写集的状态概念；`WorldStorageRoot` 只是组合根，不增加跨域可变字段。
- 原始 Tile 的压缩头、固定槽位容量和区段 bit 格式作为内部实现保留，不能因组件化改为多份可独立更新的状态。
- 槽位、实体句柄、投射物协议 identity、TileEntity ID、TileEntity 类型、容器槽位、坐标、网络 ID 和账户 ID 分轴建模，禁止用裸 `int` 互相替代。
- 所有可变集合为私有字段；公开属性只暴露容量、计数、版本和不可变值。读取快照与写入操作属于后续 API 设计，不在本文出现。
- 由另一组件可确定的数量、尺寸和状态仅以只读属性派生，不存为第二份权威字段。

## 2. 字段类型与身份值

这些小型值类型不是 ECS 组件，不持有集合，不含行为。它们阻止不同身份轴在 API 边界被误传。

```csharp
public readonly record struct TileCoordinate(int X, int Y);

public readonly record struct SectionCoordinate(int X, int Y);

public readonly record struct PlayerSlot(int Value);

public readonly record struct NpcSlot(int Value);

public readonly record struct ProjectileSlot(int Value);

public readonly record struct WorldItemSlot(int Value);

public readonly record struct ChestSlot(int Value);

public readonly record struct SignSlot(int Value);

public readonly record struct TileEntityId(int Value);

public readonly record struct TileEntityTypeId(byte Value);

public readonly record struct OwnerProjectileIdentity(
  PlayerSlot Owner,
  int Value);

public readonly record struct EntityHandle<TSlot>(
  TSlot Slot,
  uint Generation);

public readonly record struct ProjectileHandle(
  ProjectileSlot Slot,
  uint Generation);

public readonly record struct TileMapLayout(
  int Width,
  int Height,
  int SectionWidthInTiles,
  int SectionHeightInTiles);
```

| 值类型 | 字段/属性 | 来源与约束 |
| --- | --- | --- |
| `TileCoordinate` | `X`、`Y` | 对应 Tile、Chest、Sign、TileEntity 的世界格坐标；不是持久化对象身份。 |
| `SectionCoordinate` | `X`、`Y` | 对应 `WorldSections` 的区段索引；不是 Tile 坐标。 |
| `PlayerSlot`、`NpcSlot`、`ProjectileSlot`、`WorldItemSlot` | `Value` | 对应 `whoAmI`/固定数组下标；槽位复用后不能单独解析成同一实例。 |
| `ChestSlot`、`SignSlot` | `Value` | 对应 `Main.chest`、`Main.sign` 的旧协议位置；不是 Chest/Sign 的长期业务 ID。 |
| `TileEntityId` | `Value` | 对应 `TileEntity.ID` 和 `ByID` 键；重载稳定性仍为 `partial`，不得称为 GUID。 |
| `TileEntityTypeId` | `Value` | 对应 `TileEntity.type`/类型注册表；只选择类型，不标识实例。 |
| `OwnerProjectileIdentity` | `Owner`、`Value` | 对应 `projectileIdentity[owner, identity]`；它仅在拥有者局部有效，投射物销毁时失效。 |
| `EntityHandle<TSlot>`、`ProjectileHandle` | `Slot`、`Generation` | `Generation` 防止释放后旧槽位指向复用实例；它不是网络 ID。 |
| `TileMapLayout` | `Width`、`Height`、`SectionWidthInTiles`、`SectionHeightInTiles` | Tile 存储容量和区段分割的不可变布局。它在创建时必须与 `WorldDescriptorState` 的世界尺寸相符，之后不双写。 |

## 3. 原始 Tile 与液体队列值

`TileCellState` 保留当前 `Tile` 的紧凑字段。`LiquidAmount` 和各 header 不拆成独立组件，因为液体、坡度、线缆、活跃状态和帧坐标共享同一格子的高频读写与持久化格式。

```csharp
public struct TileCellState
{
  public ushort Type;
  public ushort Wall;
  public byte LiquidAmount;
  public ushort TileHeader;
  public byte Header;
  public byte Header2;
  public byte Header3;
  public short FrameX;
  public short FrameY;
}

public readonly record struct LiquidWorkEntry(
  TileCoordinate Coordinate,
  byte Delay,
  byte KillState);

public readonly record struct LiquidBufferEntry(
  TileCoordinate Coordinate);
```

| 值类型 | 字段/属性 | 所有权 |
| --- | --- | --- |
| `TileCellState` | `Type`、`Wall`、`LiquidAmount`、`TileHeader`、`Header`、`Header2`、`Header3`、`FrameX`、`FrameY` | `TileMapStore` 的唯一 Tile 真值。液体种类继续按原 header 编码解释，不额外创建第二个 `LiquidKind` 字段。 |
| `LiquidWorkEntry` | `Coordinate`、`Delay`、`KillState` | `LiquidWorkQueueState` 的活跃模拟队列条目，对应旧 `Liquid` 的调度信息，不拥有格内液体量。 |
| `LiquidBufferEntry` | `Coordinate` | `LiquidWorkQueueState` 的缓冲调度条目，对应旧 `LiquidBuffer`，不复制 Tile 数据。 |

## 4. 组合根

`WorldStorageRoot` 只建立组件关系。它不持有世界规则、天气、Boss 进度、内容定义、网络连接或全局版本号；这些状态分别属于 `WorldSession`、`ContentCatalog` 和外部边界。

```csharp
public sealed class WorldStorageRoot
{
  public TileMapStore TileMap { get; }
  public LiquidWorkQueueState LiquidWorkQueue { get; }
  public EntitySlotStore<PlayerState, PlayerSlot> Players { get; }
  public EntitySlotStore<NpcState, NpcSlot> Npcs { get; }
  public EntitySlotStore<ProjectileState, ProjectileSlot> Projectiles { get; }
  public EntitySlotStore<WorldItemState, WorldItemSlot> WorldItems { get; }
  public ProjectileIdentityIndex ProjectileIdentities { get; }
  public WorldContainerStore WorldContainers { get; }
  public WorldSignStore WorldSigns { get; }
  public TileEntityStore TileEntities { get; }
  public TileEntityUpdateSchedule TileEntityUpdates { get; }
  public WorldSectionState Sections { get; }
}
```

| 属性 | 类型 | 状态类别 | 归属理由 |
| --- | --- | --- | --- |
| `TileMap` | `TileMapStore` | 权威 | 唯一 Tile/Wall/液体格真值。 |
| `LiquidWorkQueue` | `LiquidWorkQueueState` | 暂态调度 | 液体待处理坐标与 Tile 真值分离。 |
| `Players`、`Npcs`、`Projectiles`、`WorldItems` | 各自的 `EntitySlotStore<TState, TSlot>` | 权威槽位 | 四种实体容量、释放和协议语义不同，不能合并为一个通用实体集合。 |
| `ProjectileIdentities` | `ProjectileIdentityIndex` | 权威协议索引 | owner-local 投射物 identity 独立于 Projectile slot。 |
| `WorldContainers` | `WorldContainerStore` | 权威 | 世界 Chest 槽位、位置索引和内容。 |
| `WorldSigns` | `WorldSignStore` | 权威 | Sign 槽位、位置索引和文本。 |
| `TileEntities` | `TileEntityStore` | 权威 | TileEntity ID、位置、类型、实例索引。 |
| `TileEntityUpdates` | `TileEntityUpdateSchedule` | 派生运行时索引 | 仅追踪需要逐 tick 更新的 TileEntity ID。 |
| `Sections` | `WorldSectionState` | 权威区段状态 | 区段 loaded/frame/map/refresh bit 与区段迭代缓存。 |

## 5. `TileMapStore`

```csharp
public sealed class TileMapStore
{
  private TileCellState[,] _tiles = new TileCellState[0, 0];
  private long _mutationRevision;

  public TileMapLayout Layout { get; private set; }
  public int Width => _tiles.GetLength(0);
  public int Height => _tiles.GetLength(1);
  public long MutationRevision => _mutationRevision;
}
```

| 成员 | 种类 | 状态类别 | 说明 |
| --- | --- | --- | --- |
| `_tiles` | 私有字段 | 权威 | `Main.tile` 的受控替代，保存完整 `TileCellState` 二维网格。 |
| `_mutationRevision` | 私有字段 | 权威变更版本 | 每次成功 Tile 结构变更推进，用于失效 section/快照；不是网络确认序号。 |
| `Layout` | 公开只读属性 | 创建期不可变 | 保存 Tile 容量和区段大小；与世界描述只在创建时校验，不与其双写。 |
| `Width`、`Height` | 公开派生属性 | 派生 | 只能由 `_tiles` 长度计算。 |
| `MutationRevision` | 公开只读属性 | 投影辅助 | 只暴露版本，不暴露可写 Tile 数组。 |

## 6. `LiquidWorkQueueState`

```csharp
public sealed class LiquidWorkQueueState
{
  private LiquidWorkEntry[] _activeEntries = Array.Empty<LiquidWorkEntry>();
  private LiquidBufferEntry[] _bufferedEntries = Array.Empty<LiquidBufferEntry>();
  private HashSet<TileCoordinate> _scheduledCoordinates = new();
  private int _activeCount;
  private int _bufferedCount;

  public int ActiveCount => _activeCount;
  public int BufferedCount => _bufferedCount;
  public int ScheduledCount => _scheduledCoordinates.Count;
}
```

| 成员 | 种类 | 状态类别 | 说明 |
| --- | --- | --- | --- |
| `_activeEntries` | 私有字段 | 暂态调度 | 对应当前液体更新工作集；容量保持实现细节。 |
| `_bufferedEntries` | 私有字段 | 暂态调度 | 对应待转入活跃工作集的旧 `LiquidBuffer` 坐标。 |
| `_scheduledCoordinates` | 私有字段 | 去重索引 | 确保同一 Tile 不被重复排入队列；不得替代 Tile 的液体值。 |
| `_activeCount`、`_bufferedCount` | 私有字段 | 队列边界 | 数组内有效区间，避免扫描整个预分配容量。 |
| `ActiveCount`、`BufferedCount`、`ScheduledCount` | 公开派生属性 | 派生 | 仅供调度/诊断读取，不能作为玩法液体总量。 |

## 7. `EntitySlotStore<TState, TSlot>`

`TState` 分别为 `PlayerState`、`NpcState`、`ProjectileState`、`WorldItemState`，但这些实体的生命、移动、AI、库存、伤害与表现字段仍由各自领域组件拥有。本组件只拥有固定槽位的占用、实例句柄 generation 和状态载体引用。

```csharp
public sealed class EntitySlotEntry<TState>
  where TState : class
{
  public TState? State { get; internal set; }
  public uint Generation { get; internal set; }
  public bool IsOccupied { get; internal set; }
}

public sealed class EntitySlotStore<TState, TSlot>
  where TState : class
  where TSlot : struct
{
  private EntitySlotEntry<TState>[] _entries = Array.Empty<EntitySlotEntry<TState>>();
  private int _activeCount;

  public int Capacity => _entries.Length;
  public int ActiveCount => _activeCount;
}
```

| 成员 | 种类 | 状态类别 | 说明 |
| --- | --- | --- | --- |
| `EntitySlotEntry<TState>.State` | 属性 | 实例载体 | 当前 slot 所在的领域状态实例；slot 未占用时为 `null`。 |
| `EntitySlotEntry<TState>.Generation` | 属性 | 权威身份防护 | 每次 slot 生命周期完成后递增，使旧 `EntityHandle<TSlot>` 失效。 |
| `EntitySlotEntry<TState>.IsOccupied` | 属性 | 权威槽位状态 | 取代散落的 `active + slot` 协议判断；不等同实体玩法生命周期字段。 |
| `_entries` | 私有字段 | 权威 | 保留 Player/NPC/Projectile/WorldItem 的固定容量数组布局。 |
| `_activeCount` | 私有字段 | 权威计数 | 在占用/释放时更新，不以全数组扫描计算。 |
| `Capacity`、`ActiveCount` | 公开派生属性 | 派生 | 不暴露可变条目数组。 |

## 8. `ProjectileIdentityIndex`

Version4 常规本地生成时把 `Projectile.identity` 设为选择到的 slot，并写入 `projectileIdentity[owner, identity]`；网络接收路径可以按 owner/identity 查找、复用或重新绑定 slot。因此该索引必须独立于 `EntitySlotStore<ProjectileState, ProjectileSlot>`，但其值必须携带 slot generation。

```csharp
public sealed class ProjectileIdentityIndex
{
  private Dictionary<OwnerProjectileIdentity, ProjectileHandle> _byOwnerIdentity = new();
  private Dictionary<ProjectileHandle, OwnerProjectileIdentity> _byProjectile = new();

  public int Count => _byOwnerIdentity.Count;
}
```

| 成员 | 种类 | 状态类别 | 说明 |
| --- | --- | --- | --- |
| `_byOwnerIdentity` | 私有字段 | 权威协议索引 | 从 owner-local identity 到带 generation 的 Projectile handle。 |
| `_byProjectile` | 私有字段 | 反向索引 | 允许释放投射物时撤销所有关联，避免遗留 identity 指向复用 slot。 |
| `Count` | 公开派生属性 | 派生 | 当前可解析映射数量。 |

`Projectile.projUUID`、投射物 AI、位置、速度、寿命和 owner 玩法关系不进入本组件；它们属于投射物领域状态。`OwnerProjectileIdentity.Value` 也不自动等于 slot，尽管当前本地生成路径常令二者相同。

## 9. `WorldContainerStore` 与 `WorldChestState`

本组件只管理落在世界 Tile 上的 Chest。玩家银行、个人仓库、商店会话与玩家库存有不同所有权和生命周期，不能借 `Chest.bankChest` 兼容字段回流为世界 Chest 的主状态。

```csharp
public sealed class WorldChestState
{
  public ChestSlot Slot { get; internal set; }
  public TileCoordinate Anchor { get; internal set; }
  public ItemState[] Items { get; internal set; } = Array.Empty<ItemState>();
  public int ItemCapacity { get; internal set; }
  public string Name { get; internal set; } = string.Empty;
  public bool IsLegacyBankChest { get; internal set; }
}

public sealed class WorldContainerStore
{
  private WorldChestState?[] _chests = Array.Empty<WorldChestState?>();
  private Dictionary<TileCoordinate, ChestSlot> _chestSlotsByAnchor = new();
  private int _activeChestCount;
  private long _mutationRevision;

  public int Capacity => _chests.Length;
  public int ActiveChestCount => _activeChestCount;
  public long MutationRevision => _mutationRevision;
}
```

| 成员 | 种类 | 状态类别 | 说明 |
| --- | --- | --- | --- |
| `WorldChestState.Slot` | 属性 | 协议定位 | 旧 `Main.chest` 数组位置。 |
| `Anchor` | 属性 | 空间关系 | Chest 锚点 Tile；位置索引必须与 `_chests` 中相同对象一致。 |
| `Items`、`ItemCapacity` | 属性 | 容器权威状态 | 保存世界 Chest 的实际物品实例及容量。`ItemState` 的字段由物品领域定义。 |
| `Name` | 属性 | 容器权威状态 | 对应旧 `Chest.name`。 |
| `IsLegacyBankChest` | 属性 | 兼容字段 | 对应旧 `bankChest`；仅为导入/迁移识别，不能使玩家银行成为世界 Tile Chest。 |
| `_chests` | 私有字段 | 权威 | 固定 Chest slot 表。 |
| `_chestSlotsByAnchor` | 私有字段 | 权威反向索引 | `TileCoordinate -> ChestSlot`，与 `_chests` 在同一事务更新。 |
| `_activeChestCount` | 私有字段 | 权威计数 | 不扫描整个固定数组。 |
| `_mutationRevision` | 私有字段 | 变更版本 | 标记 Chest 内容或索引变更，供后续快照失效使用。 |
| `Capacity`、`ActiveChestCount`、`MutationRevision` | 公开派生属性 | 派生 | 只读观测值。 |

`Chest.frameCounter`、`frame`、`eatingAnimationTime` 是交互动画/表现状态，不进入 `WorldChestState`。它们由客户端表现或专用交互状态拥有。

## 10. `WorldSignStore` 与 `WorldSignState`

```csharp
public sealed class WorldSignState
{
  public SignSlot Slot { get; internal set; }
  public TileCoordinate Anchor { get; internal set; }
  public string Text { get; internal set; } = string.Empty;
}

public sealed class WorldSignStore
{
  private WorldSignState?[] _signs = Array.Empty<WorldSignState?>();
  private Dictionary<TileCoordinate, SignSlot> _signSlotsByAnchor = new();
  private int _activeSignCount;
  private long _mutationRevision;

  public int Capacity => _signs.Length;
  public int ActiveSignCount => _activeSignCount;
  public long MutationRevision => _mutationRevision;
}
```

| 成员 | 种类 | 状态类别 | 说明 |
| --- | --- | --- | --- |
| `WorldSignState.Slot` | 属性 | 协议定位 | 旧 `Main.sign` 数组位置。 |
| `Anchor`、`Text` | 属性 | 权威 | 对应 `Sign.x`、`Sign.y`、`Sign.text`。 |
| `_signs` | 私有字段 | 权威 | 固定 Sign slot 表。 |
| `_signSlotsByAnchor` | 私有字段 | 权威反向索引 | 保证一处标牌 Tile 至多对应一个 Sign slot。 |
| `_activeSignCount`、`_mutationRevision` | 私有字段 | 权威计数/版本 | 分别避免全表扫描并驱动快照失效。 |
| `Capacity`、`ActiveSignCount`、`MutationRevision` | 公开派生属性 | 派生 | 只读观测值。 |

## 11. `TileEntityStore` 与 `TileEntityRecord`

TileEntity 的实例 ID、锚点、类型和需要更新标志必须在一个记录中共同迁移。类型专属的物品槽、逻辑传感器状态、训练假人目标等字段属于各 TileEntity 领域组件，不作为本通用索引的字段；本轮证据未确认一个可独立于 `TileEntityId` 的 ECS runtime entity ID，因此不预先设计该字段。

```csharp
public sealed class TileEntityRecord
{
  public TileEntityId Id { get; internal set; }
  public TileEntityTypeId Type { get; internal set; }
  public TileCoordinate Anchor { get; internal set; }
  public bool RequiresUpdates { get; internal set; }
}

public sealed class TileEntityStore
{
  private Dictionary<TileEntityId, TileEntityRecord> _byId = new();
  private Dictionary<TileCoordinate, TileEntityId> _idsByAnchor = new();
  private int _nextId;
  private long _mutationRevision;

  public int Count => _byId.Count;
  public int NextId => _nextId;
  public long MutationRevision => _mutationRevision;
}
```

| 成员 | 种类 | 状态类别 | 说明 |
| --- | --- | --- | --- |
| `TileEntityRecord.Id` | 属性 | 权威实例 ID | 对应旧 `TileEntity.ID`/`ByID` 键。 |
| `Type` | 属性 | 内容引用 | 对应旧 `TileEntity.type`；不等同 `Id`。 |
| `Anchor` | 属性 | 权威空间关系 | 对应旧 `Position`，与 `_idsByAnchor` 必须原子一致。 |
| `RequiresUpdates` | 属性 | 权威资格 | 对应旧标志，是 `TileEntityUpdateSchedule` 的唯一输入。 |
| `_byId` | 私有字段 | 权威索引 | `TileEntityId -> TileEntityRecord`。 |
| `_idsByAnchor` | 私有字段 | 权威反向索引 | `TileCoordinate -> TileEntityId`。 |
| `_nextId` | 私有字段 | ID 分配游标 | 对应 `TileEntitiesNextID`；世界加载后的重编语义仍为 `partial`。 |
| `_mutationRevision` | 私有字段 | 变更版本 | 注册、删除、类型或锚点变化时推进。 |
| `Count`、`NextId`、`MutationRevision` | 公开派生属性 | 派生 | 只读观测值。 |

## 12. `TileEntityUpdateSchedule`

此组件是由 `TileEntityRecord.RequiresUpdates` 派生的运行时索引。它不持有 TileEntity 实例、位置或类型，因此删除 TileEntity 后不会留下另一份权威对象状态。

```csharp
public sealed class TileEntityUpdateSchedule
{
  private HashSet<TileEntityId> _scheduledIds = new();
  private TileEntityId[] _tickSnapshot = Array.Empty<TileEntityId>();
  private long _scheduleRevision;

  public int Count => _scheduledIds.Count;
  public long ScheduleRevision => _scheduleRevision;
}
```

| 成员 | 种类 | 状态类别 | 说明 |
| --- | --- | --- | --- |
| `_scheduledIds` | 私有字段 | 派生索引 | 当前需要更新的 TileEntity ID 集合。 |
| `_tickSnapshot` | 私有字段 | tick 本地快照 | 单一 tick 的稳定遍历序列；其有效期不得跨 tick。 |
| `_scheduleRevision` | 私有字段 | 缓存失效版本 | 在计划集合变化时推进。 |
| `Count`、`ScheduleRevision` | 公开派生属性 | 派生 | 只读观测值。 |

## 13. `WorldSectionState`

区段状态保留 `WorldSections` 的四个 bit 语义：loaded、framed、map drawn、needs refresh。`SetSectionLoaded` 在 Version4 当前源码为空体，因此下列字段定义的是候选目标状态，完整转换行为仍为 `partial`。

```csharp
public readonly record struct SectionIterationState(
  float CenterX,
  float CenterY,
  int X,
  int Y,
  int Leg,
  int XDirection,
  int YDirection);

public sealed class WorldSectionState
{
  public const byte LoadedMask = 1 << 0;
  public const byte FramedMask = 1 << 1;
  public const byte MapDrawnMask = 1 << 2;
  public const byte NeedsRefreshMask = 1 << 3;

  private byte[] _flags = Array.Empty<byte>();
  private int _sectionCountX;
  private int _sectionCountY;
  private int _mapSectionsRemaining;
  private SectionIterationState _frameIteration;
  private SectionIterationState _mapIteration;
  private long _revision;

  public int SectionCountX => _sectionCountX;
  public int SectionCountY => _sectionCountY;
  public int Count => _flags.Length;
  public int MapSectionsRemaining => _mapSectionsRemaining;
  public long Revision => _revision;
}
```

| 成员 | 种类 | 状态类别 | 说明 |
| --- | --- | --- | --- |
| `LoadedMask`、`FramedMask`、`MapDrawnMask`、`NeedsRefreshMask` | 常量字段 | 格式约束 | 保持 Version4 `BitsByte` 的四个现有 bit 位置。 |
| `_flags` | 私有字段 | 权威 | 每个区段的 packed bit 状态。 |
| `_sectionCountX`、`_sectionCountY` | 私有字段 | 创建期布局 | `_flags` 的二维解释尺寸；由 `TileMapLayout` 派生初始化，不单独被世界规则写入。 |
| `_mapSectionsRemaining` | 私有字段 | 权威进度计数 | 对应旧 `mapSectionsLeft`。 |
| `_frameIteration`、`_mapIteration` | 私有字段 | 暂态迭代缓存 | 对应旧 `prevFrame`、`prevMap`；不属于世界存档或网络真值。 |
| `_revision` | 私有字段 | 变更版本 | bit、计数或迭代可见状态改变时推进。 |
| `SectionCountX`、`SectionCountY`、`Count`、`MapSectionsRemaining`、`Revision` | 公开派生属性 | 派生 | 只读诊断/快照边界。 |

## 14. 明确不属于上述组件的字段

| 字段或状态 | 正确归属 | 排除原因 |
| --- | --- | --- |
| 世界名、种子、世界尺寸语义、规则、天气、事件、Boss/入侵进度 | `WorldSession` | 生命周期与 Tile/slot 存储不同。TileMap 仅保存创建期容量布局。 |
| Tile 类型、Wall 类型、NPC/Projectile/Item 默认定义 | `ContentCatalog` | 定义数据不是运行时实例状态。 |
| `Entity.position`、`velocity`、碰撞接触、路径 | Movement/Physics | 存储层只持有可解析句柄和格数据。 |
| NPC/Player/Projectile/WorldItem 的玩法字段 | 各实体领域 | `EntitySlotStore` 不重新聚合生命、AI、库存、伤害、Buff、寿命或表现状态。 |
| `Projectile.projUUID`、AI、寿命和 owner 关系 | Projectile | `ProjectileIdentityIndex` 只保存 owner-local protocol identity 到 handle 的映射。 |
| Chest 的 `frameCounter`、`frame`、`eatingAnimationTime` | 表现/交互状态 | 它们不是世界容器内容或位置不变量。 |
| 网络包、网络实体 ID、连接状态、复制节流 | Network Adapter/Session | 只在边界解析为强类型 handle，不能成为 Store 内部引用。 |
| 世界文件路径、二进制读写器、网络二进制读写器、加载临时变量 | Persistence/Replication Adapter | I/O 副作用不能回流为权威世界状态。 |

## 15. 设计状态与实施前提

| 组件 | 证据状态 | 字段设计结论 |
| --- | --- | --- |
| `TileMapStore`、`LiquidWorkQueueState` | confirmed | Tile 原始字段和液体调度坐标已由 Version4 实现确认。 |
| 四个 `EntitySlotStore`、`ProjectileIdentityIndex` | confirmed / lifecycle partial | 固定槽位和投射物 identity 映射已确认；统一 generation/释放提交仍需迁移时闭合。 |
| `WorldContainerStore`、`WorldSignStore` | confirmed | 固定槽位和位置索引从 Chest/Sign 实现与 WorldFile 路径确认。 |
| `TileEntityStore`、`TileEntityUpdateSchedule` | confirmed / persistence partial | ID/位置/更新集合确认；重载后 ID 语义需要旧存档兼容证据。 |
| `WorldSectionState` | partial | 四个 bit 和字段已确认，但 `SetSectionLoaded` 空体，完整状态转换尚未确认。 |

本文按请求不包含测试设计。实施时必须另行补充每个组件的写入 Command、只读 View、存档/网络 Adapter、System 顺序以及与旧 `Main.*` 的单向迁移计划；在此之前，本文件不是可直接替换 Version4 存储的实现说明。
