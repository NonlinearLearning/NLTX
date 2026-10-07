using EntityEcs;
using EntityEcs.Components;
using System.Runtime.CompilerServices;
using Terraria.Content;
using Terraria.Items;
using Terraria.Player;
using Terraria.Relationships;
using ItemEntityRef = Terraria.Relationships.ItemEntityRef;

[assembly: InternalsVisibleTo("Terraria.PlayerItemSpaceVerification")]

namespace Terraria.NonAuthoritative.SimulationHost;

internal sealed class RuntimeItemRegistry
{
  private const string TerrariaContentDomain = "Terraria";

  private readonly ContentCatalog _catalog;
  private readonly EntityRuntime _entityRuntime;

  internal EntityRuntime EntityRuntime => _entityRuntime;

  internal bool IsBoundTo(EntityRuntime entityRuntime) =>
    ReferenceEquals(_entityRuntime, entityRuntime);

  public RuntimeItemRegistry(ContentCatalog catalog, EntityRuntime entityRuntime)
  {
    _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
    _entityRuntime = entityRuntime ?? throw new ArgumentNullException(nameof(entityRuntime));
  }

  public PlayerInventoryItemSnapshot Create(int typeId, int stack)
  {
    ItemDefinition definition = GetDefinition(typeId);
    ValidateStack(stack, definition);
    return CreateEntity(definition, stack, worldDrop: null);
  }

  public PlayerInventoryItemSnapshot CreateMaximumStack(int typeId)
  {
    ItemDefinition definition = GetDefinition(typeId);
    return CreateEntity(definition, definition.Stack.MaxStack, worldDrop: null);
  }

  internal bool TryCreateSplitItem(
    PlayerInventoryItemSnapshot source,
    int splitStack,
    out PlayerInventoryItemSnapshot splitItem)
  {
    if (source.IsEmpty ||
        !source.MutationRevision.IsAssigned ||
        splitStack <= 0 ||
        splitStack >= source.Stack ||
        !TryResolve(source.Entity, out RuntimeEntityHandle handle) ||
        !TryCaptureItem(handle, source.Entity, out ItemInstanceProjection instance,
          out ItemStackComponent stack, out ItemMutationRevision mutationRevision) ||
        mutationRevision != source.MutationRevision ||
        stack.Quantity != source.Stack ||
        !_catalog.Items.TryGet(source.TypeId, out ItemDefinition definition) ||
        instance.DefinitionRef.ContentId.TypeId != source.TypeId ||
        definition.Stack.UniqueStack ||
        definition.Stack.MaxStack <= 1)
    {
      splitItem = default;
      return false;
    }

    ValidateStack(splitStack, definition);
    splitItem = CreateEntity(
      definition,
      splitStack,
      worldDrop: null,
      initialInstance: instance);
    return true;
  }

  public PlayerInventoryItemSnapshot CreateWorldDrop(
    int typeId,
    int stack,
    float x,
    float y,
    float velocityX,
    float velocityY,
    ColliderComponent collider,
    long spawnedAtTick,
    int timeLeftTicks,
    ReplicationId replicationId,
    int prefixId = 0)
  {
    ItemDefinition definition = GetDefinition(typeId);
    ValidateStack(stack, definition);
    ArgumentOutOfRangeException.ThrowIfNegative(spawnedAtTick);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(timeLeftTicks);
    if (!replicationId.IsAssigned)
    {
      throw new ArgumentException(
        "A replicated world item requires an assigned network identity.",
        nameof(replicationId));
    }

    ArgumentOutOfRangeException.ThrowIfNegative(prefixId);
    if (!collider.IsEnabled || !collider.HasArea)
    {
      throw new ArgumentOutOfRangeException(
        nameof(collider),
        "A world item requires an enabled collider with positive area.");
    }
    long despawnAtTick = checked(spawnedAtTick + timeLeftTicks);
    return CreateEntity(
      definition,
      stack,
      new WorldDropInitialState(
        new LocationComponent(x, y),
        new VelocityComponent(velocityX, velocityY),
        collider,
        spawnedAtTick,
        despawnAtTick,
        replicationId),
      new ItemInstanceProjection(
        CreateDefinitionReference(definition),
        prefixId,
        VariantId: default,
        DyeId: 0,
        IsFavorited: false,
        NameOverride: null));
  }

