using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Terraria.NonAuthoritative.Persistence;
using Terraria.WorldStorage;

namespace Terraria.NonAuthoritative.WorldStorage;

/// <summary>
/// Encodes the application persistence document into the pointer-based WorldFile format.
/// </summary>
/// <remarks>
/// The Tile payload remains compressed and opaque to this codec. Type-specific TileEntity and
/// CreativePowers records likewise remain opaque, while their section framing is preserved.
/// It never reads global runtime state or the exploration map.
/// </remarks>
public sealed class WorldFileDocumentEncoder : IWorldSaveEncoder
{
  private static readonly Encoding Utf8 = new UTF8Encoding(false, true);

  public WorldSaveEncodeResult Encode(WorldPersistenceDocument document)
  {
    ArgumentNullException.ThrowIfNull(document);
    try
    {
      if (document.FormatVersion != WorldFileFormatConstants.LatestWritableVersion)
      {
        return Failed(
          $"Only WorldFile version {WorldFileFormatConstants.LatestWritableVersion} can be encoded.");
      }

      if (!document.TryGetSection<WorldFileHeaderSection>(
            WorldFileHeaderSection.SectionId,
            out WorldLoadSection<WorldFileHeaderSection> headerSection) ||
          !headerSection.IsPresent)
      {
        return Failed("The world document does not contain a present world header section.");
      }

      WorldFileMetadataSection metadata = document.TryGetSection<WorldFileMetadataSection>(
        WorldFileMetadataSection.SectionId,
        out WorldLoadSection<WorldFileMetadataSection> metadataSection) &&
        metadataSection.IsPresent
          ? metadataSection.Value
          : WorldFileMetadataSection.Empty;

      IReadOnlyCollection<string> unsupportedSections = document.SectionIds
        .Where(sectionId => !string.Equals(
          sectionId,
          WorldFileHeaderSection.SectionId,
          StringComparison.Ordinal) &&
          !string.Equals(
            sectionId,
            WorldFileMetadataSection.SectionId,
            StringComparison.Ordinal) &&
          !string.Equals(
            sectionId,
            WorldFileEnvironmentSection.SectionId,
            StringComparison.Ordinal) &&
          !string.Equals(
            sectionId,
            WorldFileProgressionSection.SectionId,
            StringComparison.Ordinal) &&
          !string.Equals(
            sectionId,
            WorldFileQuestSection.SectionId,
            StringComparison.Ordinal) &&
          !string.Equals(
            sectionId,
            WorldFileBannerSection.SectionId,
            StringComparison.Ordinal) &&
          !string.Equals(
            sectionId,
            WorldFileBossProgressionSection.SectionId,
            StringComparison.Ordinal) &&
          !string.Equals(
            sectionId,
            WorldFilePartySection.SectionId,
            StringComparison.Ordinal) &&
          !string.Equals(
            sectionId,
            WorldFileSandstormSection.SectionId,
            StringComparison.Ordinal) &&
          !string.Equals(
            sectionId,
            WorldFileDefenderEventSection.SectionId,
            StringComparison.Ordinal) &&
          !string.Equals(
            sectionId,
            WorldFileBackgroundSection.SectionId,
            StringComparison.Ordinal) &&
          !string.Equals(
            sectionId,
            WorldFileEventSection.SectionId,
            StringComparison.Ordinal) &&
          !string.Equals(
            sectionId,
            WorldFileTreeTopsSection.SectionId,
            StringComparison.Ordinal) &&
          !string.Equals(
            sectionId,
            WorldFileSeasonalSection.SectionId,
            StringComparison.Ordinal) &&
          !string.Equals(
            sectionId,
            WorldFileNpcUnlockSection.SectionId,
            StringComparison.Ordinal) &&
          !string.Equals(
            sectionId,
            WorldFileTimePolicySection.SectionId,
            StringComparison.Ordinal) &&
          !string.Equals(
            sectionId,
            WorldFileSpawnSection.SectionId,
            StringComparison.Ordinal) &&
          !string.Equals(
            sectionId,
            WorldFileTilePayloadSection.SectionId,
            StringComparison.Ordinal) &&
          !string.Equals(
            sectionId,
            WorldFileChestSection.SectionId,
            StringComparison.Ordinal) &&
          !string.Equals(
            sectionId,
            WorldFileSignSection.SectionId,
            StringComparison.Ordinal) &&
          !string.Equals(
            sectionId,
            WorldFileNpcSection.SectionId,
            StringComparison.Ordinal) &&
          !string.Equals(
            sectionId,
            WorldFileTileEntitySection.SectionId,
            StringComparison.Ordinal) &&
          !string.Equals(
            sectionId,
            WorldFilePressurePlateSection.SectionId,
            StringComparison.Ordinal) &&
          !string.Equals(
            sectionId,
            WorldFileTownManagerSection.SectionId,
            StringComparison.Ordinal) &&
          !string.Equals(
            sectionId,
            WorldFileBestiarySection.SectionId,
            StringComparison.Ordinal) &&
          !string.Equals(
            sectionId,
            WorldFileCreativePowersSection.SectionId,
            StringComparison.Ordinal) &&
          !string.Equals(
            sectionId,
            WorldFileFooterSection.SectionId,
            StringComparison.Ordinal))
        .ToArray();
      if (unsupportedSections.Count > 0)
      {
        return Failed(
          "The WorldFile codec cannot encode unknown sections: " +
          string.Join(", ", unsupportedSections));
      }

      WorldFileEnvironmentSection environment =
        document.TryGetSection<WorldFileEnvironmentSection>(
          WorldFileEnvironmentSection.SectionId,
          out WorldLoadSection<WorldFileEnvironmentSection> environmentSection) &&
        environmentSection.IsPresent
          ? environmentSection.Value
          : WorldFileEnvironmentSection.Empty;
      if (!document.TryGetSection<WorldFileTilePayloadSection>(
            WorldFileTilePayloadSection.SectionId,
            out WorldLoadSection<WorldFileTilePayloadSection> tilePayloadSection) ||
          !tilePayloadSection.IsPresent)
      {
        return Failed("A complete WorldFile requires a present Tile payload section.");
      }

      WorldFileTilePayloadSection tilePayload = tilePayloadSection.Value;
      if (!document.TryGetSection<WorldFileFooterSection>(
            WorldFileFooterSection.SectionId,
            out WorldLoadSection<WorldFileFooterSection> footerSection) ||
          !footerSection.IsPresent)
      {
        return Failed("A complete WorldFile requires a present Footer section.");
      }

      WorldFileFooterSection footer = footerSection.Value;
      if (!footer.IsComplete ||
          !string.Equals(
            footer.WorldName,
            headerSection.Value.WorldName,
            StringComparison.Ordinal) ||
          footer.WorldId != headerSection.Value.WorldId)
      {
        return Failed("The WorldFile footer must match the complete world header.");
      }
      bool hasProgression = document.TryGetSection<WorldFileProgressionSection>(
        WorldFileProgressionSection.SectionId,
        out WorldLoadSection<WorldFileProgressionSection> progressionSection) &&
        progressionSection.IsPresent;
      WorldFileProgressionSection progression = hasProgression
        ? progressionSection.Value
        : WorldFileProgressionSection.Empty;
      bool hasQuests = document.TryGetSection<WorldFileQuestSection>(
        WorldFileQuestSection.SectionId,
        out WorldLoadSection<WorldFileQuestSection> questSection) &&
        questSection.IsPresent;
      WorldFileQuestSection quests = hasQuests
        ? questSection.Value
        : WorldFileQuestSection.Empty;
      bool hasBanners = document.TryGetSection<WorldFileBannerSection>(
        WorldFileBannerSection.SectionId,
        out WorldLoadSection<WorldFileBannerSection> bannerSection) &&
        bannerSection.IsPresent;
      WorldFileBannerSection banners = hasBanners
        ? bannerSection.Value
        : WorldFileBannerSection.Empty;
      bool hasBossProgression = document.TryGetSection<WorldFileBossProgressionSection>(
        WorldFileBossProgressionSection.SectionId,
        out WorldLoadSection<WorldFileBossProgressionSection> bossProgressionSection) &&
        bossProgressionSection.IsPresent;
      WorldFileBossProgressionSection bossProgression = hasBossProgression
        ? bossProgressionSection.Value
        : WorldFileBossProgressionSection.Empty;
      bool hasParty = document.TryGetSection<WorldFilePartySection>(
        WorldFilePartySection.SectionId,
        out WorldLoadSection<WorldFilePartySection> partySection) &&
        partySection.IsPresent;
      WorldFilePartySection party = hasParty
        ? partySection.Value
        : WorldFilePartySection.Empty;
      bool hasSandstorm = document.TryGetSection<WorldFileSandstormSection>(
        WorldFileSandstormSection.SectionId,
        out WorldLoadSection<WorldFileSandstormSection> sandstormSection) &&
        sandstormSection.IsPresent;
      WorldFileSandstormSection sandstorm = hasSandstorm
        ? sandstormSection.Value
        : WorldFileSandstormSection.Empty;
      bool hasDefenderEvent = document.TryGetSection<WorldFileDefenderEventSection>(
        WorldFileDefenderEventSection.SectionId,
        out WorldLoadSection<WorldFileDefenderEventSection> defenderEventSection) &&
        defenderEventSection.IsPresent;
      WorldFileDefenderEventSection defenderEvent = hasDefenderEvent
        ? defenderEventSection.Value
        : WorldFileDefenderEventSection.Empty;
      bool hasBackgrounds = document.TryGetSection<WorldFileBackgroundSection>(
        WorldFileBackgroundSection.SectionId,
        out WorldLoadSection<WorldFileBackgroundSection> backgroundSection) &&
        backgroundSection.IsPresent;
      WorldFileBackgroundSection backgrounds = hasBackgrounds
        ? backgroundSection.Value
        : WorldFileBackgroundSection.Empty;
      bool hasEvents = document.TryGetSection<WorldFileEventSection>(
        WorldFileEventSection.SectionId,
        out WorldLoadSection<WorldFileEventSection> eventSection) &&
        eventSection.IsPresent;
      WorldFileEventSection events = hasEvents
        ? eventSection.Value
        : WorldFileEventSection.Empty;
      bool hasTreeTops = document.TryGetSection<WorldFileTreeTopsSection>(
        WorldFileTreeTopsSection.SectionId,
        out WorldLoadSection<WorldFileTreeTopsSection> treeTopsSection) &&
        treeTopsSection.IsPresent;
      WorldFileTreeTopsSection treeTops = hasTreeTops
        ? treeTopsSection.Value
        : WorldFileTreeTopsSection.Empty;
      bool hasSeasonal = document.TryGetSection<WorldFileSeasonalSection>(
        WorldFileSeasonalSection.SectionId,
        out WorldLoadSection<WorldFileSeasonalSection> seasonalSection) &&
        seasonalSection.IsPresent;
      WorldFileSeasonalSection seasonal = hasSeasonal
        ? seasonalSection.Value
        : WorldFileSeasonalSection.Empty;
      bool hasNpcUnlocks = document.TryGetSection<WorldFileNpcUnlockSection>(
        WorldFileNpcUnlockSection.SectionId,
        out WorldLoadSection<WorldFileNpcUnlockSection> npcUnlockSection) &&
        npcUnlockSection.IsPresent;
      WorldFileNpcUnlockSection npcUnlocks = hasNpcUnlocks
        ? npcUnlockSection.Value
        : WorldFileNpcUnlockSection.Empty;
      bool hasTimePolicy = document.TryGetSection<WorldFileTimePolicySection>(
        WorldFileTimePolicySection.SectionId,
        out WorldLoadSection<WorldFileTimePolicySection> timePolicySection) &&
        timePolicySection.IsPresent;
      WorldFileTimePolicySection timePolicy = hasTimePolicy
        ? timePolicySection.Value
        : WorldFileTimePolicySection.Empty;
      bool hasSpawn = document.TryGetSection<WorldFileSpawnSection>(
        WorldFileSpawnSection.SectionId,
        out WorldLoadSection<WorldFileSpawnSection> spawnSection) &&
        spawnSection.IsPresent;
      WorldFileSpawnSection spawn = hasSpawn
        ? spawnSection.Value
        : WorldFileSpawnSection.Empty;
      WorldFileChestSection chests = document.TryGetSection<WorldFileChestSection>(
        WorldFileChestSection.SectionId,
        out WorldLoadSection<WorldFileChestSection> chestSection) &&
        chestSection.IsPresent
          ? chestSection.Value
          : WorldFileChestSection.Empty;
      WorldFileSignSection signs = document.TryGetSection<WorldFileSignSection>(
        WorldFileSignSection.SectionId,
        out WorldLoadSection<WorldFileSignSection> signSection) &&
        signSection.IsPresent
          ? signSection.Value
          : WorldFileSignSection.Empty;
      WorldFileNpcSection npcs = document.TryGetSection<WorldFileNpcSection>(
        WorldFileNpcSection.SectionId,
        out WorldLoadSection<WorldFileNpcSection> npcSection) &&
        npcSection.IsPresent
          ? npcSection.Value
          : WorldFileNpcSection.Empty;
      WorldFileTileEntitySection tileEntities =
        document.TryGetSection<WorldFileTileEntitySection>(
          WorldFileTileEntitySection.SectionId,
          out WorldLoadSection<WorldFileTileEntitySection> tileEntitySection) &&
        tileEntitySection.IsPresent
          ? tileEntitySection.Value
          : new WorldFileTileEntitySection(0, ReadOnlyMemory<byte>.Empty);
      WorldFilePressurePlateSection pressurePlates =
        document.TryGetSection<WorldFilePressurePlateSection>(
          WorldFilePressurePlateSection.SectionId,
          out WorldLoadSection<WorldFilePressurePlateSection> pressurePlateSection) &&
        pressurePlateSection.IsPresent
          ? pressurePlateSection.Value
          : WorldFilePressurePlateSection.Empty;
      WorldFileTownManagerSection townManager =
        document.TryGetSection<WorldFileTownManagerSection>(
          WorldFileTownManagerSection.SectionId,
          out WorldLoadSection<WorldFileTownManagerSection> townManagerSection) &&
        townManagerSection.IsPresent
          ? townManagerSection.Value
          : WorldFileTownManagerSection.Empty;
      WorldFileBestiarySection bestiary = document.TryGetSection<WorldFileBestiarySection>(
        WorldFileBestiarySection.SectionId,
        out WorldLoadSection<WorldFileBestiarySection> bestiarySection) &&
        bestiarySection.IsPresent
          ? bestiarySection.Value
          : WorldFileBestiarySection.Empty;
      WorldFileCreativePowersSection creativePowers =
        document.TryGetSection<WorldFileCreativePowersSection>(
          WorldFileCreativePowersSection.SectionId,
          out WorldLoadSection<WorldFileCreativePowersSection> creativePowersSection) &&
        creativePowersSection.IsPresent
          ? creativePowersSection.Value
          : WorldFileCreativePowersSection.Empty;
      ValidateCreativePowers(creativePowers);
      if (hasQuests)
      {
        hasProgression = true;
      }
      if (hasBanners)
      {
        hasProgression = true;
        hasQuests = true;
      }
      if (hasBossProgression)
      {
        hasProgression = true;
        hasQuests = true;
        hasBanners = true;
      }
      if (hasParty)
      {
        hasProgression = true;
        hasQuests = true;
        hasBanners = true;
        hasBossProgression = true;
      }
      if (hasSandstorm)
      {
        hasProgression = true;
        hasQuests = true;
        hasBanners = true;
        hasBossProgression = true;
        hasParty = true;
      }
      if (hasDefenderEvent)
      {
        hasProgression = true;
        hasQuests = true;
        hasBanners = true;
        hasBossProgression = true;
        hasParty = true;
        hasSandstorm = true;
      }
      if (hasBackgrounds)
      {
        hasProgression = true;
        hasQuests = true;
        hasBanners = true;
        hasBossProgression = true;
        hasParty = true;
        hasSandstorm = true;
        hasDefenderEvent = true;
      }
      if (hasEvents)
      {
        hasProgression = true;
        hasQuests = true;
        hasBanners = true;
        hasBossProgression = true;
        hasParty = true;
        hasSandstorm = true;
        hasDefenderEvent = true;
        hasBackgrounds = true;
      }
      if (hasTreeTops)
      {
        hasProgression = true;
        hasQuests = true;
        hasBanners = true;
        hasBossProgression = true;
        hasParty = true;
        hasSandstorm = true;
        hasDefenderEvent = true;
        hasBackgrounds = true;
        hasEvents = true;
      }
      if (hasSeasonal)
      {
        hasProgression = true;
        hasQuests = true;
        hasBanners = true;
        hasBossProgression = true;
        hasParty = true;
        hasSandstorm = true;
        hasDefenderEvent = true;
        hasBackgrounds = true;
        hasEvents = true;
        hasTreeTops = true;
      }
      if (hasNpcUnlocks)
      {
        hasProgression = true;
        hasQuests = true;
        hasBanners = true;
        hasBossProgression = true;
        hasParty = true;
        hasSandstorm = true;
        hasDefenderEvent = true;
        hasBackgrounds = true;
        hasEvents = true;
        hasTreeTops = true;
        hasSeasonal = true;
      }
      if (hasTimePolicy)
      {
        hasProgression = true;
        hasQuests = true;
        hasBanners = true;
        hasBossProgression = true;
        hasParty = true;
        hasSandstorm = true;
        hasDefenderEvent = true;
        hasBackgrounds = true;
        hasEvents = true;
        hasTreeTops = true;
        hasSeasonal = true;
        hasNpcUnlocks = true;
      }
      if (hasSpawn)
      {
        hasProgression = true;
        hasQuests = true;
        hasBanners = true;
        hasBossProgression = true;
        hasParty = true;
        hasSandstorm = true;
        hasDefenderEvent = true;
        hasBackgrounds = true;
        hasEvents = true;
        hasTreeTops = true;
        hasSeasonal = true;
        hasNpcUnlocks = true;
        hasTimePolicy = true;
      }
      return WorldSaveEncodeResult.Success(
        EncodeWorldFile(
          headerSection.Value,
          metadata,
          environment,
          progression,
          hasProgression,
          quests,
          hasQuests,
          banners,
          hasBanners,
          bossProgression,
          hasBossProgression,
          party,
          hasParty,
          sandstorm,
          hasSandstorm,
          defenderEvent,
          hasDefenderEvent,
          backgrounds,
          hasBackgrounds,
          events,
          hasEvents,
          treeTops,
          hasTreeTops,
          seasonal,
          hasSeasonal,
          npcUnlocks,
          hasNpcUnlocks,
          timePolicy,
          hasTimePolicy,
          spawn,
          hasSpawn,
          tilePayload,
          chests,
          signs,
          npcs,
          tileEntities,
          pressurePlates,
          townManager,
          bestiary,
          creativePowers,
          footer));
    }
    catch (InvalidDataException exception)
    {
      return Failed(exception.Message);
    }
    catch (EncoderFallbackException exception)
    {
      return Failed("The world file contains text that cannot be encoded as UTF-8. " +
        exception.Message);
    }
    catch (IOException exception)
    {
      return Failed("The world file could not be encoded. " + exception.Message);
    }
  }

