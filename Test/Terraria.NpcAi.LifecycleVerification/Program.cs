using System.Numerics;
using EntityEcs;
using EntityEcs.Components;
using Terraria.Npc;
using Terraria.Relationships;

VerifyTaskEndReasonsAndIdempotency();
VerifyCompletedAndFailedTasksAreClearedWithoutReclassification();
VerifyOldTaskAndEntityReferencesAreRejected();
VerifyReferenceBoundTaskOperations();
VerifyReferenceInvalidationBoundaries();
VerifyTerminationOwnersClearOnlyTheirOwnedTransientState();
VerifyRemovalAndSessionUnloadDoNotResolveOldRelations();

Console.WriteLine("PASS: NPC task termination lifecycle verification");

static void VerifyTaskEndReasonsAndIdempotency()
{
  foreach (NpcTaskEndReason reason in new[]
           {
             NpcTaskEndReason.Death,
             NpcTaskEndReason.Transform,
             NpcTaskEndReason.Removal,
             NpcTaskEndReason.WorldUnload,
           })
  {
    using var runtime = new EntityRuntime();
    RuntimeEntityHandle owner = CreateEntity(runtime);
    var state = new NpcTaskStateComponent();
    Require(runtime.TryAttach(owner, state), "Task state must attach to its entity.");

    NpcTaskLifecycleResult entered = NpcTaskLifecycleSystem.Enter(
      state,
      NpcTaskKind.GuideReturnHome,
      initialCursor: 17);
    NpcTaskLifecycleResult advanced = NpcTaskLifecycleSystem.Advance(
      state,
      NpcTaskKind.GuideReturnHome);
    Require(entered.Accepted && advanced.Cursor == 18,
      "The fixture task must have per-instance progress before termination.");
    Require(TryCaptureReference(runtime, owner, out NpcTaskReference reference),
      "A live owner must produce a task reference.");
    NpcTaskTerminationResult invalidReason = TerminateAttached(
      runtime,
      owner,
      reference,
      (NpcTaskEndReason)byte.MaxValue);
    Require(!invalidReason.Accepted && !invalidReason.Changed &&
        invalidReason.RejectionReason == NpcTaskTerminationRejectionReason.InvalidEndReason &&
        state.IsRunning,
      "An invalid termination reason must leave the running task unchanged.");

    NpcTaskTerminationResult first = TerminateAttached(
      runtime,
      owner,
      reference,
      reason);
    Require(first.Accepted && first.Changed &&
        first.PreviousTask.Phase == NpcTaskPhase.Running &&
        first.PreviousTask.Kind == NpcTaskKind.GuideReturnHome &&
        first.PreviousTask.Cursor == 18 &&
        first.CurrentTask.Phase == NpcTaskPhase.Idle &&
        first.CurrentTask.Kind == NpcTaskKind.None &&
        first.CurrentTask.Cursor == 0 &&
        first.CurrentTask.FailureReason == NpcTaskFailureReason.None &&
        first.CurrentTask.EndReason == reason,
      $"{reason} must cancel the running task and clear its active state.");

    NpcTaskTerminationResult repeated = TerminateAttached(
      runtime,
      owner,
      reference,
      reason);
    Require(repeated.Outcome == NpcTaskTerminationOutcome.AlreadyEnded &&
        repeated.Accepted && !repeated.Changed &&
        SameTaskState(repeated.CurrentTask, first.CurrentTask),
      $"Repeated {reason} termination must be idempotent.");

    NpcTaskTerminationResult conflicting = TerminateAttached(
      runtime,
      owner,
      reference,
      DifferentReason(reason));
    Require(conflicting.Outcome == NpcTaskTerminationOutcome.EarlierEndReasonPreserved &&
        conflicting.Accepted && !conflicting.Changed &&
        conflicting.EndReason == reason &&
        conflicting.RequestedEndReason == DifferentReason(reason) &&
        SameTaskState(conflicting.CurrentTask, first.CurrentTask),
      "A later lifecycle event must preserve the first reason as an accepted no-op.");
  }
}

static void VerifyCompletedAndFailedTasksAreClearedWithoutReclassification()
{
  VerifyTerminalTask(NpcTaskPhase.Completed, NpcTaskFailureReason.None);
  VerifyTerminalTask(NpcTaskPhase.Failed, NpcTaskFailureReason.NoPath);
}

