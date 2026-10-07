using System;

namespace Terraria.Town;

// status: proposed
// evidenceStatus: partial for normalized status and revision semantics
// crossSubsystemOwner: integration-review
/// <summary>
/// 保存城镇居民与住房的关系和找房状态。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.NPC。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/NPC.cs。</para>
/// <para>
/// 主要源成员：homeless（第 6403 行）； homelessDespawn（第 6405 行）； homeTileX（第 6411 行）； homeTileY（第 6413 行）。
/// </para>
/// <para>重组说明：AssignmentRevision 用于拆分后的版本或实例生命周期管理。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-1/design/。</para>
/// <para>拆分依据文件：2026-09-05-version4-npc-and-town-simulation-component-design.md。</para>
/// <para>依据位置：第 573 行。</para>
/// </remarks>
public sealed class TownHousingRelationComponent
{
  public TownHousingRelationComponent(
    bool isHomeless,
    TownRoomTilePoint? homeTile,
    bool homelessDespawn,
    int homeSearchTimeout,
    uint assignmentRevision)
  {
    if (homeSearchTimeout < 0)
    {
      throw new ArgumentOutOfRangeException(
        nameof(homeSearchTimeout),
        "Home search timeout cannot be negative.");
    }

    if (isHomeless && homeTile.HasValue)
    {
      throw new ArgumentException(
        "A stable homeless relation cannot also have an assigned home tile.",
        nameof(homeTile));
    }

    IsHomeless = isHomeless;
    HomeTile = homeTile;
    HomelessDespawn = homelessDespawn;
    HomeSearchTimeout = homeSearchTimeout;
    AssignmentRevision = assignmentRevision;
  }

  public bool IsHomeless { get; }

  public TownRoomTilePoint? HomeTile { get; }

  public bool HomelessDespawn { get; }

  public int HomeSearchTimeout { get; }

  public uint AssignmentRevision { get; }

  public bool HasHome => HomeTile.HasValue && !IsHomeless;
}
