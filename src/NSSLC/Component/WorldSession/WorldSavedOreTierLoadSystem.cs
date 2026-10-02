using System;

namespace Terraria.WorldSession.Components;

/// <summary>
/// Composes the world-file read and low-tier repair into one state commit.
/// </summary>
public static class WorldSavedOreTierLoadSystem
{
  public static OreTierState Load(
    WorldSavedOreTierStateComponent state,
    IWorldFileSavedOreTierAdapter fileAdapter,
    in WorldSavedOreTierFileInput input,
    in WorldSavedOreTierTileCounts tileCounts)
  {
    ArgumentNullException.ThrowIfNull(state);
    ArgumentNullException.ThrowIfNull(fileAdapter);

    OreTierState loadedState = fileAdapter.Read(in input);
    OreTierState repairedState = WorldSavedOreTierRepairQuery.Calculate(
      loadedState,
      in tileCounts);
    state.Replace(repairedState);
    return repairedState;
  }
}