static void VerifyOldTaskAndEntityReferencesAreRejected()
{
  using var runtime = new EntityRuntime();
  RuntimeEntityHandle originalOwner = CreateEntity(runtime);
  var originalState = new NpcTaskStateComponent();
  Require(runtime.TryAttach(originalOwner, originalState), "Original task state must attach.");
  _ = NpcTaskLifecycleSystem.Enter(originalState, NpcTaskKind.GuideDayPatrol);
  Require(TryCaptureReference(
      runtime,
      originalOwner,
      out NpcTaskReference oldReference),
    "Original task reference must be captured.");

  _ = TerminateAttached(
    runtime,
    originalOwner,
    oldReference,
    NpcTaskEndReason.Transform);
  NpcTaskLifecycleResult restarted = NpcTaskLifecycleSystem.Enter(
    originalState,
    NpcTaskKind.GuideDayPatrol);
  NpcTaskTerminationResult staleTask = TerminateAttached(
    runtime,
    originalOwner,
    oldReference,
    NpcTaskEndReason.Removal);
  Require(restarted.TaskGeneration > oldReference.TaskGeneration &&
      !staleTask.Accepted &&
      staleTask.RejectionReason == NpcTaskTerminationRejectionReason.StaleTaskReference &&
      staleTask.CurrentTask.Phase == NpcTaskPhase.Running,
    "An earlier run of the same task must not terminate its replacement.");

  NpcTaskReference resetReference = CaptureReference(runtime, originalOwner);
  NpcTaskLifecycleResult reset = default;
  Require(runtime.TryEdit<NpcTaskStateComponent>(
      originalOwner,
      (ref NpcTaskStateComponent state) => reset = NpcTaskLifecycleSystem.Reset(state)) &&
      reset.Phase == NpcTaskPhase.Idle,
    "Reset must clear task data and invalidate the current task context.");
  NpcTaskTerminationResult staleAfterReset = TerminateAttached(
    runtime,
    originalOwner,
    resetReference,
    NpcTaskEndReason.Removal);
  Require(!staleAfterReset.Accepted &&
      staleAfterReset.RejectionReason == NpcTaskTerminationRejectionReason.StaleTaskReference,
    "Reset must reject a task reference captured before reset.");

  Require(runtime.TryBeginTermination(originalOwner) && runtime.TryRemoveEntity(originalOwner),
    "The old entity must be removed before its slot is reused.");
  RuntimeEntityHandle replacementOwner = CreateEntity(runtime);
  var replacementState = new NpcTaskStateComponent();
  Require(replacementOwner.LocalIndex == originalOwner.LocalIndex &&
      replacementOwner.Generation != originalOwner.Generation &&
      runtime.TryAttach(replacementOwner, replacementState),
    "The fixture must reuse the local slot with a fresh generation.");
  _ = NpcTaskLifecycleSystem.Enter(replacementState, NpcTaskKind.GuideDayPatrol);
  NpcTaskTerminationResult staleEntity = TerminateAttached(
    runtime,
    replacementOwner,
    oldReference,
    NpcTaskEndReason.Death);
  Require(!staleEntity.Accepted &&
      staleEntity.RejectionReason == NpcTaskTerminationRejectionReason.StaleEntityReference &&
      replacementState.IsRunning,
    "A task reference from a released slot must not affect its replacement.");

  NpcTaskReferenceOperationResult staleAdvance = AdvanceAttached(
    runtime,
    replacementOwner,
    oldReference,
    NpcTaskKind.GuideDayPatrol);
  NpcTaskReferenceOperationResult staleComplete = CompleteAttached(
    runtime,
    replacementOwner,
    oldReference,
    NpcTaskKind.GuideDayPatrol);
  NpcTaskReferenceOperationResult staleFail = FailAttached(
    runtime,
    replacementOwner,
    oldReference,
    NpcTaskKind.GuideDayPatrol,
    NpcTaskFailureReason.NoPath);
  NpcTaskReferenceOperationResult staleInterrupt = InterruptAttached(
    runtime,
    replacementOwner,
    oldReference,
    NpcTaskKind.GuideDayPatrol,
    NpcTaskFailureReason.Aborted);
  Require(staleAdvance.RejectionReason ==
      NpcTaskReferenceOperationRejectionReason.StaleEntityReference &&
      staleComplete.RejectionReason ==
      NpcTaskReferenceOperationRejectionReason.StaleEntityReference &&
      staleFail.RejectionReason ==
      NpcTaskReferenceOperationRejectionReason.StaleEntityReference &&
      staleInterrupt.RejectionReason ==
      NpcTaskReferenceOperationRejectionReason.StaleEntityReference &&
      replacementState.IsRunning,
    "Every reference-bound operation must reject a reference from an old entity.");

  using var nextSession = new EntityRuntime();
  RuntimeEntityHandle nextSessionOwner = CreateEntity(nextSession);
  var nextSessionState = new NpcTaskStateComponent();
  Require(nextSession.TryAttach(nextSessionOwner, nextSessionState),
    "New-session task state must attach.");
  _ = NpcTaskLifecycleSystem.Enter(nextSessionState, NpcTaskKind.GuideDayPatrol);
  NpcTaskTerminationResult staleSession = TerminateAttached(
    nextSession,
    nextSessionOwner,
    oldReference,
    NpcTaskEndReason.WorldUnload);
  Require(nextSessionOwner.RuntimeId != originalOwner.RuntimeId &&
      !staleSession.Accepted &&
      staleSession.RejectionReason == NpcTaskTerminationRejectionReason.StaleEntityReference &&
      nextSessionState.IsRunning,
      "A reference from a previous session must not affect a new session.");
}

