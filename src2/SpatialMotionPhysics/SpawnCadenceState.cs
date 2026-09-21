namespace Terraria.SpatialMotionPhysics;

public sealed class SpawnCadenceState
{
  public SpawnCadenceState(int initialValue)
  {
    CheckForSpawns = initialValue;
  }

  public int CheckForSpawns { get; internal set; }
}
