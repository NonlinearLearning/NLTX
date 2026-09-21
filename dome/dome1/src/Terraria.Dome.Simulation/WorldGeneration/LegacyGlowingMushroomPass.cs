using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyGlowingMushroomPass
{
  private const ushort EmptyTileType = 0;
  private const ushort MushroomGrassType = 59;

  public static bool IsEligible(WorldTile tile)
  {
    return tile.Type == EmptyTileType;
  }

  public static void AppendRemixCommands(
    WorldGridSnapshot snapshot,
    int mushroomLayerLow,
    LegacyPassRandomState random,
    bool isRemixWorld,
    bool isSkyblockWorld,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(random);
    ArgumentNullException.ThrowIfNull(commands);
    if (!isRemixWorld || isSkyblockWorld)
    {
      return;
    }

    int minimumY = Math.Max(mushroomLayerLow, 0);
    int maximumYExclusive = snapshot.Metadata.Height - 10;
    if (minimumY >= maximumYExclusive)
    {
      return;
    }

    for (int x = 10; x < snapshot.Metadata.Width - 10; x++)
    {
      int startY = minimumY + random.Next(3);
      if (startY >= maximumYExclusive)
      {
        continue;
      }

      for (int y = startY; y < maximumYExclusive; y++)
      {
        WorldTile tile = snapshot.GetTile(x, y);
        if (!IsEligible(tile))
        {
          continue;
        }

        commands.Add(new TileChangeCommand(
          state.ReserveSequence(),
          x,
          y,
          TileChangeKind.UpdateTileType,
          MushroomGrassType,
          IsActive: tile.IsActive,
          Source: "worldgen.GlowingMushroomPatches"));
      }
    }
  }
}
