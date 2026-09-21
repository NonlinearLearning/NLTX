using System;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Server.Startup;

public sealed record WorldBootstrapRequest
{
  private WorldBootstrapRequest(
    WorldMetadata metadata,
    WorldRuleState rules,
    WorldBootstrapGenerationStatus generationStatus,
    WorldEntityLimits entityLimits)
  {
    Metadata = metadata;
    Rules = rules;
    GenerationStatus = generationStatus;
    EntityLimits = entityLimits;
  }

  public WorldMetadata Metadata { get; }
  public WorldRuleState Rules { get; }
  public WorldBootstrapGenerationStatus GenerationStatus { get; private init; }
  public WorldEntityLimits EntityLimits { get; }

  public static WorldBootstrapRequest Create(
    string name,
    WorldSeed seed,
    int width,
    int height,
    int spawnX,
    int spawnY,
    WorldRuleState rules,
    int? worldId = null,
    string seedVariant = "default",
    int randomStreamVersion = 1,
    WorldEntityLimits? entityLimits = null)
  {
    ArgumentNullException.ThrowIfNull(rules);
    WorldMetadata metadata = new(
      name,
      seed,
      width,
      height,
      worldId,
      spawnX,
      spawnY,
      seedVariant,
      randomStreamVersion);
    return new WorldBootstrapRequest(
      metadata,
      rules,
      WorldBootstrapGenerationStatus.Generated,
      (entityLimits ?? new WorldEntityLimits()).Validate());
  }

  internal WorldBootstrapRequest ForRestore()
  {
    return this with { GenerationStatus = WorldBootstrapGenerationStatus.Restored };
  }
}
