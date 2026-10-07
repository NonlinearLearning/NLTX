using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Terraria.NonAuthoritative.Persistence;
using Terraria.WorldGeneration.Components;
using Terraria.WorldGeneration.Systems;
using Terraria.WorldGeneration.Terrain.TreeTops;
using Terraria.WorldSession.Components;
using Terraria.WorldStorage;
using SessionOreTierState = Terraria.WorldSession.Components.OreTierState;
using SessionWorldEvilType = Terraria.WorldSession.Components.WorldEvilType;
using SessionWorldSecretSeedFlags = Terraria.WorldSession.Components.WorldSecretSeedFlags;

namespace Terraria.NonAuthoritative.WorldStorage;

/// <summary>
/// Captures the session-owned mutable WorldFile sections at a simulation boundary.
/// </summary>
public sealed class LoadedWorldPersistenceSnapshotSource : IWorldPersistenceSnapshotSource
{
  private readonly LoadedWorldSession _session;
  private readonly Func<LoadedWorldSession?> _activeSession;
  private readonly int _ownerThreadId;
  private readonly WorldFileTileCodec _tileCodec = new();
  private readonly WorldFileTileEntityCodec _tileEntityCodec = new();
  private readonly Func<IReadOnlyList<WorldNpcState>>? _runtimeNpcs;

  public LoadedWorldPersistenceSnapshotSource(
    LoadedWorldSession session,
    Func<LoadedWorldSession?> activeSession,
    Func<IReadOnlyList<WorldNpcState>>? runtimeNpcs = null)
  {
    _session = session ?? throw new ArgumentNullException(nameof(session));
    _activeSession = activeSession ?? throw new ArgumentNullException(nameof(activeSession));
    _runtimeNpcs = runtimeNpcs;
    _ownerThreadId = Environment.CurrentManagedThreadId;
  }

  public WorldPersistenceDocument Capture(CancellationToken cancellationToken)
  {
    EnsureOwnerThread();
    cancellationToken.ThrowIfCancellationRequested();
    // The I/O lease may be released after a failed reset; lifecycle readiness is separate.
    if (!ReferenceEquals(_activeSession(), _session) ||
        !_session.IsComplete || !_session.IsPublished || _session.IsPublicationUncertain ||
        _session.Lifecycle.RecoveryPhase != WorldLoadRecoveryPhase.Completed ||
        NSSLC.WorldGeneration.WorldGen.isGeneratingOrLoadingWorld ||
        _session.Lifecycle.IsGeneratingOrLoadingWorld ||
        _session.Lifecycle.LoadFailed ||
        _session.Lifecycle.RequiresWorldReset ||
        _session.SourceDocument is not WorldPersistenceDocument source)
    {
      throw new InvalidOperationException(
        "A runtime save requires the active, ready published session with its source document retained.");
    }

    _ = Require<WorldFileEnvironmentSection>(source, WorldFileEnvironmentSection.SectionId);
    var replacements = new List<WorldPersistenceSection>(16)
    {
      WorldPersistenceSection.Create(
        WorldFileTilePayloadSection.SectionId,
        _tileCodec.Encode(
          _session.Storage.TileMap.CreateSnapshot(),
          _session.FrameImportant,
          cancellationToken)),
      WorldPersistenceSection.Create(
        WorldFileEnvironmentSection.SectionId,
        CaptureEnvironment()),
      WorldPersistenceSection.Create(
        WorldFileNpcSection.SectionId,
        CaptureNpcs()),
      WorldPersistenceSection.Create(
        WorldFileChestSection.SectionId,
        CaptureChests()),
      WorldPersistenceSection.Create(
        WorldFileSignSection.SectionId,
        CaptureSigns()),
      WorldPersistenceSection.Create(
        WorldFileTileEntitySection.SectionId,
        CaptureTileEntities(cancellationToken)),
    };

    CaptureQuest(replacements);
    CaptureProgression(replacements);
    CaptureBossProgression(replacements);
    CaptureBanner(replacements);
    CaptureBestiary(replacements);
    CaptureNpcUnlocks(replacements);
    CaptureEvent(replacements);
    CaptureDefenderEvent(replacements);
    CaptureParty(replacements);
    CapturePressurePlates(replacements);
    CaptureSeasonal(replacements);
    CaptureSpawn(source, replacements);
    CaptureTimePolicy(replacements);
    CaptureSandstorm(replacements);
    CaptureTownManager(replacements);
    CaptureTreeTops(replacements);
    CaptureBackgrounds(replacements);
    cancellationToken.ThrowIfCancellationRequested();
    return source.WithReplacements(replacements.ToArray());
  }

