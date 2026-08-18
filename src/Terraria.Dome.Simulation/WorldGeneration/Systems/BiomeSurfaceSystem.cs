using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration.Systems;

public sealed class BiomeSurfaceSystem
{
  public BiomeSurfaceResult AppendCommands(
    WorldGridSnapshot snapshot,
    BiomeSurfaceComponent profile,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(commands);
    if (profile.BiomeId != "default")
    {
      return BiomeSurfaceResult.Unsupported($"Unsupported biome profile: {profile.BiomeId}");
    }

    if (state.Stage < WorldGenerationStage.Biome &&
        !state.TryAdvance(WorldGenerationStage.Biome))
    {
      return BiomeSurfaceResult.Unsupported("Biome stage could not be started.");
    }

    return BiomeSurfaceResult.Success();
  }
}

public sealed record BiomeSurfaceResult(bool Supported, string? FailureReason)
{
  public static BiomeSurfaceResult Success()
  {
    return new BiomeSurfaceResult(true, null);
  }

  public static BiomeSurfaceResult Unsupported(string reason)
  {
    return new BiomeSurfaceResult(false, reason);
  }
}
