namespace Terraria.NonAuthoritative.Diagnostics;

public readonly record struct RuntimeDiagnosticsOptionsCommand(
  bool ShowSplash,
  bool IgnoreErrors,
  string DefaultIp);
