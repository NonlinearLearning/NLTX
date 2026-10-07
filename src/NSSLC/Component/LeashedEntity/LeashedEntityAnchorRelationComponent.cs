using Terraria.Relationships;
using Terraria.WorldStorage;

namespace Terraria.LeashedEntity;

/// <summary>
/// Stores the entity-side relation to a leashed anchor host.
/// status: implemented
/// componentOwner: LeashedEntitySimulation
/// crossSubsystemOwner: integration-review
/// </summary>
/// <remarks>
/// <para>职责：保存拴系实体与锚点实体、方块坐标及持久身份的关系。</para>
/// <para>拆分来源：Terraria.GameContent.LeashedEntity。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria.GameContent/LeashedEntity.cs。</para>
/// <para>主要源成员：AnchorPosition（第 192 行）。</para>
/// <para>重组说明：RelationRevision 用于拆分后的版本或实例生命周期管理。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P02-leashed-entity-component-design.md。</para>
/// <para>依据位置：第 1146 行。</para>
/// </remarks>
public struct LeashedEntityAnchorRelationComponent
{
  /// <summary>
  /// Runtime relation to the anchor entity; not a persistence key.
  /// </summary>
  public EntityReference? AnchorReference;

  /// <summary>
  /// Tile-space relation fact and persistence candidate.
  /// </summary>
  public TileCoordinate? AnchorCoordinate;

  /// <summary>
  /// Candidate durable anchor identity. The owner remains unresolved.
  /// </summary>
  public TileEntityId? PersistentAnchorId;

  /// <summary>
  /// Candidate relation revision for stale attach/remove rejection.
  /// </summary>
  public long? RelationRevision;

  /// <summary>
  /// Derived runtime-reference view; not a persistence check.
  /// </summary>
  public bool HasRuntimeAnchor =>
    AnchorReference.HasValue && !AnchorReference.Value.IsEmpty;

  public LeashedEntityAnchorRelationComponent(
    EntityReference? anchorReference,
    TileCoordinate? anchorCoordinate,
    TileEntityId? persistentAnchorId,
    long? relationRevision)
  {
    AnchorReference = anchorReference;
    AnchorCoordinate = anchorCoordinate;
    PersistentAnchorId = persistentAnchorId;
    RelationRevision = relationRevision;
  }
}
