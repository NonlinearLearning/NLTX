using System;
using Terraria.WorldGeneration.Definitions;

namespace Terraria.WorldGeneration.Queries;

public static class WorldSecretSeedDerivedOptionsQuery
{
  public static WorldSecretSeedDerivedOptionsSelection Evaluate(
    WorldSecretSeedDerivedOptionsInput input)
  {
    if (input.BiggerAbandonedHousesRandomRoll is int roll &&
        (roll < 0 || roll > 2))
    {
      throw new ArgumentOutOfRangeException(
        nameof(input),
        "The abandoned-house random roll must be in the range 0..2.");
    }

    bool generateBiggerAbandonedHouses = input.BiggerAbandonedHousesEnabled;
    if (!generateBiggerAbandonedHouses && input.ErrorWorldEnabled)
    {
      if (!input.BiggerAbandonedHousesRandomRoll.HasValue)
      {
        throw new ArgumentException(
          "An explicit random roll is required for the error-world fallback branch.",
          nameof(input));
      }

      generateBiggerAbandonedHouses =
        input.BiggerAbandonedHousesRandomRoll.Value == 0;
    }

    return new WorldSecretSeedDerivedOptionsSelection(
      generateBiggerAbandonedHouses,
      input.RainbowStuffEnabled || input.TenthAnniversaryWorld);
  }
}
