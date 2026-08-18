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
  bool UsesDefaultWorld);

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
    WorldMetadata metadata = new(
      DefaultWorldName,
      new WorldSeed(DefaultWorldSeed),
      DefaultWorldWidth,
      DefaultWorldHeight,
      spawnX: DefaultSpawnX,
      spawnY: DefaultSurfaceY);
    WorldGenerationRequest request = new(metadata, DefaultSpawnX, DefaultSurfaceY);
    WorldGrid world = new WorldGenerationPipeline().Generate(request);
    using DomeSimulation simulation = new(world);
    CreateDefaultChests(simulation);
    return new WorldBootstrapResult(
      simulation.CreatePersistenceSnapshot(metadata),
      UsesDefaultWorld: true);
  }

  public static WorldBootstrapResult Load(string worldPath, bool strictImport = true)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(worldPath);
    if (!Path.IsPathFullyQualified(worldPath))
    {
      throw new ArgumentException("The world path must be absolute.", nameof(worldPath));
    }

    DomeWorldImportResult imported = new DomeWorldImportApplier().Import(worldPath, strictImport);
    return new WorldBootstrapResult(imported.Snapshot, UsesDefaultWorld: false);
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

  private readonly record struct DefaultChestSpawn(int TileX, int TileY);
}
