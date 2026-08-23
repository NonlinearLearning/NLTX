using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed record LegacyTileRunnerRequest
{
  public LegacyTileRunnerRequest(
    int x,
    int y,
    double strength,
    int steps,
    int tileType,
    bool addTile,
    double speedX,
    double speedY,
    bool noYChange,
    bool overwrite,
    int ignoreTileType)
  {
    if (x < 0 || y < 0 || !double.IsFinite(strength) || strength <= 0 || steps <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(x));
    }

    if (!double.IsFinite(speedX) || !double.IsFinite(speedY))
    {
      throw new ArgumentOutOfRangeException(nameof(speedX));
    }

    X = x;
    Y = y;
    Strength = strength;
    Steps = steps;
    TileType = tileType;
    AddTile = addTile;
    SpeedX = speedX;
    SpeedY = speedY;
    NoYChange = noYChange;
    Overwrite = overwrite;
    IgnoreTileType = ignoreTileType;
  }

  public bool AddTile { get; }

  public int IgnoreTileType { get; }

  public bool NoYChange { get; }

  public bool Overwrite { get; }

  public double SpeedX { get; }

  public double SpeedY { get; }

  public double Strength { get; }

  public int Steps { get; }

  public int TileType { get; }

  public int X { get; }

  public int Y { get; }
}
