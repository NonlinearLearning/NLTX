using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct TileFrameCoordinate(int X, int Y) : IComparable<TileFrameCoordinate>
{
  public int CompareTo(TileFrameCoordinate other)
  {
    int xComparison = X.CompareTo(other.X);
    return xComparison != 0 ? xComparison : Y.CompareTo(other.Y);
  }
}
