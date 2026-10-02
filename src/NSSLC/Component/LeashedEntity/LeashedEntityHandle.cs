using EntityEcs.Components;

namespace Terraria.LeashedEntity;

public readonly record struct LeashedEntityHandle(
  EntityId RuntimeEntityId,
  int LegacySlot,
  uint SlotGeneration)
{
  public bool IsAssigned =>
    RuntimeEntityId.IsAssigned
    && LegacySlot >= 0
    && SlotGeneration > 0;
}

