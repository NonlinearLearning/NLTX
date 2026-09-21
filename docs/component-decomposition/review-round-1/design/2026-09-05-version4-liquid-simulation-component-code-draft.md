# LiquidSimulation Component Code Draft

## 1. 草案元数据

```text
subsystemId: LiquidSimulation
taskNumber: 01
sourceDesign: D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\design\2026-09-05-version4-liquid-simulation-component-design.md
sourceReport: D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-05-version4-liquid-simulation-public-decomposition.md
outputDraftPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\design\2026-09-05-version4-liquid-simulation-component-code-draft.md
draftKind: actual-csharp-component-skeleton
designScope: component-only
designStatus: candidate
codeStatus: draft-not-implemented
evidenceStatus: partial
nltxStatus: evidence-mismatch
verificationStatus: not-run
compileStatus: not-run
selectionMethod: 基于当前会话已确认的 Component-only Design；不扫描 research 目录，不选择其他报告
```

## 2. 草案使用说明

本文件把上一份 Component-only Design 转换成接近实际 C# 文件的组件骨架。代码块用于评审字段形状、访问修饰符、默认值和文件组织，不代表这些代码已经写入生产目录，也不代表可以直接编译或替换当前实现。

本草案遵循以下边界：

- Component 只保存数据和最小派生属性，不承载液体更新、工作消费、网络发送、存档、渲染或调度行为。
- 每个核心公共类型对应一个建议的同名 `.cs` 文件；本文件只展示内容，不创建这些 `.cs` 文件。
- 代码优先复用当前 Dome 已存在的 `LiquidType` 和 `WorldTileCoordinate`，避免在草案中凭空创建重复值对象。
- `TileLiquidStateComponent` 的最终 owner、共享值对象 owner、序列状态是否独立、接触组件统一关系和脏区承载方式仍由 `BD-COMP-01` 至 `BD-COMP-05` 裁决。
- 所有标为 `candidate` 或 `proposed` 的代码都不能被解释为已经批准的生产 API。

## 3. 建议的文件组织

建议的目标目录为：

```text
D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Liquid\Components\
```

建议的文件清单：

| 文件 | 公共类型 | 草案状态 | 主要阻塞点 |
| --- | --- | --- | --- |
| `TileLiquidStateComponent.cs` | `TileLiquidStateComponent` | candidate | BD-COMP-01、BD-COMP-02 |
| `LiquidWorkStateComponent.cs` | `LiquidWorkStateComponent` | candidate | BD-COMP-02、BD-COMP-03 |
| `LiquidWorldRuntimeStateComponent.cs` | `LiquidWorldRuntimeStateComponent` | candidate | BD-COMP-02；`WetCounter` 语义 |
| `LiquidContactStateComponent.cs` | `LiquidContactStateComponent` | candidate | BD-COMP-02、BD-COMP-04 |
| `LiquidDirtySectionStateComponent.cs` | `LiquidDirtySectionStateComponent` | candidate | BD-COMP-02、BD-COMP-05 |
| `LiquidSequenceStateComponent.cs` | `LiquidSequenceStateComponent` | proposed | BD-COMP-03 |

`LiquidType.cs` 不在本草案中重复创建。当前 Dome 已有：

```text
D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Liquid\Components\LiquidType.cs
```

当前枚举成员为 `Water`、`Lava`、`Honey`、`Shimmer`，底层类型为 `byte`。根 `src` 的 `LiquidKind` 另包含 `Nano`，因此跨层统一仍属于 `BD-COMP-02` 和 `BD-COMP-04` 的未决事项。

## 4. 当前类型映射

