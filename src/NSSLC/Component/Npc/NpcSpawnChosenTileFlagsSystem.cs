using System;

namespace Terraria.Npc;

public static class NpcSpawnChosenTileFlagsSystem
{
  private const int MarbleTileType = 367;
  private const int GraniteTileType = 368;
  private const int OceanRockTileType = 53;
  private const int SpiderWallType = 62;
  private const int SpawnSpiderChanceDenominator = 3;
  private const int SpawnUndergroundDesertChanceDenominator = 3;
  private const int DualDungeonSpawnBandCount = 7;

  public static NpcSpawnChosenTileFlagsResult Calculate(
    in NpcSpawnRateInputs rateInputs,
    in NpcSpawnRateResult rateResult,
    in NpcSpawnAreaInputs areaInputs,
    in NpcSpawnTileSearchResult tileSearchResult,
    in NpcSpawnPostCheckInputs postCheckInputs,
    in NpcSpawnChosenTileWorldInputs worldInputs,
    INpcSpawnChosenTileFlagsPort port)
  {
    ArgumentNullException.ThrowIfNull(port);

    int spawnTileX = tileSearchResult.TileX;
    int spawnTileY = tileSearchResult.TileY;
    int spawnTileType = postCheckInputs.SpawnTileType;
    int playerTileX = rateInputs.Context.PlayerTileX;
    int playerTileY = rateInputs.Context.PlayerTileY;
    NpcSpawnTileFacts spawnTile = port.ReadTile(spawnTileX, spawnTileY);

    bool noWorms = rateResult.NoWorms ||
      (!tileSearchResult.SkyMob && rateInputs.Policy.NoGroundWorms);
    NpcSpawnTileFacts tileAbove = port.ReadTile(spawnTileX, spawnTileY - 1);
    bool waterTile = tileAbove.LiquidAmount > 0;
    if (waterTile)
    {
      waterTile = port.ReadTile(spawnTileX, spawnTileY - 2).LiquidAmount > 0;
      if (waterTile)
      {
        tileAbove = port.ReadTile(spawnTileX, spawnTileY - 1);
        waterTile = tileAbove.LiquidType == 0;
      }
    }

    bool nearMarble = rateInputs.BiomeAndDungeon.NearMarble;
    bool nearGranite = rateInputs.BiomeAndDungeon.NearGranite;
    if (spawnTile.TileType == MarbleTileType)
    {
      nearMarble = true;
    }
    else if (spawnTile.TileType == GraniteTileType)
    {
      nearGranite = true;
    }
    else if (port.ReadTile(playerTileX, playerTileY).TileType == MarbleTileType)
    {
      nearMarble = true;
    }
    else if (port.ReadTile(playerTileX, playerTileY).TileType == GraniteTileType)
    {
      nearGranite = true;
    }
    else
    {
      ScanNearChosenTile(
        spawnTileX,
        spawnTileY,
        areaInputs.MaxTilesX,
        areaInputs.MaxTilesY,
        port,
        ref nearMarble,
        ref nearGranite);
      ScanNearPlayerTile(
        playerTileX,
        playerTileY,
        areaInputs.MaxTilesX,
        areaInputs.MaxTilesY,
        port,
        ref nearMarble,
        ref nearGranite);
    }

    double rockLayer = rateInputs.World.RockLayer;
    double worldSurface = rateInputs.World.WorldSurface;
    int underworldLayer = rateInputs.World.UnderworldLayer;
    bool underGround = (double)spawnTileY <= rockLayer;
    if (rateInputs.World.RemixWorld)
    {
      underGround = (double)spawnTileY > rockLayer &&
        spawnTileY <= areaInputs.MaxTilesY - 190;
    }

    bool undergroundOrCavern = (double)spawnTileY > rockLayer &&
      spawnTileY < underworldLayer;
    if (worldInputs.DontStarveWorld)
    {
      undergroundOrCavern = spawnTileY < underworldLayer;
    }

    bool spawnSpider = rateInputs.Policy.SpawnSpider;
    if (undergroundOrCavern && !rateInputs.BiomeZones.ZoneDungeon &&
      !rateInputs.Policy.Invaders)
    {
      if (Next(port, 0, SpawnSpiderChanceDenominator) == 0)
      {
        int radius = Next(port, 5, 15);
        if (spawnTileX - radius >= 0 &&
          spawnTileX + radius < areaInputs.MaxTilesX)
        {
          for (int tileX = spawnTileX - radius; tileX < spawnTileX + radius; tileX++)
          {
            for (int tileY = spawnTileY - radius; tileY < spawnTileY + radius; tileY++)
            {
              if (port.ReadTile(tileX, tileY).WallType == SpiderWallType)
              {
                spawnSpider = true;
              }
            }
          }
        }
      }
      else if (port.ReadTile(playerTileX, playerTileY).WallType == SpiderWallType)
      {
        spawnSpider = true;
      }
    }

    bool spawnUndergroundDesert = rateInputs.Spatial.SpawnUndergroundDesert;
    if ((double)spawnTileY < rockLayer && spawnTileY > 200 &&
      !rateInputs.BiomeZones.ZoneDungeon && !rateInputs.Policy.Invaders)
    {
      if (Next(port, 0, SpawnUndergroundDesertChanceDenominator) == 0)
      {
        int radius = Next(port, 5, 15);
        if (spawnTileX - radius >= 0 &&
          spawnTileX + radius < areaInputs.MaxTilesX)
        {
          for (int tileX = spawnTileX - radius; tileX < spawnTileX + radius; tileX++)
          {
            for (int tileY = spawnTileY - radius; tileY < spawnTileY + radius; tileY++)
            {
              int wallType = port.ReadTile(tileX, tileY).WallType;
              if (port.AllowsUndergroundDesertEnemiesToSpawn(wallType))
              {
                spawnUndergroundDesert = true;
              }
            }
          }
        }
      }
      else
      {
        int wallType = port.ReadTile(playerTileX, playerTileY).WallType;
        if (port.AllowsUndergroundDesertEnemiesToSpawn(wallType))
        {
          spawnUndergroundDesert = true;
        }
      }
    }

    bool isSpawningInWindDirection =
      (float)(playerTileX - spawnTileX) * worldInputs.WindSpeedTarget > 0f;
    bool surfaceSpawn = (double)spawnTileY <= worldSurface;
    bool deeperThanRockLayer = (double)spawnTileY >= rockLayer;
    bool isOcean =
      ((spawnTileX < worldInputs.OceanDistance ||
        spawnTileX > areaInputs.MaxTilesX - worldInputs.OceanDistance) &&
        worldInputs.SpawnTileIsSand && (double)spawnTileY < rockLayer) ||
      (spawnTileType == OceanRockTileType && port.IsOceanDepths(spawnTileX, spawnTileY));
    bool isBeach = (double)spawnTileY <= worldSurface &&
      (spawnTileX < worldInputs.BeachDistance ||
        spawnTileX > areaInputs.MaxTilesX - worldInputs.BeachDistance);
    bool raining = rateInputs.Context.Raining;
    bool dayTime = rateInputs.Context.DayTime;

    if (rateInputs.World.RemixWorld)
    {
      if ((double)spawnTileY > worldSurface &&
        (double)spawnTileY < rockLayer)
      {
        deeperThanRockLayer = true;
      }
      else
      {
        deeperThanRockLayer = false;
      }

      bool nearRemixSurface = (double)spawnTileY < worldSurface + 5.0;
      bool belowRemixUnderworld = spawnTileY > underworldLayer;
      if (nearRemixSurface || belowRemixUnderworld)
      {
        raining = false;
      }

      if (nearRemixSurface)
      {
        dayTime = false;
      }

      if (rateInputs.BiomeZones.ZoneCorrupt || rateInputs.BiomeZones.ZoneCrimson)
      {
        isOcean = false;
        isBeach = false;
      }

      if ((double)spawnTileX < (double)areaInputs.MaxTilesX * 0.43 ||
        (double)spawnTileX > (double)areaInputs.MaxTilesX * 0.57)
      {
        if ((double)spawnTileY > rockLayer - 200.0 &&
          spawnTileY < areaInputs.MaxTilesY - 200 && Next(port, 0, 2) == 0)
        {
          isOcean = true;
        }

        if ((double)spawnTileY > rockLayer - 200.0 &&
          spawnTileY < areaInputs.MaxTilesY - 200 && Next(port, 0, 2) == 0)
        {
          isBeach = true;
        }
      }

      if ((double)spawnTileY > rockLayer - 20.0)
      {
        if (spawnTileY <= areaInputs.MaxTilesY - 190 && Next(port, 0, 3) != 0)
        {
          surfaceSpawn = true;
          dayTime = Next(port, 0, 2) == 0;
        }
        else if ((rateInputs.World.BloodMoon ||
            (rateInputs.World.Eclipse && dayTime)) &&
          (double)spawnTileX > (double)areaInputs.MaxTilesX * 0.38 + 50.0 &&
          (double)spawnTileX < (double)areaInputs.MaxTilesX * 0.62)
        {
          surfaceSpawn = true;
        }
      }
    }

    if (rateInputs.BiomeAndDungeon.DualDungeonsSpawnRules &&
      (double)spawnTileY > worldSurface && spawnTileY < underworldLayer)
    {
      // Version4 leaves the dual-dungeon zone-flag helper empty.
      switch (Next(port, 0, DualDungeonSpawnBandCount))
      {
        case 0:
        case 1:
          surfaceSpawn = true;
          underGround = false;
          deeperThanRockLayer = false;
          break;
        case 2:
          surfaceSpawn = false;
          underGround = true;
          deeperThanRockLayer = false;
          break;
        default:
          surfaceSpawn = false;
          underGround = false;
          deeperThanRockLayer = true;
          break;
      }
    }

    return new NpcSpawnChosenTileFlagsResult(
      noWorms,
      waterTile,
      nearGranite,
      nearMarble,
      underGround,
      spawnSpider,
      spawnUndergroundDesert,
      isSpawningInWindDirection,
      surfaceSpawn,
      deeperThanRockLayer,
      isOcean,
      isBeach,
      raining,
      dayTime);
  }

