using System;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldModel;

public sealed class WorldEnvironmentTickSystem
{
  public IReadOnlyList<WorldEnvironmentChange> Advance(
    WorldGrid world,
    WorldRuleSnapshot rules,
    DefaultWorldEnvironmentConvergence convergence)
  {
    ArgumentNullException.ThrowIfNull(world);
    ArgumentNullException.ThrowIfNull(convergence);
    _ = rules;
    return convergence.Advance(world);
  }
}
