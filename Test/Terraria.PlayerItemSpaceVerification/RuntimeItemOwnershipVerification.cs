using System.Numerics;
using EntityEcs;
using EntityEcs.Components;
using Terraria.Content;
using Terraria.Items;
using Terraria.NonAuthoritative.Persistence;
using Terraria.NonAuthoritative.SimulationHost;
using Terraria.Player;
using Terraria.Relationships;
using Terraria.WorldStorage;

namespace Terraria.PlayerItemSpaceVerification;

internal static class RuntimeItemOwnershipVerification
{
  private const int ExpectedReverseEmptyPickupSlotIndex = 49;

  public static void Run()
  {
    VerifyColliderGeometry();
    VerifySplitAndMutationIsolation();
    VerifyInventoryRelationRuntimeScope();
    VerifySinglePlayerInventoryOwnership();
    VerifyWorldPickupDetachAndEffectRetry();
    VerifyEffectPortCanBorrowCommittedPlayer();
    VerifyPickupEffectOrder();
    VerifyPartialAndExhaustedStackMerge();
    VerifyBorrowedMergeSourcePreflight();
    VerifyWorldPresenceReleaseCallbacksRollback();
    VerifyFullMergeReleaseCallbacksRollback();
    VerifyRejectedPickupKeepsWorldPresence();
    VerifyCoinMergeInventoryRelations();
    VerifyConsumptionAndStaleRevision();
    VerifyReleaseRejectsBorrowedState();
    VerifyRuntimeWorldItemStoreExpiryAndSpawnRollback();
    Console.WriteLine("PASS: runtime Item ownership, split, pickup, merge, consumption, and geometry");
  }

  private static void VerifyColliderGeometry()
  {
    using var fixture = new Fixture();
    PlayerInventoryItemSnapshot worldItem = fixture.Items.CreateWorldDrop(
      typeId: 23,
      stack: 1,
      x: 0f,
      y: 0f,
      velocityX: 0f,
      velocityY: 0f,
      collider: new ColliderComponent(16f, 16f),
      spawnedAtTick: 0,
      timeLeftTicks: 6_000,
      replicationId: new ReplicationId(1));

    Require(
      fixture.Items.TryGetWorldItem(worldItem.Entity, out RuntimeWorldItemProjection projection) &&
      projection.Collider.Value.Width == 16f &&
      projection.Collider.Value.Height == 16f,
      "world-item hydration must retain the source 16×16 collider");

    Require(
      fixture.Runtime.TryCapture<ColliderComponent, ColliderComponent>(
        fixture.PlayerHandle,
        static collider => collider,
        out ColliderComponent playerCollider) &&
      playerCollider.Width == 20f &&
      playerCollider.Height == 42f,
      "the Player root must retain its source 20×42 collider");

    Require(
      RuntimeWorldItemStore.CanPlayerPickUp(
        new Vector2(0f, 77f),
        new ColliderComponent(16f, 16f),
        Vector2.Zero,
        playerCollider,
        out float distance) &&
      distance == 43f,
      "pickup range must use the Player collider's 42-pixel height");
    Require(
      !RuntimeWorldItemStore.CanPlayerPickUp(
        new Vector2(0f, 77f),
        new ColliderComponent(16f, 16f),
        Vector2.Zero,
        new ColliderComponent(20f, 20f),
        out _),
      "pickup range must change when the Player collider height changes");
    Require(
      RuntimeWorldItemStore.CanPlayerPickUp(
        Vector2.Zero,
        new ColliderComponent(16f, 16f, offsetX: 3f),
        new Vector2(59f, 0f),
        playerCollider,
        out distance) &&
      distance == 48f,
      "pickup geometry must include collider offsets at the existing range boundary");

    Require(fixture.Items.Remove(worldItem.Entity), "geometry probe cleanup removes its Item root");
  }

  private static void VerifySplitAndMutationIsolation()
  {
    using var fixture = new Fixture();
    PlayerInventoryItemSnapshot source = fixture.Items.Create(typeId: 23, stack: 10);
    SetSlot(fixture, 0, source.Entity);
    RuntimeEntityHandle sourceHandle = Resolve(fixture.Runtime, source.Entity);
    Require(
      fixture.Runtime.TryEdit<ItemInstanceComponent>(
        sourceHandle,
        (ref ItemInstanceComponent instance) =>
        {
          instance.PrefixId = 2;
          instance.IsFavorited = true;
        }),
      "source instance metadata must be editable before splitting");
    Require(
      fixture.Items.TryGet(source.Entity, out source),
      "the source snapshot must refresh after its instance edit");

    Guid commandId = Guid.Parse("00000000-0000-0000-0000-00000000B601");
    Require(
      fixture.Owner.TrySplit(commandId, sourceSlotIndex: 0, amount: 4, destinationSlotIndex: 1),
      "a valid partial split must commit");
    bool hasRemaining = fixture.Owner.TryGetItemAtSlot(
      0,
      out PlayerInventoryItemSnapshot remaining);
    bool hasSplit = fixture.Owner.TryGetItemAtSlot(
      1,
      out PlayerInventoryItemSnapshot split);
    Require(
      hasRemaining &&
      hasSplit &&
      remaining.Stack == 6 &&
      split.Stack == 4 &&
      remaining.Stack + split.Stack == source.Stack,
      "a partial split must conserve quantity across the existing and new roots");
    Require(
      remaining.Entity != split.Entity &&
      remaining.Entity.Reference.EntityId != split.Entity.Reference.EntityId &&
      remaining.Entity.Reference.RuntimeId == split.Entity.Reference.RuntimeId,
      "a split must create a distinct Item root in the same runtime");
    Require(
      split.PrefixId == 2 && split.IsFavorited,
      "a split must copy the source's represented instance metadata");

    RuntimeEntityHandle splitHandle = Resolve(fixture.Runtime, split.Entity);
    Require(
      fixture.Runtime.TryCapture<ItemInstanceComponent, PersistentItemId>(
        Resolve(fixture.Runtime, remaining.Entity),
        static instance => instance.PersistentInstanceId,
        out PersistentItemId sourcePersistentId) &&
      fixture.Runtime.TryCapture<ItemInstanceComponent, PersistentItemId>(
        splitHandle,
        static instance => instance.PersistentInstanceId,
        out PersistentItemId splitPersistentId) &&
      sourcePersistentId != splitPersistentId,
      "a split must have independent persistent instance identity");
    Require(
      fixture.Owner.TrySplit(commandId, sourceSlotIndex: 0, amount: 4, destinationSlotIndex: 1) &&
      fixture.Runtime.EntityCount == 3,
      "retrying the same split command must not create another Item root");

    Require(
      fixture.Items.TryUpdate(split with { Stack = 3 }, split.MutationRevision) &&
      fixture.Items.TryGet(remaining.Entity, out PlayerInventoryItemSnapshot unchangedSource) &&
      unchangedSource.Stack == 6,
      "updating a split Item must not mutate the source ItemStackComponent");

    ItemEntityRef foreignRuntimeReference = ItemEntityRef.FromReference(
      new EntityReference(
        split.Entity.Reference.EntityId,
        new EntityRuntimeId(Guid.NewGuid()),
        EntityReferenceScope.Item));
    Require(
      !fixture.Items.TryGet(foreignRuntimeReference, out _),
      "an Item UUID paired with a different runtime must not resolve");
  }

