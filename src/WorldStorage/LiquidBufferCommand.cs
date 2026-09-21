using System;

namespace Terraria.WorldStorage;

public readonly record struct LiquidBufferCommand
{
  private LiquidBufferCommand(
    LiquidBufferCommandKind kind,
    TileCoordinate coordinate,
    bool hasCoordinate)
  {
    Kind = kind;
    Coordinate = coordinate;
    HasCoordinate = hasCoordinate;
  }

  public LiquidBufferCommandKind Kind { get; }

  public TileCoordinate Coordinate { get; }

  public int X => Coordinate.X;

  public int Y => Coordinate.Y;

  public bool HasCoordinate { get; }

  public static LiquidBufferCommand Enqueue(int x, int y)
  {
    ValidateCoordinate(x, y);
    return new LiquidBufferCommand(
      LiquidBufferCommandKind.Enqueue,
      new TileCoordinate(x, y),
      hasCoordinate: true);
  }

  public static LiquidBufferCommand Release(int x, int y)
  {
    ValidateCoordinate(x, y);
    return new LiquidBufferCommand(
      LiquidBufferCommandKind.Release,
      new TileCoordinate(x, y),
      hasCoordinate: true);
  }

  public static LiquidBufferCommand Reset()
  {
    return new LiquidBufferCommand(
      LiquidBufferCommandKind.Reset,
      default,
      hasCoordinate: false);
  }

  private static void ValidateCoordinate(int x, int y)
  {
    if (x < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(x));
    }

    if (y < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(y));
    }
  }
}
