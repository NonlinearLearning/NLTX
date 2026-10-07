using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Terraria.NonAuthoritative.Persistence;

namespace Terraria.NonAuthoritative.WorldStorage;

/// <summary>
/// Decodes bounded WorldFile sections into an application persistence document.
/// </summary>
/// <remarks>
/// The legacy world reader also mutates global runtime state and reads later sections. This
/// decoder reads the supported header/environment/progression/quest/banner/boss-progression/
/// party/sandstorm/defender-event/background/event/tree-tops/seasonal/npc-unlocks/time-policy/
/// spawn prefix and the bounded Tile payload, Chest/Sign/NPC/WeightedPressurePlates/TownManager/
/// Bestiary/Footer sections. TileEntity and CreativePowers are retained as bounded raw payloads.
/// It does not expand Tile into runtime objects, read an exploration map, or write any ECS store.
/// More section decoders can be added behind the same application port.
/// </remarks>
public sealed class WorldFileDocumentDecoder : IWorldPersistenceDocumentDecoder
{
  private static readonly Encoding Utf8 = new UTF8Encoding(false, true);

  public WorldPersistenceDecodeResult Decode(ReadOnlyMemory<byte> fileBytes)
  {
    if (fileBytes.IsEmpty)
    {
      return Failed("The world file is empty.");
    }

    try
    {
      using MemoryStream stream = new(fileBytes.ToArray(), writable: false);
      using BinaryReader reader = new(stream, Utf8, leaveOpen: true);
      int version = reader.ReadInt32();
      if (version < WorldFileFormatConstants.MinimumVersion ||
          version > WorldFileFormatConstants.LatestReadableVersion)
      {
        return Failed($"Unsupported world file version {version}.");
      }

      WorldFileMetadataSection? metadata = version >= WorldFileFormatConstants.MetadataVersion
        ? ReadAndValidateMetadata(reader)
        : null;

      int[] sectionPointers = ReadSectionPointers(reader, stream.Length);
      if (sectionPointers.Length < 2)
      {
        return Failed("The world file does not contain a bounded header section.");
      }

      if (sectionPointers.Length != WorldFileFormatConstants.MaxSectionPointers)
      {
        return Failed(
          $"The world file must contain exactly " +
          $"{WorldFileFormatConstants.MaxSectionPointers} section pointers.");
      }

      bool[] frameImportant = ReadImportanceTable(reader, stream.Length);
      int headerStart = sectionPointers[0];
      int headerEnd = sectionPointers[1];
      if (headerStart != stream.Position || headerStart < 0 || headerEnd <= headerStart ||
          headerEnd > stream.Length)
      {
        return Failed("The world file header section pointers are invalid.");
      }

      stream.Position = headerStart;
      WorldFileHeaderSection header = ReadHeader(reader, version, headerEnd);
      WorldFileEnvironmentSection? environment = ReadEnvironment(reader, headerEnd);
      WorldFileProgressionSection? progression = ReadProgression(reader, version, headerEnd);
      WorldFileQuestSection? quests = ReadQuests(
        reader,
        version,
        headerEnd,
        progression);
      WorldFileBannerSection? banners = ReadBanners(reader, version, headerEnd);
      WorldFileBossProgressionSection? bossProgression =
        ReadBossProgression(reader, version, headerEnd);
      WorldFilePartySection? party = ReadParty(reader, version, headerEnd);
      WorldFileSandstormSection? sandstorm = ReadSandstorm(reader, version, headerEnd);
      WorldFileDefenderEventSection? defenderEvent =
        ReadDefenderEvent(reader, version, headerEnd);
      WorldFileBackgroundSection? backgrounds = ReadBackgrounds(reader, version, headerEnd);
      WorldFileEventSection? events = ReadEvents(reader, version, headerEnd);
      WorldFileTreeTopsSection? treeTops = ReadTreeTops(reader, version, headerEnd);
      WorldFileSeasonalSection? seasonal = ReadSeasonal(reader, version, headerEnd);
      WorldFileNpcUnlockSection? npcUnlocks = ReadNpcUnlocks(reader, version, headerEnd);
      WorldFileTimePolicySection? timePolicy = ReadTimePolicy(reader, version, headerEnd);
      WorldFileSpawnSection? spawn = ReadSpawnSection(reader, version, headerEnd);
      EnsureSectionConsumed(reader, headerEnd, "header");
      WorldFileTilePayloadSection? tilePayload = ReadTilePayload(
        reader,
        sectionPointers,
        stream.Length,
        header,
        frameImportant);
      WorldFileChestSection? chests = ReadChests(reader, version, sectionPointers, stream.Length);
      WorldFileSignSection? signs = ReadSigns(reader, sectionPointers, stream.Length);
      WorldFileNpcSection? npcs = ReadNpcs(reader, version, sectionPointers, stream.Length);
      WorldFileTileEntitySection? tileEntities = ReadTileEntities(
        reader,
        version,
        sectionPointers,
        stream.Length);
      WorldFilePressurePlateSection? pressurePlates = ReadPressurePlates(
        reader,
        version,
        sectionPointers,
        stream.Length);
      WorldFileTownManagerSection? townManager = ReadTownManager(
        reader,
        version,
        sectionPointers,
        stream.Length);
      WorldFileBestiarySection? bestiary = ReadBestiary(
        reader,
        version,
        sectionPointers,
        stream.Length);
      WorldFileCreativePowersSection? creativePowers = ReadCreativePowers(
        reader,
        version,
        sectionPointers,
        stream.Length);
      WorldFileFooterSection? footer = ReadFooter(reader, sectionPointers, stream.Length);
      var sections = new List<WorldPersistenceSection>
      {
        WorldPersistenceSection.Create(WorldFileHeaderSection.SectionId, header)
      };
      if (metadata is not null)
      {
        sections.Add(WorldPersistenceSection.Create(
          WorldFileMetadataSection.SectionId,
          metadata));
      }
      if (environment is not null)
      {
        sections.Add(WorldPersistenceSection.Create(
          WorldFileEnvironmentSection.SectionId,
          environment));
      }

      if (progression is not null)
      {
        sections.Add(WorldPersistenceSection.Create(
          WorldFileProgressionSection.SectionId,
          progression));
      }

      if (quests is not null)
      {
        sections.Add(WorldPersistenceSection.Create(
          WorldFileQuestSection.SectionId,
          quests));
      }

      if (banners is not null)
      {
        sections.Add(WorldPersistenceSection.Create(
          WorldFileBannerSection.SectionId,
          banners));
      }

      if (bossProgression is not null)
      {
        sections.Add(WorldPersistenceSection.Create(
          WorldFileBossProgressionSection.SectionId,
          bossProgression));
      }

      if (party is not null)
      {
        sections.Add(WorldPersistenceSection.Create(
          WorldFilePartySection.SectionId,
          party));
      }

      if (sandstorm is not null)
      {
        sections.Add(WorldPersistenceSection.Create(
          WorldFileSandstormSection.SectionId,
          sandstorm));
      }

      if (defenderEvent is not null)
      {
        sections.Add(WorldPersistenceSection.Create(
          WorldFileDefenderEventSection.SectionId,
          defenderEvent));
      }

      if (backgrounds is not null)
      {
        sections.Add(WorldPersistenceSection.Create(
          WorldFileBackgroundSection.SectionId,
          backgrounds));
      }

      if (events is not null)
      {
        sections.Add(WorldPersistenceSection.Create(
          WorldFileEventSection.SectionId,
          events));
      }

      if (treeTops is not null)
      {
        sections.Add(WorldPersistenceSection.Create(
          WorldFileTreeTopsSection.SectionId,
          treeTops));
      }

      if (seasonal is not null)
      {
        sections.Add(WorldPersistenceSection.Create(
          WorldFileSeasonalSection.SectionId,
          seasonal));
      }

      if (npcUnlocks is not null)
      {
        sections.Add(WorldPersistenceSection.Create(
          WorldFileNpcUnlockSection.SectionId,
          npcUnlocks));
      }

      if (timePolicy is not null)
      {
        sections.Add(WorldPersistenceSection.Create(
          WorldFileTimePolicySection.SectionId,
          timePolicy));
      }

      if (spawn is not null)
      {
        sections.Add(WorldPersistenceSection.Create(
          WorldFileSpawnSection.SectionId,
          spawn));
      }

      if (tilePayload is not null)
      {
        sections.Add(WorldPersistenceSection.Create(
          WorldFileTilePayloadSection.SectionId,
          tilePayload));
      }

      if (chests is not null)
      {
        sections.Add(WorldPersistenceSection.Create(
          WorldFileChestSection.SectionId,
          chests));
      }

      if (signs is not null)
      {
        sections.Add(WorldPersistenceSection.Create(
          WorldFileSignSection.SectionId,
          signs));
      }

      if (npcs is not null)
      {
        sections.Add(WorldPersistenceSection.Create(
          WorldFileNpcSection.SectionId,
          npcs));
      }

      if (tileEntities is not null)
      {
        sections.Add(WorldPersistenceSection.Create(
          WorldFileTileEntitySection.SectionId,
          tileEntities));
      }

      if (pressurePlates is not null)
      {
        sections.Add(WorldPersistenceSection.Create(
          WorldFilePressurePlateSection.SectionId,
          pressurePlates));
      }

      if (townManager is not null)
      {
        sections.Add(WorldPersistenceSection.Create(
          WorldFileTownManagerSection.SectionId,
          townManager));
      }

      if (bestiary is not null)
      {
        sections.Add(WorldPersistenceSection.Create(
          WorldFileBestiarySection.SectionId,
          bestiary));
      }

      if (creativePowers is not null)
      {
        sections.Add(WorldPersistenceSection.Create(
          WorldFileCreativePowersSection.SectionId,
          creativePowers));
      }

      if (footer is not null)
      {
        sections.Add(WorldPersistenceSection.Create(
          WorldFileFooterSection.SectionId,
          footer));
      }

      WorldPersistenceDocument document = new(
        version,
        sections);
      return WorldPersistenceDecodeResult.Decoded(document);
    }
    catch (EndOfStreamException exception)
    {
      return Failed("The world file ended before its header was complete.", exception);
    }
    catch (DecoderFallbackException exception)
    {
      return Failed("The world file contains invalid UTF-8 text.", exception);
    }
    catch (InvalidDataException exception)
    {
      return Failed(exception.Message, exception);
    }
    catch (IOException exception)
    {
      return Failed("The world file could not be read.", exception);
    }
    catch (ArgumentException exception)
    {
      return Failed("The world file contains invalid header data.", exception);
    }
  }

