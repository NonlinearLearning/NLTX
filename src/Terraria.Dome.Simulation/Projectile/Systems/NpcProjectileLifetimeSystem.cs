using System;
using Arch.Core;
using Terraria.Dome.Simulation.Components;
using ArchWorld = Arch.Core.World;

namespace Terraria.Dome.Simulation.Projectile.Systems;

public sealed class NpcProjectileLifetimeSystem
{
  public bool Advance(Entity entity, ArchWorld world)
  {
    ArgumentNullException.ThrowIfNull(world);
    if (!world.IsAlive(entity))
    {
      return false;
    }

    ref ProjectileLifetimeComponent lifetime = ref world.Get<ProjectileLifetimeComponent>(entity);
    if (lifetime.RemainingTicks <= 1)
    {
      lifetime.RemainingTicks = 0;
      return true;
    }

    lifetime.RemainingTicks--;
    return false;
  }
}
