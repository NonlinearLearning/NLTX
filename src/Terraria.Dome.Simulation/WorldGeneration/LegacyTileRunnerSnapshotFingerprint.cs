using System;
using System.Security.Cryptography;
using System.Text;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed record LegacyTileRunnerSnapshotFingerprintResult(
  string GeneratedFingerprint,
  string? OracleFingerprint,
  bool Compared,
  bool Matches);

public static class LegacyTileRunnerSnapshotFingerprint
{
  public static LegacyTileRunnerSnapshotFingerprintResult Create(
    WorldGridSnapshot snapshot,
    string? oracleFingerprint = null)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    StringBuilder content = new();
    content.Append(snapshot.Metadata.Width).Append('|').Append(snapshot.Metadata.Height);
    for (int x = 0; x < snapshot.Metadata.Width; x++)
    {
      for (int y = 0; y < snapshot.Metadata.Height; y++)
      {
        WorldTile tile = snapshot.GetTile(x, y);
        content.Append('|').Append(x).Append(',').Append(y).Append(':')
          .Append(tile.IsActive ? '1' : '0').Append(':').Append(tile.Type)
          .Append(':').Append(tile.WallType).Append(':').Append(tile.LiquidAmount)
          .Append(':').Append(tile.LiquidType).Append(':').Append(tile.FrameX)
          .Append(':').Append(tile.FrameY).Append(':').Append(tile.IsInactive ? '1' : '0');
      }
    }

    string generated = Convert.ToHexString(
      SHA256.HashData(Encoding.UTF8.GetBytes(content.ToString())));
    bool compared = !string.IsNullOrWhiteSpace(oracleFingerprint);
    bool matches = compared &&
      StringComparer.OrdinalIgnoreCase.Equals(generated, oracleFingerprint);
    return new LegacyTileRunnerSnapshotFingerprintResult(
      generated,
      oracleFingerprint,
      compared,
      matches);
  }
}
