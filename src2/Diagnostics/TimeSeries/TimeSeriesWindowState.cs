namespace Terraria.NonAuthoritative.Diagnostics;

public sealed class TimeSeriesWindowState
{
  private readonly int[] _values;
  private readonly bool[] _used;
  private int _next;
  private int _count;
  private int _usedCount;
  private int _previous;
  private int _median;
  private int _p90;
  private int _maximum;

  public TimeSeriesWindowState(int capacity)
  {
    if (capacity <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(capacity));
    }

    _values = new int[capacity];
    _used = new bool[capacity];
  }

  public int Capacity => _values.Length;

  public void Add(int value)
  {
    _values[_next] += value;
    _used[_next] = true;
  }

  public void StartNextFrame()
  {
    _previous = _used[_next] ? _values[_next] : 0;

    if (_count < _values.Length)
    {
      _count++;
    }

    _next = (_next + 1) % _values.Length;
    _usedCount = 0;
    int[] sorted = new int[_values.Length];
    for (int index = 0; index < _values.Length; index++)
    {
      if (_used[index])
      {
        sorted[_usedCount++] = _values[index];
      }
    }

    if (_usedCount > 0)
    {
      Array.Sort(sorted, 0, _usedCount);
      _median = Quantile(sorted, _usedCount, 0.5);
      _p90 = Quantile(sorted, _usedCount, 0.9);
      _maximum = Quantile(sorted, _usedCount, 1.0);
    }
    else
    {
      _previous = 0;
      _median = 0;
      _p90 = 0;
      _maximum = 0;
    }

    _values[_next] = 0;
    _used[_next] = false;
  }

  public void Reset()
  {
    Array.Clear(_values);
    Array.Clear(_used);
    _next = 0;
    _count = 0;
    _usedCount = 0;
    _previous = 0;
    _median = 0;
    _p90 = 0;
    _maximum = 0;
  }

  internal TimeSeriesAggregationSnapshot CreateSnapshot()
  {
    return new TimeSeriesAggregationSnapshot(
      _previous,
      _median,
      _p90,
      _maximum,
      _usedCount);
  }

  private static int Quantile(int[] sorted, int length, double quantile)
  {
    double position = quantile * (length - 1);
    int lowerIndex = (int)Math.Floor(position);
    int upperIndex = (int)Math.Ceiling(position);
    double fraction = position - lowerIndex;
    return (int)(sorted[lowerIndex] +
      (sorted[upperIndex] - sorted[lowerIndex]) * fraction);
  }
}
