using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using EntityEcs;
using EntityEcs.Components;
using Terraria.Npc;
using Terraria.NonAuthoritative.Persistence;
using Terraria.Player;
using Terraria.Projectile;
using Terraria.Relationships;
using Terraria.SpatialSimulation.Components;
using Terraria.WorldGeneration.Components;
using Terraria.WorldStorage;

namespace Terraria.NonAuthoritative.SimulationHost;

/// <summary>Runs the real Simulation host's published-session failure, rollback, and retry path.</summary>
internal static class RuntimeWorldLoadRollbackVerification
{
  private const int StaticImmunityProjectileType = 1;
  private const int StaticImmunityCooldownTicks = 20;
  private const uint StaticImmunityObservedAtTick = 100;

  public static RuntimeNpcRollbackEvidence CaptureRuntimeNpcRollbackEvidence(
    LoadedWorldSession session,
    RuntimeNpcStore runtimeNpcs,
    RuntimePlayerStore runtimePlayers,
    RuntimeItemRegistry runtimeItems,
    RuntimeProjectileStore runtimeProjectiles,
    ProjectileStaticNpcImmunityRegistryComponent staticNpcImmunityRegistry)
  {
    return CaptureEvidence(
      session,
      runtimeNpcs,
      runtimePlayers,
      runtimeItems,
      runtimeProjectiles,
      staticNpcImmunityRegistry,
      prepareStaticImmunity: true);
  }

  public static bool HasRuntimeNpcProjectionUnchanged(
    RuntimeNpcRollbackEvidence evidence,
    LoadedWorldSession session,
    RuntimeNpcStore runtimeNpcs,
    RuntimePlayerStore runtimePlayers,
    RuntimeItemRegistry runtimeItems,
    RuntimeProjectileStore runtimeProjectiles,
    ProjectileStaticNpcImmunityRegistryComponent staticNpcImmunityRegistry)
  {
    ArgumentNullException.ThrowIfNull(evidence);
    try
    {
      RuntimeNpcRollbackEvidence current = CaptureEvidence(
        session,
        runtimeNpcs,
        runtimePlayers,
        runtimeItems,
        runtimeProjectiles,
        staticNpcImmunityRegistry,
        prepareStaticImmunity: false);
      return evidence.HasSameOwnerProjection(current);
    }
    catch (ObjectDisposedException)
    {
      return false;
    }
  }

  public static RuntimeNpcCandidateRollbackEvidence CaptureFailedCandidateNpcEvidence(
    LoadedWorldSession session,
    RuntimeNpcStore candidateStore,
    ProjectileStaticNpcImmunityRegistryComponent staticNpcImmunityRegistry)
  {
    ArgumentNullException.ThrowIfNull(session);
    ArgumentNullException.ThrowIfNull(candidateStore);
    ArgumentNullException.ThrowIfNull(staticNpcImmunityRegistry);
    if (session.IsDisposed || !session.IsPublished)
    {
      throw new InvalidOperationException(
        "Candidate NPC evidence must be captured while the published candidate is still live.");
    }

    EntityRuntime runtime = session.EntityRuntime;
    RuntimeNpcEntity[] npcs = candidateStore.CreateActiveSnapshot().ToArray();
    if (npcs.Length == 0)
    {
      throw new InvalidOperationException(
        "The failed published candidate must contain at least one NPC runtime root.");
    }

    var roots = new List<CandidateNpcRootCapture>(npcs.Length);
    foreach (RuntimeNpcEntity npc in npcs)
    {
      if (!candidateStore.TryGetEntityReference(npc.InstanceId, out EntityReference reference) ||
          reference.Scope != EntityReferenceScope.Npc ||
          !runtime.TryResolve(reference, out RuntimeEntityHandle handle) ||
          handle != npc.RuntimeHandle)
      {
        throw new InvalidOperationException(
          $"Candidate NPC {npc.InstanceId.Value} had no matching runtime reference before failure.");
      }

      roots.Add(new CandidateNpcRootCapture(npc, npc.InstanceId, reference, handle));
    }

    StaticNpcImmunityCapture staticImmunity = PrepareStaticNpcImmunity(
      staticNpcImmunityRegistry,
      roots[0].Entity.Slot.Value);

    return new RuntimeNpcCandidateRollbackEvidence(
      session,
      candidateStore,
      runtime,
      staticNpcImmunityRegistry,
      staticImmunity,
      Array.AsReadOnly(roots.ToArray()));
  }

