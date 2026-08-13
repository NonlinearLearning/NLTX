using System;
using Terraria.Dome.Client;
using Terraria.Dome.Transport;

if (args.Length != 1 || !int.TryParse(args[0], out int port))
{
  Console.Error.WriteLine("Usage: Terraria.Dome.Client <port>");
  return;
}

using DomeClient client = new();
await client.ConnectAsync(port);
Console.WriteLine("Commands: a, d, j, f, q");
while (true)
{
  string? command = Console.ReadLine();
  if (command is null || command.Equals("q", StringComparison.OrdinalIgnoreCase))
  {
    break;
  }

  ClientInputFrame frame = command.ToLowerInvariant() switch
  {
    "a" => new ClientInputFrame(MoveLeft: true),
    "d" => new ClientInputFrame(MoveRight: true),
    "j" => new ClientInputFrame(Jump: true),
    "f" => new ClientInputFrame(Fire: true),
    _ => new ClientInputFrame()
  };
  ServerSnapshot snapshot = await client.SendAsync(frame);
  Console.WriteLine(
    $"tick={snapshot.Tick} player=({snapshot.Player.X}, {snapshot.Player.Y}) " +
    $"npcHealth={snapshot.NpcHealth} projectiles={snapshot.ProjectileCount}");
}
