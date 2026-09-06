# WorldInteractionAndStructures 实际代码组件草案

## 1. 草案元数据

~~~text
subsystemId: WorldInteractionAndStructures
taskNumber: 10
sourceComponentDesign: D:\TRbackup\NLTX\docs\design\2026-09-05-version4-world-interaction-and-structures-component-design.md
outputDraftPath: D:\TRbackup\NLTX\docs\design\2026-09-05-version4-world-interaction-and-structures-component-code-draft.md
draftScope: actual-component-code-skeleton
draftStatus: candidate
componentCount: 29
verificationStatus: not-run
sourceChanges: none
~~~

这是一份面向后续实现的 C# Component 代码草案，不是已经写入 src/ 的代码。代码块按拟定文件拆分；标记为 existing 的文件对应当前 NLTX 已有类型，标记为 partial 的文件只保留当前已经出现的字段轮廓并显式标注未闭合部分，标记为 proposed 的文件尚未创建。

本草案只包含数据组件和值对象所需的字段形状、默认值、只读集合外观和局部不变量注释，不包含任何运行时编排、读写流程、网络流程、存档流程、客户端表现或测试实现。

## 2. 使用边界

### 2.1 代码草案约束

- 所有新增类型都只是 status: proposed 的代码形状，不代表已经迁移或已经接入运行时。
- Component 的可变字段使用 internal set 作为草案级写入边界；最终程序集引用关系和唯一写入者仍需整合裁决。
- 对外暴露的集合使用只读接口或只读视图；内部数组/列表不能通过公开属性直接泄漏。
- StoredItemState、TileCoordinate、EntityReference 和 ID wrapper 是值对象或关系类型，不单独成为 Component。
- TileEntityPersistentId 和 NetworkId 的底层表示仍未裁决。本草案暂时使用 int? 作为占位形状，并在代码旁写明不得与 runtime ID 合并。
- StructureFootprintComponent.Origin 与 TileEntityAnchorComponent.Origin 暂时同时保留，是为了保留已批准设计中的组合不变量；最终实现必须选择一个权威字段，不能让两个字段独立漂移。
- PylonRegistryComponent 的最终命名空间和 owner 仍未裁决；代码按 WorldStorage 侧候选路径展示。
- 代码片段遵循当前项目 net10.0、nullable enabled、implicit usings enabled 和 2-space 缩进的现有约定，但本 Markdown 没有经过编译。

### 2.2 29 个 Component 草案状态

| componentId | Component | status | 拟定文件 | 当前覆盖 |
|---|---|---|---|---|
| WIS-COMP-01 | TileCellComponent | partial | src/WorldInteraction/Tiles/TileCellComponent.cs | 同名组件，缺 raw header 和完整液体工作边界 |
| WIS-COMP-02 | TileFrameComponent | existing | src/WorldInteraction/Tiles/TileFrameComponent.cs | 同名组件 |
| WIS-COMP-03 | TileSignalTopologyComponent | existing | src/WorldInteraction/Tiles/TileSignalTopologyComponent.cs | 同名组件 |
| WIS-COMP-04 | TileLiquidWorkStateComponent | proposed | src/WorldInteraction/Tiles/TileLiquidWorkStateComponent.cs | 仅 TileCellState 局部字段 |
| WIS-COMP-05 | PressurePlateOccupancyComponent | partial | src/WorldInteraction/PressurePlates/PressurePlateOccupancyComponent.cs | 同名组件，owner 未决 |
| WIS-COMP-06 | MechanismCooldownComponent | partial | src/WorldInteraction/Wiring/MechanismCooldownComponent.cs | 同名组件，Version4 登记/清理未闭合 |
| WIS-COMP-07 | WirePropagationScratchComponent | proposed | src/WorldInteraction/Wiring/WirePropagationScratchComponent.cs | 无完整同名组件 |
| WIS-COMP-08 | PumpTransferScratchComponent | proposed | src/WorldInteraction/Wiring/PumpTransferScratchComponent.cs | 无同名组件 |
| WIS-COMP-09 | InteractionActorContextComponent | proposed | src/WorldInteraction/Interaction/InteractionActorContextComponent.cs | 无同名组件 |
| WIS-COMP-10 | TileEntityAnchorComponent | existing | src/WorldInteraction/TileEntities/TileEntityAnchorComponent.cs | 同名组件 |
| WIS-COMP-11 | TileEntityKindComponent | existing | src/WorldInteraction/TileEntities/TileEntityKindComponent.cs | 同名组件 |
| WIS-COMP-12 | TileEntityRuntimeIdComponent | partial | src/WorldInteraction/TileEntities/TileEntityRuntimeIdComponent.cs | 同名组件，索引 owner 未决 |
| WIS-COMP-13 | TileEntityPersistenceIdentityComponent | proposed | src/WorldInteraction/TileEntities/TileEntityPersistenceIdentityComponent.cs | 无同名组件 |
| WIS-COMP-14 | TileEntityNetworkIdentityComponent | proposed | src/WorldInteraction/TileEntities/TileEntityNetworkIdentityComponent.cs | 无同名组件 |
| WIS-COMP-15 | TileEntityUpdateScheduleComponent | existing | src/WorldInteraction/TileEntities/TileEntityUpdateScheduleComponent.cs | 同名组件 |
| WIS-COMP-16 | LogicSensorComponent | partial | src/WorldInteraction/TileEntities/LogicSensorComponent.cs | 同名组件，外部事实/失效清理未闭合 |
| WIS-COMP-17 | TrainingDummyComponent | partial | src/WorldInteraction/TileEntities/TrainingDummyComponent.cs | 同名组件，Version4 关系语义不完整 |
| WIS-COMP-18 | ItemFrameComponent | partial | src/WorldInteraction/TileEntities/ItemFrameComponent.cs | 同名组件 |
| WIS-COMP-19 | FoodPlatterComponent | partial | src/WorldInteraction/TileEntities/FoodPlatterComponent.cs | 同名组件 |
| WIS-COMP-20 | WeaponRackComponent | partial | src/WorldInteraction/TileEntities/WeaponRackComponent.cs | 同名组件 |
| WIS-COMP-21 | HatRackComponent | existing | src/WorldInteraction/TileEntities/HatRackComponent.cs | 同名组件 |
| WIS-COMP-22 | DisplayDollComponent | partial | src/WorldInteraction/TileEntities/DisplayDollComponent.cs | 同名组件 |
| WIS-COMP-23 | DeadCellsDisplayJarComponent | partial | src/WorldInteraction/TileEntities/DeadCellsDisplayJarComponent.cs | 同名组件 |
| WIS-COMP-24 | LeashedEntityAnchorComponent | partial | src/WorldInteraction/TileEntities/LeashedEntityAnchorComponent.cs | 同名组件 |
| WIS-COMP-25 | StructureFootprintComponent | proposed | src/WorldInteraction/Structures/StructureFootprintComponent.cs | 无通用同名组件 |
| WIS-COMP-26 | ChestStructureComponent | proposed | src/WorldInteraction/Structures/ChestStructureComponent.cs | WorldChestState 局部字段 |
| WIS-COMP-27 | MultiSlotItemPayloadComponent | proposed | src/WorldInteraction/Structures/MultiSlotItemPayloadComponent.cs | WorldChestState 局部字段 |
| WIS-COMP-28 | PylonStructureComponent | proposed | src/WorldInteraction/Structures/PylonStructureComponent.cs | 无同名组件 |
| WIS-COMP-29 | PylonRegistryComponent | partial | src/WorldStorage/Pylons/PylonRegistryComponent.cs | PylonRegistryState/Entry 局部字段 |

