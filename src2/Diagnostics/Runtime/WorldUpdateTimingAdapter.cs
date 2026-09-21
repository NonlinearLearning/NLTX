using System.Diagnostics;

namespace Terraria.NonAuthoritative.Diagnostics;

public sealed class WorldUpdateTimingAdapter
{
  private readonly Stopwatch _stopwatch = new();

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
