using System;

namespace Terraria.Npc;

/// <summary>
/// 保存 NPC 当前生命值和生命上限。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.NPC。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/NPC.cs。</para>
/// <para>主要源成员：life（第 6341 行）； lifeMax（第 6343 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-1/design/。</para>
/// <para>拆分依据文件：2026-09-05-version4-npc-and-town-simulation-component-design.md。</para>
/// <para>依据位置：第 477 行。</para>
/// </remarks>
public sealed class NpcHealthComponent
{
  public NpcHealthComponent(int currentLife, int maximumLife)
  {
    if (maximumLife < 0)
    {
      throw new ArgumentOutOfRangeException(
        nameof(maximumLife),
        "Maximum NPC life cannot be negative.");
    }

    if (currentLife < 0 || currentLife > maximumLife)
    {
      throw new ArgumentOutOfRangeException(
        nameof(currentLife),
        "Current NPC life must be within the NPC health bounds.");
    }

    _currentLife = currentLife;
    _maximumLife = maximumLife;
  }

  private int _currentLife;

  private int _maximumLife;

  public int CurrentLife => _currentLife;

  public int MaximumLife => _maximumLife;

  public bool IsDead => CurrentLife <= 0;

  internal bool TryApplyDamage(int requestedDamage, out int appliedDamage)
  {
    if (requestedDamage <= 0 || IsDead)
    {
      appliedDamage = 0;
      return false;
    }

    appliedDamage = Math.Min(requestedDamage, _currentLife);
    _currentLife -= appliedDamage;
    return true;
  }

  internal int ApplyHealing(int requestedHealing)
  {
    if (requestedHealing <= 0)
    {
      return 0;
    }

    int appliedHealing = Math.Min(requestedHealing, _maximumLife - _currentLife);
    _currentLife += appliedHealing;
    return appliedHealing;
  }

  internal bool TryApplyDamageOverTime(
    int requestedDamage,
    out int appliedDamage,
    out bool requiresForcedDeathStrike)
  {
    if (requestedDamage <= 0 || IsDead)
    {
      appliedDamage = 0;
      requiresForcedDeathStrike = false;
      return false;
    }

    appliedDamage = requestedDamage;
    int lifeAfterDirectDamage = _currentLife - requestedDamage;
    if (lifeAfterDirectDamage <= 0)
    {
      _currentLife = 1;
      requiresForcedDeathStrike = true;
    }
    else
    {
      _currentLife = lifeAfterDirectDamage;
      requiresForcedDeathStrike = false;
    }

    return true;
  }

  internal void RestoreToMaximumLife()
  {
    _currentLife = _maximumLife;
  }

  internal void SynchronizeFromParent(NpcHealthComponent parentHealth)
  {
    ArgumentNullException.ThrowIfNull(parentHealth);
    if (ReferenceEquals(this, parentHealth))
    {
      throw new ArgumentException(
        "An NPC health component cannot mirror itself.",
        nameof(parentHealth));
    }

    _currentLife = parentHealth.CurrentLife;
    _maximumLife = parentHealth.MaximumLife;
  }
}
