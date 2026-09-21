using System;

namespace Terraria.WorldSession.Components;

/// <summary>
/// Reads a copied saved-tier value without exposing component mutation.
/// </summary>
public static class WorldSavedOreTierQuery
{
  public static OreTierState Snapshot(WorldSavedOreTierStateComponent state)
  {
    ArgumentNullException.ThrowIfNull(state);
    return state.CreateSnapshot();
  }
}
