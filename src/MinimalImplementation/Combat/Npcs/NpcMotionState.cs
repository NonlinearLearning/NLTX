using System.Numerics;

namespace MinimalImplementation.Combat.Npcs;

/// <summary>Prior collision facts owned by the NPC and used by its next AI update.</summary>
public sealed record NpcMotionState(
    Vector2 OldVelocity, bool CollideX, bool CollideY, Vector2 OldPosition = default) {
  public static NpcMotionState Initial { get; } = new(Vector2.Zero, false, false);
}
