using System.Numerics;

namespace MinimalImplementation.Combat.Npcs;

/// <summary>
/// Read-only queries over one world snapshot. Move starts at the AI-corrected position
/// and resolves current liquid contact, tile/platform collision, and displacement.
/// The adapter must preserve collision velocity separately from liquid-scaled travel.
/// OldVelocity is required: capture it after walk-down-slope/liquid adjustments and before
/// tile collision, rather than copying desiredVelocity.
/// </summary>
public interface INpcGroundMotionQuery {
  bool HasSolidCollision(NpcView npc, Vector2 position);

  NpcMotionResult Move(
      NpcView npc, Vector2 position, Vector2 desiredVelocity, bool fallThroughPlatforms);
}
