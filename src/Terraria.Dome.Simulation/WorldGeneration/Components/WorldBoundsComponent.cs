using System;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct WorldBoundsComponent
{
  public WorldBoundsComponent(int width, int height)
  {
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);
    if (width % WorldGrid.SectionWidth != 0 || height % WorldGrid.SectionHeight != 0)
    {
      throw new ArgumentException("World dimensions must be whole section units.");
    }

    Width = width;
    Height = height;
    SectionWidth = WorldGrid.SectionWidth;
    SectionHeight = WorldGrid.SectionHeight;
  }

  public int Width { get; }
  public int Height { get; }
  public int SectionWidth { get; }
  public int SectionHeight { get; }

  public bool Contains(int x, int y)
  {
    return x >= 0 && x < Width && y >= 0 && y < Height;
  }
}
