using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyCaverer
{
  private static readonly LegacyTileRunnerPassInput TerminalTunnelRecipe = new(
    "Caverer",
    "terminal-tile-runner",
    -1,
    false,
    10,
    20,
    5,
    10,
    "caverer-terminal",
    false,
    false,
    true);

  public static void AppendCommands(
    WorldGridSnapshot snapshot,
    int startX,
    int startY,
    LegacyPassRandomState random,
    int worldSurfaceY,
    int rockLayerY,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> tileCommands,
    List<LiquidChangeCommand> liquidCommands,
    IDictionary<(int X, int Y), WorldTile>? projectedTiles = null)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(random);
    ArgumentNullException.ThrowIfNull(tileCommands);
    ArgumentNullException.ThrowIfNull(liquidCommands);
    projectedTiles ??= new Dictionary<(int X, int Y), WorldTile>();
    if (startX < 0 || startX >= snapshot.Metadata.Width ||
        startY < 0 || startY >= snapshot.Metadata.Height)
    {
      throw new ArgumentOutOfRangeException(nameof(startX));
    }

    if (random.Next(2) == 0)
    {
      AppendDryCaves(
        snapshot,
        startX,
        startY,
        random,
        worldSurfaceY,
        rockLayerY,
        ref state,
        tileCommands,
        liquidCommands,
        projectedTiles);
      return;
    }

    AppendWetCaves(
      snapshot,
      startX,
      startY,
      random,
      ref state,
      tileCommands,
      liquidCommands);
  }

  private static void AppendDryCaves(
    WorldGridSnapshot snapshot,
    int startX,
    int startY,
    LegacyPassRandomState random,
    int worldSurfaceY,
    int rockLayerY,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> tileCommands,
    List<LiquidChangeCommand> liquidCommands,
    IDictionary<(int X, int Y), WorldTile> projectedTiles)
  {
    int tunnelCount = random.Next(7, 9);
    (double xDirection, double yDirection) = CreateDirection(random);
    LegacyCaveTunnelResult tunnelEnd = new(startX, startY);
    for (int index = 0; index < tunnelCount; index++)
    {
      tunnelEnd = LegacyCaveTunnel.AppendCommands(
        snapshot,
        new LegacyCaveTunnelInvocation(
          tunnelEnd.EndX,
          tunnelEnd.EndY,
          xDirection,
          yDirection,
          random.Next(6, 20),
          random.Next(4, 9),
          false),
        random,
        ref state,
        tileCommands,
        liquidCommands);
      xDirection = Math.Clamp(xDirection + random.Next(-20, 21) * 0.1, -1.5, 1.5);
      yDirection = Math.Clamp(yDirection + random.Next(-20, 21) * 0.1, -1.5, 1.5);
      (double branchXDirection, double branchYDirection) = CreateDirection(random);
      LegacyCaveTunnelResult branchEnd = LegacyCaveTunnel.AppendCommands(
        snapshot,
        new LegacyCaveTunnelInvocation(
          tunnelEnd.EndX,
          tunnelEnd.EndY,
          branchXDirection,
          branchYDirection,
          random.Next(30, 50),
          random.Next(3, 6),
          false),
        random,
        ref state,
        tileCommands,
        liquidCommands);
      AppendTerminalTileRunner(
        snapshot,
        branchEnd,
        random,
        worldSurfaceY,
        rockLayerY,
        ref state,
        tileCommands,
        projectedTiles);
    }
  }

  private static void AppendWetCaves(
    WorldGridSnapshot snapshot,
    int startX,
    int startY,
    LegacyPassRandomState random,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> tileCommands,
    List<LiquidChangeCommand> liquidCommands)
  {
    int tunnelCount = random.Next(15, 30);
    (double xDirection, double yDirection) = CreateDirection(random);
    LegacyCaveTunnelResult tunnelEnd = new(startX, startY);
    for (int index = 0; index < tunnelCount; index++)
    {
      tunnelEnd = LegacyCaveTunnel.AppendCommands(
        snapshot,
        new LegacyCaveTunnelInvocation(
          tunnelEnd.EndX,
          tunnelEnd.EndY,
          xDirection,
          yDirection,
          random.Next(5, 15),
          random.Next(2, 6),
          true),
        random,
        ref state,
        tileCommands,
        liquidCommands);
      xDirection = Math.Clamp(xDirection + random.Next(-20, 21) * 0.1, -1.5, 1.5);
      yDirection = Math.Clamp(yDirection + random.Next(-20, 21) * 0.1, -1.5, 1.5);
    }
  }

  private static (double XDirection, double YDirection) CreateDirection(
    LegacyPassRandomState random)
  {
    double xDirection = random.Next(100) * 0.01;
    double yDirection = 1.0 - xDirection;
    if (random.Next(2) == 0)
    {
      xDirection = -xDirection;
    }

    if (random.Next(2) == 0)
    {
      yDirection = -yDirection;
    }

    return (xDirection, yDirection);
  }

  private static void AppendTerminalTileRunner(
    WorldGridSnapshot snapshot,
    LegacyCaveTunnelResult tunnelEnd,
    LegacyPassRandomState random,
    int worldSurfaceY,
    int rockLayerY,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> tileCommands,
    IDictionary<(int X, int Y), WorldTile> projectedTiles)
  {
    int x = Math.Clamp((int)tunnelEnd.EndX, 0, snapshot.Metadata.Width - 1);
    int y = Math.Clamp((int)tunnelEnd.EndY, 0, snapshot.Metadata.Height - 1);
    LegacyTileRunnerRequest request = new(
      x,
      y,
      random.Next(10, 20),
      random.Next(5, 10),
      -1,
      false,
      0.0,
      0.0,
      false,
      true,
      -1);
    LegacyTileRunnerTraversal.AppendCommands(
      snapshot,
      new LegacyTileRunnerPassInvocation(
        TerminalTunnelRecipe,
        request,
        x,
        y,
        (int)request.Strength,
        request.Steps,
        0),
      random,
      worldSurfaceY,
      rockLayerY,
      ref state,
      tileCommands,
      projectedTiles: projectedTiles);
  }
}