  private static void VerifyWorldPickupDetachAndEffectRetry()
  {
    var effects = new RecordingEffectPort(accept: false);
    using var fixture = new Fixture(effects);
    PlayerInventoryItemSnapshot drop = CreateWorldDrop(fixture, stack: 5);
    RuntimeEntityHandle itemHandle = Resolve(fixture.Runtime, drop.Entity);
    int releaseCount = 0;

    PlayerInventoryPickupResult first = fixture.Owner.TryPickup(
      drop,
      () =>
      {
        releaseCount++;
        return true;
      });
    Require(
      first.Applied && !first.EffectsApplied &&
      first.RejectionReason == PlayerInventoryPickupRejectionReason.EffectPortRejected,
      "an effect failure must be reported after the inventory commit");
    Require(
      first.Inventory.Target.IsMainInventory &&
      first.Inventory.Target.SlotIndex == ExpectedReverseEmptyPickupSlotIndex,
      "Gel must use reverse empty-slot order and target main-inventory slot 49");
    Require(
      releaseCount == 1 &&
      fixture.Owner.TryGetItemAtSlot(
        ExpectedReverseEmptyPickupSlotIndex,
        out PlayerInventoryItemSnapshot held) &&
      held.Entity == drop.Entity && held.Stack == 5,
      "a full pickup must preserve the Item root and stack in slot 49");
    Require(
      fixture.Items.TryGetInventoryRelation(
        drop.Entity,
        out ItemInventoryRelationComponent relation) &&
      relation.PlayerReference == fixture.PlayerReference &&
      relation.SlotKind == ItemInventorySlotKind.MainInventory &&
      relation.SlotIndex == ExpectedReverseEmptyPickupSlotIndex,
      "a full pickup must attach the Item's reverse relation to Player slot 49");
    Require(
      fixture.Runtime.Has<ItemInstanceComponent>(itemHandle) &&
      fixture.Runtime.Has<ItemStackComponent>(itemHandle) &&
      !fixture.Runtime.Has<WorldItemComponent>(itemHandle) &&
      !fixture.Runtime.Has<WorldItemStateComponent>(itemHandle) &&
      !fixture.Runtime.Has<WorldItemReservationComponent>(itemHandle) &&
      !fixture.Runtime.Has<LocationComponent>(itemHandle) &&
      !fixture.Runtime.Has<VelocityComponent>(itemHandle) &&
      !fixture.Runtime.Has<ColliderComponent>(itemHandle),
      "an inventory-only Item root must have no world-presence or spatial components");

    PlayerInventoryPickupResult retry = fixture.Owner.TryPickup(drop, () =>
    {
      releaseCount++;
      return true;
    });
    Require(
      retry == first && releaseCount == 1 && effects.Intents.Count == 1,
      "retrying a committed pickup after an effect failure must not repeat effects or transfer");
  }

  private static void VerifyEffectPortCanBorrowCommittedPlayer()
  {
    var effects = new RecordingEffectPort(accept: false);
    using var fixture = new Fixture(effects);
    PlayerInventoryItemSnapshot drop = CreateWorldDrop(fixture, stack: 1);
    bool playerWasBorrowed = false;
    effects.OnApply = _ =>
    {
      playerWasBorrowed = fixture.Runtime.TryEdit<PlayerInventorySlotsComponent>(
        fixture.PlayerHandle,
        (ref PlayerInventorySlotsComponent _) => { });
    };

    PlayerInventoryPickupResult first = fixture.Owner.TryPickup(drop, static () => true);
    Require(
      playerWasBorrowed && first.Applied && !first.EffectsApplied,
      "an effect callback must borrow the Player after inventory state commits");
    Require(
      first.Inventory.Target.IsMainInventory &&
      first.Inventory.Target.SlotIndex == ExpectedReverseEmptyPickupSlotIndex &&
      fixture.Owner.TryGetItemAtSlot(
        ExpectedReverseEmptyPickupSlotIndex,
        out PlayerInventoryItemSnapshot held) &&
      held.Entity == drop.Entity && held.Stack == 1,
      "the borrowed-Player pickup must retain its Item root in reverse-order slot 49");
    Require(
      fixture.Items.TryGetInventoryRelation(drop.Entity, out ItemInventoryRelationComponent relation) &&
      relation.PlayerReference == fixture.PlayerReference &&
      relation.SlotKind == ItemInventorySlotKind.MainInventory &&
      relation.SlotIndex == ExpectedReverseEmptyPickupSlotIndex,
      "the borrowed-Player pickup must retain the Item relation to slot 49");

    PlayerInventoryPickupResult retry = fixture.Owner.TryPickup(drop, static () =>
      throw new InvalidOperationException("A cached pickup must not release its projection again."));
    Require(
      retry == first && effects.Intents.Count == 1,
      "a Player-root borrow during effect application must preserve pickup retry idempotency");
  }

