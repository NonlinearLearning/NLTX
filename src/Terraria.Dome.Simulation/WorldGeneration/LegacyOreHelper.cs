using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyOreHelper
{
  private const ushort ConvertedTileType = 0;

  public static void AppendCommands(
    WorldGridSnapshot snapshot,
    int centerX,
    int centerY,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(commands);
    AppendCommandsExcluding(snapshot, centerX, centerY, null, ref state, commands);
  }

  public static void AppendCommandsExcluding(
    WorldGridSnapshot snapshot,
    int centerX,
    int centerY,
    IReadOnlySet<(int X, int Y)>? excludedCells,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(commands);
    if (centerX <= 0 || centerY <= 0 ||
        centerX >= snapshot.Metadata.Width - 1 ||
        centerY >= snapshot.Metadata.Height - 1)
    {
      throw new ArgumentOutOfRangeException(
        nameof(centerX),
        "The OreHelper center must have an in-world three-by-three neighborhood.");
    }

    for (int x = centerX - 1; x <= centerX + 1; x++)
    {
      for (int y = centerY - 1; y <= centerY + 1; y++)
      {
        if (excludedCells?.Contains((x, y)) == true)
        {
          continue;
        }

        WorldTile tile = snapshot.GetTile(x, y);
        if (tile.Type is not 1 and not 40)
        {
          continue;
        }

        commands.Add(new TileChangeCommand(
          state.ReserveSequence(),
          x,
          y,
          TileChangeKind.UpdateTileType,
          ConvertedTileType,
          IsActive: tile.IsActive,
          Source: "worldgen.ore.OreHelper"));
      }
    }
  }
}
