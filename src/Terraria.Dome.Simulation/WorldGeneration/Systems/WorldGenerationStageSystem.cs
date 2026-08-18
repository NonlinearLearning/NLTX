using System;

namespace Terraria.Dome.Simulation.WorldGeneration.Systems;

public sealed class WorldGenerationStageSystem
{
  public WorldGenerationBootstrap Initialize(WorldGenerationRequest request)
  {
    ArgumentNullException.ThrowIfNull(request);
    return new WorldGenerationBootstrap(
      new WorldGenerationStateComponent(request.GenerationId),
      new WorldSeedComponent(
        request.Metadata.Seed.Value,
        request.SeedVariant,
        request.RandomStreamVersion),
      new WorldBoundsComponent(request.Metadata.Width, request.Metadata.Height),
      request.Rules,
      new GenerationCursorComponent(WorldGenerationStage.Created, 0, 0, 0));
  }
}

public sealed record WorldGenerationBootstrap(
  WorldGenerationStateComponent State,
  WorldSeedComponent Seed,
  WorldBoundsComponent Bounds,
  WorldRuleSnapshotComponent Rules,
  GenerationCursorComponent Cursor);
