using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.WorldStorage;

namespace NSSLC.WorldGeneration;

/// <summary>Detached generated entity contents; no ECS store or runtime is retained.</summary>
public sealed class TileEntityStoreSnapshot {
  public IReadOnlyList<TileEntitySnapshot> Entities { get; }
  public int NextId { get; }

  public TileEntityStoreSnapshot(IReadOnlyList<TileEntitySnapshot> entities, int nextId) {
    ArgumentNullException.ThrowIfNull(entities);
    ArgumentOutOfRangeException.ThrowIfNegative(nextId);
    Entities = Array.AsReadOnly(entities.ToArray());
    NextId = nextId;
  }
}
