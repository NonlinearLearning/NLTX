namespace Terraria.WorldGeneration.Adapters;

public interface IGenerationRandomSource
{
  int NextInt(
    GenerationRandomStream stream,
    int minimumInclusive,
    int maximumExclusive);
}
