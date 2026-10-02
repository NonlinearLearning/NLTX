namespace Terraria.WorldGeneration.Biomes;

public sealed class CaveHouseBudgetState
{
  public int SharpenerCount { get; private set; }

  public int ExtractinatorCount { get; private set; }

  public void Configure(int sharpenerCount, int extractinatorCount)
  {
    if (sharpenerCount < 0 || extractinatorCount < 0)
    {
      throw new ArgumentOutOfRangeException(
        sharpenerCount < 0 ? nameof(sharpenerCount) : nameof(extractinatorCount));
    }

    SharpenerCount = sharpenerCount;
    ExtractinatorCount = extractinatorCount;
  }

  public bool TryConsumeSharpener()
  {
    if (SharpenerCount == 0)
    {
      return false;
    }

    SharpenerCount--;
    return true;
  }

  public bool TryConsumeExtractinator()
  {
    if (ExtractinatorCount == 0)
    {
      return false;
    }

    ExtractinatorCount--;
    return true;
  }

  public void Reset()
  {
    SharpenerCount = 0;
    ExtractinatorCount = 0;
  }
}
