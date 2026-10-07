using System.Collections.ObjectModel;

namespace Terraria.WorldSession.Components;

public static class WorldSessionRestoreSystem {
  public static void ApplyEnvironment(WorldSessionRestoreState owner,
      byte moonType,
      IReadOnlyList<int> treeX,
      IReadOnlyList<int> treeStyle,
      IReadOnlyList<int> caveBackX,
      IReadOnlyList<int> caveBackStyle,
      int iceBackStyle,
      int jungleBackStyle,
      int hellBackStyle,
      int spawnTileX,
      int spawnTileY,
      double worldSurface,
      double rockLayer,
      double time,
      bool dayTime,
      int moonPhase,
      bool bloodMoon,
      bool eclipse,
      int dungeonX,
      int dungeonY,
      bool crimson) {
    ArgumentNullException.ThrowIfNull(owner);
    owner.Appearance.MoonType = moonType;
    owner.Appearance.TreeX = Array.AsReadOnly(treeX.ToArray());
    owner.Appearance.TreeStyle = Array.AsReadOnly(treeStyle.ToArray());
    owner.Appearance.CaveBackX = Array.AsReadOnly(caveBackX.ToArray());
    owner.Appearance.CaveBackStyle = Array.AsReadOnly(caveBackStyle.ToArray());
    owner.Appearance.IceBackStyle = iceBackStyle;
    owner.Appearance.JungleBackStyle = jungleBackStyle;
    owner.Appearance.HellBackStyle = hellBackStyle;
    owner.Descriptor.SpawnTileX = spawnTileX;
    owner.Descriptor.SpawnTileY = spawnTileY;
    owner.Descriptor.SurfaceLayer = worldSurface;
    owner.Descriptor.RockLayer = rockLayer;
    owner.TimeWeather.Time = time;
    owner.TimeWeather.DayTime = dayTime;
    owner.TimeWeather.MoonPhase = moonPhase;
    owner.TimeWeather.BloodMoon = bloodMoon;
    owner.TimeWeather.Eclipse = eclipse;
    owner.Descriptor.DungeonTileX = dungeonX;
    owner.Descriptor.DungeonTileY = dungeonY;
    owner.Rules.WorldEvil = crimson ? WorldEvilType.Crimson : WorldEvilType.Corruption;
  }

