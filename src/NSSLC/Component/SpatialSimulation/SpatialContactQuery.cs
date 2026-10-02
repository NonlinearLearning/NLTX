using System;
using System.Collections.Immutable;

using EntityEcs.Components;

namespace Terraria.SpatialSimulation;

// status: proposed
// queryId: SPATIAL.QUERY.CONTACT
// crossSubsystemOwner: integration-review
public static class SpatialContactQuery
{
  public static SpatialContactSnapshot Evaluate(
    SpatialCollisionSnapshot snapshot)
  {
    ArgumentNullException.ThrowIfNull(snapshot);

    ImmutableArray<SpatialEntityContact>.Builder entityContacts =
      ImmutableArray.CreateBuilder<SpatialEntityContact>();
    ImmutableArray<SpatialTileContact>.Builder tileContacts =
      ImmutableArray.CreateBuilder<SpatialTileContact>();
    bool hasUnsupportedSlopeFacts = false;
    SpatialGeometrySnapshot subject = snapshot.Subject;
    SpatialGeometrySnapshot wetProbe =
      SpatialCollisionQuery.CreateWetProbe(in subject);

    for (int index = 0; index < snapshot.Entities.Length; index++)
    {
      SpatialEntitySnapshot entity = snapshot.Entities[index];
      SpatialGeometrySnapshot entityGeometry = entity.Geometry;
      if (!entity.IsCandidate ||
        snapshot.SubjectEntityId == entity.EntityId ||
        !SpatialCollisionQuery.CheckAabb(
          in subject,
          in entityGeometry))
      {
        continue;
      }

      entityContacts.Add(new SpatialEntityContact(entity.EntityId));
    }

    for (int index = 0; index < snapshot.Tiles.Length; index++)
    {
      SpatialTileSnapshot tile = snapshot.Tiles[index];
      if (!tile.Exists || !tile.IsActive)
      {
        continue;
      }

      if (tile.HasUnsupportedSlopeFacts)
      {
        hasUnsupportedSlopeFacts = true;
        continue;
      }

      SpatialGeometrySnapshot tileGeometry =
        tile.CollisionGeometry(snapshot.Revision);
      SpatialGeometrySnapshot liquidGeometry =
        tile.LiquidGeometry(snapshot.Revision);
      bool blocksMovement = tile.BlocksMovement &&
        SpatialCollisionQuery.CheckAabb(
          in subject,
          in tileGeometry);
      bool isWet = tile.HasLiquid &&
        SpatialCollisionQuery.CheckAabb(
          in wetProbe,
          in liquidGeometry);

      if (!blocksMovement && !isWet)
      {
        continue;
      }

      tileContacts.Add(new SpatialTileContact(
        tile.X,
        tile.Y,
        blocksMovement,
        isWet,
        isWet && tile.LiquidKind == LiquidKind.Lava,
        isWet && tile.LiquidKind == LiquidKind.Honey,
        isWet && tile.LiquidKind == LiquidKind.Shimmer));
    }

    return new SpatialContactSnapshot(
      snapshot.Revision,
      entityContacts.ToImmutable(),
      tileContacts.ToImmutable(),
      hasUnsupportedSlopeFacts);
  }
}
