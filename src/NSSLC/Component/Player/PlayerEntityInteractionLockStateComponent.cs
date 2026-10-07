namespace Terraria.Player;

// status: implemented-isolated-core
// source-member: P10-828 isOperatingAnotherEntity
// crossSubsystemOwner: integration-review for entity interaction claim/release ownership
/// <summary>
/// 保存玩家是否正在操作其他实体。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>主要源成员：isOperatingAnotherEntity（第 1272 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P10-player-input-control-component-design.md。</para>
/// <para>依据位置：第 18 行。</para>
/// </remarks>
public struct PlayerEntityInteractionLockStateComponent
{
  public bool IsOperatingAnotherEntity;
}