  public static RuntimeWorldLoadRollbackReport Run(
    string candidateWorldPath,
    Func<string, WorldLoadRecoveryResult?> loadWorld,
    Func<LoadedWorldSession?> activeSession,
    Func<IReadOnlyList<LoadedWorldSession>> sessions,
    Func<bool> loadGateReleased,
    Func<LoadedWorldSession, RuntimeNpcStore> storeForSession,
    Func<RuntimeNpcStore> selectedNpcStore,
    Func<LoadedWorldSession, EntityRuntime> runtimeForSession,
    Func<LoadedWorldSession, RuntimeNpcCandidateRollbackEvidence> candidateNpcEvidenceForSession,
    Func<LoadedWorldSession, ProjectileStaticNpcImmunityRegistryComponent>
      staticNpcImmunityForSession,
    Func<bool> lateFinalizeFailureObserved,
    Action armLateFinalizeFailure,
    RuntimePlayerStore runtimePlayers,
    RuntimeItemRegistry runtimeItems,
    RuntimeProjectileStore runtimeProjectiles)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(candidateWorldPath);
    ArgumentNullException.ThrowIfNull(loadWorld);
    ArgumentNullException.ThrowIfNull(activeSession);
    ArgumentNullException.ThrowIfNull(sessions);
    ArgumentNullException.ThrowIfNull(loadGateReleased);
    ArgumentNullException.ThrowIfNull(storeForSession);
    ArgumentNullException.ThrowIfNull(selectedNpcStore);
    ArgumentNullException.ThrowIfNull(runtimeForSession);
    ArgumentNullException.ThrowIfNull(candidateNpcEvidenceForSession);
    ArgumentNullException.ThrowIfNull(staticNpcImmunityForSession);
    ArgumentNullException.ThrowIfNull(lateFinalizeFailureObserved);
    ArgumentNullException.ThrowIfNull(armLateFinalizeFailure);
    ArgumentNullException.ThrowIfNull(runtimePlayers);
    ArgumentNullException.ThrowIfNull(runtimeItems);
    ArgumentNullException.ThrowIfNull(runtimeProjectiles);

    LoadedWorldSession previousSession = activeSession() ??
      throw new InvalidOperationException(
        "The rollback probe requires a previously published active session.");
    RuntimeNpcStore previousNpcStore = storeForSession(previousSession);
    Require(ReferenceEquals(selectedNpcStore(), previousNpcStore),
      "The active host must select the NPC store mapped to its current previous session.");
    RuntimeNpcRollbackEvidence evidence = CaptureRuntimeNpcRollbackEvidence(
      previousSession,
      previousNpcStore,
      runtimePlayers,
      runtimeItems,
      runtimeProjectiles,
      staticNpcImmunityForSession(previousSession));
    EntityRuntime previousRuntime = runtimeForSession(previousSession);
    if (!ReferenceEquals(previousRuntime, previousSession.EntityRuntime) ||
        !loadGateReleased())
    {
      throw new InvalidOperationException(
        "The previous session runtime or world-load gate was invalid before the probe.");
    }

    LoadedWorldSession[] sessionsBeforeFailure = SnapshotSessions(sessions);
    armLateFinalizeFailure();
    WorldLoadRecoveryResult failedLoad = loadWorld(candidateWorldPath) ??
      throw new InvalidOperationException(
        "The production world-load hook returned no recovery result for the injected failure.");
    Require(!failedLoad.Succeeded &&
        failedLoad.TerminalAction == WorldLoadRecoveryAction.ReportLoadFailure &&
        failedLoad.Failure.Kind != WorldStorageFailureKind.None &&
        failedLoad.CleanupFailure.Kind == WorldStorageFailureKind.None &&
        failedLoad.LastLoadOutcome is not null,
      "The late-finalize fault must return a classified production recovery failure.");
    Require(lateFinalizeFailureObserved(),
      "The one-shot late-finalize fault must be observed after the candidate was published.");

    LoadedWorldSession[] sessionsAfterFailure = SnapshotSessions(sessions);
    LoadedWorldSession failedCandidate = FindSingleNewSession(
      sessionsBeforeFailure,
      sessionsAfterFailure,
      "failed candidate");
    EntityRuntime failedCandidateRuntime = runtimeForSession(failedCandidate);
    RuntimeNpcCandidateRollbackEvidence candidateNpcEvidence =
      candidateNpcEvidenceForSession(failedCandidate);
    ProjectileStaticNpcImmunityRegistryComponent failedCandidateImmunity =
      staticNpcImmunityForSession(failedCandidate);
    int failedCandidateNpcRootCountBeforeCleanup = candidateNpcEvidence.Roots.Count;
    Require(ReferenceEquals(candidateNpcEvidence.Session, failedCandidate) &&
        ReferenceEquals(candidateNpcEvidence.Runtime, failedCandidateRuntime) &&
        ReferenceEquals(candidateNpcEvidence.StaticNpcImmunityRegistry, failedCandidateImmunity) &&
        !ReferenceEquals(failedCandidateImmunity, evidence.StaticNpcImmunityRegistry) &&
        failedCandidateNpcRootCountBeforeCleanup > 0,
      "The published failed candidate must have captured NPC roots before production cleanup.");
    Require(failedCandidate.IsDisposed && failedCandidateRuntime.EntityCount == 0,
      "The production finally must dispose the failed candidate and remove every captured runtime root.");
    int failedCandidateNpcStoreCountAfterCleanup =
      candidateNpcEvidence.Store.CreateActiveSnapshot().Count;
    bool failedCandidateNpcStoreIdentityProjectionsRejectedAfterCleanup =
      candidateNpcEvidence.Roots.All(root =>
        !candidateNpcEvidence.Store.TryGetEntityReference(root.InstanceId, out _));
    Require(failedCandidateNpcStoreCountAfterCleanup == 0 &&
        failedCandidateNpcStoreIdentityProjectionsRejectedAfterCleanup &&
        !IsStaticNpcImmune(
          candidateNpcEvidence.StaticNpcImmunityRegistry,
          candidateNpcEvidence.StaticImmunity),
      "Production candidate cleanup must empty the candidate NPC store and clear its old identity projections.");
    Require(ReferenceEquals(activeSession(), previousSession) && !previousSession.IsDisposed,
      "The failed candidate load must leave the original session active and usable.");
    bool previousSelectedNpcStoreRestoredAfterFailure =
      ReferenceEquals(storeForSession(previousSession), previousNpcStore) &&
      ReferenceEquals(selectedNpcStore(), previousNpcStore);
    Require(previousSelectedNpcStoreRestoredAfterFailure &&
        ReferenceEquals(
          staticNpcImmunityForSession(previousSession),
          evidence.StaticNpcImmunityRegistry) &&
        IsStaticNpcImmune(evidence.StaticNpcImmunityRegistry, evidence.StaticImmunity),
      "Production rollback must reselect the exact previous RuntimeNpcStore object.");
    bool failedCandidateNpcReferencesRejectedAfterCleanup =
      failedCandidateNpcStoreIdentityProjectionsRejectedAfterCleanup &&
      CandidateReferencesAreRejected(candidateNpcEvidence, previousRuntime);
    Require(failedCandidateNpcReferencesRejectedAfterCleanup,
      "The restored previous runtime must reject all references from the failed candidate.");
    Require(HasRuntimeNpcProjectionUnchanged(
        evidence,
        previousSession,
        selectedNpcStore(),
        runtimePlayers,
        runtimeItems,
        runtimeProjectiles,
        evidence.StaticNpcImmunityRegistry),
      "The previous NPC, Player, Item, or Projectile owner state changed during rollback.");
    Require(loadGateReleased(),
      "The failed production world-load attempt must release the load gate.");

