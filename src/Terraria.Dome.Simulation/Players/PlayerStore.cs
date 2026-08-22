using System;
using System.Collections;
using System.Collections.Generic;
using Arch.Core;

namespace Terraria.Dome.Simulation.Players;

public sealed class PlayerStore : IReadOnlyDictionary<PlayerHandle, Entity>
{
  private readonly Dictionary<PlayerHandle, Entity> _entities = new();

  public int Count => _entities.Count;

  public IEnumerable<PlayerHandle> Keys => _entities.Keys;

  public IEnumerable<Entity> Values => _entities.Values;

  public Entity this[PlayerHandle player] => _entities[player];

  public void Add(PlayerHandle player, Entity entity)
  {
    if (!player.IsValid)
    {
      throw new ArgumentOutOfRangeException(nameof(player));
    }

    if (!_entities.TryAdd(player, entity))
    {
      throw new ArgumentException("The player handle is already owned.", nameof(player));
    }
  }

  public bool ContainsKey(PlayerHandle player)
  {
    return _entities.ContainsKey(player);
  }

  public bool Remove(PlayerHandle player, out Entity entity)
  {
    return _entities.Remove(player, out entity);
  }

  public bool TryGetValue(PlayerHandle player, out Entity entity)
  {
    return _entities.TryGetValue(player, out entity);
  }

  public IEnumerator<KeyValuePair<PlayerHandle, Entity>> GetEnumerator()
  {
    return _entities.GetEnumerator();
  }

  IEnumerator IEnumerable.GetEnumerator()
  {
    return GetEnumerator();
  }
}