  private static WorldFileMetadataSection ReadAndValidateMetadata(BinaryReader reader)
  {
    ulong metadata = reader.ReadUInt64();
    if ((metadata & 0x00FFFFFFFFFFFFFFUL) != WorldFileFormatConstants.MetadataMagic ||
        (metadata >> 56) != WorldFileFormatConstants.WorldFileType)
    {
      throw new InvalidDataException("The world file metadata is not a World file header.");
    }

    uint revision = reader.ReadUInt32();
    ulong flags = reader.ReadUInt64();
    return new WorldFileMetadataSection(revision, (flags & 1UL) != 0);
  }

  private static int[] ReadSectionPointers(BinaryReader reader, long streamLength)
  {
    short sectionCount = reader.ReadInt16();
    if (sectionCount != WorldFileFormatConstants.MaxSectionPointers)
    {
      throw new InvalidDataException("The world file section pointer count is invalid.");
    }

    int[] pointers = new int[sectionCount];
    int previous = 0;
    for (int index = 0; index < pointers.Length; index++)
    {
      int pointer = reader.ReadInt32();
      if (pointer < 0 || pointer > streamLength || (index > 0 && pointer < previous))
      {
        throw new InvalidDataException("The world file contains an invalid section pointer.");
      }

      pointers[index] = pointer;
      previous = pointer;
    }

    return pointers;
  }

  private static bool[] ReadImportanceTable(BinaryReader reader, long streamLength)
  {
    ushort importanceCount = ReadBoundedUInt16(reader, streamLength);
    if (importanceCount == 0)
    {
      throw new InvalidDataException(
        "The world file does not contain a Tile frame-importance table.");
    }

    int packedByteCount = (importanceCount + 7) / 8;
    EnsureAvailable(reader, packedByteCount, streamLength);
    bool[] frameImportant = new bool[importanceCount];
    for (int index = 0; index < frameImportant.Length; index++)
    {
      if (index % 8 == 0)
      {
        byte packed = reader.ReadByte();
        for (int bit = 0; bit < 8 && index + bit < frameImportant.Length; bit++)
        {
          frameImportant[index + bit] = (packed & (1 << bit)) != 0;
        }
      }
    }

    return frameImportant;
  }

  private static WorldFileHeaderSection ReadHeader(
    BinaryReader reader,
    int version,
    long headerEnd)
  {
    string worldName = ReadBoundedString(reader, headerEnd);
    string? seedText = null;
    ulong? generatorVersion = null;
    if (version >= WorldFileFormatConstants.SeedVersion)
    {
      seedText = version == WorldFileFormatConstants.SeedVersion
        ? reader.ReadInt32().ToString(CultureInfo.InvariantCulture)
        : ReadBoundedString(reader, headerEnd);
      generatorVersion = ReadBoundedUInt64(reader, headerEnd);
    }

    Guid? uniqueId = version >= WorldFileFormatConstants.UniqueIdVersion
      ? new Guid(ReadBoundedBytes(reader, 16, headerEnd))
      : null;
    int worldId = ReadBoundedInt32(reader, headerEnd);
    int leftWorld = ReadBoundedInt32(reader, headerEnd);
    int rightWorld = ReadBoundedInt32(reader, headerEnd);
    int topWorld = ReadBoundedInt32(reader, headerEnd);
    int bottomWorld = ReadBoundedInt32(reader, headerEnd);
    int maxTilesY = ReadBoundedInt32(reader, headerEnd);
    int maxTilesX = ReadBoundedInt32(reader, headerEnd);

    int gameMode;
    bool drunkWorld = false;
    bool getGoodWorld = false;
    bool tenthAnniversaryWorld = false;
    bool dontStarveWorld = false;
    bool notTheBeesWorld = false;
    bool remixWorld = false;
    bool noTrapsWorld = false;
    bool zenithWorld = false;
    bool skyblockWorld = false;
    if (version >= WorldFileFormatConstants.GameModeVersion)
    {
      gameMode = ReadBoundedInt32(reader, headerEnd);
      drunkWorld = ReadBoundedBoolean(reader, headerEnd, version >= 222);
      getGoodWorld = ReadBoundedBoolean(reader, headerEnd, version >= 227);
      tenthAnniversaryWorld = ReadBoundedBoolean(reader, headerEnd, version >= 238);
      dontStarveWorld = ReadBoundedBoolean(reader, headerEnd, version >= 239);
      notTheBeesWorld = ReadBoundedBoolean(reader, headerEnd, version >= 241);
      remixWorld = ReadBoundedBoolean(reader, headerEnd, version >= 249);
      noTrapsWorld = ReadBoundedBoolean(reader, headerEnd, version >= 266);
      if (version >= 267)
      {
        zenithWorld = ReadBoundedBoolean(reader, headerEnd);
      }
      else
      {
        zenithWorld = remixWorld && drunkWorld;
        if (zenithWorld)
        {
          noTrapsWorld = true;
        }
      }
      skyblockWorld = ReadBoundedBoolean(reader, headerEnd, version >= 302);
    }
    else
    {
      gameMode = version >= 112 && ReadBoundedBoolean(reader, headerEnd) ? 1 : 0;
      if (version == 208 && ReadBoundedBoolean(reader, headerEnd))
      {
        gameMode = 2;
      }
    }

    DateTime? creationTime = version >= WorldFileFormatConstants.CreationTimeVersion
      ? DateTime.FromBinary(ReadBoundedInt64(reader, headerEnd))
      : null;
    DateTime? lastPlayed = version >= WorldFileFormatConstants.LastPlayedVersion
      ? DateTime.FromBinary(ReadBoundedInt64(reader, headerEnd))
      : null;

    return new WorldFileHeaderSection(
      worldName,
      seedText,
      generatorVersion,
      uniqueId,
      worldId,
      leftWorld,
      rightWorld,
      topWorld,
      bottomWorld,
      maxTilesX,
      maxTilesY,
      gameMode,
      drunkWorld,
      getGoodWorld,
      tenthAnniversaryWorld,
      dontStarveWorld,
      notTheBeesWorld,
      remixWorld,
      noTrapsWorld,
      zenithWorld,
      skyblockWorld,
      creationTime,
      lastPlayed);
  }

