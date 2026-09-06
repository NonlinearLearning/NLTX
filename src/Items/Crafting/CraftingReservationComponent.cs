namespace Terraria.Items;

public sealed class CraftingReservationComponent
{
  public CraftingReservationComponent(
    ReservationId reservationId,
    TransactionId transactionId,
    PersistentContainerId sourceContainerId,
    SlotIndex sourceSlot,
    RuntimeEntityId sourceItemEntity,
    int quantity,
    long expectedContainerRevision,
    long? createdAt = null,
    long? expiresAt = null,
    ReservationState state = ReservationState.Active)
  {
    ReservationId = reservationId;
    TransactionId = transactionId;
    SourceContainerId = sourceContainerId;
    SourceSlot = sourceSlot;
    SourceItemEntity = sourceItemEntity;
    Quantity = quantity;
    ExpectedContainerRevision = expectedContainerRevision;
    CreatedAt = createdAt;
    ExpiresAt = expiresAt;
    State = state;
  }

  public ReservationId ReservationId;
  public TransactionId TransactionId;
  public PersistentContainerId SourceContainerId;
  public SlotIndex SourceSlot;
  public RuntimeEntityId SourceItemEntity;
  public int Quantity;
  public long ExpectedContainerRevision;
  public long? CreatedAt;
  public long? ExpiresAt;
  public ReservationState State;

  public bool IsActive => State == ReservationState.Active;

  public bool IsExpiredAt(long currentTick) =>
    IsActive &&
    ExpiresAt.HasValue &&
    currentTick >= ExpiresAt.Value;
}
