using System.Collections.Generic;
using System.Numerics;
using System;

namespace Terraria.Projectile;

public struct ProjectileTrailCacheComponent
{
  public ProjectileTrailCacheComponent()
    : this(10)
  {
  }

  public ProjectileTrailCacheComponent(int historyLength)
  {
    if (historyLength < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(historyLength));
    }

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