  public static void ApplyProgression(WorldSessionRestoreState owner,
      bool downedBoss1,
      bool downedBoss2,
      bool downedBoss3,
      bool downedQueenBee,
      bool downedMechBoss1,
      bool downedMechBoss2,
      bool downedMechBoss3,
      bool downedMechBossAny,
      bool downedPlantBoss,
      bool downedGolemBoss,
      bool downedSlimeKing,
      bool savedGoblin,
      bool savedWizard,
      bool savedMech,
      bool downedGoblins,
      bool downedClown,
      bool downedFrost,
      bool downedPirates,
      bool shadowOrbSmashed,
      bool spawnMeteor,
      byte shadowOrbCount,
      int altarCount,
      bool hardMode,
      bool afterPartyOfDoom,
      int invasionDelay,
      int invasionSize,
      int invasionType,
      double invasionX,
      double slimeRainTime,
      byte sundialCooldown,
      bool raining,
      int rainTime,
      float maxRain,
      int cobaltOreTier,
      int mythrilOreTier,
      int adamantiteOreTier,
      IReadOnlyList<byte> backgroundStyles,
      int cloudBackgroundActive,
      short cloudCount,
      float windSpeedTarget) {
    ArgumentNullException.ThrowIfNull(owner);
    owner.Progression.Bosses.Boss1 = downedBoss1;
    owner.Progression.Bosses.Boss2 = downedBoss2;
    owner.Progression.Bosses.Boss3 = downedBoss3;
    owner.Progression.Bosses.QueenBee = downedQueenBee;
    owner.Progression.Bosses.MechBoss1 = downedMechBoss1;
    owner.Progression.Bosses.MechBoss2 = downedMechBoss2;
    owner.Progression.Bosses.MechBoss3 = downedMechBoss3;
    owner.Milestones.AnyMechBossDowned = downedMechBossAny;
    owner.Progression.Bosses.PlantBoss = downedPlantBoss;
    owner.Progression.Bosses.GolemBoss = downedGolemBoss;
    owner.Progression.Bosses.SlimeKing = downedSlimeKing;
    owner.Progression.SavedNpcs.Goblin = savedGoblin;
    owner.Progression.SavedNpcs.Wizard = savedWizard;
    owner.Progression.SavedNpcs.Mechanic = savedMech;
    owner.Progression.Invasions.Goblins = downedGoblins;
    owner.Progression.Invasions.Clown = downedClown;
    owner.Progression.Invasions.Frost = downedFrost;
    owner.Progression.Invasions.Pirates = downedPirates;
    owner.Milestones.ShadowOrbSmashed = shadowOrbSmashed;
    owner.Progression.PendingEvents.SpawnMeteor = spawnMeteor;
    owner.Milestones.ShadowOrbCount = shadowOrbCount;
    owner.Milestones.AltarCount = altarCount;
    owner.Rules.HardMode = hardMode;
    owner.Progression.PendingEvents.AfterPartyOfDoom = afterPartyOfDoom;
    owner.Progression.Invasion.Delay = invasionDelay;
    owner.Progression.Invasion.Size = invasionSize;
    owner.Progression.Invasion.Type = (InvasionType)invasionType;
    owner.Progression.Invasion.PositionX = invasionX;
    owner.TimeWeather.SlimeRainTime = slimeRainTime;
    owner.TimeWeather.SundialCooldown = sundialCooldown;
    owner.TimeWeather.Raining = raining;
    owner.TimeWeather.RainTime = rainTime;
    owner.TimeWeather.RainStrength = maxRain;
    owner.Rules.SavedOreTiers = owner.Rules.SavedOreTiers with { Cobalt = cobaltOreTier };
    owner.Rules.SavedOreTiers = owner.Rules.SavedOreTiers with { Mythril = mythrilOreTier };
    owner.Rules.SavedOreTiers = owner.Rules.SavedOreTiers with { Adamantite = adamantiteOreTier };
    owner.Appearance.BackgroundStyles = Array.AsReadOnly(backgroundStyles.ToArray());
    owner.Appearance.CloudBackgroundActive = cloudBackgroundActive;
    owner.Appearance.CloudCount = cloudCount;
    owner.TimeWeather.WindTarget = windSpeedTarget;
  }

  public static void ApplyQuest(WorldSessionRestoreState owner,
      IReadOnlyList<string> anglerWhoFinishedToday,
      bool savedAngler,
      int anglerQuest,
      bool savedStylist,
      bool savedTaxCollector,
      bool savedGolfer,
      int invasionSizeStart,
      int cultistDelay) {
    ArgumentNullException.ThrowIfNull(owner);
    owner.History.AnglerWhoFinishedToday = Array.AsReadOnly(anglerWhoFinishedToday.ToArray());
    owner.Progression.SavedNpcs.Angler = savedAngler;
    owner.History.AnglerQuest = anglerQuest;
    owner.Progression.SavedNpcs.Stylist = savedStylist;
    owner.Progression.SavedNpcs.TaxCollector = savedTaxCollector;
    owner.Progression.SavedNpcs.Golfer = savedGolfer;
    owner.Progression.Invasion.SizeStart = invasionSizeStart;
    owner.TimeWeather.CultistDelay = cultistDelay;
  }

  public static void ApplyBanner(WorldSessionRestoreState owner,
      IReadOnlyList<int> killCounts,
      IReadOnlyList<ushort> claimableCounts) {
    ArgumentNullException.ThrowIfNull(owner);
    owner.History.BannerKillCounts = Array.AsReadOnly(killCounts.ToArray());
    owner.History.BannerClaimableCounts = Array.AsReadOnly(claimableCounts.ToArray());
  }

