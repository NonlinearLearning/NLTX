using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Terraria.SpatialSimulation;

// status: proposed
// snapshotId: SPATIAL.SNAPSHOT.COLLISION
// crossSubsystemOwner: integration-review
public sealed class SpatialCollisionSnapshot
{
  public SpatialCollisionSnapshot(
    long revision,
    SpatialGeometrySnapshot subject,
    long? subjectEntityId,
    IEnumerable<SpatialEntitySnapshot> entities,
    IEnumerable<SpatialTileSnapshot> tiles)
  {
    if (revision < 0)
    {
      throw new ArgumentOutOfRangeException(
        nameof(revision),
        revision,
        "Revision must be non-negative.");
    }

    if (subject.Revision != revision)
    {
      throw new ArgumentException(
        "Subject geometry must use the collision snapshot revision.",
        nameof(subject));
    }

    if (subjectEntityId is < 0)
    {
      throw new ArgumentOutOfRangeException(
        nameof(subjectEntityId),
        subjectEntityId,
        "Subject entity id must be non-negative.");
    }

    ArgumentNullException.ThrowIfNull(entities);
    ArgumentNullException.ThrowIfNull(tiles);

    Revision = revision;
    Subject = subject;
    SubjectEntityId = subjectEntityId;
    Entities = ImmutableArray.CreateRange(entities);
    Tiles = ImmutableArray.CreateRange(tiles);
  }

  public long Revision { get; }

  public SpatialGeometrySnapshot Subject { get; }

  public long? SubjectEntityId { get; }

  public ImmutableArray<SpatialEntitySnapshot> Entities { get; }

  public ImmutableArray<SpatialTileSnapshot> Tiles { get; }
}
