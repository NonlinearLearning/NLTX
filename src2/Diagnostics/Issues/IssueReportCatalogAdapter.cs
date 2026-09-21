namespace Terraria.NonAuthoritative.Diagnostics;

public sealed class IssueReportCatalogAdapter
{
  private readonly List<IssueReportRecord> _reports = new();

  public void Add(string reportText, DateTimeOffset timeReported)
  {
    if (string.IsNullOrWhiteSpace(reportText))
    {
      throw new ArgumentException(
        "Report text is required.",
        nameof(reportText));
    }

    _reports.Add(new IssueReportRecord(timeReported, reportText));
  }

  public IReadOnlyList<IssueReportRecord> Snapshot()
  {
    return _reports.ToArray();
  }
}
