namespace Terraria.Dome.Simulation.WorldGeneration;

public static class PaintColorQuery
{
  private static readonly PaintColorValue White = new(byte.MaxValue, byte.MaxValue, byte.MaxValue,
    byte.MaxValue);

  public static PaintColorValue GetColor(int color)
  {
    return color switch
    {
      1 or 13 => new(byte.MaxValue, 0, 0, byte.MaxValue),
      2 or 14 => new(byte.MaxValue, 127, 0, byte.MaxValue),
      3 or 15 => new(byte.MaxValue, byte.MaxValue, 0, byte.MaxValue),
      4 or 16 => new(127, byte.MaxValue, 0, byte.MaxValue),
      5 or 17 => new(0, byte.MaxValue, 0, byte.MaxValue),
      6 or 18 => new(0, byte.MaxValue, 127, byte.MaxValue),
      7 or 19 => new(0, byte.MaxValue, byte.MaxValue, byte.MaxValue),
      8 or 20 => new(0, 127, byte.MaxValue, byte.MaxValue),
      9 or 21 => new(0, 0, byte.MaxValue, byte.MaxValue),
      10 or 22 => new(127, 0, byte.MaxValue, byte.MaxValue),
      11 or 23 => new(byte.MaxValue, 0, byte.MaxValue, byte.MaxValue),
      12 or 24 => new(byte.MaxValue, 0, 127, byte.MaxValue),
      25 => new(75, 75, 75, byte.MaxValue),
      26 => White,
      27 => new(175, 175, 175, byte.MaxValue),
      28 => new(byte.MaxValue, 178, 125, byte.MaxValue),
      29 => new(25, 25, 25, byte.MaxValue),
      30 => new(200, 200, 200, 150),
      _ => White
    };
  }
}
