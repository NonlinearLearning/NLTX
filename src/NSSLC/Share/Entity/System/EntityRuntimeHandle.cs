using System;

namespace Terraria.Share.Relationships;

/// <summary>
/// Runtime projection used to reject stale world or slot references.
/// </summary>
public readonly record struct EntityRuntimeHandle {
  public EntityUuid Id { get; }
  public long WorldScope { get; }
  public long Generation { get; }

  public bool IsValid => Id.IsValid && WorldScope > 0 && Generation > 0;

  public EntityRuntimeHandle(EntityUuid id, long worldScope, long generation) {
    if (!id.IsValid) {
      throw new ArgumentException("A runtime handle requires a valid entity UUID.", nameof(id));
    }
    if (worldScope <= 0) {
      throw new ArgumentOutOfRangeException(nameof(worldScope));
    }
    if (generation <= 0) {
      throw new ArgumentOutOfRangeException(nameof(generation));
    }
    Id = id;
    WorldScope = worldScope;
    Generation = generation;
  }

  public bool Matches(EntityRuntimeHandle other) {
    return this == other;
  }
}
