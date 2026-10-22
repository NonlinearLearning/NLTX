using System.Collections.Generic;

namespace Terraria.Projectile;

public enum ProjectileAttribute {
  NoEnchantments = 1,
  NoEnchantmentVisuals = 2,
  Reflected = 3
}

/// <summary>
/// 保存射弹当前生效的附魔限制和反射属性标记。
/// </summary>
/// <remarks>
/// <para>
/// 用途：用枚举集合表达 NoEnchantments、NoEnchantmentVisuals 和 Reflected 等标记，让相关攻击和表现流程读取。
/// </para>
/// <para>
/// 当前实现：Attributes 是可变列表；调用方添加或移除标记。列表不会自动去重，附魔和反射行为由外部系统根据标记执行。
/// </para>
/// </remarks>
public sealed class ProjectileAttributeComponent {
  /// <summary>
  /// 当前生效的射弹属性列表；元素是 ProjectileAttribute 枚举值。
  /// </summary>
  public List<ProjectileAttribute> Attributes { get; } = new();
}