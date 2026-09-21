namespace Terraria.NonAuthoritative.Diagnostics;

public readonly record struct TimeSeriesAggregationSnapshot(
  int Previous,
  int Median,
  int P90,
  int Maximum,
  int Count);
