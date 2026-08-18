using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration.Systems;

public sealed class TerrainBaseSystem
{
  public void AppendCommands(
    WorldGrid world,
    WorldGenerationRequest request,
    TerrainProfileComponent profile,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands)
  {
    ArgumentNullException.ThrowIfNull(world);
    ArgumentNullException.ThrowIfNull(request);
    ArgumentNullException.ThrowIfNull(commands);
    if (state.Stage < WorldGenerationStage.Terrain &&
        !state.TryAdvance(WorldGenerationStage.Terrain))
    {
      throw new InvalidOperationException("Terrain stage could not be started.");
    }

    TerrainDefinition definition = TerrainDefinition.Default;
    DeterministicRandom random = new(request.Metadata.Seed.Value);
    for (int x = 0; x < world.Width; x++)
    {
      int surfaceVariation = random.NextInclusive(
        -definition.SurfaceVariation,
        definition.SurfaceVariation);
      int surfaceY = Math.Clamp(
        profile.SurfaceY + surfaceVariation,
        0,
        world.Height - 1);
      for (int y = surfaceY; y < world.Height; y++)
      {
        commands.Add(new TileChangeCommand(
          state.ReserveSequence(),
          x,
          y,
          TileChangeKind.Place,
          definition.GroundTileType));
      }
    }
  }

  private sealed class DeterministicRandom
  {
    private uint _state;

    public DeterministicRandom(int seed)
    {
      _state = unchecked((uint)seed);
    }

    public int NextInclusive(int minimum, int maximum)
    {
      _state = (_state * 1664525U) + 1013904223U;
      uint range = (uint)(maximum - minimum + 1);
      return minimum + (int)(_state % range);
    }
  }
}
