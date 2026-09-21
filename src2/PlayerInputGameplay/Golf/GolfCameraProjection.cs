using System.Numerics;

namespace NLTX.PlayerInputGameplay.Golf;

public sealed class GolfCameraProjection
{
  public int? TrackedProjectileId { get; private set; }

  public Vector2? TrackedLocation { get; private set; }

  public void Follow(int projectileId, Vector2 location)
  {
    TrackedProjectileId = projectileId;
    TrackedLocation = location;
  }

  public void Clear()
  {
    TrackedProjectileId = null;
    TrackedLocation = null;
  }
}
