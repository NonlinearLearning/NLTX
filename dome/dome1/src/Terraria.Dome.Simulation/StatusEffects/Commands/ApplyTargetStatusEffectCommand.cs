using System;
using Arch.Core;

namespace Terraria.Dome.Simulation.StatusEffects.Commands;

public readonly record struct ApplyTargetStatusEffectCommand(
  Entity Target,
  PlayerHandle Source,
  ushort Type,
  int DurationTicks)
{
  public void Validate()
  {
    if (!Source.IsValid || Type == 0 || DurationTicks <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(DurationTicks));
    }
  }
}
