using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria.NonAuthoritative.Persistence;
using Terraria.WorldStorage;

namespace MinimalImplementation.Persistence;

/// <summary>Completes a new-world document from generated and owner-prepared section values.</summary>
public sealed class WorldInitialDocumentBuilder
{
  public const int WorldFileVersion = 326;

  private static readonly string[] RequiredSectionIds =
  [
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
  ];

  private readonly CreativeWorldPowerDocumentCodec _creativeCodec;

  public WorldInitialDocumentBuilder(CreativeWorldPowerDocumentCodec creativeCodec)
  {
    ArgumentNullException.ThrowIfNull(creativeCodec);
    _creativeCodec = creativeCodec;
  }

  public WorldPersistenceDocument Build(
    WorldPersistenceDocument generatedSections,
    CreativeWorldPowerSnapshot creativePowers)
  {
    ArgumentNullException.ThrowIfNull(generatedSections);
    ArgumentNullException.ThrowIfNull(creativePowers);
    if (generatedSections.FormatVersion != WorldFileVersion)
    {
      throw new InvalidDataException("New worlds must target WorldFile version 326.");
    }

    if (!generatedSections.TryGetSection<WorldFileHeaderSection>(
          WorldFileHeaderSection.SectionId,
          out WorldLoadSection<WorldFileHeaderSection> headerSection) ||
        !headerSection.IsPresent)
    {
      throw new InvalidDataException("A new world requires a complete header section.");
    }

    if (!generatedSections.TryGetSection<WorldFileTilePayloadSection>(
          WorldFileTilePayloadSection.SectionId,
          out WorldLoadSection<WorldFileTilePayloadSection> tileSection) ||
        !tileSection.IsPresent || tileSection.Value.CompressedPayload.IsEmpty ||
        tileSection.Value.MaxTilesX != headerSection.Value.MaxTilesX ||
        tileSection.Value.MaxTilesY != headerSection.Value.MaxTilesY)
    {
      throw new InvalidDataException(
        "A new world requires nonempty generated tiles matching the header dimensions.");
    }

    WorldFileHeaderSection header = headerSection.Value;
    if (header.UniqueId is null || header.UniqueId == Guid.Empty)
    {
      throw new InvalidDataException("A new world requires a nonzero persisted world identifier.");
    }

    WorldPersistenceDocument document = generatedSections.WithReplacements(
      WorldPersistenceSection.Create(
        WorldFileMetadataSection.SectionId,
        WorldFileMetadataSection.Empty),
      WorldPersistenceSection.Create(
        WorldFileCreativePowersSection.SectionId,
        new WorldFileCreativePowersSection(_creativeCodec.Encode(creativePowers))),
      WorldPersistenceSection.Create(
        WorldFileFooterSection.SectionId,
        new WorldFileFooterSection(true, header.WorldName, header.WorldId)));

    string[] missingSections = RequiredSectionIds
      .Where(sectionId => !document.SectionIds.Contains(sectionId, StringComparer.Ordinal))
      .ToArray();
    string[] unknownSections = document.SectionIds
      .Where(sectionId => !RequiredSectionIds.Contains(sectionId, StringComparer.Ordinal))
      .ToArray();
    if (missingSections.Length != 0 || unknownSections.Length != 0)
    {
      throw new InvalidDataException(
        $"The new world section set is incomplete or unknown. Missing: " +
        $"{string.Join(", ", missingSections)}; unknown: {string.Join(", ", unknownSections)}.");
    }

    return document;
  }
}
