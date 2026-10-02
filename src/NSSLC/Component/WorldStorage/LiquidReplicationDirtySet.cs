namespace Terraria.WorldStorage;

public sealed class LiquidReplicationDirtySet
{
  public long LastPublishedRevision;
  public HashSet<TileCoordinate> PendingCoordinates = new();
  public HashSet<TileCoordinate> PublishingCoordinates = new();
  public int PendingChangeCount => PendingCoordinates.Count;
  public bool IsPublishing => PublishingCoordinates.Count != 0;
}
