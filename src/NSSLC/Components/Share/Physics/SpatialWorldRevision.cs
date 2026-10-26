using System;

namespace Terraria.Physics;

/// <summary>Identifies the world snapshot used by one spatial simulation batch.</summary>
public readonly record struct SpatialWorldRevision(ulong Epoch, long Version)
{
  public bool IsValid => Epoch != 0 && Version >= 0;

  public static SpatialWorldRevision Initial => new(1, 0);
}
