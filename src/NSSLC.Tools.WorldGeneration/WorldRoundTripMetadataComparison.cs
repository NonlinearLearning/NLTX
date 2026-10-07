using Terraria.NonAuthoritative.Persistence;
using Terraria.WorldSession.Components;
using Terraria.WorldStorage;
using static WorldPersistenceRoundTrip;

internal static class WorldRoundTripMetadataComparison {
  public static void Compare(WorldPersistenceDocument document, LoadedWorldSession loaded) {
    WorldSessionRestoreState world = loaded.World;
    var environment = Get<WorldFileEnvironmentSection>(document,
        WorldFileEnvironmentSection.SectionId);
    Require(environment.MoonType == world.Appearance.MoonType, "Environment.MoonType differs.");
    Require(environment.TreeX.SequenceEqual(world.Appearance.TreeX), "Environment.TreeX differs.");
    Require(
        environment.TreeStyle.SequenceEqual(world.Appearance.TreeStyle),
        "Environment.TreeStyle differs.");
    Require(
        environment.CaveBackX.SequenceEqual(world.Appearance.CaveBackX),
        "Environment.CaveBackX differs.");
    Require(
        environment.CaveBackStyle.SequenceEqual(world.Appearance.CaveBackStyle),
        "Environment.CaveBackStyle differs.");
    Require(
        environment.IceBackStyle == world.Appearance.IceBackStyle,
        "Environment.IceBackStyle differs.");
    Require(
        environment.JungleBackStyle == world.Appearance.JungleBackStyle,
        "Environment.JungleBackStyle differs.");
    Require(
        environment.HellBackStyle == world.Appearance.HellBackStyle,
        "Environment.HellBackStyle differs.");
    Require(
        environment.SpawnTileX == world.Descriptor.SpawnTileX,
        "Environment.SpawnTileX differs.");
    Require(
        environment.SpawnTileY == world.Descriptor.SpawnTileY,
        "Environment.SpawnTileY differs.");
    Require(
        environment.WorldSurface == world.Descriptor.SurfaceLayer,
        "Environment.WorldSurface differs.");
    Require(environment.RockLayer == world.Descriptor.RockLayer, "Environment.RockLayer differs.");
    Require(environment.Time == world.TimeWeather.Time, "Environment.Time differs.");
    Require(environment.DayTime == world.TimeWeather.DayTime, "Environment.DayTime differs.");
    Require(environment.MoonPhase == world.TimeWeather.MoonPhase, "Environment.MoonPhase differs.");
    Require(environment.BloodMoon == world.TimeWeather.BloodMoon, "Environment.BloodMoon differs.");
    Require(environment.Eclipse == world.TimeWeather.Eclipse, "Environment.Eclipse differs.");
    Require(environment.DungeonX == world.Descriptor.DungeonTileX, "Environment.DungeonX differs.");
    Require(environment.DungeonY == world.Descriptor.DungeonTileY, "Environment.DungeonY differs.");
    Require(
        environment.Crimson == (world.Rules.WorldEvil == WorldEvilType.Crimson),
        "Environment.Crimson differs.");
    var progression = Get<WorldFileProgressionSection>(document,
        WorldFileProgressionSection.SectionId);
    Require(
        progression.DownedBoss1 == world.Progression.Bosses.Boss1,
        "Progression.DownedBoss1 differs.");
    Require(
        progression.DownedBoss2 == world.Progression.Bosses.Boss2,
        "Progression.DownedBoss2 differs.");
    Require(
        progression.DownedBoss3 == world.Progression.Bosses.Boss3,
        "Progression.DownedBoss3 differs.");
    Require(
        progression.DownedQueenBee == world.Progression.Bosses.QueenBee,
        "Progression.DownedQueenBee differs.");
    Require(
        progression.DownedMechBoss1 == world.Progression.Bosses.MechBoss1,
        "Progression.DownedMechBoss1 differs.");
    Require(
        progression.DownedMechBoss2 == world.Progression.Bosses.MechBoss2,
        "Progression.DownedMechBoss2 differs.");
    Require(
        progression.DownedMechBoss3 == world.Progression.Bosses.MechBoss3,
        "Progression.DownedMechBoss3 differs.");
    Require(
        progression.DownedMechBossAny == world.Milestones.AnyMechBossDowned,
        "Progression.DownedMechBossAny differs.");
    Require(
        progression.DownedPlantBoss == world.Progression.Bosses.PlantBoss,
        "Progression.DownedPlantBoss differs.");
    Require(
        progression.DownedGolemBoss == world.Progression.Bosses.GolemBoss,
        "Progression.DownedGolemBoss differs.");
    Require(
        progression.DownedSlimeKing == world.Progression.Bosses.SlimeKing,
        "Progression.DownedSlimeKing differs.");
    Require(
        progression.SavedGoblin == world.Progression.SavedNpcs.Goblin,
        "Progression.SavedGoblin differs.");
    Require(
        progression.SavedWizard == world.Progression.SavedNpcs.Wizard,
        "Progression.SavedWizard differs.");
    Require(
        progression.SavedMech == world.Progression.SavedNpcs.Mechanic,
        "Progression.SavedMech differs.");
    Require(
        progression.DownedGoblins == world.Progression.Invasions.Goblins,
        "Progression.DownedGoblins differs.");
    Require(
        progression.DownedClown == world.Progression.Invasions.Clown,
        "Progression.DownedClown differs.");
    Require(
        progression.DownedFrost == world.Progression.Invasions.Frost,
        "Progression.DownedFrost differs.");
    Require(
        progression.DownedPirates == world.Progression.Invasions.Pirates,
        "Progression.DownedPirates differs.");
    Require(
        progression.ShadowOrbSmashed == world.Milestones.ShadowOrbSmashed,
        "Progression.ShadowOrbSmashed differs.");
    Require(
        progression.SpawnMeteor == world.Progression.PendingEvents.SpawnMeteor,
        "Progression.SpawnMeteor differs.");
    Require(
        progression.ShadowOrbCount == world.Milestones.ShadowOrbCount,
        "Progression.ShadowOrbCount differs.");
    Require(
        progression.AltarCount == world.Milestones.AltarCount,
        "Progression.AltarCount differs.");
    Require(progression.HardMode == world.Rules.HardMode, "Progression.HardMode differs.");
    Require(
        progression.AfterPartyOfDoom == world.Progression.PendingEvents.AfterPartyOfDoom,
        "Progression.AfterPartyOfDoom differs.");
    Require(
        progression.InvasionDelay == world.Progression.Invasion.Delay,
        "Progression.InvasionDelay differs.");
    Require(
        progression.InvasionSize == world.Progression.Invasion.Size,
        "Progression.InvasionSize differs.");
    Require(
        progression.InvasionType == (int)world.Progression.Invasion.Type,
        "Progression.InvasionType differs.");
    Require(
        progression.InvasionX == world.Progression.Invasion.PositionX,
        "Progression.InvasionX differs.");
    Require(
        progression.SlimeRainTime == world.TimeWeather.SlimeRainTime,
        "Progression.SlimeRainTime differs.");
    Require(
        progression.SundialCooldown == world.TimeWeather.SundialCooldown,
        "Progression.SundialCooldown differs.");
    Require(progression.Raining == world.TimeWeather.Raining, "Progression.Raining differs.");
    Require(progression.RainTime == world.TimeWeather.RainTime, "Progression.RainTime differs.");
    Require(progression.MaxRain == world.TimeWeather.RainStrength, "Progression.MaxRain differs.");
    Require(
        progression.CobaltOreTier == world.Rules.SavedOreTiers.Cobalt,
        "Progression.CobaltOreTier differs.");
    Require(
        progression.MythrilOreTier == world.Rules.SavedOreTiers.Mythril,
        "Progression.MythrilOreTier differs.");
    Require(
        progression.AdamantiteOreTier == world.Rules.SavedOreTiers.Adamantite,
        "Progression.AdamantiteOreTier differs.");
    Require(
        progression.BackgroundStyles.SequenceEqual(world.Appearance.BackgroundStyles),
        "Progression.BackgroundStyles differs.");
    Require(
        progression.CloudBackgroundActive == world.Appearance.CloudBackgroundActive,
        "Progression.CloudBackgroundActive differs.");
    Require(
        progression.CloudCount == world.Appearance.CloudCount,
        "Progression.CloudCount differs.");
    Require(
        progression.WindSpeedTarget == world.TimeWeather.WindTarget,
        "Progression.WindSpeedTarget differs.");
    var quest = Get<WorldFileQuestSection>(document,
        WorldFileQuestSection.SectionId);
    Require(
        quest.AnglerWhoFinishedToday.SequenceEqual(world.History.AnglerWhoFinishedToday),
        "Quest.AnglerWhoFinishedToday differs.");
    Require(quest.SavedAngler == world.Progression.SavedNpcs.Angler, "Quest.SavedAngler differs.");
    Require(quest.AnglerQuest == world.History.AnglerQuest, "Quest.AnglerQuest differs.");
    Require(
        quest.SavedStylist == world.Progression.SavedNpcs.Stylist,
        "Quest.SavedStylist differs.");
    Require(
        quest.SavedTaxCollector == world.Progression.SavedNpcs.TaxCollector,
        "Quest.SavedTaxCollector differs.");
    Require(quest.SavedGolfer == world.Progression.SavedNpcs.Golfer, "Quest.SavedGolfer differs.");
    Require(
        quest.InvasionSizeStart == world.Progression.Invasion.SizeStart,
        "Quest.InvasionSizeStart differs.");
    Require(quest.CultistDelay == world.TimeWeather.CultistDelay, "Quest.CultistDelay differs.");
    var banner = Get<WorldFileBannerSection>(document,
        WorldFileBannerSection.SectionId);
    Require(
        banner.KillCounts.SequenceEqual(world.History.BannerKillCounts),
        "Banner.KillCounts differs.");
    Require(
        banner.ClaimableCounts.SequenceEqual(world.History.BannerClaimableCounts),
        "Banner.ClaimableCounts differs.");
    var bossProgression = Get<WorldFileBossProgressionSection>(document,
        WorldFileBossProgressionSection.SectionId);
    Require(
        bossProgression.FastForwardTimeToDawn == world.TimeWeather.FastForwardTimeToDawn,
        "BossProgression.FastForwardTimeToDawn differs.");
    Require(
        bossProgression.DownedFishron == world.Progression.Bosses.Fishron,
        "BossProgression.DownedFishron differs.");
    Require(
        bossProgression.DownedMartians == world.Progression.Invasions.Martians,
        "BossProgression.DownedMartians differs.");
    Require(
        bossProgression.DownedAncientCultist == world.Progression.Bosses.AncientCultist,
        "BossProgression.DownedAncientCultist differs.");
    Require(
        bossProgression.DownedMoonlord == world.Progression.Bosses.Moonlord,
        "BossProgression.DownedMoonlord differs.");
    Require(
        bossProgression.DownedHalloweenKing == world.Progression.Bosses.HalloweenKing,
        "BossProgression.DownedHalloweenKing differs.");
    Require(
        bossProgression.DownedHalloweenTree == world.Progression.Bosses.HalloweenTree,
        "BossProgression.DownedHalloweenTree differs.");
    Require(
        bossProgression.DownedChristmasIceQueen == world.Progression.Bosses.ChristmasIceQueen,
        "BossProgression.DownedChristmasIceQueen differs.");
    Require(
        bossProgression.DownedChristmasSantank == world.Progression.Bosses.ChristmasSantank,
        "BossProgression.DownedChristmasSantank differs.");
    Require(
        bossProgression.DownedChristmasTree == world.Progression.Bosses.ChristmasTree,
        "BossProgression.DownedChristmasTree differs.");
    Require(
        bossProgression.DownedTowerSolar == world.Progression.Lunar.DownedSolarTower,
        "BossProgression.DownedTowerSolar differs.");
    Require(
        bossProgression.DownedTowerVortex == world.Progression.Lunar.DownedVortexTower,
        "BossProgression.DownedTowerVortex differs.");
    Require(
        bossProgression.DownedTowerNebula == world.Progression.Lunar.DownedNebulaTower,
        "BossProgression.DownedTowerNebula differs.");
    Require(
        bossProgression.DownedTowerStardust == world.Progression.Lunar.DownedStardustTower,
        "BossProgression.DownedTowerStardust differs.");
    Require(
        bossProgression.TowerActiveSolar == world.Progression.Lunar.SolarTowerActive,
        "BossProgression.TowerActiveSolar differs.");
    Require(
        bossProgression.TowerActiveVortex == world.Progression.Lunar.VortexTowerActive,
        "BossProgression.TowerActiveVortex differs.");
    Require(
        bossProgression.TowerActiveNebula == world.Progression.Lunar.NebulaTowerActive,
        "BossProgression.TowerActiveNebula differs.");
    Require(
        bossProgression.TowerActiveStardust == world.Progression.Lunar.StardustTowerActive,
        "BossProgression.TowerActiveStardust differs.");
    Require(
        bossProgression.LunarApocalypseIsUp == world.Progression.Lunar.LunarApocalypseIsUp,
        "BossProgression.LunarApocalypseIsUp differs.");
    var party = Get<WorldFilePartySection>(document,
        WorldFilePartySection.SectionId);
    Require(party.Manual == world.TimeWeather.BirthdayParty.ManualParty, "Party.Manual differs.");
    Require(
        party.Genuine == world.TimeWeather.BirthdayParty.GenuineParty,
        "Party.Genuine differs.");
    Require(
        party.Cooldown == world.TimeWeather.BirthdayParty.PartyDaysOnCooldown,
        "Party.Cooldown differs.");
    Require(
        party.CelebratingNpcIds.SequenceEqual(world.TimeWeather.BirthdayParty.CelebratingNpcIds),
        "Party.CelebratingNpcIds differs.");
    var sandstorm = Get<WorldFileSandstormSection>(document,
        WorldFileSandstormSection.SectionId);
    Require(
        sandstorm.Happening == world.TimeWeather.Sandstorm.Happening,
        "Sandstorm.Happening differs.");
    Require(
        sandstorm.TimeLeft == world.TimeWeather.Sandstorm.TimeLeft,
        "Sandstorm.TimeLeft differs.");
    Require(
        sandstorm.Severity == world.TimeWeather.Sandstorm.Severity,
        "Sandstorm.Severity differs.");
    Require(
        sandstorm.IntendedSeverity == world.TimeWeather.Sandstorm.IntendedSeverity,
        "Sandstorm.IntendedSeverity differs.");
    var defenderEvent = Get<WorldFileDefenderEventSection>(document,
        WorldFileDefenderEventSection.SectionId);
    Require(
        defenderEvent.SavedBartender == world.Progression.SavedNpcs.Bartender,
        "DefenderEvent.SavedBartender differs.");
    Require(
        defenderEvent.DownedInvasionTier1 == world.Progression.Dd2.DownedTier1,
        "DefenderEvent.DownedInvasionTier1 differs.");
    Require(
        defenderEvent.DownedInvasionTier2 == world.Progression.Dd2.DownedTier2,
        "DefenderEvent.DownedInvasionTier2 differs.");
    Require(
        defenderEvent.DownedInvasionTier3 == world.Progression.Dd2.DownedTier3,
        "DefenderEvent.DownedInvasionTier3 differs.");
    var background = Get<WorldFileBackgroundSection>(document,
        WorldFileBackgroundSection.SectionId);
    Require(
        background.Styles.SequenceEqual(world.Appearance.AdditionalBackgroundStyles),
        "Background.Styles differs.");
    var eventFlags = Get<WorldFileEventSection>(document,
        WorldFileEventSection.SectionId);
    Require(
        eventFlags.CombatBookWasUsed == world.Progression.NpcWorldUnlocks.CombatBookUsed,
        "Event.CombatBookWasUsed differs.");
    Require(
        eventFlags.LanternNightCooldown == world.TimeWeather.LanternNight.LanternNightsOnCooldown,
        "Event.LanternNightCooldown differs.");
    Require(
        eventFlags.LanternNightGenuine == world.TimeWeather.LanternNight.GenuineLanterns,
        "Event.LanternNightGenuine differs.");
    Require(
        eventFlags.LanternNightManual == world.TimeWeather.LanternNight.ManualLanterns,
        "Event.LanternNightManual differs.");
    Require(
        eventFlags.LanternNightNextNightIsGenuine ==
            world.TimeWeather.LanternNight.NextNightIsLanternNight,
        "Event.LanternNightNextNightIsGenuine differs.");
    var seasonal = Get<WorldFileSeasonalSection>(document,
        WorldFileSeasonalSection.SectionId);
    Require(
        seasonal.ForceHalloweenForToday == world.Progression.PendingEvents.ForceHalloweenForToday,
        "Seasonal.ForceHalloweenForToday differs.");
    Require(
        seasonal.ForceChristmasForToday == world.Progression.PendingEvents.ForceChristmasForToday,
        "Seasonal.ForceChristmasForToday differs.");
    Require(
        seasonal.CopperOreTier == world.Rules.SavedOreTiers.Copper,
        "Seasonal.CopperOreTier differs.");
    Require(
        seasonal.IronOreTier == world.Rules.SavedOreTiers.Iron,
        "Seasonal.IronOreTier differs.");
    Require(
        seasonal.SilverOreTier == world.Rules.SavedOreTiers.Silver,
        "Seasonal.SilverOreTier differs.");
    Require(
        seasonal.GoldOreTier == world.Rules.SavedOreTiers.Gold,
        "Seasonal.GoldOreTier differs.");
    var npcUnlock = Get<WorldFileNpcUnlockSection>(document,
        WorldFileNpcUnlockSection.SectionId);
    Require(
        npcUnlock.BoughtCat == world.Progression.NpcWorldUnlocks.BoughtCat,
        "NpcUnlock.BoughtCat differs.");
    Require(
        npcUnlock.BoughtDog == world.Progression.NpcWorldUnlocks.BoughtDog,
        "NpcUnlock.BoughtDog differs.");
    Require(
        npcUnlock.BoughtBunny == world.Progression.NpcWorldUnlocks.BoughtBunny,
        "NpcUnlock.BoughtBunny differs.");
    Require(
        npcUnlock.DownedEmpressOfLight == world.Progression.Bosses.EmpressOfLight,
        "NpcUnlock.DownedEmpressOfLight differs.");
    Require(
        npcUnlock.DownedQueenSlime == world.Progression.Bosses.QueenSlime,
        "NpcUnlock.DownedQueenSlime differs.");
    Require(
        npcUnlock.DownedDeerclops == world.Progression.Bosses.Deerclops,
        "NpcUnlock.DownedDeerclops differs.");
    Require(
        npcUnlock.UnlockedSlimeBlueSpawn == world.Progression.UnlockedNpcSpawns.SlimeBlue,
        "NpcUnlock.UnlockedSlimeBlueSpawn differs.");
    Require(
        npcUnlock.UnlockedMerchantSpawn == world.Progression.UnlockedNpcSpawns.Merchant,
        "NpcUnlock.UnlockedMerchantSpawn differs.");
    Require(
        npcUnlock.UnlockedDemolitionistSpawn == world.Progression.UnlockedNpcSpawns.Demolitionist,
        "NpcUnlock.UnlockedDemolitionistSpawn differs.");
    Require(
        npcUnlock.UnlockedPartyGirlSpawn == world.Progression.UnlockedNpcSpawns.PartyGirl,
        "NpcUnlock.UnlockedPartyGirlSpawn differs.");
    Require(
        npcUnlock.UnlockedDyeTraderSpawn == world.Progression.UnlockedNpcSpawns.DyeTrader,
        "NpcUnlock.UnlockedDyeTraderSpawn differs.");
    Require(
        npcUnlock.UnlockedTruffleSpawn == world.Progression.UnlockedNpcSpawns.Truffle,
        "NpcUnlock.UnlockedTruffleSpawn differs.");
    Require(
        npcUnlock.UnlockedArmsDealerSpawn == world.Progression.UnlockedNpcSpawns.ArmsDealer,
        "NpcUnlock.UnlockedArmsDealerSpawn differs.");
    Require(
        npcUnlock.UnlockedNurseSpawn == world.Progression.UnlockedNpcSpawns.Nurse,
        "NpcUnlock.UnlockedNurseSpawn differs.");
    Require(
        npcUnlock.UnlockedPrincessSpawn == world.Progression.UnlockedNpcSpawns.Princess,
        "NpcUnlock.UnlockedPrincessSpawn differs.");
    Require(
        npcUnlock.CombatBookVolumeTwoWasUsed ==
            world.Progression.NpcWorldUnlocks.CombatBookVolumeTwoUsed,
        "NpcUnlock.CombatBookVolumeTwoWasUsed differs.");
    Require(
        npcUnlock.PeddlersSatchelWasUsed == world.Progression.NpcWorldUnlocks.PeddlersSatchelUsed,
        "NpcUnlock.PeddlersSatchelWasUsed differs.");
    Require(
        npcUnlock.UnlockedSlimeGreenSpawn == world.Progression.UnlockedNpcSpawns.SlimeGreen,
        "NpcUnlock.UnlockedSlimeGreenSpawn differs.");
    Require(
        npcUnlock.UnlockedSlimeOldSpawn == world.Progression.UnlockedNpcSpawns.SlimeOld,
        "NpcUnlock.UnlockedSlimeOldSpawn differs.");
    Require(
        npcUnlock.UnlockedSlimePurpleSpawn == world.Progression.UnlockedNpcSpawns.SlimePurple,
        "NpcUnlock.UnlockedSlimePurpleSpawn differs.");
    Require(
        npcUnlock.UnlockedSlimeRainbowSpawn == world.Progression.UnlockedNpcSpawns.SlimeRainbow,
        "NpcUnlock.UnlockedSlimeRainbowSpawn differs.");
    Require(
        npcUnlock.UnlockedSlimeRedSpawn == world.Progression.UnlockedNpcSpawns.SlimeRed,
        "NpcUnlock.UnlockedSlimeRedSpawn differs.");
    Require(
        npcUnlock.UnlockedSlimeYellowSpawn == world.Progression.UnlockedNpcSpawns.SlimeYellow,
        "NpcUnlock.UnlockedSlimeYellowSpawn differs.");
    Require(
        npcUnlock.UnlockedSlimeCopperSpawn == world.Progression.UnlockedNpcSpawns.SlimeCopper,
        "NpcUnlock.UnlockedSlimeCopperSpawn differs.");
    var timePolicy = Get<WorldFileTimePolicySection>(document,
        WorldFileTimePolicySection.SectionId);
    Require(
        timePolicy.FastForwardTimeToDusk == world.TimeWeather.FastForwardTimeToDusk,
        "TimePolicy.FastForwardTimeToDusk differs.");
    Require(
        timePolicy.MoondialCooldown == world.TimeWeather.MoondialCooldown,
        "TimePolicy.MoondialCooldown differs.");
    Require(
        timePolicy.ForceHalloweenForever == world.SeasonPolicy.ForceHalloweenForever,
        "TimePolicy.ForceHalloweenForever differs.");
    Require(
        timePolicy.ForceChristmasForever == world.SeasonPolicy.ForceChristmasForever,
        "TimePolicy.ForceChristmasForever differs.");
    Require(
        timePolicy.VampireSeed == ((world.Rules.SecretSeeds & WorldSecretSeedFlags.Vampire) != 0),
        "TimePolicy.VampireSeed differs.");
    Require(
        timePolicy.InfectedSeed == ((world.Rules.SecretSeeds & WorldSecretSeedFlags.Infected) != 0),
        "TimePolicy.InfectedSeed differs.");
    Require(
        timePolicy.MeteorShowerCount == world.Progression.PendingEvents.MeteorShowerCount,
        "TimePolicy.MeteorShowerCount differs.");
    Require(timePolicy.CoinRain == world.TimeWeather.CoinRain, "TimePolicy.CoinRain differs.");
    Require(
        timePolicy.TeamBasedSpawnsSeed ==
            ((world.Rules.SecretSeeds & WorldSecretSeedFlags.TeamBasedSpawns) != 0),
        "TimePolicy.TeamBasedSpawnsSeed differs.");
    var header = Get<WorldFileHeaderSection>(document,
        WorldFileHeaderSection.SectionId);
    var descriptor = world.Descriptor;
    Require(header.WorldId == descriptor.WorldId &&
        header.UniqueId == descriptor.UniqueId && header.WorldName == descriptor.Name &&
        header.SeedText == descriptor.SeedText &&
        header.WorldGeneratorVersion == descriptor.WorldGeneratorVersion &&
        header.MaxTilesX == descriptor.SizeX && header.MaxTilesY == descriptor.SizeY &&
        header.LeftWorld == descriptor.LeftWorld && header.RightWorld == descriptor.RightWorld &&
        header.TopWorld == descriptor.TopWorld && header.BottomWorld == descriptor.BottomWorld &&
        header.GameMode == (int)world.Rules.GameMode &&
        header.CreationTime == loaded.CreationTime && header.LastPlayed == loaded.LastPlayed,
        "World identity, rules or timestamps differ.");
    WorldFileMetadataSection metadata = Get<WorldFileMetadataSection>(document,
        WorldFileMetadataSection.SectionId);
    Require(loaded.Metadata is not null &&
        metadata.Revision == loaded.Metadata.Revision &&
        metadata.IsFavorite == loaded.Metadata.IsFavorite, "Archive metadata differs.");
    WorldFileTreeTopsSection treeTops = Get<WorldFileTreeTopsSection>(document,
        WorldFileTreeTopsSection.SectionId);
    Require(treeTops.Variations.SequenceEqual(
        Enumerable.Range(0, loaded.TreeTops.AreaCount).Select(loaded.TreeTops.GetTreeStyle)),
        "Tree top styles differ.");
    var spawn = Get<WorldFileSpawnSection>(document,
        WorldFileSpawnSection.SectionId);
    Require(spawn.WorldManifestJson == loaded.ManifestJson &&
        spawn.ExtraSpawnPoints.Select(point => new TileCoordinate(
            point.X, point.Y)).SequenceEqual(world.Descriptor.ExtraSpawnPoints),
        "Spawn configuration differs.");
    WorldFileTownManagerSection rooms = Get<WorldFileTownManagerSection>(document,
        WorldFileTownManagerSection.SectionId);
    Require(rooms.Rooms.Count == loaded.TownHousing.AssignedRoomCount,
        "Town housing room counts differ.");
    foreach (WorldFileTownRoomRecord room in rooms.Rooms) {
      bool hasRoom = Terraria.WorldGeneration.Systems.TownHousingRegistrySystem.TryGetRoom(
        loaded.TownHousing,
        new Terraria.WorldGeneration.Components.TownHousingResidentKey(room.NpcType),
        out var position);
      Require(hasRoom && position.X == room.TileX && position.Y == room.TileY,
          "Town housing room differs.");
    }
    WorldFileBestiarySection bestiary = Get<WorldFileBestiarySection>(document,
        WorldFileBestiarySection.SectionId);
    Require(
        bestiary.KillCounts.Count == world.History.BestiaryKillCounts.Count &&
        bestiary.KillCounts.All(record =>
            world.History.BestiaryKillCounts.GetValueOrDefault(record.PersistentId, -1) ==
            record.Count) &&
        bestiary.SeenNpcIds.SequenceEqual(world.History.SeenNpcIds) &&
        bestiary.ChattedNpcIds.SequenceEqual(world.History.ChattedNpcIds),
        "Bestiary history differs.");
    WorldFilePressurePlateSection plates = Get<WorldFilePressurePlateSection>(document,
        WorldFilePressurePlateSection.SectionId);
    Require(plates.Plates.Select(plate =>
        new TileCoordinate(plate.X, plate.Y)).SequenceEqual(
            loaded.Storage.PressurePlates.Anchors), "Pressure plate anchors differ.");
    WorldFileNpcSection npcs = Get<WorldFileNpcSection>(document, WorldFileNpcSection.SectionId);
    Require(npcs.ShimmeredTownNpcIds.SequenceEqual(
        world.History.ShimmeredTownNpcIds), "Shimmered NPC identities differ.");
    Require(!loaded.HasCreativePowers && loaded.IsComplete,
        "CreativePowers or completion state differs.");
    Require(document.SectionIds.Order(StringComparer.Ordinal).SequenceEqual(
        loaded.CommittedSections.Append(WorldFileTreeTopsSection.SectionId)
            .Order(StringComparer.Ordinal)), "Not every document section was committed.");
  }

  private static T Get<T>(WorldPersistenceDocument document, string id) where T : notnull {
    if (!document.TryGetSection<T>(id, out var section) || !section.IsPresent) {
      throw new InvalidOperationException("Missing expected section " + id);
    }
    return section.Value;
  }
}