  private WorldFileEnvironmentSection CaptureEnvironment()
  {
    WorldSessionRestoreState world = _session.World;
    WorldAppearanceStateComponent appearance = world.Appearance;
    WorldTimeWeatherState time = _session.World.TimeWeather;
    return new WorldFileEnvironmentSection(
      appearance.MoonType,
      appearance.TreeX,
      appearance.TreeStyle,
      appearance.CaveBackX,
      appearance.CaveBackStyle,
      appearance.IceBackStyle,
      appearance.JungleBackStyle,
      appearance.HellBackStyle,
      world.Descriptor.SpawnTileX,
      world.Descriptor.SpawnTileY,
      world.Descriptor.SurfaceLayer,
      world.Descriptor.RockLayer,
      time.Time,
      time.DayTime,
      time.MoonPhase,
      time.BloodMoon,
      time.Eclipse,
      world.Descriptor.DungeonTileX,
      world.Descriptor.DungeonTileY,
      world.Rules.WorldEvil == SessionWorldEvilType.Crimson);
  }

  private WorldFileNpcSection CaptureNpcs()
  {
    IReadOnlyList<int> shimmered = _session.World.History.ShimmeredTownNpcIds;
    var townNpcs = new List<WorldFileNpcRecord>();
    var savedNpcs = new List<WorldFileNpcRecord>();
    if (_runtimeNpcs is not null)
    {
      foreach (WorldNpcState npc in _runtimeNpcs())
      {
        AddRecord(npc, townNpcs, savedNpcs);
      }
    }
    else
    {
      for (int index = 0; index < _session.Storage.Npcs.Capacity; index++)
      {
        if (!_session.Storage.Npcs.TryGetOccupiedAt(
              index,
              out _,
              out _,
              out WorldEntityState? entity))
        {
          continue;
        }

        if (entity is not WorldNpcState npc)
        {
          throw new InvalidDataException(
            "The world NPC store contains an unsupported entity state.");
        }

        AddRecord(npc, townNpcs, savedNpcs);
      }
    }

    return new WorldFileNpcSection(shimmered, townNpcs, savedNpcs);
  }

  private static void AddRecord(
    WorldNpcState npc,
    List<WorldFileNpcRecord> townNpcs,
    List<WorldFileNpcRecord> savedNpcs)
  {
    var record = new WorldFileNpcRecord(
      npc.NetId,
      npc.LegacyTypeName,
      npc.IsTownNpc,
      npc.Name,
      npc.X,
      npc.Y,
      npc.Homeless,
      npc.Home.X,
      npc.Home.Y,
      npc.Variation,
      npc.HomelessDespawn);
    (npc.IsTownNpc ? townNpcs : savedNpcs).Add(record);
  }

  private WorldFileChestSection CaptureChests()
  {
    IReadOnlyList<WorldChestSnapshot> snapshots = _session.Storage.WorldContainers.CreateSnapshot();
    var records = new List<WorldFileChestRecord>(snapshots.Count);
    foreach (WorldChestSnapshot chest in snapshots)
    {
      var items = new List<WorldFileChestItem>(chest.Items.Count);
      foreach (Terraria.Items.ItemState item in chest.Items)
      {
        items.Add(new WorldFileChestItem(item.Stack, item.Type, item.Prefix));
      }

      records.Add(new WorldFileChestRecord(
        chest.Anchor.X,
        chest.Anchor.Y,
        chest.Name,
        items));
    }

    return new WorldFileChestSection(records);
  }

