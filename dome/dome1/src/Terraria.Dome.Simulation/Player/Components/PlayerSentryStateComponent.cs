using System;

namespace Terraria.Dome.Simulation.Player.Components;

public struct PlayerSentryStateComponent
{
  public const int DefaultMaximumTurrets = 1;

  public int MaximumTurrets { get; private set; }
  public int LastAppliedMaximumTurrets { get; private set; }
  public bool ReconcileRequested { get; private set; }
  private int _appliedEquipmentCapacityBonus;
  private int _appliedBuffCapacityBonus;
  private int _appliedArmorSetCapacityBonus;

  public PlayerSentryStateComponent()
  {
    MaximumTurrets = DefaultMaximumTurrets;
    LastAppliedMaximumTurrets = DefaultMaximumTurrets;
    _appliedEquipmentCapacityBonus = 0;
    _appliedBuffCapacityBonus = 0;
    _appliedArmorSetCapacityBonus = 0;
    ReconcileRequested = false;
  }

  public bool ShouldReconcile => ReconcileRequested ||
    MaximumTurrets != LastAppliedMaximumTurrets;

  public void SetMaximumTurrets(int maximumTurrets)
  {
    ArgumentOutOfRangeException.ThrowIfNegative(maximumTurrets);
    if (MaximumTurrets == maximumTurrets)
    {
      return;
    }

    MaximumTurrets = maximumTurrets;
    ReconcileRequested = true;
  }

  public void RequestReconcile()
  {
    ReconcileRequested = true;
  }

  public void MarkReconciled()
  {
    LastAppliedMaximumTurrets = MaximumTurrets;
    ReconcileRequested = false;
  }

  internal void SetEquipmentCapacityBonus(int capacityBonus)
  {
    ArgumentOutOfRangeException.ThrowIfNegative(capacityBonus);
    if (_appliedEquipmentCapacityBonus == capacityBonus)
    {
      return;
    }

    _appliedEquipmentCapacityBonus = capacityBonus;
    RecalculateDerivedMaximumTurrets();
  }

  internal void SetBuffCapacityBonus(int capacityBonus)
  {
    ArgumentOutOfRangeException.ThrowIfNegative(capacityBonus);
    if (_appliedBuffCapacityBonus == capacityBonus)
    {
      return;
    }

    _appliedBuffCapacityBonus = capacityBonus;
    RecalculateDerivedMaximumTurrets();
  }

  internal void SetArmorSetCapacityBonus(int capacityBonus)
  {
    ArgumentOutOfRangeException.ThrowIfNegative(capacityBonus);
    if (_appliedArmorSetCapacityBonus == capacityBonus)
    {
      return;
    }

    _appliedArmorSetCapacityBonus = capacityBonus;
    RecalculateDerivedMaximumTurrets();
  }

  private void RecalculateDerivedMaximumTurrets()
  {
    SetMaximumTurrets(checked(
      DefaultMaximumTurrets + _appliedEquipmentCapacityBonus + _appliedBuffCapacityBonus +
      _appliedArmorSetCapacityBonus));
  }
}
