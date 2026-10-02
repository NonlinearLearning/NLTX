using System.Numerics;

namespace NLTX.PlayerInputGameplay.Intent;

public enum GuessedPlayerIntention
{
  None,
  Move,
  Aim,
  Interact
}

public sealed class PlayerIntentionStateComponent
{
  public int LastX { get; private set; }

  public int LastY { get; private set; }

  public Vector2 LastPosition { get; private set; }

  public Vector2 LastCenter { get; private set; }

  public Vector2 LastMouse { get; private set; }

  public int LastDirection { get; private set; }

  public int LastWidth { get; private set; }

  public GuessedPlayerIntention Intention { get; private set; }

  public int TimeWithIntention { get; private set; }

  public int PlayerActiveActionTimeLeft { get; private set; }

  public void Update(
    int x,
    int y,
    Vector2 position,
    Vector2 center,
    Vector2 mouse,
    int direction,
    int width,
    GuessedPlayerIntention intention,
    int activeActionTimeLeft)
  {
    if (width < 0 || activeActionTimeLeft < 0)
    {
      throw new ArgumentOutOfRangeException(width < 0 ? nameof(width) : nameof(activeActionTimeLeft));
    }

    TimeWithIntention = Intention == intention ? checked(TimeWithIntention + 1) : 0;
    LastX = x;
    LastY = y;
    LastPosition = position;
    LastCenter = center;
    LastMouse = mouse;
    LastDirection = direction;
    LastWidth = width;
    Intention = intention;
    PlayerActiveActionTimeLeft = activeActionTimeLeft;
  }
}
