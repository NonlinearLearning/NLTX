namespace NLTX.PlayerInputGameplay.Runtime;

public sealed class RenderRequestComponent
{
  public bool RenderNow { get; private set; }

  public void Request()
  {
    RenderNow = true;
  }

  public bool Consume()
  {
    var wasRequested = RenderNow;
    RenderNow = false;
    return wasRequested;
  }
}
