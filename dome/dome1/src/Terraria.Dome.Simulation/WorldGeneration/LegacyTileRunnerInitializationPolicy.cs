namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed record LegacyTileRunnerInitialization(
  double DirectionX,
  double DirectionY,
  short LiquidType,
  bool ConsumedDirectionRandom,
  bool ConsumedLiquidTypeRandom);

public static class LegacyTileRunnerInitializationPolicy
{
  public static LegacyTileRunnerInitialization Create(
    double speedX,
    double speedY,
    int randomDirectionX,
    int randomDirectionY,
    int liquidTypeRoll3,
    int liquidTypeRoll4,
    bool notTheBees,
    bool dontStarve,
    bool remixWorld,
    bool drunkWorld,
    bool tenthAnniversary,
    bool getGoodWorld)
  {
    bool hasExplicitSpeed = speedX != 0.0 || speedY != 0.0;
    double directionX = hasExplicitSpeed ? speedX : randomDirectionX * 0.1;
    double directionY = hasExplicitSpeed ? speedY : randomDirectionY * 0.1;
    short liquidType = 0;
    bool consumedLiquidTypeRandom = false;
    if (notTheBees && dontStarve && !remixWorld && liquidTypeRoll3 == 0)
    {
      liquidType = 2;
      consumedLiquidTypeRandom = true;
    }
    else if (liquidTypeRoll4 == 0)
    {
      consumedLiquidTypeRandom = true;
      if (drunkWorld && tenthAnniversary && remixWorld && !notTheBees)
      {
        liquidType = 3;
      }
      else if (getGoodWorld)
      {
        liquidType = 1;
      }
    }

    return new LegacyTileRunnerInitialization(
      directionX,
      directionY,
      liquidType,
      !hasExplicitSpeed,
      consumedLiquidTypeRandom);
  }
}
