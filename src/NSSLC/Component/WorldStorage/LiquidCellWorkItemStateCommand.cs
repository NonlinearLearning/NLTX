using System;

namespace Terraria.WorldStorage;

public readonly record struct LiquidCellWorkItemStateCommand
{
  public LiquidCellWorkItemStateCommand(int x, int y, int kill, int delay)
  {
    ValidateNonNegative(x, nameof(x));
    ValidateNonNegative(y, nameof(y));
    ValidateNonNegative(kill, nameof(kill));
    ValidateNonNegative(delay, nameof(delay));

    X = x;
    Y = y;
    Kill = kill;
    Delay = delay;
  }

  public int X { get; }

  public int Y { get; }

  public int Kill { get; }

  public int Delay { get; }

  public TileCoordinate Coordinate => new(X, Y);

  public LiquidCellWorkItemStateCommand WithKillAndDelay(int kill, int delay)
  {
    return new LiquidCellWorkItemStateCommand(X, Y, kill, delay);
  }

  private static void ValidateNonNegative(int value, string parameterName)
  {
    if (value < 0)
    {
      throw new ArgumentOutOfRangeException(parameterName);
    }
  }
}
