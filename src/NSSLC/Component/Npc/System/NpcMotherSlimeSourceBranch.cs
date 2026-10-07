namespace Terraria.Npc;

[Flags]
public enum NpcMotherSlimeSourceBranch
{
  None = 0,
  WetMovement = 1 << 0,
  FrozenSentinel = 1 << 1,
  TargetInitialization = 1 << 2,
  GroundedCounter = 1 << 3,
  DirectionTurn = 1 << 4,
  JumpImpulse = 1 << 5,
  NetworkSync = 1 << 6,
  TargetReacquire = 1 << 7,
  AirborneMovement = 1 << 8,
  Ai2Cooldown = 1 << 9,
}
