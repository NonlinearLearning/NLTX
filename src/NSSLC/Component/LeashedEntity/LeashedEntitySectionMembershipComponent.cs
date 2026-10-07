using Terraria.WorldStorage;

namespace Terraria.LeashedEntity;

/// <summary>
/// Stores entity-side section membership and local activity observation.
/// status: implemented
/// componentOwner: LeashedEntitySimulation
/// crossSubsystemOwner: integration-review
/// </summary>
/// <remarks>
/// <para>职责：保存拴系实体所属区块、区块槽位及激活状态。</para>
/// <para>拆分来源：Terraria.GameContent.LeashedEntity。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria.GameContent/LeashedEntity.cs。</para>
/// <para>主要源成员：sectionSlot（第 183 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P02-leashed-entity-component-design.md。</para>
/// <para>依据位置：第 1142 行。</para>
/// </remarks>
public struct LeashedEntitySectionMembershipComponent
{
  /// <summary>
  /// Current world section. Global section activity is not stored here.
  /// </summary>
  public SectionCoordinate Section;

  /// <summary>
  /// Reusable slot in the current section bucket; -1 means not indexed.
  /// </summary>
  public int SectionSlot;

  /// <summary>
  /// Local activity observation/cache, not the global authority.
  /// </summary>
  public bool IsSectionActive;

  /// <summary>
  /// Candidate observation tick, not a global clock owner.
  /// </summary>
  public long? LastActivationTick;

  /// <summary>
  /// Derived membership view; compaction may change the slot.
  /// </summary>
  public bool IsIndexed => SectionSlot >= 0;

  public LeashedEntitySectionMembershipComponent(
    SectionCoordinate section,
    int sectionSlot,
    bool isSectionActive,
    long? lastActivationTick)
  {
    Section = section;
    SectionSlot = sectionSlot;
    IsSectionActive = isSectionActive;
    LastActivationTick = lastActivationTick;
  }
}
