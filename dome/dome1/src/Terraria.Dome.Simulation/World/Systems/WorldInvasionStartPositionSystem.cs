namespace Terraria.Dome.Simulation.WorldModel.Systems;

public readonly record struct WorldInvasionStartPositionResult(bool IsResolved, double Position);

public sealed class WorldInvasionStartPositionSystem
{
  public WorldInvasionStartPositionResult Resolve(int invasionType, int spawnTileX)
  {
    return invasionType == 4
      ? new WorldInvasionStartPositionResult(true, spawnTileX - 1.0)
      : new WorldInvasionStartPositionResult(false, 0.0);
  }
}
