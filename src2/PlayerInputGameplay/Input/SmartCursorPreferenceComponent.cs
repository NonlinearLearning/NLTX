namespace NLTX.PlayerInputGameplay.Input;

public sealed class SmartCursorPreferenceComponent
{
  public bool WantedByMouse { get; private set; }

  public bool WantedByGamePad { get; private set; }

  public void Set(bool wantedByMouse, bool wantedByGamePad)
  {
    WantedByMouse = wantedByMouse;
    WantedByGamePad = wantedByGamePad;
  }
}
