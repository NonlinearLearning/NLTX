using System.Numerics;
using EntityEcs;
using Terraria.Content;
using Terraria.NonAuthoritative.Persistence;
using Terraria.Npc;
using Terraria.Projectile;
using Terraria.Relationships;

namespace Terraria.NonAuthoritative.SimulationHost;

internal static class NpcTaskLifecycleProbe
{
  internal static object Run(
    RuntimeNpcStore npcs,
    LoadedWorldSession session,
    ContentCatalog catalog,
    Vector2 position)
  {
    int initialCount = npcs.ActiveCount;
    int worldId = session.World.Descriptor.WorldId;
    object referenceOperations = VerifyReferenceBoundOperations(npcs, catalog, worldId, position);
    RuntimeNpcEntity npc = Spawn(npcs, catalog, worldId, position);
    npc.EnterTask(NpcTaskKind.GuideDayPatrol);
    Require(npc.TryCaptureTaskReference(out NpcTaskReference oldTask), "Capture old task.");
    npc.EnterTask(NpcTaskKind.GuideReturnHome);
    RuntimeNpcEntity.NpcTaskSnapshot before = npc.CaptureTaskSnapshot();
    Require(!npc.TryTerminateTask(oldTask, NpcTaskEndReason.Removal, out var stale) &&
        stale.RejectionReason == NpcTaskTerminationRejectionReason.StaleTaskReference &&
        npc.CaptureTaskSnapshot() == before,
      "A replaced task reference must reject without changing the current task.");

    bool borrowRan = false;
    bool releasedWhileBorrowed = false;
    Require(session.EntityRuntime.TryInspect<NpcTaskStateComponent>(
      npc.RuntimeHandle,
      (in NpcTaskStateComponent task) =>
      {
        borrowRan = true;
        releasedWhileBorrowed = npcs.TryRelease(npc);
      }), "Borrow the attached task component.");
    Require(borrowRan && !releasedWhileBorrowed && npc.CaptureTaskSnapshot() == before,
      "Release must reject a borrowed owner before ending its task.");
    Require(npc.TryCaptureTaskReference(out NpcTaskReference currentTask), "Capture current task.");
    Require(npc.TryTerminateTask(currentTask, NpcTaskEndReason.Transform, out var transformed) &&
        transformed.Changed && transformed.PreviousTask.Phase == NpcTaskPhase.Running &&
        transformed.CurrentTask.Phase == NpcTaskPhase.Idle,
      "A transform boundary must clear the active task before any new profile is entered.");
    Require(npcs.TryRelease(npc, NpcTaskEndReason.WorldUnload, out var afterTransform) &&
        afterTransform.Outcome == NpcTaskTerminationOutcome.EarlierEndReasonPreserved &&
        afterTransform.EndReason == NpcTaskEndReason.Transform &&
        !afterTransform.Changed && !npc.TryCaptureTaskReference(out _) &&
        !npc.TryTerminateTask(currentTask, NpcTaskEndReason.Removal, out _),
      "Unload after transform must preserve the first reason and remove the owner.");

    RuntimeNpcEntity replacement = Spawn(npcs, catalog, worldId, position);
    replacement.EnterTask(NpcTaskKind.GuideDayPatrol);
    NpcTaskTerminationResult oldEntity = default;
    Require(replacement.Slot == npc.Slot && replacement.RuntimeHandle != npc.RuntimeHandle &&
        !replacement.TryTerminateTask(currentTask, NpcTaskEndReason.Removal, out oldEntity) &&
        oldEntity.RejectionReason == NpcTaskTerminationRejectionReason.StaleEntityReference &&
        replacement.CaptureTaskSnapshot().Phase == NpcTaskPhase.Running,
      "A reused slot must not accept the previous entity's task reference.");
    Require(npcs.TryRelease(replacement, NpcTaskEndReason.Removal, out var removed) &&
        removed.EndReason == NpcTaskEndReason.Removal && removed.Changed,
      "Ordinary release must end the running task before removing its owner.");

    Require(npcs.TrySpawnParentChild(
      SimulationContentSupportManifest.BlueSlimeNetId,
      SimulationContentSupportManifest.BlueSlimeNetId,
      position, 0, catalog, worldId, out var binding),
      "Create a real parent and child for the death boundary.");
    if (!npcs.TryGetAt(binding.ParentSlot, out RuntimeNpcEntity? parent) || parent is null ||
        !npcs.TryGetAt(binding.ChildSlot, out RuntimeNpcEntity? child) || child is null)
    {
      throw new InvalidOperationException("Resolve both relation owners.");
    }
    parent.EnterTask(NpcTaskKind.GuideDayPatrol);
    child.EnterTask(NpcTaskKind.GuideReturnHome);
    Require(parent.TryCaptureTaskReference(out NpcTaskReference parentTask), "Capture parent task.");
    Require(child.TryCaptureTaskReference(out NpcTaskReference childTask), "Capture child task.");
    RuntimeNpcProjectileTargetSnapshot target = npcs.CreateProjectileTargetSnapshot()
      .Single(candidate => candidate.RuntimeHandle == parent.RuntimeHandle);
    npcs.AdvanceDamageTrackingTo(tickNumber: 0);
    Require(npcs.TryApplyProjectileHit(
      target, damage: 100_000, ownerSlot: 0, tickNumber: 0, knockback: 0f, hitDirection: 1,
      out var strike, out _) && strike.CombatResult.DeathTransitioned &&
        !npcs.TryResolveEntityReference(binding.ParentReference, out _) &&
        !npcs.TryResolveEntityReference(binding.ChildReference, out _) &&
        !parent.TryTerminateTask(parentTask, NpcTaskEndReason.Death, out _) &&
        !child.TryTerminateTask(childTask, NpcTaskEndReason.Death, out _) &&
        npcs.ActiveCount == initialCount,
      "A real lethal hit must remove both task and relation owners without replaying cleanup.");

    object motherSlimeSplit = VerifyMotherSlimeDeathSplit(npcs, catalog, worldId, position);

    object resetAndDispose = VerifyResetAndDispose(catalog, worldId, position);
    return new
    {
      Passed = true,
      StaleTaskRejected = stale.RejectionReason.ToString(),
      BorrowedReleaseRejected = borrowRan && !releasedWhileBorrowed,
      Transform = transformed,
      UnloadAfterTransform = afterTransform,
      StaleEntityRejected = oldEntity.RejectionReason.ToString(),
      Removal = removed,
      ProjectileDeathRemovedParentAndChild = true,
      MotherSlimeDeathSplit = motherSlimeSplit,
      ReferenceOperations = referenceOperations,
      ResetAndDispose = resetAndDispose,
      InitialCount = initialCount,
      FinalCount = npcs.ActiveCount,
    };
  }

