using System.Numerics;

namespace Terraria.Player.Mobility;

public readonly record struct PlayerGrappleForcesResult(
  bool HasForceContributingGrapple,
  int? PreferredDirection,
  Vector2 PreferredVelocity);
