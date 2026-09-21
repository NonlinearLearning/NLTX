namespace Terraria.NonAuthoritative.Diagnostics;

public static class TimeSeriesAggregationQuery
{
  public static TimeSeriesAggregationSnapshot Snapshot(
    TimeSeriesWindowState state)
  {
    ArgumentNullException.ThrowIfNull(state);
    return state.CreateSnapshot();
  }
}
