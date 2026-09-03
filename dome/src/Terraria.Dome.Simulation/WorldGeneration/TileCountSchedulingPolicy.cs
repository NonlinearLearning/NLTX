using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class TileCountSchedulingPolicy
{
  public const int UpdatesPerColumnScan = 30;

  public static TileCountSchedulingResult Advance(
    TileCountSchedulingState state,
    int worldWidth)
  {
    if (worldWidth <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(worldWidth));
    }

    if (state.UpdateCounter < 0 || state.UpdateCounter >= UpdatesPerColumnScan ||
        state.ColumnX < 0 || state.ColumnX >= worldWidth)
    {
      throw new ArgumentOutOfRangeException(nameof(state));
    }

    int nextCounter = state.UpdateCounter + 1;
    if (nextCounter < UpdatesPerColumnScan)
    {
      TileCountSchedulingState nextState = new(nextCounter, state.ColumnX);
      return new TileCountSchedulingResult(nextState, false, state.ColumnX);
    }

    int nextColumn = state.ColumnX + 1;
    if (nextColumn >= worldWidth)
    {
      nextColumn = 0;
    }

    TileCountSchedulingState scanState = new(0, nextColumn);
    return new TileCountSchedulingResult(scanState, true, state.ColumnX);
  }
}
