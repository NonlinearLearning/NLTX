using System;
using System.Collections.Generic;
using Terraria.Relationships;
using EntityEcs.Components;

namespace EntityEcs;

public sealed class EntityRuntime : IDisposable
{
  private readonly Dictionary<Type, IComponentStore> _componentStores = new();
  private readonly Stack<int> _availableIndices = new();
  private readonly List<uint> _generations = new();
  private readonly List<EntityRecord?> _entities = new();
  private readonly EntityIdentityRegistry _identityRegistry;
  private readonly int _ownerThreadId;
  private bool _isDisposed;

  public EntityRuntime(EntityIdentityRegistry? identityRegistry = null)
  {
    _identityRegistry = identityRegistry ?? new EntityIdentityRegistry();
    _ownerThreadId = Environment.CurrentManagedThreadId;
    if (!_identityRegistry.IsOwnedByCurrentThread)
    {
      throw new InvalidOperationException("The entity runtime and identity registry must share an owner thread.");
    }

    RuntimeId = new EntityRuntimeId(Guid.NewGuid());
  }

  public EntityRuntimeId RuntimeId { get; }

  public int EntityCount { get; private set; }

  public void EnsureCanDispose()
  {
    if (_isDisposed)
    {
      return;
    }

    if (_ownerThreadId != Environment.CurrentManagedThreadId)
    {
      throw new InvalidOperationException("Entity runtime access is restricted to its owner thread.");
    }

    foreach (EntityRecord? entity in _entities)
    {
      if (entity is { BorrowCount: not 0 })
      {
        throw new InvalidOperationException(
          "Entity runtime cannot be disposed while component access is borrowed.");
      }
    }
  }

  public void Dispose()
  {
    if (_isDisposed)
    {
      return;
    }

    EnsureCanDispose();

    foreach (EntityRecord? entity in _entities)
    {
      if (entity is null)
      {
        continue;
      }

      if (!_identityRegistry.Unregister(entity.Handle, entity.Uuid))
      {
        throw new InvalidOperationException("The entity identity registry does not match its runtime record.");
      }

      foreach (IComponentStore store in _componentStores.Values)
      {
        store.Remove(entity.Handle);
      }
    }

    _entities.Clear();
    _generations.Clear();
    _availableIndices.Clear();
    _componentStores.Clear();
    EntityCount = 0;
    _isDisposed = true;
  }

  public RuntimeEntityHandle CreateEntity()
  {
    VerifyAccess();
    (int localIndex, uint generation) = AllocateHandleParts();
    var handle = new RuntimeEntityHandle(RuntimeId, localIndex, generation);
    EntityUuid uuid = default;
    bool identityRegistered = false;

    try
    {
      uuid = _identityRegistry.Register(handle);
      identityRegistered = true;
      var entity = new EntityRecord(handle, uuid);
      ComponentStore<EntityIdentityComponent> identityStore = GetOrCreateStore<EntityIdentityComponent>();
      if (!identityStore.TryAttach(handle, new EntityIdentityComponent(uuid)))
      {
        throw new InvalidOperationException("A new entity already has an identity component.");
      }

      entity.Components.Add(typeof(EntityIdentityComponent));
      _entities[localIndex] = entity;
      EntityCount++;
      return handle;
    }
    catch
    {
      foreach (IComponentStore store in _componentStores.Values)
      {
        store.Remove(handle);
      }

      if (identityRegistered && !_identityRegistry.Unregister(handle, uuid))
      {
        throw new InvalidOperationException("Failed to release an incomplete entity identity registration.");
      }

      _entities[localIndex] = null;
      _availableIndices.Push(localIndex);
      throw;
    }
  }

  public bool TryPublishEntity(RuntimeEntityHandle handle)
  {
    VerifyAccess();
    if (!TryGetRecord(handle, out EntityRecord entity) ||
        entity.Status != EntityRuntimeStatus.Constructing ||
        entity.BorrowCount != 0)
    {
      return false;
    }

    entity.Status = EntityRuntimeStatus.Running;
    return true;
  }

  public bool TryGetStatus(RuntimeEntityHandle handle, out EntityRuntimeStatus status)
  {
    VerifyAccess();
    if (!TryGetRecord(handle, out EntityRecord entity))
    {
      status = default;
      return false;
    }

    status = entity.Status;
    return true;
  }

