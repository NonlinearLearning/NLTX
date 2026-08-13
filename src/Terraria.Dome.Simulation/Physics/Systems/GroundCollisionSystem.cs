using System.Collections.Generic;
using Arch.Core;
using Terraria.Dome.Simulation.Components;

namespace Terraria.Dome.Simulation.Physics.Systems;

internal sealed class GroundCollisionSystem
{
  public void Apply(World world, IReadOnlyCollection<Entity> players)
  {
    foreach (Entity entity in players)
    {
      ref TransformComponent transform = ref world.Get<TransformComponent>(entity);
      ref VelocityComponent velocity = ref world.Get<VelocityComponent>(entity);
      ref PhysicsStateComponent physics = ref world.Get<PhysicsStateComponent>(entity);
      if (transform.Y > 0.0f)
      {
        physics.IsGrounded = false;
        continue;
      }

      transform.Y = 0.0f;
      velocity.Y = 0.0f;
      physics.IsGrounded = true;
    }
  }
}
