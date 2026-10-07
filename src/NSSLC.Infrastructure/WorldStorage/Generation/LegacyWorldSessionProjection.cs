using System;
using System.Collections.Generic;
using System.Linq;
using NSSLC.WorldGeneration;
using NSSLC.WorldGeneration.GameContent;
using NSSLC.WorldGeneration.IO;
using Terraria.NonAuthoritative.Persistence;
using Terraria.WorldSession.Components;

namespace Terraria.NonAuthoritative.WorldStorage;

/// <summary>Projects committed world-session settings onto the legacy generation runtime.</summary>
/// <remarks>
/// The load gate must remain raised while this projection runs. Tile and entity stores have
/// separate projections; this class only owns legacy scalar settings and fixed-size appearance
/// tables represented by <see cref="WorldSessionRestoreState"/>.
/// </remarks>
public static class LegacyWorldSessionProjection
{
  /// <summary>Projects owner clock and event values after each committed simulation tick.</summary>
  public static WorldStorageOperationResult PublishRuntimeTickState(LoadedWorldSession session)
  {
    ArgumentNullException.ThrowIfNull(session);
    if (!session.IsComplete || !session.IsPublished || session.IsPublicationUncertain ||
        !ReferenceEquals(WorldStorageCoordinatorFactory.ActiveGeneratedWorldSession, session))
    {
      return Invalid("Runtime state can only be projected from the active published session.");
    }

    WorldTimeWeatherState time = session.World.TimeWeather;
    PendingWorldEventsState pending = session.World.Progression.PendingEvents;
    InvasionRuntimeState invasion = session.World.Progression.Invasion;

    Main.dayTime = time.DayTime;
    Main.time = time.Time;
    Main.moonPhase = time.MoonPhase;
    Main.bloodMoon = time.BloodMoon;
    Main.eclipse = time.Eclipse;
    Main.pumpkinMoon = time.PumpkinMoon;
    Main.raining = time.Raining;
    Main.rainTime = time.RainTime;
    Main.maxRaining = time.RainStrength;
    Main.cloudAlpha = time.RainStrength;
    Main.coinRain = time.CoinRain;
    Main.windSpeedTarget = time.WindTarget;
    Main.windSpeedCurrent = time.WindCurrent;
    Main.slimeRain = time.SlimeRain;
    Main.slimeRainTime = time.SlimeRainTime;
    Main.slimeRainKillCount = time.SlimeRainKillCount;
    Main.slimeWarningTime = time.SlimeWarningTime;
    Main.fastForwardTimeToDawn = time.FastForwardTimeToDawn;
    Main.fastForwardTimeToDusk = time.FastForwardTimeToDusk;
    Main.sundialCooldown = time.SundialCooldown;
    Main.moondialCooldown = time.MoondialCooldown;
    Main.forceHalloweenForToday = pending.ForceHalloweenForToday;
    Main.forceXMasForToday = pending.ForceChristmasForToday;
    Main.afterPartyOfDoom = pending.AfterPartyOfDoom;
    Main.invasionDelay = invasion.Delay;
    Main.invasionSize = invasion.Size;
    Main.invasionType = (int)invasion.Type;
    Main.invasionX = invasion.PositionX;
    Main.invasionSizeStart = invasion.SizeStart;
    Main.invasionWarn = invasion.WarningTimer;
    WorldGen.spawnEye = pending.SpawnEye;
    WorldGen.spawnHardBoss = pending.SpawnHardBoss;
    WorldGen.spawnMeteor = pending.SpawnMeteor;
    WorldGen.meteorShowerCount = pending.MeteorShowerCount;
    PublishCalendarState(session.World);
    return WorldStorageOperationResult.Success;
  }

  public static WorldStorageOperationResult PublishCoreState(LoadedWorldSession session)
  {
    ArgumentNullException.ThrowIfNull(session);
    return ProjectCoreState(session, allowPublished: false);
  }

  internal static WorldStorageOperationResult ReprojectPublishedCoreState(
    LoadedWorldSession session)
  {
    ArgumentNullException.ThrowIfNull(session);
    return ProjectCoreState(session, allowPublished: true);
  }

