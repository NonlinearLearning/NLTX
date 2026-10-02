using System;
using System.Collections.Generic;

namespace Terraria.WorldGeneration.Housing;

public sealed class CactusWaterGridSnapshot
{
  private readonly byte[] _liquidAmounts;

  public CactusWaterGridSnapshot(
    int width,
    int height,
    IReadOnlyList<byte> liquidAmounts)
  {
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);
    ArgumentNullException.ThrowIfNull(liquidAmounts);

    long cellCount = (long)width * height;
    if (cellCount > int.MaxValue || liquidAmounts.Count != cellCount)
    {
      throw new ArgumentException(
        "Liquid amounts must contain exactly one value for every grid cell.",
        nameof(liquidAmounts));
    }

    Width = width;
    Height = height;
    _liquidAmounts = new byte[liquidAmounts.Count];
    for (int index = 0; index < liquidAmounts.Count; index++)
    {
      _liquidAmounts[index] = liquidAmounts[index];
    }
  }

  public int Width { get; }

  public int Height { get; }

  public byte GetLiquidAmount(int x, int y)
  {
    if (!IsInside(x, y))
    {
      throw new ArgumentOutOfRangeException(nameof(x));
    }

    return _liquidAmounts[(y * Width) + x];
  }

  public bool IsInside(int x, int y)
  {
    return x >= 0 && x < Width && y >= 0 && y < Height;
  }
}
