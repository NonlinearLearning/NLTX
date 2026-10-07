namespace Terraria.LeashedEntity;

/// <summary>
/// Stores the compatibility mapping to Version4's reusable whoAmI slot.
/// status: implemented
/// componentOwner: LeashedEntitySimulation compatibility boundary
/// crossSubsystemOwner: integration-review
/// </summary>
/// <remarks>
/// <para>职责：保存拴系实体的旧数组槽位和槽位代数。</para>
/// <para>拆分来源：Terraria.GameContent.LeashedEntity。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria.GameContent/LeashedEntity.cs。</para>
/// <para>主要源成员：ByWhoAmI（第 181 行）； whoAmI（第 187 行）。</para>
/// <para>重组说明：SlotGeneration 用于拆分后的版本或实例生命周期管理。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P02-leashed-entity-component-design.md。</para>
/// <para>依据位置：第 1141 行。</para>
/// </remarks>
public struct LeashedEntityLegacySlotComponent
{
  /// <summary>
  /// Reusable Version4 whoAmI/ByWhoAmI slot; -1 means unassigned.
  /// </summary>
  public int Slot;

  /// <summary>
  /// Candidate reuse guard. Version4 does not provide this field.
  /// </summary>
  public uint SlotGeneration;

  /// <summary>
  /// Derived slot-assignment view only.
  /// </summary>
  public bool IsAssigned => Slot >= 0;

  public LeashedEntityLegacySlotComponent()
    : this(-1, 0)
  {
  }

  public LeashedEntityLegacySlotComponent(int slot, uint slotGeneration)
  {
    Slot = slot;
    SlotGeneration = slotGeneration;
  }
}
