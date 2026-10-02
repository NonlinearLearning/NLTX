namespace Terraria.SpatialMotionPhysics;

public sealed class PylonRegistryAdapter
{
  public PylonRegistrySnapshot CreateSnapshot(
    IEnumerable<TeleportPylonSnapshotEntry> entries,
    uint revision = 0)
  {
    return new PylonRegistrySnapshot(entries, revision);
  }
}