  private static WorldFileEnvironmentSection? ReadEnvironment(
    BinaryReader reader,
    long headerEnd)
  {
    if (reader.BaseStream.Position >= headerEnd)
    {
      return null;
    }

    byte moonType = ReadBoundedByte(reader, headerEnd);
    int[] treeX = ReadFixedInt32Values(reader, 3, headerEnd);
    int[] treeStyle = ReadFixedInt32Values(reader, 4, headerEnd);
    int[] caveBackX = ReadFixedInt32Values(reader, 3, headerEnd);
    int[] caveBackStyle = ReadFixedInt32Values(reader, 4, headerEnd);
    int iceBackStyle = ReadBoundedInt32(reader, headerEnd);
    int jungleBackStyle = ReadBoundedInt32(reader, headerEnd);
    int hellBackStyle = ReadBoundedInt32(reader, headerEnd);
    int spawnTileX = ReadBoundedInt32(reader, headerEnd);
    int spawnTileY = ReadBoundedInt32(reader, headerEnd);
    double worldSurface = ReadBoundedDouble(reader, headerEnd);
    double rockLayer = ReadBoundedDouble(reader, headerEnd);
    double time = ReadBoundedDouble(reader, headerEnd);
    bool dayTime = ReadBoundedBoolean(reader, headerEnd);
    int moonPhase = ReadBoundedInt32(reader, headerEnd);
    bool bloodMoon = ReadBoundedBoolean(reader, headerEnd);
    bool eclipse = ReadBoundedBoolean(reader, headerEnd);
    int dungeonX = ReadBoundedInt32(reader, headerEnd);
    int dungeonY = ReadBoundedInt32(reader, headerEnd);
    bool crimson = ReadBoundedBoolean(reader, headerEnd);
    return new WorldFileEnvironmentSection(
      moonType,
      treeX,
      treeStyle,
      caveBackX,
      caveBackStyle,
      iceBackStyle,
      jungleBackStyle,
      hellBackStyle,
      spawnTileX,
      spawnTileY,
      worldSurface,
      rockLayer,
      time,
      dayTime,
      moonPhase,
      bloodMoon,
      eclipse,
      dungeonX,
      dungeonY,
      crimson);
  }

  private static WorldFileProgressionSection? ReadProgression(
    BinaryReader reader,
    int version,
    long headerEnd)
  {
    if (reader.BaseStream.Position >= headerEnd)
    {
      return null;
    }

    bool downedBoss1 = ReadBoundedBoolean(reader, headerEnd);
    bool downedBoss2 = ReadBoundedBoolean(reader, headerEnd);
    bool downedBoss3 = ReadBoundedBoolean(reader, headerEnd);
    bool downedQueenBee = ReadBoundedBoolean(reader, headerEnd);
    bool downedMechBoss1 = ReadBoundedBoolean(reader, headerEnd);
    bool downedMechBoss2 = ReadBoundedBoolean(reader, headerEnd);
    bool downedMechBoss3 = ReadBoundedBoolean(reader, headerEnd);
    bool downedMechBossAny = ReadBoundedBoolean(reader, headerEnd);
    bool downedPlantBoss = ReadBoundedBoolean(reader, headerEnd);
    bool downedGolemBoss = ReadBoundedBoolean(reader, headerEnd);
    bool downedSlimeKing = ReadBoundedBoolean(
      reader,
      headerEnd,
      version >= WorldFileFormatConstants.SlimeKingVersion);
    bool savedGoblin = ReadBoundedBoolean(reader, headerEnd);
    bool savedWizard = ReadBoundedBoolean(reader, headerEnd);
    bool savedMech = ReadBoundedBoolean(reader, headerEnd);
    bool downedGoblins = ReadBoundedBoolean(reader, headerEnd);
    bool downedClown = ReadBoundedBoolean(reader, headerEnd);
    bool downedFrost = ReadBoundedBoolean(reader, headerEnd);
    bool downedPirates = ReadBoundedBoolean(reader, headerEnd);
    bool shadowOrbSmashed = ReadBoundedBoolean(reader, headerEnd);
    bool spawnMeteor = ReadBoundedBoolean(reader, headerEnd);
    byte shadowOrbCount = ReadBoundedByte(reader, headerEnd);
    int altarCount = ReadBoundedInt32(reader, headerEnd);
    bool hardMode = ReadBoundedBoolean(reader, headerEnd);
    bool afterPartyOfDoom = ReadBoundedBoolean(
      reader,
      headerEnd,
      version >= WorldFileFormatConstants.AfterPartyOfDoomVersion);
    int invasionDelay = ReadBoundedInt32(reader, headerEnd);
    int invasionSize = ReadBoundedInt32(reader, headerEnd);
    int invasionType = ReadBoundedInt32(reader, headerEnd);
    double invasionX = ReadBoundedDouble(reader, headerEnd);
    double slimeRainTime = version >= WorldFileFormatConstants.SlimeRainVersion
      ? ReadBoundedDouble(reader, headerEnd)
      : 0;
    byte sundialCooldown = version >= WorldFileFormatConstants.SundialVersion
      ? ReadBoundedByte(reader, headerEnd)
      : (byte)0;
    bool raining = ReadBoundedBoolean(reader, headerEnd);
    int rainTime = ReadBoundedInt32(reader, headerEnd);
    float maxRain = ReadBoundedSingle(reader, headerEnd);
    int cobaltOreTier = ReadBoundedInt32(reader, headerEnd);
    int mythrilOreTier = ReadBoundedInt32(reader, headerEnd);
    int adamantiteOreTier = ReadBoundedInt32(reader, headerEnd);
    byte[] backgroundStyles = new byte[8];
    for (int index = 0; index < backgroundStyles.Length; index++)
    {
      backgroundStyles[index] = ReadBoundedByte(reader, headerEnd);
    }

    int cloudBackgroundActive = ReadBoundedInt32(reader, headerEnd);
    short cloudCount = ReadBoundedInt16(reader, headerEnd);
    float windSpeedTarget = ReadBoundedSingle(reader, headerEnd);
    return new WorldFileProgressionSection(
      downedBoss1,
      downedBoss2,
      downedBoss3,
      downedQueenBee,
      downedMechBoss1,
      downedMechBoss2,
      downedMechBoss3,
      downedMechBossAny,
      downedPlantBoss,
      downedGolemBoss,
      downedSlimeKing,
      savedGoblin,
      savedWizard,
      savedMech,
      downedGoblins,
      downedClown,
      downedFrost,
      downedPirates,
      shadowOrbSmashed,
      spawnMeteor,
      shadowOrbCount,
      altarCount,
      hardMode,
      afterPartyOfDoom,
      invasionDelay,
      invasionSize,
      invasionType,
      invasionX,
      slimeRainTime,
      sundialCooldown,
      raining,
      rainTime,
      maxRain,
      cobaltOreTier,
      mythrilOreTier,
      adamantiteOreTier,
      backgroundStyles,
      cloudBackgroundActive,
      cloudCount,
      windSpeedTarget);
  }

  private static WorldFileQuestSection? ReadQuests(
    BinaryReader reader,
    int version,
    long headerEnd,
    WorldFileProgressionSection? progression)
  {
    if (version < WorldFileFormatConstants.AnglerFinishedTodayVersion)
    {
      return new WorldFileQuestSection(
        Array.Empty<string>(),
        savedAngler: false,
        anglerQuest: 0,
        savedStylist: false,
        savedTaxCollector: false,
        savedGolfer: false,
        invasionSizeStart: CalculateLegacyInvasionSizeStart(progression),
        cultistDelay: 86400);
    }

    if (reader.BaseStream.Position >= headerEnd)
    {
      return null;
    }

    int completedTodayCount = ReadBoundedInt32(reader, headerEnd);
    if (completedTodayCount < 0 ||
        completedTodayCount > WorldFileFormatConstants.MaxStringListEntries)
    {
      throw new InvalidDataException("The world file contains an invalid Angler completion count.");
    }

    string[] anglerWhoFinishedToday = new string[completedTodayCount];
    for (int index = 0; index < anglerWhoFinishedToday.Length; index++)
    {
      anglerWhoFinishedToday[index] = ReadBoundedString(reader, headerEnd);
    }

    bool savedAngler = ReadBoundedBoolean(
      reader,
      headerEnd,
      version >= WorldFileFormatConstants.SavedAnglerVersion);
    int anglerQuest = version >= WorldFileFormatConstants.AnglerQuestVersion
      ? ReadBoundedInt32(reader, headerEnd)
      : 0;
    bool savedStylist = ReadBoundedBoolean(
      reader,
      headerEnd,
      version >= WorldFileFormatConstants.SavedStylistVersion);
    bool savedTaxCollector = ReadBoundedBoolean(
      reader,
      headerEnd,
      version >= WorldFileFormatConstants.SavedTaxCollectorVersion);
    bool savedGolfer = ReadBoundedBoolean(
      reader,
      headerEnd,
      version >= WorldFileFormatConstants.SavedGolferVersion);
    int invasionSizeStart = version >= WorldFileFormatConstants.InvasionSizeStartVersion
      ? ReadBoundedInt32(reader, headerEnd)
      : CalculateLegacyInvasionSizeStart(progression);
    int cultistDelay = version >= WorldFileFormatConstants.CultistDelayVersion
      ? ReadBoundedInt32(reader, headerEnd)
      : 86400;
    return new WorldFileQuestSection(
      anglerWhoFinishedToday,
      savedAngler,
      anglerQuest,
      savedStylist,
      savedTaxCollector,
      savedGolfer,
      invasionSizeStart,
      cultistDelay);
  }

