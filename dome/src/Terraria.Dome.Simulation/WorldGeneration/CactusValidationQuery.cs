using System;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldModel.Definitions;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class CactusValidationQuery
{
  private const ushort CactusTileType = 80;

  public static CactusValidationResult Evaluate(
    WorldGridSnapshot snapshot,
    TileDefinitionRegistry tileDefinitions,
    int x,
    int y)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(tileDefinitions);
    if (!snapshot.Metadata.IsInside(x, y))
    {
      throw new ArgumentOutOfRangeException(nameof(x));
    }

    int supportX = x;
    int supportY = y;
    int height = 0;
    while (snapshot.Metadata.IsInside(supportX, supportY) &&
           snapshot.GetTile(supportX, supportY).IsActive &&
           snapshot.GetTile(supportX, supportY).Type == CactusTileType)
    {
      height++;
      supportY++;
    }

    bool hasSideBranch = HasCactus(snapshot, supportX - 1, supportY) ||
      HasCactus(snapshot, supportX + 1, supportY);
    if (hasSideBranch)
    {
      supportX += HasCactus(snapshot, supportX - 1, supportY) ? -1 : 1;
      hasSideBranch &= HasCactus(snapshot, supportX, supportY - 1);
    }

    WorldTile support = snapshot.Metadata.IsInside(supportX, supportY)
      ? snapshot.GetTile(supportX, supportY)
      : default;
    bool supported = support.IsActive && !support.IsHalfBrick && support.Slope == 0 &&
      CactusFrameQuery.RegisterSupportedGroundDefaults().Contains(support.Type) &&
      TileStateQuery.IsSolid(support, tileDefinitions);
    bool attached = x == supportX ||
      HasCactus(snapshot, x, y + 1) ||
      HasCactus(snapshot, x - 1, y) ||
      HasCactus(snapshot, x + 1, y);
    supported &= attached;

    return new CactusValidationResult(
      supported,
      !supported,
      supportX,
      supportY,
      height,
      hasSideBranch);
  }

  private static bool HasCactus(WorldGridSnapshot snapshot, int x, int y)
  {
    return snapshot.Metadata.IsInside(x, y) &&
      snapshot.GetTile(x, y).IsActive &&
      snapshot.GetTile(x, y).Type == CactusTileType;
  }
}
