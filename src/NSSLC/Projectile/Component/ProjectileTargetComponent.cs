using System;
using Terraria.Share.Relationships;

namespace Terraria.Unshared.Projectiles;

/// <summary>当前锁定的目标：句柄加版本号。</summary>
/// <remarks>
/// 目标用句柄而不是 <c>Entity</c>：句柄是跨域可见的稳定身份，能安全地被快照投影到 AI 事实里。
/// <see cref="Revision" /> 在"锁定目标发生变化"时递增，AI 输出的事件据此判断自己针对的是
/// 同一目标；换目标不递增会让旧观察命中新实体——这是目标锁定的经典错配。
/// </remarks>
public readonly record struct ProjectileTargetComponent
{
  public ProjectileTargetComponent(EntityRuntimeHandle target, int revision = 1)
  {
    if (!target.IsValid)
    {
      throw new ArgumentException("A projectile target handle is required.", nameof(target));
    }

    if (revision <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(revision));
    }

    Target = target;
    Revision = revision;
  }

  /// <summary>目标实体句柄。</summary>
  public EntityRuntimeHandle Target { get; }

  /// <summary>锁定版本，换目标时递增。</summary>
  public int Revision { get; }

  /// <summary>把锁定切换到另一个目标；同一目标返回自身，避免版本号无意义递增。</summary>
  public ProjectileTargetComponent Retarget(EntityRuntimeHandle target)
  {
    if (!target.IsValid)
    {
      throw new ArgumentException("A projectile target handle is required.", nameof(target));
    }

    return target == Target ? this : new ProjectileTargetComponent(target, Revision + 1);
  }
}