    LoadedWorldSession[] sessionsBeforeRetry = sessionsAfterFailure;
    WorldLoadRecoveryResult successfulLoad = loadWorld(candidateWorldPath) ??
      throw new InvalidOperationException(
        "The production world-load hook returned no recovery result for the retry.");
    Require(successfulLoad.Succeeded &&
        successfulLoad.TerminalAction == WorldLoadRecoveryAction.NotifyWorldLoaded &&
        successfulLoad.Failure.Kind == WorldStorageFailureKind.None &&
        successfulLoad.CleanupFailure.Kind == WorldStorageFailureKind.None,
      "The retry must complete through the production world-load recovery path.");

    LoadedWorldSession nextSession = activeSession() ??
      throw new InvalidOperationException(
        "The successful retry did not publish an active session.");
    LoadedWorldSession retryCandidate = FindSingleNewSession(
      sessionsBeforeRetry,
      SnapshotSessions(sessions),
      "successful retry candidate");
    EntityRuntime nextRuntime = runtimeForSession(nextSession);
    ProjectileStaticNpcImmunityRegistryComponent retryImmunity =
      staticNpcImmunityForSession(nextSession);
    Require(ReferenceEquals(nextSession, retryCandidate) &&
        !ReferenceEquals(nextSession, previousSession) &&
        !ReferenceEquals(nextSession, failedCandidate) &&
        !ReferenceEquals(retryImmunity, evidence.StaticNpcImmunityRegistry) &&
        !ReferenceEquals(retryImmunity, candidateNpcEvidence.StaticNpcImmunityRegistry) &&
        nextSession.IsComplete && nextSession.IsPublished && !nextSession.IsDisposed,
      "The retry must publish a fresh complete medium-world session.");
    Require(previousSession.IsDisposed && previousRuntime.EntityCount == 0,
      "Successful retry must retire the previous session and remove its runtime roots.");
    Require(CapturedReferencesAreRejected(evidence, nextRuntime),
      "The new runtime must reject every NPC, Player, Item, and Projectile reference from the old world.");
    Require(CandidateReferencesAreRejected(candidateNpcEvidence, nextRuntime),
      "The successful retry runtime must reject all references from the failed candidate.");
    Require(loadGateReleased(),
      "The successful retry must leave the world-load gate released.");

