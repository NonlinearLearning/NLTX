using System;

namespace Terraria.Dome.Server.Startup;

public sealed record WorldEntityLimits(
  int MaximumPlayers = 255,
  int MaximumNpcs = 200,
  int MaximumProjectiles = 1000,
  int MaximumWorldItems = 8192,
  int MaximumChests = 8000)
{
  public WorldEntityLimits Validate()
  {
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(MaximumPlayers);
    ArgumentOutOfRangeException.ThrowIfGreaterThan(MaximumPlayers, byte.MaxValue);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(MaximumNpcs);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(MaximumProjectiles);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(MaximumWorldItems);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(MaximumChests);
    return this;
  }
}
