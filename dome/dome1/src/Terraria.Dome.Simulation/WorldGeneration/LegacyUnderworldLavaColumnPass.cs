using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyUnderworldLavaColumnPass
{
  private const byte LavaLiquidType = 1;
  private const int LavaMaximumYInset = 10;

  public static void AppendCommands(
    WorldGridSnapshot snapshot,
    LegacyPassRandomState random,
    ref WorldGenerationStateComponent state,
    List<LiquidChangeCommand> commands,
    bool isSkyblockWorld = false)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(random);
    ArgumentNullException.ThrowIfNull(commands);
    if (isSkyblockWorld)
    {
      return;
    }

    int topY = LegacyUnderworldLavaColumnPolicy.CreateInitialTop(
      snapshot.Metadata.Height,
      random);
    for (int x = 10; x < snapshot.Metadata.Width - 10; x++)
    {
      topY = LegacyUnderworldLavaColumnPolicy.AdvanceTop(
        topY,
        snapshot.Metadata.Height,
        random);
      for (int y = topY; y < snapshot.Metadata.Height - LavaMaximumYInset; y++)
      {
        if (!LegacyUnderworldLavaColumnPolicy.ShouldFillLava(snapshot.GetTile(x, y)))
        {
          continue;
        }

        commands.Add(new LiquidChangeCommand(
          state.ReserveSequence(),
          x,
          y,
          byte.MaxValue,
          LavaLiquidType,
          Source: "worldgen.underworld.lava-column"));
      }
    }
  }
}
