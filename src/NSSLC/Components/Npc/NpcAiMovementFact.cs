using System.Numerics;

namespace Terraria.Npc;

/// <summary>
/// Detached movement facts projected from the authoritative spatial components for one AI tick.
/// </summary>
public readonly record struct NpcAiMovementFact(
    Vector2 Position,
    Vector2 Velocity,
    bool IsGrounded);
