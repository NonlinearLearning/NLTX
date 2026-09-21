namespace Terraria.WorldGeneration.Dungeon.Placement;

public readonly record struct DungeonPlacementDecision(
  bool Accepted,
  string? RejectionReason = null)
{
  public static DungeonPlacementDecision Accept() => new(true);

  public static DungeonPlacementDecision Reject(string reason) => new(false, reason);
}
