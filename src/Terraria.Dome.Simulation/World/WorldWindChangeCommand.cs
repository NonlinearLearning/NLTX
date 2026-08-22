using System;

namespace Terraria.Dome.Simulation.WorldModel;

public readonly record struct WorldWindChangeCommand(
  float TargetSpeed,
  long Sequence)
{
  public bool IsValid =>
    Sequence >= 0 && float.IsFinite(TargetSpeed) && MathF.Abs(TargetSpeed) <= 0.8f;
}
