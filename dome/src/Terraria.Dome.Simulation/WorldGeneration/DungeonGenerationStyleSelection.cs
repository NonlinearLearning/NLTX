using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class DungeonGenerationStyleSelection
{
  public static bool IsValidStyleId(int styleId)
  {
    return styleId >= 0 && styleId < DungeonGenerationStyleId.Count;
  }

  public static void ValidateStyleId(int styleId)
  {
    if (!IsValidStyleId(styleId))
    {
      throw new ArgumentOutOfRangeException(nameof(styleId));
    }
  }
}
