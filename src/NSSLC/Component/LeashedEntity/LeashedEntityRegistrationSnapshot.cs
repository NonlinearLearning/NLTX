using Terraria.Relationships;
using Terraria.WorldStorage;

namespace Terraria.LeashedEntity;

/// <summary>
/// Immutable view returned after a lifecycle commit. Component values are copied into the view.
/// </summary>
public sealed record LeashedEntityRegistrationSnapshot
{
  public LeashedEntityRegistrationSnapshot(
    EntityReference runtimeEntityReference,
    LeashedEntityStateComponent state,
    LeashedEntityLegacySlotComponent legacySlot,
    LeashedEntityLifecycleComponent lifecycle,
    LeashedEntitySectionMembershipComponent sectionMembership,
    LeashedEntityAnchorRelationComponent anchorRelation)
  {
    RuntimeEntityReference = runtimeEntityReference;
    State = state;
    LegacySlot = legacySlot;
    Lifecycle = lifecycle;
    SectionMembership = sectionMembership;
    AnchorRelation = anchorRelation;
  }

  public EntityReference RuntimeEntityReference { get; init; }

  public LeashedEntityStateComponent State { get; init; }

  public LeashedEntityLegacySlotComponent LegacySlot { get; init; }

  public LeashedEntityLifecycleComponent Lifecycle { get; init; }

  public LeashedEntitySectionMembershipComponent SectionMembership { get; init; }

  public LeashedEntityAnchorRelationComponent AnchorRelation { get; init; }

  public LeashedEntityHandle Handle =>
    new(RuntimeEntityReference, LegacySlot.Slot, LegacySlot.SlotGeneration);
}
