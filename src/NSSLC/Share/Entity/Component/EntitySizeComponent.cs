using System;
using System.Numerics;

namespace Terraria.Share.Combat;

/// <summary>Stores the collision bounds used by spatial and combat systems.</summary>
public readonly record struct EntitySizeComponent {
  public int Width { get; }
  public int Height { get; }

  /// <summary>Returns the logical dimensions in the coordinate type used by spatial queries.</summary>
  public Vector2 AsVector2()
  {
    return new Vector2(Width, Height);
  }

  public EntitySizeComponent(int width, int height) {
    if (width <= 0) {
      throw new ArgumentOutOfRangeException(nameof(width));
    }
    if (height <= 0) {
      throw new ArgumentOutOfRangeException(nameof(height));
    }
    Width = width;
    Height = height;
  }
}