    return new RuntimeWorldLoadRollbackReport(
      FailedCandidateDisposed: failedCandidate.IsDisposed,
      FailedCandidateEntityCountAfterDispose: failedCandidateRuntime.EntityCount,
      FailedCandidateNpcRootCountBeforeCleanup: failedCandidateNpcRootCountBeforeCleanup,
      FailedCandidateNpcStoreCountAfterCleanup: failedCandidateNpcStoreCountAfterCleanup,
      FailedCandidateNpcReferencesRejectedAfterCleanup:
        failedCandidateNpcReferencesRejectedAfterCleanup,
      LateFinalizeFailureObserved: lateFinalizeFailureObserved(),
      PreviousSessionRestored: true,
      PreviousNpcStoreObjectRestored: true,
      PreviousSelectedNpcStoreRestoredAfterFailure: previousSelectedNpcStoreRestoredAfterFailure,
      PreviousOwnerStateUnchanged: true,
      PreviousNpcCount: evidence.NpcStates.Count,
      PreviousPlayerCount: 1,
      PreviousItemCount: evidence.Player.Items.Count,
      PreviousProjectileCount: evidence.Projectiles.Count,
      PreviousStaticNpcImmunityPreservedAfterFailure: true,
      FailedCandidateStaticNpcImmunityCleared: true,
      RetryStaticNpcImmunityRegistryIsIndependent: true,
      PreviousSessionDisposedAfterRetry: previousSession.IsDisposed,
      PreviousEntityCountAfterRetry: previousRuntime.EntityCount,
      RetryPublishedFreshSession: true,
      OldOwnerReferencesRejectedByRetry: true,
      LoadGateReleasedAfterFailureAndRetry: true,
      PreviousRuntimeId: previousRuntime.RuntimeId,
      FailedCandidateRuntimeId: failedCandidateRuntime.RuntimeId,
      RetryRuntimeId: nextRuntime.RuntimeId);
  }

  private static IReadOnlyList<NpcCapture> CaptureNpcs(
    LoadedWorldSession session,
    RuntimeNpcStore runtimeNpcs,
    EntityRuntime runtime)
  {
    RuntimeNpcEntity[] npcs = runtimeNpcs.CreateActiveSnapshot().ToArray();
    if (npcs.Length == 0)
    {
      throw new InvalidOperationException(
        "The previous small-world runtime must contain at least one supported NPC.");
    }

    var captures = new List<NpcCapture>(npcs.Length);
    foreach (RuntimeNpcEntity npc in npcs)
    {
      if (!runtimeNpcs.TryGetEntityReference(npc.InstanceId, out EntityReference reference) ||
          reference.Scope != EntityReferenceScope.Npc ||
          !runtime.TryResolve(reference, out RuntimeEntityHandle resolvedHandle) ||
          resolvedHandle != npc.RuntimeHandle ||
          npc.RuntimeHandle.RuntimeId != session.WorldRuntimeId)
      {
        throw new InvalidOperationException(
          $"NPC {npc.InstanceId.Value} did not resolve to its captured runtime handle.");
      }

      captures.Add(NpcCapture.Create(npc, runtime, reference, resolvedHandle));
    }

    return Array.AsReadOnly(captures.ToArray());
  }

  private static RuntimeNpcRollbackEvidence CaptureEvidence(
    LoadedWorldSession session,
    RuntimeNpcStore runtimeNpcs,
    RuntimePlayerStore runtimePlayers,
    RuntimeItemRegistry runtimeItems,
    RuntimeProjectileStore runtimeProjectiles,
    ProjectileStaticNpcImmunityRegistryComponent staticNpcImmunityRegistry,
    bool prepareStaticImmunity)
  {
    ArgumentNullException.ThrowIfNull(session);
    ArgumentNullException.ThrowIfNull(runtimeNpcs);
    ArgumentNullException.ThrowIfNull(runtimePlayers);
    ArgumentNullException.ThrowIfNull(runtimeItems);
    ArgumentNullException.ThrowIfNull(runtimeProjectiles);
    ArgumentNullException.ThrowIfNull(staticNpcImmunityRegistry);
    if (session.IsDisposed || !session.IsPublished || !session.IsComplete)
    {
      throw new InvalidOperationException(
        "Rollback evidence requires the currently published complete session.");
    }

    EntityRuntime runtime = session.EntityRuntime;
    if (!runtimeItems.IsBoundTo(runtime) || runtimePlayers.Players.Count != 1)
    {
      throw new InvalidOperationException(
        "Rollback evidence requires one live Player and its Item registry in the previous runtime.");
    }

    IReadOnlyList<NpcCapture> npcs = CaptureNpcs(session, runtimeNpcs, runtime);
    NpcCapture firstNpc = npcs[0];
    StaticNpcImmunityCapture staticImmunity = prepareStaticImmunity
      ? PrepareStaticNpcImmunity(staticNpcImmunityRegistry, firstNpc.Slot.Value)
      : CaptureStaticNpcImmunity(staticNpcImmunityRegistry, firstNpc.Slot.Value);
    if (!staticImmunity.IsImmuneAtStart ||
        !staticImmunity.IsActiveBeforeExpiry ||
        staticImmunity.IsExpiredAtExpiry)
    {
      throw new InvalidOperationException(
        "The previous world must retain the exact non-zero static NPC immunity expiry.");
    }

    PlayerCapture player = CapturePlayer(runtimePlayers, runtimeItems, runtime);
    IReadOnlyList<ProjectileCapture> projectiles = CaptureProjectiles(
      runtime,
      player.Reference);
    if (!projectiles.Any(projectile => projectile.Identity.OwnerReference == player.Reference))
    {
      throw new InvalidOperationException(
        "Rollback evidence requires a live Projectile owned by the captured Player.");
    }

    return new RuntimeNpcRollbackEvidence(
      session,
      runtimeNpcs,
      runtimePlayers,
      runtimeItems,
      runtimeProjectiles,
      staticNpcImmunityRegistry,
      staticImmunity,
      runtime,
      runtime.EntityCount,
      npcs,
      player,
      projectiles,
      runtimeProjectiles.ActiveCount);
  }

  private static StaticNpcImmunityCapture PrepareStaticNpcImmunity(
    ProjectileStaticNpcImmunityRegistryComponent registry,
    int npcSlot)
  {
    var policy = new ProjectileHitImmunityPolicyComponent(
      usesStaticNpcImmunity: true,
      staticNpcCooldownTicks: StaticImmunityCooldownTicks);
    var penetration = new ProjectilePenetrationStateComponent(
      remainingHits: 2,
      maximumHits: 2);
    if (!ProjectileStaticNpcImmunitySystem.RecordAcceptedNpcHit(
          registry,
          policy,
          penetration,
          StaticImmunityProjectileType,
          npcSlot,
          StaticImmunityObservedAtTick))
    {
      throw new InvalidOperationException(
        "The existing accepted-hit rule rejected the static NPC immunity probe.");
    }

    return CaptureStaticNpcImmunity(registry, npcSlot);
  }

  private static StaticNpcImmunityCapture CaptureStaticNpcImmunity(
    ProjectileStaticNpcImmunityRegistryComponent registry,
    int npcSlot)
  {
    uint expiryTick = checked(StaticImmunityObservedAtTick + StaticImmunityCooldownTicks);
    return new StaticNpcImmunityCapture(
      StaticImmunityProjectileType,
      npcSlot,
      expiryTick,
      IsImmuneAtStart: ProjectileStaticNpcImmunitySystem.IsNpcImmune(
        registry,
        StaticImmunityProjectileType,
        npcSlot,
        StaticImmunityObservedAtTick),
      IsActiveBeforeExpiry: ProjectileStaticNpcImmunitySystem.IsNpcImmune(
        registry,
        StaticImmunityProjectileType,
        npcSlot,
        expiryTick - 1),
      IsExpiredAtExpiry: ProjectileStaticNpcImmunitySystem.IsNpcImmune(
        registry,
        StaticImmunityProjectileType,
        npcSlot,
        expiryTick));
  }

  private static bool IsStaticNpcImmune(
    ProjectileStaticNpcImmunityRegistryComponent registry,
    StaticNpcImmunityCapture capture)
  {
    return ProjectileStaticNpcImmunitySystem.IsNpcImmune(
      registry,
      capture.ProjectileType,
      capture.NpcSlot,
      StaticImmunityObservedAtTick);
  }

  private static bool IsStaticNpcImmunityCleared(
    ProjectileStaticNpcImmunityRegistryComponent registry,
    StaticNpcImmunityCapture capture)
  {
    return !ProjectileStaticNpcImmunitySystem.IsNpcImmune(
        registry,
        capture.ProjectileType,
        capture.NpcSlot,
        StaticImmunityObservedAtTick) &&
      !ProjectileStaticNpcImmunitySystem.IsNpcImmune(
        registry,
        capture.ProjectileType,
        capture.NpcSlot,
        capture.ExpiryTick - 1) &&
      !ProjectileStaticNpcImmunitySystem.IsNpcImmune(
        registry,
        capture.ProjectileType,
        capture.NpcSlot,
        capture.ExpiryTick);
  }

  private static PlayerCapture CapturePlayer(
    RuntimePlayerStore runtimePlayers,
    RuntimeItemRegistry runtimeItems,
    EntityRuntime runtime)
  {
    RuntimePlayerEntity player = runtimePlayers.Players[0];
    EntityReference reference = player.Reference;
    if (!runtime.TryResolve(reference, out RuntimeEntityHandle resolvedHandle) ||
        resolvedHandle != player.RuntimeHandle ||
        !runtimePlayers.TryResolveEntityReference(reference, out RuntimePlayerEntity? resolvedPlayer) ||
        !ReferenceEquals(player, resolvedPlayer))
    {
      throw new InvalidOperationException(
        "The previous Player reference did not resolve to its original root and owner object.");
    }

    var items = new List<ItemCapture>();
    for (int slot = 0; slot < PlayerInventorySlotsComponent.MainInventorySlotCount; slot++)
    {
      if (!player.Inventory.TryGetItemAtSlot(slot, out PlayerInventoryItemSnapshot item))
      {
        continue;
      }

      if (item.IsEmpty || !runtime.TryResolve(item.Entity.Reference, out RuntimeEntityHandle itemHandle) ||
          !runtimeItems.TryGet(item.Entity, out PlayerInventoryItemSnapshot ownerItem) ||
          ownerItem != item)
      {
        throw new InvalidOperationException(
          $"The previous Player's inventory Item in slot {slot} did not resolve to its owner state.");
      }

      items.Add(new ItemCapture(slot, item, item.Entity.Reference, itemHandle));
    }

    if (items.Count == 0)
    {
      throw new InvalidOperationException(
        "The previous Player must own at least one resolvable inventory Item.");
    }

    if (!TryCapture(runtime, player.RuntimeHandle, out MovementGroundedStateComponent grounded))
    {
      throw new InvalidOperationException(
        "The previous Player root has no movement grounded component.");
    }

    return new PlayerCapture(
      player,
      player.Slot,
      reference,
      resolvedHandle,
      player.Location,
      player.Velocity,
      grounded,
      player.Lifecycle,
      player.Vitals,
      Array.AsReadOnly(items.ToArray()));
  }

  private static IReadOnlyList<ProjectileCapture> CaptureProjectiles(
    EntityRuntime runtime,
    EntityReference playerReference)
  {
    RuntimeEntityHandle[] handles = runtime.Match<
      ProjectileIdentityComponent,
      LocationComponent>();
    var captures = new List<ProjectileCapture>(handles.Length);
    foreach (RuntimeEntityHandle handle in handles)
    {
      if (!runtime.TryGetReference(handle, EntityReferenceScope.Projectile, out EntityReference reference) ||
          !TryCapture(runtime, handle, out ProjectileIdentityComponent identity) ||
          !TryCapture(runtime, handle, out LocationComponent location) ||
          !TryCapture(runtime, handle, out VelocityComponent velocity) ||
          !TryCapture(runtime, handle, out ProjectileBehaviorStateComponent behavior) ||
          !TryCapture(runtime, handle, out ProjectileLifetimeStateComponent lifetime))
      {
        throw new InvalidOperationException(
          $"Projectile root {handle} is missing a required rollback owner component.");
      }

      captures.Add(new ProjectileCapture(
        reference,
        handle,
        identity,
        new ProjectileKinematicsStateComponent(
          new Vector2(location.X, location.Y),
          new Vector2(velocity.X, velocity.Y)),
        behavior,
        lifetime));
    }

    if (!captures.Any(projectile => projectile.Identity.OwnerReference == playerReference))
    {
      throw new InvalidOperationException(
        "The rollback probe requires an active Projectile owned by the captured Player.");
    }

    return Array.AsReadOnly(captures.ToArray());
  }

  private static bool CapturedReferencesAreRejected(
    RuntimeNpcRollbackEvidence evidence,
    EntityRuntime runtime)
  {
    foreach (NpcCapture npc in evidence.NpcStates)
    {
      if (runtime.TryResolve(npc.Reference, out _))
      {
        return false;
      }
    }

    if (runtime.TryResolve(evidence.Player.Reference, out _))
    {
      return false;
    }

    foreach (ItemCapture item in evidence.Player.Items)
    {
      if (runtime.TryResolve(item.Reference, out _))
      {
        return false;
      }
    }

    foreach (ProjectileCapture projectile in evidence.Projectiles)
    {
      if (runtime.TryResolve(projectile.Reference, out _))
      {
        return false;
      }
    }

    return true;
  }

  private static bool CandidateReferencesAreRejected(
    RuntimeNpcCandidateRollbackEvidence evidence,
    EntityRuntime runtime)
  {
    return evidence.Roots.All(root => !runtime.TryResolve(root.Reference, out _));
  }

  private static bool TryCapture<TComponent>(
    EntityRuntime runtime,
    RuntimeEntityHandle handle,
    out TComponent component)
    where TComponent : struct
  {
    return runtime.TryCapture<TComponent, TComponent>(
      handle,
      static value => value,
      out component);
  }

  private static LoadedWorldSession[] SnapshotSessions(
    Func<IReadOnlyList<LoadedWorldSession>> sessions)
  {
    IReadOnlyList<LoadedWorldSession> current = sessions() ??
      throw new InvalidOperationException("The host returned a null session snapshot.");
    return current.ToArray();
  }

  private static LoadedWorldSession FindSingleNewSession(
    IReadOnlyList<LoadedWorldSession> previous,
    IReadOnlyList<LoadedWorldSession> current,
    string description)
  {
    LoadedWorldSession[] created = current
      .Where(candidate => !previous.Any(existing => ReferenceEquals(existing, candidate)))
      .ToArray();
    if (created.Length != 1)
    {
      throw new InvalidOperationException(
        $"The production load must create exactly one {description}; observed {created.Length}.");
    }

    return created[0];
  }

  private static void Require(bool condition, string message)
  {
    if (!condition)
    {
      throw new InvalidOperationException(message);
    }
  }

  internal sealed record NpcCapture(
    RuntimeNpcEntity Entity,
    NpcInstanceId InstanceId,
    Terraria.Npc.NpcSlot Slot,
    uint SlotGeneration,
    EntityReference Reference,
    RuntimeEntityHandle Handle,
    WorldNpcState SavedState,
    SavedNpcValues SavedValues,
    LocationComponent Location,
    VelocityComponent Velocity,
    MovementGroundedStateComponent Grounded,
    int CurrentLife,
    int MaximumLife,
    bool IsActive,
    NpcAiStateComponent AiState,
    int BehaviorAction,
    long LastBehaviorUpdatedTick,
    RuntimeNpcEntity.NpcDirectionSnapshot Direction,
    RuntimeNpcEntity.NpcMovementTickSnapshot MovementTick,
    RuntimeNpcEntity.NpcTaskSnapshot Task,
    RuntimeNpcEntity.NpcHousingRelationSnapshot Housing,
    string GivenName,
    RuntimeNpcEntity.NpcPresentationSnapshot Presentation)
  {
    internal static NpcCapture Create(
      RuntimeNpcEntity entity,
      EntityRuntime runtime,
      EntityReference reference,
      RuntimeEntityHandle handle)
    {
      return new NpcCapture(
        entity,
        entity.InstanceId,
        entity.Slot,
        entity.SlotGeneration,
        reference,
        handle,
        entity.SavedState,
        new SavedNpcValues(
          entity.SavedState.NetId,
          entity.SavedState.LegacyTypeName,
          entity.SavedState.IsTownNpc,
          entity.SavedState.Name,
          entity.SavedState.X,
          entity.SavedState.Y,
          entity.SavedState.Homeless,
          entity.SavedState.Home,
          entity.SavedState.Variation,
          entity.SavedState.HomelessDespawn),
        entity.Location,
        entity.Velocity,
        CaptureRequired<MovementGroundedStateComponent>(runtime, entity, handle),
        entity.CurrentLife,
        entity.MaximumLife,
        entity.IsActive,
        entity.CaptureAiState(),
        entity.BehaviorAction,
        entity.LastBehaviorUpdatedTick,
        entity.CaptureDirection(),
        entity.CaptureMovementTickSnapshot(),
        entity.CaptureTaskSnapshot(),
        entity.CaptureHousingRelationSnapshot(),
        entity.CaptureGivenName(),
        entity.CapturePresentationSnapshot());
    }

    internal bool HasSameState(NpcCapture other)
    {
      return ReferenceEquals(Entity, other.Entity) &&
        InstanceId == other.InstanceId &&
        Slot == other.Slot &&
        SlotGeneration == other.SlotGeneration &&
        Reference == other.Reference &&
        Handle == other.Handle &&
        ReferenceEquals(SavedState, other.SavedState) &&
        SavedValues == other.SavedValues &&
        Location.X == other.Location.X &&
        Location.Y == other.Location.Y &&
        Velocity.X == other.Velocity.X &&
        Velocity.Y == other.Velocity.Y &&
        Grounded == other.Grounded &&
        CurrentLife == other.CurrentLife &&
        MaximumLife == other.MaximumLife &&
        IsActive == other.IsActive &&
        AiState.Equals(other.AiState) &&
        BehaviorAction == other.BehaviorAction &&
        LastBehaviorUpdatedTick == other.LastBehaviorUpdatedTick &&
        Direction == other.Direction &&
        MovementTick == other.MovementTick &&
        Task == other.Task &&
        Housing == other.Housing &&
        GivenName == other.GivenName &&
        Presentation == other.Presentation;
    }

    private static TComponent CaptureRequired<TComponent>(
      EntityRuntime runtime,
      RuntimeNpcEntity entity,
      RuntimeEntityHandle handle)
      where TComponent : struct
    {
      if (!TryCapture(runtime, handle, out TComponent component))
      {
        throw new InvalidOperationException(
          $"NPC {entity.InstanceId.Value} is missing {typeof(TComponent).Name}.");
      }

      return component;
    }
  }

  internal sealed record PlayerCapture(
    RuntimePlayerEntity Entity,
    int Slot,
    EntityReference Reference,
    RuntimeEntityHandle Handle,
    LocationComponent Location,
    VelocityComponent Velocity,
    MovementGroundedStateComponent Grounded,
    RuntimePlayerEntity.LifecycleSnapshot Lifecycle,
    RuntimePlayerEntity.VitalsSnapshot Vitals,
    IReadOnlyList<ItemCapture> Items)
  {
    internal bool HasSameState(PlayerCapture other)
    {
      return ReferenceEquals(Entity, other.Entity) &&
        Slot == other.Slot &&
        Reference == other.Reference &&
        Handle == other.Handle &&
        Location.X == other.Location.X &&
        Location.Y == other.Location.Y &&
        Velocity.X == other.Velocity.X &&
        Velocity.Y == other.Velocity.Y &&
        Grounded == other.Grounded &&
        Lifecycle == other.Lifecycle &&
        Vitals == other.Vitals &&
        Items.SequenceEqual(other.Items);
    }
  }

  internal sealed record ItemCapture(
    int Slot,
    PlayerInventoryItemSnapshot Item,
    EntityReference Reference,
    RuntimeEntityHandle Handle);

  internal sealed record ProjectileCapture(
    EntityReference Reference,
    RuntimeEntityHandle Handle,
    ProjectileIdentityComponent Identity,
    ProjectileKinematicsStateComponent Kinematics,
    ProjectileBehaviorStateComponent Behavior,
    ProjectileLifetimeStateComponent Lifetime)
  {
    internal bool HasSameState(ProjectileCapture other)
    {
      return Reference == other.Reference &&
        Handle == other.Handle &&
        Identity.Equals(other.Identity) &&
        Kinematics.Position == other.Kinematics.Position &&
        Kinematics.Velocity == other.Kinematics.Velocity &&
        Behavior.Ai0 == other.Behavior.Ai0 &&
        Behavior.Ai1 == other.Behavior.Ai1 &&
        Behavior.Ai2 == other.Behavior.Ai2 &&
        Behavior.LocalAi0 == other.Behavior.LocalAi0 &&
        Behavior.LocalAi1 == other.Behavior.LocalAi1 &&
        Behavior.LocalAi2 == other.Behavior.LocalAi2 &&
        Lifetime.Active == other.Lifetime.Active &&
        Lifetime.TimeLeft == other.Lifetime.TimeLeft &&
        Lifetime.EndReason == other.Lifetime.EndReason;
    }
  }

  internal readonly record struct SavedNpcValues(
    int? NetId,
    string? LegacyTypeName,
    bool IsTownNpc,
    string Name,
    float X,
    float Y,
    bool Homeless,
    Terraria.WorldStorage.TileCoordinate Home,
    int? Variation,
    bool HomelessDespawn);
}

