using System.Numerics;

namespace Terraria.Npc;

internal sealed class NpcFloatingEyeAiBehavior : INpcAiBehavior {
  public bool CanHandle(in NpcAiInput input) {
    return input.Definition.NetId == 2;
  }

  public NpcAiDecision Evaluate(in NpcAiInput input) {
    NpcAiStateComponent state = input.State;
    if (state.State3 == 0f) {
      state.State0 = 75f;
      state.State1 = 0f;
      state.State2 = input.Slot % 2 == 0 ? -1f : 1f;
      state.State3 = 1f;
    }

    Vector2 center = input.Position +
      new Vector2(
        input.Definition.Movement.Width * 0.5f,
        input.Definition.Movement.Height * 0.5f);
    NpcAiTargetSnapshot? target = NpcAiBehaviorMath.FindNearestLivingTarget(
      center,
      input.Targets);
    if (target is not NpcAiTargetSnapshot livingTarget) {
      return new NpcAiDecision(
        Action: 0,
        TargetSlot: -1,
        input.Velocity * 0.9f,
        state,
        HasVelocityOverride: true,
        SkipMovement: false);
    }

    if (state.State0 <= 0f) {
      state.State1 = state.State1 == 0f ? 1f : 0f;
      state.State0 = state.State1 == 1f ? 45f : 75f;
      if (state.State1 == 0f) {
        state.State2 = center.X <= livingTarget.Center.X ? -1f : 1f;
      }
    }

    state.State0--;
    bool diving = state.State1 == 1f;
    Vector2 destination = diving
      ? livingTarget.Center
      : livingTarget.Center + new Vector2(state.State2 * 96f, -88f);
    Vector2 desiredVelocity = destination - center;
    float distance = desiredVelocity.Length();
    float speed = diving ? 5.2f : 2.4f;
    desiredVelocity = distance > 0.001f
      ? desiredVelocity * (Math.Min(distance, speed) / distance)
      : Vector2.Zero;
    float acceleration = diving ? 0.22f : 0.12f;
    Vector2 velocity = NpcAiBehaviorMath.MoveTowards(
      input.Velocity,
      desiredVelocity,
      acceleration);
    return new NpcAiDecision(
      diving ? 1 : 0,
      livingTarget.Slot,
      velocity,
      state,
      HasVelocityOverride: true,
      SkipMovement: false);
  }
}
