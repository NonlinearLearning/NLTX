using System;
using System.Collections.Generic;
using System.IO;

namespace Terraria.Dome.Server.Startup;

public sealed record ServerLaunchOptions(
  string WorldPath,
  int Port,
  int NpcStreamSpeed = NpcStreamSpeedPolicy.DefaultTicks,
  int MaximumConnections = byte.MaxValue)
{
  public static ServerLaunchOptions Parse(IReadOnlyList<string> args)
  {
    ArgumentNullException.ThrowIfNull(args);
    string? worldPath = null;
    int? port = null;
    int npcStreamSpeed = NpcStreamSpeedPolicy.DefaultTicks;
    bool hasNpcStreamSpeed = false;
    int maximumConnections = byte.MaxValue;
    bool hasMaximumConnections = false;
    for (int index = 0; index < args.Count; index++)
    {
      string argument = args[index];
      switch (argument)
      {
        case "--world":
          if (worldPath is not null || ++index >= args.Count ||
              string.IsNullOrWhiteSpace(args[index]))
          {
            throw new ArgumentException("--world must be provided exactly once.", nameof(args));
          }

          worldPath = args[index];
          break;
        case "--port":
          if (port.HasValue || ++index >= args.Count ||
              !int.TryParse(args[index], out int parsedPort))
          {
            throw new ArgumentException("--port must be provided exactly once.", nameof(args));
          }

          port = parsedPort;
          break;
        case "--npc-stream-speed":
          if (hasNpcStreamSpeed || ++index >= args.Count ||
              !int.TryParse(args[index], out int parsedSpeed))
          {
            throw new ArgumentException(
              "--npc-stream-speed must be provided exactly once as an integer.",
              nameof(args));
          }

          hasNpcStreamSpeed = true;
          npcStreamSpeed = NpcStreamSpeedPolicy.Normalize(parsedSpeed);
          break;
        case "--max-net-players":
          if (hasMaximumConnections || ++index >= args.Count ||
              !int.TryParse(args[index], out int parsedMaximumConnections))
          {
            throw new ArgumentException(
              "--max-net-players must be provided at most once as an integer.",
              nameof(args));
          }

          if (parsedMaximumConnections is < 1 or > byte.MaxValue)
          {
            throw new ArgumentOutOfRangeException(
              nameof(args), "--max-net-players must be between 1 and 255.");
          }

          hasMaximumConnections = true;
          maximumConnections = parsedMaximumConnections;
          break;
        default:
          throw new ArgumentException($"Unknown server argument '{argument}'.", nameof(args));
      }
    }

    if (worldPath is null || !port.HasValue)
    {
      throw new ArgumentException("Both --world and --port are required.", nameof(args));
    }

    if (!Path.IsPathFullyQualified(worldPath) ||
        !string.Equals(Path.GetExtension(worldPath), ".wld", StringComparison.OrdinalIgnoreCase))
    {
      throw new ArgumentException("--world must be an absolute .wld path.", nameof(args));
    }

    if (port.Value is < 1 or > 65535 || port.Value == 7778)
    {
      throw new ArgumentOutOfRangeException(nameof(args), "The requested port is not allowed.");
    }

    return new ServerLaunchOptions(
      Path.GetFullPath(worldPath),
      port.Value,
      npcStreamSpeed,
      maximumConnections);
  }
}
