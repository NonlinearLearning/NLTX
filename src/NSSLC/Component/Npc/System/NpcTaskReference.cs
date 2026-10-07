using System;
using Terraria.Relationships;

namespace Terraria.Npc;

public readonly record struct NpcTaskReference
{
  public NpcTaskReference(RuntimeEntityHandle entityHandle, ulong taskGeneration)
  {
    if (!entityHandle.IsAssigned)
    {
      throw new ArgumentException("A task reference requires an entity handle.", nameof(entityHandle));
    }

    EntityHandle = entityHandle;
    TaskGeneration = taskGeneration;
  }

  public RuntimeEntityHandle EntityHandle { get; }

  public ulong TaskGeneration { get; }

  public bool IsAssigned => EntityHandle.IsAssigned;
}
