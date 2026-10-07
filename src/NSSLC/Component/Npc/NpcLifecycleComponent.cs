using System;

namespace Terraria.Npc;

/// <summary>
/// 保存 NPC 活动状态、生命周期阶段和剩余活动时间。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.NPC。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/NPC.cs。</para>
/// <para>主要源成员：active（第 5897 行）； timeLeft（第 6319 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-1/design/。</para>
/// <para>拆分依据文件：2026-09-05-version4-npc-and-town-simulation-component-design.md。</para>
/// <para>依据位置：第 381 行。</para>
/// </remarks>
public sealed class NpcLifecycleComponent
{
  public NpcLifecycleComponent(
    bool isActive,
    int remainingActiveTicks,
    NpcLifecycleStage stage)
  {
    if (remainingActiveTicks < 0)
    {
      throw new ArgumentOutOfRangeException(
        nameof(remainingActiveTicks),
        "Remaining active ticks cannot be negative.");
    }

    if (stage == NpcLifecycleStage.Despawned && isActive)
    {
      throw new ArgumentException(
        "A despawned NPC cannot be active.",
        nameof(isActive));
    }

    _isActive = isActive;
    RemainingActiveTicks = remainingActiveTicks;
    _stage = stage;
  }

  // Compatibility constructor for the existing stage/despawn-ticks API.
  public NpcLifecycleComponent(NpcLifecycleStage stage, int remainingDespawnTicks)
  {
    if (remainingDespawnTicks < 0)
    {
      throw new ArgumentOutOfRangeException(
        nameof(remainingDespawnTicks),
        "Remaining despawn ticks cannot be negative.");
    }

    _isActive = stage != NpcLifecycleStage.Despawned;
    RemainingActiveTicks = remainingDespawnTicks;
    _stage = stage;
  }

  private bool _isActive;

  private NpcLifecycleStage _stage;

  public bool IsActive => _isActive;

  public int RemainingActiveTicks { get; }

  public NpcLifecycleStage Stage => _stage;

  // Compatibility alias retained while callers move to RemainingActiveTicks.
  public int RemainingDespawnTicks => RemainingActiveTicks;

  internal bool TryCommitDespawn()
  {
    if (!_isActive || _stage == NpcLifecycleStage.Despawned)
    {
      return false;
    }

    _isActive = false;
    _stage = NpcLifecycleStage.Despawned;
    return true;
  }
}
