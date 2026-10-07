using Terraria.Relationships;

namespace Terraria.LeashedEntity;

public readonly record struct LeashedEntityHandle(
  EntityReference RuntimeEntityReference,
  int LegacySlot,
  uint SlotGeneration)
{
  public bool IsAssigned =>
    !RuntimeEntityReference.IsEmpty
    && RuntimeEntityReference.Scope != EntityReferenceScope.None
    && LegacySlot >= 0
    && SlotGeneration > 0;
}
