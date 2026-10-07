namespace Terraria.Teleportation;

// Stores the short-lived teleport request guard.
// Position commits, visuals and network acknowledgements remain external effects.
/// <summary>
/// 保存玩家是否处于传送过渡过程。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>主要源成员：teleporting（第 646 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-P04-player-lifecycle-interaction-component-design.md。
/// </para>
/// <para>依据位置：第 303 行。</para>
/// </remarks>
public sealed class PlayerTeleportTransitionComponent
{
  public bool IsTeleporting { get; set; }
}
