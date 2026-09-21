namespace NLTX.PlayerInputGameplay.Focus;

public sealed class WindowFocusAdapter
{
  public bool IsSelectedApplication { get; private set; }

  public void SetSelected(bool selected)
  {
    IsSelectedApplication = selected;
  }
}
