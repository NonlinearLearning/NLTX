using System;
using System.Numerics;
using Terraria.Ai;

namespace Terraria.Npc;

/// <summary>
/// Ordinary type-1 subset of the original AI_001_Slimes behavior.
///
/// The original slime counts grounded frames before jumping, accelerates toward its target while
/// airborne, and damps horizontal speed while resting. The spatial system applies gravity and
/// collision after this behavior emits the intent.
/// </summary>
public sealed class SlimeAiBehavior : AiBehavior<NpcAiIntent>
{
  private static readonly AiRequirements Requirements = AiRequirements.Empty
      .Require<NpcAiMovementFact>()
      .Require<NpcAiTargetFact>();
  private float _jumpTime;

  public SlimeAiBehavior() : base(Requirements)
  {
  }

  protected override void CalculateCore(
      AiSnapshot snapshot,
      float deltaTime,
      AiOutput<NpcAiIntent> output)
  {
    NpcAiMovementFact movement = snapshot.Get<NpcAiMovementFact>();
    NpcAiTargetFact target = snapshot.Get<NpcAiTargetFact>();
    _jumpTime += deltaTime;
    bool shouldJump = movement.IsGrounded && _jumpTime >= 1.6f;
    if (shouldJump)
    {
      _jumpTime = 0f;
    }

    float direction = target.IsUsable
        ? MathF.Sign(target.Position.X - movement.Position.X)
        : 0f;
    Vector2 desired;
    if (target.IsUsable)
    {
      desired = new Vector2(direction * 2.5f, 0f);
    }
    else
    {
      desired = movement.IsGrounded
          ? Vector2.Zero
          : new Vector2(movement.Velocity.X * 0.93f, 0f);
    }
    NpcBehaviorPhase phase = target.IsUsable
        ? NpcBehaviorPhase.Chase
        : NpcBehaviorPhase.Idle;
    output.Emit(new NpcAiIntent(
        NpcAiIntentKind.SetBehaviorState,
        phase,
        desiredVelocity: desired,
        jumpRequested: shouldJump,
        stateSlot0: direction,
        stateSlot1: _jumpTime));
    output.Emit(new NpcAiIntent(
        NpcAiIntentKind.Move,
        phase,
        desiredVelocity: desired,
        jumpRequested: shouldJump,
        stateSlot0: direction,
        stateSlot1: _jumpTime));
  }

  protected override void StopCore(AiOutput<NpcAiIntent> output)
  {
    output.Emit(new NpcAiIntent(NpcAiIntentKind.StopMovement));
  }
}
