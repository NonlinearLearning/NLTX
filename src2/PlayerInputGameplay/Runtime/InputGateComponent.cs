namespace NLTX.PlayerInputGameplay.Runtime;

public sealed class InputGateComponent
{
  public bool IsMouseBlocked { get; private set; }

  public void SetBlocked(bool blocked)
  {
    IsMouseBlocked = blocked;
  }
}
