namespace NLTX.PlayerInputGameplay.Intent;

public sealed class PlayerInteractionAnchorComponent
{
  public int InteractEntityId { get; private set; } = -1;

  public int X { get; private set; }

  public int Y { get; private set; }

  public bool InUse => InteractEntityId != -1;

  public void Set(int interactEntityId, int x, int y)
  {
    InteractEntityId = interactEntityId;
    X = x;
    Y = y;
  }

  public void Clear()
  {
    InteractEntityId = -1;
    X = 0;
    Y = 0;
  }
}
