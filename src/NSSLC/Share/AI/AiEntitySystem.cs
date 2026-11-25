using System;
using System.Collections.Generic;

namespace Terraria.Share.Ai;

/// <summary>
/// ECS host adapter for AI bindings. It stores no component data and never writes an entity.
/// </summary>
public sealed class AiEntitySystem<TEntity, TIntent> : IDisposable
    where TEntity : notnull
    where TIntent : unmanaged {
  private readonly AiSystem<TIntent> _aiSystem;
  private readonly Dictionary<TEntity, AiBinding<TIntent>> _bindings = [];
  private bool _disposed;

  public int BindingCount => _bindings.Count;

  public AiEntitySystem(AiSystem<TIntent> aiSystem) {
    ArgumentNullException.ThrowIfNull(aiSystem);
    _aiSystem = aiSystem;
  }

  /// <summary>
  /// Creates one independent runtime for the entity; the host retains the entity key.
  /// </summary>
  public void Attach(TEntity entity, in AiComponent component) {
    EnsureActive();
    ArgumentNullException.ThrowIfNull(entity);
    AiBinding<TIntent> binding = _aiSystem.Bind(component);
    if (!_bindings.TryAdd(entity, binding)) {
      binding.Dispose();
      throw new InvalidOperationException("The entity already has an AI binding.");
    }
  }

  public bool IsAttached(TEntity entity) {
    EnsureActive();
    ArgumentNullException.ThrowIfNull(entity);
    return _bindings.ContainsKey(entity);
  }

  public IReadOnlyList<Type> GetRequiredComponents(TEntity entity) {
    EnsureActive();
    ArgumentNullException.ThrowIfNull(entity);
    if (!_bindings.TryGetValue(entity, out AiBinding<TIntent>? binding)) {
      throw new InvalidOperationException("The entity has no AI binding.");
    }
    return binding.RequiredComponents;
  }

  /// <summary>
  /// Calculates from a host-owned snapshot and returns the entity-associated result.
  /// The snapshot is not stored after this call.
  /// </summary>
  public AiEntityResult<TEntity, TIntent> Calculate(
      TEntity entity, in AiComponent component, AiSnapshot snapshot, float deltaTime) {
    EnsureActive();
    ArgumentNullException.ThrowIfNull(entity);
    if (!_bindings.TryGetValue(entity, out AiBinding<TIntent>? binding)) {
      throw new InvalidOperationException("The entity has no AI binding.");
    }
    AiResult<TIntent> result = binding.Calculate(component, snapshot, deltaTime);
    return new AiEntityResult<TEntity, TIntent>(entity, result);
  }

  /// <summary>
  /// Captures host state and runs the same calculation path as the snapshot overload.
  /// </summary>
  public AiEntityResult<TEntity, TIntent> Calculate(
      TEntity entity, in AiComponent component, IAiSnapshotProvider<TEntity> snapshots,
      float deltaTime) {
    EnsureActive();
    ArgumentNullException.ThrowIfNull(entity);
    ArgumentNullException.ThrowIfNull(snapshots);
    if (!_bindings.TryGetValue(entity, out AiBinding<TIntent>? binding)) {
      throw new InvalidOperationException("The entity has no AI binding.");
    }
    AiSnapshot snapshot = snapshots.Capture(entity, binding.RequiredComponents);
    ArgumentNullException.ThrowIfNull(snapshot);
    AiResult<TIntent> result = binding.Calculate(component, snapshot, deltaTime);
    return new AiEntityResult<TEntity, TIntent>(entity, result);
  }

  public AiEntityResult<TEntity, TIntent> Stop(TEntity entity) {
    EnsureActive();
    ArgumentNullException.ThrowIfNull(entity);
    if (!_bindings.TryGetValue(entity, out AiBinding<TIntent>? binding)) {
      throw new InvalidOperationException("The entity has no AI binding.");
    }
    return new AiEntityResult<TEntity, TIntent>(entity, binding.Stop());
  }

  /// <summary>Stops, returns cleanup intents, disposes and removes one entity binding.</summary>
  public AiEntityResult<TEntity, TIntent> Detach(TEntity entity) {
    EnsureActive();
    ArgumentNullException.ThrowIfNull(entity);
    if (!_bindings.Remove(entity, out AiBinding<TIntent>? binding)) {
      throw new InvalidOperationException("The entity has no AI binding.");
    }
    try {
      AiResult<TIntent> result = binding.Stop();
      return new AiEntityResult<TEntity, TIntent>(entity, result);
    } finally {
      binding.Dispose();
    }
  }

  /// <summary>
  /// Idempotent lifecycle cleanup for hosts that discover component removal by query.
  /// </summary>
  public bool TryDetach(TEntity entity, out AiEntityResult<TEntity, TIntent> result) {
    EnsureActive();
    ArgumentNullException.ThrowIfNull(entity);
    if (!_bindings.ContainsKey(entity)) {
      result = default;
      return false;
    }
    result = Detach(entity);
    return true;
  }

  /// <summary>
  /// Stops the old behavior and installs a fresh runtime for a changed AI declaration.
  /// </summary>
  public AiEntityResult<TEntity, TIntent> Rebind(TEntity entity, in AiComponent component) {
    EnsureActive();
    ArgumentNullException.ThrowIfNull(entity);
    if (!_bindings.TryGetValue(entity, out AiBinding<TIntent>? previous)) {
      throw new InvalidOperationException("The entity has no AI binding.");
    }

    AiBinding<TIntent> replacement = _aiSystem.Bind(component);
    try {
      AiResult<TIntent> stopped = previous.Stop();
      _bindings[entity] = replacement;
      previous.Dispose();
      return new AiEntityResult<TEntity, TIntent>(entity, stopped);
    } catch {
      replacement.Dispose();
      throw;
    }
  }

  /// <summary>
  /// Stops every binding and returns all cleanup results in no defined entity order.
  /// </summary>
  public IReadOnlyList<AiEntityResult<TEntity, TIntent>> StopAll() {
    EnsureActive();
    List<AiEntityResult<TEntity, TIntent>> results = new(_bindings.Count);
    foreach (KeyValuePair<TEntity, AiBinding<TIntent>> pair in _bindings) {
      results.Add(new AiEntityResult<TEntity, TIntent>(pair.Key, pair.Value.Stop()));
    }
    return Array.AsReadOnly(results.ToArray());
  }

  public void Dispose() {
    if (_disposed) {
      return;
    }
    _disposed = true;
    foreach (AiBinding<TIntent> binding in _bindings.Values) {
      binding.Dispose();
    }
    _bindings.Clear();
  }

  private void EnsureActive() {
    ObjectDisposedException.ThrowIf(_disposed, this);
  }
}