  private static int CalculateLegacyInvasionSizeStart(
    WorldFileProgressionSection? progression)
  {
    if (progression is null || progression.InvasionType <= 0 || progression.InvasionSize <= 0)
    {
      return 0;
    }

    int initialSize;
    int groupSize;
    switch (progression.InvasionType)
    {
      case 1:
      case 2:
        initialSize = 80;
        groupSize = 40;
        break;
      case 3:
        initialSize = 120;
        groupSize = 60;
        break;
      case 4:
        initialSize = 160;
        groupSize = 40;
        break;
      default:
        return 0;
    }

    int additionalGroups = (int)Math.Ceiling(
      (float)(progression.InvasionSize - initialSize) / groupSize);
    return additionalGroups > 0
      ? initialSize + additionalGroups * groupSize
      : initialSize;
  }

  private static WorldFileBannerSection? ReadBanners(
    BinaryReader reader,
    int version,
    long headerEnd)
  {
    if (version < WorldFileFormatConstants.BannerSectionVersion ||
        reader.BaseStream.Position >= headerEnd)
    {
      return null;
    }

    int killCountLength = ReadBoundedInt16(reader, headerEnd);
    if (killCountLength < 0 ||
        killCountLength > WorldFileFormatConstants.MaxBannerEntries)
    {
      throw new InvalidDataException("The world file contains an invalid banner count length.");
    }

    int[] killCounts = new int[killCountLength];
    for (int index = 0; index < killCounts.Length; index++)
    {
      killCounts[index] = ReadBoundedInt32(reader, headerEnd);
    }

    ushort[] claimableCounts = Array.Empty<ushort>();
    if (version >= WorldFileFormatConstants.BannerClaimableVersion)
    {
      int claimableCountLength = ReadBoundedInt16(reader, headerEnd);
      if (claimableCountLength < 0 ||
          claimableCountLength > WorldFileFormatConstants.MaxBannerEntries)
      {
        throw new InvalidDataException(
          "The world file contains an invalid claimable banner count length.");
      }

      claimableCounts = new ushort[claimableCountLength];
      for (int index = 0; index < claimableCounts.Length; index++)
      {
        claimableCounts[index] = ReadBoundedUInt16(reader, headerEnd);
      }
    }

    return new WorldFileBannerSection(killCounts, claimableCounts);
  }

  private static WorldFileBossProgressionSection? ReadBossProgression(
    BinaryReader reader,
    int version,
    long headerEnd)
  {
    if (version < WorldFileFormatConstants.FastForwardDawnVersion ||
        reader.BaseStream.Position >= headerEnd)
    {
      return null;
    }

    bool fastForwardTimeToDawn = ReadBoundedBoolean(reader, headerEnd);
    bool downedFishron = ReadBoundedBoolean(
      reader,
      headerEnd,
      version >= WorldFileFormatConstants.BossProgressionVersion);
    bool downedMartians = ReadBoundedBoolean(
      reader,
      headerEnd,
      version >= WorldFileFormatConstants.BossProgressionVersion);
    bool downedAncientCultist = ReadBoundedBoolean(
      reader,
      headerEnd,
      version >= WorldFileFormatConstants.BossProgressionVersion);
    bool downedMoonlord = ReadBoundedBoolean(
      reader,
      headerEnd,
      version >= WorldFileFormatConstants.BossProgressionVersion);
    bool downedHalloweenKing = ReadBoundedBoolean(
      reader,
      headerEnd,
      version >= WorldFileFormatConstants.BossProgressionVersion);
    bool downedHalloweenTree = ReadBoundedBoolean(
      reader,
      headerEnd,
      version >= WorldFileFormatConstants.BossProgressionVersion);
    bool downedChristmasIceQueen = ReadBoundedBoolean(
      reader,
      headerEnd,
      version >= WorldFileFormatConstants.BossProgressionVersion);
    bool downedChristmasSantank = ReadBoundedBoolean(
      reader,
      headerEnd,
      version >= WorldFileFormatConstants.BossProgressionVersion);
    bool downedChristmasTree = ReadBoundedBoolean(
      reader,
      headerEnd,
      version >= WorldFileFormatConstants.BossProgressionVersion);
    bool downedTowerSolar = ReadBoundedBoolean(
      reader,
      headerEnd,
      version >= WorldFileFormatConstants.LunarTowerProgressionVersion);
    bool downedTowerVortex = ReadBoundedBoolean(
      reader,
      headerEnd,
      version >= WorldFileFormatConstants.LunarTowerProgressionVersion);
    bool downedTowerNebula = ReadBoundedBoolean(
      reader,
      headerEnd,
      version >= WorldFileFormatConstants.LunarTowerProgressionVersion);
    bool downedTowerStardust = ReadBoundedBoolean(
      reader,
      headerEnd,
      version >= WorldFileFormatConstants.LunarTowerProgressionVersion);
    bool towerActiveSolar = ReadBoundedBoolean(
      reader,
      headerEnd,
      version >= WorldFileFormatConstants.LunarTowerProgressionVersion);
    bool towerActiveVortex = ReadBoundedBoolean(
      reader,
      headerEnd,
      version >= WorldFileFormatConstants.LunarTowerProgressionVersion);
    bool towerActiveNebula = ReadBoundedBoolean(
      reader,
      headerEnd,
      version >= WorldFileFormatConstants.LunarTowerProgressionVersion);
    bool towerActiveStardust = ReadBoundedBoolean(
      reader,
      headerEnd,
      version >= WorldFileFormatConstants.LunarTowerProgressionVersion);
    bool lunarApocalypseIsUp = ReadBoundedBoolean(
      reader,
      headerEnd,
      version >= WorldFileFormatConstants.LunarTowerProgressionVersion);
    return new WorldFileBossProgressionSection(
      fastForwardTimeToDawn,
      downedFishron,
      downedMartians,
      downedAncientCultist,
      downedMoonlord,
      downedHalloweenKing,
      downedHalloweenTree,
      downedChristmasIceQueen,
      downedChristmasSantank,
      downedChristmasTree,
      downedTowerSolar,
      downedTowerVortex,
      downedTowerNebula,
      downedTowerStardust,
      towerActiveSolar,
      towerActiveVortex,
      towerActiveNebula,
      towerActiveStardust,
      lunarApocalypseIsUp);
  }

  private static WorldFilePartySection? ReadParty(
    BinaryReader reader,
    int version,
    long headerEnd)
  {
    if (version < WorldFileFormatConstants.PartyVersion ||
        reader.BaseStream.Position >= headerEnd)
    {
      return null;
    }

    bool manual = ReadBoundedBoolean(reader, headerEnd);
    bool genuine = ReadBoundedBoolean(reader, headerEnd);
    int cooldown = ReadBoundedInt32(reader, headerEnd);
    int celebratingCount = ReadBoundedInt32(reader, headerEnd);
    if (celebratingCount < 0 ||
        celebratingCount > WorldFileFormatConstants.MaxPartyEntries)
    {
      throw new InvalidDataException(
        "The world file contains an invalid party celebration count.");
    }

    int[] celebratingNpcIds = new int[celebratingCount];
    for (int index = 0; index < celebratingNpcIds.Length; index++)
    {
      celebratingNpcIds[index] = ReadBoundedInt32(reader, headerEnd);
    }

    return new WorldFilePartySection(
      manual,
      genuine,
      cooldown,
      celebratingNpcIds);
  }

  private static WorldFileSandstormSection? ReadSandstorm(
    BinaryReader reader,
    int version,
    long headerEnd)
  {
    if (version < WorldFileFormatConstants.SandstormVersion ||
        reader.BaseStream.Position >= headerEnd)
    {
      return null;
    }

    return new WorldFileSandstormSection(
      ReadBoundedBoolean(reader, headerEnd),
      ReadBoundedInt32(reader, headerEnd),
      ReadBoundedSingle(reader, headerEnd),
      ReadBoundedSingle(reader, headerEnd));
  }

  private static WorldFileDefenderEventSection? ReadDefenderEvent(
    BinaryReader reader,
    int version,
    long headerEnd)
  {
    if (version < WorldFileFormatConstants.DefenderEventVersion ||
        reader.BaseStream.Position >= headerEnd)
    {
      return null;
    }

    return new WorldFileDefenderEventSection(
      ReadBoundedBoolean(reader, headerEnd),
      ReadBoundedBoolean(reader, headerEnd),
      ReadBoundedBoolean(reader, headerEnd),
      ReadBoundedBoolean(reader, headerEnd));
  }

