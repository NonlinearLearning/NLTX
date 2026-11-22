# Version4 世界会话与进度系统组件设计

> 依据：[Version4 世界会话与进度系统代码盘点及组件拆分报告](Version4世界会话与进度系统代码盘点及组件拆分报告.md)。
>
> 本文只定义世界会话组件、嵌入值对象的字段和派生属性；不定义 System、Query、Command、Adapter、Projection、持久化格式或测试。

## 1. 字段约定

- 未标注的字段是权威状态。其来源、生命周期与持久化/网络证据见盘点报告第 3 节。
- `=>` 属性是由本组件权威字段重复计算的派生值，不持久化、不作为网络写入源。
- 标为“暂态”的字段随世界加载、清理或当前事件运行而失效；它仍是服务器权威状态，但不是长期进度。
- 不在本文定义的内容包括 Tile、液体、实体槽位、区段位图、玩家/NPC/投射物实例、表现缓存、网络 DTO 和存档 DTO。

| 组件 | 状态边界 | Version4 主要来源 |
| --- | --- | --- |
| `WorldSessionRoot` | 单一世界会话的组合入口；只持有状态组件引用 | 拆分报告的会话根建议 |
| `SessionPhaseState` | 世界装载、生成、就绪与卸载阶段 | `Main._worldPreparationState`、`WorldGen.isGeneratingOrLoadingWorld` |
| `WorldDescriptorState` | 世界身份、尺寸、边界、地层与锚点 | `WorldFileData`、`Main.worldName`、`Main.worldSurface` |
| `WorldRulesState` | 难度、特殊种子、邪恶类型、感染、矿层与祭坛规则 | `Main`、`WorldGen` |
| `WorldTimeWeatherState` | 世界时钟、雨、风目标/计数与同生命周期天气事件 | `Main`、`BirthdayParty`、`LanternNight`、`Sandstorm` |
| `WorldEventProgressState` | Boss、NPC 解锁、塔/Lunar、入侵、DD2 与待处理世界事件 | `NPC`、`Main`、`DD2Event`、`WorldGen` |
| `WorldSpawnPressureState` | 刷怪节流、城镇刷新优先级与 Slime Rain 预算 | `WorldGen`、`Main` |

## 2. `WorldSessionRoot`

```csharp
public sealed class WorldSessionRoot
{
  // 组合引用不是子组件字段的第二份状态；世界会话只保留一个根。
  public SessionPhaseState Phase = new();
  public WorldDescriptorState Descriptor = new();
  public WorldRulesState Rules = new();
  public WorldTimeWeatherState TimeWeather = new();
  public WorldEventProgressState EventProgress = new();
  public WorldSpawnPressureState SpawnPressure = new();

  // 派生：根不镜像 Phase 的生命周期值。
  public bool IsReady => Phase.IsReady;
}
```

## 3. `SessionPhaseState`

```csharp
public sealed class SessionPhaseState
{
  // 权威：世界尚未初始化、加载、生成、可 Tick、失败或卸载。
  public WorldSessionPhase Phase;

  // 派生：不单独存储 Main._worldPreparationState 的镜像布尔值。
  public bool IsReady => Phase == WorldSessionPhase.Ready;
  public bool IsLoadingOrGenerating =>
      Phase == WorldSessionPhase.Loading ||
      Phase == WorldSessionPhase.Generating;
  public bool IsTerminal =>
      Phase == WorldSessionPhase.Failed ||
      Phase == WorldSessionPhase.Unloading;
}

public enum WorldSessionPhase : byte
{
  Uninitialized,
  Loading,
  Generating,
  Ready,
  Failed,
  Unloading
}
```

## 4. `WorldDescriptorState`

```csharp
public sealed class WorldDescriptorState
{
  // 权威、持久化：来自 WorldFileData。
  public int WorldId;
  public Guid UniqueId;
  public string Name = string.Empty;
  public string SeedText = string.Empty;
  public ulong WorldGeneratorVersion;

  // 权威、持久化：世界尺寸和世界坐标边界。
  public int SizeX;
  public int SizeY;
  public float LeftWorld;
  public float RightWorld;
  public float TopWorld;
  public float BottomWorld;

  // 权威、持久化：生成完成后写入的世界地层和锚点。
  public double SurfaceLayer;
  public double RockLayer;
  public int SpawnTileX;
  public int SpawnTileY;
  public int DungeonTileX;
  public int DungeonTileY;

  // 派生：区段内容和 Loaded 位图属于 WorldStorage，不属于本组件。
  public int SectionCountX => SizeX / 200;
  public int SectionCountY => SizeY / 150;
  public bool HasSurfaceLayer => SurfaceLayer > 50.0;
  public WorldBounds Bounds =>
      new(LeftWorld, TopWorld, RightWorld, BottomWorld);
}

public readonly record struct WorldBounds(
    float Left,
    float Top,
    float Right,
    float Bottom);
```

