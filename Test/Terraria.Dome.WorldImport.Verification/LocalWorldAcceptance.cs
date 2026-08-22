using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using Terraria.Dome.Server;
using Terraria.Dome.Server.Import;
using Terraria.WorldCompatibility.Model;
using Terraria.WorldFile.V319.Model;

internal static class LocalWorldAcceptance
{
  public static void RunIfConfigured()
  {
    string? path = Environment.GetEnvironmentVariable("TERRARIA_WLD_ACCEPTANCE_PATH");
    if (string.IsNullOrWhiteSpace(path))
    {
      return;
    }

    if (!Path.IsPathFullyQualified(path))
    {
      throw new InvalidOperationException("The acceptance world path must be absolute.");
    }

    DomeWorldImportResult result = new DomeWorldImportApplier().Import(path);
    if (result.Document.Version != 319 || result.Document.Metadata.Width <= 0 ||
        result.Document.Metadata.Height <= 0 || result.Compatibility.Tiles.Count == 0)
    {
      throw new InvalidOperationException("The configured v319 world did not produce valid state.");
    }

    VerifyRepresentativeTileProjection(result);

    using TcpListener probe = new(IPAddress.Loopback, 0);
    probe.Start();
    int port = ((IPEndPoint)probe.LocalEndpoint).Port;
    probe.Stop();
    if (port is 7777 or 7778)
    {
      throw new InvalidOperationException("The acceptance verifier selected a protected port.");
    }

    using DomeServer server = new(result.Snapshot);
    server.Start(port);
    if (server.Port != port)
    {
      throw new InvalidOperationException(
        "The configured world did not start on the temporary port.");
    }
  }

  private static void VerifyRepresentativeTileProjection(DomeWorldImportResult result)
  {
    int width = result.Document.Metadata.Width;
    int height = result.Document.Metadata.Height;
    (int X, int Y)[] coordinates =
    [
      (0, 0),
      (0, height - 1),
      (width - 1, 0),
      (width - 1, height - 1),
      (width / 2, height / 2)
    ];
    foreach ((int x, int y) in coordinates)
    {
      int index = checked((x * height) + y);
      LegacyTile expected = result.Document.Tiles[index];
      CompatibilityTile actual = result.Compatibility.Tiles[index];
      if (actual.X != x || actual.Y != y ||
          actual.IsActive != expected.IsActive ||
          actual.TileType != expected.TileType ||
          actual.WallType != expected.WallType ||
          actual.LiquidAmount != expected.LiquidAmount ||
          actual.LiquidKind != expected.LiquidKind ||
          actual.HasWire != expected.HasWire ||
          actual.HasWire2 != expected.HasWire2 ||
          actual.HasWire3 != expected.HasWire3 ||
          actual.HasWire4 != expected.HasWire4 ||
          actual.IsHalfBrick != expected.IsHalfBrick ||
          actual.Slope != expected.Slope ||
          actual.IsActuated != expected.IsActuated ||
          actual.IsInactive != expected.IsInactive ||
          actual.FrameX != expected.FrameX ||
          actual.FrameY != expected.FrameY ||
          actual.TileColor != expected.TileColor ||
          actual.WallColor != expected.WallColor ||
          actual.IsInvisibleBlock != expected.IsInvisibleBlock ||
          actual.IsInvisibleWall != expected.IsInvisibleWall ||
          actual.IsFullbrightBlock != expected.IsFullbrightBlock ||
          actual.IsFullbrightWall != expected.IsFullbrightWall)
      {
        throw new InvalidOperationException(
          "A representative v319 tile did not project exactly.");
      }
    }
  }
}
