using System;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyUnderworldAshRunnerFactory
{
  public static LegacyTileRunnerRequest? TryCreate(
    WorldGridSnapshot snapshot,
    LegacyPassRandomState random,
    int column)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(random);
    if (column < 0 || column >= snapshot.Metadata.Width || snapshot.Metadata.Height <= 135)
    {
      throw new ArgumentOutOfRangeException(nameof(column));
    }

    if (random.Next(50) != 0)
    {
      return null;
    }

    int scanY = snapshot.Metadata.Height - 65;
    while (!snapshot.GetTile(column, scanY).IsActive && scanY > snapshot.Metadata.Height - 135)
    {
      scanY--;
    }

    return new LegacyTileRunnerRequest(
      random.Next(snapshot.Metadata.Width),
      scanY + random.Next(20, 50),
      random.Next(15, 20),
      1000,
      57,
      addTile: true,
      speedX: 0.0,
      speedY: random.Next(1, 3),
      noYChange: true,
      overwrite: false,
      ignoreTileType: -1);
  }
}