  private static void VerifySinglePlayerInventoryOwnership()
  {
    using var fixture = new Fixture();
    PlayerInventoryItemSnapshot incoming = fixture.Items.Create(typeId: 23, stack: 1);
    SetSlot(fixture, 0, incoming.Entity);

    RuntimeEntityHandle secondPlayerHandle = CreatePlayer(fixture.Runtime);
    Require(
      fixture.Runtime.TryGetReference(
        secondPlayerHandle,
        EntityReferenceScope.Player,
        out EntityReference secondPlayerReference),
      "the second Player root must receive its own scoped reference");
    var secondOwner = new RuntimePlayerInventoryOwner(
      fixture.Runtime,
      secondPlayerHandle,
      fixture.Items,
      playerSlot: 1);

    PlayerInventoryItemSnapshot destination = fixture.Items.Create(typeId: 23, stack: 9_998);
    Require(
      fixture.Items.TrySetInventoryRelation(
        destination.Entity,
        secondPlayerReference,
        ItemInventorySlotKind.MainInventory,
        slotIndex: 0) &&
      fixture.Runtime.TryEdit<PlayerInventorySlotsComponent>(
        secondPlayerHandle,
        (ref PlayerInventorySlotsComponent slots) =>
          slots.MainInventorySlots[0] = destination.Entity),
      "the second owner must attach its unrelated destination Item to slot zero");

    Require(
      fixture.Items.TryGet(destination.Entity, out PlayerInventoryItemSnapshot before) &&
      before.Stack == 9_998,
      "the second owner's destination stack must be captured before the rejected transfer");
    PlayerInventoryPickupResult rejected = secondOwner.TryPickup(incoming);
    Require(
      !rejected.Applied &&
      secondOwner.TryGetItemAtSlot(0, out PlayerInventoryItemSnapshot unchangedDestination) &&
      unchangedDestination.Stack == before.Stack &&
      unchangedDestination.MutationRevision == before.MutationRevision,
      "a second owner must reject the same Item root before changing its merge destination");
    Require(
      fixture.Owner.TryGetItemAtSlot(0, out PlayerInventoryItemSnapshot stillOwned) &&
      stillOwned.Entity == incoming.Entity &&
      fixture.Items.TryGetInventoryRelation(incoming.Entity, out ItemInventoryRelationComponent relation) &&
      relation.PlayerReference == fixture.PlayerReference &&
      relation.SlotKind == ItemInventorySlotKind.MainInventory &&
      relation.SlotIndex == 0,
      "rejecting a second owner's transfer must preserve the first slot and reverse relation");
  }

  private static void VerifyInventoryRelationRuntimeScope()
  {
    using var fixture = new Fixture();
    using var foreignRuntime = new EntityRuntime();
    var foreignItems = new RuntimeItemRegistry(SimulationContentBootstrap.Build(), foreignRuntime);
    RuntimeEntityHandle foreignPlayerHandle = CreatePlayer(foreignRuntime);
    Require(
      foreignRuntime.TryGetReference(
        foreignPlayerHandle,
        EntityReferenceScope.Player,
        out EntityReference foreignPlayerReference),
      "the foreign Player root must receive a scoped reference");

    PlayerInventoryItemSnapshot item = fixture.Items.Create(typeId: 23, stack: 1);
    Require(
      !fixture.Items.TrySetInventoryRelation(
        item.Entity,
        foreignPlayerReference,
        ItemInventorySlotKind.MainInventory,
        slotIndex: 0) &&
      !fixture.Items.TryGetInventoryRelation(item.Entity, out _),
      "an Item registry must reject a Player relation from another EntityRuntime");

    bool mismatchedOwnerRejected = false;
    try
    {
      _ = new RuntimePlayerInventoryOwner(
        fixture.Runtime,
        fixture.PlayerHandle,
        foreignItems,
        playerSlot: 2);
    }
    catch (ArgumentException)
    {
      mismatchedOwnerRejected = true;
    }

    Require(
      mismatchedOwnerRejected,
      "a Player owner must reject an Item registry bound to a different EntityRuntime");

    RuntimeEntityHandle expiredPlayerHandle = CreatePlayer(fixture.Runtime);
    Require(
      fixture.Runtime.TryGetReference(
        expiredPlayerHandle,
        EntityReferenceScope.Player,
        out EntityReference expiredPlayerReference) &&
      fixture.Runtime.TryBeginTermination(expiredPlayerHandle) &&
      fixture.Runtime.TryRemoveEntity(expiredPlayerHandle),
      "the temporary Player root must be removable for the stale-reference scenario");
    Require(
      !fixture.Items.TrySetInventoryRelation(
        item.Entity,
        expiredPlayerReference,
        ItemInventorySlotKind.MainInventory,
        slotIndex: 0),
      "an Item registry must reject a Player reference whose root is no longer live");
    Require(
      fixture.Items.Remove(item.Entity),
      "the unowned scope-probe Item root must be removed after rejection");
  }

