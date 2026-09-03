using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacySiltInvocationFactory
{
  public static LegacyTileRunnerRequest Create(
    LegacySiltPassDefinition definition,
    LegacyPassRandomState random,
    int x,
    int y)
  {
    ArgumentNullException.ThrowIfNull(definition);
    ArgumentNullException.ThrowIfNull(random);
    return new LegacyTileRunnerRequest(
      x,
      y,
      random.Next(definition.MinimumStrength, definition.MaximumStrengthExclusive),
      random.Next(definition.MinimumSteps, definition.MaximumStepsExclusive),
      definition.TileType,
      addTile: true,
      speedX: 0.0,
      speedY: 0.0,
      noYChange: false,
      overwrite: false,
      ignoreTileType: -1);
  }

  public static LegacyTileRunnerRequest? TryCreate(
    LegacySiltPassDefinition definition,
    LegacyPassRandomState random,
    int width,
    int minimumY,
    int maximumYExclusive,
    int wallType)
  {
    ArgumentNullException.ThrowIfNull(definition);
    ArgumentNullException.ThrowIfNull(random);
    definition.Validate();
    if (width <= 0 || minimumY < 0 || maximumYExclusive <= minimumY)
    {
      throw new ArgumentOutOfRangeException(nameof(width));
    }

    int x = random.Next(width);
    int y = random.Next(minimumY, maximumYExclusive);
    if (!definition.IsWallEligible(wallType))
    {
      return null;
    }

    return Create(definition, random, x, y);
  }
}
