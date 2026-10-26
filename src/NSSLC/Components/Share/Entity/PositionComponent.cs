using System;
using System.Numerics;

namespace Terraria.Combat;

/// <summary>
/// Compatibility projection for callers that still use the former position name.
/// New spatial code uses <see cref="Terraria.Entity.LocationComponent"/> as its only
/// authoritative location component.
/// </summary>
[Obsolete("Use Terraria.Entity.LocationComponent for authoritative spatial state.")]
public readonly record struct PositionComponent(Vector2 Coordinates)
{
  public Terraria.Entity.LocationComponent ToLocation()
  {
    return new Terraria.Entity.LocationComponent(Coordinates);
  }
}
