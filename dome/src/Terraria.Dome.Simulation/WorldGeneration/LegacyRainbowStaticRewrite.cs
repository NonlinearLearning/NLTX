using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyRainbowStaticRewrite
{
  public static void AppendCommands(
    WorldGridSnapshot snapshot,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(commands);
    for (int x = 0; x < snapshot.Metadata.Width; x++)
    {
      for (int y = 0; y < snapshot.Metadata.Height; y++)
      {
        WorldTile tile = snapshot.GetTile(x, y);
        if (tile.Type is 189 or 202)
        {
          ushort tileType = tile.Type == 189 ? (ushort)719 : (ushort)692;
          commands.Add(new TileChangeCommand(
            state.ReserveSequence(), x, y, TileChangeKind.UpdateTileType, tileType,
            Source: "worldgen.biome.RainbowStaticRewrite"));
        }

        if (tile.WallType == 82)
        {
          commands.Add(new TileChangeCommand(
            state.ReserveSequence(), x, y, TileChangeKind.SetWall, 0, WallType: 346,
            Source: "worldgen.biome.RainbowStaticRewrite"));
        }
      }
    }
  }
}
