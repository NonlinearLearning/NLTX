using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly struct DungeonFeatureArea
{
  public DungeonFeatureArea(int left, int top, int right, int bottom)
  {
    Left = left;
    Top = top;
    Right = right;
    Bottom = bottom;
  }

  public int Left { get; }
  public int Top { get; }
  public int Right { get; }
  public int Bottom { get; }

  public bool HasHitbox => Left <= Right && Top <= Bottom;

  public int Width => HasHitbox ? checked(Right - Left + 1) : 0;

  public int Height => HasHitbox ? checked(Bottom - Top + 1) : 0;

  public int CellCount => checked(Width * Height);

  public bool Contains(int x, int y)
  {
    return HasHitbox && x >= Left && x <= Right && y >= Top && y <= Bottom;
  }

  public static DungeonFeatureArea FromCenterAndFluff(int x, int y, int fluff)
  {
    if (fluff < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(fluff));
    }

    return new DungeonFeatureArea(
      checked(x - fluff),
      checked(y - fluff),
      checked(x + fluff),
      checked(y + fluff));
  }
}
