using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class SpecialTownNpcSpawningQuery
{
  private const int TruffleNpcType = 160;

  public static SpecialTownNpcSpawningResult Evaluate(
    int npcType,
    bool truffleUnlocked,
    bool roomAboveWorldSurface,
    bool noFunctionalSurface,
    int mushroomTileCount,
    int mushroomTileThreshold)
  {
    ArgumentOutOfRangeException.ThrowIfNegative(mushroomTileCount);
    ArgumentOutOfRangeException.ThrowIfNegative(mushroomTileThreshold);
    bool usedTruffleRule = npcType == TruffleNpcType;
    bool isAllowed = !usedTruffleRule ||
      (truffleUnlocked || !roomAboveWorldSurface || noFunctionalSurface) &&
      mushroomTileCount >= mushroomTileThreshold;
    return new SpecialTownNpcSpawningResult(
      isAllowed,
      npcType,
      mushroomTileCount,
      mushroomTileThreshold,
      usedTruffleRule,
      true);
  }
}