  private WorldFileSignSection CaptureSigns()
  {
    IReadOnlyList<WorldSignSnapshot> snapshots = _session.Storage.WorldSigns.CreateSnapshot();
    var records = new List<WorldFileSignRecord>(snapshots.Count);
    foreach (WorldSignSnapshot sign in snapshots)
    {
      records.Add(new WorldFileSignRecord(sign.Text, sign.Anchor.X, sign.Anchor.Y));
    }

    return new WorldFileSignSection(records);
  }

  private WorldFileTileEntitySection CaptureTileEntities(CancellationToken cancellationToken)
  {
    return _tileEntityCodec.Encode(
      _session.Storage.TileEntities.CreateSnapshot(),
      cancellationToken);
  }

  private void CaptureProgression(
    List<WorldPersistenceSection> replacements)
  {
    WorldSessionRestoreState world = _session.World;
    WorldTimeWeatherState time = world.TimeWeather;
    WorldEventProgressState state = world.Progression;
    BossProgressFlags bosses = state.Bosses;
    SavedNpcProgressFlags savedNpcs = state.SavedNpcs;
    InvasionProgressFlags invasions = state.Invasions;
    InvasionRuntimeState invasion = state.Invasion;
    WorldMilestoneStateComponent milestones = world.Milestones;
    SessionOreTierState oreTiers = world.Rules.SavedOreTiers;
    WorldAppearanceStateComponent appearance = world.Appearance;
    replacements.Add(WorldPersistenceSection.Create(
      WorldFileProgressionSection.SectionId,
      new WorldFileProgressionSection(
        downedBoss1: bosses.Boss1,
        downedBoss2: bosses.Boss2,
        downedBoss3: bosses.Boss3,
        downedQueenBee: bosses.QueenBee,
        downedMechBoss1: bosses.MechBoss1,
        downedMechBoss2: bosses.MechBoss2,
        downedMechBoss3: bosses.MechBoss3,
        downedMechBossAny: milestones.AnyMechBossDowned,
        downedPlantBoss: bosses.PlantBoss,
        downedGolemBoss: bosses.GolemBoss,
        downedSlimeKing: bosses.SlimeKing,
        savedGoblin: savedNpcs.Goblin,
        savedWizard: savedNpcs.Wizard,
        savedMech: savedNpcs.Mechanic,
        downedGoblins: invasions.Goblins,
        downedClown: invasions.Clown,
        downedFrost: invasions.Frost,
        downedPirates: invasions.Pirates,
        shadowOrbSmashed: milestones.ShadowOrbSmashed,
        spawnMeteor: state.PendingEvents.SpawnMeteor,
        shadowOrbCount: milestones.ShadowOrbCount,
        altarCount: milestones.AltarCount,
        hardMode: world.Rules.HardMode,
        afterPartyOfDoom: state.PendingEvents.AfterPartyOfDoom,
        invasionDelay: invasion.Delay,
        invasionSize: invasion.Size,
        invasionType: (int)invasion.Type,
        invasionX: invasion.PositionX,
        slimeRainTime: time.SlimeRainTime,
        sundialCooldown: checked((byte)time.SundialCooldown),
        raining: time.Raining,
        rainTime: time.RainTime,
        maxRain: time.RainStrength,
        cobaltOreTier: oreTiers.Cobalt,
        mythrilOreTier: oreTiers.Mythril,
        adamantiteOreTier: oreTiers.Adamantite,
        backgroundStyles: appearance.BackgroundStyles,
        cloudBackgroundActive: appearance.CloudBackgroundActive,
        cloudCount: appearance.CloudCount,
        windSpeedTarget: time.WindTarget)));
  }

