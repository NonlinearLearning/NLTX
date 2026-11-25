using System;
using System.Collections.Generic;
using System.Numerics;

namespace Terraria.Unshared.Npc;

/// <summary>Calculates the supported ordinary type-1 slime movement step from captured facts.</summary>
public static class SlimeAiRules
{
  private const float SimulationTicksPerSecond = 60f;

  public static IReadOnlyList<NpcAiIntent> Calculate(
      NpcAiMovementFact movement,
      NpcAiTargetFact target,
      NpcAiLifeFact life,
      NpcAiWorldFact world,
      float jumpTimer,
      bool initialized,
      float previousDirection,
      float deltaTime)
  {
    if (!float.IsFinite(jumpTimer) || !float.IsFinite(previousDirection) ||
        !float.IsFinite(deltaTime) || deltaTime <= 0f)
    {
      throw new ArgumentOutOfRangeException(nameof(deltaTime));
    }

    float horizontalSpeed = movement.Velocity.X / SimulationTicksPerSecond;
    bool aggressive = IsAggressive(movement.Position, life, world);
    float direction = target.IsUsable
        ? MathF.Sign(target.Position.X - movement.Position.X)
        : 0f;
    if (direction == 0f)
    {
      direction = previousDirection == 0f
          ? horizontalSpeed < 0f ? -1f : 1f
          : MathF.Sign(previousDirection);
    }

    float nextJumpTimer = initialized ? jumpTimer : -100f;
    bool jumpRequested = false;
    float jumpSpeed = 0f;
    if (movement.IsGrounded)
    {
      horizontalSpeed *= 0.8f;
      if (MathF.Abs(horizontalSpeed) < 0.1f)
      {
        horizontalSpeed = 0f;
      }

      float frameCount = deltaTime * SimulationTicksPerSecond;
      nextJumpTimer += frameCount * (aggressive ? 2f : 1f);
      int jump = ResolveJump(nextJumpTimer);
      if (jump > 0)
      {
        jumpRequested = true;
        if (jump == 3)
        {
          horizontalSpeed += 3f * direction;
          jumpSpeed = 8f * SimulationTicksPerSecond;
          nextJumpTimer = -200f;
        }
        else
        {
          horizontalSpeed += 2f * direction;
          jumpSpeed = 6f * SimulationTicksPerSecond;
          nextJumpTimer = jump == 1 ? -1120f : -2120f;
        }
      }
    }
    else if (target.IsUsable &&
        (direction > 0f ? horizontalSpeed < 3f : horizontalSpeed > -3f))
    {
      if (direction > 0f ? horizontalSpeed > -0.01f : horizontalSpeed < 0.01f)
      {
        horizontalSpeed += 0.2f * direction;
      }
      else
      {
        horizontalSpeed *= 0.93f;
      }
    }

    Vector2 desiredVelocity = new(horizontalSpeed * SimulationTicksPerSecond, 0f);
    NpcBehaviorPhase phase = target.IsUsable
        ? NpcBehaviorPhase.Chase
        : NpcBehaviorPhase.Idle;
    return
    [
      new NpcAiIntent(
          NpcAiIntentKind.SetBehaviorState,
          phase,
          desiredVelocity: desiredVelocity,
          actionPhase: 1,
          stateSlot0: direction,
          stateSlot1: nextJumpTimer,
          jumpRequested: jumpRequested,
          jumpSpeed: jumpSpeed),
      new NpcAiIntent(
          NpcAiIntentKind.Move,
          phase,
          desiredVelocity: desiredVelocity,
          stateSlot0: direction,
          stateSlot1: nextJumpTimer,
          jumpRequested: jumpRequested,
          jumpSpeed: jumpSpeed)
    ];
  }

  private static int ResolveJump(float jumpTimer)
  {
    if (jumpTimer >= 0f)
    {
      return 1;
    }

    if (jumpTimer >= -1000f && jumpTimer <= -500f)
    {
      return 2;
    }

    return jumpTimer >= -2000f && jumpTimer <= -1500f ? 3 : 0;
  }

  private static bool IsAggressive(
      Vector2 position,
      NpcAiLifeFact life,
      NpcAiWorldFact world)
  {
    return world.IsKnown && life.IsKnown &&
        (!world.DayTime || life.CurrentLife != life.MaximumLife ||
         position.Y > world.WorldSurfaceY || world.SlimeRain);
  }
}
