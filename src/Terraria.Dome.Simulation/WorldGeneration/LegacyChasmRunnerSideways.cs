using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed record LegacyChasmRunnerSidewaysInvocation(
  int StartX,
  int StartY,
  int Direction,
  int Steps,
  int RockLayerY,
  ushort EvilTileType,
  ushort EvilWallType,
  bool NoSurface)
{
  public void Validate()
  {
    if (Direction is not (-1 or 1))
    {
      throw new ArgumentOutOfRangeException(nameof(Direction));
    }

    if (Steps < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(Steps));
    }
  }
}

public static class LegacyChasmRunnerSideways
{
  private const int MaximumRadius = 20;
  private const int MinimumRadius = 7;
  private const int ProtectedCloudTileType = 204;
  private const int ProtectedDungeonBrickTileType = 31;
  private const int ProtectedGrassTileType = 22;

  public static void AppendCommands(
    WorldGridSnapshot snapshot,
    LegacyChasmRunnerSidewaysInvocation invocation,
    LegacyEvilReplacementDefinitions definitions,
    LegacyPassRandomState random,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(invocation);
    ArgumentNullException.ThrowIfNull(definitions);
    ArgumentNullException.ThrowIfNull(random);
    ArgumentNullException.ThrowIfNull(commands);
    invocation.Validate();
    double remainingSteps = invocation.Steps;
    double centerX = invocation.StartX;
    double centerY = invocation.StartY;
    double directionX = random.Next(10, 21) * 0.1 * invocation.Direction;
    double directionY = random.Next(-10, 10) * 0.01;
    double radius = random.Next(5) + MinimumRadius;

    while (radius > 0.0)
    {
      if (remainingSteps > 0.0)
      {
        radius += random.Next(3);
        radius -= random.Next(3);
        radius = Math.Clamp(radius, MinimumRadius, MaximumRadius);
        if (remainingSteps == 1.0 && radius < 10.0)
        {
          radius = 10.0;
        }
      }
      else
      {
        radius -= random.Next(4);
      }

      if (centerY > invocation.RockLayerY && remainingSteps > 0.0 && !invocation.NoSurface)
      {
        remainingSteps = 0.0;
      }

      remainingSteps--;
      AppendCarveCommands(snapshot, centerX, centerY, radius, definitions, random, ref state, commands);
      centerX += directionX;
      centerY += directionY;
      directionY += random.Next(-10, 10) * 0.1;
      if (centerY < invocation.StartY - 20)
      {
        directionY += random.Next(20) * 0.01;
      }

      if (centerY > invocation.StartY + 20)
      {
        directionY -= random.Next(20) * 0.01;
      }

      directionY = Math.Clamp(directionY, -0.5, 0.5);
      directionX += random.Next(-10, 11) * 0.01;
      directionX = invocation.Direction < 0
        ? Math.Clamp(directionX, -2.0, -0.5)
        : Math.Clamp(directionX, 0.5, 2.0);
      AppendEvilCommands(
        snapshot,
        invocation,
        centerX,
        centerY,
        radius,
        definitions,
        random,
        ref state,
        commands);
    }
  }

  private static void AppendCarveCommands(
    WorldGridSnapshot snapshot,
    double centerX,
    double centerY,
    double radius,
    LegacyEvilReplacementDefinitions definitions,
    LegacyPassRandomState random,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands)
  {
    GetBounds(snapshot, centerX, centerY, radius * 0.5, false, out int minimumX,
      out int maximumXExclusive, out int minimumY, out int maximumYExclusive);
    for (int x = minimumX; x < maximumXExclusive; x++)
    {
      for (int y = minimumY; y < maximumYExclusive; y++)
      {
        WorldTile tile = snapshot.GetTile(x, y);
        if (IsProtectedTileType(tile.Type) ||
            !LegacyEvilReplacementQuery.CanReplace(tile, definitions) ||
            !IsInsideManhattanRadius(x, y, centerX, centerY,
              radius * 0.5 * (1.0 + random.Next(-10, 11) * 0.015)))
        {
          continue;
        }

        commands.Add(new TileChangeCommand(
          state.ReserveSequence(),
          x,
          y,
          TileChangeKind.Kill,
          0,
          Source: "worldgen.cave.ChasmRunnerSideways"));
      }
    }
  }

  private static void AppendEvilCommands(
    WorldGridSnapshot snapshot,
    LegacyChasmRunnerSidewaysInvocation invocation,
    double centerX,
    double centerY,
    double radius,
    LegacyEvilReplacementDefinitions definitions,
    LegacyPassRandomState random,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands)
  {
    GetBounds(snapshot, centerX, centerY, radius * 1.1, true, out int minimumX,
      out int maximumXExclusive, out int minimumY, out int maximumYExclusive);
    for (int x = minimumX; x < maximumXExclusive; x++)
    {
      for (int y = minimumY; y < maximumYExclusive; y++)
      {
        WorldTile tile = snapshot.GetTile(x, y);
        if (!LegacyEvilReplacementQuery.CanReplace(tile, definitions) ||
            tile.WallType == invocation.EvilWallType ||
            !IsInsideManhattanRadius(x, y, centerX, centerY,
              radius * 1.1 * (1.0 + random.Next(-10, 11) * 0.015)))
        {
          continue;
        }

        if (!tile.IsActive || !IsProtectedTileType(tile.Type))
        {
          commands.Add(new TileChangeCommand(
            state.ReserveSequence(),
            x,
            y,
            TileChangeKind.UpdateTileType,
            invocation.EvilTileType,
            Source: "worldgen.cave.ChasmRunnerSideways",
            IsActive: true));
        }

        commands.Add(new TileChangeCommand(
          state.ReserveSequence(),
          x,
          y,
          TileChangeKind.SetWall,
          0,
          WallType: invocation.EvilWallType,
          Source: "worldgen.cave.ChasmRunnerSideways"));
      }
    }
  }

  private static void GetBounds(
    WorldGridSnapshot snapshot,
    double centerX,
    double centerY,
    double radius,
    bool excludeOuterEdge,
    out int minimumX,
    out int maximumXExclusive,
    out int minimumY,
    out int maximumYExclusive)
  {
    minimumX = Math.Max(excludeOuterEdge ? 1 : 0, (int)(centerX - radius));
    maximumXExclusive = Math.Min(snapshot.Metadata.Width - 1, (int)(centerX + radius));
    minimumY = Math.Max(0, (int)(centerY - radius));
    maximumYExclusive = Math.Min(snapshot.Metadata.Height, (int)(centerY + radius));
  }

  private static bool IsInsideManhattanRadius(
    int x,
    int y,
    double centerX,
    double centerY,
    double radius)
  {
    return Math.Abs(x - centerX) + Math.Abs(y - centerY) < radius;
  }

  private static bool IsProtectedTileType(ushort tileType)
  {
    return tileType is ProtectedDungeonBrickTileType or ProtectedGrassTileType or ProtectedCloudTileType;
  }
}
