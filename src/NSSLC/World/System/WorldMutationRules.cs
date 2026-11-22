namespace Terraria.World;

/// <summary>
/// Pure coalescing rules for pending tile mutations.
/// </summary>
/// <remarks>
/// <see cref="WorldRuntime" /> owns the pending dictionary; whether a change is worth recording,
/// cancels an earlier entry or merges into it is decided here.
/// </remarks>
public static class WorldMutationRules
{
  /// <summary>True when the change differs from the previous facts.</summary>
  public static bool IsRecordable(WorldTileState previous, WorldTileState current)
  {
    return previous != current;
  }

  /// <summary>True when the new change returns the tile to the pending entry's origin.</summary>
  public static bool Cancels(WorldMutation existing, WorldTileState current)
  {
    return existing.Previous == current;
  }

  /// <summary>Creates the first pending entry for a coordinate.</summary>
  public static WorldMutation Create(
      WorldTileCoordinate coordinate,
      WorldTileState previous,
      WorldTileState current,
      int dropItemType)
  {
    return new WorldMutation(coordinate, previous, current, dropItemType);
  }

  /// <summary>Folds a later change into an existing entry, keeping the first known drop.</summary>
  public static WorldMutation Merge(
      WorldMutation existing,
      WorldTileState current,
      int dropItemType)
  {
    return existing with
    {
      Current = current,
      DropItemType = existing.DropItemType > 0 ? existing.DropItemType : dropItemType
    };
  }
}
