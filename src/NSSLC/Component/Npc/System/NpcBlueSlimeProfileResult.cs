using System.Numerics;

namespace Terraria.Npc;

public readonly record struct NpcBlueSlimeProfileResult(
  Vector2 Velocity,
  int Defense,
  NpcBlueSlimeProfileState State,
  int Direction,
  int AiAction,
  bool NetUpdateRequested,
  bool TargetClosestRequested,
  bool ContainedItemGenerationRequested,
  NpcBlueSlimeSourceBranch Branches)
{
  public bool IsFrozen => (Branches & NpcBlueSlimeSourceBranch.FrozenSentinel) != 0;
}