## 5. `WorldRulesState`

```csharp
public sealed class WorldRulesState
{
  // 权威、持久化：GameMode 来自 WorldFileData；覆盖值对应 Main 的私有难度覆盖。
  public int GameMode;
  public float? DifficultyOverride;
  public bool HardMode;

  // 权威、持久化：世界生成时固定的特殊种子组合。
  public WorldSecretSeedFlags SecretSeeds;

  // 权威、持久化：世界邪恶和生态扩散许可。
  public WorldEvilType WorldEvil;
  public bool InfectionSpreadAllowed = true;
  public SeasonalEventOverrideState SeasonalOverrides;

  // 权威、持久化：矿层与祭坛/暗影球进度。
  public OreTierState OreTiers;
  public bool ShadowOrbSmashed;
  public int ShadowOrbCount;
  public int AltarCount;

  // 派生：不复制成独立的难度真值。
  public float EffectiveDifficulty => DifficultyOverride ?? GameMode switch
  {
    1 => GameDifficultyLevel.Expert,
    2 => GameDifficultyLevel.Master,
    _ => GameDifficultyLevel.Classic
  };

  public bool IsJourneyMode => GameMode == 3;
  public bool IsExpertMode => EffectiveDifficulty >= GameDifficultyLevel.Expert;
  public bool IsMasterMode => EffectiveDifficulty >= GameDifficultyLevel.Master;
}

[Flags]
public enum WorldSecretSeedFlags : ulong
{
  None = 0,
  Drunk = 1UL << 0,
  GetGoodWorld = 1UL << 1,
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
  DualDungeons = 1UL << 12
}

public enum WorldEvilType : byte
{
  Corruption,
  Crimson
}

public struct SeasonalEventOverrideState
{
  // 对应 Main.forceHalloweenForever / forceXMasForever；由 WorldFile 持久化。
  public bool ForceHalloweenForever;
  public bool ForceChristmasForever;
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

public static class GameDifficultyLevel
{
  public const float Classic = 1f;
  public const float Expert = 2f;
  public const float Master = 3f;
}
```

## 6. `WorldTimeWeatherState`

```csharp
public sealed class WorldTimeWeatherState
{
  // 权威、每 tick：世界时钟。
  public bool DayTime = true;
  public double Time;
  public int MoonPhase;
  public int DayRate;

  // 权威、每 tick：降雨状态和强度。
  public bool Raining;
  public int RainTime;
  public float RainStrength;
  public int CoinRain;

  // 权威、每 tick：风的目标和调节计数。
  public float WindTarget;
  public int WeatherCounter;
  public int WindCounter;
  public int ExtremeWindCounter;

  // 权威：与昼夜共同开始/停止的世界事件。
  public bool BloodMoon;
  public bool Eclipse;
  public bool PumpkinMoon;
  public bool SnowMoon;
  public bool SlimeRain;
  public double SlimeRainTime;
  public int SlimeRainKillCount;
  public int SlimeWarningTime;
  public int SlimeWarningDelay;

  // 权威、暂态：时间快进与冷却。
  public bool FastForwardTimeToDawn;
  public bool FastForwardTimeToDusk;
  public int SundialCooldown;
  public int MoondialCooldown;
  public int CultistDelay;

  // 权威：与时钟/天气同步更新的子状态。
  public BirthdayPartyState BirthdayParty;
  public LanternNightState LanternNight;
  public SandstormState Sandstorm;

  // 派生：当前风速由 WindTarget、计数和物理规则重建；不持久化。
  public MoonPhaseValue CurrentMoonPhase => (MoonPhaseValue)MoonPhase;
  public bool IsRainingForever => RainTime >= 5_184_000;
}

public enum MoonPhaseValue : byte
{
  Full,
  ThreeQuartersAtLeft,
  HalfAtLeft,
  QuarterAtLeft,
  Empty,
  QuarterAtRight,
  HalfAtRight,
  ThreeQuartersAtRight
}

public struct BirthdayPartyState
{
  public bool ManualParty;
  public bool GenuineParty;
  public int PartyDaysOnCooldown;

  // 暂态：派对期可掉落蛋糕的世界事件旗标。
  public bool FreeCakeAvailable;
  public bool FreeCakeAvailable;

  // 暂态：运行期 NPC 槽位，不是持久化或网络实体 ID。
  public List<int> CelebratingNpcSlots;

  public bool IsUp => GenuineParty || ManualParty;
}

public struct LanternNightState
{
  public bool ManualLanterns;
  public bool GenuineLanterns;
  public bool NextNightIsLanternNight;
  public int LanternNightsOnCooldown;

  public bool IsUp => GenuineLanterns || ManualLanterns;
}

public struct SandstormState
{
  public bool Happening;
  public int TimeLeft;
  public float Severity;
  public float IntendedSeverity;
}
```