  public bool TryGetReference(
    RuntimeEntityHandle handle,
    EntityReferenceScope scope,
    out EntityReference reference)
  {
    VerifyAccess();
    if (scope == EntityReferenceScope.None ||
        !Enum.IsDefined(scope) ||
        !TryGetRecord(handle, out EntityRecord entity) ||
        entity.Status != EntityRuntimeStatus.Running)
    {
      reference = EntityReference.None;
      return false;
    }

    reference = new EntityReference(entity.Uuid, RuntimeId, scope);
    return true;
  }

  public bool TryResolve(EntityReference reference, out RuntimeEntityHandle handle)
  {
    VerifyAccess();
    if (reference.IsEmpty ||
        reference.RuntimeId != RuntimeId ||
        !_identityRegistry.TryResolve(reference, out RuntimeEntityHandle candidate) ||
        !TryGetRecord(candidate, out EntityRecord entity) ||
        entity.Status != EntityRuntimeStatus.Running)
    {
      handle = default;
      return false;
    }

    handle = candidate;
    return true;
  }

  public bool Has<TComponent>(RuntimeEntityHandle handle)
    where TComponent : notnull
  {
    VerifyAccess();
    return TryGetRunningRecord(handle, out EntityRecord entity) &&
      entity.Components.Contains(typeof(TComponent));
  }

  public bool TryAttach<TComponent>(RuntimeEntityHandle handle, TComponent component)
    where TComponent : notnull
  {
    VerifyAccess();
    ArgumentNullException.ThrowIfNull(component);
    if (!TryGetAccessibleRecord(handle, out EntityRecord entity) || entity.BorrowCount != 0)
    {
      return false;
    }

    ComponentStore<TComponent> store = GetOrCreateStore<TComponent>();
    if (!store.TryAttach(handle, component))
    {
      return false;
    }

    entity.Components.Add(typeof(TComponent));
    return true;
  }

  public bool TryReplace<TComponent>(RuntimeEntityHandle handle, TComponent component)
    where TComponent : notnull
  {
    VerifyAccess();
    ArgumentNullException.ThrowIfNull(component);
    if (!TryGetRunningRecord(handle, out EntityRecord entity) || entity.BorrowCount != 0)
    {
      return false;
    }

    return TryGetStore<TComponent>(out ComponentStore<TComponent> store) &&
      store.TryReplace(handle, component);
  }

  public bool TryReplace<TComponent, TProjection>(
    RuntimeEntityHandle handle,
    EntityComponentSnapshot<TProjection> expectedSnapshot,
    TComponent component)
    where TComponent : notnull
    where TProjection : struct
  {
    VerifyAccess();
    ArgumentNullException.ThrowIfNull(component);
    if (!TryGetRunningRecord(handle, out EntityRecord entity) || entity.BorrowCount != 0)
    {
      return false;
    }

    return TryGetStore<TComponent>(out ComponentStore<TComponent> store) &&
      store.TryReplace(handle, expectedSnapshot, component);
  }

  public bool TryDetach<TComponent>(RuntimeEntityHandle handle)
    where TComponent : notnull
  {
    VerifyAccess();
    if (!TryGetRunningRecord(handle, out EntityRecord entity) || entity.BorrowCount != 0 ||
        !entity.Components.Contains(typeof(TComponent)) ||
        !TryGetStore<TComponent>(out ComponentStore<TComponent> store) ||
        !store.Remove(handle))
    {
      return false;
    }

    entity.Components.Remove(typeof(TComponent));
    return true;
  }

  public bool TryEdit<TComponent>(
    RuntimeEntityHandle handle,
    EntityComponentEditor<TComponent> editor)
    where TComponent : notnull
  {
    VerifyAccess();
    ArgumentNullException.ThrowIfNull(editor);
    if (!TryGetRunningRecord(handle, out EntityRecord entity) || entity.BorrowCount != 0 ||
        !TryGetStore<TComponent>(out ComponentStore<TComponent> store))
    {
      return false;
    }

    entity.BorrowCount++;
    try
    {
      return store.TryEdit(handle, editor);
    }
    finally
    {
      entity.BorrowCount--;
    }
  }

