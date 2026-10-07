using System.Numerics;

namespace Terraria.Npc;

internal sealed class NpcGuideAiBehavior : INpcAiBehavior {
  private const int _guideNetId = 22;

  public bool CanHandle(in NpcAiInput input) {
    return input.Definition.NetId == _guideNetId;
  }

  public NpcAiDecision Evaluate(in NpcAiInput input) {
    NpcAiStateComponent state = input.State;
    int direction = 0;
    if (!input.Environment.DayTime) {
      state.State0 = 0f;
      state.State1 = 0f;
      if (input.Environment.HasHome) {
        float deltaX = input.Environment.HomeCenter.X -
          (input.Position.X + input.Definition.Movement.Width * 0.5f);
        if (MathF.Abs(deltaX) > 16f) {
          direction = Math.Sign(deltaX);
        }
      }
    } else {
      if (state.State0 <= 0f) {
        int phase = ((int)state.State1 + 1) % 4;
        state.State1 = phase;
        state.State0 = phase is 1 or 3 ? 120f : 45f;
      }

      state.State0--;
      direction = (int)state.State1 switch {
        1 => 1,
        3 => -1,
        _ => 0,
      };
    }

    Vector2 velocity = input.Velocity;
    velocity.X = direction == 0
      ? NpcAiBehaviorMath.MoveTowardsZero(velocity.X, 0.08f)
      : Math.Clamp(velocity.X + direction * 0.08f, -1.2f, 1.2f);
    return new NpcAiDecision(
      direction,
      TargetSlot: -1,
      velocity,
      state,
      HasVelocityOverride: true,
      SkipMovement: false);
  }
}