| 当前 NLTX 类型 | 草案映射 | 映射状态 | 说明 |
| --- | --- | --- | --- |
| `D:\TRbackup\NLTX\src\WorldInteraction\Tiles\TileCellComponent.cs:3-10` | `TileLiquidStateComponent.Amount`、`Type` | partial | 当前类型同时包含 `TileType`、`IsActive`、`WallType`，不能直接作为纯液体组件。 |
| `D:\TRbackup\NLTX\src\Share\Entity\Components\LiquidComponent.cs:3-35` | `LiquidContactStateComponent` | partial | 根 src 类型表达实体接触，但 `LiquidKind.Nano` 和 Dome `LiquidType` 尚未统一。 |
| `D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Liquid\Components\LiquidContactComponent.cs:5-59` | `LiquidContactStateComponent` | existing / partial | 字段接近目标，但 `WetTickCount` 当前为 `byte`，草案采用更宽的 `int`。 |
| `D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Liquid\Components\LiquidWorldStateComponent.cs:5-90` | `LiquidWorldRuntimeStateComponent` | existing / partial | 当前类型包含保护模式行为和策略对象；草案只抽取世界运行数据。 |
| `D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Liquid\Components\LiquidUpdateQueueComponent.cs:8-146` | `LiquidWorkStateComponent` 的字段来源 | existing / partial | 当前类型是带容器行为的工作集合；草案只抽取单个工作关联的数据形状。 |
| `D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Liquid\Components\LiquidDirtySectionComponent.cs:6-25` | `LiquidDirtySectionStateComponent` | existing / partial | 当前为 World 范围字典，草案展示单个 Section 的状态形状。 |
| `D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Liquid\Components\LiquidSourceComponent.cs:3-8` | 不直接映射 | partial | 来源标识不等于 Tile 液体权威状态，也不进入六项核心组件清单。 |

## 5. C# 组件骨架

### 5.1 TileLiquidStateComponent.cs

建议路径：

```text
D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Liquid\Components\TileLiquidStateComponent.cs
```

代码草案：

```csharp
namespace Terraria.Dome.Simulation.Liquid.Components;

public struct TileLiquidStateComponent
{
  public byte Amount;
  public LiquidType Type;

  public bool IsEmpty => Amount == 0;
}
```

字段说明：

| 成员 | 类型 | 默认值 | 草案语义 |
| --- | --- | --- | --- |
| `Amount` | `byte` | `0` | 对应 Version4 `Tile.liquid`，是 TileCell 液体数量的候选权威值。 |
| `Type` | `LiquidType` | `Water`（枚举零值） | 对应 Version4 `liquidType`；当 `Amount == 0` 时类型值被视为无效，建议归一化为枚举零值。 |
| `IsEmpty` | `bool` 派生属性 | `true` | 只由 `Amount` 派生，不保存第二份状态。 |

采用 `struct` 的理由是当前候选组件只有两个权威数据字段和一个派生属性，适合按 TileCell 承载数据。该选择仍受 BD-COMP-01 约束：如果最终 owner 是 WorldStorage 的集中存储，组件可能成为存储行值；如果 owner 是 WorldInteraction 受控 seam，则需要调整访问边界，不应把整个 `TileCellComponent` 原样搬进来。

当前草案不加入以下成员：

- `TileType`、`WallType`、`IsActive`。
- `CheckingLiquid`、`SkipLiquid` 或其他工作标志。
- `LiquidBuffer` 坐标、网络版本或实体接触结果。

### 5.2 LiquidWorkStateComponent.cs

建议路径：

```text
D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Liquid\Components\LiquidWorkStateComponent.cs
```

代码草案：

```csharp
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.Liquid.Components;

public struct LiquidWorkStateComponent
{
  public WorldTileCoordinate Coordinate;
  public bool IsPending;
  public bool SkipOnce;
  public int KillScore;
  public int Delay;
  public long Sequence;
  public int RetryCount;
  public bool IsBuffered;
}
```

字段说明：

| 成员 | 类型 | 默认值 | 草案语义 |
| --- | --- | --- | --- |
| `Coordinate` | `WorldTileCoordinate` | `(0, 0)` | 对应 `Liquid.x`、`Liquid.y` 或缓冲工作项坐标。当前先复用 Dome 已有值对象。 |
| `IsPending` | `bool` | `false` | 对应 `checkingLiquid` 的工作关联语义；不表示液体数量非零。 |
| `SkipOnce` | `bool` | `false` | 对应 `skipLiquid` 的一次性跳过标志。 |
| `KillScore` | `int` | `0` | 对应 `Liquid.kill` 的工作项计分或清理关联值。 |
| `Delay` | `int` | `0` | 对应 `Liquid.delay` 的暂态延迟值；单位和上限未在本草案中锁定。 |
| `Sequence` | `long` | `0` | 工作关联的候选稳定身份；不定义消费顺序。 |
| `RetryCount` | `int` | `0` | 工作关联的候选重试次数；Version4 到该字段不是完全一一对应。 |
| `IsBuffered` | `bool` | `false` | 表示该工作关联是否位于溢出工作集；不表示 `LiquidBuffer` 是第二份液体存储。 |

代码边界：

