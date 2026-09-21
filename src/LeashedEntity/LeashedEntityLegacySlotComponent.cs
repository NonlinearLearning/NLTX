namespace Terraria.LeashedEntity;

/// <summary>
/// Stores the compatibility mapping to Version4's reusable whoAmI slot.
/// status: implemented
/// componentOwner: LeashedEntitySimulation compatibility boundary
/// crossSubsystemOwner: integration-review
/// </summary>
public struct LeashedEntityLegacySlotComponent
{
  /// <summary>
  /// Reusable Version4 whoAmI/ByWhoAmI slot; -1 means unassigned.
  /// </summary>
  public int Slot;

  /// <summary>
  /// Candidate reuse guard. Version4 does not provide this field.
  /// </summary>
  public uint SlotGeneration;

  /// <summary>
  /// Derived slot-assignment view only.
  /// </summary>
  public bool IsAssigned => Slot >= 0;

  public LeashedEntityLegacySlotComponent()
    : this(-1, 0)
  {
  }

  public LeashedEntityLegacySlotComponent(int slot, uint slotGeneration)
  {
    Slot = slot;
    SlotGeneration = slotGeneration;
  }
}
