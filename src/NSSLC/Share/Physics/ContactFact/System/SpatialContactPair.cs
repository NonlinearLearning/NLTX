namespace Terraria.Share.Physics;

/// <summary>One broad-phase AABB contact. Damage and response are handled elsewhere.</summary>
public readonly record struct SpatialContactPair<TEntity>(
    TEntity First,
    TEntity Second,
    EntityCollisionContact Contact)
    where TEntity : notnull;
