using System;
using System.Collections.Generic;
using Arch.Core;
using World = Arch.Core.World;
using Terraria.Dome.Simulation.Components;
using Terraria.Dome.Simulation.Movement.Components;
using Terraria.Dome.Simulation.Player.Components;
using Terraria.Dome.Simulation.Player.Definitions;

using EntityEcs.Components;
namespace Terraria.Dome.Simulation.Player.Systems;

internal sealed class PlayerControlSystem
{
  private const float JumpSpeed = 4.0f;
  private const float PlayerSpeed = 3.0f;

  public void Apply(Arch.Core.World world, IReadOnlyCollection<Entity> players)
  {
    foreach (Entity entity in players)
    {
      ControlInputComponent input = world.Get<ControlInputComponent>(entity);
      ref VelocityComponent velocity = ref world.Get<VelocityComponent>(entity);
      ref DirectionComponent facing = ref world.Get<DirectionComponent>(entity);
      ref MovementIntentComponent intent = ref world.Get<MovementIntentComponent>(entity);
      ref PlayerMountStateComponent mount = ref world.Get<PlayerMountStateComponent>(entity);
      float direction = 0.0f;

      if (input.MoveLeft && !input.MoveRight)
      {
        direction = -1.0f;
      }
      else if (input.MoveRight && !input.MoveLeft)
      {
        direction = 1.0f;
      }

      if (mount.IsMounted)
      {
        float targetMagnitude = input.Dash && MountCapabilityRegistry.CanDash(mount.MountType)
          ? MountCapabilityRegistry.GetDashSpeed(mount.MountType)
          : MountCapabilityRegistry.GetRunSpeed(mount.MountType);
        float targetSpeed = direction * targetMagnitude;
        float acceleration = MountCapabilityRegistry.GetAcceleration(mount.MountType);
        velocity.X = MoveTowards(velocity.X, targetSpeed, acceleration);
      }
      else
      {
        velocity.X = direction * PlayerSpeed;
      }
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
        velocity.Y = mount.IsMounted
          ? MountCapabilityRegistry.GetJumpSpeed(mount.MountType)
          : JumpSpeed;
      }
      else if (input.Up && mount.IsMounted &&
               mount.TryConsumeFlightInput(true))
      {
        velocity.Y = MountCapabilityRegistry.GetJumpSpeed(mount.MountType);
      }
    }
  }

  private static float MoveTowards(float current, float target, float maximumDelta)
  {
    float delta = target - current;
    if (MathF.Abs(delta) <= maximumDelta)
    {
      return target;
    }

    return current + MathF.Sign(delta) * maximumDelta;
  }
}
