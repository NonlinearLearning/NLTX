namespace Terraria.NonAuthoritative.Diagnostics;

public readonly record struct IssueReportRecord(
  DateTimeOffset TimeReported,
  string ReportText);
