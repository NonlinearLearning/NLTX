using System.Numerics;

namespace NLTX.PlayerInputGameplay.Intent;

public sealed class IntentionTrackingSystem
{
  public void Track(
    PlayerIntentionStateComponent state,
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
    ArgumentNullException.ThrowIfNull(state);
    state.Update(x, y, position, center, mouse, direction, width, intention, activeActionTimeLeft);
  }
}
