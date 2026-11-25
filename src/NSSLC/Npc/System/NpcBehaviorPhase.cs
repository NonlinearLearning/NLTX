namespace Terraria.Unshared.Npc;

/// <summary>Observable phase of an NPC behavior execution.</summary>
public enum NpcBehaviorPhase : byte
{
  Idle = 0,
  Wander = 1,
  Investigate = 2,
  Chase = 3,
  Attack = 4,
  Flee = 5,
  ReturnHome = 6,
  Stunned = 7
}
