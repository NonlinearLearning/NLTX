using System.Collections.Generic;
using Arch.Core;
using World = Arch.Core.World;
using Terraria.Dome.Simulation.Components;
using Terraria.Dome.Simulation.Player.Components;

namespace Terraria.Dome.Simulation.Player.Systems;

internal sealed class PlayerGravitySystem
{
  private const float GravityPerTick = -1.0f;

  public void Apply(Arch.Core.World world, IReadOnlyCollection<Entity> players)
  {
    foreach (Entity entity in players)
    {
      ref VelocityComponent velocity = ref world.Get<VelocityComponent>(entity);
      PhysicsStateComponent physics = world.Get<PhysicsStateComponent>(entity);
      PlayerMountStateComponent mount = world.Get<PlayerMountStateComponent>(entity);
      if (mount.IsHoverActive)
      {
        continue;
      }
      float gravityDirection = physics.GravityDirection == 0.0f
        ? 1.0f
        : physics.GravityDirection;
      velocity.Y += GravityPerTick * gravityDirection;
    }
  }
}
