namespace Terraria.Npc;

// status: proposed
// evidenceStatus: confirmed for Version4 whoAmI/slot usage
// crossSubsystemOwner: integration-review
public sealed class NpcLegacySlotComponent
{
  public NpcLegacySlotComponent(NpcSlot? legacySlot = null)
  {
    LegacySlot = legacySlot ?? new NpcSlot(-1);
  }

  public NpcSlot LegacySlot { get; private set; }

  public bool IsAssigned => LegacySlot.IsAssigned;

  public void SetLegacySlot(NpcSlot legacySlot)
  {
    LegacySlot = legacySlot;
  }
}