  private static object VerifyMotherSlimeDeathSplit(
    RuntimeNpcStore store,
    ContentCatalog catalog,
    int worldId,
    Vector2 position)
  {
    int countBefore = store.ActiveCount;
    if (!store.TrySpawn(16, position, catalog, worldId, out RuntimeNpcEntity? parent) ||
        parent is null)
    {
      throw new InvalidOperationException(
        "The Mother Slime host probe could not create its parent.");
    }
    NpcInstanceId parentInstanceId = parent.InstanceId;
    Require(store.TryGetEntityReference(parentInstanceId, out EntityReference parentReference),
      "The Mother Slime host probe could not resolve its parent reference.");
    RuntimeNpcProjectileTargetSnapshot target = store.CreateProjectileTargetSnapshot()
      .Single(candidate => candidate.RuntimeHandle == parent.RuntimeHandle);
    store.AdvanceDamageTrackingTo(tickNumber: 1);
    Require(store.TryApplyProjectileHit(
          target,
          damage: 100_000,
          ownerSlot: 0,
          tickNumber: 1,
          knockback: 0f,
          hitDirection: 1,
          out NpcStrikeResult strike,
          out _),
      "The Mother Slime host probe could not apply its lethal hit.");
    IReadOnlyList<RuntimeNpcEntity> children = store.CreateActiveSnapshot()
      .Where(npc => npc.InstanceId != parentInstanceId && npc.Definition.NetId == 1)
      .ToArray();
    Require(strike.CombatResult.DeathTransitioned &&
        !store.TryResolveEntityReference(parentReference, out _) &&
        children.Count is >= 2 and <= 4 &&
        children.All(child => child.CurrentLife == 30 &&
          child.CaptureDefenseValue() == 4 &&
          child.CaptureAiState().State0 is 0f or -1000f or -2000f),
      "A lethal Mother Slime host hit must commit terminal death after spawning configured Blue Slime children.");
    foreach (RuntimeNpcEntity child in children)
    {
      Require(store.TryRelease(child), "The Mother Slime host probe could not release a split child.");
    }

    Require(store.ActiveCount == countBefore, "The Mother Slime host probe leaked a split child.");
    return new
    {
      Passed = true,
      ParentTerminalDeathCommitted = true,
      ChildCount = children.Count,
      ChildDefaultsApplied = true,
      ChildrenReleased = true,
    };
  }

