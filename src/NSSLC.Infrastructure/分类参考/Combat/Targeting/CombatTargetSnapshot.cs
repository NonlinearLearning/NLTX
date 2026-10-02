using System.Numerics;

namespace Terraria.Combat.Targeting;

public readonly record struct CombatTargetSnapshot(
  CombatTargetKind TargetKind,
  CombatRectangle Hitbox,
  float Width,
  float Height,
  Vector2 Position,
  Vector2 Velocity)
{
  public static CombatTargetSnapshot Invalid => new(
    CombatTargetKind.Invalid,
    new CombatRectangle(0f, 0f, 0f, 0f),
    0f,
    0f,
    Vector2.Zero,
    Vector2.Zero);

  public Vector2 Center => Hitbox.Center;

  public Vector2 Size => new(Width, Height);

  public bool IsInvalid =>
    TargetKind == CombatTargetKind.Invalid || Width <= 0f || Height <= 0f;
}