  public bool TryGet(ItemEntityRef entity, out PlayerInventoryItemSnapshot item)
  {
    if (!TryResolve(entity, out RuntimeEntityHandle handle) ||
        !TryCaptureItem(handle, entity, out ItemInstanceProjection instance,
          out ItemStackComponent stack, out ItemMutationRevision mutationRevision) ||
        !instance.DefinitionRef.ContentId.IsDefined ||
        !_catalog.Items.TryGet(instance.DefinitionRef.ContentId.TypeId,
          out ItemDefinition definition) ||
        !stack.IsValid ||
        stack.IsEmpty)
    {
      item = default;
      return false;
    }

    item = CreateSnapshot(entity, instance, stack, definition, mutationRevision);
    return true;
  }

  public bool TryUpdate(
    PlayerInventoryItemSnapshot item,
    ItemMutationRevision expectedMutationRevision)
  {
    if (item.Entity.IsEmpty ||
        !expectedMutationRevision.IsAssigned ||
        item.MutationRevision != expectedMutationRevision ||
        item.Entity.Reference != expectedMutationRevision.ItemReference ||
        item.TypeId <= 0 ||
        item.Stack <= 0 ||
        !_catalog.Items.TryGet(item.TypeId, out ItemDefinition definition) ||
        definition.Stack.MaxStack <= 0 ||
        item.Stack > definition.Stack.MaxStack ||
        !TryResolve(item.Entity, out RuntimeEntityHandle handle) ||
        !TryCaptureItem(handle, item.Entity, out _, out _,
          out ItemMutationRevision currentMutationRevision) ||
        currentMutationRevision != expectedMutationRevision)
    {
      return false;
    }

    ItemDefinitionRef definitionReference = CreateDefinitionReference(definition);
    return _entityRuntime.TryEditPair<ItemInstanceComponent, ItemStackComponent>(
      handle,
      (ref ItemInstanceComponent instance, ref ItemStackComponent stack) =>
      {
        instance.DefinitionRef = definitionReference;
        instance.PrefixId = item.PrefixId;
        instance.IsFavorited = item.IsFavorited;
        stack.Quantity = item.Stack;
        stack.MaximumQuantity = definition.Stack.MaxStack;
      });
  }

  public bool Remove(ItemEntityRef entity)
  {
    if (!TryResolve(entity, out RuntimeEntityHandle handle) ||
        !_entityRuntime.TryBeginTermination(handle))
    {
      return false;
    }

    if (!_entityRuntime.TryRemoveEntity(handle))
    {
      throw new InvalidOperationException("A terminating item entity could not be removed.");
    }

    return true;
  }

  public bool CanRemove(ItemEntityRef entity)
  {
    return TryResolve(entity, out RuntimeEntityHandle handle) &&
      TryCaptureItem(handle, entity, out _, out _, out _);
  }

  internal bool TryGetInventoryRelation(
    ItemEntityRef entity,
    out ItemInventoryRelationComponent relation)
  {
    if (TryResolve(entity, out RuntimeEntityHandle handle) &&
        _entityRuntime.TryCapture<ItemInventoryRelationComponent, ItemInventoryRelationComponent>(
          handle,
          static component => component,
          out relation))
    {
      return true;
    }

    relation = default;
    return false;
  }