- 该组件不包含 `Amount` 或 `Type`，避免复制 `TileLiquidStateComponent` 的权威值。
- 该组件不包含 `List`、`HashSet`、`Dictionary` 或排序逻辑；容器本身不属于单个工作关联组件。
- `WorldTileCoordinate` 最终是否改为 `TileCoordinate` 取决于 BD-COMP-02；当前不能同时引用根 `src` 中的多个同名坐标类型。
- `Sequence` 与 `LiquidSequenceStateComponent` 的关系取决于 BD-COMP-03；如果不保留序列组件，必须重新定义该字段的来源，不能使用数组下标代替稳定身份。

### 5.3 LiquidWorldRuntimeStateComponent.cs

建议路径：

```text
D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Liquid\Components\LiquidWorldRuntimeStateComponent.cs
```

代码草案：

```csharp
namespace Terraria.Dome.Simulation.Liquid.Components;

public sealed class LiquidWorldRuntimeStateComponent
{
  public const int DefaultMaxLiquidBuffer = 50000;
  public const int DefaultMaxLiquid = 25000;
  public const int DefaultCycles = 10;

  public int MaxLiquidBuffer { get; set; } = DefaultMaxLiquidBuffer;
  public int MaxLiquid { get; set; } = DefaultMaxLiquid;
  public int CurrentMaxLiquid { get; set; } = DefaultMaxLiquid;
  public int Cycles { get; set; } = DefaultCycles;
  public int CurrentWorkCount { get; set; }
  public bool IsStuckCleanup { get; set; }
  public bool QuickFall { get; set; }
  public bool QuickSettle { get; set; }
  public int SkipCount { get; set; }
  public int StuckCount { get; set; }
  public int StuckAmount { get; set; }
  public int WetCounter { get; set; }
  public int PanicCounter { get; set; }
  public bool PanicMode { get; set; }
  public int PanicY { get; set; }
}
```

字段说明：

| 成员 | 类型 | 默认值 | 草案语义 |
| --- | --- | --- | --- |
| `MaxLiquidBuffer` | `int` | `50000` | Version4 的溢出工作集容量上限。 |
| `MaxLiquid` | `int` | `25000` | Version4 的默认液体处理预算上限。 |
| `CurrentMaxLiquid` | `int` | `25000` 草案初值 | 当前世界实际使用的预算上限；重新初始化时可被调整。 |
| `Cycles` | `int` | `10` | Version4 的周期配置值。 |
| `CurrentWorkCount` | `int` | `0` | 当前液体工作关联的唯一汇总计数。 |
| `SkipCount` | `int` | `0` | 世界运行态跳过计数；Version4 字段归属需要补证。 |
| `StuckCount` | `int` | `0` | 卡滞观测计数。 |
| `StuckAmount` | `int` | `0` | 卡滞关联数量统计。 |
| `IsStuckCleanup` | `bool` | `false` | 卡滞清理保护状态。 |
| `QuickFall` | `bool` | `false` | 快速下落保护/加速状态。 |
| `QuickSettle` | `bool` | `false` | 快速稳定保护/加速状态。 |
| `WetCounter` | `int` | `0` | 世界级湿润相关计数；可能最终移出本组件。 |
| `PanicCounter` | `int` | `0` | 保护模式计数。 |
| `PanicMode` | `bool` | `false` | panic 保护状态。 |
| `PanicY` | `int` | `0` | panic 保护关联高度。 |

代码边界：

- 草案保留可变属性，是因为世界运行态在生命周期内会改变；但不在属性中加入验证、状态转换或策略对象。
- `MaxLiquidBuffer` 只表示容量，不持有缓冲容器；`CurrentWorkCount` 只表示汇总，不持有坐标。
- 当前 Dome 的 `LiquidWorldStateComponent` 中的 `Mode`、`PanicPolicy`、`ObserveQueueLength` 和 `SetMode` 不直接复制到本组件；它们包含行为和策略语义，超出 Component-only 草案范围。
- `WetCounter` 是否仍属于世界运行态是 BD-GAP-05；在裁决前不能删除，也不能宣布它是实体接触计数。

### 5.4 LiquidContactStateComponent.cs

建议路径：

```text
D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Liquid\Components\LiquidContactStateComponent.cs
```

代码草案：

```csharp
namespace Terraria.Dome.Simulation.Liquid.Components;

public struct LiquidContactStateComponent
{
  public long? ResolvedAtTick;
  public bool IsWet;
  public bool IsLavaWet;
  public bool IsHoneyWet;
  public bool IsShimmerWet;
  public int WetTickCount;
  public LiquidType? DominantLiquidType;

  public bool HasAnyLiquidContact => IsWet || IsLavaWet ||
    IsHoneyWet || IsShimmerWet;
}
```

