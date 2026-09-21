using System;
using Terraria.WorldGeneration.Adapters;

namespace Terraria.WorldGeneration.Passes;

public sealed class GemSelectionPassScratch
{
  public const int GemCount = 6;

  private readonly bool[] _allowedGems = new bool[GemCount];

  public bool HasSelection { get; private set; }

  public void Begin(IGenerationRandomSource random)
  {
    ArgumentNullException.ThrowIfNull(random);
    Clear();

    _allowedGems[random.NextInt(
      GenerationRandomStream.WorldGeneration,
      0,
      GemCount)] = true;

    for (int gem = 0; gem < GemCount; gem++)
    {
      if (random.NextInt(GenerationRandomStream.WorldGeneration, 0, GemCount) == 0)
      {
        _allowedGems[gem] = true;
      }
    }

    HasSelection = true;
  }

  public int SelectGem(IGenerationRandomSource random)
  {
    ArgumentNullException.ThrowIfNull(random);
    EnsureSelection();

    int gem = random.NextInt(
      GenerationRandomStream.WorldGeneration,
      0,
      GemCount);
    while (!_allowedGems[gem])
    {
      gem = random.NextInt(
        GenerationRandomStream.WorldGeneration,
        0,
        GemCount);
    }

    return gem;
  }

  public ushort SelectGemTile(IGenerationRandomSource random)
  {
    ArgumentNullException.ThrowIfNull(random);
    return random.NextInt(GenerationRandomStream.WorldGeneration, 0, 20) == 0
      ? GemTileFor(SelectGem(random))
      : (ushort)1;
  }

  public bool IsAllowed(int gem)
  {
    return _allowedGems[ValidateGem(gem)];
  }

  public void Clear()
  {
    Array.Clear(_allowedGems);
    HasSelection = false;
  }

  private static ushort GemTileFor(int gem)
  {
    return gem switch
    {
      0 => 67,
      1 => 66,
      2 => 63,
      3 => 65,
      4 => 64,
      _ => 68,
    };
  }

  private void EnsureSelection()
  {
    if (!HasSelection)
    {
      throw new InvalidOperationException(
        "A gem selection pass must be started before selecting a gem.");
    }
  }

  private static int ValidateGem(int gem)
  {
    if ((uint)gem >= GemCount)
    {
      throw new ArgumentOutOfRangeException(nameof(gem));
    }

    return gem;
  }
}
