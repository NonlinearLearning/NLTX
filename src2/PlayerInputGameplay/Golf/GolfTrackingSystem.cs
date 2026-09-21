using System.Numerics;

namespace NLTX.PlayerInputGameplay.Golf;

public sealed class GolfTrackingSystem
{
  public void Observe(
    GolfLocalTrackingComponent tracking,
    double timestamp,
    int projectileId,
    Vector2 location,
    int swingCount,
    bool active,
    bool golfBall,
    bool locallyOwned)
  {
    ArgumentNullException.ThrowIfNull(tracking);
    tracking.ObserveBall(timestamp, projectileId, location, swingCount, active, golfBall, locallyOwned);
  }

  public void Advance(GolfLocalTrackingComponent tracking, int ticks)
  {
    ArgumentNullException.ThrowIfNull(tracking);
    tracking.Advance(ticks);
  }
}
