using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyWorldInfectionConversionPass
{
  public static bool TryCommit(
    WorldGrid world,
    WorldGridSnapshot sourceSnapshot,
    LegacyWorldInfectionConversionInput input,
    LegacyPassRandomState random,
    ref WorldGenerationStateComponent state,
    out LegacyWorldInfectionConversionCommitResult result)
  {
    ArgumentNullException.ThrowIfNull(world);
    ArgumentNullException.ThrowIfNull(sourceSnapshot);
    ArgumentNullException.ThrowIfNull(random);
    List<LegacyWorldInfectionConversionCommand> commands = new();
    LegacyWorldInfectionConversionCommandEmitter.AppendCommands(
      sourceSnapshot,
      input,
      random,
      ref state,
      commands);
    return LegacyWorldInfectionConversionCommitBoundary.TryCommit(
      world,
      sourceSnapshot,
      commands,
      ref state,
      out result);
  }
}
