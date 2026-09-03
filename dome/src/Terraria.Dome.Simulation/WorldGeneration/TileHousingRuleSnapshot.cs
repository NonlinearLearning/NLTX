namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct TileHousingRuleSnapshot(bool PreventInfiniteRopeFraming)
{
  public const bool BubblesAreSolidForHousing = true;
}
