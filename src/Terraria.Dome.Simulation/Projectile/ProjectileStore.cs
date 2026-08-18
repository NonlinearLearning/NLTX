using System;
using System.Collections.Generic;
using Arch.Core;

namespace Terraria.Dome.Simulation.Projectile;

public sealed class ProjectileStore
{
  private readonly Dictionary<Entity, int> _replicationIdsByEntity = new();

  public void Add(Entity entity, int replicationId)
  {
    if (replicationId <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(replicationId));
    }

    if (!_replicationIdsByEntity.TryAdd(entity, replicationId))
    {
      throw new ArgumentException("The projectile entity is already owned.", nameof(entity));
    }
  }

  public bool Remove(Entity entity, out int replicationId)
  {
    return _replicationIdsByEntity.Remove(entity, out replicationId);
  }

  public bool TryGetValue(Entity entity, out int replicationId)
  {
    return _replicationIdsByEntity.TryGetValue(entity, out replicationId);
  }
}
