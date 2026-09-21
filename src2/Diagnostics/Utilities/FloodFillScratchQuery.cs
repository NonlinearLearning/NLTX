namespace Terraria.NonAuthoritative.Diagnostics;

public sealed class FloodFillScratchQuery
{
  private readonly List<GridPoint> _queueOne = new();
  private readonly List<GridPoint> _queueTwo = new();
  private readonly BitSet2DScratch _visited = new();

  public void Begin(GridPoint center, int maxDistance)
  {
    _queueOne.Clear();
    _queueTwo.Clear();
    _visited.Reset(center, maxDistance);
  }

  public bool TryVisit(GridPoint point)
  {
    if (!_visited.InBounds(point) || !_visited.Add(point))
    {
      return false;
    }

    _queueOne.Add(point);
    return true;
  }

  public IReadOnlyList<GridPoint> CurrentQueue => _queueOne;
}
