using System;

namespace Terraria.WorldSession.Components;

public static class WorldSavedOreTierRepairSystem
{
  public static OreTierState Apply(
    WorldSavedOreTierStateComponent state,
    in WorldSavedOreTierTileCounts tileCounts)
  {
    ArgumentNullException.ThrowIfNull(state);

    OreTierState repairedState = WorldSavedOreTierRepairQuery.Calculate(
      state.Value,
      tileCounts);
    state.Replace(repairedState);
    return repairedState;
  }
}
