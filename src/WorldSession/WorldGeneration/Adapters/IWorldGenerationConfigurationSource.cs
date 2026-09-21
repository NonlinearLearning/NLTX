using Terraria.WorldGeneration.Definitions;

namespace Terraria.WorldGeneration.Adapters;

public interface IWorldGenerationConfigurationSource
{
  WorldGenerationConfigurationDefinition Load(
    WorldGenerationConfigurationRequest request);
}
