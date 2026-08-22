namespace Terraria.Dome.Simulation.WorldGeneration;

public static class HarvestableHerbQuery
{
  public static bool IsHarvestableWithSeed(
    int type,
    int style,
    int y,
    bool alchemyPlantHarvestable)
  {
    return type switch
    {
      83 or 84 => type == 84 || alchemyPlantHarvestable,
      _ => false
    };
  }
}
