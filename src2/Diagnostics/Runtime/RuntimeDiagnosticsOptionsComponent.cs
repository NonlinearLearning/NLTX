namespace Terraria.NonAuthoritative.Diagnostics;

public sealed class RuntimeDiagnosticsOptionsComponent
{
  private bool _showSplash;
  private bool _ignoreErrors;
  private string _defaultIp;

  public RuntimeDiagnosticsOptionsComponent(
    RuntimeDiagnosticsOptionsSnapshot initial)
  {
    _showSplash = initial.ShowSplash;
    _ignoreErrors = initial.IgnoreErrors;
    _defaultIp = initial.DefaultIp ?? string.Empty;
  }

  internal RuntimeDiagnosticsOptionsSnapshot CreateSnapshot()
  {
    return new RuntimeDiagnosticsOptionsSnapshot(
      _showSplash,
      _ignoreErrors,
      _defaultIp);
  }

  internal void Apply(RuntimeDiagnosticsOptionsCommand command)
  {
    _showSplash = command.ShowSplash;
    _ignoreErrors = command.IgnoreErrors;
    _defaultIp = command.DefaultIp ?? string.Empty;
  }
}