  private static void ScanNearChosenTile(
    int spawnTileX,
    int spawnTileY,
    int maxTilesX,
    int maxTilesY,
    INpcSpawnChosenTileFlagsPort port,
    ref bool nearMarble,
    ref bool nearGranite)
  {
    int radius = Next(port, 20, 31);
    int tileXStep = Next(port, 1, 4);
    if (spawnTileX - radius < 0)
    {
      radius = spawnTileX;
    }

    if (spawnTileY - radius < 0)
    {
      radius = spawnTileY;
    }

    if (spawnTileX + radius >= maxTilesX)
    {
      radius = maxTilesX - spawnTileX - 1;
    }

    if (spawnTileY + radius >= maxTilesY)
    {
      radius = maxTilesY - spawnTileY - 1;
    }

    for (int tileX = spawnTileX - radius; tileX <= spawnTileX + radius; tileX += tileXStep)
    {
      int tileYStep = Next(port, 1, 4);
      for (int tileY = spawnTileY - radius; tileY <= spawnTileY + radius; tileY += tileYStep)
      {
        int tileType = port.ReadTile(tileX, tileY).TileType;
        if (tileType == MarbleTileType)
        {
          nearMarble = true;
        }

        if (tileType == GraniteTileType)
        {
          nearGranite = true;
        }
      }
    }
  }

