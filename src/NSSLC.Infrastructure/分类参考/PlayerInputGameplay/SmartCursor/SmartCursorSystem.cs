namespace NLTX.PlayerInputGameplay.SmartCursor;

public sealed class SmartCursorSystem
{
  public void Scan(
    SmartCursorUsageInfo usage,
    SmartCursorTargetBuffer targets,
    Func<int, int, bool> isReachable)
  {
    ArgumentNullException.ThrowIfNull(targets);
    ArgumentNullException.ThrowIfNull(isReachable);
    targets.Clear();
    for (var x = usage.ReachableStartX; x <= usage.ReachableEndX; x++)
    {
      for (var y = usage.ReachableStartY; y <= usage.ReachableEndY; y++)
      {
        if (isReachable(x, y))
        {
          targets.Add(new SmartCursorTarget(x, y));
        }
      }
    }
  }
}