  internal bool TrySetInventoryRelation(
    ItemEntityRef entity,
    EntityReference playerReference,
    ItemInventorySlotKind slotKind,
    int slotIndex)
  {
    if (!TryResolve(entity, out RuntimeEntityHandle handle) ||
        !TryResolveInventoryPlayer(playerReference, out _))
    {
      return false;
    }

    var expected = new ItemInventoryRelationComponent(
      playerReference,
      slotKind,
      slotIndex);
    if (_entityRuntime.TryCapture<ItemInventoryRelationComponent, ItemInventoryRelationComponent>(
          handle,
          static component => component,
          out ItemInventoryRelationComponent current))
    {
      return current == expected;
    }

    return _entityRuntime.TryAttach(handle, expected);
  }

  private bool TryResolveInventoryPlayer(
    EntityReference playerReference,
    out RuntimeEntityHandle playerHandle)
  {
    if (playerReference.IsEmpty ||
        playerReference.Scope != EntityReferenceScope.Player ||
        playerReference.RuntimeId != _entityRuntime.RuntimeId ||
        !_entityRuntime.TryResolve(playerReference, out playerHandle) ||
        !_entityRuntime.TryGetStatus(playerHandle, out EntityRuntimeStatus status) ||
        status != EntityRuntimeStatus.Running ||
        !_entityRuntime.Has<PlayerInventorySlotsComponent>(playerHandle))
    {
      playerHandle = default;
      return false;
    }

    return true;
  }

  internal bool TryClearInventoryRelation(
    ItemEntityRef entity,
    ItemInventoryRelationComponent expected)
  {
    return TryResolve(entity, out RuntimeEntityHandle handle) &&
      _entityRuntime.TryCapture<ItemInventoryRelationComponent, ItemInventoryRelationComponent>(
        handle,
        static component => component,
        out ItemInventoryRelationComponent current) &&
      current == expected &&
      _entityRuntime.TryDetach<ItemInventoryRelationComponent>(handle);
  }

  public bool TryGetRuntimeEntityId(ItemEntityRef entity, out RuntimeEntityId runtimeEntityId)
  {
    if (entity.IsEmpty || entity.Reference.RuntimeId != _entityRuntime.RuntimeId ||
        !TryResolve(entity, out _))
    {
      runtimeEntityId = RuntimeEntityId.Empty;
      return false;
    }

    runtimeEntityId = RuntimeEntityId.FromItemReference(entity);
    return true;
  }

  internal bool TryGetWorldItem(
    ItemEntityRef entity,
    out RuntimeWorldItemProjection projection)
  {
    if (TryResolve(entity, out RuntimeEntityHandle handle) &&
        _entityRuntime.TryCaptureVersioned<WorldItemComponent, WorldItemComponentProjection>(
          handle,
          static component => new WorldItemComponentProjection(
            component.SpawnedAtTick,
            component.DespawnAtTick,
            component.IsInstanced,
            component.IsBeingGrabbed,
            component.IsOnConveyor,
            component.Revision),
          out EntityComponentSnapshot<WorldItemComponentProjection> worldItem) &&
        _entityRuntime.TryCaptureVersioned<LocationComponent, LocationComponent>(
          handle,
          static component => component,
          out EntityComponentSnapshot<LocationComponent> location) &&
        _entityRuntime.TryCaptureVersioned<VelocityComponent, VelocityComponent>(
          handle,
          static component => component,
          out EntityComponentSnapshot<VelocityComponent> velocity) &&
        _entityRuntime.TryCaptureVersioned<ColliderComponent, ColliderComponent>(
          handle,
          static component => component,
          out EntityComponentSnapshot<ColliderComponent> collider))
    {
      projection = new RuntimeWorldItemProjection(
        entity,
        worldItem,
        location,
        velocity,
        collider);
      return true;
    }

    projection = default;
    return false;
  }

