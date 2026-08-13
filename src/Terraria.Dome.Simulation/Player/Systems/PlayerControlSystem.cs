using System.Collections.Generic;
using Arch.Core;
using Terraria.Dome.Simulation.Components;

namespace Terraria.Dome.Simulation.Player.Systems;

internal sealed class PlayerControlSystem
{
  private const float JumpSpeed = 4.0f;
  private const float PlayerSpeed = 3.0f;

  public void Apply(World world, IReadOnlyCollection<Entity> players)
  {
    foreach (Entity entity in players)
    {
      PlayerInputComponent input = world.Get<PlayerInputComponent>(entity);
      ref VelocityComponent velocity = ref world.Get<VelocityComponent>(entity);
      ref FacingComponent facing = ref world.Get<FacingComponent>(entity);
      float direction = 0.0f;

      if (input.MoveLeft && !input.MoveRight)
      {
        direction = -1.0f;
      }
      else if (input.MoveRight && !input.MoveLeft)
      {
        direction = 1.0f;
      }

      velocity.X = direction * PlayerSpeed;
      if (direction != 0.0f)
      {
        facing.Horizontal = direction > 0.0f ? 1 : -1;
      }

      PhysicsStateComponent physics = world.Get<PhysicsStateComponent>(entity);
      if (input.Jump && physics.IsGrounded)
      {
        velocity.Y = JumpSpeed;
      }
    }
  }
}