  private static byte[] EncodeWorldFile(
    WorldFileHeaderSection header,
    WorldFileMetadataSection metadata,
    WorldFileEnvironmentSection environment,
    WorldFileProgressionSection progression,
    bool hasProgression,
    WorldFileQuestSection quests,
    bool hasQuests,
    WorldFileBannerSection banners,
    bool hasBanners,
    WorldFileBossProgressionSection bossProgression,
    bool hasBossProgression,
    WorldFilePartySection party,
    bool hasParty,
    WorldFileSandstormSection sandstorm,
    bool hasSandstorm,
    WorldFileDefenderEventSection defenderEvent,
    bool hasDefenderEvent,
    WorldFileBackgroundSection backgrounds,
    bool hasBackgrounds,
    WorldFileEventSection events,
    bool hasEvents,
    WorldFileTreeTopsSection treeTops,
    bool hasTreeTops,
    WorldFileSeasonalSection seasonal,
    bool hasSeasonal,
    WorldFileNpcUnlockSection npcUnlocks,
    bool hasNpcUnlocks,
    WorldFileTimePolicySection timePolicy,
    bool hasTimePolicy,
    WorldFileSpawnSection spawn,
    bool hasSpawn,
    WorldFileTilePayloadSection tilePayload,
    WorldFileChestSection chests,
    WorldFileSignSection signs,
    WorldFileNpcSection npcs,
    WorldFileTileEntitySection tileEntities,
    WorldFilePressurePlateSection pressurePlates,
    WorldFileTownManagerSection townManager,
    WorldFileBestiarySection bestiary,
    WorldFileCreativePowersSection creativePowers,
    WorldFileFooterSection footer)
  {
    ValidateHeader(header);
    ValidateEnvironment(environment, header);
    ValidateTilePayload(tilePayload, header);
    ValidateFooter(footer, header);

    using MemoryStream stream = new();
    using (BinaryWriter writer = new(stream, Utf8, leaveOpen: true))
    {
      writer.Write(WorldFileFormatConstants.LatestWritableVersion);
      writer.Write(WorldFileFormatConstants.MetadataMagic |
        ((ulong)WorldFileFormatConstants.WorldFileType << 56));
      writer.Write(metadata.Revision);
      writer.Write(metadata.IsFavorite ? 1UL : 0UL);

      const short sectionCount = WorldFileFormatConstants.MaxSectionPointers;
      writer.Write(sectionCount);
      long pointerStart = stream.Position;
      for (int index = 0; index < sectionCount; index++)
      {
        writer.Write(0);
      }

      WriteImportanceTable(writer, tilePayload.FrameImportant);

      int headerStart = checked((int)stream.Position);
      writer.Write(header.WorldName);
      writer.Write(header.SeedText ?? string.Empty);
      writer.Write(header.WorldGeneratorVersion ?? 0UL);
      writer.Write((header.UniqueId ?? Guid.Empty).ToByteArray());
      writer.Write(header.WorldId);
      writer.Write(header.LeftWorld);
      writer.Write(header.RightWorld);
      writer.Write(header.TopWorld);
      writer.Write(header.BottomWorld);
      writer.Write(header.MaxTilesY);
      writer.Write(header.MaxTilesX);
      writer.Write(header.GameMode);
      writer.Write(header.DrunkWorld);
      writer.Write(header.GetGoodWorld);
      writer.Write(header.TenthAnniversaryWorld);
      writer.Write(header.DontStarveWorld);
      writer.Write(header.NotTheBeesWorld);
      writer.Write(header.RemixWorld);
      writer.Write(header.NoTrapsWorld);
      writer.Write(header.ZenithWorld);
      writer.Write(header.SkyblockWorld);
      writer.Write((header.CreationTime ?? DateTime.UnixEpoch).ToBinary());
      writer.Write((header.LastPlayed ?? DateTime.UnixEpoch).ToBinary());
      WriteEnvironment(writer, environment);
      WriteProgression(writer, progression);
      WriteQuests(writer, quests);
      WriteBanners(writer, banners);
      WriteBossProgression(writer, bossProgression);
      WriteParty(writer, party);
      WriteSandstorm(writer, sandstorm);
      WriteDefenderEvent(writer, defenderEvent);
      WriteBackgrounds(writer, backgrounds);
      WriteEvents(writer, events);
      WriteTreeTops(writer, treeTops);
      WriteSeasonal(writer, seasonal);
      WriteNpcUnlocks(writer, npcUnlocks);
      WriteTimePolicy(writer, timePolicy);
      WriteSpawn(writer, spawn);

      int headerEnd = checked((int)stream.Position);
      writer.Write(tilePayload.CompressedPayload.Span);
      int tileEnd = checked((int)stream.Position);
      WriteChests(writer, chests);
      int chestEnd = checked((int)stream.Position);
      WriteSigns(writer, signs);
      int signEnd = checked((int)stream.Position);
      WriteNpcs(writer, npcs);
      int npcEnd = checked((int)stream.Position);
      WriteTileEntities(writer, tileEntities);
      int tileEntityEnd = checked((int)stream.Position);
      WritePressurePlates(writer, pressurePlates);
      int pressurePlateEnd = checked((int)stream.Position);
      WriteTownManager(writer, townManager);
      int townManagerEnd = checked((int)stream.Position);
      WriteBestiary(writer, bestiary);
      int bestiaryEnd = checked((int)stream.Position);
      writer.Write(creativePowers.SerializedPayload.Span);
      int creativePowersEnd = checked((int)stream.Position);
      WriteFooter(writer, footer);

      long endPosition = stream.Position;
      stream.Position = pointerStart;
      writer.Write(headerStart);
      writer.Write(headerEnd);
      writer.Write(tileEnd);
      writer.Write(chestEnd);
      writer.Write(signEnd);
      writer.Write(npcEnd);
      writer.Write(tileEntityEnd);
      writer.Write(pressurePlateEnd);
      writer.Write(townManagerEnd);
      writer.Write(bestiaryEnd);
      writer.Write(creativePowersEnd);
      stream.Position = endPosition;
      writer.Flush();
    }

    return stream.ToArray();
  }

