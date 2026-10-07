using EntityEcs;
using EntityEcs.Components;
using Terraria.Items;
using Terraria.Relationships;

namespace Terraria.Items.Verification;

internal static class WorldItemReservationVerification
{
  private static readonly ReservationId FirstReservation = new(
    Guid.Parse("00000000-0000-0000-0000-000000000101"));
  private static readonly ReservationId SecondReservation = new(
    Guid.Parse("00000000-0000-0000-0000-000000000202"));

  public static void Run()
  {
    using var runtime = new EntityRuntime();
    RuntimeEntityHandle itemHandle = runtime.CreateEntity();
    RuntimeEntityHandle firstPlayerHandle = runtime.CreateEntity();
    RuntimeEntityHandle secondPlayerHandle = runtime.CreateEntity();

    Assert(runtime.TryAttach(itemHandle, new LocationComponent(128.0f, 256.0f)),
      "EntityRuntime attaches world-item location");
    Assert(runtime.TryAttach(itemHandle, new VelocityComponent(0.0f, 0.0f)),
      "EntityRuntime attaches world-item velocity");
    Assert(runtime.TryAttach(itemHandle, new ColliderComponent(16.0f, 16.0f)),
      "EntityRuntime attaches the source 16×16 world-item collider");
    Assert(runtime.TryAttach(itemHandle, new WorldItemComponent(
      spawnedAtTick: 20,
      despawnAtTick: 500)),
      "EntityRuntime attaches world-item lifetime");
    Assert(runtime.TryAttach(itemHandle,
      new WorldItemStateComponent(new ReplicationId(41))),
      "EntityRuntime attaches replication state");
    Assert(runtime.TryAttach(itemHandle, new WorldItemReservationComponent()),
      "EntityRuntime attaches reservation state");
    Assert(runtime.TryPublishEntity(itemHandle),
      "EntityRuntime publishes the world item");
    Assert(runtime.TryPublishEntity(firstPlayerHandle),
      "EntityRuntime publishes first player identity");
    Assert(runtime.TryPublishEntity(secondPlayerHandle),
      "EntityRuntime publishes second player identity");

    RuntimeEntityId itemEntity = GetEntityId(
      runtime, itemHandle, EntityReferenceScope.Item);
    RuntimeEntityId firstPlayer = GetEntityId(
      runtime, firstPlayerHandle, EntityReferenceScope.Player);
    RuntimeEntityId secondPlayer = GetEntityId(
      runtime, secondPlayerHandle, EntityReferenceScope.Player);
    Assert(itemEntity.Reference.Scope == EntityReferenceScope.Item &&
      itemEntity.Reference.RuntimeId == runtime.RuntimeId,
      "RuntimeEntityId retains the item scope and issuing runtime");
    Assert(firstPlayer.Reference.Scope == EntityReferenceScope.Player &&
      firstPlayer.Reference.RuntimeId == runtime.RuntimeId,
      "reservation owners retain their Player scope and issuing runtime");
    var system = new WorldItemReservationSystem();
    var firstClaim = new WorldItemReservationCommand(
      itemEntity,
      new ReplicationId(41),
      firstPlayer,
      FirstReservation,
      CurrentTick: 100,
      DurationTicks: 30,
      ExpectedReservationRevision: 0);

    WorldItemReservationResult assigned = Reserve(
      runtime, itemHandle, itemEntity, system, firstClaim);
    Assert(assigned.Applied && !assigned.IsIdempotent,
      "first owner claim changes the reservation");
    Assert(assigned.ReservationRevision == 1,
      "first owner claim advances reservation revision once");
    AssertReservation(runtime, itemHandle, firstPlayer, FirstReservation,
      expectedExpiry: 130, expectedRevision: 1);

    WorldItemReservationResult retried = Reserve(
      runtime, itemHandle, itemEntity, system, firstClaim);
    Assert(retried.Applied && retried.IsIdempotent,
      "replaying the same active reservation is idempotent");
    Assert(retried.ReservationRevision == 1,
      "idempotent retry does not advance reservation revision");

    WorldItemReservationResult wrongEntity = Reserve(
      runtime,
      itemHandle,
      itemEntity,
      system,
      firstClaim with { ItemEntity = secondPlayer });
    Assert(!wrongEntity.Applied && wrongEntity.RejectionReason ==
      WorldItemReservationRejectionReason.StaleWorldItem,
      "a command for a different scoped entity cannot reserve this item");

    RuntimeEntityId sameUuidFromAnotherRuntime = RuntimeEntityId.FromEntityReference(
      new EntityReference(
        itemEntity.Reference.EntityId,
        new EntityRuntimeId(Guid.NewGuid()),
        EntityReferenceScope.Item));
    WorldItemReservationResult wrongRuntime = Reserve(
      runtime,
      itemHandle,
      itemEntity,
      system,
      firstClaim with { ItemEntity = sameUuidFromAnotherRuntime });
    Assert(!wrongRuntime.Applied && wrongRuntime.RejectionReason ==
      WorldItemReservationRejectionReason.StaleWorldItem,
      "the same entity UUID from another runtime cannot reserve this item");

    RuntimeEntityId samePlayerUuidFromAnotherRuntime = RuntimeEntityId.FromEntityReference(
      new EntityReference(
        firstPlayer.Reference.EntityId,
        new EntityRuntimeId(Guid.NewGuid()),
        EntityReferenceScope.Player));
    WorldItemReservationResult foreignRuntimePlayer = Reserve(
      runtime,
      itemHandle,
      itemEntity,
      system,
      firstClaim with { PlayerEntity = samePlayerUuidFromAnotherRuntime });
    Assert(!foreignRuntimePlayer.Applied && foreignRuntimePlayer.RejectionReason ==
      WorldItemReservationRejectionReason.InvalidPlayer,
      "a Player reference from another runtime cannot claim this item's reservation");

    WorldItemReservationResult inactive = Reserve(
      runtime,
      itemHandle,
      itemEntity,
      system,
      firstClaim with
      {
        CurrentTick = 101,
        ExpectedReservationRevision = 1
      },
      isActive: false);
    Assert(!inactive.Applied && inactive.RejectionReason ==
      WorldItemReservationRejectionReason.InactiveWorldItem,
      "the owner must confirm active world presence before reservation");
    AssertReservation(runtime, itemHandle, firstPlayer, FirstReservation,
      expectedExpiry: 130, expectedRevision: 1);

    WorldItemReservationResult contested = Reserve(
      runtime,
      itemHandle,
      itemEntity,
      system,
      firstClaim with
      {
        PlayerEntity = secondPlayer,
        ReservationId = SecondReservation,
        CurrentTick = 101,
        ExpectedReservationRevision = 1
      });
    Assert(!contested.Applied && contested.RejectionReason ==
      WorldItemReservationRejectionReason.ReservedByAnotherPlayer,
      "another player cannot take an active reservation");
    AssertReservation(runtime, itemHandle, firstPlayer, FirstReservation,
      expectedExpiry: 130, expectedRevision: 1);

    WorldItemReservationResult wrongOwnerRelease = Release(
      runtime,
      itemHandle,
      itemEntity,
      system,
      new WorldItemReservationReleaseCommand(
        itemEntity,
        new ReplicationId(41),
        secondPlayer,
        FirstReservation,
        CurrentTick: 102,
        ExpectedReservationRevision: 1));
    Assert(!wrongOwnerRelease.Applied && wrongOwnerRelease.RejectionReason ==
      WorldItemReservationRejectionReason.NotReservationOwner,
      "a non-owner cannot release the reservation");
    AssertReservation(runtime, itemHandle, firstPlayer, FirstReservation,
      expectedExpiry: 130, expectedRevision: 1);

    WorldItemReservationResult expiredReassignment = Reserve(
      runtime,
      itemHandle,
      itemEntity,
      system,
      firstClaim with
      {
        PlayerEntity = secondPlayer,
        ReservationId = SecondReservation,
        CurrentTick = 130,
        ExpectedReservationRevision = 1
      });
    Assert(expiredReassignment.Applied && !expiredReassignment.IsIdempotent,
      "a new player can reserve the item when the prior claim expires");
    AssertReservation(runtime, itemHandle, secondPlayer, SecondReservation,
      expectedExpiry: 160, expectedRevision: 2);

    WorldItemReservationResult staleRelease = Release(
      runtime,
      itemHandle,
      itemEntity,
      system,
      new WorldItemReservationReleaseCommand(
        itemEntity,
        new ReplicationId(41),
        firstPlayer,
        FirstReservation,
        CurrentTick: 131,
        ExpectedReservationRevision: 1));
    Assert(!staleRelease.Applied && staleRelease.RejectionReason ==
      WorldItemReservationRejectionReason.NotReservationOwner,
      "an old owner cannot release a reassigned reservation");

    WorldItemReservationResult released = Release(
      runtime,
      itemHandle,
      itemEntity,
      system,
      new WorldItemReservationReleaseCommand(
        itemEntity,
        new ReplicationId(41),
        secondPlayer,
        SecondReservation,
        CurrentTick: 132,
        ExpectedReservationRevision: 2));
    Assert(released.Applied && released.ReservationRevision == 3,
      "the current owner can release the reservation");
    AssertReservation(runtime, itemHandle, RuntimeEntityId.Empty, null,
      expectedExpiry: null, expectedRevision: 3);

    WorldItemReservationResult expiredItem = Reserve(
      runtime,
      itemHandle,
      itemEntity,
      system,
      firstClaim with
      {
        CurrentTick = 500,
        ExpectedReservationRevision = 3
      });
    Assert(!expiredItem.Applied && expiredItem.RejectionReason ==
      WorldItemReservationRejectionReason.ExpiredWorldItem,
      "an expired world item cannot receive a reservation");

    bool changedReplicationId = runtime.TryEdit<WorldItemStateComponent>(
      itemHandle,
      (ref WorldItemStateComponent state) =>
      {
        state.ReplicationId = new ReplicationId(42);
      });
    Assert(changedReplicationId, "EntityRuntime updates the item replication id");
    WorldItemReservationResult staleSlot = Reserve(
      runtime,
      itemHandle,
      itemEntity,
      system,
      firstClaim with
      {
        CurrentTick = 134,
        ExpectedReservationRevision = 3
      });
    Assert(!staleSlot.Applied && staleSlot.RejectionReason ==
      WorldItemReservationRejectionReason.StaleWorldItem,
      "a stale network replication id cannot reserve a reused slot");
    AssertReservation(runtime, itemHandle, RuntimeEntityId.Empty, null,
      expectedExpiry: null, expectedRevision: 3);
  }

