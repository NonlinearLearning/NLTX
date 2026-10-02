using System.Numerics;

namespace NLTX.PlayerInputGameplay.Golf;

public sealed class GolfBallTrackRecord
{
  private readonly List<Vector2> _hitLocations = new();

  public IReadOnlyList<Vector2> HitLocations => _hitLocations;

  public void AddHit(Vector2 location)
  {
    _hitLocations.Add(location);
  }

  public void Clear()
  {
    _hitLocations.Clear();
  }
}
