namespace Terraria.Dome.Simulation.Items.Definitions;

public readonly record struct ItemAppearanceDefinition(
  int HairDye = -1,
  byte Paint = 0,
  byte PaintCoating = 0)
{
  public bool PaintOrCoating => Paint != 0 || PaintCoating != 0;
}
