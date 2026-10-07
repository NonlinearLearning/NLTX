using System;
using System.Collections.Generic;
using System.Linq;
using EntityEcs;
using Terraria.Relationships;

namespace Terraria.WorldStorage;

public sealed class TileEntityStore : IDisposable
{
  private Dictionary<TileEntityId, TileEntityRecord> _byId = new();
  private Dictionary<TileCoordinate, TileEntityRecord> _byAnchor = new();
  private readonly EntityRuntime _runtime;
  private readonly TileEntityUpdateSchedule _updates;
  private int _nextId;
  private long _mutationRevision;
  private bool _isDisposed;

  public TileEntityStore(EntityRuntime runtime, TileEntityUpdateSchedule updates)
  {
    _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
    _updates = updates ?? throw new ArgumentNullException(nameof(updates));
  }

  public int Count
  {
    get
    {
      ThrowIfDisposed();
      return _byId.Count;
    }
  }

  public int NextId
  {
    get
    {
      ThrowIfDisposed();
      return _nextId;
    }
  }

  public long MutationRevision
  {
    get
    {
      ThrowIfDisposed();
      return _mutationRevision;
    }
  }

  public IReadOnlyList<TileEntityId> CreateScheduledIdSnapshot()
  {
    ThrowIfDisposed();
    return Array.AsReadOnly(_byId.Values
      .Where(static record => record.RequiresUpdates)
      .Select(static record => record.Id)
      .OrderBy(static id => id.Value)
      .ToArray());
  }

  public IReadOnlyList<TileEntitySnapshot> CreateSnapshot()
  {
    ThrowIfDisposed();
    var snapshots = new List<TileEntitySnapshot>(_byId.Count);
    foreach (TileEntityRecord record in _byId.Values)
    {
      if (!TryGetSnapshot(record.Id, out TileEntitySnapshot? snapshot) || snapshot is null)
      {
        throw new InvalidOperationException(
          $"TileEntity {record.Id.Value} could not be resolved for persistence capture.");
      }

      snapshots.Add(snapshot);
    }

    snapshots.Sort(static (left, right) => left.Id.Value.CompareTo(right.Id.Value));
    return Array.AsReadOnly(snapshots.ToArray());
  }

  public bool TryGetSnapshot(TileEntityId id, out TileEntitySnapshot? snapshot)
  {
    ThrowIfDisposed();
    snapshot = null;
    if (!_byId.TryGetValue(id, out TileEntityRecord? record) ||
        !TryGetRuntimeState(record, out TileEntityRuntimeState runtimeState))
    {
      return false;
    }

    TileEntitySnapshot payload = record.PersistencePayload;
    short npcIndex = payload.NpcIndex;
    byte logicCheck = payload.LogicCheck;
    bool logicOn = payload.LogicOn;
    if (runtimeState.Type.Value == 0)
    {
      npcIndex = runtimeState.NpcIndex;
    }
    else if (runtimeState.Type.Value == 2)
    {
      logicCheck = runtimeState.LogicCheck;
      logicOn = runtimeState.LogicOn;
    }

    snapshot = new TileEntitySnapshot(
      runtimeState.Id,
      runtimeState.Type,
      runtimeState.Anchor,
      payload.Items,
      npcIndex,
      logicCheck,
      logicOn,
      payload.Pose);
    return true;
  }

  public bool TryGetEntityReference(TileEntityId id, out EntityReference reference)
  {
    ThrowIfDisposed();
    reference = EntityReference.None;
    if (!_byId.TryGetValue(id, out TileEntityRecord? record) ||
        !IsCurrentBinding(record) ||
        !_runtime.TryGetReference(
          record.RuntimeHandle,
          EntityReferenceScope.Any,
          out reference))
    {
      reference = EntityReference.None;
      return false;
    }

    return true;
  }

  public bool TryGetEntityReferenceByAnchor(
    TileCoordinate anchor,
    out EntityReference reference)
  {
    ThrowIfDisposed();
    reference = EntityReference.None;
    if (!_byAnchor.TryGetValue(anchor, out TileEntityRecord? record) ||
        !_byId.TryGetValue(record.Id, out TileEntityRecord? idRecord) ||
        !ReferenceEquals(record, idRecord))
    {
      return false;
    }

    return TryGetEntityReference(record.Id, out reference);
  }

  public bool TryGetRuntimeState(TileEntityId id, out TileEntityRuntimeState runtimeState)
  {
    ThrowIfDisposed();
    if (_byId.TryGetValue(id, out TileEntityRecord? record))
    {
      return TryGetRuntimeState(record, out runtimeState);
    }

    runtimeState = default;
    return false;
  }

  public bool TryGetLogicSensorCountedData(TileEntityId id, out int countedData)
  {
    ThrowIfDisposed();
    if (TryGetRuntimeState(id, out TileEntityRuntimeState state) && state.Type.Value == 2)
    {
      countedData = state.LogicSensorCountedData;
      return true;
    }

    countedData = 0;
    return false;
  }

