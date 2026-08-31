using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyDungeonDesertCleanupPass
{
  private const int InitialWidth = 1;
  private const int WorldInset = 5;
  private const int InitialYOffset = 10;
  private const int MinimumRandomDepth = 25;
  private const int MaximumRandomDepth = 46;
  private const int WidthThreshold = 20;
  private const int LargeWidthThreshold = 40;

  public static void AppendCommands(
    WorldGridSnapshot snapshot,
    int dungeonX,
    int worldSurfaceY,
    int buriedEntranceSandDugoutYOffset,
    int lowestCloudY,
    LegacyPassRandomState random,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(random);
    ArgumentNullException.ThrowIfNull(commands);
    if (dungeonX < 0 || dungeonX >= snapshot.Metadata.Width ||
        worldSurfaceY < 0 || buriedEntranceSandDugoutYOffset < 0 ||
        lowestCloudY < 0 || lowestCloudY >= snapshot.Metadata.Height)
    {
      throw new ArgumentOutOfRangeException(nameof(dungeonX));
    }

    int y = worldSurfaceY - InitialYOffset + buriedEntranceSandDugoutYOffset -
      random.Next(MinimumRandomDepth, MaximumRandomDepth);
    int leftWidth = InitialWidth;
    int rightWidth = InitialWidth;
    while (y > lowestCloudY)
    {
      for (int x = dungeonX - leftWidth; x <= dungeonX + rightWidth; x++)
      {
        if (!IsInsideInset(snapshot, x, y))
        {
          continue;
        }

        commands.Add(new TileChangeCommand(
          state.ReserveSequence(),
          x,
          y,
          TileChangeKind.Kill,
          TileType: 0,
          PreserveTileState: true,
          Source: "worldgen.dungeon.desert-cleanup"));
        if (IsInsideInset(snapshot, x, y + 1))
        {
          commands.Add(new TileChangeCommand(
            state.ReserveSequence(),
            x,
            y,
            TileChangeKind.SetWall,
            TileType: 0,
            WallType: 0,
            Source: "worldgen.dungeon.desert-cleanup"));
          commands.Add(new TileChangeCommand(
            state.ReserveSequence(),
            x,
            y + 1,
            TileChangeKind.SetWall,
            TileType: 0,
            WallType: 0,
            Source: "worldgen.dungeon.desert-cleanup"));
        }
      }

      y--;
      leftWidth = AdvanceWidth(leftWidth, random);
      rightWidth = AdvanceWidth(rightWidth, random);
    }
  }

  private static int AdvanceWidth(int width, LegacyPassRandomState random)
  {
    return width >= LargeWidthThreshold
      ? width + random.Next(1, 3)
      : width >= WidthThreshold
      ? width + random.Next(2, 4)
      : width + random.Next(4, 5);
  }

  private static bool IsInsideInset(WorldGridSnapshot snapshot, int x, int y)
  {
    return x >= WorldInset && x < snapshot.Metadata.Width - WorldInset &&
      y >= WorldInset && y < snapshot.Metadata.Height - WorldInset;
  }
}
