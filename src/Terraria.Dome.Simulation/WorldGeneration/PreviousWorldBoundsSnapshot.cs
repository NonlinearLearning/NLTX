using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct PreviousWorldBoundsSnapshot
{
  public PreviousWorldBoundsSnapshot(int width, int height)
  {
    ArgumentOutOfRangeException.ThrowIfNegative(width);
    ArgumentOutOfRangeException.ThrowIfNegative(height);
    Width = width;
    Height = height;
  }

  public int Width { get; }

  public int Height { get; }
}