  private static void VerifyPartialAndExhaustedStackMerge()
  {
    using (var partialFixture = new Fixture())
    {
      PlayerInventoryItemSnapshot existing = partialFixture.Items.Create(23, 9_998);
      SetSlot(partialFixture, 0, existing.Entity);
      PlayerInventoryItemSnapshot drop = CreateWorldDrop(partialFixture, stack: 7);
      int releaseCount = 0;

      PlayerInventoryPickupResult pickup = partialFixture.Owner.TryPickup(drop, () =>
      {
        releaseCount++;
        return true;
      });
      Require(
        pickup.Applied && pickup.RemainingStack == 6 && releaseCount == 0,
        "a partial merge must leave its remainder in the world and retain the projection");
      Require(
        partialFixture.Owner.TryGetItemAtSlot(0, out PlayerInventoryItemSnapshot merged) &&
        merged.Entity == existing.Entity && merged.Stack == 9_999 &&
        partialFixture.Items.TryGet(drop.Entity, out PlayerInventoryItemSnapshot remainder) &&
        remainder.Stack == 6 && remainder.Entity == drop.Entity,
        "a partial merge must preserve the destination root and incoming remainder root");
      Require(
        partialFixture.Items.TryGetWorldItem(drop.Entity, out _) &&
        partialFixture.Runtime.Has<LocationComponent>(Resolve(partialFixture.Runtime, drop.Entity)) &&
        partialFixture.Runtime.Has<ColliderComponent>(Resolve(partialFixture.Runtime, drop.Entity)),
        "a partial world remainder must retain its world and spatial components");
    }

    using (var fullFixture = new Fixture())
    {
      PlayerInventoryItemSnapshot existing = fullFixture.Items.Create(23, 9_992);
      SetSlot(fullFixture, 0, existing.Entity);
      PlayerInventoryItemSnapshot drop = CreateWorldDrop(fullFixture, stack: 7);
      int releaseCount = 0;

      PlayerInventoryPickupResult pickup = fullFixture.Owner.TryPickup(drop, () =>
      {
        releaseCount++;
        return true;
      });
      Require(
        pickup.Applied && pickup.RemainingStack == 0 && releaseCount == 1,
        "an exhausted merge must release the world projection once");
      Require(
        fullFixture.Owner.TryGetItemAtSlot(0, out PlayerInventoryItemSnapshot merged) &&
        merged.Entity == existing.Entity && merged.Stack == 9_999 &&
        !fullFixture.Items.TryGet(drop.Entity, out _),
        "an exhausted merge must preserve the destination root and remove only the incoming root");
    }
  }

  private static void VerifyBorrowedMergeSourcePreflight()
  {
    using var fixture = new Fixture();
    PlayerInventoryItemSnapshot existing = fixture.Items.Create(typeId: 23, stack: 9_998);
    SetSlot(fixture, 0, existing.Entity);
    PlayerInventoryItemSnapshot drop = CreateWorldDrop(fixture, stack: 7);
    bool releaseAttempted = false;
    PlayerInventoryPickupResult pickup = default;

    Require(
      fixture.Runtime.TryEdit<ItemStackComponent>(
        Resolve(fixture.Runtime, drop.Entity),
        (ref ItemStackComponent _) =>
        {
          pickup = fixture.Owner.TryPickup(drop, () =>
          {
            releaseAttempted = true;
            return true;
          });
        }),
      "the incoming Item root must be borrowable for the merge rejection scenario");
    Require(
      !pickup.Applied && !releaseAttempted &&
      fixture.Owner.TryGetItemAtSlot(0, out PlayerInventoryItemSnapshot unchanged) &&
      unchanged.Stack == existing.Stack &&
      unchanged.MutationRevision == existing.MutationRevision &&
      fixture.Items.TryGet(drop.Entity, out PlayerInventoryItemSnapshot retained) &&
      retained.Stack == drop.Stack &&
      fixture.Items.TryGetWorldItem(drop.Entity, out _),
      "a borrowed incoming root must reject merge before destination payload, revision, or world state changes");
  }

  private static void VerifyWorldPresenceReleaseCallbacksRollback()
  {
    VerifyWorldPresenceReleaseCallbackRollback(throwFromCallback: false);
    VerifyWorldPresenceReleaseCallbackRollback(throwFromCallback: true);
    VerifyEmptySlotPickupReleaseFailure(throwFromCallback: false);
    VerifyEmptySlotPickupReleaseFailure(throwFromCallback: true);
  }

  private static void VerifyWorldPresenceReleaseCallbackRollback(bool throwFromCallback)
  {
    using var fixture = new Fixture();
    PlayerInventoryItemSnapshot drop = CreateWorldDrop(fixture, stack: 2);
    bool callbackInvoked = false;
    bool callbackThrew = false;
    bool detached = false;
    try
    {
      detached = fixture.Items.TryDetachWorldPresence(drop.Entity, () =>
      {
        callbackInvoked = true;
        if (throwFromCallback)
        {
          throw new InvalidOperationException("The world projection release callback failed.");
        }

        return false;
      });
    }
    catch (InvalidOperationException)
    {
      callbackThrew = true;
    }

    Require(
      callbackInvoked && (throwFromCallback ? callbackThrew : !detached) &&
      HasWorldPresence(fixture, drop.Entity) &&
      fixture.Items.TryGet(drop.Entity, out PlayerInventoryItemSnapshot retained) &&
      retained.Stack == drop.Stack &&
      !fixture.Items.TryGetInventoryRelation(drop.Entity, out _) &&
      fixture.Runtime.EntityCount == 2,
      "a rejected or throwing projection release must restore every world, spatial, and reservation component");
  }

