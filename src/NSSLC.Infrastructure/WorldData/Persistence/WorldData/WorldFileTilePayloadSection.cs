using System;
using System.Collections.Generic;

namespace Terraria.NonAuthoritative.Persistence;

/// <summary>
/// The compressed Tile section and its frame-importance table.
/// </summary>
/// <remarks>
/// The payload remains in WorldFile's bounded binary representation. It is not expanded into
/// runtime Tile objects; the tile owner can decode it into its own store during Prepare/Commit.
/// </remarks>
public sealed class WorldFileTilePayloadSection
{
  public const string SectionId = "world.tile-payload";

  public WorldFileTilePayloadSection(
    int maxTilesX,
    int maxTilesY,
    IReadOnlyList<bool> frameImportant,
    ReadOnlyMemory<byte> compressedPayload)
  {
    if (maxTilesX <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(maxTilesX));
    }

    if (maxTilesY <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(maxTilesY));
    }

    ArgumentNullException.ThrowIfNull(frameImportant);
    if (frameImportant.Count > ushort.MaxValue)
    {
      throw new ArgumentOutOfRangeException(nameof(frameImportant));
    }

    MaxTilesX = maxTilesX;
    MaxTilesY = maxTilesY;
    FrameImportant = Array.AsReadOnly(new List<bool>(frameImportant).ToArray());
    CompressedPayload = compressedPayload.ToArray();
  }

  public int MaxTilesX { get; }

  public int MaxTilesY { get; }

  public IReadOnlyList<bool> FrameImportant { get; }

  public ReadOnlyMemory<byte> CompressedPayload { get; }
}
