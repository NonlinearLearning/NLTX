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
  public long PixelWidth => (long)Width * 16;
  public long PixelHeight => (long)Height * 16;
  public int SectionColumnCount => Width / SectionWidth;
  public int SectionRowCount => Height / SectionHeight;

  public bool Contains(int x, int y)
  {
    return x >= 0 && x < Width && y >= 0 && y < Height;
  }

  public bool Contains(int x, int y, int fluff)
  {
    ArgumentOutOfRangeException.ThrowIfNegative(fluff);
    return x >= fluff && x < Width - fluff && y >= fluff && y < Height - fluff;
  }

  public bool ContainsRectangle(int x, int y, int width, int height, int fluff = 0)
  {
    ArgumentOutOfRangeException.ThrowIfNegative(height);
    ArgumentOutOfRangeException.ThrowIfNegative(width);
    ArgumentOutOfRangeException.ThrowIfNegative(fluff);
    long right = (long)x + width;
    long bottom = (long)y + height;
    return x >= fluff && right < Width - fluff && y >= fluff && bottom < Height - fluff;
  }
}