static void VerifyReferenceBoundTaskOperations()
{
  using var runtime = new EntityRuntime();
  RuntimeEntityHandle owner = CreateEntity(runtime);
  var state = new NpcTaskStateComponent();
  Require(runtime.TryAttach(owner, state), "Reference operation state must attach.");

  NpcTaskKind kind = NpcTaskKind.GuideDayPatrol;
  _ = NpcTaskLifecycleSystem.Enter(state, kind, initialCursor: 3);
  NpcTaskReference oldReference = CaptureReference(runtime, owner);
  NpcTaskReferenceOperationResult invalidReference = AdvanceAttached(
    runtime,
    owner,
    default,
    kind);
  Require(!invalidReference.Accepted &&
      invalidReference.RejectionReason ==
      NpcTaskReferenceOperationRejectionReason.InvalidReference &&
      state.IsRunning && state.Cursor == 3,
    "An unassigned reference must be rejected without changing the running task.");
  NpcTaskReferenceOperationResult invalidKindReference = AdvanceAttached(
    runtime,
    owner,
    oldReference,
    (NpcTaskKind)byte.MaxValue);
  NpcTaskLifecycleResult invalidKindEnter = NpcTaskLifecycleSystem.Enter(
    state,
    (NpcTaskKind)byte.MaxValue);
  Require(!invalidKindReference.Accepted &&
      invalidKindReference.RejectionReason ==
      NpcTaskReferenceOperationRejectionReason.InvalidTaskKind &&
      !invalidKindEnter.Accepted && state.IsRunning && state.Cursor == 3,
    "An undefined task kind must be rejected without creating or changing a runtime task.");

  NpcTaskReferenceOperationResult advanced = AdvanceAttached(
    runtime,
    owner,
    oldReference,
    kind);
  Require(advanced.Accepted && advanced.Changed &&
      advanced.CurrentTask.Cursor == 4 &&
      advanced.CurrentTask.Phase == NpcTaskPhase.Running,
    "A current reference must advance its running task.");

  NpcTaskReferenceOperationResult completed = CompleteAttached(
    runtime,
    owner,
    oldReference,
    kind);
  Require(completed.Accepted && completed.Changed &&
      completed.CurrentTask.Phase == NpcTaskPhase.Completed,
    "A current reference must complete its running task.");
  NpcTaskReferenceOperationResult repeatedComplete = CompleteAttached(
    runtime,
    owner,
    oldReference,
    kind);
  NpcTaskReferenceOperationResult repeatedFailAfterComplete = FailAttached(
    runtime,
    owner,
    oldReference,
    kind,
    NpcTaskFailureReason.NoPath);
  Require(!repeatedComplete.Accepted && !repeatedComplete.Changed &&
      repeatedComplete.RejectionReason ==
      NpcTaskReferenceOperationRejectionReason.NotRunning &&
      !repeatedFailAfterComplete.Accepted &&
      repeatedFailAfterComplete.RejectionReason ==
      NpcTaskReferenceOperationRejectionReason.NotRunning &&
      SameTaskState(repeatedComplete.PreviousTask, repeatedComplete.CurrentTask) &&
      SameTaskState(repeatedFailAfterComplete.PreviousTask,
        repeatedFailAfterComplete.CurrentTask),
    "Repeated complete/fail operations must be no-op results after completion.");

  _ = NpcTaskLifecycleSystem.Enter(state, kind, initialCursor: 10);
  NpcTaskReference restartedReference = CaptureReference(runtime, owner);
  NpcTaskReferenceOperationResult staleAdvance = AdvanceAttached(
    runtime,
    owner,
    oldReference,
    kind);
  NpcTaskReferenceOperationResult staleComplete = CompleteAttached(
    runtime,
    owner,
    oldReference,
    kind);
  NpcTaskReferenceOperationResult staleFail = FailAttached(
    runtime,
    owner,
    oldReference,
    kind,
    NpcTaskFailureReason.NoPath);
  NpcTaskReferenceOperationResult staleInterrupt = InterruptAttached(
    runtime,
    owner,
    oldReference,
    kind,
    NpcTaskFailureReason.Aborted);
  Require(restartedReference.TaskGeneration > oldReference.TaskGeneration &&
      staleAdvance.RejectionReason ==
      NpcTaskReferenceOperationRejectionReason.StaleTaskReference &&
      staleComplete.RejectionReason ==
      NpcTaskReferenceOperationRejectionReason.StaleTaskReference &&
      staleFail.RejectionReason ==
      NpcTaskReferenceOperationRejectionReason.StaleTaskReference &&
      staleInterrupt.RejectionReason ==
      NpcTaskReferenceOperationRejectionReason.StaleTaskReference &&
      state.IsRunning && state.Cursor == 10,
    "Every reference-bound operation must reject an old same-kind task reference.");

  NpcTaskReferenceOperationResult currentAdvance = AdvanceAttached(
    runtime,
    owner,
    restartedReference,
    kind);
  Require(currentAdvance.Accepted && currentAdvance.CurrentTask.Cursor == 11,
    "A new reference must advance the restarted task.");

  NpcTaskReferenceOperationResult mismatchedKind = CompleteAttached(
    runtime,
    owner,
    restartedReference,
    NpcTaskKind.GuideReturnHome);
  Require(!mismatchedKind.Accepted &&
      mismatchedKind.RejectionReason ==
      NpcTaskReferenceOperationRejectionReason.TaskKindMismatch &&
      state.IsRunning && state.Cursor == 11,
    "A reference operation must reject a mismatched task kind without mutation.");

  NpcTaskReferenceOperationResult invalidFailure = FailAttached(
    runtime,
    owner,
    restartedReference,
    kind,
    NpcTaskFailureReason.None);
  Require(!invalidFailure.Accepted &&
      invalidFailure.RejectionReason ==
      NpcTaskReferenceOperationRejectionReason.InvalidFailureReason &&
      state.IsRunning && state.Cursor == 11 &&
      state.FailureReason == NpcTaskFailureReason.None,
    "An invalid failure reason must not modify the running task.");

  NpcTaskReferenceOperationResult invalidFailureValue = FailAttached(
    runtime,
    owner,
    restartedReference,
    kind,
    (NpcTaskFailureReason)byte.MaxValue);
  Require(!invalidFailureValue.Accepted &&
      invalidFailureValue.RejectionReason ==
      NpcTaskReferenceOperationRejectionReason.InvalidFailureReason &&
      state.IsRunning && state.Cursor == 11 &&
      state.FailureReason == NpcTaskFailureReason.None,
    "An undefined failure reason must not modify the running task.");

  NpcTaskReferenceOperationResult failed = FailAttached(
    runtime,
    owner,
    restartedReference,
    kind,
    NpcTaskFailureReason.NoPath);
  Require(failed.Accepted && failed.Changed &&
      failed.CurrentTask.Phase == NpcTaskPhase.Failed &&
      failed.CurrentTask.FailureReason == NpcTaskFailureReason.NoPath,
    "A valid reference must fail its running task once.");
  NpcTaskReferenceOperationResult repeatedFail = FailAttached(
    runtime,
    owner,
    restartedReference,
    kind,
    NpcTaskFailureReason.NoPath);
  NpcTaskReferenceOperationResult interruptAfterFailure = InterruptAttached(
    runtime,
    owner,
    restartedReference,
    kind,
    NpcTaskFailureReason.Aborted);
  Require(!repeatedFail.Accepted && !repeatedFail.Changed &&
      repeatedFail.RejectionReason ==
      NpcTaskReferenceOperationRejectionReason.NotRunning &&
      !interruptAfterFailure.Accepted &&
      interruptAfterFailure.RejectionReason ==
      NpcTaskReferenceOperationRejectionReason.NotRunning &&
      SameTaskState(repeatedFail.PreviousTask, repeatedFail.CurrentTask) &&
      state.Phase == NpcTaskPhase.Failed &&
      state.FailureReason == NpcTaskFailureReason.NoPath,
    "Repeated fail/interrupt operations must not replay effects or change failure state.");

  _ = NpcTaskLifecycleSystem.Enter(state, kind);
  NpcTaskReference interruptReference = CaptureReference(runtime, owner);
  NpcTaskReferenceOperationResult interrupted = InterruptAttached(
    runtime,
    owner,
    interruptReference,
    kind,
    NpcTaskFailureReason.Aborted);
  NpcTaskReferenceOperationResult repeatedInterrupt = InterruptAttached(
    runtime,
    owner,
    interruptReference,
    kind,
    NpcTaskFailureReason.Aborted);
  Require(interrupted.Accepted && interrupted.Changed &&
      interrupted.CurrentTask.Phase == NpcTaskPhase.Interrupted &&
      repeatedInterrupt.RejectionReason ==
      NpcTaskReferenceOperationRejectionReason.NotRunning,
    "Interrupt must accept once and reject a repeated operation as non-running.");
}

