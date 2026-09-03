using System;
using System.Collections.Generic;

namespace Terraria.WorldFile.V319.Model;

public sealed class LegacyTileEntity
{
  public LegacyTileEntity(
    int id,
    byte type,
    int x,
    int y,
    IReadOnlyList<byte> payload,
    bool isOpaque = false)
  {
    Id = id;
    Type = type;
    X = x;
    Y = y;
    IsOpaque = isOpaque;
    Payload = new LegacyReadOnlyList<byte>(payload ??
      throw new ArgumentNullException(nameof(payload)));
  }

  public int Id { get; }

  public bool IsOpaque { get; }

  public IReadOnlyList<byte> Payload { get; }

  public byte Type { get; }

  public int X { get; }

  public int Y { get; }
}
