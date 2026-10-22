using System;
using System.IO;
using System.Linq;
using System.Threading;
using MinimalImplementation.Persistence;
using MinimalImplementation.Server.World;
using NSSLC.WorldGeneration;
using Terraria.NonAuthoritative.Persistence;
using Terraria.NonAuthoritative.WorldStorage;
using Terraria.WorldStorage;
using TileCoordinate = MinimalImplementation.Server.World.TileCoordinate;

namespace MinimalImplementation.Server.Host;

/// <summary>Projects a completed generation into the server's current persistence contract.</summary>
public static class GeneratedWorldDocumentFactory {
  public static WorldPersistenceDocument Create(
      GeneratedWorld world, WorldFacts facts, string worldName, Guid uniqueId,
      DateTime creationTime, CancellationToken cancellationToken = default) {
    ArgumentNullException.ThrowIfNull(world);
    ArgumentNullException.ThrowIfNull(facts);
    cancellationToken.ThrowIfCancellationRequested();
    if (world.FrameImportant.Count != WorldFacts.TileCount ||
        !world.FrameImportant.SequenceEqual(facts.FrameImportant)) {
      throw new InvalidDataException("The generator and server frame rules do not agree.");
    }

    var geometry = new WorldGeometry(world.Width, world.Height);
    if (world.Width <= 0 || world.Height <= 0 ||
        (long)world.Width * world.Height > WorldGeometry.MaximumTileCount ||
        !geometry.Contains(new TileCoordinate(world.SpawnX, world.SpawnY))) {
      throw new InvalidDataException("The generated dimensions or spawn are invalid.");
    }

    var tiles = new WorldTile[geometry.TileCount];
    int activeTiles = 0;
    for (int y = 0; y < world.Height; y++) {
      cancellationToken.ThrowIfCancellationRequested();
      for (int x = 0; x < world.Width; x++) {
        GeneratedTile tile = world.GetTile(x, y);
        tiles[y * world.Width + x] = ConvertTile(tile);
        if (tile.Active) {
          activeTiles++;
        }
      }
    }
    if (activeTiles == 0) {
      throw new InvalidDataException("World generation returned an empty tile map.");
    }

    WorldFileTilePayloadSection tileSection = new WorldFileTilePayloadEncoder().Encode(
        geometry, tiles, facts, cancellationToken);
    WorldFileTileEntitySection entitySection = new WorldFileTileEntityCodec().Encode(
        world.EntityContents.Entities, cancellationToken);
    WorldPersistenceDocument document = GeneratedWorldDocumentProjection.Create(
        world, worldName, uniqueId, creationTime, tileSection, entitySection);
    cancellationToken.ThrowIfCancellationRequested();
    return new WorldInitialDocumentBuilder(new CreativeWorldPowerDocumentCodec()).Build(
        document, CreativeWorldPowerSnapshot.Defaults);
  }

  private static WorldTile ConvertTile(GeneratedTile tile) {
    bool halfBlock = (tile.TileHeader & 0x400) != 0;
    return new WorldTile(
        Active: tile.Active,
        Type: tile.Type,
        Wall: tile.Wall,
        FrameX: tile.FrameX,
        FrameY: tile.FrameY,
        LiquidAmount: tile.Liquid,
        LiquidType: (LiquidType)tile.LiquidType,
        TileColor: (byte)(tile.TileHeader & 0x1f),
        WallColor: (byte)(tile.Header & 0x1f),
        Wires: (TileWireMask)(((tile.TileHeader & 0x380) >> 7) |
            ((tile.Header & 0x80) >> 4)),
        Actuator: (tile.TileHeader & 0x800) != 0,
        Inactive: (tile.TileHeader & 0x40) != 0,
        HalfBlock: halfBlock,
        Slope: halfBlock ? (byte)0 : (byte)((tile.TileHeader & 0x7000) >> 12),
        TileHeader3: (byte)(tile.Header3 & 0xe0),
        WallHeader: (byte)((tile.TileHeader & 0x8000) >> 8));
  }
}
