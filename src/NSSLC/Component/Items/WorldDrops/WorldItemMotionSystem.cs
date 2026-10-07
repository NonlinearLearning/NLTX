namespace Terraria.Items;

/// <summary>Validates and commits revisioned world-item motion state.</summary>
/// <remarks>
/// The caller owns thread affinity and resolves the current entity through EntityRuntime.
/// This system owns the item's lifecycle, identity, and synchronized-state revision rules.
/// </remarks>
public sealed class WorldItemMotionSystem
{
  public WorldItemMotionResult Apply(
    in WorldItemMotionCommand command,
    RuntimeEntityId currentItemEntity,
    ReplicationId currentReplicationId,
    WorldItemComponent worldItem,
    WorldPosition currentPosition,
    WorldVector currentVelocity)
  {
    ArgumentNullException.ThrowIfNull(worldItem);

    WorldItemMotionRejectionReason validationError = ValidateStateUpdate(
      command.ItemEntity,
      currentItemEntity,
      command.ReplicationId,
      currentReplicationId,
      command.CurrentTick,
      command.ExpectedRevision,
      worldItem,
      command.Position,
      command.Velocity,
      currentPosition,
      currentVelocity);
    if (validationError != WorldItemMotionRejectionReason.None)
    {
      return WorldItemMotionResult.Rejected(
        worldItem.Revision,
        validationError);
    }

    bool changed = command.Position != currentPosition ||
      command.Velocity != currentVelocity;
    if (!changed)
    {
      return new WorldItemMotionResult(
        Applied: true,
        Changed: false,
        Revision: worldItem.Revision,
        RejectionReason: WorldItemMotionRejectionReason.None);
    }

    if (worldItem.Revision == long.MaxValue)
    {
      return WorldItemMotionResult.Rejected(
        worldItem.Revision,
        WorldItemMotionRejectionReason.RevisionExhausted);
    }

    worldItem.Revision++;
    return new WorldItemMotionResult(
      Applied: true,
      Changed: true,
      Revision: worldItem.Revision,
      RejectionReason: WorldItemMotionRejectionReason.None);
  }

  /// <summary>
  /// Applies one authenticated network state update, including motion and Steam's
  /// shimmer and enemy-grab-delay extensions, as one world-item revision.
  /// </summary>
  public WorldItemMotionResult ApplySynchronizedState(
    in WorldItemSynchronizedStateCommand command,
    RuntimeEntityId currentItemEntity,
    ReplicationId currentReplicationId,
    WorldItemComponent worldItem,
    WorldPosition currentPosition,
    WorldVector currentVelocity)
  {
    ArgumentNullException.ThrowIfNull(worldItem);

    WorldItemMotionRejectionReason validationError = ValidateStateUpdate(
      command.ItemEntity,
      currentItemEntity,
      command.ReplicationId,
      currentReplicationId,
      command.CurrentTick,
      command.ExpectedRevision,
      worldItem,
      command.Position,
      command.Velocity,
      currentPosition,
      currentVelocity,
      command.ShimmerTime);
    if (validationError != WorldItemMotionRejectionReason.None)
    {
      return WorldItemMotionResult.Rejected(
        worldItem.Revision,
        validationError);
    }

    bool changed = command.Position != currentPosition ||
      command.Velocity != currentVelocity ||
      command.IsShimmered != worldItem.IsShimmered ||
      command.ShimmerTime != worldItem.ShimmerTime ||
      command.EnemyGrabDelayTime != worldItem.EnemyGrabDelayTime;
    if (!changed)
    {
      return new WorldItemMotionResult(
        Applied: true,
        Changed: false,
        Revision: worldItem.Revision,
        RejectionReason: WorldItemMotionRejectionReason.None);
    }

    if (worldItem.Revision == long.MaxValue)
    {
      return WorldItemMotionResult.Rejected(
        worldItem.Revision,
        WorldItemMotionRejectionReason.RevisionExhausted);
    }

    worldItem.IsShimmered = command.IsShimmered;
    worldItem.ShimmerTime = command.ShimmerTime;
    worldItem.EnemyGrabDelayTime = command.EnemyGrabDelayTime;
    worldItem.Revision++;
    return new WorldItemMotionResult(
      Applied: true,
      Changed: true,
      Revision: worldItem.Revision,
      RejectionReason: WorldItemMotionRejectionReason.None);
  }

  private static WorldItemMotionRejectionReason ValidateStateUpdate(
    RuntimeEntityId expectedItemEntity,
    RuntimeEntityId currentItemEntity,
    ReplicationId expectedReplicationId,
    ReplicationId currentReplicationId,
    long currentTick,
    long expectedRevision,
    WorldItemComponent worldItem,
    WorldPosition requestedPosition,
    WorldVector requestedVelocity,
    WorldPosition currentPosition,
    WorldVector currentVelocity,
    float? synchronizedShimmerTime = null)
  {
    if (!MatchesItem(expectedItemEntity, currentItemEntity, expectedReplicationId,
          currentReplicationId))
    {
      return WorldItemMotionRejectionReason.StaleWorldItem;
    }

    if (currentTick < 0 || currentTick < worldItem.SpawnedAtTick)
    {
      return WorldItemMotionRejectionReason.InvalidTick;
    }

    if (worldItem.IsExpiredAt(currentTick))
    {
      return WorldItemMotionRejectionReason.ExpiredWorldItem;
    }

    if (!IsFinite(requestedPosition) ||
      !IsFinite(requestedVelocity) ||
      !IsFinite(currentPosition) ||
      !IsFinite(currentVelocity))
    {
      return WorldItemMotionRejectionReason.InvalidMotion;
    }

    if (synchronizedShimmerTime.HasValue &&
      (!float.IsFinite(synchronizedShimmerTime.Value) ||
        synchronizedShimmerTime.Value < 0.0f ||
        !float.IsFinite(worldItem.ShimmerTime) ||
        worldItem.ShimmerTime < 0.0f))
    {
      return WorldItemMotionRejectionReason.InvalidSynchronizedState;
    }

    if (worldItem.Revision < 0 || expectedRevision != worldItem.Revision)
    {
      return WorldItemMotionRejectionReason.RevisionConflict;
    }

    return WorldItemMotionRejectionReason.None;
  }

  private static bool MatchesItem(
    RuntimeEntityId expectedItemEntity,
    RuntimeEntityId currentItemEntity,
    ReplicationId expectedReplicationId,
    ReplicationId currentReplicationId)
  {
    return !expectedItemEntity.IsEmpty &&
      expectedItemEntity.Reference.Scope == Terraria.Relationships.EntityReferenceScope.Item &&
      expectedItemEntity == currentItemEntity &&
      expectedReplicationId.IsAssigned &&
      expectedReplicationId == currentReplicationId;
  }

  private static bool IsFinite(WorldPosition position) =>
    float.IsFinite(position.X) && float.IsFinite(position.Y);

  private static bool IsFinite(WorldVector velocity) =>
    float.IsFinite(velocity.X) && float.IsFinite(velocity.Y);

}