## 3. 文件布局和依赖草图

代码草案建议按能力边界保持以下布局：

~~~text
src/
├─ WorldInteraction/
│  ├─ Interaction/
│  │  └─ InteractionActorContextComponent.cs
│  ├─ PressurePlates/
│  │  └─ PressurePlateOccupancyComponent.cs
│  ├─ Structures/
│  │  ├─ ChestStructureComponent.cs
│  │  ├─ MultiSlotItemPayloadComponent.cs
│  │  ├─ PylonStructureComponent.cs
│  │  └─ StructureFootprintComponent.cs
│  ├─ TileEntities/
│  │  ├─ DeadCellsDisplayJarComponent.cs
│  │  ├─ DisplayDollComponent.cs
│  │  ├─ FoodPlatterComponent.cs
│  │  ├─ HatRackComponent.cs
│  │  ├─ ItemFrameComponent.cs
│  │  ├─ LeashedEntityAnchorComponent.cs
│  │  ├─ LogicSensorComponent.cs
│  │  ├─ StoredItemState.cs
│  │  ├─ TileEntityAnchorComponent.cs
│  │  ├─ TileEntityKindComponent.cs
│  │  ├─ TileEntityNetworkIdentityComponent.cs
│  │  ├─ TileEntityPersistenceIdentityComponent.cs
│  │  ├─ TileEntityRuntimeIdComponent.cs
│  │  ├─ TileEntityUpdateScheduleComponent.cs
│  │  ├─ TrainingDummyComponent.cs
│  │  └─ WeaponRackComponent.cs
│  ├─ Tiles/
│  │  ├─ LiquidKind.cs
│  │  ├─ TileCellComponent.cs
│  │  ├─ TileCoordinate.cs
│  │  ├─ TileFrameComponent.cs
│  │  ├─ TileLiquidWorkStateComponent.cs
│  │  └─ TileSignalTopologyComponent.cs
│  └─ Wiring/
│     ├─ MechanismCooldownComponent.cs
│     ├─ MechanismCooldownEntry.cs
│     ├─ PumpTransferScratchComponent.cs
│     └─ WirePropagationScratchComponent.cs
└─ WorldStorage/
   └─ Pylons/
      └─ PylonRegistryComponent.cs
~~~

当前已有的值对象和类型注册文件继续沿用现有位置。尤其是 TileCoordinate、LiquidKind、LogicCheckType、TileEntityKindId、TileEntityRuntimeId、StoredItemState、EntityReference、ChestSlot 和 PylonRegistryEntry 不应因为草案布局而复制出第二份类型。

## 4. Tile 和基础组件代码草案

### 4.1 src/WorldInteraction/Tiles/TileCellComponent.cs

状态：partial。当前类型已存在；以下代码是在现有五个语义字段基础上补出 raw header 的草案形状。

~~~csharp
namespace Terraria.WorldInteraction.Tiles;

public sealed class TileCellComponent
{
  public ushort TileType { get; internal set; }

  public bool IsActive { get; internal set; }

  public ushort WallType { get; internal set; }

  public byte LiquidAmount { get; internal set; }

  public LiquidKind LiquidKind { get; internal set; }

  // Version4 Tile.sTileHeader 的兼容形状。
  // 具体 header 位语义仍需与 TileSignalTopologyComponent 整合。
  public ushort StructuralHeader { get; internal set; }

  // Version4 bTileHeader、bTileHeader2、bTileHeader3 的兼容形状。
  public byte Header1 { get; internal set; }

  public byte Header2 { get; internal set; }

  public byte Header3 { get; internal set; }
}
~~~

字段不变量：

- LiquidAmount 和 LiquidKind 共同解释液体事实；LiquidAmount == 0 时不能从 LiquidKind 推断存在液体。
- StructuralHeader、Header1、Header2、Header3 不得与线路或 active 语义形成两个互相独立的可写副本。
- 液体工作标记不放在此类型中，归入 TileLiquidWorkStateComponent。

