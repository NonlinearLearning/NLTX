namespace Terraria.NonAuthoritative.Persistence;

/// <summary>
/// Versioned town-NPC, boss and slime unlock flags from a WorldFile continuation.
/// </summary>
public sealed class WorldFileNpcUnlockSection
{
  public const string SectionId = "world.npc-unlocks";

  public WorldFileNpcUnlockSection(
    bool boughtCat,
    bool boughtDog,
    bool boughtBunny,
    bool downedEmpressOfLight,
    bool downedQueenSlime,
    bool downedDeerclops,
    bool unlockedSlimeBlueSpawn,
    bool unlockedMerchantSpawn,
    bool unlockedDemolitionistSpawn,
    bool unlockedPartyGirlSpawn,
    bool unlockedDyeTraderSpawn,
    bool unlockedTruffleSpawn,
    bool unlockedArmsDealerSpawn,
    bool unlockedNurseSpawn,
    bool unlockedPrincessSpawn,
    bool combatBookVolumeTwoWasUsed,
    bool peddlersSatchelWasUsed,
    bool unlockedSlimeGreenSpawn,
    bool unlockedSlimeOldSpawn,
    bool unlockedSlimePurpleSpawn,
    bool unlockedSlimeRainbowSpawn,
    bool unlockedSlimeRedSpawn,
    bool unlockedSlimeYellowSpawn,
    bool unlockedSlimeCopperSpawn)
  {
    BoughtCat = boughtCat;
    BoughtDog = boughtDog;
    BoughtBunny = boughtBunny;
    DownedEmpressOfLight = downedEmpressOfLight;
    DownedQueenSlime = downedQueenSlime;
    DownedDeerclops = downedDeerclops;
    UnlockedSlimeBlueSpawn = unlockedSlimeBlueSpawn;
    UnlockedMerchantSpawn = unlockedMerchantSpawn;
    UnlockedDemolitionistSpawn = unlockedDemolitionistSpawn;
    UnlockedPartyGirlSpawn = unlockedPartyGirlSpawn;
    UnlockedDyeTraderSpawn = unlockedDyeTraderSpawn;
    UnlockedTruffleSpawn = unlockedTruffleSpawn;
    UnlockedArmsDealerSpawn = unlockedArmsDealerSpawn;
    UnlockedNurseSpawn = unlockedNurseSpawn;
    UnlockedPrincessSpawn = unlockedPrincessSpawn;
    CombatBookVolumeTwoWasUsed = combatBookVolumeTwoWasUsed;
    PeddlersSatchelWasUsed = peddlersSatchelWasUsed;
    UnlockedSlimeGreenSpawn = unlockedSlimeGreenSpawn;
    UnlockedSlimeOldSpawn = unlockedSlimeOldSpawn;
    UnlockedSlimePurpleSpawn = unlockedSlimePurpleSpawn;
    UnlockedSlimeRainbowSpawn = unlockedSlimeRainbowSpawn;
    UnlockedSlimeRedSpawn = unlockedSlimeRedSpawn;
    UnlockedSlimeYellowSpawn = unlockedSlimeYellowSpawn;
    UnlockedSlimeCopperSpawn = unlockedSlimeCopperSpawn;
  }

  public bool BoughtCat { get; }
  public bool BoughtDog { get; }
  public bool BoughtBunny { get; }
  public bool DownedEmpressOfLight { get; }
  public bool DownedQueenSlime { get; }
  public bool DownedDeerclops { get; }
  public bool UnlockedSlimeBlueSpawn { get; }
  public bool UnlockedMerchantSpawn { get; }
  public bool UnlockedDemolitionistSpawn { get; }
  public bool UnlockedPartyGirlSpawn { get; }
  public bool UnlockedDyeTraderSpawn { get; }
  public bool UnlockedTruffleSpawn { get; }
  public bool UnlockedArmsDealerSpawn { get; }
  public bool UnlockedNurseSpawn { get; }
  public bool UnlockedPrincessSpawn { get; }
  public bool CombatBookVolumeTwoWasUsed { get; }
  public bool PeddlersSatchelWasUsed { get; }
  public bool UnlockedSlimeGreenSpawn { get; }
  public bool UnlockedSlimeOldSpawn { get; }
  public bool UnlockedSlimePurpleSpawn { get; }
  public bool UnlockedSlimeRainbowSpawn { get; }
  public bool UnlockedSlimeRedSpawn { get; }
  public bool UnlockedSlimeYellowSpawn { get; }
  public bool UnlockedSlimeCopperSpawn { get; }

  public static WorldFileNpcUnlockSection Empty => new(
    boughtCat: false,
    boughtDog: false,
    boughtBunny: false,
    downedEmpressOfLight: false,
    downedQueenSlime: false,
    downedDeerclops: false,
    unlockedSlimeBlueSpawn: false,
    unlockedMerchantSpawn: false,
    unlockedDemolitionistSpawn: false,
    unlockedPartyGirlSpawn: false,
    unlockedDyeTraderSpawn: false,
    unlockedTruffleSpawn: false,
    unlockedArmsDealerSpawn: false,
    unlockedNurseSpawn: false,
    unlockedPrincessSpawn: false,
    combatBookVolumeTwoWasUsed: false,
    peddlersSatchelWasUsed: false,
    unlockedSlimeGreenSpawn: false,
    unlockedSlimeOldSpawn: false,
    unlockedSlimePurpleSpawn: false,
    unlockedSlimeRainbowSpawn: false,
    unlockedSlimeRedSpawn: false,
    unlockedSlimeYellowSpawn: false,
    unlockedSlimeCopperSpawn: false);
}
