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
    if (current.Revision < previous.Revision)
    {
      throw new InvalidOperationException(
        "A pylon projection cannot apply an older registry revision.");
    }

    HashSet<TeleportPylonSnapshotEntry> previousEntries =
      previous.Entries.ToHashSet();
    HashSet<TeleportPylonSnapshotEntry> currentEntries =
      current.Entries.ToHashSet();

    foreach (TeleportPylonSnapshotEntry entry in previous.Entries)
    {
      if (!currentEntries.Contains(entry))
      {
        sink.Remove(entry);
      }
    }

    foreach (TeleportPylonSnapshotEntry entry in current.Entries)
    {
      if (!previousEntries.Contains(entry))
      {
        sink.Add(entry);
      }
    }
  }

  public interface IFullStateSink
  {
    void Publish(
      uint revision,
      IReadOnlyList<TeleportPylonSnapshotEntry> entries);
  }

  public static void ProjectFullState(
    PylonRegistrySnapshot current,
    IFullStateSink sink)
  {
    ArgumentNullException.ThrowIfNull(current);
    ArgumentNullException.ThrowIfNull(sink);
    sink.Publish(current.Revision, current.Entries);
  }
}