  private static WorldFileBackgroundSection? ReadBackgrounds(
    BinaryReader reader,
    int version,
    long headerEnd)
  {
    if (reader.BaseStream.Position >= headerEnd ||
        version < WorldFileFormatConstants.AdditionalBackgroundVersion)
    {
      return null;
    }

    byte[] styles = new byte[5];
    styles[0] = ReadBoundedByte(reader, headerEnd);
    styles[1] = version >= WorldFileFormatConstants.AdditionalBackground9Version
      ? ReadBoundedByte(reader, headerEnd)
      : (byte)0;
    if (version > WorldFileFormatConstants.AdditionalBackgroundVersion)
    {
      styles[2] = ReadBoundedByte(reader, headerEnd);
      styles[3] = ReadBoundedByte(reader, headerEnd);
      styles[4] = ReadBoundedByte(reader, headerEnd);
    }

    return new WorldFileBackgroundSection(styles);
  }

  private static WorldFileEventSection? ReadEvents(
    BinaryReader reader,
    int version,
    long headerEnd)
  {
    if (reader.BaseStream.Position >= headerEnd ||
        version < WorldFileFormatConstants.CombatBookVersion)
    {
      return null;
    }

    bool combatBookWasUsed = ReadBoundedBoolean(reader, headerEnd);
    int lanternNightCooldown = 0;
    bool lanternNightGenuine = false;
    bool lanternNightManual = false;
    bool lanternNightNextNightIsGenuine = false;
    if (version >= WorldFileFormatConstants.LanternNightVersion)
    {
      lanternNightCooldown = ReadBoundedInt32(reader, headerEnd);
      lanternNightGenuine = ReadBoundedBoolean(reader, headerEnd);
      lanternNightManual = ReadBoundedBoolean(reader, headerEnd);
      lanternNightNextNightIsGenuine = ReadBoundedBoolean(reader, headerEnd);
    }

    return new WorldFileEventSection(
      combatBookWasUsed,
      lanternNightCooldown,
      lanternNightGenuine,
      lanternNightManual,
      lanternNightNextNightIsGenuine);
  }

  private static WorldFileTreeTopsSection? ReadTreeTops(
    BinaryReader reader,
    int version,
    long headerEnd)
  {
    if (reader.BaseStream.Position >= headerEnd ||
        version < WorldFileFormatConstants.TreeTopsVersion)
    {
      return null;
    }

    int variationCount = ReadBoundedInt32(reader, headerEnd);
    if (variationCount < 0 || variationCount > WorldFileFormatConstants.MaxTreeTopsEntries)
    {
      throw new InvalidDataException("The world file contains an invalid TreeTops count.");
    }

    int[] variations = new int[variationCount];
    for (int index = 0; index < variations.Length; index++)
    {
      variations[index] = ReadBoundedInt32(reader, headerEnd);
    }

    return new WorldFileTreeTopsSection(variations);
  }

  private static WorldFileSeasonalSection? ReadSeasonal(
    BinaryReader reader,
    int version,
    long headerEnd)
  {
    if (reader.BaseStream.Position >= headerEnd ||
        version < WorldFileFormatConstants.SeasonalFlagsVersion)
    {
      return null;
    }

    bool forceHalloweenForToday = ReadBoundedBoolean(reader, headerEnd);
    bool forceChristmasForToday = ReadBoundedBoolean(reader, headerEnd);
    int copperOreTier = -1;
    int ironOreTier = -1;
    int silverOreTier = -1;
    int goldOreTier = -1;
    if (version >= WorldFileFormatConstants.SavedOreTiersVersion)
    {
      copperOreTier = ReadBoundedInt32(reader, headerEnd);
      ironOreTier = ReadBoundedInt32(reader, headerEnd);
      silverOreTier = ReadBoundedInt32(reader, headerEnd);
      goldOreTier = ReadBoundedInt32(reader, headerEnd);
    }

    return new WorldFileSeasonalSection(
      forceHalloweenForToday,
      forceChristmasForToday,
      copperOreTier,
      ironOreTier,
      silverOreTier,
      goldOreTier);
  }

  private static WorldFileNpcUnlockSection? ReadNpcUnlocks(
    BinaryReader reader,
    int version,
    long headerEnd)
  {
    if (reader.BaseStream.Position >= headerEnd ||
        version < WorldFileFormatConstants.BoughtPetsVersion)
    {
      return null;
    }

    bool boughtCat = ReadBoundedBoolean(reader, headerEnd);
    bool boughtDog = ReadBoundedBoolean(reader, headerEnd);
    bool boughtBunny = ReadBoundedBoolean(reader, headerEnd);
    bool downedEmpressOfLight = ReadBoundedBoolean(
      reader,
      headerEnd,
      version >= WorldFileFormatConstants.EmpressQueenSlimeVersion);
    bool downedQueenSlime = ReadBoundedBoolean(
      reader,
      headerEnd,
      version >= WorldFileFormatConstants.EmpressQueenSlimeVersion);
    bool downedDeerclops = ReadBoundedBoolean(
      reader,
      headerEnd,
      version >= WorldFileFormatConstants.DeerclopsVersion);
    bool unlockedSlimeBlueSpawn = ReadBoundedBoolean(
      reader,
      headerEnd,
      version >= WorldFileFormatConstants.SlimeBlueVersion);
    bool unlockedMerchantSpawn = ReadBoundedBoolean(
      reader,
      headerEnd,
      version >= WorldFileFormatConstants.TownNpcUnlocksVersion);
    bool unlockedDemolitionistSpawn = ReadBoundedBoolean(
      reader,
      headerEnd,
      version >= WorldFileFormatConstants.TownNpcUnlocksVersion);
    bool unlockedPartyGirlSpawn = ReadBoundedBoolean(
      reader,
      headerEnd,
      version >= WorldFileFormatConstants.TownNpcUnlocksVersion);
    bool unlockedDyeTraderSpawn = ReadBoundedBoolean(
      reader,
      headerEnd,
      version >= WorldFileFormatConstants.TownNpcUnlocksVersion);
    bool unlockedTruffleSpawn = ReadBoundedBoolean(
      reader,
      headerEnd,
      version >= WorldFileFormatConstants.TownNpcUnlocksVersion);
    bool unlockedArmsDealerSpawn = ReadBoundedBoolean(
      reader,
      headerEnd,
      version >= WorldFileFormatConstants.TownNpcUnlocksVersion);
    bool unlockedNurseSpawn = ReadBoundedBoolean(
      reader,
      headerEnd,
      version >= WorldFileFormatConstants.TownNpcUnlocksVersion);
    bool unlockedPrincessSpawn = ReadBoundedBoolean(
      reader,
      headerEnd,
      version >= WorldFileFormatConstants.TownNpcUnlocksVersion);
    bool combatBookVolumeTwoWasUsed = ReadBoundedBoolean(
      reader,
      headerEnd,
      version >= WorldFileFormatConstants.CombatBookVolumeTwoVersion);
    bool peddlersSatchelWasUsed = ReadBoundedBoolean(
      reader,
      headerEnd,
      version >= WorldFileFormatConstants.PeddlersSatchelVersion);
    bool unlockedSlimeGreenSpawn = ReadBoundedBoolean(
      reader,
      headerEnd,
      version >= WorldFileFormatConstants.SlimeVariantsVersion);
    bool unlockedSlimeOldSpawn = ReadBoundedBoolean(
      reader,
      headerEnd,
      version >= WorldFileFormatConstants.SlimeVariantsVersion);
    bool unlockedSlimePurpleSpawn = ReadBoundedBoolean(
      reader,
      headerEnd,
      version >= WorldFileFormatConstants.SlimeVariantsVersion);
    bool unlockedSlimeRainbowSpawn = ReadBoundedBoolean(
      reader,
      headerEnd,
      version >= WorldFileFormatConstants.SlimeVariantsVersion);
    bool unlockedSlimeRedSpawn = ReadBoundedBoolean(
      reader,
      headerEnd,
      version >= WorldFileFormatConstants.SlimeVariantsVersion);
    bool unlockedSlimeYellowSpawn = ReadBoundedBoolean(
      reader,
      headerEnd,
      version >= WorldFileFormatConstants.SlimeVariantsVersion);
    bool unlockedSlimeCopperSpawn = ReadBoundedBoolean(
      reader,
      headerEnd,
      version >= WorldFileFormatConstants.SlimeVariantsVersion);

    return new WorldFileNpcUnlockSection(
      boughtCat,
      boughtDog,
      boughtBunny,
      downedEmpressOfLight,
      downedQueenSlime,
      downedDeerclops,
      unlockedSlimeBlueSpawn,
      unlockedMerchantSpawn,
      unlockedDemolitionistSpawn,
      unlockedPartyGirlSpawn,
      unlockedDyeTraderSpawn,
      unlockedTruffleSpawn,
      unlockedArmsDealerSpawn,
      unlockedNurseSpawn,
      unlockedPrincessSpawn,
      combatBookVolumeTwoWasUsed,
      peddlersSatchelWasUsed,
      unlockedSlimeGreenSpawn,
      unlockedSlimeOldSpawn,
      unlockedSlimePurpleSpawn,
      unlockedSlimeRainbowSpawn,
      unlockedSlimeRedSpawn,
      unlockedSlimeYellowSpawn,
      unlockedSlimeCopperSpawn);
  }

