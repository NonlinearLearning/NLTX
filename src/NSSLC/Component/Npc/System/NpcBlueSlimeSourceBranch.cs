namespace Terraria.Npc;

[Flags]
public enum NpcBlueSlimeSourceBranch
{
  None = 0,
  ContainedItemGeneration = 1 << 0,
  DirectionInitialization = 1 << 1,
  TargetInitialization = 1 << 2,
  WetMovement = 1 << 3,
  GroundedCounter = 1 << 4,
  JumpImpulse = 1 << 5,
  AirborneAcceleration = 1 << 6,
  TargetReacquire = 1 << 7,
  NetworkSync = 1 << 8,
  FrozenSentinel = 1 << 9,
  DirtSlimeGroundCounter = 1 << 10,
  StoneSlimeGravity = 1 << 11,
  CloudSlimeGravity = 1 << 12,
  WoodSlimeDefense = 1 << 13,
  Ai2Cooldown = 1 << 14,
  StoredAi1GroundAcceleration = 1 << 15,
}
