using System;
using System.Collections.Generic;
using System.Linq;

namespace NSSLC.WorldGeneration;

/// <summary>
/// Metadata captured while creation owns the generation lock; no live global reads.
/// </summary>
public sealed class GeneratedWorldSettings {
  public int GameMode { get; }
  public byte MoonType { get; }
  public IReadOnlyList<int> TreeX { get; }
  public IReadOnlyList<int> TreeStyle { get; }
  public IReadOnlyList<int> CaveBackX { get; }
  public IReadOnlyList<int> CaveBackStyle { get; }
  public int IceBackStyle { get; }
  public int JungleBackStyle { get; }
  public int HellBackStyle { get; }
  public int DungeonX { get; }
  public int DungeonY { get; }
  public int MoonPhase { get; }
  public int RainTime { get; }
  public int InvasionDelay { get; }
  public int InvasionSize { get; }
  public int InvasionType { get; }
  public int InvasionSizeStart { get; }
  public int SundialCooldown { get; }
  public int MoondialCooldown { get; }
  public int CoinRain { get; }
  public double Time { get; }
  public bool DayTime { get; }
  public bool BloodMoon { get; }
  public bool Eclipse { get; }
  public bool Raining { get; }
  public bool HardMode { get; }
  public double InvasionX { get; }
  public double SlimeRainTime { get; }
  public float MaxRain { get; }
  public float WindSpeedTarget { get; }
  public int CloudBackgroundActive { get; }
  public short CloudCount { get; }
  public bool FastForwardTimeToDawn { get; }
  public bool FastForwardTimeToDusk { get; }
  public bool ForceHalloweenForToday { get; }
  public bool ForceChristmasForToday { get; }
  public bool ForceHalloweenForever { get; }
  public bool ForceChristmasForever { get; }
  public bool AfterPartyOfDoom { get; }
  public IReadOnlyList<byte> BackgroundStyles { get; }
  public IReadOnlyList<byte> AdditionalBackgroundStyles { get; }
  public IReadOnlyList<int> TreeTops { get; }
  public int CopperOreTier { get; }
  public int IronOreTier { get; }
  public int SilverOreTier { get; }
  public int GoldOreTier { get; }
  public int CobaltOreTier { get; }
  public int MythrilOreTier { get; }
  public int AdamantiteOreTier { get; }
  public bool ShadowOrbSmashed { get; }
  public bool SpawnMeteor { get; }
  public byte ShadowOrbCount { get; }
  public int AltarCount { get; }
  public bool DownedBoss1 { get; }
  public bool DownedBoss2 { get; }
  public bool DownedBoss3 { get; }
  public bool DownedQueenBee { get; }
  public bool DownedMechBoss1 { get; }
  public bool DownedMechBoss2 { get; }
  public bool DownedMechBoss3 { get; }
  public bool DownedMechBossAny { get; }
  public bool DownedPlantBoss { get; }
  public bool DownedGolemBoss { get; }
  public bool DownedSlimeKing { get; }
  public bool SavedGoblin { get; }
  public bool SavedWizard { get; }
  public bool SavedMech { get; }
  public bool DownedGoblins { get; }
  public bool DownedClown { get; }
  public bool DownedFrost { get; }
  public bool DownedPirates { get; }
  public bool SavedAngler { get; }
  public bool SavedStylist { get; }
  public bool SavedTaxCollector { get; }
  public bool SavedGolfer { get; }
  public bool SavedBartender { get; }
  public bool DownedFishron { get; }
  public bool DownedMartians { get; }
  public bool DownedAncientCultist { get; }
  public bool DownedMoonlord { get; }
  public bool DownedHalloweenKing { get; }
  public bool DownedHalloweenTree { get; }
  public bool DownedChristmasIceQueen { get; }
  public bool DownedChristmasSantank { get; }
  public bool DownedChristmasTree { get; }
  public bool DownedTowerSolar { get; }
  public bool DownedTowerVortex { get; }
  public bool DownedTowerNebula { get; }
  public bool DownedTowerStardust { get; }
  public bool TowerActiveSolar { get; }
  public bool TowerActiveVortex { get; }
  public bool TowerActiveNebula { get; }
  public bool TowerActiveStardust { get; }
  public bool LunarApocalypseIsUp { get; }
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
  public bool CombatBookWasUsed { get; }

