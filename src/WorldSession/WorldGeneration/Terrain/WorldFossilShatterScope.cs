using System;

namespace Terraria.WorldGeneration.Terrain;

public sealed class WorldFossilShatterScope : IDisposable
{
  public const int FossilTileType = 404;

  public bool IsActive { get; private set; }

  public bool TryEnter(int tileType)
  {
    if (IsActive || tileType != FossilTileType)
    {
      return false;
    }

    IsActive = true;
    return true;
  }

  public void Dispose()
  {
    IsActive = false;
  }
}