  internal bool TryUpdateWorldItem(
    in RuntimeWorldItemProjection expected,
    LocationComponent location,
    VelocityComponent velocity)
  {
    if (expected.Entity.IsEmpty ||
        !TryResolve(expected.Entity, out RuntimeEntityHandle handle) ||
        !TryGetWorldItem(expected.Entity, out RuntimeWorldItemProjection current) ||
        !HasSameRevision(expected.WorldItem, current.WorldItem) ||
        !HasSameRevision(expected.Location, current.Location) ||
        !HasSameRevision(expected.Velocity, current.Velocity) ||
        !HasSameRevision(expected.Collider, current.Collider))
    {
      return false;
    }

    return _entityRuntime.TryEditComponents<
      LocationComponent,
      VelocityComponent,
      WorldItemComponent>(
      handle,
      (ref LocationComponent currentLocation,
       ref VelocityComponent currentVelocity,
       ref WorldItemComponent worldItem) =>
      {
        currentLocation = location;
        currentVelocity = velocity;
        worldItem.Revision = checked(worldItem.Revision + 1);
      });
  }

  internal bool TryApplyNetworkSync(
    ItemEntityRef entity,
    int typeId,
    int stackCount,
    int prefixId,
    LocationComponent location,
    VelocityComponent velocity)
  {
    if (stackCount <= 0 ||
        prefixId < 0 ||
        !_catalog.Items.TryGet(typeId, out ItemDefinition definition) ||
        stackCount > definition.Stack.MaxStack ||
        !TryResolve(entity, out RuntimeEntityHandle handle))
    {
      return false;
    }

    ItemDefinitionRef definitionReference = CreateDefinitionReference(definition);
    return _entityRuntime.TryEditComponents<
      ItemInstanceComponent,
      ItemStackComponent,
      LocationComponent,
      VelocityComponent,
      WorldItemComponent>(
      handle,
      (ref ItemInstanceComponent instance,
       ref ItemStackComponent stack,
       ref LocationComponent currentLocation,
       ref VelocityComponent currentVelocity,
       ref WorldItemComponent worldItem) =>
      {
        instance.DefinitionRef = definitionReference;
        instance.PrefixId = prefixId;
        stack.Quantity = stackCount;
        stack.MaximumQuantity = definition.Stack.MaxStack;
        currentLocation = location;
        currentVelocity = velocity;
        worldItem.Revision = checked(worldItem.Revision + 1);
      });
  }

