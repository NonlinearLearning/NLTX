namespace Terraria.Dome.Simulation.WorldObjects;

public static class ChestValidationQuery
{
  public static bool IsValid(ChestComponent chest, bool tileFootprintValid)
  {
    return chest.ChestId > 0 && chest.Inventory is not null && tileFootprintValid;
  }
}
