namespace Terraria.NonAuthoritative.Diagnostics;

public abstract class DiagnosticMetricProjection
{
  private readonly Dictionary<string, int> _values =
    new(StringComparer.Ordinal);

  public void Record(string metricName, int value)
  {
    if (string.IsNullOrWhiteSpace(metricName))
    {
      throw new ArgumentException(
        "A metric name is required.",
        nameof(metricName));
    }

    _values[metricName] = value;
  }

  public IReadOnlyDictionary<string, int> Snapshot()
  {
    return new Dictionary<string, int>(_values);
  }
}
