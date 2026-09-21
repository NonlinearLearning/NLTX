using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyMountinater
{
  private const int MaximumInitialSize = 120;
  private const int MinimumInitialSize = 80;
  private const int MaximumInitialHeight = 55;
  private const int MinimumInitialHeight = 40;
  private const double MaximumHorizontalVelocity = 0.5;
  private const double MinimumHorizontalVelocity = -0.5;
  private const double MaximumVerticalVelocity = -0.5;
  private const double MinimumVerticalVelocity = -1.5;
  private const int MaximumVelocityAdjustment = 10;
  private const int MinimumVelocityAdjustment = -10;
  private const int MaximumSizeScale = 120;
  private const int MinimumSizeScale = 80;
  private const int MaximumHorizontalDirection = 10;
  private const int MinimumHorizontalDirection = -10;
  private const int MaximumVerticalDirection = -10;
  private const int MinimumVerticalDirection = -20;
  private const int MaximumSizeDecrement = 4;

  public static void AppendCommands(
    WorldGridSnapshot snapshot,
    int startX,
    int startY,
    LegacyPassRandomState random,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(random);
    ArgumentNullException.ThrowIfNull(commands);
    if (state.Stage < WorldGenerationStage.Cave &&
        !state.TryAdvance(WorldGenerationStage.Cave))
    {
      throw new InvalidOperationException("Cave stage could not be started.");
    }

    double size = random.Next(MinimumInitialSize, MaximumInitialSize);
    double height = random.Next(MinimumInitialHeight, MaximumInitialHeight);
    double centerX = startX;
    double centerY = startY + height / 2.0;
    double velocityX = random.Next(MinimumHorizontalDirection, MaximumHorizontalDirection + 1) * 0.1;
    double velocityY = random.Next(MinimumVerticalDirection, MaximumVerticalDirection) * 0.1;
    HashSet<(int X, int Y)> placed = new();

    while (size > 0.0 && height > 0.0)
    {
      size -= random.Next(MaximumSizeDecrement);
      height--;
      int minimumX = Math.Max(0, (int)(centerX - size * 0.5));
      int maximumXExclusive = Math.Min(snapshot.Metadata.Width, (int)(centerX + size * 0.5));
      int minimumY = Math.Max(0, (int)(centerY - size * 0.5));
      int maximumYExclusive = Math.Min(snapshot.Metadata.Height, (int)(centerY + size * 0.5));
      double scaledSize = size * random.Next(MinimumSizeScale, MaximumSizeScale) * 0.01;
      AppendEmptyTileCommands(
        snapshot,
        centerX,
        centerY,
        scaledSize,
        minimumX,
        maximumXExclusive,
        minimumY,
        maximumYExclusive,
        placed,
        ref state,
        commands);

      centerX += velocityX;
      centerY += velocityY;
      velocityX += random.Next(MinimumVelocityAdjustment, MaximumVelocityAdjustment + 1) * 0.05;
      velocityY += random.Next(MinimumVelocityAdjustment, MaximumVelocityAdjustment + 1) * 0.05;
      velocityX = Math.Clamp(velocityX, MinimumHorizontalVelocity, MaximumHorizontalVelocity);
      velocityY = Math.Clamp(velocityY, MinimumVerticalVelocity, MaximumVerticalVelocity);
    }
  }

  private static void AppendEmptyTileCommands(
    WorldGridSnapshot snapshot,
    double centerX,
    double centerY,
    double scaledSize,
    int minimumX,
    int maximumXExclusive,
    int minimumY,
    int maximumYExclusive,
    HashSet<(int X, int Y)> placed,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands)
  {
    for (int x = minimumX; x < maximumXExclusive; x++)
    {
      for (int y = minimumY; y < maximumYExclusive; y++)
      {
        double deltaX = Math.Abs(x - centerX);
        double deltaY = Math.Abs(y - centerY);
        if (Math.Sqrt(deltaX * deltaX + deltaY * deltaY) >= scaledSize * 0.4 ||
            placed.Contains((x, y)) || snapshot.GetTile(x, y).IsActive)
        {
          continue;
        }

        placed.Add((x, y));
        commands.Add(new TileChangeCommand(
          state.ReserveSequence(),
          x,
          y,
          TileChangeKind.Place,
          0,
          Source: "worldgen.cave.Mountinater"));
      }
    }
  }
}
