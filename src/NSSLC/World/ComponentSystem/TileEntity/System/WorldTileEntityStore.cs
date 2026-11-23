using System;
using System.Collections.Generic;

namespace Terraria.World;

/// <summary>
/// Authoritative in-memory table of the world's chests, signs and tile entities.
/// </summary>
/// <remarks>
/// The world file is the source: the server decodes chest slots, sign text and tile entity
/// records once at load and keeps them here, so later ticks answer queries without touching the
/// file. Positions key the chest and sign tables, exactly as Version4 resolves them, while tile
/// entities keep their persisted id.
/// </remarks>
public sealed class WorldTileEntityStore
{
  private readonly object _gate = new();
  private readonly Dictionary<WorldTileCoordinate, WorldChestState> _chests = [];
  private readonly Dictionary<WorldTileCoordinate, WorldSignState> _signs = [];
  private readonly Dictionary<short, WorldTileEntityState> _entities = [];
  private long _revision;

  public int ChestCount
  {
    get
    {
      lock (_gate)
      {
        return _chests.Count;
      }
    }
  }

  public int SignCount
  {
    get
    {
      lock (_gate)
      {
        return _signs.Count;
      }
    }
  }

  public int TileEntityCount
  {
    get
    {
      lock (_gate)
      {
        return _entities.Count;
      }
    }
  }

  public long Revision
  {
    get
    {
      lock (_gate)
      {
        return _revision;
      }
    }
  }

  /// <summary>Replaces the whole table; called once per world load.</summary>
  public void Load(
      IReadOnlyList<WorldChestState> chests,
      IReadOnlyList<WorldSignState> signs,
      IReadOnlyList<WorldTileEntityState> entities)
  {
    ArgumentNullException.ThrowIfNull(chests);
    ArgumentNullException.ThrowIfNull(signs);
    ArgumentNullException.ThrowIfNull(entities);
    lock (_gate)
    {
      _chests.Clear();
      _signs.Clear();
      _entities.Clear();
      for (int index = 0; index < chests.Count; index++)
      {
        WorldChestState chest = chests[index];
        if (chest is not null)
        {
          _chests[chest.Position] = chest;
        }
      }

      for (int index = 0; index < signs.Count; index++)
      {
        WorldSignState sign = signs[index];
        if (sign is not null)
        {
          _signs[sign.Position] = sign;
        }
      }

      for (int index = 0; index < entities.Count; index++)
      {
        WorldTileEntityState entity = entities[index];
        if (entity.IsValid)
        {
          _entities[entity.Id] = entity;
        }
      }

      _revision++;
    }
  }

  public bool TryGetChest(WorldTileCoordinate position, out WorldChestState? chest)
  {
    lock (_gate)
    {
      return _chests.TryGetValue(position, out chest);
    }
  }

  public bool TryGetSign(WorldTileCoordinate position, out WorldSignState? sign)
  {
    lock (_gate)
    {
      return _signs.TryGetValue(position, out sign);
    }
  }

  public bool TryGetTileEntity(short id, out WorldTileEntityState entity)
  {
    lock (_gate)
    {
      return _entities.TryGetValue(id, out entity);
    }
  }

  /// <summary>Snapshot of every tile entity, ordered by persisted id.</summary>
  public WorldTileEntityState[] CopyTileEntities()
  {
    lock (_gate)
    {
      WorldTileEntityState[] result = [.. _entities.Values];
      Array.Sort(result, static (left, right) => left.Id.CompareTo(right.Id));
      return result;
    }
  }

  /// <summary>Snapshot of every tile entity of one kind.</summary>
  public WorldTileEntityState[] CopyTileEntities(WorldTileEntityKind kind)
  {
    List<WorldTileEntityState> matches = [];
    lock (_gate)
    {
      foreach (WorldTileEntityState entity in _entities.Values)
      {
        if (entity.Kind == kind)
        {
          matches.Add(entity);
        }
      }
    }

    matches.Sort(static (left, right) => left.Id.CompareTo(right.Id));
    return [.. matches];
  }

  public int CountTileEntities(WorldTileEntityKind kind)
  {
    int count = 0;
    lock (_gate)
    {
      foreach (WorldTileEntityState entity in _entities.Values)
      {
        if (entity.Kind == kind)
        {
          count++;
        }
      }
    }

    return count;
  }
}
