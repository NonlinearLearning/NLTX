using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldGeneration.Commands;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldModel.Definitions;

namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed record LegacyChasmRunnerInvocation(
  int StartX,
  int StartY,
  int Steps,
  bool MakeOrb,
  double WorldSurface,
  double RockLayer,
  ushort EvilTileType,
  ushort EvilWallType)
{
  public void Validate()
  {
    if (Steps < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(Steps));
    }
  }
}

public static class LegacyChasmRunner
{
  public static void AppendCommands(
    WorldGridSnapshot snapshot,
    LegacyChasmRunnerInvocation invocation,
    LegacyEvilReplacementDefinitions definitions,
    TileDefinitionRegistry tileDefinitions,
    LegacyPassRandomState random,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands,
    List<StructurePlacementCommand> structures)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(invocation);
    ArgumentNullException.ThrowIfNull(definitions);
    ArgumentNullException.ThrowIfNull(tileDefinitions);
    ArgumentNullException.ThrowIfNull(random);
    ArgumentNullException.ThrowIfNull(commands);
    ArgumentNullException.ThrowIfNull(structures);
    invocation.Validate();

    double centerX = invocation.StartX;
    double centerY = invocation.StartY;
    double directionX = random.Next(-10, 11) * 0.1;
    double directionY = random.Next(11) * 0.2 + 0.5;
    int remainingSteps = invocation.Steps;
    bool sidewaysStarted = false;
    while (remainingSteps >= -1)
    {
      double radius = Math.Clamp(7.0 + random.Next(5), 7.0, 20.0);
      AppendCarveCommands(snapshot, centerX, centerY, radius, definitions, random, ref state, commands);
      if (!sidewaysStarted && centerY > invocation.WorldSurface + 20.0)
      {
        sidewaysStarted = true;
        LegacyChasmRunnerSideways.AppendCommands(
          snapshot,
          new LegacyChasmRunnerSidewaysInvocation(
            (int)centerX,
            (int)centerY,
            -1,
            random.Next(20, 40),
            (int)invocation.RockLayer,
            EvilTileType: 0,
            EvilWallType: 1,
            NoSurface: false),
          definitions,
          random,
          ref state,
          commands);
        LegacyChasmRunnerSideways.AppendCommands(
          snapshot,
          new LegacyChasmRunnerSidewaysInvocation(
            (int)centerX,
            (int)centerY,
            1,
            random.Next(20, 40),
            (int)invocation.RockLayer,
            EvilTileType: 0,
            EvilWallType: 1,
            NoSurface: false),
          definitions,
          random,
          ref state,
          commands);
      }

      if (remainingSteps == 0 && invocation.MakeOrb)
      {
        _ = LegacyShadowOrbPlacement.TryAppendIntent(
          snapshot,
          (int)centerX,
          (int)centerY,
          crimsonHeart: false,
          ref state,
          structures);
      }
      else if (remainingSteps == -1 && invocation.MakeOrb)
      {
        TryAppendTerminalPot(
          snapshot,
          invocation,
          tileDefinitions,
          centerX,
          centerY,
          random,
          ref state,
          commands);
      }

      centerX += directionX;
      centerY += directionY;
      directionX = Math.Clamp(directionX + random.Next(-10, 11) * 0.01, -0.3, 0.3);
      AppendEvilConversionCommands(
        snapshot,
        invocation,
        centerX,
        centerY,
        radius,
        definitions,
        random,
        ref state,
        commands);
      remainingSteps--;
    }
  }

  private static void AppendEvilConversionCommands(
    WorldGridSnapshot snapshot,
    LegacyChasmRunnerInvocation invocation,
    double centerX,
    double centerY,
    double radius,
    LegacyEvilReplacementDefinitions definitions,
    LegacyPassRandomState random,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands)
  {
    int minimumX = Math.Max(0, (int)(centerX - radius * 1.1));
    int maximumXExclusive = Math.Min(snapshot.Metadata.Width, (int)(centerX + radius * 1.1));
    int minimumY = Math.Max(0, (int)(centerY - radius * 1.1));
    int maximumYExclusive = Math.Min(snapshot.Metadata.Height, (int)(centerY + radius * 1.1));
    for (int x = minimumX; x < maximumXExclusive; x++)
    {
      for (int y = minimumY; y < maximumYExclusive; y++)
      {
        WorldTile tile = snapshot.GetTile(x, y);
        if (!IsInsideEvilConversionRadius(x, y, centerX, centerY, radius, random) ||
            !LegacyEvilReplacementQuery.CanReplace(tile, definitions))
        {
          continue;
        }

        bool mustActivate = (tile.Type != invocation.EvilTileType &&
          y > invocation.StartY + random.Next(3, 20)) || invocation.Steps <= 5;
        if (tile.Type != 31)
        {
          commands.Add(new TileChangeCommand(
            state.ReserveSequence(),
            x,
            y,
            TileChangeKind.UpdateTileType,
            invocation.EvilTileType,
            Source: "worldgen.cave.ChasmRunner",
            IsActive: mustActivate ? true : tile.IsActive));
        }
      }
    }

    for (int x = minimumX; x < maximumXExclusive; x++)
    {
      for (int y = minimumY; y < maximumYExclusive; y++)
      {
        WorldTile tile = snapshot.GetTile(x, y);
        if (!IsInsideEvilConversionRadius(x, y, centerX, centerY, radius, random) ||
            !LegacyEvilReplacementQuery.CanReplace(tile, definitions))
        {
          continue;
        }

        if (tile.Type != 31)
        {
          commands.Add(new TileChangeCommand(
            state.ReserveSequence(),
            x,
            y,
            TileChangeKind.UpdateTileType,
            invocation.EvilTileType,
            Source: "worldgen.cave.ChasmRunner",
            IsActive: invocation.Steps <= 5 ? true : tile.IsActive));
        }

        if (y > invocation.StartY + random.Next(3, 20))
        {
          commands.Add(new TileChangeCommand(
            state.ReserveSequence(),
            x,
            y,
            TileChangeKind.SetWall,
            0,
            WallType: invocation.EvilWallType,
            Source: "worldgen.cave.ChasmRunner"));
        }
      }
    }
  }

  private static void TryAppendTerminalPot(
    WorldGridSnapshot snapshot,
    LegacyChasmRunnerInvocation invocation,
    TileDefinitionRegistry tileDefinitions,
    double centerX,
    double centerY,
    LegacyPassRandomState random,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands)
  {
    const int MaximumAttempts = 10000;
    for (int attempt = 0; attempt < MaximumAttempts; attempt++)
    {
      int x = Math.Clamp(random.Next((int)centerX - 25, (int)centerX + 25), 5,
        snapshot.Metadata.Width - 5);
      int y = Math.Clamp(random.Next((int)centerY - 50, (int)centerY), 5,
        snapshot.Metadata.Height - 5);
      if (y <= invocation.WorldSurface)
      {
        return;
      }

      if (LegacyPlace3x2.TryAppendCommands(
            snapshot,
            tileDefinitions,
            x,
            y,
            tileType: 26,
            style: 0,
            ref state,
            commands))
      {
        return;
      }
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
    int minimumX = Math.Max(0, (int)(centerX - radius * 0.5));
    int maximumXExclusive = Math.Min(snapshot.Metadata.Width, (int)(centerX + radius * 0.5));
    int minimumY = Math.Max(0, (int)(centerY - radius * 0.5));
    int maximumYExclusive = Math.Min(snapshot.Metadata.Height, (int)(centerY + radius * 0.5));
    for (int x = minimumX; x < maximumXExclusive; x++)
    {
      for (int y = minimumY; y < maximumYExclusive; y++)
      {
        WorldTile tile = snapshot.GetTile(x, y);
        double threshold = radius * 0.5 * (1.0 + random.Next(-10, 11) * 0.015);
        if (!LegacyEvilReplacementQuery.CanReplace(tile, definitions) ||
            Math.Abs(x - centerX) + Math.Abs(y - centerY) >= threshold)
        {
          continue;
        }

        commands.Add(new TileChangeCommand(
          state.ReserveSequence(),
          x,
          y,
          TileChangeKind.Kill,
          0,
          Source: "worldgen.cave.ChasmRunner"));
      }
    }
  }

  private static bool IsInsideEvilConversionRadius(
    int x,
    int y,
    double centerX,
    double centerY,
    double radius,
    LegacyPassRandomState random)
  {
    double threshold = radius * 1.1 * (1.0 + random.Next(-10, 11) * 0.015);
    return Math.Abs(x - centerX) + Math.Abs(y - centerY) < threshold;
  }
}