  private static object VerifyResetAndDispose(ContentCatalog catalog, int worldId, Vector2 position)
  {
    var immunity = new ProjectileStaticNpcImmunityRegistryComponent(1, RuntimeNpcStore.MaximumNpcCapacity);
    var store = new RuntimeNpcStore(immunity);
    try
    {
      RuntimeNpcEntity beforeReset = Spawn(store, catalog, worldId, position);
      beforeReset.EnterTask(NpcTaskKind.GuideReturnHome);
      Require(beforeReset.TryCaptureTaskReference(out NpcTaskReference oldReference),
        "Capture a task before store reset.");
      store.Reset();
      RuntimeNpcEntity afterReset = Spawn(store, catalog, worldId, position);
      afterReset.EnterTask(NpcTaskKind.GuideDayPatrol);
      Require(IsUnavailable(() => beforeReset.TryCaptureTaskReference(out _)) &&
          !afterReset.TryTerminateTask(oldReference, NpcTaskEndReason.WorldUnload, out var stale) &&
          stale.RejectionReason == NpcTaskTerminationRejectionReason.StaleEntityReference &&
          afterReset.CaptureTaskSnapshot().Phase == NpcTaskPhase.Running,
        "Reset must invalidate old owners and keep the replacement task independent.");
      Require(afterReset.TryCaptureTaskReference(out NpcTaskReference disposeReference),
        "Capture a task before store disposal.");
      store.Dispose();
      store.Dispose();
      Require(store.ActiveCount == 0 &&
          IsUnavailable(() => afterReset.TryCaptureTaskReference(out _)) &&
          IsUnavailable(() =>
            afterReset.TryTerminateTask(disposeReference, NpcTaskEndReason.WorldUnload, out _)),
        "Disposal must remove task owners and remain safe when repeated.");
      return new { ResetInvalidatedOldTask = true, DisposeInvalidatedTask = true, RepeatDisposePassed = true };
    }
    finally
    {
      store.Dispose();
    }
  }

