# Version4 世界生成、Biome 与生态系统组件设计

> 来源：《[Version4 世界生成、Biome 与生态系统公共拆分报告](Version4世界生成Biome生态系统公共拆分报告.md)》。
>
> 范围：本文只定义组件及其关联值类型的字段和派生属性。不定义 System、Query、Command、Adapter、Projection、持久化格式、网络协议或测试。

`WorldStorage` 中的 `TileMap`、`WallMap`、`LiquidMap`、`TileEntity` 和实体槽位不属于本设计的组件；它们是独立的专用存储。`GenerationDefinitionCatalog`、`StructureReservationState` 和 `SceneSnapshot` 也不是可持久化的 WorldSession 组件，原因见文末。

建议命名空间：`Terraria.WorldGeneration.Components`。每个核心公开类型在实际实现时使用独立的同名 PascalCase 文件。

```text
WorldSession
├─ WorldDescriptorState
├─ WorldRulesState
├─ WorldGenerationLifecycleState
└─ WorldEcologyScheduleState

WorldStorage
└─ TownHousingRegistry
```

## 1. `WorldDescriptorState`

世界身份、生成种子、网格尺寸、边界及生成/加载后稳定的地形锚点。

```csharp
public sealed class WorldDescriptorState
{
  public int WorldId;
  public Guid UniqueId;
  public string Name = string.Empty;
  public string SeedText = string.Empty;
  public ulong GeneratorVersion;

  public int SizeX;
  public int SizeY;
  public WorldBounds Bounds;

  public double SurfaceLayer;
  public double RockLayer;
  public int SpawnTileX;
  public int SpawnTileY;
  public int DungeonTileX;
  public int DungeonTileY;

  public int SectionCountX => SizeX / 200;
  public int SectionCountY => SizeY / 150;
  public bool HasSurface => SurfaceLayer > 50.0;
}

public readonly record struct WorldBounds(
  double Left,
  double Top,
  double Right,
  double Bottom);
```

## 2. `WorldRulesState`

创建或加载时确定、在生态和 Biome 资格判断中长期读取的规则集合。

```csharp
public sealed class WorldRulesState
{
  public WorldGameMode GameMode;
  public bool HardMode;
  public WorldSecretSeedFlags SecretSeeds;
  public WorldEvilType WorldEvil;
  public OreTierState OreTiers;

  public bool IsJourneyMode => GameMode == WorldGameMode.Journey;
  public bool IsExpertMode => GameMode == WorldGameMode.Expert;
  public bool IsMasterMode => GameMode == WorldGameMode.Master;
  public bool UsesDualDungeons =>
    (SecretSeeds & WorldSecretSeedFlags.DualDungeons) != 0;
  public bool IsSkyblockWorld =>
    (SecretSeeds & WorldSecretSeedFlags.Skyblock) != 0;
}

public enum WorldGameMode : byte
{
  Classic,
  Expert,
  Master,
  Journey,
}

public enum WorldEvilType : byte
{
  Corruption,
  Crimson,
}

[Flags]
public enum WorldSecretSeedFlags : ulong
{
  None = 0,
  Drunk = 1UL << 0,
  ForTheWorthy = 1UL << 1,
  TenthAnniversary = 1UL << 2,
  DontStarve = 1UL << 3,
  NotTheBees = 1UL << 4,
  Remix = 1UL << 5,
  NoTraps = 1UL << 6,
  Zenith = 1UL << 7,
  Skyblock = 1UL << 8,
  Vampire = 1UL << 9,
  Infected = 1UL << 10,
  TeamBasedSpawns = 1UL << 11,
  DualDungeons = 1UL << 12,
  GetGoodWorld = 1UL << 13,
}

public struct OreTierState
{
  public int Copper;
  public int Iron;
  public int Silver;
  public int Gold;
  public int Cobalt;
  public int Mythril;
  public int Adamantite;
}
```

## 3. `WorldGenerationLifecycleState`

一个世界从创建、加载、生成到可运行或失败的唯一阶段状态。它取代
`generatingWorld`、`isGeneratingOrLoadingWorld` 等可被双写的散布 bool。

```csharp
public sealed class WorldGenerationLifecycleState
{
  public WorldPreparationState Phase;
  public WorldGenerationFailure Failure;
  public ulong GenerationRevision;

  public bool IsLoadingOrGenerating =>
    Phase == WorldPreparationState.Loading ||
    Phase == WorldPreparationState.Generating;
  public bool IsReady => Phase == WorldPreparationState.Ready;
  public bool CanUpdateSimulation => IsReady;
  public bool HasFailed => Phase == WorldPreparationState.Failed;
}

public enum WorldPreparationState : byte
{
  Uninitialized,
  Loading,
  Generating,
  Ready,
  Failed,
  Unloading,
}

public enum WorldGenerationFailure : byte
{
  None,
  LoadFailed,
  GenerationFailed,
  Cancelled,
}
```

## 4. `WorldEcologyScheduleState`

生态传播和增殖的运行期调度状态。它只记录何时、从哪里和以何种预算采样；不保存
Tile、Biome 结论或场景扫描结果。

```csharp
public sealed class WorldEcologyScheduleState
{
  public bool IsInfectionSpreadAllowed = true;

  public int OvergroundSampleX;
  public int OvergroundSampleD;
  public int UndergroundSampleX;
  public int UndergroundSampleD;

  public int WorldUpdateRate;
  public int EcologyMutationBudget;
  public int TownHousingScanCursor;
  public int PrioritizedTownNpcType = -1;

  public bool IsEcologyPropagationEnabled =>
    IsInfectionSpreadAllowed && WorldUpdateRate > 0;
  public bool HasPrioritizedTownNpc => PrioritizedTownNpcType >= 0;
  public bool HasEcologyBudget => EcologyMutationBudget > 0;
}
```

