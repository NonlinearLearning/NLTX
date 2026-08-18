using System;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed class WorldGenerationCheckpoint
{
  public WorldGenerationCheckpoint(
    WorldGridSnapshot snapshot,
    WorldGenerationStateComponent state,
    GenerationCursorComponent cursor)
  {
    Snapshot = snapshot ?? throw new ArgumentNullException(nameof(snapshot));
    State = state;
    Cursor = cursor;
  }

  public GenerationCursorComponent Cursor { get; }

  public WorldGenerationStateComponent State { get; }

  public WorldGridSnapshot Snapshot { get; }

  public WorldGrid RestoreWorld()
  {
    return WorldGrid.FromSnapshot(Snapshot);
  }
}
