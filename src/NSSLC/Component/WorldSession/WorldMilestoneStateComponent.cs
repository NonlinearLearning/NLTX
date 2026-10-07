namespace Terraria.WorldSession.Components;

/// <summary>
/// 保存世界机械首领、暗影珠和祭坛里程碑。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.WorldGen。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/WorldGen.cs。</para>
/// <para>
/// 主要源成员：shadowOrbSmashed（第 4153 行）； shadowOrbCount（第 4155 行）； altarCount（第 4157 行）。
/// </para>
/// <para>重组说明：机械首领里程碑由 NPC 的世界首领进度重组。</para>
/// </remarks>
public sealed class WorldMilestoneStateComponent {
  public bool AnyMechBossDowned { get; internal set; }
  public bool ShadowOrbSmashed { get; internal set; }
  public byte ShadowOrbCount { get; internal set; }
  public int AltarCount { get; internal set; }
}
