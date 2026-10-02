namespace Terraria.Npc;

public readonly record struct NpcSpawnBranchSelectionInputs(
  NpcSpawnAcceptedCandidate Candidate,
  NpcSpawnTowerSelectionInputs Tower,
  NpcSpawnSkyMobSelectionInputs SkyMob,
  NpcSpawnInvasionSelectionInputs Invasion,
  NpcSpawnLegacyWallClassificationFacts LegacyWallFacts,
  NpcSpawnCritterSelectionInputs Critter,
  NpcSpawnEarlierLegacyBranchDisposition EarlierUnmodeledBranches);
