using Terraria.Items;
using Terraria.Relationships;

namespace Terraria.Items.Verification;

internal static class WorldItemMotionVerification
{
  public static void Run()
  {
    EntityRuntimeId runtimeId = new(Guid.NewGuid());
    RuntimeEntityId itemEntity = RuntimeEntityId.FromEntityReference(
      new EntityReference(
        new EntityUuid(Guid.NewGuid()),
        runtimeId,
        EntityReferenceScope.Item));
    var replicationId = new ReplicationId(41);
    var worldItem = new WorldItemComponent(spawnedAtTick: 20, despawnAtTick: 200);
    var system = new WorldItemMotionSystem();
    var position = new WorldPosition(10.0f, 15.0f);
    var velocity = new WorldVector(1.0f, 2.0f);
    var update = new WorldItemMotionCommand(
      itemEntity,
      replicationId,
      CurrentTick: 100,
      ExpectedRevision: 0,
      new WorldPosition(20.0f, 30.0f),
      new WorldVector(4.0f, -2.0f));

    WorldItemMotionResult applied = system.Apply(
      update,
      itemEntity,
      replicationId,
      worldItem,
      position,
      velocity);
    Assert(applied.Applied && applied.Changed && applied.Revision == 1 &&
      worldItem.Revision == 1,
      "a valid motion update advances the world-item revision once");

    WorldItemMotionResult unchanged = system.Apply(
      update with { ExpectedRevision = 1 },
      itemEntity,
      replicationId,
      worldItem,
      update.Position,
      update.Velocity);
    Assert(unchanged.Applied && !unchanged.Changed && unchanged.Revision == 1 &&
      worldItem.Revision == 1,
      "an unchanged motion update does not advance the revision");

    WorldItemMotionResult staleRevision = system.Apply(
      update,
      itemEntity,
      replicationId,
      worldItem,
      update.Position,
      update.Velocity);
    Assert(!staleRevision.Applied && staleRevision.RejectionReason ==
      WorldItemMotionRejectionReason.RevisionConflict && worldItem.Revision == 1,
      "a stale motion revision is rejected without changing item state");

    RuntimeEntityId anotherItem = RuntimeEntityId.FromEntityReference(
      new EntityReference(
        new EntityUuid(Guid.NewGuid()),
        runtimeId,
        EntityReferenceScope.Item));
    WorldItemMotionResult staleItem = system.Apply(
      update with { ItemEntity = anotherItem, ExpectedRevision = 1 },
      itemEntity,
      replicationId,
      worldItem,
      update.Position,
      update.Velocity);
    Assert(!staleItem.Applied && staleItem.RejectionReason ==
      WorldItemMotionRejectionReason.StaleWorldItem,
      "a motion command for another item identity is rejected");

    WorldItemMotionResult staleReplication = system.Apply(
      update with { ExpectedRevision = 1 },
      itemEntity,
      new ReplicationId(42),
      worldItem,
      update.Position,
      update.Velocity);
    Assert(!staleReplication.Applied && staleReplication.RejectionReason ==
      WorldItemMotionRejectionReason.StaleWorldItem,
      "a stale network replication identity is rejected");

    WorldItemMotionResult invalidTick = system.Apply(
      update with { CurrentTick = 19, ExpectedRevision = 1 },
      itemEntity,
      replicationId,
      worldItem,
      update.Position,
      update.Velocity);
    Assert(!invalidTick.Applied && invalidTick.RejectionReason ==
      WorldItemMotionRejectionReason.InvalidTick,
      "motion cannot be applied before the item's spawn tick");

    WorldItemMotionResult expired = system.Apply(
      update with { CurrentTick = 200, ExpectedRevision = 1 },
      itemEntity,
      replicationId,
      worldItem,
      update.Position,
      update.Velocity);
    Assert(!expired.Applied && expired.RejectionReason ==
      WorldItemMotionRejectionReason.ExpiredWorldItem,
      "motion cannot be applied after the item's despawn tick");

    WorldItemMotionResult nonFinitePosition = system.Apply(
      update with
      {
        ExpectedRevision = 1,
        Position = new WorldPosition(float.NaN, 30.0f)
      },
      itemEntity,
      replicationId,
      worldItem,
      update.Position,
      update.Velocity);
    Assert(!nonFinitePosition.Applied && nonFinitePosition.RejectionReason ==
      WorldItemMotionRejectionReason.InvalidMotion,
      "non-finite requested motion is rejected");

    WorldItemMotionResult nonFiniteCurrentVelocity = system.Apply(
      update with { ExpectedRevision = 1 },
      itemEntity,
      replicationId,
      worldItem,
      update.Position,
      new WorldVector(float.PositiveInfinity, 0.0f));
    Assert(!nonFiniteCurrentVelocity.Applied && nonFiniteCurrentVelocity.RejectionReason ==
      WorldItemMotionRejectionReason.InvalidMotion,
      "non-finite current motion is rejected before committing an update");

    var synchronizedState = new WorldItemSynchronizedStateCommand(
      itemEntity,
      replicationId,
      CurrentTick: 100,
      ExpectedRevision: 1,
      update.Position,
      update.Velocity,
      IsShimmered: true,
      ShimmerTime: 0.5f,
      EnemyGrabDelayTime: 12);
    WorldItemMotionResult synchronized = system.ApplySynchronizedState(
      synchronizedState,
      itemEntity,
      replicationId,
      worldItem,
      update.Position,
      update.Velocity);
    Assert(synchronized.Applied && synchronized.Changed &&
      synchronized.Revision == 2 && worldItem.Revision == 2 &&
      worldItem.IsShimmered && worldItem.ShimmerTime == 0.5f &&
      worldItem.EnemyGrabDelayTime == 12,
      "a synchronized shimmer and grab-delay update commits as one revision");

    WorldItemMotionResult synchronizedReplay = system.ApplySynchronizedState(
      synchronizedState with { ExpectedRevision = 2 },
      itemEntity,
      replicationId,
      worldItem,
      update.Position,
      update.Velocity);
    Assert(synchronizedReplay.Applied && !synchronizedReplay.Changed &&
      synchronizedReplay.Revision == 2 && worldItem.Revision == 2,
      "an exact synchronized-state replay does not advance the revision");

    WorldItemMotionResult invalidSynchronizedState = system.ApplySynchronizedState(
      synchronizedState with { ExpectedRevision = 2, ShimmerTime = float.NaN },
      itemEntity,
      replicationId,
      worldItem,
      update.Position,
      update.Velocity);
    Assert(!invalidSynchronizedState.Applied &&
      invalidSynchronizedState.RejectionReason ==
        WorldItemMotionRejectionReason.InvalidSynchronizedState &&
      worldItem.Revision == 2 && worldItem.ShimmerTime == 0.5f &&
      worldItem.EnemyGrabDelayTime == 12,
      "a non-finite shimmer timer is rejected without a partial update");

    WorldItemMotionResult negativeShimmerTime = system.ApplySynchronizedState(
      synchronizedState with { ExpectedRevision = 2, ShimmerTime = -0.01f },
      itemEntity,
      replicationId,
      worldItem,
      update.Position,
      update.Velocity);
    Assert(!negativeShimmerTime.Applied &&
      negativeShimmerTime.RejectionReason ==
        WorldItemMotionRejectionReason.InvalidSynchronizedState &&
      worldItem.Revision == 2 && worldItem.ShimmerTime == 0.5f,
      "a negative shimmer timer is rejected without a partial update");

    var exhaustedItem = new WorldItemComponent(
      spawnedAtTick: 20,
      revision: long.MaxValue);
    WorldItemMotionResult exhaustedRevision = system.Apply(
      update with { ExpectedRevision = long.MaxValue },
      itemEntity,
      replicationId,
      exhaustedItem,
      position,
      velocity);
    Assert(!exhaustedRevision.Applied && exhaustedRevision.RejectionReason ==
      WorldItemMotionRejectionReason.RevisionExhausted &&
      exhaustedItem.Revision == long.MaxValue,
      "revision exhaustion rejects the update without wrapping");
  }

  private static void Assert(bool condition, string description)
  {
    if (!condition)
    {
      throw new InvalidOperationException(description);
    }
  }
}
