using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed record LegacyWavyCavererInvocation(
  int StartX,
  int StartY,
  double WaveStrengthScalar,
  double WavePercentScalar,
  int Steps,
  int TileType)
{
  public void Validate()
  {
    if (WaveStrengthScalar < 0.0)
    {
      throw new ArgumentOutOfRangeException(nameof(WaveStrengthScalar));
    }

    if (WavePercentScalar < 0.0)
    {
      throw new ArgumentOutOfRangeException(nameof(WavePercentScalar));
    }

    if (Steps < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(Steps));
    }
  }
}

public static class LegacyWavyCaverer
{
  private const int WorldFluff = 20;

  public static void AppendCommands(
    WorldGridSnapshot snapshot,
    LegacyWavyCavererInvocation invocation,
    LegacyPassRandomState random,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(invocation);
    ArgumentNullException.ThrowIfNull(random);
    ArgumentNullException.ThrowIfNull(commands);
    invocation.Validate();
    if (state.Stage < WorldGenerationStage.Cave &&
        !state.TryAdvance(WorldGenerationStage.Cave))
    {
      throw new InvalidOperationException("Cave stage could not be started.");
    }

    bool movesLeft = invocation.StartX > snapshot.Metadata.Width / 2;
    int minimumSize = 2 + random.Next(2);
    int maximumSize = 15 + random.Next(11);
    int sizeStep = 1 + random.Next(2);
    int taperSteps = (int)Math.Ceiling((double)maximumSize / sizeStep);
    double amplitude = 1.0;
    double frequency = 1.0;
    int verticalDrift = (int)(-1.0 + random.NextDouble() * 3.0);
    int size = minimumSize;
    int interiorStep = 0;
    double centerX = invocation.StartX;

    for (int step = 0; step < invocation.Steps; step++)
    {
      bool isBeginning = step < taperSteps;
      bool isEnding = step >= invocation.Steps - taperSteps;
      centerX += movesLeft ? -1.0 : 1.0;
      if (!isBeginning && !isEnding)
      {
        interiorStep++;
        amplitude = Math.Min(
          2.0,
          Math.Max(0.5, amplitude + (-0.5 + random.NextDouble()) * 0.25));
        frequency = Math.Min(
          1.1,
          Math.Max(0.9, frequency + (-0.5 + random.NextDouble()) * 0.02));
      }

      double wave = Math.Sin(interiorStep * 0.1 * frequency * invocation.WavePercentScalar) *
        amplitude * invocation.WaveStrengthScalar;
      double centerY = invocation.StartY + wave + interiorStep * verticalDrift;
      int previousSize = size;
      if (isBeginning)
      {
        size = Math.Min(maximumSize, size + sizeStep);
      }
      else if (isEnding)
      {
        size = Math.Max(minimumSize, size - sizeStep);
      }

      centerY -= (previousSize + size) / 4;
      AppendColumn(snapshot, invocation.TileType, centerX, centerY, size, ref state, commands);
    }
  }

  private static void AppendColumn(
    WorldGridSnapshot snapshot,
    int tileType,
    double centerX,
    double centerY,
    int size,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands)
  {
    int x = (int)centerX;
    for (int offset = 0; offset < size; offset++)
    {
      int y = (int)centerY + offset;
      if (!IsInWorld(snapshot, x, y))
      {
        continue;
      }

      commands.Add(new TileChangeCommand(
        state.ReserveSequence(),
        x,
        y,
        tileType >= 0 ? TileChangeKind.Place : TileChangeKind.Kill,
        tileType >= 0 ? checked((ushort)tileType) : (ushort)0,
        Source: "worldgen.cave.WavyCaverer"));
    }
  }

  private static bool IsInWorld(WorldGridSnapshot snapshot, int x, int y)
  {
    return x >= WorldFluff && x < snapshot.Metadata.Width - WorldFluff &&
      y >= WorldFluff && y < snapshot.Metadata.Height - WorldFluff;
  }
}
