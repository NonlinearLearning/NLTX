namespace Terraria.SpatialMotionPhysics;

public static class SpawnCadenceSystem
{
  public static void Tick(SpawnCadenceState state)
  {
    ArgumentNullException.ThrowIfNull(state);
    if (state.CheckForSpawns > 0)
    {
      state.CheckForSpawns--;
    }
  }
}