  internal bool TryDetachWorldPresence(
    ItemEntityRef entity,
    Func<bool>? releaseWorldProjection = null)
  {
    if (!TryResolve(entity, out RuntimeEntityHandle handle))
    {
      return false;
    }

    if (!_entityRuntime.Has<WorldItemComponent>(handle))
    {
      return releaseWorldProjection is null &&
        !_entityRuntime.Has<WorldItemStateComponent>(handle) &&
        !_entityRuntime.Has<WorldItemReservationComponent>(handle) &&
        !_entityRuntime.Has<LocationComponent>(handle) &&
        !_entityRuntime.Has<VelocityComponent>(handle) &&
        !_entityRuntime.Has<ColliderComponent>(handle);
    }

    if (releaseWorldProjection is null)
    {
      return false;
    }

    if (!_entityRuntime.Has<LocationComponent>(handle) ||
        !_entityRuntime.Has<VelocityComponent>(handle) ||
        !_entityRuntime.Has<ColliderComponent>(handle))
    {
      return false;
    }

    if (!_entityRuntime.TryCaptureVersioned<WorldItemComponent, WorldItemComponentProjection>(
          handle,
          static component => new WorldItemComponentProjection(
            component.SpawnedAtTick,
            component.DespawnAtTick,
            component.IsInstanced,
            component.IsBeingGrabbed,
            component.IsOnConveyor,
            component.Revision),
          out EntityComponentSnapshot<WorldItemComponentProjection> worldItem) ||
        !_entityRuntime.TryCapture<LocationComponent, LocationComponent>(
          handle,
          static component => component,
          out LocationComponent location) ||
        !_entityRuntime.TryCapture<VelocityComponent, VelocityComponent>(
          handle,
          static component => component,
          out VelocityComponent velocity) ||
        !_entityRuntime.TryCapture<ColliderComponent, ColliderComponent>(
          handle,
          static component => component,
          out ColliderComponent collider))
    {
      return false;
    }

    bool hasWorldState = _entityRuntime.Has<WorldItemStateComponent>(handle);
    WorldItemStateProjection worldState = default;
    if (hasWorldState &&
        !_entityRuntime.TryCapture<WorldItemStateComponent, WorldItemStateProjection>(
          handle,
          static component => new WorldItemStateProjection(
            component.ReplicationId,
            component.SpawnSource),
          out worldState))
    {
      return false;
    }

    bool hasReservation = _entityRuntime.Has<WorldItemReservationComponent>(handle);
    EntityComponentSnapshot<WorldItemReservationProjection> reservationSnapshot = default;
    if (hasReservation &&
        !_entityRuntime.TryCaptureVersioned<
          WorldItemReservationComponent,
          WorldItemReservationProjection>(
          handle,
          static component => new WorldItemReservationProjection(
            component.ReservationId,
            component.ReservedFor,
            component.ReservationExpiresAt,
            component.IgnoreOwner,
            component.IgnoreOwnerUntilTick,
            component.NoGrabUntilTick,
            component.EnemyPickupBlockedUntilTick,
            component.ReservationRevision),
          out reservationSnapshot))
    {
      return false;
    }

    WorldItemReservationProjection reservation = reservationSnapshot.Value;

    bool detachedWorldItem = false;
    bool detachedWorldState = false;
    bool detachedLocation = false;
    bool detachedVelocity = false;
    bool detachedCollider = false;
    bool detachedReservation = false;
    try
    {
      detachedWorldItem = _entityRuntime.TryDetach<WorldItemComponent>(handle);
      detachedWorldState = hasWorldState &&
        _entityRuntime.TryDetach<WorldItemStateComponent>(handle);
      detachedLocation = _entityRuntime.TryDetach<LocationComponent>(handle);
      detachedVelocity = _entityRuntime.TryDetach<VelocityComponent>(handle);
      detachedCollider = _entityRuntime.TryDetach<ColliderComponent>(handle);
      detachedReservation = hasReservation &&
        _entityRuntime.TryDetach<WorldItemReservationComponent>(handle);

      if (!detachedWorldItem || (hasWorldState && !detachedWorldState) ||
          !detachedLocation || !detachedVelocity || !detachedCollider ||
          (hasReservation && !detachedReservation))
      {
        RestoreWorldPresence(
          handle,
          worldItem.Value,
          worldState,
          reservation,
          location,
          velocity,
          collider,
          detachedWorldItem,
          detachedWorldState,
          detachedLocation,
          detachedVelocity,
          detachedCollider,
          detachedReservation);
        return false;
      }

      if (releaseWorldProjection is not null && !releaseWorldProjection())
      {
        RestoreWorldPresence(
          handle,
          worldItem.Value,
          worldState,
          reservation,
          location,
          velocity,
          collider,
          restoreWorldItem: true,
          restoreWorldState: detachedWorldState,
          restoreLocation: true,
          restoreVelocity: true,
          restoreCollider: true,
          restoreReservation: detachedReservation);
        return false;
      }

      return true;
    }
    catch (Exception detachException)
    {
      try
      {
        RestoreWorldPresence(
          handle,
          worldItem.Value,
          worldState,
          reservation,
          location,
          velocity,
          collider,
          detachedWorldItem,
          detachedWorldState,
          detachedLocation,
          detachedVelocity,
          detachedCollider,
          detachedReservation);
      }
      catch (Exception rollbackException)
      {
        throw new AggregateException(
          "World item detachment failed and its components could not be restored.",
          detachException,
          rollbackException);
      }

      throw;
    }
  }

