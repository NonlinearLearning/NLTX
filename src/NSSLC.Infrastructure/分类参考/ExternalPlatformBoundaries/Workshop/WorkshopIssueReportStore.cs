namespace Terraria.ExternalPlatformBoundaries.Workshop;

public sealed class WorkshopIssueReportStore
{
  private readonly Func<DateTimeOffset> _clock;
  private readonly int _capacity;
  private readonly List<WorkshopIssueReportSnapshot> _reports = new();

  public WorkshopIssueReportStore(Func<DateTimeOffset> clock, int capacity = 1000)
  {
    _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    if (capacity <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(capacity));
    }

    _capacity = capacity;
  }

  public WorkshopIssueReportSnapshot Add(string message)
  {
    var report = new WorkshopIssueReportSnapshot(_clock(), message);
    _reports.Add(report);
    if (_reports.Count > _capacity)
    {
      _reports.RemoveAt(0);
    }

    return report;
  }

  public IReadOnlyList<WorkshopIssueReportSnapshot> Snapshot()
  {
    return Array.AsReadOnly(_reports.ToArray());
  }
}
