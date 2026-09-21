namespace NLTX.ClientPresentation.MapCameraRendering;

public sealed class MapInvalidationQueueComponent
{
  private readonly int _capacity;
  private readonly List<MapInvalidationCommand> _entries = new();

  public MapInvalidationQueueComponent(int capacity)
  {
    if (capacity <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(capacity));
    }

    _capacity = capacity;
  }

  public int Count => _entries.Count;

  public uint Revision { get; private set; }

  internal bool Enqueue(MapInvalidationCommand command)
  {
    SceneScanRectangle incoming = command.Area.Normalize();
    if (incoming.Width <= 0 || incoming.Height <= 0)
    {
      throw new ArgumentException("A map invalidation area must be non-empty.", nameof(command));
    }

    for (int index = 0; index < _entries.Count; index++)
    {
      MapInvalidationCommand existing = _entries[index];
      if (!Touches(existing.Area.Normalize(), incoming))
      {
        continue;
      }

      _entries[index] = new MapInvalidationCommand(
        Merge(existing.Area.Normalize(), incoming),
        Math.Max(existing.SourceRevision, command.SourceRevision));
      Revision++;
      return true;
    }

    if (_entries.Count >= _capacity)
    {
      return false;
    }

    _entries.Add(new MapInvalidationCommand(incoming, command.SourceRevision));
    Revision++;
    return true;
  }

  internal bool TryDequeue(out MapInvalidationCommand command)
  {
    if (_entries.Count == 0)
    {
      command = default;
      return false;
    }

    command = _entries[0];
    _entries.RemoveAt(0);
    Revision++;
    return true;
  }

  private static bool Touches(SceneScanRectangle first, SceneScanRectangle second)
  {
    long firstRight = (long)first.X + first.Width;
    long firstBottom = (long)first.Y + first.Height;
    long secondRight = (long)second.X + second.Width;
    long secondBottom = (long)second.Y + second.Height;
    return first.X <= secondRight && second.X <= firstRight &&
      first.Y <= secondBottom && second.Y <= firstBottom;
  }

  private static SceneScanRectangle Merge(
    SceneScanRectangle first,
    SceneScanRectangle second)
  {
    int left = Math.Min(first.X, second.X);
    int top = Math.Min(first.Y, second.Y);
    int right = checked(Math.Max(
      (long)first.X + first.Width,
      (long)second.X + second.Width) > int.MaxValue
      ? int.MaxValue
      : (int)Math.Max(
        (long)first.X + first.Width,
        (long)second.X + second.Width));
    int bottom = checked(Math.Max(
      (long)first.Y + first.Height,
      (long)second.Y + second.Height) > int.MaxValue
      ? int.MaxValue
      : (int)Math.Max(
        (long)first.Y + first.Height,
        (long)second.Y + second.Height));
    return new SceneScanRectangle(left, top, right - left, bottom - top);
  }
}
