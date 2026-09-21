using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class RandomRectanglePointPolicy
{
  public static RandomRectanglePointResult Next(
    GenerationRandomState state,
    int x,
    int y,
    int width,
    int height)
  {
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);
    (GenerationRandomState nextXState, int offsetX) = state.NextExclusive(width);
    (GenerationRandomState nextYState, int offsetY) = nextXState.NextExclusive(height);
    return new RandomRectanglePointResult(nextYState, x + offsetX, y + offsetY);
  }
}
