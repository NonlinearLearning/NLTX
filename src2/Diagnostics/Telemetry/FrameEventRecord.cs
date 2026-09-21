namespace Terraria.NonAuthoritative.Diagnostics;

public readonly record struct FrameEventRecord(
  OperationCategory Category,
  long Timestamp);
