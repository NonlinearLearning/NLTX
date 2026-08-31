using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyUnderworldLavaShelfPass
{
  private const byte LavaLiquidType = 1;

  public static void AppendCommands(
    WorldGridSnapshot snapshot,
    ref WorldGenerationStateComponent state,
    List<LiquidChangeCommand> commands,
    bool isSkyblockWorld = false)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(commands);
    if (isSkyblockWorld)
    {
      return;
    }

    int[] shelfOffsets =
    [
      LegacyUnderworldLavaShelfPolicy.FirstShelfOffset,
      LegacyUnderworldLavaShelfPolicy.SecondShelfOffset
    ];
    for (int shelfIndex = 0; shelfIndex < shelfOffsets.Length; shelfIndex++)
    {
      int y = LegacyUnderworldLavaShelfPolicy.GetShelfY(
        snapshot.Metadata.Height,
        shelfOffsets[shelfIndex]);
      for (int x = 0; x < snapshot.Metadata.Width; x++)
      {
        if (!LegacyUnderworldLavaShelfPolicy.ShouldFill(snapshot.GetTile(x, y)))
        {
          continue;
        }

        commands.Add(new LiquidChangeCommand(
          state.ReserveSequence(),
          x,
          y,
          byte.MaxValue,
          LavaLiquidType,
          Source: "worldgen.underworld.lava-shelf"));
      }
    }
  }
}