  private static void WriteImportanceTable(
    BinaryWriter writer,
    IReadOnlyList<bool> frameImportant)
  {
    if (frameImportant.Count > ushort.MaxValue)
    {
      throw new InvalidDataException(
        "The Tile frame-importance table is too large.");
    }

    writer.Write((ushort)frameImportant.Count);
    byte packed = 0;
    for (int index = 0; index < frameImportant.Count; index++)
    {
      if (frameImportant[index])
      {
        packed |= (byte)(1 << (index % 8));
      }

      if (index % 8 == 7 || index == frameImportant.Count - 1)
      {
        writer.Write(packed);
        packed = 0;
      }
    }
  }

  private static void WriteChests(
    BinaryWriter writer,
    WorldFileChestSection chests)
  {
    if (chests.Chests.Count > short.MaxValue)
    {
      throw new InvalidDataException("The chest section contains too many chests.");
    }

    writer.Write((short)chests.Chests.Count);
    for (int chestIndex = 0; chestIndex < chests.Chests.Count; chestIndex++)
    {
      WorldFileChestRecord chest = chests.Chests[chestIndex];
      ValidateChest(chest);
      writer.Write(chest.X);
      writer.Write(chest.Y);
      writer.Write(chest.Name);
      writer.Write(chest.Items.Count);
      for (int itemIndex = 0; itemIndex < chest.Items.Count; itemIndex++)
      {
        WorldFileChestItem item = chest.Items[itemIndex];
        writer.Write((short)item.Stack);
        if (!item.IsEmpty)
        {
          writer.Write(item.Type);
          writer.Write(item.Prefix);
        }
      }
    }
  }