  private static void ScanNearPlayerTile(
    int playerTileX,
    int playerTileY,
    int maxTilesX,
    int maxTilesY,
    INpcSpawnChosenTileFlagsPort port,
    ref bool nearMarble,
    ref bool nearGranite)
  {
    int radius = Next(port, 30, 61);
    int tileXStep = Next(port, 3, 7);
    if (playerTileX - radius < 0)
    {
      radius = playerTileX;
    }

    if (playerTileY - radius < 0)
    {
      radius = playerTileY;
    }

    if (playerTileX + radius >= maxTilesX)
    {
      radius = maxTilesX - playerTileX - 2;
    }

    if (playerTileY + radius >= maxTilesY)
    {
      radius = maxTilesY - playerTileY - 2;
    }

    for (int tileX = playerTileX - radius; tileX <= playerTileX + radius; tileX += tileXStep)
    {
      int tileYStep = Next(port, 3, 7);
      for (int tileY = playerTileY - radius; tileY <= playerTileY + radius; tileY += tileYStep)
      {
        int tileType = port.ReadTile(tileX, tileY).TileType;
        if (tileType == MarbleTileType)
        {
          nearMarble = true;
        }

        if (tileType == GraniteTileType)
        {
          nearGranite = true;
        }
      }
    }
  }

  private static int Next(
    INpcSpawnChosenTileFlagsPort port,
    int minimumInclusive,
    int maximumExclusive)
  {
    int value = port.Next(minimumInclusive, maximumExclusive);
    if (value < minimumInclusive || value >= maximumExclusive)
    {
      throw new InvalidOperationException(
        "The chosen-tile random port returned a value outside its requested range.");
    }

    return value;
  }
}
