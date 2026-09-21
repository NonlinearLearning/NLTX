namespace Terraria.ExternalPlatformBoundaries.Workshop;

public sealed class WorkshopIssueReportSnapshot
{
  public WorkshopIssueReportSnapshot(DateTimeOffset timeReported, string message)
  {
    ArgumentNullException.ThrowIfNull(message);
    TimeReported = timeReported;
    Message = message;
  }

  public DateTimeOffset TimeReported { get; }

  public string Message { get; }
}