  private static WorldFileTimePolicySection? ReadTimePolicy(
    BinaryReader reader,
    int version,
    long headerEnd)
  {
    if (reader.BaseStream.Position >= headerEnd ||
        version < WorldFileFormatConstants.FastForwardDuskVersion)
    {
      return null;
    }

    bool fastForwardTimeToDusk = ReadBoundedBoolean(reader, headerEnd);
    byte moondialCooldown = ReadBoundedByte(reader, headerEnd);
    bool forceHalloweenForever = ReadBoundedBoolean(
      reader,
      headerEnd,
      version >= WorldFileFormatConstants.ForceSeasonForeverVersion);
    bool forceChristmasForever = ReadBoundedBoolean(
      reader,
      headerEnd,
      version >= WorldFileFormatConstants.ForceSeasonForeverVersion);
    bool vampireSeed = ReadBoundedBoolean(
      reader,
      headerEnd,
      version >= WorldFileFormatConstants.VampireSeedVersion);
    bool infectedSeed = ReadBoundedBoolean(
      reader,
      headerEnd,
      version >= WorldFileFormatConstants.InfectedSeedVersion);
    int meteorShowerCount = version >= WorldFileFormatConstants.MeteorShowerVersion
      ? ReadBoundedInt32(reader, headerEnd)
      : 0;
    int coinRain = version >= WorldFileFormatConstants.MeteorShowerVersion
      ? ReadBoundedInt32(reader, headerEnd)
      : 0;
    bool teamBasedSpawnsSeed = ReadBoundedBoolean(
      reader,
      headerEnd,
      version >= WorldFileFormatConstants.TeamBasedSpawnsVersion);

    return new WorldFileTimePolicySection(
      fastForwardTimeToDusk,
      moondialCooldown,
      forceHalloweenForever,
      forceChristmasForever,
      vampireSeed,
      infectedSeed,
      meteorShowerCount,
      coinRain,
      teamBasedSpawnsSeed);
  }

  private static WorldFileSpawnSection? ReadSpawnSection(
    BinaryReader reader,
    int version,
    long headerEnd)
  {
    if (reader.BaseStream.Position >= headerEnd ||
        version < WorldFileFormatConstants.SpawnSectionVersion)
    {
      return null;
    }

    int pointCount = ReadBoundedByte(reader, headerEnd);
    if (pointCount > WorldFileFormatConstants.MaxExtraSpawnPoints)
    {
      throw new InvalidDataException("The world file contains too many extra spawn points.");
    }

    WorldFileExtraSpawnPoint[] points = new WorldFileExtraSpawnPoint[pointCount];
    for (int index = 0; index < points.Length; index++)
    {
      points[index] = new WorldFileExtraSpawnPoint(
        ReadBoundedInt16(reader, headerEnd),
        ReadBoundedInt16(reader, headerEnd));
    }

    bool dualDungeonsSeed = ReadBoundedBoolean(
      reader,
      headerEnd,
      version >= WorldFileFormatConstants.DualDungeonsVersion);
    bool moreLightningSeed = ReadBoundedBoolean(
      reader,
      headerEnd,
      version >= WorldFileFormatConstants.LightningSeedsVersion);
    bool noLightningSeed = ReadBoundedBoolean(
      reader,
      headerEnd,
      version >= WorldFileFormatConstants.LightningSeedsVersion);
    if (version >= WorldFileFormatConstants.ManifestVersion &&
        version < WorldFileFormatConstants.LegacyManifestFieldEndVersion)
    {
      _ = ReadBoundedUInt32(reader, headerEnd);
    }

    string? worldManifestJson = version >= WorldFileFormatConstants.ManifestVersion
      ? ReadBoundedString(reader, headerEnd)
      : null;
    return new WorldFileSpawnSection(
      points,
      dualDungeonsSeed,
      moreLightningSeed,
      noLightningSeed,
      worldManifestJson);
  }

  private static WorldFileChestSection? ReadChests(
    BinaryReader reader,
    int version,
    IReadOnlyList<int> sectionPointers,
    long streamLength)
  {
    if (sectionPointers.Count < 4)
    {
      return null;
    }

    int sectionStart = sectionPointers[2];
    int sectionEnd = sectionPointers[3];
    ValidateSectionRange(sectionStart, sectionEnd, streamLength, "chest");
    reader.BaseStream.Position = sectionStart;

    short chestCount = ReadBoundedInt16(reader, sectionEnd);
    if (chestCount < 0 || chestCount > WorldFileFormatConstants.MaxChests)
    {
      throw new InvalidDataException("The world file contains an invalid chest count.");
    }

    int legacyItemCount = version < WorldFileFormatConstants.ChestReworkVersion
      ? ReadBoundedInt16(reader, sectionEnd)
      : 0;
    if (legacyItemCount < 0 || legacyItemCount > WorldFileFormatConstants.MaxChestItems)
    {
      throw new InvalidDataException("The world file contains an invalid legacy chest item count.");
    }

    WorldFileChestRecord[] chests = new WorldFileChestRecord[chestCount];
    for (int index = 0; index < chests.Length; index++)
    {
      int x = ReadBoundedInt32(reader, sectionEnd);
      int y = ReadBoundedInt32(reader, sectionEnd);
      string name = ReadBoundedString(reader, sectionEnd);
      int itemCount = version >= WorldFileFormatConstants.ChestReworkVersion
        ? ReadBoundedInt32(reader, sectionEnd)
        : legacyItemCount;
      if (itemCount < 0 || itemCount > WorldFileFormatConstants.MaxChestItems)
      {
        throw new InvalidDataException("The world file contains an invalid chest item count.");
      }

      WorldFileChestItem[] items = new WorldFileChestItem[itemCount];
      for (int itemIndex = 0; itemIndex < items.Length; itemIndex++)
      {
        short stack = ReadBoundedInt16(reader, sectionEnd);
        int type = 0;
        byte prefix = 0;
        if (stack != 0)
        {
          type = ReadBoundedInt32(reader, sectionEnd);
          prefix = ReadBoundedByte(reader, sectionEnd);
        }

        items[itemIndex] = new WorldFileChestItem(stack, type, prefix);
      }

      chests[index] = new WorldFileChestRecord(x, y, name, items);
    }

    EnsureSectionConsumed(reader, sectionEnd, "chest");
    return new WorldFileChestSection(chests);
  }

  private static WorldFileTilePayloadSection? ReadTilePayload(
    BinaryReader reader,
    IReadOnlyList<int> sectionPointers,
    long streamLength,
    WorldFileHeaderSection header,
    IReadOnlyList<bool> frameImportant)
  {
    if (sectionPointers.Count < 3)
    {
      return null;
    }

    int sectionStart = sectionPointers[1];
    int sectionEnd = sectionPointers[2];
    ValidateSectionRange(sectionStart, sectionEnd, streamLength, "Tile");
    reader.BaseStream.Position = sectionStart;
    int payloadLength = checked(sectionEnd - sectionStart);
    if (payloadLength == 0)
    {
      throw new InvalidDataException("The world file Tile section is empty.");
    }

    if (payloadLength > WorldFileFormatConstants.MaxRawSectionBytes)
    {
      throw new InvalidDataException("The world file Tile section is too large.");
    }

    byte[] compressedPayload = ReadBoundedBytes(reader, payloadLength, sectionEnd);
    return new WorldFileTilePayloadSection(
      header.MaxTilesX,
      header.MaxTilesY,
      frameImportant,
      compressedPayload);
  }

  private static WorldFileSignSection? ReadSigns(
    BinaryReader reader,
    IReadOnlyList<int> sectionPointers,
    long streamLength)
  {
    if (sectionPointers.Count < 5)
    {
      return null;
    }

    int sectionStart = sectionPointers[3];
    int sectionEnd = sectionPointers[4];
    ValidateSectionRange(sectionStart, sectionEnd, streamLength, "sign");
    reader.BaseStream.Position = sectionStart;

    short signCount = ReadBoundedInt16(reader, sectionEnd);
    if (signCount < 0 || signCount > WorldFileFormatConstants.MaxSigns)
    {
      throw new InvalidDataException("The world file contains an invalid sign count.");
    }

    WorldFileSignRecord[] signs = new WorldFileSignRecord[signCount];
    for (int index = 0; index < signs.Length; index++)
    {
      string text = ReadBoundedString(reader, sectionEnd);
      int x = ReadBoundedInt32(reader, sectionEnd);
      int y = ReadBoundedInt32(reader, sectionEnd);
      signs[index] = new WorldFileSignRecord(text, x, y);
    }

    EnsureSectionConsumed(reader, sectionEnd, "sign");
    return new WorldFileSignSection(signs);
  }

