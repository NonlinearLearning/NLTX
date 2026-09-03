using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldGeneration;
using Terraria.Dome.Simulation.WorldGeneration.Systems;

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
    IReadOnlyList<TileFrameRequest> requests = TileFrameEvaluationQuery.CreateRequests(
      snapshot,
      pendingChanges,
      TileFrameMutationKind.TileChange);
    return new TileFrameCommandSystem().TryAppendCommands(requests, ref state, commands);
  }
}
