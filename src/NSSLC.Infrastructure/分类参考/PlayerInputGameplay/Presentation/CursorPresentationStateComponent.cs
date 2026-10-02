namespace NLTX.PlayerInputGameplay.Presentation;

public sealed class CursorPresentationStateComponent
{
  public byte MouseTextColor { get; private set; }

  public void SetMouseTextColor(byte color)
  {
    MouseTextColor = color;
  }
}
