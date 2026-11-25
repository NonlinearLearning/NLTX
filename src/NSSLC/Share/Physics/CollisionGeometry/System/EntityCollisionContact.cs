using System;
using System.Numerics;

namespace Terraria.Share.Physics;

/// <summary>
/// Deterministic geometric contact produced by the shared entity collision subsystem.
/// </summary>
public readonly record struct EntityCollisionContact
{
  public EntityCollisionContact(Vector2 point, Vector2 normal)
  {
    if (!float.IsFinite(point.X) || !float.IsFinite(point.Y))
    {
      throw new ArgumentOutOfRangeException(nameof(point));
    }

    if (!float.IsFinite(normal.X) || !float.IsFinite(normal.Y))
    {
      throw new ArgumentOutOfRangeException(nameof(normal));
    }

    Point = point;
    Normal = normal;
  }

  public Vector2 Point { get; }

  public Vector2 Normal { get; }
}