  private static object VerifyReferenceBoundOperations(
    RuntimeNpcStore store, ContentCatalog catalog, int worldId, Vector2 position)
  {
    RuntimeNpcEntity npc = Spawn(store, catalog, worldId, position);
    npc.EnterTask(NpcTaskKind.GuideDayPatrol);
    Require(npc.TryCaptureTaskReference(out NpcTaskReference first), "Capture first task run.");
    Require(npc.TryCompleteTask(first, NpcTaskKind.GuideDayPatrol, out _),
      "Complete the first run before restarting the same kind.");
    npc.EnterTask(NpcTaskKind.GuideDayPatrol);
    RuntimeNpcEntity.NpcTaskSnapshot before = npc.CaptureTaskSnapshot();
    Require(npc.TryCaptureTaskReference(out NpcTaskReference current), "Capture replacement run.");
    Require(current.TaskGeneration > first.TaskGeneration &&
        !npc.TryAdvanceTask(first, NpcTaskKind.GuideDayPatrol, out var oldAdvance) &&
        oldAdvance.RejectionReason == NpcTaskReferenceOperationRejectionReason.StaleTaskReference &&
        !npc.TryCompleteTask(first, NpcTaskKind.GuideDayPatrol, out var oldComplete) &&
        oldComplete.RejectionReason == NpcTaskReferenceOperationRejectionReason.StaleTaskReference &&
        !npc.TryFailTask(first, NpcTaskKind.GuideDayPatrol, NpcTaskFailureReason.NoPath,
          out var oldFail) &&
        oldFail.RejectionReason == NpcTaskReferenceOperationRejectionReason.StaleTaskReference &&
        !npc.TryInterruptTask(first, NpcTaskKind.GuideDayPatrol,
          NpcTaskFailureReason.TargetUnavailable, out var oldInterrupt) &&
        oldInterrupt.RejectionReason == NpcTaskReferenceOperationRejectionReason.StaleTaskReference &&
        npc.CaptureTaskSnapshot() == before,
      "All late operations must reject the previous run of the same task without mutation.");
    Require(npc.TryAdvanceTask(current, NpcTaskKind.GuideDayPatrol, out var advanced) &&
        advanced.CurrentTask.Cursor == 1 &&
        npc.TryCompleteTask(current, NpcTaskKind.GuideDayPatrol, out _) &&
        !npc.TryCompleteTask(current, NpcTaskKind.GuideDayPatrol, out var repeatedComplete) &&
        repeatedComplete.RejectionReason == NpcTaskReferenceOperationRejectionReason.NotRunning,
      "The current run must progress once and reject repeated completion.");

    npc.EnterTask(NpcTaskKind.GuideReturnHome);
    Require(npc.TryCaptureTaskReference(out NpcTaskReference interrupt), "Capture interrupt run.");
    Require(!npc.TryFailTask(interrupt, NpcTaskKind.GuideReturnHome,
        (NpcTaskFailureReason)byte.MaxValue, out var invalidReason) &&
        invalidReason.RejectionReason == NpcTaskReferenceOperationRejectionReason.InvalidFailureReason &&
        npc.CaptureTaskSnapshot().Phase == NpcTaskPhase.Running &&
        npc.TryInterruptTask(interrupt, NpcTaskKind.GuideReturnHome,
          NpcTaskFailureReason.TargetUnavailable, out _),
      "Invalid reasons must leave state unchanged while the current interrupt succeeds.");
    npc.EnterTask(NpcTaskKind.GuideReturnHome);
    Require(npc.TryCaptureTaskReference(out NpcTaskReference failedRun), "Capture failure run.");
    Require(npc.TryFailTask(failedRun, NpcTaskKind.GuideReturnHome, NpcTaskFailureReason.NoPath,
        out var failed) && failed.CurrentTask.Phase == NpcTaskPhase.Failed &&
        !npc.TryFailTask(failedRun, NpcTaskKind.GuideReturnHome, NpcTaskFailureReason.NoPath,
          out var repeatedFailure) &&
        repeatedFailure.RejectionReason == NpcTaskReferenceOperationRejectionReason.NotRunning &&
        store.TryRelease(npc),
      "The current failure must commit once and release through the task end boundary.");
    return new
    {
      SameKindRestartRejectedAllOldOperations = true,
      CurrentAdvanceCompletePassed = true,
      CurrentInterruptFailurePassed = true,
      InvalidReasonLeftStateUnchanged = true,
      RepeatedCompletionFailureRejected = true,
    };
  }

  private static RuntimeNpcEntity Spawn(
    RuntimeNpcStore store, ContentCatalog catalog, int worldId, Vector2 position)
  {
    if (!store.TrySpawn(SimulationContentSupportManifest.BlueSlimeNetId, position, catalog, worldId,
        out RuntimeNpcEntity? npc) || npc is null)
    {
      throw new InvalidOperationException("The task lifecycle probe could not create its NPC owner.");
    }

    return npc;
  }

  private static void Require(bool condition, string message)
  {
    if (!condition)
    {
      throw new InvalidOperationException(message);
    }
  }

  private static bool IsUnavailable(Func<bool> operation)
  {
    try
    {
      return !operation();
    }
    catch (ObjectDisposedException)
    {
      return true;
    }
  }
}
