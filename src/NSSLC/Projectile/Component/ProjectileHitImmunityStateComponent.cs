using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Terraria.Share.Relationships;

namespace Terraria.Unshared.Projectiles;

/// <summary>
/// Declares the projectile hit-immunity capability and stores its target-specific windows.
/// </summary>
public sealed class ProjectileHitImmunityStateComponent
{
  private readonly List<ProjectileHitImmunity> _immunities = [];
  private readonly ReadOnlyCollection<ProjectileHitImmunity> _readOnlyImmunities;

  public ProjectileHitImmunityStateComponent()
  {
    _readOnlyImmunities = _immunities.AsReadOnly();
  }

  public IReadOnlyList<ProjectileHitImmunity> Immunities => _readOnlyImmunities;

  /// <summary>True when the target's recorded window still covers the given tick.</summary>
  /// <remarks>
  /// 边界与 <see cref="Terraria.Share.Combat.ImmunityWindowComponent" /> 一致：授予于 N、时长 D，
  /// AppliesTo 在 N..N+D-1 为真，N+D 起放行。参考端 localNPCImmunity 是递减计数，
  /// 授予后先递减再判定，受保护 tick 数恰好等于配置值。
  /// </remarks>
  public bool AppliesTo(EntityRuntimeHandle target, long tick)
  {
    if (!target.IsValid || tick < 0)
    {
      return false;
    }

    for (int index = 0; index < _immunities.Count; index++)
    {
      ProjectileHitImmunity immunity = _immunities[index];
      if (immunity.AppliesTo(target, tick))
      {
        return true;
      }
    }

    return false;
  }

  public void Record(EntityRuntimeHandle target, long currentTick, long untilTick)
  {
    if (!target.IsValid)
    {
      throw new ArgumentException("A hit immunity target handle is required.", nameof(target));
    }

    if (currentTick < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(currentTick));
    }

    if (untilTick < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(untilTick));
    }

    Prune(currentTick);
    ProjectileHitImmunity immunity = new(target, untilTick);
    for (int index = 0; index < _immunities.Count; index++)
    {
      if (_immunities[index].Target.Matches(target))
      {
        _immunities[index] = immunity;
        return;
      }
    }

    _immunities.Add(immunity);
  }

  public int Prune(long tick)
  {
    if (tick < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(tick));
    }

    return _immunities.RemoveAll(immunity => immunity.UntilTick <= tick);
  }
}

/// <summary>One generation-scoped target immunity window owned by a projectile.</summary>
public readonly record struct ProjectileHitImmunity
{
  public ProjectileHitImmunity(EntityRuntimeHandle target, long untilTick)
  {
    if (!target.IsValid)
    {
      throw new ArgumentException("A hit immunity target handle is required.", nameof(target));
    }

    if (untilTick < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(untilTick));
    }

    Target = target;
    UntilTick = untilTick;
  }

  public EntityRuntimeHandle Target { get; }

  public long UntilTick { get; }

  public bool AppliesTo(EntityRuntimeHandle target, long tick)
  {
    return Target.Matches(target) && tick < UntilTick;
  }
}
