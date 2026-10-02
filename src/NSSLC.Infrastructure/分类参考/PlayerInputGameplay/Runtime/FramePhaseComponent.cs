namespace NLTX.PlayerInputGameplay.Runtime;

public sealed class FramePhaseComponent
{
  public bool IsDrawingOrUpdating { get; private set; }

  public void BeginUpdate()
  {
    IsDrawingOrUpdating = true;
  }

  public void EndUpdate()
  {
    IsDrawingOrUpdating = false;
  }
}
