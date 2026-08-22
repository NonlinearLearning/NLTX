using System;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class CactusFrameQuery
{
  private const ushort CactusTileType = 80;

  public static CactusFrameResult Evaluate(WorldGridSnapshot snapshot, int x, int y)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    if (!snapshot.Metadata.IsInside(x, y))
    {
      throw new ArgumentOutOfRangeException(nameof(x));
    }

    int supportX = x;
    int supportY = y;
    while (IsCactus(snapshot, supportX, supportY))
    {
      supportY++;
      if (!snapshot.Metadata.IsInside(supportX, supportY))
      {
        return new CactusFrameResult(false, supportX, supportY);
      }

      if (IsCactus(snapshot, supportX, supportY))
      {
        continue;
      }

      if (IsCactus(snapshot, supportX - 1, supportY) &&
          IsCactus(snapshot, supportX - 1, supportY - 1) && supportX >= x)
      {
        supportX--;
      }

      if (IsCactus(snapshot, supportX + 1, supportY) &&
          IsCactus(snapshot, supportX + 1, supportY - 1) && supportX <= x)
      {
        supportX++;
      }
    }

    WorldTile support = snapshot.GetTile(supportX, supportY);
    if (!IsSupportedGround(support))
    {
      return new CactusFrameResult(true, supportX, supportY);
    }

    if (x != supportX)
    {
      bool hasDownwardCactus = IsCactus(snapshot, x, y + 1);
      bool hasLeftCactus = IsCactus(snapshot, x - 1, y);
      bool hasRightCactus = IsCactus(snapshot, x + 1, y);
      return new CactusFrameResult(
        !hasDownwardCactus && !hasLeftCactus && !hasRightCactus,
        supportX,
        supportY);
    }

    bool hasSupportedDownwardConnection = snapshot.Metadata.IsInside(x, y + 1) &&
      (IsCactus(snapshot, x, y + 1) ||
      IsSupportedGround(snapshot.GetTile(x, y + 1)));
    return new CactusFrameResult(!hasSupportedDownwardConnection, supportX, supportY);
  }

  private static bool IsCactus(WorldGridSnapshot snapshot, int x, int y)
  {
    return snapshot.Metadata.IsInside(x, y) && snapshot.GetTile(x, y) is
    {
      IsActive: true,
      Type: CactusTileType
    };
  }

  private static bool IsSupportedGround(WorldTile tile)
  {
    return tile.IsActive && !tile.IsHalfBrick && tile.Slope == 0 &&
      tile.Type is 53 or 112 or 116 or 234;
  }
}