  private static void WriteSigns(
    BinaryWriter writer,
    WorldFileSignSection signs)
  {
    if (signs.Signs.Count > short.MaxValue)
    {
      throw new InvalidDataException("The sign section contains too many signs.");
    }

    writer.Write((short)signs.Signs.Count);
    for (int index = 0; index < signs.Signs.Count; index++)
    {
      WorldFileSignRecord sign = signs.Signs[index];
      ValidateText(sign.Text, "A sign exceeds the supported UTF-8 length.");
      writer.Write(sign.Text);
      writer.Write(sign.X);
      writer.Write(sign.Y);
    }
  }

  private static void WriteNpcs(
    BinaryWriter writer,
    WorldFileNpcSection npcs)
  {
    if (npcs.ShimmeredTownNpcIds.Count > WorldFileFormatConstants.MaxShimmeredNpcIds ||
        npcs.TownNpcs.Count > WorldFileFormatConstants.MaxNpcRecords ||
        npcs.SavedNpcs.Count > WorldFileFormatConstants.MaxNpcRecords)
    {
      throw new InvalidDataException("The NPC section contains too many records.");
    }

    writer.Write(npcs.ShimmeredTownNpcIds.Count);
    for (int index = 0; index < npcs.ShimmeredTownNpcIds.Count; index++)
    {
      int npcId = npcs.ShimmeredTownNpcIds[index];
      if (npcId < 0)
      {
        throw new InvalidDataException("The NPC section contains a negative shimmered ID.");
      }

      writer.Write(npcId);
    }

    WriteNpcRecords(writer, npcs.TownNpcs, isTownNpc: true);
    WriteNpcRecords(writer, npcs.SavedNpcs, isTownNpc: false);
  }

