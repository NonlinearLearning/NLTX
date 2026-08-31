using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyCaveOpenaterPass
{
  private const int MaximumIterations = 100;

  public static void AppendCommands(
    WorldGridSnapshot snapshot,
    LegacyMountainCaveOpeningRequest request,
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

    if (!snapshot.Metadata.IsInside(request.X, request.Y))
    {
      throw new ArgumentOutOfRangeException(nameof(request));
    }

    double width = random.Next(7, 12);
    int direction = random.Next(2) == 0 ? -1 : 1;
    if (random.Next(10) != 0)
    {
      direction = request.X < snapshot.Metadata.Width / 2 ? 1 : -1;
    }

    double centerX = request.X;
    double centerY = request.Y;
    double velocityX = direction;
    double velocityY = 0.0;
    int remainingIterations = MaximumIterations;
    while (remainingIterations > 0 && snapshot.Metadata.IsInside((int)centerX, (int)centerY))
    {
      WorldTile centerTile = snapshot.GetTile((int)centerX, (int)centerY);
      if (centerTile.WallType == 0 ||
          (centerTile.IsActive && !LegacyGenerationClearabilityPolicy.CanClear(
            centerTile.Type,
            isProtectedDungeonTile)))
      {
        break;
      }

      remainingIterations--;
      double variation = width * random.Next(80, 120) * 0.01;
      AppendCircleCommands(
        snapshot,
        centerX,
        centerY,
        variation,
        isProtectedDungeonTile,
        ref state,
        commands);
      centerX += velocityX;
      centerY += velocityY;
      velocityX += random.Next(-10, 11) * 0.05;
      velocityY += random.Next(-10, 11) * 0.05;
      velocityX = Math.Clamp(velocityX, direction - 0.5, direction + 0.5);
      velocityY = Math.Clamp(velocityY, -0.5, 0.0);
    }
  }

  private static void AppendCircleCommands(
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
        if (!LegacyGenerationClearabilityPolicy.CanClear(tile.Type, isProtectedDungeonTile))
        {
          continue;
        }

        commands.Add(new TileChangeCommand(
          state.ReserveSequence(),
          x,
          y,
          TileChangeKind.Kill,
          TileType: 0,
          Source: "worldgen.mountainCave.openater"));
      }
    }
  }
}
