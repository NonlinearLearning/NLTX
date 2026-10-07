using System;

namespace Terraria.Projectile;

/// <summary>
/// 保存射弹旋转、精灵朝向、步进速度和更新计数。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Projectile。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Projectile.cs。</para>
/// <para>
/// 主要源成员：rotation（第 116 行）； gfxOffY（第 132 行）； stepSpeed（第 134 行）； spriteDirection（第 146 行）；
/// numUpdates（第 200 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P15-projectile-component-design.md。</para>
/// <para>依据位置：第 16 行。</para>
/// </remarks>
public struct ProjectileTrajectoryStateComponent
{
  public ProjectileTrajectoryStateComponent(
    float rotation,
    int spriteDirection,
    float stepSpeed,
    int numUpdates,
    float gfxOffY)
  {
    ValidateFinite(rotation, nameof(rotation));
    ValidateFinite(stepSpeed, nameof(stepSpeed));
    ValidateFinite(gfxOffY, nameof(gfxOffY));
    if (stepSpeed < 0.0f)
    {
      throw new ArgumentOutOfRangeException(nameof(stepSpeed));
    }

    Rotation = rotation;
    SpriteDirection = spriteDirection;
    StepSpeed = stepSpeed;
    NumUpdates = numUpdates;
    GfxOffY = gfxOffY;
  }

  public float Rotation;
  public int SpriteDirection;
  public float StepSpeed;
  // -1 is the exhausted-loop sentinel; projectile behavior may reset it to 0.
  public int NumUpdates;
  public float GfxOffY;

  private static void ValidateFinite(float value, string parameterName)
  {
    if (!float.IsFinite(value))
    {
      throw new ArgumentOutOfRangeException(parameterName);
    }
  }
}