  private static void WriteNpcRecords(
    BinaryWriter writer,
    IReadOnlyList<WorldFileNpcRecord> records,
    bool isTownNpc)
  {
    for (int index = 0; index < records.Count; index++)
    {
      WorldFileNpcRecord record = records[index];
      ValidateNpc(record, isTownNpc);
      writer.Write(true);
      if (record.NetId is int netId)
      {
        writer.Write(netId);
      }
      else
      {
        writer.Write(record.LegacyTypeName ?? string.Empty);
      }

      if (isTownNpc)
      {
        writer.Write(record.Name);
      }

      writer.Write(record.PositionX);
      writer.Write(record.PositionY);
      if (isTownNpc)
      {
        writer.Write(record.Homeless);
        writer.Write(record.HomeTileX);
        writer.Write(record.HomeTileY);
        writer.Write((byte)(record.TownNpcVariationIndex.HasValue ? 1 : 0));
        if (record.TownNpcVariationIndex is int variationIndex)
        {
          writer.Write(variationIndex);
        }

        writer.Write(record.HomelessDespawn);
      }
    }

    writer.Write(false);
  }

  private static void WriteTileEntities(
    BinaryWriter writer,
    WorldFileTileEntitySection tileEntities)
  {
    if (tileEntities.EntityCount > WorldFileFormatConstants.MaxTileEntities ||
        tileEntities.SerializedRecords.Length > WorldFileFormatConstants.MaxRawSectionBytes)
    {
      throw new InvalidDataException("The TileEntity section exceeds the supported bounds.");
    }

    if ((tileEntities.EntityCount == 0 && !tileEntities.SerializedRecords.IsEmpty) ||
        (tileEntities.EntityCount > 0 && tileEntities.SerializedRecords.IsEmpty))
    {
      throw new InvalidDataException(
        "The TileEntity count does not match its opaque record payload.");
    }

    writer.Write(tileEntities.EntityCount);
    writer.Write(tileEntities.SerializedRecords.Span);
  }

  private static void WritePressurePlates(
    BinaryWriter writer,
    WorldFilePressurePlateSection pressurePlates)
  {
    if (pressurePlates.Plates.Count > WorldFileFormatConstants.MaxPressurePlates)
    {
      throw new InvalidDataException(
        "The pressure plate section contains too many records.");
    }

    writer.Write(pressurePlates.Plates.Count);
    for (int index = 0; index < pressurePlates.Plates.Count; index++)
    {
      WorldFilePressurePlateRecord plate = pressurePlates.Plates[index];
      writer.Write(plate.X);
      writer.Write(plate.Y);
    }
  }

  private static void WriteTownManager(
    BinaryWriter writer,
    WorldFileTownManagerSection townManager)
  {
    if (townManager.Rooms.Count > WorldFileFormatConstants.MaxTownRooms)
    {
      throw new InvalidDataException("The TownManager section contains too many rooms.");
    }

    writer.Write(townManager.Rooms.Count);
    for (int index = 0; index < townManager.Rooms.Count; index++)
    {
      WorldFileTownRoomRecord room = townManager.Rooms[index];
      writer.Write(room.NpcType);
      writer.Write(room.TileX);
      writer.Write(room.TileY);
    }
  }

  private static void WriteBestiary(
    BinaryWriter writer,
    WorldFileBestiarySection bestiary)
  {
    if (bestiary.KillCounts.Count > WorldFileFormatConstants.MaxBestiaryEntries ||
        bestiary.SeenNpcIds.Count > WorldFileFormatConstants.MaxBestiaryEntries ||
        bestiary.ChattedNpcIds.Count > WorldFileFormatConstants.MaxBestiaryEntries)
    {
      throw new InvalidDataException("The Bestiary section contains too many entries.");
    }

    writer.Write(bestiary.KillCounts.Count);
    for (int index = 0; index < bestiary.KillCounts.Count; index++)
    {
      WorldFileBestiaryKillCount kill = bestiary.KillCounts[index];
      ValidateText(kill.PersistentId, "A Bestiary identifier exceeds the supported UTF-8 length.");
      writer.Write(kill.PersistentId);
      writer.Write(kill.Count);
    }

    WriteTextList(writer, bestiary.SeenNpcIds);
    WriteTextList(writer, bestiary.ChattedNpcIds);
  }

  private static void WriteTextList(
    BinaryWriter writer,
    IReadOnlyList<string> values)
  {
    writer.Write(values.Count);
    for (int index = 0; index < values.Count; index++)
    {
      ValidateText(values[index], "A Bestiary identifier exceeds the supported UTF-8 length.");
      writer.Write(values[index]);
    }
  }

  private static void WriteFooter(
    BinaryWriter writer,
    WorldFileFooterSection footer)
  {
    ValidateText(footer.WorldName, "The WorldFile footer name exceeds the supported UTF-8 length.");
    writer.Write(footer.IsComplete);
    writer.Write(footer.WorldName);
    writer.Write(footer.WorldId);
  }

  private static void ValidateCreativePowers(WorldFileCreativePowersSection creativePowers)
  {
    ReadOnlySpan<byte> payload = creativePowers.SerializedPayload.Span;
    if (payload.IsEmpty || payload[^1] != 0)
    {
      throw new InvalidDataException(
        "The CreativePowers payload must be non-empty and end with its terminator.");
    }
  }

