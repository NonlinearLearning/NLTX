using System;

namespace Terraria.WorldGeneration.Passes;

public readonly record struct WorldSizeProfile
{
  public WorldSizeProfile(int width, int height)
  {
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);

    Width = width;
    Height = height;
  }

  public int Width { get; }

  public int Height { get; }
}
