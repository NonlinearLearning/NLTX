namespace Terraria.SpatialMotionPhysics;

public static class PylonRegistryProjection
{
  public interface IPylonProjectionSink
  {
    void Add(TeleportPylonSnapshotEntry entry);

    void Remove(TeleportPylonSnapshotEntry entry);
  }

  public static void Project(
    PylonRegistrySnapshot previous,
    PylonRegistrySnapshot current,
    IPylonProjectionSink sink)
  {
    ArgumentNullException.ThrowIfNull(previous);
    ArgumentNullException.ThrowIfNull(current);
    ArgumentNullException.ThrowIfNull(sink);

    HashSet<TeleportPylonSnapshotEntry> previousEntries =
      previous.Entries.ToHashSet();
    HashSet<TeleportPylonSnapshotEntry> currentEntries =
      current.Entries.ToHashSet();

    foreach (TeleportPylonSnapshotEntry entry in previousEntries)
    {
      if (!currentEntries.Contains(entry))
      {
        sink.Remove(entry);
      }
    }

    foreach (TeleportPylonSnapshotEntry entry in currentEntries)
    {
      if (!previousEntries.Contains(entry))
      {
        sink.Add(entry);
      }
    }
  }
}
