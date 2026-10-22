using System.Numerics;

namespace MinimalImplementation.Combat.Npcs;

/// <summary>Server-resolved tank-pet bounds and line of sight for this NPC tick.</summary>
public sealed record NpcTankTarget(Vector2 Position, int Width, int Height, bool CanHit);
