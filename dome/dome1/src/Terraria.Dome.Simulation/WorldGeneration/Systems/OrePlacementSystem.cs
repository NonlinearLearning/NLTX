using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration.Systems;

public sealed class OrePlacementSystem
{
  public void AppendCommands(
    WorldGridSnapshot snapshot,
    WorldGenerationRequest request,
    OreDefinition definition,
    TileProtectionComponent protection,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(request);
    ArgumentNullException.ThrowIfNull(commands);
    OrePlacementTransactionSystem transactionSystem = new();
    if (!transactionSystem.TryPrepare(
          snapshot,
          request,
          definition,
          protection,
          out OrePlacementPreparationResult preparation))
    {
      return;
    }

    _ = transactionSystem.TryAppendCommands(
      snapshot,
      preparation,
      protection,
      ref state,
      commands);
  }
}
