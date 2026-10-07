namespace Terraria.Npc;

/// <summary>
/// Narrow capability request corresponding to the source call to
/// AI_007_TryForcingSitting. The owner performs the real-time tile and seat
/// occupancy checks and commits the source state writes when it succeeds.
/// </summary>
public readonly record struct NpcGuideForceSittingRequest(
  int HomeFloorX,
  int HomeFloorY);
