namespace NLTX.PlayerInputGameplay.Presentation;

public interface INetDiagnosticsView
{
  string Description { get; }
}

public sealed class EmptyNetDiagnosticsView : INetDiagnosticsView
{
  public string Description => string.Empty;
}

public sealed class NetDiagnosticsPresentationAdapter
{
  private INetDiagnosticsView? _activeView;

  public INetDiagnosticsView ActiveView => _activeView ??= new EmptyNetDiagnosticsView();

  public void SetActiveView(INetDiagnosticsView? view)
  {
    _activeView = view;
  }
}