  public bool TryInspect<TComponent>(
    RuntimeEntityHandle handle,
    EntityComponentInspector<TComponent> inspector)
    where TComponent : notnull
  {
    VerifyAccess();
    ArgumentNullException.ThrowIfNull(inspector);
    if (!TryGetRunningRecord(handle, out EntityRecord entity) || entity.BorrowCount != 0 ||
        !TryGetStore<TComponent>(out ComponentStore<TComponent> store))
    {
      return false;
    }

    entity.BorrowCount++;
    try
    {
      return store.TryInspect(handle, inspector);
    }
    finally
    {
      entity.BorrowCount--;
    }
  }

  public bool TryEditPair<TFirstComponent, TSecondComponent>(
    RuntimeEntityHandle handle,
    EntityComponentPairEditor<TFirstComponent, TSecondComponent> editor)
    where TFirstComponent : notnull
    where TSecondComponent : notnull
  {
    VerifyAccess();
    ArgumentNullException.ThrowIfNull(editor);
    if (typeof(TFirstComponent) == typeof(TSecondComponent) ||
        !TryGetRunningRecord(handle, out EntityRecord entity) || entity.BorrowCount != 0 ||
        !TryGetStore<TFirstComponent>(out ComponentStore<TFirstComponent> firstStore) ||
        !TryGetStore<TSecondComponent>(out ComponentStore<TSecondComponent> secondStore) ||
        !firstStore.Contains(handle) ||
        !secondStore.Contains(handle))
    {
      return false;
    }

    bool secondComponentEdited = false;
    entity.BorrowCount++;
    try
    {
      bool firstComponentEdited = firstStore.TryEdit(
        handle,
        (ref TFirstComponent firstComponent) =>
        {
          TFirstComponent firstValue = firstComponent;
          secondComponentEdited = secondStore.TryEdit(
            handle,
            (ref TSecondComponent secondComponent) =>
            {
              TSecondComponent secondValue = secondComponent;
              editor(ref firstValue, ref secondValue);
              secondComponent = secondValue;
            });
          firstComponent = firstValue;
        });
      return firstComponentEdited && secondComponentEdited;
    }
    finally
    {
      entity.BorrowCount--;
    }
  }

  public bool TryEditComponents<TFirstComponent, TSecondComponent, TThirdComponent>(
    RuntimeEntityHandle handle,
    EntityComponentTripleEditor<
      TFirstComponent,
      TSecondComponent,
      TThirdComponent> editor)
    where TFirstComponent : notnull
    where TSecondComponent : notnull
    where TThirdComponent : notnull
  {
    VerifyAccess();
    ArgumentNullException.ThrowIfNull(editor);
    if (typeof(TFirstComponent) == typeof(TSecondComponent) ||
        typeof(TFirstComponent) == typeof(TThirdComponent) ||
        typeof(TSecondComponent) == typeof(TThirdComponent) ||
        !TryGetRunningRecord(handle, out EntityRecord entity) || entity.BorrowCount != 0 ||
        !TryGetStore<TFirstComponent>(out ComponentStore<TFirstComponent> firstStore) ||
        !TryGetStore<TSecondComponent>(out ComponentStore<TSecondComponent> secondStore) ||
        !TryGetStore<TThirdComponent>(out ComponentStore<TThirdComponent> thirdStore) ||
        !firstStore.Contains(handle) ||
        !secondStore.Contains(handle) ||
        !thirdStore.Contains(handle))
    {
      return false;
    }

    entity.BorrowCount++;
    try
    {
      bool secondEdited = false;
      bool thirdEdited = false;
      bool firstEdited = firstStore.TryEdit(
        handle,
        (ref TFirstComponent firstComponent) =>
        {
          TFirstComponent firstValue = firstComponent;
          secondEdited = secondStore.TryEdit(
            handle,
            (ref TSecondComponent secondComponent) =>
            {
              TSecondComponent secondValue = secondComponent;
              thirdEdited = thirdStore.TryEdit(
                handle,
                (ref TThirdComponent thirdComponent) =>
                {
                  TThirdComponent thirdValue = thirdComponent;
                  editor(ref firstValue, ref secondValue, ref thirdValue);
                  thirdComponent = thirdValue;
                });
              secondComponent = secondValue;
            });
          firstComponent = firstValue;
        });
      return firstEdited && secondEdited && thirdEdited;
    }
    finally
    {
      entity.BorrowCount--;
    }
  }

