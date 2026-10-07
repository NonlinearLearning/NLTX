namespace Terraria.WorldSession.Calendar;

/// <summary>
/// 保存世界已击退入侵和小丑的历史记录。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.NPC。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/NPC.cs。</para>
/// <para>
/// 主要源成员：downedGoblins（第 6221 行）； downedFrost（第 6223 行）； downedPirates（第 6225 行）； downedClown（第
/// 6227 行）； downedMartians（第 6233 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-p13-npc-town-progression-component-design.md。</para>
/// <para>依据位置：第 1020 行。</para>
/// </remarks>
public sealed class InvasionHistoryStateComponent
{
  public bool DefeatedGoblins;
  public bool DefeatedFrost;
  public bool DefeatedPirates;
  public bool DefeatedMartians;

  // Compatibility candidate only. Version4 direct evidence for this flag is incomplete.
  public bool DefeatedClown;
}
