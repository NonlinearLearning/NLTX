using System;

namespace Terraria.Dome.Simulation.WorldModel.Systems;

public enum WorldInvasionClearFlag
{
  Goblins,
  Frost,
  Pirates,
  Martians
}

public sealed class WorldInvasionClearFlagSystem
{
  public WorldInvasionClearFlag Resolve(int invasionType)
  {
    return invasionType switch
    {
      1 => WorldInvasionClearFlag.Goblins,
      2 => WorldInvasionClearFlag.Frost,
      3 => WorldInvasionClearFlag.Pirates,
      4 => WorldInvasionClearFlag.Martians,
      _ => throw new ArgumentOutOfRangeException(nameof(invasionType))
    };
  }
}
