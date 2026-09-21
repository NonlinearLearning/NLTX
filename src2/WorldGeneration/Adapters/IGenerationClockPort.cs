namespace Terraria.WorldGeneration.Adapters;

public interface IGenerationClockPort
{
  TimeSpan ReadElapsedTime();
}
