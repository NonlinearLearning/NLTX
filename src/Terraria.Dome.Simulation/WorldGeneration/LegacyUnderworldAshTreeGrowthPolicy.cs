using System;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyUnderworldAshTreeGrowthPolicy
{
  public static bool ShouldAttempt(
    WorldGridSnapshot snapshot,
    LegacyPassRandomState random,
    int x,
    int y,
    bool isRemixWorld)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(random);
    if (isRemixWorld || !LegacyUnderworldAshBrickPolicy.IsOuterColumn(x, snapshot.Metadata.Width) ||
        y < snapshot.Metadata.Height - 200 || y >= snapshot.Metadata.Height - 50 || y <= 0)
    {
      return false;
    }

    WorldTile tile = snapshot.GetTile(x, y);
    if (!tile.IsActive || tile.Type != 633 || snapshot.GetTile(x, y - 1).IsActive)
    {
      return false;
    }

    return random.Next(3) == 0;
  }
}
