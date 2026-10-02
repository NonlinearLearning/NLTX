using System;

namespace Terraria.WorldSession.NpcProgression.LunarTower;

public sealed class LunarTowerEncounterStateComponent
{
  private readonly int _lunarShieldPowerNormal;

  public LunarTowerEncounterStateComponent(int lunarShieldPowerNormal = 100)
  {
    ArgumentOutOfRangeException.ThrowIfNegative(lunarShieldPowerNormal);
    _lunarShieldPowerNormal = lunarShieldPowerNormal;
  }

  public int LunarShieldPowerNormal => _lunarShieldPowerNormal;

  public int ShieldStrengthTowerSolar { get; private set; }

  public int ShieldStrengthTowerVortex { get; private set; }

  public int ShieldStrengthTowerNebula { get; private set; }

  public int ShieldStrengthTowerStardust { get; private set; }

  public bool TowerActiveSolar { get; private set; }

  public bool TowerActiveVortex { get; private set; }

  public bool TowerActiveNebula { get; private set; }

  public bool TowerActiveStardust { get; private set; }

  public bool LunarApocalypseIsUp { get; private set; }

  public int GetShieldStrength(LunarTowerKind towerKind)
  {
    return towerKind switch
    {
      LunarTowerKind.Solar => ShieldStrengthTowerSolar,
      LunarTowerKind.Vortex => ShieldStrengthTowerVortex,
      LunarTowerKind.Nebula => ShieldStrengthTowerNebula,
      LunarTowerKind.Stardust => ShieldStrengthTowerStardust,
      _ => throw new ArgumentOutOfRangeException(nameof(towerKind), towerKind, "Unknown lunar tower kind."),
    };
  }

  public bool IsTowerActive(LunarTowerKind towerKind)
  {
    return towerKind switch
    {
      LunarTowerKind.Solar => TowerActiveSolar,
      LunarTowerKind.Vortex => TowerActiveVortex,
      LunarTowerKind.Nebula => TowerActiveNebula,
      LunarTowerKind.Stardust => TowerActiveStardust,
      _ => throw new ArgumentOutOfRangeException(nameof(towerKind), towerKind, "Unknown lunar tower kind."),
    };
  }

  public void SetShieldStrength(LunarTowerKind towerKind, int shieldStrength)
  {
    ValidateShieldStrength(shieldStrength);

    switch (towerKind)
    {
      case LunarTowerKind.Solar:
        ShieldStrengthTowerSolar = shieldStrength;
        break;
      case LunarTowerKind.Vortex:
        ShieldStrengthTowerVortex = shieldStrength;
        break;
      case LunarTowerKind.Nebula:
        ShieldStrengthTowerNebula = shieldStrength;
        break;
      case LunarTowerKind.Stardust:
        ShieldStrengthTowerStardust = shieldStrength;
        break;
      default:
        throw new ArgumentOutOfRangeException(nameof(towerKind), towerKind, "Unknown lunar tower kind.");
    }
  }

  public int ApplyShieldDamage(LunarTowerKind towerKind, int damage = 1)
  {
    ArgumentOutOfRangeException.ThrowIfNegative(damage);
    var remainingShield = Math.Max(0, GetShieldStrength(towerKind) - damage);
    SetShieldStrength(towerKind, remainingShield);
    return remainingShield;
  }

  public void SetTowerActive(LunarTowerKind towerKind, bool isActive)
  {
    switch (towerKind)
    {
      case LunarTowerKind.Solar:
        TowerActiveSolar = isActive;
        break;
      case LunarTowerKind.Vortex:
        TowerActiveVortex = isActive;
        break;
      case LunarTowerKind.Nebula:
        TowerActiveNebula = isActive;
        break;
      case LunarTowerKind.Stardust:
        TowerActiveStardust = isActive;
        break;
      default:
        throw new ArgumentOutOfRangeException(nameof(towerKind), towerKind, "Unknown lunar tower kind.");
    }
  }

  public void SetApocalypseActive(bool isActive)
  {
    LunarApocalypseIsUp = isActive;
  }

  public void Reset()
  {
    ShieldStrengthTowerSolar = 0;
    ShieldStrengthTowerVortex = 0;
    ShieldStrengthTowerNebula = 0;
    ShieldStrengthTowerStardust = 0;
    TowerActiveSolar = false;
    TowerActiveVortex = false;
    TowerActiveNebula = false;
    TowerActiveStardust = false;
    LunarApocalypseIsUp = false;
  }

  private void ValidateShieldStrength(int shieldStrength)
  {
    if (shieldStrength < 0 || shieldStrength > _lunarShieldPowerNormal)
    {
      throw new ArgumentOutOfRangeException(
        nameof(shieldStrength),
        "Tower shield strength must remain within the configured normal shield range.");
    }
  }
}
