using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyCoatEverythingIlluminant
{
  public static void AppendCommands(
    WorldGridSnapshot snapshot,
    bool justSomeThings,
    bool justRandomSpots,
    LegacyCoatEverythingIlluminantProfile profile,
    LegacyPassRandomState random,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(profile);
    ArgumentNullException.ThrowIfNull(profile.SelectiveTileTypes);
    ArgumentNullException.ThrowIfNull(random);
    ArgumentNullException.ThrowIfNull(commands);
    for (int x = 0; x < snapshot.Metadata.Width; x++)
    {
      for (int y = 0; y < snapshot.Metadata.Height; y++)
      {
        WorldTile tile = snapshot.GetTile(x, y);
        if (justSomeThings)
        {
          if (profile.SelectiveTileTypes.Contains(tile.Type))
          {
            AppendCoatingCommand(x, y, true, null, null, null, ref state, commands);
          }

          continue;
        }

        if (justRandomSpots)
        {
          if (random.Next(2) == 0)
          {
            AppendCoatingCommand(x, y, true, true, false, false, ref state, commands);
          }

          continue;
        }

        AppendCoatingCommand(x, y, true, true, null, null, ref state, commands);
      }
    }
  }

  private static void AppendCoatingCommand(
    int x,
    int y,
    bool? isFullbrightBlock,
    bool? isFullbrightWall,
    bool? isInvisibleBlock,
    bool? isInvisibleWall,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands)
  {
    commands.Add(new TileChangeCommand(
      state.ReserveSequence(),
      x,
      y,
      TileChangeKind.SetCoating,
      0,
      IsInvisibleBlock: isInvisibleBlock,
      IsInvisibleWall: isInvisibleWall,
      IsFullbrightBlock: isFullbrightBlock,
      IsFullbrightWall: isFullbrightWall,
      Source: "worldgen.secretseed.CoatEverythingIlluminant"));
  }
}
