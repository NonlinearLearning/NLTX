using System.Collections.Generic;
using System.Numerics;

namespace Terraria.Player.Mobility;

// Preserve grapple slot order because the legacy speed cap reads the first slot's projectile type.
public readonly record struct PlayerGrappleForcesInput(
  IReadOnlyList<PlayerGrappleProjectileSnapshot> ProjectilesInSlotOrder,
  Vector2 FromPosition,
  Vector2 PlayerCenter,
  Vector2 CurrentVelocity,
  bool ControlLeft,
  bool ControlRight,
  bool ControlUp,
  bool ControlDown,
  float GravityDirection);
