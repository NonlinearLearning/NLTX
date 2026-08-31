using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyDirtInRocksPassDefinition
{
  public const double Density = 0.005;
  public const int MinimumRemixOffset = -1;
  public const int MaximumRemixOffsetExclusive = 3;

  public static LegacyTileRunnerPassInput CreateDefaultRecipe()
  {
    return new LegacyTileRunnerPassInput(
      "DirtInRocks",
      "rock-layer-dirt",
      0,
      false,
      2,
      6,
      2,
      40,
      "rockLayerLow..maxTilesY",
      true,
      true,
      true);
  }

  public static int CalculateInvocationCount(int width, int height)
  {
    if (width <= 0 || height <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(width));
    }

    return checked((int)Math.Ceiling(width * (double)height * Density));
  }

  public static void Validate()
  {
    LegacyTileRunnerPassInput recipe = CreateDefaultRecipe();
    if (recipe.PassName != "DirtInRocks" || recipe.TileType != 0 || recipe.AddTile ||
        recipe.MinimumStrength != 2 || recipe.MaximumStrengthExclusive != 6 ||
        recipe.MinimumSteps != 2 || recipe.MaximumStepsExclusive != 40 ||
        recipe.VerticalRange != "rockLayerLow..maxTilesY" || !recipe.UsesRandomX ||
        !recipe.UsesRandomY || !recipe.ResetsRandomFromWorldSeed ||
        !double.IsFinite(Density) || Density < 0.0 ||
        MinimumRemixOffset >= MaximumRemixOffsetExclusive)
    {
      throw new InvalidOperationException(
        "DirtInRocks pass definition contains an invalid source contract.");
    }
  }
}
