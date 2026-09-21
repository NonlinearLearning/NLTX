using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyNoSpiderCavesCleanup
{
  private const ushort SpiderWallType = 62;

  public static void AppendCommands(
    WorldGridSnapshot snapshot,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(commands);
    for (int y = 5; y < snapshot.Metadata.Height - 5; y++)
    {
      for (int x = 5; x < snapshot.Metadata.Width - 5; x++)
      {
        if (snapshot.GetTile(x, y).WallType is not (15 or 86 or 178 or 180 or 204 or 205 or 206 or 207))
        {
          continue;
        }

        commands.Add(new TileChangeCommand(
          state.ReserveSequence(), x, y, TileChangeKind.SetWall, 0, WallType: SpiderWallType,
          Source: "worldgen.biome.NoSpiderCavesCleanup"));
      }
    }
  }
}
