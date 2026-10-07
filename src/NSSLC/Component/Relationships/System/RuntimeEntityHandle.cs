using System;

namespace Terraria.Relationships;

public readonly record struct RuntimeEntityHandle
{
  public RuntimeEntityHandle(EntityRuntimeId runtimeId, int localIndex, uint generation)
  {
    if (!runtimeId.IsAssigned)
    {
      throw new ArgumentException("A runtime handle requires a runtime ID.", nameof(runtimeId));
    }

    ArgumentOutOfRangeException.ThrowIfNegative(localIndex);
    ArgumentOutOfRangeException.ThrowIfZero(generation);

    RuntimeId = runtimeId;
    LocalIndex = localIndex;
    Generation = generation;
  }

  public EntityRuntimeId RuntimeId { get; }

  public int LocalIndex { get; }

  public uint Generation { get; }

  public bool IsAssigned => RuntimeId.IsAssigned && Generation != 0;
}
