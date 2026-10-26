using System;
using System.Numerics;

namespace Terraria.Physics;

/// <summary>
/// Describes the geometry used by spatial queries. Collision response belongs to a system.
/// </summary>
public readonly record struct ColliderComponent
{
  public ColliderComponent(
      Vector2 size,
      Vector2 offset = default,
      ColliderShape shape = ColliderShape.AxisAlignedBox,
      bool enabled = true)
  {
    if (!float.IsFinite(size.X) || !float.IsFinite(size.Y) || size.X <= 0f || size.Y <= 0f)
    {
      throw new ArgumentOutOfRangeException(nameof(size));
    }

    if (!float.IsFinite(offset.X) || !float.IsFinite(offset.Y))
    {
      throw new ArgumentOutOfRangeException(nameof(offset));
    }

    if (!Enum.IsDefined(shape))
    {
      throw new ArgumentOutOfRangeException(nameof(shape));
    }

    Size = size;
    Offset = offset;
    Shape = shape;
    Enabled = enabled;
  }

  public Vector2 Size { get; }

  public Vector2 Offset { get; }

  public ColliderShape Shape { get; }

  public bool Enabled { get; }

  public float Width => Size.X;

  public float Height => Size.Y;
}
