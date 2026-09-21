namespace Terraria.NonAuthoritative.Diagnostics;

public static class IssueReportProjection
{
  public static IReadOnlyList<IssueReportRecord> Snapshot(
    IssueReportCatalogAdapter catalog)
  {
    ArgumentNullException.ThrowIfNull(catalog);
    return catalog.Snapshot();
  }
}
