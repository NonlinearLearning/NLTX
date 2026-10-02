using System;

namespace Terraria.SpatialSimulation;

// status: proposed
// snapshotId: SPATIAL.SNAPSHOT.ENTITY
// crossSubsystemOwner: integration-review
public readonly record struct SpatialEntitySnapshot
{
  public SpatialEntitySnapshot(
    long entityId,
    SpatialGeometrySnapshot geometry,
    bool isActive = true,
    bool collides = true)
  {
    if (entityId < 0)
    {
      throw new ArgumentOutOfRangeException(
        nameof(entityId),
        entityId,
        "Entity id must be non-negative.");
    }

    EntityId = entityId;
    Geometry = geometry;
    IsActive = isActive;
    Collides = collides;
  }

  public long EntityId { get; }

  public SpatialGeometrySnapshot Geometry { get; }

  public bool IsActive { get; }

  public bool Collides { get; }

  public bool IsCandidate => IsActive && Collides && Geometry.HasArea;
}
