using EntityEcs.Components;
using Terraria.WorldStorage;

namespace Terraria.LeashedEntity;

/// <summary>
/// Immutable view returned after a lifecycle commit. Component values are copied into the view.
/// </summary>
public sealed record LeashedEntityRegistrationSnapshot
{
  public LeashedEntityRegistrationSnapshot(
    EntityId runtimeEntityId,
    LeashedEntityStateComponent state,
    LeashedEntityLegacySlotComponent legacySlot,
    LeashedEntityLifecycleComponent lifecycle,
    LeashedEntitySectionMembershipComponent sectionMembership)
  {
    RuntimeEntityId = runtimeEntityId;
    State = state;
    LegacySlot = legacySlot;
    Lifecycle = lifecycle;
    SectionMembership = sectionMembership;
  }

  public EntityId RuntimeEntityId { get; init; }

  public LeashedEntityStateComponent State { get; init; }

  public LeashedEntityLegacySlotComponent LegacySlot { get; init; }

  public LeashedEntityLifecycleComponent Lifecycle { get; init; }

  public LeashedEntitySectionMembershipComponent SectionMembership { get; init; }

  public LeashedEntityHandle Handle =>
    new(RuntimeEntityId, LegacySlot.Slot, LegacySlot.SlotGeneration);
}

