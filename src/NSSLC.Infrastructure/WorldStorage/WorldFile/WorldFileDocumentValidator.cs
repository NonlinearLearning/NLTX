using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Terraria.NonAuthoritative.Persistence;
using Terraria.WorldStorage;

namespace Terraria.NonAuthoritative.WorldStorage;

/// <summary>
/// Performs application-level checks for the supported prefix produced by the WorldFile decoder.
/// </summary>
public sealed class WorldFileDocumentValidator : IWorldPersistenceDocumentValidator
{
  private static readonly Encoding Utf8 = new UTF8Encoding(false, true);

  public WorldStorageFailure Validate(WorldPersistenceDocument document)
  {
    ArgumentNullException.ThrowIfNull(document);
    if (document.FormatVersion < WorldFileFormatConstants.MinimumVersion ||
        document.FormatVersion > WorldFileFormatConstants.LatestReadableVersion)
    {
      return WorldStorageFailure.Create(
        WorldStorageFailureKind.InvalidData,
        $"Unsupported world file version {document.FormatVersion}.");
    }

    if (!document.TryGetSection<WorldFileHeaderSection>(
          WorldFileHeaderSection.SectionId,
          out WorldLoadSection<WorldFileHeaderSection> section) ||
        !section.IsPresent)
    {
      return WorldStorageFailure.Create(
        WorldStorageFailureKind.InvalidData,
        "The world document does not contain a world header section.");
    }

    WorldFileHeaderSection header = section.Value;
    if (string.IsNullOrWhiteSpace(header.WorldName) ||
        !HasValidTextLength(header.WorldName) ||
        (header.SeedText is not null && !HasValidTextLength(header.SeedText)) ||
        header.MaxTilesX <= 0 || header.MaxTilesY <= 0 ||
        header.LeftWorld >= header.RightWorld || header.TopWorld >= header.BottomWorld)
    {
      return WorldStorageFailure.Create(
        WorldStorageFailureKind.InvalidData,
        "The world header contains invalid bounds or dimensions.");
    }

    if (!document.TryGetSection<WorldFileTilePayloadSection>(
          WorldFileTilePayloadSection.SectionId,
          out WorldLoadSection<WorldFileTilePayloadSection> requiredTileSection) ||
        !requiredTileSection.IsPresent)
    {
      return WorldStorageFailure.Create(
        WorldStorageFailureKind.InvalidData,
        "The world document does not contain a present Tile payload section.");
    }

    if (!document.TryGetSection<WorldFileFooterSection>(
          WorldFileFooterSection.SectionId,
          out WorldLoadSection<WorldFileFooterSection> requiredFooterSection) ||
        !requiredFooterSection.IsPresent)
    {
      return WorldStorageFailure.Create(
        WorldStorageFailureKind.InvalidData,
        "The world document does not contain a present Footer section.");
    }

    WorldFileTilePayloadSection tilePayload = requiredTileSection.Value;
    if (tilePayload.MaxTilesX != header.MaxTilesX ||
        tilePayload.MaxTilesY != header.MaxTilesY ||
        tilePayload.FrameImportant.Count == 0 ||
        tilePayload.FrameImportant.Count > ushort.MaxValue ||
        tilePayload.CompressedPayload.IsEmpty ||
        tilePayload.CompressedPayload.Length > WorldFileFormatConstants.MaxRawSectionBytes)
    {
      return WorldStorageFailure.Create(
        WorldStorageFailureKind.InvalidData,
        "The world Tile section dimensions or importance table are invalid.");
    }

    WorldFileFooterSection footer = requiredFooterSection.Value;
    if (!footer.IsComplete ||
        footer.WorldId != header.WorldId ||
        !string.Equals(footer.WorldName, header.WorldName, StringComparison.Ordinal) ||
        !HasValidTextLength(footer.WorldName))
    {
      return WorldStorageFailure.Create(
        WorldStorageFailureKind.InvalidData,
        "The world file footer is incomplete or does not match its header.");
    }

    if (header.GameMode is < 0 or > WorldFileFormatConstants.MaximumGameMode)
    {
      return WorldStorageFailure.Create(
        WorldStorageFailureKind.InvalidData,
        "The world header contains an unsupported game mode.");
    }

    if (document.TryGetSection<WorldFileEnvironmentSection>(
          WorldFileEnvironmentSection.SectionId,
          out WorldLoadSection<WorldFileEnvironmentSection> environmentSection) &&
        environmentSection.IsPresent)
    {
      WorldFileEnvironmentSection environment = environmentSection.Value;
      if (environment.SpawnTileX < 0 || environment.SpawnTileX >= header.MaxTilesX ||
          environment.SpawnTileY < 0 || environment.SpawnTileY >= header.MaxTilesY ||
          !IsValidCoordinate(environment.DungeonX, header.MaxTilesX) ||
          !IsValidCoordinate(environment.DungeonY, header.MaxTilesY) ||
          environment.WorldSurface > environment.RockLayer)
      {
        return WorldStorageFailure.Create(
          WorldStorageFailureKind.InvalidData,
          "The world environment contains invalid positions or layer ordering.");
      }
    }

    if (document.TryGetSection<WorldFileProgressionSection>(
          WorldFileProgressionSection.SectionId,
          out WorldLoadSection<WorldFileProgressionSection> progressionSection) &&
        progressionSection.IsPresent)
    {
      WorldFileProgressionSection progression = progressionSection.Value;
      if (progression.AltarCount < 0 ||
          progression.InvasionSize < 0 ||
          progression.RainTime < 0 ||
          progression.CloudCount < 0 ||
          !double.IsFinite(progression.InvasionX) ||
          !double.IsFinite(progression.SlimeRainTime) ||
          !float.IsFinite(progression.MaxRain) ||
          !float.IsFinite(progression.WindSpeedTarget))
      {
        return WorldStorageFailure.Create(
          WorldStorageFailureKind.InvalidData,
          "The world progression contains invalid counts or non-finite values.");
      }
    }

    if (document.TryGetSection<WorldFileQuestSection>(
          WorldFileQuestSection.SectionId,
          out WorldLoadSection<WorldFileQuestSection> questSection) &&
        questSection.IsPresent)
    {
      WorldFileQuestSection quests = questSection.Value;
      if (quests.AnglerWhoFinishedToday.Count > WorldFileFormatConstants.MaxStringListEntries ||
          quests.InvasionSizeStart < 0 ||
          quests.CultistDelay < 0)
      {
        return WorldStorageFailure.Create(
          WorldStorageFailureKind.InvalidData,
          "The world quest section contains invalid counts.");
      }
    }

    if (document.TryGetSection<WorldFileBannerSection>(
          WorldFileBannerSection.SectionId,
          out WorldLoadSection<WorldFileBannerSection> bannerSection) &&
        bannerSection.IsPresent)
    {
      WorldFileBannerSection banners = bannerSection.Value;
      if (banners.KillCounts.Count > WorldFileFormatConstants.MaxBannerEntries ||
          banners.ClaimableCounts.Count > WorldFileFormatConstants.MaxBannerEntries)
      {
        return WorldStorageFailure.Create(
          WorldStorageFailureKind.InvalidData,
          "The world banner section contains too many entries.");
      }

      for (int index = 0; index < banners.KillCounts.Count; index++)
      {
        if (banners.KillCounts[index] < 0)
        {
          return WorldStorageFailure.Create(
            WorldStorageFailureKind.InvalidData,
            "The world banner section contains a negative kill count.");
        }
      }
    }

    if (document.TryGetSection<WorldFilePartySection>(
          WorldFilePartySection.SectionId,
          out WorldLoadSection<WorldFilePartySection> partySection) &&
        partySection.IsPresent &&
        (partySection.Value.CelebratingNpcIds.Count > WorldFileFormatConstants.MaxPartyEntries ||
         partySection.Value.Cooldown < 0))
    {
      return WorldStorageFailure.Create(
        WorldStorageFailureKind.InvalidData,
        "The world party section contains too many NPC entries or a negative cooldown.");
    }

    if (document.TryGetSection<WorldFileEventSection>(
          WorldFileEventSection.SectionId,
          out WorldLoadSection<WorldFileEventSection> eventSection) &&
        eventSection.IsPresent &&
        eventSection.Value.LanternNightCooldown < 0)
    {
      return WorldStorageFailure.Create(
        WorldStorageFailureKind.InvalidData,
        "The world event section contains a negative Lantern Night cooldown.");
    }

    if (document.TryGetSection<WorldFileSandstormSection>(
          WorldFileSandstormSection.SectionId,
          out WorldLoadSection<WorldFileSandstormSection> sandstormSection) &&
        sandstormSection.IsPresent)
    {
      WorldFileSandstormSection sandstorm = sandstormSection.Value;
      if (sandstorm.TimeLeft < 0 ||
          !float.IsFinite(sandstorm.Severity) ||
          !float.IsFinite(sandstorm.IntendedSeverity))
      {
        return WorldStorageFailure.Create(
          WorldStorageFailureKind.InvalidData,
          "The world sandstorm section contains invalid values.");
      }
    }

    if (document.TryGetSection<WorldFileTreeTopsSection>(
          WorldFileTreeTopsSection.SectionId,
          out WorldLoadSection<WorldFileTreeTopsSection> treeTopsSection) &&
        treeTopsSection.IsPresent &&
        treeTopsSection.Value.Variations.Count > WorldFileFormatConstants.MaxTreeTopsEntries)
    {
      return WorldStorageFailure.Create(
        WorldStorageFailureKind.InvalidData,
        "The world TreeTops section contains too many entries.");
    }

    if (document.TryGetSection<WorldFileTimePolicySection>(
          WorldFileTimePolicySection.SectionId,
          out WorldLoadSection<WorldFileTimePolicySection> timePolicySection) &&
        timePolicySection.IsPresent &&
        (timePolicySection.Value.MeteorShowerCount < 0 ||
         timePolicySection.Value.CoinRain < 0))
    {
      return WorldStorageFailure.Create(
        WorldStorageFailureKind.InvalidData,
          "The world time policy section contains negative event counts.");
    }

    if (document.TryGetSection<WorldFileSpawnSection>(
          WorldFileSpawnSection.SectionId,
          out WorldLoadSection<WorldFileSpawnSection> spawnSection) &&
        spawnSection.IsPresent)
    {
      WorldFileSpawnSection spawn = spawnSection.Value;
      if (document.FormatVersion < WorldFileFormatConstants.LightningSeedsVersion &&
          (spawn.MoreLightningSeed || spawn.NoLightningSeed))
      {
        return WorldStorageFailure.Create(
          WorldStorageFailureKind.InvalidData,
          "The world spawn section contains lightning seed flags unsupported by its version.");
      }

      if (spawn.ExtraSpawnPoints.Count > WorldFileFormatConstants.MaxExtraSpawnPoints)
      {
        return WorldStorageFailure.Create(
          WorldStorageFailureKind.InvalidData,
          "The world spawn section contains too many points.");
      }

      if (spawn.WorldManifestJson is not null)
      {
        try
        {
          if (Utf8.GetByteCount(spawn.WorldManifestJson) >
              WorldFileFormatConstants.MaxTextBytes)
          {
            return WorldStorageFailure.Create(
              WorldStorageFailureKind.InvalidData,
              "The world manifest exceeds the supported UTF-8 length.");
          }
        }
        catch (EncoderFallbackException)
        {
          return WorldStorageFailure.Create(
            WorldStorageFailureKind.InvalidData,
            "The world manifest contains invalid UTF-16 text.");
        }
      }
    }

    if (document.TryGetSection<WorldFileChestSection>(
          WorldFileChestSection.SectionId,
          out WorldLoadSection<WorldFileChestSection> chestSection) &&
        chestSection.IsPresent)
    {
      if (chestSection.Value.Chests.Count > WorldFileFormatConstants.MaxChests)
      {
        return WorldStorageFailure.Create(
          WorldStorageFailureKind.InvalidData,
          "The world chest section contains too many chests.");
      }

      for (int chestIndex = 0; chestIndex < chestSection.Value.Chests.Count; chestIndex++)
      {
        WorldFileChestRecord chest = chestSection.Value.Chests[chestIndex];
        if (!IsValidChestCoordinate(chest.X, header.MaxTilesX) ||
            !IsValidChestCoordinate(chest.Y, header.MaxTilesY) ||
            chest.Items.Count > WorldFileFormatConstants.MaxChestItems ||
            !HasValidTextLength(chest.Name))
        {
          return WorldStorageFailure.Create(
            WorldStorageFailureKind.InvalidData,
            "The world chest section contains an invalid record.");
        }

        for (int itemIndex = 0; itemIndex < chest.Items.Count; itemIndex++)
        {
          WorldFileChestItem item = chest.Items[itemIndex];
          if (item.Stack < short.MinValue || item.Stack > short.MaxValue)
          {
            return WorldStorageFailure.Create(
              WorldStorageFailureKind.InvalidData,
              "The world chest section contains an invalid item.");
          }
        }
      }
    }

    if (document.TryGetSection<WorldFileSignSection>(
          WorldFileSignSection.SectionId,
          out WorldLoadSection<WorldFileSignSection> signSection) &&
        signSection.IsPresent)
    {
      if (signSection.Value.Signs.Count > WorldFileFormatConstants.MaxSigns)
      {
        return WorldStorageFailure.Create(
          WorldStorageFailureKind.InvalidData,
          "The world sign section contains too many signs.");
      }

      for (int signIndex = 0; signIndex < signSection.Value.Signs.Count; signIndex++)
      {
        WorldFileSignRecord sign = signSection.Value.Signs[signIndex];
        if (!IsValidTileCoordinate(sign.X, header.MaxTilesX) ||
            !IsValidTileCoordinate(sign.Y, header.MaxTilesY) ||
            !HasValidTextLength(sign.Text))
        {
          return WorldStorageFailure.Create(
            WorldStorageFailureKind.InvalidData,
            "The world sign section contains an invalid record.");
        }
      }
    }

    if (document.TryGetSection<WorldFileNpcSection>(
          WorldFileNpcSection.SectionId,
          out WorldLoadSection<WorldFileNpcSection> npcSection) &&
        npcSection.IsPresent)
    {
      WorldFileNpcSection npcs = npcSection.Value;
      if (npcs.ShimmeredTownNpcIds.Count > WorldFileFormatConstants.MaxShimmeredNpcIds ||
          npcs.TownNpcs.Count > WorldFileFormatConstants.MaxNpcRecords ||
          npcs.SavedNpcs.Count > WorldFileFormatConstants.MaxNpcRecords)
      {
        return WorldStorageFailure.Create(
          WorldStorageFailureKind.InvalidData,
          "The world NPC section contains too many records.");
      }

      for (int index = 0; index < npcs.ShimmeredTownNpcIds.Count; index++)
      {
        if (npcs.ShimmeredTownNpcIds[index] < 0)
        {
          return WorldStorageFailure.Create(
            WorldStorageFailureKind.InvalidData,
            "The world NPC section contains a negative shimmered NPC identifier.");
        }
      }

      if (!ValidateNpcRecords(npcs.TownNpcs, header.MaxTilesX, header.MaxTilesY, true) ||
          !ValidateNpcRecords(npcs.SavedNpcs, header.MaxTilesX, header.MaxTilesY, false))
      {
        return WorldStorageFailure.Create(
          WorldStorageFailureKind.InvalidData,
          "The world NPC section contains an invalid record.");
      }
    }

    if (document.TryGetSection<WorldFileTileEntitySection>(
          WorldFileTileEntitySection.SectionId,
          out WorldLoadSection<WorldFileTileEntitySection> tileEntitySection) &&
        tileEntitySection.IsPresent)
    {
      WorldFileTileEntitySection tileEntities = tileEntitySection.Value;
      if (tileEntities.EntityCount > WorldFileFormatConstants.MaxTileEntities ||
          tileEntities.SerializedRecords.Length > WorldFileFormatConstants.MaxRawSectionBytes ||
          (tileEntities.EntityCount == 0 && !tileEntities.SerializedRecords.IsEmpty) ||
          (tileEntities.EntityCount > 0 && tileEntities.SerializedRecords.IsEmpty))
      {
        return WorldStorageFailure.Create(
          WorldStorageFailureKind.InvalidData,
          "The world TileEntity section count does not match its opaque payload or exceeds " +
          "the supported bounds.");
      }

      if (document.FormatVersion == WorldFileFormatConstants.LatestWritableVersion &&
          tileEntities.EntityCount > 0)
      {
        try
        {
          foreach (TileEntitySnapshot entity in new WorldFileTileEntityCodec().Decode(tileEntities))
          {
            if (!IsValidTileCoordinate(entity.Anchor.X, header.MaxTilesX) ||
                !IsValidTileCoordinate(entity.Anchor.Y, header.MaxTilesY))
            {
              return WorldStorageFailure.Create(
                WorldStorageFailureKind.InvalidData,
                "A world TileEntity anchor lies outside the world.");
            }
          }
        }
        catch (Exception exception) when (exception is IOException or InvalidDataException)
        {
          return WorldStorageFailure.Create(WorldStorageFailureKind.InvalidData, exception.Message);
        }
      }
    }

    if (document.TryGetSection<WorldFilePressurePlateSection>(
          WorldFilePressurePlateSection.SectionId,
          out WorldLoadSection<WorldFilePressurePlateSection> pressurePlateSection) &&
        pressurePlateSection.IsPresent)
    {
      if (pressurePlateSection.Value.Plates.Count > WorldFileFormatConstants.MaxPressurePlates)
      {
        return WorldStorageFailure.Create(
          WorldStorageFailureKind.InvalidData,
          "The world pressure plate section contains too many records.");
      }

      for (int index = 0; index < pressurePlateSection.Value.Plates.Count; index++)
      {
        WorldFilePressurePlateRecord plate = pressurePlateSection.Value.Plates[index];
        if (!IsValidTileCoordinate(plate.X, header.MaxTilesX) ||
            !IsValidTileCoordinate(plate.Y, header.MaxTilesY))
        {
          return WorldStorageFailure.Create(
            WorldStorageFailureKind.InvalidData,
            "The world pressure plate section contains an invalid coordinate.");
        }
      }
    }

    if (document.TryGetSection<WorldFileTownManagerSection>(
          WorldFileTownManagerSection.SectionId,
          out WorldLoadSection<WorldFileTownManagerSection> townManagerSection) &&
        townManagerSection.IsPresent)
    {
      WorldFileTownManagerSection townManager = townManagerSection.Value;
      if (townManager.Rooms.Count > WorldFileFormatConstants.MaxTownRooms)
      {
        return WorldStorageFailure.Create(
          WorldStorageFailureKind.InvalidData,
          "The world TownManager section contains too many rooms.");
      }

      for (int index = 0; index < townManager.Rooms.Count; index++)
      {
        WorldFileTownRoomRecord room = townManager.Rooms[index];
        if (room.NpcType < 0 ||
            !IsValidTileCoordinate(room.TileX, header.MaxTilesX) ||
            !IsValidTileCoordinate(room.TileY, header.MaxTilesY))
        {
          return WorldStorageFailure.Create(
            WorldStorageFailureKind.InvalidData,
            "The world TownManager section contains an invalid room.");
        }
      }
    }

    if (document.TryGetSection<WorldFileBestiarySection>(
          WorldFileBestiarySection.SectionId,
          out WorldLoadSection<WorldFileBestiarySection> bestiarySection) &&
        bestiarySection.IsPresent)
    {
      WorldFileBestiarySection bestiary = bestiarySection.Value;
      if (bestiary.KillCounts.Count > WorldFileFormatConstants.MaxBestiaryEntries ||
          bestiary.SeenNpcIds.Count > WorldFileFormatConstants.MaxBestiaryEntries ||
          bestiary.ChattedNpcIds.Count > WorldFileFormatConstants.MaxBestiaryEntries)
      {
        return WorldStorageFailure.Create(
          WorldStorageFailureKind.InvalidData,
          "The world Bestiary section contains too many entries.");
      }

      for (int index = 0; index < bestiary.KillCounts.Count; index++)
      {
        WorldFileBestiaryKillCount kill = bestiary.KillCounts[index];
        if (kill.Count < 0 || !HasValidTextLength(kill.PersistentId))
        {
          return WorldStorageFailure.Create(
            WorldStorageFailureKind.InvalidData,
            "The world Bestiary section contains an invalid kill count.");
        }
      }

      if (!HasValidTexts(bestiary.SeenNpcIds) ||
          !HasValidTexts(bestiary.ChattedNpcIds))
      {
        return WorldStorageFailure.Create(
          WorldStorageFailureKind.InvalidData,
          "The world Bestiary section contains an invalid identifier.");
      }
    }

    if (document.TryGetSection<WorldFileCreativePowersSection>(
          WorldFileCreativePowersSection.SectionId,
          out WorldLoadSection<WorldFileCreativePowersSection> creativePowersSection) &&
        creativePowersSection.IsPresent &&
        (creativePowersSection.Value.SerializedPayload.IsEmpty ||
         creativePowersSection.Value.SerializedPayload.Span[^1] != 0 ||
         creativePowersSection.Value.SerializedPayload.Length >
         WorldFileFormatConstants.MaxRawSectionBytes))
    {
      return WorldStorageFailure.Create(
        WorldStorageFailureKind.InvalidData,
        "The world CreativePowers section is empty or too large.");
    }

    return WorldStorageFailure.None;
  }

