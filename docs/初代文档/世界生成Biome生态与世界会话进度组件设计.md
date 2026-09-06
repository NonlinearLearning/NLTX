# 世界生成、Biome 与生态：世界会话及进度组件设计

本设计从《[世界生成、Biome 与生态：世界会话及进度公共拆分审计报告](世界生成Biome生态与世界会话进度公共拆分审计报告.md)》收敛而来，只定义组件的权威字段和可重复计算的只读属性。

它不包含 System、Query、Command、Adapter、Projection、持久化格式、网络协议或测试设计。`TileMap`、`WallMap`、`LiquidMap`、`TileEntity`、`Chest`、实体槽位和 `TownHousingRegistry` 均是并列的 `WorldStorage` 状态，不属于下列世界会话组件。

所有 `=>` 属性都是派生值：不得持久化、网络同步或作为第二份权威状态写入。除明确标为运行态的字段外，字段均为世界级权威状态。

```text
WorldSession
├─ WorldDescriptorState
├─ WorldRulesState
├─ WorldClockWeatherState
├─ WorldEventProgressState
├─ WorldAlterationProgressState
├─ WorldEcologyScheduleState
└─ WorldGenerationLifecycleState
```

建议命名空间：`Terraria.WorldSession.Components`。实际实现时每个核心公开类型使用独立的同名 PascalCase 文件。

## 1. `WorldDescriptorState`

世界的持久化身份、生成种子、尺寸、边界和地形锚点。它不保存网络连接 ID、客户端 ID、ECS 实体 ID 或实体槽位。

```csharp
public sealed class WorldDescriptorState
{
  // Persistent identity and generation metadata.
  public int WorldId;
  public Guid UniqueId;
  public string Name = string.Empty;
  public string SeedText = string.Empty;
  public ulong GeneratorVersion;

  // Tile dimensions and world-space boundaries.
  public int SizeX;
  public int SizeY;
  public WorldBounds Bounds;

  // Terrain anchors established by load or generation.
  public double SurfaceLayer;
  public double RockLayer;
  public int SpawnTileX;
  public int SpawnTileY;
  public int DungeonTileX;
  public int DungeonTileY;

  // Derived properties.
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

世界创建或加载后长期稳定的规则。世界邪恶、难度、秘密种子和矿脉层级必须作为同一次规则快照读取；暗影球、祭坛与待消费的世界变化不属于本组件。

```csharp
public sealed class WorldRulesState
{
  // Game mode and progression rule.
  public WorldGameMode GameMode;
  public bool HardMode;

  // Creation-time world variants.
  public WorldSecretSeedFlags SecretSeeds;
  public WorldEvilType WorldEvil;

  // Long-lived ore selections made by generation or world load.
  public OreTierState OreTiers;

  // Derived properties.
  public bool IsJourneyMode => GameMode == WorldGameMode.Journey;
  public bool IsExpertMode => GameMode == WorldGameMode.Expert;
  public bool IsMasterMode => GameMode == WorldGameMode.Master;
}

