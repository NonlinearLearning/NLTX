namespace Terraria.Unshared.Npc;

/// <summary>Domain intent kinds emitted by NPC behavior implementations.</summary>
public enum NpcAiIntentKind : byte
{
  SetBehaviorState = 0,
  Move = 1,
  StopMovement = 2,
  BeginCombatAction = 3,
  CancelCombatAction = 4
}
