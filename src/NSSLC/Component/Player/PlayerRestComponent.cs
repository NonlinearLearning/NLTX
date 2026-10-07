namespace Terraria.Player;

/// <summary>
/// 保存玩家当前休息活动。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>主要源成员：sitting（第 2286 行）； sleeping（第 2288 行）。</para>
/// <para>重组说明：Activities 是将坐下、睡眠等休息状态合并表达的活动位集。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-P04-player-lifecycle-interaction-component-design.md。
/// </para>
/// <para>依据位置：第 70 行。</para>
/// </remarks>
public sealed class PlayerRestComponent
{
  public PlayerRestActivity Activities { get; internal set; }
}
