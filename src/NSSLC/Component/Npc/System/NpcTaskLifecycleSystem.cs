using Terraria.Relationships;

namespace Terraria.Npc;

public static class NpcTaskLifecycleSystem
{
  public static NpcTaskLifecycleResult Enter(
    NpcTaskStateComponent state,
    NpcTaskKind kind,
    int initialCursor = 0)
  {
    ArgumentNullException.ThrowIfNull(state);
    if (kind == NpcTaskKind.None || !Enum.IsDefined(kind) || initialCursor < 0)
    {
      return Snapshot(state, accepted: false, changed: false);
    }

    if (state.IsRunning && state.Kind == kind)
    {
      return Snapshot(state, accepted: true, changed: false);
    }

    bool changed = state.Phase != NpcTaskPhase.Idle || state.Kind != kind;
    if (state.IsRunning && state.Kind != kind)
    {
      state.Interrupt(NpcTaskFailureReason.TaskReplaced);
    }

    state.Start(kind, initialCursor);
    return Snapshot(state, accepted: true, changed);
  }

  /// <summary>
  /// Advances a task when the caller owns a synchronous, current-state operation.
  /// Delayed or leased work must use the reference-bound overload.
  /// </summary>
  public static NpcTaskLifecycleResult Advance(
    NpcTaskStateComponent state,
    NpcTaskKind kind)
  {
    ArgumentNullException.ThrowIfNull(state);
    if (!state.IsRunning || state.Kind != kind)
    {
      return Snapshot(state, accepted: false, changed: false);
    }

    state.Advance();
    return Snapshot(state, accepted: true, changed: true);
  }

  /// <summary>
  /// Advances a task only when the entity handle, task generation, running phase,
  /// and expected kind all match the attached state.
  /// </summary>
  public static NpcTaskReferenceOperationResult Advance(
    NpcTaskStateComponent state,
    RuntimeEntityHandle currentEntityHandle,
    NpcTaskReference reference,
    NpcTaskKind kind)
  {
    ArgumentNullException.ThrowIfNull(state);
    NpcTaskReferenceOperationResult validation = ValidateReference(
      state,
      currentEntityHandle,
      reference,
      kind,
      NpcTaskFailureReason.None,
      validateFailureReason: false);
    if (!validation.Accepted)
    {
      return validation;
    }

    NpcTaskLifecycleResult before = validation.CurrentTask;
    state.Advance();
    return AcceptedOperation(before, Snapshot(state, accepted: true, changed: true));
  }

  /// <summary>
  /// Completes a task for a synchronous, current-state caller.
  /// Delayed or leased work must use the reference-bound overload.
  /// </summary>
  public static NpcTaskLifecycleResult Complete(
    NpcTaskStateComponent state,
    NpcTaskKind kind)
  {
    ArgumentNullException.ThrowIfNull(state);
    if (!state.IsRunning || state.Kind != kind)
    {
      return Snapshot(state, accepted: false, changed: false);
    }

    state.Complete();
    return Snapshot(state, accepted: true, changed: true);
  }

  /// <summary>
  /// Completes a task only when the entity handle, task generation, running phase,
  /// and expected kind all match the attached state.
  /// </summary>
  public static NpcTaskReferenceOperationResult Complete(
    NpcTaskStateComponent state,
    RuntimeEntityHandle currentEntityHandle,
    NpcTaskReference reference,
    NpcTaskKind kind)
  {
    ArgumentNullException.ThrowIfNull(state);
    NpcTaskReferenceOperationResult validation = ValidateReference(
      state,
      currentEntityHandle,
      reference,
      kind,
      NpcTaskFailureReason.None,
      validateFailureReason: false);
    if (!validation.Accepted)
    {
      return validation;
    }

    NpcTaskLifecycleResult before = validation.CurrentTask;
    state.Complete();
    return AcceptedOperation(before, Snapshot(state, accepted: true, changed: true));
  }

  /// <summary>
  /// Fails a task for a synchronous, current-state caller.
  /// Delayed or leased work must use the reference-bound overload.
  /// </summary>
  public static NpcTaskLifecycleResult Fail(
    NpcTaskStateComponent state,
    NpcTaskKind kind,
    NpcTaskFailureReason reason)
  {
    ArgumentNullException.ThrowIfNull(state);
    if (!state.IsRunning || state.Kind != kind || reason == NpcTaskFailureReason.None ||
        !Enum.IsDefined(reason))
    {
      return Snapshot(state, accepted: false, changed: false);
    }

    state.Fail(reason);
    return Snapshot(state, accepted: true, changed: true);
  }

