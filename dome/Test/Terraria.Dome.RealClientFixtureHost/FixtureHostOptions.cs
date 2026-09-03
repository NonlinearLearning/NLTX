using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Terraria.Dome.RealClientFixtureHost;

public sealed class FixtureHostOptions
{
  private const int ProtectedSharedServerPort = 7778;

  private FixtureHostOptions(string? worldPath, int port)
  {
    WorldPath = worldPath;
    Port = port;
  }

  public string? WorldPath { get; }

  public int Port { get; }

  public static FixtureHostOptions Parse(IReadOnlyList<string> args)
  {
    ArgumentNullException.ThrowIfNull(args);

    string? worldPath = null;
    int? port = null;
    for (int index = 0; index < args.Count; index++)
    {
      switch (args[index])
      {
        case "--world":
          if (worldPath is not null || ++index >= args.Count ||
              string.IsNullOrWhiteSpace(args[index]) ||
              !Path.IsPathFullyQualified(args[index]))
          {
            throw new ArgumentException("--world must be provided at most once.", nameof(args));
          }

          worldPath = Path.GetFullPath(args[index]);
          if (!string.Equals(
                Path.GetExtension(worldPath),
                ".wld",
                StringComparison.OrdinalIgnoreCase))
          {
            throw new ArgumentException("--world must be an absolute .wld path.", nameof(args));
          }

          break;
        case "--port":
          if (port.HasValue || ++index >= args.Count ||
              !int.TryParse(
                args[index],
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out int parsedPort))
          {
            throw new ArgumentException(
              "Exactly one valid --port value is required.",
              nameof(args));
          }

          port = parsedPort;
          break;
        default:
          throw new ArgumentException(
            $"Unknown fixture host argument '{args[index]}'.",
            nameof(args));
      }
    }

    if (!port.HasValue || port.Value is < 1 or > 65535)
    {
      throw new ArgumentException("Exactly one valid --port value is required.", nameof(args));
    }

    if (port.Value == ProtectedSharedServerPort)
    {
      throw new ArgumentException("Port 7778 is reserved for the shared server.", nameof(args));
    }

    return new FixtureHostOptions(worldPath, port.Value);
  }
}