  public bool CommitLogicSensorState(TileEntityId id, bool isOn, int countedData)
  {
    ThrowIfDisposed();
    ArgumentOutOfRangeException.ThrowIfNegative(countedData);
    if (!_byId.TryGetValue(id, out TileEntityRecord? record) || record.Type.Value != 2)
    {
      return false;
    }

    bool stateChanged = false;
    bool countedDataChanged = false;
    bool edited = _runtime.TryEdit<TileEntityLogicSensorComponent>(
      record.RuntimeHandle,
      (ref TileEntityLogicSensorComponent component) =>
      {
        stateChanged = component.IsOn != isOn;
        countedDataChanged = component.CountedData != countedData;
        component.IsOn = isOn;
        component.CountedData = countedData;
      });
    if (!edited || (!stateChanged && !countedDataChanged))
    {
      return false;
    }

    _mutationRevision++;
    return stateChanged;
  }

  public bool CommitTrainingDummyNpcIndex(TileEntityId id, short npcIndex)
  {
    ThrowIfDisposed();
    if (!_byId.TryGetValue(id, out TileEntityRecord? record) || record.Type.Value != 0)
    {
      return false;
    }

    bool changed = false;
    bool edited = _runtime.TryEdit<TileEntityTrainingDummyComponent>(
      record.RuntimeHandle,
      (ref TileEntityTrainingDummyComponent component) =>
      {
        changed = component.NpcIndex != npcIndex;
        component.NpcIndex = npcIndex;
      });
    if (!edited || !changed)
    {
      return false;
    }

    _mutationRevision++;
    return true;
  }

  public bool Remove(TileEntityId id)
  {
    ThrowIfDisposed();
    _ = _updates.Count;
    if (!_byId.TryGetValue(id, out TileEntityRecord? record))
    {
      return false;
    }

    if (!_runtime.TryBeginTermination(record.RuntimeHandle))
    {
      return false;
    }

    _updates.Unschedule(id);
    if (!_runtime.TryRemoveEntity(record.RuntimeHandle))
    {
      throw new InvalidOperationException(
        "A terminating TileEntity could not be removed from its runtime.");
    }

    _byId.Remove(id);
    _byAnchor.Remove(record.Anchor);
    _mutationRevision++;
    return true;
  }

  internal void Replace(IReadOnlyList<TileEntitySnapshot> prepared, int nextId)
  {
    ThrowIfDisposed();
    ReplaceCore(prepared, nextId, preserveLogicSensorCountedData: false);
  }

  public void CommitRuntimeSnapshot(TileEntityStoreSnapshot snapshot)
  {
    ThrowIfDisposed();
    ArgumentNullException.ThrowIfNull(snapshot);
    ReplaceCore(snapshot.Entities, snapshot.NextId, preserveLogicSensorCountedData: true);
  }

  public void Dispose()
  {
    if (_isDisposed)
    {
      return;
    }

    _ = _updates.Count;
    ReplaceCore(
      Array.Empty<TileEntitySnapshot>(),
      nextId: 0,
      preserveLogicSensorCountedData: false);
    _isDisposed = true;
  }

