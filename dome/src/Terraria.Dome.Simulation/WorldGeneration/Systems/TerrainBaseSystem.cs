using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration.Systems;

public sealed class TerrainBaseSystem
{
  private const int SpawnClearPriority = 1;

  public void AppendCommands(
    WorldGridSnapshot snapshot,
    WorldGenerationRequest request,
    TerrainProfileComponent profile,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    GenerationRandomState random = new(unchecked((uint)request.Metadata.Seed.Value));
    AppendCommands(snapshot, request, profile, ref state, ref random, commands);
  }

  public void AppendLegacyCommands(
    WorldGridSnapshot snapshot,
    WorldGenerationRequest request,
    TerrainProfileComponent profile,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands)
  {
    LegacyPassRandomState random = new(request.Metadata.Seed.Value);
    if (request.TerrainProfile is not null)
    {
      AppendProfileTerrainCommands(snapshot, request, profile, ref state, random, commands);
      return;
    }

    AppendLegacyCommands(snapshot, request, profile, ref state, random, commands);
  }

  private static void AppendProfileTerrainCommands(
    WorldGridSnapshot snapshot,
    WorldGenerationRequest request,
    TerrainProfileComponent profile,
    ref WorldGenerationStateComponent state,
    LegacyPassRandomState random,
    List<TileChangeCommand> commands)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(request);
    ArgumentNullException.ThrowIfNull(commands);
    if (state.Stage < WorldGenerationStage.Terrain &&
        !state.TryAdvance(WorldGenerationStage.Terrain))
    {
      throw new InvalidOperationException("Terrain stage could not be started.");
    }

    LegacyTerrainRuntimeProfile runtimeProfile = request.TerrainProfile ??
      throw new InvalidOperationException("A terrain runtime profile is required.");
    const int smallWorldWidth = 4200;
    const double standardLowerSurfaceFactor = 0.17;
    const double smallWorldLowerSurfaceFactor = 0.19;
    const int flatBeachPadding = 5;
    int initialSurfaceFactor = random.Next(90, 110);
    int initialRockFactor = random.Next(90, 110);
    double derivedSurface = snapshot.Metadata.Height * 0.3 * initialSurfaceFactor * 0.005;
    double surface = runtimeProfile.InitialWorldSurface ?? derivedSurface;
    double derivedRockLayer =
      derivedSurface + snapshot.Metadata.Height * 0.2 * initialRockFactor * 0.01;
    double rockLayer = runtimeProfile.InitialRockLayer ?? derivedRockLayer;
    double surfaceLow = surface;
    double surfaceHigh = surface;
    double rockLayerLow = rockLayer;
    double rockLayerHigh = rockLayer;
    LegacyTerrainFeatureKind feature = LegacyTerrainFeatureKind.Plateau;
    int featureRun = runtimeProfile.LeftBeachEnd + flatBeachPadding;
    LegacySurfaceHistory history = new(500);
    double lowerSurfaceFactor = snapshot.Metadata.Width <= smallWorldWidth
      ? smallWorldLowerSurfaceFactor
      : standardLowerSurfaceFactor;

    for (int x = 0; x < snapshot.Metadata.Width; x++)
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
      if (x > snapshot.Metadata.Width * 0.45 &&
          x < snapshot.Metadata.Width * 0.55 &&
          (feature == LegacyTerrainFeatureKind.Mountain ||
            feature == LegacyTerrainFeatureKind.Valley))
      {
        feature = (LegacyTerrainFeatureKind)random.Next(3);
      }

