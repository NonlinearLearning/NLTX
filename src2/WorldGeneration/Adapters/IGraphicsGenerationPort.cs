using Terraria.WorldGeneration.Projections;

namespace Terraria.WorldGeneration.Adapters;

public interface IGraphicsGenerationPort
{
  void Present(WorldGenerationGraphicsSnapshot snapshot);
}
