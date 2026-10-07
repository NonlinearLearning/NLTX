using System;

namespace Terraria.Projectile;

/// <summary>The projectile's non-geometric scale value.</summary>
/// <remarks>
/// <para>职责：保存射弹缩放比例。</para>
/// <para>拆分来源：Terraria.Projectile。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Projectile.cs。</para>
/// <para>主要源成员：scale（第 114 行）。</para>
/// </remarks>
public readonly record struct ProjectileScaleComponent
{
  public ProjectileScaleComponent(float scale)
  {
    if (!float.IsFinite(scale) || scale < 0.0f)
    {
      throw new ArgumentOutOfRangeException(nameof(scale));
    }

    Scale = scale;
  }

  public float Scale { get; }
}