  internal GeneratedWorldSettings() {
    GameMode = Main.GameMode;
    MoonType = checked((byte)Main.moonType);
    TreeX = Array.AsReadOnly(Main.treeX.Take(3).ToArray());
    TreeStyle = Array.AsReadOnly(Main.treeStyle.ToArray());
    CaveBackX = Array.AsReadOnly(Main.caveBackX.Take(3).ToArray());
    CaveBackStyle = Array.AsReadOnly(Main.caveBackStyle.ToArray());
    IceBackStyle = Main.iceBackStyle;
    JungleBackStyle = Main.jungleBackStyle;
    HellBackStyle = Main.hellBackStyle;
    DungeonX = Main.dungeonX;
    DungeonY = Main.dungeonY;
    MoonPhase = Main.moonPhase;
    RainTime = Main.rainTime;
    InvasionDelay = Main.invasionDelay;
    InvasionSize = Main.invasionSize;
    InvasionType = Main.invasionType;
    InvasionSizeStart = Main.invasionSizeStart;
    SundialCooldown = Main.sundialCooldown;
    MoondialCooldown = Main.moondialCooldown;
    CoinRain = Main.coinRain;
    Time = Main.time;
    DayTime = Main.dayTime;
    BloodMoon = Main.bloodMoon;
    Eclipse = Main.eclipse;
    Raining = Main.raining;
    HardMode = Main.hardMode;
    InvasionX = Main.invasionX;
    SlimeRainTime = Main.slimeRainTime;
    MaxRain = Main.maxRaining;
    WindSpeedTarget = Main.windSpeedTarget;
    CloudBackgroundActive = (int)Main.cloudBGActive;
    CloudCount = checked((short)Main.numClouds);
    FastForwardTimeToDawn = Main.fastForwardTimeToDawn;
    FastForwardTimeToDusk = Main.fastForwardTimeToDusk;
    ForceHalloweenForToday = Main.forceHalloweenForToday;
    ForceChristmasForToday = Main.forceXMasForToday;
    ForceHalloweenForever = Main.forceHalloweenForever;
    ForceChristmasForever = Main.forceXMasForever;
    AfterPartyOfDoom = Main.afterPartyOfDoom;
    BackgroundStyles = Array.AsReadOnly(new byte[] {
      checked((byte)WorldGen.treeBG1), checked((byte)WorldGen.corruptBG),
      checked((byte)WorldGen.jungleBG), checked((byte)WorldGen.snowBG),
      checked((byte)WorldGen.hallowBG), checked((byte)WorldGen.crimsonBG),
      checked((byte)WorldGen.desertBG), checked((byte)WorldGen.oceanBG)
    });
    AdditionalBackgroundStyles = Array.AsReadOnly(new byte[] {
      checked((byte)WorldGen.mushroomBG), checked((byte)WorldGen.underworldBG),
      checked((byte)WorldGen.treeBG2), checked((byte)WorldGen.treeBG3),
      checked((byte)WorldGen.treeBG4)
    });
    TreeTops = Array.AsReadOnly(Enumerable.Range(0, GameContent.TreeTopsInfo.AreaId.Count)
        .Select(WorldGen.TreeTops.GetTreeStyle).ToArray());
    CopperOreTier = WorldGen.SavedOreTiers.Copper;
    IronOreTier = WorldGen.SavedOreTiers.Iron;
    SilverOreTier = WorldGen.SavedOreTiers.Silver;
    GoldOreTier = WorldGen.SavedOreTiers.Gold;
    CobaltOreTier = WorldGen.SavedOreTiers.Cobalt;
    MythrilOreTier = WorldGen.SavedOreTiers.Mythril;
    AdamantiteOreTier = WorldGen.SavedOreTiers.Adamantite;
    ShadowOrbSmashed = WorldGen.shadowOrbSmashed;
    SpawnMeteor = WorldGen.spawnMeteor;
    ShadowOrbCount = checked((byte)WorldGen.shadowOrbCount);
    AltarCount = WorldGen.altarCount;
    DownedBoss1 = NPC.downedBoss1;
    DownedBoss2 = NPC.downedBoss2;
    DownedBoss3 = NPC.downedBoss3;
    DownedQueenBee = NPC.downedQueenBee;
    DownedMechBoss1 = NPC.downedMechBoss1;
    DownedMechBoss2 = NPC.downedMechBoss2;
    DownedMechBoss3 = NPC.downedMechBoss3;
    DownedMechBossAny = NPC.downedMechBossAny;
    DownedPlantBoss = NPC.downedPlantBoss;
    DownedGolemBoss = NPC.downedGolemBoss;
    DownedSlimeKing = NPC.downedSlimeKing;
    SavedGoblin = NPC.savedGoblin;
    SavedWizard = NPC.savedWizard;
    SavedMech = NPC.savedMech;
    DownedGoblins = NPC.downedGoblins;
    DownedClown = NPC.downedClown;
    DownedFrost = NPC.downedFrost;
    DownedPirates = NPC.downedPirates;
    SavedAngler = NPC.savedAngler;
    SavedStylist = NPC.savedStylist;
    SavedTaxCollector = NPC.savedTaxCollector;
    SavedGolfer = NPC.savedGolfer;
    SavedBartender = NPC.savedBartender;
    DownedFishron = NPC.downedFishron;
    DownedMartians = NPC.downedMartians;
    DownedAncientCultist = NPC.downedAncientCultist;
    DownedMoonlord = NPC.downedMoonlord;
    DownedHalloweenKing = NPC.downedHalloweenKing;
    DownedHalloweenTree = NPC.downedHalloweenTree;
    DownedChristmasIceQueen = NPC.downedChristmasIceQueen;
    DownedChristmasSantank = NPC.downedChristmasSantank;
    DownedChristmasTree = NPC.downedChristmasTree;
    DownedTowerSolar = NPC.downedTowerSolar;
    DownedTowerVortex = NPC.downedTowerVortex;
    DownedTowerNebula = NPC.downedTowerNebula;
    DownedTowerStardust = NPC.downedTowerStardust;
    TowerActiveSolar = NPC.TowerActiveSolar;
    TowerActiveVortex = NPC.TowerActiveVortex;
    TowerActiveNebula = NPC.TowerActiveNebula;
    TowerActiveStardust = NPC.TowerActiveStardust;
    LunarApocalypseIsUp = NPC.LunarApocalypseIsUp;
    BoughtCat = NPC.boughtCat;
    BoughtDog = NPC.boughtDog;
    BoughtBunny = NPC.boughtBunny;
    DownedEmpressOfLight = NPC.downedEmpressOfLight;
    DownedQueenSlime = NPC.downedQueenSlime;
    DownedDeerclops = NPC.downedDeerclops;
    UnlockedSlimeBlueSpawn = NPC.unlockedSlimeBlueSpawn;
    UnlockedMerchantSpawn = NPC.unlockedMerchantSpawn;
    UnlockedDemolitionistSpawn = NPC.unlockedDemolitionistSpawn;
    UnlockedPartyGirlSpawn = NPC.unlockedPartyGirlSpawn;
    UnlockedDyeTraderSpawn = NPC.unlockedDyeTraderSpawn;
    UnlockedTruffleSpawn = NPC.unlockedTruffleSpawn;
    UnlockedArmsDealerSpawn = NPC.unlockedArmsDealerSpawn;
    UnlockedNurseSpawn = NPC.unlockedNurseSpawn;
    UnlockedPrincessSpawn = NPC.unlockedPrincessSpawn;
    CombatBookVolumeTwoWasUsed = NPC.combatBookVolumeTwoWasUsed;
    PeddlersSatchelWasUsed = NPC.peddlersSatchelWasUsed;
    UnlockedSlimeGreenSpawn = NPC.unlockedSlimeGreenSpawn;
    UnlockedSlimeOldSpawn = NPC.unlockedSlimeOldSpawn;
    UnlockedSlimePurpleSpawn = NPC.unlockedSlimePurpleSpawn;
    UnlockedSlimeRainbowSpawn = NPC.unlockedSlimeRainbowSpawn;
    UnlockedSlimeRedSpawn = NPC.unlockedSlimeRedSpawn;
    UnlockedSlimeYellowSpawn = NPC.unlockedSlimeYellowSpawn;
    UnlockedSlimeCopperSpawn = NPC.unlockedSlimeCopperSpawn;
    CombatBookWasUsed = NPC.combatBookWasUsed;
  }
}
