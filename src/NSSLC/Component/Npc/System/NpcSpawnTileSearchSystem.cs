using System;
using Terraria.Npc.Queries;

namespace Terraria.Npc;

public static class NpcSpawnTileSearchSystem
{
  private const int MaximumAttempts = 50;

  public static NpcSpawnTileSearchResult Find(
    in NpcSpawnAreaInputs areaInputs,
    in NpcSpawnRateInputs rateInputs,
    in NpcSpawnRateResult rateResult,
    INpcSpawnTileSearchPort port)
  {
    ArgumentNullException.ThrowIfNull(port);

    NpcSpawnAreaResult area = NpcSpawnAreaQuery.Calculate(in areaInputs);
    NpcSpawnTileRectangle spawnArea = area.SpawnArea;
    NpcSpawnTileRectangle safeArea = area.SafeArea;
    NpcSpawnContextSnapshot context = rateInputs.Context;
    NpcSpawnPolicyEligibilitySnapshot policy = rateInputs.Policy;
    NpcSpawnRateWorldInputs world = rateInputs.World;
    bool skyMob = policy.SkyMob;

    for (var attempt = 0; attempt < MaximumAttempts; attempt++)
    {
      int tileX = Next(port, spawnArea.Left, spawnArea.Right);
      int tileY = Next(port, spawnArea.Top, spawnArea.Bottom);

      if (port.IsActiveSolidTile(tileX, tileY) ||
        (!policy.IgnoreSafeWalls && port.IsHouseWallTile(tileX, tileY)))
      {
        continue;
      }

      if (!policy.Invaders &&
        (double)tileY < world.WorldSurface * 0.3499999940395355 &&
        !rateResult.SpawnFriendly &&
        ((double)tileX < (double)areaInputs.MaxTilesX * 0.45 ||
         (double)tileX > (double)areaInputs.MaxTilesX * 0.55 ||
         world.HardMode))
      {
        skyMob = true;
      }
      else if (!policy.Invaders &&
        (double)tileY < world.WorldSurface * 0.44999998807907104 &&
        !rateResult.SpawnFriendly &&
        world.HardMode &&
        Next(port, 10) == 0)
      {
        skyMob = true;
      }
      else
      {
        while (tileY < areaInputs.MaxTilesY &&
          tileY < spawnArea.Bottom &&
          !port.IsActiveSolidTile(tileX, tileY))
        {
          tileY++;
        }

        if (tileY >= spawnArea.Bottom)
        {
          continue;
        }
      }

      if (!safeArea.Contains(tileX, tileY) &&
        HasTileSpawnSpace(
          tileX,
          tileY,
          context.SpawnSpaceX,
          context.SpawnSpaceY,
          areaInputs.MaxTilesX,
          areaInputs.MaxTilesY,
          port))
      {
        bool xRange = tileX >= safeArea.Left && tileX < safeArea.Right;
        return new NpcSpawnTileSearchResult(
          true,
          tileX,
          tileY,
          xRange,
          skyMob,
          area);
      }
    }

    return new NpcSpawnTileSearchResult(
      false,
      0,
      0,
      false,
      skyMob,
      area);
  }

  private static bool HasTileSpawnSpace(
    int tileX,
    int tileY,
    int spawnSpaceX,
    int spawnSpaceY,
    int maxTilesX,
    int maxTilesY,
    INpcSpawnTileSearchPort port)
  {
    NpcSpawnTileRectangle tileArea = new(
      tileX - spawnSpaceX / 2,
      tileY - spawnSpaceY,
      spawnSpaceX,
      spawnSpaceY);
    if (tileArea.Left < 0 ||
      tileArea.Right >= maxTilesX ||
      tileArea.Top < 0 ||
      tileArea.Bottom >= maxTilesY)
    {
      return false;
    }

    for (int x = tileArea.Left; x < tileArea.Right; x++)
    {
      for (int y = tileArea.Top; y < tileArea.Bottom; y++)
      {
        NpcSpawnTileSpaceFacts facts = port.CaptureTileSpaceFacts(x, y);
        if (!NpcSpawnTileSpaceQuery.CanSpawn(in facts))
        {
          return false;
        }
      }
    }

    return true;
  }

  private static int Next(
    INpcSpawnRateRandomPort random,
    int exclusiveUpperBound)
  {
    if (exclusiveUpperBound <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(exclusiveUpperBound));
    }

    int value = random.Next(exclusiveUpperBound);
    if (value < 0 || value >= exclusiveUpperBound)
    {
      throw new InvalidOperationException(
        "The spawn random port returned a value outside its requested range.");
    }

    return value;
  }

  private static int Next(
    INpcSpawnTileSearchPort random,
    int minimumInclusive,
    int maximumExclusive)
  {
    if (minimumInclusive > maximumExclusive)
    {
      throw new ArgumentOutOfRangeException(nameof(maximumExclusive));
    }

    int value = random.Next(minimumInclusive, maximumExclusive);
    if (minimumInclusive == maximumExclusive)
    {
      if (value != minimumInclusive)
      {
        throw new InvalidOperationException(
          "The spawn random port returned a value outside its degenerate range.");
      }

      return value;
    }

    if (value < minimumInclusive || value >= maximumExclusive)
    {
      throw new InvalidOperationException(
        "The spawn random port returned a value outside its requested range.");
    }

    return value;
  }
}
