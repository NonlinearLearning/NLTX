namespace Terraria.Npc;

public readonly record struct NpcSpawnAcceptedCandidate(
  int PlayerIndex,
  NpcSpawnRateInputs RateInputs,
  NpcSpawnRateResult RateResult,
  NpcSpawnTileSearchResult TileSearchResult,
  NpcSpawnPostCheckInputs PostCheckInputs,
  NpcSpawnChosenTileFlagsResult ChosenTileFlags)
{
  public bool NoWormsForSpawn => ChosenTileFlags.NoWorms;
}