static void VerifyReferenceInvalidationBoundaries()
{
  foreach (NpcTaskEndReason reason in new[]
           {
             NpcTaskEndReason.Death,
             NpcTaskEndReason.Transform,
             NpcTaskEndReason.Removal,
             NpcTaskEndReason.WorldUnload,
           })
  {
    using var runtime = new EntityRuntime();
    RuntimeEntityHandle owner = CreateEntity(runtime);
    var state = new NpcTaskStateComponent();
    Require(runtime.TryAttach(owner, state), "Boundary task state must attach.");
    NpcTaskKind kind = NpcTaskKind.GuideReturnHome;
    _ = NpcTaskLifecycleSystem.Enter(state, kind);
    NpcTaskReference reference = CaptureReference(runtime, owner);
    NpcTaskTerminationResult termination = TerminateAttached(
      runtime,
      owner,
      reference,
      reason);
    Require(termination.Accepted && termination.Changed,
      $"{reason} must terminate the running task before reference invalidation checks.");

    NpcTaskReferenceOperationResult advance = AdvanceAttached(
      runtime,
      owner,
      reference,
      kind);
    NpcTaskReferenceOperationResult complete = CompleteAttached(
      runtime,
      owner,
      reference,
      kind);
    NpcTaskReferenceOperationResult fail = FailAttached(
      runtime,
      owner,
      reference,
      kind,
      NpcTaskFailureReason.NoPath);
    NpcTaskReferenceOperationResult interrupt = InterruptAttached(
      runtime,
      owner,
      reference,
      kind,
      NpcTaskFailureReason.Aborted);
    Require(advance.RejectionReason ==
        NpcTaskReferenceOperationRejectionReason.NotRunning &&
        complete.RejectionReason ==
        NpcTaskReferenceOperationRejectionReason.NotRunning &&
        fail.RejectionReason ==
        NpcTaskReferenceOperationRejectionReason.NotRunning &&
        interrupt.RejectionReason ==
        NpcTaskReferenceOperationRejectionReason.NotRunning &&
        state.Phase == NpcTaskPhase.Idle &&
        state.Kind == NpcTaskKind.None,
      $"{reason} must invalidate every reference-bound task operation.");
  }

  using var resetRuntime = new EntityRuntime();
  RuntimeEntityHandle resetOwner = CreateEntity(resetRuntime);
  var resetState = new NpcTaskStateComponent();
  Require(resetRuntime.TryAttach(resetOwner, resetState),
    "Reset boundary task state must attach.");
  NpcTaskKind resetKind = NpcTaskKind.GuideDayPatrol;
  _ = NpcTaskLifecycleSystem.Enter(resetState, resetKind);
  NpcTaskReference resetReference = CaptureReference(resetRuntime, resetOwner);
  NpcTaskLifecycleResult reset = default;
  Require(resetRuntime.TryEdit<NpcTaskStateComponent>(
      resetOwner,
      (ref NpcTaskStateComponent state) => reset = NpcTaskLifecycleSystem.Reset(state)) &&
      reset.Phase == NpcTaskPhase.Idle,
    "Reset must invalidate the captured task reference.");
  Require(AdvanceAttached(resetRuntime, resetOwner, resetReference, resetKind)
      .RejectionReason == NpcTaskReferenceOperationRejectionReason.StaleTaskReference &&
      CompleteAttached(resetRuntime, resetOwner, resetReference, resetKind)
      .RejectionReason == NpcTaskReferenceOperationRejectionReason.StaleTaskReference &&
      FailAttached(resetRuntime, resetOwner, resetReference, resetKind,
        NpcTaskFailureReason.NoPath).RejectionReason ==
      NpcTaskReferenceOperationRejectionReason.StaleTaskReference &&
      InterruptAttached(resetRuntime, resetOwner, resetReference, resetKind,
        NpcTaskFailureReason.Aborted).RejectionReason ==
      NpcTaskReferenceOperationRejectionReason.StaleTaskReference,
    "Reset must reject every operation using the pre-reset reference.");
}

