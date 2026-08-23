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

    long size = invasionType switch
    {
      3 => 120L + 60L * qualifiedPlayerCount,
      4 => 160L + 40L * qualifiedPlayerCount,
      _ => 80L + 40L * qualifiedPlayerCount
    };

    if (size > int.MaxValue)
    {
      throw new ArgumentOutOfRangeException(nameof(qualifiedPlayerCount));
    }

    return (int)size;
  }
}