  private static void VerifyEmptySlotPickupReleaseFailure(bool throwFromCallback)
  {
    using var fixture = new Fixture();
    PlayerInventoryItemSnapshot drop = CreateWorldDrop(fixture, stack: 1);
    bool callbackInvoked = false;
    bool callbackThrew = false;
    PlayerInventoryPickupResult pickup = default;
    try
    {
      pickup = fixture.Owner.TryPickup(drop, () =>
      {
        callbackInvoked = true;
        if (throwFromCallback)
        {
          throw new InvalidOperationException("The world slot could not be released.");
        }

        return false;
      });
    }
    catch (InvalidOperationException)
    {
      callbackThrew = true;
    }

    Require(
      callbackInvoked && (throwFromCallback ? callbackThrew : !pickup.Applied) &&
      fixture.Owner.CountUsedSlots() == 0 &&
      !fixture.Items.TryGetInventoryRelation(drop.Entity, out _) &&
      HasWorldPresence(fixture, drop.Entity) &&
      fixture.Items.TryGet(drop.Entity, out PlayerInventoryItemSnapshot retained) &&
      retained.Stack == drop.Stack,
      "an empty-slot pickup must roll back its provisional containment relation when world release fails");
  }

  private static void VerifyFullMergeReleaseCallbacksRollback()
  {
    VerifyFullMergeReleaseCallbackRollback(throwFromCallback: false);
    VerifyFullMergeReleaseCallbackRollback(throwFromCallback: true);
  }

  private static void VerifyFullMergeReleaseCallbackRollback(bool throwFromCallback)
  {
    using var fixture = new Fixture();
    PlayerInventoryItemSnapshot existing = fixture.Items.Create(typeId: 23, stack: 9_992);
    SetSlot(fixture, 0, existing.Entity);
    PlayerInventoryItemSnapshot drop = CreateWorldDrop(fixture, stack: 7);
    bool callbackInvoked = false;
    bool callbackThrew = false;
    PlayerInventoryPickupResult pickup = default;
    try
    {
      pickup = fixture.Owner.TryPickup(drop, () =>
      {
        callbackInvoked = true;
        if (throwFromCallback)
        {
          throw new InvalidOperationException("The world slot could not be released.");
        }

        return false;
      });
    }
    catch (InvalidOperationException)
    {
      callbackThrew = true;
    }

    Require(
      callbackInvoked && (throwFromCallback ? callbackThrew : !pickup.Applied) &&
      fixture.Owner.TryGetItemAtSlot(0, out PlayerInventoryItemSnapshot unchangedExisting) &&
      unchangedExisting.Entity == existing.Entity && unchangedExisting.Stack == existing.Stack &&
      fixture.Items.TryGetInventoryRelation(existing.Entity, out ItemInventoryRelationComponent existingRelation) &&
      existingRelation.PlayerReference == fixture.PlayerReference && existingRelation.SlotIndex == 0 &&
      fixture.Items.TryGet(drop.Entity, out PlayerInventoryItemSnapshot unchangedIncoming) &&
      unchangedIncoming.Stack == drop.Stack &&
      !fixture.Items.TryGetInventoryRelation(drop.Entity, out _) &&
      HasWorldPresence(fixture, drop.Entity) && fixture.Runtime.EntityCount == 3,
      "a failed full merge must restore destination quantity and preserve the incoming world Item without a false relation");
  }

  private static void VerifyPickupEffectOrder()
  {
    var effects = new RecordingEffectPort(accept: true);
    using var fixture = new Fixture(effects);
    PlayerInventoryItemSnapshot drop = CreateWorldDrop(fixture, stack: 1);
    PlayerInventoryPickupResult pickup = fixture.Owner.TryPickup(drop, static () => true);

    Require(
      pickup.Applied && pickup.EffectsApplied &&
      effects.Intents.Select(static intent => intent.Kind).SequenceEqual(
      [
        PlayerInventoryEffectIntentKind.PickupSound,
        PlayerInventoryEffectIntentKind.PickupLog,
        PlayerInventoryEffectIntentKind.PickupText,
        PlayerInventoryEffectIntentKind.Achievement,
        PlayerInventoryEffectIntentKind.PostAction,
      ]),
      "pickup effects must retain sound, log, text, achievement, and post-action ordering");
  }

  private static void VerifyRejectedPickupKeepsWorldPresence()
  {
    using (var fixture = new Fixture())
    {
      PlayerInventoryItemSnapshot drop = CreateWorldDrop(fixture, stack: 1);
      int releaseAttempts = 0;
      PlayerInventoryPickupResult rejected = fixture.Owner.TryPickup(drop, () =>
      {
        releaseAttempts++;
        return false;
      });
      RuntimeEntityHandle handle = Resolve(fixture.Runtime, drop.Entity);
      Require(
        !rejected.Applied && releaseAttempts == 1 && fixture.Owner.CountUsedSlots() == 0,
        "a failed world-slot release must reject pickup without adding a Player relation");
      Require(
        fixture.Runtime.Has<WorldItemComponent>(handle) &&
        fixture.Runtime.Has<WorldItemStateComponent>(handle) &&
        fixture.Runtime.Has<WorldItemReservationComponent>(handle) &&
        fixture.Runtime.Has<LocationComponent>(handle) &&
        fixture.Runtime.Has<VelocityComponent>(handle) &&
        fixture.Runtime.Has<ColliderComponent>(handle) &&
        fixture.Items.TryGet(drop.Entity, out PlayerInventoryItemSnapshot unchanged) &&
        unchanged.Stack == drop.Stack &&
        !fixture.Items.TryGetInventoryRelation(drop.Entity, out _),
        "failed pickup must restore the complete world-item component composition");
    }

    using (var fixture = new Fixture())
    {
      for (int index = 0; index < PlayerInventorySlotsComponent.MainInventorySlotCount; index++)
      {
        PlayerInventoryItemSnapshot fullStack = fixture.Items.CreateMaximumStack(23);
        SetSlot(fixture, index, fullStack.Entity);
      }

      PlayerInventoryItemSnapshot drop = CreateWorldDrop(fixture, stack: 1);
      bool released = false;
      PlayerInventoryPickupResult rejected = fixture.Owner.TryPickup(drop, () =>
      {
        released = true;
        return true;
      });
      Require(
        !rejected.Applied && !released && fixture.Owner.CountUsedSlots() ==
          PlayerInventorySlotsComponent.MainInventorySlotCount,
        "full inventory rejection must leave the world projection and Player slots unchanged");
      Require(
        fixture.Items.TryGetWorldItem(drop.Entity, out _) &&
        fixture.Items.TryGet(drop.Entity, out PlayerInventoryItemSnapshot unchanged) &&
        unchanged.Stack == 1,
        "full inventory rejection must retain the incoming root and quantity");
    }
  }

