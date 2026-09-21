namespace Terraria.NonAuthoritative.Diagnostics;

public sealed class TimeLogEntryState
{
  private readonly Func<int, string> _format;
  private readonly TimeSeriesWindowState[] _data;
  private bool _pendingDisplay;
  private int _activeDataSeries;

  public TimeLogEntryState(
    string name,
    Func<int, string> format,
    int budget,
    int seriesCapacity = 300)
  {
    if (string.IsNullOrWhiteSpace(name))
    {
      throw new ArgumentException("An entry name is required.", nameof(name));
    }

    if (budget < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(budget));
    }

    if (seriesCapacity <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(seriesCapacity));
    }

    _format = format ?? throw new ArgumentNullException(nameof(format));
    _data =
    [
      new TimeSeriesWindowState(seriesCapacity),
      new TimeSeriesWindowState(seriesCapacity)
    ];
    Name = name;
    Budget = budget;
  }

  public string Name { get; }

  public int Budget { get; }

  public bool PendingDisplay => _pendingDisplay;

  public string Format(int value)
  {
    return _format.Invoke(value);
  }

  public void Add(int value)
  {
    _data[_activeDataSeries].Add(value);
  }

  public void StartNextFrame()
  {
    _data[_activeDataSeries].StartNextFrame();
  }

  public void SetPendingDisplay(bool pendingDisplay)
  {
    _pendingDisplay = pendingDisplay;
  }

  public void Reset()
  {
    foreach (TimeSeriesWindowState series in _data)
    {
      series.Reset();
    }

    _pendingDisplay = false;
    _activeDataSeries = 0;
  }

  public TimeLogEntrySnapshot CreateSnapshot()
  {
    return new TimeLogEntrySnapshot(
      Name,
      Budget,
      _pendingDisplay,
      TimeSeriesAggregationQuery.Snapshot(_data[_activeDataSeries]));
  }
}
