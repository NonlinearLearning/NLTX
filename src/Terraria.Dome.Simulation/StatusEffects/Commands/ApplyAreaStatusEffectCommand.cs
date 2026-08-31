using System;

namespace Terraria.Dome.Simulation.StatusEffects.Commands;

public readonly record struct ApplyAreaStatusEffectCommand(
  PlayerHandle Source,
  float CenterX,
  float CenterY,
  float Radius,
  ushort Type,
  int DurationTicks)
{
  public void Validate()
  {
    if (!Source.IsValid || !float.IsFinite(CenterX) || !float.IsFinite(CenterY) ||
        !float.IsFinite(Radius) || Radius <= 0.0f || Type == 0 || DurationTicks <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(Radius));
    }
  }
}
