using System;
using Terraria.WorldGeneration.Definitions;

namespace Terraria.WorldGeneration.Adapters;

public sealed class WorldGenerationConfigurationAdapter :
  IWorldGenerationConfigurationSource
{
  private readonly Func<WorldGenerationConfigurationRequest,
    WorldGenerationConfigurationDefinition> _loader;

  public WorldGenerationConfigurationAdapter(
    Func<WorldGenerationConfigurationRequest,
      WorldGenerationConfigurationDefinition> loader)
  {
    ArgumentNullException.ThrowIfNull(loader);
    _loader = loader;
  }

  public WorldGenerationConfigurationDefinition Load(
    WorldGenerationConfigurationRequest request)
  {
    return _loader(request) ?? throw new InvalidOperationException(
      "The configuration source returned no definition.");
  }
}
