namespace Terraria.WorldGeneration.Components;

public readonly record struct WorldGenerationLandmassValue(
  int TypeId,
  double PositionX,
  double PositionY,
  int RadiusOrHalfSize,
  int Style)
{
  public double TopY => PositionY - RadiusOrHalfSize;
}