### 4.2 src/WorldInteraction/Tiles/TileCoordinate.cs

状态：existing，值对象，不是 Component。

~~~csharp
namespace Terraria.WorldInteraction.Tiles;

public readonly record struct TileCoordinate(int X, int Y);
~~~

### 4.3 src/WorldInteraction/Tiles/LiquidKind.cs

状态：existing，值枚举，不是 Component。

~~~csharp
namespace Terraria.WorldInteraction.Tiles;

public enum LiquidKind : byte
{
  Water,
  Lava,
  Honey,
  Shimmer,
}
~~~

### 4.4 src/WorldInteraction/Tiles/TileFrameComponent.cs

状态：existing。

~~~csharp
namespace Terraria.WorldInteraction.Tiles;

public sealed class TileFrameComponent
{
  public short TileFrameX { get; internal set; }

  public short TileFrameY { get; internal set; }

  public short WallFrameX { get; internal set; }

  public short WallFrameY { get; internal set; }
}
~~~

字段不变量：

- 四个 frame 字段共同表示一个 Tile 单元的 frame 状态。
- frame 不替代 Tile 类型、墙体、线路拓扑或液体工作状态。
- frame 的合法范围和重算 owner 仍由 Tile/WorldStorage 整合裁决。

### 4.5 src/WorldInteraction/Tiles/TileSignalTopologyComponent.cs

状态：existing。

~~~csharp
namespace Terraria.WorldInteraction.Tiles;

public sealed class TileSignalTopologyComponent
{
  public bool HasWire1 { get; internal set; }

  public bool HasWire2 { get; internal set; }

  public bool HasWire3 { get; internal set; }

  public bool HasWire4 { get; internal set; }

  public bool HasActuator { get; internal set; }

  public bool IsActuated { get; internal set; }
}
~~~

字段不变量：

- 四种 wire、actuator 和 actuated 是同一 Tile 的拓扑事实，但不能与传播工作集混合。
- IsActuated 不等于 TileCellComponent.IsActive。
- HasActuator == false 时 IsActuated 的恢复语义仍需裁决。

### 4.6 src/WorldInteraction/Tiles/TileLiquidWorkStateComponent.cs

状态：proposed。当前没有同名 Component；字段证据来自 src/WorldStorage/TileCellState.cs。

~~~csharp
namespace Terraria.WorldInteraction.Tiles;

public sealed class TileLiquidWorkStateComponent
{
  public bool IsCheckingLiquid { get; internal set; }

  public uint LastLiquidChangedRevision { get; internal set; }

  public bool ShouldSkipLiquid { get; internal set; }
}
~~~

字段不变量：

- 此类不能保存 LiquidAmount 或 LiquidKind 的第二份副本。
- LastLiquidChangedRevision 的 World/Section owner 未决。
- 工作完成、取消、Tile 清理和 Section 卸载都必须有复位语义。

### 4.7 src/WorldInteraction/PressurePlates/PressurePlateOccupancyComponent.cs

状态：partial。保留当前的防御性只读快照外观。

~~~csharp
using System.Collections.Frozen;
using Terraria.Relationships;
using Terraria.WorldInteraction.Tiles;

namespace Terraria.WorldInteraction.PressurePlates;

public sealed class PressurePlateOccupancyComponent
{
  private readonly Dictionary<TileCoordinate, HashSet<EntityReference>> _occupantsByPlate = new();

  public IReadOnlyDictionary<TileCoordinate, IReadOnlySet<EntityReference>> OccupantsByPlate
  {
    get
    {
      Dictionary<TileCoordinate, IReadOnlySet<EntityReference>> occupants = new();

      foreach ((TileCoordinate coordinate, HashSet<EntityReference> occupantsAtPlate) in _occupantsByPlate)
      {
        occupants.Add(coordinate, occupantsAtPlate.ToFrozenSet());
      }

      return occupants.ToFrozenDictionary();
    }
  }

  public bool NeedsFirstUpdate { get; internal set; }
}
~~~

字段不变量：

- 公开属性只能返回快照，不能把内部 Dictionary 或 HashSet 泄漏出去。
- EntityReference 的 scope 必须是允许参与压力板占用的实体类型。
- 占用关系、首次更新默认值和实体销毁清理的唯一 owner 尚未确定，crossSubsystemOwner: integration-review。

### 4.8 src/WorldInteraction/Wiring/MechanismCooldownEntry.cs

状态：existing，条目值对象，不是 Component。

~~~csharp
using Terraria.WorldInteraction.Tiles;

namespace Terraria.WorldInteraction.Wiring;

public readonly record struct MechanismCooldownEntry(
    TileCoordinate Position,
    int RemainingTicks);
~~~

条目不变量：

- RemainingTicks 不得为负。
- 同一坐标不能有冲突的活跃条目。
- 该条目只描述机械冷却，不描述一次传播的访问状态。

### 4.9 src/WorldInteraction/Wiring/MechanismCooldownComponent.cs

状态：partial。

~~~csharp
using System.Collections.ObjectModel;

namespace Terraria.WorldInteraction.Wiring;

public sealed class MechanismCooldownComponent
{
  private readonly List<MechanismCooldownEntry> _entries = new();
  private readonly ReadOnlyCollection<MechanismCooldownEntry> _entriesView;

  public MechanismCooldownComponent()
  {
    _entriesView = _entries.AsReadOnly();
  }

  public IReadOnlyList<MechanismCooldownEntry> Entries => _entriesView;

  public int CannonCooldownTicks { get; internal set; }

  public int BunnyCannonCooldownTicks { get; internal set; }

  public int SnowballCannonCooldownTicks { get; internal set; }
}
~~~

字段不变量：

