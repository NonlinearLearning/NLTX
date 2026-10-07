using System;

namespace Terraria.WorldSession.NpcProgression.LunarTower;

/// <summary>
/// 保存天界塔活动、护盾强度和天界入侵状态。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.NPC。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/NPC.cs。</para>
/// <para>
/// 主要源成员：ShieldStrengthTowerSolar（第 6265 行）； ShieldStrengthTowerVortex（第 6267 行）；
/// ShieldStrengthTowerNebula（第 6269 行）； ShieldStrengthTowerStardust（第 6271 行）；
/// LunarShieldPowerNormal（第 6273 行）； TowerActiveSolar（第 6275 行）； TowerActiveVortex（第 6277 行）；
/// TowerActiveNebula（第 6279 行）； TowerActiveStardust（第 6281 行）； LunarApocalypseIsUp（第 6283 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-p13-npc-town-progression-component-design.md。</para>
/// <para>依据位置：第 135 行。</para>
/// </remarks>
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
