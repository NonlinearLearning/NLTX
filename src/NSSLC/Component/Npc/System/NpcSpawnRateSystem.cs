using System;

namespace Terraria.Npc;

public static class NpcSpawnRateSystem
{
  public static NpcSpawnRateResult Calculate(
    in NpcSpawnRateInputs input,
    INpcSpawnRateRandomPort random)
  {
    ArgumentNullException.ThrowIfNull(random);

    NpcSpawnContextSnapshot context = input.Context;
    NpcSpawnPolicyEligibilitySnapshot policy = input.Policy;
    NpcSpawnBiomeAndDungeonEligibilitySnapshot dungeon = input.BiomeAndDungeon;
    NpcSpawnBiomeZoneEligibilitySnapshot zones = input.BiomeZones;
    NpcSpawnEventAndTowerEligibilitySnapshot events = input.EventAndTower;
    NpcSpawnRatePlayerInputs player = input.Player;
    NpcSpawnRateWorldInputs world = input.World;

    if (world.DefaultSpawnRate <= 0)
    {
      throw new ArgumentOutOfRangeException(
        nameof(input),
        "Default spawn rate must be positive.");
    }

    if (world.DefaultMaxSpawns <= 0)
    {
      throw new ArgumentOutOfRangeException(
        nameof(input),
        "Default maximum spawn count must be positive.");
    }

    int spawnRate = world.DefaultSpawnRate;
    int maxSpawns = world.DefaultMaxSpawns;
    if (world.HardMode)
    {
      spawnRate = (int)((double)world.DefaultSpawnRate * 0.9);
      maxSpawns = world.DefaultMaxSpawns + 1;
    }

    if (player.PositionY > (float)(world.UnderworldLayer * 16))
    {
      maxSpawns = (int)((float)maxSpawns * 2f);
    }
    else if ((double)player.PositionY > world.RockLayer * 16.0 + world.ScreenHeightPixels)
    {
      if (world.RemixWorld)
      {
        if (world.HardMode)
        {
          spawnRate = (int)((double)spawnRate * 0.45);
          maxSpawns = (int)((float)maxSpawns * 1.8f);
        }
        else
        {
          spawnRate = (int)((double)spawnRate * 0.5);
          maxSpawns = (int)((float)maxSpawns * 1.7f);
        }
      }
      else
      {
        spawnRate = (int)((double)spawnRate * 0.4);
        maxSpawns = (int)((float)maxSpawns * 1.9f);
      }
    }
    else if ((double)player.PositionY >
             world.WorldSurface * 16.0 + world.ScreenHeightPixels)
    {
      if (world.RemixWorld)
      {
        spawnRate = (int)((double)spawnRate * 0.4);
        maxSpawns = (int)((float)maxSpawns * 1.9f);
      }
      else if (world.HardMode)
      {
        spawnRate = (int)((double)spawnRate * 0.45);
        maxSpawns = (int)((float)maxSpawns * 1.8f);
      }
      else
      {
        spawnRate = (int)((double)spawnRate * 0.5);
        maxSpawns = (int)((float)maxSpawns * 1.7f);
      }
    }
    else if (world.RemixWorld)
    {
      if (!context.DayTime)
      {
        spawnRate = (int)((double)spawnRate * 0.6);
        maxSpawns = (int)((float)maxSpawns * 1.3f);
      }
    }
    else if (!context.DayTime)
    {
      spawnRate = (int)((double)spawnRate * 0.6);
      maxSpawns = (int)((float)maxSpawns * 1.3f);
      if (world.BloodMoon)
      {
        spawnRate = (int)((double)spawnRate * 0.3);
        maxSpawns = (int)((float)maxSpawns * 1.8f);
      }

      if ((world.PumpkinMoon || world.SnowMoon) &&
          (double)player.PositionY < world.WorldSurface * 16.0)
      {
        spawnRate = (int)((double)spawnRate * 0.2);
        maxSpawns *= 2;
      }
    }
    else if (context.DayTime && world.Eclipse)
    {
      spawnRate = (int)((double)spawnRate * 0.2);
      maxSpawns = (int)((float)maxSpawns * 1.9f);
    }

    if (world.RemixWorld)
    {
      if (!context.DayTime)
      {
        if (world.BloodMoon)
        {
          spawnRate = (int)((double)spawnRate * 0.3);
          maxSpawns = (int)((float)maxSpawns * 1.8f);
          if ((double)player.PositionY > world.RockLayer * 16.0 +
              world.ScreenHeightPixels)
          {
            spawnRate = (int)((double)spawnRate * 0.6);
          }
        }

        if (world.PumpkinMoon || world.SnowMoon)
        {
          spawnRate = (int)((double)spawnRate * 0.2);
          maxSpawns *= 2;
          if ((double)player.PositionY > world.RockLayer * 16.0 +
              world.ScreenHeightPixels)
          {
            spawnRate = (int)((double)spawnRate * 0.6);
          }
        }
      }
      else if (context.DayTime && world.Eclipse)
      {
        spawnRate = (int)((double)spawnRate * 0.2);
        maxSpawns = (int)((float)maxSpawns * 1.9f);
      }
    }

    if (zones.ZoneSnow &&
        (double)(player.PositionY / 16f) < world.WorldSurface)
    {
      maxSpawns = (int)((float)maxSpawns + (float)maxSpawns * world.CloudAlpha);
      spawnRate = (int)((float)spawnRate * (1f - world.CloudAlpha + 1f) / 2f);
    }

    if (world.DrunkWorld && world.PlayerTileHasDrunkWorldWall)
    {
      spawnRate = (int)((double)spawnRate * 0.3);
      maxSpawns = (int)((float)maxSpawns * 1.8f);
    }

    if (dungeon.InDualDungeon || zones.ZoneDungeon)
    {
      spawnRate = (int)((double)spawnRate * 0.3);
      maxSpawns = (int)((float)maxSpawns * 1.8f);
    }
    else if (zones.ZoneSandstorm)
    {
      spawnRate = (int)((float)spawnRate * (world.HardMode ? 0.4f : 0.9f));
      maxSpawns = (int)((float)maxSpawns * (world.HardMode ? 1.5f : 1.2f));
    }
    else if (player.ZoneUndergroundDesert)
    {
      spawnRate = (int)((float)spawnRate * 0.2f);
      maxSpawns = (int)((float)maxSpawns * 3f);
    }
    else if (zones.ZoneJungle)
    {
      if (policy.TownNpcCount == 0)
      {
        spawnRate = (int)((double)spawnRate * 0.4);
        maxSpawns = (int)((float)maxSpawns * 1.5f);
      }
      else if (policy.TownNpcCount == 1)
      {
        spawnRate = (int)((double)spawnRate * 0.55);
        maxSpawns = (int)((double)maxSpawns * 1.4);
      }
      else if (policy.TownNpcCount == 2)
      {
        spawnRate = (int)((double)spawnRate * 0.7);
        maxSpawns = (int)((float)maxSpawns * 1.3f);
      }
      else
      {
        spawnRate = (int)((double)spawnRate * 0.85);
        maxSpawns = (int)((float)maxSpawns * 1.2f);
      }
    }
    else if (zones.ZoneCorrupt || zones.ZoneCrimson)
    {
      spawnRate = (int)((double)spawnRate * 0.65);
      maxSpawns = (int)((float)maxSpawns * 1.3f);
    }
    else if (zones.ZoneMeteor)
    {
      spawnRate = (int)((double)spawnRate * 0.4);
      maxSpawns = (int)((float)maxSpawns * 1.1f);
    }

    if (zones.ZoneLihzhardTemple)
    {
      spawnRate = (int)((float)spawnRate * 0.8f);
      maxSpawns = (int)((float)maxSpawns * 1.2f);
      if (world.RemixWorld)
      {
        spawnRate = (int)((double)spawnRate * 0.4);
        maxSpawns = (int)((float)maxSpawns * 1.5f);
      }
    }

    if (world.RemixWorld && (zones.ZoneCorrupt || zones.ZoneCrimson) &&
        (double)(player.PositionY / 16f) < world.WorldSurface)
    {
      spawnRate = (int)((double)spawnRate * 0.5);
      maxSpawns *= 2;
    }

    if (zones.ZoneHallow &&
        (double)player.PositionY > world.RockLayer * 16.0 +
          world.ScreenHeightPixels)
    {
      spawnRate = (int)((double)spawnRate * 0.65);
      maxSpawns = (int)((float)maxSpawns * 1.3f);
    }

    if (dungeon.TresspassingDualDungeon)
    {
      spawnRate = (int)((float)spawnRate * 0.6f);
      maxSpawns = (int)((float)maxSpawns * 1.3f);
    }

    if (world.WallOfFleshPresent &&
        player.PositionY > (float)(world.UnderworldLayer * 16))
    {
      maxSpawns = (int)((float)maxSpawns * 0.3f);
      spawnRate *= 3;
    }

    if ((double)player.NearbyActiveNpcSlots < (double)maxSpawns * 0.2)
    {
      spawnRate = (int)((float)spawnRate * 0.6f);
    }
    else if ((double)player.NearbyActiveNpcSlots < (double)maxSpawns * 0.4)
    {
      spawnRate = (int)((float)spawnRate * 0.7f);
    }
    else if ((double)player.NearbyActiveNpcSlots < (double)maxSpawns * 0.6)
    {
      spawnRate = (int)((float)spawnRate * 0.8f);
    }
    else if ((double)player.NearbyActiveNpcSlots < (double)maxSpawns * 0.8)
    {
      spawnRate = (int)((float)spawnRate * 0.9f);
    }

    if ((double)(player.PositionY / 16f) >
          (world.WorldSurface + world.RockLayer) / 2.0 ||
        zones.ZoneCorrupt || zones.ZoneCrimson)
    {
      if ((double)player.NearbyActiveNpcSlots < (double)maxSpawns * 0.2)
      {
        spawnRate = (int)((float)spawnRate * 0.7f);
      }
      else if ((double)player.NearbyActiveNpcSlots < (double)maxSpawns * 0.4)
      {
        spawnRate = (int)((float)spawnRate * 0.9f);
      }
    }

    if (world.RemixWorld && (double)(player.PositionY / 16f) < world.WorldSurface &&
        (zones.ZoneCorrupt || zones.ZoneCrimson))
    {
      spawnRate = (int)((double)spawnRate * 0.8);
      maxSpawns *= 2;
    }

    if (player.IsInvisible)
    {
      spawnRate = (int)((float)spawnRate * 1.2f);
      maxSpawns = (int)((float)maxSpawns * 0.8f);
    }

    if (player.IsCalmed)
    {
      spawnRate = (int)((float)spawnRate * 1.65f);
      maxSpawns = (int)((float)maxSpawns * 0.6f);
    }

    if (player.HasSunflower)
    {
      spawnRate = (int)((float)spawnRate * 1.2f);
      maxSpawns = (int)((float)maxSpawns * 0.8f);
    }

    if (player.HasAnglerSetSpawnReduction)
    {
      spawnRate = (int)((float)spawnRate * 1.3f);
      maxSpawns = (int)((float)maxSpawns * 0.7f);
    }

    if (player.EnemySpawnsEnabled)
    {
      spawnRate = (int)((double)spawnRate * 0.5);
      maxSpawns = (int)((float)maxSpawns * 2f);
    }

    if (events.ZoneWaterCandle)
    {
      if (!events.ZonePeaceCandle)
      {
        spawnRate = (int)((double)spawnRate * 0.75);
        maxSpawns = (int)((float)maxSpawns * 1.5f);
      }
    }
    else if (events.ZonePeaceCandle)
    {
      spawnRate = (int)((double)spawnRate * 1.3);
      maxSpawns = (int)((float)maxSpawns * 0.7f);
    }

    if (events.ZoneWaterCandle &&
        (double)(player.PositionY / 16f) <
          world.WorldSurface * 0.3499999940395355)
    {
      spawnRate = (int)((double)spawnRate * 0.5);
    }

    if (player.HasNearbyFairy)
    {
      spawnRate = (int)((float)spawnRate * 1.2f);
      maxSpawns = (int)((float)maxSpawns * 0.8f);
    }

    if ((double)spawnRate < (double)world.DefaultSpawnRate * 0.1)
    {
      spawnRate = (int)((double)world.DefaultSpawnRate * 0.1);
    }

    if (maxSpawns > world.DefaultMaxSpawns * 3)
    {
      maxSpawns = world.DefaultMaxSpawns * 3;
    }

    if (world.GetGoodWorld)
    {
      spawnRate = (int)((float)spawnRate * 0.8f);
      maxSpawns = (int)((float)maxSpawns * 1.2f);
    }

    if (world.JourneyMode && world.SpawnRatePowerUnlocked &&
        world.HasRemappedJourneySpawnRate)
    {
      if (!float.IsFinite(world.JourneySpawnRateValue) ||
          world.JourneySpawnRateValue <= 0f)
      {
        throw new ArgumentOutOfRangeException(
          nameof(input),
          "An unlocked Journey spawn-rate value must be finite and positive.");
      }

      spawnRate = (int)((float)spawnRate / world.JourneySpawnRateValue);
      maxSpawns = (int)((float)maxSpawns * world.JourneySpawnRateValue);
    }

    if ((world.PumpkinMoon || world.SnowMoon) &&
        (world.RemixWorld || (double)player.PositionY < world.WorldSurface * 16.0))
    {
      maxSpawns = (int)((double)world.DefaultMaxSpawns *
        (2.0 + 0.3 * (double)context.NumberOfActivePlayers));
      spawnRate = 20;
    }

    if (world.OldOnesArmyOngoing && events.ZoneOldOneArmy)
    {
      maxSpawns = world.DefaultMaxSpawns;
      spawnRate = world.DefaultSpawnRate;
    }

    if (policy.Invaders)
    {
      maxSpawns = (int)((double)world.DefaultMaxSpawns *
        (2.0 + 0.3 * (double)context.NumberOfActivePlayers));
      spawnRate = 20;
    }

    if (zones.ZoneDungeon && !world.DownedBoss3)
    {
      spawnRate = 10;
    }

    if (world.SkyblockLowTiles)
    {
      spawnRate /= 2;
    }

    bool infectedEvil = zones.ZoneCorrupt || zones.ZoneCrimson;
    if (world.InfectedSeed)
    {
      infectedEvil = false;
    }

    bool isNightMoonOrEclipse =
      ((!world.BloodMoon && !world.PumpkinMoon && !world.SnowMoon) || context.DayTime) &&
      (!world.Eclipse || !context.DayTime);
    if (!policy.Invaders && isNightMoonOrEclipse && !infectedEvil &&
        !zones.ZoneCrimson && !zones.ZoneMeteor && !events.ZoneOldOneArmy)
    {
      if (player.CenterY / 16f > (float)world.UnderworldLayer &&
          !input.Spatial.InRemixStartingArea)
      {
        if (policy.TownNpcCount == 1)
        {
          if (!world.SkyblockLowTiles)
          {
            if (Next(random, 2) == 0)
            {
              policy = policy with { NoWorms = true };
            }

            if (Next(random, 10) == 0)
            {
              policy = policy with { SpawnFriendly = true };
              maxSpawns = (int)((double)(float)maxSpawns * 0.5);
            }
            else
            {
              spawnRate = (int)((double)(float)spawnRate * 1.25);
            }
          }
        }
        else if (policy.TownNpcCount == 2)
        {
          if (Next(random, 4) != 0)
          {
            policy = policy with { NoWorms = true };
          }

          if (Next(random, 5) == 0)
          {
            policy = policy with { SpawnFriendly = true };
            maxSpawns = (int)((double)(float)maxSpawns * 0.5);
          }
          else
          {
            spawnRate = (int)((double)(float)spawnRate * 1.5);
          }
        }
        else if (policy.TownNpcCount >= 3)
        {
          if (Next(random, 10) != 0)
          {
            policy = policy with { NoWorms = true };
          }

          if (Next(random, 3) == 0)
          {
            policy = policy with { SpawnFriendly = true };
            maxSpawns = (int)((double)(float)maxSpawns * 0.5);
          }
          else
          {
            spawnRate = (int)((float)spawnRate * 2f);
          }
        }
      }
      else if (policy.TownNpcCount == 1)
      {
        policy = policy with { NoWorms = true };
        if (!world.SkyblockLowTiles)
        {
          if (zones.ZoneGraveyard &&
              (!events.ZonePeaceCandle || Next(random, 3) == 0))
          {
            spawnRate = (int)((double)(float)spawnRate * 1.66);
            if (Next(random, 9) == 1)
            {
              policy = policy with { SpawnFriendly = true };
              maxSpawns = (int)((double)(float)maxSpawns * 0.6);
            }
          }
          else if (Next(random, 3) == 1)
          {
            policy = policy with { SpawnFriendly = true };
            maxSpawns = (int)((double)(float)maxSpawns * 0.6);
          }
          else
          {
            spawnRate = (int)((float)spawnRate * 2f);
          }
        }
      }
      else if (policy.TownNpcCount == 2)
      {
        policy = policy with { NoWorms = true };
        if (zones.ZoneGraveyard &&
            (!events.ZonePeaceCandle || Next(random, 3) == 0))
        {
          spawnRate = (int)((double)(float)spawnRate * 2.33);
          if (Next(random, 6) == 1)
          {
            policy = policy with { SpawnFriendly = true };
            maxSpawns = (int)((double)(float)maxSpawns * 0.6);
          }
        }
        else if (Next(random, 3) != 0)
        {
          policy = policy with { SpawnFriendly = true };
          maxSpawns = (int)((double)(float)maxSpawns * 0.6);
        }
        else
        {
          spawnRate = (int)((float)spawnRate * 3f);
        }
      }
      else if (policy.TownNpcCount >= 3)
      {
        policy = policy with { NoWorms = true };
        if (zones.ZoneGraveyard &&
            (!events.ZonePeaceCandle || Next(random, 3) == 0))
        {
          spawnRate = (int)((float)spawnRate * 3f);
          if (Next(random, 3) == 1)
          {
            policy = policy with { SpawnFriendly = true };
            maxSpawns = (int)((double)(float)maxSpawns * 0.6);
          }
        }
        else
        {
          if (!world.ExpertMode || Next(random, 30) != 0)
          {
            policy = policy with { SpawnFriendly = true };
          }

          maxSpawns = (int)((double)(float)maxSpawns * 0.6);
        }
      }
    }

    if (!policy.SpawnFriendly &&
        RollOnlyBadLuckExtreme(random, context.Luck, 50) == 0)
    {
      spawnRate = (int)((float)spawnRate * 0.85f);
      maxSpawns = (int)((float)maxSpawns * 1.15f);
    }

    return new NpcSpawnRateResult(
      spawnRate,
      maxSpawns,
      policy.NoWorms,
      policy.SpawnFriendly);
  }

  private static int Next(INpcSpawnRateRandomPort random, int exclusiveUpperBound)
  {
    int value = random.Next(exclusiveUpperBound);
    if (value < 0 || value >= exclusiveUpperBound)
    {
      throw new InvalidOperationException(
        "The spawn random port returned a value outside its requested range.");
    }

    return value;
  }

  private static int RollOnlyBadLuckExtreme(
    INpcSpawnRateRandomPort random,
    float luck,
    int range)
  {
    int value = random.RollOnlyBadLuckExtreme(luck, range);
    if (value < 0 || value >= range)
    {
      throw new InvalidOperationException(
        "The luck random port returned a value outside its requested range.");
    }

    return value;
  }
}
