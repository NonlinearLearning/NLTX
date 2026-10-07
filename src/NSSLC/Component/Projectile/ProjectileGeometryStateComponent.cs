using System;

namespace Terraria.Projectile;

/// <summary>
/// 保存射弹尺寸和缩放。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Entity。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Entity.cs。</para>
/// <para>主要源成员：width（第 22 行）； height（第 24 行）。</para>
/// <para>拆分来源：Terraria.Projectile。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Projectile.cs。</para>
/// <para>主要源成员：scale（第 114 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P15-projectile-component-design.md。</para>
/// <para>依据位置：第 16 行。</para>
/// </remarks>
public struct ProjectileGeometryStateComponent
{
  public ProjectileGeometryStateComponent(
    float scale = 1.0f)
    : this(0, 0, scale)
  {
  }

  public ProjectileGeometryStateComponent(
    int width,
    int height,
    float scale = 1.0f)
  {
    if (width < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(width));
    }

    if (height < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(height));
    }

    if (!float.IsFinite(scale) || scale < 0.0f)
    {
      throw new ArgumentOutOfRangeException(nameof(scale));
    }

    Width = width;
    Height = height;
    Scale = scale;
  }

  public int Width;
  public int Height;
  public float Scale;
}
