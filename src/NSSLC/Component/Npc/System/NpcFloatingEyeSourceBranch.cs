using System;

namespace Terraria.Npc;

[Flags]
public enum NpcFloatingEyeSourceBranch
{
  None = 0,
  CollisionBounce = 1 << 0,
  Discouraged = 1 << 1,
  TargetSelection = 1 << 2,
  GenericAcceleration = 1 << 3,
  Dust = 1 << 4,
  WetMovement = 1 << 5,
  WetTargetSelection = 1 << 6,
}
