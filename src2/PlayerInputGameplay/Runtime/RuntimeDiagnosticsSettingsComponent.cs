namespace NLTX.PlayerInputGameplay.Runtime;

public sealed class RuntimeDiagnosticsSettingsComponent
{
  public bool VerboseNetplay { get; private set; }

  public bool StopTimeOuts { get; private set; }

  public void Set(bool verboseNetplay, bool stopTimeOuts)
  {
    VerboseNetplay = verboseNetplay;
    StopTimeOuts = stopTimeOuts;
  }
}
