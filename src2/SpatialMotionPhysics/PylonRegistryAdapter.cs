namespace Terraria.SpatialMotionPhysics;

public sealed class PylonRegistryAdapter
{
  public PylonRegistrySnapshot CreateSnapshot(
    IEnumerable<TeleportPylonSnapshotEntry> entries)
  {
    return new PylonRegistrySnapshot(entries);
  }
}
