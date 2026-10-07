namespace Terraria.Network;

/// <summary>Detached authoritative Steam 21 projection produced after a world-item commit.</summary>
public readonly record struct NetworkWorldItemSyncProjection(
  short ItemIndex,
  short ItemType,
  short Stack,
  byte Prefix,
  float PositionX,
  float PositionY,
  float VelocityX,
  float VelocityY,
  bool IsShimmered,
  float ShimmerTime,
  byte EnemyGrabDelayTime);
