namespace Terraria.NonAuthoritative.Diagnostics;

public static class RuntimeDiagnosticsOptionsQuery
{
  public static RuntimeDiagnosticsOptionsSnapshot Snapshot(
    RuntimeDiagnosticsOptionsComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    return component.CreateSnapshot();
  }
}
