namespace Terraria.WorldSession.Queries;

public sealed class SceneMetricsAdapter
{
  private readonly Dictionary<string, int> _metrics = new(StringComparer.Ordinal);

  public void Set(string name, int value)
  {
    if (string.IsNullOrWhiteSpace(name))
    {
      throw new ArgumentException("A metric name is required.", nameof(name));
    }

    _metrics[name] = value;
  }

  public IReadOnlyDictionary<string, int> Snapshot()
  {
    return new Dictionary<string, int>(_metrics, StringComparer.Ordinal);
  }
}
