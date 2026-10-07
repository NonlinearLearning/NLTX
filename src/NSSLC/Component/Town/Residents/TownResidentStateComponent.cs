namespace Terraria.Town.Residents;

/// <summary>
/// 保存 NPC 是否为城镇居民。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.NPC。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/NPC.cs。</para>
/// <para>主要源成员：townNPC（第 6397 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-p13-npc-town-progression-component-design.md。</para>
/// <para>依据位置：第 138 行。</para>
/// </remarks>
public sealed class TownResidentStateComponent
{
  public bool IsTownResident { get; private set; }

  public void SetResident(bool isResident)
  {
    IsTownResident = isResident;
  }

  public void Reset()
  {
    IsTownResident = false;
  }
}
