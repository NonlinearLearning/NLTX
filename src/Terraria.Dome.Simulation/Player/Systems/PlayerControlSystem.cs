using System.Collections.Generic;
using Arch.Core;
using World = Arch.Core.World;
using Terraria.Dome.Simulation.Components;
using Terraria.Dome.Simulation.Movement.Components;

namespace Terraria.Dome.Simulation.Player.Systems;

internal sealed class PlayerControlSystem
{
  private const float JumpSpeed = 4.0f;
  private const float PlayerSpeed = 3.0f;

  public void Apply(Arch.Core.World world, IReadOnlyCollection<Entity> players)
  {
    foreach (Entity entity in players)
    {
      PlayerInputComponent input = world.Get<PlayerInputComponent>(entity);
      ref VelocityComponent velocity = ref world.Get<VelocityComponent>(entity);
      ref FacingComponent facing = ref world.Get<FacingComponent>(entity);
      ref MovementIntentComponent intent = ref world.Get<MovementIntentComponent>(entity);
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
      intent.HorizontalDirection = direction switch
      {
        < 0.0f => -1,
        > 0.0f => 1,
        _ => 0
      };
      intent.JumpRequested = input.Jump;
      intent.FireRequested = input.Fire;
      intent.UseItemRequested = input.UseItem;
      if (input.Facing != 0)
      {
        facing.Horizontal = input.Facing;
      }
      else if (direction != 0.0f)
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
