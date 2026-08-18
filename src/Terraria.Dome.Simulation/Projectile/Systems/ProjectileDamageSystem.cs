using System.Collections.Generic;
using Arch.Core;
using World = Arch.Core.World;
using Terraria.Dome.Simulation.Combat.Components;
using Terraria.Dome.Simulation.Combat.Events;
using Terraria.Dome.Simulation.Combat.Systems;
using Terraria.Dome.Simulation.Components;

namespace Terraria.Dome.Simulation.Projectile.Systems;

public sealed class ProjectileDamageSystem
{
  private const int HitImmunityTicks = 1;
  private readonly HitImmunitySystem _immunitySystem = new();

  public IReadOnlyList<DamageRequestedEvent> Resolve(
    Arch.Core.World world,
    IEnumerable<DamageRequestedEvent> candidates,
    HitImmunityComponent immunity)
  {
    List<DamageRequestedEvent> ordered = new(candidates);
    ordered.Sort(static (left, right) =>
    {
      int projectileComparison = left.ProjectileIdentity.CompareTo(right.ProjectileIdentity);
      return projectileComparison != 0
        ? projectileComparison
        : left.TargetIdentity.CompareTo(right.TargetIdentity);
    });

    List<DamageRequestedEvent> accepted = new();
    for (int index = 0; index < ordered.Count; index++)
    {
      DamageRequestedEvent candidate = ordered[index];
      if (!world.IsAlive(candidate.Projectile) || !world.IsAlive(candidate.Target) ||
          _immunitySystem.IsImmune(immunity, candidate.TargetIdentity))
      {
        continue;
      }

      ref ProjectilePenetrationComponent penetration =
        ref world.Get<ProjectilePenetrationComponent>(candidate.Projectile);
      if (penetration.RemainingPenetration == 0)
      {
        continue;
      }

      if (penetration.RemainingPenetration > 0)
      {
        penetration.RemainingPenetration--;
      }

      _immunitySystem.Apply(immunity, candidate.TargetIdentity, HitImmunityTicks);
      accepted.Add(candidate);
    }

    return accepted;
  }
}
