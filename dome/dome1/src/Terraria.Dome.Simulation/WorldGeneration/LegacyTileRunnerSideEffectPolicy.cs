namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed record LegacyTileRunnerSideEffectProfile(
  bool InjectsLiquidBeforeClear,
  short InjectedLiquidType,
  bool SuppressedByRemixOceanDepth,
  bool SetsLava,
  bool ClearsActiveTile,
  bool ClearsLiquidOnActivation,
  bool ClearsLiquidForType59,
  bool ClearsLavaOnActivation,
  bool WritesSurfaceWall);

public static class LegacyTileRunnerSideEffectPolicy
{
  public static LegacyTileRunnerSideEffectProfile Classify(
    int tileType,
    bool addTile,
    bool noYChange,
    bool tileIsActive,
    int tileY,
    int waterLine,
    int lavaLine,
    double worldSurface)
  {
    return Classify(
      tileType,
      addTile,
      noYChange,
      tileIsActive,
      tileY,
      waterLine,
      lavaLine,
      worldSurface,
      liquidType: 0,
      remixWorld: false,
      rockLayer: 0,
      maxTilesY: 0,
      isOceanDepth: false);
  }

  public static LegacyTileRunnerSideEffectProfile Classify(
    int tileType,
    bool addTile,
    bool noYChange,
    bool tileIsActive,
    int tileY,
    int waterLine,
    int lavaLine,
    double worldSurface,
    short liquidType,
    bool remixWorld,
    int rockLayer,
    int maxTilesY,
    bool isOceanDepth,
    byte currentLiquidAmount = 0)
  {
    bool suppressedByRemixOceanDepth = tileType == -2 && remixWorld &&
      tileY > lavaLine &&
      (tileY < rockLayer - 80 || tileY > maxTilesY - 350) && isOceanDepth;
    bool injectsLiquid = tileType == -2 && tileIsActive &&
      (tileY < waterLine || tileY > lavaLine);
    bool setsLava = injectsLiquid && tileY > lavaLine && !suppressedByRemixOceanDepth;
    short injectedLiquidType = liquidType;
    bool clearsActive = tileType < 0;
    bool activates = tileType >= 0 && addTile;
    bool clearsLiquidForType59 = tileType == 59 && tileY > waterLine &&
      currentLiquidAmount > 0;
    bool writesWall = tileType >= 0 && noYChange &&
      tileType != 59 && tileY < worldSurface;
    return new LegacyTileRunnerSideEffectProfile(
      injectsLiquid,
      injectedLiquidType,
      suppressedByRemixOceanDepth,
      setsLava,
      clearsActive,
      activates,
      clearsLiquidForType59,
      activates,
      writesWall);
  }
}
