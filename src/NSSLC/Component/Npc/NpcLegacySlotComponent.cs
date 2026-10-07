namespace Terraria.Npc;

// status: proposed
// evidenceStatus: confirmed for Version4 whoAmI/slot usage
// crossSubsystemOwner: integration-review
public sealed class NpcLegacySlotComponent
{
  public NpcLegacySlotComponent(NpcSlot? legacySlot = null, uint generation = 0)
  {
    LegacySlot = legacySlot ?? new NpcSlot(-1);
    Generation = generation;
  }

  public NpcSlot LegacySlot { get; private set; }

  /// <summary>The generation of the legacy slot projection, or zero when not assigned.</summary>
  public uint Generation { get; private set; }

  public bool IsAssigned => LegacySlot.IsAssigned;

  public void SetLegacySlot(NpcSlot legacySlot, uint generation = 0)
  {
    LegacySlot = legacySlot;
    Generation = generation;
  }
}
