using System;
using System.Collections.Generic;

namespace Terraria.Ai;

/// <summary>
/// Converts host-owned ECS state into one calculation-local AI snapshot.
/// </summary>
public interface IAiSnapshotProvider<TEntity> where TEntity : notnull {
  AiSnapshot Capture(TEntity entity, IReadOnlyList<Type> requiredComponents);
}
