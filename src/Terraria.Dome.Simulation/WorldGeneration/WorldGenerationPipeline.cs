using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldGeneration.Systems;
using Terraria.Dome.Simulation.WorldModel;
using PipelineLiquidChangeCommand = Terraria.Dome.Simulation.Commands.LiquidChangeCommand;

namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed class WorldGenerationPipeline
{
  private const int SpawnClearHalfWidth = 4;
  private const int SpawnClearHeight = 7;

  public WorldGrid Generate(WorldGenerationRequest request)
  {
    return GenerateCore(request, traceStages: null);
  }

  public WorldGenerationTrace GenerateWithTrace(WorldGenerationRequest request)
  {
    List<WorldGenerationStageSnapshot> traceStages = new();
    WorldGrid world = GenerateCore(request, traceStages);
    return new WorldGenerationTrace(world.CreateSnapshot(request.Metadata), traceStages);
  }

  private WorldGrid GenerateCore(
    WorldGenerationRequest request,
    List<WorldGenerationStageSnapshot>? traceStages)
  {
    ArgumentNullException.ThrowIfNull(request);
    EnsureSupportedRules(request);
    WorldGrid world = new(
      request.Metadata.Width,
      request.Metadata.Height,
      initializeLegacyEmptyFrames: true);
    WorldGenerationBootstrap bootstrap = new WorldGenerationStageSystem().Initialize(request);
    WorldGenerationStateComponent state = bootstrap.State;
    List<TileChangeCommand> commands = new();
    if (!state.TryAdvance(WorldGenerationStage.Terrain))
    {
      throw new InvalidOperationException("World generation did not enter the terrain stage.");
    }

    TerrainProfileComponent terrainProfile = CreateTerrainProfile(request);
    new TerrainBaseSystem().AppendLegacyCommands(
      world,
      request,
      terrainProfile,
      ref state,
      commands);
    ClearSpawnArea(commands, request, ref state);
    if (!new TileChangeCommitSystem().TryCommit(
          world,
          commands,
          out TileChangeCommitResult commitResult))
    {
      throw new InvalidOperationException(commitResult.FailureReason);
    }

    CaptureStage(traceStages, WorldGenerationStage.Terrain, world, request, state);

    commands.Clear();
    if (request.DirtWallSurfaceOffsetChanges is not null)
    {
      new DirtWallBackgroundSystem().AppendCommands(
        world.CreateSnapshot(request.Metadata),
        request.SurfaceY,
        request.DirtWallSurfaceOffsetChanges,
        ref state,
        commands);
      if (!new TileChangeCommitSystem().TryCommit(world, commands, out commitResult))
      {
        throw new InvalidOperationException(commitResult.FailureReason);
      }

      commands.Clear();
    }

    WorldGridSnapshot terrainSnapshot = world.CreateSnapshot(request.Metadata);
    new CaveCarvingSystem().AppendCommands(
      terrainSnapshot,
      request,
      new CaveCarvingComponent("single-tunnel", radius: 1, density: 4),
      ref state,
      commands);
    if (!new TileChangeCommitSystem().TryCommit(
          world,
          commands,
          out commitResult))
    {
      throw new InvalidOperationException(commitResult.FailureReason);
    }

    CaptureStage(traceStages, WorldGenerationStage.Cave, world, request, state);

    commands.Clear();
    WorldGridSnapshot caveSnapshot = world.CreateSnapshot(request.Metadata);
    BiomeSurfaceResult biomeResult = new BiomeSurfaceSystem().AppendCommands(
      caveSnapshot,
      new BiomeSurfaceComponent("default"),
      ref state,
      commands);
    if (!biomeResult.Supported)
    {
      throw new InvalidOperationException(biomeResult.FailureReason);
    }

    if (!new TileChangeCommitSystem().TryCommit(
          world,
          commands,
          out commitResult))
    {
      throw new InvalidOperationException("World generation did not commit its terrain stage.");
    }

    CaptureStage(traceStages, WorldGenerationStage.Biome, world, request, state);

    commands.Clear();
    TileProtectionComponent protection = new(
      request.SpawnX,
      request.SurfaceY,
      SpawnClearHalfWidth,
      SpawnClearHeight);
    WorldGridSnapshot biomeSnapshot = world.CreateSnapshot(request.Metadata);
    OreDefinition oreDefinition = new(
      "copper",
      tileType: 7,
      minDepth: Math.Min(request.Metadata.Height - 1, request.SurfaceY + 10),
      maxDepth: Math.Min(request.Metadata.Height - 1, request.SurfaceY + 80),
      veinRadius: 1,
      priority: 10);
    new OrePlacementSystem().AppendCommands(
      biomeSnapshot,
      request,
      oreDefinition,
      protection,
      ref state,
      commands);
    if (!new TileChangeCommitSystem().TryCommit(world, commands, out commitResult))
    {
      throw new InvalidOperationException(commitResult.FailureReason);
    }

    CaptureStage(traceStages, WorldGenerationStage.Ore, world, request, state);

    commands.Clear();
    StructureDefinition structureDefinition = new(
      "starter-house",
      width: 4,
      height: 3,
      tileType: 5,
      wallType: 1,
      allowReplaceExisting: true);
    StructurePlacementSystem structureSystem = new();
    if (!structureSystem.TryPrepare(
          world.CreateSnapshot(request.Metadata),
          structureDefinition,
          originX: Math.Min(request.Metadata.Width - 4, 20),
          originY: Math.Min(request.Metadata.Height - 3, request.SurfaceY + 1),
          protection,
          out StructurePlacementComponent structurePlacement,
          out string? structureFailure) ||
        !structureSystem.AppendCommands(
          world.CreateSnapshot(request.Metadata),
          structureDefinition,
          structurePlacement,
          protection,
          ref state,
          commands))
    {
      throw new InvalidOperationException(
        structureFailure ?? "World generation structure placement failed.");
    }

    List<TileChangeCommand> frameInput = new(commands);
    if (!new TileChangeCommitSystem().TryCommit(world, commands, out commitResult))
    {
      throw new InvalidOperationException(commitResult.FailureReason);
    }

    CaptureStage(traceStages, WorldGenerationStage.Structure, world, request, state);

    commands.Clear();
    TreeDefinition treeDefinition = new(
      "ordinary",
      trunkTileType: 3,
      leafTileType: 4,
      minimumHeight: 3,
      maximumHeight: 5,
      canopyRadius: 2);
    TreePlacementComponent treePlacement = new(
      treeDefinition.Id,
      Math.Min(request.Metadata.Width - 1, 100),
      Math.Min(request.Metadata.Height - 1, request.SurfaceY + 1));
    if (!new TreePlacementSystem().TryAppendCommands(
          world.CreateSnapshot(request.Metadata),
          request,
          treeDefinition,
          treePlacement,
          protection,
          ref state,
          commands) ||
        !new TileChangeCommitSystem().TryCommit(world, commands, out commitResult))
    {
        throw new InvalidOperationException("World generation tree placement failed.");
    }

    CaptureStage(traceStages, WorldGenerationStage.Tree, world, request, state);

    commands.Clear();
    LiquidDefinition water = new("water", type: 0, maxAmount: byte.MaxValue);
    LiquidSourceComponent liquidSource = new(
      Math.Min(request.Metadata.Width - 1, request.SpawnX + 20),
      Math.Min(request.Metadata.Height - 1, request.SurfaceY + 10),
      water.Type,
      byte.MaxValue,
      "worldgen");
    List<LiquidWorkItemComponent> workItems = new();
    List<PipelineLiquidChangeCommand> liquidCommands = new();
    new LiquidSourceSystem().AppendWorkItems(
      new[] { liquidSource },
      new WorldBoundsComponent(request.Metadata.Width, request.Metadata.Height),
      ref state,
      workItems);
    LiquidPropagationResult propagationResult = new LiquidPropagationSystem().TryAppendCommands(
      world.CreateSnapshot(request.Metadata),
      new[] { water },
      Array.Empty<LiquidMergeComponent>(),
      workItems,
      budget: 16,
      ref state,
      liquidCommands);
    if (!propagationResult.Succeeded)
    {
      throw new InvalidOperationException(propagationResult.FailureReason);
    }

    if (!new LiquidChangeCommitSystem().TryCommit(
          world,
          liquidCommands,
          new[] { water },
          out LiquidChangeCommitResult liquidCommitResult))
    {
      throw new InvalidOperationException(liquidCommitResult.FailureReason);
    }

    CaptureStage(traceStages, WorldGenerationStage.Liquid, world, request, state);

    List<TileFrameCommand> frameCommands = new();
    if (!new TileFrameSystem().TryAppendCommands(
          world.CreateSnapshot(request.Metadata),
          frameInput,
          ref state,
          frameCommands))
    {
      throw new InvalidOperationException("World generation framing failed.");
    }

    if (!new TileChangeCommitSystem().TryCommit(
          world,
          frameCommands,
          out TileFrameCommitResult frameCommitResult))
    {
      throw new InvalidOperationException(frameCommitResult.FailureReason);
    }

    CaptureStage(traceStages, WorldGenerationStage.Framing, world, request, state);

    if (!state.TryAdvance(WorldGenerationStage.Committed))
    {
      throw new InvalidOperationException("World generation did not enter the commit stage.");
    }

    WorldGenerationValidationResult validationResult =
      new WorldGenerationValidationSystem().Validate(
        world.CreateSnapshot(request.Metadata),
        ref state,
        workItems,
        new[] { structurePlacement });
    if (!validationResult.Succeeded)
    {
      throw new InvalidOperationException(validationResult.FailureReason);
    }

    CaptureStage(traceStages, WorldGenerationStage.Committed, world, request, state);

    return world;
  }

  private static void CaptureStage(
    List<WorldGenerationStageSnapshot>? traceStages,
    WorldGenerationStage stage,
    WorldGrid world,
    WorldGenerationRequest request,
    WorldGenerationStateComponent state)
  {
    traceStages?.Add(new WorldGenerationStageSnapshot(
      stage,
      world.CreateSnapshot(request.Metadata),
      state.NextSequence));
  }

  private static void EnsureSupportedRules(WorldGenerationRequest request)
  {
    if (request.SeedVariant != "default" ||
        request.Rules.SecretSeedVariant != "default" ||
        request.Rules.Difficulty != 0 ||
        request.Rules.IsHardmode)
    {
      throw new NotSupportedException(
        "The current world-generation pipeline supports only default world rules.");
    }
  }

  private static TerrainProfileComponent CreateTerrainProfile(WorldGenerationRequest request)
  {
    int surfaceY = request.SurfaceY;
    int rockLayerY = request.RockLayerY;
    if (request.TerrainProfile is not null)
    {
      surfaceY = Math.Clamp(
        (int)Math.Round(request.TerrainProfile.WorldSurface),
        0,
        request.Metadata.Height - 2);
      rockLayerY = Math.Clamp(
        (int)Math.Round(request.TerrainProfile.RockLayer),
        surfaceY + 1,
        request.Metadata.Height - 1);
    }

    int underworldY = Math.Max(rockLayerY + 1, request.Metadata.Height - 1);
    return new TerrainProfileComponent(surfaceY, rockLayerY, underworldY);
  }

  private static void ClearSpawnArea(
    List<TileChangeCommand> commands,
    WorldGenerationRequest request,
    ref WorldGenerationStateComponent state)
  {
    int startX = Math.Max(0, request.SpawnX - SpawnClearHalfWidth);
    int endX = Math.Min(request.Metadata.Width - 1, request.SpawnX + SpawnClearHalfWidth);
    int startY = request.SurfaceY;
    int endY = Math.Min(request.Metadata.Height - 1, request.SurfaceY + SpawnClearHeight);
    for (int y = startY; y <= endY; y++)
    {
      for (int x = startX; x <= endX; x++)
      {
        commands.Add(new TileChangeCommand(
          state.ReserveSequence(),
          x,
          y,
          TileChangeKind.Kill,
          0));
      }
    }
  }

}