internal sealed class RuntimeNpcRollbackEvidence
{
  internal RuntimeNpcRollbackEvidence(
    LoadedWorldSession session,
    RuntimeNpcStore npcStore,
    RuntimePlayerStore playerStore,
    RuntimeItemRegistry itemRegistry,
    RuntimeProjectileStore projectileStore,
    ProjectileStaticNpcImmunityRegistryComponent staticNpcImmunityRegistry,
    StaticNpcImmunityCapture staticImmunity,
    EntityRuntime runtime,
    int entityCount,
    IReadOnlyList<RuntimeWorldLoadRollbackVerification.NpcCapture> npcs,
    RuntimeWorldLoadRollbackVerification.PlayerCapture player,
    IReadOnlyList<RuntimeWorldLoadRollbackVerification.ProjectileCapture> projectiles,
    int projectileCount)
  {
    Session = session;
    NpcStore = npcStore;
    PlayerStore = playerStore;
    ItemRegistry = itemRegistry;
    ProjectileStore = projectileStore;
    StaticNpcImmunityRegistry = staticNpcImmunityRegistry;
    StaticImmunity = staticImmunity;
    Runtime = runtime;
    EntityCount = entityCount;
    NpcStates = npcs;
    Player = player;
    Projectiles = projectiles;
    ProjectileCount = projectileCount;
  }

