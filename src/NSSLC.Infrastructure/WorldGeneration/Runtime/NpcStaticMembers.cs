namespace NSSLC.WorldGeneration;

public partial class NPC
{
  public static bool boughtBunny;
  public static bool boughtCat;
  public static bool boughtDog;
  private static bool EoCKilledToday;
  private static bool WoFKilledToday;
  public static bool[] npcsFoundForCheckActive = new bool[NPCID.Count];
  public static void ClearFoundActiveNPCs()
  {
    System.Array.Clear(npcsFoundForCheckActive, 0, npcsFoundForCheckActive.Length);
  }
  public static bool combatBookVolumeTwoWasUsed;
  public static bool combatBookWasUsed;
  public static bool downedAncientCultist;
  public static bool downedBoss1;
  public static bool downedBoss2;
  public static bool downedBoss3;
  public static bool downedChristmasIceQueen;
  public static bool downedChristmasSantank;
  public static bool downedChristmasTree;
  public static bool downedClown;
  public static bool downedDeerclops;
  public static bool downedEmpressOfLight;
  public static bool downedFishron;
  public static bool downedFrost;
  public static bool downedGoblins;
  public static bool downedGolemBoss;
  public static bool downedHalloweenKing;
  public static bool downedHalloweenTree;
  public static bool downedMartians;
  public static bool downedMechBoss1;
  public static bool downedMechBoss2;
  public static bool downedMechBoss3;
  public static bool downedMechBossAny;
  public static bool downedMoonlord;
  public static bool downedPirates;
  public static bool downedPlantBoss;
  public static bool downedQueenBee;
  public static bool downedQueenSlime;
  public static bool downedSlimeKing;
  public static bool downedTowerNebula;
  public static bool downedTowerSolar;
  public static bool downedTowerStardust;
  public static bool downedTowerVortex;
  public static int[,] cavernMonsterType = new int[2, 3];
  public static int butterflyChance;
  public static int fireFlyChance;
  public static int fireFlyFriendly;
  public static int fireFlyMultiple;
  public static bool freeCake;
  public static dynamic GetAvailableAmountOfNPCsToSpawnUpToSlot(params dynamic[] arguments) => default;
  public static int goldCritterChance;
  public static dynamic KickOutLookForHomeTimeout = default;
  public static bool LunarApocalypseIsUp;
  public static int MaxMoonLordCountdown;
  public static int mechQueen;
  public static int MoonLordCountdown;
  public static bool peddlersSatchelWasUsed;
  public static void ResetBadgerHatTime()
  {
    EoCKilledToday = false;
    WoFKilledToday = false;
  }
  public static dynamic RevengeManager = default;
  public static int safeRangeX;
  public static int safeRangeY;
  public static bool savedAngler;
  public static bool savedBartender;
  public static bool savedGoblin;
  public static bool savedGolfer;
  public static bool savedMech;
  public static bool savedStylist;
  public static bool savedTaxCollector;
  public static bool savedWizard;
  public static int sHeight;
  public static int ShieldStrengthTowerMax;
  public static int ShieldStrengthTowerNebula;
  public static int ShieldStrengthTowerSolar;
  public static int ShieldStrengthTowerStardust;
  public static int ShieldStrengthTowerVortex;
  public static bool[] ShimmeredTownNPCs = new bool[NPCID.Count];
  public static int[] spawnSlotProtected = new int[200];
  public static int sWidth;
  public static int stinkBugChance;
  public static bool TowerActiveNebula;
  public static bool TowerActiveSolar;
  public static bool TowerActiveStardust;
  public static bool TowerActiveVortex;
  public static bool unlockedArmsDealerSpawn;
  public static bool unlockedDemolitionistSpawn;
  public static bool unlockedDyeTraderSpawn;
  public static bool unlockedMerchantSpawn;
  public static bool unlockedNurseSpawn;
  public static bool unlockedPartyGirlSpawn;
  public static bool unlockedPrincessSpawn;
  public static bool unlockedSlimeBlueSpawn;
  public static bool unlockedSlimeCopperSpawn;
  public static bool unlockedSlimeGreenSpawn;
  public static bool unlockedSlimeOldSpawn;
  public static bool unlockedSlimePurpleSpawn;
  public static bool unlockedSlimeRainbowSpawn;
  public static bool unlockedSlimeRedSpawn;
  public static bool unlockedSlimeYellowSpawn;
  public static bool unlockedTruffleSpawn;
  public static float waveKills;
}