  private void CaptureTimePolicy(
    List<WorldPersistenceSection> replacements)
  {
    WorldTimeWeatherState time = _session.World.TimeWeather;
    WorldSessionRestoreState world = _session.World;
    SessionWorldSecretSeedFlags seeds = world.Rules.SecretSeeds;
    replacements.Add(WorldPersistenceSection.Create(
      WorldFileTimePolicySection.SectionId,
      new WorldFileTimePolicySection(
        time.FastForwardTimeToDusk,
        checked((byte)time.MoondialCooldown),
        world.SeasonPolicy.ForceHalloweenForever,
        world.SeasonPolicy.ForceChristmasForever,
        (seeds & SessionWorldSecretSeedFlags.Vampire) != 0,
        (seeds & SessionWorldSecretSeedFlags.Infected) != 0,
        world.Progression.PendingEvents.MeteorShowerCount,
        time.CoinRain,
        (seeds & SessionWorldSecretSeedFlags.TeamBasedSpawns) != 0)));
  }

  private void CaptureBanner(
    List<WorldPersistenceSection> replacements)
  {
    WorldNpcHistoryStateComponent history = _session.World.History;
    replacements.Add(WorldPersistenceSection.Create(
      WorldFileBannerSection.SectionId,
      new WorldFileBannerSection(history.BannerKillCounts, history.BannerClaimableCounts)));
  }

  private void CaptureBestiary(
    List<WorldPersistenceSection> replacements)
  {
    WorldNpcHistoryStateComponent history = _session.World.History;
    var killCounts = new List<WorldFileBestiaryKillCount>(history.BestiaryKillCounts.Count);
    foreach (KeyValuePair<string, int> entry in history.BestiaryKillCounts.OrderBy(
               entry => entry.Key,
               StringComparer.Ordinal))
    {
      killCounts.Add(new WorldFileBestiaryKillCount(entry.Key, entry.Value));
    }

    replacements.Add(WorldPersistenceSection.Create(
      WorldFileBestiarySection.SectionId,
      new WorldFileBestiarySection(killCounts, history.SeenNpcIds, history.ChattedNpcIds)));
  }

  private void CaptureQuest(
    List<WorldPersistenceSection> replacements)
  {
    WorldSessionRestoreState world = _session.World;
    SavedNpcProgressFlags savedNpcs = world.Progression.SavedNpcs;
    replacements.Add(WorldPersistenceSection.Create(
      WorldFileQuestSection.SectionId,
      new WorldFileQuestSection(
        world.History.AnglerWhoFinishedToday,
        savedNpcs.Angler,
        world.History.AnglerQuest,
        savedNpcs.Stylist,
        savedNpcs.TaxCollector,
        savedNpcs.Golfer,
        world.Progression.Invasion.SizeStart,
        world.TimeWeather.CultistDelay)));
  }

  private void CaptureBossProgression(
    List<WorldPersistenceSection> replacements)
  {
    WorldSessionRestoreState world = _session.World;
    WorldEventProgressState progression = world.Progression;
    BossProgressFlags bosses = progression.Bosses;
    InvasionProgressFlags invasions = progression.Invasions;
    LunarProgressState lunar = progression.Lunar;
    replacements.Add(WorldPersistenceSection.Create(
      WorldFileBossProgressionSection.SectionId,
      new WorldFileBossProgressionSection(
        world.TimeWeather.FastForwardTimeToDawn,
        bosses.Fishron,
        invasions.Martians,
        bosses.AncientCultist,
        bosses.Moonlord,
        bosses.HalloweenKing,
        bosses.HalloweenTree,
        bosses.ChristmasIceQueen,
        bosses.ChristmasSantank,
        bosses.ChristmasTree,
        lunar.DownedSolarTower,
        lunar.DownedVortexTower,
        lunar.DownedNebulaTower,
        lunar.DownedStardustTower,
        lunar.SolarTowerActive,
        lunar.VortexTowerActive,
        lunar.NebulaTowerActive,
        lunar.StardustTowerActive,
        lunar.LunarApocalypseIsUp)));
  }

