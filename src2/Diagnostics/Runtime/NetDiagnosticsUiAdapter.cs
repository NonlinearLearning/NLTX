namespace Terraria.NonAuthoritative.Diagnostics;

public sealed class NetDiagnosticsUiAdapter
{
  private readonly Action<RuntimeDiagnosticsOptionsSnapshot> _publish;

  public NetDiagnosticsUiAdapter(
    Action<RuntimeDiagnosticsOptionsSnapshot> publish)
  {
    _publish = publish ?? throw new ArgumentNullException(nameof(publish));
  }

  public void Publish(RuntimeDiagnosticsOptionsSnapshot snapshot)
  {
    _publish.Invoke(snapshot);
  }
}
