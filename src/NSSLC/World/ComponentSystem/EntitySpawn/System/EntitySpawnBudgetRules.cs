using System.Numerics;

namespace Terraria.EntitySpawn;

/// <summary>
/// Port of Version4 <c>Spawner.GetSpawnRate</c>: turns world flags and player depth into one roll
/// divisor and one nearby-population cap.
/// </summary>
/// <remarks>
/// The reference computes both from the spawning player, so the same world can roll at different
/// rates for a surface and an underworld player in the same tick. Numbers are taken verbatim from
/// the reference; the remix-world branches are absent because the server does not host a remix
/// world yet, and the invasion/tower branches are absent because those events are not modelled.
/// </remarks>
public static class EntitySpawnBudgetRules
{
  /// <summary>Version4 <c>Spawner.defaultSpawnRate</c>.</summary>
  public const int DefaultSpawnRate = 600;

  /// <summary>Version4 <c>Spawner.defaultMaxSpawns</c>.</summary>
  public const int DefaultMaximumSpawns = 5;

  public static EntitySpawnBudget For(
      in EntitySpawnEnvironment environment,
      Vector2 playerPosition,
      float tileSize)
  {
    int spawnRate = DefaultSpawnRate;
    int maximumSpawns = DefaultMaximumSpawns;
    if (environment.HardMode)
    {
      spawnRate = (int)(DefaultSpawnRate * 0.9);
      maximumSpawns = DefaultMaximumSpawns + 1;
    }

    float playerTileY = playerPosition.Y / tileSize;
    if (playerTileY > environment.UnderworldTileY)
    {
      maximumSpawns = (int)(maximumSpawns * 2f);
    }
    else if (playerTileY > environment.RockLayerTileY)
    {
      // 洞穴层：频率 ×0.4，上限 ×1.9。
      spawnRate = (int)(spawnRate * 0.4);
      maximumSpawns = (int)(maximumSpawns * 1.9f);
    }
    else if (playerTileY > environment.WorldSurfaceTileY)
    {
      // 地下层：困难模式更密（×0.45 / ×1.8），普通模式 ×0.5 / ×1.7。
      if (environment.HardMode)
      {
        spawnRate = (int)(spawnRate * 0.45);
        maximumSpawns = (int)(maximumSpawns * 1.8f);
      }
      else
      {
        spawnRate = (int)(spawnRate * 0.5);
        maximumSpawns = (int)(maximumSpawns * 1.7f);
      }
    }
    else if (!environment.DayTime)
    {
      spawnRate = (int)(spawnRate * 0.6);
      maximumSpawns = (int)(maximumSpawns * 1.3f);
      if (environment.BloodMoon)
      {
        spawnRate = (int)(spawnRate * 0.3);
        maximumSpawns = (int)(maximumSpawns * 1.8f);
      }
    }
    else if (environment.Eclipse)
    {
      spawnRate = (int)(spawnRate * 0.2);
      maximumSpawns = (int)(maximumSpawns * 3f);
    }

    int minimumRate = (int)(DefaultSpawnRate * 0.1);
    if (spawnRate < minimumRate)
    {
      spawnRate = minimumRate;
    }

    int maximumCap = DefaultMaximumSpawns * 3;
    if (maximumSpawns > maximumCap)
    {
      maximumSpawns = maximumCap;
    }

    return new EntitySpawnBudget(spawnRate, maximumSpawns);
  }
}
