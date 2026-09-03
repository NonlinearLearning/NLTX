using System;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct LegacyUnderworldLiquidCaveOrigin(int X, int Y, bool SurfaceRunnersAllowed);

public static class LegacyUnderworldLiquidCaveOriginPolicy
{
  public static bool TrySelect(
    WorldGridSnapshot snapshot,
    LegacyPassRandomState random,
    int column,
    bool isDrunkWorld,
    bool isRemixWorld,
    out LegacyUnderworldLiquidCaveOrigin origin)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(random);
    origin = default;
    if (column < 0 || column >= snapshot.Metadata.Width || snapshot.Metadata.Height <= 140)
    {
      throw new ArgumentOutOfRangeException(nameof(column));
    }

    if (random.Next(13) != 0)
    {
      return false;
    }

    int y = snapshot.Metadata.Height - 65;
    while ((snapshot.GetTile(column, y).LiquidAmount > 0 || snapshot.GetTile(column, y).IsActive) &&
           y > snapshot.Metadata.Height - 140)
    {
      y--;
    }

    bool inCentralBand = column > snapshot.Metadata.Width * 0.4 &&
      column < snapshot.Metadata.Width * 0.6;
    bool surfaceRunnersAllowed = (!isDrunkWorld && !isRemixWorld) ||
      random.Next(3) == 0 || !inCentralBand;
    origin = new LegacyUnderworldLiquidCaveOrigin(column, y, surfaceRunnersAllowed);
    return true;
  }
}
