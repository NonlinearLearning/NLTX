namespace Terraria.NonAuthoritative.Diagnostics;

public sealed class TimeLoggerFrameState
{
  private readonly int _frameCount;
  private readonly List<TimeLogEntryState> _entries = new();
  private int _currentFrame;
  private bool _currentlyLogging;

  public TimeLoggerFrameState(int frameCount)
  {
    if (frameCount <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(frameCount));
    }

    _frameCount = frameCount;
  }

  public int FrameCount => _frameCount;

  public int CurrentFrame => _currentFrame;

  public bool CurrentlyLogging => _currentlyLogging;

  public IReadOnlyList<TimeLogEntryState> Entries => _entries.ToArray();

  internal void Register(TimeLogEntryState entry)
  {
    if (_entries.Contains(entry))
    {
      throw new InvalidOperationException("The time-log entry is already registered.");
    }

    _entries.Add(entry);
  }

  internal void Start()
  {
    if (_currentlyLogging)
    {
      throw new InvalidOperationException("A frame is already active.");
    }

    _currentlyLogging = true;
    _currentFrame++;
  }

  internal void End()
  {
    if (!_currentlyLogging)
    {
      throw new InvalidOperationException("No frame is active.");
    }

    foreach (TimeLogEntryState entry in _entries)
    {
      entry.StartNextFrame();
    }

    _currentlyLogging = false;
  }
}
