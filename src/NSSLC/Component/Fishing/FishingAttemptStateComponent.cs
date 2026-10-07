namespace Terraria.Fishing;

// status: proposed
// componentId: FISHING-ATTEMPT-STATE
// designStatus: decision-required
// crossSubsystemOwner: integration-review
/// <summary>
/// 保存一次钓鱼尝试的参与实体、阶段和终止原因。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Projectile。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Projectile.cs。</para>
/// <para>主要源成员：bobber（第 104 行）； owner（第 126 行）。</para>
/// <para>重组说明：AttemptId、Phase、Revision 和终止原因是钓鱼生命周期显式化后的状态。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-1/design/。</para>
/// <para>
/// 拆分依据文件：2026-09-05-version4-fishing-and-catch-simulation-component-code-draft.md。
/// </para>
/// <para>依据位置：第 121 行。</para>
/// </remarks>
public struct FishingAttemptStateComponent
{
  public FishingAttemptStateComponent(
    FishingAttemptId attemptId,
    PlayerEntityId owner,
    ProjectileEntityId? bobber)
  {
    AttemptId = attemptId;
    Owner = owner;
    Bobber = bobber;
    Phase = FishingAttemptPhase.Waiting;
    Revision = 0;
    TerminalReason = null;
  }

  // Version4 has no explicit attempt identity.
  public FishingAttemptId AttemptId;

  // Do not store Projectile.owner's network slot directly.
  public PlayerEntityId Owner;

  // This is a relation, not a copy of Projectile state or a network ID.
  public ProjectileEntityId? Bobber;

  // Map from the current Dome FishingBobberPhase only after BD-COMP-01 is resolved.
  public FishingAttemptPhase Phase;

  // Candidate convergence/version field; Version4 has no explicit attempt revision.
  public uint Revision;

  // Null while the attempt remains non-terminal.
  public FishingTerminalReason? TerminalReason;

  public bool IsTerminal => TerminalReason.HasValue;
}
