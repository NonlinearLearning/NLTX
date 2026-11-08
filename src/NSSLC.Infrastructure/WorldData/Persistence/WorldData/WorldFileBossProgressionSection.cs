namespace Terraria.NonAuthoritative.Persistence;

/// <summary>
/// The versioned boss progression prefix that follows banner counts in a WorldFile header.
/// </summary>
/// <remarks>
/// Older files omit some or all of these values. The decoder supplies false defaults for omitted
/// fields, including the Lunar Tower values introduced in WorldFile version 140, and an owner API
/// can apply its own progression invariants later.
/// </remarks>
public sealed class WorldFileBossProgressionSection
{
  public const string SectionId = "world.boss-progression";

  public WorldFileBossProgressionSection(
    bool fastForwardTimeToDawn,
    bool downedFishron,
    bool downedMartians,
    bool downedAncientCultist,
    bool downedMoonlord,
    bool downedHalloweenKing,
    bool downedHalloweenTree,
    bool downedChristmasIceQueen,
    bool downedChristmasSantank,
    bool downedChristmasTree,
    bool downedTowerSolar = false,
    bool downedTowerVortex = false,
    bool downedTowerNebula = false,
    bool downedTowerStardust = false,
    bool towerActiveSolar = false,
    bool towerActiveVortex = false,
    bool towerActiveNebula = false,
    bool towerActiveStardust = false,
    bool lunarApocalypseIsUp = false)
  {
    FastForwardTimeToDawn = fastForwardTimeToDawn;
    DownedFishron = downedFishron;
    DownedMartians = downedMartians;
    DownedAncientCultist = downedAncientCultist;
    DownedMoonlord = downedMoonlord;
    DownedHalloweenKing = downedHalloweenKing;
    DownedHalloweenTree = downedHalloweenTree;
    DownedChristmasIceQueen = downedChristmasIceQueen;
    DownedChristmasSantank = downedChristmasSantank;
    DownedChristmasTree = downedChristmasTree;
    DownedTowerSolar = downedTowerSolar;
    DownedTowerVortex = downedTowerVortex;
    DownedTowerNebula = downedTowerNebula;
    DownedTowerStardust = downedTowerStardust;
    TowerActiveSolar = towerActiveSolar;
    TowerActiveVortex = towerActiveVortex;
    TowerActiveNebula = towerActiveNebula;
    TowerActiveStardust = towerActiveStardust;
    LunarApocalypseIsUp = lunarApocalypseIsUp;
  }

  public bool FastForwardTimeToDawn { get; }

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

  public static WorldFileBossProgressionSection Empty => new(
    fastForwardTimeToDawn: false,
    downedFishron: false,
    downedMartians: false,
    downedAncientCultist: false,
    downedMoonlord: false,
    downedHalloweenKing: false,
    downedHalloweenTree: false,
    downedChristmasIceQueen: false,
    downedChristmasSantank: false,
    downedChristmasTree: false);
}
