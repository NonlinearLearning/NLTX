namespace Terraria.NonAuthoritative.Diagnostics;

public sealed class FrameTelemetryRing
{
  private readonly FrameTelemetrySlot[] _slots;
  private readonly List<FrameTelemetrySlotSnapshot> _completedFrames = new();
  private int _activeIndex;
  private bool _active;

  public FrameTelemetryRing(int capacity)
  {
    if (capacity <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(capacity));
    }

    _slots = Enumerable.Range(0, capacity)
      .Select(static _ => new FrameTelemetrySlot())
      .ToArray();
  }

  public void BeginFrame(long timestamp)
  {
    if (_active)
    {
      throw new InvalidOperationException("A frame is already active.");
    }

    FrameTelemetrySlot slot = _slots[_activeIndex];
    slot.Begin();
    slot.Record(OperationCategory.Idle, timestamp);
    _active = true;
  }

  public void Record(OperationCategory category, long timestamp)
  {
    if (!_active)
    {
      throw new InvalidOperationException("No frame is active.");
    }

    _slots[_activeIndex].Record(category, timestamp);
  }

  public void EndFrame(
    long allocatedBytes,
    GcAllocationSnapshot gcSnapshot)
  {
    if (!_active)
    {
      throw new InvalidOperationException("No frame is active.");
    }

    FrameTelemetrySlot slot = _slots[_activeIndex];
    slot.Record(OperationCategory.End, gcSnapshot.PauseTime.Ticks);
    slot.End(allocatedBytes, gcSnapshot);
    _completedFrames.Add(slot.CreateSnapshot());
    if (_completedFrames.Count > _slots.Length)
    {
      _completedFrames.RemoveAt(0);
    }

    _activeIndex = (_activeIndex + 1) % _slots.Length;
    _active = false;
  }

  public FrameTelemetrySnapshot CreateSnapshot()
  {
    return new FrameTelemetrySnapshot(_completedFrames.ToArray());
  }
}
