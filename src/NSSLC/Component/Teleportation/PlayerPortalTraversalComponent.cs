namespace Terraria.Teleportation;

// Stores short-lived Portal/Pylon traversal metadata and physics requests.
// Teleport commits and visual effects remain owned by their respective adapters.
/// <summary>
/// 保存玩家最近传送门、传送门物理窗口和晶塔样式。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>主要源成员：lastPortalColorIndex（第 2376 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-P04-player-lifecycle-interaction-component-design.md。
/// </para>
/// <para>依据位置：第 556 行。</para>
/// </remarks>
public sealed class PlayerPortalTraversalComponent
{
  public int LastPortalColorIndex { get; set; }

  public int PortalPhysicsRemainingTicks { get; set; }

  public bool PortalPhysicsRequested { get; set; }

  public int LastPylonStyle { get; set; }
}
