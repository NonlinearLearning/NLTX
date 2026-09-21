using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyPaintEverythingGray
{
  private const ushort HeartstoneTileType = 178;
  private const byte GrayPaintColor = 27;
  private const byte WhitePaintColor = 26;

  public static void AppendCommands(
    WorldGridSnapshot snapshot,
    int worldSurfaceY,
    bool useWhitePaint,
    bool justTheSurface,
    bool justTreasure,
    IReadOnlySet<ushort> oreTileTypes,
    IReadOnlySet<ushort> gemTileTypes,
    LegacyPassRandomState random,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(oreTileTypes);
    ArgumentNullException.ThrowIfNull(gemTileTypes);
    ArgumentNullException.ThrowIfNull(random);
    ArgumentNullException.ThrowIfNull(commands);
    if (worldSurfaceY < 0 || worldSurfaceY > snapshot.Metadata.Height)
    {
      throw new ArgumentOutOfRangeException(nameof(worldSurfaceY));
    }

    byte paintColor = useWhitePaint ? WhitePaintColor : GrayPaintColor;
    for (int x = 0; x < snapshot.Metadata.Width; x++)
    {
      int upperBoundExclusive = justTheSurface
        ? Math.Min(snapshot.Metadata.Height, worldSurfaceY + random.Next(3))
        : snapshot.Metadata.Height;
      for (int y = 0; y < upperBoundExclusive; y++)
      {
        WorldTile tile = snapshot.GetTile(x, y);
        if (justTreasure)
        {
          if (oreTileTypes.Contains(tile.Type) ||
              gemTileTypes.Contains(tile.Type) ||
              tile.Type == HeartstoneTileType)
          {
            commands.Add(new TileChangeCommand(
              state.ReserveSequence(),
              x,
              y,
              TileChangeKind.SetPaint,
              0,
              TileColor: paintColor,
              Source: "worldgen.secretseed.PaintEverythingGray"));
          }

          continue;
        }

        commands.Add(new TileChangeCommand(
          state.ReserveSequence(),
          x,
          y,
          TileChangeKind.SetPaint,
          0,
          TileColor: paintColor,
          WallColor: paintColor,
          Source: "worldgen.secretseed.PaintEverythingGray"));
      }
    }
  }
}
