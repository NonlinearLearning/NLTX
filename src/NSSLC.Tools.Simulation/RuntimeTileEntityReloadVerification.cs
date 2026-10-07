using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.NonAuthoritative.Persistence;
using Terraria.Relationships;
using Terraria.WorldInteraction.TileEntities;
using Terraria.WorldStorage;

namespace Terraria.NonAuthoritative.SimulationHost;

internal static class RuntimeTileEntityReloadVerification
{
  internal readonly record struct CapturedEntity(
    TileEntityId Id,
    TileEntityTypeId Type,
    TileCoordinate Anchor,
    EntityReference Reference,
    byte LogicCheck,
    bool LogicOn);

  internal sealed record CapturedState(
    EntityRuntimeId PreviousWorldRuntimeId,
    IReadOnlyList<CapturedEntity> Entities,
    IReadOnlyList<TileEntityId> ScheduledIds)
  {
    internal CapturedEntity GetEntity(byte type)
    {
      return Entities.Single(entity => entity.Type.Value == type);
    }
  }

  private const byte TrainingDummyType = 0;
  private const byte LogicSensorType = 2;

  public static CapturedState Capture(LoadedWorldSession previousSession)
  {
    ArgumentNullException.ThrowIfNull(previousSession);

    TileEntityStore store = previousSession.Storage.TileEntities;
    IReadOnlyList<TileEntitySnapshot> snapshots = store.CreateSnapshot();
    if (snapshots.Count != 2)
    {
      throw new InvalidOperationException(
        $"The TileEntity reload fixture requires two records, but captured {snapshots.Count}.");
    }

    var capturedEntities = new List<CapturedEntity>(snapshots.Count);
    int trainingDummyCount = 0;
    int logicSensorCount = 0;
    foreach (TileEntitySnapshot snapshot in snapshots)
    {
      if (snapshot.Type.Value == TrainingDummyType)
      {
        trainingDummyCount++;
      }
      else if (snapshot.Type.Value == LogicSensorType)
      {
        logicSensorCount++;
        if (snapshot.LogicCheck > (byte)LogicCheckType.Liquid)
        {
          throw new NotSupportedException(
            $"Logic Sensor check {snapshot.LogicCheck} is not supported by this simulation host.");
        }
      }
      else
      {
        throw new NotSupportedException(
          $"TileEntity type {snapshot.Type.Value} is outside the verifier's supported scope.");
      }

      if (!store.TryGetRuntimeState(snapshot.Id, out TileEntityRuntimeState state) ||
          state.Type != snapshot.Type || state.Anchor != snapshot.Anchor ||
          !store.TryGetEntityReference(snapshot.Id, out EntityReference reference) ||
          reference != state.RuntimeReference ||
          !previousSession.EntityRuntime.TryResolve(reference, out _))
      {
        throw new InvalidOperationException(
          $"TileEntity {snapshot.Id.Value} had no resolvable root at capture time.");
      }

      capturedEntities.Add(new CapturedEntity(
        snapshot.Id,
        snapshot.Type,
        snapshot.Anchor,
        reference,
        state.LogicCheck,
        state.LogicOn));
    }

    if (trainingDummyCount != 1 || logicSensorCount != 1)
    {
      throw new InvalidOperationException(
        "The TileEntity reload fixture requires exactly one Training Dummy and one Logic Sensor.");
    }

    IReadOnlyList<TileEntityId> scheduledIds = store.CreateScheduledIdSnapshot();
    var recordIds = new HashSet<TileEntityId>(capturedEntities.Select(static entity => entity.Id));
    if (scheduledIds.Count != recordIds.Count ||
        scheduledIds.Any(id => !recordIds.Contains(id)) ||
        previousSession.Storage.TileEntityUpdates.Count != scheduledIds.Count ||
        scheduledIds.Any(id => !previousSession.Storage.TileEntityUpdates.IsScheduled(id)))
    {
      throw new InvalidOperationException(
        "The previous session's TileEntity update schedule did not match its supported records.");
    }

    return new CapturedState(
      previousSession.WorldRuntimeId,
      Array.AsReadOnly(capturedEntities.ToArray()),
      Array.AsReadOnly(scheduledIds.ToArray()));
  }