  public bool TryEditComponents<
    TFirstComponent,
    TSecondComponent,
    TThirdComponent,
    TFourthComponent,
    TFifthComponent>(
    RuntimeEntityHandle handle,
    EntityComponentFiveEditor<
      TFirstComponent,
      TSecondComponent,
      TThirdComponent,
      TFourthComponent,
      TFifthComponent> editor)
    where TFirstComponent : notnull
    where TSecondComponent : notnull
    where TThirdComponent : notnull
    where TFourthComponent : notnull
    where TFifthComponent : notnull
  {
    VerifyAccess();
    ArgumentNullException.ThrowIfNull(editor);
    if (typeof(TFirstComponent) == typeof(TSecondComponent) ||
        typeof(TFirstComponent) == typeof(TThirdComponent) ||
        typeof(TFirstComponent) == typeof(TFourthComponent) ||
        typeof(TFirstComponent) == typeof(TFifthComponent) ||
        typeof(TSecondComponent) == typeof(TThirdComponent) ||
        typeof(TSecondComponent) == typeof(TFourthComponent) ||
        typeof(TSecondComponent) == typeof(TFifthComponent) ||
        typeof(TThirdComponent) == typeof(TFourthComponent) ||
        typeof(TThirdComponent) == typeof(TFifthComponent) ||
        typeof(TFourthComponent) == typeof(TFifthComponent) ||
        !TryGetRunningRecord(handle, out EntityRecord entity) || entity.BorrowCount != 0 ||
        !TryGetStore<TFirstComponent>(out ComponentStore<TFirstComponent> firstStore) ||
        !TryGetStore<TSecondComponent>(out ComponentStore<TSecondComponent> secondStore) ||
        !TryGetStore<TThirdComponent>(out ComponentStore<TThirdComponent> thirdStore) ||
        !TryGetStore<TFourthComponent>(out ComponentStore<TFourthComponent> fourthStore) ||
        !TryGetStore<TFifthComponent>(out ComponentStore<TFifthComponent> fifthStore) ||
        !firstStore.Contains(handle) || !secondStore.Contains(handle) ||
        !thirdStore.Contains(handle) || !fourthStore.Contains(handle) || !fifthStore.Contains(handle))
    {
      return false;
    }

    entity.BorrowCount++;
    try
    {
      bool secondEdited = false;
      bool thirdEdited = false;
      bool fourthEdited = false;
      bool fifthEdited = false;
      bool firstEdited = firstStore.TryEdit(
        handle,
        (ref TFirstComponent firstComponent) =>
        {
          TFirstComponent firstValue = firstComponent;
          secondEdited = secondStore.TryEdit(
            handle,
            (ref TSecondComponent secondComponent) =>
            {
              TSecondComponent secondValue = secondComponent;
              thirdEdited = thirdStore.TryEdit(
                handle,
                (ref TThirdComponent thirdComponent) =>
                {
                  TThirdComponent thirdValue = thirdComponent;
                  fourthEdited = fourthStore.TryEdit(
                    handle,
                    (ref TFourthComponent fourthComponent) =>
                    {
                      TFourthComponent fourthValue = fourthComponent;
                      fifthEdited = fifthStore.TryEdit(
                        handle,
                        (ref TFifthComponent fifthComponent) =>
                        {
                          TFifthComponent fifthValue = fifthComponent;
                          editor(
                            ref firstValue,
                            ref secondValue,
                            ref thirdValue,
                            ref fourthValue,
                            ref fifthValue);
                          fifthComponent = fifthValue;
                        });
                      fourthComponent = fourthValue;
                    });
                  thirdComponent = thirdValue;
                });
              secondComponent = secondValue;
            });
          firstComponent = firstValue;
        });
      return firstEdited && secondEdited && thirdEdited && fourthEdited && fifthEdited;
    }
    finally
    {
      entity.BorrowCount--;
    }
  }

