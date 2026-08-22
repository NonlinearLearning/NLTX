using System;

namespace Terraria.Dome.Simulation.Wiring.Definitions;

public sealed class LampDefinition
{
  public LampDefinition(
    ushort tileType,
    int width,
    int height,
    short frameXOffset,
    short frameYOffset = 0)
  {
    if (width <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(width));
    }

    if (height <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(height));
    }

    if (frameXOffset == 0 && frameYOffset == 0)
    {
      throw new ArgumentException("A lamp definition needs a frame transition.");
    }

    TileType = tileType;
    Width = width;
    Height = height;
    FrameXOffset = frameXOffset;
    FrameYOffset = frameYOffset;
  }

  public short FrameXOffset { get; }
  public short FrameYOffset { get; }
  public int Height { get; }
  public ushort TileType { get; }
  public int Width { get; }
}
