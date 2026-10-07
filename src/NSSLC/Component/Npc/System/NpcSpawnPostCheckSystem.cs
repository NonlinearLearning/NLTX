using System;

namespace Terraria.Npc;

public static class NpcSpawnPostCheckSystem
{
  private const int DualDungeonForbiddenTileType = 48;
  private const int CorruptSandstoneTileType = 477;
  private const int CrimsonSandstoneTileType = 492;
  private const int SandstoneSpawnRejectionChance = 10;
  private const int SandstoneSpawnChanceDenominator = 100;

  public static bool IsAccepted(
    in NpcSpawnPostCheckInputs inputs,
    INpcSpawnRateRandomPort random)
  {
    ArgumentNullException.ThrowIfNull(random);

    if (inputs.ZoneDungeon &&
      (!inputs.IsDungeonTile || inputs.SpawnWallType == 0))
    {
      return false;
    }

    if (inputs.DualDungeonsSeed &&
      inputs.SpawnTileType == DualDungeonForbiddenTileType)
    {
      return false;
    }

    if (inputs.HasLiquidAtTileAbove &&
      inputs.HasLiquidAtTwoTilesAbove &&
      !inputs.TileAboveIsLava)
    {
      if (inputs.TileAboveIsShimmer || inputs.TileAboveIsHoney)
      {
        return false;
      }
    }

    bool isSandstoneTile =
      inputs.SpawnTileType == CorruptSandstoneTileType ||
      inputs.SpawnTileType == CrimsonSandstoneTileType;
    bool specialEventActive = inputs.BloodMoon ||
      inputs.Eclipse ||
      inputs.InvasionType > 0 ||
      inputs.PumpkinMoon ||
      inputs.SnowMoon ||
      inputs.SlimeRain;
    if (isSandstoneTile && !specialEventActive)
    {
      int roll = random.Next(SandstoneSpawnChanceDenominator);
      if (roll < 0 || roll >= SandstoneSpawnChanceDenominator)
      {
        throw new InvalidOperationException(
          "The spawn random port returned a value outside its requested range.");
      }

      if (roll < SandstoneSpawnRejectionChance)
      {
        return false;
      }
    }

    return true;
  }
}
