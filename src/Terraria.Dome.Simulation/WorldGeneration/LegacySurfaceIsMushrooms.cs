using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacySurfaceIsMushrooms
{
  private const ushort ConvertedWallType = 80;
  private const ushort DirtTileType = 0;
  private const ushort GlowingMushroomTileType = 70;
  private const ushort GrassTileType = 2;
  private const ushort MushroomGrassTileType = 60;
  private const ushort MushroomWallType = 15;

  private static readonly IReadOnlySet<ushort> MushroomReplacementWallTypes = new HashSet<ushort>
  {
    63, 64, 65, 66, 68, 205, 206, 207
  };

  public static void AppendNormalModeCommands(
    WorldGridSnapshot snapshot,
    int worldSurfaceY,
    bool skyblockWorld,
    LegacyPassRandomState random,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(random);
    ArgumentNullException.ThrowIfNull(commands);
    if (worldSurfaceY < 0 || worldSurfaceY > snapshot.Metadata.Height)
    {
      throw new ArgumentOutOfRangeException(nameof(worldSurfaceY));
    }

    if (skyblockWorld)
    {
      return;
    }

    for (int x = 0; x < snapshot.Metadata.Width; x++)
    {
      int upperBoundExclusive = Math.Min(snapshot.Metadata.Height, worldSurfaceY + random.Next(3));
      for (int y = 0; y < upperBoundExclusive; y++)
      {
        WorldTile tile = snapshot.GetTile(x, y);
        if (tile.Type == MushroomGrassTileType)
        {
          AppendTileTypeChange(x, y, GlowingMushroomTileType, ref state, commands);
        }

        if (tile.WallType == MushroomWallType ||
            MushroomReplacementWallTypes.Contains(tile.WallType))
        {
          commands.Add(new TileChangeCommand(
            state.ReserveSequence(),
            x,
            y,
            TileChangeKind.SetWall,
            0,
            WallType: ConvertedWallType,
            Source: "worldgen.biome.SurfaceIsMushrooms"));
        }

        if (tile.Type == DirtTileType)
        {
          AppendTileTypeChange(x, y, 59, ref state, commands);
        }

        if (tile.Type == GrassTileType)
        {
          AppendTileTypeChange(x, y, MushroomGrassTileType, ref state, commands);
        }
      }
    }
  }

  private static void AppendTileTypeChange(
    int x,
    int y,
    ushort tileType,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands)
  {
    commands.Add(new TileChangeCommand(
      state.ReserveSequence(),
      x,
      y,
      TileChangeKind.UpdateTileType,
      tileType,
      Source: "worldgen.biome.SurfaceIsMushrooms"));
  }
}
