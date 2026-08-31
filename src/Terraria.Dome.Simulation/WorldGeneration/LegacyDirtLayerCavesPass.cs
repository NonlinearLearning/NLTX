using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyDirtLayerCavesPass
{
  private const int SourceLine = 12406;
  private static readonly LegacyTileRunnerPassInput _recipe = new(
    "DirtLayerCaves",
    "dirt-layer",
    -1,
    false,
    5,
    15,
    30,
    200,
    "worldSurfaceLow..rockLayerHigh",
    true,
    true,
    true);

  public static int SourceAnchorLine => SourceLine;

  public static bool IsSupportedWorldWidth(int width)
  {
    if (width <= 0)
    {
      return false;
    }

    LegacyDirtLayerCavesPassDefinition definition =
      LegacyDirtLayerCavesPassDefinitionFactory.CreateDefault();
    return width > definition.SmallHolesBeachAvoidance * 2;
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
    profile.Validate(snapshot.Metadata);
    LegacyDirtLayerCavesPassDefinition definition =
      LegacyDirtLayerCavesPassDefinitionFactory.CreateDefault();
    definition.Validate();
    if (isSkyblockWorld)
    {
      return;
    }

    if (!IsSupportedWorldWidth(snapshot.Metadata.Width))
    {
      throw new ArgumentOutOfRangeException(
        nameof(snapshot),
        "DirtLayerCaves requires a width wider than both beach avoidance margins.");
    }

    int minimumY = checked((int)profile.WorldSurfaceLow);
    int maximumYExclusive = checked((int)profile.RockLayerHigh + 1);
    if (minimumY < 0 || maximumYExclusive <= minimumY ||
        maximumYExclusive > snapshot.Metadata.Height)
    {
      throw new ArgumentOutOfRangeException(nameof(profile));
    }

    if (state.Stage < WorldGenerationStage.Cave &&
        !state.TryAdvance(WorldGenerationStage.Cave))
    {
      throw new InvalidOperationException("DirtLayerCaves could not enter the cave stage.");
    }

    int invocationCount = definition.CalculateInvocationCount(
      snapshot.Metadata.Width,
      snapshot.Metadata.Height,
      isRemixWorld);
    int worldSurfaceY = checked((int)profile.WorldSurface);
    int rockLayerY = checked((int)profile.RockLayer);
    for (int index = 0; index < invocationCount; index++)
    {
      LegacyTileRunnerPassInvocation invocation = CreateInvocation(
        snapshot,
        profile,
        definition,
        random,
        isRemixWorld);
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

  private static LegacyTileRunnerPassInvocation CreateInvocation(
    WorldGridSnapshot snapshot,
    LegacyTerrainRuntimeProfile profile,
    LegacyDirtLayerCavesPassDefinition definition,
    LegacyPassRandomState random,
    bool isRemixWorld)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(profile);
    ArgumentNullException.ThrowIfNull(definition);
    ArgumentNullException.ThrowIfNull(random);
    int minimumY = checked((int)profile.WorldSurfaceLow);
    int maximumYExclusive = checked((int)profile.RockLayerHigh + 1);
    int tileType = definition.SelectTileType(random);
    int xDraw = random.Next(0, snapshot.Metadata.Width);
    int yDraw = random.Next(minimumY, maximumYExclusive);
    int retryCount = 0;
    while (!IsCandidateAccepted(
             xDraw,
             yDraw,
             snapshot.Metadata.Width,
             profile.WorldSurfaceHigh,
             profile.WorldSurface,
             definition.SmallHolesBeachAvoidance))
    {
      xDraw = random.Next(0, snapshot.Metadata.Width);
      yDraw = random.Next(minimumY, maximumYExclusive);
      retryCount++;
    }

    int strengthDraw = definition.ScaleStrength(
      random.Next(definition.MinimumStrength, definition.MaximumStrengthExclusive),
      isRemixWorld);
    int stepsDraw = definition.ScaleSteps(
      random.Next(definition.MinimumSteps, definition.MaximumStepsExclusive),
      isRemixWorld);
    LegacyTileRunnerRequest request = new(
      xDraw,
      yDraw,
      strengthDraw,
      stepsDraw,
      tileType,
      addTile: false,
      speedX: 0.0,
      speedY: 0.0,
      noYChange: false,
      overwrite: true,
      ignoreTileType: -1);
    return new LegacyTileRunnerPassInvocation(
      _recipe with { TileType = tileType },
      request,
      xDraw,
      yDraw,
      strengthDraw,
      stepsDraw,
      RandomDrawCount: 5 + retryCount * 2);
  }

  public static bool IsCandidateAccepted(
    int candidateX,
    int candidateY,
    int width,
    double worldSurfaceHigh,
    double worldSurface,
    int smallHolesBeachAvoidance)
  {
    if (width <= 0 || candidateX < 0 || candidateX >= width ||
        !double.IsFinite(worldSurfaceHigh) || !double.IsFinite(worldSurface) ||
        smallHolesBeachAvoidance < 0)
    {
      return false;
    }

    bool inBeachAvoidance = candidateX < smallHolesBeachAvoidance ||
      candidateX > width - smallHolesBeachAvoidance;
    bool inSpawnCenter = candidateX >= width * 0.45 && candidateX <= width * 0.55;
    return !(inBeachAvoidance && candidateY < worldSurfaceHigh) &&
      !(inSpawnCenter && candidateY < worldSurface);
  }
}