- 三个炮计数从零开始，不得进入负数。
- Entries 与炮计数属于相同的冷却生命周期，但不能和传播 scratch 共用清理。
- Version4 _mechX/_mechY/_mechTime 的容量和越界删除仍未完整映射到该类型。

## 5. 线路工作集和交互上下文代码草案

### 5.1 src/WorldInteraction/Wiring/WirePropagationScratchComponent.cs

状态：proposed。这是一次传播范围的暂态 Component，不属于任何单个 wire Tile。

~~~csharp
using Terraria.WorldInteraction.Tiles;

namespace Terraria.WorldInteraction.Wiring;

public sealed class WirePropagationScratchComponent
{
  public const int TeleportTargetCount = 2;

  private readonly HashSet<TileCoordinate> _skippedTiles = new();
  private readonly Queue<TileCoordinate> _frontier = new();
  private readonly Queue<byte> _frontierDirections = new();
  private readonly Dictionary<TileCoordinate, byte> _tilesToProcess = new();
  private readonly Queue<TileCoordinate> _currentGates = new();
  private readonly Queue<TileCoordinate> _nextGates = new();
  private readonly Queue<TileCoordinate> _lampsToCheck = new();
  private readonly HashSet<TileCoordinate> _completedGates = new();
  private readonly Dictionary<TileCoordinate, byte> _pixelBoxTriggers = new();
  private readonly TileCoordinate?[] _teleportTargets = new TileCoordinate?[TeleportTargetCount];

  public IReadOnlySet<TileCoordinate> SkippedTiles => _skippedTiles;

  public IReadOnlyCollection<TileCoordinate> Frontier => _frontier;

  public IReadOnlyCollection<byte> FrontierDirections => _frontierDirections;

  public IReadOnlyDictionary<TileCoordinate, byte> TilesToProcess => _tilesToProcess;

  public IReadOnlyCollection<TileCoordinate> CurrentGates => _currentGates;

  public IReadOnlyCollection<TileCoordinate> NextGates => _nextGates;

  public IReadOnlyCollection<TileCoordinate> LampsToCheck => _lampsToCheck;

  public IReadOnlySet<TileCoordinate> CompletedGates => _completedGates;

  public IReadOnlyDictionary<TileCoordinate, byte> PixelBoxTriggers => _pixelBoxTriggers;

  public IReadOnlyList<TileCoordinate?> TeleportTargets => _teleportTargets;

  public byte? CurrentWireColor { get; internal set; }

  public bool IsRunning { get; internal set; }

  public bool BlockPlayerTeleportationForOneIteration { get; internal set; }
}
~~~

草案说明：

- Frontier 与 FrontierDirections 必须保持一一对应；当前只暴露只读集合，具体推进操作不在 Component 草案中定义。
- TeleportTargets 只保留两个传播槽位；实际位置类型和传送关系仍需整合裁决。
- 所有集合都是暂态缓存，不能进入世界存档。
- CurrentWireColor 不能成为 TileSignalTopologyComponent 的镜像。
- 当前 Version4 使用 DoubleStack，本草案使用 Queue 只是容器形状占位，不宣称顺序等价。

### 5.2 src/WorldInteraction/Wiring/PumpTransferScratchComponent.cs

状态：proposed。

~~~csharp
using System.Collections.ObjectModel;
using Terraria.WorldInteraction.Tiles;

namespace Terraria.WorldInteraction.Wiring;

public sealed class PumpTransferScratchComponent
{
  public const int MaxPumpCount = 20;

  private readonly TileCoordinate[] _inputPumpPositions = new TileCoordinate[MaxPumpCount];
  private readonly TileCoordinate[] _outputPumpPositions = new TileCoordinate[MaxPumpCount];
  private readonly ReadOnlyCollection<TileCoordinate> _inputPumpPositionsView;
  private readonly ReadOnlyCollection<TileCoordinate> _outputPumpPositionsView;

  public PumpTransferScratchComponent()
  {
    _inputPumpPositionsView = Array.AsReadOnly(_inputPumpPositions);
    _outputPumpPositionsView = Array.AsReadOnly(_outputPumpPositions);
  }

  public IReadOnlyList<TileCoordinate> InputPumpPositions => _inputPumpPositionsView;

  public IReadOnlyList<TileCoordinate> OutputPumpPositions => _outputPumpPositionsView;

  public int InputPumpCount { get; internal set; }

  public int OutputPumpCount { get; internal set; }
}
~~~

字段不变量：

- InputPumpCount 和 OutputPumpCount 必须处于 0..MaxPumpCount。
- 坐标数组只服务当前传播，不保存液体量、类型或转移结果。
- XferWater 在 Version4 当前文件中为空体，液体 owner、容量和失败语义仍为 evidence-gap。
- crossSubsystemOwner: integration-review。

### 5.3 src/WorldInteraction/Interaction/InteractionActorContextComponent.cs

状态：proposed。

~~~csharp
namespace Terraria.WorldInteraction.Interaction;

public sealed class InteractionActorContextComponent
{
  // null 表示没有可确认的 Version4 玩家索引。
  // Version4 的兼容无调用者值为 255，但不把 255 暴露为有效玩家。
  public byte? InitiatingPlayerIndex { get; internal set; }
}
~~~

字段不变量：

- 有效玩家索引为 0..254；255 只作为旧边界兼容值，不是有效玩家。
- 该 Component 只属于一次交互上下文，不能挂在 World 静态实体上。
- 玩家索引到 NLTX EntityReference 的映射未锁定，crossSubsystemOwner: integration-review。

## 6. TileEntity 通用身份和专属状态代码草案

### 6.1 src/WorldInteraction/TileEntities/TileEntityAnchorComponent.cs

状态：existing。

~~~csharp
using Terraria.WorldInteraction.Tiles;

