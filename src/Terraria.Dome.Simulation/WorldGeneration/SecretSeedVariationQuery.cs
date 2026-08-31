using System;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class SecretSeedVariationQuery
{
  private const string ActuallyNoTrapsVariant = "actually-no-traps";
  private const string CoatEverythingEchoVariant = "coat-everything-echo";
  private const string CoatEverythingIlluminantVariant = "coat-everything-illuminant";
  private const string ErrorWorldVariant = "error-world";
  private const string ExtraFloatingIslandsVariant = "extra-floating-islands";
  private const string ExtraLivingTreesVariant = "extra-living-trees";
  private const string NoSpiderCavesVariant = "no-spider-caves";
  private const string NoSurfaceVariant = "no-surface";
  private const string PaintEverythingGrayVariant = "paint-everything-gray";
  private const string PaintEverythingNegativeVariant = "paint-everything-negative";
  private const string SurfaceIsDesertVariant = "surface-is-desert";

  public static SecretSeedVariationSnapshot Evaluate(
    IReadOnlySet<string> enabledVariants,
    int activeSecretSeedCount,
    bool skyblockWorld)
  {
    ArgumentNullException.ThrowIfNull(enabledVariants);
    if (activeSecretSeedCount < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(activeSecretSeedCount));
    }

    bool paintGray = enabledVariants.Contains(PaintEverythingGrayVariant);
    bool paintNegative = enabledVariants.Contains(PaintEverythingNegativeVariant);
    bool coatEcho = enabledVariants.Contains(CoatEverythingEchoVariant);
    bool coatIlluminant = enabledVariants.Contains(CoatEverythingIlluminantVariant);
    bool noSurface = enabledVariants.Contains(NoSurfaceVariant);
    bool extraTrees = enabledVariants.Contains(ExtraLivingTreesVariant);
    bool extraIslands = enabledVariants.Contains(ExtraFloatingIslandsVariant);
    bool errorWorld = enabledVariants.Contains(ErrorWorldVariant);
    bool noSpiderCaves = enabledVariants.Contains(NoSpiderCavesVariant);
    bool noTraps = enabledVariants.Contains(ActuallyNoTrapsVariant);
    bool surfaceDesert = enabledVariants.Contains(SurfaceIsDesertVariant);

    return new SecretSeedVariationSnapshot(
      paintGray && !PaintGrayJustTreasure(paintGray, activeSecretSeedCount) &&
        (!paintNegative && !coatEcho ? coatIlluminant : true),
      paintGray && activeSecretSeedCount >= 4,
      paintGray && enabledVariants.Contains("world-is-frozen"),
      paintNegative && !PaintNegativeJustSomeThings(paintNegative, activeSecretSeedCount) &&
        (!paintGray && !coatEcho ? coatIlluminant : true),
      paintNegative && activeSecretSeedCount >= 4,
      coatEcho && !CoatEchoJustSomeThings(coatEcho, activeSecretSeedCount) &&
        (!paintGray && !paintNegative ? activeSecretSeedCount >= 3 : true),
      coatEcho && activeSecretSeedCount >= 4,
      !CoatIlluminantJustSomeThings(coatEcho, paintNegative, paintGray, activeSecretSeedCount) &&
        coatEcho,
      CoatIlluminantJustSomeThings(coatEcho, paintNegative, paintGray, activeSecretSeedCount),
      noSurface && !errorWorld && !extraIslands,
      noSurface && !errorWorld && !extraTrees,
      noSurface && !errorWorld,
      noSurface && !errorWorld,
      extraTrees && (activeSecretSeedCount >= 6 || noSurface),
      extraIslands && skyblockWorld,
      !extraIslands || activeSecretSeedCount < 6 ? noSurface : true,
      errorWorld && activeSecretSeedCount >= 6,
      noSpiderCaves && activeSecretSeedCount < 4,
      noSpiderCaves && activeSecretSeedCount >= 4,
      noTraps && activeSecretSeedCount < 4,
      surfaceDesert && !noSurface,
      surfaceDesert && noSurface,
      activeSecretSeedCount);
  }

  private static bool PaintGrayJustTreasure(bool enabled, int count)
  {
    return enabled && count >= 4;
  }

  private static bool PaintNegativeJustSomeThings(bool enabled, int count)
  {
    return enabled && count >= 4;
  }

  private static bool CoatEchoJustSomeThings(bool enabled, int count)
  {
    return enabled && count >= 4;
  }

  private static bool CoatIlluminantJustSomeThings(
    bool coatEcho,
    bool paintNegative,
    bool paintGray,
    int count)
  {
    if (!coatEcho)
    {
      return false;
    }

    return count < 3 && !paintGray ? paintNegative : true;
  }
}
