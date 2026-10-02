namespace Terraria.WorldStorage;

public readonly record struct SectionIterationState(
  float CenterX,
  float CenterY,
  int X,
  int Y,
  int Leg,
  int XDirection,
  int YDirection);
