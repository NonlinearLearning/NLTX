using System;
using System.Collections.Generic;
using System.IO;

namespace Terraria.Dome.RealClientFixtureHost;

internal static class FixtureHostOptionsVerification
{
  internal const int DoorTileX = 2100;
  internal const int DoorTileY = 300;
  internal const int AnchorTileX = DoorTileX;
  internal const int AnchorTileY = DoorTileY - 1;
  internal const int SignTileX = 2102;
  internal const int SignTileY = 300;
  internal const int ChestTileX = 2104;
  internal const int ChestTileY = 300;

  public static void Verify()
  {
    FixtureHostOptions options = FixtureHostOptions.Parse(["--port", "7845"]);
    if (options.Port != 7845 || options.WorldPath is not null)
    {
      throw new InvalidOperationException("Fixture host did not retain its port.");
    }

    string worldPath = Path.Combine(Path.GetTempPath(), "fixture-test.wld");
    FixtureHostOptions imported = FixtureHostOptions.Parse(
      ["--world", worldPath, "--port", "7845"]);
    if (imported.Port != 7845 || !Path.IsPathFullyQualified(imported.WorldPath!))
    {
      throw new InvalidOperationException("Fixture host did not retain its world path.");
    }

    VerifyRejected([]);
    VerifyRejected(["--port"]);
    VerifyRejected(["--port", "7845", "--port", "7846"]);
    VerifyRejected(["--port", "invalid"]);
    VerifyRejected(["--port", "0"]);
    VerifyRejected(["--port", "7778"]);
    VerifyRejected(["--world", "relative.wld", "--port", "7845"]);
    VerifyRejected(["--world", worldPath, "--world", worldPath, "--port", "7845"]);
    VerifyRejected(["--unknown", "7845"]);
  }

  public static void VerifyReadyRecord(FixtureHostReadyRecord ready)
  {
    ArgumentNullException.ThrowIfNull(ready);

    if (ready.DoorTileX != DoorTileX ||
        ready.DoorTileY != DoorTileY ||
        ready.AnchorTileX != AnchorTileX ||
        ready.AnchorTileY != AnchorTileY ||
        ready.SignTileX != SignTileX ||
        ready.SignTileY != SignTileY ||
        ready.ChestTileX != ChestTileX ||
        ready.ChestTileY != ChestTileY)
    {
      throw new InvalidOperationException(
        "Fixture host did not report its expected object coordinates.");
    }
  }

  private static void VerifyRejected(IReadOnlyList<string> args)
  {
    try
    {
      _ = FixtureHostOptions.Parse(args);
    }
    catch (ArgumentException)
    {
      return;
    }

    throw new InvalidOperationException("Fixture host accepted invalid options.");
  }
}

internal sealed record FixtureHostReadyRecord(
  int Port,
  int DoorId,
  int SignId,
  int ChestId,
  int DoorTileX,
  int DoorTileY,
  int AnchorTileX,
  int AnchorTileY,
  int SignTileX,
  int SignTileY,
  int ChestTileX,
  int ChestTileY);
