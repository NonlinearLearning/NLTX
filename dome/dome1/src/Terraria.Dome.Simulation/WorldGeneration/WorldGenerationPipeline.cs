using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldGeneration.Systems;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed class WorldGenerationPipeline
{
  private const int SpawnProtectionHalfWidth = 4;
  private const int SpawnProtectionHeight = 7;

  public WorldGrid Generate(WorldGenerationRequest request)
  {
    return GenerateCore(request, traceStages: null);
  }

  public WorldGenerationTrace GenerateWithTrace(WorldGenerationRequest request)
  {
    List<WorldGenerationStageSnapshot> traceStages = new();
    WorldGrid world = GenerateCore(request, traceStages);
    TerrainProfileComponent terrainProfile = CreateTerrainProfile(request);
    return new WorldGenerationTrace(
      world.CreateSnapshot(request.Metadata),
      traceStages,
      WorldGenerationLifecycleQuery.FromLegacyFlags(
        generatingWorld: true,
        isGeneratingOrLoadingWorld: true),
      WorldGenerationLifecycleQuery.FromLegacyFlags(
        generatingWorld: false,
        isGeneratingOrLoadingWorld: false),
      request.DistanceDefaults,
      terrainProfile);
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
      world.CreateSnapshot(request.Metadata),
      request,
      terrainProfile,
      ref state,
      commands);
    if (!new TileChangeCommitSystem().TryCommit(
          world,
          commands,
          out TileChangeCommitResult commitResult))
    {
      throw new InvalidOperationException(commitResult.FailureReason);
    }

    CaptureStage(traceStages, WorldGenerationStage.Terrain, world, request, state);

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

    commands.Clear();
    new LegacyCavePassSystem().AppendCommands(
      world.CreateSnapshot(request.Metadata),
      request,
      ref state,
      commands);
    if (!new TileChangeCommitSystem().TryCommit(
          world,
          commands,
          out commitResult))
    {
      throw new InvalidOperationException(commitResult.FailureReason);
    }

    if (request.TerrainProfile is not null)
    {
      List<LegacyCaveCoordinate> caveHistory = new();
      AppendSmallHolesCommands(world, request, ref state);
      AppendSurfaceCavesCommands(world, request, ref state);
      AppendMountainCavesCommands(world, request, ref state, caveHistory);
      AppendWavyCavesCommands(world, request, ref state);
      AppendTunnelsCommands(world, request, ref state);
      AppendIceBiomeCommands(world, request, ref state);
      AppendMudCavesToJungleGrassCommands(world, request, ref state);
      AppendSiltCommands(world, request, ref state);
      AppendOresAndShiniesCommands(world, request, ref state);
      AppendWebsCommands(world, request, ref state, caveHistory);
      AppendUnderworldSurfaceCommands(world, request, ref state);
      AppendUnderworldLavaColumnCommands(world, request, ref state);
      AppendUnderworldLiquidCaveCommands(world, request, ref state);
      AppendUnderworldLowerCommands(world, request, ref state);
      AppendUnderworldLavaShelfCommands(world, request, ref state);
      AppendWorldInfectionCommands(world, request, ref state);
    }

    commands.Clear();
    if (request.DirtWallSurfaceOffsetChanges is not null || request.TerrainProfile is not null)
    {
      DirtWallBackgroundSystem dirtWallSystem = new();
      if (request.DirtWallSurfaceOffsetChanges is not null)
      {
        if (request.TerrainProfile is LegacyTerrainRuntimeProfile runtimeProfile)
        {
          dirtWallSystem.AppendCommands(
            world.CreateSnapshot(request.Metadata),
            runtimeProfile,
            request.DirtWallSurfaceOffsetChanges,
            ref state,
            commands);
        }
        else
        {
          dirtWallSystem.AppendCommands(
            world.CreateSnapshot(request.Metadata),
            request.SurfaceY,
            request.DirtWallSurfaceOffsetChanges,
            ref state,
            commands);
        }
      }
      else
      {
        LegacyTerrainRuntimeProfile runtimeProfile = request.TerrainProfile ??
          throw new InvalidOperationException(
            "DirtWallBackgrounds requires a terrain runtime profile or offset input.");
        dirtWallSystem.AppendCommands(
          world.CreateSnapshot(request.Metadata),
          runtimeProfile,
          new LegacyPassRandomState(request.Metadata.Seed.Value),
          ref state,
          commands);
      }

      if (!new TileChangeCommitSystem().TryCommit(world, commands, out commitResult))
      {
        throw new InvalidOperationException(commitResult.FailureReason);
      }

    }

    if (request.TerrainProfile is LegacyTerrainRuntimeProfile rocksProfile)
    {
      commands.Clear();
      LegacyRocksInDirtPass.AppendCommands(
        world.CreateSnapshot(request.Metadata),
        rocksProfile,
        new LegacyPassRandomState(request.Metadata.Seed.Value),
        isSkyblockWorld: request.IsSkyblockWorld,
        ref state,
        commands);
      if (!new TileChangeCommitSystem().TryCommit(world, commands, out commitResult))
      {
        throw new InvalidOperationException(commitResult.FailureReason);
      }

      commands.Clear();
      LegacyDirtInRocksPass.AppendCommands(
        world.CreateSnapshot(request.Metadata),
        rocksProfile,
        new LegacyPassRandomState(request.Metadata.Seed.Value),
        request.Metadata.IsRemixWorld ?? false,
        isSkyblockWorld: request.IsSkyblockWorld,
        ref state,
        commands);
      if (!new TileChangeCommitSystem().TryCommit(world, commands, out commitResult))
      {
        throw new InvalidOperationException(commitResult.FailureReason);
      }

      commands.Clear();
      LegacyClayPass.AppendCommands(
        world.CreateSnapshot(request.Metadata),
        rocksProfile,
        new LegacyPassRandomState(request.Metadata.Seed.Value),
        request.Metadata.IsRemixWorld ?? false,
        isSkyblockWorld: request.IsSkyblockWorld,
        ref state,
        commands);
      if (!new TileChangeCommitSystem().TryCommit(world, commands, out commitResult))
      {
        throw new InvalidOperationException(commitResult.FailureReason);
      }

      commands.Clear();
      LegacyRockLayerCavesPass.AppendCommands(
        world.CreateSnapshot(request.Metadata),
        rocksProfile,
        new LegacyPassRandomState(request.Metadata.Seed.Value),
        request.Metadata.IsRemixWorld ?? false,
        isSkyblockWorld: request.IsSkyblockWorld,
        ref state,
        commands);
      if (!new TileChangeCommitSystem().TryCommit(world, commands, out commitResult))
      {
        throw new InvalidOperationException(commitResult.FailureReason);
      }
    }

    CaptureStage(traceStages, WorldGenerationStage.Cave, world, request, state);

    commands.Clear();
    WorldGridSnapshot caveSnapshot = world.CreateSnapshot(request.Metadata);
    BiomeSurfaceResult biomeResult = new BiomeSurfaceSystem().AppendCommands(
      caveSnapshot,
      request.BiomeSurfaceDefinition is BiomeSurfaceDefinition biomeDefinition
        ? new BiomeSurfaceComponent(biomeDefinition, request.SurfaceY)
        : new BiomeSurfaceComponent("default"),
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
      SpawnProtectionHalfWidth,
      SpawnProtectionHeight);
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

    frameInput.AddRange(commands);

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
    new LiquidSourceSystem().AppendWorkItems(
      new[] { liquidSource },
      new WorldBoundsComponent(request.Metadata.Width, request.Metadata.Height),
      ref state,
      workItems);
    LiquidPropagationSession liquidSession = new(
      world,
      request.Metadata,
      new[] { water },
      Array.Empty<LiquidMergeComponent>(),
      state,
      workItems);
    while (liquidSession.PendingWorkItemCount > 0)
    {
      LiquidPropagationAdvanceResult propagationResult = liquidSession.Advance(budget: 128);
      if (!propagationResult.Succeeded)
      {
        throw new InvalidOperationException(propagationResult.FailureReason);
      }
    }

    state = liquidSession.State;
    workItems = new List<LiquidWorkItemComponent>();

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

    CaptureStage(
      traceStages,
      WorldGenerationStage.Framing,
      world,
      request,
      state,
      new TileFrameBudget(
        frameCommands.Count,
        frameCommitResult.AppliedCount,
        frameCommands.Count));

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
    WorldGenerationStateComponent state,
    TileFrameBudget? frameBudget = null)
  {
    traceStages?.Add(new WorldGenerationStageSnapshot(
      stage,
      world.CreateSnapshot(request.Metadata),
      state.NextSequence,
      frameBudget));
  }

  private static void EnsureSupportedRules(WorldGenerationRequest request)
  {
    if (request.SeedVariant != "default" ||
        request.Rules.SecretSeedVariant != "default" ||
        request.Rules.Difficulty != 0 ||
        request.Rules.IsHardmode)
    {
      throw new UnsupportedWorldGenerationRulesException(request);
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

  private static void AppendSmallHolesCommands(
    WorldGrid world,
    WorldGenerationRequest request,
    ref WorldGenerationStateComponent state)
  {
    LegacyTerrainRuntimeProfile profile = request.TerrainProfile ??
      throw new InvalidOperationException("SmallHoles requires a legacy terrain runtime profile.");
    LegacyTileRunnerLiquidContext liquidContext = new(
      profile.WaterLine,
      profile.LavaLine,
      LiquidType: 0,
      request.Metadata.IsRemixWorld ?? false,
      request.RockLayerY,
      request.Metadata.Height,
      IsOceanDepth: false);
    LegacySmallHolesPassDefinition definition =
      LegacySmallHolesPassDefinitionFactory.CreateDefault();
    List<TileChangeCommand> tileCommands = new();
    List<LiquidChangeCommand> liquidCommands = new();
    LegacySmallHolesPassExecution.AppendEnvelopeCommands(
      world.CreateSnapshot(request.Metadata),
      definition,
      new LegacyPassRandomState(request.Metadata.Seed.Value),
      liquidContext,
      request.SurfaceY,
      request.RockLayerY,
      definition.CalculateIterationCount(request.Metadata.Width, request.Metadata.Height),
      request.IsSkyblockWorld,
      ref state,
      tileCommands,
      liquidCommands);
    LegacyTileRunnerPassCommandBatch batch = new(
      "SmallHoles",
      tileCommands.AsReadOnly(),
      liquidCommands.AsReadOnly(),
      state.NextSequence);
    if (!LegacyTileRunnerCommandCommitBoundary.TryCommit(
          world,
          batch,
          new[]
          {
            new LiquidDefinition("water", 0, byte.MaxValue),
            new LiquidDefinition("lava", 1, byte.MaxValue)
          },
          out LegacyTileRunnerCommandCommitResult result))
    {
      throw new InvalidOperationException(result.FailureReason);
    }
  }

  private static void AppendSurfaceCavesCommands(
    WorldGrid world,
    WorldGenerationRequest request,
    ref WorldGenerationStateComponent state)
  {
    LegacyTerrainRuntimeProfile profile = request.TerrainProfile ??
      throw new InvalidOperationException(
        "SurfaceCaves requires a legacy terrain runtime profile.");
    List<TileChangeCommand> tileCommands = new();
    List<LiquidChangeCommand> liquidCommands = new();
    LegacySurfaceCavesPass.AppendCommands(
      world.CreateSnapshot(request.Metadata),
      profile,
      new LegacyPassRandomState(request.Metadata.Seed.Value),
      request.Metadata.IsRemixWorld ?? false,
      isSkyblockWorld: request.IsSkyblockWorld,
      isNoSurfaceWorld: false,
      ref state,
      tileCommands,
      liquidCommands);
    if (tileCommands.Count == 0 && liquidCommands.Count == 0)
    {
      return;
    }

    LegacyTileRunnerPassCommandBatch batch = new(
      "SurfaceCaves",
      tileCommands.AsReadOnly(),
      liquidCommands.AsReadOnly(),
      state.NextSequence);
    if (!LegacyTileRunnerCommandCommitBoundary.TryCommit(
          world,
          batch,
          new[] { new LiquidDefinition("water", 0, byte.MaxValue) },
          out LegacyTileRunnerCommandCommitResult result))
    {
      throw new InvalidOperationException(result.FailureReason);
    }
  }

  private static void AppendMountainCavesCommands(
    WorldGrid world,
    WorldGenerationRequest request,
    ref WorldGenerationStateComponent state,
    ICollection<LegacyCaveCoordinate> caveHistory)
  {
    LegacyTerrainRuntimeProfile profile = request.TerrainProfile ??
      throw new InvalidOperationException(
        "MountainCaves requires a legacy terrain runtime profile.");
    List<TileChangeCommand> tileCommands = new();
    LegacyMountainCavesPass.AppendCommands(
      world.CreateSnapshot(request.Metadata),
      profile,
      new LegacyPassRandomState(request.Metadata.Seed.Value),
      ref state,
      tileCommands,
      caveHistory,
      isRemixWorld: request.Metadata.IsRemixWorld ?? false,
      isSkyblockWorld: request.IsSkyblockWorld,
      isNoSurfaceWorld: false,
      isSurfaceDesertWorld: false);
    if (tileCommands.Count == 0)
    {
      return;
    }

    LegacyTileRunnerPassCommandBatch batch = new(
      "MountainCaves",
      tileCommands.AsReadOnly(),
      Array.Empty<LiquidChangeCommand>(),
      state.NextSequence);
    if (!LegacyTileRunnerCommandCommitBoundary.TryCommit(
          world,
          batch,
          Array.Empty<LiquidDefinition>(),
          out LegacyTileRunnerCommandCommitResult result))
    {
      throw new InvalidOperationException(result.FailureReason);
    }
  }

  private static void AppendWavyCavesCommands(
    WorldGrid world,
    WorldGenerationRequest request,
    ref WorldGenerationStateComponent state)
  {
    LegacyTerrainRuntimeProfile profile = request.TerrainProfile ??
      throw new InvalidOperationException(
        "WavyCaves requires a legacy terrain runtime profile.");
    List<TileChangeCommand> tileCommands = new();
    LegacyWavyCavesPass.AppendCommands(
      world.CreateSnapshot(request.Metadata),
      profile,
      new LegacyPassRandomState(request.Metadata.Seed.Value),
      LegacyWavyCavesPassDefinition.CreateDefault(),
      isRemixWorld: request.Metadata.IsRemixWorld ?? false,
      isSkyblockWorld: request.IsSkyblockWorld,
      isDontStarveWorld: request.IsDontStarveWorld,
      ref state,
      commands: tileCommands);
    if (tileCommands.Count == 0)
    {
      return;
    }

    LegacyTileRunnerPassCommandBatch batch = new(
      "WavyCaves",
      tileCommands.AsReadOnly(),
      Array.Empty<LiquidChangeCommand>(),
      state.NextSequence);
    if (!LegacyTileRunnerCommandCommitBoundary.TryCommit(
          world,
          batch,
          Array.Empty<LiquidDefinition>(),
          out LegacyTileRunnerCommandCommitResult result))
    {
      throw new InvalidOperationException(result.FailureReason);
    }
  }

  private static void AppendIceBiomeCommands(
    WorldGrid world,
    WorldGenerationRequest request,
    ref WorldGenerationStateComponent state)
  {
    if (!request.IsIceBiomeWorld)
    {
      return;
    }

    LegacyTerrainRuntimeProfile profile = request.TerrainProfile ??
      throw new InvalidOperationException(
        "IceBiome requires a legacy terrain runtime profile.");
    List<TileChangeCommand> tileCommands = new();
    LegacyIceBiomeSurfacePass.AppendCommands(
      world.CreateSnapshot(request.Metadata),
      profile,
      LegacyIceBiomeSurfaceDefinition.CreateDefault(),
      new LegacyPassRandomState(request.Metadata.Seed.Value),
      request.IsSkyblockWorld,
      request.Metadata.IsRemixWorld ?? false,
      ref state,
      tileCommands);
    if (tileCommands.Count == 0)
    {
      return;
    }

    if (!new TileChangeCommitSystem().TryCommit(
          world,
          tileCommands,
          out TileChangeCommitResult result))
    {
      throw new InvalidOperationException(result.FailureReason);
    }
  }

  private static void AppendTunnelsCommands(
    WorldGrid world,
    WorldGenerationRequest request,
    ref WorldGenerationStateComponent state)
  {
    LegacyTerrainRuntimeProfile profile = request.TerrainProfile ??
      throw new InvalidOperationException(
        "Tunnels requires a legacy terrain runtime profile.");
    List<TileChangeCommand> tileCommands = new();
    List<LiquidChangeCommand> liquidCommands = new();
    LegacyTunnelsPass.AppendCommands(
      world.CreateSnapshot(request.Metadata),
      profile,
      new LegacyPassRandomState(request.Metadata.Seed.Value),
      request.Metadata.IsRemixWorld ?? false,
      request.IsSkyblockWorld,
      request.IsNoSurfaceWorld,
      request.IsSurfaceDesertWorld,
      request.IsTenthAnniversaryWorld,
      ref state,
      tileCommands,
      liquidCommands);
    if (tileCommands.Count == 0 && liquidCommands.Count == 0)
    {
      return;
    }

    LegacyTileRunnerPassCommandBatch batch = new(
      "Tunnels",
      tileCommands.AsReadOnly(),
      liquidCommands.AsReadOnly(),
      state.NextSequence);
    if (!LegacyTileRunnerCommandCommitBoundary.TryCommit(
          world,
          batch,
          Array.Empty<LiquidDefinition>(),
          out LegacyTileRunnerCommandCommitResult result))
    {
      throw new InvalidOperationException(result.FailureReason);
    }
  }

  private static void AppendMudCavesToJungleGrassCommands(
    WorldGrid world,
    WorldGenerationRequest request,
    ref WorldGenerationStateComponent state)
  {
    if (request.TerrainProfile is null)
    {
      throw new InvalidOperationException(
        "MudCavesToJungleGrass requires a legacy terrain runtime profile.");
    }
    List<TileChangeCommand> tileCommands = new();
    LegacyMudCavesToJungleGrassPass.AppendCommands(
      world.CreateSnapshot(request.Metadata),
      request.IsSkyblockWorld,
      ref state,
      tileCommands);
    if (tileCommands.Count == 0)
    {
      return;
    }

    if (!new TileChangeCommitSystem().TryCommit(
          world,
          tileCommands,
          out TileChangeCommitResult result))
    {
      throw new InvalidOperationException(result.FailureReason);
    }
  }

  private static void AppendSiltCommands(
    WorldGrid world,
    WorldGenerationRequest request,
    ref WorldGenerationStateComponent state)
  {
    LegacyTerrainRuntimeProfile profile = request.TerrainProfile ??
      throw new InvalidOperationException("Silt requires a legacy terrain runtime profile.");
    LegacyPassRandomState random = new(request.Metadata.Seed.Value);
    IReadOnlyList<LegacyTileRunnerPassInvocation> invocations =
      LegacySiltPass.CreateInvocationRecords(
        world.CreateSnapshot(request.Metadata),
        LegacySiltPassDefinition.CreateDefault(),
        LegacySiltPassDefinition.CreateSecondary(),
        random,
        (int)profile.RockLayerHigh,
        (int)profile.WorldSurface,
        (int)profile.RockLayer,
        request.Metadata.IsRemixWorld ?? false,
        request.IsSkyblockWorld);
    if (invocations.Count == 0)
    {
      return;
    }

    List<TileChangeCommand> tileCommands = new();
    Dictionary<(int X, int Y), WorldTile> projectedTiles = new();
    WorldGridSnapshot snapshot = world.CreateSnapshot(request.Metadata);
    foreach (LegacyTileRunnerPassInvocation invocation in invocations)
    {
      LegacyTileRunnerTraversal.AppendCommands(
        snapshot,
        invocation,
        random,
        request.SurfaceY,
        request.RockLayerY,
        ref state,
        tileCommands,
        projectedTiles: projectedTiles);
    }

    LegacyTileRunnerPassCommandBatch batch = new(
      "Silt",
      tileCommands.AsReadOnly(),
      Array.Empty<LiquidChangeCommand>(),
      state.NextSequence);
    if (!LegacyTileRunnerCommandCommitBoundary.TryCommit(
          world,
          batch,
          Array.Empty<LiquidDefinition>(),
          out LegacyTileRunnerCommandCommitResult result))
    {
      throw new InvalidOperationException(result.FailureReason);
    }
  }

  private static void AppendOresAndShiniesCommands(
    WorldGrid world,
    WorldGenerationRequest request,
    ref WorldGenerationStateComponent state)
  {
    if (request.IsSkyblockWorld)
    {
      return;
    }

    LegacyPassRandomState random = new(request.Metadata.Seed.Value);
    List<TileChangeCommand> commands = new();
    LegacyOresAndShiniesPass.AppendCommands(
      world.CreateSnapshot(request.Metadata),
      request.TerrainProfile ?? throw new InvalidOperationException(
        "OresAndShinies requires a legacy terrain runtime profile."),
      random,
      request.Metadata.IsRemixWorld ?? false,
      isDrunkWorld: false,
      request.IsSkyblockWorld,
      ref state,
      commands);
    if (commands.Count == 0)
    {
      return;
    }

    LegacyTileRunnerPassCommandBatch batch = new(
      "OresAndShinies",
      commands.AsReadOnly(),
      Array.Empty<LiquidChangeCommand>(),
      state.NextSequence);
    if (!LegacyTileRunnerCommandCommitBoundary.TryCommit(
          world,
          batch,
          Array.Empty<LiquidDefinition>(),
          out LegacyTileRunnerCommandCommitResult result))
    {
      throw new InvalidOperationException(result.FailureReason);
    }
  }

  private static void AppendWebsCommands(
    WorldGrid world,
    WorldGenerationRequest request,
    ref WorldGenerationStateComponent state,
    IReadOnlyList<LegacyCaveCoordinate> caveHistory)
  {
    if (request.IsSkyblockWorld)
    {
      return;
    }

    LegacyTerrainRuntimeProfile profile = request.TerrainProfile ??
      throw new InvalidOperationException("Webs requires a legacy terrain runtime profile.");
    LegacyPassRandomState random = new(request.Metadata.Seed.Value);
    IReadOnlyList<LegacyTileRunnerPassInvocation> invocations = LegacyWebsPass.CreateInvocations(
      world.CreateSnapshot(request.Metadata),
      LegacyWebsPassDefinitionFactory.CreateDefault(),
      random,
      (int)profile.WorldSurface,
      (int)profile.WorldSurfaceLow,
      (int)profile.WorldSurfaceHigh,
      request.Metadata.Height - 20,
      LegacyWebsPassDefinitionFactory.CreateDefault().CalculateIterationCount(
        request.Metadata.Width,
        request.Metadata.Height),
      caveHistory);
    if (invocations.Count == 0)
    {
      return;
    }

    List<TileChangeCommand> tileCommands = new();
    Dictionary<(int X, int Y), WorldTile> projectedTiles = new();
    WorldGridSnapshot snapshot = world.CreateSnapshot(request.Metadata);
    foreach (LegacyTileRunnerPassInvocation invocation in invocations)
    {
      LegacyTileRunnerTraversal.AppendCommands(
        snapshot,
        invocation,
        random,
        request.SurfaceY,
        request.RockLayerY,
        ref state,
        tileCommands,
        projectedTiles: projectedTiles);
    }

    LegacyTileRunnerPassCommandBatch batch = new(
      "Webs",
      tileCommands.AsReadOnly(),
      Array.Empty<LiquidChangeCommand>(),
      state.NextSequence);
    if (!LegacyTileRunnerCommandCommitBoundary.TryCommit(
          world,
          batch,
          Array.Empty<LiquidDefinition>(),
          out LegacyTileRunnerCommandCommitResult result))
    {
      throw new InvalidOperationException(result.FailureReason);
    }
  }

  private static void AppendUnderworldSurfaceCommands(
    WorldGrid world,
    WorldGenerationRequest request,
    ref WorldGenerationStateComponent state)
  {
    if (request.IsSkyblockWorld)
    {
      return;
    }

    List<TileChangeCommand> tileCommands = new();
    LegacyUnderworldSurfacePass.AppendCommands(
      world.CreateSnapshot(request.Metadata),
      new LegacyPassRandomState(request.Metadata.Seed.Value),
      ref state,
      tileCommands,
      isNotTheBeesWorld: false,
      isSkyblockWorld: request.IsSkyblockWorld);
    if (tileCommands.Count == 0)
    {
      return;
    }

    if (!new TileChangeCommitSystem().TryCommit(
          world,
          tileCommands,
          out TileChangeCommitResult result))
    {
      throw new InvalidOperationException(result.FailureReason);
    }
  }

  private static void AppendUnderworldLavaColumnCommands(
    WorldGrid world,
    WorldGenerationRequest request,
    ref WorldGenerationStateComponent state)
  {
    if (request.IsSkyblockWorld)
    {
      return;
    }

    List<LiquidChangeCommand> liquidCommands = new();
    LegacyUnderworldLavaColumnPass.AppendCommands(
      world.CreateSnapshot(request.Metadata),
      new LegacyPassRandomState(request.Metadata.Seed.Value),
      ref state,
      liquidCommands,
      request.IsSkyblockWorld);
    if (liquidCommands.Count == 0)
    {
      return;
    }

    if (!new LiquidChangeCommitSystem().TryCommit(
          world,
          liquidCommands,
          new[] { new LiquidDefinition("lava", 1, byte.MaxValue) },
          out LiquidChangeCommitResult result))
    {
      throw new InvalidOperationException(result.FailureReason);
    }
  }

  private static void AppendUnderworldLiquidCaveCommands(
    WorldGrid world,
    WorldGenerationRequest request,
    ref WorldGenerationStateComponent state)
  {
    if (request.IsSkyblockWorld)
    {
      return;
    }

    LegacyPassRandomState random = new(request.Metadata.Seed.Value);
    List<TileChangeCommand> tileCommands = new();
    LegacyUnderworldLiquidCavePass.AppendCommands(
      world.CreateSnapshot(request.Metadata),
      random,
      request.SurfaceY,
      request.RockLayerY,
      ref state,
      tileCommands,
      isSkyblockWorld: request.IsSkyblockWorld);
    if (tileCommands.Count == 0)
    {
      return;
    }

    if (!new TileChangeCommitSystem().TryCommit(
          world,
          tileCommands,
          out TileChangeCommitResult result))
    {
      throw new InvalidOperationException(result.FailureReason);
    }
  }

  private static void AppendUnderworldLowerCommands(
    WorldGrid world,
    WorldGenerationRequest request,
    ref WorldGenerationStateComponent state)
  {
    if (request.IsSkyblockWorld)
    {
      return;
    }

    List<TileChangeCommand> tileCommands = new();
    LegacyUnderworldLowerPass.AppendCommands(
      world.CreateSnapshot(request.Metadata),
      new LegacyPassRandomState(request.Metadata.Seed.Value),
      request.SurfaceY,
      request.RockLayerY,
      ref state,
      tileCommands,
      isSkyblockWorld: request.IsSkyblockWorld);
    if (tileCommands.Count == 0)
    {
      return;
    }

    if (!new TileChangeCommitSystem().TryCommit(
          world,
          tileCommands,
          out TileChangeCommitResult result))
    {
      throw new InvalidOperationException(result.FailureReason);
    }
  }

  private static void AppendUnderworldLavaShelfCommands(
    WorldGrid world,
    WorldGenerationRequest request,
    ref WorldGenerationStateComponent state)
  {
    if (request.IsSkyblockWorld)
    {
      return;
    }

    List<LiquidChangeCommand> liquidCommands = new();
    LegacyUnderworldLavaShelfPass.AppendCommands(
      world.CreateSnapshot(request.Metadata),
      ref state,
      liquidCommands,
      request.IsSkyblockWorld);
    if (liquidCommands.Count == 0)
    {
      return;
    }

    if (!new LiquidChangeCommitSystem().TryCommit(
          world,
          liquidCommands,
          new[] { new LiquidDefinition("lava", 1, byte.MaxValue) },
          out LiquidChangeCommitResult result))
    {
      throw new InvalidOperationException(result.FailureReason);
    }
  }

  private static void AppendWorldInfectionCommands(
    WorldGrid world,
    WorldGenerationRequest request,
    ref WorldGenerationStateComponent state)
  {
    if (request.IsSkyblockWorld || request.WorldGenParamEvil < 0)
    {
      return;
    }

    int underworldLayerY = request.Metadata.Height - 200;
    if (underworldLayerY <= request.RockLayerY)
    {
      return;
    }

    LegacyWorldInfectionConversionInput input = new(
      request.SurfaceY,
      request.RockLayerY,
      underworldLayerY,
      NoInfection: false,
      request.IsNoSurfaceWorld,
      HallowOnSurface: false,
      DrunkWorld: false,
      Crimson: request.WorldGenParamEvil == 1,
      CrimsonLeft: false,
      request.IsSkyblockWorld);
    WorldGridSnapshot snapshot = world.CreateSnapshot(request.Metadata);
    if (!LegacyWorldInfectionConversionPass.TryCommit(
          world,
          snapshot,
          input,
          new LegacyPassRandomState(request.Metadata.Seed.Value),
          ref state,
          out LegacyWorldInfectionConversionCommitResult result))
    {
      throw new InvalidOperationException(result.FailureReason);
    }
  }

}
