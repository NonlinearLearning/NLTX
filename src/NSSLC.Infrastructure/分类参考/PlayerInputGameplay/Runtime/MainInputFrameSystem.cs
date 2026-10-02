namespace NLTX.PlayerInputGameplay.Runtime;

public sealed class MainInputFrameSystem
{
  public void BeginFrame(
    MainInputFrameComponent input,
    MainFrameTimingComponent timing,
    int mouseX,
    int mouseY,
    bool mouseRight,
    bool mouseRightRelease,
    float elapsedSeconds)
  {
    ArgumentNullException.ThrowIfNull(input);
    ArgumentNullException.ThrowIfNull(timing);
    input.Capture(mouseX, mouseY, mouseRight, mouseRightRelease);
    timing.Advance(elapsedSeconds);
  }

  public void EndFrame(MainInputFrameComponent input)
  {
    ArgumentNullException.ThrowIfNull(input);
    input.ClearTransientEdges();
  }
}