字段说明：

| 成员 | 类型 | 默认值 | 草案语义 |
| --- | --- | --- | --- |
| `ResolvedAtTick` | `long?` | `null` | 最近一次实体接触结果对应的世界 tick。 |
| `IsWet` | `bool` | `false` | 一般湿润结果。 |
| `IsLavaWet` | `bool` | `false` | 岩浆湿润结果。 |
| `IsHoneyWet` | `bool` | `false` | 蜂蜜湿润结果。 |
| `IsShimmerWet` | `bool` | `false` | 微光湿润结果。 |
| `WetTickCount` | `int` | `0` | 湿润结果持续计数；草案从当前 Dome 的 `byte` 扩大为 `int`，这是兼容决策点。 |
| `DominantLiquidType` | `LiquidType?` | `null` | 当前接触结果中的主液体类型。 |
| `HasAnyLiquidContact` | `bool` 派生属性 | `false` | 由四个湿润标志派生。 |

代码边界：

- 该组件附着于 Player、NPC、Projectile 等实体，不附着于 TileCell。
- 不包含 `LiquidAmount`、Tile 坐标或工作关联；接触结果不能反向成为 Tile 液体权威。
- 当前 Dome 的 `Replace`、`Clear` 等修改方法不放入草案；状态写入边界尚未完成 owner 裁决。
- 根 `LiquidComponent` 的 `InLiquid`、`LiquidTimer` 和 `LiquidKind.Nano` 尚未形成与 Dome 类型的唯一映射，受 BD-COMP-04 约束。

### 5.5 LiquidDirtySectionStateComponent.cs

建议路径：

```text
D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Liquid\Components\LiquidDirtySectionStateComponent.cs
```

代码草案：

```csharp
namespace Terraria.Dome.Simulation.Liquid.Components;

public struct LiquidDirtySectionStateComponent
{
  public long Revision;
  public bool IsDirty;
}
```

字段说明：

| 成员 | 类型 | 默认值 | 草案语义 |
| --- | --- | --- | --- |
| `Revision` | `long` | `0` | 单个 WorldSection 的液体状态版本。 |
| `IsDirty` | `bool` | `false` | 表示该 Section 存在尚未发布的液体变更。 |

承载说明：

- 该草案展示的是“每个 WorldSection 一个组件”的形状，因此 Section 身份由外部实体/索引承载，不在组件内重复保存坐标。
- 当前 Dome 的 `LiquidDirtySectionComponent` 是 World 范围 `Dictionary<WorldSectionCoordinates, long>`；它可以作为过渡承载，但不能与每 Section 组件同时成为权威。
- 如果 BD-COMP-05 最终选择 World 范围集合，则本文件的字段可以保留为集合元素形状，但公共类型和附着范围需要重命名或重新裁决。
- 本组件不保存 Tile 数量、变更坐标列表、网络快照或发布过程。

### 5.6 LiquidSequenceStateComponent.cs

建议路径：

```text
D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Liquid\Components\LiquidSequenceStateComponent.cs
```

代码草案：

```csharp
namespace Terraria.Dome.Simulation.Liquid.Components;

public struct LiquidSequenceStateComponent
{
  public long NextLiquidSequence;
}
```

字段说明：

| 成员 | 类型 | 默认值 | 草案语义 |
| --- | --- | --- | --- |
| `NextLiquidSequence` | `long` | `0` | 下一次工作关联身份的候选来源。 |

代码边界：

- 该类型不是 Version4 原生类型，而是当前设计为区分工作身份和世界运行计数提出的候选。
- 它不保存排序规则、不保存工作项集合，也不执行递增操作；这些行为不属于 Component-only 草案。
- 如果 BD-COMP-03 选择合并，则字段可以移动到 `LiquidWorldRuntimeStateComponent`；如果选择拒绝，则本文件和该 `.cs` 文件均不应落地。

## 6. 默认值与状态不变量

以下规则是代码草案必须保留的注释级契约；它们尚未转成组件方法或运行时验证：

