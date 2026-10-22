using System.IO;
using Terraria.NonAuthoritative.Persistence;
using Terraria.WorldStorage;

namespace MinimalImplementation.Server.World;

/// <summary>Builds a P03 candidate only from a decoded and validated Version4 world document.</summary>
public sealed class WorldDocumentWorldFactory
{
  private readonly WorldFacts _facts;
  private readonly WorldFileTilePayloadDecoder _tileDecoder;

  public WorldDocumentWorldFactory(WorldFacts facts, WorldFileTilePayloadDecoder tileDecoder)
  {
    ArgumentNullException.ThrowIfNull(facts);
    ArgumentNullException.ThrowIfNull(tileDecoder);
    _facts = facts;
    _tileDecoder = tileDecoder;
  }

  public WorldPreparationRequest CreateCandidate(WorldPersistenceDocument document)
  {
    ArgumentNullException.ThrowIfNull(document);
    if (!document.TryGetSection<WorldFileHeaderSection>(
          WorldFileHeaderSection.SectionId,
          out WorldLoadSection<WorldFileHeaderSection> headerSection) ||
        !document.TryGetSection<WorldFileEnvironmentSection>(
          WorldFileEnvironmentSection.SectionId,
          out WorldLoadSection<WorldFileEnvironmentSection> environmentSection) ||
        !document.TryGetSection<WorldFileTilePayloadSection>(
          WorldFileTilePayloadSection.SectionId,
          out WorldLoadSection<WorldFileTilePayloadSection> tileSection))
    {
      throw new InvalidDataException("The world document lacks required P03 header, environment or tile data.");
    }

    WorldFileHeaderSection header = headerSection.Value;
    WorldFileEnvironmentSection environment = environmentSection.Value;
    WorldFileTilePayloadSection tilePayload = tileSection.Value;
    if (header.UniqueId is null || header.UniqueId == Guid.Empty)
    {
      throw new InvalidDataException("The world document does not contain a usable unique id.");
    }

    if (header.MaxTilesX != tilePayload.MaxTilesX || header.MaxTilesY != tilePayload.MaxTilesY)
    {
      throw new InvalidDataException("The world header dimensions do not match its tile payload.");
    }

    if (environment.MoonPhase is < 0 or > 7)
    {
      throw new InvalidDataException("The world moon phase is outside the Version4 range.");
    }

    var geometry = new WorldGeometry(header.MaxTilesX, header.MaxTilesY);
    var spawn = new TileCoordinate(environment.SpawnTileX, environment.SpawnTileY);
    var headerFacts = new WorldHeaderFacts(
      header.SeedText,
      header.WorldGeneratorVersion,
      header.LeftWorld,
      header.RightWorld,
      header.TopWorld,
      header.BottomWorld,
      header.GameMode,
      header.DrunkWorld,
      header.GetGoodWorld,
      header.TenthAnniversaryWorld,
      header.DontStarveWorld,
      header.NotTheBeesWorld,
      header.RemixWorld,
      header.NoTrapsWorld,
      header.ZenithWorld,
      header.SkyblockWorld,
      header.CreationTime,
      header.LastPlayed,
      environment.MoonType,
      environment.TreeX,
      environment.TreeStyle,
      environment.CaveBackX,
      environment.CaveBackStyle,
      environment.IceBackStyle,
      environment.JungleBackStyle,
      environment.HellBackStyle,
      environment.WorldSurface,
      environment.RockLayer,
      environment.BloodMoon,
      environment.Eclipse,
      environment.DungeonX,
      environment.DungeonY,
      environment.Crimson);
    var worldHeader = new WorldHeaderSnapshot(
      header.WorldName,
      geometry,
      spawn,
      header.UniqueId.Value,
      environment.DayTime,
      environment.Time,
      (byte)environment.MoonPhase,
      header.WorldId,
      headerFacts,
      CaptureTeamSpawns(document));
    IReadOnlyList<WorldTile> tiles = _tileDecoder.Decode(tilePayload, _facts);
    return new WorldPreparationRequest(worldHeader, tiles, _facts.Fingerprint);
  }

  private static WorldTeamSpawnFacts CaptureTeamSpawns(WorldPersistenceDocument document) {
    if (!document.TryGetSection<WorldFileTimePolicySection>(
        WorldFileTimePolicySection.SectionId, out var policy) ||
        !document.TryGetSection<WorldFileSpawnSection>(
        WorldFileSpawnSection.SectionId, out var spawns)) {
      return WorldTeamSpawnFacts.Unavailable;
    }
    return new WorldTeamSpawnFacts(policy.Value.TeamBasedSpawnsSeed,
        spawns.Value.ExtraSpawnPoints.Select(point => new TileCoordinate(point.X, point.Y))
            .ToArray());
  }
}
