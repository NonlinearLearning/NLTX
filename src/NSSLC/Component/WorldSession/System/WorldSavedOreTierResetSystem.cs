using System;

namespace Terraria.WorldSession.Components;

public static class WorldSavedOreTierResetSystem
{
  public static void Reset(WorldSavedOreTierStateComponent state)
  {
    ArgumentNullException.ThrowIfNull(state);
    state.Replace(OreTierState.Uninitialized);
  }
}
