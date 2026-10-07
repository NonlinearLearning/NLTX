using NSSLC.WorldGeneration;
using Terraria.NonAuthoritative.Persistence;

namespace Terraria.NonAuthoritative.WorldStorage;

/// <summary>Maps a captured generated world to all sections of a standard WorldFile 319.</summary>
public static class GeneratedWorldDocumentProjection {
  public static WorldPersistenceDocument Create(GeneratedWorld world, string worldName,
      Guid uniqueId, DateTime creationTime, ulong generatorVersion = 0) {
    ArgumentNullException.ThrowIfNull(world);
    ArgumentException.ThrowIfNullOrWhiteSpace(worldName);
    GeneratedWorldSettings settings = world.Settings;
    var sections = new List<WorldPersistenceSection>();
    Add(sections, WorldFileHeaderSection.SectionId, new WorldFileHeaderSection(
        worldName, world.Seed, generatorVersion, uniqueId, world.WorldId,
        0, world.Width * 16, 0, world.Height * 16, world.Width, world.Height,
        settings.GameMode, false, false, false, false, false, false, false, false, false,
        creationTime, creationTime));
    Add(sections, WorldFileMetadataSection.SectionId, WorldFileMetadataSection.Empty);
    Add(sections, WorldFileEnvironmentSection.SectionId, new WorldFileEnvironmentSection(
        moonType: settings.MoonType,
        treeX: settings.TreeX,
        treeStyle: settings.TreeStyle,
        caveBackX: settings.CaveBackX,
        caveBackStyle: settings.CaveBackStyle,
        iceBackStyle: settings.IceBackStyle,
        jungleBackStyle: settings.JungleBackStyle,
        hellBackStyle: settings.HellBackStyle,
        spawnTileX: world.SpawnX,
        spawnTileY: world.SpawnY,
        worldSurface: world.Surface,
        rockLayer: world.RockLayer,
        time: settings.Time,
        dayTime: settings.DayTime,
        moonPhase: settings.MoonPhase,
        bloodMoon: settings.BloodMoon,
        eclipse: settings.Eclipse,
        dungeonX: settings.DungeonX,
        dungeonY: settings.DungeonY,
        crimson: world.Crimson));
    Add(sections, WorldFileProgressionSection.SectionId, new WorldFileProgressionSection(
        downedBoss1: settings.DownedBoss1,
        downedBoss2: settings.DownedBoss2,
        downedBoss3: settings.DownedBoss3,
        downedQueenBee: settings.DownedQueenBee,
        downedMechBoss1: settings.DownedMechBoss1,
        downedMechBoss2: settings.DownedMechBoss2,
        downedMechBoss3: settings.DownedMechBoss3,
        downedMechBossAny: settings.DownedMechBossAny,
        downedPlantBoss: settings.DownedPlantBoss,
        downedGolemBoss: settings.DownedGolemBoss,
        downedSlimeKing: settings.DownedSlimeKing,
        savedGoblin: settings.SavedGoblin,
        savedWizard: settings.SavedWizard,
        savedMech: settings.SavedMech,
        downedGoblins: settings.DownedGoblins,
        downedClown: settings.DownedClown,
        downedFrost: settings.DownedFrost,
        downedPirates: settings.DownedPirates,
        shadowOrbSmashed: settings.ShadowOrbSmashed,
        spawnMeteor: settings.SpawnMeteor,
        shadowOrbCount: settings.ShadowOrbCount,
        altarCount: settings.AltarCount,
        hardMode: settings.HardMode,
        afterPartyOfDoom: settings.AfterPartyOfDoom,
        invasionDelay: settings.InvasionDelay,
        invasionSize: settings.InvasionSize,
        invasionType: settings.InvasionType,
        invasionX: settings.InvasionX,
        slimeRainTime: settings.SlimeRainTime,
        sundialCooldown: checked((byte)settings.SundialCooldown),
        raining: settings.Raining,
        rainTime: settings.RainTime,
        maxRain: settings.MaxRain,
        cobaltOreTier: settings.CobaltOreTier,
        mythrilOreTier: settings.MythrilOreTier,
        adamantiteOreTier: settings.AdamantiteOreTier,
        backgroundStyles: settings.BackgroundStyles,
        cloudBackgroundActive: settings.CloudBackgroundActive,
        cloudCount: settings.CloudCount,
        windSpeedTarget: settings.WindSpeedTarget));
    Add(sections, WorldFileQuestSection.SectionId, new WorldFileQuestSection(
        anglerWhoFinishedToday: Array.Empty<string>(),
        savedAngler: settings.SavedAngler,
        anglerQuest: 0,
        savedStylist: settings.SavedStylist,
        savedTaxCollector: settings.SavedTaxCollector,
        savedGolfer: settings.SavedGolfer,
        invasionSizeStart: settings.InvasionSizeStart,
        cultistDelay: 86400));
    Add(sections, WorldFileBossProgressionSection.SectionId, new WorldFileBossProgressionSection(
        fastForwardTimeToDawn: settings.FastForwardTimeToDawn,
        downedFishron: settings.DownedFishron,
        downedMartians: settings.DownedMartians,
        downedAncientCultist: settings.DownedAncientCultist,
        downedMoonlord: settings.DownedMoonlord,
        downedHalloweenKing: settings.DownedHalloweenKing,
        downedHalloweenTree: settings.DownedHalloweenTree,
        downedChristmasIceQueen: settings.DownedChristmasIceQueen,
        downedChristmasSantank: settings.DownedChristmasSantank,
        downedChristmasTree: settings.DownedChristmasTree,
        downedTowerSolar: settings.DownedTowerSolar,
        downedTowerVortex: settings.DownedTowerVortex,
        downedTowerNebula: settings.DownedTowerNebula,
        downedTowerStardust: settings.DownedTowerStardust,
        towerActiveSolar: settings.TowerActiveSolar,
        towerActiveVortex: settings.TowerActiveVortex,
        towerActiveNebula: settings.TowerActiveNebula,
        towerActiveStardust: settings.TowerActiveStardust,
        lunarApocalypseIsUp: settings.LunarApocalypseIsUp));
    Add(sections, WorldFileBackgroundSection.SectionId, new WorldFileBackgroundSection(
        styles: settings.AdditionalBackgroundStyles));
    Add(sections, WorldFileEventSection.SectionId, new WorldFileEventSection(
        combatBookWasUsed: settings.CombatBookWasUsed,
        lanternNightCooldown: 0,
        lanternNightGenuine: false,
        lanternNightManual: false,
        lanternNightNextNightIsGenuine: false));
    Add(sections, WorldFileSeasonalSection.SectionId, new WorldFileSeasonalSection(
        forceHalloweenForToday: settings.ForceHalloweenForToday,
        forceChristmasForToday: settings.ForceChristmasForToday,
        copperOreTier: settings.CopperOreTier,
        ironOreTier: settings.IronOreTier,
        silverOreTier: settings.SilverOreTier,
        goldOreTier: settings.GoldOreTier));
    Add(sections, WorldFileNpcUnlockSection.SectionId, new WorldFileNpcUnlockSection(
        boughtCat: settings.BoughtCat,
        boughtDog: settings.BoughtDog,
        boughtBunny: settings.BoughtBunny,
        downedEmpressOfLight: settings.DownedEmpressOfLight,
        downedQueenSlime: settings.DownedQueenSlime,
        downedDeerclops: settings.DownedDeerclops,
        unlockedSlimeBlueSpawn: settings.UnlockedSlimeBlueSpawn,
        unlockedMerchantSpawn: settings.UnlockedMerchantSpawn,
        unlockedDemolitionistSpawn: settings.UnlockedDemolitionistSpawn,
        unlockedPartyGirlSpawn: settings.UnlockedPartyGirlSpawn,
        unlockedDyeTraderSpawn: settings.UnlockedDyeTraderSpawn,
        unlockedTruffleSpawn: settings.UnlockedTruffleSpawn,
        unlockedArmsDealerSpawn: settings.UnlockedArmsDealerSpawn,
        unlockedNurseSpawn: settings.UnlockedNurseSpawn,
        unlockedPrincessSpawn: settings.UnlockedPrincessSpawn,
        combatBookVolumeTwoWasUsed: settings.CombatBookVolumeTwoWasUsed,
        peddlersSatchelWasUsed: settings.PeddlersSatchelWasUsed,
        unlockedSlimeGreenSpawn: settings.UnlockedSlimeGreenSpawn,
        unlockedSlimeOldSpawn: settings.UnlockedSlimeOldSpawn,
        unlockedSlimePurpleSpawn: settings.UnlockedSlimePurpleSpawn,
        unlockedSlimeRainbowSpawn: settings.UnlockedSlimeRainbowSpawn,
        unlockedSlimeRedSpawn: settings.UnlockedSlimeRedSpawn,
        unlockedSlimeYellowSpawn: settings.UnlockedSlimeYellowSpawn,
        unlockedSlimeCopperSpawn: settings.UnlockedSlimeCopperSpawn));
    Add(sections, WorldFileTimePolicySection.SectionId, new WorldFileTimePolicySection(
        fastForwardTimeToDusk: settings.FastForwardTimeToDusk,
        moondialCooldown: checked((byte)settings.MoondialCooldown),
        forceHalloweenForever: settings.ForceHalloweenForever,
        forceChristmasForever: settings.ForceChristmasForever,
        vampireSeed: false,
        infectedSeed: false,
        meteorShowerCount: 0,
        coinRain: settings.CoinRain,
        teamBasedSpawnsSeed: false));
    Add(sections, WorldFileBannerSection.SectionId, WorldFileBannerSection.Empty);
    Add(sections, WorldFilePartySection.SectionId, WorldFilePartySection.Empty);
    Add(sections, WorldFileSandstormSection.SectionId, WorldFileSandstormSection.Empty);
    Add(sections, WorldFileDefenderEventSection.SectionId,
        new WorldFileDefenderEventSection(settings.SavedBartender, false, false, false));
    Add(sections, WorldFileTreeTopsSection.SectionId,
        new WorldFileTreeTopsSection(settings.TreeTops));
    Add(sections, WorldFileSpawnSection.SectionId, new WorldFileSpawnSection(
        Array.Empty<WorldFileExtraSpawnPoint>(), false, false, false, world.ManifestJson));
    Add(sections, WorldFileTilePayloadSection.SectionId, new WorldFileTileCodec().Encode(world));
    Add(sections, WorldFileChestSection.SectionId, new WorldFileChestSection(world.Chests
        .Select(chest => new WorldFileChestRecord(chest.X, chest.Y, chest.Name, chest.Items
            .Select(item => new WorldFileChestItem(item.Stack, item.Type, item.Prefix))
            .ToArray())).ToArray()));
    Add(sections, WorldFileSignSection.SectionId, new WorldFileSignSection(world.Signs
        .Select(sign => new WorldFileSignRecord(sign.Text, sign.X, sign.Y)).ToArray()));
    WorldFileNpcRecord[] townNpcs = world.Npcs.Where(npc => npc.IsTownNpc)
        .Select(MapNpc).ToArray();
    WorldFileNpcRecord[] savedNpcs = world.Npcs.Where(npc => !npc.IsTownNpc)
        .Select(MapNpc).ToArray();
    Add(sections, WorldFileNpcSection.SectionId, new WorldFileNpcSection(
        Array.Empty<int>(), townNpcs, savedNpcs));
    Add(sections, WorldFileTileEntitySection.SectionId,
        WorldFileTileEntityCodec.EncodeGenerated(world.TileEntities));
    Add(sections, WorldFilePressurePlateSection.SectionId, WorldFilePressurePlateSection.Empty);
    Add(sections, WorldFileTownManagerSection.SectionId, new WorldFileTownManagerSection(
        world.Npcs.Where(npc => !npc.Homeless).Select(npc => new WorldFileTownRoomRecord(
            npc.Type, npc.HomeX, npc.HomeY)).ToArray()));
    Add(sections, WorldFileBestiarySection.SectionId, WorldFileBestiarySection.Empty);
    Add(sections, WorldFileCreativePowersSection.SectionId, WorldFileCreativePowersSection.Empty);
    Add(sections, WorldFileFooterSection.SectionId,
        new WorldFileFooterSection(true, worldName, world.WorldId));
    return new WorldPersistenceDocument(319, sections);
  }

  private static WorldFileNpcRecord MapNpc(GeneratedNpc npc) {
    return new WorldFileNpcRecord(npc.Type, null, npc.IsTownNpc, npc.Name, npc.X, npc.Y,
        npc.Homeless, npc.HomeX, npc.HomeY, npc.IsTownNpc ? npc.Variation : null,
        npc.HomelessDespawn);
  }

  private static void Add<T>(List<WorldPersistenceSection> sections, string id, T value)
      where T : notnull {
    sections.Add(WorldPersistenceSection.Create(id, value));
  }
}
