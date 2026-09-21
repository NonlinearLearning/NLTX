namespace Terraria.NonAuthoritative.WorldStorage.Geometry;

public sealed class WorldGeometryComponent
{
  public WorldGeometryComponent(
    float leftWorld,
    float rightWorld,
    float topWorld,
    float bottomWorld,
    int maxTilesX,
    int maxTilesY,
    int maxSectionsX,
    int maxSectionsY)
  {
    ValidateFinite(leftWorld, nameof(leftWorld));
    ValidateFinite(rightWorld, nameof(rightWorld));
    ValidateFinite(topWorld, nameof(topWorld));
    ValidateFinite(bottomWorld, nameof(bottomWorld));
    if (rightWorld < leftWorld)
    {
      throw new ArgumentOutOfRangeException(nameof(rightWorld));
    }

    if (bottomWorld < topWorld)
    {
      throw new ArgumentOutOfRangeException(nameof(bottomWorld));
    }

    if (maxTilesX <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(maxTilesX));
    }

    if (maxTilesY <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(maxTilesY));
    }

    if (maxSectionsX < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(maxSectionsX));
    }

    if (maxSectionsY < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(maxSectionsY));
    }

    LeftWorld = leftWorld;
    RightWorld = rightWorld;
    TopWorld = topWorld;
    BottomWorld = bottomWorld;
    MaxTilesX = maxTilesX;
    MaxTilesY = maxTilesY;
    MaxSectionsX = maxSectionsX;
    MaxSectionsY = maxSectionsY;
  }

  public float LeftWorld { get; }

  public float RightWorld { get; }

  public float TopWorld { get; }

  public float BottomWorld { get; }

  public int MaxTilesX { get; }

  public int MaxTilesY { get; }

  public int MaxSectionsX { get; }

  public int MaxSectionsY { get; }

  private static void ValidateFinite(float value, string parameterName)
  {
    if (float.IsNaN(value) || float.IsInfinity(value))
    {
      throw new ArgumentOutOfRangeException(parameterName);
    }
  }
}
