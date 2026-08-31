using System;
using Terraria.Dome.Simulation.Components;

namespace Terraria.Dome.Simulation.Projectile.Systems;

public sealed class ProjectileTileCollisionPolicy
{
  private const int Type876 = 876;

  public bool ShouldCollide(ProjectileTileCollisionComponent state)
  {
    return state.Enabled;
  }

  public ProjectileTileCollisionComponent ApplyLegacyAiOverride(
    int projectileType,
    float ai1,
    ProjectileTileCollisionComponent state)
  {
    if (projectileType != Type876)
    {
      return state;
    }

    if (!float.IsFinite(ai1))
    {
      throw new ArgumentOutOfRangeException(nameof(ai1));
    }

    return state with { Enabled = ai1 == 0.0f };
  }
}
