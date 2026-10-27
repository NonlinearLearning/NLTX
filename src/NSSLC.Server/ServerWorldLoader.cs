using MinimalImplementation.Server.World;
using Terraria.Liquid;
using Terraria.NonAuthoritative.Persistence;
using Terraria.NonAuthoritative.WorldStorage;
using Terraria.WorldStorage;
using Terraria.World;
using WorldRuntimeTile = Terraria.World.WorldTileState;

namespace NSSLC.Server;

internal sealed class LoadedServerWorld
{
  public required string Path { get; init; }

  public required WorldFileHeaderSection Header { get; init; }

  public required WorldFileEnvironmentSection Environment { get; init; }

  public required IReadOnlyList<WorldRuntimeTile> Tiles { get; init; }

  public required IReadOnlyList<bool> FrameImportant { get; init; }
}

internal static class ServerWorldLoader
{
  private const int MaximumWorldFileBytes = 256 * 1024 * 1024;

  public static LoadedServerWorld Load(string? requestedPath)
  {
    string worldPath = ResolvePath(requestedPath);
    byte[] bytes = File.ReadAllBytes(worldPath);
    if (bytes.Length > MaximumWorldFileBytes)
    {
      throw new InvalidDataException(
          $"The world file exceeds the {MaximumWorldFileBytes}-byte server limit.");
    }

    WorldFileDocumentDecoder decoder = new();
    WorldPersistenceDecodeResult decoded = decoder.Decode(bytes);
    if (!decoded.Succeeded || decoded.Document is null)
    {
      throw new InvalidDataException(
          $"The world file could not be decoded: {decoded.Failure.Detail ?? "invalid data"}.");
    }

    WorldStorageFailure validation = new WorldFileDocumentValidator().Validate(decoded.Document);
    if (validation.Kind != WorldStorageFailureKind.None)
    {
      throw new InvalidDataException(
          $"The world file failed validation: {validation.Detail ?? validation.Kind.ToString()}.");
    }

    WorldFileHeaderSection header = Required<WorldFileHeaderSection>(
        decoded.Document,
        WorldFileHeaderSection.SectionId);
    WorldFileEnvironmentSection environment = Required<WorldFileEnvironmentSection>(
        decoded.Document,
        WorldFileEnvironmentSection.SectionId);
    WorldFileTilePayloadSection tilePayload = Required<WorldFileTilePayloadSection>(
        decoded.Document,
        WorldFileTilePayloadSection.SectionId);
    WorldFacts facts = WorldFacts.CreateVersion4();
    IReadOnlyList<MinimalImplementation.Server.World.WorldTile> decodedTiles =
        new WorldFileTilePayloadDecoder().Decode(tilePayload, facts);
    var tiles = new WorldRuntimeTile[decodedTiles.Count];
    for (int index = 0; index < decodedTiles.Count; index++)
    {
      MinimalImplementation.Server.World.WorldTile tile = decodedTiles[index];
      LiquidContactFlags liquid = tile.LiquidAmount == 0 ? LiquidContactFlags.None : tile.LiquidType switch
      {
        LiquidType.Water => LiquidContactFlags.Water,
        LiquidType.Lava => LiquidContactFlags.Lava,
        LiquidType.Honey => LiquidContactFlags.Honey,
        LiquidType.Shimmer => LiquidContactFlags.Shimmer,
        _ => LiquidContactFlags.None
      };
      tiles[index] = new WorldRuntimeTile(
          tile.Active,
          tile.Type,
          tile.Wall,
          tile.FrameX,
          tile.FrameY,
          liquid,
          tile.LiquidAmount,
          tile.TileColor);
    }

    return new LoadedServerWorld
    {
      Path = worldPath,
      Header = header,
      Environment = environment,
      Tiles = Array.AsReadOnly(tiles),
      FrameImportant = tilePayload.FrameImportant
    };
  }

  private static string ResolvePath(string? requestedPath)
  {
    string path = string.IsNullOrWhiteSpace(requestedPath)
        ? Environment.GetEnvironmentVariable("NSSLC_WORLD_FILE") ??
          Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "src", "World", "科研.wld")
        : requestedPath;
    string fullPath = Path.GetFullPath(path);
    if (!File.Exists(fullPath))
    {
      throw new FileNotFoundException("The configured world file does not exist.", fullPath);
    }

    return fullPath;
  }

  private static TSection Required<TSection>(WorldPersistenceDocument document, string sectionId)
      where TSection : notnull
  {
    if (!document.TryGetSection(sectionId, out WorldLoadSection<TSection> section) ||
        !section.IsPresent)
    {
      throw new InvalidDataException($"The world file is missing section '{sectionId}'.");
    }

    return section.Value;
  }
}
