using System;
using Terraria.WorldGeneration.Adapters;

namespace Terraria.WorldGeneration.Passes;

public sealed class WorldGenerationRandomPort : IGenerationRandomSource
{
  private readonly Func<GenerationRandomStream, int, int, int> _nextInt;

  public WorldGenerationRandomPort(
    Func<GenerationRandomStream, int, int, int> nextInt)
  {
    ArgumentNullException.ThrowIfNull(nextInt);
    _nextInt = nextInt;
  }

  public int NextInt(
    GenerationRandomStream stream,
    int minimumInclusive,
    int maximumExclusive)
  {
    if (!Enum.IsDefined(stream))
    {
      throw new ArgumentOutOfRangeException(nameof(stream));
    }

    if (maximumExclusive <= minimumInclusive)
    {
      throw new ArgumentOutOfRangeException(nameof(maximumExclusive));
    }

    int value = _nextInt.Invoke(
      stream,
      minimumInclusive,
      maximumExclusive);
    if (value < minimumInclusive || value >= maximumExclusive)
    {
      throw new InvalidOperationException(
        "The generation random source returned a value outside the requested range.");
    }

    return value;
  }
}
