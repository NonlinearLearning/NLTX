using System;

namespace Terraria.Dome.Simulation.WorldGeneration.Systems;

public sealed class WorldGenerationStageSystem
{
  public WorldGenerationBootstrap Initialize(WorldGenerationRequest request)
  {
    ArgumentNullException.ThrowIfNull(request);
    WorldGenerationStateComponent state = new(request.GenerationId);
    GenerationCursorComponent cursor = new(WorldGenerationStage.Created, 0, 0, 0);
    return new WorldGenerationBootstrap(
      state,
      new WorldSeedComponent(
        request.Metadata.Seed.Value,
        request.SeedVariant,
        request.RandomStreamVersion),
      new WorldBoundsComponent(request.Metadata.Width, request.Metadata.Height),
      request.Rules,
      cursor,
      WorldGenerationRuntimeState.Create(state, cursor, request.RandomStreamVersion));
  }
}

public sealed record WorldGenerationBootstrap(
  WorldGenerationStateComponent State,
  WorldSeedComponent Seed,
  WorldBoundsComponent Bounds,
  WorldRuleSnapshotComponent Rules,
  GenerationCursorComponent Cursor,
  WorldGenerationRuntimeState Runtime);
