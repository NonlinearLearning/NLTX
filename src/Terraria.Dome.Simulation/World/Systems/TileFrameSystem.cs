using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldGeneration;

namespace Terraria.Dome.Simulation.WorldModel;

public sealed class TileFrameSystem
{
  public bool TryAppendCommands(
    WorldGridSnapshot snapshot,
    IReadOnlyCollection<TileChangeCommand> pendingChanges,
    ref WorldGenerationStateComponent state,
    List<TileFrameCommand> commands)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(pendingChanges);
    ArgumentNullException.ThrowIfNull(commands);
    if (state.Stage < WorldGenerationStage.Framing &&
        !state.TryAdvance(WorldGenerationStage.Framing))
    {
      return false;
    }

    SortedSet<(int X, int Y)> affectedTiles = new();
    foreach (TileChangeCommand change in pendingChanges)
    {
      AddAffected(change.X, change.Y);
    }

    foreach ((int x, int y) in affectedTiles)
    {
      int activeNeighbors = CountActiveNeighbors(snapshot, x, y, pendingChanges);
      commands.Add(new TileFrameCommand(
        state.ReserveSequence(),
        x,
        y,
        (short)(activeNeighbors * 18),
        0));
    }

    return true;

    void AddAffected(int x, int y)
    {
      AddIfInside(x, y);
      AddIfInside(x - 1, y);
      AddIfInside(x + 1, y);
      AddIfInside(x, y - 1);
      AddIfInside(x, y + 1);
    }

    void AddIfInside(int x, int y)
    {
      if (!snapshot.Metadata.IsInside(x, y))
      {
        return;
      }

      _ = affectedTiles.Add((x, y));
    }
  }

  private static int CountActiveNeighbors(
    WorldGridSnapshot snapshot,
    int x,
    int y,
    IReadOnlyCollection<TileChangeCommand> pendingChanges)
  {
    int activeNeighbors = 0;
    foreach ((int neighborX, int neighborY) in new[]
    {
      (x - 1, y), (x + 1, y), (x, y - 1), (x, y + 1)
    })
    {
      if (!snapshot.Metadata.IsInside(neighborX, neighborY))
      {
        continue;
      }

      bool active = snapshot.GetTile(neighborX, neighborY).IsActive;
      foreach (TileChangeCommand change in pendingChanges)
      {
        if (change.X != neighborX || change.Y != neighborY)
        {
          continue;
        }

        active = change.Kind == TileChangeKind.Place;
      }

      if (active)
      {
        activeNeighbors++;
      }
    }

    return activeNeighbors;
  }
}
