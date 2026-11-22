using System;
using Terraria.Ai.Htn.Contexts;
using Terraria.Ai.Htn.PrimitiveTasks;

namespace Terraria.Ai.Htn;

/// <summary>Per-entity host. Domain, context and callbacks are used on one simulation thread.</summary>
public sealed class HtnAgent<TContext> : IDisposable where TContext : BaseContext {
  private readonly Planner<TContext> _planner = new();
  private bool _disposed;

  public TContext Context { get; }
  public Domain<TContext> Domain { get; }

  public HtnAgent(Domain<TContext> domain, TContext context) {
    ArgumentNullException.ThrowIfNull(domain);
    ArgumentNullException.ThrowIfNull(context);
    Domain = domain;
    Context = context;
    if (!context.IsInitialized) {
      context.Init();
    }
  }

  public void Tick() {
    ObjectDisposedException.ThrowIf(_disposed, this);
    // Bound immediate retry recursion. Failed/completed plans are retried on a later tick.
    try {
      _planner.Tick(Domain, Context, allowImmediateReplanAndExecute: false);
    } catch {
      Stop();
      throw;
    }
  }

  public void Stop() {
    try {
      if (Context.PlannerState.CurrentTask is IPrimitiveTask task) {
        task.Operator?.Abort(Context);
      }
    } finally {
      Context.PlannerState.CurrentTask = null;
      Context.PlannerState.Plan.Clear();
      Context.PlannerState.LastStatus = TaskStatus.Failure;
      Context.PartialPlanQueue.Clear();
      Context.HasPausedPartialPlan = false;
      Context.MethodTraversalRecord.Clear();
      Context.LastMTR.Clear();
      Context.MTRDebug?.Clear();
      Context.LastMTRDebug?.Clear();
      Context.DecompositionLog?.Clear();
      foreach (var stack in Context.WorldStateChangeStack) {
        stack.Clear();
      }
      Context.ContextState = ContextState.Executing;
      Context.IsDirty = true;
    }
  }

  public void Dispose() {
    if (_disposed) {
      return;
    }
    _disposed = true;
    Stop();
  }
}