  private void CaptureNpcUnlocks(
    List<WorldPersistenceSection> replacements)
  {
    WorldEventProgressState progression = _session.World.Progression;
    NpcWorldUnlockFlags worldUnlocks = progression.NpcWorldUnlocks;
    NpcSpawnUnlockFlags spawns = progression.UnlockedNpcSpawns;
    BossProgressFlags bosses = progression.Bosses;
    replacements.Add(WorldPersistenceSection.Create(
      WorldFileNpcUnlockSection.SectionId,
      new WorldFileNpcUnlockSection(
        worldUnlocks.BoughtCat,
        worldUnlocks.BoughtDog,
        worldUnlocks.BoughtBunny,
        bosses.EmpressOfLight,
        bosses.QueenSlime,
        bosses.Deerclops,
        spawns.SlimeBlue,
        spawns.Merchant,
        spawns.Demolitionist,
        spawns.PartyGirl,
        spawns.DyeTrader,
        spawns.Truffle,
        spawns.ArmsDealer,
        spawns.Nurse,
        spawns.Princess,
        worldUnlocks.CombatBookVolumeTwoUsed,
        worldUnlocks.PeddlersSatchelUsed,
        spawns.SlimeGreen,
        spawns.SlimeOld,
        spawns.SlimePurple,
        spawns.SlimeRainbow,
        spawns.SlimeRed,
        spawns.SlimeYellow,
        spawns.SlimeCopper)));
  }

  private void CaptureEvent(
    List<WorldPersistenceSection> replacements)
  {
    WorldSessionRestoreState world = _session.World;
    LanternNightState lanternNight = world.TimeWeather.LanternNight;
    replacements.Add(WorldPersistenceSection.Create(
      WorldFileEventSection.SectionId,
      new WorldFileEventSection(
        world.Progression.NpcWorldUnlocks.CombatBookUsed,
        lanternNight.LanternNightsOnCooldown,
        lanternNight.GenuineLanterns,
        lanternNight.ManualLanterns,
        lanternNight.NextNightIsLanternNight)));
  }

  private void CaptureDefenderEvent(
    List<WorldPersistenceSection> replacements)
  {
    WorldEventProgressState progression = _session.World.Progression;
    replacements.Add(WorldPersistenceSection.Create(
      WorldFileDefenderEventSection.SectionId,
      new WorldFileDefenderEventSection(
        progression.SavedNpcs.Bartender,
        progression.Dd2.DownedTier1,
        progression.Dd2.DownedTier2,
        progression.Dd2.DownedTier3)));
  }

  private void CaptureParty(
    List<WorldPersistenceSection> replacements)
  {
    BirthdayPartyState party = _session.World.TimeWeather.BirthdayParty;
    replacements.Add(WorldPersistenceSection.Create(
      WorldFilePartySection.SectionId,
      new WorldFilePartySection(
        party.ManualParty,
        party.GenuineParty,
        party.PartyDaysOnCooldown,
        party.CelebratingNpcIds)));
  }

  private void CapturePressurePlates(
    List<WorldPersistenceSection> replacements)
  {
    IReadOnlyList<TileCoordinate> anchors = _session.Storage.PressurePlates.Anchors;
    var plates = new List<WorldFilePressurePlateRecord>(anchors.Count);
    foreach (TileCoordinate anchor in anchors)
    {
      plates.Add(new WorldFilePressurePlateRecord(anchor.X, anchor.Y));
    }

    replacements.Add(WorldPersistenceSection.Create(
      WorldFilePressurePlateSection.SectionId,
      new WorldFilePressurePlateSection(plates)));
  }

  private void CaptureSeasonal(
    List<WorldPersistenceSection> replacements)
  {
    WorldSessionRestoreState world = _session.World;
    PendingWorldEventsState pending = world.Progression.PendingEvents;
    SessionOreTierState oreTiers = world.Rules.SavedOreTiers;
    replacements.Add(WorldPersistenceSection.Create(
      WorldFileSeasonalSection.SectionId,
      new WorldFileSeasonalSection(
        pending.ForceHalloweenForToday,
        pending.ForceChristmasForToday,
        oreTiers.Copper,
        oreTiers.Iron,
        oreTiers.Silver,
        oreTiers.Gold)));
  }

