using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class TileFontMeasurementQuery
{
  private const int CharacterAdvance = 6;
  private const int GlyphHeight = 5;
  private const string SupportedCharacters =
    "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789- ";

  public static TileFontMeasurement Measure(string text)
  {
    ArgumentNullException.ThrowIfNull(text);
    int lineWidth = 0;
    int lineOffset = 0;
    int measuredWidth = 0;
    int measuredHeight = GlyphHeight;
    foreach (char character in text)
    {
      if (character == '\n')
      {
        lineWidth = 0;
        lineOffset += CharacterAdvance;
        measuredHeight = lineOffset + GlyphHeight;
      }

      if (SupportedCharacters.IndexOf(character) >= 0)
      {
        lineWidth += CharacterAdvance;
        measuredWidth = Math.Max(measuredWidth, lineWidth - 1);
      }
    }

    return new TileFontMeasurement(measuredWidth, measuredHeight);
  }
}