  private static void VerifyCoinMergeInventoryRelations()
  {
    using var fixture = new Fixture(catalog: BuildCatalogWithCoins());
    PlayerInventoryItemSnapshot copper = fixture.Items.Create(typeId: 71, stack: 99);
    PlayerInventoryItemSnapshot silver = fixture.Items.Create(typeId: 72, stack: 8);
    SetSlot(fixture, 0, copper.Entity);
    SetSlot(fixture, 1, silver.Entity);
    bool sourceRemovalRejectedWhileBorrowed = false;
    Require(
      fixture.Runtime.TryEdit<ItemStackComponent>(
        Resolve(fixture.Runtime, copper.Entity),
        (ref ItemStackComponent _) =>
          sourceRemovalRejectedWhileBorrowed = !fixture.Items.CanRemove(copper.Entity)) &&
      sourceRemovalRejectedWhileBorrowed,
      "coin source deletion preflight must reject a borrowed Item root");
    PlayerInventoryItemSnapshot drop = CreateWorldDrop(fixture, stack: 1, typeId: 71);

    PlayerInventoryPickupResult pickup = fixture.Owner.TryPickup(drop, static () => true);
    Require(
      pickup.Applied && pickup.CoinMergeAttempted && pickup.CoinMerge.Applied &&
      !fixture.Owner.TryGetItemAtSlot(0, out _) &&
      fixture.Owner.TryGetItemAtSlot(1, out PlayerInventoryItemSnapshot mergedSilver) &&
      mergedSilver.Entity == silver.Entity && mergedSilver.TypeId == 72 && mergedSilver.Stack == 9 &&
      fixture.Items.TryGetInventoryRelation(silver.Entity, out ItemInventoryRelationComponent silverRelation) &&
      silverRelation.PlayerReference == fixture.PlayerReference && silverRelation.SlotIndex == 1 &&
      !fixture.Items.TryGet(copper.Entity, out _) && !fixture.Items.TryGet(drop.Entity, out _),
      "coin upgrading must validate both slot relations, preflight source deletion, and retain the destination relation");
  }

  private static ContentCatalog BuildCatalogWithCoins()
  {
    ContentCatalog baseline = SimulationContentBootstrap.Build();
    Require(baseline.Items.TryGet(23, out ItemDefinition gel),
      "the simulation catalog must provide a validated base Item definition for synthetic coin verification");
    ItemDefinition[] coinDefinitions = new[] { 71, 72, 73 }
      .Select(typeId => gel with
      {
        Identity = new ItemIdentityDefinition(typeId, $"verification.coin-{typeId}", "player-item-space"),
        Stack = new ItemStackDefinition(9_999, UniqueStack: false, IsMaterial: false, DefaultStack: 1),
      })
      .ToArray();
    ItemDefinition[] definitions = baseline.Snapshot.Items.DefinitionsByType.Values
      .Concat(coinDefinitions)
      .ToArray();
    Dictionary<int, string> itemIdentities = baseline.Snapshot.Identities.ItemPersistentIdByType
      .ToDictionary(static pair => pair.Key, static pair => pair.Value);
    foreach (ItemDefinition coin in coinDefinitions)
    {
      itemIdentities.Add(coin.Identity.TypeId, coin.Identity.PersistentId);
    }

    var identities = new ContentIdentityCatalog(
      itemIdentities,
      baseline.Snapshot.Identities.NpcPersistentIdByNetId,
      baseline.Snapshot.Identities.ProjectilePersistentIdByType,
      baseline.Snapshot.Identities.NpcBestiaryCreditIdByNetId);
    var snapshot = new ContentCatalogSnapshot(
      catalogRevision: checked(baseline.CatalogRevision + 1),
      sourceKey: "player-item-space-coin-verification",
      items: new ItemDefinitionCatalog(definitions),
      npcs: baseline.Snapshot.Npcs,
      projectiles: baseline.Snapshot.Projectiles,
      identities: identities,
      buffs: baseline.Snapshot.Buffs,
      tiles: baseline.Snapshot.Tiles,
      walls: baseline.Snapshot.Walls,
      recipes: baseline.Snapshot.Recipes,
      recipeGroups: baseline.Snapshot.RecipeGroups,
      dropRules: baseline.Snapshot.DropRules,
      fishingDropRules: baseline.Snapshot.FishingDropRules);
    return ContentCatalogBuildSystem.Build(snapshot);
  }

