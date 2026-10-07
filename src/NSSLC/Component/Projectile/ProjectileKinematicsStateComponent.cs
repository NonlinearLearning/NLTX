using System;
using System.Numerics;

namespace Terraria.Projectile;

/// <summary>
/// 保存射弹当前位置和速度。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Entity。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Entity.cs。</para>
/// <para>主要源成员：position（第 10 行）； velocity（第 12 行）。</para>
/// <para>拆分依据目录：docs/system-decomposition/reports/。</para>
/// <para>
/// 拆分依据文件：2026-09-30-system-decomposition-authoritative-P15-projectile-execution.md。
/// </para>
/// <para>依据位置：第 299 行。</para>
/// </remarks>
public struct ProjectileKinematicsStateComponent
{
  public ProjectileKinematicsStateComponent(Vector2 position, Vector2 velocity)
  {
    ValidateFinite(position, nameof(position));
    ValidateFinite(velocity, nameof(velocity));
    Position = position;
    Velocity = velocity;
  }

  public Vector2 Position;
  public Vector2 Velocity;

  private static void ValidateFinite(Vector2 value, string parameterName)
  {
    if (!float.IsFinite(value.X) || !float.IsFinite(value.Y))
    {
      throw new ArgumentOutOfRangeException(parameterName);
    }
  }
}
