using System;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Systems;

/// <summary>
/// Owns bounded mountain-cave history recording without applying cave effects.
/// </summary>
public static class MountainCaveHistorySystem
{
  public static bool TryAppend(
    MountainCaveHistoryComponent component,
    int x,
    int y)
  {
    ArgumentNullException.ThrowIfNull(component);
    return component.TryAppend(x, y);
  }

  public static void Clear(MountainCaveHistoryComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    component.Clear();
  }
}
