namespace Terraria.Player.Accessories;

// status: implemented-isolated-core
// crossSubsystemOwner: integration-review for Item, WorldInteraction, Jump, NPC, Combat, and Lighting
/// <summary>
/// 保存磁吸、挖掘和其他玩家辅助能力。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：manaMagnet（第 598 行）； lifeMagnet（第 600 行）； treasureMagnet（第 602 行）； chiselSpeed（第 604 行）；
/// lifeForce（第 606 行）； hasDeadCellsDownDash（第 608 行）； calmed（第 610 行）； inferno（第 612 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P07-player-mobility-component-design.md。</para>
/// <para>依据位置：第 338 行。</para>
/// </remarks>
public sealed class PlayerUtilityCapabilityComponent
{
  public bool ManaMagnet { get; set; }

  public bool LifeMagnet { get; set; }

  public bool TreasureMagnet { get; set; }

  public bool ChiselSpeed { get; set; }

  public bool LifeForce { get; set; }

  public bool HasDeadCellsDownDash { get; set; }

  public bool Calmed { get; set; }

  public bool Inferno { get; set; }
}
