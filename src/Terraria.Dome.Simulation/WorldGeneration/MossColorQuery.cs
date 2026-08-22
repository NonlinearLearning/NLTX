namespace Terraria.Dome.Simulation.WorldGeneration;

public static class MossColorQuery
{
  public static int GetColor(int tileType)
  {
    return tileType switch
    {
      179 or 512 => 0,
      180 or 513 => 1,
      181 or 514 => 2,
      182 or 515 => 3,
      183 or 516 => 4,
      381 or 517 => 5,
      534 or 535 => 6,
      536 or 537 => 7,
      539 or 540 => 8,
      625 or 626 => 9,
      627 or 628 => 10,
      _ => -1
    };
  }
}
