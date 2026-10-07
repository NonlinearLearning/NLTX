using System;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Systems;

public static class FloatingIslandPlacementSystem
{
  public static void Commit(
    FloatingIslandPlacementStateComponent component,
    in FloatingIslandPlacementSnapshot snapshot)
  {
    ArgumentNullException.ThrowIfNull(component);
    if (snapshot.GenerationId != component.GenerationId)
    {
      throw new ArgumentException(
        "Floating-island state cannot be committed to another generation.",
        nameof(snapshot));
    }

    if (snapshot.Capacity != FloatingIslandPlacementStateComponent.Capacity)
    {
      throw new ArgumentException(
        "Floating-island metadata capacity must remain 300.",
        nameof(snapshot));
    }

    if (snapshot.Count < 0 || snapshot.Count > snapshot.Capacity)
    {
      throw new ArgumentOutOfRangeException(nameof(snapshot));
    }

    ArgumentNullException.ThrowIfNull(snapshot.Houses);
    component.ReplaceState(
      snapshot.SkyLakes,
      snapshot.SkyIslandHouseCount,
      snapshot.Count,
      snapshot.Houses);
  }

  public static bool TryAppend(
    FloatingIslandPlacementStateComponent component,
    FloatingIslandHouseSnapshot house)
  {
    ArgumentNullException.ThrowIfNull(component);
    return component.TryAppend(house);
  }

  public static void Clear(FloatingIslandPlacementStateComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    component.ClearHouses();
  }
}