  private static WorldStorageOperationResult ProjectCoreState(
    LoadedWorldSession session,
    bool allowPublished)
  {
    bool eligibleSession = allowPublished
      ? session.IsComplete && session.IsPublished && !session.IsPublicationUncertain &&
        !session.Lifecycle.IsGeneratingOrLoadingWorld &&
        ReferenceEquals(WorldStorageCoordinatorFactory.ActiveGeneratedWorldSession, session)
      : session.IsComplete && session.IsPublicationUncertain && !session.IsPublished &&
        session.Lifecycle.IsGeneratingOrLoadingWorld;
    if (!eligibleSession)
    {
      return Invalid("World settings can only be published during a gated session commit.");
    }

    WorldSessionRestoreState world = session.World;
    WorldDescriptorState descriptor = world.Descriptor;
    if (Main.maxTilesX != descriptor.SizeX || Main.maxTilesY != descriptor.SizeY ||
        Main.tile.GetLength(0) != descriptor.SizeX ||
        Main.tile.GetLength(1) != descriptor.SizeY)
    {
      return Invalid("The legacy tile map must be published before world settings.");
    }

    WorldFileData? activeWorldFileData = Main.ActiveWorldFileData;
    if (activeWorldFileData is null)
    {
      return Invalid("The host must provide active world-file metadata before publication.");
    }

    if (!HasLength(Main.treeX, 3) || !HasLength(Main.treeStyle, 4) ||
        !HasLength(Main.caveBackX, 3) || !HasLength(Main.caveBackStyle, 4))
    {
      return Invalid("The legacy appearance arrays do not match the WorldFile layout.");
    }

    if (!HasFixedLengthOrAbsent(world.Appearance.TreeX, 3) ||
        !HasFixedLengthOrAbsent(world.Appearance.TreeStyle, 4) ||
        !HasFixedLengthOrAbsent(world.Appearance.CaveBackX, 3) ||
        !HasFixedLengthOrAbsent(world.Appearance.CaveBackStyle, 4) ||
        !HasFixedLengthOrAbsent(world.Appearance.BackgroundStyles, 8) ||
        !HasFixedLengthOrAbsent(world.Appearance.AdditionalBackgroundStyles, 5))
    {
      return Invalid(
        "Loaded appearance data must be absent or match the WorldFile layout.");
    }

    int[] treeX = CopyFixedOrDefault(world.Appearance.TreeX, 3);
    int[] treeStyle = CopyFixedOrDefault(world.Appearance.TreeStyle, 4);
    int[] caveBackX = CopyFixedOrDefault(world.Appearance.CaveBackX, 3);
    int[] caveBackStyle = CopyFixedOrDefault(world.Appearance.CaveBackStyle, 4);
    byte[] backgroundStyles = CopyFixedOrDefault(world.Appearance.BackgroundStyles, 8);
    byte[] additionalBackgroundStyles =
      CopyFixedOrDefault(world.Appearance.AdditionalBackgroundStyles, 5);
    NSSLC.WorldGeneration.Geometry.Point[] runtimeExtraSpawnPoints =
      new NSSLC.WorldGeneration.Geometry.Point[world.Descriptor.ExtraSpawnPoints.Count];
    for (int index = 0; index < runtimeExtraSpawnPoints.Length; index++)
    {
      var point = world.Descriptor.ExtraSpawnPoints[index];
      runtimeExtraSpawnPoints[index] = new NSSLC.WorldGeneration.Geometry.Point(point.X, point.Y);
    }

    WorldTimeWeatherState timeWeather = world.TimeWeather;
    WorldEventProgressState progression = world.Progression;
    WorldSecretSeedFlags seeds = world.Rules.SecretSeeds;

    activeWorldFileData.ApplyLoadedIdentity(
      descriptor.SeedText,
      descriptor.WorldId,
      descriptor.UniqueId,
      descriptor.WorldGeneratorVersion,
      session.CreationTime,
      session.LastPlayed);
    if (session.Metadata is not null)
    {
      activeWorldFileData.ApplyLoadedMetadata(
        session.Metadata.Revision,
        session.Metadata.IsFavorite);
    }

    Main.worldName = descriptor.Name;
    WorldGen.Manifest = NSSLC.WorldGeneration.WorldBuilding.WorldManifest.Deserialize(
      session.ManifestJson ?? string.Empty);
    Main.GameMode = (int)world.Rules.GameMode;
    Main.expertMode = world.Rules.GameMode is WorldGameMode.Expert or WorldGameMode.Master;
    Main.masterMode = world.Rules.GameMode == WorldGameMode.Master;
    Main.hardMode = world.Rules.HardMode;
    Main.drunkWorld = HasSeed(seeds, WorldSecretSeedFlags.Drunk);
    Main.getGoodWorld = HasSeed(seeds, WorldSecretSeedFlags.ForTheWorthy);
    Main.tenthAnniversaryWorld = HasSeed(seeds, WorldSecretSeedFlags.TenthAnniversary);
    Main.dontStarveWorld = HasSeed(seeds, WorldSecretSeedFlags.DontStarve);
    Main.notTheBeesWorld = HasSeed(seeds, WorldSecretSeedFlags.NotTheBees);
    Main.remixWorld = HasSeed(seeds, WorldSecretSeedFlags.Remix);
    Main.noTrapsWorld = HasSeed(seeds, WorldSecretSeedFlags.NoTraps);
    Main.zenithWorld = HasSeed(seeds, WorldSecretSeedFlags.Zenith);
    Main.skyblockWorld = HasSeed(seeds, WorldSecretSeedFlags.Skyblock);
    Main.vampireSeed = HasSeed(seeds, WorldSecretSeedFlags.Vampire);
    Main.infectedSeed = HasSeed(seeds, WorldSecretSeedFlags.Infected);
    Main.teamBasedSpawnsSeed = HasSeed(seeds, WorldSecretSeedFlags.TeamBasedSpawns);
    Main.dualDungeonsSeed = HasSeed(seeds, WorldSecretSeedFlags.DualDungeons);
    WorldGen.crimson = world.Rules.WorldEvil == WorldEvilType.Crimson;
    WorldGen.WorldGenParam_Evil = WorldGen.crimson ? 1 : 0;

    Array.Copy(treeX, Main.treeX, treeX.Length);
    Array.Copy(treeStyle, Main.treeStyle, treeStyle.Length);
    Array.Copy(caveBackX, Main.caveBackX, caveBackX.Length);
    Array.Copy(caveBackStyle, Main.caveBackStyle, caveBackStyle.Length);
    Main.moonType = world.Appearance.MoonType;
    Main.iceBackStyle = world.Appearance.IceBackStyle;
    Main.jungleBackStyle = world.Appearance.JungleBackStyle;
    Main.hellBackStyle = world.Appearance.HellBackStyle;
    WorldGen.treeBG1 = backgroundStyles[0];
    WorldGen.corruptBG = backgroundStyles[1];
    WorldGen.jungleBG = backgroundStyles[2];
    WorldGen.snowBG = backgroundStyles[3];
    WorldGen.hallowBG = backgroundStyles[4];
    WorldGen.crimsonBG = backgroundStyles[5];
    WorldGen.desertBG = backgroundStyles[6];
    WorldGen.oceanBG = backgroundStyles[7];
    WorldGen.mushroomBG = additionalBackgroundStyles[0];
    WorldGen.underworldBG = additionalBackgroundStyles[1];
    WorldGen.treeBG2 = additionalBackgroundStyles[2];
    WorldGen.treeBG3 = additionalBackgroundStyles[3];
    WorldGen.treeBG4 = additionalBackgroundStyles[4];
    ExtraSpawnPointManager.extraSpawnPoints = runtimeExtraSpawnPoints;

    Main.dayTime = timeWeather.DayTime;
    Main.time = timeWeather.Time;
    Main.moonPhase = timeWeather.MoonPhase;
    Main.bloodMoon = timeWeather.BloodMoon;
    Main.eclipse = timeWeather.Eclipse;
    Main.pumpkinMoon = timeWeather.PumpkinMoon;
    Main.raining = timeWeather.Raining;
    Main.rainTime = timeWeather.RainTime;
    Main.maxRaining = timeWeather.RainStrength;
    Main.cloudAlpha = timeWeather.RainStrength;
    Main.coinRain = timeWeather.CoinRain;
    Main.windSpeedTarget = timeWeather.WindTarget;
    Main.windSpeedCurrent = timeWeather.WindCurrent;
    Main.slimeRain = timeWeather.SlimeRain;
    Main.slimeRainTime = timeWeather.SlimeRainTime;
    Main.slimeWarningTime = timeWeather.SlimeWarningTime;
    Main.fastForwardTimeToDawn = timeWeather.FastForwardTimeToDawn;
    Main.fastForwardTimeToDusk = timeWeather.FastForwardTimeToDusk;
    Main.sundialCooldown = timeWeather.SundialCooldown;
    Main.moondialCooldown = timeWeather.MoondialCooldown;
    Main.cloudBGActive = world.Appearance.CloudBackgroundActive;
    Main.numClouds = world.Appearance.CloudCount;
    Main.forceHalloweenForToday = progression.PendingEvents.ForceHalloweenForToday;
    Main.forceXMasForToday = progression.PendingEvents.ForceChristmasForToday;
    Main.forceHalloweenForever = world.SeasonPolicy.ForceHalloweenForever;
    Main.forceXMasForever = world.SeasonPolicy.ForceChristmasForever;
    Main.afterPartyOfDoom = progression.PendingEvents.AfterPartyOfDoom;
    Main.invasionDelay = progression.Invasion.Delay;
    Main.invasionSize = progression.Invasion.Size;
    Main.invasionType = (int)progression.Invasion.Type;
    Main.invasionX = progression.Invasion.PositionX;
    Main.invasionSizeStart = progression.Invasion.SizeStart;
    Main.invasionWarn = progression.Invasion.WarningTimer;

    WorldGen.spawnEye = progression.PendingEvents.SpawnEye;
    WorldGen.spawnHardBoss = progression.PendingEvents.SpawnHardBoss;
    WorldGen.spawnMeteor = progression.PendingEvents.SpawnMeteor;
    WorldGen.meteorShowerCount = progression.PendingEvents.MeteorShowerCount;
    WorldGen.shadowOrbSmashed = world.Milestones.ShadowOrbSmashed;
    WorldGen.shadowOrbCount = world.Milestones.ShadowOrbCount;
    WorldGen.altarCount = world.Milestones.AltarCount;
    WorldGen.SavedOreTiers.Copper = world.Rules.SavedOreTiers.Copper;
    WorldGen.SavedOreTiers.Iron = world.Rules.SavedOreTiers.Iron;
    WorldGen.SavedOreTiers.Silver = world.Rules.SavedOreTiers.Silver;
    WorldGen.SavedOreTiers.Gold = world.Rules.SavedOreTiers.Gold;
    WorldGen.SavedOreTiers.Cobalt = world.Rules.SavedOreTiers.Cobalt;
    WorldGen.SavedOreTiers.Mythril = world.Rules.SavedOreTiers.Mythril;
    WorldGen.SavedOreTiers.Adamantite = world.Rules.SavedOreTiers.Adamantite;

    PublishNpcProgression(progression, world.Milestones.AnyMechBossDowned);
    Main.anglerWhoFinishedToday = world.History.AnglerWhoFinishedToday.ToList();
    Main.anglerQuestFinished = false;
    Main.anglerQuest = world.History.AnglerQuest;
    Main.BestiaryTracker.Publish(
      world.History.BestiaryKillCounts,
      world.History.SeenNpcIds,
      world.History.ChattedNpcIds);
    PublishCalendarState(world);
    WorldGen.TownManager.Bind(world.TownHousing);
    return WorldStorageOperationResult.Success;
  }

