using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyWebsInvocationFactory
{
  public static LegacyTileRunnerRequest Create(
    LegacyWebsPassDefinition definition,
    LegacyPassRandomState random,
    int width,
    int minimumY,
    int maximumYExclusive)
  {
    ArgumentNullException.ThrowIfNull(definition);
    ArgumentNullException.ThrowIfNull(random);
    if (width <= definition.MinimumXInset * 2 || minimumY < 0 ||
        maximumYExclusive <= minimumY)
    {
      throw new ArgumentOutOfRangeException(nameof(width));
    }

    int x = random.Next(definition.MinimumXInset, width - definition.MinimumXInset);
    int y = random.Next(minimumY, maximumYExclusive);
    double speedX = random.Next(2) == 0 ? 1.0 : -1.0;
    return new LegacyTileRunnerRequest(
      x,
      y,
      random.Next(definition.MinimumStrength, definition.MaximumStrengthExclusive),
      random.Next(definition.MinimumSteps, definition.MaximumStepsExclusive),
      definition.TileType,
      addTile: true,
      speedX,
      definition.SpeedY,
      noYChange: false,
      overwrite: false,
      ignoreTileType: -1);
  }
}