  private static WorldFileNpcSection? ReadNpcs(
    BinaryReader reader,
    int version,
    IReadOnlyList<int> sectionPointers,
    long streamLength)
  {
    if (sectionPointers.Count < 6)
    {
      return null;
    }

    int sectionStart = sectionPointers[4];
    int sectionEnd = sectionPointers[5];
    ValidateSectionRange(sectionStart, sectionEnd, streamLength, "NPC");
    reader.BaseStream.Position = sectionStart;

    List<int> shimmeredTownNpcIds = new();
    if (version >= WorldFileFormatConstants.NpcShimmeredVersion)
    {
      int shimmeredCount = ReadBoundedInt32(reader, sectionEnd);
      if (shimmeredCount < 0 ||
          shimmeredCount > WorldFileFormatConstants.MaxShimmeredNpcIds)
      {
        throw new InvalidDataException(
          "The world file contains an invalid shimmered NPC count.");
      }

      shimmeredTownNpcIds = new List<int>(shimmeredCount);
      for (int index = 0; index < shimmeredCount; index++)
      {
        int npcId = ReadBoundedInt32(reader, sectionEnd);
        if (npcId < 0)
        {
          throw new InvalidDataException(
            "The world file contains a negative shimmered NPC identifier.");
        }

        shimmeredTownNpcIds.Add(npcId);
      }
    }

    List<WorldFileNpcRecord> townNpcs = ReadNpcRecords(
      reader,
      version,
      sectionEnd,
      isTownNpc: true);
    List<WorldFileNpcRecord> savedNpcs = new();
    if (version >= WorldFileFormatConstants.NpcSavedSectionVersion)
    {
      savedNpcs = ReadNpcRecords(reader, version, sectionEnd, isTownNpc: false);
    }

    EnsureSectionConsumed(reader, sectionEnd, "NPC");
    return new WorldFileNpcSection(shimmeredTownNpcIds, townNpcs, savedNpcs);
  }

  private static WorldFilePressurePlateSection? ReadPressurePlates(
    BinaryReader reader,
    int version,
    IReadOnlyList<int> sectionPointers,
    long streamLength)
  {
    if (version < WorldFileFormatConstants.WeightedPressurePlatesVersion ||
        sectionPointers.Count < 8)
    {
      return null;
    }

    int sectionStart = sectionPointers[6];
    int sectionEnd = sectionPointers[7];
    ValidateSectionRange(sectionStart, sectionEnd, streamLength, "pressure plate");
    reader.BaseStream.Position = sectionStart;

    int plateCount = ReadBoundedInt32(reader, sectionEnd);
    if (plateCount < 0 || plateCount > WorldFileFormatConstants.MaxPressurePlates)
    {
      throw new InvalidDataException(
        "The world file contains an invalid pressure plate count.");
    }

    WorldFilePressurePlateRecord[] plates = new WorldFilePressurePlateRecord[plateCount];
    for (int index = 0; index < plates.Length; index++)
    {
      plates[index] = new WorldFilePressurePlateRecord(
        ReadBoundedInt32(reader, sectionEnd),
        ReadBoundedInt32(reader, sectionEnd));
    }

    EnsureSectionConsumed(reader, sectionEnd, "pressure plate");
    return new WorldFilePressurePlateSection(plates);
  }

  private static WorldFileTileEntitySection? ReadTileEntities(
    BinaryReader reader,
    int version,
    IReadOnlyList<int> sectionPointers,
    long streamLength)
  {
    if (version < WorldFileFormatConstants.TileEntitySectionVersion ||
        sectionPointers.Count < 7)
    {
      return null;
    }

    int sectionStart = sectionPointers[5];
    int sectionEnd = sectionPointers[6];
    ValidateSectionRange(sectionStart, sectionEnd, streamLength, "TileEntity");
    reader.BaseStream.Position = sectionStart;

    int entityCount = ReadBoundedInt32(reader, sectionEnd);
    if (entityCount < 0 || entityCount > WorldFileFormatConstants.MaxTileEntities)
    {
      throw new InvalidDataException("The world file contains an invalid TileEntity count.");
    }

    long serializedLength = sectionEnd - reader.BaseStream.Position;
    if (serializedLength < 0 ||
        serializedLength > WorldFileFormatConstants.MaxRawSectionBytes)
    {
      throw new InvalidDataException("The world file TileEntity section is too large.");
    }

    if ((entityCount == 0 && serializedLength != 0) ||
        (entityCount > 0 && serializedLength == 0))
    {
      throw new InvalidDataException(
        "The world file TileEntity count does not match its opaque record payload.");
    }

    byte[] serializedRecords = ReadBoundedBytes(reader, (int)serializedLength, sectionEnd);
    EnsureSectionConsumed(reader, sectionEnd, "TileEntity");
    return new WorldFileTileEntitySection(entityCount, serializedRecords);
  }

  private static WorldFileTownManagerSection? ReadTownManager(
    BinaryReader reader,
    int version,
    IReadOnlyList<int> sectionPointers,
    long streamLength)
  {
    if (version < WorldFileFormatConstants.TownManagerSectionVersion ||
        sectionPointers.Count < 9)
    {
      return null;
    }

    int sectionStart = sectionPointers[7];
    int sectionEnd = sectionPointers[8];
    ValidateSectionRange(sectionStart, sectionEnd, streamLength, "TownManager");
    reader.BaseStream.Position = sectionStart;

    int roomCount = ReadBoundedInt32(reader, sectionEnd);
    if (roomCount < 0 || roomCount > WorldFileFormatConstants.MaxTownRooms)
    {
      throw new InvalidDataException("The world file contains an invalid town room count.");
    }

    WorldFileTownRoomRecord[] rooms = new WorldFileTownRoomRecord[roomCount];
    for (int index = 0; index < rooms.Length; index++)
    {
      int npcType = ReadBoundedInt32(reader, sectionEnd);
      int tileX = ReadBoundedInt32(reader, sectionEnd);
      int tileY = ReadBoundedInt32(reader, sectionEnd);
      rooms[index] = new WorldFileTownRoomRecord(npcType, tileX, tileY);
    }

    EnsureSectionConsumed(reader, sectionEnd, "TownManager");
    return new WorldFileTownManagerSection(rooms);
  }

  private static WorldFileBestiarySection? ReadBestiary(
    BinaryReader reader,
    int version,
    IReadOnlyList<int> sectionPointers,
    long streamLength)
  {
    if (version < WorldFileFormatConstants.BestiarySectionVersion ||
        sectionPointers.Count < 10)
    {
      return null;
    }

    int sectionStart = sectionPointers[8];
    int sectionEnd = sectionPointers[9];
    ValidateSectionRange(sectionStart, sectionEnd, streamLength, "Bestiary");
    if ((long)sectionEnd - sectionStart > WorldFileFormatConstants.MaxRawSectionBytes)
    {
      throw new InvalidDataException("The world file Bestiary section is too large.");
    }

    reader.BaseStream.Position = sectionStart;

    int killCount = ReadBoundedInt32(reader, sectionEnd);
    if (killCount < 0 || killCount > WorldFileFormatConstants.MaxBestiaryEntries)
    {
      throw new InvalidDataException("The world file contains an invalid bestiary kill count.");
    }

    WorldFileBestiaryKillCount[] kills = new WorldFileBestiaryKillCount[killCount];
    for (int index = 0; index < kills.Length; index++)
    {
      string persistentId = ReadBoundedString(reader, sectionEnd);
      int count = ReadBoundedInt32(reader, sectionEnd);
      if (count < 0)
      {
        throw new InvalidDataException("The world file contains a negative bestiary kill count.");
      }

      kills[index] = new WorldFileBestiaryKillCount(persistentId, count);
    }

    string[] seenNpcIds = ReadBoundedStringList(reader, sectionEnd, "bestiary sight");
    string[] chattedNpcIds = ReadBoundedStringList(reader, sectionEnd, "bestiary chat");
    EnsureSectionConsumed(reader, sectionEnd, "Bestiary");
    return new WorldFileBestiarySection(kills, seenNpcIds, chattedNpcIds);
  }

