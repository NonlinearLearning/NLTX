using System;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation;

public sealed class TileEntityPersistentState
{
  public TileEntityPersistentState(
    int id,
    byte type,
    int tileX,
    int tileY,
    IReadOnlyList<byte> payload,
    bool isOpaque)
  {
    if (id <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(id));
    }

    Id = id;
    Type = type;
    TileX = tileX;
    TileY = tileY;
    Payload = new SnapshotReadOnlyList<byte>(payload ??
      throw new ArgumentNullException(nameof(payload)));
    IsOpaque = isOpaque;
  }

  public int Id { get; }

  public bool IsOpaque { get; }

  public IReadOnlyList<byte> Payload { get; }

  public byte Type { get; }

  public int TileX { get; }

  public int TileY { get; }
}