  private PlayerInventoryItemSnapshot CreateEntity(
    ItemDefinition definition,
    int stack,
    WorldDropInitialState? worldDrop,
    ItemInstanceProjection? initialInstance = null)
  {
    RuntimeEntityHandle handle = default;
    bool entityCreated = false;
    try
    {
      handle = _entityRuntime.CreateEntity();
      entityCreated = true;

      ItemInstanceProjection instanceState = initialInstance ?? new ItemInstanceProjection(
        CreateDefinitionReference(definition),
        PrefixId: 0,
        VariantId: default,
        DyeId: 0,
        IsFavorited: false,
        NameOverride: null);
      var instance = new ItemInstanceComponent(
        CreateDefinitionReference(definition),
        new PersistentItemId(Guid.NewGuid()),
        instanceState.PrefixId,
        instanceState.VariantId,
        instanceState.DyeId,
        instanceState.IsFavorited,
        instanceState.NameOverride);
      var itemStack = new ItemStackComponent(stack, definition.Stack.MaxStack);
      if (!_entityRuntime.TryAttach(handle, instance) ||
          !_entityRuntime.TryAttach(handle, itemStack))
      {
        throw new InvalidOperationException("The item components could not be attached.");
      }

      if (worldDrop is WorldDropInitialState initialWorldDrop &&
          (!_entityRuntime.TryAttach(handle, initialWorldDrop.Location) ||
           !_entityRuntime.TryAttach(handle, initialWorldDrop.Velocity) ||
           !_entityRuntime.TryAttach(handle, initialWorldDrop.Collider) ||
           !_entityRuntime.TryAttach(
             handle,
             new WorldItemComponent(
               initialWorldDrop.SpawnedAtTick,
               initialWorldDrop.DespawnAtTick)) ||
           !_entityRuntime.TryAttach(
             handle,
             new WorldItemStateComponent(initialWorldDrop.ReplicationId)) ||
           !_entityRuntime.TryAttach(handle, new WorldItemReservationComponent())))
      {
        throw new InvalidOperationException("The world item components could not be attached.");
      }

      if (!_entityRuntime.TryPublishEntity(handle) ||
          !_entityRuntime.TryGetReference(
            handle,
            EntityReferenceScope.Item,
            out EntityReference reference))
      {
        throw new InvalidOperationException("The item entity could not be committed.");
      }

      ItemEntityRef itemReference = ItemEntityRef.FromReference(reference);
      if (!TryCaptureItem(handle, itemReference, out ItemInstanceProjection instanceProjection,
            out ItemStackComponent stackProjection,
            out ItemMutationRevision mutationRevision))
      {
        throw new InvalidOperationException("The committed item could not be captured.");
      }

      return CreateSnapshot(
        itemReference,
        instanceProjection,
        stackProjection,
        definition,
        mutationRevision);
    }
    catch (Exception createException)
    {
      if (entityCreated)
      {
        try
        {
          RemoveIncompleteEntity(handle);
        }
        catch (Exception cleanupException)
        {
          throw new AggregateException(
            "Item creation failed and its incomplete entity could not be removed.",
            createException,
            cleanupException);
        }
      }

      throw;
    }
  }

  private bool TryCaptureItem(
    RuntimeEntityHandle handle,
    ItemEntityRef itemReference,
    out ItemInstanceProjection instance,
    out ItemStackComponent stack,
    out ItemMutationRevision mutationRevision)
  {
    if (!_entityRuntime.TryCaptureVersioned<
          ItemInstanceComponent,
          ItemInstanceProjection>(
          handle,
          static component => new ItemInstanceProjection(
            component.DefinitionRef,
            component.PrefixId,
            component.VariantId,
            component.DyeId,
            component.IsFavorited,
            component.NameOverride),
          out EntityComponentSnapshot<ItemInstanceProjection> instanceSnapshot) ||
        !_entityRuntime.TryCaptureVersioned<ItemStackComponent, ItemStackComponent>(
          handle,
          static component => component,
          out EntityComponentSnapshot<ItemStackComponent> stackSnapshot))
    {
      instance = default;
      stack = default;
      mutationRevision = default;
      return false;
    }

    instance = instanceSnapshot.Value;
    stack = stackSnapshot.Value;
    mutationRevision = new ItemMutationRevision(
      itemReference.Reference,
      instanceSnapshot.AttachmentRevision,
      instanceSnapshot.DataRevision,
      stackSnapshot.AttachmentRevision,
      stackSnapshot.DataRevision);
    return true;
  }