  private static void VerifyRuntimeWorldItemStoreExpiryAndSpawnRollback()
  {
    var catalog = SimulationContentBootstrap.Build();
    using (var session = new LoadedWorldSession())
    {
      var registry = new RuntimeItemRegistry(catalog, session.EntityRuntime);
      var worldItems = new RuntimeWorldItemStore(session, catalog, registry);
      using var players = new RuntimePlayerStore(session.EntityRuntime, session.IdentityRegistry);
      worldItems.SpawnProbeItem(typeId: 23, stack: 1, position: Vector2.Zero, timeLeft: 1);
      Require(
        worldItems.ActiveCount == 1 && session.EntityRuntime.EntityCount == 1,
        "the runtime world-item store must create an Item root and a world slot together");

      worldItems.Update(tickNumber: 1, players: players);
      Require(
        worldItems.ExpiredCount == 1 && worldItems.ActiveCount == 0 &&
        session.EntityRuntime.EntityCount == 0,
        "world-item expiry must release the slot and remove the expired Item root");
    }

    using (var session = new LoadedWorldSession(maximumWorldItemSlots: 1))
    {
      var registry = new RuntimeItemRegistry(catalog, session.EntityRuntime);
      var worldItems = new RuntimeWorldItemStore(session, catalog, registry);
      Require(
        session.Storage.WorldItems.TryAllocate(new OccupiedWorldItemSlotState(), out _, out _),
        "the bounded session must reserve its only world-item slot before the spawn failure scenario");
      bool allocationFailureReported = false;
      try
      {
        worldItems.SpawnProbeItem(typeId: 23, stack: 1, position: Vector2.Zero, timeLeft: 1);
      }
      catch (InvalidOperationException)
      {
        allocationFailureReported = true;
      }

      Require(
        allocationFailureReported && worldItems.ActiveCount == 1 &&
        session.EntityRuntime.EntityCount == 0,
        "a failed world-slot allocation must remove the Item root created for that spawn");
    }
  }

  private static bool HasWorldPresence(Fixture fixture, ItemEntityRef item)
  {
    RuntimeEntityHandle handle = Resolve(fixture.Runtime, item);
    return fixture.Runtime.Has<WorldItemComponent>(handle) &&
      fixture.Runtime.Has<WorldItemStateComponent>(handle) &&
      fixture.Runtime.Has<WorldItemReservationComponent>(handle) &&
      fixture.Runtime.Has<LocationComponent>(handle) &&
      fixture.Runtime.Has<VelocityComponent>(handle) &&
      fixture.Runtime.Has<ColliderComponent>(handle) &&
      fixture.Items.TryGetWorldItem(item, out _);
  }

  private static void VerifyConsumptionAndStaleRevision()
  {
    using var fixture = new Fixture();
    PlayerInventoryItemSnapshot source = fixture.Items.Create(23, 6);
    SetSlot(fixture, 0, source.Entity);
    Guid consumeCommand = Guid.Parse("00000000-0000-0000-0000-00000000B602");
    Require(
      fixture.Owner.TryConsume(consumeCommand, typeId: 23, amount: 2) &&
      fixture.Owner.TryConsume(consumeCommand, typeId: 23, amount: 2) &&
      fixture.Owner.TryGetItemAtSlot(0, out PlayerInventoryItemSnapshot remaining) &&
      remaining.Stack == 4,
      "a repeated consume command must decrement its item exactly once");

    Require(
      fixture.Owner.TryConsume(
        Guid.Parse("00000000-0000-0000-0000-00000000B603"),
        typeId: 23,
        amount: 4) &&
      !fixture.Owner.TryGetItemAtSlot(0, out _) &&
      !fixture.Items.TryGet(source.Entity, out _),
      "consuming the remaining quantity must clear its Player relation and Item root");

    PlayerInventoryItemSnapshot stale = fixture.Items.Create(23, 10);
    Require(
      fixture.Items.TryUpdate(stale with { Stack = 8 }, stale.MutationRevision) &&
      !fixture.Items.TryUpdate(stale with { Stack = 9 }, stale.MutationRevision) &&
      fixture.Items.TryGet(stale.Entity, out PlayerInventoryItemSnapshot current) &&
      current.Stack == 8,
      "an Item snapshot must not overwrite a stack after its mutation revision changes");
  }

