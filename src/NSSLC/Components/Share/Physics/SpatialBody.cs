namespace Terraria.Physics;

/// <summary>Pairs a detached spatial snapshot with its entity-collision policy.</summary>
public readonly record struct SpatialBody<TEntity>(
    SpatialEntitySnapshot<TEntity> Snapshot,
    EntityCollisionCapabilityComponent Collision)
    where TEntity : notnull;
