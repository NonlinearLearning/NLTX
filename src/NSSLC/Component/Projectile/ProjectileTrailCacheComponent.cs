using System.Collections.Generic;
using System.Numerics;
using System;

namespace Terraria.Projectile;

/// <summary>
/// 保存射弹历史位置、旋转、精灵朝向及鞭子轨迹点。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Projectile。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Projectile.cs。</para>
/// <para>主要源成员：oldPos（第 180 行）； oldRot（第 182 行）； oldSpriteDirection（第 184 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P15-projectile-component-design.md。</para>
/// <para>依据位置：第 16 行。</para>
/// </remarks>
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
