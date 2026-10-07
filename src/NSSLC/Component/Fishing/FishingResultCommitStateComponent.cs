using Terraria.Player;

namespace Terraria.Fishing;

// status: proposed
// componentId: FISHING-RESULT-COMMIT-STATE
// designStatus: decision-required
// crossSubsystemOwner: integration-review
/// <summary>
/// 保存钓鱼结果提交状态、外部结果身份及重复提交控制信息。
/// </summary>
/// <remarks>
/// <para>拆分来源：由 Projectile 的钓鱼结果生成与提交流程重组。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Projectile.cs。</para>
/// <para>重组说明：提交阶段、重试次数、外部结果编号和幂等键是拆分事务时新增的状态。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-1/design/。</para>
/// <para>
/// 拆分依据文件：2026-09-05-version4-fishing-and-catch-simulation-component-code-draft.md。
/// </para>
/// <para>依据位置：第 617 行。</para>
/// </remarks>
public struct FishingResultCommitStateComponent
{
  public FishingResultCommitStateComponent(
    FishingOutcomeKind resultKind,
    TileCoordinate origin,
    IdempotencyKey idempotencyKey)
  {
    ResultKind = resultKind;
    CommitState = FishingCommitState.NotSubmitted;
    CommitAttemptCount = 0;
    ExternalResultId = null;
    Origin = origin;
    IdempotencyKey = idempotencyKey;
  }

  // Must agree with FishingCatchDecisionComponent.OutcomeKind.
  public FishingOutcomeKind ResultKind;

  // Version4 has no unified result commit state.
  public FishingCommitState CommitState;

  public ushort CommitAttemptCount;

  // Item and NPC IDs are discriminated and externally owned.
  public FishingExternalResultId? ExternalResultId;

  // Existing coordinate candidate; source ownership remains under review.
  public TileCoordinate Origin;

  // Version4 has no explicit idempotency key.
  public IdempotencyKey IdempotencyKey;

  public bool IsCommitted =>
    CommitState == FishingCommitState.Committed;
}
