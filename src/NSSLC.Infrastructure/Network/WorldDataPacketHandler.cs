using Terraria.Network;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;
using Terraria.NonAuthoritative.Persistence;
using Terraria.WorldStorage;

namespace NSSLC.Infrastructure.Network;

/// <summary>Projects a decoded, immutable world document into the real packet-7 response.</summary>
public sealed class WorldDataPacketHandler(WorldPersistenceDocument world,
    Func<NetworkSessionContext, bool> isCurrentSender)
    : IPacketHandler<RequestWorldDataPacket> {
  private readonly WorldPersistenceDocument _world = world
      ?? throw new ArgumentNullException(nameof(world));
  private readonly Func<NetworkSessionContext, bool> _isCurrentSender = isCurrentSender
      ?? throw new ArgumentNullException(nameof(isCurrentSender));

  public ValueTask<PacketHandlingResult> HandleAsync(NetworkSessionContext context,
      RequestWorldDataPacket packet, CancellationToken cancellationToken) {
    cancellationToken.ThrowIfCancellationRequested();
    if (context.Stage != NetworkSessionStage.AwaitPlayerData) {
      return ValueTask.FromResult(new PacketHandlingResult(false,
          rejectionCode: "InvalidWorldSessionStage"));
    }
    if (context.WorldRuntimeId is null || !_isCurrentSender(context)) {
      return ValueTask.FromResult(new PacketHandlingResult(false,
          rejectionCode: "StaleWorldSender"));
    }
    WorldDataPacket response = WorldDataPacketProjection.Create(_world);
    var dispatch = new OutboundDispatch(response, PacketDispatchKind.Single,
        [context.Connection], allowedStages: NetworkSessionStage.AwaitSectionRequest);
    return ValueTask.FromResult(new PacketHandlingResult(true, [dispatch],
        nextStage: NetworkSessionStage.AwaitSectionRequest));
  }
}

public static class WorldDataPacketProjection {
  public static WorldDataPacket Create(WorldPersistenceDocument document) {
    ArgumentNullException.ThrowIfNull(document);
    WorldFileHeaderSection header = Required<WorldFileHeaderSection>(document,
        WorldFileHeaderSection.SectionId);
    WorldFileEnvironmentSection environment = Required<WorldFileEnvironmentSection>(document,
        WorldFileEnvironmentSection.SectionId);
    WorldFileProgressionSection progression = Required<WorldFileProgressionSection>(document,
        WorldFileProgressionSection.SectionId);
    WorldFileBossProgressionSection boss = Required<WorldFileBossProgressionSection>(document,
        WorldFileBossProgressionSection.SectionId);
    WorldFilePartySection party = Required<WorldFilePartySection>(document,
        WorldFilePartySection.SectionId);
    WorldFileSandstormSection sandstorm = Required<WorldFileSandstormSection>(document,
        WorldFileSandstormSection.SectionId);
    WorldFileDefenderEventSection defender = Required<WorldFileDefenderEventSection>(document,
        WorldFileDefenderEventSection.SectionId);
    WorldFileEventSection events = Required<WorldFileEventSection>(document,
        WorldFileEventSection.SectionId);
    WorldFileSeasonalSection seasonal = Required<WorldFileSeasonalSection>(document,
        WorldFileSeasonalSection.SectionId);
    WorldFileNpcUnlockSection npcUnlocks = Required<WorldFileNpcUnlockSection>(document,
        WorldFileNpcUnlockSection.SectionId);
    WorldFileTimePolicySection timePolicy = Required<WorldFileTimePolicySection>(document,
        WorldFileTimePolicySection.SectionId);
    WorldFileBackgroundSection backgrounds = Required<WorldFileBackgroundSection>(document,
        WorldFileBackgroundSection.SectionId);
    WorldFileTreeTopsSection treeTops = Required<WorldFileTreeTopsSection>(document,
        WorldFileTreeTopsSection.SectionId);
    WorldFileSpawnSection spawn = Required<WorldFileSpawnSection>(document,
        WorldFileSpawnSection.SectionId);

    if (header.SkyblockWorld) {
      throw new NotSupportedException(
          "World-data synchronization for Skyblock worlds requires the low-tile state owner.");
    }

    byte[] flags = CreateWorldFlags(header, environment, progression, boss, party, sandstorm,
        defender, events, seasonal, npcUnlocks, timePolicy, spawn);
    byte[] networkBackgrounds = CreateBackgroundTypes(progression, backgrounds);
    return new WorldDataPacket {
      Time = checked((int)environment.Time),
      TimeFlags = CreateTimeFlags(environment),
      MoonPhase = checked((byte)environment.MoonPhase),
      MaxTilesX = checked((short)header.MaxTilesX),
      MaxTilesY = checked((short)header.MaxTilesY),
      SpawnTileX = checked((short)environment.SpawnTileX),
      SpawnTileY = checked((short)environment.SpawnTileY),
      WorldSurface = checked((short)environment.WorldSurface),
      RockLayer = checked((short)environment.RockLayer),
      WorldId = header.WorldId,
      WorldName = header.WorldName,
      GameMode = checked((byte)header.GameMode),
      WorldGuid = (header.UniqueId ?? throw new InvalidDataException(
          "The world header does not contain its unique identifier.")).ToByteArray(),
      WorldGeneratorVersion = header.WorldGeneratorVersion ?? throw new InvalidDataException(
          "The world header does not contain its generator version."),
      MoonType = environment.MoonType,
      BackgroundTypes = networkBackgrounds,
      SpecialBackgroundStyles = [
        checked((byte)environment.IceBackStyle),
        checked((byte)environment.JungleBackStyle),
        checked((byte)environment.HellBackStyle)
      ],
      WindSpeedTarget = progression.WindSpeedTarget,
      CloudCount = checked((byte)progression.CloudCount),
      TreePositions = environment.TreeX.ToArray(),
      TreeStyles = ToBytes(environment.TreeStyle),
      CaveBackgroundPositions = environment.CaveBackX.ToArray(),
      CaveBackgroundStyles = ToBytes(environment.CaveBackStyle),
      TreeTopStyles = ToBytes(treeTops.Variations),
      MaximumRain = progression.Raining ? progression.MaxRain : 0,
      WorldFlagGroups = flags,
      SundialCooldown = progression.SundialCooldown,
      MoondialCooldown = timePolicy.MoondialCooldown,
      SavedOreTiers = [
        checked((short)seasonal.CopperOreTier),
        checked((short)seasonal.IronOreTier),
        checked((short)seasonal.SilverOreTier),
        checked((short)seasonal.GoldOreTier),
        checked((short)progression.CobaltOreTier),
        checked((short)progression.MythrilOreTier),
        checked((short)progression.AdamantiteOreTier)
      ],
      InvasionType = checked((sbyte)progression.InvasionType),
      LobbyId = 0,
      SandstormSeverity = sandstorm.IntendedSeverity,
      ExtraSpawnPoints = spawn.ExtraSpawnPoints
          .Select(point => new Packet7ExtraSpawnPoint(point.X, point.Y)).ToArray(),
      DungeonX = checked((short)environment.DungeonX),
      DungeonY = checked((short)environment.DungeonY)
    };
  }

