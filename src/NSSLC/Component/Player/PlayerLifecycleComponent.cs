namespace Terraria.Player;

/// <summary>
/// 保存玩家生死阶段、死亡时间、重生倒计时和观战目标。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>主要源成员：dead（第 1134 行）； deadTime（第 1136 行）； respawnTimer（第 1140 行）。</para>
/// <para>重组说明：Phase 和观战目标是玩家生命周期重组后的表达。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-P04-player-lifecycle-interaction-component-design.md。
/// </para>
/// <para>依据位置：第 68 行。</para>
/// </remarks>
public struct PlayerLifecycleComponent
{
  public PlayerLifecycleComponent(PlayerLifecyclePhase phase, int respawnRemainingTicks)
  {
    Phase = phase;
    DeadElapsedTicks = 0;
    RespawnRemainingTicks = respawnRemainingTicks;
    SpectatingTargetSlot = null;
  }

  public PlayerLifecyclePhase Phase { get; internal set; }

  public int DeadElapsedTicks { get; internal set; }

  public int RespawnRemainingTicks { get; internal set; }

  public LegacyPlayerSlot? SpectatingTargetSlot { get; internal set; }

  public bool IsDead => Phase is PlayerLifecyclePhase.Dead or PlayerLifecyclePhase.Respawning;

  public bool CanRespawn => IsDead && RespawnRemainingTicks <= 0;
}
