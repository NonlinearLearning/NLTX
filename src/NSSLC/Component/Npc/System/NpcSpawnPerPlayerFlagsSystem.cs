using System;

namespace Terraria.Npc;

public static class NpcSpawnPerPlayerFlagsSystem
{
  private const float InvasionTownNpcRange = 3000f;
  private const int InvasionRandomDenominator = 3;
  private const int HelmetOfTimItemType = 4256;
  private const int TimArmorSetFirstItemType = 1282;
  private const int TimArmorSetLastItemType = 1287;
  private const int TimArmorHeadSlotCancellationItemType = 238;
  private const int StartingHealthMaximum = 100;
  private const int SpiderWallType = 73;
  private const int LivingTreeWallType = 244;

  public static NpcSpawnRateInputs Prepare(
    int playerIndex,
    INpcSpawnPerPlayerFlagsPort port)
  {
    ArgumentNullException.ThrowIfNull(port);

    NpcSpawnPerPlayerFlagsPrelude prelude =
      port.CapturePerPlayerFlagsPrelude(playerIndex);
    NpcSpawnInvasionInputs invasionInputs = port.CaptureInvasionInputs(playerIndex);
    bool invaders = ShouldSpawnInvasionEnemies(in invasionInputs, port);
    NpcSpawnPerPlayerFlagsPostlude postlude =
      port.CapturePerPlayerFlagsPostlude(playerIndex);
    NpcSpawnRateInputs rateInputs = port.CaptureSpawnRateInputs(playerIndex);

    return Calculate(in rateInputs, in prelude, invaders, in postlude);
  }

  public static NpcSpawnRateInputs Calculate(
    in NpcSpawnRateInputs rateInputs,
    in NpcSpawnPerPlayerFlagsPrelude prelude,
    bool invaders,
    in NpcSpawnPerPlayerFlagsPostlude postlude)
  {
    bool shadowCandle = prelude.EventAndTower.ZoneShadowCandle;
    bool towerInvasion = prelude.EventAndTower.ZoneTowerSolar ||
      prelude.EventAndTower.ZoneTowerNebula ||
      prelude.EventAndTower.ZoneTowerVortex ||
      prelude.EventAndTower.ZoneTowerStardust;
    bool inDualDungeon = prelude.DualDungeonsSeed &&
      prelude.PlayerInsideUnbreakableWalls;

    NpcSpawnContextSnapshot context = rateInputs.Context with
    {
      PlayerTileX = prelude.Context.PlayerTileX,
      PlayerTileY = prelude.Context.PlayerTileY,
      Luck = prelude.Context.Luck,
      DayTime = prelude.Context.DayTime,
      Raining = prelude.Context.Raining,
    };
    NpcSpawnPolicyEligibilitySnapshot policy = rateInputs.Policy with
    {
      TownNpcCount = shadowCandle ? 0 : postlude.PlayerTownNpcCount,
      SkyMob = false,
      NoWorms = !shadowCandle && postlude.PlayerTileIsInWorld &&
        postlude.PlayerTileHasHouseWall,
      NoGroundWorms = !shadowCandle &&
        postlude.PlayerAfkCounter >= postlude.AfkTimeNeededForNoWormSpawns,
      Invaders = invaders || towerInvasion,
      SpawnFriendly = false,
      IgnoreSafeWalls = towerInvasion,
      SpawnSpider = false,
      OffensiveToTim = IsOffensiveToTim(postlude),
      PlayerHasStartingHealth = postlude.PlayerMaximumLife <= StartingHealthMaximum,
    };
    NpcSpawnSpatialEligibilitySnapshot spatial = rateInputs.Spatial with
    {
      SpawnUndergroundDesert = false,
      HardDungeon = prelude.DownedPlantBoss && prelude.HardMode,
      SkyBehindPlayer = postlude.PlayerTileHasLightWall ||
        postlude.PlayerTileWallType == SpiderWallType,
      LivingTree = postlude.PlayerTileWallType == LivingTreeWallType,
      InRemixStartingArea = IsInRemixStartingArea(postlude),
    };
    NpcSpawnBiomeAndDungeonEligibilitySnapshot biomeAndDungeon =
      rateInputs.BiomeAndDungeon with
      {
        WaterTile = false,
        NearGranite = false,
        NearMarble = false,
        DualDungeonsSpawnRules = prelude.DualDungeonsSeed,
        InDualDungeon = inDualDungeon,
        TresspassingDualDungeon = inDualDungeon &&
          prelude.DungeonProgressCanSafelyMatch <
            prelude.DungeonProgressPlayerNeedsToMatch,
      };
    NpcSpawnBiomeZoneEligibilitySnapshot biomeZones = rateInputs.BiomeZones with
    {
      ZoneCorrupt = prelude.BiomeZones.ZoneCorrupt,
      ZoneCrimson = prelude.BiomeZones.ZoneCrimson,
      ZoneHallow = prelude.BiomeZones.ZoneHallow,
      ZoneJungle = prelude.BiomeZones.ZoneJungle,
      ZoneSnow = prelude.BiomeZones.ZoneSnow,
      ZoneGlowshroom = prelude.BiomeZones.ZoneGlowshroom,
      ZoneMeteor = prelude.BiomeZones.ZoneMeteor,
      ZoneGraveyard = prelude.BiomeZones.ZoneGraveyard,
      ZoneDungeon = prelude.BiomeZones.ZoneDungeon,
      ZoneLihzhardTemple = prelude.BiomeZones.ZoneLihzhardTemple,
      ZoneSandstorm = prelude.BiomeZones.ZoneSandstorm,
    };

    return rateInputs with
    {
      Context = context,
      Policy = policy,
      Spatial = spatial,
      BiomeAndDungeon = biomeAndDungeon,
      BiomeZones = biomeZones,
      EventAndTower = prelude.EventAndTower,
    };
  }