  private static byte[] CreateWorldFlags(WorldFileHeaderSection header,
      WorldFileEnvironmentSection environment, WorldFileProgressionSection progression,
      WorldFileBossProgressionSection boss, WorldFilePartySection party,
      WorldFileSandstormSection sandstorm, WorldFileDefenderEventSection defender,
      WorldFileEventSection events, WorldFileSeasonalSection seasonal,
      WorldFileNpcUnlockSection npcUnlocks, WorldFileTimePolicySection timePolicy,
      WorldFileSpawnSection spawn) {
    byte[] flags = new byte[11];
    Set(flags, 0, 0, progression.ShadowOrbSmashed);
    Set(flags, 0, 1, progression.DownedBoss1);
    Set(flags, 0, 2, progression.DownedBoss2);
    Set(flags, 0, 3, progression.DownedBoss3);
    Set(flags, 0, 4, progression.HardMode);
    Set(flags, 0, 5, progression.DownedClown);
    Set(flags, 0, 7, progression.DownedPlantBoss);

    Set(flags, 1, 0, progression.DownedMechBoss1);
    Set(flags, 1, 1, progression.DownedMechBoss2);
    Set(flags, 1, 2, progression.DownedMechBoss3);
    Set(flags, 1, 3, progression.DownedMechBossAny);
    Set(flags, 1, 4, progression.CloudBackgroundActive >= 1);
    Set(flags, 1, 5, environment.Crimson);

    Set(flags, 2, 1, boss.FastForwardTimeToDawn);
    Set(flags, 2, 2, progression.SlimeRainTime > 0);
    Set(flags, 2, 3, progression.DownedSlimeKing);
    Set(flags, 2, 4, progression.DownedQueenBee);
    Set(flags, 2, 5, boss.DownedFishron);
    Set(flags, 2, 6, boss.DownedMartians);
    Set(flags, 2, 7, boss.DownedAncientCultist);

    Set(flags, 3, 0, boss.DownedMoonlord);
    Set(flags, 3, 1, boss.DownedHalloweenKing);
    Set(flags, 3, 2, boss.DownedHalloweenTree);
    Set(flags, 3, 3, boss.DownedChristmasIceQueen);
    Set(flags, 3, 4, boss.DownedChristmasSantank);
    Set(flags, 3, 5, boss.DownedChristmasTree);
    Set(flags, 3, 6, progression.DownedGolemBoss);
    Set(flags, 3, 7, party.Manual || party.Genuine);

    Set(flags, 4, 0, progression.DownedPirates);
    Set(flags, 4, 1, progression.DownedFrost);
    Set(flags, 4, 2, progression.DownedGoblins);
    Set(flags, 4, 3, sandstorm.Happening);
    Set(flags, 4, 5, defender.DownedInvasionTier1);
    Set(flags, 4, 6, defender.DownedInvasionTier2);
    Set(flags, 4, 7, defender.DownedInvasionTier3);

    Set(flags, 5, 0, events.CombatBookWasUsed);
    Set(flags, 5, 1, events.LanternNightGenuine || events.LanternNightManual);
    Set(flags, 5, 2, boss.DownedTowerSolar);
    Set(flags, 5, 3, boss.DownedTowerVortex);
    Set(flags, 5, 4, boss.DownedTowerNebula);
    Set(flags, 5, 5, boss.DownedTowerStardust);
    Set(flags, 5, 6, seasonal.ForceHalloweenForToday);
    Set(flags, 5, 7, seasonal.ForceChristmasForToday);

    Set(flags, 6, 0, npcUnlocks.BoughtCat);
    Set(flags, 6, 1, npcUnlocks.BoughtDog);
    Set(flags, 6, 2, npcUnlocks.BoughtBunny);
    Set(flags, 6, 4, header.DrunkWorld);
    Set(flags, 6, 5, npcUnlocks.DownedEmpressOfLight);
    Set(flags, 6, 6, npcUnlocks.DownedQueenSlime);
    Set(flags, 6, 7, header.GetGoodWorld);

    Set(flags, 7, 0, header.TenthAnniversaryWorld);
    Set(flags, 7, 1, header.DontStarveWorld);
    Set(flags, 7, 2, npcUnlocks.DownedDeerclops);
    Set(flags, 7, 3, header.NotTheBeesWorld);
    Set(flags, 7, 4, header.RemixWorld);
    Set(flags, 7, 5, npcUnlocks.UnlockedSlimeBlueSpawn);
    Set(flags, 7, 6, npcUnlocks.CombatBookVolumeTwoWasUsed);
    Set(flags, 7, 7, npcUnlocks.PeddlersSatchelWasUsed);

    Set(flags, 8, 0, npcUnlocks.UnlockedSlimeGreenSpawn);
    Set(flags, 8, 1, npcUnlocks.UnlockedSlimeOldSpawn);
    Set(flags, 8, 2, npcUnlocks.UnlockedSlimePurpleSpawn);
    Set(flags, 8, 3, npcUnlocks.UnlockedSlimeRainbowSpawn);
    Set(flags, 8, 4, npcUnlocks.UnlockedSlimeRedSpawn);
    Set(flags, 8, 5, npcUnlocks.UnlockedSlimeYellowSpawn);
    Set(flags, 8, 6, npcUnlocks.UnlockedSlimeCopperSpawn);
    Set(flags, 8, 7, timePolicy.FastForwardTimeToDusk);

    Set(flags, 9, 0, header.NoTrapsWorld);
    Set(flags, 9, 1, header.ZenithWorld);
    Set(flags, 9, 2, npcUnlocks.UnlockedTruffleSpawn);
    Set(flags, 9, 3, timePolicy.VampireSeed);
    Set(flags, 9, 4, timePolicy.InfectedSeed);
    Set(flags, 9, 5, timePolicy.TeamBasedSpawnsSeed);
    Set(flags, 9, 7, spawn.DualDungeonsSeed);
    Set(flags, 10, 1, timePolicy.ForceHalloweenForever);
    Set(flags, 10, 2, timePolicy.ForceChristmasForever);
    Set(flags, 10, 3, spawn.MoreLightningSeed);
    Set(flags, 10, 4, spawn.NoLightningSeed);
    return flags;
  }

