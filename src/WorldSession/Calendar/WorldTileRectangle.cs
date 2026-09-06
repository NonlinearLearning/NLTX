using System;

namespace Terraria.WorldSession.Calendar;

// Provisional boundary type; the final world-geometry owner remains integration-review.
public readonly record struct WorldTileRectangle(
  int Left,
  int Top,
  int Right,
  int Bottom)
{
  public long Width => (long)Right - Left;

  public long Height => (long)Bottom - Top;

  public bool IsEmpty => Width == 0 && Height == 0;

  public void Validate()
  {
    if (Right < Left)
    {
      throw new ArgumentException(
        "The right edge cannot be left of the left edge.",
        nameof(Right));
    }

    if (Bottom < Top)
    {
      throw new ArgumentException(
        "The bottom edge cannot be above the top edge.",
        nameof(Bottom));
    }
  }
}
