namespace NLTX.PlayerInputGameplay.Presentation;

public sealed class PointerTransitionStateComponent
{
  public bool MouseRightRelease { get; private set; }

  public void Capture(bool mouseRightRelease)
  {
    MouseRightRelease = mouseRightRelease;
  }

  public bool ConsumeRelease()
  {
    var released = MouseRightRelease;
    MouseRightRelease = false;
    return released;
  }
}
