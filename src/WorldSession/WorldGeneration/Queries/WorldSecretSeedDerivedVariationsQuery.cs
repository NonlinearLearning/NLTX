using System;
using Terraria.WorldGeneration.Components;
using Terraria.WorldGeneration.Definitions;

namespace Terraria.WorldGeneration.Queries;

public static class WorldSecretSeedDerivedVariationsQuery
{
  public static WorldSecretSeedDerivedVariationsSelection Evaluate(
    WorldSecretSeedDerivedVariationsInput input)
  {
    WorldSecretSeedRuntimeRegistrySnapshot runtime = input.RuntimeRegistry;
    if (!runtime.IsActive)
    {
      throw new ArgumentException(
        "Secret-seed variations require an active runtime registry snapshot.",
        nameof(input));
    }

    bool paintGray = IsEnabled(runtime, input.VisualAndSurfaceRules.PaintEverythingGray);
    bool paintNegative = IsEnabled(runtime, input.VisualAndSurfaceRules.PaintEverythingNegative);
    bool coatEcho = IsEnabled(runtime, input.VisualAndSurfaceRules.CoatEverythingEcho);
    bool coatIlluminant =
      IsEnabled(runtime, input.VisualAndSurfaceRules.CoatEverythingIlluminant);
    bool noSurface = IsEnabled(runtime, input.VisualAndSurfaceRules.NoSurface);
    bool worldIsFrozen = IsEnabled(runtime, input.VisualAndSurfaceRules.WorldIsFrozen);
    bool extraLivingTrees =
      IsEnabled(runtime, input.TerrainAndStructureRules.ExtraLivingTrees);
    bool extraFloatingIslands =
      IsEnabled(runtime, input.TerrainAndStructureRules.ExtraFloatingIslands);
    bool errorWorld = IsEnabled(runtime, input.ProgressionAndInfectionRules.ErrorWorld);
    bool noSpiderCaves = IsEnabled(runtime, input.TerrainAndStructureRules.NoSpiderCaves);
    bool actuallyNoTraps =
      IsEnabled(runtime, input.TerrainAndStructureRules.ActuallyNoTraps);
    bool surfaceIsDesert =
      IsEnabled(runtime, input.ProgressionAndInfectionRules.SurfaceIsDesert);

    int activeSecretSeedCount = runtime.ActiveSecretSeedCount;
    bool paintGrayJustTreasure = paintGray && activeSecretSeedCount >= 4;
    bool paintNegativeJustSomeThings = paintNegative && activeSecretSeedCount >= 4;
    bool coatEchoJustSomeThings = coatEcho && activeSecretSeedCount >= 4;
    bool coatIlluminantJustSomeThings =
      coatEcho &&
      (activeSecretSeedCount >= 3 || paintGray || paintNegative);
    bool coatIlluminantJustRandomSpots =
      !coatIlluminantJustSomeThings && coatEcho;
    bool surfaceIsDesertSwap = surfaceIsDesert && noSurface;

    return new WorldSecretSeedDerivedVariationsSelection(
      paintGray && !paintGrayJustTreasure &&
        (!paintNegative && !coatEcho ? coatIlluminant : true),
      paintGrayJustTreasure,
      paintGray && worldIsFrozen,
      paintNegative && !paintNegativeJustSomeThings &&
        (!paintGray && !coatEcho ? coatIlluminant : true),
      paintNegativeJustSomeThings,
      coatEcho && !coatEchoJustSomeThings &&
        (!paintGray && !paintNegative ? activeSecretSeedCount >= 3 : true),
      coatEchoJustSomeThings,
      coatIlluminantJustRandomSpots,
      coatIlluminantJustSomeThings,
      noSurface && !errorWorld && !extraFloatingIslands,
      noSurface && !errorWorld && !extraLivingTrees,
      noSurface && !errorWorld,
      noSurface && !errorWorld,
      extraLivingTrees && (activeSecretSeedCount >= 6 || noSurface),
      extraFloatingIslands && input.SkyblockWorld,
      !extraFloatingIslands || activeSecretSeedCount < 6
        ? noSurface
        : true,
      errorWorld && activeSecretSeedCount >= 6,
      noSpiderCaves && activeSecretSeedCount < 4,
      noSpiderCaves && activeSecretSeedCount >= 4,
      actuallyNoTraps && activeSecretSeedCount < 4,
      surfaceIsDesert && !surfaceIsDesertSwap,
      surfaceIsDesertSwap,
      activeSecretSeedCount,
      runtime.GenerationId,
      runtime.RuntimeVersion);
  }

  private static bool IsEnabled(
    WorldSecretSeedRuntimeRegistrySnapshot runtime,
    WorldSecretSeedDefinition definition)
  {
    return runtime.IsEnabled(definition.Variant);
  }
}
