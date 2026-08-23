using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed record LegacyTileRunnerPassInvocation(
  LegacyTileRunnerPassInput Recipe,
  LegacyTileRunnerRequest Request,
  int XDraw,
  int YDraw,
  int StrengthDraw,
  int StepsDraw,
  int RandomDrawCount);

public static class LegacyTileRunnerPassInvocationFactory
{
  public static LegacyTileRunnerPassInvocation Create(
    LegacyTileRunnerPassInput recipe,
    LegacyPassRandomState random,
    int minimumXInclusive,
    int maximumXExclusive,
    int minimumYInclusive,
    int maximumYExclusive,
    double speedX = 0.0,
    double speedY = 0.0,
    bool noYChange = false,
    bool overwrite = true,
    int ignoreTileType = -1)
  {
    ArgumentNullException.ThrowIfNull(recipe);
    ArgumentNullException.ThrowIfNull(random);
    if (minimumXInclusive < 0 || maximumXExclusive <= minimumXInclusive ||
        minimumYInclusive < 0 || maximumYExclusive <= minimumYInclusive)
    {
      throw new ArgumentOutOfRangeException(nameof(minimumXInclusive));
    }

    int xDraw = random.Next(minimumXInclusive, maximumXExclusive);
    int yDraw = random.Next(minimumYInclusive, maximumYExclusive);
    int strengthDraw = random.Next(recipe.MinimumStrength, recipe.MaximumStrengthExclusive);
    int stepsDraw = random.Next(recipe.MinimumSteps, recipe.MaximumStepsExclusive);
    LegacyTileRunnerRequest request = new(
      xDraw,
      yDraw,
      strengthDraw,
      stepsDraw,
      recipe.TileType,
      recipe.AddTile,
      speedX,
      speedY,
      noYChange,
      overwrite,
      ignoreTileType);
    return new LegacyTileRunnerPassInvocation(
      recipe,
      request,
      xDraw,
      yDraw,
      strengthDraw,
      stepsDraw,
      RandomDrawCount: 4);
  }
}
