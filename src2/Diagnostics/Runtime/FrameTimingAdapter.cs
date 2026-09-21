using System.Diagnostics;

namespace Terraria.NonAuthoritative.Diagnostics;

public sealed class FrameTimingAdapter
{
  private readonly Stopwatch _stopwatch = new();

  public bool IsRunning => _stopwatch.IsRunning;

  public void Start()
  {
    _stopwatch.Restart();
  }

  public TimeSpan Stop()
  {
    if (!_stopwatch.IsRunning)
    {
      return TimeSpan.Zero;
    }

    _stopwatch.Stop();
    return _stopwatch.Elapsed;
  }
}
