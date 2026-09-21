using System;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyDigExtraHoles
{
  private static readonly LegacyTileRunnerPassInput Recipe = new(
    "SecretSeed", "dig-extra-holes", -1, false, 5, 16, 30, 201,
    "50..maxTilesY-50", true, true, true);

  public static IReadOnlyList<LegacyTileRunnerPassInvocation> CreateInvocations(
    int width,
    int height,
    LegacyPassRandomState random)
  {
    ArgumentNullException.ThrowIfNull(random);
    if (width <= 100 || height <= 100)
    {
      throw new ArgumentOutOfRangeException(nameof(width));
    }

    int count = (int)(width * 0.1);
    List<LegacyTileRunnerPassInvocation> invocations = new(count);
    for (int index = 0; index < count; index++)
    {
      int x = random.Next(50, width - 50);
      int y = random.Next(50, height - 50);
      int strength = random.Next(5, 16);
      int randomDrawCount = 4;
      if (random.Next(3) == 0)
      {
        strength += random.Next(15);
        randomDrawCount++;
      }

      int steps = random.Next(30, 201);
      double speedX = random.Next(-15, 16) * 0.1;
      double speedY = random.Next(10, 26) * 0.1;
      LegacyTileRunnerRequest request = new(
        x, y, strength, steps, -1, false, speedX, speedY, true, true, -1);
      invocations.Add(new LegacyTileRunnerPassInvocation(
        Recipe, request, x, y, strength, steps, randomDrawCount + 3));
    }

    return invocations;
  }
}
