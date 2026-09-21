namespace Terraria.NonAuthoritative.Diagnostics;

public readonly record struct GcAllocationSnapshot(
  TimeSpan PauseTime,
  int CollectionCount,
  long AllocatedBytes);
