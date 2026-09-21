using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacySurfaceIsDesertNoSurfaceCleanup
{
  public static void AppendCommands(
    WorldGridSnapshot snapshot,
    bool skyblockWorld,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(commands);
    if (skyblockWorld)
    {
      return;
    }

    for (int y = 5; y < snapshot.Metadata.Height - 5; y++)
    {
      for (int x = 5; x < snapshot.Metadata.Width - 5; x++)
      {
        WorldTile tile = snapshot.GetTile(x, y);
        if (tile.WallType is not (187 or 216) || tile.Type is not (147 or 161))
        {
          continue;
        }

        ushort tileType = tile.Type == 147 ? (ushort)397 : (ushort)396;
        commands.Add(new TileChangeCommand(
          state.ReserveSequence(), x, y, TileChangeKind.UpdateTileType, tileType,
          Source: "worldgen.biome.SurfaceIsDesertNoSurfaceCleanup"));
      }
    }
  }
}
