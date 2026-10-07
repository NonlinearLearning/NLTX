using Terraria.Relationships;

namespace EntityEcs.Components;

/// <summary>
/// 保存实体创建原因、来源主体和因果关联。
/// </summary>
/// <remarks>
/// <para>拆分来源：由 IEntitySource 及 EntitySource_Parent 的实体创建来源模型重组。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria.DataStructures/EntitySource_Parent.cs。</para>
/// <para>重组说明：来源种类、实体引用和因果关系按 ECS 重组，不是单个旧实体的字段搬迁。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/non-authoritative/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-non-authoritative-P06-entity-lifecycle-attribution-component-design.md。
/// </para>
/// <para>依据位置：第 41 行。</para>
/// </remarks>
public struct EntityProvenanceComponent
{
  public EntityProvenanceKind Kind;
  public EntityReference? SourceEntity;
  public EntityReference? ParentEntity;
  public EntityReference? StruckEntity;
  public int? SourceItemDefinitionId;
  public int? SourceProjectileDefinitionId;
  public WorldEventSourceKind? WorldEvent;

  public bool IsRootCause =>
    SourceEntity is null &&
    ParentEntity is null &&
    SourceItemDefinitionId is null &&
    SourceProjectileDefinitionId is null;
}
