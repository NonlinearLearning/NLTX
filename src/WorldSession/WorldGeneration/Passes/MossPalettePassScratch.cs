using System;
using Terraria.WorldGeneration.Adapters;

namespace Terraria.WorldGeneration.Passes;

public sealed class MossPalettePassScratch
{
  private static readonly ushort[] NeonMossPalette = [539, 536, 534, 625];

  private readonly int[] _mossTypes = new int[3];

  public ushort NeonMossType { get; private set; }

  public bool HasOrdinaryPalette { get; private set; }

  public void Refresh(IGenerationRandomSource random, bool justNeon = false)
  {
    ArgumentNullException.ThrowIfNull(random);

    NeonMossType = NeonMossPalette[random.NextInt(
      GenerationRandomStream.WorldGeneration,
      0,
      NeonMossPalette.Length)];
    if (justNeon)
    {
      return;
    }

    _mossTypes[0] = NextOrdinaryMoss(random);
    _mossTypes[1] = NextDistinctOrdinaryMoss(random, _mossTypes[0]);
    _mossTypes[2] = NextDistinctOrdinaryMoss(
      random,
      _mossTypes[0],
      _mossTypes[1]);
    HasOrdinaryPalette = true;
  }

  public int GetOrdinaryMossType(int region)
  {
    if (!HasOrdinaryPalette)
    {
      throw new InvalidOperationException(
        "An ordinary moss palette must be refreshed before it is read.");
    }

    if ((uint)region >= _mossTypes.Length)
    {
      throw new ArgumentOutOfRangeException(nameof(region));
    }

    return _mossTypes[region];
  }

  public int GetOrdinaryMossTypeForColumn(int x, int worldWidth)
  {
    ArgumentOutOfRangeException.ThrowIfNegative(x);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(worldWidth);

    int region = x < worldWidth * 0.334d
      ? 0
      : x < worldWidth * 0.667d
        ? 1
        : 2;
    return GetOrdinaryMossType(region);
  }

  public void Clear()
  {
    Array.Clear(_mossTypes);
    NeonMossType = 0;
    HasOrdinaryPalette = false;
  }

  private static int NextOrdinaryMoss(IGenerationRandomSource random)
  {
    return random.NextInt(GenerationRandomStream.WorldGeneration, 0, 5);
  }

  private static int NextDistinctOrdinaryMoss(
    IGenerationRandomSource random,
    int firstExcluded,
    int? secondExcluded = null)
  {
    int value = NextOrdinaryMoss(random);
    while (value == firstExcluded || value == secondExcluded)
    {
      value = NextOrdinaryMoss(random);
    }

    return value;
  }
}