  /// <summary>
  /// Fails a task only when the entity handle, task generation, running phase,
  /// expected kind, and failure reason all match the attached state.
  /// </summary>
  public static NpcTaskReferenceOperationResult Fail(
    NpcTaskStateComponent state,
    RuntimeEntityHandle currentEntityHandle,
    NpcTaskReference reference,
    NpcTaskKind kind,
    NpcTaskFailureReason reason)
  {
    ArgumentNullException.ThrowIfNull(state);
    NpcTaskReferenceOperationResult validation = ValidateReference(
      state,
      currentEntityHandle,
      reference,
      kind,
      reason,
      validateFailureReason: true);
    if (!validation.Accepted)
    {
      return validation;
    }

    NpcTaskLifecycleResult before = validation.CurrentTask;
    state.Fail(reason);
    return AcceptedOperation(before, Snapshot(state, accepted: true, changed: true));
  }

  /// <summary>
  /// Interrupts a task for a synchronous, current-state caller.
  /// Delayed or leased work must use the reference-bound overload.
  /// </summary>
  public static NpcTaskLifecycleResult Interrupt(
    NpcTaskStateComponent state,
    NpcTaskKind kind,
    NpcTaskFailureReason reason)
  {
    ArgumentNullException.ThrowIfNull(state);
    if (!state.IsRunning || state.Kind != kind || reason == NpcTaskFailureReason.None ||
        !Enum.IsDefined(reason))
    {
      return Snapshot(state, accepted: false, changed: false);
    }

    state.Interrupt(reason);
    return Snapshot(state, accepted: true, changed: true);
  }

  /// <summary>
  /// Interrupts a task only when the entity handle, task generation, running phase,
  /// expected kind, and failure reason all match the attached state.
  /// </summary>
  public static NpcTaskReferenceOperationResult Interrupt(
    NpcTaskStateComponent state,
    RuntimeEntityHandle currentEntityHandle,
    NpcTaskReference reference,
    NpcTaskKind kind,
    NpcTaskFailureReason reason)
  {
    ArgumentNullException.ThrowIfNull(state);
    NpcTaskReferenceOperationResult validation = ValidateReference(
      state,
      currentEntityHandle,
      reference,
      kind,
      reason,
      validateFailureReason: true);
    if (!validation.Accepted)
    {
      return validation;
    }

    NpcTaskLifecycleResult before = validation.CurrentTask;
    state.Interrupt(reason);
    return AcceptedOperation(before, Snapshot(state, accepted: true, changed: true));
  }

  public static NpcTaskLifecycleResult Reset(NpcTaskStateComponent state)
  {
    ArgumentNullException.ThrowIfNull(state);
    ulong taskGeneration = state.TaskGeneration;
    state.Reset();
    return Snapshot(state, accepted: true, changed: state.TaskGeneration != taskGeneration);
  }

  public static bool TryCaptureReference(
    NpcTaskStateComponent state,
    RuntimeEntityHandle entityHandle,
    out NpcTaskReference reference)
  {
    ArgumentNullException.ThrowIfNull(state);
    if (!entityHandle.IsAssigned)
    {
      reference = default;
      return false;
    }

    reference = new NpcTaskReference(entityHandle, state.TaskGeneration);
    return true;
  }

  public static NpcTaskTerminationResult Terminate(
    NpcTaskStateComponent state,
    RuntimeEntityHandle currentEntityHandle,
    NpcTaskReference reference,
    NpcTaskEndReason reason)
  {
    ArgumentNullException.ThrowIfNull(state);
    NpcTaskLifecycleResult before = Snapshot(state, accepted: true, changed: false);
    if (!currentEntityHandle.IsAssigned || !reference.IsAssigned)
    {
      return Rejected(
        NpcTaskTerminationRejectionReason.InvalidReference,
        reason,
        before);
    }

    if (reference.EntityHandle != currentEntityHandle)
    {
      return Rejected(
        NpcTaskTerminationRejectionReason.StaleEntityReference,
        reason,
        before);
    }

    if (reason == NpcTaskEndReason.None || !Enum.IsDefined(reason))
    {
      return Rejected(
        NpcTaskTerminationRejectionReason.InvalidEndReason,
        reason,
        before);
    }

    if (reference.TaskGeneration != state.TaskGeneration)
    {
      return Rejected(
        NpcTaskTerminationRejectionReason.StaleTaskReference,
        reason,
        before);
    }

    if (state.LastEndedTaskGeneration == reference.TaskGeneration &&
        state.LastEndReason != NpcTaskEndReason.None)
    {
      if (state.LastEndReason != reason)
      {
        return new NpcTaskTerminationResult(
          Outcome: NpcTaskTerminationOutcome.EarlierEndReasonPreserved,
          RejectionReason: NpcTaskTerminationRejectionReason.None,
          RequestedEndReason: reason,
          EndReason: state.LastEndReason,
          PreviousTask: before,
          CurrentTask: before);
      }

      return new NpcTaskTerminationResult(
        Outcome: NpcTaskTerminationOutcome.AlreadyEnded,
        RejectionReason: NpcTaskTerminationRejectionReason.None,
        RequestedEndReason: reason,
        EndReason: state.LastEndReason,
        PreviousTask: before,
        CurrentTask: before);
    }

    bool changed = state.Phase != NpcTaskPhase.Idle ||
      state.Kind != NpcTaskKind.None ||
      state.Cursor != 0 ||
      state.FailureReason != NpcTaskFailureReason.None ||
      state.LastEndReason != reason;
    state.End(reason);
    NpcTaskLifecycleResult after = Snapshot(state, accepted: true, changed);
    return new NpcTaskTerminationResult(
      Outcome: NpcTaskTerminationOutcome.Ended,
      RejectionReason: NpcTaskTerminationRejectionReason.None,
      RequestedEndReason: reason,
      EndReason: reason,
      PreviousTask: before,
      CurrentTask: after);
  }