  private static bool ShouldSpawnInvasionEnemies(
    in NpcSpawnInvasionInputs inputs,
    INpcSpawnPerPlayerFlagsPort port)
  {
    if (inputs.InvasionType <= 0 || inputs.InvasionDelay != 0 || inputs.InvasionSize <= 0)
    {
      return false;
    }

    if ((double)inputs.PlayerPositionY < inputs.WorldSurface * 16.0 + inputs.ScreenHeightPixels ||
      (double)inputs.SpawnTileY > inputs.WorldSurface)
    {
      if ((double)inputs.PlayerPositionX > inputs.InvasionX * 16.0 - InvasionTownNpcRange &&
        (double)inputs.PlayerPositionX < inputs.InvasionX * 16.0 + InvasionTownNpcRange)
      {
        return true;
      }

      if (inputs.InvasionX >= (double)(inputs.MaxTilesX / 2 - 5) &&
        inputs.InvasionX <= (double)(inputs.MaxTilesX / 2 + 5))
      {
        for (var npcIndex = 0; npcIndex < inputs.MaxNpcSlots; npcIndex++)
        {
          if (port.IsTownNpcSlot(npcIndex) &&
            Math.Abs(inputs.PlayerPositionX - port.GetNpcCenterX(npcIndex)) <
              InvasionTownNpcRange)
          {
            int randomValue = port.Next(InvasionRandomDenominator);
            if (randomValue < 0 || randomValue >= InvasionRandomDenominator)
            {
              throw new InvalidOperationException(
                "The invasion random port returned a value outside its requested range.");
            }

            if (randomValue == 0)
            {
              break;
            }

            return true;
          }
        }
      }
    }

    return false;
  }

  private static bool IsOffensiveToTim(NpcSpawnPerPlayerFlagsPostlude inputs)
  {
    bool hasTimHelmet = inputs.ArmorSlot1ItemType == HelmetOfTimItemType ||
      (inputs.ArmorSlot1ItemType >= TimArmorSetFirstItemType &&
        inputs.ArmorSlot1ItemType <= TimArmorSetLastItemType);
    return hasTimHelmet && inputs.ArmorSlot0ItemType !=
      TimArmorHeadSlotCancellationItemType;
  }

  private static bool IsInRemixStartingArea(NpcSpawnPerPlayerFlagsPostlude inputs)
  {
    double playerTileX = (double)(inputs.PlayerCenterX / 16f);
    return inputs.RemixWorld &&
      playerTileX > (double)inputs.MaxTilesX * 0.39 + 50.0 &&
      playerTileX < (double)inputs.MaxTilesX * 0.61;
  }
}