| 规则 | 涉及组件 | 约束 |
| --- | --- | --- |
| 空液体归一化 | `TileLiquidStateComponent` | `Amount == 0` 时 `Type` 不具有有效语义，建议保持枚举零值。 |
| 权威值不复制 | `TileLiquidStateComponent`、`LiquidWorkStateComponent` | 工作组件不能缓存一份可独立修改的 `Amount` 或 `Type`。 |
| 工作状态可缺省 | `LiquidWorkStateComponent` | 没有待处理关联时，组件可不存在；缺省不改变 Tile 液体数量。 |
| 工作状态不持久化 | `LiquidWorkStateComponent` | 世界加载后根据 Tile 液体状态重建，不能把 `IsPending` 等字段当作世界存档字段。 |
| 世界运行态不持久化 | `LiquidWorldRuntimeStateComponent` | 世界重新初始化时运行计数和保护状态回到初始化值。 |
| 接触状态不反向写入 Tile | `LiquidContactStateComponent` | 实体湿润结果只能作为实体侧状态，不能成为 Tile 的液体数量来源。 |
| Section 标记不存液体 | `LiquidDirtySectionStateComponent` | `Revision` 和 `IsDirty` 不能替代 Tile 液体字段。 |
| 序列不等于顺序 | `LiquidSequenceStateComponent`、`LiquidWorkStateComponent` | `Sequence` 只提供身份或去重依据，不以字段声明处理先后。 |

## 7. 与现有代码的最小兼容映射

本节只记录字段映射，不定义迁移过程，也不授权修改现有类型。

| 现有成员 | 目标草案成员 | 映射处理 | 风险 |
| --- | --- | --- | --- |
| `TileCellComponent.LiquidAmount` | `TileLiquidStateComponent.Amount` | 直接映射候选 | `TileCellComponent` 还混有非液体字段，owner 未闭合。 |
| `TileCellComponent.LiquidKind` | `TileLiquidStateComponent.Type` | 需要 `LiquidKind` 到 `LiquidType` 的明确映射 | `Nano` 没有 Version4 对应值。 |
| `LiquidComponent.IsWet` 等标志 | `LiquidContactStateComponent` 同名标志 | 可保留语义映射 | 根 src 与 Dome 组件同时存在。 |
| `LiquidComponent.LiquidTimer` | `LiquidContactStateComponent.WetTickCount` | 需要 `byte` 到 `int` 的兼容决策 | 当前 Dome 也使用 `byte WetTickCount`。 |
| `LiquidContactComponent.ResolvedAtTick` | `LiquidContactStateComponent.ResolvedAtTick` | 直接映射候选 | `WorldTime` 是否替代 `long` 未裁决。 |
| `LiquidContactComponent.DominantLiquidType` | 同名字段 | 直接映射候选 | `LiquidType` owner 仍受 BD-COMP-02 影响。 |
| `LiquidUpdateQueueComponent` 的 `LiquidUpdateNode` | `LiquidWorkStateComponent.Coordinate`、`Sequence` | 只抽取单项状态 | 当前队列容器、去重和重试行为不进入组件。 |
| `LiquidUpdateQueueComponent.GetRetryCount` 的结果 | `LiquidWorkStateComponent.RetryCount` | 语义候选 | 需要确定按坐标还是按工作实例计数。 |
| `LiquidWorldStateComponent.MaximumQueueLength` | `LiquidWorldRuntimeStateComponent.MaxLiquidBuffer` | 不能直接等同 | 一个是 Dome 队列容量，一个是 Version4 液体缓冲容量。 |
| `LiquidWorldStateComponent.TickBudget` | `LiquidWorldRuntimeStateComponent.CurrentMaxLiquid` | 不能直接等同 | 预算命名和运行语义需要补证。 |
| `LiquidDirtySectionComponent.Revisions` | `LiquidDirtySectionStateComponent.Revision` | 取单 Section 元素形状 | World 集合和 Section 组件只能有一个权威。 |

## 8. 明确不写入代码组件的内容

以下内容即使在旧代码中存在，也不放进本次组件草案：

- 液体更新、下落、稳定、反应、清理和预算消费逻辑。
- 工作集合的排序、入队、出队、重试、取消和容量管理逻辑。
- 网络包、网络快照、持久化格式和客户端表现快照。
- `LiquidBuffer` 容器本身；只保留 `IsBuffered` 这一项工作关联状态候选。
- `LiquidRenderer` 相关表现数据。
- `LiquidSourceComponent` 的来源标记；它尚未证明是六项核心组件中的独立权威状态。
- `Mode`、`PanicPolicy` 以及保护状态转换方法；这些属于行为/策略边界，不属于数据组件。
- 任何 `EntityReference`、`NetworkId`、`WorldTime` 或 `WorldSectionId` 的新定义；共享值对象 owner 尚未裁决。

