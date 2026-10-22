using System.Numerics;

namespace MinimalImplementation.Combat.Npcs;

/// <summary>
/// All queries use the same captured world. Missing capture coverage is an integration error;
/// GetTile must not substitute an empty tile. Move must also return the real OldPosition from
/// Collision_MoveWhileDry/Wet, after any walk-down-slope correction and before displacement.
/// </summary>
public interface INpcFighterMotionQuery : INpcGroundMotionQuery {
  NpcFighterTile GetTile(int x, int y);

  bool CanHitTarget(NpcView npc, Vector2 position, NpcTargetPlayer target);
}
