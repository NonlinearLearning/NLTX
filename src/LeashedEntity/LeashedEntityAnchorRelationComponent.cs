using Terraria.Relationships;
using Terraria.WorldStorage;

namespace Terraria.LeashedEntity;

/// <summary>
/// Stores the entity-side relation to a leashed anchor host.
/// status: proposed
/// componentOwner: LeashedEntitySimulation
/// crossSubsystemOwner: integration-review
/// </summary>
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
