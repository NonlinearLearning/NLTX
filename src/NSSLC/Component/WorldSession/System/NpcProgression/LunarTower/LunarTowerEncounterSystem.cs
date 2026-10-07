using System;

namespace Terraria.WorldSession.NpcProgression.LunarTower;

public sealed class LunarTowerEncounterSystem
{
  private readonly LunarTowerEncounterStateComponent _state;

  public LunarTowerEncounterSystem(LunarTowerEncounterStateComponent state)
  {
    _state = state ?? throw new ArgumentNullException(nameof(state));
  }

  public void BeginApocalypse()
  {
    SetAllTowersActive(true);
    SetAllShields(_state.LunarShieldPowerNormal);
    _state.SetApocalypseActive(true);
  }

  public int ApplyShieldDamage(LunarTowerKind towerKind, int damage = 1)
  {
    return _state.ApplyShieldDamage(towerKind, damage);
  }

  public void SetTowerActive(LunarTowerKind towerKind, bool isActive)
  {
    _state.SetTowerActive(towerKind, isActive);
  }

  public LunarTowerApocalypseRecomputeResult RecomputeApocalypse(
    LunarTowerPresenceSnapshot presence,
    int impendingDoomCountdownTime)
  {
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(impendingDoomCountdownTime);
    bool wasApocalypseActive = _state.LunarApocalypseIsUp;
    if (!wasApocalypseActive)
    {
      return new LunarTowerApocalypseRecomputeResult(
        false,
        false,
        false,
        0);
    }

    bool deactivatedMissingTowers = false;
    deactivatedMissingTowers |= DeactivateIfMissing(LunarTowerKind.Solar, presence.SolarActive);
    deactivatedMissingTowers |= DeactivateIfMissing(LunarTowerKind.Vortex, presence.VortexActive);
    deactivatedMissingTowers |= DeactivateIfMissing(LunarTowerKind.Nebula, presence.NebulaActive);
    deactivatedMissingTowers |= DeactivateIfMissing(
      LunarTowerKind.Stardust,
      presence.StardustActive);

    if (HasActiveTower() || presence.MoonLordActive)
    {
      return new LunarTowerApocalypseRecomputeResult(
        true,
        deactivatedMissingTowers,
        false,
        0);
    }

    _state.SetApocalypseActive(false);
    return new LunarTowerApocalypseRecomputeResult(
      true,
      deactivatedMissingTowers,
      true,
      impendingDoomCountdownTime);
  }

  public void Reset()
  {
    _state.Reset();
  }

  private bool DeactivateIfMissing(LunarTowerKind towerKind, bool isPresent)
  {
    if (isPresent || !_state.IsTowerActive(towerKind))
    {
      return false;
    }

    _state.SetTowerActive(towerKind, false);
    return true;
  }

  private bool HasActiveTower()
  {
    return _state.TowerActiveSolar
      || _state.TowerActiveVortex
      || _state.TowerActiveNebula
      || _state.TowerActiveStardust;
  }

  private void SetAllShields(int shieldStrength)
  {
    _state.SetShieldStrength(LunarTowerKind.Solar, shieldStrength);
    _state.SetShieldStrength(LunarTowerKind.Vortex, shieldStrength);
    _state.SetShieldStrength(LunarTowerKind.Nebula, shieldStrength);
    _state.SetShieldStrength(LunarTowerKind.Stardust, shieldStrength);
  }

  private void SetAllTowersActive(bool isActive)
  {
    _state.SetTowerActive(LunarTowerKind.Solar, isActive);
    _state.SetTowerActive(LunarTowerKind.Vortex, isActive);
    _state.SetTowerActive(LunarTowerKind.Nebula, isActive);
    _state.SetTowerActive(LunarTowerKind.Stardust, isActive);
  }
}
