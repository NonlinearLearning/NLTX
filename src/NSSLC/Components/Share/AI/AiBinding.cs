using System;
using System.Collections.Generic;

namespace Terraria.Ai;

/// <summary>
/// Per-entity runtime, owned by the host on one simulation thread. Consume Stop before Dispose.
/// The host must retire it when the owning Arch entity/world is removed or replaced.
/// </summary>
public sealed class AiBinding<TIntent> : IDisposable where TIntent : unmanaged {
  private readonly AiBehavior<TIntent> _behavior;
  private readonly AiOutput<TIntent> _output = new();
  private bool _active;
  private bool _retired;
  private bool _faulted;
  private bool _inOperation;
  private bool _disposed;

  public Type BehaviorType { get; }
  public IReadOnlyList<Type> RequiredComponents => _behavior.RequiredComponents;

  internal AiBinding(Type behaviorType, AiBehavior<TIntent> behavior) {
    BehaviorType = behaviorType;
    _behavior = behavior;
  }

  public AiResult<TIntent> Calculate(in AiComponent component, AiSnapshot snapshot,
                                      float deltaTime) {
    ArgumentNullException.ThrowIfNull(snapshot);
    if (!float.IsFinite(deltaTime) || deltaTime < 0f) {
      throw new ArgumentOutOfRangeException(nameof(deltaTime));
    }
    if (_faulted) {
      throw new InvalidOperationException("This AI binding faulted; stop, dispose and bind again.");
    }
    Enter();
    try {
      if (_retired || component.BehaviorType != BehaviorType) {
        _retired = true;
        StopIfActive();
        return _output.ToResult(AiStatus.BindingChanged);
      }
      if (!component.Enabled) {
        StopIfActive();
        return _output.ToResult(AiStatus.Disabled);
      }
      Type[] missing = _behavior.FindMissing(snapshot);
      if (missing.Length != 0) {
        StopIfActive();
        return _output.ToResult(AiStatus.MissingComponents, missing);
      }
      _active = true;
      _behavior.Calculate(snapshot, deltaTime, _output);
      return _output.ToResult(AiStatus.Calculated);
    } catch {
      _faulted = true;
      throw;
    } finally {
      Exit();
    }
  }

  /// <summary>
  /// Returns cancellation intents once. A healthy binding can calculate again afterward.
  /// </summary>
  public AiResult<TIntent> Stop() {
    Enter();
    try {
      StopIfActive();
      return _output.ToResult(AiStatus.Stopped);
    } catch {
      _faulted = true;
      throw;
    } finally {
      Exit();
    }
  }

  /// <summary>
  /// Resource cleanup only. Call and consume Stop first to deliver cancellation intents.
  /// </summary>
  public void Dispose() {
    if (_disposed) {
      return;
    }
    Enter();
    _disposed = true;
    try {
      try {
        StopIfActive();
      } finally {
        _behavior.Dispose(_output);
      }
    } finally {
      Exit();
    }
  }

  private void StopIfActive() {
    if (!_active) {
      return;
    }
    // Clear before callbacks so a failing Stop is never retried as a duplicate action.
    _active = false;
    _behavior.Stop(_output);
  }

  private void Enter() {
    ObjectDisposedException.ThrowIf(_disposed, this);
    if (_inOperation) {
      throw new InvalidOperationException("AI binding operations cannot be reentered.");
    }
    _inOperation = true;
    _output.Begin();
  }

  private void Exit() {
    _output.End();
    _inOperation = false;
  }
}
