using Terraria.Dome.Simulation.Liquid.Components;

namespace Terraria.Dome.Simulation.Liquid.Definitions;

public readonly record struct LiquidRuleDefinition(
  LiquidType Type,
  LiquidGravity Gravity,
  byte TransferAmount,
  bool Hazardous)
{
  public bool CanMergeWith(LiquidType other)
  {
    return Type == other;
  }
}
