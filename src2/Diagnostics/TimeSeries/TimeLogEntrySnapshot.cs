namespace Terraria.NonAuthoritative.Diagnostics;

public readonly record struct TimeLogEntrySnapshot(
  string Name,
  int Budget,
  bool PendingDisplay,
  TimeSeriesAggregationSnapshot Series);