  private void ReplaceCore(
    IReadOnlyList<TileEntitySnapshot> prepared,
    int nextId,
    bool preserveLogicSensorCountedData)
  {
    ThrowIfDisposed();
    ArgumentNullException.ThrowIfNull(prepared);
    ArgumentOutOfRangeException.ThrowIfNegative(nextId);
    _ = _updates.Count;

    var seenIds = new HashSet<TileEntityId>();
    var seenAnchors = new HashSet<TileCoordinate>();
    foreach (TileEntitySnapshot entity in prepared)
    {
      ArgumentNullException.ThrowIfNull(entity);
      if (entity.Id.Value < 0 || entity.Id.Value >= nextId || !seenIds.Add(entity.Id))
      {
        throw new InvalidDataException("TileEntity IDs are invalid or duplicated.");
      }

      if (!seenAnchors.Add(entity.Anchor))
      {
        throw new InvalidDataException("TileEntity anchors are duplicated.");
      }
    }

    var nextById = new Dictionary<TileEntityId, TileEntityRecord>(prepared.Count);
    var nextByAnchor = new Dictionary<TileCoordinate, TileEntityRecord>(prepared.Count);
    var retainedHandles = new HashSet<RuntimeEntityHandle>();
    var createdHandles = new List<RuntimeEntityHandle>();
    var stateUpdates = new List<TileEntityRuntimeUpdate>();
    int appliedUpdates = 0;

    try
    {
      foreach (TileEntitySnapshot entity in prepared)
      {
        if (_byId.TryGetValue(entity.Id, out TileEntityRecord? previous) &&
            previous.Type == entity.Type && previous.Anchor == entity.Anchor)
        {
          if (!TryGetRuntimeState(previous, out TileEntityRuntimeState previousState))
          {
            throw new InvalidOperationException(
              $"TileEntity {entity.Id.Value} could not be resolved for replacement.");
          }

          int countedData = preserveLogicSensorCountedData && entity.Type.Value == 2
            ? previousState.LogicSensorCountedData
            : 0;
          var updatedRecord = new TileEntityRecord(
            entity.Id,
            entity.Type,
            entity.Anchor,
            previous.RuntimeHandle,
            entity);
          nextById.Add(entity.Id, updatedRecord);
          retainedHandles.Add(previous.RuntimeHandle);
          stateUpdates.Add(new TileEntityRuntimeUpdate(
            previous.RuntimeHandle,
            entity,
            previousState,
            countedData));
          nextByAnchor.Add(entity.Anchor, updatedRecord);
          continue;
        }

        RuntimeEntityHandle handle = CreateRuntimeEntity(entity);
        createdHandles.Add(handle);
        var record = new TileEntityRecord(entity.Id, entity.Type, entity.Anchor, handle, entity);
        nextById.Add(entity.Id, record);
        nextByAnchor.Add(entity.Anchor, record);
      }

      foreach (TileEntityRecord previous in _byId.Values)
      {
        if (!retainedHandles.Contains(previous.RuntimeHandle) &&
            !CanRetire(previous.RuntimeHandle))
        {
          throw new InvalidOperationException(
          $"TileEntity {previous.Id.Value} cannot be replaced while borrowed.");
        }
      }

      foreach (TileEntityRuntimeUpdate update in stateUpdates)
      {
        if (!TryApplySnapshotToRuntime(update.Handle, update.Snapshot, update.CountedData))
        {
          throw new InvalidOperationException(
            "A TileEntity runtime capability could not accept its prepared persistence data.");
        }

        appliedUpdates++;
      }

      foreach (RuntimeEntityHandle handle in createdHandles)
      {
        if (!_runtime.TryPublishEntity(handle))
        {
          throw new InvalidOperationException(
            "A prepared TileEntity runtime root could not be published.");
        }
      }
    }
    catch
    {
      try
      {
        RestoreRuntimeUpdates(stateUpdates, appliedUpdates);
      }
      finally
      {
        foreach (RuntimeEntityHandle handle in createdHandles)
        {
          if (!RetireRuntimeEntity(handle))
          {
            throw new InvalidOperationException(
              "A TileEntity created during a failed replacement could not be cleaned up.");
          }
        }
      }

      throw;
    }

    foreach (TileEntityRecord previous in _byId.Values)
    {
      if (!retainedHandles.Contains(previous.RuntimeHandle) &&
          !RetireRuntimeEntity(previous.RuntimeHandle))
      {
        throw new InvalidOperationException(
          $"TileEntity {previous.Id.Value} could not be retired after replacement preflight.");
      }
    }

    _byId = nextById;
    _byAnchor = nextByAnchor;
    _nextId = nextId;
    _updates.Replace(CreateScheduledIdSnapshot());
    _mutationRevision++;
  }

  private RuntimeEntityHandle CreateRuntimeEntity(TileEntitySnapshot entity)
  {
    RuntimeEntityHandle handle = _runtime.CreateEntity();
    try
    {
      if (!_runtime.TryAttach(
            handle,
            new TileEntityBindingComponent(entity.Id, entity.Type, entity.Anchor)))
      {
        throw new InvalidOperationException(
          "A TileEntity binding component could not be attached.");
      }

      if (entity.Type.Value == 0 &&
          !_runtime.TryAttach(handle, new TileEntityTrainingDummyComponent(entity.NpcIndex)))
      {
        throw new InvalidOperationException("A Training Dummy capability could not be attached.");
      }

      if (entity.Type.Value == 2 &&
          !_runtime.TryAttach(
            handle,
            new TileEntityLogicSensorComponent(entity.LogicCheck, entity.LogicOn, 0)))
      {
        throw new InvalidOperationException("A Logic Sensor capability could not be attached.");
      }

      return handle;
    }
    catch
    {
      if (!_runtime.TryRemoveEntity(handle))
      {
        throw new InvalidOperationException("An incomplete TileEntity root could not be removed.");
      }

      throw;
    }
  }

