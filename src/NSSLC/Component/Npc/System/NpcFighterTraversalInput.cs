namespace Terraria.Npc;

/// <summary>
/// Tile and target facts captured at the Fighter AI call boundary.
/// The profile consumes facts only; probing and tile mutation stay in the host effect boundary.
/// </summary>
public readonly record struct NpcFighterTraversalInput(
  bool SolidTileOneAhead,
  bool SolidTileTwoAhead,
  bool SolidTileThreeAhead,
  bool DoorAhead,
  bool DoorCanOpen,
  int DoorTileX,
  int DoorTileY,
  bool TargetAbove,
  bool TargetLineOfSight,
  bool ExpertMode);