  private static RuntimeEntityId GetEntityId(
    EntityRuntime runtime,
    RuntimeEntityHandle handle,
    EntityReferenceScope scope)
  {
    bool found = runtime.TryGetReference(handle, scope, out EntityReference reference);
    Assert(found, "EntityRuntime returns a scoped entity reference");
    return RuntimeEntityId.FromEntityReference(reference);
  }

  private static WorldItemReservationResult Reserve(
    EntityRuntime runtime,
    RuntimeEntityHandle itemHandle,
    RuntimeEntityId itemEntity,
    WorldItemReservationSystem system,
    in WorldItemReservationCommand command,
    bool? isActive = null)
  {
    WorldItemReservationCommand ownedCommand = command;
    bool activeWorldItem = isActive ?? HasActiveWorldItem(runtime, itemHandle);
    WorldItemReservationResult result = default;
    bool edited = runtime.TryEditComponents<
      WorldItemStateComponent,
      WorldItemComponent,
      WorldItemReservationComponent>(
      itemHandle,
      (ref WorldItemStateComponent itemState,
       ref WorldItemComponent worldItem,
       ref WorldItemReservationComponent reservation) =>
      {
        result = system.Reserve(
          ownedCommand,
          itemEntity,
          activeWorldItem,
          itemState,
          worldItem,
          reservation);
      });
    Assert(edited, "EntityRuntime edits world-item reservation components");
    return result;
  }

