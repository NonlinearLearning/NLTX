using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct DungeonBoundsSnapshot
{
  private DungeonBoundsSnapshot(int left, int top, int right, int bottom)
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

  public int X => Left;

  public int Y => Top;

  public int Width => Right - Left;

  public int Height => Bottom - Top;

  public int Size => Math.Max(Width, Height);

  public int CenterX => (Left + Right) / 2;

  public int CenterY => (Top + Bottom) / 2;

  public static DungeonBoundsSnapshot Create(
    int left,
    int top,
    int right,
    int bottom,
    int worldWidth,
    int worldHeight)
  {
    ValidateWorldDimension(worldWidth, nameof(worldWidth));
    ValidateWorldDimension(worldHeight, nameof(worldHeight));
    int clampedLeft = ClampToWorld(left, worldWidth);
    int clampedRight = ClampToWorld(right, worldWidth);
    int clampedTop = ClampToWorld(top, worldHeight);
    int clampedBottom = ClampToWorld(bottom, worldHeight);
    if (clampedRight <= clampedLeft)
    {
      clampedRight = Math.Min(worldWidth - 10, clampedLeft + 1);
    }

    if (clampedBottom <= clampedTop)
    {
      clampedBottom = Math.Min(worldHeight - 10, clampedTop + 1);
    }

    return new DungeonBoundsSnapshot(clampedLeft, clampedTop, clampedRight, clampedBottom);
  }

  public DungeonBoundsSnapshot Inflate(int amount, int worldWidth, int worldHeight)
  {
    return Create(
      Left - amount,
      Top - amount,
      Right + amount,
      Bottom + amount,
      worldWidth,
      worldHeight);
  }

  public DungeonBoundsSnapshot Shrink(int amount, int worldWidth, int worldHeight)
  {
    return Create(
      Left + amount,
      Top + amount,
      Right - amount,
      Bottom - amount,
      worldWidth,
      worldHeight);
  }

  public bool Contains(int x, int y)
  {
    return x >= Left && x < Right && y >= Top && y < Bottom;
  }

  public bool ContainsWithFluff(int x, int y, int fluff)
  {
    if (fluff < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(fluff));
    }

    return x >= Left - fluff && x < Right + fluff &&
      y >= Top - fluff && y < Bottom + fluff;
  }

  public bool Intersects(DungeonBoundsSnapshot other)
  {
    return Left < other.Right && Right > other.Left &&
      Top < other.Bottom && Bottom > other.Top;
  }

  public bool IntersectsLineThreePointCheck(
    int startX,
    int startY,
    int endX,
    int endY)
  {
    return Contains(startX, startY) || Contains(endX, endY) ||
      Contains((startX + endX) / 2, (startY + endY) / 2);
  }

  private static int ClampToWorld(int value, int worldDimension)
  {
    return Math.Clamp(value, 10, worldDimension - 10);
  }

  private static void ValidateWorldDimension(int value, string parameterName)
  {
    if (value <= 20)
    {
      throw new ArgumentOutOfRangeException(parameterName);
    }
  }
}