  private static void WriteEnvironment(
    BinaryWriter writer,
    WorldFileEnvironmentSection environment)
  {
    writer.Write(environment.MoonType);
    WriteFixedInt32Values(writer, environment.TreeX);
    WriteFixedInt32Values(writer, environment.TreeStyle);
    WriteFixedInt32Values(writer, environment.CaveBackX);
    WriteFixedInt32Values(writer, environment.CaveBackStyle);
    writer.Write(environment.IceBackStyle);
    writer.Write(environment.JungleBackStyle);
    writer.Write(environment.HellBackStyle);
    writer.Write(environment.SpawnTileX);
    writer.Write(environment.SpawnTileY);
    writer.Write(environment.WorldSurface);
    writer.Write(environment.RockLayer);
    writer.Write(environment.Time);
    writer.Write(environment.DayTime);
    writer.Write(environment.MoonPhase);
    writer.Write(environment.BloodMoon);
    writer.Write(environment.Eclipse);
    writer.Write(environment.DungeonX);
    writer.Write(environment.DungeonY);
    writer.Write(environment.Crimson);
  }

  private static void WriteProgression(
    BinaryWriter writer,
    WorldFileProgressionSection progression)
  {
    ValidateProgression(progression);
    writer.Write(progression.DownedBoss1);
    writer.Write(progression.DownedBoss2);
    writer.Write(progression.DownedBoss3);
    writer.Write(progression.DownedQueenBee);
    writer.Write(progression.DownedMechBoss1);
    writer.Write(progression.DownedMechBoss2);
    writer.Write(progression.DownedMechBoss3);
    writer.Write(progression.DownedMechBossAny);
    writer.Write(progression.DownedPlantBoss);
    writer.Write(progression.DownedGolemBoss);
    writer.Write(progression.DownedSlimeKing);
    writer.Write(progression.SavedGoblin);
    writer.Write(progression.SavedWizard);
    writer.Write(progression.SavedMech);
    writer.Write(progression.DownedGoblins);
    writer.Write(progression.DownedClown);
    writer.Write(progression.DownedFrost);
    writer.Write(progression.DownedPirates);
    writer.Write(progression.ShadowOrbSmashed);
    writer.Write(progression.SpawnMeteor);
    writer.Write(progression.ShadowOrbCount);
    writer.Write(progression.AltarCount);
    writer.Write(progression.HardMode);
    writer.Write(progression.AfterPartyOfDoom);
    writer.Write(progression.InvasionDelay);
    writer.Write(progression.InvasionSize);
    writer.Write(progression.InvasionType);
    writer.Write(progression.InvasionX);
    writer.Write(progression.SlimeRainTime);
    writer.Write(progression.SundialCooldown);
    writer.Write(progression.Raining);
    writer.Write(progression.RainTime);
    writer.Write(progression.MaxRain);
    writer.Write(progression.CobaltOreTier);
    writer.Write(progression.MythrilOreTier);
    writer.Write(progression.AdamantiteOreTier);
    for (int index = 0; index < progression.BackgroundStyles.Count; index++)
    {
      writer.Write(progression.BackgroundStyles[index]);
    }

    writer.Write(progression.CloudBackgroundActive);
    writer.Write(progression.CloudCount);
    writer.Write(progression.WindSpeedTarget);
  }

  private static void WriteQuests(
    BinaryWriter writer,
    WorldFileQuestSection quests)
  {
    ValidateQuests(quests);
    writer.Write(quests.AnglerWhoFinishedToday.Count);
    for (int index = 0; index < quests.AnglerWhoFinishedToday.Count; index++)
    {
      writer.Write(quests.AnglerWhoFinishedToday[index]);
    }

    writer.Write(quests.SavedAngler);
    writer.Write(quests.AnglerQuest);
    writer.Write(quests.SavedStylist);
    writer.Write(quests.SavedTaxCollector);
    writer.Write(quests.SavedGolfer);
    writer.Write(quests.InvasionSizeStart);
    writer.Write(quests.CultistDelay);
  }

  private static void WriteBanners(
    BinaryWriter writer,
    WorldFileBannerSection banners)
  {
    ValidateBanners(banners);
    writer.Write((short)banners.KillCounts.Count);
    for (int index = 0; index < banners.KillCounts.Count; index++)
    {
      writer.Write(banners.KillCounts[index]);
    }

    writer.Write((short)banners.ClaimableCounts.Count);
    for (int index = 0; index < banners.ClaimableCounts.Count; index++)
    {
      writer.Write(banners.ClaimableCounts[index]);
    }
  }

  private static void WriteBossProgression(
    BinaryWriter writer,
    WorldFileBossProgressionSection bossProgression)
  {
    writer.Write(bossProgression.FastForwardTimeToDawn);
    writer.Write(bossProgression.DownedFishron);
    writer.Write(bossProgression.DownedMartians);
    writer.Write(bossProgression.DownedAncientCultist);
    writer.Write(bossProgression.DownedMoonlord);
    writer.Write(bossProgression.DownedHalloweenKing);
    writer.Write(bossProgression.DownedHalloweenTree);
    writer.Write(bossProgression.DownedChristmasIceQueen);
    writer.Write(bossProgression.DownedChristmasSantank);
    writer.Write(bossProgression.DownedChristmasTree);
    writer.Write(bossProgression.DownedTowerSolar);
    writer.Write(bossProgression.DownedTowerVortex);
    writer.Write(bossProgression.DownedTowerNebula);
    writer.Write(bossProgression.DownedTowerStardust);
    writer.Write(bossProgression.TowerActiveSolar);
    writer.Write(bossProgression.TowerActiveVortex);
    writer.Write(bossProgression.TowerActiveNebula);
    writer.Write(bossProgression.TowerActiveStardust);
    writer.Write(bossProgression.LunarApocalypseIsUp);
  }

  private static void WriteParty(
    BinaryWriter writer,
    WorldFilePartySection party)
  {
    ValidateParty(party);
    writer.Write(party.Manual);
    writer.Write(party.Genuine);
    writer.Write(party.Cooldown);
    writer.Write(party.CelebratingNpcIds.Count);
    for (int index = 0; index < party.CelebratingNpcIds.Count; index++)
    {
      writer.Write(party.CelebratingNpcIds[index]);
    }
  }

  private static void WriteSandstorm(
    BinaryWriter writer,
    WorldFileSandstormSection sandstorm)
  {
    ValidateSandstorm(sandstorm);
    writer.Write(sandstorm.Happening);
    writer.Write(sandstorm.TimeLeft);
    writer.Write(sandstorm.Severity);
    writer.Write(sandstorm.IntendedSeverity);
  }

  private static void WriteDefenderEvent(
    BinaryWriter writer,
    WorldFileDefenderEventSection defenderEvent)
  {
    writer.Write(defenderEvent.SavedBartender);
    writer.Write(defenderEvent.DownedInvasionTier1);
    writer.Write(defenderEvent.DownedInvasionTier2);
    writer.Write(defenderEvent.DownedInvasionTier3);
  }

  private static void WriteBackgrounds(
    BinaryWriter writer,
    WorldFileBackgroundSection backgrounds)
  {
    ValidateBackgrounds(backgrounds);
    for (int index = 0; index < backgrounds.Styles.Count; index++)
    {
      writer.Write(backgrounds.Styles[index]);
    }
  }

