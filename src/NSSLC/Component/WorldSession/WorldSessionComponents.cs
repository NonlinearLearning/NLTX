using System;
using System.Collections.Generic;

namespace Terraria.WorldSession.Components;

public enum WorldPreparationState : byte { Uninitialized, Loading, Generating, Ready, Failed, Unloading }
public static class GameDifficultyLevel { public static readonly float Journey = 0.5f; public static readonly float Classic = 1f; public static readonly float Expert = 2f; public static readonly float Master = 3f; public static readonly float Legendary = 4f; }

public enum MoonPhaseValue : byte { Full, ThreeQuartersAtLeft, HalfAtLeft, QuarterAtLeft, Empty, QuarterAtRight, HalfAtRight, ThreeQuartersAtRight }
public struct BirthdayPartyState { public BirthdayPartyState() { CelebratingNpcIds = new(); } public bool ManualParty; public bool GenuineParty; public int PartyDaysOnCooldown; public List<int> CelebratingNpcIds; public bool IsUp => GenuineParty || ManualParty; }
public struct LanternNightState { public bool ManualLanterns; public bool GenuineLanterns; public bool NextNightIsLanternNight; public int LanternNightsOnCooldown; public bool IsUp => GenuineLanterns || ManualLanterns; }
public struct SandstormState { public bool Happening; public int TimeLeft; public float Severity; public float IntendedSeverity; }
public sealed class WorldTimeWeatherState
{
  public bool DayTime = true; public double Time; public short SunModY; public short MoonModY; public int MoonPhase; public long ClockRevision; public bool Raining; public int RainTime; public float RainStrength; public int CoinRain;
  public float WindTarget; public float WindCurrent; public int WindCounter; public int ExtremeWindCounter; public bool BloodMoon; public bool Eclipse; public bool PumpkinMoon; public bool SnowMoon; public bool SlimeRain; public double SlimeRainTime; public int SlimeRainKillCount; public int SlimeWarningTime;
  public bool FastForwardTimeToDawn; public bool FastForwardTimeToDusk; public int SundialCooldown; public int MoondialCooldown; public int CultistDelay; public BirthdayPartyState BirthdayParty; public LanternNightState LanternNight; public SandstormState Sandstorm;
  public bool IsRainingForever => RainTime >= 5_184_000; public MoonPhaseValue CurrentMoonPhase => (MoonPhaseValue)MoonPhase; public float WindForVisuals => WindCurrent;
}

public struct BossProgressFlags { public bool Boss1, Boss2, Boss3, QueenBee, SlimeKing, PlantBoss, GolemBoss, Fishron, AncientCultist, Moonlord, EmpressOfLight, QueenSlime, Deerclops, HalloweenTree, HalloweenKing, ChristmasIceQueen, ChristmasTree, ChristmasSantank; public bool MechBoss1, MechBoss2, MechBoss3; }
public struct InvasionProgressFlags { public bool Goblins, Frost, Pirates, Martians, Clown; }
public struct SavedNpcProgressFlags { public bool Goblin, Wizard, Mechanic, Angler, Stylist, TaxCollector, Bartender, Golfer; }
public struct NpcSpawnUnlockFlags { public bool Merchant, Demolitionist, PartyGirl, DyeTrader, Truffle, ArmsDealer, Nurse, Princess, SlimeBlue, SlimeGreen, SlimeOld, SlimePurple, SlimeRainbow, SlimeRed, SlimeYellow, SlimeCopper; }
public struct NpcWorldUnlockFlags { public bool BoughtCat, BoughtDog, BoughtBunny, CombatBookUsed, CombatBookVolumeTwoUsed, PeddlersSatchelUsed; }
public struct LunarProgressState { public bool DownedSolarTower, DownedVortexTower, DownedNebulaTower, DownedStardustTower, SolarTowerActive, VortexTowerActive, NebulaTowerActive, StardustTowerActive; public int SolarShieldStrength, VortexShieldStrength, NebulaShieldStrength, StardustShieldStrength; public bool LunarApocalypseIsUp; public int MoonLordCountdown; }
public enum InvasionType : sbyte { None = 0, Goblin = 1, Frost = 2, Pirate = 3, Martian = 4 }
public struct InvasionRuntimeState { public InvasionType Type; public double PositionX; public int Size, SizeStart, Delay, WarningTimer, Progress, ProgressMax, ProgressIcon, ProgressWave; }
public struct Dd2ProgressState { public bool DownedTier1, DownedTier2, DownedTier3, Ongoing, WonThisRun, LostThisRun; public int OngoingDifficulty, LaneSpawnRate, TimeLeftUntilSpawningBegins; }
public struct PendingWorldEventsState { public bool SpawnEye; public int SpawnHardBoss; public bool SpawnMeteor; public int MeteorShowerCount; public bool AfterPartyOfDoom; public bool ForceHalloweenForToday; public bool ForceChristmasForToday; }
public sealed class WorldEventProgressState
{
  public BossProgressFlags Bosses; public InvasionProgressFlags Invasions; public SavedNpcProgressFlags SavedNpcs; public NpcSpawnUnlockFlags UnlockedNpcSpawns; public NpcWorldUnlockFlags NpcWorldUnlocks; public LunarProgressState Lunar; public InvasionRuntimeState Invasion; public Dd2ProgressState Dd2; public PendingWorldEventsState PendingEvents;
  public bool AnyMechBossDowned => Bosses.MechBoss1 || Bosses.MechBoss2 || Bosses.MechBoss3; public bool AnyInvasionActive => Invasion.Type != InvasionType.None || Dd2.Ongoing; public bool LunarApocalypseActive => Lunar.LunarApocalypseIsUp;
}
public sealed class WorldSpawnPressureState
{
  public int NpcSpawnDelay; public int NpcSpawnPeriod; public int CheckForTownNpcSpawns; public int PrioritizedTownNpcType; public float EventSpawnBudget; public float SlimeRainNpcSlots; public bool[] SlimeRainNpcEnabled = Array.Empty<bool>();
  public int ActivePlayerCount { get; } public int ActiveTownNpcCount { get; } public bool HasEligiblePlayer { get; } public bool IsSpawnSuppressed { get; }
}
