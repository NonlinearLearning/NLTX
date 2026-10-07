namespace Terraria.Player.Environment;

// status: implemented
// componentId: PLAYER.COMP.ZONE_AND_ENVIRONMENT_STATE
// source-members: P08-662, P08-665..P08-670
// crossSubsystemOwner: integration-review
/// <summary>
/// 保存玩家区域位集和环境 Buff 免疫计时。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：environmentBuffImmunityTimer（第 917 行）； zone1（第 923 行）； zone2（第 925 行）； zone3（第 927 行）；
/// zone4（第 929 行）； zone5（第 931 行）； _wasInShimmerZone（第 933 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P08-player-environment-armor-component-design.md。</para>
/// <para>依据位置：第 105 行。</para>
/// </remarks>
public sealed class PlayerZoneAndEnvironmentStateComponent
{
  public int EnvironmentBuffImmunityTimer { get; internal set; }

  // The legacy BitsByte values are kept as bytes until the protocol type is available in NLTX.
  public byte Zone1 { get; internal set; }

  public byte Zone2 { get; internal set; }

  public byte Zone3 { get; internal set; }

  public byte Zone4 { get; internal set; }

  public byte Zone5 { get; internal set; }

  public bool WasInShimmerZone { get; internal set; }

  public void ApplyNetworkZones(byte zone1, byte zone2, byte zone3, byte zone4, byte zone5)
  {
    Zone1 = zone1;
    Zone2 = zone2;
    Zone3 = zone3;
    Zone4 = zone4;
    Zone5 = zone5;
  }

}
