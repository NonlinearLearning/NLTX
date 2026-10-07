namespace Terraria.Player.Mobility;

// status: implemented-isolated-core
// crossSubsystemOwner: integration-review for Inventory, Item, Projectile, network, and persistence
/// <summary>
/// 保存绳索、绳圈和大型宝石扫描状态。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：ropeCount（第 672 行）； cordage（第 694 行）； gem（第 696 行）； gemCount（第 698 行）； ownedLargeGems（第
/// 700 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P07-player-mobility-component-design.md。</para>
/// <para>依据位置：第 490 行。</para>
/// </remarks>
public sealed class PlayerRopeStateComponent
{
  public int RopeCount { get; set; }

  public bool HasCordage { get; set; }

  public int SelectedGem { get; set; } = -1;

  public int GemScanCounter { get; set; }

  // Version4 BitsByte storage is represented by its byte payload until the shared value type is available.
  public byte OwnedLargeGems { get; set; }
}
