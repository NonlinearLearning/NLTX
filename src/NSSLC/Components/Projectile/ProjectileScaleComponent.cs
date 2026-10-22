using System;

namespace Terraria.Projectile;

/// <summary>
/// 保存射弹的缩放比例值。
/// </summary>
/// <remarks>
/// <para>职责：保存射弹缩放比例。</para>
/// <para>拆分来源：Terraria.Projectile。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Projectile.cs。</para>
/// <para>主要源成员：scale（第 114 行）。</para>
/// <para>
/// 用途：为射弹大小相关规则提供缩放输入，例如按 0.5 或 2.0 的比例处理表现。实际几何和碰撞变化由消费该值的系统决定。
/// </para>
/// <para>
/// 当前实现：构造时要求比例是有限且非负的浮点数，并作为只读值保存；不会自行修改碰撞形状或位置。
/// </para>
/// <para>
/// 原始分级记录（2026-10-08）：C153，L2（通用可选玩法）。分级理由：附魔限制、反射、跨实体静态免疫和动态缩放是基础直线射弹之外的规则。
/// </para>
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

  /// <summary>
  /// 非负且有限的射弹缩放比例；1 表示原比例。
  /// </summary>
  public float Scale { get; }
}
