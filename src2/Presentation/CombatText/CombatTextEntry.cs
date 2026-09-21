using System.Numerics;

namespace Terraria.Presentation.CombatText;

public readonly record struct CombatTextEntry(
  Vector2 Position,
  Vector2 Velocity,
  float Alpha,
  int AlphaDirection,
  string Text,
  float Scale,
  float Rotation,
  CombatTextColor Color,
  bool IsActive,
  int RemainingTicks,
  bool IsCritical,
  bool IsDamageOverTime)
{
  public static CombatTextEntry Empty => new(
    Vector2.Zero,
    Vector2.Zero,
    0f,
    1,
    string.Empty,
    1f,
    0f,
    default,
    false,
    0,
    false,
    false);
}
