namespace Terraria.LeashedEntity;

/// <summary>
/// Stores the per-entity normal butterfly variant.
/// status: implemented
/// componentOwner: LeashedEntitySimulation
/// crossSubsystemOwner: integration-review
/// </summary>
/// <remarks>
/// <para>职责：保存拴系蝴蝶的外观变体。</para>
/// <para>拆分来源：Terraria.GameContent.LeashedEntities.NormalButterflyLeashedCritter。</para>
/// <para>
/// 原始文件：D:/TRbackup/Version4/Terraria.GameContent.LeashedEntities/NormalButterflyLeashedCritter.cs。
/// </para>
/// <para>主要源成员：variant（第 9 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P02-leashed-entity-component-design.md。</para>
/// <para>依据位置：第 1216 行。</para>
/// </remarks>
public struct LeashedButterflyVariantComponent
{
  /// <summary>
  /// Item/style-selected butterfly variant. The accepted range is validated by the future
  /// content/network boundary and is not inferred by this passive component.
  /// </summary>
  public byte Variant;

  public LeashedButterflyVariantComponent(byte variant)
  {
    Variant = variant;
  }
}
