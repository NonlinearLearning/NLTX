using System;
using System.Text.Json;
using System.Threading;
using Terraria.Dome.RealClientFixtureHost;
using Terraria.Dome.Server;
using Terraria.Dome.Server.Import;
using Terraria.Dome.Simulation.WorldModel;

FixtureHostOptions options = FixtureHostOptions.Parse(args);
FixtureHostOptionsVerification.Verify();
using DomeServer server = CreateServer(options);

int doorId = 0;
int signId = 0;
int chestId = 0;
if (options.WorldPath is null)
{
  doorId = server.CreateDoor(
    FixtureHostOptionsVerification.DoorTileX,
    FixtureHostOptionsVerification.DoorTileY);
  signId = server.CreateSign(
    FixtureHostOptionsVerification.SignTileX,
    FixtureHostOptionsVerification.SignTileY,
    "Fixture Sign");
  chestId = server.CreateChest(
    FixtureHostOptionsVerification.ChestTileX,
    FixtureHostOptionsVerification.ChestTileY);
  if (!server.World.TrySetTile(
        FixtureHostOptionsVerification.AnchorTileX,
        FixtureHostOptionsVerification.AnchorTileY,
        new WorldTile(IsActive: true, Type: 1)))
  {
    throw new InvalidOperationException("Fixture host could not create the collision anchor.");
  }
}

server.Start(options.Port);
FixtureHostReadyRecord readyRecord = new(
  server.Port,
  doorId,
  signId,
  chestId,
  FixtureHostOptionsVerification.DoorTileX,
  FixtureHostOptionsVerification.DoorTileY,
  FixtureHostOptionsVerification.AnchorTileX,
  FixtureHostOptionsVerification.AnchorTileY,
  FixtureHostOptionsVerification.SignTileX,
  FixtureHostOptionsVerification.SignTileY,
  FixtureHostOptionsVerification.ChestTileX,
  FixtureHostOptionsVerification.ChestTileY);
if (options.WorldPath is null)
{
  FixtureHostOptionsVerification.VerifyReadyRecord(readyRecord);
}
Console.WriteLine($"READY {JsonSerializer.Serialize(readyRecord)}");

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

static DomeServer CreateServer(FixtureHostOptions options)
{
  if (options.WorldPath is null)
  {
    return new DomeServer();
  }

  DomeWorldImportResult import = new DomeWorldImportApplier().Import(options.WorldPath);
  Console.WriteLine(
    $"Imported test world: {import.Snapshot.World.Metadata.Name} " +
    $"{import.Snapshot.World.Metadata.Width}x{import.Snapshot.World.Metadata.Height}, " +
    $"spawn=({import.Snapshot.World.Metadata.SpawnX},{import.Snapshot.World.Metadata.SpawnY}).");
  return new DomeServer(import.Snapshot);
}