  public bool TryEditComponents<
    TFirstComponent,
    TSecondComponent,
    TThirdComponent,
    TFourthComponent,
    TFifthComponent,
    TSixthComponent>(
    RuntimeEntityHandle handle,
    EntityComponentSixEditor<
      TFirstComponent,
      TSecondComponent,
      TThirdComponent,
      TFourthComponent,
      TFifthComponent,
      TSixthComponent> editor)
    where TFirstComponent : notnull
    where TSecondComponent : notnull
    where TThirdComponent : notnull
    where TFourthComponent : notnull
    where TFifthComponent : notnull
    where TSixthComponent : notnull
  {
    VerifyAccess();
    ArgumentNullException.ThrowIfNull(editor);
    if (typeof(TFirstComponent) == typeof(TSecondComponent) ||
        typeof(TFirstComponent) == typeof(TThirdComponent) ||
        typeof(TFirstComponent) == typeof(TFourthComponent) ||
        typeof(TFirstComponent) == typeof(TFifthComponent) ||
        typeof(TFirstComponent) == typeof(TSixthComponent) ||
        typeof(TSecondComponent) == typeof(TThirdComponent) ||
        typeof(TSecondComponent) == typeof(TFourthComponent) ||
        typeof(TSecondComponent) == typeof(TFifthComponent) ||
        typeof(TSecondComponent) == typeof(TSixthComponent) ||
        typeof(TThirdComponent) == typeof(TFourthComponent) ||
        typeof(TThirdComponent) == typeof(TFifthComponent) ||
        typeof(TThirdComponent) == typeof(TSixthComponent) ||
        typeof(TFourthComponent) == typeof(TFifthComponent) ||
        typeof(TFourthComponent) == typeof(TSixthComponent) ||
        typeof(TFifthComponent) == typeof(TSixthComponent) ||
        !TryGetRunningRecord(handle, out EntityRecord entity) || entity.BorrowCount != 0 ||
        !TryGetStore<TFirstComponent>(out ComponentStore<TFirstComponent> firstStore) ||
        !TryGetStore<TSecondComponent>(out ComponentStore<TSecondComponent> secondStore) ||
        !TryGetStore<TThirdComponent>(out ComponentStore<TThirdComponent> thirdStore) ||
        !TryGetStore<TFourthComponent>(out ComponentStore<TFourthComponent> fourthStore) ||
        !TryGetStore<TFifthComponent>(out ComponentStore<TFifthComponent> fifthStore) ||
        !TryGetStore<TSixthComponent>(out ComponentStore<TSixthComponent> sixthStore) ||
        !firstStore.Contains(handle) || !secondStore.Contains(handle) ||
        !thirdStore.Contains(handle) || !fourthStore.Contains(handle) ||
        !fifthStore.Contains(handle) || !sixthStore.Contains(handle))
    {
      return false;
    }

    entity.BorrowCount++;
    try
    {
      bool secondEdited = false;
      bool thirdEdited = false;
      bool fourthEdited = false;
      bool fifthEdited = false;
      bool sixthEdited = false;
      bool firstEdited = firstStore.TryEdit(
        handle,
        (ref TFirstComponent firstComponent) =>
        {
          TFirstComponent firstValue = firstComponent;
          secondEdited = secondStore.TryEdit(
            handle,
            (ref TSecondComponent secondComponent) =>
            {
              TSecondComponent secondValue = secondComponent;
              thirdEdited = thirdStore.TryEdit(
                handle,
                (ref TThirdComponent thirdComponent) =>
                {
                  TThirdComponent thirdValue = thirdComponent;
                  fourthEdited = fourthStore.TryEdit(
                    handle,
                    (ref TFourthComponent fourthComponent) =>
                    {
                      TFourthComponent fourthValue = fourthComponent;
                      fifthEdited = fifthStore.TryEdit(
                        handle,
                        (ref TFifthComponent fifthComponent) =>
                        {
                          TFifthComponent fifthValue = fifthComponent;
                          sixthEdited = sixthStore.TryEdit(
                            handle,
                            (ref TSixthComponent sixthComponent) =>
                            {
                              TSixthComponent sixthValue = sixthComponent;
                              editor(
                                ref firstValue,
                                ref secondValue,
                                ref thirdValue,
                                ref fourthValue,
                                ref fifthValue,
                                ref sixthValue);
                              sixthComponent = sixthValue;
                            });
                          fifthComponent = fifthValue;
                        });
                      fourthComponent = fourthValue;
                    });
                  thirdComponent = thirdValue;
                });
              secondComponent = secondValue;
            });
          firstComponent = firstValue;
        });
      return firstEdited && secondEdited && thirdEdited && fourthEdited && fifthEdited && sixthEdited;
    }
    finally
    {
      entity.BorrowCount--;
    }
  }

