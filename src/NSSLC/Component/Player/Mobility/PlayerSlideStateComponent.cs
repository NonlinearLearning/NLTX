namespace Terraria.Player.Mobility;

// status: implemented-isolated-core
// crossSubsystemOwner: integration-review for Equipment, Tile, Collision, Spatial, Item, and persistence
/// <summary>
/// 保存玩家滑行方向、溜冰和攀墙能力。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：sliding（第 714 行）； slideDir（第 716 行）； iceSkate（第 720 行）； spikedBoots（第 724 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P07-player-mobility-component-design.md。</para>
/// <para>依据位置：第 565 行。</para>
/// </remarks>
public sealed class PlayerSlideStateComponent
{
  public bool IsSliding { get; set; }

  public int Direction { get; set; }

  public bool IceSkate { get; set; }

  public int SpikedBootsLevel { get; set; }
}
