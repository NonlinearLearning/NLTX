using Arch.Core;
using World = Arch.Core.World;
using Terraria.Dome.Simulation.Components;

namespace Terraria.Dome.Simulation.Projectile.Systems;

public sealed class ProjectileLifetimeSystem
{
  public bool Advance(Entity entity, Arch.Core.World world)
  {
    ref ProjectileLifetimeComponent lifetime = ref world.Get<ProjectileLifetimeComponent>(entity);
    lifetime.RemainingTicks--;
    return lifetime.RemainingTicks <= 0;
  }
}
