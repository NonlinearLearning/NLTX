namespace Terraria.Player.Mobility;

// status: implemented-isolated-core
// crossSubsystemOwner: integration-review for Tile, Collision, Mount, and Spatial ownership
/// <summary>
/// 保存地面奔跑、楼梯下落和斜坡移动状态。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>主要源成员：stairFall（第 638 行）； powerrun（第 755 行）； runningOnSand（第 757 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P07-player-mobility-component-design.md。</para>
/// <para>依据位置：第 410 行。</para>
/// </remarks>
public sealed class PlayerGroundTraversalStateComponent
{
  public bool StairFall { get; set; }

  public bool IsSloping { get; set; }

  public float AcceleratedRunSpeed { get; set; }

  public bool PowerRun { get; set; }

  public bool RunningOnSand { get; set; }
}
