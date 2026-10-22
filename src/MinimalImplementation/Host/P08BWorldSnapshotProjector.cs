using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MinimalImplementation.Server.World;
using Terraria.NonAuthoritative.Persistence;
using Terraria.WorldStorage;

namespace MinimalImplementation.Server.Host;

/// <summary>
/// Projects the P03 save snapshot back onto the loaded 27-section persistence document.
/// </summary>
public sealed class P08BWorldSnapshotProjector
{
  private readonly WorldFileTilePayloadEncoder _tilePayloadEncoder = new();

  private static readonly IReadOnlyList<string> RequiredSectionIds = Array.AsReadOnly(
    new[]
    {
      WorldFileHeaderSection.SectionId,
      WorldFileMetadataSection.SectionId,
      WorldFileEnvironmentSection.SectionId,
      WorldFileProgressionSection.SectionId,
      WorldFileQuestSection.SectionId,
      WorldFileBannerSection.SectionId,
      WorldFileBossProgressionSection.SectionId,
      WorldFilePartySection.SectionId,
      WorldFileSandstormSection.SectionId,
      WorldFileDefenderEventSection.SectionId,
      WorldFileBackgroundSection.SectionId,
      WorldFileEventSection.SectionId,
      WorldFileTreeTopsSection.SectionId,
      WorldFileSeasonalSection.SectionId,
      WorldFileNpcUnlockSection.SectionId,
      WorldFileTimePolicySection.SectionId,
      WorldFileSpawnSection.SectionId,
      WorldFileTilePayloadSection.SectionId,
      WorldFileChestSection.SectionId,
      WorldFileSignSection.SectionId,
      WorldFileNpcSection.SectionId,
      WorldFileTileEntitySection.SectionId,
      WorldFilePressurePlateSection.SectionId,
      WorldFileTownManagerSection.SectionId,
      WorldFileBestiarySection.SectionId,
      WorldFileCreativePowersSection.SectionId,
      WorldFileFooterSection.SectionId
    });

