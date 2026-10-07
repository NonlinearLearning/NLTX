namespace Terraria.Player;

// status: implemented-isolated-core
// source-members: P09-720, P09-721, P09-722, P09-723
// crossSubsystemOwner: environment ticking and derived capability projections remain integration-review
/// <summary>
/// 保存玩家呼吸、熔岩耐受和液体视觉资源。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：breathMax（第 1037 行）； breath（第 1039 行）； lavaMax（第 1041 行）； lavaTime（第 1043 行）；
/// ignoreWater（第 1045 行）； lavaVision（第 1047 行）； lavaOpacity（第 1049 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-P09-player-inventory-equipment-component-design.md。
/// </para>
/// <para>依据位置：第 137 行。</para>
/// </remarks>
public sealed class PlayerResourceStateComponent
{
  public const int DefaultBreathMax = 200;

  public const float MinimumLavaOpacity = 0.4f;

  public const float MaximumLavaOpacity = 1f;

  public int BreathMax { get; internal set; } = DefaultBreathMax;

  public int Breath { get; internal set; } = DefaultBreathMax;

  public int LavaMax { get; internal set; }

  public int LavaTime { get; internal set; }

  public bool IgnoreWater { get; internal set; }

  public bool LavaVision { get; internal set; }

  public float LavaOpacity { get; internal set; } = MaximumLavaOpacity;
}
