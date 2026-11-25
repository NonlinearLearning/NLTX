using System;
using System.Numerics;

namespace Terraria.Share.Entities;

/// <summary>
/// Stores an entity's authoritative world-space origin. The coordinate is the collider origin
/// used by the spatial query (normally the entity's top-left corner).
/// </summary>
public struct LocationComponent
{
  public LocationComponent(Vector2 coordinates)
  {
    Validate(coordinates);
    Coordinates = coordinates;
  }

  private Vector2 _coordinates;

  public Vector2 Coordinates
  {
    get => _coordinates;
    set
    {
      Validate(value);
      _coordinates = value;
    }
  }

  public static LocationComponent At(float x, float y)
  {
    return new LocationComponent(new Vector2(x, y));
  }

  private static void Validate(Vector2 coordinates)
  {
    if (!float.IsFinite(coordinates.X) || !float.IsFinite(coordinates.Y))
    {
      throw new ArgumentOutOfRangeException(nameof(coordinates));
    }
  }
}
