using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyNoSurfaceTopFill
{
  private const ushort ConvertedTileType = 59;
  private const int MinimumTopRows = 100;
  private const ushort MushroomGrassTileType = 60;
  private const ushort ObsidianTileType = 70;

  public static void AppendCommands(
    WorldGridSnapshot snapshot,
    bool skyblockWorld,
    LegacyEvilReplacementDefinitions definitions,
    LegacyPassRandomState random,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(definitions);
    ArgumentNullException.ThrowIfNull(random);
    ArgumentNullException.ThrowIfNull(commands);
    if (skyblockWorld)
    {
      return;
    }

    for (int x = 0; x < snapshot.Metadata.Width; x++)
    {
      int rows = Math.Min(snapshot.Metadata.Height, MinimumTopRows + random.Next(2));
      for (int y = 0; y < rows; y++)
      {
        WorldTile tile = snapshot.GetTile(x, y);
        if (definitions.DungeonWallTypes.Contains(tile.WallType))
        {
          continue;
        }

        bool isDungeonTile = definitions.DungeonTileTypes.Contains(tile.Type);
        ushort targetTileType = tile.Type is MushroomGrassTileType or ObsidianTileType
          ? ConvertedTileType
          : tile.Type;
        if (isDungeonTile && targetTileType == tile.Type)
        {
          continue;
        }

        commands.Add(new TileChangeCommand(
          state.ReserveSequence(),
          x,
          y,
          TileChangeKind.UpdateTileType,
          targetTileType,
          Source: "worldgen.biome.NoSurfaceFillTheTop",
          IsActive: isDungeonTile ? tile.IsActive : true));
      }
    }
  }
}
