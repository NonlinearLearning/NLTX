namespace Terraria.WorldGeneration.Definitions;

public readonly record struct WorldSecretSeedTerrainAndStructureRulesSelection(
  bool ExtraLivingTrees,
  bool ExtraFloatingIslands,
  bool BiggerAbandonedHouses,
  bool AddTeleporters,
  bool NoSpiderCaves,
  bool ActuallyNoTraps,
  bool DigExtraHoles,
  bool RoundLandmasses,
  bool ExtraLiquid,
  bool PortalGunInChests,
  bool DualDungeons);
