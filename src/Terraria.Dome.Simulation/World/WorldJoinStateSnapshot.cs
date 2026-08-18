namespace Terraria.Dome.Simulation.WorldModel;

public readonly record struct WorldJoinStateSnapshot(
  byte GoodBiomeTileType,
  byte EvilBiomeTileType,
  byte BloodBiomeTileType,
  byte AnglerQuest,
  ushort SolarTowerShieldStrength,
  ushort VortexTowerShieldStrength,
  ushort NebulaTowerShieldStrength,
  ushort StardustTowerShieldStrength,
  ushort CavernMonsterTypeOne,
  ushort CavernMonsterTypeTwo,
  ushort CavernMonsterTypeThree,
  ushort CavernMonsterTypeFour,
  ushort CavernMonsterTypeFive,
  ushort CavernMonsterTypeSix)
{
  public static WorldJoinStateSnapshot CreateDefault()
  {
    return new WorldJoinStateSnapshot(
      GoodBiomeTileType: 0,
      EvilBiomeTileType: 6,
      BloodBiomeTileType: 0,
      AnglerQuest: 20,
      SolarTowerShieldStrength: 0,
      VortexTowerShieldStrength: 0,
      NebulaTowerShieldStrength: 0,
      StardustTowerShieldStrength: 0,
      CavernMonsterTypeOne: 503,
      CavernMonsterTypeTwo: 498,
      CavernMonsterTypeThree: 505,
      CavernMonsterTypeFour: 497,
      CavernMonsterTypeFive: 496,
      CavernMonsterTypeSix: 496);
  }
}
