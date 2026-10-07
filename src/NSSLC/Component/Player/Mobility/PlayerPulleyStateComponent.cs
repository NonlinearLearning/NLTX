namespace Terraria.Player.Mobility;

// status: implemented-isolated-core
// crossSubsystemOwner: integration-review for Tile, Collision, Spatial, Projectile, network, and persistence
/// <summary>
/// 保存玩家滑轮活动状态和朝向。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>主要源成员：pulleyDir（第 704 行）； pulley（第 706 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P07-player-mobility-component-design.md。</para>
/// <para>依据位置：第 496 行。</para>
/// </remarks>
public sealed class PlayerPulleyStateComponent
{
  public byte Direction { get; set; }

  public bool IsActive { get; set; }
}