namespace Terraria.WorldInteraction.TileEntities;

public sealed class TileEntityAnchorComponent
{
  public TileCoordinate Origin { get; internal set; }
}
~~~

Origin 是 TileEntity 的权威锚点候选。它不能被当成多格结构的完整覆盖范围；覆盖范围由 StructureFootprintComponent 表达。

### 6.2 src/WorldInteraction/TileEntities/TileEntityKindComponent.cs

状态：existing。

~~~csharp
namespace Terraria.WorldInteraction.TileEntities;

public sealed class TileEntityKindComponent
{
  public TileEntityKindId Kind { get; internal set; }
}
~~~

Kind 是类型注册 ID，不是 TileType、runtime ID、persistent ID 或 network ID。

### 6.3 src/WorldInteraction/TileEntities/TileEntityRuntimeIdComponent.cs

状态：partial。

~~~csharp
namespace Terraria.WorldInteraction.TileEntities;

public sealed class TileEntityRuntimeIdComponent
{
  public TileEntityRuntimeId Id { get; internal set; }
}
~~~

Id 只表示当前世界会话内的运行时实例身份。Version4 的 ByID、TileEntitiesNextID 和实例 ID 证明运行时索引存在，但不证明它可以直接承担长期存档或网络身份。

### 6.4 src/WorldInteraction/TileEntities/TileEntityPersistenceIdentityComponent.cs

状态：proposed。底层类型未决，以下 int? 只能作为草案占位。

~~~csharp
namespace Terraria.WorldInteraction.TileEntities;

public sealed class TileEntityPersistenceIdentityComponent
{
  // TODO [BD-COMP-04]:
  // 替换为整合裁决后的持久化 ID 类型。
  // 不得直接复用 TileEntityRuntimeIdComponent.Id。
  public int? PersistentId { get; internal set; }
}
~~~

草案约束：

- null 表示当前没有已确认的持久化身份。
- 这里的 int? 不是 Version4 已证实的独立字段类型。
- 旧存档中写入的 Version4 ID 只能作为兼容输入候选，不能直接推出最终 persistent ID 设计。
- crossSubsystemOwner: integration-review。

### 6.5 src/WorldInteraction/TileEntities/TileEntityNetworkIdentityComponent.cs

状态：proposed。底层类型未决，以下 int? 只能作为草案占位。

~~~csharp
namespace Terraria.WorldInteraction.TileEntities;

public sealed class TileEntityNetworkIdentityComponent
{
  // TODO [BD-COMP-04]:
  // 替换为连接范围内确定的 NetworkId 类型。
  // 不得与 runtime ID 或 persistent ID 共用字段。
  public int? NetworkId { get; internal set; }
}
~~~

草案约束：

- null 表示当前连接范围没有已确认的网络身份。
- 网络写入省略 Version4 ID，但当前证据不足以确认网络身份来源。
- 该 Component 不应进入 World 持久化快照。
- crossSubsystemOwner: integration-review。

### 6.6 src/WorldInteraction/TileEntities/TileEntityUpdateScheduleComponent.cs

状态：existing。

~~~csharp
namespace Terraria.WorldInteraction.TileEntities;

public sealed class TileEntityUpdateScheduleComponent
{
  public bool RequiresUpdates { get; internal set; }
}
~~~

RequiresUpdates 只表达实体是否具有更新资格，不保存队列位置、更新时间或工作集。Version4 的 UpdateEntities 是索引集合，不复制为实体字段。

### 6.7 src/WorldInteraction/TileEntities/LogicSensorComponent.cs

状态：partial。

~~~csharp
namespace Terraria.WorldInteraction.TileEntities;

public sealed class LogicSensorComponent
{
  public LogicCheckType CheckType { get; internal set; }

  public bool IsOn { get; internal set; }

  public int CountedData { get; internal set; }
}
~~~

字段不变量：

- CheckType 必须与宿主 Tile frame 语义一致。
- IsOn 是传感器权威状态候选，不能仅由 frame 的当前值替代。
- CountedData 是短暂的状态记忆，不是物品数量。
- Date/Player/Liquid 外部事实不复制进此 Component；CountedData 的存档语义仍未确认。

### 6.8 src/WorldInteraction/TileEntities/TrainingDummyComponent.cs

状态：partial。

~~~csharp
using Terraria.Relationships;

namespace Terraria.WorldInteraction.TileEntities;

public sealed class TrainingDummyComponent
{
  public EntityReference Npc { get; internal set; } = EntityReference.None;

  public int ActivationRetryCooldownTicks { get; internal set; }

  public bool IsActive => !Npc.IsEmpty;
}
~~~

字段不变量：

- NPC 的完整状态不复制到训练假人 Component。
- IsActive 是派生属性，不单独保存。
- Npc 失效时必须回到 EntityReference.None；Version4 的 int NPC 索引到 EntityReference 的映射仍未锁定。

### 6.9 src/WorldInteraction/TileEntities/StoredItemState.cs

状态：existing，值对象，不是 Component。

~~~csharp
namespace Terraria.WorldInteraction.TileEntities;

public readonly record struct StoredItemState(int Type, byte Prefix, int Stack)
{
  public bool IsEmpty => Type == 0 || Stack <= 0;
}
~~~

Type、Prefix、Stack 必须作为一个 payload tuple 共同验证、恢复和清空；不要把它们拆成三个独立的 Component。

### 6.10 单物品承载 Components

以下三个文件状态均为 partial，且故意不合并为一个通用 Component，因为宿主 Tile 类型、合法性和破坏/掉落生命周期不同。

#### src/WorldInteraction/TileEntities/ItemFrameComponent.cs

~~~csharp
namespace Terraria.WorldInteraction.TileEntities;

