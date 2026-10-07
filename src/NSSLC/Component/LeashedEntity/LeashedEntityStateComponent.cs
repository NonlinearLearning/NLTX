namespace Terraria.LeashedEntity;

/// <summary>
/// Stores the definition binding for one leashed entity.
/// status: implemented
/// componentOwner: LeashedEntitySimulation
/// crossSubsystemOwner: integration-review
/// </summary>
/// <remarks>
/// <para>职责：保存拴系实体的内容定义引用。</para>
/// <para>拆分来源：Terraria.GameContent.LeashedEntity。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria.GameContent/LeashedEntity.cs。</para>
/// <para>主要源成员：Type（第 190 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P02-leashed-entity-component-design.md。</para>
/// <para>依据位置：第 1145 行。</para>
/// </remarks>
public struct LeashedEntityStateComponent
{
  /// <summary>
  /// The Version4 registry definition reference. Zero means unbound.
  /// </summary>
  public int DefinitionId;

  /// <summary>
  /// Indicates whether the entity has a validated definition binding.
  /// </summary>
  public bool IsBound => DefinitionId > 0;

  public LeashedEntityStateComponent(int definitionId)
  {
    DefinitionId = definitionId;
  }
}
