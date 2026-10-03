using System;

namespace Terraria.NonAuthoritative.Persistence;

/// <summary>
/// Bounded TileEntity records retained for a TileEntity owner to decode.
/// </summary>
/// <remarks>
/// TileEntity records contain type-specific extension data. The application layer retains the
/// exact record bytes and count without depending on the legacy type registry or runtime store.
/// </remarks>
public sealed class WorldFileTileEntitySection
{
  public const string SectionId = "world.tile-entities";

  public WorldFileTileEntitySection(int entityCount, ReadOnlyMemory<byte> serializedRecords)
  {
    if (entityCount < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(entityCount));
    }

    EntityCount = entityCount;
    SerializedRecords = serializedRecords.ToArray();
  }

  public int EntityCount { get; }

  public ReadOnlyMemory<byte> SerializedRecords { get; }
}