public sealed class ItemFrameComponent
{
  public StoredItemState Item { get; internal set; }
}
~~~

#### src/WorldInteraction/TileEntities/FoodPlatterComponent.cs

~~~csharp
namespace Terraria.WorldInteraction.TileEntities;

public sealed class FoodPlatterComponent
{
  public StoredItemState Item { get; internal set; }
}
~~~

#### src/WorldInteraction/TileEntities/WeaponRackComponent.cs

~~~csharp
namespace Terraria.WorldInteraction.TileEntities;

public sealed class WeaponRackComponent
{
  public StoredItemState Item { get; internal set; }
}
~~~

单物品 Component 的共同不变量：

- 空 payload 使用 StoredItemState 零值；空 payload 不得伴随正 stack。
- Weapon Rack 的 payload 非空会影响宿主可破坏性。
- 宿主破坏时 payload 消费与结构清理必须保持一致，但具体外部效果不属于 Component 草案。

### 6.11 src/WorldInteraction/TileEntities/HatRackComponent.cs

状态：existing。

~~~csharp
using System.Collections.ObjectModel;

namespace Terraria.WorldInteraction.TileEntities;

public sealed class HatRackComponent
{
  private readonly StoredItemState[] _items = new StoredItemState[2];
  private readonly StoredItemState[] _dyes = new StoredItemState[2];
  private readonly ReadOnlyCollection<StoredItemState> _itemsView;
  private readonly ReadOnlyCollection<StoredItemState> _dyesView;

  public HatRackComponent()
  {
    _itemsView = Array.AsReadOnly(_items);
    _dyesView = Array.AsReadOnly(_dyes);
  }

  public IReadOnlyList<StoredItemState> Items => _itemsView;

  public IReadOnlyList<StoredItemState> Dyes => _dyesView;

  public bool ContainsItems =>
      _items.Any(item => !item.IsEmpty) ||
      _dyes.Any(item => !item.IsEmpty);
}
~~~

字段不变量：

- Items 和 Dyes 的容量固定为 2。
- ContainsItems 是派生值，不单独保存。
- 两组槽位共同决定宿主是否为空，但不与 Display Doll 的槽位混用。

### 6.12 src/WorldInteraction/TileEntities/DisplayDollComponent.cs

状态：partial。

~~~csharp
using System.Collections.ObjectModel;

namespace Terraria.WorldInteraction.TileEntities;

public sealed class DisplayDollComponent
{
  private readonly StoredItemState[] _equipment = new StoredItemState[9];
  private readonly StoredItemState[] _dyes = new StoredItemState[9];
  private readonly StoredItemState[] _miscellaneous = new StoredItemState[1];
  private readonly ReadOnlyCollection<StoredItemState> _equipmentView;
  private readonly ReadOnlyCollection<StoredItemState> _dyesView;
  private readonly ReadOnlyCollection<StoredItemState> _miscellaneousView;

  public DisplayDollComponent()
  {
    _equipmentView = Array.AsReadOnly(_equipment);
    _dyesView = Array.AsReadOnly(_dyes);
    _miscellaneousView = Array.AsReadOnly(_miscellaneous);
  }

  public IReadOnlyList<StoredItemState> Equipment => _equipmentView;

  public IReadOnlyList<StoredItemState> Dyes => _dyesView;

  public IReadOnlyList<StoredItemState> Miscellaneous => _miscellaneousView;

  public byte Pose { get; internal set; }

  public bool ContainsItems =>
      _equipment.Any(item => !item.IsEmpty) ||
      _dyes.Any(item => !item.IsEmpty) ||
      _miscellaneous.Any(item => !item.IsEmpty);
}
~~~

字段不变量：

- Equipment、Dyes、Miscellaneous 的容量固定为 9、9、1。
- Pose 是状态字段，不是表现对象引用；未知姿态值的恢复策略仍需补证。
- ContainsItems 是派生值，不单独存储。

### 6.13 src/WorldInteraction/TileEntities/DeadCellsDisplayJarComponent.cs

状态：partial。

~~~csharp
namespace Terraria.WorldInteraction.TileEntities;

public sealed class DeadCellsDisplayJarComponent
{
  public StoredItemState Item { get; internal set; }
}
~~~

Version4 的 TEDeadCellsDisplayJar.WriteExtraData 和 ReadExtraData 为空体，因此此草案只固定单物品字段形状，不为存档/网络默认值补造实现。

### 6.14 src/WorldInteraction/TileEntities/LeashedEntityAnchorComponent.cs

状态：partial。

~~~csharp
namespace Terraria.WorldInteraction.TileEntities;

public sealed class LeashedEntityAnchorComponent
{
  public int ItemType { get; internal set; }

  public bool HasItem => ItemType > 0;
}
~~~

HasItem 是派生属性。无物品的系绳锚点不能被强制附加正物品；系绳目标关系不是本 Component 的 ID 字段。

## 7. 结构、Chest 和 Pylon 代码草案

### 7.1 src/WorldInteraction/Structures/StructureFootprintComponent.cs

状态：proposed。

~~~csharp
using Terraria.WorldInteraction.Tiles;

namespace Terraria.WorldInteraction.Structures;

public sealed class StructureFootprintComponent
{
  // 与 TileEntityAnchorComponent.Origin 的一致性是组合不变量。
  public TileCoordinate Origin { get; internal set; }

  public int Width { get; internal set; } = 1;

  public int Height { get; internal set; } = 1;

  public ushort? HostTileType { get; internal set; }
}
~~~

字段不变量：

- Width 和 Height 必须大于零。
- Origin、Width、Height 和 HostTileType 共同描述结构覆盖范围。
- 不保存覆盖格的 Tile 副本；覆盖格集合是由 footprint 派生的坐标范围。
- Pylon 已证实为 3x4；Hat Rack 等多格宿主的完整统一表仍未锁定。
- Origin 与 TileEntity common anchor 的重复字段必须在最终实现中选出唯一权威 owner。

