using System;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class RandomWorldPointPolicy
{
  public static RandomRectanglePointResult Next(
    GenerationRandomState state,
    WorldMetadata metadata,
    int top = 0,
    int right = 0,
    int bottom = 0,
    int left = 0)
  {
    ArgumentNullException.ThrowIfNull(metadata);
    ArgumentOutOfRangeException.ThrowIfNegative(top);
    ArgumentOutOfRangeException.ThrowIfNegative(right);
    ArgumentOutOfRangeException.ThrowIfNegative(bottom);
    ArgumentOutOfRangeException.ThrowIfNegative(left);
    return RandomRectanglePointPolicy.Next(
      state,
      left,
      top,
      metadata.Width - left - right,
      metadata.Height - top - bottom);
  }
}
