using System;

namespace Terraria.Dome.Simulation.Projectile.Definitions;

public static class LegacySentryPersistenceRegistry
{
  private static readonly int[] PersistingProjectileTypes =
  [
    663,
    665,
    667,
    677,
    678,
    679,
    688,
    689,
    690,
    691,
    692,
    693
  ];

  public static bool ShouldPersist(
    int projectileType,
    bool eventActive,
    bool isDd2Summon = false)
  {
    if (projectileType < 0 || !eventActive)
    {
      return false;
    }

    return isDd2Summon || Array.BinarySearch(PersistingProjectileTypes, projectileType) >= 0;
  }
}