  public bool TryCapture<TComponent, TProjection>(
    RuntimeEntityHandle handle,
    Func<TComponent, TProjection> capture,
    out TProjection projection)
    where TComponent : notnull
    where TProjection : struct
  {
    VerifyAccess();
    ArgumentNullException.ThrowIfNull(capture);
    EntitySnapshotProjection<TProjection>.EnsureSafe();
    if (!TryGetRunningRecord(handle, out EntityRecord entity) || entity.BorrowCount != 0 ||
        !TryGetStore<TComponent>(out ComponentStore<TComponent> store))
    {
      projection = default;
      return false;
    }

    entity.BorrowCount++;
    try
    {
      return store.TryCapture(handle, capture, out projection);
    }
    finally
    {
      entity.BorrowCount--;
    }
  }

  public bool TryCaptureVersioned<TComponent, TProjection>(
    RuntimeEntityHandle handle,
    Func<TComponent, TProjection> capture,
    out EntityComponentSnapshot<TProjection> snapshot)
    where TComponent : notnull
    where TProjection : struct
  {
    VerifyAccess();
    ArgumentNullException.ThrowIfNull(capture);
    EntitySnapshotProjection<TProjection>.EnsureSafe();
    if (!TryGetRunningRecord(handle, out EntityRecord entity) || entity.BorrowCount != 0 ||
        !TryGetStore<TComponent>(out ComponentStore<TComponent> store))
    {
      snapshot = default;
      return false;
    }

    entity.BorrowCount++;
    try
    {
      if (!store.TryCaptureVersioned(
            handle,
            capture,
            out TProjection projection,
            out long attachmentRevision,
            out long dataRevision))
      {
        snapshot = default;
        return false;
      }

      snapshot = new EntityComponentSnapshot<TProjection>(
        handle,
        typeof(TComponent),
        attachmentRevision,
        dataRevision,
        projection);
      return true;
    }
    finally
    {
      entity.BorrowCount--;
    }
  }

  public RuntimeEntityHandle[] Match<TComponent>()
    where TComponent : notnull
  {
    VerifyAccess();
    return MatchRecords(entity => entity.Components.Contains(typeof(TComponent)));
  }

  public void Match<TComponent>(List<RuntimeEntityHandle> destination)
    where TComponent : notnull
  {
    VerifyAccess();
    ArgumentNullException.ThrowIfNull(destination);
    destination.Clear();
    foreach (EntityRecord? entity in _entities)
    {
      if (entity is { Status: EntityRuntimeStatus.Running } &&
          entity.Components.Contains(typeof(TComponent)))
      {
        destination.Add(entity.Handle);
      }
    }
  }

  public RuntimeEntityHandle[] Match<TFirstComponent, TSecondComponent>()
    where TFirstComponent : notnull
    where TSecondComponent : notnull
  {
    VerifyAccess();
    return MatchRecords(entity =>
      entity.Components.Contains(typeof(TFirstComponent)) &&
      entity.Components.Contains(typeof(TSecondComponent)));
  }

  public void Match<TFirstComponent, TSecondComponent>(
    List<RuntimeEntityHandle> destination)
    where TFirstComponent : notnull
    where TSecondComponent : notnull
  {
    VerifyAccess();
    ArgumentNullException.ThrowIfNull(destination);
    destination.Clear();
    foreach (EntityRecord? entity in _entities)
    {
      if (entity is { Status: EntityRuntimeStatus.Running } &&
          entity.Components.Contains(typeof(TFirstComponent)) &&
          entity.Components.Contains(typeof(TSecondComponent)))
      {
        destination.Add(entity.Handle);
      }
    }
  }

  public bool TryBeginTermination(RuntimeEntityHandle handle)
  {
    VerifyAccess();
    if (!TryGetRecord(handle, out EntityRecord entity) ||
        entity.Status != EntityRuntimeStatus.Running ||
        entity.BorrowCount != 0)
    {
      return false;
    }

    entity.Status = EntityRuntimeStatus.Terminating;
    return true;
  }

