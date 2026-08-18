using System;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation;

public sealed class OpaqueCompatibilityRecord
{
  public OpaqueCompatibilityRecord(
    int sectionIndex,
    long offset,
    string reason,
    int? type,
    int? tileX,
    int? tileY,
    IReadOnlyList<byte> payload,
    bool isRequired)
  {
    SectionIndex = sectionIndex;
    Offset = offset;
    Reason = reason ?? throw new ArgumentNullException(nameof(reason));
    Type = type;
    TileX = tileX;
    TileY = tileY;
    Payload = new SnapshotReadOnlyList<byte>(payload ??
      throw new ArgumentNullException(nameof(payload)));
    IsRequired = isRequired;
  }

  public long Offset { get; }

  public IReadOnlyList<byte> Payload { get; }

  public string Reason { get; }

  public int SectionIndex { get; }

  public int? TileX { get; }

  public int? TileY { get; }

  public int? Type { get; }

  public bool IsRequired { get; }
}
