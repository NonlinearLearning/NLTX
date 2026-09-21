using System;

namespace Terraria.WorldGeneration.Components;

public sealed class WorldDimensionCompatibilityState
{
  public int LastMaxTilesX { get; private set; }

  public int LastMaxTilesY { get; private set; }

  public bool HasPreviousDimensions => LastMaxTilesX > 0 && LastMaxTilesY > 0;

  public void Capture(int lastMaxTilesX, int lastMaxTilesY)
  {
    if (lastMaxTilesX < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(lastMaxTilesX));
    }

    if (lastMaxTilesY < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(lastMaxTilesY));
    }

    LastMaxTilesX = lastMaxTilesX;
    LastMaxTilesY = lastMaxTilesY;
  }

  public void Clear()
  {
    LastMaxTilesX = 0;
    LastMaxTilesY = 0;
  }
}
