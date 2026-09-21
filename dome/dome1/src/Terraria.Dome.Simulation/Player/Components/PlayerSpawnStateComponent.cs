namespace Terraria.Dome.Simulation.Player.Components;

public struct PlayerSpawnStateComponent
{
  public const int UnsetCoordinate = -1;

  public int SpawnX;
  public int SpawnY;

  public PlayerSpawnStateComponent(int spawnX = UnsetCoordinate, int spawnY = UnsetCoordinate)
  {
    SpawnX = spawnX;
    SpawnY = spawnY;
  }

  public readonly bool HasSpawn => SpawnX >= 0 && SpawnY >= 0;

  public void Set(int spawnX, int spawnY)
  {
    if (spawnX < 0 || spawnY < 0)
    {
      throw new System.ArgumentOutOfRangeException(nameof(spawnX));
    }

    SpawnX = spawnX;
    SpawnY = spawnY;
  }

  public void Clear()
  {
    SpawnX = UnsetCoordinate;
    SpawnY = UnsetCoordinate;
  }
}
