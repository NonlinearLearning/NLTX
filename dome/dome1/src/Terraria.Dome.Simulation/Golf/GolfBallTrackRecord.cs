using System.Collections.Generic;
using System.Numerics;

namespace Terraria.Dome.Simulation.Golf;

public sealed class GolfBallTrackRecord
{
  private readonly List<Vector2> _hitLocations = new();

  public IReadOnlyList<Vector2> HitLocations => _hitLocations;

  public void RecordHit(Vector2 location)
  {
    _hitLocations.Add(location);
  }
}
