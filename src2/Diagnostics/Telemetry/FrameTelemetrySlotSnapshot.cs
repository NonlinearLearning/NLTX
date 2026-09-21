namespace Terraria.NonAuthoritative.Diagnostics;

public readonly record struct FrameTelemetrySlotSnapshot(
  IReadOnlyList<FrameEventRecord> Events,
  long AllocatedBytes,
  GcAllocationSnapshot GcSnapshot);
