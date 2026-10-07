namespace Terraria.Items;

/// <summary>
/// 保存合成事务占用的材料来源、数量和预留生命周期。
/// </summary>
/// <remarks>
/// <para>拆分来源：由 Recipe 的配方判定、材料消费与产物创建流程重组。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Recipe.cs。</para>
/// <para>重组说明：材料预留、事务阶段、来源版本和事务序号是合成流程拆分时新增的状态。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-07-version4-second-round-review-merged.md。</para>
/// <para>依据位置：第 2176 行。</para>
/// </remarks>
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
