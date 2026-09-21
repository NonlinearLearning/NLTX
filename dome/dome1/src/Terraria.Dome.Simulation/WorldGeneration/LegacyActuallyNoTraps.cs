using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyActuallyNoTraps
{
  public static void AppendHardModeCommands(
    WorldGridSnapshot snapshot,
    bool actuallyNoTrapsForReal,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(commands);
    if (!actuallyNoTrapsForReal)
    {
      return;
    }

    for (int x = 0; x < snapshot.Metadata.Width; x++)
    {
      for (int y = 0; y < snapshot.Metadata.Height; y++)
      {
        if (snapshot.GetTile(x, y).Type is not (48 or 232))
        {
          continue;
        }

        commands.Add(new TileChangeCommand(
          state.ReserveSequence(), x, y, TileChangeKind.Kill, 0,
          Source: "worldgen.biome.ActuallyNoTraps"));
      }
    }
  }
}