      if (x > snapshot.Metadata.Width * 0.48 && x < snapshot.Metadata.Width * 0.52)
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
        snapshot.Metadata.Height * lowerSurfaceFactor,
        snapshot.Metadata.Height * 0.26,
        snapshot.Metadata.Height * 0.23);
      surface = clamp.Surface;
      if (clamp.ResetFeatureRun)
      {
        featureRun = 0;
      }

      while (random.Next(0, 3) == 0)
      {
        rockLayer += random.Next(-2, 3);
      }

      if (rockLayer < surface + snapshot.Metadata.Height * 0.06)
      {
        rockLayer += 1.0;
      }

      if (rockLayer > surface + snapshot.Metadata.Height * 0.35)
      {
        rockLayer -= 1.0;
      }

      history.Record(surface);
      AppendColumnCommands(snapshot, ref state, commands, x, surface, rockLayer);
      if (x == runtimeProfile.RightBeachStart - flatBeachPadding)
      {
        if (surface > snapshot.Metadata.Height * 0.23)
        {
          IReadOnlyList<LegacySurfaceRetargetCommand> retargetCommands =
            history.PrepareRetargetBatch(x, snapshot.Metadata.Height * 0.23);
          foreach (LegacySurfaceRetargetCommand retargetCommand in retargetCommands)
          {
            if (retargetCommand.X < 0 || retargetCommand.X >= snapshot.Metadata.Width)
            {
              continue;
            }

            AppendColumnCommands(
              snapshot,
              ref state,
              commands,
              retargetCommand.X,
              retargetCommand.WorldSurface,
              rockLayer);
          }
        }

        feature = LegacyTerrainFeatureKind.Plateau;
        featureRun = snapshot.Metadata.Width - x;
      }
    }

    AppendSpawnClearCommands(snapshot, request, ref state, commands);
  }

  private static void AppendColumnCommands(
    WorldGridSnapshot snapshot,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands,
    int x,
    double surface,
    double rockLayer)
  {
    int surfaceStart = Math.Clamp((int)surface, 0, snapshot.Metadata.Height - 1);
    for (int y = surfaceStart; y < snapshot.Metadata.Height; y++)
    {
      ushort tileType = (double)y < rockLayer ? (ushort)0 : (ushort)1;
      commands.Add(new TileChangeCommand(
        state.ReserveSequence(),
        x,
        y,
        TileChangeKind.Place,
        tileType,
        FrameX: -1,
        FrameY: -1,
        Source: "worldgen.terrain"));
    }
  }

  private static void AppendLegacyCommands(
    WorldGridSnapshot snapshot,
    WorldGenerationRequest request,
    TerrainProfileComponent profile,
    ref WorldGenerationStateComponent state,
    LegacyPassRandomState random,
    List<TileChangeCommand> commands)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(request);
    ArgumentNullException.ThrowIfNull(commands);
    if (state.Stage < WorldGenerationStage.Terrain &&
        !state.TryAdvance(WorldGenerationStage.Terrain))
    {
      throw new InvalidOperationException("Terrain stage could not be started.");
    }

    TerrainDefinition definition = TerrainDefinition.Default;
    for (int x = 0; x < snapshot.Metadata.Width; x++)
    {
      int surfaceVariation = random.Next(
        -definition.SurfaceVariation,
        definition.SurfaceVariation + 1);
      int surfaceY = Math.Clamp(
        profile.SurfaceY + surfaceVariation,
        0,
        snapshot.Metadata.Height - 1);
      for (int y = surfaceY; y < snapshot.Metadata.Height; y++)
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
          FrameY: -1,
          Source: "worldgen.terrain"));
      }
    }

    AppendSpawnClearCommands(snapshot, request, ref state, commands);
  }

  public void AppendCommands(
    WorldGridSnapshot snapshot,
    WorldGenerationRequest request,
    TerrainProfileComponent profile,
    ref WorldGenerationStateComponent state,
    ref GenerationRandomState random,
    List<TileChangeCommand> commands)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(request);
    ArgumentNullException.ThrowIfNull(commands);
    if (state.Stage < WorldGenerationStage.Terrain &&
        !state.TryAdvance(WorldGenerationStage.Terrain))
    {
      throw new InvalidOperationException("Terrain stage could not be started.");
    }

    TerrainDefinition definition = TerrainDefinition.Default;
    for (int x = 0; x < snapshot.Metadata.Width; x++)
    {
      (random, int surfaceVariation) = random.NextInclusive(
        -definition.SurfaceVariation,
        definition.SurfaceVariation);
      int surfaceY = Math.Clamp(
        profile.SurfaceY + surfaceVariation,
        0,
        snapshot.Metadata.Height - 1);
      for (int y = surfaceY; y < snapshot.Metadata.Height; y++)
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
          FrameY: -1,
          Source: "worldgen.terrain"));
      }
    }
  }

  private static void AppendSpawnClearCommands(
    WorldGridSnapshot snapshot,
    WorldGenerationRequest request,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands)
  {
    TerrainDefinition definition = TerrainDefinition.Default;
    int startX = Math.Max(0, request.SpawnX - definition.SpawnClearHalfWidth);
    int endX = Math.Min(
      snapshot.Metadata.Width - 1,
      request.SpawnX + definition.SpawnClearHalfWidth);
    int startY = Math.Max(0, request.SurfaceY);
    int endY = Math.Min(
      snapshot.Metadata.Height - 1,
      request.SurfaceY + definition.SpawnClearHeight);
    for (int y = startY; y <= endY; y++)
    {
      for (int x = startX; x <= endX; x++)
      {
        commands.Add(new TileChangeCommand(
          state.ReserveSequence(),
          x,
          y,
          TileChangeKind.Kill,
          TileType: 0,
          Priority: SpawnClearPriority,
          Source: "worldgen.spawn-clear"));
      }
    }
  }

}
