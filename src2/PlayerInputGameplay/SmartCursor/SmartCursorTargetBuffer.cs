namespace NLTX.PlayerInputGameplay.SmartCursor;

public readonly record struct SmartCursorTarget(int X, int Y);

public sealed class SmartCursorTargetBuffer
{
  private readonly List<SmartCursorTarget> _targets = new();

  public IReadOnlyList<SmartCursorTarget> Targets => _targets;

  public void Add(SmartCursorTarget target)
  {
    _targets.Add(target);
  }

  public void Clear()
  {
    _targets.Clear();
  }
}
