using System.Numerics;

namespace Terraria.Projectile;

/// <summary>
/// 保存射弹的二维轨迹方向。
/// </summary>
/// <remarks>
/// <para>拆分来源：由 Projectile 的速度与轨迹朝向更新流程重组。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Projectile.cs。</para>
/// <para>重组说明：二维 Trajectory 是 NLTX 的轨迹方向表达；原 Entity.direction 是水平整数朝向。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P15-projectile-component-design.md。</para>
/// <para>依据位置：第 664 行。</para>
/// </remarks>
public struct ProjectileDirectionComponent
{
  public ProjectileDirectionComponent(Vector2 trajectory)
  {
    Trajectory = trajectory;
  }

  public Vector2 Trajectory;
}
