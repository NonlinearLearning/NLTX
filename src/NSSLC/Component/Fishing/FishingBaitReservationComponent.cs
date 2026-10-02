namespace Terraria.Fishing;

// status: proposed
// componentId: FISHING-BAIT-RESERVATION
// designStatus: decision-required
// crossSubsystemOwner: integration-review
public struct FishingBaitReservationComponent
{
  public FishingBaitReservationComponent(
    ItemInstanceId baitItemInstanceId,
    int baitItemTypeId,
    FishingAttemptId reservationKey)
  {
    BaitItemInstanceId = baitItemInstanceId;
    BaitItemTypeId = baitItemTypeId;
    ExpectedQuantity = 1;
    ReservationState = BaitReservationState.Unbound;
    ReservationKey = reservationKey;
    ConsumptionRevision = 0;
  }

  // Current ItemEntityRef is only a candidate mapping to this proposed identity.
  public ItemInstanceId BaitItemInstanceId;

  // Content/type snapshot; it does not identify the instance by itself.
  public int BaitItemTypeId;

  public int ExpectedQuantity;

  // Final owner and transitions remain under BD-COMP-03 review.
  public BaitReservationState ReservationState;

  // Stable for one fishing attempt.
  public FishingAttemptId ReservationKey;

  // Version4 has no explicit consumption revision.
  public uint ConsumptionRevision;

  public bool IsBound =>
    ReservationState != BaitReservationState.Unbound;
}
