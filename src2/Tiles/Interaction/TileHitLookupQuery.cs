namespace Terraria.Tiles.Interaction;

public static class TileHitLookupQuery
{
  public static bool TryFind(
    TileHitTrackingComponent component,
    int x,
    int y,
    TileHitKind hitKind,
    out int slot,
    out TileHitEntry entry)
  {
    ArgumentNullException.ThrowIfNull(component);

    for (int orderIndex = 0; orderIndex < TileHitTrackingPolicy.Capacity; orderIndex++)
    {
      int candidateSlot = component.Order[orderIndex];
      TileHitEntry candidate = component.Entries[candidateSlot];
      if (candidate.IsActive && candidate.X == x && candidate.Y == y &&
        candidate.Kind == hitKind)
      {
        slot = candidateSlot;
        entry = candidate;
        return true;
      }
    }

    slot = -1;
    entry = TileHitEntry.Empty;
    return false;
  }
}
