using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration.Systems;

public sealed class TerrainBaseSystem
{
  public void AppendCommands(
    WorldGrid world,
    WorldGenerationRequest request,
    TerrainProfileComponent profile,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands)
  {
    GenerationRandomState random = new(unchecked((uint)request.Metadata.Seed.Value));
    AppendCommands(world, request, profile, ref state, ref random, commands);
  }

  public void AppendLegacyCommands(
    WorldGrid world,
    WorldGenerationRequest request,
    TerrainProfileComponent profile,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands)
  {
    LegacyPassRandomState random = new(request.Metadata.Seed.Value);
    if (request.TerrainProfile is not null)
    {
      AppendProfileTerrainCommands(world, request, profile, ref state, random, commands);
      return;
    }

    AppendLegacyCommands(world, request, profile, ref state, random, commands);
  }

  private static void AppendProfileTerrainCommands(
    WorldGrid world,
    WorldGenerationRequest request,
    TerrainProfileComponent profile,
    ref WorldGenerationStateComponent state,
    LegacyPassRandomState random,
    List<TileChangeCommand> commands)
  {
    ArgumentNullException.ThrowIfNull(world);
    ArgumentNullException.ThrowIfNull(request);
    ArgumentNullException.ThrowIfNull(commands);
    if (state.Stage < WorldGenerationStage.Terrain &&
        !state.TryAdvance(WorldGenerationStage.Terrain))
    {
      throw new InvalidOperationException("Terrain stage could not be started.");
    }

    LegacyTerrainRuntimeProfile runtimeProfile = request.TerrainProfile ??
      throw new InvalidOperationException("A terrain runtime profile is required.");
    const int flatBeachPadding = 5;
    int initialSurfaceFactor = random.Next(90, 110);
    int initialRockFactor = random.Next(90, 110);
    double derivedSurface = world.Height * 0.3 * initialSurfaceFactor * 0.005;
    double surface = runtimeProfile.InitialWorldSurface ?? derivedSurface;
    double derivedRockLayer = derivedSurface + world.Height * 0.2 * initialRockFactor * 0.01;
    double rockLayer = runtimeProfile.InitialRockLayer ?? derivedRockLayer;
    double surfaceLow = surface;
    double surfaceHigh = surface;
    double rockLayerLow = rockLayer;
    double rockLayerHigh = rockLayer;
    LegacyTerrainFeatureKind feature = LegacyTerrainFeatureKind.Plateau;
    int featureRun = 0;
    LegacySurfaceHistory history = new(500);

    for (int x = 0; x < world.Width; x++)
    {
      surfaceLow = Math.Min(surfaceLow, surface);
      surfaceHigh = Math.Max(surfaceHigh, surface);
      rockLayerLow = Math.Min(rockLayerLow, rockLayer);
      rockLayerHigh = Math.Max(rockLayerHigh, rockLayer);
      if (featureRun <= 0)
      {
        feature = (LegacyTerrainFeatureKind)random.Next(0, 5);
        featureRun = random.Next(5, 40);
        if (feature == LegacyTerrainFeatureKind.Plateau)
        {
          featureRun *= (int)(random.Next(5, 30) * 0.2);
        }
      }

      featureRun--;
      if (x > world.Width * 0.48 && x < world.Width * 0.52)
      {
        feature = LegacyTerrainFeatureKind.Plateau;
      }

      surface += LegacyTerrainSurfaceOffsetPolicy.Next(random, feature, specialWorld: false);
      LegacyTerrainSurfaceClampResult clamp = LegacyTerrainSurfaceClampPolicy.Apply(
        surface,
        x,
        runtimeProfile.LeftBeachEnd,
        runtimeProfile.RightBeachStart,
        flatBeachPadding,
        world.Height * 0.17,
        world.Height * 0.26,
        world.Height * 0.23);
      surface = clamp.Surface;
      if (clamp.ResetFeatureRun)
      {
        featureRun = 0;
      }

      while (random.Next(0, 3) == 0)
      {
        rockLayer += random.Next(-2, 3);
      }

      if (rockLayer < surface + world.Height * 0.06)
      {
        rockLayer += 1.0;
      }

      if (rockLayer > surface + world.Height * 0.35)
      {
        rockLayer -= 1.0;
      }

      history.Record(surface);
      AppendColumnCommands(world, ref state, commands, x, surface, rockLayer);
      if (x == runtimeProfile.RightBeachStart - flatBeachPadding && surface > world.Height * 0.23)
      {
        IReadOnlyList<LegacySurfaceRetargetCommand> retargetCommands =
          history.PrepareRetargetBatch(x, world.Height * 0.23);
        foreach (LegacySurfaceRetargetCommand retargetCommand in retargetCommands)
        {
          AppendColumnCommands(
            world,
            ref state,
            commands,
            retargetCommand.X,
            retargetCommand.WorldSurface,
            rockLayer);
        }
        feature = LegacyTerrainFeatureKind.Plateau;
        featureRun = world.Width - x;
      }
    }
  }