### 7.2 src/WorldInteraction/Structures/ChestStructureComponent.cs

状态：proposed。当前 WorldChestState 提供局部字段形状。

~~~csharp
using StorageTileCoordinate = Terraria.WorldStorage.TileCoordinate;
using Terraria.WorldStorage;

namespace Terraria.WorldInteraction.Structures;

public sealed class ChestStructureComponent
{
  public ChestSlot Slot { get; internal set; }

  public StorageTileCoordinate Anchor { get; internal set; }

  public string Name { get; internal set; } = string.Empty;

  public bool IsLegacyBankChest { get; internal set; }
}
~~~

字段不变量：

- Anchor 必须与 World 坐标索引一致。
- Slot 是 Chest 容器索引，不是 TileEntity runtime ID。
- Name 的长度/编码约束必须在兼容边界落实；Component 不承担编码过程。
- Chest 结构绑定不保存 item 数组；item 数组归入 MultiSlotItemPayloadComponent。
- Chest 是否可破坏需要同时考虑 footprint 和 payload。

### 7.3 src/WorldInteraction/Structures/MultiSlotItemPayloadComponent.cs

状态：proposed。

~~~csharp
using System.Collections.ObjectModel;
using Terraria.Items;

namespace Terraria.WorldInteraction.Structures;

public sealed class MultiSlotItemPayloadComponent
{
  private readonly List<ItemState> _items = new();
  private readonly ReadOnlyCollection<ItemState> _itemsView;

  public MultiSlotItemPayloadComponent()
  {
    _itemsView = _items.AsReadOnly();
  }

  public IReadOnlyList<ItemState> Items => _itemsView;

  public int ItemCapacity { get; internal set; }
}
~~~

字段不变量：

- ItemCapacity 不得为负。
- Items.Count 必须等于约定容量；当前 NLTX 默认容量为 0，Version4 默认 40 仍只是兼容候选。
- 容量变化不能静默丢失非空 item。
- ItemState 是 Terraria.Items 侧值对象，不自动转换成 StoredItemState。
- 物品 payload 与 Chest 结构绑定共同参与宿主清理，但不合并为一个 Component。

### 7.4 src/WorldInteraction/Structures/PylonStructureComponent.cs

状态：proposed。Version4 的 TeleportPylonType 在当前 NLTX 没有已确认的同名类型，因此暂用 byte 保存兼容候选。

~~~csharp
namespace Terraria.WorldInteraction.Structures;

public sealed class PylonStructureComponent
{
  // Version4 style -> TeleportPylonType 的具体 NLTX 类型仍未确定。
  public byte PylonKind { get; internal set; }

  public ushort TileType { get; internal set; } = 597;

  public bool RequiresSolidSupport { get; internal set; } = true;
}
~~~

字段不变量：

- PylonKind 必须能够由宿主 frame/style 推导；0 的含义仍需裁决。
- TileType 当前由 Version4 Pylon 类型的 597 事实支撑。
- 结构合法性还包括 3x4 footprint 和底部三个支撑格，不能只检查一个 Tile。
- IsValid 不在此存为第二份可写状态。

### 7.5 src/WorldStorage/Pylons/PylonRegistryComponent.cs

状态：partial。这是 World 级候选 Component；当前 owner 在 WorldStorage 与 WorldInteraction 之间未决。

~~~csharp
using System.Collections.ObjectModel;
using Terraria.WorldStorage;

namespace Terraria.WorldStorage.Pylons;

public sealed class PylonRegistryComponent
{
  private readonly List<PylonRegistryEntry> _currentPylons = new();
  private readonly List<PylonRegistryEntry> _previousPylons = new();
  private readonly ReadOnlyCollection<PylonRegistryEntry> _currentPylonsView;
  private readonly ReadOnlyCollection<PylonRegistryEntry> _previousPylonsView;

  public PylonRegistryComponent()
  {
    _currentPylonsView = _currentPylons.AsReadOnly();
    _previousPylonsView = _previousPylons.AsReadOnly();
  }

  public IReadOnlyList<PylonRegistryEntry> CurrentPylons => _currentPylonsView;

  public IReadOnlyList<PylonRegistryEntry> PreviousPylons => _previousPylonsView;

  public int Count => _currentPylons.Count;

  public int RefreshCooldownTicksRemaining { get; internal set; }

  public uint Revision { get; internal set; }

  public bool HasPendingRefresh =>
      RefreshCooldownTicksRemaining == 0;
}
~~~

字段不变量：

- CurrentPylons 和 PreviousPylons 不能共享可变底层集合。
- PylonRegistryEntry.Position、Kind、TileEntityId 和 IsValid 必须与 Pylon 实体及 footprint 一致。
- CanTeleport 继续作为条目派生值，不复制为 Component 字段。
- HasPendingRefresh 的零值语义与 Version4/当前 NLTX 命名存在冲突，不能在草案中宣称最终正确。
- Revision 是否与 TileMap/TileEntity revision 统一仍未决，crossSubsystemOwner: integration-review。

## 8. Entity 与 Component 组合代码注释

本节只给出组合声明草图，不引入任何运行时执行类型。

~~~csharp
// 伪代码：仅表达组合约束，不是可编译 ECS 注册代码。

// Tile entity
// Required:
//   TileCellComponent
//   TileFrameComponent
//   TileSignalTopologyComponent
// Optional:
//   TileLiquidWorkStateComponent

// Ordinary TileEntity
// Required:
//   TileEntityAnchorComponent
//   TileEntityKindComponent
//   TileEntityRuntimeIdComponent
//   TileEntityUpdateScheduleComponent
// Optional:
//   TileEntityPersistenceIdentityComponent
//   TileEntityNetworkIdentityComponent
//   StructureFootprintComponent

