using System;
using System.Numerics;

namespace Terraria.Combat;

/// <summary>Optional spatial fact attached to a damage request.</summary>
public readonly record struct DamageContact
{
  public DamageContact(Vector2 point, Vector2 normal)
  {
    if (!float.IsFinite(point.X) || !float.IsFinite(point.Y) ||
        !float.IsFinite(normal.X) || !float.IsFinite(normal.Y))
    {
      throw new ArgumentOutOfRangeException(nameof(point));
    }

    Point = point;
    Normal = normal;
  }

  public Vector2 Point { get; }

  public Vector2 Normal { get; }
}
