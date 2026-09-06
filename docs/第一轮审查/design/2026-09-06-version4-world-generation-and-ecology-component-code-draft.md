# Version4 WorldGenerationAndEcology 实际代码组件草案

本文把 [WorldGenerationAndEcology Component Design](D:/TRbackup/NLTX/docs/design/2026-09-05-version4-world-generation-and-ecology-component-design.md) 转换为接近实际 C# 源码的组件草案。

这里的“实际代码草案”指：给出目标文件、命名空间、公开字段/属性、构造约束和可直接继续评审的 C# 形状；不指已经把这些代码写入 `.cs` 文件，也不指已经注册到 ECS、接入生成流程或通过编译。

## 1. 草案元数据

```yaml
documentType: component-code-draft
designScope: component-code-draft
designStatus: decision-required
artifactStatus: draft-not-implemented
evidenceStatus: partial
nltxStatus: partial
verificationStatus: not-run
subsystemId: WorldGenerationAndEcology
taskNumber: 18
generatedAt: 2026-09-06
targetProject: D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Terraria.Dome.Simulation.csproj
targetDirectory: D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\WorldGeneration\Components
targetNamespace: Terraria.Dome.Simulation.WorldGeneration
sourceDesign: D:\TRbackup\NLTX\docs\design\2026-09-05-version4-world-generation-and-ecology-component-design.md
sourceReport: D:\TRbackup\NLTX\docs\research\2026-09-05-version4-world-generation-and-ecology-public-decomposition.md
```

### 1.1 本次需求变更记录

本次需求原文：

> 生成实际代码组件草案md

提出渠道：本次对话。提出时间：2026-09-06（Asia/Shanghai）。

本次变化只增加一份代码形状草案，不改变上一份 Component-only Design 的 13 个 Component 边界，不创建生产 `.cs`、`.csproj`、测试或迁移文件。

建议变更等级：`moderate`。原因是文档从字段级设计进入代码级类型、构造约束和文件归属，可能暴露 namespace、值类型和跨项目引用决策，但当前交付仍然是只读设计产物。

## 2. 交付边界

### 2.1 本草案提供的内容

- 13 个目标 Component 的候选 C# 类型；
- 组件目标文件和当前目标项目；
- 支撑 Component 字段的最小值类型和枚举；
- 构造函数校验、只读派生属性、nullable/unset 语义和集合防御性复制方向；
- 当前 NLTX 类型到候选 Component 的替换或拆分关系；
- 每个代码片段尚未解决的 owner、跨项目引用和迁移风险。

### 2.2 本草案不提供的内容

- 不修改任何 `.cs` 或 `.csproj` 文件；
- 不注册 Arch 或其他 ECS 框架组件；当前项目已有组件没有统一 `IComponent` 标记接口，因此本草案不凭空加入 marker interface；
- 不实现 System、Query、Command、Event、Adapter、Projection、WorldFile、Liquid solver 或 Pass 执行器；
- 不决定生成调度顺序、并行策略、恢复协议、网络/存档格式或持久化提交时机；
- 不把当前已有的 `IsReady`、server host ready、局部 Tile commit 或 `WorldGenerationPipeline` 解释成完整行为等价；
- 不声明下列代码片段可以在当前工作树中直接编译。

## 3. 实际目标目录与命名空间

### 3.1 目标位置

候选生产文件全部放在：

`D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\WorldGeneration\Components\`

目标 namespace 使用当前同目录组件的既有约定：

```csharp
namespace Terraria.Dome.Simulation.WorldGeneration;
```

虽然目录名为 `Components`，当前 `WorldGeneration\Components\` 下的 `WorldSeedComponent`、`WorldRuleSnapshotComponent`、`WorldGenerationStateComponent` 和 `WorldBoundsComponent` 都使用父 namespace，而不是新增的 `Terraria.Dome.Simulation.WorldGeneration.Components`。本草案保持该约定，避免因目录名自动引入新的 namespace 边界。

### 3.2 文件清单

| 文件 | 核心类型 | 状态 | 说明 |
| --- | --- | --- | --- |
| `WorldDescriptorComponent.cs` | `WorldDescriptorComponent` | `proposed` | 世界身份和稳定几何；使用现有 `WorldBoundsComponent`。 |
| `WorldGenerationRulesComponent.cs` | `WorldGenerationRulesComponent` | `proposed` | 生成输入和 secret-seed 规则；不放运行期 Hardmode/OreTiers。 |
| `WorldGenerationLifecycleComponent.cs` | `WorldGenerationLifecycleComponent` | `proposed` | 世界准备阶段和失败事实；不单独成为完整 ready barrier。 |
| `WorldGenerationPlanComponent.cs` | `WorldGenerationPlanComponent` | `proposed` | 不可变 Pass 描述集合和权重。 |
| `GenerationPassDescriptor.cs` | `GenerationPassDescriptor` | `proposed` | Plan 内嵌值对象，不是独立 ECS Component。 |
| `WorldGenerationPassStateComponent.cs` | `WorldGenerationPassStateComponent` | `proposed` | active Pass 游标、快照和失败事实。 |
| `WorldGenerationRandomStateComponent.cs` | `WorldGenerationRandomStateComponent` | `proposed` | generation 期随机流状态。 |
| `WorldTerrainStateComponent.cs` | `WorldTerrainStateComponent` | `proposed` | 生成期地形事实和计算 revision。 |
| `BiomeEcologyStateComponent.cs` | `BiomeEcologyStateComponent` | `proposed` | 世界级 Biome/感染/统计摘要。 |
| `EcologyScheduleComponent.cs` | `EcologyScheduleComponent` | `proposed` | 生态更新开关、抽样点和预算。 |
| `HousingScanStateComponent.cs` | `HousingScanStateComponent` | `proposed` | 住房扫描中的临时状态。 |
| `TownHousingAssignmentComponent.cs` | `TownHousingAssignmentComponent` | `proposed` | 居民到房间的稳定关系和 homeless 集合。 |
| `WorldLiquidHandoffComponent.cs` | `WorldLiquidHandoffComponent` | `proposed` | 生成/加载与运行期液体状态之间的摘要交接。 |
| `ReadyTransitionComponent.cs` | `ReadyTransitionComponent` | `proposed` | 完整世界 ready facet 的候选汇总。 |
| `WorldEvilType.cs` | `WorldEvilType` | `proposed` | 规则/生态共享值类型候选；owner 未决。 |
| `WorldPreparationState.cs` | `WorldPreparationState` | `proposed` | Lifecycle 阶段值类型候选。 |
| `WorldGenerationFailure.cs` | `WorldGenerationFailure` | `proposed` | Lifecycle 失败值类型候选。 |
| `PersistentEntityId.cs` | `PersistentEntityId` | `proposed` | Town housing 稳定居民 key 候选；不能直接替换当前 int key。 |
| `TilePosition.cs` | `TilePosition` | `proposed` | Component 内部 tile 坐标值类型候选。 |

以上文件清单是代码草案清单，不是本次实际创建清单。当前工作树已有 `WorldBoundsComponent.cs` 和 `WorldGenerationStage.cs`，本草案直接引用它们，不重新定义同名类型。

## 4. 代码形状的共通约束

### 4.1 Component 形状选择

- 不可变、小型值状态优先 `readonly record struct`；
- 具有较多字段、集合或明确 world 生命周期的状态优先 `sealed class`，避免大 struct 的隐式复制；
- 组件不包含执行生成、扫描、传播或发布的业务方法；构造函数只执行参数校验和防御性复制；
- 派生属性只能从同一组件字段计算，不能作为第二份可写 authority；
- `IReadOnlyList`、`IReadOnlyDictionary` 和 `IReadOnlySet` 只表达调用方不应修改，实际实现仍必须防御性复制或冻结内部集合；
- `null` 只用于“尚未解析/尚未计算/不适用”，不能把合法的零坐标、零计数或 `false` 与未知状态混淆。

### 4.2 组件写入边界

草案中的 `public` 可变属性仅表示 ECS 存储需要可更新数据，不表示任意调用方可以随意写入。实际实现时应由对应的专责状态转换边界替换整个组件或通过受控写入完成更新。本草案不定义该边界的 System/Command 名称。

### 4.3 关系字段

所有 generation 相关组件都保留 `GenerationId` 或明确的 generation revision 关系。不能通过组件挂载顺序、文件名、静态全局变量或当前 `WorldGenerationServerState` 的对象嵌套来推断它们属于同一代。

`WorldId`、`UniqueId`、`GenerationId`、Pass ID、checkpoint revision、publication revision、居民 persistent ID 和网络 ID 不是同一类标识。任何合并都必须经过 `crossSubsystemOwner: integration-review` 裁决。

## 5. 支撑值类型草案

下列类型是 Component 字段使用的值类型，不是独立 ECS Component。它们的底层类型和 owner 仍可能在后续集成审查中调整。

### 5.1 `WorldEvilType.cs`、`WorldPreparationState.cs`、`WorldGenerationFailure.cs`

```csharp
namespace Terraria.Dome.Simulation.WorldGeneration;