  private static bool HasSeed(WorldSecretSeedFlags seeds, WorldSecretSeedFlags value)
  {
    return (seeds & value) != 0;
  }

  private static void PublishNpcProgression(
    WorldEventProgressState progression,
    bool anyMechBossDowned)
  {
    BossProgressFlags bosses = progression.Bosses;
    InvasionProgressFlags invasions = progression.Invasions;
    SavedNpcProgressFlags savedNpcs = progression.SavedNpcs;
    NpcSpawnUnlockFlags unlockedSpawns = progression.UnlockedNpcSpawns;
    NpcWorldUnlockFlags unlocks = progression.NpcWorldUnlocks;
    LunarProgressState lunar = progression.Lunar;
    Dd2ProgressState dd2 = progression.Dd2;

    NPC.downedBoss1 = bosses.Boss1;
    NPC.downedBoss2 = bosses.Boss2;
    NPC.downedBoss3 = bosses.Boss3;
    NPC.downedQueenBee = bosses.QueenBee;
    NPC.downedSlimeKing = bosses.SlimeKing;
    NPC.downedMechBoss1 = bosses.MechBoss1;
    NPC.downedMechBoss2 = bosses.MechBoss2;
    NPC.downedMechBoss3 = bosses.MechBoss3;
    NPC.downedMechBossAny = anyMechBossDowned;
    NPC.downedPlantBoss = bosses.PlantBoss;
    NPC.downedGolemBoss = bosses.GolemBoss;
    NPC.downedFishron = bosses.Fishron;
    NPC.downedAncientCultist = bosses.AncientCultist;
    NPC.downedMoonlord = bosses.Moonlord;
    NPC.downedEmpressOfLight = bosses.EmpressOfLight;
    NPC.downedQueenSlime = bosses.QueenSlime;
    NPC.downedDeerclops = bosses.Deerclops;
    NPC.downedHalloweenKing = bosses.HalloweenKing;
    NPC.downedHalloweenTree = bosses.HalloweenTree;
    NPC.downedChristmasIceQueen = bosses.ChristmasIceQueen;
    NPC.downedChristmasTree = bosses.ChristmasTree;
    NPC.downedChristmasSantank = bosses.ChristmasSantank;
    NPC.downedGoblins = invasions.Goblins;
    NPC.downedFrost = invasions.Frost;
    NPC.downedPirates = invasions.Pirates;
    NPC.downedMartians = invasions.Martians;
    NPC.downedClown = invasions.Clown;
    NPC.savedGoblin = savedNpcs.Goblin;
    NPC.savedWizard = savedNpcs.Wizard;
    NPC.savedMech = savedNpcs.Mechanic;
    NPC.savedAngler = savedNpcs.Angler;
    NPC.savedStylist = savedNpcs.Stylist;
    NPC.savedTaxCollector = savedNpcs.TaxCollector;
    NPC.savedBartender = savedNpcs.Bartender;
    NPC.savedGolfer = savedNpcs.Golfer;
    NPC.boughtCat = unlocks.BoughtCat;
    NPC.boughtDog = unlocks.BoughtDog;
    NPC.boughtBunny = unlocks.BoughtBunny;
    NPC.combatBookWasUsed = unlocks.CombatBookUsed;
    NPC.combatBookVolumeTwoWasUsed = unlocks.CombatBookVolumeTwoUsed;
    NPC.peddlersSatchelWasUsed = unlocks.PeddlersSatchelUsed;
    NPC.unlockedMerchantSpawn = unlockedSpawns.Merchant;
    NPC.unlockedDemolitionistSpawn = unlockedSpawns.Demolitionist;
    NPC.unlockedPartyGirlSpawn = unlockedSpawns.PartyGirl;
    NPC.unlockedDyeTraderSpawn = unlockedSpawns.DyeTrader;
    NPC.unlockedTruffleSpawn = unlockedSpawns.Truffle;
    NPC.unlockedArmsDealerSpawn = unlockedSpawns.ArmsDealer;
    NPC.unlockedNurseSpawn = unlockedSpawns.Nurse;
    NPC.unlockedPrincessSpawn = unlockedSpawns.Princess;
    NPC.unlockedSlimeBlueSpawn = unlockedSpawns.SlimeBlue;
    NPC.unlockedSlimeGreenSpawn = unlockedSpawns.SlimeGreen;
    NPC.unlockedSlimeOldSpawn = unlockedSpawns.SlimeOld;
    NPC.unlockedSlimePurpleSpawn = unlockedSpawns.SlimePurple;
    NPC.unlockedSlimeRainbowSpawn = unlockedSpawns.SlimeRainbow;
    NPC.unlockedSlimeRedSpawn = unlockedSpawns.SlimeRed;
    NPC.unlockedSlimeYellowSpawn = unlockedSpawns.SlimeYellow;
    NPC.unlockedSlimeCopperSpawn = unlockedSpawns.SlimeCopper;
    NPC.downedTowerSolar = lunar.DownedSolarTower;
    NPC.downedTowerVortex = lunar.DownedVortexTower;
    NPC.downedTowerNebula = lunar.DownedNebulaTower;
    NPC.downedTowerStardust = lunar.DownedStardustTower;
    NPC.TowerActiveSolar = lunar.SolarTowerActive;
    NPC.TowerActiveVortex = lunar.VortexTowerActive;
    NPC.TowerActiveNebula = lunar.NebulaTowerActive;
    NPC.TowerActiveStardust = lunar.StardustTowerActive;
    NPC.LunarApocalypseIsUp = lunar.LunarApocalypseIsUp;
    NPC.MoonLordCountdown = lunar.MoonLordCountdown;
    DD2Event.DownedInvasionT1 = dd2.DownedTier1;
    DD2Event.DownedInvasionT2 = dd2.DownedTier2;
    DD2Event.DownedInvasionT3 = dd2.DownedTier3;
  }