  internal LoadedWorldSession Session { get; }
  internal RuntimeNpcStore NpcStore { get; }
  internal RuntimePlayerStore PlayerStore { get; }
  internal RuntimeItemRegistry ItemRegistry { get; }
  internal RuntimeProjectileStore ProjectileStore { get; }
  internal ProjectileStaticNpcImmunityRegistryComponent StaticNpcImmunityRegistry { get; }
  internal StaticNpcImmunityCapture StaticImmunity { get; }
  internal EntityRuntime Runtime { get; }
  internal int EntityCount { get; }
  internal IReadOnlyList<RuntimeWorldLoadRollbackVerification.NpcCapture> NpcStates { get; }
  internal RuntimeWorldLoadRollbackVerification.PlayerCapture Player { get; }
  internal IReadOnlyList<RuntimeWorldLoadRollbackVerification.ProjectileCapture> Projectiles { get; }
  internal int ProjectileCount { get; }

  internal bool HasSameOwnerProjection(RuntimeNpcRollbackEvidence other)
  {
    return ReferenceEquals(Session, other.Session) &&
      ReferenceEquals(NpcStore, other.NpcStore) &&
      ReferenceEquals(PlayerStore, other.PlayerStore) &&
      ReferenceEquals(ItemRegistry, other.ItemRegistry) &&
      ReferenceEquals(ProjectileStore, other.ProjectileStore) &&
      ReferenceEquals(StaticNpcImmunityRegistry, other.StaticNpcImmunityRegistry) &&
      StaticImmunity == other.StaticImmunity &&
      ReferenceEquals(Runtime, other.Runtime) &&
      EntityCount == other.EntityCount &&
      NpcStates.Count == other.NpcStates.Count &&
      NpcStates.Zip(other.NpcStates).All(pair => pair.First.HasSameState(pair.Second)) &&
      Player.HasSameState(other.Player) &&
      Projectiles.Count == other.Projectiles.Count &&
      Projectiles.Zip(other.Projectiles).All(pair => pair.First.HasSameState(pair.Second)) &&
      ProjectileCount == other.ProjectileCount;
  }

}

