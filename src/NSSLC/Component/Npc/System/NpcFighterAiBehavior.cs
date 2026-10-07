using System.Numerics;

namespace Terraria.Npc;

internal sealed class NpcFighterAiBehavior : INpcAiBehavior {
  public bool CanHandle(in NpcAiInput input) {
    return input.Definition.NetId == 3;
  }

  public NpcAiDecision Evaluate(in NpcAiInput input) {
    Vector2 center = input.Position +
      new Vector2(
        input.Definition.Movement.Width * 0.5f,
        input.Definition.Movement.Height * 0.5f);
    int direction = NpcAiBehaviorMath.ResolveHorizontalDirection(center, input.Targets);
    NpcAiStateComponent state = input.State;
    state.State0 = direction;
    Vector2 velocity = input.Velocity;
    velocity.X = direction == 0
      ? NpcAiBehaviorMath.MoveTowardsZero(velocity.X, 0.08f)
      : Math.Clamp(velocity.X + direction * 0.1f, -2f, 2f);
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
