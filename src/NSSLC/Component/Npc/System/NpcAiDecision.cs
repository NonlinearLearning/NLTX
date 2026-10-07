using System.Numerics;

namespace Terraria.Npc;

public readonly record struct NpcAiDecision(
  int Action,
  int TargetSlot,
  Vector2 Velocity,
  NpcAiStateComponent State,
  bool HasVelocityOverride,
  bool SkipMovement,
  bool NoGravity = false,
  bool NoTileCollide = false) {
  public bool HasTarget => TargetSlot >= 0;
}
