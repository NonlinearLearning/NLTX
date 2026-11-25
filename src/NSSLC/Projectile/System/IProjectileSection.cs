using Terraria.Share.Entities;

namespace Terraria.Unshared.Projectiles;

/// <summary>Declares one content family's contribution to a projectile composition.</summary>
/// <remarks>
/// 为什么是两遍而不是一遍：<see cref="ProjectileHomingSection" /> 的转向率会进入
/// <c>MovementCapabilityComponent.acceleration</c>，这是真实存在的跨族读数。
/// 单遍折叠会让装配结果取决于 section 在列表里的先后——等于把隐式顺序重新引入，
/// 而顺序约束既没有机器检查，插入新族时还会静默改变装配结果。
/// <para>
/// <see cref="Collect" /> 只写本族参数键；<see cref="Contribute" /> 只读参数块并构造组件。
/// 参数块在折叠结束后 Seal，届时任何写入都抛异常。
/// </para>
/// </remarks>
public interface IProjectileSection
{
  /// <summary>声明本族写入参数块的数值；禁止读他族参数、禁止构造组件、禁止访问世界。</summary>
  void Collect(in ProjectileSectionContext context, ProjectileCompositionParameters parameters);

  /// <summary>读参数块并构造本族组件；禁止修改参数块。</summary>
  void Contribute(
      in ProjectileSectionContext context,
      ProjectileCompositionParameters parameters,
      EntityBuilder builder);
}
