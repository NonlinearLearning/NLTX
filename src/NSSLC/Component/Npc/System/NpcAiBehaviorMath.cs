using System.Numerics;

namespace Terraria.Npc;

internal static class NpcAiBehaviorMath {
  public static NpcAiTargetSnapshot? FindNearestLivingTarget(
    Vector2 origin,
    IReadOnlyList<NpcAiTargetSnapshot> targets) {
    NpcAiTargetSnapshot? nearest = null;
    float nearestDistanceSquared = float.PositiveInfinity;
    foreach (NpcAiTargetSnapshot target in targets) {
      if (!target.IsLiving) {
        continue;
      }

      float distanceSquared = Vector2.DistanceSquared(origin, target.Center);
      if (distanceSquared < nearestDistanceSquared) {
        nearest = target;
        nearestDistanceSquared = distanceSquared;
      }
    }

    return nearest;
  }

  public static int ResolveHorizontalDirection(
    Vector2 origin,
    IReadOnlyList<NpcAiTargetSnapshot> targets,
    float deadZone = 2f) {
    NpcAiTargetSnapshot? target = FindNearestLivingTarget(origin, targets);
    if (target is not NpcAiTargetSnapshot livingTarget) {
      return 0;
    }

    float deltaX = livingTarget.Center.X - origin.X;
    return MathF.Abs(deltaX) < deadZone ? 0 : Math.Sign(deltaX);
  }

  public static float MoveTowardsZero(float value, float amount) {
    if (value > 0f) {
      return Math.Max(0f, value - amount);
    }

    return Math.Min(0f, value + amount);
  }

  public static Vector2 MoveTowards(
    Vector2 current,
    Vector2 target,
    float maximumDelta) {
    Vector2 delta = target - current;
    float length = delta.Length();
    if (length > maximumDelta) {
      delta *= maximumDelta / length;
    }

    return current + delta;
  }

  public static NpcAiDecision KeepCurrent(
    in NpcAiInput input,
    int action = 0,
    bool skipMovement = false) {
    return new NpcAiDecision(
      action,
      TargetSlot: -1,
      input.Velocity,
      input.State,
      HasVelocityOverride: false,
      skipMovement);
  }
}