public enum WorldEvilType : byte
{
  Corruption,
  Crimson
}
```

```csharp
namespace Terraria.Dome.Simulation.WorldGeneration;

public enum WorldPreparationState : byte
{
  Uninitialized,
  Loading,
  Generating,
  Ready,
  Failed,
  Unloading
}
```

```csharp
namespace Terraria.Dome.Simulation.WorldGeneration;

public enum WorldGenerationFailure : byte
{
  None,
  LoadFailed,
  GenerationFailed,
  Cancelled
}
```

这些枚举与 `src\WorldSession\WorldGeneration\` 下的同名值类型语义相近，但 `Terraria.Dome.Simulation` 当前项目没有对 `Terraria.WorldSession` 的项目引用。本草案不直接引入跨项目引用；最终 owner 由 `BD-COMP-06` 决定。

### 5.2 `TilePosition.cs`

```csharp
namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct TilePosition(int X, int Y);
```

`TilePosition` 只表达 Tile 坐标，不表达 World Entity ID、区段坐标、像素坐标或网络坐标。它不检查 World bounds；需要 bounds 检查的构造边界由持有 `WorldBoundsComponent` 的组件完成。

### 5.3 `PersistentEntityId.cs`

```csharp
namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct PersistentEntityId(ulong Value)
{
  public bool IsValid => Value != 0;
}
```

这是 Town housing 的候选稳定 key，不是当前 `TownHousingResidentKey(int NpcType)` 的无条件替换。若最终 owner 继续使用 NPC type，`TownHousingAssignmentComponent` 必须在代码层改用兼容 key，并保留稳定 identity 缺口说明。

### 5.4 `GenerationPassDescriptor.cs`

```csharp
using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct GenerationPassDescriptor
{
  public GenerationPassDescriptor(string id, double weight, int version = 1)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(id);
    if (!double.IsFinite(weight) || weight < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(weight));
    }

    if (version <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(version));
    }

    Id = id;
    Weight = weight;
    Version = version;
  }

  public string Id { get; }

  public double Weight { get; }

  public int Version { get; }
}
```

`GenerationPassDescriptor` 只描述 Plan 的数据，不包含 delegate、执行器、命令集合、WorldGrid 或调度回调。Version4 `AddGenerationPass` 的直接入口和 `RunPass` 证据不完整，因此 `Version` 只是候选字段。

## 6. 13 个 Component 代码草案

### 6.1 `WorldDescriptorComponent.cs`

目标类型：`WorldDescriptorComponent`。候选形状为 `sealed class`，因为它包含可选元数据、稳定几何和值对象关系；`SizeX/SizeY` 不重复存储，直接从现有 `WorldBoundsComponent` 派生。

```csharp
using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed class WorldDescriptorComponent
{
  public WorldDescriptorComponent(
    int worldId,
    string name,
    int seed,
    WorldBoundsComponent bounds,
    double surfaceLayer,
    double rockLayer,
    TilePosition spawnTile,
    TilePosition? dungeonTile = null,
    Guid? uniqueId = null,
    string? seedText = null,
    ulong? generatorVersion = null)
  {
    if (worldId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(worldId));
    }

    ArgumentException.ThrowIfNullOrWhiteSpace(name);
    if (!double.IsFinite(surfaceLayer) || !double.IsFinite(rockLayer))
    {
      throw new ArgumentOutOfRangeException(nameof(surfaceLayer));
    }

    if (surfaceLayer > rockLayer)
    {
      throw new ArgumentException(
        "The surface layer cannot be below the rock layer.",
        nameof(rockLayer));
    }

    if (!bounds.Contains(spawnTile.X, spawnTile.Y))
    {
      throw new ArgumentOutOfRangeException(nameof(spawnTile));
    }

    if (dungeonTile is TilePosition selectedDungeonTile &&
        !bounds.Contains(selectedDungeonTile.X, selectedDungeonTile.Y))
    {
      throw new ArgumentOutOfRangeException(nameof(dungeonTile));
    }

    if (uniqueId == Guid.Empty)
    {
      uniqueId = null;
    }

    WorldId = worldId;
    UniqueId = uniqueId;
    Name = name;
    Seed = seed;
    SeedText = seedText;
    GeneratorVersion = generatorVersion;
    Bounds = bounds;
    SurfaceLayer = surfaceLayer;
    RockLayer = rockLayer;
    SpawnTile = spawnTile;
    DungeonTile = dungeonTile;
  }

  public int WorldId { get; }

  public Guid? UniqueId { get; }

  public string Name { get; }

  public int Seed { get; }

  public string? SeedText { get; }

  public ulong? GeneratorVersion { get; }

  public WorldBoundsComponent Bounds { get; }

  public double SurfaceLayer { get; }

  public double RockLayer { get; }

  public TilePosition SpawnTile { get; }

  public TilePosition? DungeonTile { get; }

  public int SizeX => Bounds.Width;

  public int SizeY => Bounds.Height;

  public int SectionCountX => Bounds.SectionColumnCount;

  public int SectionCountY => Bounds.SectionRowCount;

  public bool HasSurface => SurfaceLayer > 50.0;
}
```

代码决策：

- `WorldBoundsComponent` 是当前 Dome 已有组件，避免再次定义 `WorldBounds`/`WorldBoundsComponent` 两套尺寸 authority；
- `UniqueId`、`GeneratorVersion` 和 `SeedText` 使用 nullable，是为了显式保留加载/兼容输入未提供的情况；
- `DungeonTile` 使用 `TilePosition?`，避免用 `(-1, -1)` 伪装为坐标；
- `WorldId` 的唯一 owner 仍是 `BD-COMP-01`，该代码片段不能独立解决 WorldSession、WorldMetadata 和 WorldFile 的重复字段问题。

当前映射：`src\WorldSession\WorldGeneration\WorldDescriptorState.cs`、`dome\src\Terraria.Dome.Simulation\World\WorldMetadata.cs`、现有 `WorldBoundsComponent`。目标类型是 `proposed`，不表示替换已发生。

### 6.2 `WorldGenerationRulesComponent.cs`

目标类型：`WorldGenerationRulesComponent`。候选形状为 `sealed class`，创建后规则只读；可选规则使用 `init` 属性，便于创建边界一次性组装而不提供运行期任意 setter。

```csharp
using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed class WorldGenerationRulesComponent
{
  public WorldGenerationRulesComponent(
    int difficulty,
    string seedVariant,
    WorldEvilType? worldEvil = null)
  {
    if (difficulty < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(difficulty));
    }

    ArgumentException.ThrowIfNullOrWhiteSpace(seedVariant);
    Difficulty = difficulty;
    SeedVariant = seedVariant;
    WorldEvil = worldEvil;
  }

  public int Difficulty { get; }

  public string SeedVariant { get; }

  public WorldEvilType? WorldEvil { get; }

  public bool IsRemixWorld { get; init; }

  public bool IsEverythingWorld { get; init; }

  public bool IsNoTrapsWorld { get; init; }

  public bool IsDrunkWorld { get; init; }

  public bool IsGoodWorld { get; init; }

  public bool IsDontStarveWorld { get; init; }

  public bool IsNotTheBeesWorld { get; init; }

  public bool IsSkyblockWorld { get; init; }

  public bool IsNoSurfaceWorld { get; init; }

  public bool IsSurfaceDesertWorld { get; init; }

  public bool IsIceBiomeWorld { get; init; }

  public bool IsTenthAnniversaryWorld { get; init; }

  public bool NoInfection { get; init; }

  public bool ExtraLiquid { get; init; }

  public bool WorldIsFrozen { get; init; }

  public bool StartInHardmode { get; init; }
}
```

代码决策：

- 不加入 `HardMode`、`OreTiers` 或后台转换字段；`StartInHardmode` 只描述创建输入；
- `WorldEvil` 使用 nullable 表达规则输入尚未解析，但它与 `BiomeEcologyStateComponent` 的 canonical owner 仍需裁决；
- `IsGoodWorld`、`IsDontStarveWorld` 等字段保持显式命名，避免把多个 secret seed 重新压缩成不可审计的整数；
- `init` 只提供对象创建时的装配语法，不表示本草案已经定义了生成规则的写入 System。

当前映射：`WorldRuleSnapshotComponent`、`WorldSeedComponent`、`WorldMetadata` 的规则字段、`WorldSession\WorldRulesState.cs` 和 `WorldSecretSeedFlags.cs`。当前 Dome 已有规则快照但没有这个完整目标 Component，故标为 `proposed`。

### 6.3 `WorldGenerationLifecycleComponent.cs`

目标类型：`WorldGenerationLifecycleComponent`。它只表示 World 生命周期阶段和失败，不把阶段 `Ready` 当作完整 World ready barrier。

```csharp
using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct WorldGenerationLifecycleComponent
{
  public WorldGenerationLifecycleComponent(
    long generationId,
    WorldPreparationState phase = WorldPreparationState.Uninitialized,
    WorldGenerationFailure failure = WorldGenerationFailure.None,
    ulong generationRevision = 0)
  {
    if (generationId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(generationId));
    }

    if (!Enum.IsDefined(phase))
    {
      throw new ArgumentOutOfRangeException(nameof(phase));
    }

    if (!Enum.IsDefined(failure))
    {
      throw new ArgumentOutOfRangeException(nameof(failure));
    }

    if (phase == WorldPreparationState.Failed &&
        failure == WorldGenerationFailure.None)
    {
      throw new ArgumentException(
        "A failed lifecycle must carry a failure category.",
        nameof(failure));
    }

    GenerationId = generationId;
    Phase = phase;
    Failure = failure;
    GenerationRevision = generationRevision;
  }

  public long GenerationId { get; }

  public WorldPreparationState Phase { get; }

  public WorldGenerationFailure Failure { get; }

  public ulong GenerationRevision { get; }

  public bool IsLoadingOrGenerating =>
    Phase == WorldPreparationState.Loading ||
    Phase == WorldPreparationState.Generating;

  public bool IsPhaseReady =>
    Phase == WorldPreparationState.Ready &&
    Failure == WorldGenerationFailure.None;

  public bool IsReady => IsPhaseReady;

  public bool HasFailed =>
    Phase == WorldPreparationState.Failed ||
    Failure != WorldGenerationFailure.None;
}
```

`IsReady` 在这段代码中只是兼容性的 phase-level 派生属性，不能作为完整 World ready 的唯一依据。实际发布条件由 `ReadyTransitionComponent` 的 facet 组合决定；本草案故意没有在 Lifecycle 中提供 `CanUpdateSimulation` 的独立真值。

当前映射：`WorldGenerationStateComponent`、`WorldGenerationLifecycleState`、`WorldPreparationState`、`WorldGenerationFailure`。当前状态类型存在，但 ready barrier 仍不完整，因此目标覆盖仍为 `partial`。

### 6.4 `WorldGenerationPlanComponent.cs`

目标类型：`WorldGenerationPlanComponent`。该 Component 只保存不可变计划，不保存执行器、命令批次、WorldGrid 或 active cursor。

```csharp
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed class WorldGenerationPlanComponent
{
  public WorldGenerationPlanComponent(
    long generationId,
    int planVersion,
    IReadOnlyList<GenerationPassDescriptor> passDescriptors,
    IReadOnlyCollection<string>? disabledPassIds = null)
  {
    if (generationId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(generationId));
    }

    if (planVersion <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(planVersion));
    }

    ArgumentNullException.ThrowIfNull(passDescriptors);
    List<GenerationPassDescriptor> descriptors = new(passDescriptors.Count);
    HashSet<string> descriptorIds = new(StringComparer.Ordinal);
    double totalWeight = 0;
    foreach (GenerationPassDescriptor descriptor in passDescriptors)
    {
      if (!descriptorIds.Add(descriptor.Id))
      {
        throw new ArgumentException(
          "Generation pass identifiers must be unique.",
          nameof(passDescriptors));
      }

      descriptors.Add(descriptor);
      totalWeight += descriptor.Weight;
      if (!double.IsFinite(totalWeight))
      {
        throw new ArgumentOutOfRangeException(
          nameof(passDescriptors),
          "The total generation pass weight must be finite.");
      }
    }

    List<string> disabledIds = disabledPassIds is null
      ? new List<string>()
      : new List<string>(disabledPassIds.Count);
    if (disabledPassIds is not null)
    {
      HashSet<string> disabledSet = new(StringComparer.Ordinal);
      foreach (string passId in disabledPassIds)
      {
        ArgumentException.ThrowIfNullOrWhiteSpace(passId);
        if (!descriptorIds.Contains(passId) || !disabledSet.Add(passId))
        {
          throw new ArgumentException(
            "Disabled passes must be unique members of the plan.",
            nameof(disabledPassIds));
        }

        disabledIds.Add(passId);
      }
    }

    GenerationId = generationId;
    PlanVersion = planVersion;
    PassDescriptors = descriptors.AsReadOnly();
    DisabledPassIds = disabledIds.AsReadOnly();
    TotalWeight = totalWeight;
  }

  public long GenerationId { get; }

  public int PlanVersion { get; }

  public IReadOnlyList<GenerationPassDescriptor> PassDescriptors { get; }

  public double TotalWeight { get; }

  public IReadOnlyList<string> DisabledPassIds { get; }
}
```

这里保留了 `System.Collections.ObjectModel` 的防御性只读包装。`TotalWeight` 是构造时计算的派生快照；如果 Version4 的权重语义并非简单总和，应在后续裁决后修改字段不变量，而不能让调用方自行解释。

当前映射：`WorldGenerationPipeline`、`WorldGenerationStage`、Pass 定义和 `WorldGenerationRequest` 的输入。现有代码有局部编排和执行，但没有已确认的完整不可变计划 authority，因此目标标为 `proposed`。

### 6.5 `WorldGenerationPassStateComponent.cs`

目标类型：`WorldGenerationPassStateComponent`。它保存 active Pass 的数据状态，不复制当前 `WorldGenerationPassState` 中的 `ReadSnapshot`、`Commands` 等运行期对象。

```csharp
using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public struct WorldGenerationPassStateComponent
{
  public WorldGenerationPassStateComponent(
    long generationId,
    WorldGenerationStage stage = WorldGenerationStage.Created,
    string? activePassId = null,
    int passVersion = 0,
    long cursor = 0,
    ulong? readSnapshotRevision = null,
    ulong? checkpointRevision = null,
    string? failureReason = null,
    bool pauseRequested = false,
    bool abortRequested = false)
  {
    if (generationId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(generationId));
    }

    if (!Enum.IsDefined(stage))
    {
      throw new ArgumentOutOfRangeException(nameof(stage));
    }

    if (cursor < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(cursor));
    }

    if (passVersion < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(passVersion));
    }

    if (activePassId is null && passVersion != 0)
    {
      throw new ArgumentException(
        "An inactive pass cannot carry a pass version.",
        nameof(passVersion));
    }

    if (activePassId is not null && passVersion <= 0)
    {
      throw new ArgumentException(
        "An active pass must carry a positive pass version.",
        nameof(passVersion));
    }

    GenerationId = generationId;
    Stage = stage;
    ActivePassId = activePassId;
    PassVersion = passVersion;
    Cursor = cursor;
    ReadSnapshotRevision = readSnapshotRevision;
    CheckpointRevision = checkpointRevision;
    FailureReason = failureReason;
    PauseRequested = pauseRequested;
    AbortRequested = abortRequested;
  }

  public long GenerationId;

  public WorldGenerationStage Stage;

  public string? ActivePassId;

  public int PassVersion;

  public long Cursor;

  public ulong? ReadSnapshotRevision;

  public ulong? CheckpointRevision;

  public string? FailureReason;

  public bool PauseRequested;

  public bool AbortRequested;

  public bool HasActivePass => ActivePassId is not null;

  public bool HasFailure => !string.IsNullOrWhiteSpace(FailureReason);
}
```

`Cursor` 的存在只说明可恢复的逻辑位置，不证明任何 Tile 命令、Liquid 命令或外部副作用已经提交。`ReadSnapshotRevision` 和 `CheckpointRevision` 不能跨 `GenerationId` 使用。当前映射为 `WorldGenerationPassState`、`GenerationCursorComponent` 和 `WorldGenerationCheckpoint` 的数据部分；现有 `RunPass` 证据为空，故不能声明执行语义已闭合。

### 6.6 `WorldGenerationRandomStateComponent.cs`

目标类型：`WorldGenerationRandomStateComponent`。seed identity 留在 Descriptor/Rules；此组件只保存 generation 期随机流的演进状态。

```csharp
using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct WorldGenerationRandomStateComponent
{
  public WorldGenerationRandomStateComponent(
    long generationId,
    uint state,
    int streamVersion,
    string? passId = null,
    long cursor = 0)
  {
    if (generationId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(generationId));
    }

    if (streamVersion <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(streamVersion));
    }

    if (cursor < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(cursor));
    }

    GenerationId = generationId;
    State = state;
    StreamVersion = streamVersion;
    PassId = passId;
    Cursor = cursor;
  }

  public long GenerationId { get; }

  public uint State { get; }

  public int StreamVersion { get; }

  public string? PassId { get; }

  public long Cursor { get; }
}
```

该代码不提供 `Advance`、`Next` 或任何随机 API；随机演进属于运行时行为，不应藏在 Component 数据类型中。当前映射为 `WorldGenerationRandomSnapshot`、`GenerationRandomState` 和 `WorldSeedComponent` 的数据部分。`StreamVersion`、Pass 关联和 checkpoint 版本体系仍属于 `BD-COMP-06`。

### 6.7 `WorldTerrainStateComponent.cs`

目标类型：`WorldTerrainStateComponent`。使用 nullable terrain facts 区分“尚未计算”与合法的零值；不重复存储 Descriptor 的 surface/rock/spawn/Dungeon canonical 字段。

```csharp
using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed class WorldTerrainStateComponent
{
  public WorldTerrainStateComponent(long generationId)
  {
    if (generationId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(generationId));
    }

    GenerationId = generationId;
  }

  public long GenerationId { get; }

  public ulong TerrainRevision { get; init; }

  public int? UnderworldLayerY { get; init; }

  public int? OceanLevelY { get; init; }

  public int? BeachDistance { get; init; }

  public int? BeachSandDepth { get; init; }

  public double? SurfaceOffset { get; init; }

  public TilePosition? Jungle { get; init; }

  public TilePosition? Snow { get; init; }

  public TilePosition? Desert { get; init; }

  public bool? SurfaceIsDesert { get; init; }

  public bool? SurfaceIsMushrooms { get; init; }

  public bool? SurfaceIsInSpace { get; init; }

  public bool? IsOceanAtSpawn { get; init; }

  public bool? IsBeachAtSpawn { get; init; }

  public bool? IsNoSurface { get; init; }

  public bool? IsRemix { get; init; }

  public bool? IsErrorWorld { get; init; }
}
```

`init` 属性表示每次状态变化应形成新的组件值或受控替换；本草案不定义替换 API。`TerrainRevision == 0` 只表示尚未形成可引用的 terrain revision，不能把它解释为地形为空。当前映射为 `WorldGenerationTerrainState` 和相关 `WorldMetadata`/terrain profile；同名现有类型仍是值对象而非该目标 Component，覆盖为 `partial`。

### 6.8 `BiomeEcologyStateComponent.cs`

目标类型：`BiomeEcologyStateComponent`。它保存 revisioned 世界级生态摘要，不把 `BiomeTileCheck` 或 SceneMetrics 的局部派生结果当作 authority。

```csharp
using System;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed class BiomeEcologyStateComponent
{
  public BiomeEcologyStateComponent(
    long generationId,
    IReadOnlySet<string>? biomeTags = null)
  {
    if (generationId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(generationId));
    }

    GenerationId = generationId;
    BiomeTags = CopyTags(biomeTags);
  }

  public long GenerationId { get; }

  public ulong BiomeRevision { get; init; }

  public IReadOnlySet<string> BiomeTags { get; init; }

  public bool? WorldIsInfected { get; init; }

  public WorldEvilType? WorldEvil { get; init; }

  public ulong ConversionRevision { get; init; }

  public int? TotalEvil { get; init; }

  public int? TotalBlood { get; init; }

  public int? TotalGood { get; init; }

  public int? TotalSolid { get; init; }

  public ulong? TilePresenceRevision { get; init; }

  private static IReadOnlySet<string> CopyTags(IReadOnlySet<string>? source)
  {
    HashSet<string> copy = new(StringComparer.Ordinal);
    if (source is null)
    {
      return copy;
    }

    foreach (string tag in source)
    {
      ArgumentException.ThrowIfNullOrWhiteSpace(tag);
      copy.Add(tag);
    }

    return copy;
  }
}
```

这里的 `CopyTags` 是最小防御性复制草案。若实际 ECS 存储允许组件跨线程共享，最终实现应将集合冻结为不可变表示，而不是只依赖 `IReadOnlySet` 的静态类型。`WorldEvil` 与 Rules 的重复字段不能双写，`BiomeTags` 的 vocabulary 和 authority 仍是 evidence-gap。

当前映射：`WorldInfectionAlignment*`、`TilePresenceScan*`、`BiomeSurfaceSystem` 及 WorldFile 生态字段。现有类型是分散事实和查询结果，尚未证明已经形成此世界级 Component，故标为 `proposed`。

### 6.9 `EcologyScheduleComponent.cs`

目标类型：`EcologyScheduleComponent`。住房扫描 cursor 和优先 Town NPC 不再放入生态调度组件。

```csharp
using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed class EcologyScheduleComponent
{
  public EcologyScheduleComponent(long generationId)
  {
    if (generationId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(generationId));
    }

    GenerationId = generationId;
  }

  public long GenerationId { get; }

  public bool IsInfectionSpreadAllowed { get; init; }

  public int? OvergroundSampleX { get; init; }

  public int? OvergroundSampleY { get; init; }

  public int? UndergroundSampleX { get; init; }

  public int? UndergroundSampleY { get; init; }

  public int? WorldUpdateRate { get; init; }

  public int? EcologyMutationBudget { get; init; }

  public ulong ScheduleRevision { get; init; }

  public bool IsEcologyPropagationEnabled =>
    IsInfectionSpreadAllowed &&
    WorldUpdateRate is > 0;

  public bool HasConfiguredBudget => EcologyMutationBudget is >= 0;
}
```

`HasConfiguredBudget` 的业务含义仍需区分“配置为零”和“尚未配置”；若该区别无法由 `int?` 明确表达，应将其改为专用 budget 值类型，不在调用方用魔数解释。当前映射为 `WorldEcologyScheduleState`，但原状态混有 `TownHousingScanCursor` 和 `PrioritizedTownNpcType`，需在后续迁移中拆出，当前覆盖为 `partial`。

### 6.10 `HousingScanStateComponent.cs`

目标类型：`HousingScanStateComponent`。该类型保留一次扫描过程中的临时事实；nullable 字段表达尚未扫描或尚未找到候选房间。

```csharp
using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed class HousingScanStateComponent
{
  public HousingScanStateComponent(long generationId)
  {
    if (generationId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(generationId));
    }

    GenerationId = generationId;
  }

  public long GenerationId { get; }

  public ulong ScanRevision { get; init; }

  public long Cursor { get; init; }

  public int? PrioritizedTownNpcType { get; init; }

  public int RoomTiles { get; init; }

  public int MaxRoomTiles { get; init; }

  public int MaxRoomSize { get; init; }

  public int? RoomX1 { get; init; }

  public int? RoomX2 { get; init; }

  public int? RoomY1 { get; init; }

  public int? RoomY2 { get; init; }

  public int? BestX { get; init; }

  public int? BestY { get; init; }

  public int? HighScore { get; init; }

  public bool? CanSpawn { get; init; }

  public bool? HouseTile { get; init; }

  public bool? RoomTorch { get; init; }

  public bool? RoomDoor { get; init; }

  public bool? RoomChair { get; init; }

  public bool? RoomTable { get; init; }

  public bool? RoomHasStinkbug { get; init; }

  public bool? RoomHasEchoStinkbug { get; init; }

  public bool CurrentlyTryingAlternateSpot { get; init; }

  public int? SharedRoomX { get; init; }

  public TilePosition? LastFoundHouse { get; init; }

  public string? FailureReason { get; init; }
}
```

代码级约束：

- `Cursor`、计数和尺寸不能为负；这些校验可在最终构造工厂或专责写入边界集中实现；
- 如果同时出现 room bounds，必须满足 `X1 <= X2`、`Y1 <= Y2` 并位于 Descriptor bounds 内；
- `false` 不代表未扫描，故房间组成字段使用 `bool?`；
- `PrioritizedTownNpcType` 是兼容输入，不是 `PersistentEntityId`；
- `HousingComplete` 不能由该组件存在、`CanSpawn == true` 或 `LastFoundHouse` 非空单独推导。

当前映射：`WorldGenerationHousingState`、`TileHousingRuleSnapshot`、`TileHousingRuleQuery`、Version4 `RoomNeeds`/`QuickFindHome` 相关状态。现有覆盖只有部分值和规则，目标标为 `proposed`。

### 6.11 `TownHousingAssignmentComponent.cs`

目标类型：`TownHousingAssignmentComponent`。该类型持有稳定关系结果，不持有房间扫描过程。

```csharp
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed class TownHousingAssignmentComponent
{
  public TownHousingAssignmentComponent(
    long generationId,
    IReadOnlyDictionary<PersistentEntityId, TilePosition>? assignedRooms = null,
    IReadOnlySet<PersistentEntityId>? homelessResidents = null,
    ulong revision = 0,
    ulong? sourceScanRevision = null)
  {
    if (generationId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(generationId));
    }

    ArgumentNullException.ThrowIfNull(assignedRooms);
    ArgumentNullException.ThrowIfNull(homelessResidents);
    Dictionary<PersistentEntityId, TilePosition> roomCopy =
      new(assignedRooms.Count);
    foreach (KeyValuePair<PersistentEntityId, TilePosition> entry in assignedRooms)
    {
      if (!entry.Key.IsValid)
      {
        throw new ArgumentException(
          "Assigned residents must have a persistent identity.",
          nameof(assignedRooms));
      }

      roomCopy.Add(entry.Key, entry.Value);
    }

    HashSet<PersistentEntityId> homelessCopy = new(homelessResidents);
    foreach (PersistentEntityId resident in homelessCopy)
    {
      if (!resident.IsValid)
      {
        throw new ArgumentException(
          "Homeless residents must have a persistent identity.",
          nameof(homelessResidents));
      }

      if (roomCopy.ContainsKey(resident))
      {
        throw new ArgumentException(
          "A resident cannot be assigned and homeless at the same time.",
          nameof(homelessResidents));
      }
    }

    GenerationId = generationId;
    AssignedRooms = new ReadOnlyDictionary<PersistentEntityId, TilePosition>(
      roomCopy);
    HomelessResidents = homelessCopy.ToFrozenSet();
    Revision = revision;
    SourceScanRevision = sourceScanRevision;
  }

  public long GenerationId { get; }

  public IReadOnlyDictionary<PersistentEntityId, TilePosition> AssignedRooms { get; }

  public IReadOnlySet<PersistentEntityId> HomelessResidents { get; }

  public ulong Revision { get; }

  public ulong? SourceScanRevision { get; }

  public int AssignedRoomCount => AssignedRooms.Count;

  public int HomelessResidentCount => HomelessResidents.Count;
}
```

此代码片段使用了 `Enumerable.ToFrozenSet`，因此实际文件需要 `System.Linq` 和目标 SDK 对 frozen collection 的支持；如果项目不采用该 BCL API，应改为仓库批准的不可变集合实现，不得退回直接暴露内部 `HashSet`。

这里的 `PersistentEntityId` 只是候选方案。当前 `TownHousingRegistry` 使用 `TownHousingResidentKey(int NpcType)`，且 WorldSession 与 Dome 尚未确认同一居民 identity owner；因此该 Component 的 Entity 粒度和 key 类型由 `BD-COMP-03` 决定，当前不能直接落地。

当前映射：`TownHousingRegistry`、`TownHousingResidentKey`、NPC home/住房状态和 WorldFile 加载后重新住房检查。稳定 assignment、homeless 关系和持久化 revision 尚未形成已证实的完整 Component，目标标为 `proposed`。

### 6.12 `WorldLiquidHandoffComponent.cs`

目标类型：`WorldLiquidHandoffComponent`。只保存生成/加载完成所需的交接摘要，不复制 `Liquid` solver 的内部工作集合、`numLiquid`、panic 或 quick-settle 状态。

```csharp
using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct WorldLiquidHandoffComponent
{
  public WorldLiquidHandoffComponent(
    long generationId,
    ulong propagationRevision = 0,
    int pendingWorkItemCount = 0,
    long completedVisitCount = 0,
    bool stable = false,
    ulong? snapshotRevision = null,
    string? failureReason = null)
  {
    if (generationId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(generationId));
    }

    if (pendingWorkItemCount < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(pendingWorkItemCount));
    }

    if (completedVisitCount < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(completedVisitCount));
    }

    if (stable && pendingWorkItemCount != 0)
    {
      throw new ArgumentException(
        "A liquid handoff cannot be stable while work items remain pending.",
        nameof(stable));
    }

    GenerationId = generationId;
    PropagationRevision = propagationRevision;
    PendingWorkItemCount = pendingWorkItemCount;
    CompletedVisitCount = completedVisitCount;
    Stable = stable;
    SnapshotRevision = snapshotRevision;
    FailureReason = failureReason;
  }

  public long GenerationId { get; }

  public ulong PropagationRevision { get; }

  public int PendingWorkItemCount { get; }

  public long CompletedVisitCount { get; }

  public bool Stable { get; }

  public ulong? SnapshotRevision { get; }

  public string? FailureReason { get; }
}
```

`Stable` 不能仅由 `PendingWorkItemCount == 0`、单次 `LiquidCheck` 或 server host ready 推导；它必须由后续 owner 定义最低 handoff 条件。当前映射：`LiquidPropagationSession`、`LiquidPropagationSystem`、`LiquidWorkItemComponent`、Version4 `Liquid.cs` 和 WorldFile 加载后 settle。目标覆盖为 `partial`，Component 本身为 `proposed`。

### 6.13 `ReadyTransitionComponent.cs`

目标类型：`ReadyTransitionComponent`。它是 world-level ready facet 汇总状态候选，不是 server host 的 ready 镜像，也不执行发布。

```csharp
using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct ReadyTransitionComponent
{
  public ReadyTransitionComponent(
    long generationId,
    ulong generationRevision,
    bool passesValidated = false,
    bool liquidStable = false,
    bool housingComplete = false,
    bool persistenceCommitted = false,
    ulong? publicationRevision = null,
    string? failureReason = null)
  {
    if (generationId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(generationId));
    }

    GenerationId = generationId;
    GenerationRevision = generationRevision;
    PassesValidated = passesValidated;
    LiquidStable = liquidStable;
    HousingComplete = housingComplete;
    PersistenceCommitted = persistenceCommitted;
    PublicationRevision = publicationRevision;
    FailureReason = failureReason;
  }

  public long GenerationId { get; }

  public ulong GenerationRevision { get; }

  public bool PassesValidated { get; }

  public bool LiquidStable { get; }

  public bool HousingComplete { get; }

  public bool PersistenceCommitted { get; }

  public ulong? PublicationRevision { get; }

  public string? FailureReason { get; }

  public bool IsReadyCandidate =>
    FailureReason is null &&
    PassesValidated &&
    LiquidStable &&
    HousingComplete &&
    PersistenceCommitted &&
    PublicationRevision.HasValue;
}
```

`IsReadyCandidate` 是从 facet 和 publication revision 派生的只读候选值；它也不能被解释为网络广播已完成。`PublicationRevision` 是否与 WorldFile revision 相同、是否允许撤销、以及 server host 如何观察它，仍由 `BD-COMP-05` 和 `BD-COMP-06` 决定。

当前映射：`WorldGenerationLifecycleState.IsReady`、server host `IsReady`、Version4 `WorldGen.Finish`、WorldFile metadata commit 和有限验证状态。当前没有证据证明这些事实已经组成单一完整 barrier，因此目标标为 `proposed`。

## 7. World Entity 组合草案

### 7.1 组合

```text
WorldEntity
├─ WorldDescriptorComponent
├─ WorldGenerationRulesComponent
├─ WorldGenerationLifecycleComponent
├─ WorldGenerationPlanComponent
├─ WorldGenerationPassStateComponent
├─ WorldGenerationRandomStateComponent
├─ WorldTerrainStateComponent
├─ BiomeEcologyStateComponent
├─ EcologyScheduleComponent
├─ HousingScanStateComponent
├─ TownHousingAssignmentComponent
├─ WorldLiquidHandoffComponent
└─ ReadyTransitionComponent
```

上述树只表达 Entity 与 Component 的组合，不定义任何系统顺序。`HousingScanStateComponent` 可以在没有 active scan 时不附着或携带空 revision；`TownHousingAssignmentComponent` 的 World/Town/居民 Entity 粒度仍由 `BD-COMP-03` 决定。

### 7.2 不把这些对象复制进 World Component

以下对象继续保持独立值对象、快照、外部存储或边界对象：

- `WorldGrid`、Tile、Wall、TileEntity、Liquid work item 和区段数据；
- `WorldGenerationServerState`；它是混合状态，不作为第 14 个巨型 Component；
- `WorldGenerationTrace`、`WorldGenerationCheckpoint`、`WorldGenerationRandomSnapshot`；它们是诊断/恢复/快照对象；
- `WorldGenerationPipeline`、`WorldGenerator`、`WorldGen`；它们不是数据 Component；
- `BiomeTileCheck`、`WaterCheck`、`RoomNeeds`、`QuickFindHome`；它们是查询/扫描过程的输入或派生结果；
- Hardmode、OreTiers、后台转换和 `WorldFile.IOLock` 状态；它们属于 WorldProgressionAndTransition 边界；
- server host ready、网络 ready 和 WorldFile metadata；它们可以与 `ReadyTransitionComponent` 建立关系，但不能成为同一字段的第二个 authority。

## 8. 现有 NLTX 类型的替换与并存限制

### 8.1 可作为代码迁移输入的现有类型

| 现有类型 | 草案对应 | 处理方式 |
| --- | --- | --- |
| `WorldSeedComponent` | Descriptor/Rules/Random 的输入 | 不原样复制；seed identity 与 random stream 分开。 |
| `WorldRuleSnapshotComponent` | `WorldGenerationRulesComponent` | 先保留兼容映射，最终避免 `HardMode`/`OreTiers` 回流。 |
| `WorldGenerationStateComponent` | Lifecycle/Pass/Ready 的部分输入 | 不把 `Stage`、`NextSequence`、`IsComplete` 整体复制；逐字段判断 owner。 |
| `WorldGenerationPassState` | `WorldGenerationPassStateComponent` | 不复制 `ReadSnapshot`、`Commands` 和行为方法。 |
| `WorldGenerationRandomSnapshot` / `GenerationRandomState` | `WorldGenerationRandomStateComponent` | 保留状态值，补 generation/pass 关系；不复制随机 API。 |
| `WorldGenerationTerrainState` | `WorldTerrainStateComponent` | 将 sentinel/零值改为 nullable 或明确 revision；不重复 Descriptor geometry。 |
| `WorldGenerationHousingState` | `HousingScanStateComponent` | 房间扫描临时值迁出混合 server state。 |
| `TownHousingRegistry` | `TownHousingAssignmentComponent` | 先解决 key 和 Entity 粒度；不能直接把 int NPC type 当 persistent ID。 |
| `WorldGenerationServerState` | 13 个目标 Component 的多个输入簇 | 禁止整体包装或新增同等巨型 Component。 |
| `LiquidPropagationSession` | `WorldLiquidHandoffComponent` 的输入 | session 是运行对象，不是 handoff Component。 |

### 8.2 不允许直接并存的语义重复

在没有迁移决策之前，以下组合不能作为两个可写 authority 并存：

- `WorldDescriptorComponent` 与 `WorldDescriptorState` 同时写 WorldId/UniqueId/尺寸/坐标；
- `WorldGenerationRulesComponent` 与 `WorldRuleSnapshotComponent` 同时写同一组规则；
- `WorldGenerationLifecycleComponent.IsReady` 与 server host `IsReady` 同时作为 World ready 真值；
- `WorldTerrainStateComponent` 与 `WorldGenerationTerrainState` 同时写同一地形事实；
- `TownHousingAssignmentComponent` 与 `TownHousingRegistry` 同时写同一居民房间关系；
- `WorldLiquidHandoffComponent.Stable` 与 Liquid solver 内部标志同时作为跨边界 stable 真值；
- `ReadyTransitionComponent` 与 `WorldGenerationStateComponent.IsComplete` 同时作为完整生成完成真值。

## 9. 编译前必须解决的决策

本节不是实现计划，而是把代码片段变成生产源码前必须完成的类型和 owner 裁决。

### 9.1 `BD-CODE-01`：目标项目与 WorldSession 引用

- 当前事实：目标 Dome Simulation 项目只直接引用 Entity ECS 项目，未见对 `Terraria.WorldSession` 的项目引用。
- 候选方案：A）Dome 保持本地值类型并通过 Adapter 映射；B）增加项目引用并共享 WorldSession 类型；C）把共享类型移动到独立公共项目。
- 影响：决定 `WorldEvilType`、`WorldPreparationState`、`WorldGenerationFailure`、`TilePosition` 和 Descriptor 的最终 namespace 与重复定义风险。
- 当前状态：`decision-required`；本草案选择 A 作为暂时可读代码形状，不代表允许修改 `.csproj`。

### 9.2 `BD-CODE-02`：WorldBounds canonical owner

- 当前事实：Dome 已有 `WorldBoundsComponent`，WorldSession 还有 `WorldBounds` 值对象，WorldMetadata 也保存宽高。
- 候选方案：A）Dome Component 直接使用现有 `WorldBoundsComponent`；B）共享 `WorldBounds` 值对象；C）Descriptor 自己保存尺寸并由 Adapter 转换。
- 影响：决定 `WorldDescriptorComponent` 的字段形状以及坐标验证是否会产生第二套 section 规则。
- 当前状态：暂选 A；`crossSubsystemOwner: integration-review`。

### 9.3 `BD-CODE-03`：规则与生态的 `WorldEvil` owner

- 当前事实：Rules 需要生成输入，Biome/Ecology 需要运行期生态结果；Version4 `WorldEvil` 语义跨越这两个面。
- 候选方案：A）Rules 是输入 authority，Biome 只保存结果/镜像；B）Biome 是运行期 authority，Rules 只保存 initial value；C）拆成 initial/current 两个明确值对象。
- 影响：决定两个 Component 是否保留 nullable `WorldEvil`，以及加载/生成/感染转换的关系字段。
- 当前状态：`decision-required`。

### 9.4 `BD-CODE-04`：住房居民 key 与 Entity 粒度

- 当前事实：现有 `TownHousingResidentKey` 是 `int NpcType`，草案使用 `PersistentEntityId` 候选。
- 候选方案：A）World Entity 上的 persistent ID map；B）Town/居民 Entity 上的 assignment Component；C）继续使用兼容 NPC type key，并明确非稳定性。
- 影响：决定 `TownHousingAssignmentComponent` 的构造参数、集合字段和是否需要新增公共 identity 类型。
- 当前状态：`decision-required`；不能在草案阶段把现有 int key 静默升级为 persistent identity。

### 9.5 `BD-CODE-05`：Liquid handoff stable 条件

- 当前事实：生成结束检查、WorldFile 加载 settle、`LiquidPropagationSession` 和运行期 Liquid solver 都有部分状态。
- 候选方案：A）World Component 只保存摘要；B）Liquid domain 持有完整 stable authority，World 只读关系；C）生成 handoff 与运行期 solver 分别持有不同 Component。
- 影响：决定 `Stable` 是 world facet、liquid domain 结果还是跨边界 publication 输入。
- 当前状态：`decision-required`；本草案仅给出最小摘要字段。

### 9.6 `BD-CODE-06`：Ready barrier facet owner

- 当前事实：Lifecycle `IsReady`、server host ready、Pass validation、Liquid settle、Housing、WorldFile commit 和 Finish 通知分散存在。
- 候选方案：A）World Entity 持有唯一 ReadyTransition 聚合；B）各子域拥有 facet，World 只保存只读汇总；C）Persistence/Publication domain 持有最终 barrier。
- 影响：决定 `ReadyTransitionComponent` 是否保留、`PublicationRevision` 的写入边界以及 `IsReadyCandidate` 的名称。
- 当前状态：`decision-required`；绝不把 server host `IsReady` 直接当作该字段的 owner。

### 9.7 `BD-CODE-07`：版本与 revision 类型

- 当前事实：当前代码同时存在 generation ID、sequence、cursor、snapshot、checkpoint、section revision 和 WorldFile metadata 版本语义。
- 候选方案：A）统一强类型 revision；B）各 Component 保留局部 `ulong/long`，由关系字段映射；C）按 persistence、generation、publication 分三类强类型。
- 影响：决定构造函数参数、checkpoint 关联和 ready facet 是否可做同代验证。
- 当前状态：`decision-required`；本草案暂用 primitive 只为呈现字段形状，不代表版本体系已经统一。

## 10. 草案级静态检查清单

下列检查仅针对本文档内容，不是生产编译或测试结果：

- [x] 目标文件和 namespace 已明确；
- [x] 13 个目标 Component 均有独立的候选 C# 类型；
- [x] 组件字段未包含 System、Query、Command、Event、Adapter 或 Projection；
- [x] `WorldGenerationServerState` 未被原样包装为新 Component；
- [x] Hardmode/OreTiers 未并入普通生成 Component；
- [x] `WorldLiquidHandoffComponent` 未复制完整 Liquid solver 内部状态；
- [x] `ReadyTransitionComponent` 未把 server host ready 当作完整 World ready；
- [x] Town housing 使用 persistent ID 仅作为候选，并记录当前 int key 冲突；
- [x] nullable 字段用于区分未解析/未计算与合法零值/false；
- [x] 集合字段标明防御性复制或冻结要求；
- [x] 未创建 `.cs`、`.csproj`、测试、迁移或构建输出文件；
- [ ] C# 编译验证：未运行；
- [ ] focused verifier：未运行；
- [ ] 行为等价验证：未运行；
- [ ] owner 决策：未完成。

## 11. 后续落地前的最小审查顺序

这不是本次执行的实现计划，只是代码草案的安全使用顺序：

1. 先裁决目标项目/namespace 和公共值类型 owner；
2. 再裁决 WorldId/UniqueId、WorldEvil、housing key、Liquid stable 和 ready barrier；
3. 以裁决后的字段集合更新本草案，再为每个核心公开类型建立同名独立 `.cs` 文件；
4. 保留当前兼容类型直到唯一 authority、读取者、写入者和生命周期证据闭合；
5. 按仓库的 `BUILD-CONCURRENCY-1` 规则只编译受影响项目，并补充状态转换、集合不变量和边界验证。

本节不授权当前任务执行上述落地动作。

## 12. 最终声明

本文件是实际代码组件草案，不是生产代码。

文中的 C# 片段是 `status: proposed` 的候选类型形状；它们尚未写入 `.cs` 文件、尚未加入项目编译项、尚未注册为 ECS 组件、尚未迁移现有状态，也未声明与 Version4 行为等价。

本文件不定义 System、Query、Command、Event、Adapter、Projection、调度顺序、运行时实现、测试计划或迁移计划。

`verificationStatus: not-run` 保持有效。本次未运行任何 `dotnet` 编译/测试命令，未创建编译输出，未修改上一份 Component-only Design、研究报告或生产源码。
