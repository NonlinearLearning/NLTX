namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct SecretSeedVariationSnapshot(
  bool PaintEverythingGrayJustTheSurface,
  bool PaintEverythingGrayJustTreasure,
  bool PaintEverythingGrayUseWhite,
  bool PaintEverythingNegativeJustUnderground,
  bool PaintEverythingNegativeJustSomeThings,
  bool CoatEverythingJustInnerBlocks,
  bool CoatEverythingEchoJustSomeThings,
  bool CoatEverythingIlluminantJustRandomSpots,
  bool CoatEverythingIlluminantJustSomeThings,
  bool NoSurfaceNoFloatingIslands,
  bool NoSurfaceNoLivingTrees,
  bool NoSurfaceNoPyramids,
  bool NoSurfaceNoSwordShrines,
  bool ExtraLivingTreesReducedAmount,
  bool ExtraFloatingIslandsNormalAmount,
  bool ExtraFloatingIslandsReducedAmount,
  bool ErrorWorldBalancedChests,
  bool NoSpiderCavesActuallyNoSpiderCaves,
  bool NoSpiderCavesILiedMoreSpiderCaves,
  bool ActuallyNoTrapsForRealIMeanIt,
  bool SurfaceIsDesertNormalFunction,
  bool SurfaceIsDesertSwapDesertAndSnowBiomes,
  int ActiveSecretSeedCount)
{
  public int ErrorWorldAdjustment(double value)
  {
    return ActiveSecretSeedCount < 1
      ? 4
      : (int)(value * ((ActiveSecretSeedCount + 3) / 4));
  }
}
