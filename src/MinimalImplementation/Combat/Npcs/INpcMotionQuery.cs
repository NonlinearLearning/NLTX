using System.Numerics;

namespace MinimalImplementation.Combat.Npcs;

/// <summary>
/// Read-only P03 adapter over one world snapshot. It must not mutate NPCs or other owners.
/// Move resolves tile/platform/liquid collision and displacement for the supplied velocity.
/// OldVelocity must be captured after walk-down-slope/liquid adjustments, before tile collision.
/// </summary>
public interface INpcMotionQuery {
  bool IsWet(NpcView npc);

  NpcMotionResult Move(NpcView npc, Vector2 desiredVelocity);
}
