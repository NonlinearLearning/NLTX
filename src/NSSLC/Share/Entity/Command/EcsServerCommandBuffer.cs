using Arch.Buffer;
using Arch.Core;
using ArchEntity = Arch.Core.Entity;
using ArchWorld = Arch.Core.World;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

namespace Terraria.Entity;

/// <summary>Owns structural Arch changes produced by one ECS server update.</summary>
/// <remarks>
/// Value writes may happen in a domain system when that system owns the component. Entity
/// creation, destruction and component add/remove operations are recorded here and played back
/// at the end of the host-controlled update boundary.
/// </remarks>
public sealed class EcsServerCommandBuffer : IDisposable
{
  private static long _nextSequence;
  private readonly CommandBuffer _buffer;
  private readonly ArchWorld? _world;
  private readonly HashSet<ArchEntity> _bufferedEntities = [];
  private readonly List<ArchEntity> _bufferedEntityOrder = [];
  private readonly Dictionary<ArchEntity, ArchEntity> _resolvedEntities = [];
  private readonly HashSet<ArchEntity> _destroyedEntities = [];
  private readonly List<Action<EcsServerCommandBuffer>> _afterPlayback = [];
  private readonly List<Action> _onDiscard = [];
  private readonly bool _isBound;
  private bool _playbackStarted;
  private bool _playedBack;
  private bool _playbackFailed;
  private bool _disposed;

  public EcsServerCommandBuffer(int initialCapacity = 128)
  {
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(initialCapacity);
    _buffer = new CommandBuffer(initialCapacity);
    Tick = -1;
    Owner = "standalone";
    Sequence = Interlocked.Increment(ref _nextSequence);
  }

  internal EcsServerCommandBuffer(
      ArchWorld world,
      long tick,
      string owner,
      int initialCapacity = 128)
      : this(world, tick, owner, initialCapacity, isBound: true)
  {
  }

  private EcsServerCommandBuffer(
      ArchWorld? world,
      long tick,
      string owner,
      int initialCapacity,
      bool isBound = false)
  {
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(initialCapacity);
    if (isBound)
    {
      ArgumentNullException.ThrowIfNull(world);
      if (tick < 0)
      {
        throw new ArgumentOutOfRangeException(nameof(tick));
      }
    }

    ArgumentException.ThrowIfNullOrWhiteSpace(owner);
    _buffer = new CommandBuffer(initialCapacity);
    _world = world;
    _isBound = isBound;
    Tick = tick;
    Owner = owner;
    Sequence = Interlocked.Increment(ref _nextSequence);
  }

  public int Size => _buffer.Size;

  public long Tick { get; }

  public string Owner { get; }

  public long Sequence { get; }

  public bool IsBound => _isBound;

  public bool IsPlayedBack => _playedBack;

  public bool IsPlaybackFailed => _playbackFailed;

  public bool IsDisposed => _disposed;

  /// <summary>Registers work that must run after all buffered entities have been resolved.</summary>
  public void RegisterAfterPlayback(Action<EcsServerCommandBuffer> callback)
  {
    EnsureWritable();
    ArgumentNullException.ThrowIfNull(callback);
    _afterPlayback.Add(callback);
  }

  /// <summary>Registers cleanup for commands that are discarded without a successful playback.</summary>
  public void RegisterOnDiscard(Action callback)
  {
    EnsureWritable();
    ArgumentNullException.ThrowIfNull(callback);
    _onDiscard.Add(callback);
  }

  public ArchEntity Create(params ComponentType[] types)
  {
    EnsureWritable();
    ArgumentNullException.ThrowIfNull(types);
    ArchEntity entity = _buffer.Create(types);
    _bufferedEntities.Add(entity);
    _bufferedEntityOrder.Add(entity);
    return entity;
  }

  public void Destroy(in ArchEntity entity)
  {
    EnsureWritable();
    EnsureEntityUsable(entity);
    if (!_destroyedEntities.Add(entity))
    {
      throw new InvalidOperationException(
          "The same ECS entity cannot be destroyed twice in one command buffer.");
    }

    _buffer.Destroy(entity);
  }

  public void Set<T>(in ArchEntity entity, in T component)
  {
    EnsureWritable();
    EnsureEntityUsable(entity);
    _buffer.Set(entity, component);
  }

  public void Add<T>(in ArchEntity entity, in T component)
  {
    EnsureWritable();
    EnsureEntityUsable(entity);
    _buffer.Add(entity, component);
  }

  public void Remove<T>(in ArchEntity entity)
  {
    EnsureWritable();
    EnsureEntityUsable(entity);
    _buffer.Remove<T>(entity);
  }

