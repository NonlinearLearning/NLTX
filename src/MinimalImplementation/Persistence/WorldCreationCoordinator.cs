using System;
using System.Collections.Generic;
using Terraria.NonAuthoritative.Persistence;
using Terraria.WorldStorage;

namespace MinimalImplementation.Persistence;

/// <summary>
/// Builds a complete, detached Version4 document for an explicit create-world request.
/// </summary>
/// <remarks>
/// This coordinator does not read or write a path. Existing-world loading stays in
/// <see cref="MinimalImplementation.Server.Persistence.WorldDocumentLoader"/> and file commit stays
/// in <see cref="WorldDocumentSaver"/>. Keeping those effects outside this class prevents a missing
/// file from silently turning a normal load into an overwrite-capable create operation.
/// </remarks>
public sealed class WorldCreationCoordinator
{
  public const int WorldFileVersion = 326;
  private const int TreeTopVariationCount = 13;

  private readonly WorldInitialDocumentBuilder _initialDocumentBuilder;

  public WorldCreationCoordinator(WorldInitialDocumentBuilder initialDocumentBuilder)
  {
    ArgumentNullException.ThrowIfNull(initialDocumentBuilder);
    _initialDocumentBuilder = initialDocumentBuilder;
  }

  public WorldPersistenceDocument CreateDocument(
    WorldCreationRequest request,
    CreativeWorldPowerSnapshot creativePowers)
  {
    ArgumentNullException.ThrowIfNull(request);
    ArgumentNullException.ThrowIfNull(creativePowers);

    var header = new WorldFileHeaderSection(
      request.WorldName,
      request.SeedText,
      request.WorldGeneratorVersion,
      request.UniqueId,
      request.WorldId,
      leftWorld: 0,
      rightWorld: request.MaxTilesX,
      topWorld: 0,
      bottomWorld: request.MaxTilesY,
      request.MaxTilesX,
      request.MaxTilesY,
      request.GameMode,
      drunkWorld: false,
      getGoodWorld: false,
      tenthAnniversaryWorld: false,
      dontStarveWorld: false,
      notTheBeesWorld: false,
      remixWorld: false,
      noTrapsWorld: false,
      zenithWorld: false,
      skyblockWorld: false,
      request.CreationTime,
      request.LastPlayed);

    var environment = new WorldFileEnvironmentSection(
      moonType: 0,
      treeX: new int[3],
      treeStyle: new int[4],
      caveBackX: new int[3],
      caveBackStyle: new int[4],
      iceBackStyle: 0,
      jungleBackStyle: 0,
      hellBackStyle: 0,
      request.SpawnTileX,
      request.SpawnTileY,
      worldSurface: request.SpawnTileY,
      rockLayer: request.SpawnTileY,
      time: 0,
      dayTime: true,
      moonPhase: 0,
      bloodMoon: false,
      eclipse: false,
      dungeonX: 0,
      dungeonY: 0,
      crimson: false);

    var tilePayload = new WorldFileTilePayloadSection(
      request.MaxTilesX,
      request.MaxTilesY,
      request.FrameImportant,
      request.CompressedTilePayload);

    WorldPersistenceDocument generated = new(
      WorldFileVersion,
      new WorldPersistenceSection[]
      {
        WorldPersistenceSection.Create(WorldFileHeaderSection.SectionId, header),
        WorldPersistenceSection.Create(
          WorldFileMetadataSection.SectionId,
          WorldFileMetadataSection.Empty),
        WorldPersistenceSection.Create(
          WorldFileEnvironmentSection.SectionId,
          environment),
        WorldPersistenceSection.Create(
          WorldFileProgressionSection.SectionId,
          WorldFileProgressionSection.Empty),
        WorldPersistenceSection.Create(WorldFileQuestSection.SectionId, WorldFileQuestSection.Empty),
        WorldPersistenceSection.Create(WorldFileBannerSection.SectionId, WorldFileBannerSection.Empty),
        WorldPersistenceSection.Create(
          WorldFileBossProgressionSection.SectionId,
          WorldFileBossProgressionSection.Empty),
        WorldPersistenceSection.Create(WorldFilePartySection.SectionId, WorldFilePartySection.Empty),
        WorldPersistenceSection.Create(
          WorldFileSandstormSection.SectionId,
          WorldFileSandstormSection.Empty),
        WorldPersistenceSection.Create(
          WorldFileDefenderEventSection.SectionId,
          WorldFileDefenderEventSection.Empty),
        WorldPersistenceSection.Create(
          WorldFileBackgroundSection.SectionId,
          WorldFileBackgroundSection.Empty),
        WorldPersistenceSection.Create(WorldFileEventSection.SectionId, WorldFileEventSection.Empty),
        WorldPersistenceSection.Create(
          WorldFileTreeTopsSection.SectionId,
          new WorldFileTreeTopsSection(new int[TreeTopVariationCount])),
        WorldPersistenceSection.Create(
          WorldFileSeasonalSection.SectionId,
          WorldFileSeasonalSection.Empty),
        WorldPersistenceSection.Create(
          WorldFileNpcUnlockSection.SectionId,
          WorldFileNpcUnlockSection.Empty),
        WorldPersistenceSection.Create(
          WorldFileTimePolicySection.SectionId,
          WorldFileTimePolicySection.Empty),
        WorldPersistenceSection.Create(WorldFileSpawnSection.SectionId, WorldFileSpawnSection.Empty),
        WorldPersistenceSection.Create(WorldFileTilePayloadSection.SectionId, tilePayload),
        WorldPersistenceSection.Create(WorldFileChestSection.SectionId, WorldFileChestSection.Empty),
        WorldPersistenceSection.Create(WorldFileSignSection.SectionId, WorldFileSignSection.Empty),
        WorldPersistenceSection.Create(WorldFileNpcSection.SectionId, WorldFileNpcSection.Empty),
        WorldPersistenceSection.Create(
          WorldFileTileEntitySection.SectionId,
          new WorldFileTileEntitySection(0, ReadOnlyMemory<byte>.Empty)),
        WorldPersistenceSection.Create(
          WorldFilePressurePlateSection.SectionId,
          WorldFilePressurePlateSection.Empty),
        WorldPersistenceSection.Create(
          WorldFileTownManagerSection.SectionId,
          WorldFileTownManagerSection.Empty),
        WorldPersistenceSection.Create(
          WorldFileBestiarySection.SectionId,
          WorldFileBestiarySection.Empty),
        WorldPersistenceSection.Create(
          WorldFileCreativePowersSection.SectionId,
          WorldFileCreativePowersSection.Empty),
        WorldPersistenceSection.Create(
          WorldFileFooterSection.SectionId,
          new WorldFileFooterSection(false, request.WorldName, request.WorldId))
      });

    return _initialDocumentBuilder.Build(generated, creativePowers);
  }
}