public enum WorldGameMode : byte
{
  Classic,
  Expert,
  Master,
  Journey,
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

public enum WorldEvilType : byte
{
  Corruption,
  Crimson,
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

## 3. `WorldClockWeatherState`

以世界 Tick 为单位变化的时钟、天气和共享日历事件运行态。云、雨、风的渲染 alpha 和相机度量不是本组件字段。

```csharp
public sealed class WorldClockWeatherState
{
  // World clock.
  public bool IsDaytime = true;
  public double TimeOfDay;
  public MoonPhase MoonPhase;

  // Rain and wind. Target and timer fields are authoritative; rendering is not.
  public bool IsRaining;
  public int RainTimeRemaining;
  public float RainIntensity;
  public float WindTarget;
  public float WindCurrent;
  public int WindUpdateCounter;
  public int ExtremeWindCounter;

  // Moon and weather event runtime.
  public bool BloodMoon;
  public bool Eclipse;
  public bool PumpkinMoon;
  public bool SnowMoon;
  public SlimeRainRuntimeState SlimeRain;
  public BirthdayPartyRuntimeState BirthdayParty;
  public LanternNightRuntimeState LanternNight;
  public SandstormRuntimeState Sandstorm;

  // Time manipulation and event cooldowns.
  public bool FastForwardToDawn;
  public bool FastForwardToDusk;
  public int SundialCooldown;
  public int MoondialCooldown;
  public int CultistDelay;

  // Derived properties.
  public bool IsNighttime => !IsDaytime;
  public bool IsRainingForever => RainTimeRemaining >= 5_184_000;
  public bool HasMoonEvent => PumpkinMoon || SnowMoon;
  public bool HasWeatherEvent => SlimeRain.IsActive || Sandstorm.IsActive;
}

public enum MoonPhase : byte
{
  Full,
  ThreeQuartersAtLeft,
  HalfAtLeft,
  QuarterAtLeft,
  Empty,
  QuarterAtRight,
  HalfAtRight,
  ThreeQuartersAtRight,
}

public struct SlimeRainRuntimeState
{
  public bool IsActive;
  public double TimeRemaining;
  public int KillCount;
  public int WarningTimeRemaining;
}

public struct BirthdayPartyRuntimeState
{
  public bool IsManuallyStarted;
  public bool IsNaturallyStarted;
  public int DaysUntilEligible;

  public bool IsActive => IsManuallyStarted || IsNaturallyStarted;
}

public struct LanternNightRuntimeState
{
  public bool IsManuallyStarted;
  public bool IsNaturallyStarted;
  public bool IsScheduledForNextNight;
  public int NightsUntilEligible;

  public bool IsActive => IsManuallyStarted || IsNaturallyStarted;
}

public struct SandstormRuntimeState
{
  public bool IsActive;
  public int TimeRemaining;
  public float Severity;
  public float TargetSeverity;
}
```

## 4. `WorldEventProgressState`

Boss、NPC 解锁、入侵、四柱/月球和 DD2 的世界级完成状态与运行态。任何字段都不得引用某个 NPC 的 `whoAmI`、实体槽位或网络 ID。

```csharp
public sealed class WorldEventProgressState
{
  // Long-lived completion and unlock flags.
  public BossProgressFlags Bosses;
  public InvasionCompletionFlags CompletedInvasions;
  public SavedNpcProgressFlags SavedNpcs;
  public NpcSpawnUnlockFlags NpcSpawnUnlocks;
  public NpcWorldUnlockFlags NpcWorldUnlocks;

  // Long-lived and current celestial-event state.
  public LunarProgressState Lunar;
  public Dd2ProgressState Dd2;

  // Current invasion transaction state.
  public InvasionRuntimeState Invasion;

  // Derived properties.
  public bool AnyMechanicalBossDefeated =>
    Bosses.DestroyerDefeated || Bosses.TwinsDefeated || Bosses.SkeletronPrimeDefeated;
  public bool IsInvasionActive => Invasion.Type != InvasionType.None;
  public bool IsLunarApocalypseActive => Lunar.IsLunarApocalypseActive;
  public bool IsDd2Active => Dd2.IsActive;
}

public struct BossProgressFlags
{
  public bool EyeOfCthulhuDefeated;
  public bool EaterOfWorldsOrBrainOfCthulhuDefeated;
  public bool SkeletronDefeated;
  public bool QueenBeeDefeated;
  public bool KingSlimeDefeated;
  public bool WallOfFleshDefeated;
  public bool DestroyerDefeated;
  public bool TwinsDefeated;
  public bool SkeletronPrimeDefeated;
  public bool PlanteraDefeated;
  public bool GolemDefeated;
  public bool DukeFishronDefeated;
  public bool LunaticCultistDefeated;
  public bool MoonLordDefeated;
  public bool EmpressOfLightDefeated;
  public bool QueenSlimeDefeated;
  public bool DeerclopsDefeated;
  public bool PumpkingDefeated;
  public bool MourningWoodDefeated;
  public bool IceQueenDefeated;
  public bool SantaNk1Defeated;
  public bool EverscreamDefeated;
}

public struct InvasionCompletionFlags
{
  public bool GoblinArmyDefeated;
  public bool FrostLegionDefeated;
  public bool PirateInvasionDefeated;
  public bool MartianMadnessDefeated;
  public bool ClownDefeated;
}

public struct SavedNpcProgressFlags
{
  public bool GoblinTinkererSaved;
  public bool WizardSaved;
  public bool MechanicSaved;
  public bool AnglerSaved;
  public bool StylistSaved;
  public bool TaxCollectorSaved;
  public bool TavernkeepSaved;
  public bool GolferSaved;
}

public struct NpcSpawnUnlockFlags
{
  public bool MerchantUnlocked;
  public bool DemolitionistUnlocked;
  public bool PartyGirlUnlocked;
  public bool DyeTraderUnlocked;
  public bool TruffleUnlocked;
  public bool ArmsDealerUnlocked;
  public bool NurseUnlocked;
  public bool PrincessUnlocked;
  public bool BlueSlimeUnlocked;
  public bool GreenSlimeUnlocked;
  public bool OldShakingChestUnlocked;
  public bool PurpleSlimeUnlocked;
  public bool RainbowSlimeUnlocked;
  public bool RedSlimeUnlocked;
  public bool YellowSlimeUnlocked;
  public bool CopperSlimeUnlocked;
}

public struct NpcWorldUnlockFlags
{
  public bool CatUnlocked;
  public bool DogUnlocked;
  public bool BunnyUnlocked;
  public bool CombatBookUsed;
  public bool CombatBookVolumeTwoUsed;
  public bool PeddlersSatchelUsed;
}

public struct LunarProgressState
{
  // Permanent completion.
  public bool SolarTowerDefeated;
  public bool VortexTowerDefeated;
  public bool NebulaTowerDefeated;
  public bool StardustTowerDefeated;

  // Current event runtime.
  public bool IsSolarTowerActive;
  public bool IsVortexTowerActive;
  public bool IsNebulaTowerActive;
  public bool IsStardustTowerActive;
  public int SolarShieldStrength;
  public int VortexShieldStrength;
  public int NebulaShieldStrength;
  public int StardustShieldStrength;
  public bool IsLunarApocalypseActive;
  public int MoonLordCountdown;
}

public enum InvasionType : sbyte
{
  None = 0,
  GoblinArmy = 1,
  FrostLegion = 2,
  PirateInvasion = 3,
  MartianMadness = 4,
}

public struct InvasionRuntimeState
{
  public InvasionType Type;
  public double PositionX;
  public int RemainingSize;
  public int InitialSize;
  public int StartDelay;
  public int WarningTimeRemaining;

  // All progress fields are committed together.
  public int Progress;
  public int ProgressMaximum;
  public int ProgressIcon;
  public int ProgressWave;

  public bool IsActive => Type != InvasionType.None;
  public bool IsComplete => IsActive && RemainingSize <= 0;
}

public struct Dd2ProgressState
{
  // World-long completion.
  public bool TierOneDefeated;
  public bool TierTwoDefeated;
  public bool TierThreeDefeated;

  // Current run.
  public bool IsActive;
  public bool WonCurrentRun;
  public bool LostCurrentRun;
  public int Difficulty;
  public int LaneSpawnRate;
  public int TimeUntilSpawningBegins;
}
```

## 5. `WorldAlterationProgressState`

可长期存档的世界改造进度，以及必须被后续玩法消费的一次性世界变化请求。它不缓存 Tile 区域副本；实际改图位置由 Tile 命令提供。

```csharp
public sealed class WorldAlterationProgressState
{
  // Persistent world-alteration progress.
  public bool HasSmashedShadowOrb;
  public int ShadowOrbCount;
  public int DestroyedAltarCount;

  // Pending one-shot alterations or event requests.
  public PendingWorldAlterationState Pending;

  // Derived properties.
  public bool HasDestroyedAnyAltar => DestroyedAltarCount > 0;
  public bool HasPendingAlteration =>
    Pending.ShouldSpawnMeteor || Pending.ShouldSpawnEye || Pending.HardBossToSpawn != 0;
}

public struct PendingWorldAlterationState
{
  public bool ShouldSpawnMeteor;
  public bool ShouldSpawnEye;
  public int HardBossToSpawn;
  public int MeteorShowerCount;
  public bool AfterPartyOfDoom;
  public bool ForceHalloweenToday;
  public bool ForceChristmasToday;
}
```

## 6. `WorldEcologyScheduleState`

生态传播、地图采样和世界级刷怪节奏的运行态。它记录何时以及从哪里开始调度，不能保存 `SceneMetrics`、Tile 总量、Biome 扫描结果或玩家/NPC 实体引用。

```csharp
public sealed class WorldEcologyScheduleState
{
  // Ecology gate. The source of the gate may be a rules or creative-power input,
  // but this component owns the current scheduling permission.
  public bool IsInfectionSpreadAllowed = true;

  // Incremental ecology sampling cursors.
  public int EcologySampleX;
  public int EcologySampleD;

  // World-level NPC and town-NPC spawn scheduling.
  public int NpcSpawnDelay;
  public int NpcSpawnPeriod;
  public int TownNpcSpawnCheckCounter;
  public int PrioritizedTownNpcType;

  // Derived properties.
  public bool IsEcologyPropagationEnabled => IsInfectionSpreadAllowed;
  public bool HasPrioritizedTownNpc => PrioritizedTownNpcType >= 0;
}
```

## 7. `WorldGenerationLifecycleState`

加载、生成、可运行、失败和卸载的单一阶段真相。`generatingWorld`、`isGeneratingOrLoadingWorld` 等重复 bool 不再单独保存；后台 Task、线程 ID、pass 控制器和 `GenerationProgress` 也不是该组件字段。

```csharp
public sealed class WorldGenerationLifecycleState
{
  // The sole lifecycle authority for a world session.
  public WorldPreparationState Phase;

  // A stable failure classification, not an exception object or Task handle.
  public WorldGenerationFailure Failure;

  // Derived properties.
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

## 8. 字段归属汇总

| 组件 | 只保存 | 明确不保存 |
| --- | --- | --- |
| `WorldDescriptorState` | 世界身份、尺寸、边界、地层、出生和地牢锚点 | 生命周期 bool、网络/实体 ID、Tile 存储 |
| `WorldRulesState` | 模式、Hardmode、秘密种子、世界邪恶、矿脉层级 | 天气、入侵波次、暗影球、祭坛、实体状态 |
| `WorldClockWeatherState` | 时钟、月相、雨风、月/天气事件及冷却 | 渲染 alpha、云、音乐、相机度量 |
| `WorldEventProgressState` | Boss、NPC 解锁、入侵、月球、DD2 的完成与运行态 | 实体槽位、Tile 变更请求、网络 DTO |
| `WorldAlterationProgressState` | 暗影球、祭坛和待消费的世界改造请求 | Tile 副本、Biome/场景扫描缓存 |
| `WorldEcologyScheduleState` | 传播许可、采样游标、刷怪与城镇 NPC 调度 | Tile 总量、`SceneMetrics`、玩家/NPC 引用 |
| `WorldGenerationLifecycleState` | 阶段与稳定失败分类 | `Task`、线程 ID、pass 列表、生成进度 UI |
