namespace NLTX.PlayerInputGameplay.Runtime;

public sealed class MainInputFrameComponent
{
  public int MouseX { get; private set; }

  public int MouseY { get; private set; }

  public bool MouseRight { get; private set; }

  public bool MouseRightRelease { get; private set; }

  public void Capture(int mouseX, int mouseY, bool mouseRight, bool mouseRightRelease)
  {
    MouseX = mouseX;
    MouseY = mouseY;
    MouseRight = mouseRight;
    MouseRightRelease = mouseRightRelease;
  }

  public void ClearTransientEdges()
  {
    MouseRightRelease = false;
  }
}
