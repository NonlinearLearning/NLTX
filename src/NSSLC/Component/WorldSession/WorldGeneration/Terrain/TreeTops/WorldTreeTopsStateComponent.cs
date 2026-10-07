using System;

namespace Terraria.WorldGeneration.Terrain.TreeTops;

/// <summary>
/// 保存世界各区域的树冠样式。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.WorldGen。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/WorldGen.cs。</para>
/// <para>主要源成员：TreeTops（第 4336 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-P16-world-lifecycle-housing-metrics-component-design.md。
/// </para>
/// <para>依据位置：第 938 行。</para>
/// </remarks>
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
