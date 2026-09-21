namespace Terraria.NonAuthoritative.Diagnostics;

public readonly record struct DebugRuntimeOptionsSnapshot(
  bool EnableDebugCommands,
  bool ReportCommandUsage,
  int ServerPing,
  double UpdateWaitInMs,
  bool NoLimits,
  bool ShowNetOffsetDust,
  Vector2Value FakeNetOffset,
  bool NoDamage,
  bool ProjectilesAimAtDummies,
  bool PracticeMode);
