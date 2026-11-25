using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Terraria.Share.Relationships;

namespace Terraria.Share.Combat;

/// <summary>Stores per-attacker immunity windows owned by one target entity.</summary>
/// <remarks>
/// 对应参考端 NPC.immune[256]：按攻击者分槽，不分通道。参考端的 NPC 侧就是单值，
/// GetImmuneTime 只按 fromWho 写一格，没有通道维度；这里照抄这个不对称，
/// 因为「顺手纠正」等于造一个没有参考依据的维度。
/// <para>
/// 键用 EntityRuntimeHandle 而不是参考端的玩家下标：ECS 里没有稳定下标，句柄带 generation，
/// 槽位复用不会误命中。
/// </para>
/// <para>
/// 边界与 <see cref="ImmunityWindowComponent" /> 一致：授予于 N、时长 D 时 UntilTick=N+D，
/// 且 N..N+D-1 受保护、N+D 起放行，受保护 tick 数恰好是 D（参考端 NPC.immune 先递减再判定）。
/// </para>
/// <para>
/// 形状与 ProjectileHitImmunityStateComponent 一致（稀疏表 + Prune），并且它是一个组件，
/// 不是注入到系统里的可变队列。
/// </para>
/// </remarks>
public sealed class AttackerImmunityComponent
{
  private readonly List<AttackerImmunity> _windows = [];
  private readonly ReadOnlyCollection<AttackerImmunity> _readOnlyWindows;

  public AttackerImmunityComponent()
  {
    _readOnlyWindows = _windows.AsReadOnly();
  }

  /// <summary>Read-only view of the live windows, for diagnostics and assertions.</summary>
  public IReadOnlyList<AttackerImmunity> Windows => _readOnlyWindows;

  /// <summary>True when the attacker's window still covers the given tick.</summary>
  public bool IsActive(EntityRuntimeHandle attacker, long tick)
  {
    if (!attacker.IsValid || tick < 0)
    {
      return false;
    }

    for (int index = 0; index < _windows.Count; index++)
    {
      if (_windows[index].AppliesTo(attacker, tick))
      {
        return true;
      }
    }

    return false;
  }

  /// <summary>Extends one attacker's window. Shorter grants never shrink an existing window.</summary>
  public void Grant(EntityRuntimeHandle attacker, long tick, int durationTicks)
  {
    if (!attacker.IsValid)
    {
      throw new ArgumentException("An attacker handle is required.", nameof(attacker));
    }

    if (tick < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(tick));
    }

    if (durationTicks < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(durationTicks));
    }

    if (durationTicks == 0)
    {
      return;
    }

    long until = checked(tick + durationTicks);
    for (int index = 0; index < _windows.Count; index++)
    {
      if (_windows[index].Attacker.Matches(attacker))
      {
        if (until > _windows[index].UntilTick)
        {
          _windows[index] = new AttackerImmunity(attacker, until);
        }

        return;
      }
    }

    _windows.Add(new AttackerImmunity(attacker, until));
  }

  /// <summary>Drops every window that no longer covers the given tick.</summary>
  /// <returns>Number of removed windows.</returns>
  public int Prune(long tick)
  {
    if (tick < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(tick));
    }

    return _windows.RemoveAll(window => window.UntilTick <= tick);
  }
}

/// <summary>One generation-scoped attacker immunity window.</summary>
public readonly record struct AttackerImmunity
{
  public AttackerImmunity(EntityRuntimeHandle attacker, long untilTick)
  {
    if (!attacker.IsValid)
    {
      throw new ArgumentException("An attacker handle is required.", nameof(attacker));
    }

    if (untilTick < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(untilTick));
    }

    Attacker = attacker;
    UntilTick = untilTick;
  }

  public EntityRuntimeHandle Attacker { get; }

  public long UntilTick { get; }

  public bool AppliesTo(EntityRuntimeHandle attacker, long tick)
  {
    return Attacker.Matches(attacker) && tick < UntilTick;
  }
}