  public WorldPersistenceDocument Project(
    WorldPersistenceDocument initialDocument,
    WorldSaveSnapshot baseline,
    WorldSaveSnapshot snapshot)
  {
    ArgumentNullException.ThrowIfNull(initialDocument);
    ArgumentNullException.ThrowIfNull(baseline);
    ArgumentNullException.ThrowIfNull(snapshot);
    ValidateSectionSet(initialDocument);

    if (baseline.WorldEpoch != snapshot.WorldEpoch ||
        baseline.Header.Geometry != snapshot.Header.Geometry ||
        !string.Equals(
          baseline.Facts.Fingerprint,
          snapshot.Facts.Fingerprint,
          StringComparison.Ordinal) ||
        baseline.Tiles.Count != snapshot.Header.Geometry.TileCount ||
        snapshot.Tiles.Count != snapshot.Header.Geometry.TileCount)
    {
      throw new InvalidDataException(
        "The save snapshot does not match the loaded world geometry, facts or epoch.");
    }

    WorldFileHeaderSection header = CreateHeader(snapshot.Header);
    WorldFileEnvironmentSection environment = CreateEnvironment(snapshot.Header);
    WorldFileFooterSection footer = new(
      isComplete: true,
      worldName: snapshot.Header.Name,
      worldId: snapshot.Header.WorldId);
    WorldFileTilePayloadSection tilePayload = GetSection<WorldFileTilePayloadSection>(
      initialDocument,
      WorldFileTilePayloadSection.SectionId);
    if (tilePayload.MaxTilesX != snapshot.Header.Geometry.Width ||
        tilePayload.MaxTilesY != snapshot.Header.Geometry.Height)
    {
      throw new InvalidDataException(
        "The loaded tile payload dimensions do not match the authoritative snapshot geometry.");
    }

    ValidateFrameImportance(tilePayload, snapshot.Facts.FrameImportant);
    if (!TilesEqual(baseline.Tiles, snapshot.Tiles))
    {
      tilePayload = _tilePayloadEncoder.Encode(
        snapshot.Header.Geometry,
        snapshot.Tiles,
        snapshot.Facts);
    }

    return new WorldPersistenceDocument(
      initialDocument.FormatVersion,
      new WorldPersistenceSection[]
      {
        WorldPersistenceSection.Create(WorldFileHeaderSection.SectionId, header),
        WorldPersistenceSection.Create(
          WorldFileMetadataSection.SectionId,
          GetSection<WorldFileMetadataSection>(initialDocument, WorldFileMetadataSection.SectionId)),
        WorldPersistenceSection.Create(WorldFileEnvironmentSection.SectionId, environment),
        WorldPersistenceSection.Create(
          WorldFileProgressionSection.SectionId,
          GetSection<WorldFileProgressionSection>(
            initialDocument,
            WorldFileProgressionSection.SectionId)),
        WorldPersistenceSection.Create(
          WorldFileQuestSection.SectionId,
          GetSection<WorldFileQuestSection>(initialDocument, WorldFileQuestSection.SectionId)),
        WorldPersistenceSection.Create(
          WorldFileBannerSection.SectionId,
          GetSection<WorldFileBannerSection>(initialDocument, WorldFileBannerSection.SectionId)),
        WorldPersistenceSection.Create(
          WorldFileBossProgressionSection.SectionId,
          GetSection<WorldFileBossProgressionSection>(
            initialDocument,
            WorldFileBossProgressionSection.SectionId)),
        WorldPersistenceSection.Create(
          WorldFilePartySection.SectionId,
          GetSection<WorldFilePartySection>(initialDocument, WorldFilePartySection.SectionId)),
        WorldPersistenceSection.Create(
          WorldFileSandstormSection.SectionId,
          GetSection<WorldFileSandstormSection>(initialDocument, WorldFileSandstormSection.SectionId)),
        WorldPersistenceSection.Create(
          WorldFileDefenderEventSection.SectionId,
          GetSection<WorldFileDefenderEventSection>(
            initialDocument,
            WorldFileDefenderEventSection.SectionId)),
        WorldPersistenceSection.Create(
          WorldFileBackgroundSection.SectionId,
          GetSection<WorldFileBackgroundSection>(
            initialDocument,
            WorldFileBackgroundSection.SectionId)),
        WorldPersistenceSection.Create(
          WorldFileEventSection.SectionId,
          GetSection<WorldFileEventSection>(initialDocument, WorldFileEventSection.SectionId)),
        WorldPersistenceSection.Create(
          WorldFileTreeTopsSection.SectionId,
          GetSection<WorldFileTreeTopsSection>(initialDocument, WorldFileTreeTopsSection.SectionId)),
        WorldPersistenceSection.Create(
          WorldFileSeasonalSection.SectionId,
          GetSection<WorldFileSeasonalSection>(initialDocument, WorldFileSeasonalSection.SectionId)),
        WorldPersistenceSection.Create(
          WorldFileNpcUnlockSection.SectionId,
          GetSection<WorldFileNpcUnlockSection>(initialDocument, WorldFileNpcUnlockSection.SectionId)),
        WorldPersistenceSection.Create(
          WorldFileTimePolicySection.SectionId,
          GetSection<WorldFileTimePolicySection>(
            initialDocument,
            WorldFileTimePolicySection.SectionId)),
        WorldPersistenceSection.Create(
          WorldFileSpawnSection.SectionId,
          GetSection<WorldFileSpawnSection>(initialDocument, WorldFileSpawnSection.SectionId)),
        WorldPersistenceSection.Create(WorldFileTilePayloadSection.SectionId, tilePayload),
        WorldPersistenceSection.Create(
          WorldFileChestSection.SectionId,
          GetSection<WorldFileChestSection>(initialDocument, WorldFileChestSection.SectionId)),
        WorldPersistenceSection.Create(
          WorldFileSignSection.SectionId,
          GetSection<WorldFileSignSection>(initialDocument, WorldFileSignSection.SectionId)),
        WorldPersistenceSection.Create(
          WorldFileNpcSection.SectionId,
          GetSection<WorldFileNpcSection>(initialDocument, WorldFileNpcSection.SectionId)),
        WorldPersistenceSection.Create(
          WorldFileTileEntitySection.SectionId,
          GetSection<WorldFileTileEntitySection>(
            initialDocument,
            WorldFileTileEntitySection.SectionId)),
        WorldPersistenceSection.Create(
          WorldFilePressurePlateSection.SectionId,
          GetSection<WorldFilePressurePlateSection>(
            initialDocument,
            WorldFilePressurePlateSection.SectionId)),
        WorldPersistenceSection.Create(
          WorldFileTownManagerSection.SectionId,
          GetSection<WorldFileTownManagerSection>(
            initialDocument,
            WorldFileTownManagerSection.SectionId)),
        WorldPersistenceSection.Create(
          WorldFileBestiarySection.SectionId,
          GetSection<WorldFileBestiarySection>(initialDocument, WorldFileBestiarySection.SectionId)),
        WorldPersistenceSection.Create(
          WorldFileCreativePowersSection.SectionId,
          GetSection<WorldFileCreativePowersSection>(
            initialDocument,
            WorldFileCreativePowersSection.SectionId)),
        WorldPersistenceSection.Create(WorldFileFooterSection.SectionId, footer)
      });
  }

