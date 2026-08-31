using System.Collections.Frozen;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class CoatingColorQuery
{
  private static readonly IReadOnlyDictionary<int, CoatingColorValue> DefaultColors =
    new Dictionary<int, CoatingColorValue>
    {
      [1] = new(235, 170, byte.MaxValue, byte.MaxValue),
      [2] = new(180, 245, byte.MaxValue, byte.MaxValue)
    }.ToFrozenDictionary();

  public static IReadOnlyDictionary<int, CoatingColorValue> RegisterDefaults()
  {
    return DefaultColors;
  }

  public static CoatingColorValue GetColor(int coating)
  {
    return DefaultColors.TryGetValue(coating, out CoatingColorValue value) ? value : default;
  }
}
