using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyMudCavesToJungleGrassPass
{
  private const ushort MudTileType = 59;
  private const ushort JungleGrassTileType = 60;

  public static void AppendCommands(
    WorldGridSnapshot snapshot,
    bool isSkyblockWorld,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(commands);
    if (isSkyblockWorld)
    {
      return;
    }

    for (int x = 0; x < snapshot.Metadata.Width; x++)
    {
      for (int y = 0; y < snapshot.Metadata.Height; y++)
      {
        WorldTile tile = snapshot.GetTile(x, y);
        if (!tile.IsActive || tile.Type != MudTileType)
        {
          continue;
        }

        commands.Add(new TileChangeCommand(
          state.ReserveSequence(),
          x,
          y,
          TileChangeKind.UpdateTileType,
          JungleGrassTileType,
          Source: "worldgen.MudCavesToJungleGrass"));
      }
    }
  }
}