// Single-item TileEntity
// Required:
//   Ordinary TileEntity components
//   one of ItemFrameComponent / FoodPlatterComponent / WeaponRackComponent
// Optional:
//   StructureFootprintComponent

// Multi-slot TileEntity
// Required:
//   Ordinary TileEntity components
//   one of HatRackComponent / DisplayDollComponent
// Optional:
//   StructureFootprintComponent

// Chest entity
// Required:
//   ChestStructureComponent
//   StructureFootprintComponent
//   MultiSlotItemPayloadComponent

// Pylon entity
// Required:
//   Ordinary TileEntity components
//   PylonStructureComponent
//   StructureFootprintComponent

// World-level context
// Optional:
//   PressurePlateOccupancyComponent
//   MechanismCooldownComponent
//   PylonRegistryComponent

// One interaction work context
// Optional:
//   InteractionActorContextComponent
//   WirePropagationScratchComponent
//   PumpTransferScratchComponent
~~~

组合不变量：

- Chest 不因为具有结构锚点就自动成为 TileEntity；它的 Slot、坐标索引和多槽 payload 是独立边界。
- Pylon 实体的 PylonStructureComponent 与 World 级 PylonRegistryComponent 不互相复制完整结构字段。
- 线路拓扑 Component 与传播 scratch Component 不能互换。
- 任何 TileEntity 身份组合都不能把 runtime ID、persistent ID 和 network ID 当成同一字段。

## 9. 现有文件到草案的变更边界

| 范围 | 草案处理 |
|---|---|
| 当前 src/WorldInteraction 已有 Component | 复制其已存在字段形状；只在文档中提出补充字段，不改文件 |
| 当前 src/WorldStorage State | 只作为字段证据和映射来源；不把存储聚合直接变成单一 Component |
| dome/src | 不纳入本次草案的实现代码 |
| Test | 不新增 verifier 或测试；当前局部验证仅作为覆盖说明 |
| Version4/完整参考/tModLoader/SS14 | 只作为证据来源，不改动、不复制实现 |
| docs/research 和原 Component-only Design | 只读，不覆盖、不追加 |
| .csproj 和项目引用 | 不修改；本草案不声称依赖关系已落地 |

## 10. 草案级 evidence-gap 和实现前阻塞项

### 10.1 字段/类型阻塞

- TileCellComponent raw header 与 TileSignalTopologyComponent 的位语义没有完整映射。
- TileLiquidWorkStateComponent 的 revision owner 以及与 LiquidSimulation 的关系未决定。
- TileEntityPersistenceIdentityComponent.PersistentId 和 TileEntityNetworkIdentityComponent.NetworkId 目前只是 int? 占位，不能直接落地。
- PylonStructureComponent.PylonKind 是否使用枚举、byte wrapper 或其他稳定值类型未决定。
- StructureFootprintComponent.Origin 与 TileEntityAnchorComponent.Origin 的重复字段需要唯一权威裁决。
- MultiSlotItemPayloadComponent 的容量和 Items 更新接口还没有在不引入外部副作用的前提下锁定。

### 10.2 生命周期/owner 阻塞

- Tile、frame、TileEntity 和 Chest 的共同提交边界未决定。
- 多格结构缺格、底部支撑失败和物品非空时的清理原子性未决定。
- PylonRegistryComponent 的 current/previous 快照交换、刷新冷却和 revision owner 未决定。
- 压力板占用与 InteractionActorContextComponent 的跨域映射未决定。
- Version4 CheckMech、XferWater、CheckLogicGate、MassWireOperationInner、GeyserTrap、DeActive、ReActive 等删减成员不能由草案补成已实现行为。
- Dead Cells Display Jar 的 Version4 读写空体不能被本草案的 StoredItemState 字段解释为完整兼容。

所有上述项目保持 partial 或 unresolved，未以默认值或代码占位符伪装成已决方案。

## 11. 手工评审清单

在任何源码落地之前，应逐项确认：

- [ ] 每个拟创建文件都有明确的 Component owner 和 namespace。
- [ ] 每个 status: proposed 类型已得到 owner、生命周期和程序集依赖的整合裁决。
- [ ] runtime ID、persistent ID、network ID 和外部 EntityReference 没有共用字段。
- [ ] Tile 结构字段、frame、wire topology 和液体工作状态没有产生隐式双写。
- [ ] Frontier 与 FrontierDirections 的容器语义经过 Version4 行为核对。
- [ ] 泵容量 20、端点清理和液体转移失败语义已由 LiquidSimulation 共同确认。
- [ ] 多格结构的 Origin、Width、Height、HostTileType 和宿主 Tile 变化具有同一不变量 owner。
- [ ] Chest 的结构绑定与多槽 payload 能够独立恢复，又不会在破坏时产生部分清理。
- [ ] Pylon 实体状态和 World 注册快照没有互相覆盖。
- [ ] 所有公开集合都不会泄漏可写内部容器。
- [ ] 当前 draft 未被误读为代码已创建、已迁移、行为等价或验证通过。

## 12. 最终声明

本文件是 WorldInteractionAndStructures 的实际代码组件草案 Markdown，不是已经创建的 C# 源码。

本文件没有修改 src/、Test/、dome/src/、dome/Test/、Version4、完整参考源码、tModLoader 文档、Space Station 14、原研究报告或 Component-only Design。

本文件没有创建 .cs、.csproj 或测试文件，没有运行构建或测试，verificationStatus: not-run。

所有 status: proposed 的 Component 都只是代码形状提案；所有 status: partial 的 Component 都只表示局部字段覆盖。本文不声明代码已创建、已迁移、行为等价、API 兼容或验证通过。
