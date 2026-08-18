using System;
using System.Collections.Generic;
using Terraria.WorldFile.V319.Model;

namespace Terraria.WorldCompatibility.Model;

public sealed class CompatibilityTileEntitySnapshot
{
  public CompatibilityTileEntitySnapshot(
    int id,
    byte type,
    int x,
    int y,
    IReadOnlyList<byte> payload,
    bool isOpaque)
  {
    Id = id;
    Type = type;
    X = x;
    Y = y;
    IsOpaque = isOpaque;
    Payload = new CompatibilityReadOnlyList<byte>(payload ??
      throw new ArgumentNullException(nameof(payload)));
  }

  public int Id { get; }

  public bool IsOpaque { get; }

  public IReadOnlyList<byte> Payload { get; }

  public byte Type { get; }

  public int X { get; }

  public int Y { get; }

  internal static CompatibilityTileEntitySnapshot From(LegacyTileEntity entity)
  {
    ArgumentNullException.ThrowIfNull(entity);
    return new CompatibilityTileEntitySnapshot(
      entity.Id,
      entity.Type,
      entity.X,
      entity.Y,
      entity.Payload,
      entity.IsOpaque);
  }
}