  private static void VerifyReleaseRejectsBorrowedState()
  {
    using var fixture = new Fixture();
    PlayerInventoryItemSnapshot firstItem = fixture.Items.Create(typeId: 23, stack: 4);
    PlayerInventoryItemSnapshot secondItem = fixture.Items.Create(typeId: 23, stack: 5);
    SetSlot(fixture, 0, firstItem.Entity);
    SetSlot(fixture, 1, secondItem.Entity);
    bool playerBorrowRejectedRelease = false;
    bool playerBorrowRejectedValidation = false;
    Require(
      fixture.Runtime.TryEdit<PlayerInventorySlotsComponent>(
        fixture.PlayerHandle,
        (ref PlayerInventorySlotsComponent slots) =>
        {
          playerBorrowRejectedValidation = !fixture.Owner.TryValidateReleaseAllItems();
          try
          {
            fixture.Owner.ReleaseAllItems();
          }
          catch (InvalidOperationException)
          {
            playerBorrowRejectedRelease = true;
          }
        }),
      "the Player root must be borrowable for the release preflight scenario");
    Require(
      playerBorrowRejectedValidation && playerBorrowRejectedRelease &&
      fixture.Owner.TryGetItemAtSlot(0, out PlayerInventoryItemSnapshot afterPlayerBorrowFirst) &&
      afterPlayerBorrowFirst.Entity == firstItem.Entity &&
      fixture.Owner.TryGetItemAtSlot(1, out PlayerInventoryItemSnapshot afterPlayerBorrowSecond) &&
      afterPlayerBorrowSecond.Entity == secondItem.Entity &&
      fixture.Runtime.EntityCount == 3,
      "a borrowed Player root must leave every slot and Item root unchanged when release is rejected");

    bool itemBorrowRejectedRelease = false;
    bool itemBorrowRejectedValidation = false;
    Require(
      fixture.Runtime.TryEdit<ItemStackComponent>(
        Resolve(fixture.Runtime, secondItem.Entity),
        (ref ItemStackComponent _) =>
        {
          itemBorrowRejectedValidation = !fixture.Owner.TryValidateReleaseAllItems();
          try
          {
            fixture.Owner.ReleaseAllItems();
          }
          catch (InvalidOperationException)
          {
            itemBorrowRejectedRelease = true;
          }
        }),
      "the Item root must be borrowable for the release preflight scenario");
    Require(
      itemBorrowRejectedValidation && itemBorrowRejectedRelease &&
      fixture.Owner.TryGetItemAtSlot(0, out PlayerInventoryItemSnapshot afterItemBorrowFirst) &&
      afterItemBorrowFirst.Entity == firstItem.Entity &&
      fixture.Owner.TryGetItemAtSlot(1, out PlayerInventoryItemSnapshot afterItemBorrowSecond) &&
      afterItemBorrowSecond.Entity == secondItem.Entity &&
      fixture.Items.TryGetInventoryRelation(firstItem.Entity, out ItemInventoryRelationComponent firstRelation) &&
      firstRelation.PlayerReference == fixture.PlayerReference && firstRelation.SlotIndex == 0 &&
      fixture.Items.TryGetInventoryRelation(secondItem.Entity, out ItemInventoryRelationComponent secondRelation) &&
      secondRelation.PlayerReference == fixture.PlayerReference && secondRelation.SlotIndex == 1 &&
      fixture.Runtime.EntityCount == 3,
      "borrowing the later Item must preserve the earlier Item, every slot, and both reverse relations");

    fixture.Owner.ReleaseAllItems();
    Require(
      !fixture.Items.TryGet(firstItem.Entity, out _) &&
      !fixture.Items.TryGet(secondItem.Entity, out _) &&
      fixture.Runtime.EntityCount == 1,
      "retrying release after both borrows end must remove every Item root without residue");
  }

  private static PlayerInventoryItemSnapshot CreateWorldDrop(
    Fixture fixture,
    int stack,
    int typeId = 23)
  {
    return fixture.Items.CreateWorldDrop(
      typeId,
      stack: stack,
      x: 0f,
      y: 0f,
      velocityX: 0f,
      velocityY: 0f,
      collider: new ColliderComponent(16f, 16f),
      spawnedAtTick: 0,
      timeLeftTicks: 6_000,
      replicationId: new ReplicationId(1));
  }

  private static void SetSlot(Fixture fixture, int slot, ItemEntityRef item)
  {
    Require(
      fixture.Items.TrySetInventoryRelation(
        item,
        fixture.PlayerReference,
        ItemInventorySlotKind.MainInventory,
        slot) &&
      fixture.Runtime.TryEdit<PlayerInventorySlotsComponent>(
        fixture.PlayerHandle,
        (ref PlayerInventorySlotsComponent slots) => slots.MainInventorySlots[slot] = item),
      "Player inventory slot and Item reverse relation must commit together");
  }

  private static RuntimeEntityHandle CreatePlayer(EntityRuntime runtime)
  {
    RuntimeEntityHandle handle = runtime.CreateEntity();
    Require(
      runtime.TryAttach(handle, new PlayerInventorySlotsComponent()) &&
      runtime.TryAttach(handle, new LocationComponent(0f, 0f)) &&
      runtime.TryAttach(handle, new ColliderComponent(20f, 42f)) &&
      runtime.TryPublishEntity(handle),
      "a Player root must attach its inventory and source geometry before publication");
    return handle;
  }

  private static RuntimeEntityHandle Resolve(EntityRuntime runtime, ItemEntityRef item)
  {
    Require(runtime.TryResolve(item.Reference, out RuntimeEntityHandle handle),
      "the typed Item reference must resolve in its issuing runtime");
    return handle;
  }

  private static void Require(bool condition, string message)
  {
    if (!condition)
    {
      throw new InvalidOperationException($"Assertion failed: {message}.");
    }
  }

  private sealed class Fixture : IDisposable
  {
    public Fixture(
      IPlayerInventoryEffectPort? effects = null,
      ContentCatalog? catalog = null)
    {
      ContentCatalog content = catalog ?? SimulationContentBootstrap.Build();
      Items = new RuntimeItemRegistry(content, Runtime);
      PlayerHandle = CreatePlayer(Runtime);
      Require(
        Runtime.TryGetReference(PlayerHandle, EntityReferenceScope.Player, out EntityReference playerReference),
        "Player root must expose its scoped reference");
      PlayerReference = playerReference;
      Owner = new RuntimePlayerInventoryOwner(Runtime, PlayerHandle, Items, 0, effects);
    }

    public EntityRuntime Runtime { get; } = new();

    public RuntimeEntityHandle PlayerHandle { get; }

    public EntityReference PlayerReference { get; }

    public RuntimeItemRegistry Items { get; }

    public RuntimePlayerInventoryOwner Owner { get; }

    public void Dispose()
    {
      Runtime.Dispose();
    }
  }

  private sealed class RecordingEffectPort(bool accept) : IPlayerInventoryEffectPort
  {
    public List<PlayerInventoryEffectIntent> Intents { get; } = [];

    public Action<PlayerInventoryEffectIntent>? OnApply { get; set; }

    public bool TryApply(in PlayerInventoryEffectIntent intent)
    {
      Intents.Add(intent);
      OnApply?.Invoke(intent);
      return accept;
    }
  }

  private sealed class OccupiedWorldItemSlotState : WorldEntityState
  {
  }
}