  private static WorldItemReservationResult Release(
    EntityRuntime runtime,
    RuntimeEntityHandle itemHandle,
    RuntimeEntityId itemEntity,
    WorldItemReservationSystem system,
    in WorldItemReservationReleaseCommand command)
  {
    WorldItemReservationReleaseCommand ownedCommand = command;
    bool activeWorldItem = HasActiveWorldItem(runtime, itemHandle);
    WorldItemReservationResult result = default;
    bool edited = runtime.TryEditComponents<
      WorldItemStateComponent,
      WorldItemComponent,
      WorldItemReservationComponent>(
      itemHandle,
      (ref WorldItemStateComponent itemState,
       ref WorldItemComponent worldItem,
       ref WorldItemReservationComponent reservation) =>
      {
        result = system.Release(
          ownedCommand,
          itemEntity,
          activeWorldItem,
          itemState,
          worldItem,
          reservation);
      });
    Assert(edited, "EntityRuntime edits world-item reservation components");
    return result;
  }

  private static bool HasActiveWorldItem(
    EntityRuntime runtime,
    RuntimeEntityHandle itemHandle)
  {
    return runtime.TryGetStatus(itemHandle, out EntityRuntimeStatus status) &&
      status == EntityRuntimeStatus.Running &&
      runtime.Has<WorldItemStateComponent>(itemHandle) &&
      runtime.Has<WorldItemComponent>(itemHandle) &&
      runtime.Has<LocationComponent>(itemHandle) &&
      runtime.Has<VelocityComponent>(itemHandle) &&
      runtime.Has<ColliderComponent>(itemHandle);
  }

  private static void AssertReservation(
    EntityRuntime runtime,
    RuntimeEntityHandle itemHandle,
    RuntimeEntityId owner,
    ReservationId? reservationId,
    long? expectedExpiry,
    long expectedRevision)
  {
    bool inspected = runtime.TryInspect<WorldItemReservationComponent>(
      itemHandle,
      (in WorldItemReservationComponent state) =>
      {
        Assert(state.ReservedFor == (owner.IsEmpty ? null : owner),
          "reservation owner state");
        Assert(state.ReservationId == reservationId, "reservation id state");
        Assert(state.ReservationExpiresAt == expectedExpiry,
          "reservation expiration state");
        Assert(state.ReservationRevision == expectedRevision,
          "reservation revision state");
      });
    Assert(inspected, "EntityRuntime inspects reservation state");
  }

  private static void Assert(bool condition, string name)
  {
    if (!condition)
    {
      throw new InvalidOperationException($"Assertion failed: {name}.");
    }
  }
}
