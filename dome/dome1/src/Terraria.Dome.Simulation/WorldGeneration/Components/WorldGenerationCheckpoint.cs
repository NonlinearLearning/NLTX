using System;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed class WorldGenerationCheckpoint
{
  public WorldGenerationCheckpoint(
    WorldGridSnapshot snapshot,
    WorldGenerationStateComponent state,
    GenerationCursorComponent cursor,
    WorldGenerationRuntimeState runtime)
  {
    Snapshot = snapshot ?? throw new ArgumentNullException(nameof(snapshot));
    State = state;
    Cursor = cursor;
    if (runtime.GenerationId != state.GenerationId ||
        runtime.Stage != state.Stage ||
        runtime.NextSequence != state.NextSequence ||
        runtime.Random.State != cursor.RandomState)
    {
      throw new ArgumentException(
        "The generation runtime state does not match the checkpoint cursor.",
        nameof(runtime));
    }

    Runtime = runtime;
  }

  public GenerationCursorComponent Cursor { get; }

  public WorldGenerationStateComponent State { get; }

  public WorldGenerationRuntimeState Runtime { get; }

  public WorldGridSnapshot Snapshot { get; }

  public WorldGrid RestoreWorld()
  {
    return WorldGrid.FromSnapshot(Snapshot);
  }
}