static void VerifyTerminationOwnersClearOnlyTheirOwnedTransientState()
{
  using var runtime = new EntityRuntime();
  RuntimeEntityHandle owner = CreateEntity(runtime);
  RuntimeEntityHandle targetEntity = CreateEntity(runtime);
  Require(runtime.TryGetReference(
      targetEntity,
      EntityReferenceScope.Npc,
      out EntityReference targetReference),
    "A target reference must be available for the transform fixture.");
  var taskState = new NpcTaskStateComponent();
  var targetState = new NpcTargetSelectionStateComponent();
  var effects = new NpcImmediateEffectStateComponent();
  var lifecycle = new NpcLifecycleComponent(
    isActive: true,
    remainingActiveTicks: 0,
    NpcLifecycleStage.Active);
  Require(runtime.TryAttach(owner, taskState) &&
      runtime.TryAttach(owner, targetState) &&
      runtime.TryAttach(owner, effects) &&
      runtime.TryAttach(owner, new NpcTargetComponent(
        NpcTargetKind.Npc,
        targetReference,
        legacyTargetIndex: 4)) &&
      runtime.TryAttach(owner, lifecycle),
    "Owner-specific lifecycle components must attach.");

  NpcTargetSelectionInputs selectedInputs = CreateTargetInputs(
    new NpcPlayerTargetSnapshot(
      Slot: 4,
      Geometry: new NpcTargetGeometrySnapshot(new Vector2(20f, 4f), 20, 20),
      IsActive: true,
      IsDead: false,
      IsGhost: false,
      Aggro: 0,
      NoAggro: false,
      Gross: true,
      ItemAnimation: 0,
      TankPet: null));
  _ = NpcTargetSelectionSystem.SelectAndCommit(in selectedInputs, targetState);
  NpcTargetSelectionStateSnapshot beforeQuery = CaptureTargetState(targetState);
  NpcTargetSelectionInputs queryInputs = CreateTargetInputs(
    new NpcPlayerTargetSnapshot(
      Slot: 9,
      Geometry: new NpcTargetGeometrySnapshot(new Vector2(-2f, 8f), 20, 20),
      IsActive: true,
      IsDead: false,
      IsGhost: false,
      Aggro: 0,
      NoAggro: false,
      Gross: true,
      ItemAnimation: 0,
      TankPet: null));
  _ = NpcTargetSelectionSystem.Select(in queryInputs);
  Require(CaptureTargetState(targetState) == beforeQuery,
    "The target query must not mutate the committed target cache.");

  effects.CommitHomeTeleport(new Vector2(600f, 80f), candidateOffset: 1);
  effects.CommitDust(new Vector2(10f, 12f), Vector2.UnitY);
  effects.CommitJump(-5f);
  effects.CommitDespawnEncouragement(10);
  effects.CommitDoorOpen(tileX: 8, tileY: 11, direction: -1);
  effects.CommitNetworkUpdate();
  effects.CommitHousingRevalidationFailure();
  effects.CommitHousingRegistrySynchronization();
  NpcTaskLifecycleResult entered = NpcTaskLifecycleSystem.Enter(
    taskState,
    NpcTaskKind.GuideReturnHome);
  Require(TryCaptureReference(runtime, owner, out NpcTaskReference reference),
    "Task owner reference must be captured before termination.");
  NpcTaskTerminationResult termination = TerminateAttached(
    runtime,
    owner,
    reference,
    NpcTaskEndReason.Transform);
  Require(termination.Accepted && lifecycle.IsActive &&
      lifecycle.Stage == NpcLifecycleStage.Active &&
      effects.HomeTeleportRequested && effects.DustCount == 1 &&
      effects.JumpRequested && effects.DoorOpenRequested &&
      entered.TaskGeneration == reference.TaskGeneration,
    "Task termination must not write lifecycle/effect state or replay pending effects.");

  bool targetReset = false;
  bool resetTargetCache = runtime.TryEdit<NpcTargetSelectionStateComponent>(
    owner,
    (ref NpcTargetSelectionStateComponent state) =>
      targetReset = NpcTargetSelectionSystem.ResetForTermination(state));
  bool discardEffects = runtime.TryEdit<NpcImmediateEffectStateComponent>(
    owner,
    (ref NpcImmediateEffectStateComponent state) => state.DiscardPendingEffects());
  bool targetComponentReplaced = runtime.TryReplace(owner, new NpcTargetComponent());
  bool targetComponentHasNoTarget = runtime.TryCapture<NpcTargetComponent, bool>(
    owner,
    static target => !target.HasTarget && target.TargetKind == NpcTargetKind.None,
    out bool hasNoTarget) && hasNoTarget;
  Require(resetTargetCache && targetReset && discardEffects &&
      targetComponentReplaced && targetComponentHasNoTarget &&
      targetState.TargetKind == NpcTargetKind.None &&
      targetState.LegacyTargetIndex == -1 &&
      float.IsPositiveInfinity(targetState.Score) &&
      !targetState.NetUpdateRequested &&
      effects.DespawnEncouragementTicks == 0 &&
      effects.DustCount == 0 &&
      !effects.NetworkUpdateRequested &&
      !effects.JumpRequested &&
      effects.JumpVelocityY == 0f &&
      !effects.DoorOpenRequested &&
      effects.DoorTileX == 0 && effects.DoorTileY == 0 && effects.DoorDirection == 0 &&
      !effects.HomeTeleportRequested &&
      !effects.HomeTeleportSucceeded && !effects.HomeTeleportFailed &&
      effects.HomeTeleportCandidateOffset == 0 &&
      effects.HomeTeleportFailureReason == NpcTaskFailureReason.None &&
      effects.HomeTeleportPosition == Vector2.Zero &&
      !effects.HousingRevalidationFailed &&
      !effects.HousingRegistrySynchronized &&
      effects.LastDustPosition == Vector2.Zero &&
      effects.LastDustVelocity == Vector2.Zero,
    "Target owner and effect owner must discard their own stale transient values.");
}

