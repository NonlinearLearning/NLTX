using System;
using System.IO;
using Terraria.Dome.Server.Import;
using Terraria.Dome.Simulation;
using Terraria.Dome.Simulation.Items;
using Terraria.Dome.Simulation.WorldGeneration;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Server.Startup;

public sealed record WorldBootstrapResult(
  DomeSimulationSnapshot Snapshot,
  bool UsesDefaultWorld,
  WorldEntityLimits EntityLimits)
{
  public long InitialSnapshotRevision => Snapshot.TickNumber;
  public long FirstScheduledTick => checked(Snapshot.TickNumber + 1);
}

public static class WorldBootstrap
{
  internal const int DefaultSpawnX = 2100;
  internal const int DefaultSurfaceY = 300;

  private const ushort DefaultChestItemType = 1;
  private const int DefaultChestItemQuantity = 1;
  private const int DefaultWorldHeight = 1200;
  private const string DefaultWorldName = "Dome World";
  private const int DefaultWorldSeed = 1456;
  private const int DefaultWorldWidth = 4200;

  private static readonly DefaultChestSpawn[] DefaultChestSpawns =
  [
    new(1700, 250), new(1800, 250), new(1900, 250), new(2000, 250), new(2100, 250),
    new(2200, 250), new(2300, 250), new(2400, 250), new(2500, 250), new(1700, 350),
    new(1800, 350), new(1900, 350), new(2000, 350), new(2100, 350), new(2200, 350),
    new(2300, 350), new(2400, 350), new(2500, 350), new(1750, 400), new(1875, 400),
    new(2000, 400), new(2125, 400), new(2250, 400)
  ];

  public static WorldBootstrapResult CreateDefault()
  {
    WorldBootstrapRequest bootstrapRequest = WorldBootstrapRequest.Create(
      DefaultWorldName,
      new WorldSeed(DefaultWorldSeed),
      DefaultWorldWidth,
      DefaultWorldHeight,
      DefaultSpawnX,
      DefaultSurfaceY,
      new WorldRuleState());
    WorldMetadata metadata = bootstrapRequest.Metadata;
    WorldGrid world = GenerateWorld(bootstrapRequest);
    using DomeSimulation simulation = new(world, metadata.Seed);
    CreateDefaultChests(simulation);
    return new WorldBootstrapResult(
      simulation.CreatePersistenceSnapshot(metadata),
      UsesDefaultWorld: true,
      EntityLimits: bootstrapRequest.EntityLimits);
  }

  public static WorldBootstrapResult Create(WorldBootstrapRequest request)
  {
    ArgumentNullException.ThrowIfNull(request);
    if (request.GenerationStatus != WorldBootstrapGenerationStatus.Generated)
    {
      throw new ArgumentException(
        "A generated request is required for a new world bootstrap.",
        nameof(request));
    }

    WorldGrid world = GenerateWorld(request);
    using DomeSimulation simulation = new(
      world,
      request.Metadata.Seed,
      ToSimulationLimits(request.EntityLimits));
    return new WorldBootstrapResult(
      simulation.CreatePersistenceSnapshot(request.Metadata, request.Rules),
      UsesDefaultWorld: false,
      EntityLimits: request.EntityLimits);
  }

  public static WorldBootstrapResult Restore(
    WorldBootstrapRequest request,
    DomeSimulationSnapshot snapshot)
  {
    ArgumentNullException.ThrowIfNull(request);
    ArgumentNullException.ThrowIfNull(snapshot);
    if (snapshot.World.Metadata != request.Metadata || snapshot.WorldRules != request.Rules)
    {
      throw new ArgumentException(
        "The restore snapshot does not match the bootstrap identity or rules.",
        nameof(snapshot));
    }

    using DomeSimulation simulation = new(
      WorldGrid.FromSnapshot(snapshot.World),
      snapshot,
      ToSimulationLimits(request.EntityLimits));
    return new WorldBootstrapResult(
      snapshot,
      UsesDefaultWorld: false,
      EntityLimits: request.EntityLimits);
  }

  public static WorldBootstrapResult Load(string worldPath, bool strictImport = true)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(worldPath);
    if (!Path.IsPathFullyQualified(worldPath))
    {
      throw new ArgumentException("The world path must be absolute.", nameof(worldPath));
    }

    DomeWorldImportResult imported = new DomeWorldImportApplier().Import(worldPath, strictImport);
    return new WorldBootstrapResult(
      imported.Snapshot,
      UsesDefaultWorld: false,
      EntityLimits: new WorldEntityLimits());
  }

  private static void CreateDefaultChests(DomeSimulation simulation)
  {
    for (int index = 0; index < DefaultChestSpawns.Length; index++)
    {
      DefaultChestSpawn spawn = DefaultChestSpawns[index];
      int chestId = simulation.CreateChest(spawn.TileX, spawn.TileY);
      simulation.SetChestItem(
        chestId,
        chestSlot: 0,
        new ItemStack(DefaultChestItemType, DefaultChestItemQuantity));
    }
  }

  private static WorldGrid GenerateWorld(WorldBootstrapRequest request)
  {
    WorldGenerationRequest generationRequest = new(
      request.Metadata,
      request.Metadata.SpawnX,
      request.Metadata.SpawnY);
    return new WorldGenerationPipeline().Generate(generationRequest);
  }

  private static SimulationEntityLimits ToSimulationLimits(WorldEntityLimits limits)
  {
    return new SimulationEntityLimits(
      limits.MaximumPlayers,
      limits.MaximumNpcs,
      limits.MaximumProjectiles,
      limits.MaximumWorldItems,
      limits.MaximumChests);
  }

  private readonly record struct DefaultChestSpawn(int TileX, int TileY);
}
