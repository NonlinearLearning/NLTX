using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed record LegacyCaveTunnelInvocation(
  double StartX,
  double StartY,
  double XDirection,
  double YDirection,
  int Steps,
  int Size,
  bool IsWet)
{
  public void Validate(WorldGridSnapshot snapshot)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    if (Steps < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(Steps));
    }

    if (Size < 1)
    {
      throw new ArgumentOutOfRangeException(nameof(Size));
    }

    if (snapshot.Metadata.Width <= Size * 2 + 2 ||
        snapshot.Metadata.Height <= Size * 2 + 2)
    {
      throw new ArgumentOutOfRangeException(nameof(snapshot));
    }
  }
}

public readonly record struct LegacyCaveTunnelResult(double EndX, double EndY);

public static class LegacyCaveTunnel
{
  private const byte WaterLiquidType = 0;

  public static LegacyCaveTunnelResult AppendCommands(
    WorldGridSnapshot snapshot,
    LegacyCaveTunnelInvocation invocation,
    LegacyPassRandomState random,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> tileCommands,
    List<LiquidChangeCommand> liquidCommands)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(invocation);
    ArgumentNullException.ThrowIfNull(random);
    ArgumentNullException.ThrowIfNull(tileCommands);
    ArgumentNullException.ThrowIfNull(liquidCommands);
    invocation.Validate(snapshot);
    if (state.Stage < WorldGenerationStage.Cave &&
        !state.TryAdvance(WorldGenerationStage.Cave))
    {
      throw new InvalidOperationException("Cave stage could not be started.");
    }

    double centerX = Math.Clamp(
      invocation.StartX,
      invocation.Size + 1.0,
      snapshot.Metadata.Width - invocation.Size - 1.0);
    double centerY = Math.Clamp(
      invocation.StartY,
      invocation.Size + 1.0,
      snapshot.Metadata.Height - invocation.Size - 1.0);
    double xDrift = 0.0;
    double yDrift = 0.0;
    double radius = invocation.Size;
    for (int step = 0; step < invocation.Steps; step++)
    {
      AppendStepCommands(
        snapshot,
        centerX,
        centerY,
        radius,
        invocation.IsWet,
        random,
        ref state,
        tileCommands,
        liquidCommands);
      radius += random.Next(-50, 51) * 0.03;
      radius = Math.Clamp(radius, invocation.Size * 0.6, invocation.Size * 2.0);
      xDrift += random.Next(-20, 21) * 0.01;
      yDrift += random.Next(-20, 21) * 0.01;
      xDrift = Math.Clamp(xDrift, -1.0, 1.0);
      yDrift = Math.Clamp(yDrift, -1.0, 1.0);
      centerX += (invocation.XDirection + xDrift) * 0.6;
      centerY += (invocation.YDirection + yDrift) * 0.6;
    }

    return new LegacyCaveTunnelResult(centerX, centerY);
  }

  private static void AppendStepCommands(
    WorldGridSnapshot snapshot,
    double centerX,
    double centerY,
    double radius,
    bool isWet,
    LegacyPassRandomState random,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> tileCommands,
    List<LiquidChangeCommand> liquidCommands)
  {
    for (int x = (int)(centerX - radius); x <= centerX + radius; x++)
    {
      for (int y = (int)(centerY - radius); y <= centerY + radius; y++)
      {
        double threshold = radius * (1.0 + random.Next(-10, 11) * 0.005);
        if (Math.Abs(x - centerX) + Math.Abs(y - centerY) >= threshold ||
            x < 0 || x >= snapshot.Metadata.Width || y < 0 || y >= snapshot.Metadata.Height)
        {
          continue;
        }

        tileCommands.Add(new TileChangeCommand(
          state.ReserveSequence(),
          x,
          y,
          TileChangeKind.Kill,
          0,
          Source: "worldgen.cave.digTunnel"));
        if (isWet)
        {
          liquidCommands.Add(new LiquidChangeCommand(
            state.ReserveSequence(),
            x,
            y,
            byte.MaxValue,
            WaterLiquidType,
            Source: "worldgen.cave.digTunnel"));
        }
      }
    }
  }
}
