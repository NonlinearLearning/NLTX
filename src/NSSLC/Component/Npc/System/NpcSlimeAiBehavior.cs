using System.Numerics;

namespace Terraria.Npc;

internal sealed class NpcSlimeAiBehavior : INpcAiBehavior {
  public bool CanHandle(in NpcAiInput input) {
    return input.Definition.NetId is 1 or 16;
  }

  public NpcAiDecision Evaluate(in NpcAiInput input) {
    NpcAiStateComponent state = input.State;
    Vector2 center = input.Position + new Vector2(
      input.Definition.Movement.Width * 0.5f,
      input.Definition.Movement.Height * 0.5f);
    int direction = NpcAiBehaviorMath.ResolveHorizontalDirection(center, input.Targets);
    state.State1 = direction;
    Vector2 velocity = input.Velocity;
    if (input.Environment.IsGrounded) {
      if (direction != 0 && state.State0 <= 0f) {
        velocity.X = direction * 2f;
        velocity.Y = -5f;
        state.State0 = 30f;
      } else {
        velocity.X = NpcAiBehaviorMath.MoveTowardsZero(velocity.X, 0.12f);
      }
    }

    if (state.State0 > 0f) {
      state.State0--;
    }

    NpcAiTargetSnapshot? target = NpcAiBehaviorMath.FindNearestLivingTarget(
      center,
      input.Targets);
    return new NpcAiDecision(
      direction,
      target?.Slot ?? -1,
      velocity,
      state,
      HasVelocityOverride: true,
      SkipMovement: false);
  }
}
