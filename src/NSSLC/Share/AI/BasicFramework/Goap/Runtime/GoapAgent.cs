// Project host adapted from CrashKonijn/GOAP AgentBehaviour action lifecycle.
// See LICENSE.txt, NOTICE.txt and SOURCES.md. No Unity component or scheduler dependency.
using System;
using System.Collections.Generic;
using System.Numerics;
using Terraria.Share.Ai.Goap.Core;

namespace Terraria.Share.Ai.Goap.Runtime;

/// <summary>One instance per entity. Call Run on the owning simulation thread.</summary>
public sealed class GoapAgent : IAgent, IDisposable {
  private readonly ActionRunner _runner;
  private readonly GoapActionProvider _provider;
  private readonly HashSet<IAction> _disabled = new(ReferenceEqualityComparer.Instance);
  private bool _resolveRequested = true;
  private bool _disposed;
  private bool _isPaused;

  public IActionState ActionState { get; } = new ActionState();
  public IAgentEnvironment Environment { get; }
  public Vector3 Position => Environment.Position;
  public AgentState State { get; private set; }
  public AgentMoveState MoveState { get; private set; }
  public ITarget? CurrentTarget => ActionState.Data?.Target;
  public bool IsPaused {
    get => _isPaused;
    set {
      if (_isPaused == value) {
        return;
      }
      _isPaused = value;
      if (value) {
        StopAction(false);
      } else {
        ResolveAction();
      }
    }
  }

  public GoapAgent(IAgentEnvironment environment, GoapActionProvider provider) {
    ArgumentNullException.ThrowIfNull(environment);
    ArgumentNullException.ThrowIfNull(provider);
    Environment = environment;
    _provider = provider;
    _runner = new ActionRunner(this);
  }

  public void Run(float deltaTime) {
    ObjectDisposedException.ThrowIf(_disposed, this);
    if (!float.IsFinite(deltaTime) || deltaTime < 0f) {
      throw new ArgumentOutOfRangeException(nameof(deltaTime));
    }
    if (IsPaused) {
      return;
    }

    if (_resolveRequested && (ActionState.Action == null ||
                             ActionState.RunState?.MayResolve(this) == true)) {
      _resolveRequested = false;
      _provider.ResolveAction(this);
    }
    if (ActionState.Action != null) {
      try {
        _runner.Run(deltaTime);
      } catch {
        // Cleanup is required even when a domain callback throws. Never swallow its failure.
        StopAction(false);
        throw;
      }
    }
  }

  public bool IsInRange() {
    if (ActionState.Action == null || ActionState.Data == null) {
      return false;
    }
    ITarget? target = CurrentTarget;
    float distance = target == null ? 0f : Vector3.Distance(Position, target.Position);
    return ActionState.Action.IsInRange(this, distance, ActionState.Data);
  }

  public void SetState(AgentState state) {
    State = state;
  }

  public void SetMoveState(AgentMoveState state) {
    MoveState = state;
    if (state == AgentMoveState.InRange) {
      Environment.StopMoving();
    }
  }

  public void SetAction(IAction action, ITarget? target) {
    ObjectDisposedException.ThrowIf(_disposed, this);
    ArgumentNullException.ThrowIfNull(action);
    StopAction(false);
    IActionData data = action.GetData();
    data.Target = target;
    ActionState.SetAction(action, data);
    SetState(AgentState.StartingAction);
    try {
      action.Start(this, data);
    } catch {
      StopAction(false);
      throw;
    }
  }

  public void StopAction(bool resolveAction = true) {
    IAction? action = ActionState.Action;
    IActionData? data = ActionState.Data;
    try {
      if (action != null && data != null) {
        action.Stop(this, data);
      }
    } finally {
      ResetAction(resolveAction);
    }
  }

  public void CompleteAction(bool resolveAction = true) {
    IAction? action = ActionState.Action;
    IActionData? data = ActionState.Data;
    try {
      if (action != null && data != null) {
        action.Complete(this, data);
      }
    } finally {
      ResetAction(resolveAction);
    }
  }

  /// <summary>Request resolution on a subsequent tick; no recursive planning in callbacks.</summary>
  public void ResolveAction() {
    _resolveRequested = true;
  }

  public bool IsDisabled(IAction action) {
    return _disabled.Contains(action);
  }

  public void SetEnabled(IAction action, bool enabled) {
    if (enabled) {
      _disabled.Remove(action);
    } else {
      _disabled.Add(action);
      if (ReferenceEquals(ActionState.Action, action)) {
        StopAction();
      }
    }
    ResolveAction();
  }

  public void Dispose() {
    if (_disposed) {
      return;
    }
    _disposed = true;
    try {
      StopAction(false);
    } finally {
      _disabled.Clear();
    }
  }

  private void ResetAction(bool resolveAction) {
    ActionState.Reset();
    State = AgentState.NoAction;
    MoveState = AgentMoveState.Idle;
    _resolveRequested = resolveAction;
    Environment.StopMoving();
  }
}
