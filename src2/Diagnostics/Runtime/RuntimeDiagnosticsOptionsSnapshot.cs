namespace Terraria.NonAuthoritative.Diagnostics;

public readonly record struct RuntimeDiagnosticsOptionsSnapshot(
  bool ShowSplash,
  bool IgnoreErrors,
  string DefaultIp);
