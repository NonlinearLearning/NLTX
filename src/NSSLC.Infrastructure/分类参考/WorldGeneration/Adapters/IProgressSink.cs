using Terraria.WorldGeneration.Projections;

namespace Terraria.WorldGeneration.Adapters;

public interface IProgressSink
{
  void Publish(WorldGenerationProgressProjection progress);
}
