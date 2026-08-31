using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyUnderworldSurfacePass
{
  private const int AshTileType = 57;
  private const ushort SpiderCaveWallType = 62;
  private const int SurfaceMinimumOffset = 20;

  public static void AppendCommands(
    WorldGridSnapshot snapshot,
    LegacyPassRandomState random,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands,
    bool isNotTheBeesWorld = false,
    bool isSkyblockWorld = false)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(random);
    ArgumentNullException.ThrowIfNull(commands);
    if (isSkyblockWorld)
    {
      return;
    }

    int topY = LegacyUnderworldSurfaceColumnPolicy.CreateInitialTop(
      snapshot.Metadata.Height,
      random);
    for (int x = 0; x < snapshot.Metadata.Width; x++)
    {
      LegacyUnderworldSurfaceColumn column = LegacyUnderworldSurfaceColumnPolicy.Advance(
        topY,
        snapshot.Metadata.Height,
        random,
        isNotTheBeesWorld);
      topY = column.TopY;
      int startY = Math.Max(0, column.EffectiveTopY - SurfaceMinimumOffset);
      for (int y = startY; y < snapshot.Metadata.Height; y++)
      {
        if (isNotTheBeesWorld && y <= column.EffectiveTopY)
        {
          commands.Add(new TileChangeCommand(
            state.ReserveSequence(),
            x,
            y,
            TileChangeKind.SetWall,
            TileType: 0,
            WallType: SpiderCaveWallType,
            Source: "worldgen.underworld.surface-spider-wall"));
        }

        if (y >= column.TopY)
        {
          commands.Add(new TileChangeCommand(
            state.ReserveSequence(),
            x,
            y,
            TileChangeKind.Kill,
            TileType: 0,
            Source: "worldgen.underworld.surface-clear"));
        }
        else
        {
          commands.Add(new TileChangeCommand(
            state.ReserveSequence(),
            x,
            y,
            TileChangeKind.UpdateTileType,
            TileType: AshTileType,
            IsActive: true,
            Source: "worldgen.underworld.surface-ash"));
        }
      }
    }
  }
}
