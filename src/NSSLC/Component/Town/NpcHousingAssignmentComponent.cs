namespace Terraria.Town;

/// <summary>
/// 保存 NPC 住房分配状态、位置和再次搜索条件。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.NPC。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/NPC.cs。</para>
/// <para>
/// 主要源成员：homeless（第 6403 行）； homelessDespawn（第 6405 行）； lookForHomeTimeout（第 6407 行）； homeTileX（第
/// 6411 行）； homeTileY（第 6413 行）。
/// </para>
/// <para>重组说明：AssignmentRevision 用于拆分后的版本或实例生命周期管理。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-p13-npc-town-progression-component-design.md。</para>
/// <para>依据位置：第 149 行。</para>
/// </remarks>
public sealed class NpcHousingAssignmentComponent
{
  public NpcHousingAssignmentComponent(
    TownHousingStatus status,
    TownRoomTilePoint? homeTile,
    int searchCooldownTicks,
    bool despawnWhenHomeless,
    uint assignmentRevision)
  {
    Status = status;
    HomeTile = homeTile;
    SearchCooldownTicks = searchCooldownTicks;
    DespawnWhenHomeless = despawnWhenHomeless;
    AssignmentRevision = assignmentRevision;
  }

  public TownHousingStatus Status { get; }

  public TownRoomTilePoint? HomeTile { get; }

  public int SearchCooldownTicks { get; }

  public bool DespawnWhenHomeless { get; }

  public uint AssignmentRevision { get; }

  public bool IsHomeless => Status == TownHousingStatus.Homeless;

  public bool HasHome => HomeTile.HasValue && !IsHomeless;

  public bool IsEligibleForSearch =>
    (Status is TownHousingStatus.Homeless or TownHousingStatus.LookingForHome) &&
    SearchCooldownTicks == 0;
}
