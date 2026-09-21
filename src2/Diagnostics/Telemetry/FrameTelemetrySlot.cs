namespace Terraria.NonAuthoritative.Diagnostics;

public sealed class FrameTelemetrySlot
{
  private readonly List<FrameEventRecord> _events = new();

  public IReadOnlyList<FrameEventRecord> Events => _events.ToArray();

  public long AllocatedBytes { get; private set; }

  public GcAllocationSnapshot GcSnapshot { get; private set; }

  internal void Begin()
  {
    _events.Clear();
    AllocatedBytes = 0;
    GcSnapshot = default;
  }

  internal void Record(OperationCategory category, long timestamp)
  {
    if (_events.Count < 1000 &&
      (_events.Count == 0 || _events[^1].Category != category))
    {
      _events.Add(new FrameEventRecord(category, timestamp));
    }
  }

  internal void End(
    long allocatedBytes,
    GcAllocationSnapshot gcSnapshot)
  {
    AllocatedBytes = allocatedBytes;
    GcSnapshot = gcSnapshot;
  }

  internal FrameTelemetrySlotSnapshot CreateSnapshot()
  {
    return new FrameTelemetrySlotSnapshot(
      _events.ToArray(),
      AllocatedBytes,
      GcSnapshot);
  }
}