  public bool IsReadyForTermination(RuntimeEntityHandle handle)
  {
    VerifyAccess();
    return TryGetRecord(handle, out EntityRecord entity) &&
      (entity.Status is EntityRuntimeStatus.Running or EntityRuntimeStatus.Terminating) &&
      entity.BorrowCount == 0;
  }

  public bool TryRemoveEntity(RuntimeEntityHandle handle)
  {
    VerifyAccess();
    if (!TryGetRecord(handle, out EntityRecord entity) ||
        entity.Status == EntityRuntimeStatus.Running ||
        entity.BorrowCount != 0)
    {
      return false;
    }

    if (!_identityRegistry.Unregister(handle, entity.Uuid))
    {
      throw new InvalidOperationException("The entity identity registry does not match its runtime record.");
    }

    foreach (IComponentStore store in _componentStores.Values)
    {
      store.Remove(handle);
    }

    _entities[handle.LocalIndex] = null;
    _availableIndices.Push(handle.LocalIndex);
    EntityCount--;
    return true;
  }

  private RuntimeEntityHandle[] MatchRecords(Func<EntityRecord, bool> predicate)
  {
    var matches = new List<RuntimeEntityHandle>();
    foreach (EntityRecord? entity in _entities)
    {
      if (entity is { Status: EntityRuntimeStatus.Running } && predicate(entity))
      {
        matches.Add(entity.Handle);
      }
    }

    return matches.ToArray();
  }

  private bool TryGetAccessibleRecord(RuntimeEntityHandle handle, out EntityRecord entity)
  {
    return TryGetRecord(handle, out entity) && entity.Status != EntityRuntimeStatus.Terminating;
  }

  private bool TryGetRunningRecord(RuntimeEntityHandle handle, out EntityRecord entity)
  {
    return TryGetRecord(handle, out entity) && entity.Status == EntityRuntimeStatus.Running;
  }

  private bool TryGetRecord(RuntimeEntityHandle handle, out EntityRecord entity)
  {
    if (handle.RuntimeId == RuntimeId &&
        handle.IsAssigned &&
        handle.LocalIndex < _entities.Count &&
        _entities[handle.LocalIndex] is EntityRecord candidate &&
        candidate.Handle == handle)
    {
      entity = candidate;
      return true;
    }

    entity = null!;
    return false;
  }

  private ComponentStore<TComponent> GetOrCreateStore<TComponent>()
    where TComponent : notnull
  {
    if (_componentStores.TryGetValue(typeof(TComponent), out IComponentStore? store))
    {
      return (ComponentStore<TComponent>)store;
    }

    var newStore = new ComponentStore<TComponent>();
    _componentStores.Add(typeof(TComponent), newStore);
    return newStore;
  }

  private bool TryGetStore<TComponent>(out ComponentStore<TComponent> store)
    where TComponent : notnull
  {
    if (_componentStores.TryGetValue(typeof(TComponent), out IComponentStore? value))
    {
      store = (ComponentStore<TComponent>)value;
      return true;
    }

    store = null!;
    return false;
  }

  private (int LocalIndex, uint Generation) AllocateHandleParts()
  {
    while (_availableIndices.TryPop(out int localIndex))
    {
      uint previousGeneration = _generations[localIndex];
      if (previousGeneration == uint.MaxValue)
      {
        continue;
      }

      uint generation = previousGeneration + 1;
      _generations[localIndex] = generation;
      return (localIndex, generation);
    }

    int newIndex = _entities.Count;
    _entities.Add(null);
    _generations.Add(1);
    return (newIndex, 1);
  }

  private void VerifyAccess()
  {
    if (_isDisposed)
    {
      throw new ObjectDisposedException(nameof(EntityRuntime));
    }

    if (_ownerThreadId != Environment.CurrentManagedThreadId)
    {
      throw new InvalidOperationException("Entity runtime access is restricted to its owner thread.");
    }
  }

  private sealed class EntityRecord(RuntimeEntityHandle handle, EntityUuid uuid)
  {
    public RuntimeEntityHandle Handle { get; } = handle;

    public EntityUuid Uuid { get; } = uuid;

    public HashSet<Type> Components { get; } = new();

    public EntityRuntimeStatus Status { get; set; } = EntityRuntimeStatus.Constructing;

    public int BorrowCount { get; set; }
  }
}