  public static RuntimeTileEntityReloadReport Evaluate(
    CapturedState captured,
    LoadedWorldSession newSession,
    RuntimeNpcStore runtimeNpcs,
    int updatePassCount)
  {
    ArgumentNullException.ThrowIfNull(captured);
    ArgumentNullException.ThrowIfNull(newSession);
    ArgumentNullException.ThrowIfNull(runtimeNpcs);
    ArgumentOutOfRangeException.ThrowIfNegative(updatePassCount);

    if (newSession.WorldRuntimeId == captured.PreviousWorldRuntimeId)
    {
      throw new InvalidOperationException(
        "The TileEntity reload target reused the previous session's runtime identity.");
    }

    TileEntityStore store = newSession.Storage.TileEntities;
    IReadOnlyList<TileEntitySnapshot> reloadedSnapshots = store.CreateSnapshot();
    bool recordsMatch = RecordsMatch(captured.Entities, reloadedSnapshots);
    if (!recordsMatch)
    {
      throw new InvalidOperationException(
        "Reloaded TileEntity IDs, supported types, or anchors differed from the captured records.");
    }

    bool runtimeReferencesChanged = true;
    bool previousReferencesRejected = true;
    bool reloadedReferencesResolve = true;
    foreach (CapturedEntity entity in captured.Entities)
    {
      if (!store.TryGetEntityReference(entity.Id, out EntityReference reloadedReference) ||
          !store.TryGetRuntimeState(entity.Id, out TileEntityRuntimeState state))
      {
        runtimeReferencesChanged = false;
        reloadedReferencesResolve = false;
        previousReferencesRejected = false;
        continue;
      }

      runtimeReferencesChanged &=
        reloadedReference.EntityId != entity.Reference.EntityId &&
        reloadedReference.RuntimeId != entity.Reference.RuntimeId;
      previousReferencesRejected &=
        !newSession.EntityRuntime.TryResolve(entity.Reference, out _);
      reloadedReferencesResolve &=
        state.RuntimeReference == reloadedReference &&
        newSession.EntityRuntime.TryResolve(reloadedReference, out _);
    }

    IReadOnlyList<TileEntityId> reloadedScheduledIds = store.CreateScheduledIdSnapshot();
    bool scheduleMatchesRecords =
      captured.ScheduledIds.Count == reloadedScheduledIds.Count &&
      captured.ScheduledIds.ToHashSet().SetEquals(reloadedScheduledIds) &&
      newSession.Storage.TileEntityUpdates.Count == reloadedScheduledIds.Count &&
      reloadedScheduledIds.All(newSession.Storage.TileEntityUpdates.IsScheduled);

    CapturedEntity trainingDummy = captured.GetEntity(TrainingDummyType);
    bool trainingDummyBindingValid = false;
    if (store.TryGetRuntimeState(trainingDummy.Id, out TileEntityRuntimeState dummyState) &&
        dummyState.Type.Value == TrainingDummyType &&
        dummyState.NpcIndex >= 0 &&
        runtimeNpcs.TryCaptureTrainingDummyBinding(
          dummyState.NpcIndex,
          dummyState.Anchor,
          out RuntimeNpcTrainingDummyBinding binding))
    {
      trainingDummyBindingValid =
        binding.Slot.Value == dummyState.NpcIndex &&
        binding.Anchor == dummyState.Anchor &&
        binding.Reference.Scope == EntityReferenceScope.Npc &&
        binding.Reference.RuntimeId == newSession.WorldRuntimeId &&
        newSession.EntityRuntime.TryResolve(
          binding.Reference,
          out RuntimeEntityHandle resolvedHandle) &&
        resolvedHandle == binding.RuntimeHandle;
    }

    CapturedEntity logicSensor = captured.GetEntity(LogicSensorType);
    if (!store.TryGetRuntimeState(logicSensor.Id, out TileEntityRuntimeState sensorState) ||
        sensorState.Type.Value != LogicSensorType)
    {
      throw new InvalidOperationException(
        "The reloaded Logic Sensor did not resolve from its TileEntity root.");
    }

    byte logicSensorCheck = sensorState.LogicCheck;
    bool logicSensorOn = sensorState.LogicOn;
    bool logicSensorCheckMatches = logicSensorCheck == logicSensor.LogicCheck;
    bool logicSensorStateMatches = logicSensorOn == logicSensor.LogicOn;
    bool updatePassCountMatches = updatePassCount == captured.ScheduledIds.Count &&
      updatePassCount == reloadedScheduledIds.Count;

    Require(runtimeReferencesChanged,
      "The reloaded TileEntity runtime references did not all change.");
    Require(previousReferencesRejected,
      "The new session accepted a previous-session TileEntity reference.");
    Require(reloadedReferencesResolve,
      "A reloaded TileEntity reference did not resolve to its runtime root.");
    Require(scheduleMatchesRecords,
      "The reloaded TileEntity update schedule did not match the captured records.");
    Require(trainingDummyBindingValid,
      "The reloaded Training Dummy did not have a valid NPC runtime binding.");
    Require(logicSensorCheckMatches,
      $"Logic Sensor check changed from {logicSensor.LogicCheck} to {logicSensorCheck}.");
    Require(logicSensorStateMatches,
      $"The reloaded Logic Sensor state changed from {logicSensor.LogicOn} to {logicSensorOn}.");
    Require(updatePassCountMatches,
      $"The TileEntity phase ran {updatePassCount} updates for " +
      $"{reloadedScheduledIds.Count} scheduled records.");

    return new RuntimeTileEntityReloadReport(
      runtimeReferencesChanged,
      previousReferencesRejected,
      reloadedReferencesResolve,
      scheduleMatchesRecords,
      trainingDummyBindingValid,
      logicSensorCheck,
      logicSensorOn,
      updatePassCount);
  }

  private static bool RecordsMatch(
    IReadOnlyList<CapturedEntity> capturedEntities,
    IReadOnlyList<TileEntitySnapshot> reloadedSnapshots)
  {
    if (capturedEntities.Count != reloadedSnapshots.Count)
    {
      return false;
    }

    var reloadedById = reloadedSnapshots.ToDictionary(static snapshot => snapshot.Id);
    return capturedEntities.All(entity =>
      reloadedById.TryGetValue(entity.Id, out TileEntitySnapshot? snapshot) &&
      snapshot.Type == entity.Type &&
      snapshot.Anchor == entity.Anchor);
  }

  private static void Require(bool condition, string message)
  {
    if (!condition)
    {
      throw new InvalidOperationException(message);
    }
  }

}

internal sealed record RuntimeTileEntityReloadReport(
  bool RuntimeReferencesChanged,
  bool PreviousReferencesRejected,
  bool ReloadedReferencesResolve,
  bool ScheduleMatchesRecords,
  bool TrainingDummyBindingValid,
  byte LogicSensorCheck,
  bool LogicSensorOn,
  int UpdatePassCount);
