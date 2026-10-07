using System;

namespace Terraria.WorldGeneration.Terrain.TreeTops;

public sealed class WorldTreeTopsStateComponent
{
  public const int TreeTopsAreaCount = WorldTreeTopsAreaId.Count;

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
    if (!WorldTreeTopsSystem.IsValidStyle(areaId, style))
    {
      throw new ArgumentOutOfRangeException(
        nameof(style),
        style,
        $"Tree tops area {areaId} does not support style {style}.");
    }

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