  public static void ApplyBossProgression(WorldSessionRestoreState owner,
      bool fastForwardTimeToDawn,
      bool downedFishron,
      bool downedMartians,
      bool downedAncientCultist,
      bool downedMoonlord,
      bool downedHalloweenKing,
      bool downedHalloweenTree,
      bool downedChristmasIceQueen,
      bool downedChristmasSantank,
      bool downedChristmasTree,
      bool downedTowerSolar,
      bool downedTowerVortex,
      bool downedTowerNebula,
      bool downedTowerStardust,
      bool towerActiveSolar,
      bool towerActiveVortex,
      bool towerActiveNebula,
      bool towerActiveStardust,
      bool lunarApocalypseIsUp) {
    ArgumentNullException.ThrowIfNull(owner);
    owner.TimeWeather.FastForwardTimeToDawn = fastForwardTimeToDawn;
    owner.Progression.Bosses.Fishron = downedFishron;
    owner.Progression.Invasions.Martians = downedMartians;
    owner.Progression.Bosses.AncientCultist = downedAncientCultist;
    owner.Progression.Bosses.Moonlord = downedMoonlord;
    owner.Progression.Bosses.HalloweenKing = downedHalloweenKing;
    owner.Progression.Bosses.HalloweenTree = downedHalloweenTree;
    owner.Progression.Bosses.ChristmasIceQueen = downedChristmasIceQueen;
    owner.Progression.Bosses.ChristmasSantank = downedChristmasSantank;
    owner.Progression.Bosses.ChristmasTree = downedChristmasTree;
    owner.Progression.Lunar.DownedSolarTower = downedTowerSolar;
    owner.Progression.Lunar.DownedVortexTower = downedTowerVortex;
    owner.Progression.Lunar.DownedNebulaTower = downedTowerNebula;
    owner.Progression.Lunar.DownedStardustTower = downedTowerStardust;
    owner.Progression.Lunar.SolarTowerActive = towerActiveSolar;
    owner.Progression.Lunar.VortexTowerActive = towerActiveVortex;
    owner.Progression.Lunar.NebulaTowerActive = towerActiveNebula;
    owner.Progression.Lunar.StardustTowerActive = towerActiveStardust;
    owner.Progression.Lunar.LunarApocalypseIsUp = lunarApocalypseIsUp;
  }

  public static void ApplyParty(WorldSessionRestoreState owner,
      bool manual,
      bool genuine,
      int cooldown,
      IReadOnlyList<int> celebratingNpcIds) {
    ArgumentNullException.ThrowIfNull(owner);
    owner.TimeWeather.BirthdayParty.ManualParty = manual;
    owner.TimeWeather.BirthdayParty.GenuineParty = genuine;
    owner.TimeWeather.BirthdayParty.PartyDaysOnCooldown = cooldown;
    owner.TimeWeather.BirthdayParty.CelebratingNpcIds = celebratingNpcIds.ToList();
  }

  public static void ApplySandstorm(WorldSessionRestoreState owner,
      bool happening,
      int timeLeft,
      float severity,
      float intendedSeverity) {
    ArgumentNullException.ThrowIfNull(owner);
    owner.TimeWeather.Sandstorm.Happening = happening;
    owner.TimeWeather.Sandstorm.TimeLeft = timeLeft;
    owner.TimeWeather.Sandstorm.Severity = severity;
    owner.TimeWeather.Sandstorm.IntendedSeverity = intendedSeverity;
  }

  public static void ApplyDefenderEvent(WorldSessionRestoreState owner,
      bool savedBartender,
      bool downedInvasionTier1,
      bool downedInvasionTier2,
      bool downedInvasionTier3) {
    ArgumentNullException.ThrowIfNull(owner);
    owner.Progression.SavedNpcs.Bartender = savedBartender;
    owner.Progression.Dd2.DownedTier1 = downedInvasionTier1;
    owner.Progression.Dd2.DownedTier2 = downedInvasionTier2;
    owner.Progression.Dd2.DownedTier3 = downedInvasionTier3;
  }

  public static void ApplyBackground(WorldSessionRestoreState owner,
      IReadOnlyList<byte> styles) {
    ArgumentNullException.ThrowIfNull(owner);
    owner.Appearance.AdditionalBackgroundStyles = Array.AsReadOnly(styles.ToArray());
  }

  public static void ApplyEvent(WorldSessionRestoreState owner,
      bool combatBookWasUsed,
      int lanternNightCooldown,
      bool lanternNightGenuine,
      bool lanternNightManual,
      bool lanternNightNextNightIsGenuine) {
    ArgumentNullException.ThrowIfNull(owner);
    owner.Progression.NpcWorldUnlocks.CombatBookUsed = combatBookWasUsed;
    owner.TimeWeather.LanternNight.LanternNightsOnCooldown = lanternNightCooldown;
    owner.TimeWeather.LanternNight.GenuineLanterns = lanternNightGenuine;
    owner.TimeWeather.LanternNight.ManualLanterns = lanternNightManual;
    owner.TimeWeather.LanternNight.NextNightIsLanternNight = lanternNightNextNightIsGenuine;
  }

