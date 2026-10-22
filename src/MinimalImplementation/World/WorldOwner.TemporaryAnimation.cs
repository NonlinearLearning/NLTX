namespace MinimalImplementation.Server.World;

public sealed partial class WorldOwner {
  /// <summary>
  /// Captures packet 77's original values without changing world state. The host queues the
  /// broadcast from the single server commit loop and includes the sender among recipients.
  /// </summary>
  public bool TryCreateTemporaryAnimationIntent(
      ulong worldEpoch, short animationType, ushort tileType, TileCoordinate coordinate,
      out WorldTemporaryAnimationIntent intent) {
    lock (_gate) {
      intent = default;
      if (!IsCurrentWorld(worldEpoch) || !_header!.Geometry.Contains(coordinate)) {
        return false;
      }

      // Same-version Animation.NewTemporaryAnimation (106-124) checks only map coordinates.
      // Animation.UpdateAll runs inside Main's !dedServ branch, not the server world tick.
      intent = new WorldTemporaryAnimationIntent(
          _worldEpoch, CurrentVersion, animationType, tileType, coordinate);
      return true;
    }
  }
}
