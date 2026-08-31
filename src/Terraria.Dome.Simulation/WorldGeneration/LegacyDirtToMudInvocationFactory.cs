using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyDirtToMudInvocationFactory
{
  public static LegacyTileRunnerRequest Create(
    LegacyDirtToMudPassDefinition definition,
    LegacyPassRandomState random,
    int width,
    int minimumY,
    int maximumYExclusive)
  {
    ArgumentNullException.ThrowIfNull(definition);
    ArgumentNullException.ThrowIfNull(random);
    definition.Validate();
    if (width <= 0 || minimumY < 0 || maximumYExclusive <= minimumY)
    {
      throw new ArgumentOutOfRangeException(nameof(width));
    }

    return new LegacyTileRunnerRequest(
      random.Next(width),
      random.Next(minimumY, maximumYExclusive),
      random.Next(definition.MinimumStrength, definition.MaximumStrengthExclusive),
      random.Next(definition.MinimumSteps, definition.MaximumStepsExclusive),
      definition.TileType,
      addTile: false,
      speedX: 0.0,
      speedY: 0.0,
      noYChange: false,
      overwrite: true,
      ignoreTileType: definition.OverrideTileType);
  }
}