## 9. 编译前阻塞项

虽然代码块已经尽量采用当前项目可识别的 C# 类型和命名空间，但在 owner 决策完成前不应将本草案当作可编译提交。主要阻塞项如下：

| 编号 | 阻塞项 | 影响代码 | 解除条件 |
| --- | --- | --- | --- |
| BD-CODE-01 | `TileLiquidStateComponent` 的真实存储 owner 未定 | `TileLiquidStateComponent.cs` | 完成 BD-COMP-01，并确定跨程序集读写边界。 |
| BD-CODE-02 | `LiquidType`、坐标、tick 和 Section 身份的共享 owner 未定 | 除 `LiquidWorldRuntimeStateComponent` 外的多个文件 | 完成 BD-COMP-02，并避免同名值对象并存为隐式映射。 |
| BD-CODE-03 | `LiquidWorkStateComponent.Sequence` 与独立序列状态的关系未定 | `LiquidWorkStateComponent.cs`、`LiquidSequenceStateComponent.cs` | 完成 BD-COMP-03，选择保留、合并或删除。 |
| BD-CODE-04 | 根接触组件与 Dome 接触组件的统一关系未定 | `LiquidContactStateComponent.cs` | 完成 BD-COMP-04，并确定 `Nano`、计时器和时间字段映射。 |
| BD-CODE-05 | Section 组件或 World 集合的唯一承载未定 | `LiquidDirtySectionStateComponent.cs` | 完成 BD-COMP-05，只保留一个权威形状。 |
| BD-CODE-06 | 世界运行态字段的精确语义尚有缺口 | `LiquidWorldRuntimeStateComponent.cs` | 关闭 `WetCounter`、`SkipCount`、`CurrentMaxLiquid` 等 evidence-gap。 |
| BD-CODE-07 | `WetTickCount` 的宽度兼容未定 | `LiquidContactStateComponent.cs` | 决定保留当前 `byte` 兼容，还是采用草案的 `int`。 |

## 10. 代码草案审查清单

本清单用于后续代码落地前的人工审查；当前不宣称已通过：

- [ ] 六个公共类型的 owner 均已裁决。
- [ ] 每个公共类型都有唯一同名文件。
- [ ] `TileLiquidStateComponent` 未重新混入 TileType、墙体或激活状态。
- [ ] `LiquidWorkStateComponent` 未复制 Tile 液体数量和类型。
- [ ] `LiquidWorldRuntimeStateComponent` 未持有工作集合或实体接触状态。
- [ ] `LiquidContactStateComponent` 未反向拥有 Tile 液体权威。
- [ ] `LiquidDirtySectionStateComponent` 的 Section 承载方式与当前 Dome 唯一权威一致。
- [ ] `LiquidSequenceStateComponent` 的保留/合并/删除决策已记录。
- [ ] `LiquidType`、Tile 坐标、Section 身份、tick 和实体引用没有重复 owner。
- [ ] 代码块中的默认值和 Version4 证据一致，兼容差异已标注。
- [ ] 所有行为、容器、网络和持久化代码保持在组件草案之外。
- [ ] 完成 owner 裁决后才运行受影响项目的串行验证。

## 11. 最终声明

本文件是 `LiquidSimulation` 的实际 C# 组件代码草案，不是生产代码变更。它把当前六项候选组件表达成接近目标文件的类型骨架，但保留了所有已知 owner、类型映射、生命周期和证据缺口。

当前建议的代码形状为：

- `TileLiquidStateComponent`：TileCell 级 `byte Amount + LiquidType Type`。
- `LiquidWorkStateComponent`：工作关联级坐标、待处理、跳过、计分、延迟、序列、重试和缓冲标志。
- `LiquidWorldRuntimeStateComponent`：World singleton 级容量、预算、计数、cycles、quick、stuck、panic 和湿润运行字段。
- `LiquidContactStateComponent`：实体级接触时间、湿润标志、持续计数和主液体类型。
- `LiquidDirtySectionStateComponent`：WorldSection 级版本和脏标志。
- `LiquidSequenceStateComponent`：World singleton 级候选序列来源。

`designStatus` 保持 `candidate`，`codeStatus` 保持 `draft-not-implemented`，`verificationStatus` 和 `compileStatus` 保持 `not-run`。本次只生成 Markdown 草案，不创建 `.cs` 文件，不修改生产代码，不运行构建或测试。