internal sealed record CandidateNpcRootCapture(
  RuntimeNpcEntity Entity,
  NpcInstanceId InstanceId,
  EntityReference Reference,
  RuntimeEntityHandle Handle);

internal readonly record struct StaticNpcImmunityCapture(
  int ProjectileType,
  int NpcSlot,
  uint ExpiryTick,
  bool IsImmuneAtStart,
  bool IsActiveBeforeExpiry,
  bool IsExpiredAtExpiry);

internal sealed class RuntimeNpcCandidateRollbackEvidence
{
  internal RuntimeNpcCandidateRollbackEvidence(
    LoadedWorldSession session,
    RuntimeNpcStore store,
    EntityRuntime runtime,
    ProjectileStaticNpcImmunityRegistryComponent staticNpcImmunityRegistry,
    StaticNpcImmunityCapture staticImmunity,
    IReadOnlyList<CandidateNpcRootCapture> roots)
  {
    Session = session;
    Store = store;
    Runtime = runtime;
    StaticNpcImmunityRegistry = staticNpcImmunityRegistry;
    StaticImmunity = staticImmunity;
    Roots = roots;
  }

  internal LoadedWorldSession Session { get; }
  internal RuntimeNpcStore Store { get; }
  internal EntityRuntime Runtime { get; }
  internal ProjectileStaticNpcImmunityRegistryComponent StaticNpcImmunityRegistry { get; }
  internal StaticNpcImmunityCapture StaticImmunity { get; }
  internal IReadOnlyList<CandidateNpcRootCapture> Roots { get; }
}

