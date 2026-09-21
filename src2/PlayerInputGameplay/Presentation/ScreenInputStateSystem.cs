using NLTX.PlayerInputGameplay.Runtime;

namespace NLTX.PlayerInputGameplay.Presentation;

public sealed class ScreenInputStateSystem
{
  public void CapturePointerTransition(
    MainInputFrameComponent input,
    PointerTransitionStateComponent pointerTransition)
  {
    ArgumentNullException.ThrowIfNull(input);
    ArgumentNullException.ThrowIfNull(pointerTransition);
    pointerTransition.Capture(input.MouseRightRelease);
  }

  public void ClearInputText(InputTextCaptureAdapter capture)
  {
    ArgumentNullException.ThrowIfNull(capture);
    capture.Clear();
  }
}
