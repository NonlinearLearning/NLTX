namespace NLTX.PlayerInputGameplay.Interaction;

public sealed class SmartInteractionTileTargets
{
  private readonly List<(int X, int Y)> _targets = new();

  public IReadOnlyList<(int X, int Y)> Targets => _targets;

  public void Add(int x, int y)
  {
    _targets.Add((x, y));
  }

  public void Clear()
  {
    _targets.Clear();
  }
}
