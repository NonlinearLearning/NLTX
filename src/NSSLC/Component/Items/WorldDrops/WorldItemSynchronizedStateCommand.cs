namespace Terraria.Items;

/// <summary>A revisioned world-item state update from its authenticated owner.</summary>
public readonly record struct WorldItemSynchronizedStateCommand(
  RuntimeEntityId ItemEntity,
  ReplicationId ReplicationId,
  long CurrentTick,
  long ExpectedRevision,
  WorldPosition Position,
  WorldVector Velocity,
  bool IsShimmered,
  float ShimmerTime,
  byte EnemyGrabDelayTime);