internal sealed record RuntimeWorldLoadRollbackReport(
  bool FailedCandidateDisposed,
  int FailedCandidateEntityCountAfterDispose,
  int FailedCandidateNpcRootCountBeforeCleanup,
  int FailedCandidateNpcStoreCountAfterCleanup,
  bool FailedCandidateNpcReferencesRejectedAfterCleanup,
  bool LateFinalizeFailureObserved,
  bool PreviousSessionRestored,
  bool PreviousNpcStoreObjectRestored,
  bool PreviousSelectedNpcStoreRestoredAfterFailure,
  bool PreviousOwnerStateUnchanged,
  int PreviousNpcCount,
  int PreviousPlayerCount,
  int PreviousItemCount,
  int PreviousProjectileCount,
  bool PreviousStaticNpcImmunityPreservedAfterFailure,
  bool FailedCandidateStaticNpcImmunityCleared,
  bool RetryStaticNpcImmunityRegistryIsIndependent,
  bool PreviousSessionDisposedAfterRetry,
  int PreviousEntityCountAfterRetry,
  bool RetryPublishedFreshSession,
  bool OldOwnerReferencesRejectedByRetry,
  bool LoadGateReleasedAfterFailureAndRetry,
  EntityRuntimeId PreviousRuntimeId,
  EntityRuntimeId FailedCandidateRuntimeId,
  EntityRuntimeId RetryRuntimeId);
