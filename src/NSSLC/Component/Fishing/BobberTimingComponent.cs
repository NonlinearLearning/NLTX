namespace Terraria.Fishing;

// status: proposed
// componentId: FISHING-BOBBER-TIMING
// designStatus: candidate
// crossSubsystemOwner: integration-review
/// <summary>
/// 保存浮标等待咬钩、咬钩阈值和收线状态。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Projectile。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Projectile.cs。</para>
/// <para>主要源成员：localAI（第 130 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-1/design/。</para>
/// <para>
/// 拆分依据文件：2026-09-05-version4-fishing-and-catch-simulation-component-code-draft.md。
/// </para>
/// <para>依据位置：第 212 行。</para>
/// </remarks>
public struct BobberTimingComponent
{
  public BobberTimingComponent(int biteThreshold)
  {
    ElapsedTicks = 0;
    BiteThreshold = biteThreshold;
    RetractRequested = false;
    BiteState = FishingBiteState.Waiting;
    LegacyState = null;
  }

  // Fishing-specific elapsed time; this is not Projectile.timeLeft.
  public int ElapsedTicks;

  // The complete reference suggests 660; Version4 did not confirm that value.
  public int BiteThreshold;

  public bool RetractRequested;

  // Must remain consistent with FishingAttemptStateComponent.Phase.
  public FishingBiteState BiteState;

  // Compatibility-only boundary for old ai/localAI values.
  public FishingLegacyBobberState? LegacyState;

  public bool HasReachedBiteThreshold =>
    BiteThreshold >= 0 && ElapsedTicks >= BiteThreshold;
}