  private static void ValidateSectionSet(WorldPersistenceDocument document)
  {
    HashSet<string> sectionIds = new(document.SectionIds, StringComparer.Ordinal);
    HashSet<string> requiredIds = new(RequiredSectionIds, StringComparer.Ordinal);
    string[] missing = requiredIds.Except(sectionIds, StringComparer.Ordinal).ToArray();
    string[] unknown = sectionIds.Except(requiredIds, StringComparer.Ordinal).ToArray();
    if (missing.Length != 0 || unknown.Length != 0)
    {
      throw new InvalidDataException(
        $"The save projector requires exactly 27 known sections. Missing: " +
        $"{string.Join(", ", missing)}; unknown: {string.Join(", ", unknown)}.");
    }
  }

  private static bool TilesEqual(
    IReadOnlyList<WorldTile> left,
    IReadOnlyList<WorldTile> right)
  {
    if (left.Count != right.Count)
    {
      return false;
    }

    for (int index = 0; index < left.Count; index++)
    {
      if (left[index] != right[index])
      {
        return false;
      }
    }

    return true;
  }

  private static void ValidateFrameImportance(
    WorldFileTilePayloadSection section,
    IReadOnlyList<bool> frameImportant)
  {
    if (section.FrameImportant.Count != frameImportant.Count)
    {
      throw new InvalidDataException(
        "The loaded tile payload has a different frame-importance facts table.");
    }

    for (int index = 0; index < frameImportant.Count; index++)
    {
      if (section.FrameImportant[index] != frameImportant[index])
      {
        throw new InvalidDataException(
          "The loaded tile payload has a different frame-importance facts table.");
      }
    }
  }

  private static TSection GetSection<TSection>(
    WorldPersistenceDocument document,
    string sectionId)
    where TSection : notnull
  {
    if (!document.TryGetSection<TSection>(
          sectionId,
          out WorldLoadSection<TSection> section) ||
        !section.IsPresent)
    {
      throw new InvalidDataException($"The save projector could not read section '{sectionId}'.");
    }

    return section.Value;
  }

  private static WorldFileHeaderSection CreateHeader(WorldHeaderSnapshot snapshot)
  {
    WorldHeaderFacts facts = snapshot.Facts;
    return new WorldFileHeaderSection(
      snapshot.Name,
      facts.SeedText,
      facts.WorldGeneratorVersion,
      snapshot.UniqueId,
      snapshot.WorldId,
      facts.LeftWorld,
      facts.RightWorld,
      facts.TopWorld,
      facts.BottomWorld,
      snapshot.Geometry.Width,
      snapshot.Geometry.Height,
      facts.GameMode,
      facts.DrunkWorld,
      facts.GetGoodWorld,
      facts.TenthAnniversaryWorld,
      facts.DontStarveWorld,
      facts.NotTheBeesWorld,
      facts.RemixWorld,
      facts.NoTrapsWorld,
      facts.ZenithWorld,
      facts.SkyblockWorld,
      facts.CreationTime,
      facts.LastPlayed);
  }

  private static WorldFileEnvironmentSection CreateEnvironment(WorldHeaderSnapshot snapshot)
  {
    WorldHeaderFacts facts = snapshot.Facts;
    return new WorldFileEnvironmentSection(
      facts.MoonType,
      facts.TreeX,
      facts.TreeStyle,
      facts.CaveBackX,
      facts.CaveBackStyle,
      facts.IceBackStyle,
      facts.JungleBackStyle,
      facts.HellBackStyle,
      snapshot.Spawn.X,
      snapshot.Spawn.Y,
      facts.WorldSurface,
      facts.RockLayer,
      snapshot.Time,
      snapshot.DayTime,
      snapshot.MoonPhase,
      facts.BloodMoon,
      facts.Eclipse,
      facts.DungeonX,
      facts.DungeonY,
      facts.Crimson);
  }
}
