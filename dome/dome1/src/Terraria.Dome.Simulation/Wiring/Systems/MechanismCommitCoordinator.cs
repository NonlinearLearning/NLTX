using System;
using Terraria.Dome.Simulation.Wiring.Components;

namespace Terraria.Dome.Simulation.Wiring.Systems;

public sealed class MechanismCommitCoordinator
{
  public MechanismCommitResult Commit(
    MechanismCommitOrder order,
    bool tileChangesValid,
    bool liquidChangesValid)
  {
    if (order.Sequence < 0 ||
        (order.ApplyTileChangesBeforeLiquid
          ? !tileChangesValid || !liquidChangesValid
          : !liquidChangesValid || !tileChangesValid))
    {
      return MechanismCommitResult.Rejected(order.Sequence);
    }

    return MechanismCommitResult.Accepted(order.Sequence);
  }
}