  private static WorldFileCreativePowersSection? ReadCreativePowers(
    BinaryReader reader,
    int version,
    IReadOnlyList<int> sectionPointers,
    long streamLength)
  {
    if (version < WorldFileFormatConstants.CreativePowersSectionVersion ||
        sectionPointers.Count < 11)
    {
      return null;
    }

    int sectionStart = sectionPointers[9];
    int sectionEnd = sectionPointers[10];
    ValidateSectionRange(sectionStart, sectionEnd, streamLength, "CreativePowers");
    reader.BaseStream.Position = sectionStart;
    int payloadLength = checked(sectionEnd - sectionStart);
    if (payloadLength == 0 || payloadLength > WorldFileFormatConstants.MaxRawSectionBytes)
    {
      throw new InvalidDataException(
        "The world file CreativePowers section does not contain a terminator or is too large.");
    }

    byte[] payload = ReadBoundedBytes(reader, payloadLength, sectionEnd);
    if (payload[^1] != 0)
    {
      throw new InvalidDataException(
        "The world file CreativePowers section does not end with its terminator.");
    }

    EnsureSectionConsumed(reader, sectionEnd, "CreativePowers");
    return new WorldFileCreativePowersSection(payload);
  }

  private static WorldFileFooterSection? ReadFooter(
    BinaryReader reader,
    IReadOnlyList<int> sectionPointers,
    long streamLength)
  {
    if (sectionPointers.Count < 11)
    {
      return null;
    }

    int footerStart = sectionPointers[10];
    if (footerStart < 0 || footerStart > streamLength)
    {
      throw new InvalidDataException("The world file footer section pointer is invalid.");
    }

    reader.BaseStream.Position = footerStart;
    bool isComplete = ReadBoundedBoolean(reader, streamLength);
    string worldName = ReadBoundedString(reader, streamLength);
    int worldId = ReadBoundedInt32(reader, streamLength);
    EnsureSectionConsumed(reader, streamLength, "footer");
    return new WorldFileFooterSection(isComplete, worldName, worldId);
  }

  private static List<WorldFileNpcRecord> ReadNpcRecords(
    BinaryReader reader,
    int version,
    long sectionEnd,
    bool isTownNpc)
  {
    List<WorldFileNpcRecord> records = new();
    while (ReadBoundedBoolean(reader, sectionEnd))
    {
      if (records.Count >= WorldFileFormatConstants.MaxNpcRecords)
      {
        throw new InvalidDataException("The world file contains too many NPC records.");
      }

      int? netId = null;
      string? legacyTypeName = null;
      if (version >= WorldFileFormatConstants.NpcNetIdVersion)
      {
        netId = ReadBoundedInt32(reader, sectionEnd);
      }
      else
      {
        legacyTypeName = ReadBoundedString(reader, sectionEnd);
      }

      string name = isTownNpc ? ReadBoundedString(reader, sectionEnd) : string.Empty;
      float positionX = ReadBoundedSingle(reader, sectionEnd);
      float positionY = ReadBoundedSingle(reader, sectionEnd);
      bool homeless = false;
      int homeTileX = 0;
      int homeTileY = 0;
      int? townNpcVariationIndex = null;
      bool homelessDespawn = false;
      if (isTownNpc)
      {
        homeless = ReadBoundedBoolean(reader, sectionEnd);
        homeTileX = ReadBoundedInt32(reader, sectionEnd);
        homeTileY = ReadBoundedInt32(reader, sectionEnd);
        if (version >= WorldFileFormatConstants.NpcTownVariationVersion)
        {
          byte flags = ReadBoundedByte(reader, sectionEnd);
          if ((flags & 1) != 0)
          {
            townNpcVariationIndex = ReadBoundedInt32(reader, sectionEnd);
          }
        }

        if (version >= WorldFileFormatConstants.NpcHomelessDespawnVersion)
        {
          homelessDespawn = ReadBoundedBoolean(reader, sectionEnd);
        }
      }

      records.Add(new WorldFileNpcRecord(
        netId,
        legacyTypeName,
        isTownNpc,
        name,
        positionX,
        positionY,
        homeless,
        homeTileX,
        homeTileY,
        townNpcVariationIndex,
        homelessDespawn));
    }

    return records;
  }

  private static void ValidateSectionRange(
    int start,
    int end,
    long streamLength,
    string sectionName)
  {
    if (start < 0 || end < start || end > streamLength)
    {
      throw new InvalidDataException(
        $"The world file {sectionName} section pointers are invalid.");
    }
  }

  private static void EnsureSectionConsumed(
    BinaryReader reader,
    long end,
    string sectionName)
  {
    if (reader.BaseStream.Position != end)
    {
      throw new InvalidDataException(
        $"The world file {sectionName} section contains unsupported trailing data.");
    }
  }

  private static int[] ReadFixedInt32Values(
    BinaryReader reader,
    int count,
    long end)
  {
    var values = new int[count];
    for (int index = 0; index < values.Length; index++)
    {
      values[index] = ReadBoundedInt32(reader, end);
    }

    return values;
  }

  private static string ReadBoundedString(BinaryReader reader, long end)
  {
    int byteCount = Read7BitEncodedInt(reader, end);
    if (byteCount < 0 || byteCount > WorldFileFormatConstants.MaxTextBytes)
    {
      throw new InvalidDataException("The world file contains an invalid string length.");
    }

    return Utf8.GetString(ReadBoundedBytes(reader, byteCount, end));
  }

  private static string[] ReadBoundedStringList(
    BinaryReader reader,
    long end,
    string sectionName)
  {
    int count = ReadBoundedInt32(reader, end);
    if (count < 0 || count > WorldFileFormatConstants.MaxBestiaryEntries)
    {
      throw new InvalidDataException($"The world file contains an invalid {sectionName} count.");
    }

    string[] values = new string[count];
    for (int index = 0; index < values.Length; index++)
    {
      values[index] = ReadBoundedString(reader, end);
    }

    return values;
  }

  private static int Read7BitEncodedInt(BinaryReader reader, long end)
  {
    int value = 0;
    int shift = 0;
    while (shift < 35)
    {
      EnsureAvailable(reader, 1, end);
      byte current = reader.ReadByte();
      value |= (current & 0x7F) << shift;
      if ((current & 0x80) == 0)
      {
        return value;
      }

      shift += 7;
    }

    throw new InvalidDataException("The world file contains an invalid 7-bit string length.");
  }

  private static byte[] ReadBoundedBytes(BinaryReader reader, int count, long end)
  {
    EnsureAvailable(reader, count, end);
    byte[] value = reader.ReadBytes(count);
    if (value.Length != count)
    {
      throw new EndOfStreamException();
    }

    return value;
  }

  private static int ReadBoundedInt32(BinaryReader reader, long end)
  {
    EnsureAvailable(reader, sizeof(int), end);
    return reader.ReadInt32();
  }

  private static long ReadBoundedInt64(BinaryReader reader, long end)
  {
    EnsureAvailable(reader, sizeof(long), end);
    return reader.ReadInt64();
  }

  private static double ReadBoundedDouble(BinaryReader reader, long end)
  {
    EnsureAvailable(reader, sizeof(double), end);
    return reader.ReadDouble();
  }

  private static float ReadBoundedSingle(BinaryReader reader, long end)
  {
    EnsureAvailable(reader, sizeof(float), end);
    return reader.ReadSingle();
  }

  private static byte ReadBoundedByte(BinaryReader reader, long end)
  {
    EnsureAvailable(reader, sizeof(byte), end);
    return reader.ReadByte();
  }

  private static short ReadBoundedInt16(BinaryReader reader, long end)
  {
    EnsureAvailable(reader, sizeof(short), end);
    return reader.ReadInt16();
  }

  private static ushort ReadBoundedUInt16(BinaryReader reader, long end)
  {
    EnsureAvailable(reader, sizeof(ushort), end);
    return reader.ReadUInt16();
  }

  private static uint ReadBoundedUInt32(BinaryReader reader, long end)
  {
    EnsureAvailable(reader, sizeof(uint), end);
    return reader.ReadUInt32();
  }

  private static ulong ReadBoundedUInt64(BinaryReader reader, long end)
  {
    EnsureAvailable(reader, sizeof(ulong), end);
    return reader.ReadUInt64();
  }

  private static bool ReadBoundedBoolean(BinaryReader reader, long end, bool present = true)
  {
    return present && ReadBoundedBytes(reader, sizeof(byte), end)[0] != 0;
  }

  private static void EnsureAvailable(BinaryReader reader, int count, long end)
  {
    if (count < 0 || reader.BaseStream.Position > end - count)
    {
      throw new EndOfStreamException();
    }
  }

  private static WorldPersistenceDecodeResult Failed(
    string detail,
    Exception? exception = null)
  {
    if (exception is not null)
    {
      detail += $" {exception.Message}";
    }

    return WorldPersistenceDecodeResult.Failed(
      WorldStorageFailure.Create(WorldStorageFailureKind.InvalidData, detail));
  }
}
