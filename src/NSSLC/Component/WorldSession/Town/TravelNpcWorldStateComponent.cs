namespace Terraria.WorldSession.Town;

/// <summary>
/// 保存世界旅行商人活动状态。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.NPC。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/NPC.cs。</para>
/// <para>主要源成员：travelNPC（第 6401 行）。</para>
/// <para>重组说明：原全局状态归属于世界，不属于单个旅行商人实例。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-p13-npc-town-progression-component-design.md。</para>
/// <para>依据位置：第 139 行。</para>
/// </remarks>
public sealed class TravelNpcWorldStateComponent
{
  public bool IsTravelNpcActive { get; private set; }

  public void SetTravelNpcActive(bool isActive)
  {
    IsTravelNpcActive = isActive;
  }

  public void Reset()
  {
    IsTravelNpcActive = false;
  }
}
