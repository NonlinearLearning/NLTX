using System;
using System.Numerics;
using Terraria.Share.Movement;

namespace Terraria.Share.Physics;

/// <summary>
/// One tick of vertical gravity integration, shared by every simulation that advances a
/// velocity per tick.
/// </summary>
/// <remarks>
/// 抽成单点是为了让投射物模拟与实体模拟不会在夹取规则上各自漂移——此前两处私有实现逐行等价。
/// </remarks>
internal static class GravityIntegration {
  /// <summary>
  /// Applies gravity along the configured direction and clamps the vertical component to
  /// <see cref="GravityStateComponent.MaxFallSpeed"/>.
  /// </summary>
  internal static Vector2 Apply(
      Vector2 velocity,
      GravityStateComponent gravity,
      float deltaTime) {
    float direction = gravity.Direction == GravityDirection.Reversed ? -1f : 1f;
    float vertical = velocity.Y + gravity.GravityMagnitude * direction * deltaTime;
    return new Vector2(
        velocity.X,
        Math.Clamp(vertical, -gravity.MaxFallSpeed, gravity.MaxFallSpeed));
  }
}
