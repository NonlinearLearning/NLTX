using System;
using ArchEntity = Arch.Core.Entity;
using Terraria.Share.Relationships;

namespace Terraria.Share.Entities;

/// <summary>
/// Represents an entity created by the current tick command buffer.
/// </summary>
/// <remarks>
/// The buffered entity is valid only for commands recorded into the same buffer. Call
/// <see cref="Resolve"/> from an after-playback callback or a later phase after the host barrier.
/// </remarks>
public sealed class EcsServerEntityCreationTicket
{
  private readonly EcsServerCommandBuffer _commands;
  private readonly EntityComponentSet _components;
  private readonly EntityRuntimeRegistry? _registry;
  private readonly EntityRuntimeHandle? _handle;
  private ArchEntity _resolvedEntity;
  private bool _completed;
  private bool _aborted;

  internal EcsServerEntityCreationTicket(
      EcsServerCommandBuffer commands,
      in ArchEntity bufferedEntity,
      EntityComponentSet components,
      EntityRuntimeRegistry? registry,
      EntityRuntimeHandle? handle)
  {
    _commands = commands ?? throw new ArgumentNullException(nameof(commands));
    _components = components ?? throw new ArgumentNullException(nameof(components));
    _registry = registry;
    _handle = handle;
    BufferedEntity = bufferedEntity;
  }

  /// <summary>Temporary entity key accepted by commands recorded in the same tick.</summary>
  public ArchEntity BufferedEntity { get; }

  /// <summary>Runtime handle allocated for the entity, when its definition declares an identity.</summary>
  public EntityRuntimeHandle? RuntimeHandle => _handle;

  public bool IsCompleted => _completed;

  public bool IsAborted => _aborted;

  /// <summary>Reads an immutable component value from the creation plan before playback.</summary>
  public TComponent GetInitial<TComponent>()
  {
    return _components.Get<TComponent>();
  }

  /// <summary>Returns the live Arch entity after successful playback.</summary>
  public ArchEntity Resolve()
  {
    if (_aborted)
    {
      throw new InvalidOperationException("The entity creation ticket was discarded.");
    }

    if (_completed)
    {
      return _resolvedEntity;
    }

    _resolvedEntity = _commands.Resolve(BufferedEntity);
    return _resolvedEntity;
  }

  internal void Complete(EcsServerCommandBuffer commands)
  {
    if (_aborted)
    {
      throw new InvalidOperationException("An aborted entity creation ticket was completed.");
    }

    if (_completed)
    {
      throw new InvalidOperationException("The entity creation ticket was completed twice.");
    }

    if (commands.WasDestroyed(BufferedEntity))
    {
      Abort();
      return;
    }

    ArchEntity resolved = commands.Resolve(BufferedEntity);
    try
    {
      if (_handle is EntityRuntimeHandle handle)
      {
        _registry!.Register(handle, resolved);
      }

      _resolvedEntity = resolved;
      _completed = true;
    }
    catch
    {
      if (_handle is EntityRuntimeHandle handle)
      {
        _registry!.Remove(handle, out _);
      }

      throw;
    }
  }

  internal void Abort()
  {
    if (_completed || _aborted)
    {
      return;
    }

    if (_handle is EntityRuntimeHandle handle)
    {
      _registry!.Remove(handle, out _);
    }

    _aborted = true;
  }
}
