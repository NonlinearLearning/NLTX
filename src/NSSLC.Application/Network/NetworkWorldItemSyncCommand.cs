namespace Terraria.Network;

/// <summary>Authenticated client state for one already-authorized world item.</summary>
public readonly record struct NetworkWorldItemSyncCommand(
  short ItemIndex,
  short ItemType,
  short Stack,
  byte Prefix,
  byte StateFlags,
  float PositionX,
  float PositionY,
  float VelocityX,
  float VelocityY,
  bool? Shimmered = null,
  float? ShimmerTime = null,
  byte? EnemyGrabDelayTime = null);