  private static void PublishCalendarState(WorldSessionRestoreState world)
  {
    BirthdayParty.ManualParty = world.TimeWeather.BirthdayParty.ManualParty;
    BirthdayParty.GenuineParty = world.TimeWeather.BirthdayParty.GenuineParty;
    BirthdayParty.PartyDaysOnCooldown = world.TimeWeather.BirthdayParty.PartyDaysOnCooldown;
    BirthdayParty.CelebratingNPCs =
      world.TimeWeather.BirthdayParty.CelebratingNpcIds.ToList();
    LanternNight.ManualLanterns = world.TimeWeather.LanternNight.ManualLanterns;
    LanternNight.GenuineLanterns = world.TimeWeather.LanternNight.GenuineLanterns;
    LanternNight.NextNightIsLanternNight =
      world.TimeWeather.LanternNight.NextNightIsLanternNight;
    LanternNight.LanternNightsOnCooldown =
      world.TimeWeather.LanternNight.LanternNightsOnCooldown;
    Sandstorm.Happening = world.TimeWeather.Sandstorm.Happening;
    Sandstorm.TimeLeft = world.TimeWeather.Sandstorm.TimeLeft;
    Sandstorm.Severity = world.TimeWeather.Sandstorm.Severity;
    Sandstorm.IntendedSeverity = world.TimeWeather.Sandstorm.IntendedSeverity;
  }

  private static bool HasLength<T>(T[]? values, int expectedLength)
  {
    return values is not null && values.Length >= expectedLength;
  }

  private static bool HasFixedLengthOrAbsent<T>(IReadOnlyList<T> values, int expectedLength)
  {
    return values.Count == 0 || values.Count == expectedLength;
  }

  private static T[] CopyFixedOrDefault<T>(IReadOnlyList<T> values, int expectedLength)
  {
    return values.Count == 0
      ? new T[expectedLength]
      : values.ToArray();
  }

  private static WorldStorageOperationResult Invalid(string detail)
  {
    return WorldStorageOperationResult.Failed(
      WorldStorageFailure.Create(WorldStorageFailureKind.InvalidData, detail));
  }
}