  private static bool ValidateNpcRecords(
    IReadOnlyList<WorldFileNpcRecord> records,
    int maxTilesX,
    int maxTilesY,
    bool expectedTownNpc)
  {
    for (int index = 0; index < records.Count; index++)
    {
      WorldFileNpcRecord record = records[index];
      if (record.IsTownNpc != expectedTownNpc ||
          (record.NetId is null && string.IsNullOrEmpty(record.LegacyTypeName)) ||
          (record.NetId is < 0) ||
          !float.IsFinite(record.PositionX) ||
          !float.IsFinite(record.PositionY) ||
          !HasValidTextLength(record.Name) ||
          (record.LegacyTypeName is not null &&
           !HasValidTextLength(record.LegacyTypeName)) ||
          (record.IsTownNpc &&
           (!IsValidCoordinate(record.HomeTileX, maxTilesX) ||
            !IsValidCoordinate(record.HomeTileY, maxTilesY) ||
            record.TownNpcVariationIndex is < 0)))
      {
        return false;
      }
    }

    return true;
  }

  private static bool IsValidCoordinate(int coordinate, int exclusiveUpperBound)
  {
    return coordinate == -1 || (coordinate >= 0 && coordinate < exclusiveUpperBound);
  }

  private static bool HasValidTextLength(string value)
  {
    try
    {
      return Utf8.GetByteCount(value) <= WorldFileFormatConstants.MaxTextBytes;
    }
    catch (EncoderFallbackException)
    {
      return false;
    }
  }

  private static bool HasValidTexts(IReadOnlyList<string> values)
  {
    for (int index = 0; index < values.Count; index++)
    {
      if (!HasValidTextLength(values[index]))
      {
        return false;
      }
    }

    return true;
  }

  private static bool IsValidTileCoordinate(int coordinate, int exclusiveUpperBound)
  {
    return coordinate >= 0 && coordinate < exclusiveUpperBound;
  }

  private static bool IsValidChestCoordinate(int coordinate, int exclusiveUpperBound)
  {
    return exclusiveUpperBound > 1 && coordinate >= 0 && coordinate < exclusiveUpperBound - 1;
  }
}
