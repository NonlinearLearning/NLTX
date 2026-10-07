namespace Terraria.Npc;

[Flags]
public enum NpcFighterSourceBranch
{
  None = 0,
  TargetSelection = 1 << 0,
  DaytimeDespawn = 1 << 1,
  IdleTurn = 1 << 2,
  HorizontalAcceleration = 1 << 3,
  NetworkSync = 1 << 4,
  ObstacleJump = 1 << 5,
  DoorInteraction = 1 << 6,
}