  private static byte[] CreateBackgroundTypes(WorldFileProgressionSection progression,
      WorldFileBackgroundSection additional) {
    if (progression.BackgroundStyles.Count != 8 || additional.Styles.Count != 5) {
      throw new InvalidDataException("The world background styles do not match the packet-7 layout.");
    }
    IReadOnlyList<byte> saved = progression.BackgroundStyles;
    IReadOnlyList<byte> extra = additional.Styles;
    return [saved[0], extra[2], extra[3], extra[4], saved[1], saved[2], saved[3], saved[4],
      saved[5], saved[6], saved[7], extra[0], extra[1]];
  }

  private static byte CreateTimeFlags(WorldFileEnvironmentSection environment) {
    byte flags = 0;
    if (environment.DayTime) flags |= 1;
    if (environment.BloodMoon) flags |= 2;
    if (environment.Eclipse) flags |= 4;
    return flags;
  }

  private static byte[] ToBytes(IReadOnlyList<int> values) =>
      values.Select(value => checked((byte)value)).ToArray();

  private static void Set(byte[] groups, int group, int bit, bool enabled) {
    if (enabled) groups[group] |= (byte)(1 << bit);
  }

  private static TSection Required<TSection>(WorldPersistenceDocument document, string sectionId)
      where TSection : notnull {
    if (!document.TryGetSection(sectionId, out WorldLoadSection<TSection> section)
        || !section.IsPresent) {
      throw new InvalidDataException($"World data is missing required section '{sectionId}'.");
    }
    return section.Value;
  }
}