  public void Playback(ArchWorld world)
  {
    EnsureWritable();
    ArgumentNullException.ThrowIfNull(world);
    EnsurePlaybackWorld(world);
    _playbackStarted = true;
    try
    {
      HashSet<ArchEntity> existingEntities = SnapshotEntities(world);
      _buffer.Playback(world, dispose: false);
      List<ArchEntity> createdEntities = ReadCreatedEntities(existingEntities, world);
      int expectedCreatedEntities = 0;
      for (int index = 0; index < _bufferedEntityOrder.Count; index++)
      {
        if (!_destroyedEntities.Contains(_bufferedEntityOrder[index]))
        {
          expectedCreatedEntities++;
        }
      }

      if (createdEntities.Count != expectedCreatedEntities)
      {
        throw new InvalidOperationException(
            "Arch playback did not expose exactly one live entity for each buffered create.");
      }

      int createdIndex = 0;
      for (int index = 0; index < _bufferedEntityOrder.Count; index++)
      {
        ArchEntity buffered = _bufferedEntityOrder[index];
        if (!_destroyedEntities.Contains(buffered))
        {
          _resolvedEntities.Add(buffered, createdEntities[createdIndex++]);
        }
      }

      _playedBack = true;
      RunAfterPlaybackCallbacks();
    }
    catch
    {
      // Arch playback can partially apply commands. A failed buffer is never replayable.
      _playbackFailed = true;
      RunDiscardCallbacks();
      throw;
    }
  }

  /// <summary>Resolves an entity created by this buffer after successful playback.</summary>
  public ArchEntity Resolve(in ArchEntity entity)
  {
    EnsureReadable();
    if (entity.Id < 0 && !_bufferedEntities.Contains(entity))
    {
      throw new ArgumentException(
          "The entity was not created by this command buffer.", nameof(entity));
    }

    if (entity.Id >= 0)
    {
      return entity;
    }

    if (_resolvedEntities.TryGetValue(entity, out ArchEntity resolved))
    {
      return resolved;
    }

    throw new InvalidOperationException(
        "The buffered entity was destroyed during playback and has no live resolution.");
  }

  public bool WasDestroyed(in ArchEntity entity)
  {
    return _destroyedEntities.Contains(entity);
  }

  public void Dispose()
  {
    if (_disposed)
    {
      return;
    }

    if (!_playedBack)
    {
      RunDiscardCallbacks();
    }

    _disposed = true;
    _buffer.Dispose();
  }

  private void EnsureWritable()
  {
    ObjectDisposedException.ThrowIf(_disposed, this);
    if (_playbackStarted)
    {
      throw new InvalidOperationException(
          _playbackFailed
              ? "The ECS structural command buffer playback failed and cannot be retried."
              : "The ECS structural command buffer has already been played back.");
    }
  }

  private void EnsureReadable()
  {
    ObjectDisposedException.ThrowIf(_disposed, this);
    if (!_playedBack)
    {
      throw new InvalidOperationException(
          "A buffered entity can only be resolved after successful playback.");
    }
  }

  private void RunAfterPlaybackCallbacks()
  {
    Action<EcsServerCommandBuffer>[] callbacks = _afterPlayback.ToArray();
    _afterPlayback.Clear();
    foreach (Action<EcsServerCommandBuffer> callback in callbacks)
    {
      callback(this);
    }

    _onDiscard.Clear();
  }

  private void RunDiscardCallbacks()
  {
    if (_onDiscard.Count == 0)
    {
      return;
    }

    Action[] callbacks = _onDiscard.ToArray();
    _onDiscard.Clear();
    _afterPlayback.Clear();
    foreach (Action callback in callbacks)
    {
      callback();
    }
  }

  private void EnsureEntityUsable(in ArchEntity entity)
  {
    if (entity == ArchEntity.Null)
    {
      throw new ArgumentException("The ECS entity must not be null.", nameof(entity));
    }

    if (_destroyedEntities.Contains(entity))
    {
      throw new InvalidOperationException(
          "An ECS entity cannot receive commands after it has been destroyed.");
    }

    if (entity.Id < 0)
    {
      if (!_bufferedEntities.Contains(entity))
      {
        throw new ArgumentException(
            "The buffered entity was not created by this command buffer.", nameof(entity));
      }

      return;
    }

    if (_world is null)
    {
      return;
    }

    if (entity.WorldId != _world.Id || !_world.IsAlive(entity))
    {
      throw new ArgumentException(
          "The ECS entity does not belong to the bound live world.", nameof(entity));
    }
  }

  private void EnsurePlaybackWorld(ArchWorld world)
  {
    if (_world is not null && !ReferenceEquals(_world, world))
    {
      throw new ArgumentException(
          "The command buffer is bound to a different Arch world.", nameof(world));
    }
  }

  private static HashSet<ArchEntity> SnapshotEntities(ArchWorld world)
  {
    HashSet<ArchEntity> entities = [];
    QueryDescription allEntities = new();
    world.Query(in allEntities, (ArchEntity entity) => entities.Add(entity));
    return entities;
  }

  private List<ArchEntity> ReadCreatedEntities(
      HashSet<ArchEntity> existingEntities,
      ArchWorld world)
  {
    PropertyInfo? entitiesProperty = typeof(CommandBuffer).GetProperty(
        "Entities",
        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
    if (entitiesProperty is null)
    {
      throw new InvalidOperationException(
          "The Arch command buffer does not expose its resolved entity list.");
    }

    object? value = entitiesProperty.GetValue(_buffer);
    if (value is not IEnumerable entities)
    {
      throw new InvalidOperationException(
          "The Arch command buffer resolved entity list is not enumerable.");
    }

    List<ArchEntity> created = [];
    foreach (object? candidate in entities)
    {
      if (candidate is ArchEntity entity &&
          !existingEntities.Contains(entity) &&
          world.IsAlive(entity))
      {
        created.Add(entity);
      }
    }

    return created;
  }
}