  private void CaptureSpawn(
    WorldPersistenceDocument source,
    List<WorldPersistenceSection> replacements)
  {
    WorldFileSpawnSection saved = Optional<WorldFileSpawnSection>(
      source,
      WorldFileSpawnSection.SectionId) ?? WorldFileSpawnSection.Empty;

    WorldSessionRestoreState world = _session.World;
    var extraSpawnPoints = new List<WorldFileExtraSpawnPoint>(
      world.Descriptor.ExtraSpawnPoints.Count);
    foreach (TileCoordinate point in world.Descriptor.ExtraSpawnPoints)
    {
      extraSpawnPoints.Add(new WorldFileExtraSpawnPoint(
        checked((short)point.X),
        checked((short)point.Y)));
    }

    replacements.Add(WorldPersistenceSection.Create(
      WorldFileSpawnSection.SectionId,
      new WorldFileSpawnSection(
        extraSpawnPoints,
        (world.Rules.SecretSeeds & SessionWorldSecretSeedFlags.DualDungeons) != 0,
        saved.MoreLightningSeed,
        saved.NoLightningSeed,
        _session.ManifestJson)));
  }

  private void CaptureTownManager(
    List<WorldPersistenceSection> replacements)
  {
    IReadOnlyList<KeyValuePair<TownHousingResidentKey, TilePosition>> assignments =
      TownHousingRegistrySystem.GetRoomAssignmentsSnapshot(_session.TownHousing);
    var rooms = new List<WorldFileTownRoomRecord>(assignments.Count);
    foreach (KeyValuePair<TownHousingResidentKey, TilePosition> assignment in assignments)
    {
      rooms.Add(new WorldFileTownRoomRecord(
        assignment.Key.NpcType,
        assignment.Value.X,
        assignment.Value.Y));
    }

    replacements.Add(WorldPersistenceSection.Create(
      WorldFileTownManagerSection.SectionId,
      new WorldFileTownManagerSection(rooms)));
  }

  private void CaptureTreeTops(
    List<WorldPersistenceSection> replacements)
  {
    WorldTreeTopsStateSnapshot treeTops = _session.TreeTops.CreateSnapshot();
    replacements.Add(WorldPersistenceSection.Create(
      WorldFileTreeTopsSection.SectionId,
      new WorldFileTreeTopsSection(treeTops.Variations)));
  }

  private void CaptureBackgrounds(
    List<WorldPersistenceSection> replacements)
  {
    replacements.Add(WorldPersistenceSection.Create(
      WorldFileBackgroundSection.SectionId,
      new WorldFileBackgroundSection(_session.World.Appearance.AdditionalBackgroundStyles)));
  }

  private void CaptureSandstorm(
    List<WorldPersistenceSection> replacements)
  {
    WorldTimeWeatherState time = _session.World.TimeWeather;
    replacements.Add(WorldPersistenceSection.Create(
      WorldFileSandstormSection.SectionId,
      new WorldFileSandstormSection(
        time.Sandstorm.Happening,
        time.Sandstorm.TimeLeft,
        time.Sandstorm.Severity,
        time.Sandstorm.IntendedSeverity)));
  }

  private static TSection Require<TSection>(WorldPersistenceDocument document, string sectionId)
    where TSection : notnull
  {
    return document.TryGetSection<TSection>(sectionId, out WorldLoadSection<TSection> section) &&
      section.IsPresent
        ? section.Value
        : throw new InvalidDataException($"The loaded world has no '{sectionId}' section.");
  }

  private static TSection? Optional<TSection>(WorldPersistenceDocument document, string sectionId)
    where TSection : class
  {
    return document.TryGetSection<TSection>(sectionId, out WorldLoadSection<TSection> section) &&
      section.IsPresent
        ? section.Value
        : null;
  }

  private void EnsureOwnerThread()
  {
    if (Environment.CurrentManagedThreadId != _ownerThreadId)
    {
      throw new InvalidOperationException(
        "World save snapshots must be captured on the session's owner thread.");
    }
  }
}
