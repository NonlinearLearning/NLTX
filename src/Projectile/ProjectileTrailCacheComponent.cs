using System.Collections.Generic;
using System.Numerics;

namespace Terraria.Projectile;

public struct ProjectileTrailCacheComponent
{
  public ProjectileTrailCacheComponent()
    : this(10)
  {
  }

  public ProjectileTrailCacheComponent(int historyLength)
  {
    OldPositions = new Vector2[historyLength];
    OldRotations = new float[historyLength];
    OldSpriteDirections = new int[historyLength];
    WhipPoints = new List<Vector2>();
  }

  public Vector2[] OldPositions;
  public float[] OldRotations;
  public int[] OldSpriteDirections;
  public List<Vector2> WhipPoints;
}
