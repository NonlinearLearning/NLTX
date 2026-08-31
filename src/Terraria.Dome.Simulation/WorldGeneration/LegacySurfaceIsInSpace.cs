using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacySurfaceIsInSpace
{
  private const ushort CloudWallType = 73;
  private const ushort SandTileType = 53;

  public static void AppendCommands(
    WorldGridSnapshot snapshot,
    int worldSurfaceY,
    bool skyblockWorld,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(commands);
    if (worldSurfaceY < 0 || worldSurfaceY > snapshot.Metadata.Height)
    {
      throw new ArgumentOutOfRangeException(nameof(worldSurfaceY));
    }

    if (skyblockWorld)
    {
      return;
    }

    for (int x = 0; x < snapshot.Metadata.Width; x++)
    {
      for (int y = 0; y < worldSurfaceY; y++)
      {
        WorldTile tile = snapshot.GetTile(x, y);
        if (tile.WallType != CloudWallType || (tile.IsActive && tile.Type != SandTileType))
        {
          continue;
        }

        commands.Add(new TileChangeCommand(
          state.ReserveSequence(),
          x,
          y,
          TileChangeKind.SetWall,
          0,
          WallType: 0,
          Source: "worldgen.biome.SurfaceIsInSpace"));
      }
    }
  }
}
