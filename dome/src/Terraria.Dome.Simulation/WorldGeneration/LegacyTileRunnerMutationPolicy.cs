namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed record LegacyTileRunnerMutationProfile(
  bool RemovesTile,
  bool WritesTileType,
  bool ActivatesTile,
  bool ClearsLiquid,
  bool WritesWall);

public static class LegacyTileRunnerMutationPolicy
{
  public static LegacyTileRunnerMutationProfile Classify(
    int tileType,
    bool addTile,
    bool noYChange)
  {
    bool removesTile = tileType < 0;
    bool writesTileType = !removesTile;
    bool activatesTile = writesTileType && addTile;
    bool clearsLiquid = activatesTile;
    bool writesWall = writesTileType && noYChange && tileType != 59;
    return new LegacyTileRunnerMutationProfile(
      removesTile,
      writesTileType,
      activatesTile,
      clearsLiquid,
      writesWall);
  }
}
