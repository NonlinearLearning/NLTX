using System;

namespace Terraria.Dome.Simulation;

public sealed record SimulationEntityLimits(
  int MaximumPlayers = 255,
  int MaximumNpcs = 200,
  int MaximumProjectiles = 1000,
  int MaximumWorldItems = 8192,
  int MaximumChests = 8000)
{
  public SimulationEntityLimits Validate()
  {
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(MaximumPlayers);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(MaximumNpcs);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(MaximumProjectiles);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(MaximumWorldItems);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(MaximumChests);
    return this;
  }
}