  private static NpcTaskLifecycleResult Snapshot(
    NpcTaskStateComponent state,
    bool accepted,
    bool changed)
  {
    return new NpcTaskLifecycleResult(
      accepted,
      changed,
      state.Kind,
      state.Phase,
      state.Cursor,
      state.FailureReason)
    {
      TaskGeneration = state.TaskGeneration,
      EndReason = state.LastEndReason,
    };
  }

  private static NpcTaskReferenceOperationResult ValidateReference(
    NpcTaskStateComponent state,
    RuntimeEntityHandle currentEntityHandle,
    NpcTaskReference reference,
    NpcTaskKind kind,
    NpcTaskFailureReason failureReason,
    bool validateFailureReason)
  {
    NpcTaskLifecycleResult currentTask = Snapshot(state, accepted: true, changed: false);
    if (!currentEntityHandle.IsAssigned ||
        !reference.IsAssigned ||
        reference.TaskGeneration == 0)
    {
      return RejectedOperation(
        NpcTaskReferenceOperationRejectionReason.InvalidReference,
        currentTask);
    }

    if (reference.EntityHandle != currentEntityHandle)
    {
      return RejectedOperation(
        NpcTaskReferenceOperationRejectionReason.StaleEntityReference,
        currentTask);
    }

    if (reference.TaskGeneration != state.TaskGeneration)
    {
      return RejectedOperation(
        NpcTaskReferenceOperationRejectionReason.StaleTaskReference,
        currentTask);
    }

    if (kind == NpcTaskKind.None || !Enum.IsDefined(kind))
    {
      return RejectedOperation(
        NpcTaskReferenceOperationRejectionReason.InvalidTaskKind,
        currentTask);
    }

    if (!state.IsRunning)
    {
      return RejectedOperation(
        NpcTaskReferenceOperationRejectionReason.NotRunning,
        currentTask);
    }

    if (state.Kind != kind)
    {
      return RejectedOperation(
        NpcTaskReferenceOperationRejectionReason.TaskKindMismatch,
        currentTask);
    }

    if (validateFailureReason &&
        (failureReason == NpcTaskFailureReason.None || !Enum.IsDefined(failureReason)))
    {
      return RejectedOperation(
        NpcTaskReferenceOperationRejectionReason.InvalidFailureReason,
        currentTask);
    }

    return new NpcTaskReferenceOperationResult(
      Accepted: true,
      Changed: false,
      RejectionReason: NpcTaskReferenceOperationRejectionReason.None,
      PreviousTask: currentTask,
      CurrentTask: currentTask);
  }

  private static NpcTaskReferenceOperationResult AcceptedOperation(
    NpcTaskLifecycleResult previousTask,
    NpcTaskLifecycleResult currentTask)
  {
    return new NpcTaskReferenceOperationResult(
      Accepted: true,
      Changed: true,
      RejectionReason: NpcTaskReferenceOperationRejectionReason.None,
      PreviousTask: previousTask,
      CurrentTask: currentTask);
  }

  private static NpcTaskReferenceOperationResult RejectedOperation(
    NpcTaskReferenceOperationRejectionReason rejectionReason,
    NpcTaskLifecycleResult currentTask)
  {
    return new NpcTaskReferenceOperationResult(
      Accepted: false,
      Changed: false,
      RejectionReason: rejectionReason,
      PreviousTask: currentTask,
      CurrentTask: currentTask);
  }

  private static NpcTaskTerminationResult Rejected(
    NpcTaskTerminationRejectionReason rejectionReason,
    NpcTaskEndReason endReason,
    NpcTaskLifecycleResult currentTask)
  {
    return new NpcTaskTerminationResult(
      Outcome: NpcTaskTerminationOutcome.Rejected,
      RejectionReason: rejectionReason,
      RequestedEndReason: endReason,
      EndReason: currentTask.EndReason,
      PreviousTask: currentTask,
      CurrentTask: currentTask);
  }
}
