namespace Terraria.SpatialMotionPhysics;

public sealed class PylonRegistrySnapshot
{
  public PylonRegistrySnapshot(IEnumerable<TeleportPylonSnapshotEntry> entries)
  {
    ArgumentNullException.ThrowIfNull(entries);
    Entries = entries.ToArray();
  }

  public IReadOnlyList<TeleportPylonSnapshotEntry> Entries { get; }
}
