using System;
using System.IO;
using System.Threading;
using Terraria.Dome.Server;
using Terraria.Dome.Server.Startup;
using Terraria.Dome.Simulation;

try
{
  ServerLaunchOptions options = ServerLaunchOptions.Parse(args);
  WorldBootstrapResult bootstrap = LoadWorldBootstrap(options.WorldPath);
  using DomeServer server = new(bootstrap);
  server.ConfigureWorldPath(options.WorldPath);
  server.ConfigureMaximumConnections(options.MaximumConnections);
  GC.Collect();
  GC.WaitForPendingFinalizers();
  GC.Collect();
  server.Start(options.Port);
  Console.WriteLine(
    $"Terraria Dome server listening on {server.Port}; " +
    $"imported world {server.World.Width}x{server.World.Height}.");
  using ManualResetEventSlim shutdown = new(initialState: false);
  ConsoleCancelEventHandler cancelHandler = (_, eventArgs) =>
  {
    eventArgs.Cancel = true;
    shutdown.Set();
  };
  Console.CancelKeyPress += cancelHandler;
  try
  {
    shutdown.Wait();
  }
  finally
  {
    Console.CancelKeyPress -= cancelHandler;
  }
  return 0;
}
catch (Exception exception) when (exception is ArgumentException ||
                                  exception is IOException ||
                                  exception is InvalidDataException ||
                                  exception is InvalidOperationException)
{
  Console.Error.WriteLine($"World import failed: {exception.Message}");
  return 1;
}

static WorldBootstrapResult LoadWorldBootstrap(string worldPath)
{
  WorldBootstrapResult bootstrap = WorldBootstrap.Load(worldPath);
  DomeSimulationSnapshot snapshot = bootstrap.Snapshot;
  Console.WriteLine(
    $"Imported world metadata: {snapshot.World.Metadata.Name} " +
    $"{snapshot.World.Metadata.Width}x{snapshot.World.Metadata.Height}, " +
    $"worldId={snapshot.World.Metadata.WorldId}, " +
    $"spawn=({snapshot.World.Metadata.SpawnX},{snapshot.World.Metadata.SpawnY}).");
  return bootstrap;
}
