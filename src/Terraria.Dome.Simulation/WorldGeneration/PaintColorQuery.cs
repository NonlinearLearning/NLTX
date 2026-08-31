using System.Collections.Frozen;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class PaintColorQuery
{
  private static readonly PaintColorValue White = new(byte.MaxValue, byte.MaxValue, byte.MaxValue,
    byte.MaxValue);
  private static readonly IReadOnlyDictionary<int, PaintColorValue> DefaultColors =
    new Dictionary<int, PaintColorValue>
    {
      [1] = new(byte.MaxValue, 0, 0, byte.MaxValue),
      [2] = new(byte.MaxValue, 127, 0, byte.MaxValue),
      [3] = new(byte.MaxValue, byte.MaxValue, 0, byte.MaxValue),
      [4] = new(127, byte.MaxValue, 0, byte.MaxValue),
      [5] = new(0, byte.MaxValue, 0, byte.MaxValue),
      [6] = new(0, byte.MaxValue, 127, byte.MaxValue),
      [7] = new(0, byte.MaxValue, byte.MaxValue, byte.MaxValue),
      [8] = new(0, 127, byte.MaxValue, byte.MaxValue),
      [9] = new(0, 0, byte.MaxValue, byte.MaxValue),
      [10] = new(127, 0, byte.MaxValue, byte.MaxValue),
      [11] = new(byte.MaxValue, 0, byte.MaxValue, byte.MaxValue),
      [12] = new(byte.MaxValue, 0, 127, byte.MaxValue),
      [13] = new(byte.MaxValue, 0, 0, byte.MaxValue),
      [14] = new(byte.MaxValue, 127, 0, byte.MaxValue),
      [15] = new(byte.MaxValue, byte.MaxValue, 0, byte.MaxValue),
      [16] = new(127, byte.MaxValue, 0, byte.MaxValue),
      [17] = new(0, byte.MaxValue, 0, byte.MaxValue),
      [18] = new(0, byte.MaxValue, 127, byte.MaxValue),
      [19] = new(0, byte.MaxValue, byte.MaxValue, byte.MaxValue),
      [20] = new(0, 127, byte.MaxValue, byte.MaxValue),
      [21] = new(0, 0, byte.MaxValue, byte.MaxValue),
      [22] = new(127, 0, byte.MaxValue, byte.MaxValue),
      [23] = new(byte.MaxValue, 0, byte.MaxValue, byte.MaxValue),
      [24] = new(byte.MaxValue, 0, 127, byte.MaxValue),
      [25] = new(75, 75, 75, byte.MaxValue),
      [26] = White,
      [27] = new(175, 175, 175, byte.MaxValue),
      [28] = new(byte.MaxValue, 178, 125, byte.MaxValue),
      [29] = new(25, 25, 25, byte.MaxValue),
      [30] = new(200, 200, 200, 150)
    }.ToFrozenDictionary();

  public static IReadOnlyDictionary<int, PaintColorValue> RegisterDefaults()
  {
    return DefaultColors;
  }

  public static PaintColorValue GetColor(int color)
  {
    return DefaultColors.TryGetValue(color, out PaintColorValue value) ? value : White;
  }
}