## 5. `TownHousingRegistry`

长期保存的城镇 NPC 与房屋位置关系。键必须是迁移时确认的稳定 NPC 类型或持久身份，
不得使用 `whoAmI`、实体槽位、网络 ID 或本地玩家 ID。

```csharp
public sealed class TownHousingRegistry
{
  public Dictionary<TownHousingResidentKey, TilePosition> AssignedRooms = new();
  public HashSet<TownHousingResidentKey> HomelessResidents = new();
  public ulong Revision;

  public int AssignedRoomCount => AssignedRooms.Count;
  public int HomelessResidentCount => HomelessResidents.Count;
  public bool IsEmpty =>
    AssignedRooms.Count == 0 && HomelessResidents.Count == 0;
}

public readonly record struct TownHousingResidentKey(int NpcType);

public readonly record struct TilePosition(int X, int Y);
```

## 6. `SceneSnapshot`（派生快照，不是组件）

场景扫描结果由 TileMap、规则、扫描中心和实体位置重复计算。该类型仅作为短生命周期
只读快照，不能进入 WorldSession 存档或作为生态权威状态。

```csharp
public sealed class SceneSnapshot
{
  public TilePosition Center;
  public SceneScanBounds ScanBounds;
  public ulong TileRevision;
  public ulong EntityRevision;
  public uint ScanTick;

  public SceneZoneFlags Zones;
  public SceneTileCounts TileCounts;
  public SceneLiquidCounts LiquidCounts;
  public SceneAmenityFlags Amenities;
}

public readonly record struct SceneScanBounds(
  int Left,
  int Top,
  int Right,
  int Bottom);

[Flags]
public enum SceneZoneFlags : ulong
{
  None = 0,
  Sky = 1UL << 0,
  Surface = 1UL << 1,
  DirtLayer = 1UL << 2,
  RockLayer = 1UL << 3,
  Underworld = 1UL << 4,
  Corruption = 1UL << 5,
  Crimson = 1UL << 6,
  Hallow = 1UL << 7,
  Jungle = 1UL << 8,
  Snow = 1UL << 9,
  Desert = 1UL << 10,
  Glowshroom = 1UL << 11,
  Dungeon = 1UL << 12,
  Beach = 1UL << 13,
  UndergroundDesert = 1UL << 14,
  Shimmer = 1UL << 15,
}

public struct SceneTileCounts
{
  public int Evil;
  public int Holy;
  public int Jungle;
  public int Snow;
  public int Mushroom;
  public int Sand;
  public int DesertSand;
  public int OceanSand;
  public int Dungeon;
  public int Meteor;
  public int Graveyard;
  public int Shimmer;
}

public struct SceneLiquidCounts
{
  public int Water;
  public int Lava;
  public int Honey;
  public int Shimmer;
}

[Flags]
public enum SceneAmenityFlags : uint
{
  None = 0,
  Campfire = 1U << 0,
  HeartLantern = 1U << 1,
  StarInBottle = 1U << 2,
  WaterCandle = 1U << 3,
  PeaceCandle = 1U << 4,
  ShadowCandle = 1U << 5,
  Sunflower = 1U << 6,
  GardenGnome = 1U << 7,
  Clock = 1U << 8,
}
```

## 7. 不作为组件的状态

| 类型 | 字段归属 | 为什么不是组件 |
| --- | --- | --- |
| `GenerationDefinitionCatalog` | pass、shape、condition、structure、Biome 的只读定义 | 初始化后不随世界实例变化；应由内容目录拥有 |
| `StructureReservationState` | 当前生成 job 已预留/保护的格子或区域 | 只在单个生成 job 中存在，随 job 结束释放 |
| `WorldGenerator.Controller` | pass、当前进度、暂停、abort、snapshot、锁 | 包含任务控制、调试和 UI 状态，不能持久化 |
| `TileMap` / `WallMap` / `LiquidMap` | 稠密格子、帧和液体量 | 是专用 WorldStorage，不是逐格 ECS component |
| 房间 flood-fill scratch | room bounds、stack、家具标志、临时 tile set | 仅在单次房间资格计算中有效 |

## 8. 组件字段归属汇总

| 组件 | 仅保存 | 不保存 |
| --- | --- | --- |
| `WorldDescriptorState` | 身份、seed、尺寸、边界、地层和世界锚点 | Tile、实体/网络 ID、job handle |
| `WorldRulesState` | 模式、难度、seed flag、世界邪恶、矿脉层级 | 生态游标、场景结果、随机生成器 |
| `WorldGenerationLifecycleState` | phase、稳定失败类别、生成 revision | `Task`、thread ID、token、pass、进度 UI |
| `WorldEcologyScheduleState` | 传播许可、采样游标、频率、预算、住房扫描游标 | Tile 总量、Biome 结果、玩家/NPC 引用 |
| `TownHousingRegistry` | 稳定居民键到房屋位置、无家标记、revision | 房间检查 scratch、NPC slot、网络 ID |
| `SceneSnapshot` | 某次扫描的中心、revision、zone/count/amenity 派生值 | 持久化世界状态、Tile 写权限、生态调度状态 |
