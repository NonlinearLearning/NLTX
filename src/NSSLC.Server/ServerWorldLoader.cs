using Terraria.WorldData;
using Terraria.Item;
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

  /// <summary>箱子、告示牌与 Tile 实体：世界文件里已有的服务器运算数据。</summary>
  public required WorldTileEntityStore TileEntities { get; init; }
}

internal static class ServerWorldLoader
{
  private const int MaximumWorldFileBytes = 256 * 1024 * 1024;
  private static readonly int[] LatestClientFrameImportantTileIds =
  [
    435, 436, 437, 438, 439,
    647, 648, 649, 650, 651, 652, 693, 694, 706, 753
  ];

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
    IReadOnlyList<WorldTile> decodedTiles =
        new WorldFileTilePayloadDecoder().Decode(tilePayload, facts);
    var tiles = new WorldRuntimeTile[decodedTiles.Count];
    for (int index = 0; index < decodedTiles.Count; index++)
    {
      WorldTile tile = decodedTiles[index];
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
          tile.TileColor,
          tile.WallColor,
          (byte)tile.Wires,
          tile.Actuator,
          tile.Inactive,
          tile.HalfBlock,
          tile.Slope,
          tile.TileHeader3,
          tile.WallHeader);
    }

    return new LoadedServerWorld
    {
      Path = worldPath,
      Header = header,
      Environment = environment,
      Tiles = Array.AsReadOnly(tiles),
      FrameImportant = CreateLatestClientFrameImportance(tilePayload.FrameImportant),
      TileEntities = LoadTileEntities(decoded.Document)
    };
  }

  /// <summary>解码世界文件里的箱子、告示牌与 Tile 实体段。</summary>
  /// <remarks>
  /// 三段在参考客户端里都参与服务器运算：箱子内容与告示牌文本要能读改，Tile 实体决定传送塔、
  /// 训练假人、物品展示框等位置。缺段视为空表，与参考实现"没有记录就没有实体"一致。
  /// </remarks>
  private static WorldTileEntityStore LoadTileEntities(WorldPersistenceDocument document)
  {
    WorldFileChestSection chestSection =
        Optional<WorldFileChestSection>(document, WorldFileChestSection.SectionId)
        ?? WorldFileChestSection.Empty;
    WorldFileSignSection signSection =
        Optional<WorldFileSignSection>(document, WorldFileSignSection.SectionId)
        ?? WorldFileSignSection.Empty;
    WorldFileTileEntitySection tileEntitySection =
        Optional<WorldFileTileEntitySection>(document, WorldFileTileEntitySection.SectionId)
        ?? new WorldFileTileEntitySection(0, ReadOnlyMemory<byte>.Empty);

    var chests = new List<WorldChestState>(chestSection.Chests.Count);
    for (int index = 0; index < chestSection.Chests.Count; index++)
    {
      WorldFileChestRecord record = chestSection.Chests[index];
      WorldChestState chest = new(
          (short)index,
          new WorldTileCoordinate(record.X, record.Y),
          record.Name);
      for (int slot = 0; slot < record.Items.Count && slot < WorldChestState.SlotCount; slot++)
      {
        WorldFileChestItem item = record.Items[slot];
        if (item.IsEmpty || !ItemIdRules.TryFromNetworkType(item.Type, out ItemId itemId))
        {
          continue;
        }

        chest.TrySetItem(slot, new ItemStack(itemId, item.Stack, item.Prefix));
      }

      chests.Add(chest);
    }

    var signs = new List<WorldSignState>(signSection.Signs.Count);
    for (int index = 0; index < signSection.Signs.Count; index++)
    {
      WorldFileSignRecord record = signSection.Signs[index];
      signs.Add(new WorldSignState(
          (short)index,
          new WorldTileCoordinate(record.X, record.Y),
          record.Text));
    }

    WorldTileEntityState[] entities = [];
    if (tileEntitySection.EntityCount > 0 &&
        !WorldTileEntityRecordDecoder.TryDecode(
            tileEntitySection.SerializedRecords,
            tileEntitySection.EntityCount,
            out entities,
            out string failure))
    {
      throw new InvalidDataException($"The tile entity section could not be decoded: {failure}");
    }

    WorldTileEntityStore store = new();
    store.Load(chests, signs, entities);
    return store;
  }

  private static TSection? Optional<TSection>(WorldPersistenceDocument document, string sectionId)
      where TSection : class
  {
    return document.TryGetSection(sectionId, out WorldLoadSection<TSection> section) &&
        section.IsPresent
        ? section.Value
        : null;
  }

  private static IReadOnlyList<bool> CreateLatestClientFrameImportance(
      IReadOnlyList<bool> persistedFrameImportant)
  {
    var result = new bool[Math.Max(754, persistedFrameImportant.Count)];
    for (int index = 0; index < persistedFrameImportant.Count; index++)
    {
      result[index] = persistedFrameImportant[index];
    }

    foreach (int tileType in LatestClientFrameImportantTileIds)
    {
      result[tileType] = true;
    }

    return Array.AsReadOnly(result);
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
