using System;

namespace Terraria.Town.Progression.Spawn;

/// <summary>
/// 保存城镇 NPC 与城镇史莱姆的生成解锁标记。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.NPC。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/NPC.cs。</para>
/// <para>
/// 主要源成员：unlockedSlimeBlueSpawn（第 6173 行）； unlockedSlimeGreenSpawn（第 6175 行）；
/// unlockedSlimeOldSpawn（第 6177 行）； unlockedSlimePurpleSpawn（第 6179 行）；
/// unlockedSlimeRainbowSpawn（第 6181 行）； unlockedSlimeRedSpawn（第 6183 行）；
/// unlockedSlimeYellowSpawn（第 6185 行）； unlockedSlimeCopperSpawn（第 6187 行）；
/// unlockedMerchantSpawn（第 6189 行）； unlockedDemolitionistSpawn（第 6191 行）；
/// unlockedPartyGirlSpawn（第 6193 行）； unlockedDyeTraderSpawn（第 6195 行）； unlockedTruffleSpawn（第
/// 6197 行）； unlockedArmsDealerSpawn（第 6199 行）； unlockedNurseSpawn（第 6201 行）；
/// unlockedPrincessSpawn（第 6203 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-p13-npc-town-progression-component-design.md。</para>
/// <para>依据位置：第 132 行。</para>
/// </remarks>
public sealed class TownSpawnUnlockStateComponent
{
  public bool SlimeBlueUnlocked { get; private set; }

  public bool SlimeGreenUnlocked { get; private set; }

  public bool SlimeOldUnlocked { get; private set; }

  public bool SlimePurpleUnlocked { get; private set; }

  public bool SlimeRainbowUnlocked { get; private set; }

  public bool SlimeRedUnlocked { get; private set; }

  public bool SlimeYellowUnlocked { get; private set; }

  public bool SlimeCopperUnlocked { get; private set; }

  public bool MerchantUnlocked { get; private set; }

  public bool DemolitionistUnlocked { get; private set; }

  public bool PartyGirlUnlocked { get; private set; }

  public bool DyeTraderUnlocked { get; private set; }

  public bool TruffleUnlocked { get; private set; }

  public bool ArmsDealerUnlocked { get; private set; }

  public bool NurseUnlocked { get; private set; }

  public bool PrincessUnlocked { get; private set; }

  public bool Unlock(TownSpawnUnlockKind unlockKind)
  {
    if (IsUnlocked(unlockKind))
    {
      return false;
    }

    switch (unlockKind)
    {
      case TownSpawnUnlockKind.SlimeBlue:
        SlimeBlueUnlocked = true;
        break;
      case TownSpawnUnlockKind.SlimeGreen:
        SlimeGreenUnlocked = true;
        break;
      case TownSpawnUnlockKind.SlimeOld:
        SlimeOldUnlocked = true;
        break;
      case TownSpawnUnlockKind.SlimePurple:
        SlimePurpleUnlocked = true;
        break;
      case TownSpawnUnlockKind.SlimeRainbow:
        SlimeRainbowUnlocked = true;
        break;
      case TownSpawnUnlockKind.SlimeRed:
        SlimeRedUnlocked = true;
        break;
      case TownSpawnUnlockKind.SlimeYellow:
        SlimeYellowUnlocked = true;
        break;
      case TownSpawnUnlockKind.SlimeCopper:
        SlimeCopperUnlocked = true;
        break;
      case TownSpawnUnlockKind.Merchant:
        MerchantUnlocked = true;
        break;
      case TownSpawnUnlockKind.Demolitionist:
        DemolitionistUnlocked = true;
        break;
      case TownSpawnUnlockKind.PartyGirl:
        PartyGirlUnlocked = true;
        break;
      case TownSpawnUnlockKind.DyeTrader:
        DyeTraderUnlocked = true;
        break;
      case TownSpawnUnlockKind.Truffle:
        TruffleUnlocked = true;
        break;
      case TownSpawnUnlockKind.ArmsDealer:
        ArmsDealerUnlocked = true;
        break;
      case TownSpawnUnlockKind.Nurse:
        NurseUnlocked = true;
        break;
      case TownSpawnUnlockKind.Princess:
        PrincessUnlocked = true;
        break;
      default:
        throw new ArgumentOutOfRangeException(nameof(unlockKind), unlockKind, "Unknown town spawn unlock kind.");
    }

    return true;
  }

  public bool IsUnlocked(TownSpawnUnlockKind unlockKind)
  {
    return unlockKind switch
    {
      TownSpawnUnlockKind.SlimeBlue => SlimeBlueUnlocked,
      TownSpawnUnlockKind.SlimeGreen => SlimeGreenUnlocked,
      TownSpawnUnlockKind.SlimeOld => SlimeOldUnlocked,
      TownSpawnUnlockKind.SlimePurple => SlimePurpleUnlocked,
      TownSpawnUnlockKind.SlimeRainbow => SlimeRainbowUnlocked,
      TownSpawnUnlockKind.SlimeRed => SlimeRedUnlocked,
      TownSpawnUnlockKind.SlimeYellow => SlimeYellowUnlocked,
      TownSpawnUnlockKind.SlimeCopper => SlimeCopperUnlocked,
      TownSpawnUnlockKind.Merchant => MerchantUnlocked,
      TownSpawnUnlockKind.Demolitionist => DemolitionistUnlocked,
      TownSpawnUnlockKind.PartyGirl => PartyGirlUnlocked,
      TownSpawnUnlockKind.DyeTrader => DyeTraderUnlocked,
      TownSpawnUnlockKind.Truffle => TruffleUnlocked,
      TownSpawnUnlockKind.ArmsDealer => ArmsDealerUnlocked,
      TownSpawnUnlockKind.Nurse => NurseUnlocked,
      TownSpawnUnlockKind.Princess => PrincessUnlocked,
      _ => throw new ArgumentOutOfRangeException(nameof(unlockKind), unlockKind, "Unknown town spawn unlock kind."),
    };
  }

  public void Reset()
  {
    SlimeBlueUnlocked = false;
    SlimeGreenUnlocked = false;
    SlimeOldUnlocked = false;
    SlimePurpleUnlocked = false;
    SlimeRainbowUnlocked = false;
    SlimeRedUnlocked = false;
    SlimeYellowUnlocked = false;
    SlimeCopperUnlocked = false;
    MerchantUnlocked = false;
    DemolitionistUnlocked = false;
    PartyGirlUnlocked = false;
    DyeTraderUnlocked = false;
    TruffleUnlocked = false;
    ArmsDealerUnlocked = false;
    NurseUnlocked = false;
    PrincessUnlocked = false;
  }
}