  private bool TryGetRuntimeState(
    TileEntityRecord record,
    out TileEntityRuntimeState runtimeState)
  {
    TileEntityBindingComponent? binding = null;
    if (!_runtime.TryInspect(
          record.RuntimeHandle,
          (in TileEntityBindingComponent component) => binding = component) ||
        binding is null || binding.Id != record.Id || binding.Type != record.Type ||
        binding.Anchor != record.Anchor)
    {
      runtimeState = default;
      return false;
    }

    TileEntitySnapshot payload = record.PersistencePayload;
    short npcIndex = payload.NpcIndex;
    byte logicCheck = payload.LogicCheck;
    bool logicOn = payload.LogicOn;
    int countedData = 0;
    if (record.Type.Value == 0)
    {
      TileEntityTrainingDummyComponent? trainingDummy = null;
      if (!_runtime.TryInspect(
            record.RuntimeHandle,
            (in TileEntityTrainingDummyComponent component) => trainingDummy = component) ||
          trainingDummy is null)
      {
        runtimeState = default;
        return false;
      }

      npcIndex = trainingDummy.Value.NpcIndex;
    }
    else if (record.Type.Value == 2)
    {
      TileEntityLogicSensorComponent? logicSensor = null;
      if (!_runtime.TryInspect(
            record.RuntimeHandle,
            (in TileEntityLogicSensorComponent component) => logicSensor = component) ||
          logicSensor is null)
      {
        runtimeState = default;
        return false;
      }

      logicCheck = logicSensor.Value.LogicCheck;
      logicOn = logicSensor.Value.IsOn;
      countedData = logicSensor.Value.CountedData;
    }

    if (!_runtime.TryGetReference(
          record.RuntimeHandle,
          EntityReferenceScope.Any,
          out EntityReference runtimeReference))
    {
      runtimeState = default;
      return false;
    }

    runtimeState = new TileEntityRuntimeState(
      binding.Id,
      binding.Type,
      binding.Anchor,
      runtimeReference,
      npcIndex,
      logicCheck,
      logicOn,
      countedData);
    return true;
  }

  private bool IsCurrentBinding(TileEntityRecord record)
  {
    TileEntityBindingComponent? binding = null;
    return _runtime.TryInspect(
        record.RuntimeHandle,
        (in TileEntityBindingComponent component) => binding = component) &&
      binding is not null && binding.Id == record.Id && binding.Type == record.Type &&
      binding.Anchor == record.Anchor;
  }

  private bool TryApplySnapshotToRuntime(
    RuntimeEntityHandle handle,
    TileEntitySnapshot snapshot,
    int countedData)
  {
    if (snapshot.Type.Value == 0)
    {
      return _runtime.TryEdit<TileEntityTrainingDummyComponent>(
        handle,
        (ref TileEntityTrainingDummyComponent component) =>
          component.NpcIndex = snapshot.NpcIndex);
    }

    if (snapshot.Type.Value == 2)
    {
      return _runtime.TryEdit<TileEntityLogicSensorComponent>(
        handle,
        (ref TileEntityLogicSensorComponent component) =>
        {
          component.LogicCheck = snapshot.LogicCheck;
          component.IsOn = snapshot.LogicOn;
          component.CountedData = countedData;
        });
    }

    return true;
  }

  private void RestoreRuntimeUpdates(IReadOnlyList<TileEntityRuntimeUpdate> updates, int count)
  {
    for (int index = count - 1; index >= 0; index--)
    {
      TileEntityRuntimeUpdate update = updates[index];
      bool restored = update.PreviousState.Type.Value switch
      {
        0 => _runtime.TryEdit<TileEntityTrainingDummyComponent>(
          update.Handle,
          (ref TileEntityTrainingDummyComponent component) =>
            component.NpcIndex = update.PreviousState.NpcIndex),
        2 => _runtime.TryEdit<TileEntityLogicSensorComponent>(
          update.Handle,
          (ref TileEntityLogicSensorComponent component) =>
          {
            component.LogicCheck = update.PreviousState.LogicCheck;
            component.IsOn = update.PreviousState.LogicOn;
            component.CountedData = update.PreviousState.LogicSensorCountedData;
          }),
        _ => true
      };
      if (!restored)
      {
        throw new InvalidOperationException(
          "A TileEntity runtime capability could not be restored.");
      }
    }
  }

  private bool CanRetire(RuntimeEntityHandle handle)
  {
    return _runtime.TryInspect(handle, static (in TileEntityBindingComponent _) => { });
  }

  private bool RetireRuntimeEntity(RuntimeEntityHandle handle)
  {
    if (!_runtime.TryGetStatus(handle, out EntityRuntimeStatus status))
    {
      return true;
    }

    if (status == EntityRuntimeStatus.Running && !_runtime.TryBeginTermination(handle))
    {
      return false;
    }

    return _runtime.TryRemoveEntity(handle);
  }

  private void ThrowIfDisposed()
  {
    ObjectDisposedException.ThrowIf(_isDisposed, this);
  }

  private readonly record struct TileEntityRuntimeUpdate(
    RuntimeEntityHandle Handle,
    TileEntitySnapshot Snapshot,
    TileEntityRuntimeState PreviousState,
    int CountedData);
}
