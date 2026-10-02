namespace Terraria.WorldGeneration.Dungeon.Bounds;

public readonly record struct DungeonBoundsMutationResult(
  bool Applied,
  string? RejectionReason = null)
{
  public static DungeonBoundsMutationResult Accepted() => new(true);

  public static DungeonBoundsMutationResult Rejected(string reason)
  {
    return new(false, reason);
  }
}
