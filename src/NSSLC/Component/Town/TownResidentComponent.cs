namespace Terraria.Town;

/// <summary>
/// 保存城镇居民身份、友好性、住房类别和交互能力。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.NPC。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/NPC.cs。</para>
/// <para>主要源成员：townNPC（第 6397 行）； housingCategory（第 6415 行）； friendly（第 6423 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-1/design/。</para>
/// <para>拆分依据文件：2026-09-05-version4-npc-and-town-simulation-component-design.md。</para>
/// <para>依据位置：第 524 行。</para>
/// </remarks>
public sealed class TownResidentComponent
{
  public TownResidentComponent(
    bool isFriendly,
    int housingCategory,
    TownResidentCapabilities capabilities)
  {
    IsTownResident = true;
    IsFriendly = isFriendly;
    HousingCategory = housingCategory;
    Capabilities = capabilities;
  }

  public bool IsTownResident { get; }

  public bool IsFriendly { get; }

  public int HousingCategory { get; }

  public TownResidentCapabilities Capabilities { get; }

  public bool CanUseHousing =>
    (Capabilities & TownResidentCapabilities.CanUseHousing) != 0;

  public bool CanOpenDialogue =>
    (Capabilities & TownResidentCapabilities.CanOpenDialogue) != 0;
}
