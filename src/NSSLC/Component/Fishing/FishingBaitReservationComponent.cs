namespace Terraria.Fishing;

// status: proposed
// componentId: FISHING-BAIT-RESERVATION
// designStatus: decision-required
// crossSubsystemOwner: integration-review
/// <summary>
/// 保存本次钓鱼预留的鱼饵实例、数量和消费状态。
/// </summary>
/// <remarks>
/// <para>拆分来源：由 Projectile 的钓鱼鱼饵选择与消费流程重组。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Projectile.cs。</para>
/// <para>重组说明：鱼饵实例引用、预留键、消费版本和预留阶段是拆分事务时新增的状态。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-1/design/。</para>
/// <para>
/// 拆分依据文件：2026-09-05-version4-fishing-and-catch-simulation-component-code-draft.md。
/// </para>
/// <para>依据位置：第 527 行。</para>
/// </remarks>
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