`windSpeedCurrent`、`cloudAlpha`、`oldMaxRaining`、云对象、音乐和帧渲染标记都不是本组件字段：它们是重建缓存或表现状态。

## 7. `WorldEventProgressState`

```csharp
public sealed class WorldEventProgressState
{
  // 权威、持久化：长期击败/完成旗标。
  public BossProgressFlags Bosses;
  public InvasionProgressFlags CompletedInvasions;
  public SavedNpcProgressFlags SavedNpcs;
  public NpcSpawnUnlockFlags UnlockedNpcSpawns;
  public NpcWorldUnlockFlags WorldUnlocks;
  public LunarProgressState Lunar;
  public Dd2ProgressState Dd2;
  public MoonEventRuntimeState MoonEvent;

  // 权威、暂态：当前标准入侵。
  public InvasionRuntimeState Invasion;

  // 权威、暂态或跨昼夜待消费事件。
  public PendingWorldEventsState PendingEvents;

  // 派生：不重复持久化聚合结论。
  public bool AnyMechBossDowned =>
      Bosses.MechBoss1 || Bosses.MechBoss2 || Bosses.MechBoss3;
  public bool IsStandardInvasionActive => Invasion.Type != InvasionType.None;
  public bool IsAnyInvasionActive => IsStandardInvasionActive || Dd2.Ongoing;
  public bool LunarApocalypseActive => Lunar.LunarApocalypseIsUp;
}

public struct BossProgressFlags
{
  public bool Boss1;
  public bool Boss2;
  public bool Boss3;
  public bool QueenBee;
  public bool SlimeKing;
  public bool PlantBoss;
  public bool GolemBoss;
  public bool Fishron;
  public bool AncientCultist;
  public bool Moonlord;
  public bool EmpressOfLight;
  public bool QueenSlime;
  public bool Deerclops;

  public bool HalloweenTree;
  public bool HalloweenKing;
  public bool ChristmasIceQueen;
  public bool ChristmasTree;
  public bool ChristmasSantank;

  // Main/NPC 的显式三机械 Boss 旗标；AnyMechBossDowned 是外层派生属性。
  public bool MechBoss1;
  public bool MechBoss2;
  public bool MechBoss3;
}

public struct InvasionProgressFlags
{
  public bool Goblins;
  public bool Frost;
  public bool Pirates;
  public bool Martians;
  public bool Clown;
}

public struct SavedNpcProgressFlags
{
  public bool TaxCollector;
  public bool Goblin;
  public bool Wizard;
  public bool Mechanic;
  public bool Angler;
  public bool Stylist;
  public bool Bartender;
  public bool Golfer;
}

public struct NpcSpawnUnlockFlags
{
  public bool Merchant;
  public bool Demolitionist;
  public bool PartyGirl;
  public bool DyeTrader;
  public bool Truffle;
  public bool ArmsDealer;
  public bool Nurse;
  public bool Princess;

  public bool SlimeBlue;
  public bool SlimeGreen;
  public bool SlimeOld;
  public bool SlimePurple;
  public bool SlimeRainbow;
  public bool SlimeRed;
  public bool SlimeYellow;
  public bool SlimeCopper;
}

public struct NpcWorldUnlockFlags
{
  public bool BoughtCat;
  public bool BoughtDog;
  public bool BoughtBunny;
  public bool CombatBookWasUsed;
  public bool CombatBookVolumeTwoWasUsed;
  public bool PeddlersSatchelWasUsed;
}

public struct LunarProgressState
{
  // 持久化：四柱已击败。
  public bool DownedSolarTower;
  public bool DownedVortexTower;
  public bool DownedNebulaTower;
  public bool DownedStardustTower;

  // 暂态：当前 Lunar 事件的塔、护盾和 Moon Lord 倒计时。
  public bool SolarTowerActive;
  public bool VortexTowerActive;
  public bool NebulaTowerActive;
  public bool StardustTowerActive;
  public int SolarShieldStrength;
  public int VortexShieldStrength;
  public int NebulaShieldStrength;
  public int StardustShieldStrength;
  public bool LunarApocalypseIsUp;
  public int MoonLordCountdown;
  public int MaxMoonLordCountdown;
}

public struct InvasionRuntimeState
{
  public InvasionType Type;
  public double PositionX;
  public int Size;
  public int SizeStart;
  public int Delay;
  public int WarningTimer;
  public int Progress;
  public int ProgressMax;
  public int ProgressIcon;
  public int ProgressWave;
}

public enum InvasionType : sbyte
{
  None = 0,
  Goblin = 1,
  Frost = 2,
  Pirate = 3,
  Martian = 4
}

public struct Dd2ProgressState
{
  // 持久化：Old One's Army 的完成层级。
  public bool DownedTier1;
  public bool DownedTier2;
  public bool DownedTier3;

  // 暂态：当前事件运行状态。
  public bool Ongoing;
  public bool WonThisRun;
  public bool LostThisRun;
  public int OngoingDifficulty;
  public int LaneSpawnRate;
  public int TimeLeftBetweenWaves;

  public bool EnemySpawningIsOnHold => TimeLeftBetweenWaves != 0;
}

public struct MoonEventRuntimeState
{
  // 暂态：南瓜月或霜月的当前波次积分和累计积分。
  public float TotalInvasionPoints;
  public float WaveKills;
  public int WaveNumber;
}

public struct PendingWorldEventsState
{
  public bool SpawnEye;
  public int SpawnHardBoss;
  public bool SpawnMeteor;
  public int MeteorShowerCount;
  public bool AfterPartyOfDoom;
  public bool ForceHalloweenForToday;
  public bool ForceChristmasForToday;
}
```