static void VerifyRemovalAndSessionUnloadDoNotResolveOldRelations()
{
  var runtime = new EntityRuntime();
  RuntimeEntityHandle owner = CreateEntity(runtime);
  RuntimeEntityHandle child = CreateEntity(runtime);
  Require(runtime.TryGetReference(owner, EntityReferenceScope.Npc, out EntityReference oldOwnerReference),
    "A published NPC reference must be available for the relation fixture.");
  Require(runtime.TryAttach(child, new EntityRelationState(
      oldOwnerReference,
      EntityRelationKind.Parent,
      attachedAtTick: 1)),
    "The child relation must attach.");

  var taskState = new NpcTaskStateComponent();
  var targetState = new NpcTargetSelectionStateComponent();
  var effects = new NpcImmediateEffectStateComponent();
  Require(runtime.TryAttach(owner, taskState) &&
      runtime.TryAttach(owner, targetState) &&
      runtime.TryAttach(owner, effects) &&
      runtime.TryAttach(owner, new NpcTargetComponent(
        NpcTargetKind.Npc,
        oldOwnerReference,
        legacyTargetIndex: 3)),
    "Transient owner state must attach before removal.");
  NpcTargetSelectionInputs cachedTargetInputs = CreateTargetInputs(
    new NpcPlayerTargetSnapshot(
      Slot: 3,
      Geometry: new NpcTargetGeometrySnapshot(new Vector2(32f, 16f), 20, 20),
      IsActive: true,
      IsDead: false,
      IsGhost: false,
      Aggro: 0,
      NoAggro: false,
      Gross: true,
      ItemAnimation: 0,
      TankPet: null));
  _ = NpcTargetSelectionSystem.SelectAndCommit(in cachedTargetInputs, targetState);
  _ = NpcTaskLifecycleSystem.Enter(taskState, NpcTaskKind.GuideDayPatrol);
  Require(TryCaptureReference(runtime, owner, out NpcTaskReference reference),
    "Removal task reference must be captured.");
  NpcTaskTerminationResult termination = TerminateAttached(
    runtime,
    owner,
    reference,
    NpcTaskEndReason.Transform);
  Require(termination.Accepted, "Removal must end its task before entity termination.");

  NpcTaskTerminationResult unload = TerminateAttached(
    runtime,
    owner,
    reference,
    NpcTaskEndReason.WorldUnload);
  Require(unload.Outcome == NpcTaskTerminationOutcome.EarlierEndReasonPreserved &&
      unload.Accepted && !unload.Changed &&
      unload.EndReason == NpcTaskEndReason.Transform,
    "Transform followed by unload must preserve the first reason and allow cleanup to proceed.");

  bool targetReset = false;
  Require(runtime.TryEdit<NpcTargetSelectionStateComponent>(
      owner,
      (ref NpcTargetSelectionStateComponent state) =>
        targetReset = NpcTargetSelectionSystem.ResetForTermination(state)) &&
      targetReset,
    "The target owner must clear its cache before release.");
  Require(runtime.TryEdit<NpcImmediateEffectStateComponent>(
      owner,
      (ref NpcImmediateEffectStateComponent state) => state.DiscardPendingEffects()),
    "The effect owner must discard uncommitted intents before release.");
  Require(runtime.TryBeginTermination(owner) && runtime.TryRemoveEntity(owner),
    "The owner must be removed through the runtime lifecycle.");
  Require(!runtime.TryResolve(oldOwnerReference, out _) &&
      !runtime.TryCapture<NpcTaskStateComponent, int>(owner, static _ => 1, out _) &&
      !runtime.TryCapture<NpcTargetComponent, int>(owner, static _ => 1, out _) &&
      !runtime.TryCapture<NpcImmediateEffectStateComponent, int>(owner, static _ => 1, out _),
    "Removed task, target, effect, and relation owners must no longer resolve.");
  Require(runtime.TryCapture<EntityRelationState, bool>(
      child,
      relation => relation.RelatedEntity == oldOwnerReference,
      out bool childStillHasOldReference) && childStillHasOldReference,
    "The fixture must retain the stale reference value to prove that it cannot resolve.");

  runtime.Dispose();
  using var nextSession = new EntityRuntime();
  RuntimeEntityHandle replacement = CreateEntity(nextSession);
  Require(replacement.RuntimeId != owner.RuntimeId &&
      replacement.LocalIndex == owner.LocalIndex &&
      !nextSession.TryResolve(oldOwnerReference, out _) &&
      !nextSession.TryCapture<NpcTaskStateComponent, int>(owner, static _ => 1, out _),
    "A new runtime session must reject old references even when the local slot matches.");
}