  private static void AppendColumnCommands(
    WorldGrid world,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands,
    int x,
    double surface,
    double rockLayer)
  {
    int surfaceStart = Math.Clamp((int)surface, 0, world.Height - 1);
    int rockLayerStart = Math.Clamp((int)rockLayer, surfaceStart + 1, world.Height - 1);
    for (int y = surfaceStart; y < world.Height; y++)
    {
      ushort tileType = y < rockLayerStart ? (ushort)0 : (ushort)1;
      commands.Add(new TileChangeCommand(
        state.ReserveSequence(),
        x,
        y,
        TileChangeKind.Place,
        tileType,
        FrameX: -1,
        FrameY: -1));
    }
  }

  private static void AppendLegacyCommands(
    WorldGrid world,
    WorldGenerationRequest request,
    TerrainProfileComponent profile,
    ref WorldGenerationStateComponent state,
    LegacyPassRandomState random,
    List<TileChangeCommand> commands)
  {
    ArgumentNullException.ThrowIfNull(world);
    ArgumentNullException.ThrowIfNull(request);
    ArgumentNullException.ThrowIfNull(commands);
    if (state.Stage < WorldGenerationStage.Terrain &&
        !state.TryAdvance(WorldGenerationStage.Terrain))
    {
      throw new InvalidOperationException("Terrain stage could not be started.");
    }

    TerrainDefinition definition = TerrainDefinition.Default;
    for (int x = 0; x < world.Width; x++)
    {
      int surfaceVariation = random.Next(
        -definition.SurfaceVariation,
        definition.SurfaceVariation + 1);
      int surfaceY = Math.Clamp(
        profile.SurfaceY + surfaceVariation,
        0,
        world.Height - 1);
      for (int y = surfaceY; y < world.Height; y++)
      {
        ushort tileType = y < profile.RockLayerY
          ? definition.StoneTileType
          : definition.GroundTileType;
        commands.Add(new TileChangeCommand(
          state.ReserveSequence(),
          x,
          y,
          TileChangeKind.Place,
          tileType,
          FrameX: -1,
          FrameY: -1));
      }
    }
  }

  public void AppendCommands(
    WorldGrid world,
    WorldGenerationRequest request,
    TerrainProfileComponent profile,
    ref WorldGenerationStateComponent state,
    ref GenerationRandomState random,
    List<TileChangeCommand> commands)
  {
    ArgumentNullException.ThrowIfNull(world);
    ArgumentNullException.ThrowIfNull(request);
    ArgumentNullException.ThrowIfNull(commands);
    if (state.Stage < WorldGenerationStage.Terrain &&
        !state.TryAdvance(WorldGenerationStage.Terrain))
    {
      throw new InvalidOperationException("Terrain stage could not be started.");
    }

    TerrainDefinition definition = TerrainDefinition.Default;
    for (int x = 0; x < world.Width; x++)
    {
      (random, int surfaceVariation) = random.NextInclusive(
        -definition.SurfaceVariation,
        definition.SurfaceVariation);
      int surfaceY = Math.Clamp(
        profile.SurfaceY + surfaceVariation,
        0,
        world.Height - 1);
      for (int y = surfaceY; y < world.Height; y++)
      {
        ushort tileType = y < profile.RockLayerY
          ? definition.StoneTileType
          : definition.GroundTileType;
        commands.Add(new TileChangeCommand(
          state.ReserveSequence(),
          x,
          y,
          TileChangeKind.Place,
          tileType,
          FrameX: -1,
          FrameY: -1));
      }
    }
  }

}