  private bool TryResolve(ItemEntityRef entity, out RuntimeEntityHandle handle)
  {
    if (entity.IsEmpty ||
        entity.Reference.Scope != EntityReferenceScope.Item ||
        entity.Reference.RuntimeId != _entityRuntime.RuntimeId)
    {
      handle = default;
      return false;
    }

    return _entityRuntime.TryResolve(entity.Reference, out handle) &&
      _entityRuntime.Has<ItemInstanceComponent>(handle) &&
      _entityRuntime.Has<ItemStackComponent>(handle);
  }

  private ItemDefinition GetDefinition(int typeId)
  {
    if (!_catalog.Items.TryGet(typeId, out ItemDefinition definition))
    {
      throw new InvalidDataException($"Item {typeId} has no simulation definition.");
    }

    return definition;
  }

  private static void ValidateStack(int stack, ItemDefinition definition)
  {
    if (stack <= 0 || stack > definition.Stack.MaxStack)
    {
      throw new ArgumentOutOfRangeException(nameof(stack));
    }
  }

  private static bool HasSameRevision<TProjection>(
    EntityComponentSnapshot<TProjection> expected,
    EntityComponentSnapshot<TProjection> current)
    where TProjection : struct
  {
    return expected.AttachmentRevision == current.AttachmentRevision &&
      expected.DataRevision == current.DataRevision;
  }

  private static ItemDefinitionRef CreateDefinitionReference(ItemDefinition definition)
  {
    return new ItemDefinitionRef(
      new ExternalContentId(TerrariaContentDomain, definition.Identity.TypeId),
      DefinitionRevision: 0);
  }

  private static PlayerInventoryItemSnapshot CreateSnapshot(
    ItemEntityRef entity,
    ItemInstanceProjection instance,
    ItemStackComponent stack,
    ItemDefinition definition,
    ItemMutationRevision mutationRevision)
  {
    bool hasAmmo = !definition.Combat.IsNotAmmo;
    return new PlayerInventoryItemSnapshot(
      entity,
      definition.Identity.TypeId,
      instance.PrefixId,
      stack.Quantity,
      stack.MaximumQuantity,
      IsFavorited: instance.IsFavorited,
      IsUniqueStack: definition.Stack.UniqueStack,
      IsCoin: definition.Identity.TypeId is >= 71 and <= 73,
      HasAmmo: hasAmmo,
      IsNotAmmo: definition.Combat.IsNotAmmo,
      CanFillEmptyAmmoSlot: hasAmmo)
    {
      MutationRevision = mutationRevision,
    };
  }

  private void RemoveIncompleteEntity(RuntimeEntityHandle handle)
  {
    if (!_entityRuntime.TryGetStatus(handle, out EntityRuntimeStatus status))
    {
      return;
    }

    if (status == EntityRuntimeStatus.Running && !_entityRuntime.TryBeginTermination(handle))
    {
      throw new InvalidOperationException("The incomplete item entity could not be terminated.");
    }

    if (!_entityRuntime.TryRemoveEntity(handle))
    {
      throw new InvalidOperationException("The incomplete item entity could not be removed.");
    }
  }