  public static void ApplySeasonal(WorldSessionRestoreState owner,
      bool forceHalloweenForToday,
      bool forceChristmasForToday,
      int copperOreTier,
      int ironOreTier,
      int silverOreTier,
      int goldOreTier) {
    ArgumentNullException.ThrowIfNull(owner);
    owner.Progression.PendingEvents.ForceHalloweenForToday = forceHalloweenForToday;
    owner.Progression.PendingEvents.ForceChristmasForToday = forceChristmasForToday;
    owner.Rules.SavedOreTiers = owner.Rules.SavedOreTiers with { Copper = copperOreTier };
    owner.Rules.SavedOreTiers = owner.Rules.SavedOreTiers with { Iron = ironOreTier };
    owner.Rules.SavedOreTiers = owner.Rules.SavedOreTiers with { Silver = silverOreTier };
    owner.Rules.SavedOreTiers = owner.Rules.SavedOreTiers with { Gold = goldOreTier };
  }

  public static void ApplyNpcUnlock(WorldSessionRestoreState owner,
      bool boughtCat,
      bool boughtDog,
      bool boughtBunny,
      bool downedEmpressOfLight,
      bool downedQueenSlime,
      bool downedDeerclops,
      bool unlockedSlimeBlueSpawn,
      bool unlockedMerchantSpawn,
      bool unlockedDemolitionistSpawn,
      bool unlockedPartyGirlSpawn,
      bool unlockedDyeTraderSpawn,
      bool unlockedTruffleSpawn,
      bool unlockedArmsDealerSpawn,
      bool unlockedNurseSpawn,
      bool unlockedPrincessSpawn,
      bool combatBookVolumeTwoWasUsed,
      bool peddlersSatchelWasUsed,
      bool unlockedSlimeGreenSpawn,
      bool unlockedSlimeOldSpawn,
      bool unlockedSlimePurpleSpawn,
      bool unlockedSlimeRainbowSpawn,
      bool unlockedSlimeRedSpawn,
      bool unlockedSlimeYellowSpawn,
      bool unlockedSlimeCopperSpawn) {
    ArgumentNullException.ThrowIfNull(owner);
    owner.Progression.NpcWorldUnlocks.BoughtCat = boughtCat;
    owner.Progression.NpcWorldUnlocks.BoughtDog = boughtDog;
    owner.Progression.NpcWorldUnlocks.BoughtBunny = boughtBunny;
    owner.Progression.Bosses.EmpressOfLight = downedEmpressOfLight;
    owner.Progression.Bosses.QueenSlime = downedQueenSlime;
    owner.Progression.Bosses.Deerclops = downedDeerclops;
    owner.Progression.UnlockedNpcSpawns.SlimeBlue = unlockedSlimeBlueSpawn;
    owner.Progression.UnlockedNpcSpawns.Merchant = unlockedMerchantSpawn;
    owner.Progression.UnlockedNpcSpawns.Demolitionist = unlockedDemolitionistSpawn;
    owner.Progression.UnlockedNpcSpawns.PartyGirl = unlockedPartyGirlSpawn;
    owner.Progression.UnlockedNpcSpawns.DyeTrader = unlockedDyeTraderSpawn;
    owner.Progression.UnlockedNpcSpawns.Truffle = unlockedTruffleSpawn;
    owner.Progression.UnlockedNpcSpawns.ArmsDealer = unlockedArmsDealerSpawn;
    owner.Progression.UnlockedNpcSpawns.Nurse = unlockedNurseSpawn;
    owner.Progression.UnlockedNpcSpawns.Princess = unlockedPrincessSpawn;
    owner.Progression.NpcWorldUnlocks.CombatBookVolumeTwoUsed = combatBookVolumeTwoWasUsed;
    owner.Progression.NpcWorldUnlocks.PeddlersSatchelUsed = peddlersSatchelWasUsed;
    owner.Progression.UnlockedNpcSpawns.SlimeGreen = unlockedSlimeGreenSpawn;
    owner.Progression.UnlockedNpcSpawns.SlimeOld = unlockedSlimeOldSpawn;
    owner.Progression.UnlockedNpcSpawns.SlimePurple = unlockedSlimePurpleSpawn;
    owner.Progression.UnlockedNpcSpawns.SlimeRainbow = unlockedSlimeRainbowSpawn;
    owner.Progression.UnlockedNpcSpawns.SlimeRed = unlockedSlimeRedSpawn;
    owner.Progression.UnlockedNpcSpawns.SlimeYellow = unlockedSlimeYellowSpawn;
    owner.Progression.UnlockedNpcSpawns.SlimeCopper = unlockedSlimeCopperSpawn;
  }

