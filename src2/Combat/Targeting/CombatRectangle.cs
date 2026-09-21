using System.Numerics;

namespace Terraria.Combat.Targeting;

public readonly record struct CombatRectangle(float X, float Y, float Width, float Height)
{
  public float Right => X + Width;

  public float Bottom => Y + Height;

  public Vector2 Center => new(X + Width / 2f, Y + Height / 2f);
}
