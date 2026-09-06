using Terraria.WorldStorage;

namespace Terraria.LeashedEntity;

/// <summary>
/// Stores entity-side section membership and local activity observation.
/// status: proposed
/// componentOwner: LeashedEntitySimulation
/// crossSubsystemOwner: integration-review
/// </summary>
public struct LeashedEntitySectionMembershipComponent
{
  /// <summary>
  /// Current world section. Global section activity is not stored here.
  /// </summary>
  public SectionCoordinate Section;

  /// <summary>
  /// Reusable slot in the current section bucket; -1 means not indexed.
  /// </summary>
  public int SectionSlot;

  /// <summary>
  /// Local activity observation/cache, not the global authority.
  /// </summary>
  public bool IsSectionActive;

  /// <summary>
  /// Candidate observation tick, not a global clock owner.
  /// </summary>
  public long? LastActivationTick;

  /// <summary>
  /// Derived membership view; compaction may change the slot.
  /// </summary>
  public bool IsIndexed => SectionSlot >= 0;

  public LeashedEntitySectionMembershipComponent(
    SectionCoordinate section,
    int sectionSlot,
    bool isSectionActive,
    long? lastActivationTick)
  {
    Section = section;
    SectionSlot = sectionSlot;
    IsSectionActive = isSectionActive;
    LastActivationTick = lastActivationTick;
  }
}