static void VerifyTerminalTask(NpcTaskPhase terminalPhase, NpcTaskFailureReason failureReason)
{
  using var runtime = new EntityRuntime();
  RuntimeEntityHandle owner = CreateEntity(runtime);
  var state = new NpcTaskStateComponent();
  Require(runtime.TryAttach(owner, state), "Terminal task state must attach.");
  _ = NpcTaskLifecycleSystem.Enter(state, NpcTaskKind.GuideReturnHome, initialCursor: 6);
  if (terminalPhase == NpcTaskPhase.Completed)
  {
    _ = NpcTaskLifecycleSystem.Complete(state, NpcTaskKind.GuideReturnHome);
  }
  else
  {
    _ = NpcTaskLifecycleSystem.Fail(state, NpcTaskKind.GuideReturnHome, failureReason);
  }

  NpcTaskReference reference = CaptureReference(runtime, owner);
  NpcTaskTerminationResult ended = TerminateAttached(
    runtime,
    owner,
    reference,
    NpcTaskEndReason.Removal);
  Require(ended.Outcome == NpcTaskTerminationOutcome.Ended &&
      ended.PreviousTask.Phase == terminalPhase &&
      ended.PreviousTask.FailureReason == failureReason &&
      ended.PreviousTask.Cursor == 6 &&
      ended.CurrentTask.Phase == NpcTaskPhase.Idle &&
      ended.CurrentTask.Kind == NpcTaskKind.None &&
      ended.CurrentTask.Cursor == 0 &&
      ended.CurrentTask.FailureReason == NpcTaskFailureReason.None,
    $"A {terminalPhase} task must be cleared without being reclassified.");
  NpcTaskTerminationResult repeated = TerminateAttached(
    runtime,
    owner,
    reference,
    NpcTaskEndReason.Removal);
  Require(repeated.Outcome == NpcTaskTerminationOutcome.AlreadyEnded &&
      !repeated.Changed &&
      SameTaskState(repeated.CurrentTask, ended.CurrentTask),
    $"A {terminalPhase} task termination must remain idempotent.");
}

static NpcTaskReference CaptureReference(EntityRuntime runtime, RuntimeEntityHandle owner)
{
  Require(TryCaptureReference(runtime, owner, out NpcTaskReference reference),
    "Task reference must be captured from the attached state.");
  return reference;
}

static bool SameTaskState(
  NpcTaskLifecycleResult first,
  NpcTaskLifecycleResult second)
{
  return first.Kind == second.Kind &&
    first.Phase == second.Phase &&
    first.Cursor == second.Cursor &&
    first.FailureReason == second.FailureReason &&
    first.TaskGeneration == second.TaskGeneration &&
    first.EndReason == second.EndReason;
}