  private void RestoreWorldPresence(
    RuntimeEntityHandle handle,
    WorldItemComponentProjection worldItem,
    WorldItemStateProjection worldState,
    WorldItemReservationProjection reservation,
    LocationComponent location,
    VelocityComponent velocity,
    ColliderComponent collider,
    bool restoreWorldItem,
    bool restoreWorldState,
    bool restoreLocation,
    bool restoreVelocity,
    bool restoreCollider,
    bool restoreReservation)
  {
    if (restoreLocation && !_entityRuntime.TryAttach(handle, location))
    {
      throw new InvalidOperationException("World item location could not be restored.");
    }

    if (restoreVelocity && !_entityRuntime.TryAttach(handle, velocity))
    {
      throw new InvalidOperationException("World item velocity could not be restored.");
    }

    if (restoreCollider && !_entityRuntime.TryAttach(handle, collider))
    {
      throw new InvalidOperationException("World item collider could not be restored.");
    }

    if (restoreWorldState &&
        !_entityRuntime.TryAttach(
          handle,
          new WorldItemStateComponent(worldState.ReplicationId, worldState.SpawnSource)))
    {
      throw new InvalidOperationException("World item state could not be restored.");
    }

    if (restoreWorldItem &&
        !_entityRuntime.TryAttach(
          handle,
          new WorldItemComponent(
            worldItem.SpawnedAtTick,
            worldItem.DespawnAtTick,
            worldItem.IsInstanced,
            worldItem.IsBeingGrabbed,
            worldItem.IsOnConveyor,
            worldItem.Revision)))
    {
      throw new InvalidOperationException("World item presence could not be restored.");
    }

    if (restoreReservation &&
        !_entityRuntime.TryAttach(
          handle,
          new WorldItemReservationComponent(
            reservation.ReservationId,
            reservation.ReservedFor,
            reservation.ReservationExpiresAt,
            reservation.IgnoreOwner,
            reservation.IgnoreOwnerUntilTick,
            reservation.NoGrabUntilTick,
            reservation.EnemyPickupBlockedUntilTick,
            reservation.ReservationRevision)))
    {
      throw new InvalidOperationException("World item reservation state could not be restored.");
    }
  }

  private readonly record struct ItemInstanceProjection(
    ItemDefinitionRef DefinitionRef,
    int PrefixId,
    ExternalContentId VariantId,
    int DyeId,
    bool IsFavorited,
    string? NameOverride);

  private readonly record struct WorldDropInitialState(
    LocationComponent Location,
    VelocityComponent Velocity,
    ColliderComponent Collider,
    long SpawnedAtTick,
    long DespawnAtTick,
    ReplicationId ReplicationId);

  private readonly record struct WorldItemStateProjection(
    ReplicationId ReplicationId,
    LootSourceRef? SpawnSource);

  private readonly record struct WorldItemReservationProjection(
    ReservationId? ReservationId,
    RuntimeEntityId? ReservedFor,
    long? ReservationExpiresAt,
    RuntimeEntityId? IgnoreOwner,
    long? IgnoreOwnerUntilTick,
    long NoGrabUntilTick,
    long EnemyPickupBlockedUntilTick,
    long ReservationRevision);
}

internal readonly record struct WorldItemComponentProjection(
  long SpawnedAtTick,
  long? DespawnAtTick,
  bool IsInstanced,
  bool IsBeingGrabbed,
  bool IsOnConveyor,
  long Revision)
{
  public bool IsExpiredAt(long currentTick) =>
    DespawnAtTick.HasValue && currentTick >= DespawnAtTick.Value;

  public long RemainingTicksAt(long currentTick) =>
    DespawnAtTick.HasValue
      ? Math.Max(0, DespawnAtTick.Value - currentTick)
      : long.MaxValue;
}

internal readonly record struct RuntimeWorldItemProjection(
  ItemEntityRef Entity,
  EntityComponentSnapshot<WorldItemComponentProjection> WorldItem,
  EntityComponentSnapshot<LocationComponent> Location,
  EntityComponentSnapshot<VelocityComponent> Velocity,
  EntityComponentSnapshot<ColliderComponent> Collider);
