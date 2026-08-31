using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyHallowOnSurface
{
  public static void AppendCommands(
    WorldGridSnapshot snapshot,
    int worldSurfaceY,
    int rockLayerY,
    bool noSurface,
    bool worldIsInfected,
    bool noInfection,
    bool skyblockWorld,
    LegacyPassRandomState random,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(random);
    ArgumentNullException.ThrowIfNull(commands);
    if (worldSurfaceY < 0 || worldSurfaceY > snapshot.Metadata.Height ||
        rockLayerY < 0 || rockLayerY > snapshot.Metadata.Height)
    {
      throw new ArgumentOutOfRangeException(nameof(worldSurfaceY));
    }

    if (skyblockWorld)
    {
      return;
    }

    int baseUpperBound = noSurface
      ? (!worldIsInfected || !noInfection ? rockLayerY : rockLayerY / 2)
      : worldSurfaceY;
    for (int x = 0; x < snapshot.Metadata.Width; x++)
    {
      int upperBound = Math.Min(snapshot.Metadata.Height, baseUpperBound + random.Next(3));
      for (int y = 0; y < upperBound; y++)
      {
        WorldTile tile = snapshot.GetTile(x, y);
        if (TryGetHallowTileType(tile.Type, noSurface, out ushort tileType))
        {
          commands.Add(new TileChangeCommand(
            state.ReserveSequence(), x, y, TileChangeKind.UpdateTileType, tileType,
            Source: "worldgen.biome.HallowOnSurface"));
        }

        if (TryGetHallowWallType(tile.WallType, out ushort wallType))
        {
          commands.Add(new TileChangeCommand(
            state.ReserveSequence(), x, y, TileChangeKind.SetWall, 0, WallType: wallType,
            Source: "worldgen.biome.HallowOnSurface"));
        }
      }
    }
  }

  private static bool TryGetHallowTileType(ushort source, bool noSurface, out ushort target)
  {
    target = source switch
    {
      1 when noSurface => 117,
      2 => 109,
      53 => 116,
      161 => 164,
      396 => 403,
      397 => 402,
      _ => source
    };
    return target != source;
  }

  private static bool TryGetHallowWallType(ushort source, out ushort target)
  {
    target = source switch
    {
      63 or 65 or 66 or 68 => 70,
      187 => 222,
      216 => 219,
      _ => source
    };
    return target != source;
  }
}
