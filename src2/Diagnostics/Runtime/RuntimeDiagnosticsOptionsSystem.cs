namespace Terraria.NonAuthoritative.Diagnostics;

public static class RuntimeDiagnosticsOptionsSystem
{
  public static void Apply(
    RuntimeDiagnosticsOptionsComponent component,
    RuntimeDiagnosticsOptionsCommand command)
  {
    ArgumentNullException.ThrowIfNull(component);
    component.Apply(command);
  }
}
