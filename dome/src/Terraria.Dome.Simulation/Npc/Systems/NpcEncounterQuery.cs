using System;

namespace Terraria.Dome.Simulation.Npc.Systems;

public static class NpcEncounterQuery
{
  private const int MechQueenNetId = 127;

  public static bool HasAnyPreHardmodeBossBeenDefeated(
    bool defeatedSlimeKing,
    bool defeatedEyeOfCthulhu,
    bool defeatedEaterOrBrain,
    bool defeatedSkeletron,
    bool defeatedQueenBee,
    bool defeatedDeerclops,
    bool defeatedWallOfFlesh)
  {
    return defeatedSlimeKing || defeatedEyeOfCthulhu || defeatedEaterOrBrain ||
      defeatedSkeletron || defeatedQueenBee || defeatedDeerclops || defeatedWallOfFlesh;
  }

  public static bool IsMechQueenUp(
    int mechQueenIndex,
    int maximumNpcSlots,
    bool isActive,
    int netId)
  {
    return mechQueenIndex >= 0 && mechQueenIndex < maximumNpcSlots && isActive &&
      netId == MechQueenNetId;
  }

  public static int GetShieldStrengthTowerMax(
    int lunarShieldPowerNormal,
    bool downedMoonLord)
  {
    if (lunarShieldPowerNormal < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(lunarShieldPowerNormal));
    }

    return downedMoonLord ? lunarShieldPowerNormal / 2 : lunarShieldPowerNormal;
  }
}