static bool TryCaptureReference(
  EntityRuntime runtime,
  RuntimeEntityHandle owner,
  out NpcTaskReference reference)
{
  NpcTaskReference captured = default;
  bool taskCaptured = false;
  bool inspected = runtime.TryInspect(
    owner,
    (in NpcTaskStateComponent state) =>
      taskCaptured = NpcTaskLifecycleSystem.TryCaptureReference(state, owner, out captured));
  reference = captured;
  return inspected && taskCaptured;
}

static NpcTaskReferenceOperationResult AdvanceAttached(
  EntityRuntime runtime,
  RuntimeEntityHandle currentOwner,
  NpcTaskReference reference,
  NpcTaskKind kind)
{
  NpcTaskReferenceOperationResult result = default;
  bool edited = runtime.TryEdit(
    currentOwner,
    (ref NpcTaskStateComponent state) =>
      result = NpcTaskLifecycleSystem.Advance(state, currentOwner, reference, kind));
  Require(edited, "EntityRuntime must validate the attached task state before advancing.");
  return result;
}

static NpcTaskReferenceOperationResult CompleteAttached(
  EntityRuntime runtime,
  RuntimeEntityHandle currentOwner,
  NpcTaskReference reference,
  NpcTaskKind kind)
{
  NpcTaskReferenceOperationResult result = default;
  bool edited = runtime.TryEdit(
    currentOwner,
    (ref NpcTaskStateComponent state) =>
      result = NpcTaskLifecycleSystem.Complete(state, currentOwner, reference, kind));
  Require(edited, "EntityRuntime must validate the attached task state before completing.");
  return result;
}

static NpcTaskReferenceOperationResult FailAttached(
  EntityRuntime runtime,
  RuntimeEntityHandle currentOwner,
  NpcTaskReference reference,
  NpcTaskKind kind,
  NpcTaskFailureReason reason)
{
  NpcTaskReferenceOperationResult result = default;
  bool edited = runtime.TryEdit(
    currentOwner,
    (ref NpcTaskStateComponent state) =>
      result = NpcTaskLifecycleSystem.Fail(
        state,
        currentOwner,
        reference,
        kind,
        reason));
  Require(edited, "EntityRuntime must validate the attached task state before failing.");
  return result;
}

static NpcTaskReferenceOperationResult InterruptAttached(
  EntityRuntime runtime,
  RuntimeEntityHandle currentOwner,
  NpcTaskReference reference,
  NpcTaskKind kind,
  NpcTaskFailureReason reason)
{
  NpcTaskReferenceOperationResult result = default;
  bool edited = runtime.TryEdit(
    currentOwner,
    (ref NpcTaskStateComponent state) =>
      result = NpcTaskLifecycleSystem.Interrupt(
        state,
        currentOwner,
        reference,
        kind,
        reason));
  Require(edited, "EntityRuntime must validate the attached task state before interrupting.");
  return result;
}

static NpcTaskTerminationResult TerminateAttached(
  EntityRuntime runtime,
  RuntimeEntityHandle currentOwner,
  NpcTaskReference reference,
  NpcTaskEndReason reason)
{
  NpcTaskTerminationResult result = default;
  bool edited = runtime.TryEdit(
    currentOwner,
    (ref NpcTaskStateComponent state) =>
      result = NpcTaskLifecycleSystem.Terminate(state, currentOwner, reference, reason));
  Require(edited, "EntityRuntime must validate the attached task component before termination.");
  return result;
}

static RuntimeEntityHandle CreateEntity(EntityRuntime runtime)
{
  RuntimeEntityHandle handle = runtime.CreateEntity();
  Require(runtime.TryPublishEntity(handle), "Fixture entities must be published before use.");
  return handle;
}

static NpcTargetSelectionInputs CreateTargetInputs(NpcPlayerTargetSnapshot target)
{
  return new NpcTargetSelectionInputs(
    NpcTargetSelectionStrategy.Normal,
    new NpcTargetGeometrySnapshot(Vector2.Zero, 20, 20),
    Direction: 1,
    DirectionY: 1,
    OldDirection: 0,
    OldDirectionY: 0,
    OldTarget: -1,
    CollideX: false,
    CollideY: false,
    Confused: false,
    Boss: false,
    FaceTarget: true,
    Players: [target],
    Npcs: []);
}

static NpcTargetSelectionStateSnapshot CaptureTargetState(NpcTargetSelectionStateComponent state)
{
  return new NpcTargetSelectionStateSnapshot(
    state.TargetKind,
    state.LegacyTargetIndex,
    state.SecondaryLegacySlot,
    state.TargetGeometry,
    state.Score,
    state.Direction,
    state.DirectionY,
    state.NetUpdateRequested);
}

static NpcTaskEndReason DifferentReason(NpcTaskEndReason reason)
{
  return reason == NpcTaskEndReason.WorldUnload
    ? NpcTaskEndReason.Removal
    : NpcTaskEndReason.WorldUnload;
}

static void Require(bool condition, string message)
{
  if (!condition)
  {
    throw new InvalidOperationException(message);
  }
}

readonly record struct NpcTargetSelectionStateSnapshot(
  NpcTargetKind TargetKind,
  int LegacyTargetIndex,
  int SecondaryLegacySlot,
  NpcTargetGeometrySnapshot TargetGeometry,
  float Score,
  int Direction,
  int DirectionY,
  bool NetUpdateRequested);
