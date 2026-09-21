using System;

namespace Terraria.WorldGeneration.Terrain.TreeTops;

public sealed class WorldTreeTopsStateComponent
{
  public const int TreeTopsAreaCount = 13;

  private readonly int[] _variations = new int[TreeTopsAreaCount];

  public int AreaCount => TreeTopsAreaCount;

  public int GetTreeStyle(int areaId)
  {
    ValidateAreaId(areaId);
    return _variations[areaId];
  }

  public void SetTreeStyle(int areaId, int style)
  {
    ValidateAreaId(areaId);
    _variations[areaId] = style;
  }

  public WorldTreeTopsStateSnapshot CreateSnapshot()
  {
    return new WorldTreeTopsStateSnapshot(_variations);
  }

  private static void ValidateAreaId(int areaId)
  {
    if ((uint)areaId >= TreeTopsAreaCount)
    {
      throw new ArgumentOutOfRangeException(nameof(areaId));
    }
  }
}