`invasionProgressDisplayLeft` 和 `invasionProgressAlpha` 是 UI 表现值，不在 `InvasionRuntimeState` 中；单个 NPC 的生命、AI、目标、伤害归因和实体槽位也不在本组件中。

## 8. `WorldSpawnPressureState`

```csharp
public sealed class WorldSpawnPressureState
{
  // 权威、每 tick：普通 NPC 与城镇 NPC 调度。
  public int NpcSpawnDelay;
  public int NpcSpawnPeriod;
  public int PrioritizedTownNpcType;
  public int TownNpcSpawnCheckCounter;

  // 权威、事件期：通用事件刷怪预算。
  public float EventSpawnBudget;

  // 权威、事件期：Slime Rain 刷怪预算与已启用定义集合。
  public float SlimeRainNpcSlots;
  public bool[] SlimeRainNpcEnabled = Array.Empty<bool>();

  // 派生：必须从当前玩家、NPC、区域和权限查询计算，不能缓存为世界真值。
  public int ActivePlayerCount { get; }
  public int ActiveTownNpcCount { get; }
  public bool HasEligiblePlayer { get; }
  public bool IsSpawnSuppressed { get; }
}
```

`NPC.totalInvasionPoints`、`NPC.waveKills` 和 `NPC.waveNumber` 是南瓜月/霜月的当前运行数据，进入 `WorldEventProgressState.MoonEvent`；实体人口、区位、生物群落、碰撞和住房结果一律保持为 Query 输入。

## 9. 排除项

| 现有数据 | 归属 | 不作为本文组件字段的理由 |
| --- | --- | --- |
| `Main.tile`、液体、箱子、TileEntity、实体数组、`projectileIdentity[,]` | `WorldStorage` | 容量、结构变更、复制和生命周期都不同于会话状态。 |
| `Projectile.owner`、`timeLeft`、`penetrate`、命中免疫、鱼漂/传送门状态 | 投射物与专用实体行为 | 属于短生命周期实体，而不是世界长期进度。 |
| `cloudAlpha`、`windSpeedCurrent`、`invasionProgressAlpha`、镜头和音频值 | 表现/重建缓存 | 不能作为存档或网络的权威来源。 |
| 玩家数、城镇 NPC 数、区域资格、住房和 Biome 结论 | 即时 Query | 随实体、区段和权限变化，缓存会产生失效风险。 |
| 存档 DTO、网络包、客户端 UI、世界文件临时读写字段 | Adapter / Projection | 外部边界不得成为会话状态的第二权威副本。 |
