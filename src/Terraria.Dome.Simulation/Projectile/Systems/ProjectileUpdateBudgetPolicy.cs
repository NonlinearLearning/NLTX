using System;

namespace Terraria.Dome.Simulation.Projectile.Systems;

public static class ProjectileUpdateBudgetPolicy
{
  public static int GetMaxUpdates(int extraUpdates)
  {
    if (extraUpdates < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(extraUpdates));
    }

    return checked(extraUpdates + 1);
  }
}
