namespace Terraria.Player;

// status: implemented-isolated-core
// source-member: P10-1186 ActuationRodLock
// crossSubsystemOwner: integration-review for Wiring/World interaction lock ownership
/// <summary>
/// 保存玩家致动魔杖的交互锁定状态。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>主要源成员：ActuationRodLock（第 2012 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P10-player-input-control-component-design.md。</para>
/// <para>依据位置：第 368 行。</para>
/// </remarks>
public struct PlayerActuationRodLockStateComponent
{
  public bool IsActuationRodLocked;
}
