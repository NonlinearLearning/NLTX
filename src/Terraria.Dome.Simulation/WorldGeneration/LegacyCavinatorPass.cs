using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyCavinatorPass
{
  public static void AppendCommands(
    WorldGridSnapshot snapshot,
    LegacyMountainCaveOpeningRequest request,
    int rockLayerY,
    LegacyPassRandomState random,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands,
    bool isSkyblockWorld = false,
    bool isProtectedDungeonTile = false)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(random);
    ArgumentNullException.ThrowIfNull(commands);
    if (isSkyblockWorld)
    {
      return;
    }

    if (!snapshot.Metadata.IsInside(request.X, request.Y) || rockLayerY < 0 ||
        rockLayerY >= snapshot.Metadata.Height)
    {
      throw new ArgumentOutOfRangeException(nameof(request));
    }

    AppendRecursive(
      snapshot,
      request.X,
      request.Y,
      request.Radius,
      rockLayerY,
      random,
      ref state,
      commands,
      isProtectedDungeonTile);
  }

  private static void AppendRecursive(
    WorldGridSnapshot snapshot,
    int originX,
    int originY,
    int remainingRecursions,
    int rockLayerY,
    LegacyPassRandomState random,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands,
    bool isProtectedDungeonTile)
  {
    double width = random.Next(7, 15);
    int direction = random.Next(2) == 0 ? -1 : 1;
    double centerX = originX;
    double centerY = originY;
    double velocityX = direction;
    double velocityY = random.Next(10, 20) * 0.01;
    int remainingSteps = random.Next(20, 40);
    while (remainingSteps > 0 && snapshot.Metadata.IsInside((int)centerX, (int)centerY))
    {
      remainingSteps--;
      double variation = width * random.Next(80, 120) * 0.01;
      if (!AppendCircleCommands(
            snapshot,
            centerX,
            centerY,
            variation,
            isProtectedDungeonTile,
            ref state,
            commands))
      {
        break;
      }

      centerX += velocityX;
      centerY += velocityY;
      velocityX += random.Next(-10, 11) * 0.05;
      velocityY += random.Next(-10, 11) * 0.05;
      velocityX = Math.Clamp(velocityX, direction - 0.5, direction + 0.5);
      velocityY = Math.Clamp(velocityY, 0.0, 2.0);
    }

    if (remainingRecursions > 0 && (int)centerY < rockLayerY + 50 &&
        snapshot.Metadata.IsInside((int)centerX, (int)centerY))
    {
      AppendRecursive(
        snapshot,
        (int)centerX,
        (int)centerY,
        remainingRecursions - 1,
        rockLayerY,
        random,
        ref state,
        commands,
        isProtectedDungeonTile);
    }
  }

  private static bool AppendCircleCommands(
    WorldGridSnapshot snapshot,
    double centerX,
    double centerY,
    double width,
    bool isProtectedDungeonTile,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands)
  {
    int minimumX = Math.Max(0, (int)(centerX - width * 0.5));
    int maximumXExclusive = Math.Min(snapshot.Metadata.Width, (int)(centerX + width * 0.5));
    int minimumY = Math.Max(0, (int)(centerY - width * 0.5));
    int maximumYExclusive = Math.Min(snapshot.Metadata.Height, (int)(centerY + width * 0.5));
    for (int x = minimumX; x < maximumXExclusive; x++)
    {
      for (int y = minimumY; y < maximumYExclusive; y++)
      {
        double deltaX = Math.Abs(x - centerX);
        double deltaY = Math.Abs(y - centerY);
        if (Math.Sqrt(deltaX * deltaX + deltaY * deltaY) >= width * 0.4)
        {
          continue;
        }

        WorldTile tile = snapshot.GetTile(x, y);
        if (isProtectedDungeonTile || !tile.IsActive ||
            !LegacyGenerationClearabilityPolicy.CanClear(tile.Type, false))
        {
          continue;
        }

        commands.Add(new TileChangeCommand(
          state.ReserveSequence(),
          x,
          y,
          TileChangeKind.Kill,
          TileType: 0,
          Source: "worldgen.mountainCave.cavinator"));
      }
    }

    return !isProtectedDungeonTile;
  }
}