  private static void WriteEvents(
    BinaryWriter writer,
    WorldFileEventSection events)
  {
    ValidateEvents(events);
    writer.Write(events.CombatBookWasUsed);
    writer.Write(events.LanternNightCooldown);
    writer.Write(events.LanternNightGenuine);
    writer.Write(events.LanternNightManual);
    writer.Write(events.LanternNightNextNightIsGenuine);
  }

  private static void WriteTreeTops(
    BinaryWriter writer,
    WorldFileTreeTopsSection treeTops)
  {
    ValidateTreeTops(treeTops);
    writer.Write(treeTops.Variations.Count);
    for (int index = 0; index < treeTops.Variations.Count; index++)
    {
      writer.Write(treeTops.Variations[index]);
    }
  }

  private static void WriteSeasonal(
    BinaryWriter writer,
    WorldFileSeasonalSection seasonal)
  {
    writer.Write(seasonal.ForceHalloweenForToday);
    writer.Write(seasonal.ForceChristmasForToday);
    writer.Write(seasonal.CopperOreTier);
    writer.Write(seasonal.IronOreTier);
    writer.Write(seasonal.SilverOreTier);
    writer.Write(seasonal.GoldOreTier);
  }

  private static void WriteNpcUnlocks(
    BinaryWriter writer,
    WorldFileNpcUnlockSection npcUnlocks)
  {
    writer.Write(npcUnlocks.BoughtCat);
    writer.Write(npcUnlocks.BoughtDog);
    writer.Write(npcUnlocks.BoughtBunny);
    writer.Write(npcUnlocks.DownedEmpressOfLight);
    writer.Write(npcUnlocks.DownedQueenSlime);
    writer.Write(npcUnlocks.DownedDeerclops);
    writer.Write(npcUnlocks.UnlockedSlimeBlueSpawn);
    writer.Write(npcUnlocks.UnlockedMerchantSpawn);
    writer.Write(npcUnlocks.UnlockedDemolitionistSpawn);
    writer.Write(npcUnlocks.UnlockedPartyGirlSpawn);
    writer.Write(npcUnlocks.UnlockedDyeTraderSpawn);
    writer.Write(npcUnlocks.UnlockedTruffleSpawn);
    writer.Write(npcUnlocks.UnlockedArmsDealerSpawn);
    writer.Write(npcUnlocks.UnlockedNurseSpawn);
    writer.Write(npcUnlocks.UnlockedPrincessSpawn);
    writer.Write(npcUnlocks.CombatBookVolumeTwoWasUsed);
    writer.Write(npcUnlocks.PeddlersSatchelWasUsed);
    writer.Write(npcUnlocks.UnlockedSlimeGreenSpawn);
    writer.Write(npcUnlocks.UnlockedSlimeOldSpawn);
    writer.Write(npcUnlocks.UnlockedSlimePurpleSpawn);
    writer.Write(npcUnlocks.UnlockedSlimeRainbowSpawn);
    writer.Write(npcUnlocks.UnlockedSlimeRedSpawn);
    writer.Write(npcUnlocks.UnlockedSlimeYellowSpawn);
    writer.Write(npcUnlocks.UnlockedSlimeCopperSpawn);
  }

  private static void WriteTimePolicy(
    BinaryWriter writer,
    WorldFileTimePolicySection timePolicy)
  {
    ValidateTimePolicy(timePolicy);
    writer.Write(timePolicy.FastForwardTimeToDusk);
    writer.Write(timePolicy.MoondialCooldown);
    writer.Write(timePolicy.ForceHalloweenForever);
    writer.Write(timePolicy.ForceChristmasForever);
    writer.Write(timePolicy.VampireSeed);
    writer.Write(timePolicy.InfectedSeed);
    writer.Write(timePolicy.MeteorShowerCount);
    writer.Write(timePolicy.CoinRain);
    writer.Write(timePolicy.TeamBasedSpawnsSeed);
  }

  private static void WriteSpawn(
    BinaryWriter writer,
    WorldFileSpawnSection spawn)
  {
    ValidateSpawn(spawn);
    writer.Write((byte)spawn.ExtraSpawnPoints.Count);
    for (int index = 0; index < spawn.ExtraSpawnPoints.Count; index++)
    {
      WorldFileExtraSpawnPoint point = spawn.ExtraSpawnPoints[index];
      writer.Write(point.X);
      writer.Write(point.Y);
    }

    writer.Write(spawn.DualDungeonsSeed);
    writer.Write(spawn.WorldManifestJson ?? string.Empty);
  }

  private static void WriteFixedInt32Values(
    BinaryWriter writer,
    IReadOnlyList<int> values)
  {
    for (int index = 0; index < values.Count; index++)
    {
      writer.Write(values[index]);
    }
  }

  private static void ValidateTilePayload(
    WorldFileTilePayloadSection tilePayload,
    WorldFileHeaderSection header)
  {
    if (tilePayload.MaxTilesX != header.MaxTilesX ||
        tilePayload.MaxTilesY != header.MaxTilesY ||
        tilePayload.FrameImportant.Count == 0 ||
        tilePayload.CompressedPayload.IsEmpty ||
        tilePayload.CompressedPayload.Length > WorldFileFormatConstants.MaxRawSectionBytes)
    {
      throw new InvalidDataException(
        "The Tile payload dimensions or size do not match the world header.");
    }
  }

  private static void ValidateFooter(
    WorldFileFooterSection footer,
    WorldFileHeaderSection header)
  {
    if (!footer.IsComplete ||
        footer.WorldId != header.WorldId ||
        !string.Equals(footer.WorldName, header.WorldName, StringComparison.Ordinal))
    {
      throw new InvalidDataException(
        "The WorldFile footer is incomplete or does not match the world header.");
    }
  }

  private static void ValidateChest(WorldFileChestRecord chest)
  {
    ValidateText(chest.Name, "A chest name exceeds the supported UTF-8 length.");
    if (chest.Items.Count > WorldFileFormatConstants.MaxChestItems)
    {
      throw new InvalidDataException("A chest contains too many item slots.");
    }

    for (int index = 0; index < chest.Items.Count; index++)
    {
      WorldFileChestItem item = chest.Items[index];
      if (item.Stack < short.MinValue || item.Stack > short.MaxValue)
      {
        throw new InvalidDataException("A chest contains an invalid item.");
      }
    }
  }

  private static void ValidateNpc(WorldFileNpcRecord record, bool isTownNpc)
  {
    if (record.NetId is not int netId || netId < 0 ||
        record.LegacyTypeName is not null ||
        !float.IsFinite(record.PositionX) ||
        !float.IsFinite(record.PositionY))
    {
      throw new InvalidDataException(
        "The current WorldFile version requires a numeric NPC net ID.");
    }

    if (isTownNpc)
    {
      ValidateText(record.Name, "An NPC name exceeds the supported UTF-8 length.");
    }
  }

