namespace Terraria.Dome.Simulation.WorldModel;

public readonly record struct LiquidMergeComponent(
  byte FirstType,
  byte SecondType,
  byte ResultType)
{
  public bool Matches(byte firstType, byte secondType)
  {
    return (FirstType == firstType && SecondType == secondType) ||
      (FirstType == secondType && SecondType == firstType);
  }
}
