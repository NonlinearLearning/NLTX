using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacySandPatchesPass
{
  private const double CentralBandMinimum = 0.46;
  private const double CentralBandMaximum = 0.54;
  private const double CentralYThresholdOffset = 150.0;
  private const int SourceLine = 12022;

  public static int SourceAnchorLine => SourceLine;

  public static IReadOnlyList<LegacyTileRunnerPassInvocation> CreateInvocations(
    WorldGridSnapshot snapshot,
    LegacyTerrainRuntimeProfile profile,
    LegacyPassRandomState random,
    bool isRemixWorld,
    bool isSkyblockWorld)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(profile);
    ArgumentNullException.ThrowIfNull(random);
    LegacySandPatchesPassDefinition definition =
      LegacySandPatchesPassDefinition.CreateDefault();
    definition.Validate();
    if (isSkyblockWorld)
    {
      return Array.Empty<LegacyTileRunnerPassInvocation>();
    }

    profile.Validate(snapshot.Metadata);
    (int minimumY, int maximumYExclusive) = definition.GetInitialYRange(
      snapshot.Metadata.Height,
      profile,
      isRemixWorld);
    (int retryMinimumY, int retryMaximumYExclusive) = definition.GetRetryYRange(profile);
    int invocationCount = definition.CalculateInvocationCount(
      snapshot.Metadata.Width,
      isRemixWorld);
    LegacyTileRunnerPassInput recipe = CreateRecipe(definition, isRemixWorld);
    List<LegacyTileRunnerPassInvocation> invocations = new(invocationCount);
    for (int index = 0; index < invocationCount; index++)
    {
      int xDraw = random.Next(0, snapshot.Metadata.Width);
      int defaultYDraw = random.Next(retryMinimumY, retryMaximumYExclusive);
      int yDraw = isRemixWorld
        ? random.Next(minimumY, maximumYExclusive)
        : defaultYDraw;
      int retryCount = 0;
      while (IsInsideCentralRetryBand(
               xDraw,
               yDraw,
               snapshot.Metadata.Width,
               profile.WorldSurface))
      {
        xDraw = random.Next(0, snapshot.Metadata.Width);
        yDraw = random.Next(retryMinimumY, retryMaximumYExclusive);
        retryCount++;
      }

      int strengthDraw = random.Next(
        definition.MinimumStrength,
        definition.MaximumStrengthExclusive);
      int stepsDraw = random.Next(
        definition.MinimumSteps,
        definition.MaximumStepsExclusive);
      LegacyTileRunnerRequest request = new(
        xDraw,
        yDraw,
        strengthDraw,
        stepsDraw,
        definition.TileType,
        addTile: false,
        speedX: 0.0,
        speedY: 0.0,
        noYChange: false,
        overwrite: true,
        ignoreTileType: -1);
      invocations.Add(new LegacyTileRunnerPassInvocation(
        recipe,
        request,
        xDraw,
        yDraw,
        strengthDraw,
        stepsDraw,
        RandomDrawCount: (isRemixWorld ? 5 : 4) + retryCount * 2));
    }

    return invocations.AsReadOnly();
  }

  public static void AppendCommands(
    WorldGridSnapshot snapshot,
    LegacyTerrainRuntimeProfile profile,
    LegacyPassRandomState random,
    bool isRemixWorld,
    bool isSkyblockWorld,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(profile);
    ArgumentNullException.ThrowIfNull(random);
    ArgumentNullException.ThrowIfNull(commands);
    IReadOnlyList<LegacyTileRunnerPassInvocation> invocations = CreateInvocations(
      snapshot,
      profile,
      random,
      isRemixWorld,
      isSkyblockWorld);
    if (invocations.Count == 0)
    {
      return;
    }

    if (state.Stage < WorldGenerationStage.Cave &&
        !state.TryAdvance(WorldGenerationStage.Cave))
    {
      throw new InvalidOperationException("SandPatches could not enter the cave stage.");
    }

    int worldSurfaceY = checked((int)profile.WorldSurface);
    int rockLayerY = checked((int)profile.RockLayer);
    foreach (LegacyTileRunnerPassInvocation invocation in invocations)
    {
      LegacyTileRunnerTraversal.AppendCommands(
        snapshot,
        invocation,
        random,
        worldSurfaceY,
        rockLayerY,
        ref state,
        commands);
    }
  }

  private static LegacyTileRunnerPassInput CreateRecipe(
    LegacySandPatchesPassDefinition definition,
    bool isRemixWorld)
  {
    return new LegacyTileRunnerPassInput(
      "SandPatches",
      "sand-patch",
      definition.TileType,
      AddTile: false,
      definition.MinimumStrength,
      definition.MaximumStrengthExclusive,
      definition.MinimumSteps,
      definition.MaximumStepsExclusive,
      isRemixWorld ? "rockLayer-100..maxTilesY-350" : "worldSurface..rockLayer",
      UsesRandomX: true,
      UsesRandomY: true,
      ResetsRandomFromWorldSeed: true);
  }

  private static bool IsInsideCentralRetryBand(
    int x,
    int y,
    int width,
    double worldSurface)
  {
    return (double)x > width * CentralBandMinimum &&
      (double)x < width * CentralBandMaximum &&
      (double)y < worldSurface + CentralYThresholdOffset;
  }
}
