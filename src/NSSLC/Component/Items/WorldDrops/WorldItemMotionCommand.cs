namespace Terraria.Items;

/// <summary>A trusted motion update for one identified world item revision.</summary>
public readonly record struct WorldItemMotionCommand(
  RuntimeEntityId ItemEntity,
  ReplicationId ReplicationId,
  long CurrentTick,
  long ExpectedRevision,
  WorldPosition Position,
  WorldVector Velocity);