  public static void ApplyTimePolicy(WorldSessionRestoreState owner,
      bool fastForwardTimeToDusk,
      byte moondialCooldown,
      bool forceHalloweenForever,
      bool forceChristmasForever,
      bool vampireSeed,
      bool infectedSeed,
      int meteorShowerCount,
      int coinRain,
      bool teamBasedSpawnsSeed) {
    ArgumentNullException.ThrowIfNull(owner);
    owner.TimeWeather.FastForwardTimeToDusk = fastForwardTimeToDusk;
    owner.TimeWeather.MoondialCooldown = moondialCooldown;
    owner.SeasonPolicy.ForceHalloweenForever = forceHalloweenForever;
    owner.SeasonPolicy.ForceChristmasForever = forceChristmasForever;
    owner.Rules.SecretSeeds = vampireSeed ? owner.Rules.SecretSeeds | WorldSecretSeedFlags.Vampire
        : owner.Rules.SecretSeeds & ~WorldSecretSeedFlags.Vampire;
    owner.Rules.SecretSeeds = infectedSeed ? owner.Rules.SecretSeeds | WorldSecretSeedFlags.Infected
        : owner.Rules.SecretSeeds & ~WorldSecretSeedFlags.Infected;
    owner.Progression.PendingEvents.MeteorShowerCount = meteorShowerCount;
    owner.TimeWeather.CoinRain = coinRain;
    owner.Rules.SecretSeeds = teamBasedSpawnsSeed
        ? owner.Rules.SecretSeeds | WorldSecretSeedFlags.TeamBasedSpawns
        : owner.Rules.SecretSeeds & ~WorldSecretSeedFlags.TeamBasedSpawns;
  }

  public static void ApplyHeader(WorldSessionRestoreState owner,
      WorldDescriptorSnapshotValue descriptor, WorldGameMode gameMode, WorldSecretSeedFlags seeds) {
    WorldDescriptorState target = owner.Descriptor;
    target.WorldId = descriptor.WorldId;
    target.UniqueId = descriptor.UniqueId;
    target.Name = descriptor.Name;
    target.SeedText = descriptor.SeedText;
    target.WorldGeneratorVersion = descriptor.WorldGeneratorVersion;
    target.SizeX = descriptor.SizeX;
    target.SizeY = descriptor.SizeY;
    target.LeftWorld = descriptor.Bounds.Left;
    target.RightWorld = descriptor.Bounds.Right;
    target.TopWorld = descriptor.Bounds.Top;
    target.BottomWorld = descriptor.Bounds.Bottom;
    owner.Rules.GameMode = gameMode;
    owner.Rules.SecretSeeds = seeds;
  }

  public static void ApplyBestiary(WorldSessionRestoreState owner,
      IReadOnlyDictionary<string, int> kills, IReadOnlyList<string> seen,
      IReadOnlyList<string> chatted) {
    owner.History.BestiaryKillCounts = new ReadOnlyDictionary<string, int>(
        new Dictionary<string, int>(kills));
    owner.History.SeenNpcIds = Array.AsReadOnly(seen.ToArray());
    owner.History.ChattedNpcIds = Array.AsReadOnly(chatted.ToArray());
  }

  public static void ApplyShimmeredTownNpcs(WorldNpcHistoryStateComponent owner,
      IReadOnlyList<int> npcIds) {
    ArgumentNullException.ThrowIfNull(owner);
    owner.ShimmeredTownNpcIds = Array.AsReadOnly(npcIds.ToArray());
  }
}
