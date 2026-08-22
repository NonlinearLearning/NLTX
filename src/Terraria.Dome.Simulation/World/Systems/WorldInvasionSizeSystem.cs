using System;

namespace Terraria.Dome.Simulation.WorldModel.Systems;

public sealed class WorldInvasionSizeSystem
{
  public int Resolve(int invasionType, int qualifiedPlayerCount)
  {
    if (invasionType is < 1 or > 4 || qualifiedPlayerCount <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(invasionType));
    }

    return invasionType switch
    {
      3 => checked(120 + 60 * qualifiedPlayerCount),
      4 => checked(160 + 40 * qualifiedPlayerCount),
      _ => checked(80 + 40 * qualifiedPlayerCount)
    };
  }
}