  private static void ValidateText(string value, string message)
  {
    if (Utf8.GetByteCount(value) > WorldFileFormatConstants.MaxTextBytes)
    {
      throw new InvalidDataException(message);
    }
  }

  private static void ValidateHeader(WorldFileHeaderSection header)
  {
    if (string.IsNullOrWhiteSpace(header.WorldName))
    {
      throw new InvalidDataException("The world header requires a world name.");
    }

    int nameByteCount = Utf8.GetByteCount(header.WorldName);
    if (nameByteCount > WorldFileFormatConstants.MaxTextBytes)
    {
      throw new InvalidDataException("The world name exceeds the supported UTF-8 length.");
    }

    if (header.SeedText is not null &&
        Utf8.GetByteCount(header.SeedText) > WorldFileFormatConstants.MaxTextBytes)
    {
      throw new InvalidDataException("The world seed exceeds the supported UTF-8 length.");
    }

    if (header.MaxTilesX <= 0 || header.MaxTilesY <= 0 ||
        header.LeftWorld >= header.RightWorld || header.TopWorld >= header.BottomWorld)
    {
      throw new InvalidDataException("The world header contains invalid bounds or dimensions.");
    }

    if (header.GameMode is < 0 or > WorldFileFormatConstants.MaximumGameMode)
    {
      throw new InvalidDataException("The world header contains an unsupported game mode.");
    }
  }

  private static void ValidateEnvironment(
    WorldFileEnvironmentSection environment,
    WorldFileHeaderSection header)
  {
    if (environment.SpawnTileX < 0 || environment.SpawnTileX >= header.MaxTilesX ||
        environment.SpawnTileY < 0 || environment.SpawnTileY >= header.MaxTilesY ||
        !IsValidCoordinate(environment.DungeonX, header.MaxTilesX) ||
        !IsValidCoordinate(environment.DungeonY, header.MaxTilesY) ||
        environment.WorldSurface > environment.RockLayer)
    {
      throw new InvalidDataException(
        "The world environment contains invalid positions or layer ordering.");
    }
  }

  private static void ValidateProgression(WorldFileProgressionSection progression)
  {
    if (progression.AltarCount < 0)
    {
      throw new InvalidDataException("The world progression contains a negative altar count.");
    }

    if (progression.InvasionSize < 0 || progression.RainTime < 0 || progression.CloudCount < 0 ||
        !double.IsFinite(progression.InvasionX) ||
        !double.IsFinite(progression.SlimeRainTime) ||
        !float.IsFinite(progression.MaxRain) ||
        !float.IsFinite(progression.WindSpeedTarget))
    {
      throw new InvalidDataException("The world progression contains a negative count.");
    }
  }

  private static void ValidateQuests(WorldFileQuestSection quests)
  {
    if (quests.AnglerWhoFinishedToday.Count > WorldFileFormatConstants.MaxStringListEntries)
    {
      throw new InvalidDataException("The world quest section contains too many Angler entries.");
    }

    for (int index = 0; index < quests.AnglerWhoFinishedToday.Count; index++)
    {
      string value = quests.AnglerWhoFinishedToday[index];
      if (Utf8.GetByteCount(value) > WorldFileFormatConstants.MaxTextBytes)
      {
        throw new InvalidDataException("An Angler name exceeds the supported UTF-8 length.");
      }
    }

    if (quests.InvasionSizeStart < 0 || quests.CultistDelay < 0)
    {
      throw new InvalidDataException("The world quest section contains a negative count.");
    }
  }

  private static void ValidateBanners(WorldFileBannerSection banners)
  {
    if (banners.KillCounts.Count > WorldFileFormatConstants.MaxBannerEntries ||
        banners.ClaimableCounts.Count > WorldFileFormatConstants.MaxBannerEntries)
    {
      throw new InvalidDataException("The world banner section contains too many entries.");
    }

    for (int index = 0; index < banners.KillCounts.Count; index++)
    {
      if (banners.KillCounts[index] < 0)
      {
        throw new InvalidDataException("The world banner section contains a negative kill count.");
      }
    }
  }

  private static void ValidateParty(WorldFilePartySection party)
  {
    if (party.CelebratingNpcIds.Count > WorldFileFormatConstants.MaxPartyEntries ||
        party.Cooldown < 0)
    {
      throw new InvalidDataException("The world party section contains too many NPC entries.");
    }
  }

  private static void ValidateBackgrounds(WorldFileBackgroundSection backgrounds)
  {
    if (backgrounds.Styles.Count != 5)
    {
      throw new InvalidDataException(
        "The world background section requires exactly five styles.");
    }
  }

  private static void ValidateEvents(WorldFileEventSection events)
  {
    if (events.LanternNightCooldown < 0)
    {
      throw new InvalidDataException(
        "The world event section contains a negative Lantern Night cooldown.");
    }
  }

  private static void ValidateSandstorm(WorldFileSandstormSection sandstorm)
  {
    if (sandstorm.TimeLeft < 0 ||
        !float.IsFinite(sandstorm.Severity) ||
        !float.IsFinite(sandstorm.IntendedSeverity))
    {
      throw new InvalidDataException("The world sandstorm section contains invalid values.");
    }
  }

  private static void ValidateTreeTops(WorldFileTreeTopsSection treeTops)
  {
    if (treeTops.Variations.Count > WorldFileFormatConstants.MaxTreeTopsEntries)
    {
      throw new InvalidDataException("The world TreeTops section contains too many entries.");
    }
  }

  private static void ValidateTimePolicy(WorldFileTimePolicySection timePolicy)
  {
    if (timePolicy.MeteorShowerCount < 0 || timePolicy.CoinRain < 0)
    {
      throw new InvalidDataException(
        "The world time policy section contains negative event counts.");
    }
  }

  private static void ValidateSpawn(WorldFileSpawnSection spawn)
  {
    if (spawn.ExtraSpawnPoints.Count > WorldFileFormatConstants.MaxExtraSpawnPoints)
    {
      throw new InvalidDataException("The world spawn section contains too many points.");
    }

    if (spawn.MoreLightningSeed || spawn.NoLightningSeed)
    {
      throw new InvalidDataException(
        "WorldFile version 319 cannot encode lightning seed flags introduced in version 323.");
    }

    if (spawn.WorldManifestJson is not null &&
        Utf8.GetByteCount(spawn.WorldManifestJson) > WorldFileFormatConstants.MaxTextBytes)
    {
      throw new InvalidDataException("The world manifest exceeds the supported UTF-8 length.");
    }
  }

  private static WorldSaveEncodeResult Failed(string detail)
  {
    return WorldSaveEncodeResult.Failed(
      WorldStorageFailure.Create(WorldStorageFailureKind.InvalidData, detail));
  }

  private static bool IsValidCoordinate(int coordinate, int exclusiveUpperBound)
  {
    return coordinate == -1 || (coordinate >= 0 && coordinate < exclusiveUpperBound);
  }
}
